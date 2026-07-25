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
            try
            {
                var raw = texture.GetRawTextureData<byte>();
                var mipmapCount = Math.Max(1, texture.mipmapCount);
                var srgb = texture.isDataSRGB;
                var jobs = Math.Max(1, SystemInfo.processorCount);
                var blockSize = AstcUtility.GetBlockSizeString(format);
                var combined = new List<byte>();

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

                    var pixels = ExtractLevelPixels(raw, offset, w, h);
                    offset += levelBytes;

                    var id = Guid.NewGuid().ToString("N");
                    var tgaPath = Path.GetFullPath(Path.Combine(AstcencCli.TempDirectory, $"{id}.tga"));
                    var astcPath = Path.GetFullPath(Path.Combine(AstcencCli.TempDirectory, $"{id}.astc"));
                    tempFiles.Add(tgaPath);
                    tempFiles.Add(astcPath);

                    AstcUtility.WriteTga(pixels, w, h, TopToBottomOrigin, tgaPath);

                    var runResult = AstcencCli.RunCompress(exePath, tgaPath, astcPath, blockSize, preset, srgb, jobs, CompressTimeoutMs);
                    if (!runResult.Success)
                    {
                        throw new InvalidOperationException($"astcenc failed for \"{texture.name}\" mip level {level} (exitCode={runResult.ExitCode}, timedOut={runResult.TimedOut}): {runResult.StdErr}");
                    }

                    var astcFileData = File.ReadAllBytes(astcPath);
                    var blockData = AstcUtility.StripAstcHeader(astcFileData, w, h, blockX, blockY);
                    combined.AddRange(blockData);
                }

                result = new Texture2D(texture.width, texture.height, format, mipmapCount > 1, !texture.isDataSRGB);
                result.LoadRawTextureData(combined.ToArray());

                // updateMipmaps=false: mip data was already generated by the caller and encoded per level above.
                // makeNoLongerReadable=false: CacheUtility.TextureCache reads the result back via GetRawTextureData
                // right after compression, so the result must remain readable.
                result.Apply(false, false);
                result.name = texture.name;
                result.wrapMode = texture.wrapMode;
                result.filterMode = texture.filterMode;
                result.anisoLevel = texture.anisoLevel;

                TextureUtility.DestroyTexture(texture);
                return new ResultRequest<Texture2D>(result, completion);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"astcenc compression failed for texture \"{texture.name}\", falling back to Unity's texture compression. {e.Message}", texture);
                if (result != null)
                {
                    UnityEngine.Object.DestroyImmediate(result);
                }

                // The input texture is intentionally left intact so the fallback path can still compress it.
                return fallback.CompressTexture(texture, format, completion);
            }
            finally
            {
                foreach (var path in tempFiles)
                {
                    DeleteFileSilently(path);
                }
            }
        }

        /// <inheritdoc/>
        /// <exception cref="NotSupportedException">Always thrown. Normal map compression is never routed to
        /// astcenc by <see cref="TextureCompressorProvider"/>, since Unity's <see cref="UnityEditor.TextureGenerator"/>
        /// pipeline handles normal map specific encoding (e.g. tangent space packing) that astcenc's generic
        /// color/alpha encoder does not replicate.</exception>
        public AsyncCallbackRequest CompressNormalMap(Texture2D texture, TextureFormat? format, bool readable, int? maxTextureSize, Action<Texture2D> completion)
        {
            throw new NotSupportedException($"{nameof(AstcencTextureCompressor)} does not support normal map compression.");
        }

        private static Color32[] ExtractLevelPixels(NativeArray<byte> raw, int offset, int width, int height)
        {
            var count = width * height;
            var pixels = new Color32[count];
            for (var i = 0; i < count; i++)
            {
                var o = offset + (i * 4);
                pixels[i] = new Color32(raw[o], raw[o + 1], raw[o + 2], raw[o + 3]);
            }
            return pixels;
        }

        private static void DeleteFileSilently(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (IOException)
            {
                // Leftovers in Temp/ are cleaned up together with the Temp folder eventually.
            }
            catch (UnauthorizedAccessException)
            {
                // Leftovers in Temp/ are cleaned up together with the Temp folder eventually.
            }
        }
    }
}
