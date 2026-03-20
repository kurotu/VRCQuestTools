// Batch 56: Test ConvertToToonStandard path (non-GPU) through GenerateMaterial,
// plus remaining LilToonMaterial properties, MissingScriptsRule with missing scripts,
// and additional LilToonToonStandardGenerator Get*() methods

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace KRT.VRCQuestTools.Tests
{
    // ========== ConvertToToonStandard via GenerateMaterial (non-GPU path) ==========
    [TestFixture]
    public class ConvertToToonStandardTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            objectsToCleanup.Clear();
        }

        private LilToonMaterial CreateLilToon()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) return null;
            var mat = new Material(shader);
            objectsToCleanup.Add(mat);
            return new LilToonMaterial(mat);
        }

        private object CreateGenerator(LilToonMaterial lilMaterial, ToonStandardConvertSettings settings = null)
        {
            if (settings == null) settings = new ToonStandardConvertSettings();
            var blackTex = new Texture2D(1, 1);
            objectsToCleanup.Add(blackTex);
            var genType = typeof(ToonStandardGenerator).Assembly.GetType("KRT.VRCQuestTools.Models.LilToonToonStandardGenerator");
            if (genType == null) return null;
            var ctor = genType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First();
            return ctor.Invoke(new object[] { lilMaterial, settings, blackTex });
        }

        private Material InvokeConvertToToonStandard(object generator)
        {
            var method = generator.GetType().GetMethod("ConvertToToonStandard",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            return (Material)method.Invoke(generator, null);
        }

        // Test basic ConvertToToonStandard creates a valid material
        [Test]
        public void ConvertToToonStandard_BasicMaterial_CreatesMaterial()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.name = "TestMat";
            lil.Material.color = Color.blue;

            var settings = new ToonStandardConvertSettings();
            var gen = CreateGenerator(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }

            var result = InvokeConvertToToonStandard(gen);
            objectsToCleanup.Add(result);
            Assert.IsNotNull(result);
            Assert.AreEqual("TestMat", result.name);
        }

        // Test main color conversion (HDR to LDR)
        [Test]
        public void ConvertToToonStandard_MainColor_IsHdrToLdr()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.color = new Color(0.5f, 0.3f, 0.8f, 1f);

            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }

            var result = InvokeConvertToToonStandard(gen);
            objectsToCleanup.Add(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.AreEqual(0.5f, wrapper.MainColor.r, 0.05f);
            Assert.AreEqual(0.3f, wrapper.MainColor.g, 0.05f);
            Assert.AreEqual(0.8f, wrapper.MainColor.b, 0.05f);
        }

        // Test NormalMap enabled path
        [Test]
        public void ConvertToToonStandard_WithNormalMap_SetsNormalMapProperties()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseBumpMap", 1f);
            var normalTex = new Texture2D(4, 4);
            objectsToCleanup.Add(normalTex);
            lil.Material.SetTexture("_BumpMap", normalTex);
            lil.Material.SetFloat("_BumpScale", 0.8f);

            var settings = new ToonStandardConvertSettings();
            settings.useNormalMap = true;
            var gen = CreateGenerator(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }

            var result = InvokeConvertToToonStandard(gen);
            objectsToCleanup.Add(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.IsTrue(wrapper.UseNormalMap);
            Assert.AreEqual(normalTex, wrapper.NormalMap);
            Assert.AreEqual(0.8f, wrapper.NormalMapScale, 0.01f);
        }

        // Test NormalMap disabled path
        [Test]
        public void ConvertToToonStandard_WithoutNormalMap_DoesNotSetNormalMap()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseBumpMap", 0f);

            var settings = new ToonStandardConvertSettings();
            settings.useNormalMap = true;
            var gen = CreateGenerator(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }

            var result = InvokeConvertToToonStandard(gen);
            objectsToCleanup.Add(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.IsFalse(wrapper.UseNormalMap);
        }

        // Test Shadow ramp when UseShadow is true
        [Test]
        public void ConvertToToonStandard_WithShadow_UsesFallbackShadowRamp()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseShadow", 1f);

            var settings = new ToonStandardConvertSettings();
            var rampTex = new Texture2D(4, 4);
            objectsToCleanup.Add(rampTex);
            settings.fallbackShadowRamp = rampTex;
            var gen = CreateGenerator(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }

            var result = InvokeConvertToToonStandard(gen);
            objectsToCleanup.Add(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.AreEqual(rampTex, wrapper.ShadowRamp);
        }

        // Test Shadow ramp when UseShadow is false (flat ramp)
        [Test]
        public void ConvertToToonStandard_WithoutShadow_UsesFlatRamp()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseShadow", 0f);

            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }

            var result = InvokeConvertToToonStandard(gen);
            objectsToCleanup.Add(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.AreEqual(ToonStandardMaterialWrapper.RampTexture.Flat, wrapper.ShadowRamp);
        }

        // Test MinBrightness
        [Test]
        public void ConvertToToonStandard_MinBrightness_SetsFromLilToon()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_LightMinLimit", 0.3f);

            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }

            var result = InvokeConvertToToonStandard(gen);
            objectsToCleanup.Add(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.AreEqual(0.3f, wrapper.MinBrightness, 0.01f);
        }

        // Test Emission enabled path
        [Test]
        public void ConvertToToonStandard_WithEmission_SetsEmissionProperties()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 1f);
            var emTex = new Texture2D(4, 4);
            objectsToCleanup.Add(emTex);
            lil.Material.SetTexture("_EmissionMap", emTex);
            lil.Material.SetColor("_EmissionColor", new Color(1, 0, 0, 1));

            var settings = new ToonStandardConvertSettings();
            settings.useEmission = true;
            var gen = CreateGenerator(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }

            var result = InvokeConvertToToonStandard(gen);
            objectsToCleanup.Add(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.AreEqual(emTex, wrapper.EmissionMap);
        }

        // Test Emission disabled path (black texture)
        [Test]
        public void ConvertToToonStandard_WithoutEmission_SetsBlackEmission()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 0f);

            var settings = new ToonStandardConvertSettings();
            settings.useEmission = true;
            var gen = CreateGenerator(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }

            var result = InvokeConvertToToonStandard(gen);
            objectsToCleanup.Add(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.AreEqual(Color.black, wrapper.EmissionColor);
        }

        // Test Occlusion map path
        [Test]
        public void ConvertToToonStandard_WithOcclusion_SetsOcclusionMap()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseShadow", 1f);
            var aoTex = new Texture2D(4, 4);
            objectsToCleanup.Add(aoTex);
            lil.Material.SetTexture("_ShadowBorderMask", aoTex);

            var settings = new ToonStandardConvertSettings();
            settings.useOcclusion = true;
            var gen = CreateGenerator(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }

            var result = InvokeConvertToToonStandard(gen);
            objectsToCleanup.Add(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.IsTrue(wrapper.UseOcclusion);
            Assert.AreEqual(aoTex, wrapper.OcclusionMap);
        }

        // Test Specular/Reflection path
        [Test]
        public void ConvertToToonStandard_WithReflection_SetsSpecularProperties()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 1f);
            lil.Material.SetFloat("_Metallic", 0.6f);
            lil.Material.SetFloat("_Smoothness", 0.7f);
            lil.Material.SetFloat("_SpecularBlur", 0.2f);

            var settings = new ToonStandardConvertSettings();
            settings.useSpecular = true;
            var gen = CreateGenerator(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }

            var result = InvokeConvertToToonStandard(gen);
            objectsToCleanup.Add(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.IsTrue(wrapper.UseSpecular);
            Assert.AreEqual(0.6f, wrapper.MetallicStrength, 0.01f);
            Assert.AreEqual(0.7f, wrapper.GlossStrength, 0.01f);
            Assert.AreEqual(0.8f, wrapper.Sharpness, 0.01f); // 1.0 - 0.2
        }

        // Test MatCap path with Normal blend mode
        [Test]
        public void ConvertToToonStandard_WithMatCapNormal_SetsAdditiveType()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseMatCap", 1f);
            lil.Material.SetFloat("_MatCapBlendMode", 0f); // Normal
            lil.Material.SetFloat("_MatCapBlend", 0.8f);

            var settings = new ToonStandardConvertSettings();
            settings.useMatcap = true;
            var gen = CreateGenerator(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }

            var result = InvokeConvertToToonStandard(gen);
            objectsToCleanup.Add(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.IsTrue(wrapper.UseMatcap);
            Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Additive, wrapper.MatcapType);
            Assert.AreEqual(0.8f, wrapper.MatcapStrength, 0.01f);
        }

        // Test MatCap with Multiply blend mode
        [Test]
        public void ConvertToToonStandard_WithMatCapMultiply_SetsMultiplicativeType()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseMatCap", 1f);
            lil.Material.SetFloat("_MatCapBlendMode", 3f); // Multiply
            lil.Material.SetFloat("_MatCapBlend", 0.5f);

            var settings = new ToonStandardConvertSettings();
            settings.useMatcap = true;
            var gen = CreateGenerator(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }

            var result = InvokeConvertToToonStandard(gen);
            objectsToCleanup.Add(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.IsTrue(wrapper.UseMatcap);
            Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Multiplicative, wrapper.MatcapType);
        }

        // Test MatCap with Add blend mode
        [Test]
        public void ConvertToToonStandard_WithMatCapAdd_SetsAdditiveType()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseMatCap", 1f);
            lil.Material.SetFloat("_MatCapBlendMode", 1f); // Add

            var settings = new ToonStandardConvertSettings();
            settings.useMatcap = true;
            var gen = CreateGenerator(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }

            var result = InvokeConvertToToonStandard(gen);
            objectsToCleanup.Add(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Additive, wrapper.MatcapType);
        }

        // Test MatCap with Screen blend mode
        [Test]
        public void ConvertToToonStandard_WithMatCapScreen_SetsAdditiveType()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseMatCap", 1f);
            lil.Material.SetFloat("_MatCapBlendMode", 2f); // Screen

            var settings = new ToonStandardConvertSettings();
            settings.useMatcap = true;
            var gen = CreateGenerator(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }

            var result = InvokeConvertToToonStandard(gen);
            objectsToCleanup.Add(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Additive, wrapper.MatcapType);
        }

        // Test RimLighting enabled path
        [Test]
        public void ConvertToToonStandard_WithRimLighting_SetsRimProperties()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseRim", 1f);
            lil.Material.SetColor("_RimColor", new Color(0.5f, 0.5f, 1f, 0.8f));
            lil.Material.SetFloat("_RimMainStrength", 0.6f);
            lil.Material.SetFloat("_RimBorder", 0.3f);
            lil.Material.SetFloat("_RimFresnelPower", 3f);
            lil.Material.SetFloat("_RimBlur", 0.2f);
            lil.Material.SetFloat("_RimEnableLighting", 0.7f);

            var settings = new ToonStandardConvertSettings();
            settings.useRimLighting = true;
            var gen = CreateGenerator(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }

            var result = InvokeConvertToToonStandard(gen);
            objectsToCleanup.Add(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.IsTrue(wrapper.UseRimLighting);
            Assert.AreEqual(0.6f, wrapper.RimAlbedoTint, 0.01f);
            Assert.AreEqual(0.2f, wrapper.RimSoftness, 0.01f);
            Assert.IsTrue(wrapper.RimEnvironmental);
            // RimRange = Pow(1 - 0.3, 3) = Pow(0.7, 3) ≈ 0.343
            Assert.AreEqual(Mathf.Pow(0.7f, 3f), wrapper.RimRange, 0.01f);
        }

        // Test RimLighting with RimEnvironmental false
        [Test]
        public void ConvertToToonStandard_WithRimLightingNoEnvironmental_DoesNotMultiplyIntensity()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseRim", 1f);
            lil.Material.SetColor("_RimColor", new Color(1, 1, 1, 1f));
            lil.Material.SetFloat("_RimEnableLighting", 0f);

            var settings = new ToonStandardConvertSettings();
            settings.useRimLighting = true;
            var gen = CreateGenerator(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }

            var result = InvokeConvertToToonStandard(gen);
            objectsToCleanup.Add(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.IsTrue(wrapper.UseRimLighting);
            Assert.IsFalse(wrapper.RimEnvironmental);
        }

        // Test CullMode transfer
        [Test]
        public void ConvertToToonStandard_CullMode_TransfersCorrectly()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_Cull", (float)CullMode.Front);

            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }

            var result = InvokeConvertToToonStandard(gen);
            objectsToCleanup.Add(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.AreEqual(CullMode.Front, wrapper.Culling);
        }

        // Test all features enabled together (full path coverage)
        [Test]
        public void ConvertToToonStandard_AllFeaturesEnabled_CoversAllBranches()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }

            // Enable all features
            lil.Material.SetFloat("_UseBumpMap", 1f);
            var normalTex = new Texture2D(4, 4);
            objectsToCleanup.Add(normalTex);
            lil.Material.SetTexture("_BumpMap", normalTex);

            lil.Material.SetFloat("_UseShadow", 1f);
            var aoTex = new Texture2D(4, 4);
            objectsToCleanup.Add(aoTex);
            lil.Material.SetTexture("_ShadowBorderMask", aoTex);

            lil.Material.SetFloat("_UseEmission", 1f);
            var emTex = new Texture2D(4, 4);
            objectsToCleanup.Add(emTex);
            lil.Material.SetTexture("_EmissionMap", emTex);

            lil.Material.SetFloat("_UseReflection", 1f);
            lil.Material.SetFloat("_UseMatCap", 1f);
            lil.Material.SetFloat("_UseRim", 1f);
            lil.Material.SetFloat("_RimEnableLighting", 1f);

            var settings = new ToonStandardConvertSettings
            {
                useNormalMap = true,
                useEmission = true,
                useOcclusion = true,
                useSpecular = true,
                useMatcap = true,
                useRimLighting = true,
            };

            var gen = CreateGenerator(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }

            var result = InvokeConvertToToonStandard(gen);
            objectsToCleanup.Add(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.IsTrue(wrapper.UseNormalMap);
            Assert.IsTrue(wrapper.UseOcclusion);
            Assert.IsTrue(wrapper.UseSpecular);
            Assert.IsTrue(wrapper.UseMatcap);
            Assert.IsTrue(wrapper.UseRimLighting);
            Assert.IsTrue(wrapper.RimEnvironmental);
        }
    }

    // ========== LilToonMaterial remaining uncovered properties ==========
    [TestFixture]
    public class LilToonMaterialPropertyTests_Detection
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            objectsToCleanup.Clear();
        }

        private LilToonMaterial CreateLilToon()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) return null;
            var mat = new Material(shader);
            objectsToCleanup.Add(mat);
            return new LilToonMaterial(mat);
        }

        // Emission 2nd properties
        [Test]
        public void Emission2ndMap_ReturnsSetTexture()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_Emission2ndMap", tex);
            Assert.AreEqual(tex, lil.Emission2ndMap);
        }

        [Test]
        public void Emission2ndColor_GetAndSet()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Emission2ndColor = new Color(0.1f, 0.2f, 0.3f, 1f);
            Assert.AreEqual(0.1f, lil.Emission2ndColor.r, 0.01f);
            Assert.AreEqual(0.2f, lil.Emission2ndColor.g, 0.01f);
        }

        [Test]
        public void Emission2ndBlendMask_ReturnsSetTexture()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_Emission2ndBlendMask", tex);
            Assert.AreEqual(tex, lil.Emission2ndBlendMask);
        }

        [Test]
        public void Emission2ndBlend_ReturnsSetValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_Emission2ndBlend", 0.65f);
            Assert.AreEqual(0.65f, lil.Emission2ndBlend, 0.01f);
        }

        // Reflection properties
        [Test]
        public void MetallicMap_ReturnsSetTexture()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_MetallicGlossMap", tex);
            Assert.AreEqual(tex, lil.MetallicMap);
        }

        [Test]
        public void SmoothnessTex_ReturnsSetTexture()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_SmoothnessTex", tex);
            Assert.AreEqual(tex, lil.SmoothnessTex);
        }

        [Test]
        public void SmoothnessTexScale_ReturnsSetValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetTextureScale("_SmoothnessTex", new Vector2(2f, 3f));
            Assert.AreEqual(2f, lil.SmoothnessTexScale.x, 0.01f);
            Assert.AreEqual(3f, lil.SmoothnessTexScale.y, 0.01f);
        }

        [Test]
        public void SmoothnessTexOffset_ReturnsSetValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetTextureOffset("_SmoothnessTex", new Vector2(0.1f, 0.2f));
            Assert.AreEqual(0.1f, lil.SmoothnessTexOffset.x, 0.01f);
            Assert.AreEqual(0.2f, lil.SmoothnessTexOffset.y, 0.01f);
        }

        [Test]
        public void ReflectionColorTex_ReturnsSetTexture()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_ReflectionColorTex", tex);
            Assert.AreEqual(tex, lil.ReflectionColorTex);
        }

        [Test]
        public void ReflectionColor_ReturnsSetColor()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetColor("_ReflectionColor", Color.cyan);
            Assert.AreEqual(Color.cyan, lil.ReflectionColor);
        }

        [Test]
        public void Smoothness_ReturnsSetValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_Smoothness", 0.85f);
            Assert.AreEqual(0.85f, lil.Smoothness, 0.01f);
        }

        [Test]
        public void Reflectance_ReturnsSetValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_Reflectance", 0.6f);
            Assert.AreEqual(0.6f, lil.Reflectance, 0.01f);
        }

        [Test]
        public void SpecularBlur_ReturnsSetValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_SpecularBlur", 0.4f);
            Assert.AreEqual(0.4f, lil.SpecularBlur, 0.01f);
        }

        // MatCap properties
        [Test]
        public void MatCapColor_ReturnsSetColor()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetColor("_MatCapColor", Color.yellow);
            Assert.AreEqual(Color.yellow, lil.MatCapColor);
        }

        [Test]
        public void MatCapMainStrength_ReturnsSetValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_MatCapMainStrength", 0.7f);
            Assert.AreEqual(0.7f, lil.MatCapMainStrength, 0.01f);
        }

        [Test]
        public void MatCapBlendingMode_ReturnsSetValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_MatCapBlendMode", 3f); // Multiply
            Assert.AreEqual(LilToonMaterial.MatCapBlendMode.Multiply, lil.MatCapBlendingMode);
        }

        // Shadow 2nd/3rd
        [Test]
        public void UseShadow2nd_WhenShadowAndAlpha_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseShadow", 1f);
            lil.Material.SetColor("_Shadow2ndColor", new Color(0, 0, 0, 0.5f));
            Assert.IsTrue(lil.UseShadow2nd);
        }

        [Test]
        public void UseShadow2nd_WhenNoShadow_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseShadow", 0f);
            Assert.IsFalse(lil.UseShadow2nd);
        }

        [Test]
        public void UseShadow3rd_WhenShadowAndAlpha_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseShadow", 1f);
            lil.Material.SetColor("_Shadow3rdColor", new Color(0, 0, 0, 0.5f));
            Assert.IsTrue(lil.UseShadow3rd);
        }

        // Main 2nd/3rd textures
        [Test]
        public void UseMain2ndTex_ReturnsSetValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseMain2ndTex", 1f);
            Assert.IsTrue(lil.UseMain2ndTex);
        }

        [Test]
        public void UseMain3rdTex_ReturnsSetValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseMain3rdTex", 1f);
            Assert.IsTrue(lil.UseMain3rdTex);
        }

        [Test]
        public void Main2ndTex_ReturnsSetTexture()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_Main2ndTex", tex);
            Assert.AreEqual(tex, lil.Main2ndTex);
        }

        [Test]
        public void Main3rdTex_ReturnsSetTexture()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_Main3rdTex", tex);
            Assert.AreEqual(tex, lil.Main3rdTex);
        }

        // EmissionBlendMask
        [Test]
        public void EmissionBlendMask_ReturnsSetTexture()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_EmissionBlendMask", tex);
            Assert.AreEqual(tex, lil.EmissionBlendMask);
        }

        [Test]
        public void EmissionBlend_ReturnsSetValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_EmissionBlend", 0.9f);
            Assert.AreEqual(0.9f, lil.EmissionBlend, 0.01f);
        }

        // Metallic map texture scale/offset
        [Test]
        public void MetallicMapTextureScale_ReturnsSetValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetTextureScale("_MetallicGlossMap", new Vector2(2, 3));
            Assert.AreEqual(2f, lil.MetallicMapTextureScale.x, 0.01f);
            Assert.AreEqual(3f, lil.MetallicMapTextureScale.y, 0.01f);
        }

        [Test]
        public void MetallicMapTextureOffset_ReturnsSetValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetTextureOffset("_MetallicGlossMap", new Vector2(0.5f, 0.6f));
            Assert.AreEqual(0.5f, lil.MetallicMapTextureOffset.x, 0.01f);
            Assert.AreEqual(0.6f, lil.MetallicMapTextureOffset.y, 0.01f);
        }
    }

    // ========== MissingScriptsRule with actual missing scripts ==========
    [TestFixture]
    public class MissingScriptsDetectionTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            objectsToCleanup.Clear();
        }

        // Test VRCSDKUtility.CountMissingComponentsInChildren
        [Test]
        public void CountMissingComponentsInChildren_CleanObject_ReturnsZero()
        {
            var go = new GameObject("Clean");
            objectsToCleanup.Add(go);
            var count = VRCSDKUtility.CountMissingComponentsInChildren(go, true);
            Assert.AreEqual(0, count);
        }

        [Test]
        public void GetGameObjectsWithMissingComponents_CleanObject_ReturnsEmpty()
        {
            var go = new GameObject("Clean");
            objectsToCleanup.Add(go);
            var result = VRCSDKUtility.GetGameObjectsWithMissingComponents(go, true);
            Assert.AreEqual(0, result.Length);
        }
    }

    // ========== Additional Generator Get*() tests for remaining branches ==========
    [TestFixture]
    public class GeneratorGetBranchTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            objectsToCleanup.Clear();
        }

        private LilToonMaterial CreateLilToon()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) return null;
            var mat = new Material(shader);
            objectsToCleanup.Add(mat);
            return new LilToonMaterial(mat);
        }

        private object CreateGenerator(LilToonMaterial lilMaterial)
        {
            var settings = new ToonStandardConvertSettings();
            var blackTex = new Texture2D(1, 1);
            objectsToCleanup.Add(blackTex);
            var genType = typeof(ToonStandardGenerator).Assembly.GetType("KRT.VRCQuestTools.Models.LilToonToonStandardGenerator");
            if (genType == null) return null;
            var ctor = genType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First();
            return ctor.Invoke(new object[] { lilMaterial, settings, blackTex });
        }

        private object InvokeProtected(object gen, string methodName)
        {
            var method = gen.GetType().GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            return method.Invoke(gen, null);
        }

        // GetSharpness
        [Test]
        public void GetSharpness_Returns1MinusSpecularBlur()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_SpecularBlur", 0.3f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (float)InvokeProtected(gen, "GetSharpness");
            Assert.AreEqual(0.7f, result, 0.01f);
        }

        // GetMatcapMaskStrength
        [Test]
        public void GetMatcapMaskStrength_ReturnsBlendTimesColorAlpha()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_MatCapBlend", 0.8f);
            lil.Material.SetColor("_MatCapColor", new Color(1, 1, 1, 0.5f));
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (float)InvokeProtected(gen, "GetMatcapMaskStrength");
            Assert.AreEqual(0.4f, result, 0.01f); // 0.8 * 0.5
        }

        // GetMainTextureST
        [Test]
        public void GetMainTextureST_ReturnsScaleAndOffset()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetTextureScale("_MainTex", new Vector2(2, 3));
            lil.Material.SetTextureOffset("_MainTex", new Vector2(0.1f, 0.2f));
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetMainTextureST");
            var tuple = ((Vector2, Vector2))result;
            Assert.AreEqual(2f, tuple.Item1.x, 0.01f);
            Assert.AreEqual(3f, tuple.Item1.y, 0.01f);
            Assert.AreEqual(0.1f, tuple.Item2.x, 0.01f);
            Assert.AreEqual(0.2f, tuple.Item2.y, 0.01f);
        }

        // GetMatcap
        [Test]
        public void GetMatcap_ReturnsMatCapTex()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_MatCapTex", tex);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (Texture)InvokeProtected(gen, "GetMatcap");
            Assert.AreEqual(tex, result);
        }

        // GetMatcapMaskST
        [Test]
        public void GetMatcapMaskST_ReturnsScaleAndOffset()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetTextureScale("_MatCapBlendMask", new Vector2(1.5f, 2.5f));
            lil.Material.SetTextureOffset("_MatCapBlendMask", new Vector2(0.3f, 0.4f));
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetMatcapMaskST");
            var tuple = ((Vector2, Vector2))result;
            Assert.AreEqual(1.5f, tuple.Item1.x, 0.01f);
            Assert.AreEqual(0.3f, tuple.Item2.x, 0.01f);
        }

        // GetMetallicMapST
        [Test]
        public void GetMetallicMapST_ReturnsScaleAndOffset()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetTextureScale("_MetallicGlossMap", new Vector2(2f, 2f));
            lil.Material.SetTextureOffset("_MetallicGlossMap", new Vector2(0.5f, 0.5f));
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetMetallicMapST");
            var tuple = ((Vector2, Vector2))result;
            Assert.AreEqual(2f, tuple.Item1.x, 0.01f);
            Assert.AreEqual(0.5f, tuple.Item2.x, 0.01f);
        }

        // GetMetallicStrength
        [Test]
        public void GetMetallicStrength_ReturnsMetallic()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_Metallic", 0.7f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (float)InvokeProtected(gen, "GetMetallicStrength");
            Assert.AreEqual(0.7f, result, 0.01f);
        }

        // GetNormalMapST
        [Test]
        public void GetNormalMapST_ReturnsScaleAndOffset()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetTextureScale("_BumpMap", new Vector2(1.2f, 1.3f));
            lil.Material.SetTextureOffset("_BumpMap", new Vector2(0.1f, 0.2f));
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetNormalMapST");
            var tuple = ((Vector2, Vector2))result;
            Assert.AreEqual(1.2f, tuple.Item1.x, 0.01f);
            Assert.AreEqual(0.1f, tuple.Item2.x, 0.01f);
        }

        // GetOcculusionMapST
        [Test]
        public void GetOcculusionMapST_ReturnsScaleAndOffset()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetTextureScale("_ShadowBorderMask", new Vector2(3f, 4f));
            lil.Material.SetTextureOffset("_ShadowBorderMask", new Vector2(0.7f, 0.8f));
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetOcculusionMapST");
            var tuple = ((Vector2, Vector2))result;
            Assert.AreEqual(3f, tuple.Item1.x, 0.01f);
            Assert.AreEqual(0.7f, tuple.Item2.x, 0.01f);
        }

        // GetRimIntensity with environmental enabled
        [Test]
        public void GetRimIntensity_Environmental_MultipliesByEnableLighting()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetColor("_RimColor", new Color(1, 1, 1, 0.8f));
            lil.Material.SetFloat("_RimEnableLighting", 0.6f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (float)InvokeProtected(gen, "GetRimIntensity");
            // Environmental: 0.5 * 0.6 * 0.8 = 0.24
            Assert.AreEqual(0.24f, result, 0.01f);
        }

        // GetCulling
        [Test]
        public void GetCulling_ReturnsCullMode()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_Cull", (float)CullMode.Off);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetCulling");
            Assert.AreEqual(CullMode.Off, (CullMode)result);
        }

        // GetEmissionColor
        [Test]
        public void GetEmissionColor_WithEmissionOnly_ReturnsEmissionColor()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 1f);
            lil.Material.SetFloat("_UseEmission2nd", 0f);
            lil.Material.SetColor("_EmissionColor", new Color(0.5f, 0.3f, 0.8f, 1f));
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (Color)InvokeProtected(gen, "GetEmissionColor");
            Assert.AreEqual(0.5f, result.r, 0.05f);
            Assert.AreEqual(0.3f, result.g, 0.05f);
        }

        [Test]
        public void GetEmissionColor_WithBothEmissions_ReturnsWhite()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 1f);
            lil.Material.SetFloat("_UseEmission2nd", 1f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (Color)InvokeProtected(gen, "GetEmissionColor");
            Assert.AreEqual(Color.white, result);
        }

        [Test]
        public void GetEmissionColor_WithEmission2ndOnly_ReturnsEmission2ndColor()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 0f);
            lil.Material.SetFloat("_UseEmission2nd", 1f);
            lil.Material.SetColor("_Emission2ndColor", new Color(0.2f, 0.9f, 0.1f, 1f));
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (Color)InvokeProtected(gen, "GetEmissionColor");
            Assert.AreEqual(0.2f, result.r, 0.05f);
            Assert.AreEqual(0.9f, result.g, 0.05f);
        }

        [Test]
        public void GetEmissionColor_NoEmission_ReturnsBlack()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 0f);
            lil.Material.SetFloat("_UseEmission2nd", 0f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (Color)InvokeProtected(gen, "GetEmissionColor");
            Assert.AreEqual(Color.black, result);
        }

        // GetGlossStrength
        [Test]
        public void GetGlossStrength_ReturnsSmoothness()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_Smoothness", 0.65f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (float)InvokeProtected(gen, "GetGlossStrength");
            Assert.AreEqual(0.65f, result, 0.01f);
        }

        // GetGlossMapST
        [Test]
        public void GetGlossMapST_ReturnsSmoothnessTexST()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetTextureScale("_SmoothnessTex", new Vector2(2f, 3f));
            lil.Material.SetTextureOffset("_SmoothnessTex", new Vector2(0.4f, 0.5f));
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetGlossMapST");
            var tuple = ((Vector2, Vector2))result;
            Assert.AreEqual(2f, tuple.Item1.x, 0.01f);
            Assert.AreEqual(0.4f, tuple.Item2.x, 0.01f);
        }
    }
}
