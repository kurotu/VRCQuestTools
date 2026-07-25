// <copyright file="AstcencTextureCompressor.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Texture compressor which uses the bundled/system astcenc CLI to encode ASTC textures out-of-process.
    /// Falls back to <see cref="UnityTextureCompressor"/> whenever the fast path is not applicable or fails.
    /// </summary>
    internal class AstcencTextureCompressor : ITextureCompressor
    {
        /// <summary>
        /// Timeout for a single astcenc invocation (one mip level). Generous because "-thorough" on large
        /// textures can take a while even with all cores; a genuine hang is still caught and falls back.
        /// </summary>
        private const int CompressTimeoutMs = 5 * 60 * 1000;

        /// <summary>
        /// Whether the TGA image descriptor written for astcenc input declares top-left origin.
        /// </summary>
        /// <remarks>
        /// Determined empirically in AstcencTextureCompressorTests.CompressTexture_Orientation_MatchesUnityCompressor:
        /// with topToBottom = false (bottom-left origin, the value predicted by <see cref="AstcUtility.WriteTga"/>'s
        /// remarks under the assumption that raw texture data uses the same row order as
        /// <see cref="Texture2D.GetPixels32()"/>), the astcenc output was vertically flipped relative to Unity's own
        /// ASTC encoder (measured diff 0.20, threshold 0.1). Flipping to true (top-left origin) makes astcenc's
        /// output match Unity's encoder, so <see cref="Texture2D.GetRawTextureData()"/> apparently returns row 0 as
        /// the *top* row for this raw-buffer path, unlike GetPixels32.
        /// </remarks>
        private const bool TopToBottomOrigin = true;

        /// <summary>
        /// Whether the TGA image descriptor written for astcenc normal-map input declares top-left origin, for
        /// pixel data obtained via <see cref="Texture2D.GetPixels32(int)"/>.
        /// </summary>
        /// <remarks>
        /// Determined empirically in AstcencNormalMapCompressionTests.CompressNormalMap_Orientation_MatchesUnityCompressor,
        /// the same way as <see cref="TopToBottomOrigin"/>: with topToBottom = false (bottom-left origin, the
        /// value that literally matches <c>GetPixels32</c>'s documented row order -- see
        /// <see cref="AstcUtility.WriteTga(Color32[], int, int, bool, string)"/>'s remarks), the astcenc output
        /// still visibly diverged from Unity's own normal map ASTC encoder (measured diff 0.0345, comfortably
        /// under the test's 0.1 threshold but far from the near-zero match a correct orientation should produce).
        /// Flipping to true (top-left origin) made the two outputs match essentially exactly (measured diff
        /// 0.0000).
        /// </remarks>
        private const bool TopToBottomOriginForNormalMap = true;

        private readonly string exePath;
        private readonly string preset;
        private readonly ITextureCompressor fallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="AstcencTextureCompressor"/> class.
        /// </summary>
        /// <param name="exePath">Full path to a usable astcenc executable.</param>
        /// <param name="version">astcenc version string (e.g. "5.6.0"), used for the cache key.</param>
        /// <param name="preset">Quality preset with a leading dash (e.g. "-thorough").</param>
        internal AstcencTextureCompressor(string exePath, string version, string preset)
        {
            // First use in the process: any file already present in AstcencCli.TempDirectory is a leftover from an
            // aborted previous run (e.g. an editor crash), since every astcenc invocation runs synchronously on the
            // main thread and cleans up its own temp files in a finally block. Safe to clear it out up front.
            AstcencCli.CleanupTempDirectoryOnce();

            this.exePath = exePath;
            this.preset = preset;
            Version = version;
            fallback = new UnityTextureCompressor();
            CacheKeyComponent = $"astcenc-{version}-{preset.TrimStart('-')}";
        }

        /// <inheritdoc/>
        public string CacheKeyComponent { get; }

        /// <summary>
        /// Gets the astcenc version string used by this compressor.
        /// </summary>
        internal string Version { get; }

        /// <summary>
        /// Gets or sets the number of textures successfully compressed through the astcenc process.
        /// Tests use this to verify that a compression actually took the astcenc path instead of
        /// silently succeeding through the Unity fallback, which produces equally valid output.
        /// </summary>
        internal static int SuccessfulCompressionCount { get; set; }

        /// <inheritdoc/>
        public AsyncCallbackRequest CompressTexture(Texture2D texture, TextureFormat format, Action<Texture2D> completion)
        {
            if (!AstcUtility.TryGetBlockSize(format, out var blockX, out var blockY) || texture.format != TextureFormat.RGBA32)
            {
                // Not a path astcenc can handle (e.g. non-ASTC target format, or an already-compressed source).
                // This is a normal, expected fallback, not an error, so no warning is logged.
                return fallback.CompressTexture(texture, format, completion);
            }

            var tempFiles = new List<string>();
            Texture2D result = null;
            var success = false;
            try
            {
                // The byte[] overload of GetRawTextureData must be used here: it succeeds even when
                // isReadable is false (the state of every baked texture after readback, which calls
                // Apply(updateMipmaps, makeNoLongerReadable: true)), while the generic NativeArray
                // overload GetRawTextureData<T>() throws UnityException for non-readable textures.
                var raw = texture.GetRawTextureData();
                var mipmapCount = Math.Max(1, texture.mipmapCount);
                var srgb = texture.isDataSRGB;
                var jobs = Math.Max(1, SystemInfo.processorCount);
                var blockSize = AstcUtility.GetBlockSizeString(format);

                // Total compressed size is known up front from the mip dimensions alone (independent of the
                // astcenc run), so the destination buffer is allocated once and each mip's decoded block data is
                // copied directly to its final offset instead of being buffered through a growable List<byte>.
                var combinedSize = 0;
                for (var level = 0; level < mipmapCount; level++)
                {
                    var w = Math.Max(1, texture.width >> level);
                    var h = Math.Max(1, texture.height >> level);
                    combinedSize += AstcUtility.GetMipDataSize(w, h, blockX, blockY);
                }
                var combined = new byte[combinedSize];
                var combinedOffset = 0;

                Directory.CreateDirectory(Path.GetFullPath(AstcencCli.TempDirectory));

                var offset = 0;
                for (var level = 0; level < mipmapCount; level++)
                {
                    var w = Math.Max(1, texture.width >> level);
                    var h = Math.Max(1, texture.height >> level);
                    var levelBytes = w * h * 4;
                    if (offset + levelBytes > raw.Length)
                    {
                        throw new InvalidDataException($"Raw texture data for \"{texture.name}\" is too short for mip level {level} (offset={offset}, needed={levelBytes}, total={raw.Length}).");
                    }

                    var id = Guid.NewGuid().ToString("N");
                    var tgaPath = Path.GetFullPath(Path.Combine(AstcencCli.TempDirectory, $"{id}.tga"));
                    var astcPath = Path.GetFullPath(Path.Combine(AstcencCli.TempDirectory, $"{id}.astc"));
                    tempFiles.Add(tgaPath);
                    tempFiles.Add(astcPath);

                    AstcUtility.WriteTga(raw, offset, levelBytes, w, h, TopToBottomOrigin, tgaPath);
                    offset += levelBytes;

                    var runResult = AstcencCli.RunCompress(exePath, tgaPath, astcPath, blockSize, preset, srgb, jobs, CompressTimeoutMs);
                    if (!runResult.Success)
                    {
                        throw new InvalidOperationException($"astcenc failed for \"{texture.name}\" mip level {level} (exitCode={runResult.ExitCode}, timedOut={runResult.TimedOut}): {runResult.StdErr}");
                    }

                    var astcFileData = File.ReadAllBytes(astcPath);
                    var blockData = AstcUtility.StripAstcHeader(astcFileData, w, h, blockX, blockY);
                    Buffer.BlockCopy(blockData, 0, combined, combinedOffset, blockData.Length);
                    combinedOffset += blockData.Length;
                }

                result = new Texture2D(texture.width, texture.height, format, mipmapCount > 1, !texture.isDataSRGB);
                result.LoadRawTextureData(combined);

                // updateMipmaps=false: mip data was already generated by the caller and encoded per level above.
                // makeNoLongerReadable=false: CacheUtility.TextureCache reads the result back via GetRawTextureData
                // right after compression, so the result must remain readable.
                result.Apply(false, false);
                result.name = texture.name;
                result.wrapMode = texture.wrapMode;
                result.filterMode = texture.filterMode;
                result.anisoLevel = texture.anisoLevel;

                success = true;
                SuccessfulCompressionCount++;
            }
            catch (Exception e)
            {
                Logger.LogWarning($"astcenc compression failed for texture \"{texture.name}\", falling back to Unity's texture compression. {e.Message}", texture);
                if (result != null)
                {
                    UnityEngine.Object.DestroyImmediate(result);
                    result = null;
                }
            }
            finally
            {
                foreach (var path in tempFiles)
                {
                    AstcencCli.DeleteFileSilently(path);
                }
            }

            // Destroying the input and constructing ResultRequest (which synchronously invokes completion) are
            // deliberately outside the try/catch above: completion is caller-supplied and may itself throw, and
            // that must not be misdiagnosed as "astcenc failed" and trigger a second, conflicting completion via
            // the fallback path below with an already-destroyed input texture.
            if (success)
            {
                TextureUtility.DestroyTexture(texture);
                return new ResultRequest<Texture2D>(result, completion);
            }

            // The input texture is intentionally left intact so the fallback path can still compress it.
            return fallback.CompressTexture(texture, format, completion);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Unity's ASTC normal map encoding stores the tangent-space normal directly as RGB (no swizzle, alpha
        /// fixed to 1.0), which astcenc's generic linear (-cl) color encoder reproduces equivalent output for, so
        /// the same encoder used for color textures applies here too. astcenc itself never generates mipmaps, so
        /// the full chain (down to 1x1) is built by this method via <see cref="NormalMapMipUtility.DownsampleNormalMap"/>,
        /// which re-normalizes after each box-filter step so mip levels do not read as flattened shading.
        /// </remarks>
        public AsyncCallbackRequest CompressNormalMap(Texture2D texture, TextureFormat? format, bool readable, int? maxTextureSize, Action<Texture2D> completion)
        {
            if (!format.HasValue || !AstcUtility.TryGetBlockSize(format.Value, out var blockX, out var blockY) || !texture.isReadable)
            {
                // Not a path astcenc can handle (e.g. a non-mobile normal map left for TextureGenerator to decide,
                // an unsupported ASTC format, or a non-readable input). This is a normal, expected fallback, not
                // an error, so no warning is logged.
                return fallback.CompressNormalMap(texture, format, readable, maxTextureSize, completion);
            }

            var tempFiles = new List<string>();
            Texture2D result = null;
            var success = false;
            try
            {
                var pixels = texture.GetPixels32(0);
                var width = texture.width;
                var height = texture.height;

                // Unity's normal map TextureGenerator pipeline always writes alpha = 1.0 (fully opaque), regardless
                // of the source texture's own alpha. Match that here, including for mip 0, so a source with a
                // meaningless (or missing) alpha channel does not leak into the compressed output.
                for (var i = 0; i < pixels.Length; i++)
                {
                    var p = pixels[i];
                    pixels[i] = new Color32(p.r, p.g, p.b, 255);
                }

                // Mirrors UnityTextureCompressor.CompressNormalMap's maxTextureSize handling: shrink mip 0 itself
                // when it exceeds the requested cap, using the same re-normalizing downsample as the mip chain
                // below (so this is just the mip chain generation starting one or more levels early).
                var currentMaxSize = Math.Max(width, height);
                var targetMaxSize = maxTextureSize.HasValue ? Math.Min(maxTextureSize.Value, currentMaxSize) : currentMaxSize;
                while (Math.Max(width, height) > targetMaxSize)
                {
                    pixels = NormalMapMipUtility.DownsampleNormalMap(pixels, width, height, out width, out height);
                }

                var jobs = Math.Max(1, SystemInfo.processorCount);
                var blockSize = AstcUtility.GetBlockSizeString(format.Value);

                // Precompute every mip level's dimensions (level 0, already capped above, down to 1x1) so the
                // destination buffer can be allocated once, mirroring CompressTexture's approach.
                var levelSizes = new List<(int Width, int Height)> { (width, height) };
                var lw = width;
                var lh = height;
                while (lw > 1 || lh > 1)
                {
                    lw = Math.Max(1, lw >> 1);
                    lh = Math.Max(1, lh >> 1);
                    levelSizes.Add((lw, lh));
                }

                var combinedSize = 0;
                foreach (var size in levelSizes)
                {
                    combinedSize += AstcUtility.GetMipDataSize(size.Width, size.Height, blockX, blockY);
                }
                var combined = new byte[combinedSize];
                var combinedOffset = 0;

                Directory.CreateDirectory(Path.GetFullPath(AstcencCli.TempDirectory));

                var levelPixels = pixels;
                var levelWidth = width;
                var levelHeight = height;
                for (var level = 0; level < levelSizes.Count; level++)
                {
                    var (w, h) = levelSizes[level];

                    var id = Guid.NewGuid().ToString("N");
                    var tgaPath = Path.GetFullPath(Path.Combine(AstcencCli.TempDirectory, $"{id}.tga"));
                    var astcPath = Path.GetFullPath(Path.Combine(AstcencCli.TempDirectory, $"{id}.astc"));
                    tempFiles.Add(tgaPath);
                    tempFiles.Add(astcPath);

                    AstcUtility.WriteTga(levelPixels, w, h, TopToBottomOriginForNormalMap, tgaPath);

                    // Normal maps are always linear (never sRGB), so always -cl.
                    var runResult = AstcencCli.RunCompress(exePath, tgaPath, astcPath, blockSize, preset, false, jobs, CompressTimeoutMs);
                    if (!runResult.Success)
                    {
                        throw new InvalidOperationException($"astcenc failed for normal map \"{texture.name}\" mip level {level} (exitCode={runResult.ExitCode}, timedOut={runResult.TimedOut}): {runResult.StdErr}");
                    }

                    var astcFileData = File.ReadAllBytes(astcPath);
                    var blockData = AstcUtility.StripAstcHeader(astcFileData, w, h, blockX, blockY);
                    Buffer.BlockCopy(blockData, 0, combined, combinedOffset, blockData.Length);
                    combinedOffset += blockData.Length;

                    if (level + 1 < levelSizes.Count)
                    {
                        levelPixels = NormalMapMipUtility.DownsampleNormalMap(levelPixels, levelWidth, levelHeight, out levelWidth, out levelHeight);
                    }
                }

                // Normal maps are always linear (isDataSRGB = false), matching UnityTextureCompressor's output.
                result = new Texture2D(width, height, format.Value, levelSizes.Count > 1, true);
                result.LoadRawTextureData(combined);

                // updateMipmaps=false: mip data was already generated and encoded per level above.
                result.Apply(false, !readable);
                result.name = texture.name;
                result.wrapMode = texture.wrapMode;
                result.filterMode = texture.filterMode;
                result.anisoLevel = texture.anisoLevel;

                // Matches UnityTextureCompressor.CompressNormalMap, which always enables streaming mipmaps via
                // TextureGenerationSettings for its output.
                TextureUtility.SetStreamingMipMaps(result, true);

                success = true;
                SuccessfulCompressionCount++;
            }
            catch (Exception e)
            {
                Logger.LogWarning($"astcenc normal map compression failed for texture \"{texture.name}\", falling back to Unity's texture compression. {e.Message}", texture);
                if (result != null)
                {
                    UnityEngine.Object.DestroyImmediate(result);
                    result = null;
                }
            }
            finally
            {
                foreach (var path in tempFiles)
                {
                    AstcencCli.DeleteFileSilently(path);
                }
            }

            if (success)
            {
                return new ResultRequest<Texture2D>(result, completion);
            }

            // Unlike CompressTexture, the input is never destroyed here (on success or failure): this mirrors
            // UnityTextureCompressor.CompressNormalMap, which also leaves the input texture untouched and always
            // returns a distinct new instance from TextureGenerator.
            return fallback.CompressNormalMap(texture, format, readable, maxTextureSize, completion);
        }
    }
}
