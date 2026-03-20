// Tests for LilToonMaterial properties not covered by other test files:
// Metallic, MainTextureScale, MainTextureOffset, MinimumBrightness,
// UseShadow2nd, UseShadow3rd, UseMain2ndTex, UseMain3rdTex, Main2ndTex, Main3rdTex,
// AOMapTextureScale, AOMapTextureOffset, CullMode,
// NormalMapTextureScale, NormalMapTextureOffset, NormalMapScale, LightMinLimit,
// EmissionMapTextureScale, EmissionMapTextureOffset, EmissionBlend,
// Emission2ndMap, Emission2ndBlend,
// MetallicMap, MetallicMapTextureScale, MetallicMapTextureOffset,
// SmoothnessTex, SmoothnessTexScale, SmoothnessTexOffset,
// ReflectionColorTex, ReflectionColor, Smoothness, Reflectance, SpecularBlur,
// MatCapColor, MatCapMainStrength, MatCapMask, MatCapMaskTextureScale, MatCapMaskTextureOffset,
// MatCapBlend, MatCapBlendingMode,
// RimLightColor, RimMainStrength, RimLightBorder, RimEnableLighting, RimFresnelPower, RimLightBlur,
// GetToonLitPlatformOverride.

using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace KRT.VRCQuestTools.Models
{
    [TestFixture]
    public class LilToonMaterialExtendedPropertyTests
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

        // --- Metallic ---

        [Test]
        public void Metallic_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_Metallic", 0.75f);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.Metallic, Is.EqualTo(0.75f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- MainTextureScale / MainTextureOffset ---

        [Test]
        public void MainTextureScale_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureScale("_MainTex", new Vector2(2.0f, 3.0f));
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.MainTextureScale, Is.EqualTo(new Vector2(2.0f, 3.0f)));
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
            mat.SetTextureOffset("_MainTex", new Vector2(0.5f, 0.25f));
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.MainTextureOffset, Is.EqualTo(new Vector2(0.5f, 0.25f)));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- MinimumBrightness / LightMinLimit ---

        [Test]
        public void MinimumBrightness_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_LightMinLimit", 0.3f);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.MinimumBrightness, Is.EqualTo(0.3f).Within(0.001f));
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
            mat.SetFloat("_LightMinLimit", 0.6f);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.LightMinLimit, Is.EqualTo(0.6f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- UseShadow2nd / UseShadow3rd ---

        [Test]
        public void UseShadow2nd_ShadowEnabledAndAlphaPositive_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseShadow", 1.0f);
            mat.SetColor("_Shadow2ndColor", new Color(1, 0, 0, 0.5f));
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.UseShadow2nd, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseShadow2nd_ShadowDisabled_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseShadow", 0.0f);
            mat.SetColor("_Shadow2ndColor", new Color(1, 0, 0, 0.5f));
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.UseShadow2nd, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseShadow2nd_AlphaZero_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseShadow", 1.0f);
            mat.SetColor("_Shadow2ndColor", new Color(1, 0, 0, 0.0f));
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.UseShadow2nd, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseShadow3rd_ShadowEnabledAndAlphaPositive_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseShadow", 1.0f);
            mat.SetColor("_Shadow3rdColor", new Color(0, 1, 0, 0.5f));
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.UseShadow3rd, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseShadow3rd_ShadowDisabled_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseShadow", 0.0f);
            mat.SetColor("_Shadow3rdColor", new Color(0, 1, 0, 0.5f));
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.UseShadow3rd, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- UseMain2ndTex / UseMain3rdTex / Main2ndTex / Main3rdTex ---

        [Test]
        public void UseMain2ndTex_Enabled_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseMain2ndTex", 1.0f);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.UseMain2ndTex, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseMain2ndTex_Disabled_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseMain2ndTex", 0.0f);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.UseMain2ndTex, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseMain3rdTex_Enabled_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseMain3rdTex", 1.0f);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.UseMain3rdTex, Is.True);
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
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.Main2ndTex, Is.EqualTo(tex));
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void Main2ndTex_NoTexture_ReturnsNull()
        {
            var mat = new Material(lilToonShader);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.Main2ndTex, Is.Null);
            }
            finally
            {
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
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.Main3rdTex, Is.EqualTo(tex));
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        // --- AOMapTextureScale / AOMapTextureOffset ---

        [Test]
        public void AOMapTextureScale_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureScale("_ShadowBorderMask", new Vector2(1.5f, 2.5f));
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.AOMapTextureScale, Is.EqualTo(new Vector2(1.5f, 2.5f)));
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
            mat.SetTextureOffset("_ShadowBorderMask", new Vector2(0.3f, 0.7f));
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.AOMapTextureOffset, Is.EqualTo(new Vector2(0.3f, 0.7f)));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- CullMode ---

        [Test]
        public void CullMode_Back_ReturnsBack()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_Cull", (float)CullMode.Back);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.CullMode, Is.EqualTo(CullMode.Back));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void CullMode_Off_ReturnsOff()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_Cull", (float)CullMode.Off);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.CullMode, Is.EqualTo(CullMode.Off));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- NormalMapTextureScale / NormalMapTextureOffset / NormalMapScale ---

        [Test]
        public void NormalMapTextureScale_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureScale("_BumpMap", new Vector2(3.0f, 4.0f));
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.NormalMapTextureScale, Is.EqualTo(new Vector2(3.0f, 4.0f)));
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
            mat.SetTextureOffset("_BumpMap", new Vector2(0.1f, 0.2f));
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.NormalMapTextureOffset, Is.EqualTo(new Vector2(0.1f, 0.2f)));
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
            mat.SetFloat("_BumpScale", 1.5f);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.NormalMapScale, Is.EqualTo(1.5f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- Emission map scale/offset/blend ---

        [Test]
        public void EmissionMapTextureScale_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureScale("_EmissionMap", new Vector2(2.0f, 2.0f));
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.EmissionMapTextureScale, Is.EqualTo(new Vector2(2.0f, 2.0f)));
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
            mat.SetTextureOffset("_EmissionMap", new Vector2(0.5f, 0.5f));
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.EmissionMapTextureOffset, Is.EqualTo(new Vector2(0.5f, 0.5f)));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void EmissionBlend_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_EmissionBlend", 0.8f);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.EmissionBlend, Is.EqualTo(0.8f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- Emission2ndMap / Emission2ndBlend ---

        [Test]
        public void Emission2ndMap_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetTexture("_Emission2ndMap", tex);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.Emission2ndMap, Is.EqualTo(tex));
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void Emission2ndBlend_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_Emission2ndBlend", 0.4f);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.Emission2ndBlend, Is.EqualTo(0.4f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- Reflection properties ---

        [Test]
        public void MetallicMap_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetTexture("_MetallicGlossMap", tex);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.MetallicMap, Is.EqualTo(tex));
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void MetallicMapTextureScale_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureScale("_MetallicGlossMap", new Vector2(1.5f, 2.0f));
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.MetallicMapTextureScale, Is.EqualTo(new Vector2(1.5f, 2.0f)));
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
            mat.SetTextureOffset("_MetallicGlossMap", new Vector2(0.25f, 0.75f));
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.MetallicMapTextureOffset, Is.EqualTo(new Vector2(0.25f, 0.75f)));
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
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.SmoothnessTex, Is.EqualTo(tex));
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void SmoothnessTexScale_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureScale("_SmoothnessTex", new Vector2(2.0f, 3.0f));
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.SmoothnessTexScale, Is.EqualTo(new Vector2(2.0f, 3.0f)));
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
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.SmoothnessTexOffset, Is.EqualTo(new Vector2(0.1f, 0.9f)));
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
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.ReflectionColorTex, Is.EqualTo(tex));
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void ReflectionColor_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetColor("_ReflectionColor", new Color(0.1f, 0.2f, 0.3f, 1.0f));
            try
            {
                var lil = new LilToonMaterial(mat);
                var c = lil.ReflectionColor;
                Assert.That(c.r, Is.EqualTo(0.1f).Within(0.01f));
                Assert.That(c.g, Is.EqualTo(0.2f).Within(0.01f));
                Assert.That(c.b, Is.EqualTo(0.3f).Within(0.01f));
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
            mat.SetFloat("_Smoothness", 0.9f);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.Smoothness, Is.EqualTo(0.9f).Within(0.001f));
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
            mat.SetFloat("_Reflectance", 0.5f);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.Reflectance, Is.EqualTo(0.5f).Within(0.001f));
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
            mat.SetFloat("_SpecularBlur", 0.3f);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.SpecularBlur, Is.EqualTo(0.3f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- MatCap extended properties ---

        [Test]
        public void MatCapColor_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetColor("_MatCapColor", new Color(0.5f, 0.6f, 0.7f, 1.0f));
            try
            {
                var lil = new LilToonMaterial(mat);
                var c = lil.MatCapColor;
                Assert.That(c.r, Is.EqualTo(0.5f).Within(0.01f));
                Assert.That(c.g, Is.EqualTo(0.6f).Within(0.01f));
                Assert.That(c.b, Is.EqualTo(0.7f).Within(0.01f));
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
            mat.SetFloat("_MatCapMainStrength", 0.8f);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.MatCapMainStrength, Is.EqualTo(0.8f).Within(0.001f));
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
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.MatCapMask, Is.EqualTo(tex));
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void MatCapMaskTextureScale_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetTextureScale("_MatCapBlendMask", new Vector2(2.0f, 3.0f));
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.MatCapMaskTextureScale, Is.EqualTo(new Vector2(2.0f, 3.0f)));
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
            mat.SetTextureOffset("_MatCapBlendMask", new Vector2(0.3f, 0.4f));
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.MatCapMaskTextureOffset, Is.EqualTo(new Vector2(0.3f, 0.4f)));
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
            mat.SetFloat("_MatCapBlend", 0.65f);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.MatCapBlend, Is.EqualTo(0.65f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void MatCapBlendingMode_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_MatCapBlendMode", 1.0f);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.MatCapBlendingMode, Is.EqualTo((LilToonMaterial.MatCapBlendMode)1));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- RimLight extended properties ---

        [Test]
        public void RimLightColor_ReturnsSetValue()
        {
            var mat = new Material(lilToonShader);
            mat.SetColor("_RimColor", new Color(1.0f, 0.5f, 0.0f, 1.0f));
            try
            {
                var lil = new LilToonMaterial(mat);
                var c = lil.RimLightColor;
                Assert.That(c.r, Is.EqualTo(1.0f).Within(0.01f));
                Assert.That(c.g, Is.EqualTo(0.5f).Within(0.01f));
                Assert.That(c.b, Is.EqualTo(0.0f).Within(0.01f));
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
            mat.SetFloat("_RimMainStrength", 0.7f);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.RimMainStrength, Is.EqualTo(0.7f).Within(0.001f));
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
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.RimLightBorder, Is.EqualTo(0.5f).Within(0.001f));
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
            mat.SetFloat("_RimEnableLighting", 1.0f);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.RimEnableLighting, Is.EqualTo(1.0f).Within(0.001f));
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
            mat.SetFloat("_RimFresnelPower", 3.5f);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.RimFresnelPower, Is.EqualTo(3.5f).Within(0.001f));
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
            mat.SetFloat("_RimBlur", 0.25f);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.RimLightBlur, Is.EqualTo(0.25f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- GetToonLitPlatformOverride ---

        [Test]
        public void GetToonLitPlatformOverride_NoTextures_ReturnsNull()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseMain2ndTex", 0.0f);
            mat.SetFloat("_UseMain3rdTex", 0.0f);
            mat.SetFloat("_UseEmission", 0.0f);
            mat.SetFloat("_UseEmission2nd", 0.0f);
            try
            {
                var lil = new LilToonMaterial(mat);
                Assert.That(lil.GetToonLitPlatformOverride(), Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetToonLitPlatformOverride_WithMainTexture_ReturnsValue()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(64, 64);
            mat.mainTexture = tex;
            mat.SetFloat("_UseMain2ndTex", 0.0f);
            mat.SetFloat("_UseMain3rdTex", 0.0f);
            mat.SetFloat("_UseEmission", 0.0f);
            mat.SetFloat("_UseEmission2nd", 0.0f);
            try
            {
                var lil = new LilToonMaterial(mat);
                // Runtime textures have no asset path, so GetBestPlatformOverrideSettings returns null
                var result = lil.GetToonLitPlatformOverride();
                Assert.That(result, Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void GetToonLitPlatformOverride_WithMain2ndTex_ReturnsValue()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(64, 64);
            mat.SetFloat("_UseMain2ndTex", 1.0f);
            mat.SetTexture("_Main2ndTex", tex);
            mat.SetFloat("_UseMain3rdTex", 0.0f);
            mat.SetFloat("_UseEmission", 0.0f);
            mat.SetFloat("_UseEmission2nd", 0.0f);
            try
            {
                var lil = new LilToonMaterial(mat);
                // Runtime textures have no asset path, so GetBestPlatformOverrideSettings returns null
                var result = lil.GetToonLitPlatformOverride();
                Assert.That(result, Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void GetToonLitPlatformOverride_WithMain3rdTex_ReturnsValue()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(64, 64);
            mat.SetFloat("_UseMain2ndTex", 0.0f);
            mat.SetFloat("_UseMain3rdTex", 1.0f);
            mat.SetTexture("_Main3rdTex", tex);
            mat.SetFloat("_UseEmission", 0.0f);
            mat.SetFloat("_UseEmission2nd", 0.0f);
            try
            {
                var lil = new LilToonMaterial(mat);
                // Runtime textures have no asset path, so GetBestPlatformOverrideSettings returns null
                var result = lil.GetToonLitPlatformOverride();
                Assert.That(result, Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void GetToonLitPlatformOverride_WithEmissionMap_ReturnsValue()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(64, 64);
            mat.SetFloat("_UseMain2ndTex", 0.0f);
            mat.SetFloat("_UseMain3rdTex", 0.0f);
            mat.SetFloat("_UseEmission", 1.0f);
            mat.SetTexture("_EmissionMap", tex);
            mat.SetFloat("_UseEmission2nd", 0.0f);
            try
            {
                var lil = new LilToonMaterial(mat);
                // Runtime textures have no asset path, so GetBestPlatformOverrideSettings returns null
                var result = lil.GetToonLitPlatformOverride();
                Assert.That(result, Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void GetToonLitPlatformOverride_WithEmission2ndMap_ReturnsValue()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(64, 64);
            mat.SetFloat("_UseMain2ndTex", 0.0f);
            mat.SetFloat("_UseMain3rdTex", 0.0f);
            mat.SetFloat("_UseEmission", 0.0f);
            mat.SetFloat("_UseEmission2nd", 1.0f);
            mat.SetTexture("_Emission2ndMap", tex);
            try
            {
                var lil = new LilToonMaterial(mat);
                // Runtime textures have no asset path, so GetBestPlatformOverrideSettings returns null
                var result = lil.GetToonLitPlatformOverride();
                Assert.That(result, Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void GetToonLitPlatformOverride_DisabledMain2nd_NotIncluded()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(64, 64);
            mat.SetFloat("_UseMain2ndTex", 0.0f);
            mat.SetTexture("_Main2ndTex", tex);
            mat.SetFloat("_UseMain3rdTex", 0.0f);
            mat.SetFloat("_UseEmission", 0.0f);
            mat.SetFloat("_UseEmission2nd", 0.0f);
            try
            {
                var lil = new LilToonMaterial(mat);
                // Disabled Main2ndTex shouldn't be included, and no main texture either
                var result = lil.GetToonLitPlatformOverride();
                Assert.That(result, Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }
    }
}
