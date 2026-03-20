// Tests for TextureUtility additional methods
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

using EditorBuildTarget = UnityEditor.BuildTarget;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    internal class TextureUtilityExtendedTests
    {
        [Test]
        public void GetCompressionFormat_ASTC6x6_ReturnsASTC6x6()
        {
            var result = TextureUtility.GetCompressionFormat(MobileTextureFormat.ASTC_6x6);
            Assert.AreEqual(TextureFormat.ASTC_6x6, result);
        }

        [Test]
        public void GetCompressionFormat_ASTC4x4_ReturnsASTC4x4()
        {
            var result = TextureUtility.GetCompressionFormat(MobileTextureFormat.ASTC_4x4);
            Assert.AreEqual(TextureFormat.ASTC_4x4, result);
        }

        [Test]
        public void GetCompressionFormat_NoOverride_ReturnsASTC6x6()
        {
            var result = TextureUtility.GetCompressionFormat(MobileTextureFormat.NoOverride);
            Assert.AreEqual(TextureFormat.ASTC_6x6, result);
        }

        [Test]
        public void CreateMinimumEmptyTexture_ReturnsValidTexture()
        {
            var tex = TextureUtility.CreateMinimumEmptyTexture();
            try
            {
                Assert.IsNotNull(tex);
                Assert.AreEqual(4, tex.width);
                Assert.AreEqual(4, tex.height);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CreateColorTexture_Color32_ReturnsCorrectColor()
        {
            var color = new Color32(255, 0, 0, 255);
            var tex = TextureUtility.CreateColorTexture(color, 4, 4);
            try
            {
                Assert.IsNotNull(tex);
                Assert.AreEqual(4, tex.width);
                Assert.AreEqual(4, tex.height);
                var pixel = tex.GetPixel(0, 0);
                Assert.AreEqual(1f, pixel.r, 0.01f);
                Assert.AreEqual(0f, pixel.g, 0.01f);
                Assert.AreEqual(0f, pixel.b, 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CreateColorTexture_SinglePixel_ReturnsTexture()
        {
            var color = new Color32(0, 255, 0, 128);
            var tex = TextureUtility.CreateColorTexture(color);
            try
            {
                Assert.IsNotNull(tex);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void IsKnownTextureFormat_ASTC_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.ASTC_4x4));
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.ASTC_6x6));
        }

        [Test]
        public void IsKnownTextureFormat_DXT_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.DXT1));
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.DXT5));
        }

        [Test]
        public void IsKnownTextureFormat_ETC_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.ETC2_RGBA8));
        }

        [Test]
        public void IsUncompressedFormat_RGBA32_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsUncompressedFormat(TextureFormat.RGBA32));
        }

        [Test]
        public void IsUncompressedFormat_ASTC_ReturnsFalse()
        {
            Assert.IsFalse(TextureUtility.IsUncompressedFormat(TextureFormat.ASTC_6x6));
        }

        [Test]
        public void IsSupportedTextureFormat_ASTC_Android_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.ASTC_6x6, EditorBuildTarget.Android));
        }

        [Test]
        public void IsSupportedTextureFormat_DXT_StandaloneWindows_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.DXT5, EditorBuildTarget.StandaloneWindows64));
        }

        [Test]
        public void AspectFitReduction_SquareToSmaller_ReturnsCorrect()
        {
            var result = TextureUtility.AspectFitReduction(1024, 1024, 512);
            Assert.AreEqual(512, result.Width);
            Assert.AreEqual(512, result.Height);
        }

        [Test]
        public void AspectFitReduction_WideToSmaller_ReturnsCorrect()
        {
            var result = TextureUtility.AspectFitReduction(2048, 1024, 512);
            Assert.AreEqual(512, result.Width);
            Assert.AreEqual(256, result.Height);
        }

        [Test]
        public void AspectFitReduction_TallToSmaller_ReturnsCorrect()
        {
            var result = TextureUtility.AspectFitReduction(512, 2048, 512);
            Assert.AreEqual(128, result.Width);
            Assert.AreEqual(512, result.Height);
        }

        [Test]
        public void AspectFitReduction_SmallerThanMax_ReturnsOriginal()
        {
            var result = TextureUtility.AspectFitReduction(256, 128, 512);
            Assert.AreEqual(256, result.Width);
            Assert.AreEqual(128, result.Height);
        }

        [Test]
        public void GetImageContentsHash_ReturnsNonEmpty()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                var hash = TextureUtility.GetImageContentsHash(tex);
                Assert.IsTrue(hash.isValid);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void GetImageContentsHash_DifferentTextures_DifferentHashes()
        {
            var tex1 = new Texture2D(4, 4);
            var tex2 = new Texture2D(8, 8);
            tex2.SetPixel(0, 0, Color.red);
            tex2.Apply();
            try
            {
                var hash1 = TextureUtility.GetImageContentsHash(tex1);
                var hash2 = TextureUtility.GetImageContentsHash(tex2);
                Assert.AreNotEqual(hash1, hash2);
            }
            finally
            {
                Object.DestroyImmediate(tex1);
                Object.DestroyImmediate(tex2);
            }
        }

        [Test]
        public void IsNormalMapAsset_NonAssetTexture_ReturnsFalse()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                var result = TextureUtility.IsNormalMapAsset(tex);
                Assert.IsFalse(result);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void DestroyTexture_NonAssetTexture_DoesNotThrow()
        {
            var tex = new Texture2D(4, 4);
            // DestroyTexture skips non-null results from GetAssetPath (returns "" for runtime textures)
            Assert.DoesNotThrow(() => TextureUtility.DestroyTexture(tex));
            Object.DestroyImmediate(tex);
        }

        [Test]
        public void DestroyTexture_Null_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => TextureUtility.DestroyTexture(null));
        }

        [Test]
        public void CopyAsReadable_CreatesReadableCopy()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.red);
            tex.Apply();
            try
            {
                var copy = TextureUtility.CopyAsReadable(tex, true);
                try
                {
                    Assert.IsNotNull(copy);
                    Assert.AreEqual(4, copy.width);
                    Assert.AreEqual(4, copy.height);
                }
                finally
                {
                    Object.DestroyImmediate(copy);
                }
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void SetStreamingMipMaps_SetsValue()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                TextureUtility.SetStreamingMipMaps(tex, true);
                Assert.IsTrue(tex.streamingMipmaps);
                TextureUtility.SetStreamingMipMaps(tex, false);
                Assert.IsFalse(tex.streamingMipmaps);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void GetBestPlatformOverrideSettings_NoTextures_ReturnsNull()
        {
            var result = TextureUtility.GetBestPlatformOverrideSettings();
            Assert.IsNull(result);
        }

        [Test]
        public void GetBestPlatformOverrideSettings_NonAssetTextures_ReturnsNull()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                var result = TextureUtility.GetBestPlatformOverrideSettings(tex);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }
    }
}
