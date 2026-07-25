// <copyright file="AstcencNormalMapCompressionTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Tests for AstcencTextureCompressor.CompressNormalMap (the astcenc normal map compression path) and its
    /// selection via TextureCompressorProvider now that normal maps are routed through the same ASTC format
    /// check as color textures.
    /// </summary>
    public class AstcencNormalMapCompressionTests
    {
        /// <summary>
        /// Resets the global log-assert flag and any test compressor override after each test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            LogAssert.ignoreFailingMessages = false;
            TextureCompressorProvider.ResetForTesting();
        }

        /// <summary>
        /// Verifies the TGA orientation used for astcenc normal map input (topToBottom, sourced from
        /// <see cref="Texture2D.GetPixels32(int)"/>) produces the same visual result as Unity's own normal map
        /// ASTC encoder for an asymmetric image, i.e. astcenc's output is not vertically flipped.
        /// </summary>
        /// <remarks>
        /// UnityTextureCompressor.CompressNormalMap's output comes from TextureGenerator.GenerateTexture, which
        /// (per NormalMapPreviewRenderingTests' class remarks) is not uploaded to the GPU: a direct
        /// Graphics.Blit of it (as TestUtils.DecodeToRGBA32 does) reads back all-zero regardless of the actual
        /// pixel data, which would make this comparison meaningless. TextureUtility.ReuploadForEditorDisplay
        /// works around that the same way production preview code does.
        /// </remarks>
        [Test]
        public void CompressNormalMap_Orientation_MatchesUnityCompressor()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            const int size = 32;

            var unitySource = CreateOrientationTestNormalMap(size);
            Texture2D unityResult = null;
            new UnityTextureCompressor().CompressNormalMap(unitySource, TextureFormat.ASTC_4x4, true, null, t => unityResult = t).WaitForCompletion();
            Assert.IsNotNull(unityResult);
            var unityUploaded = TextureUtility.ReuploadForEditorDisplay(unityResult);

            var astcSource = CreateOrientationTestNormalMap(size);
            var countBefore = AstcencTextureCompressor.SuccessfulCompressionCount;
            Texture2D astcResult = null;
            compressor.CompressNormalMap(astcSource, TextureFormat.ASTC_4x4, true, null, t => astcResult = t).WaitForCompletion();
            Assert.IsNotNull(astcResult);
            Assert.AreEqual(countBefore + 1, AstcencTextureCompressor.SuccessfulCompressionCount, "The compression must take the astcenc path, not the Unity fallback.");

            var unityDecoded = TestUtils.DecodeToRGBA32(unityUploaded, size, size);
            var astcDecoded = TestUtils.DecodeToRGBA32(astcResult, size, size);

            var diff = TestUtils.MaxDifference(unityDecoded, astcDecoded);
            Assert.Less(diff, 0.01f, $"astcenc normal map output orientation doesn't match Unity's ASTC encoder (diff={diff:F4}). " +
                "If this fails, AstcencTextureCompressor.TgaTopToBottomOrigin must be flipped.");
        }

        /// <summary>
        /// Verifies that astcenc's normal map quality is in the same order as Unity's ASTC normal map encoder
        /// for a real fixture normal map, and that the compression actually took the astcenc path.
        /// </summary>
        [Test]
        public void CompressNormalMap_Quality_SimilarToUnityCompressor()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            var sourceNormalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/VRCQuestTools-Tests/Fixtures/Textures/NormalMapSample01.png");
            Assert.IsNotNull(sourceNormalMap, "Failed to load NormalMapSample01.png.");

            LogAssert.ignoreFailingMessages = true;
            var referenceSource = TextureUtility.DownscaleNormalMap(sourceNormalMap, true, sourceNormalMap.width, sourceNormalMap.height);
            var unitySource = TextureUtility.DownscaleNormalMap(sourceNormalMap, true, sourceNormalMap.width, sourceNormalMap.height);
            var astcSource = TextureUtility.DownscaleNormalMap(sourceNormalMap, true, sourceNormalMap.width, sourceNormalMap.height);
            LogAssert.ignoreFailingMessages = false;

            Texture2D unityResult = null;
            new UnityTextureCompressor().CompressNormalMap(unitySource, TextureFormat.ASTC_6x6, true, null, t => unityResult = t).WaitForCompletion();
            Assert.IsNotNull(unityResult);

            // TextureGenerator's output is not uploaded to the GPU; re-upload before Blit-based decoding below,
            // the same way NormalMapPreviewRenderingTests does for production preview code (see also the
            // orientation test's remarks in this file).
            var unityUploaded = TextureUtility.ReuploadForEditorDisplay(unityResult);

            var countBefore = AstcencTextureCompressor.SuccessfulCompressionCount;
            Texture2D astcResult = null;
            compressor.CompressNormalMap(astcSource, TextureFormat.ASTC_6x6, true, null, t => astcResult = t).WaitForCompletion();
            Assert.IsNotNull(astcResult);
            Assert.AreEqual(countBefore + 1, AstcencTextureCompressor.SuccessfulCompressionCount, "The compression must take the astcenc path, not the Unity fallback.");

            var referenceDecoded = TestUtils.DecodeToRGBA32(referenceSource, referenceSource.width, referenceSource.height);
            var unityDecoded = TestUtils.DecodeToRGBA32(unityUploaded, unityUploaded.width, unityUploaded.height);
            var astcDecoded = TestUtils.DecodeToRGBA32(astcResult, astcResult.width, astcResult.height);

            var diffUnity = TestUtils.Difference(referenceDecoded, unityDecoded);
            var diffAstc = TestUtils.Difference(referenceDecoded, astcDecoded);

            Assert.Less(diffAstc, (diffUnity * 2f) + 0.001f,
                $"astcenc normal map quality (diff={diffAstc:F5}) should be within roughly 2x of Unity's normal map encoder quality (diff={diffUnity:F5}).");
        }

        /// <summary>
        /// Verifies that mip 1 of an astcenc-compressed normal map decodes to vectors whose average length is
        /// close to 1, i.e. <see cref="NormalMapMipUtility.DownsampleNormalMap"/>'s re-normalization is actually
        /// wired into the compression path (a plain box filter over encoded bytes would shrink it well below 1
        /// for a source with spatially varying normals).
        /// </summary>
        [Test]
        public void CompressNormalMap_Mip1_IsRenormalized()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            const int size = 64;
            var source = CreateVariedNormalMap(size);

            var countBefore = AstcencTextureCompressor.SuccessfulCompressionCount;
            Texture2D result = null;
            compressor.CompressNormalMap(source, TextureFormat.ASTC_4x4, true, null, t => result = t).WaitForCompletion();
            Assert.IsNotNull(result);
            Assert.AreEqual(countBefore + 1, AstcencTextureCompressor.SuccessfulCompressionCount, "The compression must take the astcenc path, not the Unity fallback.");
            Assert.Greater(result.mipmapCount, 1, "Expected a full mip chain down to 1x1.");

            var mip1Pixels = result.GetPixels32(1);
            Assert.Greater(mip1Pixels.Length, 0);

            var lengthSum = 0f;
            foreach (var p in mip1Pixels)
            {
                var n = NormalMapMipUtility.Decode(p);
                lengthSum += n.magnitude;
            }
            var avgLength = lengthSum / mip1Pixels.Length;
            Assert.Greater(avgLength, 0.98f, $"Average normal vector length at mip1 should be close to 1 after re-normalized downsampling (avg={avgLength:F4}).");
        }

        /// <summary>
        /// Verifies that a maxTextureSize override shrinks mip 0 to the requested size and produces the same
        /// mip chain length as Unity's own maxTextureSize handling for normal maps.
        /// </summary>
        [Test]
        public void CompressNormalMap_MaxTextureSize_ResizesAndMatchesUnityMipChain()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            const int size = 64;
            const int maxSize = 32;

            var astcSource = CreateVariedNormalMap(size);
            var countBefore = AstcencTextureCompressor.SuccessfulCompressionCount;
            Texture2D astcResult = null;
            compressor.CompressNormalMap(astcSource, TextureFormat.ASTC_4x4, true, maxSize, t => astcResult = t).WaitForCompletion();
            Assert.IsNotNull(astcResult);
            Assert.AreEqual(countBefore + 1, AstcencTextureCompressor.SuccessfulCompressionCount, "The compression must take the astcenc path, not the Unity fallback.");
            Assert.AreEqual(maxSize, astcResult.width);
            Assert.AreEqual(maxSize, astcResult.height);

            var unitySource = CreateVariedNormalMap(size);
            Texture2D unityResult = null;
            new UnityTextureCompressor().CompressNormalMap(unitySource, TextureFormat.ASTC_4x4, true, maxSize, t => unityResult = t).WaitForCompletion();
            Assert.IsNotNull(unityResult);
            Assert.AreEqual(unityResult.width, astcResult.width);
            Assert.AreEqual(unityResult.height, astcResult.height);
            Assert.AreEqual(unityResult.mipmapCount, astcResult.mipmapCount);
        }

        /// <summary>
        /// Verifies that a non-functional astcenc executable path falls back to Unity's normal map compression
        /// and logs a warning. Unlike the color path, UnityTextureCompressor.CompressNormalMap always returns a
        /// new instance from TextureGenerator (never mutates in place), so the fallback result is a distinct
        /// object from the input; the input itself must remain intact (not destroyed).
        /// </summary>
        [Test]
        public void CompressNormalMap_ExecutableMissing_FallsBackToUnityAndKeepsInputIntact()
        {
            var missingPath = Path.Combine(Path.GetTempPath(), $"vrcqt-astcenc-normal-missing-{Guid.NewGuid():N}.exe");
            var compressor = new AstcencTextureCompressor(missingPath, "0.0.0", "-medium");

            var source = CreateVariedNormalMap(16);

            LogAssert.Expect(LogType.Warning, new Regex("astcenc normal map compression failed.*falling back"));
            Texture2D result = null;
            compressor.CompressNormalMap(source, TextureFormat.ASTC_4x4, true, null, t => result = t).WaitForCompletion();

            Assert.IsNotNull(result);
            Assert.IsTrue(result, "The returned texture must be a valid, non-destroyed object.");
            Assert.AreNotSame(source, result, "UnityTextureCompressor.CompressNormalMap always returns a new instance via TextureGenerator.");
            Assert.IsTrue(source, "The input texture must remain intact (not destroyed) after falling back.");
            Assert.AreEqual((int)TextureFormat.ASTC_4x4, (int)result.format);
        }

        /// <summary>
        /// Verifies TextureCompressorProvider selects an AstcencTextureCompressor for a normal map with an ASTC
        /// format when a usable astcenc executable is available, mirroring color texture selection.
        /// </summary>
        [Test]
        public void GetCompressor_SelectsAstcencForNormalMapAstcFormat_WhenAvailable()
        {
            var path = AstcencBinaryLocator.GetAstcencPath();
            if (path == null)
            {
                Assert.Ignore("No usable astcenc executable is available in this environment.");
            }

            var compressor = TextureCompressorProvider.GetCompressor(TextureFormat.ASTC_8x8);
            Assert.IsInstanceOf<AstcencTextureCompressor>(compressor);
        }

        /// <summary>
        /// Builds a mostly-flat normal map with two distinctly colored 4x4 regions (matching the ASTC_4x4 block
        /// size) at opposite corners of the pixel array, so a vertical flip between two compressed outputs is
        /// detectable regardless of which end of the array corresponds to the top of the image.
        /// </summary>
        /// <remarks>
        /// The marker regions are whole 4x4 blocks, not single outlier texels: a lone texel that diverges from
        /// an otherwise-uniform 4x4 block is a worst case for ASTC's per-block quantization search, where two
        /// independent encoder implementations (Unity's internal encoder vs. astcenc) could plausibly reconstruct
        /// that one outlier differently enough to exceed this test's threshold without any orientation issue
        /// being involved. A uniform block, by contrast, is reproduced near-losslessly by both encoders, so a
        /// mismatch here reliably indicates a genuine flip rather than quantization noise.
        /// </remarks>
        private static Texture2D CreateOrientationTestNormalMap(int size)
        {
            const int markerSize = 4; // Matches the ASTC_4x4 block size used by the tests that call this.
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            var pixels = new Color32[size * size];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(128, 128, 255, 255); // Flat normal (0, 0, 1).
            }

            for (var y = 0; y < markerSize; y++)
            {
                for (var x = 0; x < markerSize; x++)
                {
                    pixels[(y * size) + x] = new Color32(255, 128, 128, 255); // Tilted normal, one corner block.
                    pixels[((size - 1 - y) * size) + (size - 1 - x)] = new Color32(128, 255, 128, 255); // Different tilted normal, opposite corner block.
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return tex;
        }

        /// <summary>
        /// Generates a normal map whose direction varies spatially with a period small enough that a plain
        /// (non-renormalizing) box filter downsample would produce a visibly shorter-than-1 average vector
        /// length, so mip-renormalization tests actually exercise the renormalization step.
        /// </summary>
        private static Texture2D CreateVariedNormalMap(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var nx = Mathf.Sin(x * 0.5f) * 0.6f;
                    var ny = Mathf.Sin(y * 0.5f) * 0.6f;
                    var nz = Mathf.Sqrt(Mathf.Max(0.01f, 1f - (nx * nx) - (ny * ny)));
                    var n = new Vector3(nx, ny, nz).normalized;
                    pixels[(y * size) + x] = NormalMapMipUtility.Encode(n);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return tex;
        }
    }
}
