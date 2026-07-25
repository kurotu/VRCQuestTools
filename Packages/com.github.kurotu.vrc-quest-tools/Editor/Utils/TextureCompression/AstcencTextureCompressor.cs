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
        /// Whether the TGA image descriptor written for astcenc input (both the color path, from
        /// <see cref="Texture2D.GetRawTextureData()"/>, and the normal map path, from
        /// <see cref="Texture2D.GetPixels32(int)"/>) declares top-left origin.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Determined empirically, separately for each path:
        /// </para>
        /// <para>
        /// Color path (AstcencTextureCompressorTests.CompressTexture_Orientation_MatchesUnityCompressor): with
        /// topToBottom = false (bottom-left origin, the value predicted by <see cref="AstcUtility.WriteTga"/>'s
        /// remarks under the assumption that raw texture data uses the same row order as
        /// <see cref="Texture2D.GetPixels32()"/>), the astcenc output was vertically flipped relative to Unity's
        /// own ASTC encoder (measured diff 0.20, threshold 0.1). Flipping to true (top-left origin) makes
        /// astcenc's output match Unity's encoder, so <see cref="Texture2D.GetRawTextureData()"/> apparently
        /// returns row 0 as the *top* row for this raw-buffer path, unlike GetPixels32.
        /// </para>
        /// <para>
        /// Normal map path (AstcencNormalMapCompressionTests.CompressNormalMap_Orientation_MatchesUnityCompressor):
        /// with topToBottom = false (bottom-left origin, the value that literally matches <c>GetPixels32</c>'s
        /// documented row order -- see <see cref="AstcUtility.WriteTga(Color32[], int, int, bool, string)"/>'s
        /// remarks), the astcenc output still visibly diverged from Unity's own normal map ASTC encoder (measured
        /// diff 0.0345, comfortably under the test's 0.1 threshold but far from the near-zero match a correct
        /// orientation should produce). Flipping to true (top-left origin) made the two outputs match essentially
        /// exactly (measured diff 0.0000).
        /// </para>
        /// <para>
        /// Note the normal map result is the opposite of what <see cref="AstcUtility.WriteTga"/>'s own remarks
        /// would predict for <c>GetPixels32</c> input (bottom-left origin, i.e. false) -- unlike the color path,
        /// there is no known explanation here for why top-left origin is nonetheless the empirically correct
        /// choice; this is left unresolved. The two orientation tests above are what actually guards correctness:
        /// they compare against Unity's own encoder (diff approx. 0) and will fail if this constant, or any of
        /// the surrounding row-order assumptions, ever needs to change.
        /// </para>
        /// </remarks>
        private const bool TgaTopToBottomOrigin = true;

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

            // The byte[] overload of GetRawTextureData must be used here: it succeeds even when isReadable is
            // false (the state of every baked texture after readback, which calls Apply(updateMipmaps,
            // makeNoLongerReadable: true)), while the generic NativeArray overload GetRawTextureData<T>() throws
            // UnityException for non-readable textures.
            var raw = texture.GetRawTextureData();
            var mipmapCount = Math.Max(1, texture.mipmapCount);

            var levels = new List<(int Width, int Height)>(mipmapCount);
            for (var level = 0; level < mipmapCount; level++)
            {
                levels.Add((Math.Max(1, texture.width >> level), Math.Max(1, texture.height >> level)));
            }

            var offset = 0;
            void WriteLevelTga(int level, string tgaPath)
            {
                var (w, h) = levels[level];
                var levelBytes = w * h * 4;
                if (offset + levelBytes > raw.Length)
                {
                    throw new InvalidDataException($"Raw texture data for \"{texture.name}\" is too short for mip level {level} (offset={offset}, needed={levelBytes}, total={raw.Length}).");
                }

                AstcUtility.WriteTga(raw, offset, levelBytes, w, h, TgaTopToBottomOrigin, tgaPath);
                offset += levelBytes;
            }

            // makeNoLongerReadable=false: CacheUtility.TextureCache reads the result back via GetRawTextureData
            // right after compression, so the result must remain readable.
            var result = TryCompressLevels(
                texture,
                logPrefix: string.Empty,
                levels,
                blockX,
                blockY,
                format,
                srgb: texture.isDataSRGB,
                mipChain: mipmapCount > 1,
                linear: !texture.isDataSRGB,
                makeNoLongerReadable: false,
                WriteLevelTga,
                postProcess: null);

            // Destroying the input and constructing ResultRequest (which synchronously invokes completion) are
            // deliberately outside TryCompressLevels' own try/catch: completion is caller-supplied and may itself
            // throw, and that must not be misdiagnosed as "astcenc failed" and trigger a second, conflicting
            // completion via the fallback path below with an already-destroyed input texture.
            if (result != null)
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

            var pixels = texture.GetPixels32(0);
            var width = texture.width;
            var height = texture.height;

            // Mirrors UnityTextureCompressor.CompressNormalMap's maxTextureSize handling: shrink mip 0 itself
            // when it exceeds the requested cap, using the same re-normalizing downsample as the mip chain below
            // (so this is just the mip chain generation starting one or more levels early).
            //
            // In production this loop is effectively a no-op safety net: the texture generators that call this
            // method already resize the source (via aspect-preserving reduction) to fit maxTextureSize before
            // compression runs, so width/height are normally already within range on entry. It remains here for
            // defense in depth and so unit tests can exercise maxTextureSize handling directly without going
            // through a generator. Each iteration floor-halves both dimensions together (matching the mip chain
            // below), which is not necessarily the same result Unity's own maxTextureSize handling
            // (TextureImporterPlatformSettings.maxTextureSize) would produce for a non-power-of-two source, since
            // Unity's resize is not guaranteed to be a simple floor-halving chain.
            var currentMaxSize = Math.Max(width, height);
            var targetMaxSize = maxTextureSize.HasValue ? Math.Min(maxTextureSize.Value, currentMaxSize) : currentMaxSize;
            while (Math.Max(width, height) > targetMaxSize)
            {
                pixels = NormalMapMipUtility.DownsampleNormalMap(pixels, width, height, out width, out height);
            }

            // Unity's normal map TextureGenerator pipeline always writes alpha = 1.0 (fully opaque), regardless of
            // the source texture's own alpha. Match that here so a source with a meaningless (or missing) alpha
            // channel does not leak into the compressed output. This runs after the maxTextureSize shrink above
            // (rather than on the original mip 0) because DownsampleNormalMap's re-encode step already writes
            // alpha = 255 for every pixel it produces; forcing alpha before a shrink that is going to happen
            // anyway would just be discarded work.
            pixels = NormalMapMipUtility.ForceOpaqueAlpha(pixels);

            // Precompute every mip level's dimensions (level 0, already capped above, down to 1x1).
            var levelSizes = new List<(int Width, int Height)> { (width, height) };
            var lw = width;
            var lh = height;
            while (lw > 1 || lh > 1)
            {
                lw = Math.Max(1, lw >> 1);
                lh = Math.Max(1, lh >> 1);
                levelSizes.Add((lw, lh));
            }

            var levelPixels = pixels;
            var levelWidth = width;
            var levelHeight = height;
            void WriteLevelTga(int level, string tgaPath)
            {
                var (w, h) = levelSizes[level];
                AstcUtility.WriteTga(levelPixels, w, h, TgaTopToBottomOrigin, tgaPath);
                if (level + 1 < levelSizes.Count)
                {
                    levelPixels = NormalMapMipUtility.DownsampleNormalMap(levelPixels, levelWidth, levelHeight, out levelWidth, out levelHeight);
                }
            }

            // Normal maps are always linear (never sRGB, isDataSRGB = false), matching UnityTextureCompressor's
            // output, so always -cl.
            var result = TryCompressLevels(
                texture,
                logPrefix: "normal map ",
                levelSizes,
                blockX,
                blockY,
                format.Value,
                srgb: false,
                mipChain: levelSizes.Count > 1,
                linear: true,
                makeNoLongerReadable: !readable,
                WriteLevelTga,
                postProcess: r =>
                {
                    // Matches UnityTextureCompressor.CompressNormalMap, which always enables streaming mipmaps via
                    // TextureGenerationSettings for its output.
                    TextureUtility.SetStreamingMipMaps(r, true);
                });

            if (result != null)
            {
                return new ResultRequest<Texture2D>(result, completion);
            }

            // Unlike CompressTexture, the input is never destroyed here (on success or failure): this mirrors
            // UnityTextureCompressor.CompressNormalMap, which also leaves the input texture untouched and always
            // returns a distinct new instance from TextureGenerator.
            return fallback.CompressNormalMap(texture, format, readable, maxTextureSize, completion);
        }

        /// <summary>
        /// Runs the shared astcenc compression pipeline for both <see cref="CompressTexture"/> and
        /// <see cref="CompressNormalMap"/>. For each level in <paramref name="levels"/> (in order, level 0
        /// first): invokes <paramref name="writeLevelTga"/> to produce that level's TGA input file, runs astcenc
        /// on it, validates the resulting .astc file's header, and copies its raw block data directly into a
        /// single combined buffer at the correct offset (the buffer's total size is known up front from the mip
        /// dimensions alone, so it is allocated once rather than built up through a growable list). The combined
        /// buffer is then loaded into a new <see cref="Texture2D"/>, and <paramref name="texture"/>'s name,
        /// wrap mode, filter mode, and aniso level are copied onto it.
        /// </summary>
        /// <remarks>
        /// Any exception raised while doing the above (I/O, a failed astcenc invocation, a malformed .astc file)
        /// is caught here, logged as a warning naming <paramref name="texture"/>, and reported to the caller as
        /// a failed attempt (null return; any partially-built result <see cref="Texture2D"/> is destroyed) rather
        /// than propagated, so <see cref="CompressTexture"/>/<see cref="CompressNormalMap"/> can fall back to
        /// <see cref="UnityTextureCompressor"/>. Temp files recorded for each level are always deleted in a
        /// finally block, on both success and failure.
        /// </remarks>
        /// <param name="texture">Source texture; used only for its name/wrap/filter/aniso settings and as the log context object, never read for pixel data (that is <paramref name="writeLevelTga"/>'s job).</param>
        /// <param name="logPrefix">Prefix inserted into the warning/exception messages to distinguish the normal map path ("normal map ") from the color path (""), matching the wording used before this pipeline was shared.</param>
        /// <param name="levels">Width/height of each mip level to encode, in order (level 0 first). Level 0's size is also used as the result texture's dimensions.</param>
        /// <param name="blockX">ASTC block width.</param>
        /// <param name="blockY">ASTC block height.</param>
        /// <param name="format">Target ASTC texture format.</param>
        /// <param name="srgb">Whether to invoke astcenc with sRGB (-cs) or linear (-cl) encoding.</param>
        /// <param name="mipChain">Whether the result texture should allocate a mip chain (i.e. <c>levels.Count > 1</c>).</param>
        /// <param name="linear">Whether the result texture's color space is linear (the inverse of its <c>isDataSRGB</c>).</param>
        /// <param name="makeNoLongerReadable">Passed through to <see cref="Texture2D.Apply(bool, bool)"/> as the result's <c>makeNoLongerReadable</c> argument.</param>
        /// <param name="writeLevelTga">Callback that writes the given level's TGA input file to the given path; also responsible for advancing any per-level source pixel state (e.g. the normal map path's mip chain downsampling).</param>
        /// <param name="postProcess">Optional callback invoked on the result texture, inside the try block, right before returning; used by <see cref="CompressNormalMap"/> to enable streaming mipmaps. Any exception it throws is treated the same as a compression failure.</param>
        /// <returns>The compressed <see cref="Texture2D"/> on success, or null if compression failed (a warning was already logged).</returns>
        private Texture2D TryCompressLevels(
            Texture2D texture,
            string logPrefix,
            IReadOnlyList<(int Width, int Height)> levels,
            int blockX,
            int blockY,
            TextureFormat format,
            bool srgb,
            bool mipChain,
            bool linear,
            bool makeNoLongerReadable,
            Action<int, string> writeLevelTga,
            Action<Texture2D> postProcess)
        {
            var tempFiles = new List<string>();
            Texture2D result = null;
            try
            {
                var jobs = Math.Max(1, SystemInfo.processorCount);
                var blockSize = AstcUtility.GetBlockSizeString(format);

                // Total compressed size is known up front from the mip dimensions alone (independent of the
                // astcenc run), so the destination buffer is allocated once and each mip's decoded block data is
                // copied directly to its final offset instead of being buffered through a growable List<byte>.
                var levelDataSizes = new int[levels.Count];
                var combinedSize = 0;
                for (var i = 0; i < levels.Count; i++)
                {
                    levelDataSizes[i] = AstcUtility.GetMipDataSize(levels[i].Width, levels[i].Height, blockX, blockY);
                    combinedSize += levelDataSizes[i];
                }
                var combined = new byte[combinedSize];
                var combinedOffset = 0;

                Directory.CreateDirectory(Path.GetFullPath(AstcencCli.TempDirectory));

                for (var level = 0; level < levels.Count; level++)
                {
                    var (w, h) = levels[level];

                    var id = Guid.NewGuid().ToString("N");
                    var tgaPath = Path.GetFullPath(Path.Combine(AstcencCli.TempDirectory, $"{id}.tga"));
                    var astcPath = Path.GetFullPath(Path.Combine(AstcencCli.TempDirectory, $"{id}.astc"));
                    tempFiles.Add(tgaPath);
                    tempFiles.Add(astcPath);

                    writeLevelTga(level, tgaPath);

                    var runResult = AstcencCli.RunCompress(exePath, tgaPath, astcPath, blockSize, preset, srgb, jobs, CompressTimeoutMs);
                    if (!runResult.Success)
                    {
                        throw new InvalidOperationException($"astcenc failed for {logPrefix}\"{texture.name}\" mip level {level} (exitCode={runResult.ExitCode}, timedOut={runResult.TimedOut}): {runResult.StdErr}");
                    }

                    var astcFileData = File.ReadAllBytes(astcPath);
                    AstcUtility.StripAstcHeader(astcFileData, w, h, blockX, blockY, combined, combinedOffset);
                    combinedOffset += levelDataSizes[level];
                }

                result = new Texture2D(levels[0].Width, levels[0].Height, format, mipChain, linear);
                result.LoadRawTextureData(combined);

                // updateMipmaps=false: mip data was already generated by the caller and encoded per level above.
                result.Apply(false, makeNoLongerReadable);
                result.name = texture.name;
                result.wrapMode = texture.wrapMode;
                result.filterMode = texture.filterMode;
                result.anisoLevel = texture.anisoLevel;

                postProcess?.Invoke(result);

                SuccessfulCompressionCount++;
                return result;
            }
            catch (Exception e)
            {
                Logger.LogWarning($"astcenc {logPrefix}compression failed for texture \"{texture.name}\", falling back to Unity's texture compression. {e.Message}", texture);
                if (result != null)
                {
                    UnityEngine.Object.DestroyImmediate(result);
                }
                return null;
            }
            finally
            {
                foreach (var path in tempFiles)
                {
                    AstcencCli.DeleteFileSilently(path);
                }
            }
        }
    }
}
