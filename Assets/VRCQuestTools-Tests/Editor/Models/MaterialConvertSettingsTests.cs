// <copyright file="MaterialConvertSettingsTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for material convert settings classes.
    /// </summary>
    public class MaterialConvertSettingsTests
    {
        // --- ToonStandardConvertSettings ---

        /// <summary>
        /// Test ToonStandardConvertSettings default values.
        /// </summary>
        [Test]
        public void ToonStandardConvertSettings_DefaultValues()
        {
            var settings = new ToonStandardConvertSettings();
            Assert.IsTrue(settings.generateQuestTextures);
            Assert.AreEqual(TextureSizeLimit.Max1024x1024, settings.maxTextureSize);
            Assert.AreEqual(MobileTextureFormat.NoOverride, settings.mobileTextureFormat);
            Assert.IsTrue(settings.generateShadowRamp);
            Assert.IsTrue(settings.useNormalMap);
            Assert.IsTrue(settings.useEmission);
            Assert.IsTrue(settings.useOcclusion);
            Assert.IsTrue(settings.useSpecular);
            Assert.IsTrue(settings.useMatcap);
            Assert.IsTrue(settings.useRimLighting);
        }

        /// <summary>
        /// Test SetAllFeatures enables all features.
        /// </summary>
        [Test]
        public void ToonStandardConvertSettings_SetAllFeatures_True()
        {
            var settings = new ToonStandardConvertSettings();
            settings.SetAllFeatures(false);
            settings.SetAllFeatures(true);
            Assert.IsTrue(settings.useNormalMap);
            Assert.IsTrue(settings.useEmission);
            Assert.IsTrue(settings.useOcclusion);
            Assert.IsTrue(settings.useSpecular);
            Assert.IsTrue(settings.useMatcap);
            Assert.IsTrue(settings.useRimLighting);
        }

        /// <summary>
        /// Test SetAllFeatures disables all features.
        /// </summary>
        [Test]
        public void ToonStandardConvertSettings_SetAllFeatures_False()
        {
            var settings = new ToonStandardConvertSettings();
            settings.SetAllFeatures(false);
            Assert.IsFalse(settings.useNormalMap);
            Assert.IsFalse(settings.useEmission);
            Assert.IsFalse(settings.useOcclusion);
            Assert.IsFalse(settings.useSpecular);
            Assert.IsFalse(settings.useMatcap);
            Assert.IsFalse(settings.useRimLighting);
        }

        /// <summary>
        /// Test SimpleFeatures has all features disabled.
        /// </summary>
        [Test]
        public void ToonStandardConvertSettings_SimpleFeatures()
        {
            var settings = ToonStandardConvertSettings.SimpleFeatures;
            Assert.IsTrue(settings.generateShadowRamp);
            Assert.IsFalse(settings.useNormalMap);
            Assert.IsFalse(settings.useEmission);
            Assert.IsFalse(settings.useOcclusion);
            Assert.IsFalse(settings.useSpecular);
            Assert.IsFalse(settings.useMatcap);
            Assert.IsFalse(settings.useRimLighting);
        }

        /// <summary>
        /// Test MobileTextureFormat property.
        /// </summary>
        [Test]
        public void ToonStandardConvertSettings_MobileTextureFormat_ReturnsFieldValue()
        {
            var settings = new ToonStandardConvertSettings();
            settings.mobileTextureFormat = MobileTextureFormat.ASTC_4x4;
            Assert.AreEqual(MobileTextureFormat.ASTC_4x4, settings.MobileTextureFormat);
        }

        /// <summary>
        /// Test GetCacheKey returns non-empty string.
        /// </summary>
        [Test]
        public void ToonStandardConvertSettings_GetCacheKey_ReturnsNonEmpty()
        {
            var settings = new ToonStandardConvertSettings();
            var key = settings.GetCacheKey();
            Assert.IsNotEmpty(key);
            Assert.IsTrue(key.StartsWith("{"));
            Assert.IsTrue(key.Contains("generateQuestTextures"));
        }

        /// <summary>
        /// Test GetCacheKey returns different keys for different settings.
        /// </summary>
        [Test]
        public void ToonStandardConvertSettings_GetCacheKey_DifferentForDifferentSettings()
        {
            var settings1 = new ToonStandardConvertSettings();
            var settings2 = new ToonStandardConvertSettings();
            settings2.SetAllFeatures(false);

            Assert.AreNotEqual(settings1.GetCacheKey(), settings2.GetCacheKey());
        }

        /// <summary>
        /// Test LoadDefaultAssets sets features to false and loads fallback shadow ramp.
        /// </summary>
        [Test]
        public void ToonStandardConvertSettings_LoadDefaultAssets()
        {
            var settings = new ToonStandardConvertSettings();
            settings.LoadDefaultAssets();
            Assert.IsFalse(settings.useNormalMap);
            Assert.IsFalse(settings.useEmission);
            Assert.IsFalse(settings.useOcclusion);
            Assert.IsFalse(settings.useSpecular);
            Assert.IsFalse(settings.useMatcap);
            Assert.IsFalse(settings.useRimLighting);
        }

        // --- MatCapLitConvertSettings ---

        /// <summary>
        /// Test MatCapLitConvertSettings default values.
        /// </summary>
        [Test]
        public void MatCapLitConvertSettings_DefaultValues()
        {
            var settings = new MatCapLitConvertSettings();
            Assert.IsTrue(settings.generateQuestTextures);
            Assert.AreEqual(TextureSizeLimit.Max1024x1024, settings.maxTextureSize);
            Assert.AreEqual(MobileTextureFormat.NoOverride, settings.mobileTextureFormat);
            Assert.AreEqual(0.83f, settings.mainTextureBrightness, 0.001f);
            Assert.IsTrue(settings.generateShadowFromNormalMap);
        }

        /// <summary>
        /// Test MatCapLitConvertSettings properties mirror fields.
        /// </summary>
        [Test]
        public void MatCapLitConvertSettings_Properties_MirrorFields()
        {
            var settings = new MatCapLitConvertSettings();
            settings.generateQuestTextures = false;
            settings.maxTextureSize = TextureSizeLimit.Max256x256;
            settings.mobileTextureFormat = MobileTextureFormat.ASTC_8x8;
            settings.mainTextureBrightness = 0.5f;
            settings.generateShadowFromNormalMap = false;

            Assert.IsFalse(settings.GenerateQuestTextures);
            Assert.AreEqual(TextureSizeLimit.Max256x256, settings.MaxTextureSize);
            Assert.AreEqual(MobileTextureFormat.ASTC_8x8, settings.MobileTextureFormat);
            Assert.AreEqual(0.5f, settings.MainTextureBrightness, 0.001f);
            Assert.IsFalse(settings.GenerateShadowFromNormalMap);
        }

        /// <summary>
        /// Test MatCapLitConvertSettings GetCacheKey returns non-empty string.
        /// </summary>
        [Test]
        public void MatCapLitConvertSettings_GetCacheKey_ReturnsNonEmpty()
        {
            var settings = new MatCapLitConvertSettings();
            var key = settings.GetCacheKey();
            Assert.IsNotEmpty(key);
            Assert.IsTrue(key.StartsWith("{"));
        }

        /// <summary>
        /// Test MatCapLitConvertSettings GetCacheKey varies with settings.
        /// </summary>
        [Test]
        public void MatCapLitConvertSettings_GetCacheKey_DifferentForDifferentSettings()
        {
            var settings1 = new MatCapLitConvertSettings();
            var settings2 = new MatCapLitConvertSettings();
            settings2.mainTextureBrightness = 0.1f;

            Assert.AreNotEqual(settings1.GetCacheKey(), settings2.GetCacheKey());
        }

        /// <summary>
        /// Test MatCapLitConvertSettings LoadDefaultAssets does not throw.
        /// </summary>
        [Test]
        public void MatCapLitConvertSettings_LoadDefaultAssets_DoesNotThrow()
        {
            var settings = new MatCapLitConvertSettings();
            Assert.DoesNotThrow(() => settings.LoadDefaultAssets());
        }

        // --- ToonLitConvertSettings ---

        /// <summary>
        /// Test ToonLitConvertSettings default values.
        /// </summary>
        [Test]
        public void ToonLitConvertSettings_DefaultValues()
        {
            var settings = new ToonLitConvertSettings();
            Assert.IsTrue(settings.generateQuestTextures);
            Assert.AreEqual(TextureSizeLimit.Max1024x1024, settings.maxTextureSize);
            Assert.AreEqual(MobileTextureFormat.NoOverride, settings.mobileTextureFormat);
            Assert.AreEqual(0.83f, settings.mainTextureBrightness, 0.001f);
            Assert.IsTrue(settings.generateShadowFromNormalMap);
        }

        /// <summary>
        /// Test ToonLitConvertSettings GetCacheKey returns JSON.
        /// </summary>
        [Test]
        public void ToonLitConvertSettings_GetCacheKey_ReturnsJson()
        {
            var settings = new ToonLitConvertSettings();
            var key = settings.GetCacheKey();
            Assert.IsNotEmpty(key);
            Assert.IsTrue(key.Contains("generateQuestTextures"));
        }

        /// <summary>
        /// Test ToonLitConvertSettings LoadDefaultAssets does not throw.
        /// </summary>
        [Test]
        public void ToonLitConvertSettings_LoadDefaultAssets_DoesNotThrow()
        {
            var settings = new ToonLitConvertSettings();
            Assert.DoesNotThrow(() => settings.LoadDefaultAssets());
        }

        // --- MaterialReplaceSettings ---

        /// <summary>
        /// Test MaterialReplaceSettings MobileTextureFormat throws.
        /// </summary>
        [Test]
        public void MaterialReplaceSettings_MobileTextureFormat_Throws()
        {
            var settings = new MaterialReplaceSettings();
            Assert.Throws<System.InvalidProgramException>(() =>
            {
                var _ = settings.MobileTextureFormat;
            });
        }

        /// <summary>
        /// Test MaterialReplaceSettings GetCacheKey throws.
        /// </summary>
        [Test]
        public void MaterialReplaceSettings_GetCacheKey_Throws()
        {
            var settings = new MaterialReplaceSettings();
            Assert.Throws<System.InvalidProgramException>(() => settings.GetCacheKey());
        }

        /// <summary>
        /// Test MaterialReplaceSettings LoadDefaultAssets does not throw.
        /// </summary>
        [Test]
        public void MaterialReplaceSettings_LoadDefaultAssets_DoesNotThrow()
        {
            var settings = new MaterialReplaceSettings();
            Assert.DoesNotThrow(() => settings.LoadDefaultAssets());
        }

        /// <summary>
        /// Test MaterialReplaceSettings material field is null by default.
        /// </summary>
        [Test]
        public void MaterialReplaceSettings_DefaultMaterialIsNull()
        {
            var settings = new MaterialReplaceSettings();
            Assert.IsNull(settings.material);
        }
    }
}
