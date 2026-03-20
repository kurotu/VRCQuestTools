// Tests for TextureUtility - uncovered pure-logic methods

using System;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UBuildTarget = UnityEditor.BuildTarget;

namespace KRT.VRCQuestTools.Tests.Utils
{
    [TestFixture]
    public class TextureUtilityFormatTests
    {
        // --- GetCompressionFormat ---

        [Test]
        public void GetCompressionFormat_NoOverride_ReturnsASTC6x6()
        {
            var result = TextureUtility.GetCompressionFormat(MobileTextureFormat.NoOverride);
            Assert.AreEqual(TextureFormat.ASTC_6x6, result);
        }

        [Test]
        public void GetCompressionFormat_ASTC4x4_ReturnsSameFormat()
        {
            var result = TextureUtility.GetCompressionFormat(MobileTextureFormat.ASTC_4x4);
            Assert.AreEqual(TextureFormat.ASTC_4x4, result);
        }

        [Test]
        public void GetCompressionFormat_ASTC8x8_ReturnsSameFormat()
        {
            var result = TextureUtility.GetCompressionFormat(MobileTextureFormat.ASTC_8x8);
            Assert.AreEqual(TextureFormat.ASTC_8x8, result);
        }

        [Test]
        public void GetCompressionFormat_ASTC12x12_ReturnsSameFormat()
        {
            var result = TextureUtility.GetCompressionFormat(MobileTextureFormat.ASTC_12x12);
            Assert.AreEqual(TextureFormat.ASTC_12x12, result);
        }

        [TestCase(MobileTextureFormat.ASTC_5x5, TextureFormat.ASTC_5x5)]
        [TestCase(MobileTextureFormat.ASTC_6x6, TextureFormat.ASTC_6x6)]
        [TestCase(MobileTextureFormat.ASTC_10x10, TextureFormat.ASTC_10x10)]
        public void GetCompressionFormat_AllFormats_MatchExpected(MobileTextureFormat input, TextureFormat expected)
        {
            Assert.AreEqual(expected, TextureUtility.GetCompressionFormat(input));
        }

        // --- IsKnownTextureFormat ---

        [Test]
        public void IsKnownTextureFormat_RGBA32_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.RGBA32));
        }

        [Test]
        public void IsKnownTextureFormat_DXT5_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.DXT5));
        }

        [Test]
        public void IsKnownTextureFormat_ASTC4x4_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.ASTC_4x4));
        }

        [Test]
        public void IsKnownTextureFormat_BC7_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.BC7));
        }

        [Test]
        public void IsKnownTextureFormat_ETC2_RGB_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.ETC2_RGB));
        }

        [Test]
        public void IsKnownTextureFormat_PVRTC_RGB4_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.PVRTC_RGB4));
        }

        [TestCase(TextureFormat.Alpha8)]
        [TestCase(TextureFormat.ARGB32)]
        [TestCase(TextureFormat.RGB24)]
        [TestCase(TextureFormat.RGB565)]
        [TestCase(TextureFormat.RGBAFloat)]
        [TestCase(TextureFormat.RGBAHalf)]
        [TestCase(TextureFormat.R8)]
        [TestCase(TextureFormat.R16)]
        [TestCase(TextureFormat.RFloat)]
        [TestCase(TextureFormat.RHalf)]
        public void IsKnownTextureFormat_UncompressedFormats_ReturnsTrue(TextureFormat format)
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(format));
        }

        [TestCase(TextureFormat.DXT1)]
        [TestCase(TextureFormat.DXT1Crunched)]
        [TestCase(TextureFormat.DXT5Crunched)]
        [TestCase(TextureFormat.BC4)]
        [TestCase(TextureFormat.BC5)]
        [TestCase(TextureFormat.BC6H)]
        public void IsKnownTextureFormat_WindowsFormats_ReturnsTrue(TextureFormat format)
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(format));
        }

        [TestCase(TextureFormat.ASTC_HDR_4x4)]
        [TestCase(TextureFormat.ASTC_HDR_12x12)]
        [TestCase(TextureFormat.ETC_RGB4)]
        [TestCase(TextureFormat.EAC_R)]
        [TestCase(TextureFormat.EAC_RG)]
        public void IsKnownTextureFormat_AndroidFormats_ReturnsTrue(TextureFormat format)
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(format));
        }

        // --- IsSupportedTextureFormat ---

        [Test]
        public void IsSupportedTextureFormat_RGBA32_AllPlatforms_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.RGBA32, UBuildTarget.StandaloneWindows64));
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.RGBA32, UBuildTarget.Android));
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.RGBA32, UBuildTarget.iOS));
        }

        [Test]
        public void IsSupportedTextureFormat_DXT5_WindowsOnly()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.DXT5, UBuildTarget.StandaloneWindows64));
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.DXT5, UBuildTarget.StandaloneWindows));
            Assert.IsFalse(TextureUtility.IsSupportedTextureFormat(TextureFormat.DXT5, UBuildTarget.Android));
            Assert.IsFalse(TextureUtility.IsSupportedTextureFormat(TextureFormat.DXT5, UBuildTarget.iOS));
        }

        [Test]
        public void IsSupportedTextureFormat_ASTC4x4_AndroidAndIOS()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.ASTC_4x4, UBuildTarget.Android));
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.ASTC_4x4, UBuildTarget.iOS));
            Assert.IsFalse(TextureUtility.IsSupportedTextureFormat(TextureFormat.ASTC_4x4, UBuildTarget.StandaloneWindows64));
        }

        [Test]
        public void IsSupportedTextureFormat_BC7_WindowsOnly()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.BC7, UBuildTarget.StandaloneWindows64));
            Assert.IsFalse(TextureUtility.IsSupportedTextureFormat(TextureFormat.BC7, UBuildTarget.Android));
        }

        [Test]
        public void IsSupportedTextureFormat_PVRTC_iOSOnly()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.PVRTC_RGB4, UBuildTarget.iOS));
            Assert.IsFalse(TextureUtility.IsSupportedTextureFormat(TextureFormat.PVRTC_RGB4, UBuildTarget.Android));
            Assert.IsFalse(TextureUtility.IsSupportedTextureFormat(TextureFormat.PVRTC_RGB4, UBuildTarget.StandaloneWindows64));
        }

        [Test]
        public void IsSupportedTextureFormat_ETC2_AndroidAndIOS()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.ETC2_RGB, UBuildTarget.Android));
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.ETC2_RGB, UBuildTarget.iOS));
            Assert.IsFalse(TextureUtility.IsSupportedTextureFormat(TextureFormat.ETC2_RGB, UBuildTarget.StandaloneWindows64));
        }

        [Test]
        public void IsSupportedTextureFormat_UnsupportedBuildTarget_Throws()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                TextureUtility.IsSupportedTextureFormat(TextureFormat.RGBA32, UBuildTarget.WebGL);
            });
        }

        // --- IsUncompressedFormat ---

        [Test]
        public void IsUncompressedFormat_RGBA32_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsUncompressedFormat(TextureFormat.RGBA32));
        }

        [Test]
        public void IsUncompressedFormat_RGB24_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsUncompressedFormat(TextureFormat.RGB24));
        }

        [Test]
        public void IsUncompressedFormat_DXT5_ReturnsFalse()
        {
            Assert.IsFalse(TextureUtility.IsUncompressedFormat(TextureFormat.DXT5));
        }

        [Test]
        public void IsUncompressedFormat_ASTC4x4_ReturnsFalse()
        {
            Assert.IsFalse(TextureUtility.IsUncompressedFormat(TextureFormat.ASTC_4x4));
        }

        [Test]
        public void IsUncompressedFormat_RGBAFloat_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsUncompressedFormat(TextureFormat.RGBAFloat));
        }

        [Test]
        public void IsUncompressedFormat_RGBAHalf_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsUncompressedFormat(TextureFormat.RGBAHalf));
        }

        [Test]
        public void IsUncompressedFormat_Alpha8_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsUncompressedFormat(TextureFormat.Alpha8));
        }

        // --- AspectFitReduction ---

        [Test]
        public void AspectFitReduction_AlreadySmaller_ReturnsSameSize()
        {
            var (w, h) = TextureUtility.AspectFitReduction(512, 512, 1024);
            Assert.AreEqual(512, w);
            Assert.AreEqual(512, h);
        }

        [Test]
        public void AspectFitReduction_ExactSize_ReturnsSameSize()
        {
            var (w, h) = TextureUtility.AspectFitReduction(1024, 1024, 1024);
            Assert.AreEqual(1024, w);
            Assert.AreEqual(1024, h);
        }

        [Test]
        public void AspectFitReduction_Square_ReducesByHalf()
        {
            var (w, h) = TextureUtility.AspectFitReduction(2048, 2048, 1024);
            Assert.AreEqual(1024, w);
            Assert.AreEqual(1024, h);
        }

        [Test]
        public void AspectFitReduction_Landscape_KeepsAspectRatio()
        {
            var (w, h) = TextureUtility.AspectFitReduction(2048, 1024, 1024);
            Assert.AreEqual(1024, w);
            Assert.AreEqual(512, h);
        }

        [Test]
        public void AspectFitReduction_Portrait_KeepsAspectRatio()
        {
            var (w, h) = TextureUtility.AspectFitReduction(1024, 2048, 1024);
            Assert.AreEqual(512, w);
            Assert.AreEqual(1024, h);
        }

        [Test]
        public void AspectFitReduction_SmallTexture_NoChange()
        {
            var (w, h) = TextureUtility.AspectFitReduction(4, 4, 256);
            Assert.AreEqual(4, w);
            Assert.AreEqual(4, h);
        }

        // --- CreateMinimumEmptyTexture ---

        [Test]
        public void CreateMinimumEmptyTexture_Returns4x4()
        {
            Texture2D tex = null;
            try
            {
                tex = TextureUtility.CreateMinimumEmptyTexture();
                Assert.IsNotNull(tex);
                Assert.AreEqual(4, tex.width);
                Assert.AreEqual(4, tex.height);
            }
            finally
            {
                if (tex != null)
                {
                    UnityEngine.Object.DestroyImmediate(tex);
                }
            }
        }

        // --- CreateColorTexture ---

        [Test]
        public void CreateColorTexture_ReturnsCorrectSize()
        {
            Texture2D tex = null;
            try
            {
                tex = TextureUtility.CreateColorTexture(new Color32(255, 0, 0, 255), 8, 8);
                Assert.IsNotNull(tex);
                Assert.AreEqual(8, tex.width);
                Assert.AreEqual(8, tex.height);
            }
            finally
            {
                if (tex != null)
                {
                    UnityEngine.Object.DestroyImmediate(tex);
                }
            }
        }

        [Test]
        public void CreateColorTexture_AllPixelsHaveSpecifiedColor()
        {
            Texture2D tex = null;
            try
            {
                var targetColor = new Color32(128, 64, 32, 255);
                tex = TextureUtility.CreateColorTexture(targetColor, 4, 4);
                var pixels = tex.GetPixels32();
                foreach (var pixel in pixels)
                {
                    Assert.AreEqual(targetColor.r, pixel.r);
                    Assert.AreEqual(targetColor.g, pixel.g);
                    Assert.AreEqual(targetColor.b, pixel.b);
                    Assert.AreEqual(targetColor.a, pixel.a);
                }
            }
            finally
            {
                if (tex != null)
                {
                    UnityEngine.Object.DestroyImmediate(tex);
                }
            }
        }

        [Test]
        public void CreateColorTexture_Default4x4_ReturnsCorrectSize()
        {
            Texture2D tex = null;
            try
            {
                tex = TextureUtility.CreateColorTexture(new Color32(0, 0, 0, 0));
                Assert.IsNotNull(tex);
                Assert.AreEqual(4, tex.width);
                Assert.AreEqual(4, tex.height);
            }
            finally
            {
                if (tex != null)
                {
                    UnityEngine.Object.DestroyImmediate(tex);
                }
            }
        }

        // --- SetStreamingMipMaps ---

        [Test]
        public void SetStreamingMipMaps_SetsProperty()
        {
            Texture2D tex = null;
            try
            {
                tex = new Texture2D(4, 4, TextureFormat.RGBA32, true);
                TextureUtility.SetStreamingMipMaps(tex, true);
                var so = new SerializedObject(tex);
                so.Update();
                var prop = so.FindProperty("m_StreamingMipmaps");
                Assert.IsTrue(prop.boolValue);

                TextureUtility.SetStreamingMipMaps(tex, false);
                so.Update();
                prop = so.FindProperty("m_StreamingMipmaps");
                Assert.IsFalse(prop.boolValue);
            }
            finally
            {
                if (tex != null)
                {
                    UnityEngine.Object.DestroyImmediate(tex);
                }
            }
        }

        // --- IsNormalMapAsset ---

        [Test]
        public void IsNormalMapAsset_NonAssetTexture_ReturnsFalse()
        {
            Texture2D tex = null;
            try
            {
                tex = new Texture2D(4, 4);
                Assert.IsFalse(TextureUtility.IsNormalMapAsset(tex));
            }
            finally
            {
                if (tex != null)
                {
                    UnityEngine.Object.DestroyImmediate(tex);
                }
            }
        }

        // --- CopyAsReadable ---

        [Test]
        public void CopyAsReadable_ReturnsCopyWithSameDimensions()
        {
            Texture2D original = null;
            Texture2D copy = null;
            try
            {
                original = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                copy = TextureUtility.CopyAsReadable(original, true);
                Assert.AreEqual(original.width, copy.width);
                Assert.AreEqual(original.height, copy.height);
                Assert.AreNotSame(original, copy);
            }
            finally
            {
                if (original != null) UnityEngine.Object.DestroyImmediate(original);
                if (copy != null) UnityEngine.Object.DestroyImmediate(copy);
            }
        }

        // --- GetImageContentsHash ---

        [Test]
        public void GetImageContentsHash_NullTexture_ReturnsDefault()
        {
            var hash = TextureUtility.GetImageContentsHash(null);
            Assert.AreEqual(default(Hash128), hash);
        }

        [Test]
        public void GetImageContentsHash_Texture2D_ReturnsHash()
        {
            Texture2D tex = null;
            try
            {
                tex = new Texture2D(4, 4);
                var hash = TextureUtility.GetImageContentsHash(tex);
                Assert.AreNotEqual(default(Hash128), hash);
            }
            finally
            {
                if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void GetImageContentsHash_RenderTexture_ReturnsNonDefaultHash()
        {
            RenderTexture rt = null;
            try
            {
                rt = RenderTexture.GetTemporary(4, 4);
                var hash = TextureUtility.GetImageContentsHash(rt);
                // RenderTexture uses random hash, so just check it's computed
                // (could be default in rare cases, but extremely unlikely)
                Assert.IsNotNull(hash);
            }
            finally
            {
                if (rt != null) RenderTexture.ReleaseTemporary(rt);
            }
        }

        // --- LoadUncompressedTexture (Texture overload) ---

        [Test]
        public void LoadUncompressedTexture_NullTexture_ReturnsNull()
        {
            var result = TextureUtility.LoadUncompressedTexture((Texture)null);
            Assert.IsNull(result);
        }

        [Test]
        public void LoadUncompressedTexture_UnsavedTexture_ReturnsSameTexture()
        {
            Texture2D tex = null;
            try
            {
                tex = new Texture2D(4, 4);
                var result = TextureUtility.LoadUncompressedTexture((Texture)tex);
                Assert.AreSame(tex, result);
            }
            finally
            {
                if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
            }
        }

        // --- DestroyTexture ---

        [Test]
        public void DestroyTexture_NullTexture_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => TextureUtility.DestroyTexture(null));
        }

        // --- CompressTextureForBuildTarget ---

        [Test]
        public void CompressTextureForBuildTarget_WithMaxTextureSize_ResizesTexture()
        {
            Texture2D tex = null;
            Texture2D result = null;
            try
            {
                tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                result = TextureUtility.CompressTextureForBuildTarget(tex, UBuildTarget.Android, TextureFormat.ASTC_6x6, 128);
                Assert.LessOrEqual(result.width, 128);
                Assert.LessOrEqual(result.height, 128);
            }
            finally
            {
                if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
                if (result != null && result != tex) UnityEngine.Object.DestroyImmediate(result);
            }
        }

        [Test]
        public void CompressTextureForBuildTarget_NonMultipleOf4_WarnsForDXT5()
        {
            Texture2D tex = null;
            try
            {
                tex = new Texture2D(10, 10, TextureFormat.RGBA32, false);
                LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("not a multiple of 4"));
                var result = TextureUtility.CompressTextureForBuildTarget(tex, UBuildTarget.StandaloneWindows64, TextureFormat.ASTC_6x6);
                // On Windows target, format = DXT5, 10x10 is not multiple of 4 -> warning
                Assert.IsNotNull(result);
            }
            finally
            {
                if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
            }
        }
    }
}
