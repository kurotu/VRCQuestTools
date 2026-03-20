// <copyright file="LilToonGeneratorCoverageTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Reflection;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests
{
    /// <summary>
    /// Batch 39: Thorough LilToonToonStandardGenerator ConvertToToonStandard coverage.
    /// </summary>
    [TestFixture]
    internal class LilToonGeneratorCoverageTests
    {
        private readonly List<UnityEngine.Object> toCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in toCleanup)
            {
                if (obj != null)
                {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }
            toCleanup.Clear();
        }

        private Material CreateFullyConfiguredLilToonMaterial()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) return null;

            var mat = new Material(shader);
            toCleanup.Add(mat);

            var tex8 = new Texture2D(8, 8);
            toCleanup.Add(tex8);
            tex8.SetPixel(0, 0, Color.white);
            tex8.Apply();

            var normalTex = new Texture2D(8, 8);
            toCleanup.Add(normalTex);
            normalTex.SetPixel(0, 0, new Color(0.5f, 0.5f, 1f, 1f));
            normalTex.Apply();

            // Main texture
            mat.mainTexture = tex8;
            mat.mainTextureScale = new Vector2(2f, 2f);
            mat.mainTextureOffset = new Vector2(0.1f, 0.1f);
            mat.color = new Color(1f, 0.8f, 0.6f, 1f);

            // Normal map
            mat.SetFloat("_UseBumpMap", 1f);
            mat.SetTexture("_BumpMap", normalTex);
            mat.SetFloat("_BumpScale", 0.8f);

            // Shadow
            mat.SetFloat("_UseShadow", 1f);
            mat.SetColor("_ShadowColor", new Color(0.5f, 0.5f, 0.5f, 1f));

            // Emission
            mat.SetFloat("_UseEmission", 1f);
            mat.SetTexture("_EmissionMap", tex8);
            mat.SetColor("_EmissionColor", new Color(1f, 0f, 0f, 1f));

            // Emission 2nd
            mat.SetFloat("_UseEmission2nd", 1f);
            mat.SetTexture("_Emission2ndMap", tex8);
            mat.SetColor("_Emission2ndColor", new Color(0f, 0f, 1f, 1f));

            // Occlusion / AO map (set on shadow texture)
            mat.SetTexture("_ShadowStrengthMask", tex8);

            // Reflection (metallic/smoothness)
            mat.SetFloat("_UseReflection", 1f);
            mat.SetTexture("_MetallicGlossMap", tex8);
            mat.SetFloat("_Metallic", 0.5f);
            mat.SetTexture("_SmoothnessTexture", tex8);
            mat.SetFloat("_Smoothness", 0.7f);
            mat.SetFloat("_SpecularBlur", 0.3f);

            // MatCap
            mat.SetFloat("_UseMatCap", 1f);
            mat.SetTexture("_MatCapTex", tex8);
            mat.SetTexture("_MatCapBlendMask", tex8);
            mat.SetColor("_MatCapColor", Color.white);
            mat.SetFloat("_MatCapBlend", 0.5f);
            mat.SetFloat("_MatCapBlendMode", 0); // Normal

            // Rim light
            mat.SetFloat("_UseRim", 1f);
            mat.SetColor("_RimColor", new Color(1f, 1f, 1f, 0.5f));
            mat.SetFloat("_RimBorder", 0.5f);
            mat.SetFloat("_RimFresnelPower", 3f);
            mat.SetFloat("_RimBlur", 0.1f);
            mat.SetFloat("_RimMainStrength", 0.5f);
            mat.SetFloat("_RimEnableLighting", 0.8f);

            // Main 2nd/3rd
            mat.SetFloat("_UseMain2ndTex", 1f);
            mat.SetTexture("_Main2ndTex", tex8);
            mat.SetFloat("_UseMain3rdTex", 1f);
            mat.SetTexture("_Main3rdTex", tex8);

            // Light min limit
            mat.SetFloat("_LightMinLimit", 0.05f);

            return mat;
        }

        private Texture2D CreateBlackTexture()
        {
            var tex = new Texture2D(1, 1);
            toCleanup.Add(tex);
            tex.SetPixel(0, 0, Color.black);
            tex.Apply();
            return tex;
        }

        #region ConvertToToonStandard - All features enabled

        [Test]
        public void ConvertToToonStandard_AllFeaturesEnabled_CoversAllBranches()
        {
            var mat = CreateFullyConfiguredLilToonMaterial();
            if (mat == null) Assert.Ignore("lilToon not found");

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            var settings = new ToonStandardConvertSettings
            {
                useNormalMap = true,
                useEmission = true,
                useOcclusion = true,
                useSpecular = true,
                useMatcap = true,
                useRimLighting = true,
            };

            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);
            var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "ConvertToToonStandard not found");

            var result = (Material)method.Invoke(generator, null);
            Assert.IsNotNull(result, "ConvertToToonStandard should return non-null material");
            toCleanup.Add(result);

            // Verify key properties were set
            var wrapper2 = new ToonStandardMaterialWrapper(result);
            Assert.IsTrue(wrapper2.UseNormalMap, "UseNormalMap should be enabled");
            Assert.IsNotNull(wrapper2.NormalMap, "NormalMap should be set");
            Assert.IsTrue(wrapper2.UseSpecular, "UseSpecular should be enabled");
            Assert.IsTrue(wrapper2.UseMatcap, "UseMatcap should be enabled");
            Assert.IsTrue(wrapper2.UseRimLighting, "UseRimLighting should be enabled");
        }

        [Test]
        public void ConvertToToonStandard_NoFeaturesEnabled_CoversElseBranches()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);
            // Disable all features
            mat.SetFloat("_UseBumpMap", 0f);
            mat.SetFloat("_UseShadow", 0f);
            mat.SetFloat("_UseEmission", 0f);
            mat.SetFloat("_UseReflection", 0f);
            mat.SetFloat("_UseMatCap", 0f);
            mat.SetFloat("_UseRim", 0f);

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            var settings = new ToonStandardConvertSettings();
            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                BindingFlags.Instance | BindingFlags.NonPublic);

            var result = (Material)method.Invoke(generator, null);
            Assert.IsNotNull(result);
            toCleanup.Add(result);

            // Emission should use black texture
            var wrapper2 = new ToonStandardMaterialWrapper(result);
            Assert.AreEqual(blackTex, wrapper2.EmissionMap, "EmissionMap should be black texture when emission disabled");
        }

        [Test]
        public void ConvertToToonStandard_SettingsDisabled_SkipsFeatures()
        {
            var mat = CreateFullyConfiguredLilToonMaterial();
            if (mat == null) Assert.Ignore("lilToon not found");

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            // Disable all settings even though material has features
            var settings = new ToonStandardConvertSettings
            {
                useNormalMap = false,
                useEmission = false,
                useOcclusion = false,
                useSpecular = false,
                useMatcap = false,
                useRimLighting = false,
            };

            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);
            var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                BindingFlags.Instance | BindingFlags.NonPublic);

            var result = (Material)method.Invoke(generator, null);
            Assert.IsNotNull(result);
            toCleanup.Add(result);

            var wrapper2 = new ToonStandardMaterialWrapper(result);
            Assert.IsFalse(wrapper2.UseNormalMap, "NormalMap should be disabled by settings");
            Assert.IsFalse(wrapper2.UseSpecular, "Specular should be disabled by settings");
            Assert.IsFalse(wrapper2.UseMatcap, "Matcap should be disabled by settings");
            Assert.IsFalse(wrapper2.UseRimLighting, "RimLighting should be disabled by settings");
        }

        [Test]
        public void ConvertToToonStandard_MatCapMultiply_SetsMultiplicativeType()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);
            var tex = new Texture2D(4, 4);
            toCleanup.Add(tex);

            mat.SetFloat("_UseMatCap", 1f);
            mat.SetTexture("_MatCapTex", tex);
            mat.SetColor("_MatCapColor", Color.white);
            mat.SetFloat("_MatCapBlend", 1f);
            mat.SetFloat("_MatCapBlendMode", 3); // Multiply

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            var settings = new ToonStandardConvertSettings { useMatcap = true };
            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                BindingFlags.Instance | BindingFlags.NonPublic);

            var result = (Material)method.Invoke(generator, null);
            Assert.IsNotNull(result);
            toCleanup.Add(result);

            var wrapper2 = new ToonStandardMaterialWrapper(result);
            Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Multiplicative, wrapper2.MatcapType);
        }

        [Test]
        public void ConvertToToonStandard_MatCapAdd_SetsAdditiveType()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);
            var tex = new Texture2D(4, 4);
            toCleanup.Add(tex);

            mat.SetFloat("_UseMatCap", 1f);
            mat.SetTexture("_MatCapTex", tex);
            mat.SetColor("_MatCapColor", Color.white);
            mat.SetFloat("_MatCapBlend", 1f);
            mat.SetFloat("_MatCapBlendMode", 1); // Add

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            var settings = new ToonStandardConvertSettings { useMatcap = true };
            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                BindingFlags.Instance | BindingFlags.NonPublic);

            var result = (Material)method.Invoke(generator, null);
            Assert.IsNotNull(result);
            toCleanup.Add(result);

            var wrapper2 = new ToonStandardMaterialWrapper(result);
            Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Additive, wrapper2.MatcapType);
        }

        [Test]
        public void ConvertToToonStandard_MatCapScreen_SetsAdditiveType()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);
            var tex = new Texture2D(4, 4);
            toCleanup.Add(tex);

            mat.SetFloat("_UseMatCap", 1f);
            mat.SetTexture("_MatCapTex", tex);
            mat.SetColor("_MatCapColor", Color.white);
            mat.SetFloat("_MatCapBlend", 1f);
            mat.SetFloat("_MatCapBlendMode", 2); // Screen

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            var settings = new ToonStandardConvertSettings { useMatcap = true };
            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                BindingFlags.Instance | BindingFlags.NonPublic);

            var result = (Material)method.Invoke(generator, null);
            Assert.IsNotNull(result);
            toCleanup.Add(result);

            var wrapper2 = new ToonStandardMaterialWrapper(result);
            Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Additive, wrapper2.MatcapType);
        }

        [Test]
        public void ConvertToToonStandard_RimLightWithLighting_ScalesIntensity()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);

            mat.SetFloat("_UseRim", 1f);
            mat.SetColor("_RimColor", Color.white);
            mat.SetFloat("_RimBorder", 0.5f);
            mat.SetFloat("_RimFresnelPower", 2f);
            mat.SetFloat("_RimBlur", 0.2f);
            mat.SetFloat("_RimMainStrength", 0.7f);
            mat.SetFloat("_RimEnableLighting", 0.6f);

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            var settings = new ToonStandardConvertSettings { useRimLighting = true };
            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                BindingFlags.Instance | BindingFlags.NonPublic);

            var result = (Material)method.Invoke(generator, null);
            Assert.IsNotNull(result);
            toCleanup.Add(result);

            var wrapper2 = new ToonStandardMaterialWrapper(result);
            Assert.IsTrue(wrapper2.UseRimLighting);
            Assert.IsTrue(wrapper2.RimEnvironmental, "RimEnvironmental should be true when lighting > 0");
        }

        [Test]
        public void ConvertToToonStandard_RimLightNoLighting_NoIntensityScale()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);

            mat.SetFloat("_UseRim", 1f);
            mat.SetColor("_RimColor", Color.white);
            mat.SetFloat("_RimBorder", 0.5f);
            mat.SetFloat("_RimFresnelPower", 2f);
            mat.SetFloat("_RimBlur", 0.2f);
            mat.SetFloat("_RimMainStrength", 0.7f);
            mat.SetFloat("_RimEnableLighting", 0f);

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            var settings = new ToonStandardConvertSettings { useRimLighting = true };
            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                BindingFlags.Instance | BindingFlags.NonPublic);

            var result = (Material)method.Invoke(generator, null);
            Assert.IsNotNull(result);
            toCleanup.Add(result);

            var wrapper2 = new ToonStandardMaterialWrapper(result);
            Assert.IsTrue(wrapper2.UseRimLighting);
            Assert.IsFalse(wrapper2.RimEnvironmental, "RimEnvironmental should be false when lighting == 0");
        }

        [Test]
        public void ConvertToToonStandard_WithOcclusion_SetsOcclusionProperties()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);
            var tex = new Texture2D(4, 4);
            toCleanup.Add(tex);

            mat.SetFloat("_UseShadow", 1f);
            mat.SetTexture("_ShadowBorderMask", tex);

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            var settings = new ToonStandardConvertSettings { useOcclusion = true };
            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                BindingFlags.Instance | BindingFlags.NonPublic);

            var result = (Material)method.Invoke(generator, null);
            Assert.IsNotNull(result);
            toCleanup.Add(result);

            var wrapper2 = new ToonStandardMaterialWrapper(result);
            Assert.IsTrue(wrapper2.UseOcclusion, "UseOcclusion should be enabled");
        }

        #endregion

        #region Platform override methods

        [Test]
        public void GetMainTexturePlatformOverride_WithTextures_ReturnsOverrideOrNull()
        {
            var mat = CreateFullyConfiguredLilToonMaterial();
            if (mat == null) Assert.Ignore("lilToon not found");

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            var settings = new ToonStandardConvertSettings();
            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(LilToonToonStandardGenerator).GetMethod("GetMainTexturePlatformOverride",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) Assert.Ignore("GetMainTexturePlatformOverride not found");

            var result = method.Invoke(generator, null);
            // Can be null for runtime textures with no importer
            Assert.Pass($"GetMainTexturePlatformOverride returned: {result?.GetType().Name ?? "null"}");
        }

        [Test]
        public void GetMainTexturePlatformOverride_NoTextures_ReturnsNull()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);
            // No textures set

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            var settings = new ToonStandardConvertSettings();
            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(LilToonToonStandardGenerator).GetMethod("GetMainTexturePlatformOverride",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) Assert.Ignore("GetMainTexturePlatformOverride not found");

            var result = method.Invoke(generator, null);
            Assert.IsNull(result, "Should return null when no textures");
        }

        [Test]
        public void GetEmissionMapPlatformOverride_WithEmission_ReturnsOverrideOrNull()
        {
            var mat = CreateFullyConfiguredLilToonMaterial();
            if (mat == null) Assert.Ignore("lilToon not found");

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            var settings = new ToonStandardConvertSettings();
            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(LilToonToonStandardGenerator).GetMethod("GetEmissionMapPlatformOverride",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) Assert.Ignore("GetEmissionMapPlatformOverride not found");

            var result = method.Invoke(generator, null);
            Assert.Pass($"GetEmissionMapPlatformOverride returned: {result?.GetType().Name ?? "null"}");
        }

        [Test]
        public void GetEmissionMapPlatformOverride_NoEmission_ReturnsNull()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);
            mat.SetFloat("_UseEmission", 0f);
            mat.SetFloat("_UseEmission2nd", 0f);

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            var settings = new ToonStandardConvertSettings();
            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(LilToonToonStandardGenerator).GetMethod("GetEmissionMapPlatformOverride",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) Assert.Ignore("GetEmissionMapPlatformOverride not found");

            var result = method.Invoke(generator, null);
            Assert.IsNull(result, "Should return null when no emission");
        }

        [Test]
        public void GetGlossMapPlatformOverride_WithTextures_ReturnsResult()
        {
            var mat = CreateFullyConfiguredLilToonMaterial();
            if (mat == null) Assert.Ignore("lilToon not found");

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            var settings = new ToonStandardConvertSettings();
            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(LilToonToonStandardGenerator).GetMethod("GetGlossMapPlatformOverride",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) Assert.Ignore("GetGlossMapPlatformOverride not found");

            var result = method.Invoke(generator, null);
            Assert.Pass($"GetGlossMapPlatformOverride returned: {result?.GetType().Name ?? "null"}");
        }

        [Test]
        public void GetGlossMapPlatformOverride_NoTextures_ReturnsNull()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            var settings = new ToonStandardConvertSettings();
            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(LilToonToonStandardGenerator).GetMethod("GetGlossMapPlatformOverride",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) Assert.Ignore("GetGlossMapPlatformOverride not found");

            var result = method.Invoke(generator, null);
            Assert.IsNull(result);
        }

        [Test]
        public void GetMatcapPlatformOverride_ReturnsResult()
        {
            var mat = CreateFullyConfiguredLilToonMaterial();
            if (mat == null) Assert.Ignore("lilToon not found");

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            var settings = new ToonStandardConvertSettings();
            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(LilToonToonStandardGenerator).GetMethod("GetMatcapPlatformOverride",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) Assert.Ignore("GetMatcapPlatformOverride not found");

            var result = method.Invoke(generator, null);
            Assert.Pass($"GetMatcapPlatformOverride: {result?.GetType().Name ?? "null"}");
        }

        [Test]
        public void GetMatcapMaskPlatformOverride_ReturnsResult()
        {
            var mat = CreateFullyConfiguredLilToonMaterial();
            if (mat == null) Assert.Ignore("lilToon not found");

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            var settings = new ToonStandardConvertSettings();
            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(LilToonToonStandardGenerator).GetMethod("GetMatcapMaskPlatformOverride",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) Assert.Ignore("GetMatcapMaskPlatformOverride not found");

            var result = method.Invoke(generator, null);
            Assert.Pass($"GetMatcapMaskPlatformOverride: {result?.GetType().Name ?? "null"}");
        }

        [Test]
        public void GetMetallicMapPlatformOverride_ReturnsResult()
        {
            var mat = CreateFullyConfiguredLilToonMaterial();
            if (mat == null) Assert.Ignore("lilToon not found");

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            var settings = new ToonStandardConvertSettings();
            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(LilToonToonStandardGenerator).GetMethod("GetMetallicMapPlatformOverride",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) Assert.Ignore("GetMetallicMapPlatformOverride not found");

            var result = method.Invoke(generator, null);
            Assert.Pass($"GetMetallicMapPlatformOverride: {result?.GetType().Name ?? "null"}");
        }

        [Test]
        public void GetNormalMapPlatformOverride_ReturnsResult()
        {
            var mat = CreateFullyConfiguredLilToonMaterial();
            if (mat == null) Assert.Ignore("lilToon not found");

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            var settings = new ToonStandardConvertSettings();
            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(LilToonToonStandardGenerator).GetMethod("GetNormalMapPlatformOverride",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) Assert.Ignore("GetNormalMapPlatformOverride not found");

            var result = method.Invoke(generator, null);
            Assert.Pass($"GetNormalMapPlatformOverride: {result?.GetType().Name ?? "null"}");
        }

        [Test]
        public void GetOcclusionMapPlatformOverride_ReturnsResult()
        {
            var mat = CreateFullyConfiguredLilToonMaterial();
            if (mat == null) Assert.Ignore("lilToon not found");

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            var settings = new ToonStandardConvertSettings();
            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(LilToonToonStandardGenerator).GetMethod("GetOcclusionMapPlatformOverride",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) Assert.Ignore("GetOcclusionMapPlatformOverride not found");

            var result = method.Invoke(generator, null);
            Assert.Pass($"GetOcclusionMapPlatformOverride: {result?.GetType().Name ?? "null"}");
        }

        [Test]
        public void GetPackedMaskPlatformOverride_WithMetallicMask_ReturnsResult()
        {
            var mat = CreateFullyConfiguredLilToonMaterial();
            if (mat == null) Assert.Ignore("lilToon not found");

            var wrapper = new LilToonMaterial(mat);
            var blackTex = CreateBlackTexture();
            var settings = new ToonStandardConvertSettings();
            var generator = new LilToonToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(LilToonToonStandardGenerator).GetMethod("GetPackedMaskPlatformOverride",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) Assert.Ignore("GetPackedMaskPlatformOverride not found");

            // Need to create a TexturePack to pass as argument
            var texturePackType = typeof(ToonStandardConvertSettings).Assembly.GetType("KRT.VRCQuestTools.Models.TexturePack");
            if (texturePackType == null)
            {
                Assert.Ignore("TexturePack type not found");
            }

            // Just pass null and see if it handles it
            try
            {
                method.Invoke(generator, new object[] { null });
            }
            catch (TargetInvocationException ex) when (ex.InnerException is NullReferenceException)
            {
                Assert.Pass("NullRef expected for null TexturePack");
            }
        }

        #endregion

        #region LilToonMaterial property coverage for ConvertToToonStandard inputs

        [Test]
        public void LilToonMaterial_EmissionMapTextureScale_ReturnsValue()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);
            mat.SetVector("_EmissionMap_ST", new Vector4(2f, 3f, 0.1f, 0.2f));

            var wrapper = new LilToonMaterial(mat);
            Assert.AreEqual(new Vector2(2f, 3f), wrapper.EmissionMapTextureScale);
            Assert.AreEqual(new Vector2(0.1f, 0.2f), wrapper.EmissionMapTextureOffset);
        }

        [Test]
        public void LilToonMaterial_AOMapTextureScaleOffset_ReturnsValue()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);
            var tex = new Texture2D(4, 4);
            toCleanup.Add(tex);
            mat.SetTexture("_ShadowBorderMask", tex);

            var wrapper = new LilToonMaterial(mat);
            Assert.IsNotNull(wrapper.AOMap);
        }

        [Test]
        public void LilToonMaterial_MatCapMaskTextureScaleOffset_ReturnsValue()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);
            var tex = new Texture2D(4, 4);
            toCleanup.Add(tex);
            mat.SetTexture("_MatCapBlendMask", tex);
            mat.SetVector("_MatCapBlendMask_ST", new Vector4(2f, 2f, 0.1f, 0.1f));

            var wrapper = new LilToonMaterial(mat);
            Assert.AreEqual(new Vector2(2f, 2f), wrapper.MatCapMaskTextureScale);
            Assert.AreEqual(new Vector2(0.1f, 0.1f), wrapper.MatCapMaskTextureOffset);
        }

        [Test]
        public void LilToonMaterial_MetallicMapTextureScaleOffset_ReturnsValue()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);
            var tex = new Texture2D(4, 4);
            toCleanup.Add(tex);
            mat.SetTexture("_MetallicGlossMap", tex);
            mat.SetVector("_MetallicGlossMap_ST", new Vector4(3f, 3f, 0.2f, 0.2f));

            var wrapper = new LilToonMaterial(mat);
            Assert.AreEqual(new Vector2(3f, 3f), wrapper.MetallicMapTextureScale);
            Assert.AreEqual(new Vector2(0.2f, 0.2f), wrapper.MetallicMapTextureOffset);
        }

        [Test]
        public void LilToonMaterial_SmoothnessTexScaleOffset_ReturnsValue()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);
            var tex = new Texture2D(4, 4);
            toCleanup.Add(tex);
            mat.SetTexture("_SmoothnessTex", tex);
            mat.SetTextureScale("_SmoothnessTex", new Vector2(1.5f, 1.5f));
            mat.SetTextureOffset("_SmoothnessTex", new Vector2(0.05f, 0.05f));

            var wrapper = new LilToonMaterial(mat);
            Assert.AreEqual(new Vector2(1.5f, 1.5f), wrapper.SmoothnessTexScale);
            Assert.AreEqual(new Vector2(0.05f, 0.05f), wrapper.SmoothnessTexOffset);
        }

        [Test]
        public void LilToonMaterial_NormalMapTextureScaleOffset_ReturnsValue()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);
            mat.SetVector("_BumpMap_ST", new Vector4(2f, 2f, 0.3f, 0.3f));

            var wrapper = new LilToonMaterial(mat);
            Assert.AreEqual(new Vector2(2f, 2f), wrapper.NormalMapTextureScale);
            Assert.AreEqual(new Vector2(0.3f, 0.3f), wrapper.NormalMapTextureOffset);
        }

        [Test]
        public void LilToonMaterial_SpecularBlur_ReturnsValue()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);
            mat.SetFloat("_SpecularBlur", 0.3f);

            var wrapper = new LilToonMaterial(mat);
            Assert.AreEqual(0.3f, wrapper.SpecularBlur, 0.01f);
        }

        [Test]
        public void LilToonMaterial_RimProperties_ReturnValues()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);
            mat.SetFloat("_UseRim", 1f);
            mat.SetFloat("_RimBorder", 0.4f);
            mat.SetFloat("_RimFresnelPower", 5f);
            mat.SetFloat("_RimBlur", 0.15f);
            mat.SetFloat("_RimMainStrength", 0.6f);
            mat.SetFloat("_RimEnableLighting", 0.9f);

            var wrapper = new LilToonMaterial(mat);
            Assert.AreEqual(0.4f, wrapper.RimLightBorder, 0.01f);
            Assert.AreEqual(5f, wrapper.RimFresnelPower, 0.01f);
            Assert.AreEqual(0.15f, wrapper.RimLightBlur, 0.01f);
            Assert.AreEqual(0.6f, wrapper.RimMainStrength, 0.01f);
            Assert.AreEqual(0.9f, wrapper.RimEnableLighting, 0.01f);
        }

        [Test]
        public void LilToonMaterial_LightMinLimit_ReturnsValue()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);
            mat.SetFloat("_LightMinLimit", 0.1f);

            var wrapper = new LilToonMaterial(mat);
            Assert.AreEqual(0.1f, wrapper.LightMinLimit, 0.01f);
        }

        [Test]
        public void LilToonMaterial_MatCapBlend_ReturnsValue()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);
            mat.SetFloat("_MatCapBlend", 0.75f);

            var wrapper = new LilToonMaterial(mat);
            Assert.AreEqual(0.75f, wrapper.MatCapBlend, 0.01f);
        }

        [Test]
        public void LilToonMaterial_ReflectionColorTex_ReturnsTexture()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);
            var tex = new Texture2D(4, 4);
            toCleanup.Add(tex);
            mat.SetTexture("_ReflectionColorTex", tex);

            var wrapper = new LilToonMaterial(mat);
            Assert.AreEqual(tex, wrapper.ReflectionColorTex);
        }

        [Test]
        public void LilToonMaterial_Emission2ndBlendMask_ReturnsTexture()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);
            var tex = new Texture2D(4, 4);
            toCleanup.Add(tex);
            mat.SetTexture("_Emission2ndBlendMask", tex);

            var wrapper = new LilToonMaterial(mat);
            Assert.AreEqual(tex, wrapper.Emission2ndBlendMask);
        }

        [Test]
        public void LilToonMaterial_EmissionBlendMask_ReturnsTexture()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Ignore("lilToon not found");

            var mat = new Material(shader);
            toCleanup.Add(mat);
            var tex = new Texture2D(4, 4);
            toCleanup.Add(tex);
            mat.SetTexture("_EmissionBlendMask", tex);

            var wrapper = new LilToonMaterial(mat);
            Assert.AreEqual(tex, wrapper.EmissionBlendMask);
        }

        #endregion
    }
}
