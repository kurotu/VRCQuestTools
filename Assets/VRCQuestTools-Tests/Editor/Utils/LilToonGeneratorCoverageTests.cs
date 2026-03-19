// Batch 30: Tests for LilToonToonStandardGenerator protected getter methods,
// AvatarConverter.FindDescendant, MaterialConversionGUI helpers, and more.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UObject = UnityEngine.Object;

namespace KRT.VRCQuestTools.Tests
{
    // =========================================================
    // Helper for invoking protected methods on LilToonToonStandardGenerator
    // =========================================================
    internal static class GeneratorReflectionHelper
    {
        private static readonly BindingFlags NonPublicInstance =
            BindingFlags.Instance | BindingFlags.NonPublic;

        internal static object InvokeProtected(
            LilToonToonStandardGenerator generator, string methodName, params object[] args)
        {
            var method = typeof(LilToonToonStandardGenerator).GetMethod(methodName, NonPublicInstance)
                ?? typeof(ToonStandardGenerator).GetMethod(methodName, NonPublicInstance);
            if (method == null)
            {
                throw new MissingMethodException(
                    $"Method '{methodName}' not found on LilToonToonStandardGenerator or ToonStandardGenerator");
            }
            return method.Invoke(generator, args);
        }

        internal static LilToonToonStandardGenerator CreateGenerator(
            LilToonMaterial lilMat,
            ToonStandardConvertSettings settings = null,
            Texture2D blackTex = null)
        {
            if (settings == null)
            {
                settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(true);
            }
            if (blackTex == null)
            {
                blackTex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            }
            return new LilToonToonStandardGenerator(lilMat, settings, blackTex);
        }
    }

    // =========================================================
    // LilToonToonStandardGenerator - Simple Getter Tests
    // =========================================================
    [TestFixture]
    public class LilToonGenerator_SimpleGetterTests
    {
        private Material mat;
        private LilToonMaterial lilMat;
        private Texture2D blackTex;
        private Texture2D testTex;

        [SetUp]
        public void SetUp()
        {
            mat = LilToonTestHelper.CreateLilToonMaterial();
            lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper();
            blackTex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            testTex = LilToonTestHelper.CreateTestTexture();
        }

        [TearDown]
        public void TearDown()
        {
            if (testTex != null) UObject.DestroyImmediate(testTex);
            if (blackTex != null) UObject.DestroyImmediate(blackTex);
            if (lilMat != null) UObject.DestroyImmediate(lilMat.Material);
            if (mat != null) UObject.DestroyImmediate(mat);
        }

        [Test]
        public void GetCulling_ReturnsCullMode()
        {
            lilMat.Material.SetFloat("_Cull", (float)CullMode.Front);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (CullMode)GeneratorReflectionHelper.InvokeProtected(gen, "GetCulling");
            Assert.AreEqual(CullMode.Front, result);
        }

        [Test]
        public void GetGlossMapST_ReturnsScaleOffset()
        {
            lilMat.Material.SetTextureScale("_SmoothnessTex", new Vector2(2f, 3f));
            lilMat.Material.SetTextureOffset("_SmoothnessTex", new Vector2(0.1f, 0.2f));
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (ValueTuple<Vector2, Vector2>)GeneratorReflectionHelper.InvokeProtected(gen, "GetGlossMapST");
            Assert.AreEqual(new Vector2(2f, 3f), result.Item1);
            Assert.AreEqual(new Vector2(0.1f, 0.2f), result.Item2);
        }

        [Test]
        public void GetGlossStrength_ReturnsSmoothness()
        {
            lilMat.Material.SetFloat("_Smoothness", 0.75f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (float)GeneratorReflectionHelper.InvokeProtected(gen, "GetGlossStrength");
            Assert.AreEqual(0.75f, result, 0.001f);
        }

        [Test]
        public void GetMainTextureST_ReturnsScaleOffset()
        {
            lilMat.Material.mainTextureScale = new Vector2(2f, 2f);
            lilMat.Material.mainTextureOffset = new Vector2(0.5f, 0.5f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (ValueTuple<Vector2, Vector2>)GeneratorReflectionHelper.InvokeProtected(gen, "GetMainTextureST");
            Assert.AreEqual(new Vector2(2f, 2f), result.Item1);
            Assert.AreEqual(new Vector2(0.5f, 0.5f), result.Item2);
        }

        [Test]
        public void GetMatcap_ReturnsTexture()
        {
            lilMat.Material.SetTexture("_MatCapTex", testTex);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (Texture)GeneratorReflectionHelper.InvokeProtected(gen, "GetMatcap");
            Assert.AreEqual(testTex, result);
        }

        [Test]
        public void GetMatcapMaskST_ReturnsScaleOffset()
        {
            lilMat.Material.SetTextureScale("_MatCapBlendMask", new Vector2(1.5f, 1.5f));
            lilMat.Material.SetTextureOffset("_MatCapBlendMask", new Vector2(0.3f, 0.4f));
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (ValueTuple<Vector2, Vector2>)GeneratorReflectionHelper.InvokeProtected(gen, "GetMatcapMaskST");
            Assert.AreEqual(new Vector2(1.5f, 1.5f), result.Item1);
            Assert.AreEqual(new Vector2(0.3f, 0.4f), result.Item2);
        }

        [Test]
        public void GetMatcapMaskStrength_ReturnsBlendTimesAlpha()
        {
            lilMat.Material.SetFloat("_MatCapBlend", 0.8f);
            lilMat.Material.SetColor("_MatCapColor", new Color(1, 1, 1, 0.5f));
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (float)GeneratorReflectionHelper.InvokeProtected(gen, "GetMatcapMaskStrength");
            Assert.AreEqual(0.4f, result, 0.001f);
        }

        [Test]
        public void GetMetallicMapST_ReturnsScaleOffset()
        {
            lilMat.Material.SetTextureScale("_MetallicGlossMap", new Vector2(2f, 2f));
            lilMat.Material.SetTextureOffset("_MetallicGlossMap", new Vector2(0.1f, 0.1f));
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (ValueTuple<Vector2, Vector2>)GeneratorReflectionHelper.InvokeProtected(gen, "GetMetallicMapST");
            Assert.AreEqual(new Vector2(2f, 2f), result.Item1);
            Assert.AreEqual(new Vector2(0.1f, 0.1f), result.Item2);
        }

        [Test]
        public void GetMetallicStrength_ReturnsMetallic()
        {
            lilMat.Material.SetFloat("_Metallic", 0.9f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (float)GeneratorReflectionHelper.InvokeProtected(gen, "GetMetallicStrength");
            Assert.AreEqual(0.9f, result, 0.001f);
        }

        [Test]
        public void GetMinBrightness_ReturnsLightMinLimit()
        {
            lilMat.Material.SetFloat("_LightMinLimit", 0.15f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (float)GeneratorReflectionHelper.InvokeProtected(gen, "GetMinBrightness");
            Assert.AreEqual(0.15f, result, 0.001f);
        }

        [Test]
        public void GetNormalMapST_ReturnsScaleOffset()
        {
            lilMat.Material.SetTextureScale("_BumpMap", new Vector2(3f, 3f));
            lilMat.Material.SetTextureOffset("_BumpMap", new Vector2(0.2f, 0.3f));
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (ValueTuple<Vector2, Vector2>)GeneratorReflectionHelper.InvokeProtected(gen, "GetNormalMapST");
            Assert.AreEqual(new Vector2(3f, 3f), result.Item1);
            Assert.AreEqual(new Vector2(0.2f, 0.3f), result.Item2);
        }

        [Test]
        public void GetNormalMapScale_ReturnsBumpScale()
        {
            lilMat.Material.SetFloat("_BumpScale", 1.5f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (float)GeneratorReflectionHelper.InvokeProtected(gen, "GetNormalMapScale");
            Assert.AreEqual(1.5f, result, 0.001f);
        }

        [Test]
        public void GetReflectance_ReturnsValue()
        {
            lilMat.Material.SetFloat("_Reflectance", 0.6f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (float)GeneratorReflectionHelper.InvokeProtected(gen, "GetReflectance");
            Assert.AreEqual(0.6f, result, 0.001f);
        }

        [Test]
        public void GetSharpness_ReturnsOneMinusSpecularBlur()
        {
            lilMat.Material.SetFloat("_SpecularBlur", 0.3f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (float)GeneratorReflectionHelper.InvokeProtected(gen, "GetSharpness");
            Assert.AreEqual(0.7f, result, 0.001f);
        }

        [Test]
        public void GetOcculusionMapST_ReturnsScaleOffset()
        {
            lilMat.Material.SetTextureScale("_ShadowBorderMask", new Vector2(1.5f, 2.5f));
            lilMat.Material.SetTextureOffset("_ShadowBorderMask", new Vector2(0.4f, 0.5f));
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (ValueTuple<Vector2, Vector2>)GeneratorReflectionHelper.InvokeProtected(gen, "GetOcculusionMapST");
            Assert.AreEqual(new Vector2(1.5f, 2.5f), result.Item1);
            Assert.AreEqual(new Vector2(0.4f, 0.5f), result.Item2);
        }
    }

    // =========================================================
    // LilToonToonStandardGenerator - Rim Getter Tests
    // =========================================================
    [TestFixture]
    public class LilToonGenerator_RimGetterTests
    {
        private LilToonMaterial lilMat;
        private Texture2D blackTex;

        [SetUp]
        public void SetUp()
        {
            lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper();
            blackTex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        }

        [TearDown]
        public void TearDown()
        {
            if (blackTex != null) UObject.DestroyImmediate(blackTex);
            if (lilMat != null) UObject.DestroyImmediate(lilMat.Material);
        }

        [Test]
        public void GetRimAlbedoTint_ReturnsRimMainStrength()
        {
            lilMat.Material.SetFloat("_RimMainStrength", 0.7f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (float)GeneratorReflectionHelper.InvokeProtected(gen, "GetRimAlbedoTint");
            Assert.AreEqual(0.7f, result, 0.001f);
        }

        [Test]
        public void GetRimColor_ReturnsColorWithAlphaOne()
        {
            lilMat.Material.SetColor("_RimColor", new Color(0.5f, 0.3f, 0.8f, 0.2f));
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (Color)GeneratorReflectionHelper.InvokeProtected(gen, "GetRimColor");
            Assert.AreEqual(0.5f, result.r, 0.001f);
            Assert.AreEqual(0.3f, result.g, 0.001f);
            Assert.AreEqual(0.8f, result.b, 0.001f);
            Assert.AreEqual(1.0f, result.a, 0.001f);
        }

        [Test]
        public void GetRimEnvironmental_WhenEnabled_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_RimEnableLighting", 0.5f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetRimEnvironmental");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetRimEnvironmental_WhenZero_ReturnsFalse()
        {
            lilMat.Material.SetFloat("_RimEnableLighting", 0.0f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetRimEnvironmental");
            Assert.IsFalse(result);
        }

        [Test]
        public void GetRimIntensity_WhenEnvironmental_ScalesByRimEnableLighting()
        {
            lilMat.Material.SetFloat("_RimEnableLighting", 0.8f);
            lilMat.Material.SetColor("_RimColor", new Color(1, 1, 1, 1.0f));
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (float)GeneratorReflectionHelper.InvokeProtected(gen, "GetRimIntensity");
            // environmental: 0.5f * 0.8f * 1.0f (alpha) = 0.4f
            Assert.AreEqual(0.4f, result, 0.001f);
        }

        [Test]
        public void GetRimIntensity_WhenNotEnvironmental_Returns0Point5TimesAlpha()
        {
            lilMat.Material.SetFloat("_RimEnableLighting", 0.0f);
            lilMat.Material.SetColor("_RimColor", new Color(1, 1, 1, 0.6f));
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (float)GeneratorReflectionHelper.InvokeProtected(gen, "GetRimIntensity");
            // non-environmental: 0.5f * 0.6f (alpha) = 0.3f
            Assert.AreEqual(0.3f, result, 0.001f);
        }

        [Test]
        public void GetRimRange_ComputesPow()
        {
            lilMat.Material.SetFloat("_RimBorder", 0.5f);
            lilMat.Material.SetFloat("_RimFresnelPower", 2.0f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (float)GeneratorReflectionHelper.InvokeProtected(gen, "GetRimRange");
            // Mathf.Pow(1.0 - 0.5, 2.0) = Mathf.Pow(0.5, 2.0) = 0.25
            Assert.AreEqual(0.25f, result, 0.001f);
        }

        [Test]
        public void GetRimSoftness_ReturnsRimLightBlur()
        {
            lilMat.Material.SetFloat("_RimBlur", 0.4f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (float)GeneratorReflectionHelper.InvokeProtected(gen, "GetRimSoftness");
            Assert.AreEqual(0.4f, result, 0.001f);
        }
    }

    // =========================================================
    // LilToonToonStandardGenerator - Emission Getter Branch Tests
    // =========================================================
    [TestFixture]
    public class LilToonGenerator_EmissionGetterTests
    {
        private LilToonMaterial lilMat;
        private Texture2D blackTex;
        private Texture2D testTex;

        [SetUp]
        public void SetUp()
        {
            lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper();
            blackTex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            testTex = LilToonTestHelper.CreateTestTexture();
        }

        [TearDown]
        public void TearDown()
        {
            if (testTex != null) UObject.DestroyImmediate(testTex);
            if (blackTex != null) UObject.DestroyImmediate(blackTex);
            if (lilMat != null) UObject.DestroyImmediate(lilMat.Material);
        }

        [Test]
        public void GetEmissionColor_BothEmissionEnabled_ReturnsWhite()
        {
            lilMat.Material.SetFloat("_UseEmission", 1f);
            lilMat.Material.SetFloat("_UseEmission2nd", 1f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (Color)GeneratorReflectionHelper.InvokeProtected(gen, "GetEmissionColor");
            Assert.AreEqual(Color.white, result);
        }

        [Test]
        public void GetEmissionColor_OnlyEmission1st_ReturnsEmissionColor()
        {
            lilMat.Material.SetFloat("_UseEmission", 1f);
            lilMat.Material.SetFloat("_UseEmission2nd", 0f);
            lilMat.Material.SetColor("_EmissionColor", Color.red);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (Color)GeneratorReflectionHelper.InvokeProtected(gen, "GetEmissionColor");
            Assert.AreEqual(Color.red, result);
        }

        [Test]
        public void GetEmissionColor_OnlyEmission2nd_ReturnsEmission2ndColor()
        {
            lilMat.Material.SetFloat("_UseEmission", 0f);
            lilMat.Material.SetFloat("_UseEmission2nd", 1f);
            lilMat.Material.SetColor("_Emission2ndColor", Color.green);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (Color)GeneratorReflectionHelper.InvokeProtected(gen, "GetEmissionColor");
            Assert.AreEqual(Color.green, result);
        }

        [Test]
        public void GetEmissionColor_NoEmission_ReturnsBlack()
        {
            lilMat.Material.SetFloat("_UseEmission", 0f);
            lilMat.Material.SetFloat("_UseEmission2nd", 0f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (Color)GeneratorReflectionHelper.InvokeProtected(gen, "GetEmissionColor");
            Assert.AreEqual(Color.black, result);
        }

        [Test]
        public void GetUseEmission_Both_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseEmission", 1f);
            lilMat.Material.SetFloat("_UseEmission2nd", 1f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseEmission");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseEmission_OnlyFirst_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseEmission", 1f);
            lilMat.Material.SetFloat("_UseEmission2nd", 0f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseEmission");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseEmission_None_ReturnsFalse()
        {
            lilMat.Material.SetFloat("_UseEmission", 0f);
            lilMat.Material.SetFloat("_UseEmission2nd", 0f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseEmission");
            Assert.IsFalse(result);
        }

        [Test]
        public void GetUseEmissionMap_WithEmissionMap_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseEmission", 1f);
            lilMat.Material.SetTexture("_EmissionMap", testTex);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseEmissionMap");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseEmissionMap_WithEmissionBlendMask_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseEmission", 1f);
            lilMat.Material.SetTexture("_EmissionBlendMask", testTex);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseEmissionMap");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseEmissionMap_WithEmissionBlendLessThan1_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseEmission", 1f);
            lilMat.Material.SetFloat("_EmissionBlend", 0.5f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseEmissionMap");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseEmissionMap_With2ndEmissionMap_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseEmission", 0f);
            lilMat.Material.SetFloat("_UseEmission2nd", 1f);
            lilMat.Material.SetTexture("_Emission2ndMap", testTex);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseEmissionMap");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseEmissionMap_With2ndBlendMask_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseEmission", 0f);
            lilMat.Material.SetFloat("_UseEmission2nd", 1f);
            lilMat.Material.SetTexture("_Emission2ndBlendMask", testTex);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseEmissionMap");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseEmissionMap_With2ndBlendLessThan1_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseEmission", 0f);
            lilMat.Material.SetFloat("_UseEmission2nd", 1f);
            lilMat.Material.SetFloat("_Emission2ndBlend", 0.5f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseEmissionMap");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseEmissionMap_BothEmission_NoTextures_ReturnsTrue()
        {
            // Both emission enabled, no textures, blend=1 => falls through to last return
            lilMat.Material.SetFloat("_UseEmission", 1f);
            lilMat.Material.SetFloat("_UseEmission2nd", 1f);
            lilMat.Material.SetFloat("_EmissionBlend", 1f);
            lilMat.Material.SetFloat("_Emission2ndBlend", 1f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseEmissionMap");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseEmissionMap_NoEmission_ReturnsFalse()
        {
            lilMat.Material.SetFloat("_UseEmission", 0f);
            lilMat.Material.SetFloat("_UseEmission2nd", 0f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseEmissionMap");
            Assert.IsFalse(result);
        }
    }

    // =========================================================
    // LilToonToonStandardGenerator - Boolean Use* Getter Tests
    // =========================================================
    [TestFixture]
    public class LilToonGenerator_UseBooleanGetterTests
    {
        private LilToonMaterial lilMat;
        private Texture2D blackTex;
        private Texture2D testTex;

        [SetUp]
        public void SetUp()
        {
            lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper();
            blackTex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            testTex = LilToonTestHelper.CreateTestTexture();
        }

        [TearDown]
        public void TearDown()
        {
            if (testTex != null) UObject.DestroyImmediate(testTex);
            if (blackTex != null) UObject.DestroyImmediate(blackTex);
            if (lilMat != null) UObject.DestroyImmediate(lilMat.Material);
        }

        // --- GetUseGlossMap ---
        [Test]
        public void GetUseGlossMap_NoReflection_ReturnsFalse()
        {
            lilMat.Material.SetFloat("_UseReflection", 0f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseGlossMap");
            Assert.IsFalse(result);
        }

        [Test]
        public void GetUseGlossMap_WithSmoothnessTex_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseReflection", 1f);
            lilMat.Material.SetTexture("_SmoothnessTex", testTex);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseGlossMap");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseGlossMap_WithReflectionColorTex_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseReflection", 1f);
            lilMat.Material.SetTexture("_ReflectionColorTex", testTex);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseGlossMap");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseGlossMap_WithNonWhiteReflectionColor_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseReflection", 1f);
            lilMat.Material.SetColor("_ReflectionColor", Color.red);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseGlossMap");
            Assert.IsTrue(result);
        }

        // --- GetUseMetallicMap ---
        [Test]
        public void GetUseMetallicMap_NoReflection_ReturnsFalse()
        {
            lilMat.Material.SetFloat("_UseReflection", 0f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseMetallicMap");
            Assert.IsFalse(result);
        }

        [Test]
        public void GetUseMetallicMap_WithMetallicMap_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseReflection", 1f);
            lilMat.Material.SetTexture("_MetallicGlossMap", testTex);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseMetallicMap");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseMetallicMap_WithReflectionColorTex_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseReflection", 1f);
            lilMat.Material.SetTexture("_ReflectionColorTex", testTex);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseMetallicMap");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseMetallicMap_WithNonWhiteReflectionColor_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseReflection", 1f);
            lilMat.Material.SetColor("_ReflectionColor", Color.blue);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseMetallicMap");
            Assert.IsTrue(result);
        }

        // --- GetUseMainTexture ---
        [Test]
        public void GetUseMainTexture_WithMainTexture_ReturnsTrue()
        {
            lilMat.Material.mainTexture = testTex;
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseMainTexture");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseMainTexture_NoMainTex_EmissionEnabled_SettingsDisabled_ReturnsTrue()
        {
            // Settings.useEmission=false, but lilToon has emission => forces main texture
            lilMat.Material.mainTexture = null;
            lilMat.Material.SetFloat("_UseEmission", 1f);
            var settings = new ToonStandardConvertSettings();
            settings.SetAllFeatures(true);
            settings.useEmission = false;
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, settings: settings, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseMainTexture");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseMainTexture_NoMainTex_Emission2ndEnabled_SettingsDisabled_ReturnsTrue()
        {
            lilMat.Material.mainTexture = null;
            lilMat.Material.SetFloat("_UseEmission2nd", 1f);
            var settings = new ToonStandardConvertSettings();
            settings.SetAllFeatures(true);
            settings.useEmission = false;
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, settings: settings, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseMainTexture");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseMainTexture_NoMainTex_NoEmission_ReturnsFalse()
        {
            lilMat.Material.mainTexture = null;
            lilMat.Material.SetFloat("_UseEmission", 0f);
            lilMat.Material.SetFloat("_UseEmission2nd", 0f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseMainTexture");
            Assert.IsFalse(result);
        }

        // --- GetUseNormalMap ---
        [Test]
        public void GetUseNormalMap_EnabledWithTexture_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseBumpMap", 1f);
            lilMat.Material.SetTexture("_BumpMap", testTex);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseNormalMap");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseNormalMap_EnabledNoTexture_ReturnsFalse()
        {
            lilMat.Material.SetFloat("_UseBumpMap", 1f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseNormalMap");
            Assert.IsFalse(result);
        }

        [Test]
        public void GetUseNormalMap_Disabled_ReturnsFalse()
        {
            lilMat.Material.SetFloat("_UseBumpMap", 0f);
            lilMat.Material.SetTexture("_BumpMap", testTex);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseNormalMap");
            Assert.IsFalse(result);
        }

        // --- GetUseOcclusionMap ---
        [Test]
        public void GetUseOcclusionMap_WithShadowAndAOMap_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseShadow", 1f);
            lilMat.Material.SetTexture("_ShadowBorderMask", testTex);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseOcclusionMap");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseOcclusionMap_NoShadow_ReturnsFalse()
        {
            lilMat.Material.SetFloat("_UseShadow", 0f);
            lilMat.Material.SetTexture("_ShadowBorderMask", testTex);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseOcclusionMap");
            Assert.IsFalse(result);
        }

        [Test]
        public void GetUseOcclusionMap_NoAOMap_ReturnsFalse()
        {
            lilMat.Material.SetFloat("_UseShadow", 1f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseOcclusionMap");
            Assert.IsFalse(result);
        }

        // --- GetUseMatcap & GetUseMatcapMask ---
        [Test]
        public void GetUseMatcap_Enabled_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseMatCap", 1f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseMatcap");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseMatcap_Disabled_ReturnsFalse()
        {
            lilMat.Material.SetFloat("_UseMatCap", 0f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseMatcap");
            Assert.IsFalse(result);
        }

        [Test]
        public void GetUseMatcapMask_WithMatCapAndMask_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseMatCap", 1f);
            lilMat.Material.SetTexture("_MatCapBlendMask", testTex);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseMatcapMask");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseMatcapMask_NoMatCap_ReturnsFalse()
        {
            lilMat.Material.SetFloat("_UseMatCap", 0f);
            lilMat.Material.SetTexture("_MatCapBlendMask", testTex);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseMatcapMask");
            Assert.IsFalse(result);
        }

        [Test]
        public void GetUseMatcapMask_NoMaskTexture_ReturnsFalse()
        {
            lilMat.Material.SetFloat("_UseMatCap", 1f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseMatcapMask");
            Assert.IsFalse(result);
        }

        // --- GetUseRimLighting, GetUseShadowRamp, GetUseSpecular ---
        [Test]
        public void GetUseRimLighting_Enabled_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseRim", 1f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseRimLighting");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseRimLighting_Disabled_ReturnsFalse()
        {
            lilMat.Material.SetFloat("_UseRim", 0f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseRimLighting");
            Assert.IsFalse(result);
        }

        [Test]
        public void GetUseShadowRamp_Enabled_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseShadow", 1f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseShadowRamp");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseShadowRamp_Disabled_ReturnsFalse()
        {
            lilMat.Material.SetFloat("_UseShadow", 0f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseShadowRamp");
            Assert.IsFalse(result);
        }

        [Test]
        public void GetUseSpecular_Enabled_ReturnsTrue()
        {
            lilMat.Material.SetFloat("_UseReflection", 1f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseSpecular");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseSpecular_Disabled_ReturnsFalse()
        {
            lilMat.Material.SetFloat("_UseReflection", 0f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (bool)GeneratorReflectionHelper.InvokeProtected(gen, "GetUseSpecular");
            Assert.IsFalse(result);
        }
    }

    // =========================================================
    // LilToonToonStandardGenerator - MainColor and MatcapType Tests
    // =========================================================
    [TestFixture]
    public class LilToonGenerator_MainColorAndMatcapTypeTests
    {
        private LilToonMaterial lilMat;
        private Texture2D blackTex;
        private Texture2D testTex;

        [SetUp]
        public void SetUp()
        {
            lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper();
            blackTex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            testTex = LilToonTestHelper.CreateTestTexture();
        }

        [TearDown]
        public void TearDown()
        {
            if (testTex != null) UObject.DestroyImmediate(testTex);
            if (blackTex != null) UObject.DestroyImmediate(blackTex);
            if (lilMat != null) UObject.DestroyImmediate(lilMat.Material);
        }

        [Test]
        public void GetMainColor_Normal_ReturnsMaterialColor()
        {
            lilMat.Material.color = new Color(0.5f, 0.6f, 0.7f, 1f);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = (Color)GeneratorReflectionHelper.InvokeProtected(gen, "GetMainColor");
            Assert.AreEqual(new Color(0.5f, 0.6f, 0.7f, 1f), result);
        }

        [Test]
        public void GetMainColor_MatcapNormal_NoMainTex_ReturnsBlack()
        {
            // When matcap mode=Normal, useMatcap=true, and no main texture => black
            lilMat.Material.SetFloat("_UseMatCap", 1f);
            lilMat.Material.SetFloat("_MatCapBlendMode", 0f); // Normal = 0
            lilMat.Material.mainTexture = null;
            lilMat.Material.SetFloat("_UseEmission", 0f);
            lilMat.Material.SetFloat("_UseEmission2nd", 0f);
            var settings = new ToonStandardConvertSettings();
            settings.SetAllFeatures(true);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, settings: settings, blackTex: blackTex);
            var result = (Color)GeneratorReflectionHelper.InvokeProtected(gen, "GetMainColor");
            Assert.AreEqual(Color.black, result);
        }

        [Test]
        public void GetMainColor_MatcapNormal_WithMainTex_ReturnsMaterialColor()
        {
            lilMat.Material.SetFloat("_UseMatCap", 1f);
            lilMat.Material.SetFloat("_MatCapBlendMode", 0f); // Normal
            lilMat.Material.mainTexture = testTex;
            lilMat.Material.color = Color.cyan;
            var settings = new ToonStandardConvertSettings();
            settings.SetAllFeatures(true);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, settings: settings, blackTex: blackTex);
            var result = (Color)GeneratorReflectionHelper.InvokeProtected(gen, "GetMainColor");
            Assert.AreEqual(Color.cyan, result);
        }

        [Test]
        public void GetMainColor_MatcapAdd_ReturnsMaterialColor()
        {
            // MatCapBlendMode != Normal, so mainColor fallthrough
            lilMat.Material.SetFloat("_UseMatCap", 1f);
            lilMat.Material.SetFloat("_MatCapBlendMode", 1f); // Add
            lilMat.Material.mainTexture = null;
            lilMat.Material.color = Color.yellow;
            var settings = new ToonStandardConvertSettings();
            settings.SetAllFeatures(true);
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, settings: settings, blackTex: blackTex);
            var result = (Color)GeneratorReflectionHelper.InvokeProtected(gen, "GetMainColor");
            Assert.AreEqual(Color.yellow, result);
        }

        // --- GetMapcapType ---
        [Test]
        public void GetMapcapType_Normal_ReturnsAdditive()
        {
            lilMat.Material.SetFloat("_MatCapBlendMode", 0f); // Normal
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = GeneratorReflectionHelper.InvokeProtected(gen, "GetMapcapType");
            // ToonStandardMaterialWrapper.MatcapTypeMode.Additive
            Assert.AreEqual(0, (int)result); // Additive = 0
        }

        [Test]
        public void GetMapcapType_Add_ReturnsAdditive()
        {
            lilMat.Material.SetFloat("_MatCapBlendMode", 1f); // Add
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = GeneratorReflectionHelper.InvokeProtected(gen, "GetMapcapType");
            Assert.AreEqual(0, (int)result); // Additive = 0
        }

        [Test]
        public void GetMapcapType_Screen_ReturnsAdditive()
        {
            lilMat.Material.SetFloat("_MatCapBlendMode", 2f); // Screen
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = GeneratorReflectionHelper.InvokeProtected(gen, "GetMapcapType");
            Assert.AreEqual(0, (int)result); // Additive = 0
        }

        [Test]
        public void GetMapcapType_Multiply_ReturnsMultiplicative()
        {
            lilMat.Material.SetFloat("_MatCapBlendMode", 3f); // Multiply
            var gen = GeneratorReflectionHelper.CreateGenerator(lilMat, blackTex: blackTex);
            var result = GeneratorReflectionHelper.InvokeProtected(gen, "GetMapcapType");
            Assert.AreEqual(1, (int)result); // Multiplicative = 1
        }
    }

    // =========================================================
    // AvatarConverter.FindDescendant Tests
    // =========================================================
    [TestFixture]
    public class AvatarConverter_FindDescendantTests
    {
        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in createdObjects)
            {
                if (obj != null) UObject.DestroyImmediate(obj);
            }
            createdObjects.Clear();
        }

        private GameObject CreateHierarchy()
        {
            var root = new GameObject("Root");
            createdObjects.Add(root);

            var child1 = new GameObject("ChildA");
            child1.transform.SetParent(root.transform);

            var child2 = new GameObject("ChildB");
            child2.transform.SetParent(root.transform);

            var grandchild = new GameObject("GrandChild");
            grandchild.transform.SetParent(child1.transform);

            var deepChild = new GameObject("DeepChild");
            deepChild.transform.SetParent(grandchild.transform);

            return root;
        }

        private object InvokeFindDescendant(object converter, GameObject go, string name)
        {
            var method = converter.GetType().GetMethod("FindDescendant",
                BindingFlags.Instance | BindingFlags.NonPublic);
            return method.Invoke(converter, new object[] { go, name });
        }

        private object CreateAvatarConverter()
        {
            var builderType = typeof(MaterialWrapperBuilder);
            var converter = Activator.CreateInstance(
                typeof(MaterialWrapperBuilder).Assembly.GetType("KRT.VRCQuestTools.Models.VRChat.AvatarConverter"),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new object[] { new MaterialWrapperBuilder() },
                null);
            return converter;
        }

        [Test]
        public void FindDescendant_DirectChild_FindsIt()
        {
            var root = CreateHierarchy();
            var converter = CreateAvatarConverter();
            var result = (GameObject)InvokeFindDescendant(converter, root, "ChildA");
            Assert.IsNotNull(result);
            Assert.AreEqual("ChildA", result.name);
        }

        [Test]
        public void FindDescendant_GrandChild_FindsIt()
        {
            var root = CreateHierarchy();
            var converter = CreateAvatarConverter();
            var result = (GameObject)InvokeFindDescendant(converter, root, "GrandChild");
            Assert.IsNotNull(result);
            Assert.AreEqual("GrandChild", result.name);
        }

        [Test]
        public void FindDescendant_DeepChild_FindsIt()
        {
            var root = CreateHierarchy();
            var converter = CreateAvatarConverter();
            var result = (GameObject)InvokeFindDescendant(converter, root, "DeepChild");
            Assert.IsNotNull(result);
            Assert.AreEqual("DeepChild", result.name);
        }

        [Test]
        public void FindDescendant_NotFound_ReturnsNull()
        {
            var root = CreateHierarchy();
            var converter = CreateAvatarConverter();
            var result = (GameObject)InvokeFindDescendant(converter, root, "NonExistent");
            Assert.IsNull(result);
        }

        [Test]
        public void FindDescendant_NoChildren_ReturnsNull()
        {
            var go = new GameObject("Alone");
            createdObjects.Add(go);
            var converter = CreateAvatarConverter();
            var result = (GameObject)InvokeFindDescendant(converter, go, "Missing");
            Assert.IsNull(result);
        }
    }

    // =========================================================
    // AvatarConverter.ClearSharedBlackTextureCache Tests
    // =========================================================
    [TestFixture]
    public class AvatarConverter_CacheTests
    {
        [Test]
        public void ClearSharedBlackTextureCache_DoesNotThrow()
        {
            var converterType = typeof(MaterialWrapperBuilder).Assembly
                .GetType("KRT.VRCQuestTools.Models.VRChat.AvatarConverter");
            var converter = Activator.CreateInstance(
                converterType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new object[] { new MaterialWrapperBuilder() },
                null);
            var method = converterType.GetMethod("ClearSharedBlackTextureCache",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.DoesNotThrow(() => method.Invoke(converter, null));
        }
    }

    // =========================================================
    // ToonStandardGenerator.GenerateMaterial - non-IToonStandardConvertable path
    // =========================================================
    [TestFixture]
    public class ToonStandardGenerator_GenerateMaterialFallbackTests
    {
        [Test]
        public void GenerateMaterial_NonConvertableWithoutGenerateTextures_UsesConvertToToonStandard()
        {
            // When generateQuestTextures=false, GenerateMaterial calls ConvertToToonStandard()
            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            try
            {
                var settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(true);
                settings.generateQuestTextures = false;
                settings.fallbackShadowRamp = new Texture2D(4, 4);

                var gen = new LilToonToonStandardGenerator(lilMat, settings, blackTex);
                Material result = null;
                var request = gen.GenerateMaterial(
                    lilMat,
                    UnityEditor.BuildTarget.Android,
                    false,
                    "Assets/Temp",
                    (mat) => { result = mat; });
                // Process the request
                request.WaitForCompletion();
                Assert.IsNotNull(result);
                // Should be a ToonStandard material
                Assert.IsTrue(result.shader.name.Contains("Toon Standard") || result.shader.name.Contains("VRChat"));
            }
            finally
            {
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(lilMat.Material);
            }
        }
    }

    // =========================================================
    // MaterialConversionGUI helper method tests
    // =========================================================
    [TestFixture]
    public class MaterialConversionGUI_HelperTests
    {
        private static MethodInfo GetIsHandledMaterialMethod()
        {
            // MaterialConversionGUI is in the Editor assembly
            var editorAssembly = typeof(LilToonToonStandardGenerator).Assembly;
            var guiType = editorAssembly.GetType("KRT.VRCQuestTools.Inspector.MaterialConversionGUI");
            if (guiType == null) return null;
            return guiType.GetMethod("IsHandledMaterial",
                BindingFlags.Static | BindingFlags.NonPublic);
        }

        [Test]
        public void IsHandledMaterial_NullMaterial_ReturnsFalse()
        {
            var method = GetIsHandledMaterialMethod();
            if (method == null) Assert.Inconclusive("IsHandledMaterial method not found");
            var parameters = method.GetParameters();
            var emptyConversion = Array.CreateInstance(parameters[1].ParameterType.GetElementType(), 0);
            var emptySwap = Array.CreateInstance(parameters[2].ParameterType.GetElementType(), 0);
            var result = (bool)method.Invoke(null, new object[] { null, emptyConversion, emptySwap });
            Assert.IsFalse(result);
        }

        [Test]
        public void IsHandledMaterial_NoComponents_ReturnsFalse()
        {
            var method = GetIsHandledMaterialMethod();
            if (method == null) Assert.Inconclusive("IsHandledMaterial method not found");
            var parameters = method.GetParameters();
            var emptyConversion = Array.CreateInstance(parameters[1].ParameterType.GetElementType(), 0);
            var emptySwap = Array.CreateInstance(parameters[2].ParameterType.GetElementType(), 0);

            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var result = (bool)method.Invoke(null, new object[] { mat, emptyConversion, emptySwap });
                Assert.IsFalse(result);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }
    }

    // =========================================================
    // LilToonToonStandardGenerator - ConvertToToonStandard additional branch tests
    // =========================================================
    [TestFixture]
    public class LilToonGenerator_ConvertToToonStandardBranchTests
    {
        private LilToonMaterial lilMat;
        private Texture2D blackTex;
        private Texture2D testTex;

        [SetUp]
        public void SetUp()
        {
            lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper();
            blackTex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            testTex = LilToonTestHelper.CreateTestTexture();
        }

        [TearDown]
        public void TearDown()
        {
            if (testTex != null) UObject.DestroyImmediate(testTex);
            if (blackTex != null) UObject.DestroyImmediate(blackTex);
            if (lilMat != null) UObject.DestroyImmediate(lilMat.Material);
        }

        private Material InvokeConvertToToonStandard(LilToonToonStandardGenerator gen)
        {
            var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                BindingFlags.Instance | BindingFlags.NonPublic);
            return (Material)method.Invoke(gen, null);
        }

        [Test]
        public void ConvertToToonStandard_EmissionDisabledInSettings_SetsBlackEmission()
        {
            lilMat.Material.SetFloat("_UseEmission", 1f);
            lilMat.Material.SetColor("_EmissionColor", Color.red);
            var settings = new ToonStandardConvertSettings();
            settings.SetAllFeatures(true);
            settings.useEmission = false;
            settings.fallbackShadowRamp = new Texture2D(4, 4);
            var gen = new LilToonToonStandardGenerator(lilMat, settings, blackTex);
            var result = InvokeConvertToToonStandard(gen);
            try
            {
                Assert.IsNotNull(result);
                // Emission should be disabled since settings.useEmission=false
            }
            finally
            {
                UObject.DestroyImmediate(result);
            }
        }

        [Test]
        public void ConvertToToonStandard_NormalMapDisabledInSettings_SkipsNormalMap()
        {
            lilMat.Material.SetFloat("_UseBumpMap", 1f);
            lilMat.Material.SetTexture("_BumpMap", testTex);
            var settings = new ToonStandardConvertSettings();
            settings.SetAllFeatures(true);
            settings.useNormalMap = false;
            settings.fallbackShadowRamp = new Texture2D(4, 4);
            var gen = new LilToonToonStandardGenerator(lilMat, settings, blackTex);
            var result = InvokeConvertToToonStandard(gen);
            try
            {
                Assert.IsNotNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(result);
            }
        }

        [Test]
        public void ConvertToToonStandard_OcclusionDisabledInSettings_SkipsOcclusion()
        {
            lilMat.Material.SetFloat("_UseShadow", 1f);
            lilMat.Material.SetTexture("_ShadowBorderMask", testTex);
            var settings = new ToonStandardConvertSettings();
            settings.SetAllFeatures(true);
            settings.useOcclusion = false;
            settings.fallbackShadowRamp = new Texture2D(4, 4);
            var gen = new LilToonToonStandardGenerator(lilMat, settings, blackTex);
            var result = InvokeConvertToToonStandard(gen);
            try
            {
                Assert.IsNotNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(result);
            }
        }

        [Test]
        public void ConvertToToonStandard_SpecularDisabledInSettings_SkipsSpecular()
        {
            lilMat.Material.SetFloat("_UseReflection", 1f);
            var settings = new ToonStandardConvertSettings();
            settings.SetAllFeatures(true);
            settings.useSpecular = false;
            settings.fallbackShadowRamp = new Texture2D(4, 4);
            var gen = new LilToonToonStandardGenerator(lilMat, settings, blackTex);
            var result = InvokeConvertToToonStandard(gen);
            try
            {
                Assert.IsNotNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(result);
            }
        }

        [Test]
        public void ConvertToToonStandard_MatcapDisabledInSettings_SkipsMatcap()
        {
            lilMat.Material.SetFloat("_UseMatCap", 1f);
            var settings = new ToonStandardConvertSettings();
            settings.SetAllFeatures(true);
            settings.useMatcap = false;
            settings.fallbackShadowRamp = new Texture2D(4, 4);
            var gen = new LilToonToonStandardGenerator(lilMat, settings, blackTex);
            var result = InvokeConvertToToonStandard(gen);
            try
            {
                Assert.IsNotNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(result);
            }
        }

        [Test]
        public void ConvertToToonStandard_RimDisabledInSettings_SkipsRim()
        {
            lilMat.Material.SetFloat("_UseRim", 1f);
            var settings = new ToonStandardConvertSettings();
            settings.SetAllFeatures(true);
            settings.useRimLighting = false;
            settings.fallbackShadowRamp = new Texture2D(4, 4);
            var gen = new LilToonToonStandardGenerator(lilMat, settings, blackTex);
            var result = InvokeConvertToToonStandard(gen);
            try
            {
                Assert.IsNotNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(result);
            }
        }

        [Test]
        public void ConvertToToonStandard_AllFeaturesDisabled_BasicMaterial()
        {
            var settings = new ToonStandardConvertSettings();
            settings.SetAllFeatures(false);
            settings.fallbackShadowRamp = new Texture2D(4, 4);
            var gen = new LilToonToonStandardGenerator(lilMat, settings, blackTex);
            var result = InvokeConvertToToonStandard(gen);
            try
            {
                Assert.IsNotNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(result);
            }
        }

        [Test]
        public void ConvertToToonStandard_EmissionWithEmission2ndOnly_SetsEmission2ndColor()
        {
            lilMat.Material.SetFloat("_UseEmission", 0f);
            lilMat.Material.SetFloat("_UseEmission2nd", 1f);
            lilMat.Material.SetColor("_Emission2ndColor", Color.blue);
            var settings = new ToonStandardConvertSettings();
            settings.SetAllFeatures(true);
            settings.fallbackShadowRamp = new Texture2D(4, 4);
            var gen = new LilToonToonStandardGenerator(lilMat, settings, blackTex);
            var result = InvokeConvertToToonStandard(gen);
            try
            {
                Assert.IsNotNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(result);
            }
        }

        [Test]
        public void ConvertToToonStandard_SpecularWithGlossAndMetallicMaps()
        {
            lilMat.Material.SetFloat("_UseReflection", 1f);
            lilMat.Material.SetTexture("_SmoothnessTex", testTex);
            lilMat.Material.SetTexture("_MetallicGlossMap", testTex);
            lilMat.Material.SetFloat("_Metallic", 0.5f);
            lilMat.Material.SetFloat("_Smoothness", 0.7f);
            lilMat.Material.SetFloat("_SpecularBlur", 0.2f);
            lilMat.Material.SetFloat("_Reflectance", 0.5f);
            var settings = new ToonStandardConvertSettings();
            settings.SetAllFeatures(true);
            settings.fallbackShadowRamp = new Texture2D(4, 4);
            var gen = new LilToonToonStandardGenerator(lilMat, settings, blackTex);
            var result = InvokeConvertToToonStandard(gen);
            try
            {
                Assert.IsNotNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(result);
            }
        }
    }
}
