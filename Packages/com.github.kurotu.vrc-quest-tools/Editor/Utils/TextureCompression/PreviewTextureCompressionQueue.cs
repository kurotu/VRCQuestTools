// <copyright file="PreviewTextureCompressionQueue.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
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
    /// the compressed result, the placeholder is destroyed, and the scene view is repainted.
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

        static PreviewTextureCompressionQueue()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
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
        /// This runs the exact same <see cref="ProcessItemAsync"/> production uses -- an <c>async Task</c> test
        /// method can simply <c>await</c> the returned <see cref="System.Threading.Tasks.Task"/> to block only
        /// the test (not the whole editor), since NUnit's Editor test runner already pumps the main-thread
        /// synchronization context between awaits in an <c>async Task</c> test method.
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

            DequeueAndProcess(out _);
        }

        private static async System.Threading.Tasks.Task ProcessItemAsync(PendingItem item)
        {
            try
            {
                Texture2D compressed = null;
                try
                {
                    compressed = item.IsNormalMap
                        ? await item.Compressor.CompressNormalMapAsync(item.Placeholder, item.Format, item.Readable, item.MaxTextureSize)
                        : await item.Compressor.CompressTextureAsync(item.Placeholder, item.Format.Value);
                }
                catch (Exception e)
                {
                    // The placeholder (uncompressed) stays assigned to preview materials and keeps rendering;
                    // only the background compression attempt is abandoned.
                    var name = item.Placeholder != null ? item.Placeholder.name : "<destroyed>";
                    Logger.LogWarning($"Progressive ASTC compression failed for preview texture \"{name}\"; the uncompressed preview texture remains displayed. {e.Message}");
                    return;
                }

                if (item.Placeholder == null)
                {
                    // Destroyed (e.g. by a full domain reload recovery) while compression was in flight.
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

                try
                {
                    CacheManager.Texture.Save(item.CacheFile, JsonUtility.ToJson(new CacheUtility.TextureCache(compressed, !item.IsSRGB, item.IsNormalMap, EditorUserBuildSettings.activeBuildTarget)));
                }
                catch (Exception e)
                {
                    // The material already displays the compressed texture (replaced above); only the disk cache
                    // write failed, so the next preview generation just re-does the work instead of reusing a cache hit.
                    Logger.LogWarning($"Failed to save the disk cache entry for progressively compressed preview texture \"{compressed.name}\". {e.Message}");
                }

                TextureUtility.DestroyTexture(item.Placeholder);
                SceneView.RepaintAll();
            }
            finally
            {
                pendingBytes -= item.EstimatedBytes;
                processing = false;
            }
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
            // Stop dispatching new work; any astcenc process already running in the background for the
            // in-flight item (if any) is abandoned as-is -- there is no handle to cancel it from here, and a
            // domain reload is about to tear down all managed state (including this queue) regardless. Its
            // eventual completion (if it ever resumes) finds a fresh, empty queue and a destroyed placeholder
            // reference, so ProcessItemAsync's own null-checks make that a safe no-op rather than a crash.
            //
            // The placeholder texture(s) still referenced by preview materials are deliberately NOT destroyed
            // here: they are live Unity objects owned by those materials, and the next preview regeneration
            // (after reload) will naturally replace or release them through the normal material lifecycle.
            assemblyReloading = true;
            if (updateHooked)
            {
                EditorApplication.update -= OnUpdate;
                updateHooked = false;
            }

            Pending.Clear();
            pendingBytes = 0;
            processing = false;
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
        }
    }
}
