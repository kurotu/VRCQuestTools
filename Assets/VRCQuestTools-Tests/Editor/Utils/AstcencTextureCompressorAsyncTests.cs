// <copyright file="AstcencTextureCompressorAsyncTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Tests for the async, off-main-thread compression API added to <see cref="AstcencTextureCompressor"/> for
    /// the NDMF preview's progressive texture replacement (<see cref="PreviewTextureCompressionQueue"/>). These
    /// only verify the async path is a faithful (bit-identical) split of the synchronous path and its input
    /// validation; the sync path's own behavior is covered by <c>AstcencTextureCompressorTests</c> and
    /// <c>AstcencNormalMapCompressionTests</c>, which this change does not touch.
    /// </summary>
    public class AstcencTextureCompressorAsyncTests
    {
        /// <summary>
        /// Resets the compression counter's observability and any log-assert override after each test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
        }

        /// <summary>
        /// Verifies that <see cref="AstcencTextureCompressor.CompressTextureAsync"/> produces byte-for-byte the
        /// same raw texture data as the synchronous <see cref="AstcencTextureCompressor.CompressTexture"/> for
        /// the same source pixels, format, and mip chain.
        /// </summary>
        [UnityTest]
        public IEnumerator CompressTextureAsync_ColorTexture_MatchesSyncBytes()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            var syncSource = CreateGradientTexture(32, 32, mipChain: true);
            var asyncSource = CreateGradientTexture(32, 32, mipChain: true);

            Texture2D syncResult = null;
            compressor.CompressTexture(syncSource, TextureFormat.ASTC_4x4, t => syncResult = t).WaitForCompletion();
            Assert.IsNotNull(syncResult, "Synchronous compression should succeed.");

            var task = compressor.CompressTextureAsync(asyncSource, TextureFormat.ASTC_4x4);
            yield return TestUtils.WaitForTask(task);
            var asyncResult = task.Result;
            Assert.IsNotNull(asyncResult, "Async compression should succeed.");

            Assert.AreEqual((int)syncResult.format, (int)asyncResult.format);
            Assert.AreEqual(syncResult.mipmapCount, asyncResult.mipmapCount);
            CollectionAssert.AreEqual(syncResult.GetRawTextureData(), asyncResult.GetRawTextureData(),
                "Async compression must produce identical bytes to the synchronous path for the same input.");

            // Unlike CompressTexture, CompressTextureAsync never destroys its input (see its XML doc remarks) --
            // the progressive queue's placeholder is still assigned to preview materials while compression runs.
            Assert.IsFalse(asyncSource == null, "CompressTextureAsync must not destroy its input texture.");

            UnityEngine.Object.DestroyImmediate(asyncSource);
            UnityEngine.Object.DestroyImmediate(syncResult);
            UnityEngine.Object.DestroyImmediate(asyncResult);

            // syncSource was already destroyed by the synchronous CompressTexture call on success.
        }

        /// <summary>
        /// Verifies that <see cref="AstcencTextureCompressor.CompressNormalMapAsync"/> produces byte-for-byte the
        /// same raw texture data as the synchronous <see cref="AstcencTextureCompressor.CompressNormalMap"/> for
        /// the same source pixels, format, and mip chain.
        /// </summary>
        [UnityTest]
        public IEnumerator CompressNormalMapAsync_NormalMap_MatchesSyncBytes()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            var syncSource = CreateReadableNormalTexture(16);
            var asyncSource = CreateReadableNormalTexture(16);

            Texture2D syncResult = null;
            compressor.CompressNormalMap(syncSource, TextureFormat.ASTC_4x4, true, null, t => syncResult = t).WaitForCompletion();
            Assert.IsNotNull(syncResult, "Synchronous normal map compression should succeed.");

            var task = compressor.CompressNormalMapAsync(asyncSource, TextureFormat.ASTC_4x4, true, null);
            yield return TestUtils.WaitForTask(task);
            var asyncResult = task.Result;
            Assert.IsNotNull(asyncResult, "Async normal map compression should succeed.");

            Assert.AreEqual((int)syncResult.format, (int)asyncResult.format);
            Assert.AreEqual(syncResult.mipmapCount, asyncResult.mipmapCount);
            CollectionAssert.AreEqual(syncResult.GetRawTextureData(), asyncResult.GetRawTextureData(),
                "Async normal map compression must produce identical bytes to the synchronous path for the same input.");

            UnityEngine.Object.DestroyImmediate(syncSource);
            UnityEngine.Object.DestroyImmediate(asyncSource);
            UnityEngine.Object.DestroyImmediate(syncResult);
            UnityEngine.Object.DestroyImmediate(asyncResult);
        }

        /// <summary>
        /// Verifies that maxTextureSize shrinking behaves identically between the sync and async normal map
        /// paths, since the async path re-implements (rather than shares code with) that shrink loop.
        /// </summary>
        [UnityTest]
        public IEnumerator CompressNormalMapAsync_WithMaxTextureSize_MatchesSyncBytes()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            var syncSource = CreateReadableNormalTexture(32);
            var asyncSource = CreateReadableNormalTexture(32);

            Texture2D syncResult = null;
            compressor.CompressNormalMap(syncSource, TextureFormat.ASTC_4x4, true, 16, t => syncResult = t).WaitForCompletion();
            Assert.IsNotNull(syncResult);

            var task = compressor.CompressNormalMapAsync(asyncSource, TextureFormat.ASTC_4x4, true, 16);
            yield return TestUtils.WaitForTask(task);
            var asyncResult = task.Result;
            Assert.IsNotNull(asyncResult);

            Assert.AreEqual(16, asyncResult.width);
            Assert.AreEqual(16, asyncResult.height);
            Assert.AreEqual(syncResult.width, asyncResult.width);
            Assert.AreEqual(syncResult.height, asyncResult.height);
            CollectionAssert.AreEqual(syncResult.GetRawTextureData(), asyncResult.GetRawTextureData());

            UnityEngine.Object.DestroyImmediate(syncSource);
            UnityEngine.Object.DestroyImmediate(asyncSource);
            UnityEngine.Object.DestroyImmediate(syncResult);
            UnityEngine.Object.DestroyImmediate(asyncResult);
        }

        /// <summary>
        /// Verifies that a target format astcenc cannot handle (a non-ASTC format) throws
        /// <see cref="NotSupportedException"/> from the async color path, mirroring the condition under which the
        /// synchronous path silently falls back to <see cref="UnityTextureCompressor"/> -- the async path cannot
        /// do that fallback itself (it is main-thread-only), so it must surface the condition to its caller instead.
        /// </summary>
        [Test]
        public void CompressTextureAsync_NonAstcFormat_ThrowsNotSupportedException()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            var source = CreateGradientTexture(8, 8, mipChain: false);

            // Assert.ThrowsAsync is unavailable in this project's bundled NUnit; GetAwaiter().GetResult() unwraps
            // and rethrows the original exception type (unlike Task.Result's AggregateException wrapping), so
            // Assert.Throws works directly on the synchronously-driven awaiter.
            Assert.Throws<NotSupportedException>(() => compressor.CompressTextureAsync(source, TextureFormat.DXT5).GetAwaiter().GetResult());

            UnityEngine.Object.DestroyImmediate(source);
        }

        /// <summary>
        /// Verifies that a non-readable input throws <see cref="NotSupportedException"/> from the async normal
        /// map path (the astcenc normal map path requires <see cref="Texture2D.GetPixels32(int)"/>, which needs a
        /// readable texture), mirroring the condition under which the synchronous path falls back to Unity.
        /// </summary>
        [Test]
        public void CompressNormalMapAsync_NonReadableInput_ThrowsNotSupportedException()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            var source = CreateNonReadableTexture(8);
            Assert.IsFalse(source.isReadable);

            Assert.Throws<NotSupportedException>(() => compressor.CompressNormalMapAsync(source, TextureFormat.ASTC_4x4, false, null).GetAwaiter().GetResult());

            UnityEngine.Object.DestroyImmediate(source);
        }

        private static Texture2D CreateGradientTexture(int width, int height, bool mipChain)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, mipChain, false);
            var pixels = new Color32[width * height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var r = (byte)(x * 255 / Math.Max(1, width - 1));
                    var g = (byte)(y * 255 / Math.Max(1, height - 1));
                    pixels[(y * width) + x] = new Color32(r, g, 128, 255);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(mipChain, false);
            return tex;
        }

        private static Texture2D CreateReadableNormalTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var r = (byte)(x * 255 / Math.Max(1, size - 1));
                    var g = (byte)(y * 255 / Math.Max(1, size - 1));
                    pixels[(y * size) + x] = new Color32(r, g, 255, 255);
                }
            }
            tex.SetPixels32(pixels);

            // makeNoLongerReadable=false: mirrors TextureUtility.DownscaleNormalMap's output, which is what
            // MaterialGeneratorUtility's normal map generators actually feed into CompressNormalMap(Async).
            tex.Apply(false, false);
            return tex;
        }

        private static Texture2D CreateNonReadableTexture(int size)
        {
            RenderTexture rt = null;
            var prevActive = RenderTexture.active;
            try
            {
                rt = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32);
                var source = CreateGradientTexture(size, size, mipChain: false);
                try
                {
                    Graphics.Blit(source, rt);
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(source);
                }

                RenderTexture.active = rt;
                var result = new Texture2D(size, size, TextureFormat.RGBA32, false);
                result.ReadPixels(new Rect(0, 0, size, size), 0, 0);
                result.Apply(false, true); // makeNoLongerReadable = true.
                return result;
            }
            finally
            {
                RenderTexture.active = prevActive;
                if (rt != null)
                {
                    RenderTexture.ReleaseTemporary(rt);
                }
            }
        }
    }
}
