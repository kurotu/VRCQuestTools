// Tests for MaterialGeneratorUtility
using System;
using KRT.VRCQuestTools.Models;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    internal class MaterialGeneratorUtilityTests
    {
        [Test]
        public void ConvertToNullableTextureFormat_ASTC6x6_ReturnsASTC6x6()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod(
                "ConvertToNullableTextureFormat",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(method, "ConvertToNullableTextureFormat method not found");

            var result = method.Invoke(null, new object[] { MobileTextureFormat.ASTC_6x6 });
            Assert.AreEqual(TextureFormat.ASTC_6x6, result);
        }

        [Test]
        public void ConvertToNullableTextureFormat_ASTC4x4_ReturnsASTC4x4()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod(
                "ConvertToNullableTextureFormat",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(method);

            var result = method.Invoke(null, new object[] { MobileTextureFormat.ASTC_4x4 });
            Assert.AreEqual(TextureFormat.ASTC_4x4, result);
        }

        [Test]
        public void ConvertToNullableTextureFormat_ASTC8x8_ReturnsASTC8x8()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod(
                "ConvertToNullableTextureFormat",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(method);

            var result = method.Invoke(null, new object[] { MobileTextureFormat.ASTC_8x8 });
            Assert.AreEqual(TextureFormat.ASTC_8x8, result);
        }

        [Test]
        public void ConvertToNullableTextureFormat_Default_ReturnsNull()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod(
                "ConvertToNullableTextureFormat",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(method);

            var result = method.Invoke(null, new object[] { MobileTextureFormat.NoOverride });
            Assert.IsNull(result);
        }

        [Test]
        public void TextureConfig_SRGB_HasCorrectValues()
        {
            var configType = typeof(MaterialGeneratorUtility).GetNestedType("TextureConfig",
                System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(configType, "TextureConfig type not found");

            var srgbProp = configType.GetProperty("SRGB",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(srgbProp, "SRGB property not found");

            var config = srgbProp.GetValue(null);
            var isSRGB = (bool)configType.GetField("isSRGB",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(config);
            var isNormalMap = (bool)configType.GetField("isNormalMap",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(config);
            var alphaIsTransparency = (bool)configType.GetField("alphaIsTransparency",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(config);

            Assert.IsTrue(isSRGB);
            Assert.IsFalse(isNormalMap);
            Assert.IsTrue(alphaIsTransparency);
        }

        [Test]
        public void TextureConfig_Parameter_HasCorrectValues()
        {
            var configType = typeof(MaterialGeneratorUtility).GetNestedType("TextureConfig",
                System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(configType);

            var paramProp = configType.GetProperty("Parameter",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(paramProp);

            var config = paramProp.GetValue(null);
            var isSRGB = (bool)configType.GetField("isSRGB",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(config);
            var isNormalMap = (bool)configType.GetField("isNormalMap",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(config);
            var alphaIsTransparency = (bool)configType.GetField("alphaIsTransparency",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(config);

            Assert.IsFalse(isSRGB);
            Assert.IsFalse(isNormalMap);
            Assert.IsFalse(alphaIsTransparency);
        }

        [Test]
        public void TextureConfig_NormalMap_HasCorrectValues()
        {
            var configType = typeof(MaterialGeneratorUtility).GetNestedType("TextureConfig",
                System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(configType);

            var normalProp = configType.GetProperty("NormalMap",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(normalProp);

            var config = normalProp.GetValue(null);
            var isSRGB = (bool)configType.GetField("isSRGB",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(config);
            var isNormalMap = (bool)configType.GetField("isNormalMap",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(config);
            var alphaIsTransparency = (bool)configType.GetField("alphaIsTransparency",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(config);

            Assert.IsFalse(isSRGB);
            Assert.IsTrue(isNormalMap);
            Assert.IsFalse(alphaIsTransparency);
        }
    }
}
