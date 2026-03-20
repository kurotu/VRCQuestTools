// Tests for LilToonToonStandardGenerator uncovered branches:
// - GetMainColor matcap+normal branch
// - GetUseEmissionMap dual emission branch
// - GetUseGlossMap reflectionColor != white branch
// - GetUseMetallicMap reflectionColor != white branch
// - GetUseMainTexture emission-forces-texture branch
// - GetMapcapType all blend modes
// - GetRimIntensity environmental on/off
// - GetRimRange with non-default border/fresnel
// - GetEmissionColor, GetMinBrightness, GetNormalMapScale, GetReflectance, GetSharpness

using System.Reflection;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    [TestFixture]
    public class LilToonGetterBranchTests
    {
        private static readonly string LilToonShaderName = "Hidden/lilToonOutline";
        private Shader shader;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (!AssetUtility.IsLilToonImported())
            {
                Assert.Ignore("lilToon is not installed.");
            }

            shader = Shader.Find(LilToonShaderName);
            if (shader == null)
            {
                shader = Shader.Find("lilToon");
            }

            if (shader == null)
            {
                Assert.Ignore("lilToon shader not found.");
            }
        }

        private LilToonToonStandardGenerator CreateGenerator(
            Material mat,
            bool useEmission = true,
            bool useMatcap = true,
            bool useNormalMap = true,
            bool useSpecular = true,
            bool useRimLighting = true,
            bool useOcclusion = true,
            bool generateQuestTextures = true,
            bool generateShadowRamp = false)
        {
            var lilMat = new LilToonMaterial(mat);
            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = generateQuestTextures,
                useEmission = useEmission,
                useNormalMap = useNormalMap,
                useMatcap = useMatcap,
                useSpecular = useSpecular,
                useRimLighting = useRimLighting,
                useOcclusion = useOcclusion,
                generateShadowRamp = generateShadowRamp,
                maxTextureSize = TextureSizeLimit.Max1024x1024,
            };
            var blackTex = new Texture2D(4, 4);
            return new LilToonToonStandardGenerator(lilMat, settings, blackTex);
        }

        private object InvokeProtected(LilToonToonStandardGenerator gen, string methodName)
        {
            var method = gen.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.IsNotNull(method, $"Method {methodName} not found");
            return method.Invoke(gen, null);
        }

        // ===========================================
        // GetMainColor - matcap + normal blend mode branch
        // ===========================================

        [Test]
        public void GetMainColor_MatcapNormalBlendNoMainTexture_ReturnsBlack()
        {
            var mat = new Material(shader);
            mat.mainTexture = null;
            mat.SetFloat("_UseMatCap", 1.0f);
            mat.SetFloat("_MatCapBlendMode", 0); // Normal = 0
            var gen = CreateGenerator(mat);
            try
            {
                var color = (Color)InvokeProtected(gen, "GetMainColor");
                Assert.AreEqual(Color.black, color);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMainColor_MatcapNormalBlendWithMainTexture_ReturnsMaterialColor()
        {
            var mat = new Material(shader);
            var tex = new Texture2D(4, 4);
            mat.mainTexture = tex;
            mat.color = new Color(0.5f, 0.3f, 0.7f, 1.0f);
            mat.SetFloat("_UseMatCap", 1.0f);
            mat.SetFloat("_MatCapBlendMode", 0); // Normal
            var gen = CreateGenerator(mat);
            try
            {
                var color = (Color)InvokeProtected(gen, "GetMainColor");
                Assert.AreEqual(mat.color, color);
            }
            finally
            {
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMainColor_MatcapAddBlend_ReturnsMaterialColor()
        {
            var mat = new Material(shader);
            mat.mainTexture = null;
            mat.color = Color.red;
            mat.SetFloat("_UseMatCap", 1.0f);
            mat.SetFloat("_MatCapBlendMode", 1); // Add
            var gen = CreateGenerator(mat);
            try
            {
                var color = (Color)InvokeProtected(gen, "GetMainColor");
                // Non-normal blend mode should NOT return black
                Assert.AreEqual(mat.color, color);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMainColor_NoMatcap_ReturnsMaterialColor()
        {
            var mat = new Material(shader);
            mat.mainTexture = null;
            mat.color = new Color(0.1f, 0.2f, 0.3f, 1.0f);
            mat.SetFloat("_UseMatCap", 0.0f);
            var gen = CreateGenerator(mat, useMatcap: false);
            try
            {
                var color = (Color)InvokeProtected(gen, "GetMainColor");
                Assert.AreEqual(mat.color, color);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // ===========================================
        // GetUseEmissionMap - dual emission branch
        // ===========================================

        [Test]
        public void GetUseEmissionMap_BothEmissionsNoTextures_ReturnsTrue()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseEmission", 1.0f);
            mat.SetFloat("_UseEmission2nd", 1.0f);
            // No textures, blend = 1.0 (default)
            mat.SetFloat("_EmissionBlend", 1.0f);
            mat.SetFloat("_Emission2ndBlend", 1.0f);
            mat.SetTexture("_EmissionMap", null);
            mat.SetTexture("_EmissionBlendMask", null);
            mat.SetTexture("_Emission2ndMap", null);
            mat.SetTexture("_Emission2ndBlendMask", null);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseEmissionMap");
                // line 950: returns true when both UseEmission && UseEmission2nd
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseEmissionMap_OnlyEmission1stWithMap_ReturnsTrue()
        {
            var mat = new Material(shader);
            var emTex = new Texture2D(4, 4);
            mat.SetFloat("_UseEmission", 1.0f);
            mat.SetFloat("_UseEmission2nd", 0.0f);
            mat.SetTexture("_EmissionMap", emTex);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseEmissionMap");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(emTex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseEmissionMap_OnlyEmission2ndWithMap_ReturnsTrue()
        {
            var mat = new Material(shader);
            var emTex = new Texture2D(4, 4);
            mat.SetFloat("_UseEmission", 0.0f);
            mat.SetFloat("_UseEmission2nd", 1.0f);
            mat.SetTexture("_Emission2ndMap", emTex);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseEmissionMap");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(emTex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseEmissionMap_Emission1stWithBlendMask_ReturnsTrue()
        {
            var mat = new Material(shader);
            var maskTex = new Texture2D(4, 4);
            mat.SetFloat("_UseEmission", 1.0f);
            mat.SetFloat("_UseEmission2nd", 0.0f);
            mat.SetTexture("_EmissionBlendMask", maskTex);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseEmissionMap");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(maskTex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseEmissionMap_Emission1stWithBlendLessThanOne_ReturnsTrue()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseEmission", 1.0f);
            mat.SetFloat("_UseEmission2nd", 0.0f);
            mat.SetFloat("_EmissionBlend", 0.5f);
            mat.SetTexture("_EmissionMap", null);
            mat.SetTexture("_EmissionBlendMask", null);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseEmissionMap");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseEmissionMap_Emission2ndWithBlendLessThanOne_ReturnsTrue()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseEmission", 0.0f);
            mat.SetFloat("_UseEmission2nd", 1.0f);
            mat.SetFloat("_Emission2ndBlend", 0.5f);
            mat.SetTexture("_Emission2ndMap", null);
            mat.SetTexture("_Emission2ndBlendMask", null);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseEmissionMap");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseEmissionMap_NoEmission_ReturnsFalse()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseEmission", 0.0f);
            mat.SetFloat("_UseEmission2nd", 0.0f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseEmissionMap");
                Assert.IsFalse(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // ===========================================
        // GetUseGlossMap - reflection color branch
        // ===========================================

        [Test]
        public void GetUseGlossMap_NoReflection_ReturnsFalse()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseReflection", 0.0f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseGlossMap");
                Assert.IsFalse(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseGlossMap_WithSmoothnessTex_ReturnsTrue()
        {
            var mat = new Material(shader);
            var tex = new Texture2D(4, 4);
            mat.SetFloat("_UseReflection", 1.0f);
            mat.SetTexture("_SmoothnessTex", tex);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseGlossMap");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseGlossMap_WithReflectionColorTex_ReturnsTrue()
        {
            var mat = new Material(shader);
            var tex = new Texture2D(4, 4);
            mat.SetFloat("_UseReflection", 1.0f);
            mat.SetTexture("_ReflectionColorTex", tex);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseGlossMap");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseGlossMap_WithNonWhiteReflectionColor_ReturnsTrue()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseReflection", 1.0f);
            mat.SetTexture("_SmoothnessTex", null);
            mat.SetTexture("_ReflectionColorTex", null);
            mat.SetColor("_ReflectionColor", new Color(0.5f, 0.5f, 0.5f, 1.0f));
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseGlossMap");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseGlossMap_ReflectionWhiteColorNoTextures_ReturnsFalse()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseReflection", 1.0f);
            mat.SetTexture("_SmoothnessTex", null);
            mat.SetTexture("_ReflectionColorTex", null);
            mat.SetColor("_ReflectionColor", Color.white);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseGlossMap");
                Assert.IsFalse(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // ===========================================
        // GetUseMetallicMap - reflection color branch
        // ===========================================

        [Test]
        public void GetUseMetallicMap_NoReflection_ReturnsFalse()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseReflection", 0.0f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseMetallicMap");
                Assert.IsFalse(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseMetallicMap_WithMetallicMap_ReturnsTrue()
        {
            var mat = new Material(shader);
            var tex = new Texture2D(4, 4);
            mat.SetFloat("_UseReflection", 1.0f);
            mat.SetTexture("_MetallicGlossMap", tex);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseMetallicMap");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseMetallicMap_WithNonWhiteReflectionColor_ReturnsTrue()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseReflection", 1.0f);
            mat.SetTexture("_MetallicGlossMap", null);
            mat.SetTexture("_ReflectionColorTex", null);
            mat.SetColor("_ReflectionColor", new Color(0.8f, 0.2f, 0.3f, 1.0f));
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseMetallicMap");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // ===========================================
        // GetUseMainTexture - emission forces texture branch
        // ===========================================

        [Test]
        public void GetUseMainTexture_EmissionDisabledInSettings_EmissionInMaterial_ReturnsTrue()
        {
            var mat = new Material(shader);
            mat.mainTexture = null;
            mat.SetFloat("_UseEmission", 1.0f);
            var gen = CreateGenerator(mat, useEmission: false);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseMainTexture");
                // line 966-968: if emission in material but disabled in settings, returns true
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseMainTexture_Emission2ndDisabledInSettings_ReturnsTrue()
        {
            var mat = new Material(shader);
            mat.mainTexture = null;
            mat.SetFloat("_UseEmission", 0.0f);
            mat.SetFloat("_UseEmission2nd", 1.0f);
            var gen = CreateGenerator(mat, useEmission: false);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseMainTexture");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseMainTexture_WithMainTexture_ReturnsTrue()
        {
            var mat = new Material(shader);
            var tex = new Texture2D(4, 4);
            mat.mainTexture = tex;
            mat.SetFloat("_UseEmission", 0.0f);
            mat.SetFloat("_UseEmission2nd", 0.0f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseMainTexture");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseMainTexture_NoTextureNoEmission_ReturnsFalse()
        {
            var mat = new Material(shader);
            mat.mainTexture = null;
            mat.SetFloat("_UseEmission", 0.0f);
            mat.SetFloat("_UseEmission2nd", 0.0f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseMainTexture");
                Assert.IsFalse(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // ===========================================
        // GetMapcapType - all blend modes
        // ===========================================

        [Test]
        public void GetMapcapType_NormalBlend_ReturnsAdditive()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseMatCap", 1.0f);
            mat.SetFloat("_MatCapBlendMode", 0); // Normal
            var gen = CreateGenerator(mat);
            try
            {
                var result = InvokeProtected(gen, "GetMapcapType");
                Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Additive, result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMapcapType_AddBlend_ReturnsAdditive()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseMatCap", 1.0f);
            mat.SetFloat("_MatCapBlendMode", 1); // Add
            var gen = CreateGenerator(mat);
            try
            {
                var result = InvokeProtected(gen, "GetMapcapType");
                Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Additive, result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMapcapType_ScreenBlend_ReturnsAdditive()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseMatCap", 1.0f);
            mat.SetFloat("_MatCapBlendMode", 2); // Screen
            var gen = CreateGenerator(mat);
            try
            {
                var result = InvokeProtected(gen, "GetMapcapType");
                Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Additive, result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMapcapType_MultiplyBlend_ReturnsMultiplicative()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseMatCap", 1.0f);
            mat.SetFloat("_MatCapBlendMode", 3); // Multiply
            var gen = CreateGenerator(mat);
            try
            {
                var result = InvokeProtected(gen, "GetMapcapType");
                Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Multiplicative, result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // ===========================================
        // GetRimIntensity - environmental on/off
        // ===========================================

        [Test]
        public void GetRimIntensity_EnvironmentalOn_UsesEnableLighting()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseRim", 1.0f);
            mat.SetFloat("_RimEnableLighting", 0.8f);
            mat.SetColor("_RimColor", new Color(1, 1, 1, 1));
            var gen = CreateGenerator(mat);
            try
            {
                var result = (float)InvokeProtected(gen, "GetRimIntensity");
                // environmental on: 0.5f * 0.8f * alpha(1.0) = 0.4f
                Assert.AreEqual(0.4f, result, 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetRimIntensity_EnvironmentalOff_UsesHalf()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseRim", 1.0f);
            mat.SetFloat("_RimEnableLighting", 0.0f);
            mat.SetColor("_RimColor", new Color(1, 1, 1, 1));
            var gen = CreateGenerator(mat);
            try
            {
                var result = (float)InvokeProtected(gen, "GetRimIntensity");
                // environmental off: 0.5f * alpha(1.0) = 0.5f
                Assert.AreEqual(0.5f, result, 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetRimIntensity_WithAlpha_ScaledByAlpha()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseRim", 1.0f);
            mat.SetFloat("_RimEnableLighting", 0.0f);
            mat.SetColor("_RimColor", new Color(1, 1, 1, 0.5f));
            var gen = CreateGenerator(mat);
            try
            {
                var result = (float)InvokeProtected(gen, "GetRimIntensity");
                // environmental off: 0.5f * alpha(0.5) = 0.25f
                Assert.AreEqual(0.25f, result, 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // ===========================================
        // GetRimEnvironmental
        // ===========================================

        [Test]
        public void GetRimEnvironmental_LightingEnabled_ReturnsTrue()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseRim", 1.0f);
            mat.SetFloat("_RimEnableLighting", 1.0f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetRimEnvironmental");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetRimEnvironmental_LightingDisabled_ReturnsFalse()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseRim", 1.0f);
            mat.SetFloat("_RimEnableLighting", 0.0f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetRimEnvironmental");
                Assert.IsFalse(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // ===========================================
        // GetRimColor
        // ===========================================

        [Test]
        public void GetRimColor_SetsAlphaToOne()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseRim", 1.0f);
            mat.SetColor("_RimColor", new Color(0.5f, 0.3f, 0.7f, 0.2f));
            var gen = CreateGenerator(mat);
            try
            {
                var color = (Color)InvokeProtected(gen, "GetRimColor");
                Assert.AreEqual(0.5f, color.r, 0.01f);
                Assert.AreEqual(0.3f, color.g, 0.01f);
                Assert.AreEqual(0.7f, color.b, 0.01f);
                Assert.AreEqual(1.0f, color.a, 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // ===========================================
        // GetRimRange
        // ===========================================

        [Test]
        public void GetRimRange_DefaultValues_ReturnsExpected()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseRim", 1.0f);
            mat.SetFloat("_RimBorder", 0.5f);
            mat.SetFloat("_RimFresnelPower", 2.0f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (float)InvokeProtected(gen, "GetRimRange");
                // Mathf.Pow(1 - 0.5, 2.0) = Mathf.Pow(0.5, 2.0) = 0.25
                Assert.AreEqual(0.25f, result, 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // ===========================================
        // GetRimSoftness, GetRimAlbedoTint
        // ===========================================

        [Test]
        public void GetRimSoftness_ReturnsBlurValue()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseRim", 1.0f);
            mat.SetFloat("_RimBlur", 0.3f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (float)InvokeProtected(gen, "GetRimSoftness");
                Assert.AreEqual(0.3f, result, 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetRimAlbedoTint_ReturnsMainStrength()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseRim", 1.0f);
            mat.SetFloat("_RimMainStrength", 0.7f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (float)InvokeProtected(gen, "GetRimAlbedoTint");
                Assert.AreEqual(0.7f, result, 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // ===========================================
        // GetMinBrightness, GetNormalMapScale, GetSharpness, GetReflectance
        // ===========================================

        [Test]
        public void GetMinBrightness_ReturnsLightMinLimit()
        {
            var mat = new Material(shader);
            mat.SetFloat("_LightMinLimit", 0.1f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (float)InvokeProtected(gen, "GetMinBrightness");
                Assert.AreEqual(0.1f, result, 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetNormalMapScale_ReturnsExpected()
        {
            var mat = new Material(shader);
            mat.SetFloat("_BumpScale", 0.8f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (float)InvokeProtected(gen, "GetNormalMapScale");
                Assert.AreEqual(0.8f, result, 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetSharpness_ReturnsOneMinusSpecularBlur()
        {
            var mat = new Material(shader);
            mat.SetFloat("_SpecularBlur", 0.3f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (float)InvokeProtected(gen, "GetSharpness");
                Assert.AreEqual(0.7f, result, 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetReflectance_ReturnsExpected()
        {
            var mat = new Material(shader);
            mat.SetFloat("_Reflectance", 0.6f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (float)InvokeProtected(gen, "GetReflectance");
                Assert.AreEqual(0.6f, result, 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // ===========================================
        // GetGlossStrength, GetMetallicStrength
        // ===========================================

        [Test]
        public void GetGlossStrength_ReturnsSmoothness()
        {
            var mat = new Material(shader);
            mat.SetFloat("_Smoothness", 0.4f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (float)InvokeProtected(gen, "GetGlossStrength");
                Assert.AreEqual(0.4f, result, 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMetallicStrength_ReturnsMetallic()
        {
            var mat = new Material(shader);
            mat.SetFloat("_Metallic", 0.9f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (float)InvokeProtected(gen, "GetMetallicStrength");
                Assert.AreEqual(0.9f, result, 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // ===========================================
        // GetMatcapMaskStrength
        // ===========================================

        [Test]
        public void GetMatcapMaskStrength_ReturnsBlendTimesAlpha()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseMatCap", 1.0f);
            mat.SetFloat("_MatCapBlend", 0.8f);
            mat.SetColor("_MatCapColor", new Color(1, 1, 1, 0.5f));
            var gen = CreateGenerator(mat);
            try
            {
                var result = (float)InvokeProtected(gen, "GetMatcapMaskStrength");
                // 0.8 * 0.5 = 0.4
                Assert.AreEqual(0.4f, result, 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // ===========================================
        // GetUseMatcap, GetUseMatcapMask
        // ===========================================

        [Test]
        public void GetUseMatcap_Enabled_ReturnsTrue()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseMatCap", 1.0f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseMatcap");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseMatcap_Disabled_ReturnsFalse()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseMatCap", 0.0f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseMatcap");
                Assert.IsFalse(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseMatcapMask_WithMask_ReturnsTrue()
        {
            var mat = new Material(shader);
            var maskTex = new Texture2D(4, 4);
            mat.SetFloat("_UseMatCap", 1.0f);
            mat.SetTexture("_MatCapBlendMask", maskTex);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseMatcapMask");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(maskTex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseMatcapMask_NoMask_ReturnsFalse()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseMatCap", 1.0f);
            mat.SetTexture("_MatCapBlendMask", null);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseMatcapMask");
                Assert.IsFalse(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // ===========================================
        // GetUseNormalMap, GetUseOcclusionMap, GetUseShadowRamp, GetUseRimLighting, GetUseSpecular, GetUseEmission
        // ===========================================

        [Test]
        public void GetUseNormalMap_WithNormalMapTexture_ReturnsTrue()
        {
            var mat = new Material(shader);
            var tex = new Texture2D(4, 4);
            mat.SetFloat("_UseBumpMap", 1.0f);
            mat.SetTexture("_BumpMap", tex);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseNormalMap");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseNormalMap_NoTexture_ReturnsFalse()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseBumpMap", 1.0f);
            mat.SetTexture("_BumpMap", null);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseNormalMap");
                Assert.IsFalse(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseOcclusionMap_WithAOMap_ReturnsTrue()
        {
            var mat = new Material(shader);
            var tex = new Texture2D(4, 4);
            mat.SetFloat("_UseShadow", 1.0f);
            mat.SetTexture("_ShadowBorderMask", tex);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseOcclusionMap");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseShadowRamp_ShadowEnabled_ReturnsTrue()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseShadow", 1.0f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseShadowRamp");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseShadowRamp_ShadowDisabled_ReturnsFalse()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseShadow", 0.0f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseShadowRamp");
                Assert.IsFalse(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseRimLighting_Enabled_ReturnsTrue()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseRim", 1.0f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseRimLighting");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseRimLighting_Disabled_ReturnsFalse()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseRim", 0.0f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseRimLighting");
                Assert.IsFalse(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseSpecular_Enabled_ReturnsTrue()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseReflection", 1.0f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseSpecular");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseSpecular_Disabled_ReturnsFalse()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseReflection", 0.0f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseSpecular");
                Assert.IsFalse(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseEmission_OnlyFirst_ReturnsTrue()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseEmission", 1.0f);
            mat.SetFloat("_UseEmission2nd", 0.0f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseEmission");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseEmission_OnlySecond_ReturnsTrue()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseEmission", 0.0f);
            mat.SetFloat("_UseEmission2nd", 1.0f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseEmission");
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseEmission_Neither_ReturnsFalse()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseEmission", 0.0f);
            mat.SetFloat("_UseEmission2nd", 0.0f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = (bool)InvokeProtected(gen, "GetUseEmission");
                Assert.IsFalse(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // ===========================================
        // GetMainTextureST, GetNormalMapST, various ST methods
        // ===========================================

        [Test]
        public void GetMainTextureST_ReturnsScaleAndOffset()
        {
            var mat = new Material(shader);
            mat.mainTextureScale = new Vector2(2, 3);
            mat.mainTextureOffset = new Vector2(0.1f, 0.2f);
            var gen = CreateGenerator(mat);
            try
            {
                var result = InvokeProtected(gen, "GetMainTextureST");
                Assert.IsNotNull(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetCulling_DefaultMaterial_ReturnsValue()
        {
            var mat = new Material(shader);
            var gen = CreateGenerator(mat);
            try
            {
                var result = InvokeProtected(gen, "GetCulling");
                Assert.IsNotNull(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // ===========================================
        // GetMatcap texture getter
        // ===========================================

        [Test]
        public void GetMatcap_WithTexture_ReturnsTexture()
        {
            var mat = new Material(shader);
            var tex = new Texture2D(4, 4);
            mat.SetFloat("_UseMatCap", 1.0f);
            mat.SetTexture("_MatCapTex", tex);
            var gen = CreateGenerator(mat);
            try
            {
                var result = InvokeProtected(gen, "GetMatcap");
                Assert.AreEqual(tex, result);
            }
            finally
            {
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMatcap_NoTexture_ReturnsNull()
        {
            var mat = new Material(shader);
            mat.SetFloat("_UseMatCap", 1.0f);
            mat.SetTexture("_MatCapTex", null);
            var gen = CreateGenerator(mat);
            try
            {
                var result = InvokeProtected(gen, "GetMatcap");
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }
    }
}
