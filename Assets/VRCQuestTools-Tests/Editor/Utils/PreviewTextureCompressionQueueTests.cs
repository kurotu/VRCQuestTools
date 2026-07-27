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
        /// Restores the material-texture-replacer saved in <see cref="SetUp"/>, the real (non-overridden)
        /// <see cref="PreviewTextureCompressionQueue.MaxConcurrentCompressions"/> for tests that set
        /// <see cref="PreviewTextureCompressionQueue.MaxConcurrentCompressionsOverrideForTesting"/>, and re-enables
        /// the real dispatch loop for tests that set <see cref="PreviewTextureCompressionQueue.SuspendAutoDispatchForTesting"/>
        /// -- all unconditionally, so a test that fails/throws partway through still leaves this process-wide
        /// static state clean for every later test (and real preview activity) in the same editor session.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            PreviewTextureCompressionQueue.RegisterMaterialTextureReplacer(savedReplacer);
            PreviewTextureCompressionQueue.MaxConcurrentCompressionsOverrideForTesting = null;
            PreviewTextureCompressionQueue.SuspendAutoDispatchForTesting = false;
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

            var cacheFile = $"test_progressive_{Guid.NewGuid():N}.bin";
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

            var cacheFile = $"test_progressive_normal_{Guid.NewGuid():N}.bin";
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

            var cacheFile = $"test_progressive_orphan_{Guid.NewGuid():N}.bin";
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

                var enqueued = PreviewTextureCompressionQueue.TryEnqueue(placeholder, compressor, TextureFormat.ASTC_4x4, false, false, null, "test_progressive_none.bin", true);

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

                var enqueued = PreviewTextureCompressionQueue.TryEnqueue(placeholder, compressor, TextureFormat.ASTC_4x4, false, false, null, "test_progressive_cap.bin", true);

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

            var cacheFile = $"test_progressive_fallback_{Guid.NewGuid():N}.bin";
            var enqueued = PreviewTextureCompressionQueue.TryEnqueue(placeholder, brokenCompressor, TextureFormat.ASTC_4x4, false, false, null, cacheFile, true);
            Assert.IsTrue(enqueued);

            var task = PreviewTextureCompressionQueue.ProcessNextForTesting();
            yield return TestUtils.WaitForTask(task); // Must not throw -- the fallback failure path must not either.
            var processed = task.Result;

            Assert.IsTrue(processed);
            Assert.AreSame(placeholder, capturedFrom, "The replacer must still be called with the original placeholder as `from`.");
            Assert.IsNotNull(capturedTo, "The synchronous fallback compression must still replace the placeholder in preview materials.");

            // The fallback resolves its target format from the active build target (see
            // TextureUtility.CompressTextureForBuildTarget), which this test does not control, so which backend
            // runs -- and therefore whether the result is a new object -- differs by environment: on a mobile
            // build target the astcenc backend returns a new ASTC texture and destroys the placeholder, while on a
            // non-mobile one the format resolves to DXT5 and the Unity backend compresses the placeholder in place
            // and returns that same object (which must therefore NOT be destroyed, or the replacement the preview
            // materials just received would be destroyed with it). Assert what holds for both.
            var compressedInPlace = ReferenceEquals(placeholder, capturedTo);
            Assert.IsTrue(capturedTo != null, "The compression result must be a live texture, not destroyed along with the placeholder.");
            Assert.AreNotEqual((int)TextureFormat.RGBA32, (int)capturedTo.format, "The source RGBA32 placeholder must have actually been compressed.");
            if (!compressedInPlace)
            {
                Assert.IsTrue(placeholder == null, "A placeholder replaced by a separate compressed instance must be destroyed once the replacer reports it was replaced.");
            }

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

            var cacheFile = $"test_progressive_predestroyed_{Guid.NewGuid():N}.bin";
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

            var cacheFile = $"test_progressive_underflow_{Guid.NewGuid():N}.bin";
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

        /// <summary>
        /// Verifies bounded parallel dispatch: with <see cref="PreviewTextureCompressionQueue.MaxConcurrentCompressionsOverrideForTesting"/>
        /// forced to 2 and 3 items enqueued, a single dispatch pass (<see cref="PreviewTextureCompressionQueue.DispatchAvailableForTesting"/>,
        /// mirroring one <c>EditorApplication.update</c> tick) starts exactly 2 -- the cap -- leaving the third
        /// still pending, and the two dispatched items are genuinely in flight together (not one after another)
        /// before either is awaited to completion. <see cref="PreviewTextureCompressionQueue.SuspendAutoDispatchForTesting"/>
        /// is set for the whole test: <see cref="PreviewTextureCompressionQueue.TryEnqueue"/> subscribes the real
        /// production dispatch loop to <c>EditorApplication.update</c>, which a running Unity Editor session fires
        /// for real during the <c>yield return null</c>s inside <see cref="TestUtils.WaitForTask"/> below --
        /// without suspending it, that real loop would race this test's own explicit dispatch calls the moment an
        /// in-flight slot frees up, making the "exactly 2, then exactly 1 more" expectations flaky.
        /// </summary>
        [UnityTest]
        public IEnumerator DispatchAvailableForTesting_CapTwo_DispatchesExactlyCapAndLeavesRestPending()
        {
            PreviewTextureCompressionQueue.MaxConcurrentCompressionsOverrideForTesting = 2;
            PreviewTextureCompressionQueue.SuspendAutoDispatchForTesting = true;

            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            var placeholders = new[] { CreateColorTexture(8, 8), CreateColorTexture(8, 8), CreateColorTexture(8, 8) };
            var replacedCount = 0;
            var destroyedResults = new List<Texture2D>();
            PreviewTextureCompressionQueue.RegisterMaterialTextureReplacer((from, to) =>
            {
                replacedCount++;
                destroyedResults.Add(to as Texture2D);
                return 1;
            });

            try
            {
                for (var i = 0; i < placeholders.Length; i++)
                {
                    var cacheFile = $"test_progressive_parallel_{i}_{Guid.NewGuid():N}.bin";
                    var enqueued = PreviewTextureCompressionQueue.TryEnqueue(placeholders[i], compressor, TextureFormat.ASTC_4x4, false, false, null, cacheFile, true);
                    Assert.IsTrue(enqueued);
                }
                Assert.AreEqual(3, PreviewTextureCompressionQueue.PendingCountForTesting);

                var firstBatch = PreviewTextureCompressionQueue.DispatchAvailableForTesting();

                // Both assertions happen before anything is awaited: the dispatch loop only yields control back to
                // this test method once each item's astcenc invocation is genuinely running in the background (see
                // DispatchAvailableForTesting's remarks), so this is a snapshot of real overlap, not just "2 tasks
                // were created."
                Assert.AreEqual(2, firstBatch.Length, "Exactly MaxConcurrentCompressions items should be dispatched in one pass.");
                Assert.AreEqual(2, PreviewTextureCompressionQueue.InFlightCountForTesting, "Two items must be in flight together, not processed one after another.");
                Assert.AreEqual(1, PreviewTextureCompressionQueue.PendingCountForTesting, "The third item must still be waiting for a free slot.");

                yield return TestUtils.WaitForTask(System.Threading.Tasks.Task.WhenAll(firstBatch));

                Assert.AreEqual(0, PreviewTextureCompressionQueue.InFlightCountForTesting);
                Assert.AreEqual(2, replacedCount, "Both dispatched items must have been applied via the replacer.");

                // Dispatch the remaining item (auto-dispatch is still suspended, so this is the only thing that can
                // have consumed it).
                var secondBatch = PreviewTextureCompressionQueue.DispatchAvailableForTesting();
                Assert.AreEqual(1, secondBatch.Length);
                yield return TestUtils.WaitForTask(System.Threading.Tasks.Task.WhenAll(secondBatch));

                Assert.AreEqual(0, PreviewTextureCompressionQueue.PendingCountForTesting);
                Assert.AreEqual(0, PreviewTextureCompressionQueue.InFlightCountForTesting);
                Assert.AreEqual(3, replacedCount, "Every enqueued item must eventually be applied via the replacer, whether dispatched in the first or second batch.");
                foreach (var placeholder in placeholders)
                {
                    Assert.IsTrue(placeholder == null, "Every placeholder must be destroyed once replaced.");
                }
            }
            finally
            {
                PreviewTextureCompressionQueue.SuspendAutoDispatchForTesting = false;
                foreach (var result in destroyedResults)
                {
                    if (result != null)
                    {
                        UnityEngine.Object.DestroyImmediate(result);
                    }
                }
            }
        }

        /// <summary>
        /// Verifies the in-flight budget is a hard cap, not just a typical outcome: with the cap forced to 2 and 5
        /// items enqueued, calling <see cref="PreviewTextureCompressionQueue.DispatchAvailableForTesting"/> a
        /// second time -- while the first batch's 2 items are still genuinely in flight, before either is awaited
        /// -- dispatches nothing new (mirroring <c>OnUpdate</c> firing again on the very next editor tick while
        /// the previous tick's dispatched items have not finished yet). Only after the first batch is awaited to
        /// completion does dispatching resume, and every item is eventually applied via the replacer exactly once.
        /// <see cref="PreviewTextureCompressionQueue.SuspendAutoDispatchForTesting"/> is set for the whole test --
        /// see <see cref="DispatchAvailableForTesting_CapTwo_DispatchesExactlyCapAndLeavesRestPending"/>'s remarks
        /// for why it is required whenever a test asserts an exact dispatched count around a <c>yield return</c>.
        /// </summary>
        [UnityTest]
        public IEnumerator DispatchAvailableForTesting_SecondCallWhileFirstBatchInFlight_DispatchesNothingNew()
        {
            PreviewTextureCompressionQueue.MaxConcurrentCompressionsOverrideForTesting = 2;
            PreviewTextureCompressionQueue.SuspendAutoDispatchForTesting = true;

            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            const int itemCount = 5;
            var placeholders = new Texture2D[itemCount];
            for (var i = 0; i < itemCount; i++)
            {
                placeholders[i] = CreateColorTexture(8, 8);
            }

            var replacedCount = 0;
            var results = new List<Texture2D>();
            PreviewTextureCompressionQueue.RegisterMaterialTextureReplacer((from, to) =>
            {
                replacedCount++;
                results.Add(to as Texture2D);
                return 1;
            });

            try
            {
                foreach (var placeholder in placeholders)
                {
                    var cacheFile = $"test_progressive_capbound_{Guid.NewGuid():N}.bin";
                    Assert.IsTrue(PreviewTextureCompressionQueue.TryEnqueue(placeholder, compressor, TextureFormat.ASTC_4x4, false, false, null, cacheFile, true));
                }

                var firstBatch = PreviewTextureCompressionQueue.DispatchAvailableForTesting();
                Assert.AreEqual(2, firstBatch.Length);
                Assert.AreEqual(2, PreviewTextureCompressionQueue.InFlightCountForTesting);

                // The would-be-next tick: firstBatch's 2 items are still running (not awaited yet), so the budget
                // is exhausted and this must be a no-op, regardless of the 3 items still waiting in Pending.
                var secondCallWhileFirstBatchStillRunning = PreviewTextureCompressionQueue.DispatchAvailableForTesting();
                Assert.AreEqual(0, secondCallWhileFirstBatchStillRunning.Length, "Dispatching again while the in-flight budget is exhausted must start nothing new.");
                Assert.AreEqual(2, PreviewTextureCompressionQueue.InFlightCountForTesting, "In-flight count must not change when nothing new was dispatched.");
                Assert.AreEqual(3, PreviewTextureCompressionQueue.PendingCountForTesting, "The 3 still-waiting items must remain untouched, not partially consumed.");

                yield return TestUtils.WaitForTask(System.Threading.Tasks.Task.WhenAll(firstBatch));
                Assert.AreEqual(0, PreviewTextureCompressionQueue.InFlightCountForTesting);
                Assert.AreEqual(2, replacedCount);

                // Drain the rest, batch by batch, confirming the cap holds every time (auto-dispatch is still
                // suspended, so this loop -- not the real production hook -- is what drives every remaining item).
                while (PreviewTextureCompressionQueue.PendingCountForTesting > 0)
                {
                    var batch = PreviewTextureCompressionQueue.DispatchAvailableForTesting();
                    Assert.LessOrEqual(batch.Length, 2, "No batch may ever dispatch more than the cap.");
                    Assert.LessOrEqual(PreviewTextureCompressionQueue.InFlightCountForTesting, 2, "In-flight count must never exceed the cap.");
                    yield return TestUtils.WaitForTask(System.Threading.Tasks.Task.WhenAll(batch));
                }

                Assert.AreEqual(itemCount, replacedCount, "Every enqueued item must be applied via the replacer exactly once.");
                Assert.AreEqual(0, PreviewTextureCompressionQueue.InFlightCountForTesting);
            }
            finally
            {
                PreviewTextureCompressionQueue.SuspendAutoDispatchForTesting = false;
                foreach (var result in results)
                {
                    if (result != null)
                    {
                        UnityEngine.Object.DestroyImmediate(result);
                    }
                }
            }
        }

        /// <summary>
        /// Verifies the single-core-machine (cap == 1) case behaves exactly like the pre-parallel-dispatch queue:
        /// with the cap forced to 1 and 2 items enqueued, a dispatch pass starts only one item at a time, matching
        /// what <see cref="PreviewTextureCompressionQueue.ProcessNextForTesting"/> already verified for the
        /// original single-item-at-a-time design in the other tests in this file.
        /// <see cref="PreviewTextureCompressionQueue.SuspendAutoDispatchForTesting"/> is set for the whole test --
        /// see <see cref="DispatchAvailableForTesting_CapTwo_DispatchesExactlyCapAndLeavesRestPending"/>'s remarks
        /// for why it is required whenever a test asserts an exact dispatched count around a <c>yield return</c>.
        /// </summary>
        [UnityTest]
        public IEnumerator DispatchAvailableForTesting_CapOne_MatchesPreParallelSingleItemBehavior()
        {
            PreviewTextureCompressionQueue.MaxConcurrentCompressionsOverrideForTesting = 1;
            PreviewTextureCompressionQueue.SuspendAutoDispatchForTesting = true;

            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            var placeholderA = CreateColorTexture(8, 8);
            var placeholderB = CreateColorTexture(8, 8);

            var replacedCount = 0;
            var results = new List<Texture2D>();
            PreviewTextureCompressionQueue.RegisterMaterialTextureReplacer((from, to) =>
            {
                replacedCount++;
                results.Add(to as Texture2D);
                return 1;
            });

            try
            {
                Assert.IsTrue(PreviewTextureCompressionQueue.TryEnqueue(placeholderA, compressor, TextureFormat.ASTC_4x4, false, false, null, $"test_progressive_capone_a_{Guid.NewGuid():N}.bin", true));
                Assert.IsTrue(PreviewTextureCompressionQueue.TryEnqueue(placeholderB, compressor, TextureFormat.ASTC_4x4, false, false, null, $"test_progressive_capone_b_{Guid.NewGuid():N}.bin", true));

                var firstBatch = PreviewTextureCompressionQueue.DispatchAvailableForTesting();
                Assert.AreEqual(1, firstBatch.Length, "Only one item should be dispatched at a time when the cap is 1.");
                Assert.AreEqual(1, PreviewTextureCompressionQueue.InFlightCountForTesting);
                Assert.AreEqual(1, PreviewTextureCompressionQueue.PendingCountForTesting);

                yield return TestUtils.WaitForTask(System.Threading.Tasks.Task.WhenAll(firstBatch));
                Assert.AreEqual(1, replacedCount);

                var secondBatch = PreviewTextureCompressionQueue.DispatchAvailableForTesting();
                Assert.AreEqual(1, secondBatch.Length);
                yield return TestUtils.WaitForTask(System.Threading.Tasks.Task.WhenAll(secondBatch));

                Assert.AreEqual(2, replacedCount);
                Assert.AreEqual(0, PreviewTextureCompressionQueue.PendingCountForTesting);
                Assert.AreEqual(0, PreviewTextureCompressionQueue.InFlightCountForTesting);
            }
            finally
            {
                PreviewTextureCompressionQueue.SuspendAutoDispatchForTesting = false;
                foreach (var result in results)
                {
                    if (result != null)
                    {
                        UnityEngine.Object.DestroyImmediate(result);
                    }
                }
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
