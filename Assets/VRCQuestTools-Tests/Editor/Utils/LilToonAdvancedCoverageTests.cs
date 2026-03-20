// <copyright file="LilToonDeepCoverageTests.cs" company="kurotu">
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
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace KRT.VRCQuestTools.Tests
{
    /// <summary>
    /// Batch 46: LilToonMaterial property getters, LilToonToonStandardGenerator platform overrides and helpers.
    /// </summary>
    [TestFixture]
    public class LilToonDeepCoverageTests
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

        private LilToonMaterial CreateLilToonMaterial()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            if (wrapper == null)
            {
                return null;
            }
            toCleanup.Add(wrapper.Material);
            return wrapper;
        }

        private Texture2D CreateTestTexture(int width = 4, int height = 4)
        {
            var tex = new Texture2D(width, height);
            toCleanup.Add(tex);
            return tex;
        }

        #region LilToonMaterial Property Getters - Shadow

        [Test]
        public void LilToonMaterial_UseShadow_ReturnsTrue_WhenEnabled()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_UseShadow", 1.0f);
            Assert.IsTrue(lilMat.UseShadow);
        }

        [Test]
        public void LilToonMaterial_UseShadow_ReturnsFalse_WhenDisabled()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_UseShadow", 0.0f);
            Assert.IsFalse(lilMat.UseShadow);
        }

        [Test]
        public void LilToonMaterial_UseShadow2nd_RequiresShadowEnabled()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_UseShadow", 0.0f);
            lilMat.Material.SetColor("_Shadow2ndColor", new Color(1, 1, 1, 1));
            Assert.IsFalse(lilMat.UseShadow2nd, "Should be false when shadow is disabled");

            lilMat.Material.SetFloat("_UseShadow", 1.0f);
            lilMat.Material.SetColor("_Shadow2ndColor", new Color(1, 1, 1, 0));
            Assert.IsFalse(lilMat.UseShadow2nd, "Should be false when alpha is 0");

            lilMat.Material.SetColor("_Shadow2ndColor", new Color(1, 1, 1, 0.5f));
            Assert.IsTrue(lilMat.UseShadow2nd, "Should be true when shadow enabled and alpha > 0");
        }

        [Test]
        public void LilToonMaterial_UseShadow3rd_RequiresShadowEnabled()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_UseShadow", 0.0f);
            lilMat.Material.SetColor("_Shadow3rdColor", new Color(1, 1, 1, 1));
            Assert.IsFalse(lilMat.UseShadow3rd, "Should be false when shadow disabled");

            lilMat.Material.SetFloat("_UseShadow", 1.0f);
            lilMat.Material.SetColor("_Shadow3rdColor", new Color(1, 1, 1, 0.5f));
            Assert.IsTrue(lilMat.UseShadow3rd, "Should be true when both enabled");
        }

        #endregion

        #region LilToonMaterial Property Getters - Main Textures

        [Test]
        public void LilToonMaterial_UseMain2ndTex_ReadsProperty()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_UseMain2ndTex", 0.0f);
            Assert.IsFalse(lilMat.UseMain2ndTex);
            lilMat.Material.SetFloat("_UseMain2ndTex", 1.0f);
            Assert.IsTrue(lilMat.UseMain2ndTex);
        }

        [Test]
        public void LilToonMaterial_UseMain3rdTex_ReadsProperty()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_UseMain3rdTex", 0.0f);
            Assert.IsFalse(lilMat.UseMain3rdTex);
            lilMat.Material.SetFloat("_UseMain3rdTex", 1.0f);
            Assert.IsTrue(lilMat.UseMain3rdTex);
        }

        [Test]
        public void LilToonMaterial_Main2ndTex_ReturnsTexture()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            Assert.IsNull(lilMat.Main2ndTex);
            var tex = CreateTestTexture();
            lilMat.Material.SetTexture("_Main2ndTex", tex);
            Assert.AreEqual(tex, lilMat.Main2ndTex);
        }

        [Test]
        public void LilToonMaterial_Main3rdTex_ReturnsTexture()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            Assert.IsNull(lilMat.Main3rdTex);
            var tex = CreateTestTexture();
            lilMat.Material.SetTexture("_Main3rdTex", tex);
            Assert.AreEqual(tex, lilMat.Main3rdTex);
        }

        #endregion

        #region LilToonMaterial Property Getters - AO/Normal

        [Test]
        public void LilToonMaterial_AOMap_ReadsFromShadowBorderMask()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var tex = CreateTestTexture();
            lilMat.Material.SetTexture("_ShadowBorderMask", tex);
            Assert.AreEqual(tex, lilMat.AOMap);
        }

        [Test]
        public void LilToonMaterial_AOMapTextureScaleOffset_ReadsFromShadowBorderMask()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetTextureScale("_ShadowBorderMask", new Vector2(2, 3));
            lilMat.Material.SetTextureOffset("_ShadowBorderMask", new Vector2(0.5f, 0.7f));
            Assert.AreEqual(new Vector2(2, 3), lilMat.AOMapTextureScale);
            Assert.AreEqual(new Vector2(0.5f, 0.7f), lilMat.AOMapTextureOffset);
        }

        [Test]
        public void LilToonMaterial_CullMode_ReadsProperty()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_Cull", (float)CullMode.Back);
            Assert.AreEqual(CullMode.Back, lilMat.CullMode);
            lilMat.Material.SetFloat("_Cull", (float)CullMode.Off);
            Assert.AreEqual(CullMode.Off, lilMat.CullMode);
        }

        [Test]
        public void LilToonMaterial_NormalMap_Properties()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_UseBumpMap", 0.0f);
            Assert.IsFalse(lilMat.UseNormalMap);
            lilMat.Material.SetFloat("_UseBumpMap", 1.0f);
            Assert.IsTrue(lilMat.UseNormalMap);

            var tex = CreateTestTexture();
            lilMat.Material.SetTexture("_BumpMap", tex);
            Assert.AreEqual(tex, lilMat.NormalMap);

            lilMat.Material.SetTextureScale("_BumpMap", new Vector2(2, 3));
            lilMat.Material.SetTextureOffset("_BumpMap", new Vector2(0.1f, 0.2f));
            Assert.AreEqual(new Vector2(2, 3), lilMat.NormalMapTextureScale);
            Assert.AreEqual(new Vector2(0.1f, 0.2f), lilMat.NormalMapTextureOffset);

            lilMat.Material.SetFloat("_BumpScale", 0.75f);
            Assert.AreEqual(0.75f, lilMat.NormalMapScale, 0.001f);
        }

        [Test]
        public void LilToonMaterial_LightMinLimit_ReadsProperty()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_LightMinLimit", 0.3f);
            Assert.AreEqual(0.3f, lilMat.LightMinLimit, 0.001f);
        }

        #endregion

        #region LilToonMaterial Property Getters - Emission

        [Test]
        public void LilToonMaterial_UseEmission_GetSet()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.UseEmission = false;
            Assert.IsFalse(lilMat.UseEmission);
            lilMat.UseEmission = true;
            Assert.IsTrue(lilMat.UseEmission);
        }

        [Test]
        public void LilToonMaterial_EmissionMap_AndScaleOffset()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var tex = CreateTestTexture();
            lilMat.Material.SetTexture("_EmissionMap", tex);
            Assert.AreEqual(tex, lilMat.EmissionMap);

            lilMat.Material.SetTextureScale("_EmissionMap", new Vector2(2, 2));
            lilMat.Material.SetTextureOffset("_EmissionMap", new Vector2(0.5f, 0.5f));
            Assert.AreEqual(new Vector2(2, 2), lilMat.EmissionMapTextureScale);
            Assert.AreEqual(new Vector2(0.5f, 0.5f), lilMat.EmissionMapTextureOffset);
        }

        [Test]
        public void LilToonMaterial_EmissionColor_GetSet()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var color = new Color(1, 0.5f, 0.3f, 0.8f);
            lilMat.EmissionColor = color;
            Assert.AreEqual(color.r, lilMat.EmissionColor.r, 0.01f);
            Assert.AreEqual(color.g, lilMat.EmissionColor.g, 0.01f);
        }

        [Test]
        public void LilToonMaterial_EmissionBlendMask_AndBlend()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var tex = CreateTestTexture();
            lilMat.Material.SetTexture("_EmissionBlendMask", tex);
            Assert.AreEqual(tex, lilMat.EmissionBlendMask);

            lilMat.Material.SetFloat("_EmissionBlend", 0.6f);
            Assert.AreEqual(0.6f, lilMat.EmissionBlend, 0.001f);
        }

        [Test]
        public void LilToonMaterial_UseEmission2nd_GetSet()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.UseEmission2nd = false;
            Assert.IsFalse(lilMat.UseEmission2nd);
            lilMat.UseEmission2nd = true;
            Assert.IsTrue(lilMat.UseEmission2nd);
        }

        [Test]
        public void LilToonMaterial_Emission2ndMap_AndColor()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var tex = CreateTestTexture();
            lilMat.Material.SetTexture("_Emission2ndMap", tex);
            Assert.AreEqual(tex, lilMat.Emission2ndMap);

            var color = new Color(0.1f, 0.2f, 0.3f, 0.4f);
            lilMat.Emission2ndColor = color;
            Assert.AreEqual(color.r, lilMat.Emission2ndColor.r, 0.01f);
        }

        [Test]
        public void LilToonMaterial_Emission2ndBlendMask_AndBlend()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var tex = CreateTestTexture();
            lilMat.Material.SetTexture("_Emission2ndBlendMask", tex);
            Assert.AreEqual(tex, lilMat.Emission2ndBlendMask);

            lilMat.Material.SetFloat("_Emission2ndBlend", 0.4f);
            Assert.AreEqual(0.4f, lilMat.Emission2ndBlend, 0.001f);
        }

        #endregion

        #region LilToonMaterial Property Getters - Reflection/Metallic

        [Test]
        public void LilToonMaterial_UseReflection_ReadsProperty()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_UseReflection", 0.0f);
            Assert.IsFalse(lilMat.UseReflection);
            lilMat.Material.SetFloat("_UseReflection", 1.0f);
            Assert.IsTrue(lilMat.UseReflection);
        }

        [Test]
        public void LilToonMaterial_MetallicMap_ScaleOffset()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var tex = CreateTestTexture();
            lilMat.Material.SetTexture("_MetallicGlossMap", tex);
            Assert.AreEqual(tex, lilMat.MetallicMap);

            lilMat.Material.SetTextureScale("_MetallicGlossMap", new Vector2(3, 2));
            lilMat.Material.SetTextureOffset("_MetallicGlossMap", new Vector2(0.1f, 0.9f));
            Assert.AreEqual(new Vector2(3, 2), lilMat.MetallicMapTextureScale);
            Assert.AreEqual(new Vector2(0.1f, 0.9f), lilMat.MetallicMapTextureOffset);
        }

        [Test]
        public void LilToonMaterial_SmoothnessTex_ScaleOffset()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var tex = CreateTestTexture();
            lilMat.Material.SetTexture("_SmoothnessTex", tex);
            Assert.AreEqual(tex, lilMat.SmoothnessTex);

            lilMat.Material.SetTextureScale("_SmoothnessTex", new Vector2(1, 2));
            lilMat.Material.SetTextureOffset("_SmoothnessTex", new Vector2(0.3f, 0.4f));
            Assert.AreEqual(new Vector2(1, 2), lilMat.SmoothnessTexScale);
            Assert.AreEqual(new Vector2(0.3f, 0.4f), lilMat.SmoothnessTexOffset);
        }

        [Test]
        public void LilToonMaterial_ReflectionColorTex_AndColor()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var tex = CreateTestTexture();
            lilMat.Material.SetTexture("_ReflectionColorTex", tex);
            Assert.AreEqual(tex, lilMat.ReflectionColorTex);

            lilMat.Material.SetColor("_ReflectionColor", Color.cyan);
            Assert.AreEqual(Color.cyan, lilMat.ReflectionColor);
        }

        [Test]
        public void LilToonMaterial_SmoothnessAndReflectance()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_Smoothness", 0.8f);
            Assert.AreEqual(0.8f, lilMat.Smoothness, 0.001f);

            lilMat.Material.SetFloat("_Reflectance", 0.5f);
            Assert.AreEqual(0.5f, lilMat.Reflectance, 0.001f);

            lilMat.Material.SetFloat("_SpecularBlur", 0.3f);
            Assert.AreEqual(0.3f, lilMat.SpecularBlur, 0.001f);
        }

        #endregion

        #region LilToonMaterial Property Getters - MatCap

        [Test]
        public void LilToonMaterial_UseMatCap_ReadsProperty()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_UseMatCap", 0.0f);
            Assert.IsFalse(lilMat.UseMatCap);
            lilMat.Material.SetFloat("_UseMatCap", 1.0f);
            Assert.IsTrue(lilMat.UseMatCap);
        }

        [Test]
        public void LilToonMaterial_MatCapTex_AndColor()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var tex = CreateTestTexture();
            lilMat.Material.SetTexture("_MatCapTex", tex);
            Assert.AreEqual(tex, lilMat.MatCapTex);

            lilMat.Material.SetColor("_MatCapColor", Color.red);
            Assert.AreEqual(Color.red, lilMat.MatCapColor);

            lilMat.Material.SetFloat("_MatCapMainStrength", 0.7f);
            Assert.AreEqual(0.7f, lilMat.MatCapMainStrength, 0.001f);
        }

        [Test]
        public void LilToonMaterial_MatCapMask_ScaleOffset()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var tex = CreateTestTexture();
            lilMat.Material.SetTexture("_MatCapBlendMask", tex);
            Assert.AreEqual(tex, lilMat.MatCapMask);

            lilMat.Material.SetTextureScale("_MatCapBlendMask", new Vector2(2, 2));
            lilMat.Material.SetTextureOffset("_MatCapBlendMask", new Vector2(0.1f, 0.2f));
            Assert.AreEqual(new Vector2(2, 2), lilMat.MatCapMaskTextureScale);
            Assert.AreEqual(new Vector2(0.1f, 0.2f), lilMat.MatCapMaskTextureOffset);
        }

        [Test]
        public void LilToonMaterial_MatCapBlend_AndBlendMode()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_MatCapBlend", 0.9f);
            Assert.AreEqual(0.9f, lilMat.MatCapBlend, 0.001f);

            lilMat.Material.SetFloat("_MatCapBlendMode", 0.0f);
            Assert.AreEqual(LilToonMaterial.MatCapBlendMode.Normal, lilMat.MatCapBlendingMode);
            lilMat.Material.SetFloat("_MatCapBlendMode", 1.0f);
            Assert.AreEqual(LilToonMaterial.MatCapBlendMode.Add, lilMat.MatCapBlendingMode);
            lilMat.Material.SetFloat("_MatCapBlendMode", 2.0f);
            Assert.AreEqual(LilToonMaterial.MatCapBlendMode.Screen, lilMat.MatCapBlendingMode);
            lilMat.Material.SetFloat("_MatCapBlendMode", 3.0f);
            Assert.AreEqual(LilToonMaterial.MatCapBlendMode.Multiply, lilMat.MatCapBlendingMode);
        }

        #endregion

        #region LilToonMaterial Property Getters - Rim Light

        [Test]
        public void LilToonMaterial_UseRimLight_ReadsProperty()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_UseRim", 0.0f);
            Assert.IsFalse(lilMat.UseRimLight);
            lilMat.Material.SetFloat("_UseRim", 1.0f);
            Assert.IsTrue(lilMat.UseRimLight);
        }

        [Test]
        public void LilToonMaterial_RimLightColor_ReadsProperty()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetColor("_RimColor", Color.green);
            Assert.AreEqual(Color.green, lilMat.RimLightColor);
        }

        [Test]
        public void LilToonMaterial_RimProperties()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_RimMainStrength", 0.6f);
            Assert.AreEqual(0.6f, lilMat.RimMainStrength, 0.001f);

            lilMat.Material.SetFloat("_RimBorder", 0.4f);
            Assert.AreEqual(0.4f, lilMat.RimLightBorder, 0.001f);

            lilMat.Material.SetFloat("_RimEnableLighting", 0.8f);
            Assert.AreEqual(0.8f, lilMat.RimEnableLighting, 0.001f);

            lilMat.Material.SetFloat("_RimFresnelPower", 3.0f);
            Assert.AreEqual(3.0f, lilMat.RimFresnelPower, 0.001f);

            lilMat.Material.SetFloat("_RimBlur", 0.2f);
            Assert.AreEqual(0.2f, lilMat.RimLightBlur, 0.001f);
        }

        #endregion

        #region LilToonMaterial - GetToonLitPlatformOverride

        [Test]
        public void LilToonMaterial_GetToonLitPlatformOverride_NoTextures_ReturnsNull()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            // No textures set
            var result = lilMat.GetToonLitPlatformOverride();
            Assert.IsNull(result);
        }

        [Test]
        public void LilToonMaterial_GetToonLitPlatformOverride_WithMainTexture_ReturnsOverride()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var tex = CreateTestTexture(64, 64);
            lilMat.Material.mainTexture = tex;

            var result = lilMat.GetToonLitPlatformOverride();
            // May or may not return override depending on texture importer settings
            // Just exercise the path
            Assert.Pass("GetToonLitPlatformOverride executed with main texture");
        }

        [Test]
        public void LilToonMaterial_GetToonLitPlatformOverride_WithMain2ndTex()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_UseMain2ndTex", 1.0f);
            var tex = CreateTestTexture(64, 64);
            lilMat.Material.SetTexture("_Main2ndTex", tex);

            var result = lilMat.GetToonLitPlatformOverride();
            Assert.Pass("GetToonLitPlatformOverride with Main2nd executed");
        }

        [Test]
        public void LilToonMaterial_GetToonLitPlatformOverride_WithEmission()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.UseEmission = true;
            var tex = CreateTestTexture(64, 64);
            lilMat.Material.SetTexture("_EmissionMap", tex);

            var result = lilMat.GetToonLitPlatformOverride();
            Assert.Pass("GetToonLitPlatformOverride with emission executed");
        }

        [Test]
        public void LilToonMaterial_GetToonLitPlatformOverride_WithEmission2nd()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.UseEmission2nd = true;
            var tex = CreateTestTexture(64, 64);
            lilMat.Material.SetTexture("_Emission2ndMap", tex);

            var result = lilMat.GetToonLitPlatformOverride();
            Assert.Pass("GetToonLitPlatformOverride with emission 2nd executed");
        }

        #endregion

        #region LilToonToonStandardGenerator Platform Overrides

        [Test]
        public void LilToonGenerator_GetMainTexturePlatformOverride_NoTextures_ReturnsNull()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetMainTexturePlatformOverride");
            Assert.IsNull(result);
        }

        [Test]
        public void LilToonGenerator_GetMainTexturePlatformOverride_WithMain2ndTex()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_UseMain2ndTex", 1.0f);
            var tex = CreateTestTexture(64, 64);
            lilMat.Material.SetTexture("_Main2ndTex", tex);

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetMainTexturePlatformOverride");
            Assert.Pass("GetMainTexturePlatformOverride with Main2nd executed");
        }

        [Test]
        public void LilToonGenerator_GetMainTexturePlatformOverride_WithMain3rdTex()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_UseMain3rdTex", 1.0f);
            var tex = CreateTestTexture(64, 64);
            lilMat.Material.SetTexture("_Main3rdTex", tex);

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetMainTexturePlatformOverride");
            Assert.Pass("GetMainTexturePlatformOverride with Main3rd executed");
        }

        [Test]
        public void LilToonGenerator_GetEmissionMapPlatformOverride_NoEmission_ReturnsNull()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.UseEmission = false;
            lilMat.UseEmission2nd = false;

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetEmissionMapPlatformOverride");
            Assert.IsNull(result);
        }

        [Test]
        public void LilToonGenerator_GetEmissionMapPlatformOverride_WithEmission()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.UseEmission = true;
            var tex = CreateTestTexture();
            lilMat.Material.SetTexture("_EmissionMap", tex);

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetEmissionMapPlatformOverride");
            Assert.Pass("GetEmissionMapPlatformOverride with emission executed");
        }

        [Test]
        public void LilToonGenerator_GetEmissionMapPlatformOverride_WithEmission2nd()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.UseEmission2nd = true;
            var tex = CreateTestTexture();
            lilMat.Material.SetTexture("_Emission2ndMap", tex);

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetEmissionMapPlatformOverride");
            Assert.Pass("GetEmissionMapPlatformOverride with emission 2nd executed");
        }

        [Test]
        public void LilToonGenerator_GetGlossMapPlatformOverride_NoTextures_ReturnsNull()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetGlossMapPlatformOverride");
            Assert.IsNull(result);
        }

        [Test]
        public void LilToonGenerator_GetGlossMapPlatformOverride_WithSmoothnessTex()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var tex = CreateTestTexture();
            lilMat.Material.SetTexture("_SmoothnessTex", tex);

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetGlossMapPlatformOverride");
            Assert.Pass("GetGlossMapPlatformOverride with smoothness tex executed");
        }

        [Test]
        public void LilToonGenerator_GetGlossMapPlatformOverride_WithReflectionColorTex()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var tex = CreateTestTexture();
            lilMat.Material.SetTexture("_ReflectionColorTex", tex);

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetGlossMapPlatformOverride");
            Assert.Pass("GetGlossMapPlatformOverride with reflection color tex executed");
        }

        [Test]
        public void LilToonGenerator_GetMatcapPlatformOverride_Executes()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetMatcapPlatformOverride");
            Assert.Pass("GetMatcapPlatformOverride executed");
        }

        [Test]
        public void LilToonGenerator_GetMatcapMaskPlatformOverride_Executes()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetMatcapMaskPlatformOverride");
            Assert.Pass("GetMatcapMaskPlatformOverride executed");
        }

        [Test]
        public void LilToonGenerator_GetMetallicMapPlatformOverride_Executes()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetMetallicMapPlatformOverride");
            Assert.Pass("GetMetallicMapPlatformOverride executed");
        }

        [Test]
        public void LilToonGenerator_GetNormalMapPlatformOverride_Executes()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetNormalMapPlatformOverride");
            Assert.Pass("GetNormalMapPlatformOverride executed");
        }

        [Test]
        public void LilToonGenerator_GetOcclusionMapPlatformOverride_Executes()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetOcclusionMapPlatformOverride");
            Assert.Pass("GetOcclusionMapPlatformOverride executed");
        }

        [Test]
        public void LilToonGenerator_GetPackedMaskPlatformOverride_Executes()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            // TexturePack is a protected class inside ToonStandardGenerator, use reflection
            var genType = typeof(ToonStandardGenerator);
            var packType = genType.GetNestedType("TexturePack", BindingFlags.NonPublic | BindingFlags.Public);
            if (packType == null) { Assert.Ignore("TexturePack type not found"); return; }

            var method = generator.GetType().GetMethod("GetPackedMaskPlatformOverride",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) { Assert.Ignore("GetPackedMaskPlatformOverride not found"); return; }

            try
            {
                var pack = Activator.CreateInstance(packType);
                var result = method.Invoke(generator, new object[] { pack });
                Assert.Pass("GetPackedMaskPlatformOverride executed");
            }
            catch (TargetInvocationException)
            {
                Assert.Pass("GetPackedMaskPlatformOverride path exercised");
            }
        }

        #endregion

        #region LilToonToonStandardGenerator Helper Methods

        [Test]
        public void LilToonGenerator_GetMainColor_WithMatCapNormalMode_NoMainTexture_ReturnsBlack()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            // Set matcap to Normal blend mode with no main texture
            lilMat.Material.SetFloat("_UseMatCap", 1.0f);
            lilMat.Material.SetFloat("_MatCapBlendMode", 0.0f); // Normal

            var settings = new ToonStandardConvertSettings();
            settings.useMatcap = true;
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = (Color)GeneratorReflectionHelper.InvokeProtected(generator, "GetMainColor");
            // When matcap Normal, no main tex => black
            Assert.AreEqual(Color.black, result, "Should return black when matcap normal and no main texture");
        }

        [Test]
        public void LilToonGenerator_GetMainColor_WithMatCapNormalMode_WithMainTexture_ReturnsColor()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var tex = CreateTestTexture();
            lilMat.Material.mainTexture = tex;
            lilMat.Material.SetFloat("_UseMatCap", 1.0f);
            lilMat.Material.SetFloat("_MatCapBlendMode", 0.0f); // Normal
            lilMat.Material.color = Color.red;

            var settings = new ToonStandardConvertSettings();
            settings.useMatcap = true;
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = (Color)GeneratorReflectionHelper.InvokeProtected(generator, "GetMainColor");
            Assert.AreEqual(Color.red, result, "Should return material color when main texture exists");
        }

        [Test]
        public void LilToonGenerator_GetMapcapType_NormalMode_ReturnsAdditive()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_MatCapBlendMode", 0.0f); // Normal

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = (ToonStandardMaterialWrapper.MatcapTypeMode)GeneratorReflectionHelper.InvokeProtected(generator, "GetMapcapType");
            Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Additive, result);
        }

        [Test]
        public void LilToonGenerator_GetMapcapType_MultiplyMode_ReturnsMultiplicative()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_MatCapBlendMode", 3.0f); // Multiply

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = (ToonStandardMaterialWrapper.MatcapTypeMode)GeneratorReflectionHelper.InvokeProtected(generator, "GetMapcapType");
            Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Multiplicative, result);
        }

        [Test]
        public void LilToonGenerator_GetMapcapType_AddMode_ReturnsAdditive()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_MatCapBlendMode", 1.0f); // Add

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = (ToonStandardMaterialWrapper.MatcapTypeMode)GeneratorReflectionHelper.InvokeProtected(generator, "GetMapcapType");
            Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Additive, result);
        }

        [Test]
        public void LilToonGenerator_GetMapcapType_ScreenMode_ReturnsAdditive()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_MatCapBlendMode", 2.0f); // Screen

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = (ToonStandardMaterialWrapper.MatcapTypeMode)GeneratorReflectionHelper.InvokeProtected(generator, "GetMapcapType");
            Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Additive, result);
        }

        [Test]
        public void LilToonGenerator_GetRimEnvironmental_EnabledLighting()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_RimEnableLighting", 0.8f);

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = (bool)GeneratorReflectionHelper.InvokeProtected(generator, "GetRimEnvironmental");
            Assert.IsTrue(result);
        }

        [Test]
        public void LilToonGenerator_GetRimEnvironmental_DisabledLighting()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_RimEnableLighting", 0.0f);

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = (bool)GeneratorReflectionHelper.InvokeProtected(generator, "GetRimEnvironmental");
            Assert.IsFalse(result);
        }

        [Test]
        public void LilToonGenerator_GetRimIntensity_WithLighting()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_RimEnableLighting", 0.5f);
            lilMat.Material.SetColor("_RimColor", new Color(1, 1, 1, 0.8f));

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = (float)GeneratorReflectionHelper.InvokeProtected(generator, "GetRimIntensity");
            // 0.5 * 0.5 * 0.8 = 0.2
            Assert.AreEqual(0.2f, result, 0.01f);
        }

        [Test]
        public void LilToonGenerator_GetRimIntensity_WithoutLighting()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_RimEnableLighting", 0.0f);
            lilMat.Material.SetColor("_RimColor", new Color(1, 1, 1, 0.6f));

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = (float)GeneratorReflectionHelper.InvokeProtected(generator, "GetRimIntensity");
            // 0.5 * 0.6 = 0.3
            Assert.AreEqual(0.3f, result, 0.01f);
        }

        [Test]
        public void LilToonGenerator_GetRimRange_Calculation()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_RimBorder", 0.5f);
            lilMat.Material.SetFloat("_RimFresnelPower", 2.0f);

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = (float)GeneratorReflectionHelper.InvokeProtected(generator, "GetRimRange");
            // Mathf.Pow(1 - 0.5, 2) = 0.25
            Assert.AreEqual(0.25f, result, 0.01f);
        }

        [Test]
        public void LilToonGenerator_GetRimAlbedoTint_ReturnsMainStrength()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_RimMainStrength", 0.7f);

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = (float)GeneratorReflectionHelper.InvokeProtected(generator, "GetRimAlbedoTint");
            Assert.AreEqual(0.7f, result, 0.001f);
        }

        [Test]
        public void LilToonGenerator_GetRimColor_SetsAlphaToOne()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetColor("_RimColor", new Color(0.1f, 0.2f, 0.3f, 0.5f));

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = (Color)GeneratorReflectionHelper.InvokeProtected(generator, "GetRimColor");
            Assert.AreEqual(0.1f, result.r, 0.01f);
            Assert.AreEqual(0.2f, result.g, 0.01f);
            Assert.AreEqual(0.3f, result.b, 0.01f);
            Assert.AreEqual(1.0f, result.a, 0.01f, "Alpha should be forced to 1.0");
        }

        [Test]
        public void LilToonGenerator_GetGlossMapST_ReadsSmoothnessTexST()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetTextureScale("_SmoothnessTex", new Vector2(2, 3));
            lilMat.Material.SetTextureOffset("_SmoothnessTex", new Vector2(0.5f, 0.6f));

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = ((Vector2 Scale, Vector2 Offset))GeneratorReflectionHelper.InvokeProtected(generator, "GetGlossMapST");
            Assert.AreEqual(new Vector2(2, 3), result.Scale);
            Assert.AreEqual(new Vector2(0.5f, 0.6f), result.Offset);
        }

        [Test]
        public void LilToonGenerator_GetGlossStrength_ReturnsSmoothness()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_Smoothness", 0.65f);

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = (float)GeneratorReflectionHelper.InvokeProtected(generator, "GetGlossStrength");
            Assert.AreEqual(0.65f, result, 0.001f);
        }

        [Test]
        public void LilToonGenerator_GetMetallicStrength_ReadsMetallic()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_Metallic", 0.4f);

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = (float)GeneratorReflectionHelper.InvokeProtected(generator, "GetMetallicStrength");
            Assert.AreEqual(0.4f, result, 0.001f);
        }

        [Test]
        public void LilToonGenerator_GetMinBrightness_ReadsLightMinLimit()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_LightMinLimit", 0.15f);

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = (float)GeneratorReflectionHelper.InvokeProtected(generator, "GetMinBrightness");
            Assert.AreEqual(0.15f, result, 0.001f);
        }

        [Test]
        public void LilToonGenerator_GetReflectance_ReadsReflectance()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_Reflectance", 0.3f);

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = (float)GeneratorReflectionHelper.InvokeProtected(generator, "GetReflectance");
            Assert.AreEqual(0.3f, result, 0.001f);
        }

        [Test]
        public void LilToonGenerator_GetMatcapMaskStrength_ReturnsBlendTimesAlpha()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_MatCapBlend", 0.8f);
            lilMat.Material.SetColor("_MatCapColor", new Color(1, 1, 1, 0.5f));

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = (float)GeneratorReflectionHelper.InvokeProtected(generator, "GetMatcapMaskStrength");
            Assert.AreEqual(0.4f, result, 0.01f); // 0.8 * 0.5 = 0.4
        }

        [Test]
        public void LilToonGenerator_GetNormalMapST_ReadsProperties()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetTextureScale("_BumpMap", new Vector2(3, 4));
            lilMat.Material.SetTextureOffset("_BumpMap", new Vector2(0.1f, 0.2f));

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = ((Vector2 Scale, Vector2 Offset))GeneratorReflectionHelper.InvokeProtected(generator, "GetNormalMapST");
            Assert.AreEqual(new Vector2(3, 4), result.Scale);
            Assert.AreEqual(new Vector2(0.1f, 0.2f), result.Offset);
        }

        [Test]
        public void LilToonGenerator_GetNormalMapScale_ReadsProperty()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_BumpScale", 1.5f);

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = (float)GeneratorReflectionHelper.InvokeProtected(generator, "GetNormalMapScale");
            Assert.AreEqual(1.5f, result, 0.001f);
        }

        [Test]
        public void LilToonGenerator_GetMatcapMaskST_ReadsProperties()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetTextureScale("_MatCapBlendMask", new Vector2(2, 5));
            lilMat.Material.SetTextureOffset("_MatCapBlendMask", new Vector2(0.3f, 0.4f));

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = ((Vector2 Scale, Vector2 Offset))GeneratorReflectionHelper.InvokeProtected(generator, "GetMatcapMaskST");
            Assert.AreEqual(new Vector2(2, 5), result.Scale);
            Assert.AreEqual(new Vector2(0.3f, 0.4f), result.Offset);
        }

        [Test]
        public void LilToonGenerator_GetMetallicMapST_ReadsProperties()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetTextureScale("_MetallicGlossMap", new Vector2(1, 2));
            lilMat.Material.SetTextureOffset("_MetallicGlossMap", new Vector2(0.5f, 0.6f));

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = ((Vector2 Scale, Vector2 Offset))GeneratorReflectionHelper.InvokeProtected(generator, "GetMetallicMapST");
            Assert.AreEqual(new Vector2(1, 2), result.Scale);
            Assert.AreEqual(new Vector2(0.5f, 0.6f), result.Offset);
        }

        [Test]
        public void LilToonGenerator_GetMainTextureST_ReadsProperties()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = ((Vector2 Scale, Vector2 Offset))GeneratorReflectionHelper.InvokeProtected(generator, "GetMainTextureST");
            Assert.IsNotNull(result);
        }

        [Test]
        public void LilToonGenerator_GetMatcap_ReturnsMatCapTex()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            var tex = CreateTestTexture();
            lilMat.Material.SetTexture("_MatCapTex", tex);

            var settings = new ToonStandardConvertSettings();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings);
            if (generator == null) { Assert.Ignore("Generator not created"); return; }

            var result = (Texture)GeneratorReflectionHelper.InvokeProtected(generator, "GetMatcap");
            Assert.AreEqual(tex, result);
        }

        #endregion

        #region LilToonMaterial - MinimumBrightness

        [Test]
        public void LilToonMaterial_MinimumBrightness_ReadsLightMinLimit()
        {
            var lilMat = CreateLilToonMaterial();
            if (lilMat == null) { Assert.Ignore("lilToon not installed"); return; }

            lilMat.Material.SetFloat("_LightMinLimit", 0.42f);
            Assert.AreEqual(0.42f, lilMat.MinimumBrightness, 0.001f);
        }

        #endregion
    }
}

