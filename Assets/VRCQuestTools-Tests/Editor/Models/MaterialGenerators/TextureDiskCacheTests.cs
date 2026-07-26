// <copyright file="TextureDiskCacheTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.IO;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Tests for the texture disk cache behind <see cref="MaterialGeneratorUtility"/>: a generated texture is
    /// written to <see cref="CacheManager.Texture"/> and restored from it on the next identical request.
    /// </summary>
    public class TextureDiskCacheTests
    {
        /// <summary>
        /// Setup test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            CacheManager.Texture.Clear();
        }

        /// <summary>
        /// Test that generating the same texture twice writes one cache entry and restores it byte for byte,
        /// covering the whole disk round trip (compressed result -> binary entry -> restored texture) rather
        /// than just the serialization in isolation.
        /// </summary>
        [Test]
        public void GeneratedTextureIsRestoredFromDiskCache()
        {
            var settings = new ToonLitConvertSettings
            {
                generateQuestTextures = true,
                mainTextureBrightness = 1.0f,
            };
            var generator = new ToonLitGenerator(settings);
            var cacheFolder = VRCQuestToolsSettings.TextureCacheFolder;

            Material generatedMaterial = null;
            generator.GenerateMaterial(TestUtils.LoadMaterialWrapper("Standard_NoEmission.mat"), UnityEditor.BuildTarget.Android, false, string.Empty, (m) => { generatedMaterial = m; })
                .WaitForCompletion();

            var entriesAfterGeneration = Directory.GetFiles(cacheFolder, "*.bin").Length;
            Assert.Greater(entriesAfterGeneration, 0, "Generating a texture should write a binary disk cache entry.");

            Material restoredMaterial = null;
            generator.GenerateMaterial(TestUtils.LoadMaterialWrapper("Standard_NoEmission.mat"), UnityEditor.BuildTarget.Android, false, string.Empty, (m) => { restoredMaterial = m; })
                .WaitForCompletion();

            using (var generated = DisposableObject.New(generatedMaterial))
            using (var restored = DisposableObject.New(restoredMaterial))
            using (var generatedTexture = DisposableObject.New(generated.Object.mainTexture as Texture2D))
            using (var restoredTexture = DisposableObject.New(restored.Object.mainTexture as Texture2D))
            {
                Assert.AreEqual(entriesAfterGeneration, Directory.GetFiles(cacheFolder, "*.bin").Length, "The second request should reuse the existing entry instead of writing another one.");
                Assert.IsNotNull(generatedTexture.Object);
                Assert.IsNotNull(restoredTexture.Object);
                Assert.AreEqual(generatedTexture.Object.width, restoredTexture.Object.width);
                Assert.AreEqual(generatedTexture.Object.height, restoredTexture.Object.height);
                Assert.AreEqual(generatedTexture.Object.format, restoredTexture.Object.format);
                Assert.AreEqual(generatedTexture.Object.mipmapCount, restoredTexture.Object.mipmapCount);
                Assert.AreEqual(generatedTexture.Object.GetRawTextureData(), restoredTexture.Object.GetRawTextureData(), "The restored texture should hold exactly the bytes that were cached.");
            }
        }
    }
}
