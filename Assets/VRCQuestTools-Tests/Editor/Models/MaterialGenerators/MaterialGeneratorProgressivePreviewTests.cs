// <copyright file="MaterialGeneratorProgressivePreviewTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Tests for the NDMF editor preview's "progressive" texture replacement branch added to
    /// <see cref="MaterialGeneratorUtility.GenerateTexture"/>'s completion closure: on a cache miss, with
    /// <c>forEditorPreview</c> and an astcenc-compatible format, it should hand back an uncompressed placeholder
    /// immediately and enqueue background compression via <see cref="PreviewTextureCompressionQueue"/>, instead
    /// of compressing synchronously. When astcenc is unavailable, it must fall through to the original
    /// synchronous behavior unchanged.
    /// </summary>
    public class MaterialGeneratorProgressivePreviewTests
    {
        private Func<Texture, Texture, int> savedReplacer;
        private string testTexturesPath;

        /// <summary>
        /// Saves the currently registered material-texture-replacer and prepares a scratch textures directory.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            savedReplacer = PreviewTextureCompressionQueue.MaterialTextureReplacerForTesting;
            testTexturesPath = $"Assets/VRCQuestTools-Tests/Temp/{Guid.NewGuid():N}";
        }

        /// <summary>
        /// Restores the previously registered material-texture-replacer and resets the compressor override.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            PreviewTextureCompressionQueue.RegisterMaterialTextureReplacer(savedReplacer);
            TextureCompressorProvider.ResetForTesting();
        }

        /// <summary>
        /// Verifies that when astcenc is unavailable (simulated via <see cref="TextureCompressorProvider.SetCompressorForTesting"/>,
        /// the sanctioned test seam that <see cref="TextureCompressorProvider.GetCompressor"/> -- and therefore
        /// the progressive branch's own availability check -- goes through either way), the progressive branch
        /// is never taken: the queue stays empty and the returned texture is already synchronously compressed,
        /// exactly as it was before this feature existed.
        /// </summary>
        [Test]
        public void GenerateTexture_AstcencUnavailable_DoesNotUseProgressiveQueueAndCompressesSynchronously()
        {
            var fakeUnityCompressor = new UnityTextureCompressor();
            TextureCompressorProvider.SetCompressorForTesting(fakeUnityCompressor);

            var material = new Material(Shader.Find("Standard"));
            var settings = new MockConvertSettings();

            var pendingBefore = PreviewTextureCompressionQueue.PendingCountForTesting;
            Texture2D generated = null;
            try
            {
                MaterialGeneratorUtility.GenerateTexture(
                    material,
                    settings,
                    "progressive_fallback",
                    false,
                    testTexturesPath,
                    (completion) => new ResultRequest<Texture2D>(CreateBakedTexture(32), completion),
                    (tex) => generated = tex,
                    (MaxTextureSize: 0, Format: TextureFormat.ASTC_4x4),
                    forEditorPreview: true)
                    .WaitForCompletion();

                Assert.IsNotNull(generated);
                Assert.AreEqual(pendingBefore, PreviewTextureCompressionQueue.PendingCountForTesting,
                    "No progressive compression should be queued when astcenc is unavailable.");

                // Not asserting a specific compressed format: TextureUtility.ResolveEffectiveCompressionFormat
                // only honors the ASTC platform override when the active build target is Android/iOS, falling
                // back to DXT5 otherwise -- which this test must remain correct under either way. What matters
                // here is that *some* synchronous compression happened rather than being deferred to the queue.
                Assert.AreNotEqual((int)TextureFormat.RGBA32, (int)generated.format,
                    "The texture must already be synchronously compressed, not left in the uncompressed placeholder format.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(material);
                if (generated != null)
                {
                    UnityEngine.Object.DestroyImmediate(generated);
                }
            }
        }

        /// <summary>
        /// Verifies the progressive path end-to-end when astcenc is available and a material-texture-replacer is
        /// registered (simulating the NDMF preview being active): the completion callback receives an
        /// uncompressed placeholder immediately, the compression is queued rather than run inline, and processing
        /// that queued item hands the compressed result to the registered replacer and destroys the placeholder.
        /// </summary>
        [UnityTest]
        public IEnumerator GenerateTexture_AstcencAvailable_ReturnsPlaceholderImmediatelyAndQueuesCompression()
        {
            if (AstcencBinaryLocator.GetAstcencPath() == null)
            {
                Assert.Ignore("No usable astcenc executable is available in this environment.");
            }

            // TextureUtility.ResolveEffectiveCompressionFormat only resolves an ASTC format (as opposed to
            // DXT5) for color textures when the active build target is Android/iOS, regardless of the platform
            // override passed below -- so progressive compression cannot engage at all otherwise. Unlike astcenc
            // availability, this can't be faked through a test seam (it would require actually switching the
            // active build target, which is slow and not guaranteed to have the platform module installed).
            if (EditorUserBuildSettings.activeBuildTarget != UnityEditor.BuildTarget.Android && EditorUserBuildSettings.activeBuildTarget != UnityEditor.BuildTarget.iOS)
            {
                Assert.Ignore($"Progressive preview compression requires an Android/iOS active build target; current target is {EditorUserBuildSettings.activeBuildTarget}.");
            }

            // Drain any backlog left by earlier tests in this batch (PreviewTextureCompressionQueue is a
            // process-wide static queue, processed one item at a time) using whatever replacer is currently
            // registered, so this test's own enqueue below is guaranteed to be the only -- and therefore the
            // next-dequeued -- pending item. Without this, ProcessNextForTesting() later could dequeue and
            // "process" some earlier test's leftover placeholder instead of this test's own.
            while (PreviewTextureCompressionQueue.PendingCountForTesting > 0)
            {
                var drainTask = PreviewTextureCompressionQueue.ProcessNextForTesting();
                yield return TestUtils.WaitForTask(drainTask);
            }

            Texture2D capturedCompressed = null;
            PreviewTextureCompressionQueue.RegisterMaterialTextureReplacer((from, to) =>
            {
                capturedCompressed = to as Texture2D;
                return 1;
            });

            var material = new Material(Shader.Find("Standard"));
            var settings = new MockConvertSettings();
            Texture2D generated = null;

            // Baseline rather than an absolute count: PreviewTextureCompressionQueue is a process-wide static
            // queue, and other EditMode tests running earlier in the same batch may have enqueued (but not yet
            // drained -- one item processes at a time) items of their own in this environment (Android + astcenc
            // available), since they also exercise MaterialGeneratorUtility with forEditorPreview: true.
            var pendingBefore = PreviewTextureCompressionQueue.PendingCountForTesting;
            try
            {
                MaterialGeneratorUtility.GenerateTexture(
                    material,
                    settings,
                    "progressive_used",
                    false,
                    testTexturesPath,
                    (completion) => new ResultRequest<Texture2D>(CreateBakedTexture(32), completion),
                    (tex) => generated = tex,
                    (MaxTextureSize: 0, Format: TextureFormat.ASTC_4x4),
                    forEditorPreview: true)
                    .WaitForCompletion();

                Assert.IsNotNull(generated);
                Assert.AreEqual((int)TextureFormat.RGBA32, (int)generated.format,
                    "The immediately returned placeholder must still be uncompressed; compression happens in the background.");
                Assert.AreEqual(pendingBefore + 1, PreviewTextureCompressionQueue.PendingCountForTesting,
                    "The texture should have been queued for background compression instead of compressed inline.");

                var task = PreviewTextureCompressionQueue.ProcessNextForTesting();
                yield return TestUtils.WaitForTask(task);
                var processed = task.Result;
                Assert.IsTrue(processed);

                Assert.IsNotNull(capturedCompressed);
                Assert.AreEqual((int)TextureFormat.ASTC_4x4, (int)capturedCompressed.format);
                Assert.IsTrue(generated == null, "The placeholder should have been destroyed once the queue replaced it.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(material);
                if (capturedCompressed != null)
                {
                    UnityEngine.Object.DestroyImmediate(capturedCompressed);
                }
            }
        }

        private static Texture2D CreateBakedTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(255, 0, 0, 255);
            }
            tex.SetPixels32(pixels);

            // Mirrors a GPU-readback bake result (see TextureCPUReadbackRequest/TextureGPUReadbackRequest): not readable.
            tex.Apply(false, true);
            return tex;
        }

        // Cache key is unique per instance (rather than a fixed literal) so this fixture's cache-miss path is
        // exercised every run, unaffected by a JSON cache entry a previous test session may have left on disk
        // under VRCQuestToolsSettings.TextureCacheFolder for the same material/settings/build target combination.
        private class MockConvertSettings : IMaterialConvertSettings
        {
            private readonly string key = Guid.NewGuid().ToString("N");

            public MobileTextureFormat MobileTextureFormat => MobileTextureFormat.ASTC_6x6;

            public void LoadDefaultAssets()
            {
            }

            public string GetCacheKey() => key;
        }
    }
}
