// <copyright file="TextureUtilityPlatformOverrideTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Tests for TextureUtility platform override functionality.
    /// </summary>
    public class TextureUtilityPlatformOverrideTests
    {
        private string testTexturesPath;

        /// <summary>
        /// Setup test environment.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            testTexturesPath = "Assets/VRCQuestTools-Tests/Temp";
            if (!Directory.Exists(testTexturesPath))
            {
                Directory.CreateDirectory(testTexturesPath);
            }
        }

        /// <summary>
        /// Cleanup test environment.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(testTexturesPath))
            {
                AssetDatabase.DeleteAsset(testTexturesPath);
            }
        }

        /// <summary>
        /// Test that GetPlatformOverrideSettings returns null when no textures are provided.
        /// </summary>
        [Test]
        public void GetPlatformOverrideSettings_WithNoTextures_ReturnsNull()
        {
            var result = TextureUtility.GetPlatformOverrideSettings();
            Assert.IsNull(result);
        }

        /// <summary>
        /// Test that GetPlatformOverrideSettings returns null when textures have no overrides.
        /// </summary>
        [Test]
        public void GetPlatformOverrideSettings_WithNoOverrides_ReturnsNull()
        {
            // Create a test texture without platform overrides
            // Format doesn't matter since hasOverride=false, but using valid TextureFormat for consistency
            var texture = CreateTestTexture("test_no_override.png", 512, 512, false, 0, TextureFormat.ASTC_6x6);

            var result = TextureUtility.GetPlatformOverrideSettings(texture);
            Assert.IsNull(result);
        }

        /// <summary>
        /// Test that GetPlatformOverrideSettings returns Android override settings.
        /// </summary>
        [Test]
        public void GetPlatformOverrideSettings_WithAndroidOverride_ReturnsCorrectSettings()
        {
            // Create a test texture with Android override
            var texture = CreateTestTexture("test_android_override.png", 512, 512, true, 256, TextureFormat.ASTC_6x6);

            var result = TextureUtility.GetPlatformOverrideSettings(texture);

            Assert.IsNotNull(result);
            Assert.AreEqual(256, result.Value.MaxTextureSize);
            Assert.AreEqual(TextureFormat.ASTC_6x6, result.Value.Format);
        }

        /// <summary>
        /// Test that GetPlatformOverrideSettings selects maximum resolution from multiple textures.
        /// </summary>
        [Test]
        public void GetPlatformOverrideSettings_WithMultipleTextures_SelectsMaxResolution()
        {
            // Create textures with different resolutions
            var texture1 = CreateTestTexture("test_256.png", 512, 512, true, 256, TextureFormat.ASTC_6x6);
            var texture2 = CreateTestTexture("test_512.png", 1024, 1024, true, 512, TextureFormat.ASTC_6x6);
            var texture3 = CreateTestTexture("test_1024.png", 2048, 2048, true, 1024, TextureFormat.ASTC_6x6);

            var result = TextureUtility.GetPlatformOverrideSettings(texture1, texture2, texture3);

            Assert.IsNotNull(result);
            Assert.AreEqual(1024, result.Value.MaxTextureSize);
        }

        /// <summary>
        /// Test that GetPlatformOverrideSettings selects highest quality ASTC format.
        /// </summary>
        [Test]
        public void GetPlatformOverrideSettings_WithDifferentFormats_SelectsHighestQuality()
        {
            // Create textures with different ASTC formats (4x4 is highest quality)
            var texture1 = CreateTestTexture("test_12x12.png", 512, 512, true, 512, TextureFormat.ASTC_12x12);
            var texture2 = CreateTestTexture("test_6x6.png", 512, 512, true, 512, TextureFormat.ASTC_6x6);
            var texture3 = CreateTestTexture("test_4x4.png", 512, 512, true, 512, TextureFormat.ASTC_4x4);

            var result = TextureUtility.GetPlatformOverrideSettings(texture1, texture2, texture3);

            Assert.IsNotNull(result);
            Assert.AreEqual(TextureFormat.ASTC_4x4, result.Value.Format);
        }

        /// <summary>
        /// Test that GetPlatformOverrideSettings handles mixed override and non-override textures.
        /// </summary>
        [Test]
        public void GetPlatformOverrideSettings_WithMixedTextures_UsesOnlyOverrides()
        {
            // Create one texture with override and one without
            var textureWithOverride = CreateTestTexture("test_with_override.png", 512, 512, true, 256, TextureFormat.ASTC_6x6);
            var textureWithoutOverride = CreateTestTexture("test_without_override.png", 1024, 1024, false, 0, TextureFormat.ASTC_6x6);

            var result = TextureUtility.GetPlatformOverrideSettings(textureWithOverride, textureWithoutOverride);

            Assert.IsNotNull(result);
            Assert.AreEqual(256, result.Value.MaxTextureSize);
            Assert.AreEqual(TextureFormat.ASTC_6x6, result.Value.Format);
        }

        /// <summary>
        /// Test that GetPlatformOverrideSettings handles null textures in array.
        /// </summary>
        [Test]
        public void GetPlatformOverrideSettings_WithNullTextures_IgnoresNulls()
        {
            var texture = CreateTestTexture("test_with_null.png", 512, 512, true, 256, TextureFormat.ASTC_6x6);

            var result = TextureUtility.GetPlatformOverrideSettings(null, texture, null);

            Assert.IsNotNull(result);
            Assert.AreEqual(256, result.Value.MaxTextureSize);
            Assert.AreEqual(TextureFormat.ASTC_6x6, result.Value.Format);
        }

        /// <summary>
        /// Test that GetPlatformOverrideSettings works with iOS overrides.
        /// </summary>
        [Test]
        public void GetPlatformOverrideSettings_WithIosOverride_ReturnsCorrectSettings()
        {
            // Create a test texture with iOS override
            var texture = CreateTestTexture("test_ios_override.png", 512, 512, true, 512, TextureFormat.ASTC_8x8, platformName: "iPhone");

            var result = TextureUtility.GetPlatformOverrideSettings(texture);

            Assert.IsNotNull(result);
            Assert.AreEqual(512, result.Value.MaxTextureSize);
            Assert.AreEqual(TextureFormat.ASTC_8x8, result.Value.Format);
        }

        /// <summary>
        /// Test that GetPlatformOverrideSettings combines Android and iOS overrides correctly.
        /// </summary>
        [Test]
        public void GetPlatformOverrideSettings_WithBothPlatforms_CombinesCorrectly()
        {
            // Create texture with Android override (256, ASTC_6x6)
            var textureAndroid = CreateTestTexture("test_android.png", 512, 512, true, 256, TextureFormat.ASTC_6x6, platformName: "Android");

            // Create texture with iOS override (512, ASTC_4x4)
            var textureIos = CreateTestTexture("test_ios.png", 512, 512, true, 512, TextureFormat.ASTC_4x4, platformName: "iPhone");

            var result = TextureUtility.GetPlatformOverrideSettings(textureAndroid, textureIos);

            Assert.IsNotNull(result);

            // Should select max resolution (512 from iOS) and best format (ASTC_4x4 from iOS)
            Assert.AreEqual(512, result.Value.MaxTextureSize);
            Assert.AreEqual(TextureFormat.ASTC_4x4, result.Value.Format);
        }

        private Texture2D CreateTestTexture(string name, int width, int height, bool hasOverride, int overrideMaxSize, TextureFormat overrideFormat, string platformName = "Android")
        {
            // Create a simple texture
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(255, 0, 0, 255);
            }
            texture.SetPixels32(pixels);
            texture.Apply();

            // Save as PNG
            var path = Path.Combine(testTexturesPath, name);
            var bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
            Object.DestroyImmediate(texture);

            // Configure platform override if needed
            if (hasOverride)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Assert.IsNotNull(importer);

                var importerFormat = ConvertToImporterFormat(overrideFormat);
                Assert.IsNotNull(importerFormat, $"Unsupported format for testing: {overrideFormat}");

                var platformSettings = new TextureImporterPlatformSettings
                {
                    name = platformName,
                    overridden = true,
                    maxTextureSize = overrideMaxSize,
                    format = importerFormat.Value,
                };
                importer.SetPlatformTextureSettings(platformSettings);
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

#pragma warning disable SA1204 // Static elements should appear before instance elements
        private static TextureImporterFormat? ConvertToImporterFormat(TextureFormat textureFormat)
#pragma warning restore SA1204 // Static elements should appear before instance elements
        {
            // Convert TextureFormat to TextureImporterFormat for ASTC formats
            switch (textureFormat)
            {
                case TextureFormat.ASTC_4x4:
                    return TextureImporterFormat.ASTC_4x4;
                case TextureFormat.ASTC_5x5:
                    return TextureImporterFormat.ASTC_5x5;
                case TextureFormat.ASTC_6x6:
                    return TextureImporterFormat.ASTC_6x6;
                case TextureFormat.ASTC_8x8:
                    return TextureImporterFormat.ASTC_8x8;
                case TextureFormat.ASTC_10x10:
                    return TextureImporterFormat.ASTC_10x10;
                case TextureFormat.ASTC_12x12:
                    return TextureImporterFormat.ASTC_12x12;
                default:
                    return null;
            }
        }
    }
}
