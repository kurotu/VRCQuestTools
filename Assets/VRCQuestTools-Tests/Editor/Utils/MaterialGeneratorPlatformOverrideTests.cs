// <copyright file="MaterialGeneratorPlatformOverrideTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.IO;
using KRT.VRCQuestTools.Models;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Tests for Material Generator platform override functionality.
    /// Tests that platform override compression formats are correctly applied when generating textures.
    /// </summary>
    public class MaterialGeneratorPlatformOverrideTests
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
        /// Test that generated texture applies platform override format when specified.
        /// </summary>
        [Test]
        public void GeneratedTexture_WithPlatformOverrideFormat_AppliesFormat()
        {
            // Create a test material
            var material = new Material(Shader.Find("Standard"));
            
            // Create a mock settings object
            var settings = CreateMockSettings();

            // Generate a texture with a specific override format
            var overrideFormat = TextureFormat.ASTC_4x4;
            Texture2D generatedTexture = null;
            
            MaterialGeneratorUtility.GenerateTexture(
                material,
                settings,
                "test",
                true,
                testTexturesPath,
                (completion) =>
                {
                    // Generate a simple red texture
                    var tex = new Texture2D(256, 256);
                    var pixels = new Color[256 * 256];
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        pixels[i] = Color.red;
                    }
                    tex.SetPixels(pixels);
                    tex.Apply();
                    completion?.Invoke(tex);
                    return new ResultRequest<Texture2D>(tex, completion);
                },
                (tex) => generatedTexture = tex,
                overrideFormat
            ).WaitForCompletion();

            Assert.IsNotNull(generatedTexture);

            // Check that the texture has the expected platform override settings
            var texturePath = AssetDatabase.GetAssetPath(generatedTexture);
            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            Assert.IsNotNull(importer);

            // Check Android platform settings
            var androidSettings = importer.GetPlatformTextureSettings("Android");
            Assert.IsTrue(androidSettings.overridden, "Android platform override should be enabled");
            Assert.AreEqual(TextureImporterFormat.ASTC_4x4, androidSettings.format, "Android should use ASTC_4x4 format");

            // Check iOS platform settings
            var iosSettings = importer.GetPlatformTextureSettings("iPhone");
            Assert.IsTrue(iosSettings.overridden, "iOS platform override should be enabled");
            Assert.AreEqual(TextureImporterFormat.ASTC_4x4, iosSettings.format, "iOS should use ASTC_4x4 format");
        }

        /// <summary>
        /// Test that generated texture uses settings format when no override is specified.
        /// </summary>
        [Test]
        public void GeneratedTexture_WithoutPlatformOverrideFormat_UsesSettingsFormat()
        {
            // Create a test material
            var material = new Material(Shader.Find("Standard"));
            
            // Create a mock settings object with ASTC_6x6
            var settings = CreateMockSettings(MobileTextureFormat.ASTC_6x6);

            Texture2D generatedTexture = null;
            
            MaterialGeneratorUtility.GenerateTexture(
                material,
                settings,
                "test_no_override",
                true,
                testTexturesPath,
                (completion) =>
                {
                    // Generate a simple blue texture
                    var tex = new Texture2D(256, 256);
                    var pixels = new Color[256 * 256];
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        pixels[i] = Color.blue;
                    }
                    tex.SetPixels(pixels);
                    tex.Apply();
                    completion?.Invoke(tex);
                    return new ResultRequest<Texture2D>(tex, completion);
                },
                (tex) => generatedTexture = tex,
                null  // No override format
            ).WaitForCompletion();

            Assert.IsNotNull(generatedTexture);

            // Check that the texture has platform override settings from the settings object
            var texturePath = AssetDatabase.GetAssetPath(generatedTexture);
            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            Assert.IsNotNull(importer);

            // Check Android platform settings
            var androidSettings = importer.GetPlatformTextureSettings("Android");
            Assert.IsTrue(androidSettings.overridden, "Android platform override should be enabled");
            Assert.AreEqual(TextureImporterFormat.ASTC_6x6, androidSettings.format, "Android should use ASTC_6x6 format from settings");

            // Check iOS platform settings
            var iosSettings = importer.GetPlatformTextureSettings("iPhone");
            Assert.IsTrue(iosSettings.overridden, "iOS platform override should be enabled");
            Assert.AreEqual(TextureImporterFormat.ASTC_6x6, iosSettings.format, "iOS should use ASTC_6x6 format from settings");
        }

        /// <summary>
        /// Test that override format takes precedence over settings format.
        /// </summary>
        [Test]
        public void GeneratedTexture_WithBothOverrideAndSettingsFormat_PrefersOverride()
        {
            // Create a test material
            var material = new Material(Shader.Find("Standard"));
            
            // Create a mock settings object with ASTC_12x12
            var settings = CreateMockSettings(MobileTextureFormat.ASTC_12x12);

            // But provide ASTC_4x4 as override (should take precedence)
            var overrideFormat = TextureFormat.ASTC_4x4;
            Texture2D generatedTexture = null;
            
            MaterialGeneratorUtility.GenerateTexture(
                material,
                settings,
                "test_override_precedence",
                true,
                testTexturesPath,
                (completion) =>
                {
                    // Generate a simple green texture
                    var tex = new Texture2D(256, 256);
                    var pixels = new Color[256 * 256];
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        pixels[i] = Color.green;
                    }
                    tex.SetPixels(pixels);
                    tex.Apply();
                    completion?.Invoke(tex);
                    return new ResultRequest<Texture2D>(tex, completion);
                },
                (tex) => generatedTexture = tex,
                overrideFormat
            ).WaitForCompletion();

            Assert.IsNotNull(generatedTexture);

            // Check that the texture uses the override format, not the settings format
            var texturePath = AssetDatabase.GetAssetPath(generatedTexture);
            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            Assert.IsNotNull(importer);

            // Check Android platform settings
            var androidSettings = importer.GetPlatformTextureSettings("Android");
            Assert.IsTrue(androidSettings.overridden, "Android platform override should be enabled");
            Assert.AreEqual(TextureImporterFormat.ASTC_4x4, androidSettings.format, "Should use override format ASTC_4x4, not settings format ASTC_12x12");

            // Check iOS platform settings
            var iosSettings = importer.GetPlatformTextureSettings("iPhone");
            Assert.IsTrue(iosSettings.overridden, "iOS platform override should be enabled");
            Assert.AreEqual(TextureImporterFormat.ASTC_4x4, iosSettings.format, "Should use override format ASTC_4x4, not settings format ASTC_12x12");
        }

        private IMaterialConvertSettings CreateMockSettings(MobileTextureFormat format = MobileTextureFormat.ASTC_6x6)
        {
            return new MockConvertSettings
            {
                MobileTextureFormat = format,
            };
        }

        // Mock settings class for testing
        private class MockConvertSettings : IMaterialConvertSettings
        {
            public MobileTextureFormat MobileTextureFormat { get; set; }

            public string GetCacheKey()
            {
                return "mock_cache_key";
            }
        }
    }
}
