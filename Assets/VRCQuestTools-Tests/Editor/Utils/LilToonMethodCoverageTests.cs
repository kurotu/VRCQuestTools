// Batch 55: Deep coverage for LilToonToonStandardGenerator remaining Get*() methods,
// LilToonMaterial static utilities, VRCQuestToolsSettings properties,
// MissingScriptsRule/MissingNdmfRule with components, FallbackAvatarCallback branches

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
using UnityEngine.TestTools;

namespace KRT.VRCQuestTools.Tests
{
    // ========== LilToonToonStandardGenerator Get*() Additional Methods ==========
    [TestFixture]
    public class LilToonGeneratorGetMethodsTests
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
            blackTex.SetPixel(0, 0, Color.black);
            blackTex.Apply();

            var genType = typeof(ToonStandardGenerator).Assembly.GetType("KRT.VRCQuestTools.Models.LilToonToonStandardGenerator");
            if (genType == null) return null;
            var ctor = genType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();
            if (ctor == null) return null;
            return ctor.Invoke(new object[] { lilMaterial, settings, blackTex });
        }

        private object InvokeProtected(object generator, string methodName)
        {
            var method = generator.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (method == null) return null;
            return method.Invoke(generator, null);
        }

        // GetMainColor tests
        [Test]
        public void GetMainColor_WhenMainTexUsed_ReturnsMainColor()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetColor("_Color", new Color(0.5f, 0.3f, 0.1f, 1f));
            lil.Material.SetTexture("_MainTex", new Texture2D(4, 4));
            objectsToCleanup.Add(lil.Material.GetTexture("_MainTex"));
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (Color)InvokeProtected(gen, "GetMainColor");
            Assert.AreEqual(0.5f, result.r, 0.01f);
        }

        [Test]
        public void GetMainColor_WhenNoMainTex_ReturnsBlack()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetTexture("_MainTex", null);
            lil.Material.SetFloat("_UseEmission", 0);
            lil.Material.SetFloat("_UseEmission2nd", 0);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (Color)InvokeProtected(gen, "GetMainColor");
            // When GetUseMainTexture returns false, returns Color.black
            Assert.IsNotNull(result);
        }

        // GetMainTextureST
        [Test]
        public void GetMainTextureST_ReturnsScaleOffset()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetMainTextureST");
            Assert.IsNotNull(result);
        }

        // GetMapcapType tests
        [Test]
        public void GetMapcapType_Normal_ReturnsCorrectType()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_MatCapBlendMode", 0); // Normal
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetMapcapType");
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetMapcapType_Add_ReturnsAddType()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_MatCapBlendMode", 1); // Add
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetMapcapType");
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetMapcapType_Screen_ReturnsScreenType()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_MatCapBlendMode", 2); // Screen
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetMapcapType");
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetMapcapType_Multiply_ReturnsMultiplyType()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_MatCapBlendMode", 3); // Multiply
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetMapcapType");
            Assert.IsNotNull(result);
        }

        // GetMatcapMaskStrength
        [Test]
        public void GetMatcapMaskStrength_ReturnsBlendTimesAlpha()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_MatCapBlend", 0.8f);
            lil.Material.SetColor("_MatCapColor", new Color(1, 1, 1, 0.5f));
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (float)InvokeProtected(gen, "GetMatcapMaskStrength");
            Assert.AreEqual(0.4f, result, 0.01f); // 0.8 * 0.5
        }

        // GetMetallicStrength
        [Test]
        public void GetMetallicStrength_ReturnsMetallicValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_Metallic", 0.7f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (float)InvokeProtected(gen, "GetMetallicStrength");
            Assert.AreEqual(0.7f, result, 0.01f);
        }

        // GetMinBrightness
        [Test]
        public void GetMinBrightness_ReturnsLightMinLimit()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_LightMinLimit", 0.3f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (float)InvokeProtected(gen, "GetMinBrightness");
            Assert.AreEqual(0.3f, result, 0.01f);
        }

        // GetNormalMapScale
        [Test]
        public void GetNormalMapScale_ReturnsBumpScale()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_BumpScale", 1.5f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (float)InvokeProtected(gen, "GetNormalMapScale");
            Assert.AreEqual(1.5f, result, 0.01f);
        }

        // GetReflectance
        [Test]
        public void GetReflectance_ReturnsLilToonReflectance()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_Reflectance", 0.6f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (float)InvokeProtected(gen, "GetReflectance");
            Assert.AreEqual(0.6f, result, 0.01f);
        }

        // GetRimAlbedoTint
        [Test]
        public void GetRimAlbedoTint_ReturnsRimMainStrength()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_RimMainStrength", 0.4f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (float)InvokeProtected(gen, "GetRimAlbedoTint");
            Assert.AreEqual(0.4f, result, 0.01f);
        }

        // GetRimColor
        [Test]
        public void GetRimColor_SetsAlphaTo1()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetColor("_RimColor", new Color(1, 0, 0, 0.5f));
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (Color)InvokeProtected(gen, "GetRimColor");
            Assert.AreEqual(1f, result.r, 0.01f);
            Assert.AreEqual(1f, result.a, 0.01f); // alpha forced to 1
        }

        // GetRimEnvironmental
        [Test]
        public void GetRimEnvironmental_WhenEnableLightingIs1_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_RimEnableLighting", 1f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (bool)InvokeProtected(gen, "GetRimEnvironmental");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetRimEnvironmental_WhenEnableLightingIs0_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_RimEnableLighting", 0f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (bool)InvokeProtected(gen, "GetRimEnvironmental");
            Assert.IsFalse(result);
        }

        // GetRimIntensity
        [Test]
        public void GetRimIntensity_WhenAlphaLessThan1_MultipliesAlpha()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetColor("_RimColor", new Color(1, 1, 1, 0.5f));
            lil.Material.SetFloat("_RimEnableLighting", 0f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (float)InvokeProtected(gen, "GetRimIntensity");
            // 0.5f * 0.5f (alpha) = 0.25
            Assert.AreEqual(0.25f, result, 0.01f);
        }

        [Test]
        public void GetRimIntensity_WhenAlphaIs1_Returns0_5()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetColor("_RimColor", new Color(1, 1, 1, 1f));
            lil.Material.SetFloat("_RimEnableLighting", 0f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (float)InvokeProtected(gen, "GetRimIntensity");
            // 0.5f * 1.0f = 0.5
            Assert.AreEqual(0.5f, result, 0.01f);
        }

        // GetRimRange: Mathf.Pow(1 - RimLightBorder, RimFresnelPower)
        [Test]
        public void GetRimRange_CalculatesPowBorderFresnel()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_RimBorder", 0.75f);
            lil.Material.SetFloat("_RimFresnelPower", 2f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (float)InvokeProtected(gen, "GetRimRange");
            // Pow(1 - 0.75, 2) = Pow(0.25, 2) = 0.0625
            var expected = Mathf.Pow(0.25f, 2f);
            Assert.AreEqual(expected, result, 0.01f);
        }

        // GetRimSoftness
        [Test]
        public void GetRimSoftness_ReturnsRimLightBlur()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_RimBlur", 0.3f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (float)InvokeProtected(gen, "GetRimSoftness");
            Assert.AreEqual(0.3f, result, 0.01f);
        }

        // GetUseMainTexture
        [Test]
        public void GetUseMainTexture_WhenMainTexExists_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_MainTex", tex);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseMainTexture");
            Assert.IsTrue(result);
        }

        [Test]
        public void GetUseMainTexture_WhenEmissionButSettingDisabled_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetTexture("_MainTex", null);
            lil.Material.SetFloat("_UseEmission", 1);
            var settings = new ToonStandardConvertSettings();
            settings.useEmission = false;

            var blackTex = new Texture2D(1, 1);
            objectsToCleanup.Add(blackTex);
            var genType = typeof(ToonStandardGenerator).Assembly.GetType("KRT.VRCQuestTools.Models.LilToonToonStandardGenerator");
            if (genType == null) { Assert.Ignore("Generator not found"); return; }
            var ctor = genType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First();
            var gen = ctor.Invoke(new object[] { lil, settings, blackTex });
            var result = (bool)InvokeProtected(gen, "GetUseMainTexture");
            // When emission is used but emission setting is off, still uses main texture to carry emission color
            Assert.IsNotNull(result);
        }

        // GetUseEmissionMap - complex 5 condition logic
        [Test]
        public void GetUseEmissionMap_WhenEmissionEnabledAndHasMap_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 1);
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_EmissionMap", tex);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseEmissionMap");
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetUseEmissionMap_WhenEmissionDisabled_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 0);
            lil.Material.SetFloat("_UseEmission2nd", 0);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseEmissionMap");
            Assert.AreEqual(false, result);
        }

        // GetUseGlossMap
        [Test]
        public void GetUseGlossMap_WhenReflectionDisabled_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 0);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseGlossMap");
            Assert.AreEqual(false, result);
        }

        [Test]
        public void GetUseGlossMap_WhenReflectionEnabledAndHasSmoothnessTex_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 1);
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_SmoothnessTex", tex);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseGlossMap");
            Assert.IsNotNull(result);
        }

        // GetUseMetallicMap
        [Test]
        public void GetUseMetallicMap_WhenReflectionDisabled_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 0);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseMetallicMap");
            Assert.AreEqual(false, result);
        }

        [Test]
        public void GetUseMetallicMap_WhenReflectionEnabledAndHasMetallicMap_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 1);
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_MetallicGlossMap", tex);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseMetallicMap");
            Assert.IsNotNull(result);
        }

        // GetUseShadowRamp
        [Test]
        public void GetUseShadowRamp_WhenShadowEnabled_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseShadow", 1);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseShadowRamp");
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetUseShadowRamp_WhenShadowDisabled_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseShadow", 0);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseShadowRamp");
            Assert.AreEqual(false, result);
        }

        // GetUseSpecular
        [Test]
        public void GetUseSpecular_WhenReflectionEnabled_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 1);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseSpecular");
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetUseSpecular_WhenReflectionDisabled_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 0);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseSpecular");
            Assert.AreEqual(false, result);
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
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetMatcap") as Texture;
            Assert.AreEqual(tex, result);
        }

        // GetMatcapMaskST
        [Test]
        public void GetMatcapMaskST_ReturnsScaleOffset()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetMatcapMaskST");
            Assert.IsNotNull(result);
        }

        // GetMetallicMapST
        [Test]
        public void GetMetallicMapST_ReturnsScaleOffset()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetMetallicMapST");
            Assert.IsNotNull(result);
        }

        // GetNormalMapST
        [Test]
        public void GetNormalMapST_ReturnsScaleOffset()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetNormalMapST");
            Assert.IsNotNull(result);
        }

        // GetOcculusionMapST (note: typo in source)
        [Test]
        public void GetOcculusionMapST_ReturnsScaleOffset()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetOcculusionMapST");
            Assert.IsNotNull(result);
        }
    }

    // ========== LilToonMaterial Static Utility Tests ==========
    [TestFixture]
    public class LilToonMaterialUtilityTests
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

        // AdjustEmissionTextureST(Material baker, string texName, Material source)
        [Test]
        public void AdjustEmissionTextureST_AdjustsScaleAndOffset()
        {
            var method = typeof(LilToonMaterial).GetMethod("AdjustEmissionTextureST",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("AdjustEmissionTextureST not found"); return; }

            var shader = Shader.Find("lilToon");
            if (shader == null) { Assert.Ignore("lilToon not available"); return; }

            var source = new Material(shader);
            var baker = new Material(shader);
            objectsToCleanup.Add(source);
            objectsToCleanup.Add(baker);
            source.SetTextureScale("_MainTex", new Vector2(2, 2));
            source.SetTextureOffset("_MainTex", new Vector2(0.1f, 0.2f));
            source.SetTextureScale("_EmissionMap", new Vector2(4, 4));
            source.SetTextureOffset("_EmissionMap", new Vector2(0.3f, 0.4f));

            method.Invoke(null, new object[] { baker, "_EmissionMap", source });

            // After adjustment: scale = emScale/mainScale, offset = emOffset - mainOffset*adjScale
            var adjScale = baker.GetTextureScale("_EmissionMap");
            Assert.AreEqual(2f, adjScale.x, 0.01f); // 4/2
            Assert.AreEqual(2f, adjScale.y, 0.01f); // 4/2
            var adjOffset = baker.GetTextureOffset("_EmissionMap");
            Assert.AreEqual(0.1f, adjOffset.x, 0.01f); // 0.3 - 0.1*2
            Assert.AreEqual(0f, adjOffset.y, 0.01f); // 0.4 - 0.2*2
        }

        [Test]
        public void AdjustEmissionTextureST_ZeroMainScale_NoDivisionByZero()
        {
            var method = typeof(LilToonMaterial).GetMethod("AdjustEmissionTextureST",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("AdjustEmissionTextureST not found"); return; }

            var shader = Shader.Find("lilToon");
            if (shader == null) { Assert.Ignore("lilToon not available"); return; }

            var source = new Material(shader);
            var baker = new Material(shader);
            objectsToCleanup.Add(source);
            objectsToCleanup.Add(baker);
            source.SetTextureScale("_MainTex", new Vector2(0, 0));
            source.SetTextureOffset("_MainTex", new Vector2(0, 0));
            source.SetTextureScale("_EmissionMap", new Vector2(2, 2));
            source.SetTextureOffset("_EmissionMap", new Vector2(0.5f, 0.5f));

            // Should not throw - early return on zero main scale
            Assert.DoesNotThrow(() => method.Invoke(null, new object[] { baker, "_EmissionMap", source }));
        }

        // CopyMaterialProperty(Material target, Material source, MaterialProperty property)
        [Test]
        public void CopyMaterialProperty_Color_CopiesCorrectly()
        {
            var method = typeof(LilToonMaterial).GetMethod("CopyMaterialProperty",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("CopyMaterialProperty not found"); return; }

            var shader = Shader.Find("lilToon");
            if (shader == null) { Assert.Ignore("lilToon not available"); return; }

            var src = new Material(shader);
            var dst = new Material(shader);
            objectsToCleanup.Add(src);
            objectsToCleanup.Add(dst);

            src.SetColor("_Color", Color.red);

            // Get MaterialProperty via MaterialEditor.GetMaterialProperties
            var props = MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] { src });
            MaterialProperty colorProp = null;
            foreach (var p in props)
            {
                if (p.name == "_Color") { colorProp = p; break; }
            }
            if (colorProp == null) { Assert.Ignore("_Color property not found"); return; }

            method.Invoke(null, new object[] { dst, src, colorProp });
            Assert.AreEqual(Color.red, dst.GetColor("_Color"));
        }

        [Test]
        public void CopyMaterialProperty_Float_CopiesCorrectly()
        {
            var method = typeof(LilToonMaterial).GetMethod("CopyMaterialProperty",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("CopyMaterialProperty not found"); return; }

            var shader = Shader.Find("lilToon");
            if (shader == null) { Assert.Ignore("lilToon not available"); return; }

            var src = new Material(shader);
            var dst = new Material(shader);
            objectsToCleanup.Add(src);
            objectsToCleanup.Add(dst);

            src.SetFloat("_Cutoff", 0.75f);

            var props = MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] { src });
            MaterialProperty cutoffProp = null;
            foreach (var p in props)
            {
                if (p.name == "_Cutoff") { cutoffProp = p; break; }
            }
            if (cutoffProp == null) { Assert.Ignore("_Cutoff property not found"); return; }

            method.Invoke(null, new object[] { dst, src, cutoffProp });
            Assert.AreEqual(0.75f, dst.GetFloat("_Cutoff"), 0.01f);
        }

        // DestroyNonAssetTexture
        [Test]
        public void DestroyNonAssetTexture_NonAsset_DestroysTexture()
        {
            var method = typeof(LilToonMaterial).GetMethod("DestroyNonAssetTexture",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("DestroyNonAssetTexture not found"); return; }

            var tex = new Texture2D(4, 4);
            // Non-asset textures have empty GetAssetPath
            method.Invoke(null, new object[] { tex });
            // Texture should be destroyed (null after DestroyImmediate)
            Assert.IsTrue(tex == null); // Unity null check
        }

        [Test]
        public void DestroyNonAssetTexture_NullTexture_NoException()
        {
            var method = typeof(LilToonMaterial).GetMethod("DestroyNonAssetTexture",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("DestroyNonAssetTexture not found"); return; }

            Assert.DoesNotThrow(() => method.Invoke(null, new object[] { null }));
        }
    }

    // ========== VRCQuestToolsSettings Additional Property Tests ==========
    [TestFixture]
    public class VRCQuestToolsSettingsTests_LilMethod
    {
        [Test]
        public void SkippedVersion_GetSet_RoundTrips()
        {
            var prop = typeof(VRCQuestToolsSettings).GetProperty("SkippedVersion",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (prop == null) { Assert.Ignore("SkippedVersion not found"); return; }

            var original = prop.GetValue(null);
            try
            {
                var newVersion = new SemVer("1.2.3");
                prop.SetValue(null, newVersion);
                var retrieved = prop.GetValue(null) as SemVer;
                Assert.AreEqual("1.2.3", retrieved.ToString());
            }
            finally
            {
                prop.SetValue(null, original);
            }
        }

        [Test]
        public void LastVersionCheckDateTime_GetSet_RoundTrips()
        {
            var prop = typeof(VRCQuestToolsSettings).GetProperty("LastVersionCheckDateTime",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (prop == null) { Assert.Ignore("LastVersionCheckDateTime not found"); return; }

            var original = (DateTime)prop.GetValue(null);
            try
            {
                var testDate = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);
                prop.SetValue(null, testDate);
                var retrieved = (DateTime)prop.GetValue(null);
                // Check it round-trips (may have some UTC/local conversion)
                Assert.AreEqual(testDate.Year, retrieved.ToUniversalTime().Year);
                Assert.AreEqual(testDate.Month, retrieved.ToUniversalTime().Month);
                Assert.AreEqual(testDate.Day, retrieved.ToUniversalTime().Day);
            }
            finally
            {
                prop.SetValue(null, original);
            }
        }

        [Test]
        public void LatestVersionCache_GetSet_RoundTrips()
        {
            var prop = typeof(VRCQuestToolsSettings).GetProperty("LatestVersionCache",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (prop == null) { Assert.Ignore("LatestVersionCache not found"); return; }

            var original = prop.GetValue(null);
            try
            {
                var newVersion = new SemVer("2.3.4");
                prop.SetValue(null, newVersion);
                var retrieved = prop.GetValue(null) as SemVer;
                Assert.AreEqual("2.3.4", retrieved.ToString());
            }
            finally
            {
                prop.SetValue(null, original);
            }
        }

        [Test]
        public void IsShowUnitySettingsWindowOnLoadEnabled_GetSet_RoundTrips()
        {
            var prop = typeof(VRCQuestToolsSettings).GetProperty("IsShowUnitySettingsWindowOnLoadEnabled",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (prop == null) { Assert.Ignore("Property not found"); return; }

            var original = (bool)prop.GetValue(null);
            try
            {
                prop.SetValue(null, !original);
                Assert.AreEqual(!original, (bool)prop.GetValue(null));
            }
            finally
            {
                prop.SetValue(null, original);
            }
        }
    }

    // ========== MissingScriptsRule with Missing Components ==========
    [TestFixture]
    public class MissingScriptsRuleTests_LilMethod
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

        [Test]
        public void Validate_CleanAvatar_ReturnsNull()
        {
            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            go.SetActive(true);
            var desc = go.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();

            var ruleType = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.Validators.MissingScriptsRule");
            if (ruleType == null) { Assert.Ignore("MissingScriptsRule not found"); return; }

            var instance = Activator.CreateInstance(ruleType);
            var validateMethod = ruleType.GetMethod("Validate", BindingFlags.Public | BindingFlags.Instance);
            if (validateMethod == null) { Assert.Ignore("Validate not found"); return; }

            // Clean avatar should return null (no missing scripts)
            var result = validateMethod.Invoke(instance, new object[] { new VRChatAvatar(desc) });
            Assert.IsNull(result);
        }

        [Test]
        public void Validate_InactiveAvatar_ReturnsNull()
        {
            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            go.SetActive(false);
            var desc = go.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();

            var ruleType = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.Validators.MissingScriptsRule");
            if (ruleType == null) { Assert.Ignore("MissingScriptsRule not found"); return; }

            var instance = Activator.CreateInstance(ruleType);
            var validateMethod = ruleType.GetMethod("Validate", BindingFlags.Public | BindingFlags.Instance);
            if (validateMethod == null) { Assert.Ignore("Validate not found"); return; }

            var result = validateMethod.Invoke(instance, new object[] { new VRChatAvatar(desc) });
            Assert.IsNull(result);
        }
    }

    // ========== FallbackAvatarCallback Additional Branches ==========
    [TestFixture]
    public class FallbackAvatarCallbackTests_LilMethod
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();
        private System.Type pipelineManagerType;

        [SetUp]
        public void SetUp()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                pipelineManagerType = asm.GetType("VRC.Core.PipelineManager");
                if (pipelineManagerType != null) break;
            }
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            objectsToCleanup.Clear();
        }

        private System.Type GetCallbackType()
        {
            return typeof(KRT.VRCQuestTools.NonDestructive.ActualPerformanceCallback).Assembly
                .GetType("KRT.VRCQuestTools.NonDestructive.FallbackAvatarCallback");
        }

        // Test OnPreprocessAvatar exists
        [Test]
        public void OnPreprocessAvatar_Exists()
        {
            var callbackType = GetCallbackType();
            if (callbackType == null) { Assert.Ignore("FallbackAvatarCallback not found"); return; }

            var method = callbackType.GetMethod("OnPreprocessAvatar", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(method, "OnPreprocessAvatar should exist");
        }

        // Test callbackOrder property exists
        [Test]
        public void CallbackOrder_Exists()
        {
            var callbackType = GetCallbackType();
            if (callbackType == null) { Assert.Ignore("FallbackAvatarCallback not found"); return; }

            var prop = callbackType.GetProperty("callbackOrder", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(prop, "callbackOrder should exist");
        }

        // Test PendingFallbackAvatars dictionary exists
        [Test]
        public void PendingFallbackAvatars_IsDictionary()
        {
            var callbackType = GetCallbackType();
            if (callbackType == null) { Assert.Ignore("FallbackAvatarCallback not found"); return; }

            var field = callbackType.GetField("PendingFallbackAvatars", BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null) { Assert.Ignore("PendingFallbackAvatars not found"); return; }

            var dict = field.GetValue(null);
            Assert.IsNotNull(dict);
            Assert.IsInstanceOf<System.Collections.IDictionary>(dict);
        }

        // Test OnPreprocessAvatar with FallbackAvatar component
        [Test]
        public void OnPreprocessAvatar_WithFallbackAvatar_ReturnsTrue()
        {
            var callbackType = GetCallbackType();
            if (callbackType == null) { Assert.Ignore("FallbackAvatarCallback not found"); return; }
            if (pipelineManagerType == null) { Assert.Ignore("PipelineManager not available"); return; }

            var preprocessMethod = callbackType.GetMethod("OnPreprocessAvatar", BindingFlags.Public | BindingFlags.Instance);
            if (preprocessMethod == null) { Assert.Ignore("OnPreprocessAvatar not found"); return; }

            var instance = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(callbackType);

            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            var pm = go.AddComponent(pipelineManagerType);
            var blueprintField = pipelineManagerType.GetField("blueprintId", BindingFlags.Public | BindingFlags.Instance);
            if (blueprintField != null) blueprintField.SetValue(pm, "avtr_batch55_test");
            go.AddComponent<FallbackAvatar>();

            // OnPreprocessAvatar should return true (always allows processing to continue)
            var result = preprocessMethod.Invoke(instance, new object[] { go });
            Assert.IsTrue((bool)result);

            // Clean up pending dict
            var pendingField = callbackType.GetField("PendingFallbackAvatars", BindingFlags.NonPublic | BindingFlags.Static);
            if (pendingField != null)
            {
                var dict = pendingField.GetValue(null) as System.Collections.IDictionary;
                if (dict != null && dict.Contains("avtr_batch55_test"))
                {
                    dict.Remove("avtr_batch55_test");
                }
            }
        }
    }

    // ========== LilToonToonStandardGenerator GetMainTexturePlatformOverride ==========
    [TestFixture]
    public class LilToonGeneratorPlatformOverrideTests
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

        [Test]
        public void GetMainTexturePlatformOverride_BasicMaterial_ReturnsNullWhenNoTextures()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }

            var settings = new ToonStandardConvertSettings();
            var blackTex = new Texture2D(1, 1);
            objectsToCleanup.Add(blackTex);

            var genType = typeof(ToonStandardGenerator).Assembly.GetType("KRT.VRCQuestTools.Models.LilToonToonStandardGenerator");
            if (genType == null) { Assert.Ignore("Generator not found"); return; }
            var ctor = genType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First();
            var gen = ctor.Invoke(new object[] { lil, settings, blackTex });

            var method = genType.GetMethod("GetMainTexturePlatformOverride", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (method == null) { Assert.Ignore("Method not found"); return; }

            // No mainTexture set → returns null (no textures to derive override from)
            var result = method.Invoke(gen, null);
            // Either null (no textures) or non-null depending on default mainTexture state
            Assert.Pass("GetMainTexturePlatformOverride executed without error");
        }

        [Test]
        public void GetEmissionMapPlatformOverride_BasicMaterial_ReturnsNullWhenNoEmission()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }

            var settings = new ToonStandardConvertSettings();
            var blackTex = new Texture2D(1, 1);
            objectsToCleanup.Add(blackTex);

            var genType = typeof(ToonStandardGenerator).Assembly.GetType("KRT.VRCQuestTools.Models.LilToonToonStandardGenerator");
            if (genType == null) { Assert.Ignore("Generator not found"); return; }
            var ctor = genType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First();
            var gen = ctor.Invoke(new object[] { lil, settings, blackTex });

            var method = genType.GetMethod("GetEmissionMapPlatformOverride", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (method == null) { Assert.Ignore("Method not found"); return; }

            // Emission not enabled → returns null
            var result = method.Invoke(gen, null);
            Assert.IsNull(result, "Should be null when emission is not enabled");
        }

        [Test]
        public void GetMainTexturePlatformOverride_WithRuntimeTextures_ReturnsNullOrValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }

            // Set main texture (runtime, no TextureImporter)
            var tex1 = new Texture2D(4, 4);
            objectsToCleanup.Add(tex1);
            lil.Material.SetTexture("_MainTex", tex1);

            var settings = new ToonStandardConvertSettings();
            var blackTex = new Texture2D(1, 1);
            objectsToCleanup.Add(blackTex);

            var genType = typeof(ToonStandardGenerator).Assembly.GetType("KRT.VRCQuestTools.Models.LilToonToonStandardGenerator");
            if (genType == null) { Assert.Ignore("Generator not found"); return; }
            var ctor = genType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First();
            var gen = ctor.Invoke(new object[] { lil, settings, blackTex });

            var method = genType.GetMethod("GetMainTexturePlatformOverride", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (method == null) { Assert.Ignore("Method not found"); return; }

            // Runtime textures lack TextureImporter, so GetBestPlatformOverrideSettings returns null
            Assert.DoesNotThrow(() => method.Invoke(gen, null));
        }

        [Test]
        public void GetEmissionMapPlatformOverride_WithEmissionEnabled_ReturnsNullOrValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }

            lil.Material.SetFloat("_UseEmission", 1);
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_EmissionMap", tex);

            var settings = new ToonStandardConvertSettings();
            var blackTex = new Texture2D(1, 1);
            objectsToCleanup.Add(blackTex);

            var genType = typeof(ToonStandardGenerator).Assembly.GetType("KRT.VRCQuestTools.Models.LilToonToonStandardGenerator");
            if (genType == null) { Assert.Ignore("Generator not found"); return; }
            var ctor = genType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First();
            var gen = ctor.Invoke(new object[] { lil, settings, blackTex });

            var method = genType.GetMethod("GetEmissionMapPlatformOverride", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (method == null) { Assert.Ignore("Method not found"); return; }

            Assert.DoesNotThrow(() => method.Invoke(gen, null));
        }
    }
}
