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

        /// <summary>
        /// Test that a Toon Standard packed mask keeps linear values when it is restored from the disk cache.
        /// It was stored as an sRGB texture, so the second conversion of the same material read gamma-decoded
        /// metallic/gloss/occlusion values while the first conversion read the raw ones.
        /// </summary>
        [Test]
        public void ToonStandardPackedMaskIsRestoredAsLinear()
        {
            if (!AssetUtility.IsLilToonImported() || Shader.Find("lilToon") == null)
            {
                Assert.Ignore("lilToon is not installed.");
                return;
            }
            TestUtils.AssertIgnoreOnMissingShader("VRChat/Mobile/Toon Standard");

            using var sourceMaterial = DisposableObject.New(new Material(Shader.Find("lilToon")));
            sourceMaterial.Object.SetFloat("_UseReflection", 1);
            sourceMaterial.Object.SetColor("_ReflectionColor", new Color(0.8f, 0.8f, 0.8f, 0.6f));

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = true,
                maxTextureSize = TextureSizeLimit.NoLimit,
                maskMaxTextureSize = TextureSizeLimit.NoLimit,
            };
            settings.SetAllFeatures(false);
            settings.useSpecular = true;
            var cacheFolder = VRCQuestToolsSettings.TextureCacheFolder;

            var generated = SampleMetallicMap(sourceMaterial.Object, settings);
            var entriesAfterGeneration = Directory.GetFiles(cacheFolder, "*.bin").Length;
            var restored = SampleMetallicMap(sourceMaterial.Object, settings);

            Assert.AreEqual(entriesAfterGeneration, Directory.GetFiles(cacheFolder, "*.bin").Length, "The second conversion should be restored from the cache.");
            Assert.AreEqual(generated, restored, 0.02f, "The restored mask should be decoded as linear like the generated one.");
        }

        private static float SampleMetallicMap(Material sourceMaterial, ToonStandardConvertSettings settings)
        {
            var lilMat = new LilToonMaterial(sourceMaterial);
            Material resultMat = null;
            new LilToonToonStandardGenerator(lilMat, settings, null, false).GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, string.Empty, (m) => resultMat = m).WaitForCompletion();
            using var result = DisposableObject.New(resultMat);
            var metallicMap = new ToonStandardMaterialWrapper(resultMat).MetallicMap as Texture2D;
            Assert.IsNotNull(metallicMap, "Metallic map should be generated.");

            var rt = RenderTexture.GetTemporary(metallicMap.width, metallicMap.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var previous = RenderTexture.active;
            var readable = new Texture2D(metallicMap.width, metallicMap.height, TextureFormat.RGBA32, false, true);
            try
            {
                Graphics.Blit(metallicMap, rt);
                RenderTexture.active = rt;
                readable.ReadPixels(new Rect(0, 0, metallicMap.width, metallicMap.height), 0, 0);
                readable.Apply(false, false);
                return readable.GetPixel(metallicMap.width / 2, metallicMap.height / 2).r;
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
                Object.DestroyImmediate(readable);
            }
        }
    }
}
