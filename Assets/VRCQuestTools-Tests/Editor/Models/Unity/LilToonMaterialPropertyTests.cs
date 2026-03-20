// Tests for LilToonMaterial property getters that are not yet covered.

using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Tests for LilToonMaterial property getters covering uncovered properties
    /// like Metallic, CullMode, Smoothness, Reflectance, SpecularBlur, rim light, etc.
    /// </summary>
    [TestFixture]
    public class LilToonMaterialPropertyTests
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
            isLilToonAvailable = lilToonShader != null;
        }

        [SetUp]
        public void SetUp()
        {
            if (!isLilToonAvailable)
            {
                Assert.Ignore("lilToon shader not available.");
            }
        }

        #region Basic Properties

        [Test]
        public void Metallic_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_Metallic", 0.7f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.7f, lilMat.Metallic, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void CullMode_Back_ReturnsCullBack()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_Cull", (float)CullMode.Back);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(CullMode.Back, lilMat.CullMode);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void CullMode_Off_ReturnsCullOff()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_Cull", (float)CullMode.Off);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(CullMode.Off, lilMat.CullMode);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Smoothness_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_Smoothness", 0.85f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.85f, lilMat.Smoothness, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Reflectance_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_Reflectance", 0.4f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.4f, lilMat.Reflectance, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void SpecularBlur_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_SpecularBlur", 0.6f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.6f, lilMat.SpecularBlur, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void NormalMapScale_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_BumpScale", 2.0f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(2.0f, lilMat.NormalMapScale, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void LightMinLimit_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_LightMinLimit", 0.15f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.15f, lilMat.LightMinLimit, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void MinimumBrightness_ReturnsLightMinLimit()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_LightMinLimit", 0.25f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.25f, lilMat.MinimumBrightness, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region Shadow Properties

        [Test]
        public void UseShadow2nd_WithShadowAndAlpha_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseShadow", 1);
            mat.SetColor("_Shadow2ndColor", new Color(0.5f, 0.5f, 0.5f, 0.5f));
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.IsTrue(lilMat.UseShadow2nd);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseShadow2nd_WithoutShadow_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseShadow", 0);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.IsFalse(lilMat.UseShadow2nd);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseShadow3rd_WithShadowAndAlpha_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseShadow", 1);
            mat.SetColor("_Shadow3rdColor", new Color(0.3f, 0.3f, 0.3f, 0.3f));
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.IsTrue(lilMat.UseShadow3rd);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region Main 2nd/3rd Texture Properties

        [Test]
        public void UseMain2ndTex_SetToTrue_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseMain2ndTex", 1);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.IsTrue(lilMat.UseMain2ndTex);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseMain3rdTex_SetToTrue_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseMain3rdTex", 1);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.IsTrue(lilMat.UseMain3rdTex);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Main2ndTex_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetTexture("_Main2ndTex", tex);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(tex, lilMat.Main2ndTex);
            }
            finally
            {
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Main3rdTex_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetTexture("_Main3rdTex", tex);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(tex, lilMat.Main3rdTex);
            }
            finally
            {
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region Rim Light Properties

        [Test]
        public void RimLightColor_ReturnsSetColor()
        {
            var mat = new Material(lilToonShader);
            mat.SetColor("_RimColor", new Color(1, 0.5f, 0.3f, 0.8f));
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(1f, lilMat.RimLightColor.r, 0.001f);
                Assert.AreEqual(0.5f, lilMat.RimLightColor.g, 0.001f);
                Assert.AreEqual(0.3f, lilMat.RimLightColor.b, 0.001f);
                Assert.AreEqual(0.8f, lilMat.RimLightColor.a, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void RimMainStrength_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_RimMainStrength", 0.6f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.6f, lilMat.RimMainStrength, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void RimLightBorder_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_RimBorder", 0.5f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.5f, lilMat.RimLightBorder, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void RimEnableLighting_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_RimEnableLighting", 0.7f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.7f, lilMat.RimEnableLighting, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void RimFresnelPower_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_RimFresnelPower", 3.0f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(3.0f, lilMat.RimFresnelPower, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void RimLightBlur_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_RimBlur", 0.35f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.35f, lilMat.RimLightBlur, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region MatCap Properties

        [Test]
        public void MatCapColor_ReturnsSetColor()
        {
            var mat = new Material(lilToonShader);
            mat.SetColor("_MatCapColor", new Color(0.9f, 0.8f, 0.7f, 0.6f));
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.9f, lilMat.MatCapColor.r, 0.001f);
                Assert.AreEqual(0.8f, lilMat.MatCapColor.g, 0.001f);
                Assert.AreEqual(0.7f, lilMat.MatCapColor.b, 0.001f);
                Assert.AreEqual(0.6f, lilMat.MatCapColor.a, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void MatCapMainStrength_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_MatCapMainStrength", 0.5f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.5f, lilMat.MatCapMainStrength, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void MatCapBlend_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_MatCapBlend", 0.75f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.75f, lilMat.MatCapBlend, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void MatCapBlendingMode_Multiply_ReturnsMultiply()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_MatCapBlendMode", 3); // Multiply
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(LilToonMaterial.MatCapBlendMode.Multiply, lilMat.MatCapBlendingMode);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void MatCapBlendingMode_Screen_ReturnsScreen()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_MatCapBlendMode", 2); // Screen
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(LilToonMaterial.MatCapBlendMode.Screen, lilMat.MatCapBlendingMode);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void MatCapMask_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetTexture("_MatCapBlendMask", tex);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(tex, lilMat.MatCapMask);
            }
            finally
            {
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void MatCapMaskTextureScale_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureScale("_MatCapBlendMask", new Vector2(2f, 3f));
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(2f, lilMat.MatCapMaskTextureScale.x, 0.001f);
                Assert.AreEqual(3f, lilMat.MatCapMaskTextureScale.y, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void MatCapMaskTextureOffset_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureOffset("_MatCapBlendMask", new Vector2(0.1f, 0.2f));
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.1f, lilMat.MatCapMaskTextureOffset.x, 0.001f);
                Assert.AreEqual(0.2f, lilMat.MatCapMaskTextureOffset.y, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region Reflection/Metallic Properties

        [Test]
        public void UseReflection_SetToTrue_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseReflection", 1);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.IsTrue(lilMat.UseReflection);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseReflection_SetToFalse_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseReflection", 0);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.IsFalse(lilMat.UseReflection);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void MetallicMap_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetTexture("_MetallicGlossMap", tex);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(tex, lilMat.MetallicMap);
            }
            finally
            {
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void MetallicMapTextureScale_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureScale("_MetallicGlossMap", new Vector2(4f, 5f));
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(4f, lilMat.MetallicMapTextureScale.x, 0.001f);
                Assert.AreEqual(5f, lilMat.MetallicMapTextureScale.y, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void SmoothnessTex_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetTexture("_SmoothnessTex", tex);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(tex, lilMat.SmoothnessTex);
            }
            finally
            {
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void SmoothnessTexScale_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureScale("_SmoothnessTex", new Vector2(2f, 3f));
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(2f, lilMat.SmoothnessTexScale.x, 0.001f);
                Assert.AreEqual(3f, lilMat.SmoothnessTexScale.y, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ReflectionColorTex_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetTexture("_ReflectionColorTex", tex);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(tex, lilMat.ReflectionColorTex);
            }
            finally
            {
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ReflectionColor_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetColor("_ReflectionColor", new Color(0.1f, 0.2f, 0.3f, 1));
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.1f, lilMat.ReflectionColor.r, 0.001f);
                Assert.AreEqual(0.2f, lilMat.ReflectionColor.g, 0.001f);
                Assert.AreEqual(0.3f, lilMat.ReflectionColor.b, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region Emission Properties

        [Test]
        public void EmissionBlend_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_EmissionBlend", 0.5f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.5f, lilMat.EmissionBlend, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Emission2ndBlend_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_Emission2ndBlend", 0.7f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.7f, lilMat.Emission2ndBlend, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Emission2ndColor_ReturnsSetColor()
        {
            var mat = new Material(lilToonShader);
            mat.SetColor("_Emission2ndColor", new Color(0, 1, 0, 1));
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(new Color(0, 1, 0, 1), lilMat.Emission2ndColor);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Emission2ndMap_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetTexture("_Emission2ndMap", tex);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(tex, lilMat.Emission2ndMap);
            }
            finally
            {
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Emission2ndBlendMask_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetTexture("_Emission2ndBlendMask", tex);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(tex, lilMat.Emission2ndBlendMask);
            }
            finally
            {
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region Texture Scale/Offset Properties

        [Test]
        public void MainTextureScale_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureScale("_MainTex", new Vector2(2f, 3f));
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(2f, lilMat.MainTextureScale.x, 0.001f);
                Assert.AreEqual(3f, lilMat.MainTextureScale.y, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void MainTextureOffset_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureOffset("_MainTex", new Vector2(0.1f, 0.2f));
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.1f, lilMat.MainTextureOffset.x, 0.001f);
                Assert.AreEqual(0.2f, lilMat.MainTextureOffset.y, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void AOMapTextureScale_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureScale("_ShadowBorderMask", new Vector2(1.5f, 2.5f));
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(1.5f, lilMat.AOMapTextureScale.x, 0.001f);
                Assert.AreEqual(2.5f, lilMat.AOMapTextureScale.y, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void AOMapTextureOffset_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureOffset("_ShadowBorderMask", new Vector2(0.3f, 0.4f));
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.3f, lilMat.AOMapTextureOffset.x, 0.001f);
                Assert.AreEqual(0.4f, lilMat.AOMapTextureOffset.y, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void NormalMapTextureScale_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureScale("_BumpMap", new Vector2(3f, 4f));
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(3f, lilMat.NormalMapTextureScale.x, 0.001f);
                Assert.AreEqual(4f, lilMat.NormalMapTextureScale.y, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void NormalMapTextureOffset_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureOffset("_BumpMap", new Vector2(0.5f, 0.6f));
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.5f, lilMat.NormalMapTextureOffset.x, 0.001f);
                Assert.AreEqual(0.6f, lilMat.NormalMapTextureOffset.y, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void EmissionMapTextureScale_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureScale("_EmissionMap", new Vector2(1f, 2f));
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(1f, lilMat.EmissionMapTextureScale.x, 0.001f);
                Assert.AreEqual(2f, lilMat.EmissionMapTextureScale.y, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void EmissionMapTextureOffset_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureOffset("_EmissionMap", new Vector2(0.7f, 0.8f));
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.7f, lilMat.EmissionMapTextureOffset.x, 0.001f);
                Assert.AreEqual(0.8f, lilMat.EmissionMapTextureOffset.y, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void MetallicMapTextureOffset_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureOffset("_MetallicGlossMap", new Vector2(0.3f, 0.4f));
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.3f, lilMat.MetallicMapTextureOffset.x, 0.001f);
                Assert.AreEqual(0.4f, lilMat.MetallicMapTextureOffset.y, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void SmoothnessTexOffset_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureOffset("_SmoothnessTex", new Vector2(0.1f, 0.9f));
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(0.1f, lilMat.SmoothnessTexOffset.x, 0.001f);
                Assert.AreEqual(0.9f, lilMat.SmoothnessTexOffset.y, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        #endregion

        #region GetToonLitPlatformOverride

        [Test]
        public void GetToonLitPlatformOverride_NoTextures_ReturnsNull()
        {
            var mat = new Material(lilToonShader);
            mat.mainTexture = null;
            try
            {
                var lilMat = new LilToonMaterial(mat);
                var result = lilMat.GetToonLitPlatformOverride();
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        #endregion
    }
}
