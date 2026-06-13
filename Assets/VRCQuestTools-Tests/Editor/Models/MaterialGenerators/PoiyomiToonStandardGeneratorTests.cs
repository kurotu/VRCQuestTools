// <copyright file="PoiyomiToonStandardGeneratorTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Tests for Poiyomi to ToonStandard conversion.
    /// </summary>
    public class PoiyomiToonStandardGeneratorTests
    {
        private const string PoiyomiShaderName = ".poiyomi/Poiyomi Toon";

        /// <summary>
        /// Ensure non-bake conversion preserves main texture scale/offset from _MainTex_ST.
        /// </summary>
        [Test]
        public void ConvertToToonStandard_NonBake_MainTextureST_Preserved()
        {
            if (!AssetUtility.IsPoiyomiImported())
            {
                Assert.Ignore("Poiyomi is not installed.");
                return;
            }

            TestUtils.AssertIgnoreOnMissingShader("VRChat/Mobile/Toon Standard");

            var shader = Shader.Find(PoiyomiShaderName);
            Assert.NotNull(shader, $"{PoiyomiShaderName} shader not found.");

            using var sourceMaterial = DisposableObject.New(new Material(shader));
            var expectedScale = new Vector2(2.5f, 3.0f);
            var expectedOffset = new Vector2(0.1f, 0.2f);
            sourceMaterial.Object.SetVector("_MainTex_ST", new Vector4(expectedScale.x, expectedScale.y, expectedOffset.x, expectedOffset.y));

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
            };

            var poiMat = new PoiyomiMaterial(sourceMaterial.Object);
            var generator = new PoiyomiToonStandardGenerator(poiMat, settings, null, false);

            Material resultMat = null;
            generator.GenerateMaterial(poiMat, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) =>
            {
                resultMat = mat;
            }).WaitForCompletion();
            using var resultMaterial = DisposableObject.New(resultMat);

            Assert.IsNotNull(resultMat, "Generated material should not be null.");
            var wrapper = new ToonStandardMaterialWrapper(resultMat);
            Assert.AreEqual(expectedScale, wrapper.MainTextureScale, "Main texture scale should be preserved.");
            Assert.AreEqual(expectedOffset, wrapper.MainTextureOffset, "Main texture offset should be preserved.");
        }

        /// <summary>
        /// Ensure non-bake conversion uses flat ramp when shading is disabled.
        /// </summary>
        [Test]
        public void ConvertToToonStandard_NonBake_ShadingDisabled_UsesFlatRamp()
        {
            if (!AssetUtility.IsPoiyomiImported())
            {
                Assert.Ignore("Poiyomi is not installed.");
                return;
            }

            TestUtils.AssertIgnoreOnMissingShader("VRChat/Mobile/Toon Standard");

            var shader = Shader.Find(PoiyomiShaderName);
            Assert.NotNull(shader, $"{PoiyomiShaderName} shader not found.");

            using var sourceMaterial = DisposableObject.New(new Material(shader));
            sourceMaterial.Object.SetFloat("_ShadingEnabled", 0.0f);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
            };

            var poiMat = new PoiyomiMaterial(sourceMaterial.Object);
            var generator = new PoiyomiToonStandardGenerator(poiMat, settings, null, false);

            Material resultMat = null;
            generator.GenerateMaterial(poiMat, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) => { resultMat = mat; }).WaitForCompletion();
            using var resultMaterial = DisposableObject.New(resultMat);

            Assert.IsNotNull(resultMat, "Generated material should not be null.");
            var wrapper = new ToonStandardMaterialWrapper(resultMat);
            Assert.AreEqual(ToonStandardMaterialWrapper.RampTexture.Flat, wrapper.ShadowRamp, "Shadow ramp should be flat when shading is disabled.");
        }

        /// <summary>
        /// Ensure non-bake conversion uses fallback ramp when shading is enabled.
        /// </summary>
        [Test]
        public void ConvertToToonStandard_NonBake_ShadingEnabled_UsesFallbackRamp()
        {
            if (!AssetUtility.IsPoiyomiImported())
            {
                Assert.Ignore("Poiyomi is not installed.");
                return;
            }

            TestUtils.AssertIgnoreOnMissingShader("VRChat/Mobile/Toon Standard");

            var shader = Shader.Find(PoiyomiShaderName);
            Assert.NotNull(shader, $"{PoiyomiShaderName} shader not found.");

            using var sourceMaterial = DisposableObject.New(new Material(shader));
            sourceMaterial.Object.SetFloat("_ShadingEnabled", 1.0f);

            using var fallbackRamp = DisposableObject.New(new Texture2D(2, 2));
            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
                fallbackShadowRamp = fallbackRamp.Object,
            };

            var poiMat = new PoiyomiMaterial(sourceMaterial.Object);
            var generator = new PoiyomiToonStandardGenerator(poiMat, settings, null, false);

            Material resultMat = null;
            generator.GenerateMaterial(poiMat, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) => { resultMat = mat; }).WaitForCompletion();
            using var resultMaterial = DisposableObject.New(resultMat);

            Assert.IsNotNull(resultMat, "Generated material should not be null.");
            var wrapper = new ToonStandardMaterialWrapper(resultMat);
            Assert.AreEqual(fallbackRamp.Object, wrapper.ShadowRamp, "Shadow ramp should use fallback when shading is enabled.");
        }

        /// <summary>
        /// Ensure non-bake conversion disables emission when all channels are off.
        /// </summary>
        [Test]
        public void ConvertToToonStandard_NonBake_EmissionDisabled()
        {
            if (!AssetUtility.IsPoiyomiImported())
            {
                Assert.Ignore("Poiyomi is not installed.");
                return;
            }

            TestUtils.AssertIgnoreOnMissingShader("VRChat/Mobile/Toon Standard");

            var shader = Shader.Find(PoiyomiShaderName);
            Assert.NotNull(shader, $"{PoiyomiShaderName} shader not found.");

            using var sourceMaterial = DisposableObject.New(new Material(shader));
            sourceMaterial.Object.SetFloat("_EnableEmission", 0.0f);
            sourceMaterial.Object.SetFloat("_EnableEmission1", 0.0f);
            sourceMaterial.Object.SetFloat("_EnableEmission2", 0.0f);
            sourceMaterial.Object.SetFloat("_EnableEmission3", 0.0f);

            using var sharedBlack = DisposableObject.New(new Texture2D(2, 2));
            sharedBlack.Object.SetPixel(0, 0, Color.black);
            sharedBlack.Object.Apply();

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
            };

            var poiMat = new PoiyomiMaterial(sourceMaterial.Object);
            var generator = new PoiyomiToonStandardGenerator(poiMat, settings, sharedBlack.Object, false);

            Material resultMat = null;
            generator.GenerateMaterial(poiMat, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) => { resultMat = mat; }).WaitForCompletion();
            using var resultMaterial = DisposableObject.New(resultMat);

            Assert.IsNotNull(resultMat, "Generated material should not be null.");
            var wrapper = new ToonStandardMaterialWrapper(resultMat);
            Assert.AreEqual(sharedBlack.Object, wrapper.EmissionMap, "Emission map should be shared black texture when emission is disabled.");
            Assert.AreEqual(Color.black, wrapper.EmissionColor, "Emission color should be black when emission is disabled.");
        }

        /// <summary>
        /// Ensure non-bake conversion copies normal map when set.
        /// </summary>
        [Test]
        public void ConvertToToonStandard_NonBake_NormalMap_CopiedWhenSet()
        {
            if (!AssetUtility.IsPoiyomiImported())
            {
                Assert.Ignore("Poiyomi is not installed.");
                return;
            }

            TestUtils.AssertIgnoreOnMissingShader("VRChat/Mobile/Toon Standard");

            var shader = Shader.Find(PoiyomiShaderName);
            Assert.NotNull(shader, $"{PoiyomiShaderName} shader not found.");

            using var sourceMaterial = DisposableObject.New(new Material(shader));
            using var normalMap = DisposableObject.New(new Texture2D(4, 4, TextureFormat.RGBA32, false));
            sourceMaterial.Object.SetTexture("_BumpMap", normalMap.Object);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
            };

            var poiMat = new PoiyomiMaterial(sourceMaterial.Object);
            var generator = new PoiyomiToonStandardGenerator(poiMat, settings, null, false);

            Material resultMat = null;
            generator.GenerateMaterial(poiMat, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) => { resultMat = mat; }).WaitForCompletion();
            using var resultMaterial = DisposableObject.New(resultMat);

            Assert.IsNotNull(resultMat, "Generated material should not be null.");
            var wrapper = new ToonStandardMaterialWrapper(resultMat);
            Assert.IsTrue(wrapper.UseNormalMap, "UseNormalMap should be true when source normal map exists.");
            Assert.AreEqual(normalMap.Object, wrapper.NormalMap, "Normal map texture should be copied.");
        }
    }
}
