// Tests for TextureUtility private ASTC methods via reflection.

using System;
using System.Reflection;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    internal class TextureUtilityASTCPrivateMethodTests
    {
        private static readonly Type TexUtilType = typeof(TextureUtility);

        private static int GetASTCQualityScore(TextureFormat format)
        {
            var method = TexUtilType.GetMethod("GetASTCQualityScore", BindingFlags.NonPublic | BindingFlags.Static);
            return (int)method.Invoke(null, new object[] { format });
        }

        private static TextureFormat GetBetterASTCFormat(TextureFormat? current, TextureFormat candidate)
        {
            var method = TexUtilType.GetMethod("GetBetterASTCFormat", BindingFlags.NonPublic | BindingFlags.Static);
            return (TextureFormat)method.Invoke(null, new object[] { current, candidate });
        }

        private static TextureFormat? GetMobileTextureFormatFromImporterFormat(TextureImporterFormat importerFormat)
        {
            var method = TexUtilType.GetMethod("GetMobileTextureFormatFromImporterFormat", BindingFlags.NonPublic | BindingFlags.Static);
            return (TextureFormat?)method.Invoke(null, new object[] { importerFormat });
        }

        // --- GetASTCQualityScore ---

        [Test]
        public void GetASTCQualityScore_ASTC4x4_Returns16()
        {
            Assert.AreEqual(16, GetASTCQualityScore(TextureFormat.ASTC_4x4));
        }

        [Test]
        public void GetASTCQualityScore_ASTC5x5_Returns25()
        {
            Assert.AreEqual(25, GetASTCQualityScore(TextureFormat.ASTC_5x5));
        }

        [Test]
        public void GetASTCQualityScore_ASTC6x6_Returns36()
        {
            Assert.AreEqual(36, GetASTCQualityScore(TextureFormat.ASTC_6x6));
        }

        [Test]
        public void GetASTCQualityScore_ASTC8x8_Returns64()
        {
            Assert.AreEqual(64, GetASTCQualityScore(TextureFormat.ASTC_8x8));
        }

        [Test]
        public void GetASTCQualityScore_ASTC10x10_Returns100()
        {
            Assert.AreEqual(100, GetASTCQualityScore(TextureFormat.ASTC_10x10));
        }

        [Test]
        public void GetASTCQualityScore_ASTC12x12_Returns144()
        {
            Assert.AreEqual(144, GetASTCQualityScore(TextureFormat.ASTC_12x12));
        }

        [Test]
        public void GetASTCQualityScore_NonASTCFormat_ReturnsMaxValue()
        {
            Assert.AreEqual(int.MaxValue, GetASTCQualityScore(TextureFormat.DXT1));
        }

        [Test]
        public void GetASTCQualityScore_ScoresInAscendingOrder()
        {
            var formats = new[]
            {
                TextureFormat.ASTC_4x4, TextureFormat.ASTC_5x5,
                TextureFormat.ASTC_6x6, TextureFormat.ASTC_8x8,
                TextureFormat.ASTC_10x10, TextureFormat.ASTC_12x12,
            };
            for (int i = 0; i < formats.Length - 1; i++)
            {
                Assert.Less(GetASTCQualityScore(formats[i]), GetASTCQualityScore(formats[i + 1]),
                    $"{formats[i]} should have lower score than {formats[i + 1]}");
            }
        }

        // --- GetBetterASTCFormat ---

        [Test]
        public void GetBetterASTCFormat_NullCurrent_ReturnsCandidate()
        {
            Assert.AreEqual(TextureFormat.ASTC_8x8, GetBetterASTCFormat(null, TextureFormat.ASTC_8x8));
        }

        [Test]
        public void GetBetterASTCFormat_CandidateHigherQuality_ReturnsCandidate()
        {
            Assert.AreEqual(TextureFormat.ASTC_4x4, GetBetterASTCFormat(TextureFormat.ASTC_8x8, TextureFormat.ASTC_4x4));
        }

        [Test]
        public void GetBetterASTCFormat_CurrentHigherQuality_ReturnsCurrent()
        {
            Assert.AreEqual(TextureFormat.ASTC_4x4, GetBetterASTCFormat(TextureFormat.ASTC_4x4, TextureFormat.ASTC_12x12));
        }

        [Test]
        public void GetBetterASTCFormat_SameFormat_ReturnsSame()
        {
            Assert.AreEqual(TextureFormat.ASTC_6x6, GetBetterASTCFormat(TextureFormat.ASTC_6x6, TextureFormat.ASTC_6x6));
        }

        [Test]
        public void GetBetterASTCFormat_5x5vs10x10_Returns5x5()
        {
            Assert.AreEqual(TextureFormat.ASTC_5x5, GetBetterASTCFormat(TextureFormat.ASTC_5x5, TextureFormat.ASTC_10x10));
        }

        [Test]
        public void GetBetterASTCFormat_12x12vs6x6_Returns6x6()
        {
            Assert.AreEqual(TextureFormat.ASTC_6x6, GetBetterASTCFormat(TextureFormat.ASTC_12x12, TextureFormat.ASTC_6x6));
        }

        // --- GetMobileTextureFormatFromImporterFormat ---

        [Test]
        public void GetMobileTextureFormatFromImporterFormat_ASTC4x4()
        {
            Assert.AreEqual(TextureFormat.ASTC_4x4, GetMobileTextureFormatFromImporterFormat(TextureImporterFormat.ASTC_4x4));
        }

        [Test]
        public void GetMobileTextureFormatFromImporterFormat_ASTC5x5()
        {
            Assert.AreEqual(TextureFormat.ASTC_5x5, GetMobileTextureFormatFromImporterFormat(TextureImporterFormat.ASTC_5x5));
        }

        [Test]
        public void GetMobileTextureFormatFromImporterFormat_ASTC6x6()
        {
            Assert.AreEqual(TextureFormat.ASTC_6x6, GetMobileTextureFormatFromImporterFormat(TextureImporterFormat.ASTC_6x6));
        }

        [Test]
        public void GetMobileTextureFormatFromImporterFormat_ASTC8x8()
        {
            Assert.AreEqual(TextureFormat.ASTC_8x8, GetMobileTextureFormatFromImporterFormat(TextureImporterFormat.ASTC_8x8));
        }

        [Test]
        public void GetMobileTextureFormatFromImporterFormat_ASTC10x10()
        {
            Assert.AreEqual(TextureFormat.ASTC_10x10, GetMobileTextureFormatFromImporterFormat(TextureImporterFormat.ASTC_10x10));
        }

        [Test]
        public void GetMobileTextureFormatFromImporterFormat_ASTC12x12()
        {
            Assert.AreEqual(TextureFormat.ASTC_12x12, GetMobileTextureFormatFromImporterFormat(TextureImporterFormat.ASTC_12x12));
        }

        [Test]
        public void GetMobileTextureFormatFromImporterFormat_HDR4x4_ReturnsNonHDR()
        {
            Assert.AreEqual(TextureFormat.ASTC_4x4, GetMobileTextureFormatFromImporterFormat(TextureImporterFormat.ASTC_HDR_4x4));
        }

        [Test]
        public void GetMobileTextureFormatFromImporterFormat_HDR5x5_ReturnsNonHDR()
        {
            Assert.AreEqual(TextureFormat.ASTC_5x5, GetMobileTextureFormatFromImporterFormat(TextureImporterFormat.ASTC_HDR_5x5));
        }

        [Test]
        public void GetMobileTextureFormatFromImporterFormat_HDR6x6_ReturnsNonHDR()
        {
            Assert.AreEqual(TextureFormat.ASTC_6x6, GetMobileTextureFormatFromImporterFormat(TextureImporterFormat.ASTC_HDR_6x6));
        }

        [Test]
        public void GetMobileTextureFormatFromImporterFormat_HDR8x8_ReturnsNonHDR()
        {
            Assert.AreEqual(TextureFormat.ASTC_8x8, GetMobileTextureFormatFromImporterFormat(TextureImporterFormat.ASTC_HDR_8x8));
        }

        [Test]
        public void GetMobileTextureFormatFromImporterFormat_HDR10x10_ReturnsNonHDR()
        {
            Assert.AreEqual(TextureFormat.ASTC_10x10, GetMobileTextureFormatFromImporterFormat(TextureImporterFormat.ASTC_HDR_10x10));
        }

        [Test]
        public void GetMobileTextureFormatFromImporterFormat_HDR12x12_ReturnsNonHDR()
        {
            Assert.AreEqual(TextureFormat.ASTC_12x12, GetMobileTextureFormatFromImporterFormat(TextureImporterFormat.ASTC_HDR_12x12));
        }

        [Test]
        public void GetMobileTextureFormatFromImporterFormat_DXT1_ReturnsNull()
        {
            Assert.IsNull(GetMobileTextureFormatFromImporterFormat(TextureImporterFormat.DXT1));
        }

        [Test]
        public void GetMobileTextureFormatFromImporterFormat_DXT5_ReturnsNull()
        {
            Assert.IsNull(GetMobileTextureFormatFromImporterFormat(TextureImporterFormat.DXT5));
        }

        [Test]
        public void GetMobileTextureFormatFromImporterFormat_Automatic_ReturnsNull()
        {
            Assert.IsNull(GetMobileTextureFormatFromImporterFormat(TextureImporterFormat.Automatic));
        }

        [Test]
        public void GetMobileTextureFormatFromImporterFormat_RGB24_ReturnsNull()
        {
            Assert.IsNull(GetMobileTextureFormatFromImporterFormat(TextureImporterFormat.RGB24));
        }
    }
}
