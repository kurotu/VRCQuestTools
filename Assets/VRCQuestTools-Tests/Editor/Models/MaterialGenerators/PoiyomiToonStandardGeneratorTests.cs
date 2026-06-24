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

        /// <summary>
        /// Ensure rim slot 1 is used when slot 0 is disabled but slot 1 is enabled.
        /// </summary>
        [Test]
        public void ConvertToToonStandard_NonBake_RimSlot1Only_Used()
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
            sourceMaterial.Object.SetFloat("_EnableRimLighting", 0.0f);
            sourceMaterial.Object.SetFloat("_EnableRim2Lighting", 1.0f);
            sourceMaterial.Object.SetFloat("_Rim2Style", 0.0f); // Poiyomi style
            sourceMaterial.Object.SetColor("_Rim2LightColor", Color.red);
            sourceMaterial.Object.SetFloat("_Rim2Strength", 10.0f); // -> intensity 10/20 = 0.5
            sourceMaterial.Object.SetFloat("_Rim2Width", 0.6f); // -> range 0.6

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
            Assert.IsTrue(wrapper.UseRimLighting, "Rim lighting should be enabled from slot 1.");
            Assert.AreEqual(0.6f, wrapper.RimRange, 0.01f, "Rim range should come from slot 1 width.");
            Assert.AreEqual(0.5f, wrapper.RimIntensity, 0.01f, "Rim intensity should come from slot 1 strength.");
            Assert.AreEqual(1.0f, wrapper.RimColor.r, 0.05f, "Rim color R should come from slot 1.");
            Assert.AreEqual(0.0f, wrapper.RimColor.g, 0.05f, "Rim color G should come from slot 1.");
            Assert.AreEqual(0.0f, wrapper.RimColor.b, 0.05f, "Rim color B should come from slot 1.");
        }

        /// <summary>
        /// Ensure rim lighting is disabled when both slots are off.
        /// </summary>
        [Test]
        public void ConvertToToonStandard_NonBake_RimBothDisabled_NotUsed()
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
            sourceMaterial.Object.SetFloat("_EnableRimLighting", 0.0f);
            sourceMaterial.Object.SetFloat("_EnableRim2Lighting", 0.0f);

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
            Assert.IsFalse(wrapper.UseRimLighting, "Rim lighting should be disabled when both slots are off.");
        }

        /// <summary>
        /// Ensure matcap slot 1 is used when slot 0 is disabled but slot 1 is enabled.
        /// </summary>
        [Test]
        public void ConvertToToonStandard_NonBake_MatcapSlot1Only_Used()
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
            using var matcapTex = DisposableObject.New(new Texture2D(4, 4));
            sourceMaterial.Object.SetFloat("_MatcapEnable", 0.0f);
            sourceMaterial.Object.SetFloat("_Matcap2Enable", 1.0f);
            sourceMaterial.Object.SetTexture("_Matcap2", matcapTex.Object);
            sourceMaterial.Object.SetFloat("_Matcap2Intensity", 2.5f); // -> strength 2.5/5 = 0.5
            sourceMaterial.Object.SetFloat("_Matcap2Replace", 1.0f);

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
            Assert.IsTrue(wrapper.UseMatcap, "Matcap should be enabled from slot 1.");
            Assert.AreEqual(matcapTex.Object, wrapper.Matcap, "Matcap texture should come from slot 1.");
            Assert.AreEqual(0.5f, wrapper.MatcapStrength, 0.01f, "Matcap strength should come from slot 1 intensity.");
        }

        /// <summary>
        /// Ensure Stylized Reflections (lilToon mode) takes priority over Mochie BRDF.
        /// </summary>
        [Test]
        public void ConvertToToonStandard_NonBake_StylizedReflections_PriorityOverMochie()
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

            // Enable both Mochie BRDF and Stylized Reflections; stylized should win.
            sourceMaterial.Object.SetFloat("_MochieBRDF", 1.0f);
            sourceMaterial.Object.SetFloat("_MochieMetallicMultiplier", 0.1f);
            sourceMaterial.Object.SetFloat("_MochieRoughnessMultiplier", 0.2f);

            sourceMaterial.Object.SetFloat("_StylizedSpecular", 1.0f);
            sourceMaterial.Object.SetFloat("_StylizedReflectionMode", 1.0f); // lilToon
            sourceMaterial.Object.SetFloat("_Metallic", 0.7f);
            sourceMaterial.Object.SetFloat("_Smoothness", 0.3f);
            sourceMaterial.Object.SetFloat("_Reflectance", 0.9f);

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
            Assert.IsTrue(wrapper.UseSpecular, "Specular should be enabled.");
            Assert.AreEqual(0.7f, wrapper.MetallicStrength, 0.01f, "Metallic should come from Stylized (lilToon), not Mochie.");
            Assert.AreEqual(0.3f, wrapper.GlossStrength, 0.01f, "Gloss should come from Stylized (lilToon), not Mochie.");
            Assert.AreEqual(0.9f, wrapper.Reflectance, 0.01f, "Reflectance should come from Stylized (lilToon).");
        }

        /// <summary>
        /// Ensure Stylized Reflections UnityChan mode maps to approximate ToonStandard specular.
        /// </summary>
        [Test]
        public void ConvertToToonStandard_NonBake_StylizedReflections_UnityChanMode()
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
            sourceMaterial.Object.SetFloat("_StylizedSpecular", 1.0f);
            sourceMaterial.Object.SetFloat("_StylizedReflectionMode", 0.0f); // UnityChan
            sourceMaterial.Object.SetFloat("_StylizedSpecularStrength", 0.8f); // -> reflectance 0.8
            sourceMaterial.Object.SetFloat("_HighColor_Power", 0.25f); // -> gloss 1 - 0.25 = 0.75
            sourceMaterial.Object.SetFloat("_StylizedSpecularFeather", 0.2f); // -> sharpness 1 - 0.2 = 0.8

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
            Assert.IsTrue(wrapper.UseSpecular, "Specular should be enabled from Stylized Reflections.");
            Assert.AreEqual(0.0f, wrapper.MetallicStrength, 0.01f, "UnityChan mode has no metallic.");
            Assert.AreEqual(0.75f, wrapper.GlossStrength, 0.01f, "Gloss should approximate from highlight size.");
            Assert.AreEqual(0.8f, wrapper.Sharpness, 0.01f, "Sharpness should come from feather.");
            Assert.AreEqual(0.8f, wrapper.Reflectance, 0.01f, "Reflectance should come from stylized strength.");
        }
    }
}
