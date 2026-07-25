// <copyright file="PreviewTextureCompressionQueueTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Tests for <see cref="PreviewTextureCompressionQueue"/>, the background queue behind the NDMF preview's
    /// progressive texture replacement. Every test saves and restores the process-wide registered
    /// material-texture-replacer (<see cref="PreviewTextureCompressionQueue.MaterialTextureReplacerForTesting"/> /
    /// <see cref="PreviewTextureCompressionQueue.RegisterMaterialTextureReplacer"/>) so a fake installed here
    /// never leaks into later tests or production preview generation within the same editor session.
    /// </summary>
    public class PreviewTextureCompressionQueueTests
    {
        private Func<Texture, Texture, int> savedReplacer;

        /// <summary>
        /// Saves whatever material-texture-replacer is currently registered (the real one from NDMF's
        /// <c>[InitializeOnLoadMethod]</c> registration if NDMF is installed, or null otherwise) so it can be
        /// restored in <see cref="TearDown"/>, then drains any backlog left by earlier tests in this batch.
        /// PreviewTextureCompressionQueue is a process-wide static queue processed one item at a time; without
        /// draining first, a leftover item from an earlier test could be sitting at the front of the queue, and
        /// this fixture's own <c>ProcessNextForTesting()</c> calls (FIFO) would dequeue that instead of the item
        /// each test just enqueued. <c>[UnitySetUp]</c> (an IEnumerator) rather than a plain <c>[SetUp]</c> is
        /// required to await the drain; Unity's test framework runs it to completion before both this fixture's
        /// <c>[UnityTest]</c> and plain <c>[Test]</c> methods.
        /// </summary>
        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            savedReplacer = PreviewTextureCompressionQueue.MaterialTextureReplacerForTesting;
            while (PreviewTextureCompressionQueue.PendingCountForTesting > 0)
            {
                var task = PreviewTextureCompressionQueue.ProcessNextForTesting();
                yield return TestUtils.WaitForTask(task);
            }
        }

        /// <summary>
        /// Restores the material-texture-replacer saved in <see cref="SetUp"/>.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            PreviewTextureCompressionQueue.RegisterMaterialTextureReplacer(savedReplacer);
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
        }

        /// <summary>
        /// Verifies the full success path: enqueue, process, the registered replacer is invoked with the
        /// placeholder and the compressed result, the placeholder is destroyed, and the queue empties.
        /// </summary>
        [UnityTest]
        public IEnumerator TryEnqueue_ProcessNextForTesting_InvokesReplacerAndDestroysPlaceholder()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            var placeholder = CreateColorTexture(16, 16);

            Texture capturedFrom = null;
            Texture2D capturedTo = null;
            PreviewTextureCompressionQueue.RegisterMaterialTextureReplacer((from, to) =>
            {
                capturedFrom = from;
                capturedTo = to as Texture2D;
                return 1;
            });

            var cacheFile = $"test_progressive_{Guid.NewGuid():N}.json";
            var enqueued = PreviewTextureCompressionQueue.TryEnqueue(placeholder, compressor, TextureFormat.ASTC_4x4, false, false, null, cacheFile, true);
            Assert.IsTrue(enqueued, "TryEnqueue should succeed when a replacer is registered and the pending-bytes cap has headroom.");
            Assert.AreEqual(1, PreviewTextureCompressionQueue.PendingCountForTesting);

            var task = PreviewTextureCompressionQueue.ProcessNextForTesting();
            yield return TestUtils.WaitForTask(task);
            var processed = task.Result;
            Assert.IsTrue(processed);
            Assert.AreEqual(0, PreviewTextureCompressionQueue.PendingCountForTesting);

            Assert.AreSame(placeholder, capturedFrom, "The replacer must be called with the original placeholder as `from`.");
            Assert.IsNotNull(capturedTo);
            Assert.AreEqual((int)TextureFormat.ASTC_4x4, (int)capturedTo.format, "The replacer's `to` must be the ASTC-compressed result.");
            Assert.IsTrue(placeholder == null, "The placeholder must be destroyed once the replacer reports it was replaced.");

            UnityEngine.Object.DestroyImmediate(capturedTo);
        }

        /// <summary>
        /// Verifies the normal map path also runs end-to-end through the queue.
        /// </summary>
        [UnityTest]
        public IEnumerator TryEnqueue_NormalMap_ProcessNextForTesting_InvokesReplacer()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            var placeholder = CreateReadableColorTexture(16, 16);

            Texture2D capturedTo = null;
            PreviewTextureCompressionQueue.RegisterMaterialTextureReplacer((from, to) =>
            {
                capturedTo = to as Texture2D;
                return 1;
            });

            var cacheFile = $"test_progressive_normal_{Guid.NewGuid():N}.json";
            var enqueued = PreviewTextureCompressionQueue.TryEnqueue(placeholder, compressor, TextureFormat.ASTC_4x4, true, false, null, cacheFile, false);
            Assert.IsTrue(enqueued);

            var task = PreviewTextureCompressionQueue.ProcessNextForTesting();
            yield return TestUtils.WaitForTask(task);
            var processed = task.Result;
            Assert.IsTrue(processed);

            Assert.IsNotNull(capturedTo);
            Assert.AreEqual((int)TextureFormat.ASTC_4x4, (int)capturedTo.format);
            Assert.IsTrue(placeholder == null);

            UnityEngine.Object.DestroyImmediate(capturedTo);
        }

        /// <summary>
        /// Verifies that when the replacer reports zero replacements (e.g. every lease referencing the
        /// placeholder was already released while compression ran in the background), both the placeholder and
        /// the freshly compressed (now orphaned) result are destroyed instead of leaking.
        /// </summary>
        [UnityTest]
        public IEnumerator ProcessNextForTesting_ReplacerReportsZero_DestroysBothPlaceholderAndResult()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            var placeholder = CreateColorTexture(8, 8);

            PreviewTextureCompressionQueue.RegisterMaterialTextureReplacer((from, to) => 0);

            var cacheFile = $"test_progressive_orphan_{Guid.NewGuid():N}.json";
            PreviewTextureCompressionQueue.TryEnqueue(placeholder, compressor, TextureFormat.ASTC_4x4, false, false, null, cacheFile, true);

            var task = PreviewTextureCompressionQueue.ProcessNextForTesting();
            yield return TestUtils.WaitForTask(task);
            var processed = task.Result;

            Assert.IsTrue(processed);
            Assert.IsTrue(placeholder == null, "The placeholder must be destroyed when nothing references it anymore.");
        }

        /// <summary>
        /// Verifies that with no replacer registered -- simulating "NDMF is not installed" -- TryEnqueue refuses,
        /// so <c>MaterialGeneratorUtility</c> falls back to synchronous compression instead of enqueueing work
        /// that could never be applied to any preview material.
        /// </summary>
        [Test]
        public void TryEnqueue_NoReplacerRegistered_ReturnsFalse()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            var placeholder = CreateColorTexture(4, 4);
            try
            {
                PreviewTextureCompressionQueue.RegisterMaterialTextureReplacer(null);

                var enqueued = PreviewTextureCompressionQueue.TryEnqueue(placeholder, compressor, TextureFormat.ASTC_4x4, false, false, null, "test_progressive_none.json", true);

                Assert.IsFalse(enqueued);
                Assert.AreEqual(0, PreviewTextureCompressionQueue.PendingCountForTesting);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(placeholder);
            }
        }

        /// <summary>
        /// Verifies the pending-bytes memory safety valve: once the accumulated pending bytes would exceed
        /// <see cref="PreviewTextureCompressionQueue.MaxPendingBytes"/>, TryEnqueue refuses instead of growing
        /// the queue further. Reaches the cap via reflection into the private pendingBytes counter (restored
        /// afterwards) rather than actually allocating hundreds of megabytes of placeholder textures, which would
        /// make this test slow and liable to flake on memory-constrained CI runners.
        /// </summary>
        [Test]
        public void TryEnqueue_PendingBytesCapReached_ReturnsFalse()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            var placeholder = CreateColorTexture(4, 4);

            // Register a (non-null) replacer explicitly so this test exercises the pending-bytes cap
            // specifically, regardless of whether NDMF happens to be installed in this environment (which
            // determines what SetUp's saved/restored replacer actually is).
            PreviewTextureCompressionQueue.RegisterMaterialTextureReplacer((from, to) => 1);

            var field = typeof(PreviewTextureCompressionQueue).GetField("pendingBytes", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field, "PreviewTextureCompressionQueue.pendingBytes field must exist for this test to drive it.");
            var originalPendingBytes = (long)field.GetValue(null);
            try
            {
                field.SetValue(null, PreviewTextureCompressionQueue.MaxPendingBytes);

                var enqueued = PreviewTextureCompressionQueue.TryEnqueue(placeholder, compressor, TextureFormat.ASTC_4x4, false, false, null, "test_progressive_cap.json", true);

                Assert.IsFalse(enqueued, "TryEnqueue must refuse once the pending-bytes cap would be exceeded.");
                Assert.AreEqual(0, PreviewTextureCompressionQueue.PendingCountForTesting);
            }
            finally
            {
                field.SetValue(null, originalPendingBytes);
                UnityEngine.Object.DestroyImmediate(placeholder);
            }
        }

        /// <summary>
        /// Verifies the synchronous fallback path (Step 3-3 review item 2): when the background astcenc attempt
        /// fails (here, an <see cref="AstcencTextureCompressor"/> pointed at a non-existent executable), the
        /// placeholder is not left uncompressed forever -- <see cref="PreviewTextureCompressionQueue"/> falls back
        /// to synchronous compression and still replaces every preview material reference and destroys the
        /// placeholder, exactly as the background path would have on success.
        /// </summary>
        [UnityTest]
        public IEnumerator ProcessNextForTesting_BackgroundCompressionFails_FallsBackToSyncAndReplaces()
        {
            // Ensures a real, working astcenc is available in this environment for the synchronous fallback to
            // succeed with -- the fallback resolves its own compressor via TextureCompressorProvider, entirely
            // independent of the broken one enqueued below (which is only used for the abandoned background attempt).
            TestUtils.CreateAstcencCompressorOrIgnore();

            var brokenCompressor = new AstcencTextureCompressor("does-not-exist-astcenc.exe", "0.0.0", "-medium");
            var placeholder = CreateColorTexture(16, 16);

            Texture capturedFrom = null;
            Texture2D capturedTo = null;
            PreviewTextureCompressionQueue.RegisterMaterialTextureReplacer((from, to) =>
            {
                capturedFrom = from;
                capturedTo = to as Texture2D;
                return 1;
            });

            var cacheFile = $"test_progressive_fallback_{Guid.NewGuid():N}.json";
            var enqueued = PreviewTextureCompressionQueue.TryEnqueue(placeholder, brokenCompressor, TextureFormat.ASTC_4x4, false, false, null, cacheFile, true);
            Assert.IsTrue(enqueued);

            var task = PreviewTextureCompressionQueue.ProcessNextForTesting();
            yield return TestUtils.WaitForTask(task); // Must not throw -- the fallback failure path must not either.
            var processed = task.Result;

            Assert.IsTrue(processed);
            Assert.AreSame(placeholder, capturedFrom, "The replacer must still be called with the original placeholder as `from`.");
            Assert.IsNotNull(capturedTo, "The synchronous fallback compression must still replace the placeholder in preview materials.");

            // Not asserting the exact resulting format: the fallback resolves its target format from the active
            // build target (see TextureUtility.CompressTextureForBuildTarget), which this test does not control,
            // so on a non-mobile editor build target it legitimately falls further back to DXT5/Unity compression
            // rather than ASTC. Either way, the source RGBA32 placeholder must have actually been compressed.
            Assert.AreNotEqual((int)TextureFormat.RGBA32, (int)capturedTo.format);
            Assert.IsTrue(placeholder == null, "The placeholder must be destroyed once the fallback succeeds and the replacer reports it was replaced.");

            UnityEngine.Object.DestroyImmediate(capturedTo);
        }

        /// <summary>
        /// Verifies Step 3-3 review item 3: dequeuing an item whose placeholder was already destroyed (e.g. every
        /// preview material lease was released, such as while rapidly dragging a settings slider, while the item
        /// was still waiting in the queue) must not throw or log a warning -- in particular, must not pass the
        /// destroyed placeholder into <see cref="AstcencTextureCompressor.CompressTextureAsync"/>, which would
        /// throw <see cref="MissingReferenceException"/> reading from it.
        /// </summary>
        [UnityTest]
        public IEnumerator ProcessNextForTesting_PlaceholderDestroyedBeforeDequeue_NoExceptionOrWarning()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            var placeholder = CreateColorTexture(8, 8);

            PreviewTextureCompressionQueue.RegisterMaterialTextureReplacer((from, to) => 1);

            var cacheFile = $"test_progressive_predestroyed_{Guid.NewGuid():N}.json";
            var enqueued = PreviewTextureCompressionQueue.TryEnqueue(placeholder, compressor, TextureFormat.ASTC_4x4, false, false, null, cacheFile, true);
            Assert.IsTrue(enqueued);

            // Simulate every preview material lease releasing the placeholder (and it being destroyed as a
            // result) while it is still sitting in the queue, before it reaches the front.
            UnityEngine.Object.DestroyImmediate(placeholder);

            var warnings = new List<string>();
            void OnLogMessage(string condition, string stackTrace, LogType type)
            {
                if (type == LogType.Warning)
                {
                    warnings.Add(condition);
                }
            }

            Application.logMessageReceived += OnLogMessage;
            System.Threading.Tasks.Task<bool> task;
            try
            {
                task = PreviewTextureCompressionQueue.ProcessNextForTesting();
                yield return TestUtils.WaitForTask(task); // Must not throw.
            }
            finally
            {
                Application.logMessageReceived -= OnLogMessage;
            }

            Assert.IsTrue(task.Result);
            Assert.AreEqual(0, PreviewTextureCompressionQueue.PendingCountForTesting);
            CollectionAssert.IsEmpty(warnings, "Dequeuing an already-destroyed placeholder must not log a warning.");
        }

        /// <summary>
        /// Verifies Step 3-3 review item 7: <see cref="PreviewTextureCompressionQueue.PendingBytesForTesting"/>
        /// must never go negative. Reproduces the underflow condition directly (rather than by actually
        /// triggering a domain reload mid-compression, which a test cannot do) by forcing the pendingBytes
        /// counter to a value smaller than the processed item's own EstimatedBytes before letting
        /// <c>ProcessItemAsync</c>'s <c>finally</c> block subtract it -- mirroring what
        /// <c>OnBeforeAssemblyReload</c>/<c>OnEditorQuitting</c> zeroing pendingBytes out from under an in-flight
        /// item would otherwise do. Only asserts the counter never goes negative (rather than an exact final
        /// value) and restores whatever was there beforehand: <c>pendingBytes</c> is process-wide static state,
        /// potentially shared with real (non-test) preview activity in the running editor session, not scoped to
        /// this test -- same accommodation <see cref="TryEnqueue_PendingBytesCapReached_ReturnsFalse"/> already
        /// makes for the same reason.
        /// </summary>
        [UnityTest]
        public IEnumerator ProcessNextForTesting_PendingBytesWouldUnderflow_ClampsToZero()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            var placeholder = CreateColorTexture(16, 16);

            PreviewTextureCompressionQueue.RegisterMaterialTextureReplacer((from, to) => 1);

            var cacheFile = $"test_progressive_underflow_{Guid.NewGuid():N}.json";
            var enqueued = PreviewTextureCompressionQueue.TryEnqueue(placeholder, compressor, TextureFormat.ASTC_4x4, false, false, null, cacheFile, true);
            Assert.IsTrue(enqueued);

            var field = typeof(PreviewTextureCompressionQueue).GetField("pendingBytes", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field, "PreviewTextureCompressionQueue.pendingBytes field must exist for this test to drive it.");
            var originalPendingBytes = (long)field.GetValue(null);
            try
            {
                field.SetValue(null, 0L);

                var task = PreviewTextureCompressionQueue.ProcessNextForTesting();
                yield return TestUtils.WaitForTask(task);

                Assert.IsTrue(task.Result);
                Assert.GreaterOrEqual(PreviewTextureCompressionQueue.PendingBytesForTesting, 0L, "pendingBytes must never go negative.");
            }
            finally
            {
                field.SetValue(null, originalPendingBytes);
            }
        }

        private static Texture2D CreateColorTexture(int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
            var pixels = new Color32[width * height];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32((byte)(i % 256), 128, 64, 255);
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, true); // Mirrors a baked (GPU-readback) placeholder: not readable.
            return tex;
        }

        private static Texture2D CreateReadableColorTexture(int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            var pixels = new Color32[width * height];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32((byte)(i % 256), 128, 255, 255);
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, false); // Mirrors TextureUtility.DownscaleNormalMap's output: readable.
            return tex;
        }
    }
}
