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
        /// Maximum total estimated bytes of placeholder textures allowed to be pending (enqueued but not yet
        /// compressed) at once. Progressive keeps each placeholder (an uncompressed baked RGBA32 texture, with
        /// mips) alive in memory until its compressed replacement is ready; a 2048x2048 RGBA32 texture with a
        /// full mip chain is about 22 MB (2048*2048*4 * 4/3 for the mip chain overhead). 512 MB allows roughly
        /// 23 such textures to be in flight -- generous for normal preview activity (compression is processed
        /// one at a time, so the queue drains quickly relative to typical preview edit rates) -- while still
        /// bounding the worst case (e.g. toggling material preview for many avatars at once) so it cannot balloon
        /// editor memory unboundedly. Enqueue attempts beyond this cap fall back to synchronous compression.
        /// </summary>
        internal const long MaxPendingBytes = 512L * 1024 * 1024;

        private static readonly List<PendingItem> Pending = new List<PendingItem>();

        private static Func<Texture, Texture, int> materialTextureReplacer;
        private static long pendingBytes;
        private static bool updateHooked;
        private static bool processing;
        private static bool assemblyReloading;
        private static CancellationTokenSource cts = new CancellationTokenSource();

        static PreviewTextureCompressionQueue()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            EditorApplication.quitting += OnEditorQuitting;
        }

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
        /// <param name="compressor">astcenc compressor to use (the preview-preset instance resolved by <see cref="TextureCompressorProvider"/>).</param>
        /// <param name="format">Target ASTC format. Must be non-null for color textures; may be non-null for normal maps only (a null format is never astcenc-compatible).</param>
        /// <param name="isNormalMap">Whether <paramref name="placeholder"/> is a normal map (uses <see cref="AstcencTextureCompressor.CompressNormalMapAsync"/>) or a color/parameter texture (uses <see cref="AstcencTextureCompressor.CompressTextureAsync"/>).</param>
        /// <param name="readable">Normal map only: whether the compressed result should remain readable.</param>
        /// <param name="maxTextureSize">Normal map only: optional max texture size override.</param>
        /// <param name="cacheFile">Disk cache file name to save the compressed result under once ready, matching what the synchronous path would have used.</param>
        /// <param name="isSRGB">Whether the texture is sRGB data; recorded into the disk cache entry as <c>!isSRGB</c> (linear), matching <see cref="Models.MaterialGeneratorUtility"/>'s synchronous save.</param>
        /// <returns>True when the texture was enqueued (the caller must not touch <paramref name="placeholder"/> or fall back to synchronous compression); false when no material-texture-replacer is registered (e.g. NDMF is not installed), or the pending-bytes cap would be exceeded, in which case the caller must fall back to synchronous compression itself and <paramref name="placeholder"/> remains entirely the caller's responsibility.</returns>
        internal static bool TryEnqueue(Texture2D placeholder, AstcencTextureCompressor compressor, TextureFormat? format, bool isNormalMap, bool readable, int? maxTextureSize, string cacheFile, bool isSRGB)
        {
            if (assemblyReloading || materialTextureReplacer == null)
            {
                return false;
            }

            var estimatedBytes = EstimatePlaceholderBytes(placeholder);
            if (pendingBytes + estimatedBytes > MaxPendingBytes)
            {
                return false;
            }

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
            return true;
        }

        /// <summary>
        /// Gets the number of items currently waiting to start compression (excludes the item, if any, currently being compressed).
        /// For tests only.
        /// </summary>
        internal static int PendingCountForTesting => Pending.Count;

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
        /// <returns>A task that completes once the dequeued item finishes processing, with result true; or an already-completed task with result false if the queue was empty (or something is already processing).</returns>
        internal static async System.Threading.Tasks.Task<bool> ProcessNextForTesting()
        {
            if (processing || Pending.Count == 0)
            {
                return false;
            }

            DequeueAndProcess(out var task);
            await task;
            return true;
        }

        private static void DequeueAndProcess(out System.Threading.Tasks.Task task)
        {
            processing = true;
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
            if (processing || Pending.Count == 0)
            {
                if (!processing && Pending.Count == 0 && updateHooked)
                {
                    EditorApplication.update -= OnUpdate;
                    updateHooked = false;
                }

                return;
            }

            DequeueAndProcess(out var task);

            // ProcessItemAsync already handles every failure it knows how to handle internally (falling back to
            // synchronous compression, logging, and cleaning up any orphaned texture); this ContinueWith is only
            // a last-resort safety net for an exception that slips past that -- e.g. a bug in the fallback path
            // itself, or in the pendingBytes/processing bookkeeping in ProcessItemAsync's own finally block --
            // so that it is at least logged instead of becoming an unobserved task exception. Since
            // ProcessItemAsync's own catch blocks do not rethrow, this deliberately should not normally fire; if
            // it ever does, this is the only place logging it, so there is no risk of double-logging the same
            // exception both here and inside ProcessItemAsync.
            task.ContinueWith(
                t => Logger.LogException(t.Exception),
                System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
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
                // Clamped rather than allowed to go negative: OnBeforeAssemblyReload/OnEditorQuitting zero this
                // out immediately (so a reload/quit is not blocked waiting on in-flight work), but this item's own
                // finally still runs afterwards and would otherwise subtract its EstimatedBytes a second time.
                pendingBytes = Math.Max(0, pendingBytes - item.EstimatedBytes);
                processing = false;
            }
        }

        /// <summary>
        /// Falls back to synchronous compression (<see cref="TextureUtility.CompressTextureForBuildTarget"/> /
        /// <see cref="TextureUtility.CompressNormalMap"/>, both with <c>forEditorPreview: true</c> to match what
        /// the abandoned background attempt would have produced) when the background astcenc attempt for
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
                    ? TextureUtility.CompressNormalMap(item.Placeholder, item.BuildTarget, item.Format.Value, item.Readable, item.MaxTextureSize, forEditorPreview: true)
                    : TextureUtility.CompressTextureForBuildTarget(item.Placeholder, item.BuildTarget, item.Format.Value, item.MaxTextureSize, forEditorPreview: true);
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
                // destroy their input texture as a normal part of a *successful* compression (see their own XML
                // docs), so item.Placeholder is now expected to read as destroyed here even though nothing
                // external abandoned it. It remains safe (and necessary) to pass as the replacer's `from` and to
                // TextureUtility.DestroyTexture (a no-op on an already-destroyed texture) below: this method only
                // ever reads/dereferences the reference for identity comparison, never for its (freed) contents,
                // and UnityEngine.Object's overridden equality still matches a destroyed object against itself by
                // instance ID, so the replacer still finds and updates every preview material reference correctly.
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
        /// streaming mipmaps on the result, replaces every cached preview material reference to the placeholder,
        /// saves the disk cache entry, destroys the placeholder, and repaints every editor view.
        /// </summary>
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
            if (checkPlaceholderStillAlive && item.Placeholder == null)
            {
                // Destroyed while compression was in flight (e.g. a full domain reload recovery, or every
                // preview material lease was released mid-compression).
                TextureUtility.DestroyTexture(compressed);
                return;
            }

            var replacer = materialTextureReplacer;
            var replacedCount = replacer != null ? replacer(item.Placeholder, compressed) : 0;
            if (replacedCount == 0)
            {
                // No cached preview material references the placeholder anymore (e.g. every lease was
                // released while this was compressing in the background, or the replacer was unregistered
                // mid-flight). Nothing left to update.
                TextureUtility.DestroyTexture(compressed);
                TextureUtility.DestroyTexture(item.Placeholder);
                return;
            }

            // Matches what the synchronous path (MaterialGeneratorUtility.SaveTexture) does for both color and
            // normal map textures; AstcencTextureCompressor.CompressNormalMap(Async) already does this itself for
            // the normal map path, so doing it again here is a harmless no-op for that case.
            TextureUtility.SetStreamingMipMaps(compressed, true);

            try
            {
                CacheManager.Texture.Save(item.CacheFile, JsonUtility.ToJson(new CacheUtility.TextureCache(compressed, !item.IsSRGB, item.IsNormalMap, item.BuildTarget)));
            }
            catch (Exception e)
            {
                // The material already displays the compressed texture (replaced above); only the disk cache
                // write failed, so the next preview generation just re-does the work instead of reusing a cache hit.
                Logger.LogWarning($"Failed to save the disk cache entry for progressively compressed preview texture \"{compressed.name}\". {e.Message}");
            }

            TextureUtility.DestroyTexture(item.Placeholder);

            // InternalEditorUtility.RepaintAllViews() (rather than just SceneView.RepaintAll()) also repaints the
            // Game view and the material preview thumbnails, both of which can also be displaying the just-replaced texture.
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
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

            // The in-flight item's own ProcessItemAsync (if any) is not awaited here -- it will unwind on its
            // own shortly (its astcenc call now throws, is caught, and returns without further work, since
            // cts.IsCancellationRequested is true) and its finally block re-applies this same reset
            // (pendingBytes clamped via Math.Max(0, ...), processing = false), which is safe to run twice.
            Pending.Clear();
            pendingBytes = 0;
            processing = false;

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
