// <copyright file="AstcencTextureCompressor.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        /// Determined empirically: true (top-left origin) is the value that makes astcenc's output match Unity's
        /// own ASTC encoder for both input paths --
        /// AstcencTextureCompressorTests.CompressTexture_Orientation_MatchesUnityCompressor for the color path
        /// (<see cref="Texture2D.GetRawTextureData()"/> input, measured diff 0.0000 with true vs. 0.20 with
        /// false), and AstcencNormalMapCompressionTests.CompressNormalMap_Orientation_MatchesUnityCompressor for
        /// the normal map path (<see cref="Texture2D.GetPixels32(int)"/> input, measured diff 0.0000 with true vs.
        /// 0.0345 with false).
        /// </para>
        /// <para>
        /// This is not what <see cref="AstcUtility.WriteTga(Color32[], int, int, bool, string)"/>'s own remarks
        /// would predict for <c>GetPixels32</c> input: those remarks document row 0 of <c>GetPixels32</c>'s array
        /// as the *bottom* row, which would call for topToBottom = false (the value that measured worse above).
        /// Since both paths -- one from <c>GetRawTextureData</c>, one from <c>GetPixels32</c> -- empirically want
        /// true regardless, the row-order premise behind that documented default is itself unverified and
        /// unresolved here; see <see cref="AstcUtility.WriteTga(Color32[], int, int, bool, string)"/>'s remarks
        /// for the caveat. Correctness of this constant is guarded by the two orientation tests above, not by that
        /// premise: both compare against Unity's own encoder (diff approx. 0) and will fail if this constant, or
        /// any surrounding row-order assumption, ever needs to change.
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
            // aborted previous session (e.g. an editor crash) -- this constructor runs before any astcenc
            // invocation in this session (synchronous or the async background-thread path) has written its own
            // temp files yet, and each invocation cleans up its own temp files in a finally block regardless.
            // Safe to clear it out up front.
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

        /// <summary>
        /// Gets the number of threads to pass to astcenc's -j: every core.
        /// </summary>
        /// <remarks>
        /// astcenc saturates whatever it is given, so this trades how responsive the editor stays during a
        /// background preview compression against how soon the compressed result appears. Reserving one core for
        /// the editor was tried and turned out not to be needed in practice, so the whole machine is used and
        /// each texture finishes as quickly as possible. Read on the main thread only, before handing the value
        /// to the worker.
        /// </remarks>
        private static int CompressionJobs => Math.Max(1, SystemInfo.processorCount);

        /// <inheritdoc/>
        public AsyncCallbackRequest CompressTexture(Texture2D texture, TextureFormat format, Action<Texture2D> completion)
        {
            if (!AstcUtility.TryGetBlockSize(format, out var blockX, out var blockY))
            {
                // Not a path astcenc can handle (non-ASTC target format). This is a normal, expected fallback,
                // not an error, so no warning is logged.
                Logger.LogDebug($"astcenc skipped for \"{texture.name}\" (target format {format} is not an ASTC format); using Unity's texture compression.", texture);
                return fallback.CompressTexture(texture, format, completion);
            }

            if (texture.format != TextureFormat.RGBA32)
            {
                // Not a path astcenc can handle (an already-compressed source). This is a normal, expected
                // fallback, not an error, so no warning is logged.
                Logger.LogDebug($"astcenc skipped for \"{texture.name}\" (source format {texture.format} is not RGBA32); using Unity's texture compression.", texture);
                return fallback.CompressTexture(texture, format, completion);
            }

            // The byte[] overload of GetRawTextureData must be used here: it succeeds even when isReadable is
            // false (the state of every baked texture after readback, which calls Apply(updateMipmaps,
            // makeNoLongerReadable: true)), while the generic NativeArray overload GetRawTextureData<T>() throws
            // UnityException for non-readable textures.
            //
            // This read is wrapped in its own try/catch (rather than relying on TryCompressLevels' try/catch,
            // which does not start until after this call) so a read failure here still gets the same
            // warn-and-fall-back treatment as a failure during the astcenc run itself, instead of propagating as
            // an unhandled exception out of CompressTexture.
            byte[] raw;
            try
            {
                raw = texture.GetRawTextureData();
            }
            catch (Exception e)
            {
                Logger.LogWarning($"astcenc compression failed for texture \"{texture.name}\", falling back to Unity's texture compression. {e.Message}", texture);
                return fallback.CompressTexture(texture, format, completion);
            }

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
            if (!format.HasValue)
            {
                // A non-mobile normal map left for TextureGenerator to decide. This is a normal, expected
                // fallback, not an error, so no warning is logged.
                Logger.LogDebug($"astcenc skipped for normal map \"{texture.name}\" (no target format specified); using Unity's texture compression.", texture);
                return fallback.CompressNormalMap(texture, format, readable, maxTextureSize, completion);
            }

            if (!AstcUtility.TryGetBlockSize(format.Value, out var blockX, out var blockY))
            {
                // An unsupported ASTC format. This is a normal, expected fallback, not an error, so no warning
                // is logged.
                Logger.LogDebug($"astcenc skipped for normal map \"{texture.name}\" (target format {format.Value} is not an ASTC format); using Unity's texture compression.", texture);
                return fallback.CompressNormalMap(texture, format, readable, maxTextureSize, completion);
            }

            if (!texture.isReadable)
            {
                // A non-readable input. This is a normal, expected fallback, not an error, so no warning is logged.
                Logger.LogDebug($"astcenc skipped for normal map \"{texture.name}\" (texture is not readable); using Unity's texture compression.", texture);
                return fallback.CompressNormalMap(texture, format, readable, maxTextureSize, completion);
            }

            // Wrapped in its own try/catch (rather than relying on TryCompressLevels' try/catch, which does not
            // start until much later, after the maxTextureSize/alpha preprocessing below) so a read failure here
            // still gets the same warn-and-fall-back treatment as a failure during the astcenc run itself,
            // instead of propagating as an unhandled exception out of CompressNormalMap.
            Color32[] pixels;
            try
            {
                pixels = texture.GetPixels32(0);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"astcenc normal map compression failed for texture \"{texture.name}\", falling back to Unity's texture compression. {e.Message}", texture);
                return fallback.CompressNormalMap(texture, format, readable, maxTextureSize, completion);
            }

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

            // Math.Max(1, ...): defends the loop below against an infinite spin if targetMaxSize were ever <= 0.
            // Not currently reachable -- TextureUtility.NormalizeMaxTextureSize already maps maxTextureSize values
            // <= 0 to null before they can reach here, and currentMaxSize is always >= 1 -- but the loop condition
            // alone (Math.Max(width, height) > targetMaxSize) never becomes false on its own if targetMaxSize <= 0,
            // since width/height are clamped to a minimum of 1 by DownsampleNormalMap.
            var targetMaxSize = Math.Max(1, maxTextureSize.HasValue ? Math.Min(maxTextureSize.Value, currentMaxSize) : currentMaxSize);
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
        /// Serializes background astcenc process execution across every <see cref="CompressTextureAsync"/> /
        /// <see cref="CompressNormalMapAsync"/> call, regardless of which compressor instance (final or preview
        /// preset) invoked it: astcenc already parallelizes internally via <c>-j</c> across every CPU core, so
        /// running two astcenc processes at once would only make them fight over the same cores rather than
        /// finish sooner.
        /// </summary>
        /// <remarks>
        /// Currently redundant in practice: <see cref="PreviewTextureCompressionQueue"/> is the only caller of
        /// these two methods, and it already serializes them itself (its own <c>processing</c> flag allows at
        /// most one item -- and therefore at most one <see cref="CompressTextureAsync"/>/<see cref="CompressNormalMapAsync"/>
        /// call -- in flight at a time). This gate is kept anyway as a safety net for if a second caller of these
        /// async methods is ever added outside that queue's own serialization, so it cannot silently regress into
        /// the core-contention problem described above.
        /// </remarks>
        private static readonly SemaphoreSlim AsyncCompressionGate = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Async, off-main-thread counterpart to <see cref="CompressTexture"/>, used only by the NDMF preview's
        /// progressive texture replacement queue (<see cref="PreviewTextureCompressionQueue"/>).
        /// Pixel extraction (<see cref="Texture2D.GetRawTextureData()"/>) and the final <see cref="Texture2D"/>
        /// construction happen on the calling thread, which must be Unity's main thread (this method awaits back
        /// onto it via the captured <see cref="SynchronizationContext"/>, the same mechanism the Unity editor
        /// uses for every other main-thread-resuming await); the astcenc process invocation itself -- the part
        /// that would otherwise stall the editor for the duration of compression -- runs on a background thread
        /// pool thread via <see cref="Task.Run{TResult}(Func{TResult})"/> and touches no Unity API (not even logging).
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="CompressTexture"/>, this never falls back to <see cref="UnityTextureCompressor"/> on
        /// failure -- that fallback is itself main-thread-only and would defeat the point of running here.
        /// Instead it throws, and the caller (<see cref="PreviewTextureCompressionQueue"/>) is expected to catch the
        /// exception, log a warning, and leave the pre-compression placeholder texture on screen as-is. The input
        /// <paramref name="texture"/> is never destroyed, on success or failure: unlike <see cref="CompressTexture"/>,
        /// which owns and destroys its input, the progressive queue's placeholder is the very texture already
        /// assigned to preview materials, so it must stay valid until the queue itself swaps it out.
        /// </remarks>
        /// <param name="texture">Source texture (RGBA32). Only read on the calling thread, before the awaited work starts.</param>
        /// <param name="format">Target ASTC format.</param>
        /// <param name="cancellationToken">Checked before dispatching astcenc for each mip level (never mid-level): once cancelled, no further astcenc process is started and the awaited task ends with <see cref="OperationCanceledException"/> instead of a result. Used by <see cref="PreviewTextureCompressionQueue"/> so a domain reload or editor quit stops the worker from starting more astcenc processes after the caller has separately killed whichever one was already running (see <see cref="AstcencCli.KillAllRunningProcesses"/>).</param>
        /// <returns>The compressed <see cref="Texture2D"/> on success.</returns>
        /// <exception cref="NotSupportedException">Thrown when this format/texture combination is not something the astcenc path can handle (e.g. a non-ASTC format or an already-compressed source). Callers should use the synchronous <see cref="CompressTexture"/> facade instead, which knows how to fall back to <see cref="UnityTextureCompressor"/>.</exception>
        /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is cancelled before every mip level finished compressing.</exception>
        internal async Task<Texture2D> CompressTextureAsync(Texture2D texture, TextureFormat format, CancellationToken cancellationToken = default)
        {
            if (!AstcUtility.TryGetBlockSize(format, out var blockX, out var blockY))
            {
                Logger.LogDebug($"astcenc skipped for \"{texture.name}\" (target format {format} is not an ASTC format); the caller will fall back to Unity's texture compression.", texture);
                throw new NotSupportedException($"astcenc cannot asynchronously compress texture \"{texture.name}\" ({texture.format}) to {format}.");
            }

            if (texture.format != TextureFormat.RGBA32)
            {
                Logger.LogDebug($"astcenc skipped for \"{texture.name}\" (source format {texture.format} is not RGBA32); the caller will fall back to Unity's texture compression.", texture);
                throw new NotSupportedException($"astcenc cannot asynchronously compress texture \"{texture.name}\" ({texture.format}) to {format}.");
            }

            // See CompressTexture's identical call for why the byte[] overload (not GetRawTextureData<T>()) is used.
            var raw = texture.GetRawTextureData();

            var mipmapCount = Math.Max(1, texture.mipmapCount);
            var levels = new List<(int Width, int Height)>(mipmapCount);
            for (var level = 0; level < mipmapCount; level++)
            {
                levels.Add((Math.Max(1, texture.width >> level), Math.Max(1, texture.height >> level)));
            }

            // Every remaining read of `texture` happens before the first await below, so capturing these now is
            // not strictly required for correctness (the caller must not touch `texture` from another thread
            // concurrently either way) -- it documents that nothing past this point depends on `texture` still
            // being a live, non-destroyed Unity object, and gives the worker a plain string for error messages.
            var name = texture.name;
            var wrapMode = texture.wrapMode;
            var filterMode = texture.filterMode;
            var anisoLevel = texture.anisoLevel;
            var srgb = texture.isDataSRGB;
            var linear = !texture.isDataSRGB;
            var mipChain = mipmapCount > 1;
            var jobs = CompressionJobs;

            var offset = 0;
            void WriteLevelTga(int level, string tgaPath)
            {
                var (w, h) = levels[level];
                var levelBytes = w * h * 4;
                if (offset + levelBytes > raw.Length)
                {
                    throw new InvalidDataException($"Raw texture data for \"{name}\" is too short for mip level {level} (offset={offset}, needed={levelBytes}, total={raw.Length}).");
                }

                AstcUtility.WriteTga(raw, offset, levelBytes, w, h, TgaTopToBottomOrigin, tgaPath);
                offset += levelBytes;
            }

            byte[] combined;
            var stopwatch = Stopwatch.StartNew();
            await AsyncCompressionGate.WaitAsync(cancellationToken);
            try
            {
                combined = await Task.Run(() => CompressLevelsWorker(exePath, preset, name, string.Empty, levels, blockX, blockY, format, srgb, jobs, cancellationToken, WriteLevelTga));
            }
            finally
            {
                AsyncCompressionGate.Release();
            }
            stopwatch.Stop();

            // Back on the main thread (the awaits above resume via the calling thread's SynchronizationContext):
            // safe to touch Unity API again from here on.
            var result = new Texture2D(levels[0].Width, levels[0].Height, format, mipChain, linear);
            result.LoadRawTextureData(combined);

            // makeNoLongerReadable=false: matches CompressTexture's sync path -- PreviewTextureCompressionQueue
            // reads the result back via GetRawTextureData() right after compression to save the disk cache entry.
            result.Apply(false, false);
            result.name = name;
            result.wrapMode = wrapMode;
            result.filterMode = filterMode;
            result.anisoLevel = anisoLevel;

            SuccessfulCompressionCount++;
            Logger.LogDebug($"astcenc compressed texture \"{name}\" to {format} ({result.width}x{result.height}, {levels.Count} mips, {preset}, -j {jobs}) in {stopwatch.ElapsedMilliseconds} ms (background).", result);
            return result;
        }

        /// <summary>
        /// Async, off-main-thread counterpart to <see cref="CompressNormalMap"/>. See
        /// <see cref="CompressTextureAsync(Texture2D, TextureFormat, CancellationToken)"/>'s remarks for the threading contract
        /// (main thread before/after, background thread pool thread for the astcenc process itself) and failure
        /// semantics (throws instead of falling back; input is never destroyed).
        /// </summary>
        /// <param name="texture">Normal map texture (RGB); must already be readable.</param>
        /// <param name="format">Format to compress to. Must be a supported ASTC format (non-null).</param>
        /// <param name="readable">Whether to make the output texture readable.</param>
        /// <param name="maxTextureSize">Optional max texture size override.</param>
        /// <param name="cancellationToken">See <see cref="CompressTextureAsync(Texture2D, TextureFormat, CancellationToken)"/>'s identical parameter for the cancellation contract.</param>
        /// <returns>The compressed normal map on success.</returns>
        /// <exception cref="NotSupportedException">Thrown when this format/texture combination is not something the astcenc path can handle (e.g. no format, an unsupported ASTC format, or a non-readable input). Callers should use the synchronous <see cref="CompressNormalMap"/> facade instead.</exception>
        /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is cancelled before every mip level finished compressing.</exception>
        internal async Task<Texture2D> CompressNormalMapAsync(Texture2D texture, TextureFormat? format, bool readable, int? maxTextureSize, CancellationToken cancellationToken = default)
        {
            if (!format.HasValue)
            {
                Logger.LogDebug($"astcenc skipped for normal map \"{texture.name}\" (no target format specified); the caller will fall back to Unity's texture compression.", texture);
                throw new NotSupportedException($"astcenc cannot asynchronously compress normal map \"{texture.name}\" to {format}.");
            }

            if (!AstcUtility.TryGetBlockSize(format.Value, out var blockX, out var blockY))
            {
                Logger.LogDebug($"astcenc skipped for normal map \"{texture.name}\" (target format {format.Value} is not an ASTC format); the caller will fall back to Unity's texture compression.", texture);
                throw new NotSupportedException($"astcenc cannot asynchronously compress normal map \"{texture.name}\" to {format}.");
            }

            if (!texture.isReadable)
            {
                Logger.LogDebug($"astcenc skipped for normal map \"{texture.name}\" (texture is not readable); the caller will fall back to Unity's texture compression.", texture);
                throw new NotSupportedException($"astcenc cannot asynchronously compress normal map \"{texture.name}\" to {format}.");
            }

            var pixels = texture.GetPixels32(0);
            var width = texture.width;
            var height = texture.height;
            var name = texture.name;
            var wrapMode = texture.wrapMode;
            var filterMode = texture.filterMode;
            var anisoLevel = texture.anisoLevel;

            // Mirrors CompressNormalMap's maxTextureSize handling; see its comments for details.
            var currentMaxSize = Math.Max(width, height);
            var targetMaxSize = Math.Max(1, maxTextureSize.HasValue ? Math.Min(maxTextureSize.Value, currentMaxSize) : currentMaxSize);
            while (Math.Max(width, height) > targetMaxSize)
            {
                pixels = NormalMapMipUtility.DownsampleNormalMap(pixels, width, height, out width, out height);
            }
            pixels = NormalMapMipUtility.ForceOpaqueAlpha(pixels);

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

            var jobs = CompressionJobs;

            byte[] combined;
            var stopwatch = Stopwatch.StartNew();
            await AsyncCompressionGate.WaitAsync(cancellationToken);
            try
            {
                combined = await Task.Run(() => CompressLevelsWorker(exePath, preset, name, "normal map ", levelSizes, blockX, blockY, format.Value, false, jobs, cancellationToken, WriteLevelTga));
            }
            finally
            {
                AsyncCompressionGate.Release();
            }
            stopwatch.Stop();

            // Back on the main thread; see CompressTextureAsync's identical comment.
            var result = new Texture2D(width, height, format.Value, levelSizes.Count > 1, true);
            result.LoadRawTextureData(combined);
            result.Apply(false, !readable);
            result.name = name;
            result.wrapMode = wrapMode;
            result.filterMode = filterMode;
            result.anisoLevel = anisoLevel;

            // Matches CompressNormalMap's postProcess, which always enables streaming mipmaps for its output.
            TextureUtility.SetStreamingMipMaps(result, true);

            SuccessfulCompressionCount++;
            Logger.LogDebug($"astcenc compressed normal map \"{name}\" to {format.Value} ({result.width}x{result.height}, {levelSizes.Count} mips, {preset}, -j {jobs}) in {stopwatch.ElapsedMilliseconds} ms (background).", result);
            return result;
        }

        /// <summary>
        /// Worker-thread-safe core shared by <see cref="CompressTextureAsync"/> and
        /// <see cref="CompressNormalMapAsync"/>: writes each level's TGA, runs astcenc on it, and copies the
        /// resulting raw block data into a single combined buffer. Touches no Unity engine API -- not even
        /// logging, since <see cref="Debug.Log(object)"/> is main-thread-only in the editor -- so it is safe to
        /// invoke via <see cref="Task.Run{TResult}(Func{TResult})"/>. Reports failure by throwing rather than logging;
        /// callers on the main thread are responsible for catching, logging (using the plain string parameters
        /// captured before dispatch, since the source <see cref="Texture2D"/> itself must not be touched from
        /// this thread), and falling back.
        /// </summary>
        /// <param name="exePath">Full path to the astcenc executable.</param>
        /// <param name="preset">Quality preset with a leading dash (e.g. "-medium").</param>
        /// <param name="textureNameForErrors">Source texture name, captured on the main thread, for error messages only.</param>
        /// <param name="logPrefix">Prefix inserted into error messages ("normal map " or "").</param>
        /// <param name="levels">Width/height of each mip level to encode, in order (level 0 first).</param>
        /// <param name="blockX">ASTC block width.</param>
        /// <param name="blockY">ASTC block height.</param>
        /// <param name="format">Target ASTC texture format.</param>
        /// <param name="srgb">Whether to invoke astcenc with sRGB (-cs) or linear (-cl) encoding.</param>
        /// <param name="jobs">Number of threads to pass to astcenc's -j.</param>
        /// <param name="cancellationToken">Checked (via <see cref="CancellationToken.ThrowIfCancellationRequested"/>) immediately before dispatching each mip level's astcenc process, never mid-level: once cancelled, no further astcenc process is started here. Does not touch any Unity API, so safe to check from this worker thread.</param>
        /// <param name="writeLevelTga">Callback that writes the given level's TGA input file to the given path. Must not touch any Unity API.</param>
        /// <returns>The combined raw ASTC block data for every level, concatenated in level order.</returns>
        private static byte[] CompressLevelsWorker(
            string exePath,
            string preset,
            string textureNameForErrors,
            string logPrefix,
            IReadOnlyList<(int Width, int Height)> levels,
            int blockX,
            int blockY,
            TextureFormat format,
            bool srgb,
            int jobs,
            CancellationToken cancellationToken,
            Action<int, string> writeLevelTga)
        {
            var tempFiles = new List<string>();
            try
            {
                var blockSize = AstcUtility.GetBlockSizeString(format);

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
                    // Checked before starting this level's astcenc process (not mid-level -- there is no way to
                    // interrupt a single already-running astcenc invocation from here; that is instead
                    // AstcencCli.KillAllRunningProcesses's job, called alongside cancelling this token by
                    // PreviewTextureCompressionQueue).
                    cancellationToken.ThrowIfCancellationRequested();

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
                        throw new InvalidOperationException($"astcenc failed for {logPrefix}\"{textureNameForErrors}\" mip level {level} (exitCode={runResult.ExitCode}, timedOut={runResult.TimedOut}): {runResult.StdErr}");
                    }

                    var astcFileData = File.ReadAllBytes(astcPath);
                    AstcUtility.StripAstcHeader(astcFileData, w, h, blockX, blockY, combined, combinedOffset);
                    combinedOffset += levelDataSizes[level];
                }

                return combined;
            }
            finally
            {
                foreach (var path in tempFiles)
                {
                    AstcencCli.DeleteFileSilently(path);
                }
            }
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
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var jobs = CompressionJobs;
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
                stopwatch.Stop();
                var kind = string.IsNullOrEmpty(logPrefix) ? "texture" : "normal map";
                Logger.LogDebug($"astcenc compressed {kind} \"{result.name}\" to {format} ({result.width}x{result.height}, {levels.Count} mips, {preset}, -j {jobs}) in {stopwatch.ElapsedMilliseconds} ms (synchronous).", result);
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
