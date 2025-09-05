// <copyright file="PlatformTextureFormatResolverTests.cs" company="kurotu">
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
    /// Tests for PlatformTextureFormatResolver.
    /// </summary>
    public class PlatformTextureFormatResolverTests
    {
        private PlatformTextureFormatResolver resolver;
        private ToonStandardConvertSettings toonStandardSettings;
        private ToonLitConvertSettings toonLitSettings;

        /// <summary>
        /// Set up test fixtures.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            resolver = new PlatformTextureFormatResolver();
            toonStandardSettings = new ToonStandardConvertSettings
            {
                maxTextureSize = TextureSizeLimit.Max1024x1024,
                mobileTextureFormat = MobileTextureFormat.ASTC_6x6
            };
            toonLitSettings = new ToonLitConvertSettings
            {
                maxTextureSize = TextureSizeLimit.Max512x512,
                mobileTextureFormat = MobileTextureFormat.ASTC_8x8
            };
        }

        /// <summary>
        /// Test that fallback settings are used when no platform overrides exist.
        /// </summary>
        [Test]
        public void ResolveTextureFormat_NoOverrides_UsesFallbackSettings()
        {
            // Arrange
            var emptyTextures = new List<Texture>();

            // Act
            var result = resolver.ResolveTextureFormat(emptyTextures, toonStandardSettings);

            // Assert
            Assert.IsFalse(result.fromOverride);
            Assert.AreEqual(MobileTextureFormat.ASTC_6x6, result.mobileTextureFormat);
            Assert.AreEqual(1024, result.maxTextureSize);
        }

        /// <summary>
        /// Test that ToonLit settings are correctly handled as fallback.
        /// </summary>
        [Test]
        public void ResolveTextureFormat_ToonLitSettings_UsesFallbackCorrectly()
        {
            // Arrange
            var emptyTextures = new List<Texture>();

            // Act
            var result = resolver.ResolveTextureFormat(emptyTextures, toonLitSettings);

            // Assert
            Assert.IsFalse(result.fromOverride);
            Assert.AreEqual(MobileTextureFormat.ASTC_8x8, result.mobileTextureFormat);
            Assert.AreEqual(512, result.maxTextureSize);
        }

        /// <summary>
        /// Test that null texture collection is handled gracefully.
        /// </summary>
        [Test]
        public void ResolveTextureFormat_NullTextures_UsesFallbackSettings()
        {
            // Act
            var result = resolver.ResolveTextureFormat(null, toonStandardSettings);

            // Assert
            Assert.IsFalse(result.fromOverride);
            Assert.AreEqual(MobileTextureFormat.ASTC_6x6, result.mobileTextureFormat);
            Assert.AreEqual(1024, result.maxTextureSize);
        }
    }
}