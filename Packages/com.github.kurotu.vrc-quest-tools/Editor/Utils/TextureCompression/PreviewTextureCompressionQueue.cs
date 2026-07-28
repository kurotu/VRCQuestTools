// <copyright file="PreviewTextureCompressionQueue.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Background queue for the NDMF editor preview's "progressive" texture replacement: when a texture needs
    /// ASTC compression for the preview and astcenc is available, <see cref="Models.MaterialGeneratorUtility"/>
    /// hands the uncompressed baked texture straight to the material (so the preview updates immediately, with
    /// no main-thread stall) and enqueues the same texture here for background compression via
    /// <see cref="AstcencTextureCompressor.CompressTextureAsync"/> / <see cref="AstcencTextureCompressor.CompressNormalMapAsync"/>.
    /// Once compression finishes, every cached preview material still referencing the placeholder is updated to
    /// the compressed result, the placeholder is destroyed, and every editor view is repainted.
    /// </summary>
    /// <remarks>
    /// This type lives in the main <c>VRCQuestTools-Editor</c> assembly (alongside <see cref="Models.MaterialGeneratorUtility"/>,
    /// its only caller) rather than the NDMF-gated <c>VRCQuestTools-Editor-Ndmf</c> assembly where the actual
    /// cache of preview materials (<c>KRT.VRCQuestTools.Ndmf.SharedPreviewMaterialCache</c>) lives, because the
    /// dependency only goes one way: the NDMF assembly references this one (for the NDMF preview feature as a
    /// whole), never the reverse, so this assembly cannot reference NDMF-only types directly -- and must not,
    /// since it is compiled unconditionally even when NDMF is not installed. <see cref="RegisterMaterialTextureReplacer"/>
    /// is the seam: the NDMF assembly registers its own <c>SharedPreviewMaterialCache.ReplaceTextureReferences</c>
    /// here via <c>[InitializeOnLoadMethod]</c> (so registration also survives every domain reload). Until that
    /// registration happens -- i.e. whenever NDMF is not installed -- <see cref="TryEnqueue"/> is simply never
    /// called in the first place, since nothing in this assembly runs the NDMF preview path that would call it.
    /// </remarks>
    internal static class PreviewTextureCompressionQueue
    {
        /// <summary>
        /// Estimated total bytes of pending (enqueued but not yet compressed) placeholder textures at which a
        /// single warning is logged per batch. Progressive keeps each placeholder (an uncompressed baked RGBA32
        /// texture, with mips) alive in memory until its compressed replacement is ready; a 2048x2048 RGBA32
        /// texture with a full mip chain is about 22 MB (2048*2048*4 * 4/3 for the mip chain overhead), so this
        /// is roughly 23 such textures held at once (see <see cref="MaxConcurrentCompressions"/> for how many of
        /// those are actually compressing at any moment; the rest simply wait their turn).
        /// </summary>
        /// <remarks>
        /// This is a diagnostic threshold, not a cap: passing it does not refuse the enqueue. It used to, with
        /// <see cref="Models.MaterialGeneratorUtility"/> falling back to synchronous compression -- but that
        /// traded a bounded amount of memory for the worst available outcome, a main-thread stall running
        /// astcenc at the same preset while up to <see cref="MaxConcurrentCompressions"/> background astcenc
        /// processes were already asking for every core each. It also did not actually save the memory it was
        /// meant to save, except for as long as that stall lasted: the placeholder has already been baked and
        /// assigned to the preview material by the time <see cref="TryEnqueue"/> is called, so it occupies
        /// memory either way until something replaces it. What remains is the warning, so that an unexpectedly
        /// large backlog is visible rather than silent.
        /// </remarks>
        internal const long MaxPendingBytes = 512L * 1024 * 1024;

        /// <summary>
        /// Real, core-count-derived value of <see cref="MaxConcurrentCompressions"/> (i.e. ignoring
        /// <see cref="MaxConcurrentCompressionsOverrideForTesting"/>). Computed once: <see cref="SystemInfo.processorCount"/>
        /// does not change during a session.
        /// </summary>
        private static readonly int DefaultMaxConcurrentCompressions = Math.Max(1, Math.Min(3, SystemInfo.processorCount / 4));

        // Only ever touched via Count/indexed-access (Pending[0]) or whole-list operations (Add/RemoveAt(0)/Clear),
        // never foreach'd: with more than one item allowed in flight at once (see MaxConcurrentCompressions), more
        // than one item's continuation (inside ProcessItemAsync, resumed via the captured SynchronizationContext)
        // can run within the same editor update tick -- e.g. one continuation calling TryEnqueue (via a fresh
        // preview regeneration it triggers) while another is still unwinding. An enumerator-based traversal here
        // would be unsafe under that kind of reentrancy; indexed/whole-list access is not.
        private static readonly List<PendingItem> Pending = new List<PendingItem>();

        private static Func<Texture, Texture, int> materialTextureReplacer;
        private static long pendingBytes;
        private static bool updateHooked;
        private static int inFlight;
        private static bool assemblyReloading;
        private static bool highWaterWarned;
        private static CancellationTokenSource cts = new CancellationTokenSource();

        static PreviewTextureCompressionQueue()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            EditorApplication.quitting += OnEditorQuitting;
        }

        /// <summary>
        /// Gets the maximum number of items allowed to be compressing at once (dispatched but not yet finished).
        /// </summary>
        /// <remarks>
        /// <para>
        /// The problem this solves is not astcenc being too slow: each item's astcenc process already asks for
        /// every CPU core via <c>-j</c> (see <see cref="AstcencTextureCompressor.CompressTextureAsync"/>'s
        /// <c>jobs</c> parameter), so a single item alone can already saturate the machine while it is running. The
        /// problem is that a single item is not always running astcenc: <see cref="ProcessItemAsync"/> also does
        /// real main-thread work before and after the astcenc call (pixel extraction / normal map mip generation
        /// beforehand, <see cref="Texture2D"/> construction, <c>LoadRawTextureData</c>, the disk cache write,
        /// and a repaint afterward) during which no astcenc process is running at all -- CPU sits idle
        /// during that gap. Allowing a small number of items to be in flight together means the next item's astcenc
        /// process can be starting (or already running) while the previous item is still doing that main-thread
        /// work, filling the gap instead of leaving it empty.
        /// </para>
        /// <para>
        /// This is deliberately small and NOT "one per core": running <see cref="MaxConcurrentCompressions"/> astcenc
        /// processes at once, each itself asking for every core via <c>-j</c>, means the machine is asked for
        /// <see cref="MaxConcurrentCompressions"/> times its own core count -- the goal is to keep a core or two
        /// busy through the main-thread gaps described above, not to multiply total CPU demand. The OS scheduler is
        /// left to sort out the resulting modest oversubscription; on a lightly loaded editor machine this is cheap
        /// and self-correcting (idle cores simply run whichever process is ready), and going further would just
        /// mean processes fighting harder over the same cores without a matching gain. What must not regress is the
        /// single-item case: it must not be slower than before this was introduced, which is why <c>-j</c> itself
        /// (see <see cref="AstcencTextureCompressor.CompressionJobs"/>) is left untouched at every core, rather than
        /// divided by this value.
        /// </para>
        /// </remarks>
        internal static int MaxConcurrentCompressions => MaxConcurrentCompressionsOverrideForTesting ?? DefaultMaxConcurrentCompressions;

        /// <summary>
        /// Test-only override for <see cref="MaxConcurrentCompressions"/>, so tests can deterministically exercise
        /// dispatch-cap behavior (e.g. "exactly N items are ever in flight together") without depending on the
        /// actual core count of the machine running the test. Null (the default) means "use the real,
        /// core-count-derived value." Tests that set this must restore it to null in teardown: this is process-wide
        /// static state, otherwise it would leak into unrelated tests (and real preview activity) for the rest of
        /// the editor session. Does not retroactively resize <see cref="AstcencTextureCompressor"/>'s own
        /// astcenc-process concurrency gate, which is sized once from the real value.
        /// </summary>
        internal static int? MaxConcurrentCompressionsOverrideForTesting { get; set; }

        /// <summary>
        /// Test-only switch to suspend <see cref="OnUpdate"/>'s own dispatch loop (its <c>EditorApplication.update</c>
        /// subscribe/unsubscribe bookkeeping is unaffected). Needed because <see cref="OnUpdate"/> is not just an
        /// implementation detail a test can ignore: <see cref="TryEnqueue"/> subscribes it to the real
        /// <c>EditorApplication.update</c>, which a running Unity Editor session fires for real -- including during
        /// any <c>yield return null</c> a <c>[UnityTest]</c> performs while awaiting a task (e.g. inside
        /// <see cref="TestUtils.WaitForTask"/>). Without this, a test driving dispatch deterministically via
        /// <see cref="DispatchAvailableForTesting"/> or <see cref="ProcessNextForTesting"/> would race the real
        /// production dispatch loop, which is equally entitled to dequeue and dispatch the very same pending items
        /// the moment an in-flight slot frees up. Must be restored to false in teardown -- this is process-wide
        /// static state.
        /// </summary>
        internal static bool SuspendAutoDispatchForTesting { get; set; }

        /// <summary>
        /// Registers the callback used to swap a placeholder texture for its compressed replacement across every
        /// cached NDMF preview material that references it, returning how many texture properties were replaced.
        /// Called by <c>KRT.VRCQuestTools.Ndmf.SharedPreviewMaterialCache</c> (via <c>[InitializeOnLoadMethod]</c>,
        /// so this is re-registered after every domain reload) since that is the only assembly that actually
        /// knows about cached preview materials; see this type's remarks for why the dependency cannot run the
        /// other way. Calling this again (e.g. on a later domain reload) simply replaces the previous callback.
        /// </summary>
        /// <param name="replacer">Callback taking (placeholder, replacement) and returning the number of texture properties replaced across all cached preview materials.</param>
        internal static void RegisterMaterialTextureReplacer(Func<Texture, Texture, int> replacer)
        {
            materialTextureReplacer = replacer;
        }

        /// <summary>
        /// Attempts to enqueue a texture for background ASTC compression.
        /// </summary>
        /// <param name="placeholder">Uncompressed baked texture already assigned to preview material(s); becomes owned by the queue (destroyed once replaced, or left alone on failure).</param>
        /// <param name="compressor">astcenc compressor to use (the instance resolved by <see cref="TextureCompressorProvider"/>).</param>
        /// <param name="format">Target ASTC format. Must be non-null for color textures; may be non-null for normal maps only (a null format is never astcenc-compatible).</param>
        /// <param name="isNormalMap">Whether <paramref name="placeholder"/> is a normal map (uses <see cref="AstcencTextureCompressor.CompressNormalMapAsync"/>) or a color/parameter texture (uses <see cref="AstcencTextureCompressor.CompressTextureAsync"/>).</param>
        /// <param name="readable">Normal map only: whether the compressed result should remain readable.</param>
        /// <param name="maxTextureSize">Normal map only: optional max texture size override.</param>
        /// <param name="cacheFile">Disk cache file name to save the compressed result under once ready, matching what the synchronous path would have used.</param>
        /// <param name="isSRGB">Whether the texture is sRGB data; recorded into the disk cache entry as <c>!isSRGB</c> (linear), matching <see cref="Models.MaterialGeneratorUtility"/>'s synchronous save.</param>
        /// <returns>True when the texture was enqueued (the caller must not touch <paramref name="placeholder"/> or fall back to synchronous compression); false when the queue cannot take ownership at all -- no material-texture-replacer is registered (e.g. NDMF is not installed), so nothing would ever swap the compressed result in, or an assembly reload is in progress, which is about to discard the queue -- in which case the caller must fall back to synchronous compression itself and <paramref name="placeholder"/> remains entirely the caller's responsibility. A large backlog is no longer a refusal reason; see <see cref="MaxPendingBytes"/>.</returns>
        internal static bool TryEnqueue(Texture2D placeholder, AstcencTextureCompressor compressor, TextureFormat? format, bool isNormalMap, bool readable, int? maxTextureSize, string cacheFile, bool isSRGB)
        {
            if (assemblyReloading)
            {
                Logger.LogDebug($"Progressive compression queue declined \"{placeholder.name}\" (an assembly reload is in progress); the caller will fall back to synchronous compression.", placeholder);
                return false;
            }

            if (materialTextureReplacer == null)
            {
                Logger.LogDebug($"Progressive compression queue declined \"{placeholder.name}\" (no material texture replacer is registered, e.g. NDMF is not installed); the caller will fall back to synchronous compression.", placeholder);
                return false;
            }

            var estimatedBytes = EstimatePlaceholderBytes(placeholder);

            Pending.Add(new PendingItem
            {
                Placeholder = placeholder,
                Compressor = compressor,
                Format = format,
                IsNormalMap = isNormalMap,
                Readable = readable,
                MaxTextureSize = maxTextureSize,
                CacheFile = cacheFile,
                IsSRGB = isSRGB,
                EstimatedBytes = estimatedBytes,

                // Captured now (not read back from EditorUserBuildSettings.activeBuildTarget once compression
                // finishes) so a platform switch that happens while this item is queued or compressing cannot
                // desync the eventual disk cache entry's key (which embeds the build target the caller used to
                // compute cacheFile) from its contents, and so a synchronous fallback compression (see
                // ProcessItemAsync) reproduces the exact format this item was enqueued for instead of whatever
                // the active build target resolves to by the time the fallback runs.
                BuildTarget = EditorUserBuildSettings.activeBuildTarget,
            });
            pendingBytes += estimatedBytes;
            EnsureUpdateHooked();
            WarnOnceIfBacklogIsLarge();
            Logger.LogDebug($"Progressive compression queue accepted \"{placeholder.name}\" (estimated {estimatedBytes} bytes, queue length {Pending.Count}).", placeholder);
            return true;
        }

        /// <summary>
        /// Logs a single warning per batch once <see cref="pendingBytes"/> passes <see cref="MaxPendingBytes"/>.
        /// Latched via <see cref="highWaterWarned"/> and cleared when the batch drains (see
        /// <see cref="ProcessItemAsync"/>) or work is abandoned (see <see cref="StopAllWork"/>), so one oversized
        /// batch warns once rather than once per texture, while a later batch can warn again.
        /// </summary>
        private static void WarnOnceIfBacklogIsLarge()
        {
            if (highWaterWarned || pendingBytes <= MaxPendingBytes)
            {
                return;
            }

            highWaterWarned = true;
            Logger.LogWarning($"Progressive preview compression is holding about {pendingBytes / (1024 * 1024)} MB of uncompressed placeholder textures ({Pending.Count} queued, {inFlight} compressing). They are freed as compression completes; the preview shows the uncompressed textures until then.");
        }

        /// <summary>
        /// Gets the number of items currently waiting to start compression (excludes items currently compressing --
        /// see <see cref="InFlightCountForTesting"/> for those). For tests only.
        /// </summary>
        internal static int PendingCountForTesting => Pending.Count;

        /// <summary>
        /// Gets the number of items currently dispatched (compressing, not yet finished). Never exceeds
        /// <see cref="MaxConcurrentCompressions"/>. For tests only.
        /// </summary>
        internal static int InFlightCountForTesting => inFlight;

        /// <summary>
        /// Gets the estimated total bytes of every pending (not yet compressed) placeholder. For tests only.
        /// </summary>
        internal static long PendingBytesForTesting => pendingBytes;

        /// <summary>
        /// Gets the currently registered material-texture-replacer callback, or null when none is registered
        /// (e.g. NDMF is not installed, so <see cref="RegisterMaterialTextureReplacer"/> was never called since
        /// the last domain reload). For tests only: save this in a test's setup and pass it back to
        /// <see cref="RegisterMaterialTextureReplacer"/> in teardown to restore whatever was registered before
        /// the test swapped in a fake replacer (or simulated "NDMF is not installed" by registering null) --
        /// without this, a test-installed replacer would silently leak into every later test in the same editor
        /// session, since <c>[InitializeOnLoadMethod]</c> only re-registers the real one once per domain reload.
        /// </summary>
        internal static Func<Texture, Texture, int> MaterialTextureReplacerForTesting => materialTextureReplacer;

        /// <summary>
        /// Dequeues and fully processes (through completion, success or failure) a single pending item, for
        /// tests that cannot pump <see cref="EditorApplication.update"/> the way a running editor session does.
        /// This runs the exact same <see cref="ProcessItemAsync"/> production uses. Callers must be a
        /// <c>[UnityTest]</c> <c>IEnumerator</c> test method that <c>yield return</c>s <see cref="TestUtils.WaitForTask"/>
        /// on the returned task -- see that method's own remarks for why a plain <c>[Test]</c> method cannot
        /// simply <c>await</c> it directly (this project's bundled Unity Test Framework does not support
        /// <c>async Task</c> test methods under <c>[Test]</c>, and blocking synchronously via e.g. <c>Task.Wait()</c>
        /// would deadlock).
        /// </summary>
        /// <returns>A task that completes once the dequeued item finishes processing, with result true; or an already-completed task with result false if the queue was empty (or the in-flight budget, <see cref="MaxConcurrentCompressions"/>, was already exhausted).</returns>
        internal static async System.Threading.Tasks.Task<bool> ProcessNextForTesting()
        {
            if (inFlight >= MaxConcurrentCompressions || Pending.Count == 0)
            {
                return false;
            }

            DequeueAndProcess(out var task);
            await task;
            return true;
        }

        /// <summary>
        /// Test-only hook mirroring a single tick of <see cref="OnUpdate"/>'s dispatch loop: dequeues and starts as
        /// many pending items as the current in-flight budget (<see cref="MaxConcurrentCompressions"/>) allows, in
        /// one go, without awaiting any of them to completion. Unlike <see cref="ProcessNextForTesting"/> --  which
        /// fully awaits one item before returning, so two calls can never actually overlap -- the tasks returned
        /// here genuinely run concurrently (each has already been dispatched past its first real await point by the
        /// time this method returns), letting a test observe real overlap (e.g. assert
        /// <see cref="InFlightCountForTesting"/> equals the dispatched count before awaiting, then
        /// <see cref="System.Threading.Tasks.Task.WhenAll(System.Threading.Tasks.Task[])"/> the result).
        /// </summary>
        /// <returns>The tasks started by this call, in dispatch order. Empty when nothing was pending or the in-flight budget was already exhausted.</returns>
        internal static System.Threading.Tasks.Task[] DispatchAvailableForTesting()
        {
            var tasks = new List<System.Threading.Tasks.Task>();
            while (inFlight < MaxConcurrentCompressions && Pending.Count > 0)
            {
                DequeueAndProcess(out var task);
                tasks.Add(task);
            }

            return tasks.ToArray();
        }

        private static void DequeueAndProcess(out System.Threading.Tasks.Task task)
        {
            inFlight++;
            var item = Pending[0];
            Pending.RemoveAt(0);
            task = ProcessItemAsync(item);
        }

        private static void EnsureUpdateHooked()
        {
            if (updateHooked)
            {
                return;
            }

            updateHooked = true;
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            // Dispatches as many pending items as the in-flight budget allows in this single tick -- not just one
            // -- so that, e.g., enqueuing 3 items at once on a machine with MaxConcurrentCompressions == 3 starts
            // all 3 right away instead of trickling them out one per editor update. Each iteration only reads
            // `inFlight` and `Pending.Count`, both main-thread-only state (see this type's remarks), so re-entering
            // this loop synchronously for each item is safe even though ProcessItemAsync's own continuation can
            // later resume on this same thread mid-loop-of-a-*different*-tick.
            //
            // Skipped entirely when SuspendAutoDispatchForTesting is set: see that property's remarks for why a
            // test needs this to drive dispatch deterministically via DispatchAvailableForTesting/ProcessNextForTesting
            // without this real hook racing it.
            if (!SuspendAutoDispatchForTesting)
            {
                while (inFlight < MaxConcurrentCompressions && Pending.Count > 0)
                {
                    DequeueAndProcess(out var task);

                    // ProcessItemAsync already handles every failure it knows how to handle internally (falling
                    // back to synchronous compression, logging, and cleaning up any orphaned texture); this
                    // ContinueWith is only a last-resort safety net for an exception that slips past that -- e.g.
                    // a bug in the fallback path itself, or in the pendingBytes/inFlight bookkeeping in
                    // ProcessItemAsync's own finally block -- so that it is at least logged instead of becoming an
                    // unobserved task exception. Since ProcessItemAsync's own catch blocks do not rethrow, this
                    // deliberately should not normally fire; if it ever does, this is the only place logging it,
                    // so there is no risk of double-logging the same exception both here and inside ProcessItemAsync.
                    task.ContinueWith(
                        t => Logger.LogException(t.Exception),
                        System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }
            }

            if (inFlight == 0 && Pending.Count == 0 && updateHooked)
            {
                EditorApplication.update -= OnUpdate;
                updateHooked = false;

                // The batch just finished. Every item in it wrote its own disk cache entry
                // (ApplyCompressedResult), all of them after the conversion that enqueued them already returned
                // and trimmed the cache -- so without trimming here too, a preview-only session (which never
                // runs a real conversion) would keep growing the cache past its configured limit.
                TrimTextureCache();
            }
        }

        /// <summary>
        /// Trims the texture cache down to the configured limit, ignoring failures: this is opportunistic
        /// maintenance running inside an <c>EditorApplication.update</c> callback, where a throw would just
        /// spam the console every tick.
        /// </summary>
        private static void TrimTextureCache()
        {
            try
            {
                CacheManager.Texture.Clear(Models.VRCQuestToolsSettings.TextureCacheSize);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Failed to trim the texture cache after progressive preview compression. {e.Message}");
            }
        }

        private static async System.Threading.Tasks.Task ProcessItemAsync(PendingItem item)
        {
            try
            {
                if (item.Placeholder == null)
                {
                    // Destroyed (e.g. every preview material lease referencing it was released, such as while
                    // rapidly dragging a settings slider) before this item even reached the front of the queue.
                    // Nothing to compress or apply.
                    return;
                }

                Texture2D compressed;
                try
                {
                    var token = cts.Token;
                    compressed = item.IsNormalMap
                        ? await item.Compressor.CompressNormalMapAsync(item.Placeholder, item.Format, item.Readable, item.MaxTextureSize, token)
                        : await item.Compressor.CompressTextureAsync(item.Placeholder, item.Format.Value, token);
                }
                catch (Exception e)
                {
                    if (cts.IsCancellationRequested)
                    {
                        // A domain reload or editor quit is in progress (see OnBeforeAssemblyReload /
                        // OnEditorQuitting). This is either a cooperative OperationCanceledException raised
                        // before starting the next mip level, or the in-flight astcenc process was killed by
                        // AstcencCli.KillAllRunningProcesses and surfaced here as an ordinary compression
                        // failure instead. Either way, do not spawn more work (a synchronous fallback
                        // compression) while the editor is tearing down -- the placeholder simply stays
                        // uncompressed on screen, same as before progressive existed.
                        return;
                    }

                    // The placeholder (uncompressed) stays assigned to preview materials and keeps rendering
                    // while the fallback below runs.
                    FallbackToSynchronousCompression(item, e);
                    return;
                }

                try
                {
                    // checkPlaceholderStillAlive: true -- the background attempt just spent real time (a whole
                    // astcenc process) off-thread, during which the placeholder could genuinely have been
                    // destroyed out from under this item externally (e.g. every preview material lease referencing
                    // it was released while compression ran). CompressTextureAsync/CompressNormalMapAsync
                    // themselves never destroy their input, so if item.Placeholder is dead here, something else did.
                    ApplyCompressedResult(item, compressed, checkPlaceholderStillAlive: true);
                }
                catch (Exception e)
                {
                    // The replacer callback (owned by the NDMF assembly) or a downstream step (disk cache save,
                    // placeholder destruction, repaint) threw. Logged and cleaned up here, rather than left to
                    // propagate: OnUpdate's ContinueWith is only a safety net for exceptions that were not
                    // already handled, so this must not rethrow (that would both double-log the same exception
                    // there and via this catch).
                    var name = item.Placeholder != null ? item.Placeholder.name : "<destroyed>";
                    Logger.LogWarning($"Applying progressively compressed preview texture \"{name}\" failed. {e.Message}");
                    TextureUtility.DestroyTexture(compressed);
                }
            }
            finally
            {
                // Both clamped rather than allowed to go negative: OnBeforeAssemblyReload/OnEditorQuitting reset
                // both counters to zero immediately (so a reload/quit is not blocked waiting on in-flight work),
                // but every still-in-flight item's own finally (there can be more than one at once, up to
                // MaxConcurrentCompressions) runs afterwards and would otherwise subtract its own share a second
                // (or, with several in-flight items, several times over) time.
                pendingBytes = Math.Max(0, pendingBytes - item.EstimatedBytes);
                inFlight = Math.Max(0, inFlight - 1);

                // Batch drained: re-arm the backlog warning for the next one. Keyed on the item counts rather
                // than pendingBytes, so it also re-arms under the test hooks, which can leave the byte counter
                // at an arbitrary forced value.
                if (inFlight == 0 && Pending.Count == 0)
                {
                    highWaterWarned = false;
                }
            }
        }

        /// <summary>
        /// Falls back to synchronous compression (<see cref="TextureUtility.CompressTextureForBuildTarget"/> /
        /// <see cref="TextureUtility.CompressNormalMap"/>, which produce the exact same bytes as the abandoned
        /// background attempt since both use the same astcenc preset) when the background astcenc attempt for
        /// <paramref name="item"/> failed, so a single transient failure does not leave the placeholder
        /// uncompressed -- and re-compressed from scratch on every subsequent preview regeneration, since a
        /// failed background attempt never writes a disk cache entry -- for the rest of the editor session.
        /// </summary>
        /// <param name="item">The item whose background compression attempt failed.</param>
        /// <param name="asyncException">The exception the background attempt threw, included in the logged warning.</param>
        private static void FallbackToSynchronousCompression(PendingItem item, Exception asyncException)
        {
            if (item.Placeholder == null)
            {
                // Destroyed while the background attempt was in flight; nothing left to fall back for.
                return;
            }

            var name = item.Placeholder.name;
            Texture2D compressed;
            try
            {
                compressed = item.IsNormalMap
                    ? TextureUtility.CompressNormalMap(item.Placeholder, item.BuildTarget, item.Format.Value, item.Readable, item.MaxTextureSize)
                    : TextureUtility.CompressTextureForBuildTarget(item.Placeholder, item.BuildTarget, item.Format.Value, item.MaxTextureSize);
            }
            catch (Exception e)
            {
                // The placeholder (uncompressed) stays assigned to preview materials and keeps rendering; both
                // the background and the synchronous fallback attempt were abandoned.
                Logger.LogWarning($"Progressive ASTC compression failed for preview texture \"{name}\", and the synchronous fallback also failed; the uncompressed preview texture remains displayed. Background error: {asyncException.Message} Fallback error: {e.Message}");
                return;
            }

            Logger.LogWarning($"Background ASTC compression failed for preview texture \"{name}\"; fell back to synchronous compression. {asyncException.Message}");

            try
            {
                // checkPlaceholderStillAlive: false -- unlike the background astcenc path, the synchronous
                // compression facades used above (TextureUtility.CompressTextureForBuildTarget / CompressNormalMap)
                // may consume the placeholder as a normal part of a *successful* compression, in one of two ways
                // depending on which backend the format selects: the astcenc backend destroys it and returns a new
                // instance, while the Unity backend compresses it in place and returns that same object. So here
                // item.Placeholder reading as destroyed (or as identical to the result) is expected rather than a
                // sign that something external abandoned it, and it is still correct to pass as the replacer's
                // `from`: this method only compares the reference for identity, never dereferences its (possibly
                // freed) contents, and UnityEngine.Object's overridden equality still matches a destroyed object
                // against itself by instance ID, so every preview material reference is still found and updated.
                ApplyCompressedResult(item, compressed, checkPlaceholderStillAlive: false);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Applying the synchronous fallback compression result for preview texture \"{name}\" failed. {e.Message}");
                TextureUtility.DestroyTexture(compressed);
            }
        }

        /// <summary>
        /// Shared success path for both the background compression attempt and its synchronous fallback: enables
        /// streaming mipmaps on the result, saves the disk cache entry, replaces every cached preview material
        /// reference to the placeholder, destroys the placeholder, and repaints every editor view.
        /// </summary>
        /// <remarks>
        /// The disk cache entry is written before -- and independently of -- everything to do with the preview
        /// materials, because the two are unrelated: the entry is keyed by the source material's content and the
        /// convert settings (see <see cref="Models.MaterialGeneratorUtility"/>), not by whether anything happens
        /// to be displaying the result right now. Both early exits below are reached routinely (a settings edit
        /// releases every lease on the old key while its textures are still compressing, so the placeholder is
        /// destroyed and no material references it anymore), and skipping the write there used to throw away a
        /// whole finished astcenc run -- guaranteeing it would be redone from scratch the next time that exact
        /// material and settings combination came back, e.g. when the user undid the edit.
        /// </remarks>
        /// <param name="item">The item that finished compressing.</param>
        /// <param name="compressed">The compressed result. Destroyed by this method if it ends up unused (the placeholder was destroyed mid-flight, or nothing references it anymore).</param>
        /// <param name="checkPlaceholderStillAlive">Whether to treat <c>item.Placeholder</c> reading as destroyed
        /// as "abandon this result" (true, for the background astcenc path, whose input is never destroyed by the
        /// compressor itself, so a dead placeholder here only means something external destroyed it while
        /// compression was in flight) versus proceeding anyway (false, for the synchronous fallback path, whose
        /// compression facades destroy their input as a normal side effect of success -- see
        /// <see cref="FallbackToSynchronousCompression"/>'s call site for why that is still safe).</param>
        private static void ApplyCompressedResult(PendingItem item, Texture2D compressed, bool checkPlaceholderStillAlive)
        {
            // Matches what the synchronous path (MaterialGeneratorUtility.SaveTexture) does for both color and
            // normal map textures; AstcencTextureCompressor.CompressNormalMap(Async) already does this itself for
            // the normal map path, so doing it again here is a harmless no-op for that case. Runs before the disk
            // cache write so the flag is set on the very instance whose raw bytes get stored, exactly as the
            // synchronous path does.
            TextureUtility.SetStreamingMipMaps(compressed, true);
            SaveToDiskCache(item, compressed);

            if (checkPlaceholderStillAlive && item.Placeholder == null)
            {
                // Destroyed while compression was in flight (e.g. a full domain reload recovery, or every
                // preview material lease was released mid-compression). The entry is already saved above, so
                // the compression itself was not wasted.
                TextureUtility.DestroyTexture(compressed);
                return;
            }

            var replacer = materialTextureReplacer;
            var replacedCount = replacer != null ? replacer(item.Placeholder, compressed) : 0;
            if (replacedCount == 0)
            {
                // No cached preview material references the placeholder anymore (e.g. every lease was
                // released while this was compressing in the background, or the replacer was unregistered
                // mid-flight). Nothing left to update on screen -- but, again, the entry is already saved.
                TextureUtility.DestroyTexture(compressed);
                DestroyPlaceholderUnlessItIsTheResult(item, compressed);
                return;
            }

            Logger.LogDebug($"Progressive compression replaced \"{compressed.name}\" in {replacedCount} cached preview material propert{(replacedCount == 1 ? "y" : "ies")}.", compressed);

            DestroyPlaceholderUnlessItIsTheResult(item, compressed);

            // InternalEditorUtility.RepaintAllViews() (rather than just SceneView.RepaintAll()) also repaints the
            // Game view and the material preview thumbnails, both of which can also be displaying the just-replaced texture.
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        /// <summary>
        /// Stores <paramref name="compressed"/> under <paramref name="item"/>'s cache file name, so a later
        /// preview generation (or a real conversion, which shares the same key space) reuses it instead of
        /// running astcenc again. Failures are logged and swallowed: the compressed texture is still perfectly
        /// usable on screen, only the reuse of it later is lost.
        /// </summary>
        /// <param name="item">The item that finished compressing; supplies the cache file name and the attributes recorded alongside the bytes.</param>
        /// <param name="compressed">The compressed result whose raw bytes are stored.</param>
        private static void SaveToDiskCache(PendingItem item, Texture2D compressed)
        {
            try
            {
                var cache = new CacheUtility.TextureCache(compressed, !item.IsSRGB, item.IsNormalMap, item.BuildTarget);
                CacheManager.Texture.SaveBinary(item.CacheFile, cache.WriteTo);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Failed to save the disk cache entry for progressively compressed preview texture \"{compressed.name}\". {e.Message}");
            }
        }

        /// <summary>
        /// Destroys the placeholder now that the compressed result has taken its place, unless the two are the
        /// same object.
        /// </summary>
        /// <remarks>
        /// The two compressor backends differ here: <see cref="AstcencTextureCompressor"/> always returns a new
        /// instance, but <see cref="UnityTextureCompressor"/> compresses in place via
        /// <see cref="UnityEditor.EditorUtility.CompressTexture"/> and returns the very texture it was given. The
        /// synchronous fallback goes through <see cref="TextureUtility.CompressTextureForBuildTarget"/> /
        /// <see cref="TextureUtility.CompressNormalMap"/>, which pick a backend by format, so it can hand back the
        /// placeholder itself (e.g. a non-mobile build target resolving to DXT5). Destroying it then would destroy
        /// the result the preview materials were just pointed at.
        /// </remarks>
        /// <param name="item">The item whose placeholder is being retired.</param>
        /// <param name="compressed">The compressed result that replaced it.</param>
        private static void DestroyPlaceholderUnlessItIsTheResult(PendingItem item, Texture2D compressed)
        {
            if (ReferenceEquals(item.Placeholder, compressed))
            {
                return;
            }

            TextureUtility.DestroyTexture(item.Placeholder);
        }

        private static long EstimatePlaceholderBytes(Texture2D placeholder)
        {
            if (placeholder == null)
            {
                return 0;
            }

            long baseSize = (long)placeholder.width * placeholder.height * 4;

            // A full mip chain adds roughly 1/3 more data on top of mip 0 (the standard geometric series
            // 1 + 1/4 + 1/16 + ... converges to 4/3), so scale by 4/3 when the placeholder has mips.
            if (placeholder.mipmapCount > 1)
            {
                baseSize = baseSize * 4 / 3;
            }

            return baseSize;
        }

        private static void OnBeforeAssemblyReload()
        {
            assemblyReloading = true;
            StopAllWork();
        }

        private static void OnAfterAssemblyReload()
        {
            // Normally redundant (a domain reload re-runs this type's static constructor, giving every static
            // field -- including this one and cts -- a fresh value automatically), but AssemblyReloadEvents does
            // not guarantee beforeAssemblyReload is always followed by an actual domain reload; without this,
            // that edge case would leave assemblyReloading stuck true (permanently refusing TryEnqueue) and cts
            // stuck cancelled (permanently failing every future compression attempt with OperationCanceledException).
            assemblyReloading = false;
            cts = new CancellationTokenSource();
        }

        private static void OnEditorQuitting()
        {
            StopAllWork();
        }

        /// <summary>
        /// Cancels background work and stops dispatching new work, for both <see cref="OnBeforeAssemblyReload"/>
        /// and <see cref="OnEditorQuitting"/>.
        /// </summary>
        private static void StopAllWork()
        {
            // Cancelling first (before killing processes) is what makes CompressLevelsWorker's per-level check
            // stop it from starting the *next* astcenc process; killing is what stops whichever process (if any)
            // is already running right now for the *current* level -- cancellation alone cannot interrupt a
            // worker thread already blocked in Process.WaitForExit.
            cts.Cancel();
            AstcencCli.KillAllRunningProcesses();

            if (updateHooked)
            {
                EditorApplication.update -= OnUpdate;
                updateHooked = false;
            }

            // Every in-flight item's own ProcessItemAsync (there can be more than one at once, up to
            // MaxConcurrentCompressions -- none of them are awaited here) will unwind on its own shortly (its
            // astcenc call now throws, is caught, and returns without further work, since cts.IsCancellationRequested
            // is true) and its finally block re-applies this same reset (pendingBytes and inFlight both clamped via
            // Math.Max(0, ...)), which is safe to run any number of times over.
            Pending.Clear();
            pendingBytes = 0;
            inFlight = 0;
            highWaterWarned = false;

            // The placeholder texture(s) still referenced by preview materials are deliberately NOT destroyed
            // here: they are live Unity objects owned by those materials, and the next preview regeneration
            // (after reload, or never, if the editor is quitting) will naturally replace or release them through
            // the normal material lifecycle.
        }

        private struct PendingItem
        {
            internal Texture2D Placeholder;
            internal AstcencTextureCompressor Compressor;
            internal TextureFormat? Format;
            internal bool IsNormalMap;
            internal bool Readable;
            internal int? MaxTextureSize;
            internal string CacheFile;
            internal bool IsSRGB;
            internal long EstimatedBytes;
            internal BuildTarget BuildTarget;
        }
    }
}
