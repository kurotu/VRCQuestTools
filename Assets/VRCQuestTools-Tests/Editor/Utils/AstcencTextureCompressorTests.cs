// <copyright file="AstcencTextureCompressorTests.cs" company="kurotu">
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
    /// Tests for AstcencTextureCompressor and its selection via TextureCompressorProvider.
    /// </summary>
    public class AstcencTextureCompressorTests
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
        /// Verifies that compressing a mipmapped RGBA32 texture produces raw data whose length matches the sum
        /// of AstcUtility.GetMipDataSize across all mip levels, for every supported ASTC block size.
        /// </summary>
        /// <param name="format">ASTC format under test.</param>
        [TestCase(TextureFormat.ASTC_4x4)]
        [TestCase(TextureFormat.ASTC_5x5)]
        [TestCase(TextureFormat.ASTC_6x6)]
        [TestCase(TextureFormat.ASTC_8x8)]
        [TestCase(TextureFormat.ASTC_10x10)]
        [TestCase(TextureFormat.ASTC_12x12)]
        public void CompressTexture_MipChainConcatenation_MatchesExpectedSize(TextureFormat format)
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            Assert.IsTrue(AstcUtility.TryGetBlockSize(format, out var blockX, out var blockY));

            const int size = 64;
            var source = CreateGradientTexture(size, size, mipChain: true);
            var expectedMipmapCount = source.mipmapCount;

            var expectedSize = 0;
            for (var i = 0; i < expectedMipmapCount; i++)
            {
                var w = Math.Max(1, size >> i);
                var h = Math.Max(1, size >> i);
                expectedSize += AstcUtility.GetMipDataSize(w, h, blockX, blockY);
            }

            Texture2D result = null;
            compressor.CompressTexture(source, format, t => result = t).WaitForCompletion();

            Assert.IsNotNull(result);

            // Compare by enum value, not name: EditorUtility.CompressTexture can rename ASTC_6x6 to the
            // ASTC_RGB_6x6 alias (same underlying value), but astcenc-produced textures keep the exact
            // format value that was requested since they never go through EditorUtility.CompressTexture.
            Assert.AreEqual((int)format, (int)result.format);
            Assert.AreEqual(expectedMipmapCount, result.mipmapCount);
            Assert.AreEqual(expectedSize, result.GetRawTextureData().Length);
        }

        /// <summary>
        /// Verifies non-square NPOT dimensions are handled correctly by the raw-layout mip concatenation (each
        /// mip's width/height is independently halved and rounded down, so a non-square source can produce mips
        /// whose aspect ratio drifts from the source's), and that the compressed result still decodes to
        /// approximately the same image as Unity's own ASTC encoder.
        /// </summary>
        [Test]
        public void CompressTexture_NonSquareNpot_MatchesExpectedLayoutAndQuality()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            const int width = 48;
            const int height = 20;
            const TextureFormat format = TextureFormat.ASTC_4x4;
            Assert.IsTrue(AstcUtility.TryGetBlockSize(format, out var blockX, out var blockY));

            var astcSource = CreateGradientTexture(width, height, mipChain: true);
            var mipmapCount = astcSource.mipmapCount;

            var expectedSize = 0;
            for (var level = 0; level < mipmapCount; level++)
            {
                var w = Math.Max(1, width >> level);
                var h = Math.Max(1, height >> level);
                expectedSize += AstcUtility.GetMipDataSize(w, h, blockX, blockY);
            }

            Texture2D astcResult = null;
            compressor.CompressTexture(astcSource, format, t => astcResult = t).WaitForCompletion();
            Assert.IsNotNull(astcResult);
            Assert.AreEqual(mipmapCount, astcResult.mipmapCount);
            Assert.AreEqual(expectedSize, astcResult.GetRawTextureData().Length);

            var unitySource = CreateGradientTexture(width, height, mipChain: true);
            EditorUtility.CompressTexture(unitySource, format, TextureCompressionQuality.Best);

            var unityDecoded = TestUtils.DecodeToRGBA32(unitySource, width, height);
            var astcDecoded = TestUtils.DecodeToRGBA32(astcResult, width, height);

            var diff = TestUtils.MaxDifference(unityDecoded, astcDecoded);
            Assert.Less(diff, 0.1f, $"astcenc output for a non-square NPOT texture doesn't match Unity's ASTC encoder (diff={diff:F4}).");
        }

        /// <summary>
        /// Verifies the TGA orientation used for astcenc input (topToBottom) produces the same visual result as
        /// Unity's own ASTC encoder for an asymmetric image, i.e. astcenc's output is not vertically flipped.
        /// </summary>
        [Test]
        public void CompressTexture_Orientation_MatchesUnityCompressor()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            const int size = 32;

            var unityTex = CreateOrientationTestTexture(size);
            EditorUtility.CompressTexture(unityTex, TextureFormat.ASTC_4x4, TextureCompressionQuality.Best);

            var astcSource = CreateOrientationTestTexture(size);
            Texture2D astcResult = null;
            compressor.CompressTexture(astcSource, TextureFormat.ASTC_4x4, t => astcResult = t).WaitForCompletion();
            Assert.IsNotNull(astcResult);

            var unityDecoded = TestUtils.DecodeToRGBA32(unityTex, size, size);
            var astcDecoded = TestUtils.DecodeToRGBA32(astcResult, size, size);

            var diff = TestUtils.MaxDifference(unityDecoded, astcDecoded);
            Assert.Less(diff, 0.1f, $"astcenc output orientation doesn't match Unity's ASTC encoder (diff={diff:F4}). " +
                "If this fails, AstcencTextureCompressor.TgaTopToBottomOrigin must be flipped.");
        }

        /// <summary>
        /// Verifies that a non-readable texture (isReadable == false, as produced by a GPU readback) still
        /// compresses successfully via the astcenc path, since GetRawTextureData works on such textures in the
        /// editor even though other pixel accessors (GetPixels32/GetPixelData) do not.
        /// </summary>
        [Test]
        public void CompressTexture_NonReadableInput_Succeeds()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            const int size = 16;

            var source = CreateNonReadableTexture(size);
            Assert.IsFalse(source.isReadable);

            var countBefore = AstcencTextureCompressor.SuccessfulCompressionCount;
            Texture2D result = null;
            compressor.CompressTexture(source, TextureFormat.ASTC_4x4, t => result = t).WaitForCompletion();

            Assert.IsNotNull(result);
            Assert.AreEqual((int)TextureFormat.ASTC_4x4, (int)result.format);

            // The Unity fallback also produces a valid ASTC texture, so the assertions above cannot tell the
            // two paths apart. The counter proves the astcenc process actually ran; without it this test kept
            // passing while every non-readable texture silently fell back (GetRawTextureData<byte>() throws for
            // non-readable textures while the byte[] overload does not).
            Assert.AreEqual(countBefore + 1, AstcencTextureCompressor.SuccessfulCompressionCount, "The compression must take the astcenc path, not the Unity fallback.");
        }

        /// <summary>
        /// Verifies the documented discard semantics of a successful astcenc compression (unlike
        /// <see cref="UnityTextureCompressor"/>, which compresses in place and returns the same reference): the
        /// returned texture is a new, valid instance, and the input texture has been destroyed. Unity's overloaded
        /// <c>==</c> operator returns true for a destroyed <see cref="UnityEngine.Object"/> even though the C#
        /// reference itself is not null, so <c>source == null</c> is the correct way to observe this. See also
        /// <see cref="TextureUtility.CompressTextureForBuildTarget"/>'s XML doc, which documents this contract for
        /// callers.
        /// </summary>
        [Test]
        public void CompressTexture_Success_ReturnsNewInstanceAndDestroysInput()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore();
            var source = CreateGradientTexture(16, 16, mipChain: false);

            var countBefore = AstcencTextureCompressor.SuccessfulCompressionCount;
            Texture2D result = null;
            compressor.CompressTexture(source, TextureFormat.ASTC_4x4, t => result = t).WaitForCompletion();

            Assert.AreEqual(countBefore + 1, AstcencTextureCompressor.SuccessfulCompressionCount, "The compression must take the astcenc path, not the Unity fallback.");
            Assert.IsNotNull(result);
            Assert.IsTrue(result, "The returned texture must be a valid, non-destroyed object.");
            Assert.AreNotSame(source, result, "A successful astcenc compression returns a new instance rather than mutating the input in place.");
            Assert.IsTrue(source == null, "The input texture must be destroyed on a successful astcenc compression.");
        }

        /// <summary>
        /// Verifies that a non-functional astcenc executable path falls back to Unity's texture compression,
        /// logs a warning, and leaves the original input texture intact (UnityTextureCompressor mutates the
        /// texture in place and returns the same reference, so the fallback result must be the same object).
        /// </summary>
        [Test]
        public void CompressTexture_ExecutableMissing_FallsBackToUnityAndKeepsInputIntact()
        {
            var missingPath = Path.Combine(Path.GetTempPath(), $"vrcqt-astcenc-missing-{Guid.NewGuid():N}.exe");
            var compressor = new AstcencTextureCompressor(missingPath, "0.0.0", "-medium");

            var source = CreateGradientTexture(16, 16, mipChain: false);

            LogAssert.Expect(LogType.Warning, new Regex("astcenc compression failed.*falling back"));
            Texture2D result = null;
            compressor.CompressTexture(source, TextureFormat.ASTC_4x4, t => result = t).WaitForCompletion();

            Assert.IsNotNull(result);
            Assert.AreSame(source, result, "Unity's fallback compressor mutates the input in place; the same " +
                "reference coming back proves the input texture was not destroyed before falling back.");
            Assert.AreEqual((int)TextureFormat.ASTC_4x4, (int)result.format);
        }

        /// <summary>
        /// Verifies that astcenc's "-thorough" preset produces quality in the same order as Unity's Best quality
        /// preset for a moderately complex procedural texture (astcenc diff should not be more than roughly
        /// double Unity's diff from the uncompressed source).
        /// </summary>
        [Test]
        public void CompressTexture_Quality_SimilarToUnityCompressor()
        {
            var compressor = TestUtils.CreateAstcencCompressorOrIgnore(TextureCompressorProvider.FinalPreset);
            const int size = 64;

            var reference = CreateNaturalisticTestTexture(size);
            var unitySource = CreateNaturalisticTestTexture(size);
            var astcSource = CreateNaturalisticTestTexture(size);

            EditorUtility.CompressTexture(unitySource, TextureFormat.ASTC_4x4, TextureCompressionQuality.Best);

            Texture2D astcResult = null;
            compressor.CompressTexture(astcSource, TextureFormat.ASTC_4x4, t => astcResult = t).WaitForCompletion();
            Assert.IsNotNull(astcResult);

            var referenceDecoded = TestUtils.DecodeToRGBA32(reference, size, size);
            var unityDecoded = TestUtils.DecodeToRGBA32(unitySource, size, size);
            var astcDecoded = TestUtils.DecodeToRGBA32(astcResult, size, size);

            var diffUnity = TestUtils.Difference(referenceDecoded, unityDecoded);
            var diffAstc = TestUtils.Difference(referenceDecoded, astcDecoded);

            Assert.Less(diffAstc, (diffUnity * 2f) + 0.001f,
                $"astcenc quality (diff={diffAstc:F5}) should be within roughly 2x of Unity's Best quality (diff={diffUnity:F5}).");
        }

        /// <summary>
        /// Verifies TextureCompressorProvider selects an AstcencTextureCompressor for any supported ASTC format
        /// (color or normal map alike -- selection depends only on the format) when a usable astcenc executable
        /// is available, and Unity's compressor otherwise (non-ASTC formats, or no format at all).
        /// </summary>
        [Test]
        public void GetCompressor_SelectsAstcencForAstcFormats_WhenAvailable()
        {
            var path = AstcencBinaryLocator.GetAstcencPath();
            if (path == null)
            {
                Assert.Ignore("No usable astcenc executable is available in this environment.");
            }

            var astcCompressor = TextureCompressorProvider.GetCompressor(TextureFormat.ASTC_6x6);
            Assert.IsInstanceOf<AstcencTextureCompressor>(astcCompressor);

            // Normal maps use the same ASTC format selection as color textures now that AstcencTextureCompressor
            // implements CompressNormalMap.
            var normalMapCompressor = TextureCompressorProvider.GetCompressor(TextureFormat.ASTC_6x6);
            Assert.IsInstanceOf<AstcencTextureCompressor>(normalMapCompressor);

            var dxtCompressor = TextureCompressorProvider.GetCompressor(TextureFormat.DXT5);
            Assert.IsInstanceOf<UnityTextureCompressor>(dxtCompressor);

            // A null format (e.g. a non-mobile normal map, left for TextureGenerator to decide) always falls
            // back to Unity, regardless of astcenc availability.
            var nullFormatCompressor = TextureCompressorProvider.GetCompressor(null);
            Assert.IsInstanceOf<UnityTextureCompressor>(nullFormatCompressor);
        }

        /// <summary>
        /// Verifies that editor previews get the faster preset while final conversions get the quality preset,
        /// and that the two are distinguishable through the cache key so their results never share a cache entry.
        /// </summary>
        [Test]
        public void GetCompressor_UsesFasterPresetForEditorPreview()
        {
            if (AstcencBinaryLocator.GetAstcencPath() == null)
            {
                Assert.Ignore("No usable astcenc executable is available in this environment.");
            }

            var final = (AstcencTextureCompressor)TextureCompressorProvider.GetCompressor(TextureFormat.ASTC_6x6, false);
            var preview = (AstcencTextureCompressor)TextureCompressorProvider.GetCompressor(TextureFormat.ASTC_6x6, true);

            Assert.AreNotSame(final, preview);
            StringAssert.EndsWith(TextureCompressorProvider.FinalPreset.TrimStart('-'), final.CacheKeyComponent);
            StringAssert.EndsWith(TextureCompressorProvider.PreviewPreset.TrimStart('-'), preview.CacheKeyComponent);
            Assert.AreNotEqual(final.CacheKeyComponent, preview.CacheKeyComponent);
        }

        /// <summary>
        /// Verifies the testing hook overrides the normal selection logic and can be reset.
        /// </summary>
        [Test]
        public void SetCompressorForTesting_OverridesSelection()
        {
            var fake = new UnityTextureCompressor();
            TextureCompressorProvider.SetCompressorForTesting(fake);
            Assert.AreSame(fake, TextureCompressorProvider.GetCompressor(TextureFormat.ASTC_6x6));

            TextureCompressorProvider.ResetForTesting();
            Assert.AreNotSame(fake, TextureCompressorProvider.GetCompressor(TextureFormat.ASTC_6x6));
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

        private static Texture2D CreateNaturalisticTestTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false, false);
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var r = (byte)(127 + (127 * Mathf.Sin(x * 0.3f)));
                    var g = (byte)(127 + (127 * Mathf.Cos(y * 0.25f)));
                    var b = (byte)((x * 4 + y * 4) % 256);
                    var a = (byte)(200 + ((x + y) % 56));
                    pixels[(y * size) + x] = new Color32(r, g, b, a);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return tex;
        }

        private static Texture2D CreateOrientationTestTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false, false);
            var pixels = new Color32[size * size];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(30, 30, 30, 255);
            }

            // Asymmetric markers: distinct colors at the first and last elements of the pixel array, so a
            // vertical flip between the two compressed outputs is detectable regardless of which end of the
            // array corresponds to the top of the image.
            pixels[0] = new Color32(255, 0, 0, 255);
            pixels[pixels.Length - 1] = new Color32(0, 0, 255, 255);
            tex.SetPixels32(pixels);
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
