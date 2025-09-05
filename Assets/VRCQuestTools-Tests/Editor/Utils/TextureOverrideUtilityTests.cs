// <copyright file="TextureOverrideUtilityTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using KRT.VRCQuestTools.Models;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Tests for TextureOverrideUtility.
    /// </summary>
    public class TextureOverrideUtilityTests
    {
        /// <summary>
        /// Test that GetBestPlatformOverride returns no override when no textures are provided.
        /// </summary>
        [Test]
        public void GetBestPlatformOverride_EmptyTextureList_ReturnsNoOverride()
        {
            // Arrange
            var emptyTextures = new List<Texture>();

            // Act
            var result = TextureOverrideUtility.GetBestPlatformOverride(emptyTextures);

            // Assert
            Assert.IsFalse(result.hasOverride);
            Assert.AreEqual(0, result.maxTextureSize);
            Assert.AreEqual(MobileTextureFormat.ASTC_12x12, result.textureFormat);
        }

        /// <summary>
        /// Test that GetBestPlatformOverride returns no override when textures have no platform overrides.
        /// </summary>
        [Test]
        public void GetBestPlatformOverride_TexturesWithoutOverrides_ReturnsNoOverride()
        {
            // Arrange
            var texture = TestUtils.LoadFixture<Texture2D>("Textures/albedo_1024px_png.png");
            var textures = new List<Texture> { texture };

            // Act
            var result = TextureOverrideUtility.GetBestPlatformOverride(textures);

            // Assert
            Assert.IsFalse(result.hasOverride);
        }

        /// <summary>
        /// Test that null textures are handled gracefully.
        /// </summary>
        [Test]
        public void GetBestPlatformOverride_WithNullTextures_ReturnsNoOverride()
        {
            // Arrange
            var textures = new List<Texture> { null };

            // Act
            var result = TextureOverrideUtility.GetBestPlatformOverride(textures);

            // Assert
            Assert.IsFalse(result.hasOverride);
        }

        /// <summary>
        /// Test that higher quality ASTC formats are correctly prioritized.
        /// </summary>
        [Test]
        public void IsHigherQualityFormat_ASTC4x4_IsHigherThanASTC6x6()
        {
            // This test accesses private method through reflection for testing purposes
            var method = typeof(TextureOverrideUtility).GetMethod("IsHigherQualityFormat", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            // Act
            var result = (bool)method.Invoke(null, new object[] { MobileTextureFormat.ASTC_4x4, MobileTextureFormat.ASTC_6x6 });

            // Assert
            Assert.IsTrue(result, "ASTC_4x4 should be higher quality than ASTC_6x6");
        }

        /// <summary>
        /// Test that lower quality ASTC formats are correctly identified.
        /// </summary>
        [Test]
        public void IsHigherQualityFormat_ASTC12x12_IsLowerThanASTC4x4()
        {
            // This test accesses private method through reflection for testing purposes
            var method = typeof(TextureOverrideUtility).GetMethod("IsHigherQualityFormat", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            // Act
            var result = (bool)method.Invoke(null, new object[] { MobileTextureFormat.ASTC_12x12, MobileTextureFormat.ASTC_4x4 });

            // Assert
            Assert.IsFalse(result, "ASTC_12x12 should be lower quality than ASTC_4x4");
        }
    }
}