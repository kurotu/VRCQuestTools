// <copyright file="PlatformOverrideIntegrationTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using KRT.VRCQuestTools.Models;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Integration tests for platform override functionality.
    /// </summary>
    public class PlatformOverrideIntegrationTests
    {
        /// <summary>
        /// Test that the full integration works: texture override utility resolves format and falls back properly.
        /// </summary>
        [Test]
        public void Integration_TextureOverrideUtility_ResolvesFallbackCorrectly()
        {
            // Arrange
            var fallbackSettings = new TextureOverrideUtility.FallbackTextureSettings
            {
                mobileTextureFormat = MobileTextureFormat.ASTC_6x6,
                maxTextureSize = 1024
            };
            
            // Test with empty texture list (no overrides available)
            var emptyTextures = new List<Texture>();

            // Act
            var result = TextureOverrideUtility.ResolveTextureFormat(emptyTextures, UnityEditor.BuildTarget.Android, fallbackSettings);

            // Assert
            Assert.IsFalse(result.fromOverride, "Should indicate result came from fallback, not override");
            Assert.AreEqual(MobileTextureFormat.ASTC_6x6, result.mobileTextureFormat, "Should use fallback texture format");
            Assert.AreEqual(1024, result.maxTextureSize, "Should use fallback texture size");
        }

        /// <summary>
        /// Test that the texture extraction from MaterialGeneratorUtility works correctly.
        /// </summary>
        [Test]
        public void Integration_ExtractSourceTextures_WorksWithTestMaterial()
        {
            // Arrange
            var shader = Shader.Find("Standard");
            var material = new Material(shader);
            var testTexture = TestUtils.LoadFixture<Texture2D>("Textures/albedo_1024px_png.png");
            material.mainTexture = testTexture;

            // Act - Use reflection to access private method for testing
            var method = typeof(MaterialGeneratorUtility).GetMethod("ExtractSourceTextures", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var result = (IEnumerable<Texture>)method.Invoke(null, new object[] { material });

            // Assert
            var textureList = new List<Texture>(result);
            Assert.Greater(textureList.Count, 0, "Should extract at least one texture from material");
            Assert.Contains(testTexture, textureList, "Should contain the main texture that was set");
        }

        /// <summary>
        /// Test that the quality prioritization logic works as expected for common formats.
        /// </summary>
        [Test]
        public void Integration_QualityPrioritization_WorksForCommonFormats()
        {
            // Arrange - Test the format ordering
            var qualityOrder = new[]
            {
                MobileTextureFormat.ASTC_4x4,   // Highest quality
                MobileTextureFormat.ASTC_5x5,
                MobileTextureFormat.ASTC_6x6,   // Default Unity format
                MobileTextureFormat.ASTC_8x8,
                MobileTextureFormat.ASTC_10x10,
                MobileTextureFormat.ASTC_12x12, // Lowest quality
            };

            // Use reflection to access private method
            var method = typeof(TextureOverrideUtility).GetMethod("IsHigherQualityFormat", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act & Assert - Test that each format is higher quality than all formats after it
            for (int i = 0; i < qualityOrder.Length - 1; i++)
            {
                for (int j = i + 1; j < qualityOrder.Length; j++)
                {
                    var isHigher = (bool)method.Invoke(null, new object[] { qualityOrder[i], qualityOrder[j] });
                    Assert.IsTrue(isHigher, 
                        $"{qualityOrder[i]} should be higher quality than {qualityOrder[j]}");
                }
            }
        }
    }
}