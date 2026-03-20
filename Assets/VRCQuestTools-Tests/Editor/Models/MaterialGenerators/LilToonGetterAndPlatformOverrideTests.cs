// Tests for LilToonToonStandardGenerator protected getter methods and platform override methods.

using System;
using System.Reflection;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Tests for LilToonToonStandardGenerator protected getter methods
    /// covering culling, gloss, metallic, rim, matcap, normal, reflection, ST getters,
    /// and platform override methods.
    /// </summary>
    [TestFixture]
    public class LilToonGetterAndPlatformOverrideTests
    {
        private static bool isLilToonAvailable;
        private static Shader lilToonShader;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (!AssetUtility.IsLilToonImported())
            {
                return;
            }

            var lilToonVersion = AssetUtility.LilToonVersion;
            if (lilToonVersion < new SemVer(1, 10, 0) || lilToonVersion >= new SemVer(3, 0, 0))
            {
                return;
            }

            lilToonShader = Shader.Find("lilToon");
            var toonStandardShader = Shader.Find("VRChat/Mobile/Toon Standard");
            isLilToonAvailable = lilToonShader != null && toonStandardShader != null;
        }

        [SetUp]
        public void SetUp()
        {
            if (!isLilToonAvailable)
            {
                Assert.Ignore("lilToon or Toon Standard shader not available.");
            }
        }

        private LilToonToonStandardGenerator CreateGenerator(Material sourceMaterial, ToonStandardConvertSettings settings = null)
        {
            if (settings == null)
            {
                settings = new ToonStandardConvertSettings
                {
                    generateQuestTextures = false,
                    useNormalMap = true,
                    useEmission = true,
                    useOcclusion = true,
                    useSpecular = true,
                    useMatcap = true,
                    useRimLighting = true,
                    generateShadowRamp = false,
                };
            }
            var lilMat = new LilToonMaterial(sourceMaterial);
            return new LilToonToonStandardGenerator(lilMat, settings, null);
        }

        private object InvokeProtected(LilToonToonStandardGenerator gen, string methodName)
        {
            var method = typeof(LilToonToonStandardGenerator).GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, $"Method {methodName} not found");
            return method.Invoke(gen, null);
        }

        private object InvokeProtectedWithArgs(LilToonToonStandardGenerator gen, string methodName, object[] args, Type[] paramTypes)
        {
            var method = typeof(LilToonToonStandardGenerator).GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Instance,
                null, paramTypes, null);
            Assert.IsNotNull(method, $"Method {methodName} not found");
            return method.Invoke(gen, args);
        }

        #region GetCulling Tests

        [Test]
        public void GetCulling_Default_ReturnsCullMode()
        {
            var mat = new Material(lilToonShader);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (CullMode)InvokeProtected(gen, "GetCulling");
                Assert.IsTrue(Enum.IsDefined(typeof(CullMode), result));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetCulling_SetToFront_ReturnsFront()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_Cull", (float)CullMode.Front);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (CullMode)InvokeProtected(gen, "GetCulling");
                Assert.AreEqual(CullMode.Front, result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetCulling_SetToOff_ReturnsOff()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_Cull", (float)CullMode.Off);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (CullMode)InvokeProtected(gen, "GetCulling");
                Assert.AreEqual(CullMode.Off, result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region GetGlossStrength Tests

        [Test]
        public void GetGlossStrength_ReturnsSmoothnessValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_Smoothness", 0.75f);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (float)InvokeProtected(gen, "GetGlossStrength");
                Assert.AreEqual(0.75f, result, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetGlossStrength_Zero_ReturnsZero()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_Smoothness", 0f);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (float)InvokeProtected(gen, "GetGlossStrength");
                Assert.AreEqual(0f, result, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region GetMetallicStrength Tests

        [Test]
        public void GetMetallicStrength_ReturnsMetallicValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_Metallic", 0.6f);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (float)InvokeProtected(gen, "GetMetallicStrength");
                Assert.AreEqual(0.6f, result, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region GetReflectance Tests

        [Test]
        public void GetReflectance_ReturnsReflectanceValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_Reflectance", 0.5f);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (float)InvokeProtected(gen, "GetReflectance");
                Assert.AreEqual(0.5f, result, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region GetNormalMapScale Tests

        [Test]
        public void GetNormalMapScale_ReturnsBumpScaleValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_BumpScale", 1.5f);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (float)InvokeProtected(gen, "GetNormalMapScale");
                Assert.AreEqual(1.5f, result, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region GetRimColor Tests

        [Test]
        public void GetRimColor_ReturnsColorWithAlphaOne()
        {
            var mat = new Material(lilToonShader);
            mat.SetColor("_RimColor", new Color(0.5f, 0.3f, 0.7f, 0.2f));
            try
            {
                var gen = CreateGenerator(mat);
                var result = (Color)InvokeProtected(gen, "GetRimColor");
                Assert.AreEqual(0.5f, result.r, 0.001f);
                Assert.AreEqual(0.3f, result.g, 0.001f);
                Assert.AreEqual(0.7f, result.b, 0.001f);
                Assert.AreEqual(1.0f, result.a, 0.001f, "Alpha should always be 1.0");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region GetRimAlbedoTint Tests

        [Test]
        public void GetRimAlbedoTint_ReturnsRimMainStrength()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_RimMainStrength", 0.8f);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (float)InvokeProtected(gen, "GetRimAlbedoTint");
                Assert.AreEqual(0.8f, result, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region GetRimIntensity Tests

        [Test]
        public void GetRimIntensity_WithEnvironmentalLighting_UsesEnableLighting()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_RimEnableLighting", 0.8f);
            mat.SetColor("_RimColor", new Color(1, 1, 1, 1));
            try
            {
                var gen = CreateGenerator(mat);
                var result = (float)InvokeProtected(gen, "GetRimIntensity");
                // intensity = 0.5 * 0.8 * 1.0 = 0.4
                Assert.AreEqual(0.4f, result, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetRimIntensity_WithoutEnvironmentalLighting_UsesHalf()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_RimEnableLighting", 0f);
            mat.SetColor("_RimColor", new Color(1, 1, 1, 0.6f));
            try
            {
                var gen = CreateGenerator(mat);
                var result = (float)InvokeProtected(gen, "GetRimIntensity");
                // intensity = 0.5 * 0.6 = 0.3
                Assert.AreEqual(0.3f, result, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region GetRimRange Tests

        [Test]
        public void GetRimRange_ComputesPowCorrectly()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_RimBorder", 0.5f);
            mat.SetFloat("_RimFresnelPower", 2f);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (float)InvokeProtected(gen, "GetRimRange");
                // Mathf.Pow(1-0.5, 2) = Mathf.Pow(0.5, 2) = 0.25
                Assert.AreEqual(0.25f, result, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region GetRimSoftness Tests

        [Test]
        public void GetRimSoftness_ReturnsRimBlurValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_RimBlur", 0.4f);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (float)InvokeProtected(gen, "GetRimSoftness");
                Assert.AreEqual(0.4f, result, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region GetMatcap Tests

        [Test]
        public void GetMatcap_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetTexture("_MatCapTex", tex);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (Texture)InvokeProtected(gen, "GetMatcap");
                Assert.AreEqual(tex, result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tex);
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMatcap_WithoutTexture_ReturnsNull()
        {
            var mat = new Material(lilToonShader);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (Texture)InvokeProtected(gen, "GetMatcap");
                Assert.IsNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region GetMatcapMaskStrength Tests

        [Test]
        public void GetMatcapMaskStrength_ReturnsBlendTimesAlpha()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_MatCapBlend", 0.8f);
            mat.SetColor("_MatCapColor", new Color(1, 1, 1, 0.5f));
            try
            {
                var gen = CreateGenerator(mat);
                var result = (float)InvokeProtected(gen, "GetMatcapMaskStrength");
                // 0.8 * 0.5 = 0.4
                Assert.AreEqual(0.4f, result, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region GetMapcapType Tests

        [Test]
        public void GetMapcapType_NormalMode_ReturnsAdditive()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_MatCapBlendMode", 0); // Normal
            try
            {
                var gen = CreateGenerator(mat);
                var result = InvokeProtected(gen, "GetMapcapType");
                Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Additive, result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMapcapType_AddMode_ReturnsAdditive()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_MatCapBlendMode", 1); // Add
            try
            {
                var gen = CreateGenerator(mat);
                var result = InvokeProtected(gen, "GetMapcapType");
                Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Additive, result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMapcapType_ScreenMode_ReturnsAdditive()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_MatCapBlendMode", 2); // Screen
            try
            {
                var gen = CreateGenerator(mat);
                var result = InvokeProtected(gen, "GetMapcapType");
                Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Additive, result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMapcapType_MultiplyMode_ReturnsMultiplicative()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_MatCapBlendMode", 3); // Multiply
            try
            {
                var gen = CreateGenerator(mat);
                var result = InvokeProtected(gen, "GetMapcapType");
                Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Multiplicative, result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region GetMainColor Tests

        [Test]
        public void GetMainColor_WithMatCapNormalAndNoMainTexture_ReturnsBlack()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseMatCap", 1);
            mat.SetFloat("_MatCapBlendMode", 0); // Normal
            mat.mainTexture = null;
            try
            {
                var gen = CreateGenerator(mat);
                var result = (Color)InvokeProtected(gen, "GetMainColor");
                Assert.AreEqual(Color.black, result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMainColor_WithMatCapNormalAndMainTexture_ReturnsMaterialColor()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetFloat("_UseMatCap", 1);
            mat.SetFloat("_MatCapBlendMode", 0); // Normal
            mat.mainTexture = tex;
            mat.color = new Color(0.5f, 0.6f, 0.7f, 1f);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (Color)InvokeProtected(gen, "GetMainColor");
                Assert.AreEqual(0.5f, result.r, 0.001f);
                Assert.AreEqual(0.6f, result.g, 0.001f);
                Assert.AreEqual(0.7f, result.b, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tex);
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region ST (Scale/Offset) Getter Tests

        [Test]
        public void GetGlossMapST_ReturnsSmoothnessTexScaleOffset()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureScale("_SmoothnessTex", new Vector2(2f, 3f));
            mat.SetTextureOffset("_SmoothnessTex", new Vector2(0.1f, 0.2f));
            try
            {
                var gen = CreateGenerator(mat);
                var result = (ValueTuple<Vector2, Vector2>)InvokeProtected(gen, "GetGlossMapST");
                Assert.AreEqual(2f, result.Item1.x, 0.001f);
                Assert.AreEqual(3f, result.Item1.y, 0.001f);
                Assert.AreEqual(0.1f, result.Item2.x, 0.001f);
                Assert.AreEqual(0.2f, result.Item2.y, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMainTextureST_ReturnsMainTexScaleOffset()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureScale("_MainTex", new Vector2(1.5f, 2.5f));
            mat.SetTextureOffset("_MainTex", new Vector2(0.3f, 0.4f));
            try
            {
                var gen = CreateGenerator(mat);
                var result = (ValueTuple<Vector2, Vector2>)InvokeProtected(gen, "GetMainTextureST");
                Assert.AreEqual(1.5f, result.Item1.x, 0.001f);
                Assert.AreEqual(2.5f, result.Item1.y, 0.001f);
                Assert.AreEqual(0.3f, result.Item2.x, 0.001f);
                Assert.AreEqual(0.4f, result.Item2.y, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMetallicMapST_ReturnsMetallicGlossMapScaleOffset()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureScale("_MetallicGlossMap", new Vector2(4f, 5f));
            mat.SetTextureOffset("_MetallicGlossMap", new Vector2(0.5f, 0.6f));
            try
            {
                var gen = CreateGenerator(mat);
                var result = (ValueTuple<Vector2, Vector2>)InvokeProtected(gen, "GetMetallicMapST");
                Assert.AreEqual(4f, result.Item1.x, 0.001f);
                Assert.AreEqual(5f, result.Item1.y, 0.001f);
                Assert.AreEqual(0.5f, result.Item2.x, 0.001f);
                Assert.AreEqual(0.6f, result.Item2.y, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMatcapMaskST_ReturnsMatCapBlendMaskScaleOffset()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureScale("_MatCapBlendMask", new Vector2(1f, 2f));
            mat.SetTextureOffset("_MatCapBlendMask", new Vector2(0.7f, 0.8f));
            try
            {
                var gen = CreateGenerator(mat);
                var result = (ValueTuple<Vector2, Vector2>)InvokeProtected(gen, "GetMatcapMaskST");
                Assert.AreEqual(1f, result.Item1.x, 0.001f);
                Assert.AreEqual(2f, result.Item1.y, 0.001f);
                Assert.AreEqual(0.7f, result.Item2.x, 0.001f);
                Assert.AreEqual(0.8f, result.Item2.y, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetNormalMapST_ReturnsBumpMapScaleOffset()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureScale("_BumpMap", new Vector2(3f, 4f));
            mat.SetTextureOffset("_BumpMap", new Vector2(0.1f, 0.9f));
            try
            {
                var gen = CreateGenerator(mat);
                var result = (ValueTuple<Vector2, Vector2>)InvokeProtected(gen, "GetNormalMapST");
                Assert.AreEqual(3f, result.Item1.x, 0.001f);
                Assert.AreEqual(4f, result.Item1.y, 0.001f);
                Assert.AreEqual(0.1f, result.Item2.x, 0.001f);
                Assert.AreEqual(0.9f, result.Item2.y, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetOcculusionMapST_ReturnsShadowBorderMaskScaleOffset()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureScale("_ShadowBorderMask", new Vector2(2f, 1f));
            mat.SetTextureOffset("_ShadowBorderMask", new Vector2(0.2f, 0.3f));
            try
            {
                var gen = CreateGenerator(mat);
                var result = (ValueTuple<Vector2, Vector2>)InvokeProtected(gen, "GetOcculusionMapST");
                Assert.AreEqual(2f, result.Item1.x, 0.001f);
                Assert.AreEqual(1f, result.Item1.y, 0.001f);
                Assert.AreEqual(0.2f, result.Item2.x, 0.001f);
                Assert.AreEqual(0.3f, result.Item2.y, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region GetEmissionColor Additional Tests

        [Test]
        public void GetEmissionColor_OnlySecondEmission_ReturnsSecondColor()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission", 0);
            mat.SetFloat("_UseEmission2nd", 1);
            mat.SetColor("_Emission2ndColor", new Color(0, 1, 0, 1));
            try
            {
                var gen = CreateGenerator(mat);
                var result = (Color)InvokeProtected(gen, "GetEmissionColor");
                Assert.AreEqual(new Color(0, 1, 0, 1), result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region GetMinBrightness Tests

        [Test]
        public void GetMinBrightness_ReturnsLightMinLimit()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_LightMinLimit", 0.3f);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (float)InvokeProtected(gen, "GetMinBrightness");
                Assert.AreEqual(0.3f, result, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region GetSharpness Tests

        [Test]
        public void GetSharpness_ReturnsOneMinusSpecularBlur()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_SpecularBlur", 0.3f);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (float)InvokeProtected(gen, "GetSharpness");
                Assert.AreEqual(0.7f, result, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region GetRimEnvironmental Tests

        [Test]
        public void GetRimEnvironmental_WithLighting_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_RimEnableLighting", 0.5f);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (bool)InvokeProtected(gen, "GetRimEnvironmental");
                Assert.IsTrue(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetRimEnvironmental_WithoutLighting_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_RimEnableLighting", 0f);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (bool)InvokeProtected(gen, "GetRimEnvironmental");
                Assert.IsFalse(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region Platform Override Tests

        [Test]
        public void GetMainTexturePlatformOverride_NoTextures_ReturnsNull()
        {
            var mat = new Material(lilToonShader);
            mat.mainTexture = null;
            try
            {
                var gen = CreateGenerator(mat);
                var result = InvokeProtected(gen, "GetMainTexturePlatformOverride");
                Assert.IsNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetEmissionMapPlatformOverride_NoEmission_ReturnsNull()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission", 0);
            mat.SetFloat("_UseEmission2nd", 0);
            try
            {
                var gen = CreateGenerator(mat);
                var result = InvokeProtected(gen, "GetEmissionMapPlatformOverride");
                Assert.IsNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetGlossMapPlatformOverride_NoTextures_ReturnsNull()
        {
            var mat = new Material(lilToonShader);
            try
            {
                var gen = CreateGenerator(mat);
                var result = InvokeProtected(gen, "GetGlossMapPlatformOverride");
                Assert.IsNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMatcapPlatformOverride_NoTexture_ReturnsNull()
        {
            var mat = new Material(lilToonShader);
            try
            {
                var gen = CreateGenerator(mat);
                var result = InvokeProtected(gen, "GetMatcapPlatformOverride");
                Assert.IsNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMatcapMaskPlatformOverride_NoTexture_ReturnsNull()
        {
            var mat = new Material(lilToonShader);
            try
            {
                var gen = CreateGenerator(mat);
                var result = InvokeProtected(gen, "GetMatcapMaskPlatformOverride");
                Assert.IsNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMetallicMapPlatformOverride_NoTexture_ReturnsNull()
        {
            var mat = new Material(lilToonShader);
            try
            {
                var gen = CreateGenerator(mat);
                var result = InvokeProtected(gen, "GetMetallicMapPlatformOverride");
                Assert.IsNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetNormalMapPlatformOverride_NoTexture_ReturnsNull()
        {
            var mat = new Material(lilToonShader);
            try
            {
                var gen = CreateGenerator(mat);
                var result = InvokeProtected(gen, "GetNormalMapPlatformOverride");
                Assert.IsNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetOcclusionMapPlatformOverride_NoTexture_ReturnsNull()
        {
            var mat = new Material(lilToonShader);
            try
            {
                var gen = CreateGenerator(mat);
                var result = InvokeProtected(gen, "GetOcclusionMapPlatformOverride");
                Assert.IsNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region Boolean Getter Additional Tests

        [Test]
        public void GetUseGlossMap_WithReflectionAndSmoothnessTexture_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetFloat("_UseReflection", 1);
            mat.SetTexture("_SmoothnessTex", tex);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (bool)InvokeProtected(gen, "GetUseGlossMap");
                Assert.IsTrue(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tex);
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseGlossMap_WithReflectionAndNonWhiteReflectionColor_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseReflection", 1);
            mat.SetColor("_ReflectionColor", new Color(0.5f, 0.5f, 0.5f, 1));
            try
            {
                var gen = CreateGenerator(mat);
                var result = (bool)InvokeProtected(gen, "GetUseGlossMap");
                Assert.IsTrue(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseMetallicMap_WithReflectionAndReflectionColorTex_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetFloat("_UseReflection", 1);
            mat.SetTexture("_ReflectionColorTex", tex);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (bool)InvokeProtected(gen, "GetUseMetallicMap");
                Assert.IsTrue(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tex);
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseMetallicMap_WithNoReflection_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseReflection", 0);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (bool)InvokeProtected(gen, "GetUseMetallicMap");
                Assert.IsFalse(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseEmissionMap_WithEmissionAndBlendLessThanOne_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission", 1);
            mat.SetFloat("_EmissionBlend", 0.5f);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (bool)InvokeProtected(gen, "GetUseEmissionMap");
                Assert.IsTrue(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseEmissionMap_With2ndEmissionAndBlendMask_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetFloat("_UseEmission", 0);
            mat.SetFloat("_UseEmission2nd", 1);
            mat.SetTexture("_Emission2ndBlendMask", tex);
            try
            {
                var gen = CreateGenerator(mat);
                var result = (bool)InvokeProtected(gen, "GetUseEmissionMap");
                Assert.IsTrue(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tex);
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseMainTexture_EmissionDisabledSetting_WithEmission_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission", 1);
            mat.mainTexture = null;
            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
                useEmission = false,
            };
            try
            {
                var gen = CreateGenerator(mat, settings);
                var result = (bool)InvokeProtected(gen, "GetUseMainTexture");
                Assert.IsTrue(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        #endregion
    }
}
