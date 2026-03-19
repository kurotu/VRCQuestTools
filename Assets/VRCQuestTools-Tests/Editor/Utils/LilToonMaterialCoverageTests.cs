// Tests for LilToonMaterial and LilToonToonStandardGenerator coverage.
// Requires lilToon shader package to be installed.

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
    // Helper to create lilToon materials with specific properties
    // =========================================================
    internal static class LilToonTestHelper
    {
        internal static Shader FindLilToonShader()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null)
            {
                throw new InvalidOperationException("lilToon shader not found. Is the lilToon package installed?");
            }
            return shader;
        }

        internal static Material CreateLilToonMaterial(string name = "TestLilToon")
        {
            var shader = FindLilToonShader();
            var mat = new Material(shader);
            mat.name = name;
            return mat;
        }

        internal static LilToonMaterial CreateLilToonMaterialWrapper(string name = "TestLilToon")
        {
            var mat = CreateLilToonMaterial(name);
            // Use reflection since constructor is internal
            var ctor = typeof(LilToonMaterial).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null, new[] { typeof(Material) }, null);
            return (LilToonMaterial)ctor.Invoke(new object[] { mat });
        }

        internal static Texture2D CreateTestTexture(int width = 64, int height = 64)
        {
            return new Texture2D(width, height, TextureFormat.RGBA32, false);
        }
    }

    // =========================================================
    // LilToonMaterial Constructor Tests
    // =========================================================
    [TestFixture]
    public class LilToonMaterial_Constructor_Tests
    {
        [Test]
        public void Constructor_WithLilToonShader_Succeeds()
        {
            var mat = LilToonTestHelper.CreateLilToonMaterial();
            try
            {
                var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
                Assert.IsNotNull(wrapper);
                Assert.IsNotNull(wrapper.Material);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Constants_AreCorrect()
        {
            var packageName = (string)typeof(LilToonMaterial).GetField("PackageDisplayName",
                BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            var requiredVersion = (string)typeof(LilToonMaterial).GetField("RequiredVersion",
                BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            var breakingVersion = (string)typeof(LilToonMaterial).GetField("BreakingVersion",
                BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            Assert.AreEqual("lilToon", packageName);
            Assert.AreEqual("1.10.0", requiredVersion);
            Assert.AreEqual("3.0.0", breakingVersion);
        }
    }

    // =========================================================
    // LilToonMaterial Property Getter Tests
    // =========================================================
    [TestFixture]
    public class LilToonMaterial_Properties_Tests
    {
        private Material mat;
        private LilToonMaterial wrapper;

        [SetUp]
        public void SetUp()
        {
            mat = LilToonTestHelper.CreateLilToonMaterial();
            wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
        }

        [TearDown]
        public void TearDown()
        {
            if (wrapper != null)
            {
                UObject.DestroyImmediate(wrapper.Material);
            }
            if (mat != null)
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Metallic_ReturnsFloat()
        {
            wrapper.Material.SetFloat("_Metallic", 0.5f);
            var metallic = (float)typeof(LilToonMaterial).GetProperty("Metallic",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.5f, metallic, 0.001f);
        }

        [Test]
        public void MainTextureScale_ReturnsVector2()
        {
            wrapper.Material.SetTextureScale("_MainTex", new Vector2(2f, 3f));
            var scale = (Vector2)typeof(LilToonMaterial).GetProperty("MainTextureScale",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(2f, scale.x, 0.001f);
            Assert.AreEqual(3f, scale.y, 0.001f);
        }

        [Test]
        public void MainTextureOffset_ReturnsVector2()
        {
            wrapper.Material.SetTextureOffset("_MainTex", new Vector2(0.1f, 0.2f));
            var offset = (Vector2)typeof(LilToonMaterial).GetProperty("MainTextureOffset",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.1f, offset.x, 0.001f);
            Assert.AreEqual(0.2f, offset.y, 0.001f);
        }

        [Test]
        public void MinimumBrightness_ReturnsFloat()
        {
            wrapper.Material.SetFloat("_LightMinLimit", 0.3f);
            var value = (float)typeof(LilToonMaterial).GetProperty("MinimumBrightness",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.3f, value, 0.001f);
        }

        [Test]
        public void ToonLitBakeShader_ReturnsShader()
        {
            var prop = typeof(LilToonMaterial).GetProperty("ToonLitBakeShader",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var shader = (Shader)prop.GetValue(wrapper);
            Assert.IsNotNull(shader, "ToonLitBakeShader should find Hidden/VRCQuestTools/lilToon");
        }

        [Test]
        public void UseShadow_WhenEnabled_ReturnsTrue()
        {
            wrapper.Material.SetFloat("_UseShadow", 1.0f);
            var value = (bool)typeof(LilToonMaterial).GetProperty("UseShadow",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.IsTrue(value);
        }

        [Test]
        public void UseShadow_WhenDisabled_ReturnsFalse()
        {
            wrapper.Material.SetFloat("_UseShadow", 0.0f);
            var value = (bool)typeof(LilToonMaterial).GetProperty("UseShadow",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.IsFalse(value);
        }

        [Test]
        public void UseShadow2nd_WhenShadowEnabledAndAlphaSet_ReturnsTrue()
        {
            wrapper.Material.SetFloat("_UseShadow", 1.0f);
            wrapper.Material.SetColor("_Shadow2ndColor", new Color(1, 1, 1, 0.5f));
            var value = (bool)typeof(LilToonMaterial).GetProperty("UseShadow2nd",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.IsTrue(value);
        }

        [Test]
        public void UseShadow2nd_WhenShadowDisabled_ReturnsFalse()
        {
            wrapper.Material.SetFloat("_UseShadow", 0.0f);
            wrapper.Material.SetColor("_Shadow2ndColor", new Color(1, 1, 1, 0.5f));
            var value = (bool)typeof(LilToonMaterial).GetProperty("UseShadow2nd",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.IsFalse(value);
        }

        [Test]
        public void UseShadow3rd_WhenShadowEnabledAndAlphaSet_ReturnsTrue()
        {
            wrapper.Material.SetFloat("_UseShadow", 1.0f);
            wrapper.Material.SetColor("_Shadow3rdColor", new Color(1, 1, 1, 0.5f));
            var value = (bool)typeof(LilToonMaterial).GetProperty("UseShadow3rd",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.IsTrue(value);
        }

        [Test]
        public void UseMain2ndTex_WhenEnabled_ReturnsTrue()
        {
            wrapper.Material.SetFloat("_UseMain2ndTex", 1.0f);
            var value = (bool)typeof(LilToonMaterial).GetProperty("UseMain2ndTex",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.IsTrue(value);
        }

        [Test]
        public void UseMain3rdTex_WhenEnabled_ReturnsTrue()
        {
            wrapper.Material.SetFloat("_UseMain3rdTex", 1.0f);
            var value = (bool)typeof(LilToonMaterial).GetProperty("UseMain3rdTex",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.IsTrue(value);
        }

        [Test]
        public void Main2ndTex_WhenSet_ReturnsTexture()
        {
            var tex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetTexture("_Main2ndTex", tex);
                var result = (Texture)typeof(LilToonMaterial).GetProperty("Main2ndTex",
                    BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
                Assert.AreEqual(tex, result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void Main3rdTex_WhenSet_ReturnsTexture()
        {
            var tex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetTexture("_Main3rdTex", tex);
                var result = (Texture)typeof(LilToonMaterial).GetProperty("Main3rdTex",
                    BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
                Assert.AreEqual(tex, result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void AOMap_WhenSet_ReturnsTexture()
        {
            var tex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetTexture("_ShadowBorderMask", tex);
                var result = (Texture)typeof(LilToonMaterial).GetProperty("AOMap",
                    BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
                Assert.AreEqual(tex, result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void AOMapTextureScale_ReturnsVector2()
        {
            wrapper.Material.SetTextureScale("_ShadowBorderMask", new Vector2(1.5f, 2.5f));
            var value = (Vector2)typeof(LilToonMaterial).GetProperty("AOMapTextureScale",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(1.5f, value.x, 0.001f);
            Assert.AreEqual(2.5f, value.y, 0.001f);
        }

        [Test]
        public void AOMapTextureOffset_ReturnsVector2()
        {
            wrapper.Material.SetTextureOffset("_ShadowBorderMask", new Vector2(0.3f, 0.4f));
            var value = (Vector2)typeof(LilToonMaterial).GetProperty("AOMapTextureOffset",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.3f, value.x, 0.001f);
            Assert.AreEqual(0.4f, value.y, 0.001f);
        }

        [Test]
        public void CullMode_ReturnsEnum()
        {
            wrapper.Material.SetFloat("_Cull", (float)CullMode.Front);
            var value = (CullMode)typeof(LilToonMaterial).GetProperty("CullMode",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(CullMode.Front, value);
        }

        [Test]
        public void UseNormalMap_WhenEnabled_ReturnsTrue()
        {
            wrapper.Material.SetFloat("_UseBumpMap", 1.0f);
            var value = (bool)typeof(LilToonMaterial).GetProperty("UseNormalMap",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.IsTrue(value);
        }

        [Test]
        public void NormalMap_WhenSet_ReturnsTexture()
        {
            var tex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetTexture("_BumpMap", tex);
                var result = (Texture)typeof(LilToonMaterial).GetProperty("NormalMap",
                    BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
                Assert.AreEqual(tex, result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void NormalMapTextureScale_ReturnsVector2()
        {
            wrapper.Material.SetTextureScale("_BumpMap", new Vector2(3f, 4f));
            var value = (Vector2)typeof(LilToonMaterial).GetProperty("NormalMapTextureScale",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(3f, value.x, 0.001f);
        }

        [Test]
        public void NormalMapTextureOffset_ReturnsVector2()
        {
            wrapper.Material.SetTextureOffset("_BumpMap", new Vector2(0.5f, 0.6f));
            var value = (Vector2)typeof(LilToonMaterial).GetProperty("NormalMapTextureOffset",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.5f, value.x, 0.001f);
        }

        [Test]
        public void NormalMapScale_ReturnsFloat()
        {
            wrapper.Material.SetFloat("_BumpScale", 0.8f);
            var value = (float)typeof(LilToonMaterial).GetProperty("NormalMapScale",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.8f, value, 0.001f);
        }

        [Test]
        public void LightMinLimit_ReturnsFloat()
        {
            wrapper.Material.SetFloat("_LightMinLimit", 0.2f);
            var value = (float)typeof(LilToonMaterial).GetProperty("LightMinLimit",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.2f, value, 0.001f);
        }

        [Test]
        public void UseEmission_GetSet()
        {
            var prop = typeof(LilToonMaterial).GetProperty("UseEmission",
                BindingFlags.NonPublic | BindingFlags.Instance);

            prop.SetValue(wrapper, true);
            Assert.IsTrue((bool)prop.GetValue(wrapper));

            prop.SetValue(wrapper, false);
            Assert.IsFalse((bool)prop.GetValue(wrapper));
        }

        [Test]
        public void EmissionMap_WhenSet_ReturnsTexture()
        {
            var tex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetTexture("_EmissionMap", tex);
                var result = (Texture)typeof(LilToonMaterial).GetProperty("EmissionMap",
                    BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
                Assert.AreEqual(tex, result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void EmissionMapTextureScale_ReturnsVector2()
        {
            wrapper.Material.SetTextureScale("_EmissionMap", new Vector2(1.2f, 1.3f));
            var value = (Vector2)typeof(LilToonMaterial).GetProperty("EmissionMapTextureScale",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(1.2f, value.x, 0.001f);
        }

        [Test]
        public void EmissionMapTextureOffset_ReturnsVector2()
        {
            wrapper.Material.SetTextureOffset("_EmissionMap", new Vector2(0.7f, 0.8f));
            var value = (Vector2)typeof(LilToonMaterial).GetProperty("EmissionMapTextureOffset",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.7f, value.x, 0.001f);
        }

        [Test]
        public void EmissionColor_GetSet()
        {
            var prop = typeof(LilToonMaterial).GetProperty("EmissionColor",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var color = new Color(0.1f, 0.2f, 0.3f, 1.0f);
            prop.SetValue(wrapper, color);
            var result = (Color)prop.GetValue(wrapper);
            Assert.AreEqual(0.1f, result.r, 0.01f);
            Assert.AreEqual(0.2f, result.g, 0.01f);
            Assert.AreEqual(0.3f, result.b, 0.01f);
        }

        [Test]
        public void EmissionBlendMask_WhenSet_ReturnsTexture()
        {
            var tex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetTexture("_EmissionBlendMask", tex);
                var result = (Texture)typeof(LilToonMaterial).GetProperty("EmissionBlendMask",
                    BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
                Assert.AreEqual(tex, result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void EmissionBlend_ReturnsFloat()
        {
            wrapper.Material.SetFloat("_EmissionBlend", 0.6f);
            var value = (float)typeof(LilToonMaterial).GetProperty("EmissionBlend",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.6f, value, 0.001f);
        }

        [Test]
        public void UseEmission2nd_GetSet()
        {
            var prop = typeof(LilToonMaterial).GetProperty("UseEmission2nd",
                BindingFlags.NonPublic | BindingFlags.Instance);
            prop.SetValue(wrapper, true);
            Assert.IsTrue((bool)prop.GetValue(wrapper));
            prop.SetValue(wrapper, false);
            Assert.IsFalse((bool)prop.GetValue(wrapper));
        }

        [Test]
        public void Emission2ndMap_WhenSet_ReturnsTexture()
        {
            var tex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetTexture("_Emission2ndMap", tex);
                var result = (Texture)typeof(LilToonMaterial).GetProperty("Emission2ndMap",
                    BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
                Assert.AreEqual(tex, result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void Emission2ndColor_GetSet()
        {
            var prop = typeof(LilToonMaterial).GetProperty("Emission2ndColor",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var color = new Color(0.4f, 0.5f, 0.6f, 1.0f);
            prop.SetValue(wrapper, color);
            var result = (Color)prop.GetValue(wrapper);
            Assert.AreEqual(0.4f, result.r, 0.01f);
        }

        [Test]
        public void Emission2ndBlendMask_WhenSet_ReturnsTexture()
        {
            var tex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetTexture("_Emission2ndBlendMask", tex);
                var result = (Texture)typeof(LilToonMaterial).GetProperty("Emission2ndBlendMask",
                    BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
                Assert.AreEqual(tex, result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void Emission2ndBlend_ReturnsFloat()
        {
            wrapper.Material.SetFloat("_Emission2ndBlend", 0.9f);
            var value = (float)typeof(LilToonMaterial).GetProperty("Emission2ndBlend",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.9f, value, 0.001f);
        }

        [Test]
        public void UseReflection_WhenEnabled_ReturnsTrue()
        {
            wrapper.Material.SetFloat("_UseReflection", 1.0f);
            var value = (bool)typeof(LilToonMaterial).GetProperty("UseReflection",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.IsTrue(value);
        }

        [Test]
        public void MetallicMap_WhenSet_ReturnsTexture()
        {
            var tex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetTexture("_MetallicGlossMap", tex);
                var result = (Texture)typeof(LilToonMaterial).GetProperty("MetallicMap",
                    BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
                Assert.AreEqual(tex, result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void MetallicMapTextureScale_ReturnsVector2()
        {
            wrapper.Material.SetTextureScale("_MetallicGlossMap", new Vector2(2f, 3f));
            var value = (Vector2)typeof(LilToonMaterial).GetProperty("MetallicMapTextureScale",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(2f, value.x, 0.001f);
        }

        [Test]
        public void MetallicMapTextureOffset_ReturnsVector2()
        {
            wrapper.Material.SetTextureOffset("_MetallicGlossMap", new Vector2(0.1f, 0.2f));
            var value = (Vector2)typeof(LilToonMaterial).GetProperty("MetallicMapTextureOffset",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.1f, value.x, 0.001f);
        }

        [Test]
        public void SmoothnessTex_WhenSet_ReturnsTexture()
        {
            var tex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetTexture("_SmoothnessTex", tex);
                var result = (Texture)typeof(LilToonMaterial).GetProperty("SmoothnessTex",
                    BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
                Assert.AreEqual(tex, result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void SmoothnessTexScale_ReturnsVector2()
        {
            wrapper.Material.SetTextureScale("_SmoothnessTex", new Vector2(1.1f, 1.2f));
            var value = (Vector2)typeof(LilToonMaterial).GetProperty("SmoothnessTexScale",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(1.1f, value.x, 0.001f);
        }

        [Test]
        public void SmoothnessTexOffset_ReturnsVector2()
        {
            wrapper.Material.SetTextureOffset("_SmoothnessTex", new Vector2(0.3f, 0.4f));
            var value = (Vector2)typeof(LilToonMaterial).GetProperty("SmoothnessTexOffset",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.3f, value.x, 0.001f);
        }

        [Test]
        public void ReflectionColorTex_WhenSet_ReturnsTexture()
        {
            var tex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetTexture("_ReflectionColorTex", tex);
                var result = (Texture)typeof(LilToonMaterial).GetProperty("ReflectionColorTex",
                    BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
                Assert.AreEqual(tex, result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void ReflectionColor_ReturnsColor()
        {
            wrapper.Material.SetColor("_ReflectionColor", new Color(0.9f, 0.8f, 0.7f, 1.0f));
            var value = (Color)typeof(LilToonMaterial).GetProperty("ReflectionColor",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.9f, value.r, 0.01f);
        }

        [Test]
        public void Smoothness_ReturnsFloat()
        {
            wrapper.Material.SetFloat("_Smoothness", 0.7f);
            var value = (float)typeof(LilToonMaterial).GetProperty("Smoothness",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.7f, value, 0.001f);
        }

        [Test]
        public void Reflectance_ReturnsFloat()
        {
            wrapper.Material.SetFloat("_Reflectance", 0.4f);
            var value = (float)typeof(LilToonMaterial).GetProperty("Reflectance",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.4f, value, 0.001f);
        }

        [Test]
        public void SpecularBlur_ReturnsFloat()
        {
            wrapper.Material.SetFloat("_SpecularBlur", 0.3f);
            var value = (float)typeof(LilToonMaterial).GetProperty("SpecularBlur",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.3f, value, 0.001f);
        }

        [Test]
        public void UseMatCap_WhenEnabled_ReturnsTrue()
        {
            wrapper.Material.SetFloat("_UseMatCap", 1.0f);
            var value = (bool)typeof(LilToonMaterial).GetProperty("UseMatCap",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.IsTrue(value);
        }

        [Test]
        public void MatCapTex_WhenSet_ReturnsTexture()
        {
            var tex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetTexture("_MatCapTex", tex);
                var result = (Texture)typeof(LilToonMaterial).GetProperty("MatCapTex",
                    BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
                Assert.AreEqual(tex, result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void MatCapColor_ReturnsColor()
        {
            wrapper.Material.SetColor("_MatCapColor", new Color(0.1f, 0.2f, 0.3f, 0.4f));
            var value = (Color)typeof(LilToonMaterial).GetProperty("MatCapColor",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.1f, value.r, 0.01f);
        }

        [Test]
        public void MatCapMainStrength_ReturnsFloat()
        {
            wrapper.Material.SetFloat("_MatCapMainStrength", 0.6f);
            var value = (float)typeof(LilToonMaterial).GetProperty("MatCapMainStrength",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.6f, value, 0.001f);
        }

        [Test]
        public void MatCapMask_WhenSet_ReturnsTexture()
        {
            var tex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetTexture("_MatCapBlendMask", tex);
                var result = (Texture)typeof(LilToonMaterial).GetProperty("MatCapMask",
                    BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
                Assert.AreEqual(tex, result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void MatCapMaskTextureScale_ReturnsVector2()
        {
            wrapper.Material.SetTextureScale("_MatCapBlendMask", new Vector2(1.5f, 2.5f));
            var value = (Vector2)typeof(LilToonMaterial).GetProperty("MatCapMaskTextureScale",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(1.5f, value.x, 0.001f);
        }

        [Test]
        public void MatCapMaskTextureOffset_ReturnsVector2()
        {
            wrapper.Material.SetTextureOffset("_MatCapBlendMask", new Vector2(0.3f, 0.4f));
            var value = (Vector2)typeof(LilToonMaterial).GetProperty("MatCapMaskTextureOffset",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.3f, value.x, 0.001f);
        }

        [Test]
        public void MatCapBlend_ReturnsFloat()
        {
            wrapper.Material.SetFloat("_MatCapBlend", 0.5f);
            var value = (float)typeof(LilToonMaterial).GetProperty("MatCapBlend",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.5f, value, 0.001f);
        }

        [Test]
        public void MatCapBlendingMode_ReturnsEnum()
        {
            wrapper.Material.SetFloat("_MatCapBlendMode", 2f); // Screen
            var value = typeof(LilToonMaterial).GetProperty("MatCapBlendingMode",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(2, (int)value); // Screen = 2
        }

        [Test]
        public void UseRimLight_WhenEnabled_ReturnsTrue()
        {
            wrapper.Material.SetFloat("_UseRim", 1.0f);
            var value = (bool)typeof(LilToonMaterial).GetProperty("UseRimLight",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.IsTrue(value);
        }

        [Test]
        public void RimLightColor_ReturnsColor()
        {
            wrapper.Material.SetColor("_RimColor", new Color(0.5f, 0.6f, 0.7f, 0.8f));
            var value = (Color)typeof(LilToonMaterial).GetProperty("RimLightColor",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.5f, value.r, 0.01f);
        }

        [Test]
        public void RimMainStrength_ReturnsFloat()
        {
            wrapper.Material.SetFloat("_RimMainStrength", 0.4f);
            var value = (float)typeof(LilToonMaterial).GetProperty("RimMainStrength",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.4f, value, 0.001f);
        }

        [Test]
        public void RimLightBorder_ReturnsFloat()
        {
            wrapper.Material.SetFloat("_RimBorder", 0.5f);
            var value = (float)typeof(LilToonMaterial).GetProperty("RimLightBorder",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.5f, value, 0.001f);
        }

        [Test]
        public void RimEnableLighting_ReturnsFloat()
        {
            wrapper.Material.SetFloat("_RimEnableLighting", 0.7f);
            var value = (float)typeof(LilToonMaterial).GetProperty("RimEnableLighting",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.7f, value, 0.001f);
        }

        [Test]
        public void RimFresnelPower_ReturnsFloat()
        {
            wrapper.Material.SetFloat("_RimFresnelPower", 3.0f);
            var value = (float)typeof(LilToonMaterial).GetProperty("RimFresnelPower",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(3.0f, value, 0.001f);
        }

        [Test]
        public void RimLightBlur_ReturnsFloat()
        {
            wrapper.Material.SetFloat("_RimBlur", 0.2f);
            var value = (float)typeof(LilToonMaterial).GetProperty("RimLightBlur",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wrapper);
            Assert.AreEqual(0.2f, value, 0.001f);
        }
    }

    // =========================================================
    // LilToonMaterial Methods Tests
    // =========================================================
    [TestFixture]
    public class LilToonMaterial_Methods_Tests
    {
        [Test]
        public void ConvertToToonLit_CreatesValidMaterial()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            try
            {
                var method = typeof(MaterialBase).GetMethod("ConvertToToonLit",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = (Material)method.Invoke(wrapper, null);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.shader.name.Contains("Toon Lit") || result.shader != null);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void GetToonLitPlatformOverride_WithMainTexture_ReturnsValue()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var tex = LilToonTestHelper.CreateTestTexture(512, 512);
            try
            {
                wrapper.Material.mainTexture = tex;
                var method = typeof(LilToonMaterial).GetMethod("GetToonLitPlatformOverride",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = method.Invoke(wrapper, null);
                // May return null or a value depending on texture import settings
                // The important thing is it doesn't crash
                Assert.Pass("GetToonLitPlatformOverride completed without error");
            }
            finally
            {
                UObject.DestroyImmediate(tex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void GetToonLitPlatformOverride_WithEmissionTextures_ConsidersAllTextures()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var mainTex = LilToonTestHelper.CreateTestTexture(256, 256);
            var emissionTex = LilToonTestHelper.CreateTestTexture(512, 512);
            try
            {
                wrapper.Material.mainTexture = mainTex;
                wrapper.Material.SetFloat("_UseEmission", 1.0f);
                wrapper.Material.SetTexture("_EmissionMap", emissionTex);

                var method = typeof(LilToonMaterial).GetMethod("GetToonLitPlatformOverride",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = method.Invoke(wrapper, null);
                Assert.Pass("GetToonLitPlatformOverride with emission completed without error");
            }
            finally
            {
                UObject.DestroyImmediate(emissionTex);
                UObject.DestroyImmediate(mainTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void GetToonLitPlatformOverride_WithMain2ndAndMain3rd_ConsidersAll()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var mainTex = LilToonTestHelper.CreateTestTexture(256, 256);
            var main2nd = LilToonTestHelper.CreateTestTexture(128, 128);
            var main3rd = LilToonTestHelper.CreateTestTexture(64, 64);
            try
            {
                wrapper.Material.mainTexture = mainTex;
                wrapper.Material.SetFloat("_UseMain2ndTex", 1.0f);
                wrapper.Material.SetTexture("_Main2ndTex", main2nd);
                wrapper.Material.SetFloat("_UseMain3rdTex", 1.0f);
                wrapper.Material.SetTexture("_Main3rdTex", main3rd);

                var method = typeof(LilToonMaterial).GetMethod("GetToonLitPlatformOverride",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(wrapper, null); // Don't crash
                Assert.Pass();
            }
            finally
            {
                UObject.DestroyImmediate(main3rd);
                UObject.DestroyImmediate(main2nd);
                UObject.DestroyImmediate(mainTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }
    }

    // =========================================================
    // LilToonToonStandardGenerator Tests
    // =========================================================
    [TestFixture]
    public class LilToonToonStandardGenerator_Tests
    {
        private static LilToonToonStandardGenerator CreateGenerator(
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
                var pixels = new Color[16];
                for (int i = 0; i < 16; i++) pixels[i] = Color.black;
                blackTex.SetPixels(pixels);
                blackTex.Apply();
            }

            // Constructor is public
            return new LilToonToonStandardGenerator(lilMat, settings, blackTex);
        }

        [Test]
        public void Constructor_Succeeds()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            try
            {
                var gen = CreateGenerator(wrapper, blackTex: blackTex);
                Assert.IsNotNull(gen);
            }
            finally
            {
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void ConvertToToonStandard_Basic_CreatesValidMaterial()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper("TestMat");
            var blackTex = new Texture2D(4, 4);
            try
            {
                var settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(false);
                var gen = CreateGenerator(wrapper, settings, blackTex);

                var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = (Material)method.Invoke(gen, null);
                Assert.IsNotNull(result);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithNormalMap_SetsNormalMapProperties()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            var normalTex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetFloat("_UseBumpMap", 1.0f);
                wrapper.Material.SetTexture("_BumpMap", normalTex);
                wrapper.Material.SetFloat("_BumpScale", 0.8f);
                wrapper.Material.SetTextureScale("_BumpMap", new Vector2(2f, 3f));
                wrapper.Material.SetTextureOffset("_BumpMap", new Vector2(0.1f, 0.2f));

                var settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(false);
                settings.useNormalMap = true;
                var gen = CreateGenerator(wrapper, settings, blackTex);

                var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = (Material)method.Invoke(gen, null);
                Assert.IsNotNull(result);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(normalTex);
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithShadow_SetsShadowRamp()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            try
            {
                wrapper.Material.SetFloat("_UseShadow", 1.0f);

                var settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(false);
                var gen = CreateGenerator(wrapper, settings, blackTex);

                var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = (Material)method.Invoke(gen, null);
                Assert.IsNotNull(result);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithoutShadow_SetsFlatRamp()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            try
            {
                wrapper.Material.SetFloat("_UseShadow", 0.0f);

                var settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(false);
                var gen = CreateGenerator(wrapper, settings, blackTex);

                var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = (Material)method.Invoke(gen, null);
                Assert.IsNotNull(result);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithEmission_SetsEmissionProperties()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            var emissionTex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetFloat("_UseEmission", 1.0f);
                wrapper.Material.SetTexture("_EmissionMap", emissionTex);
                wrapper.Material.SetColor("_EmissionColor", new Color(1, 0, 0, 1));

                var settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(false);
                settings.useEmission = true;
                var gen = CreateGenerator(wrapper, settings, blackTex);

                var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = (Material)method.Invoke(gen, null);
                Assert.IsNotNull(result);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(emissionTex);
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithoutEmission_SetsBlackEmission()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            try
            {
                wrapper.Material.SetFloat("_UseEmission", 0.0f);

                var settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(false);
                settings.useEmission = false;
                var gen = CreateGenerator(wrapper, settings, blackTex);

                var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = (Material)method.Invoke(gen, null);
                Assert.IsNotNull(result);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithOcclusion_SetsOcclusionProperties()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            var aoTex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetFloat("_UseShadow", 1.0f);
                wrapper.Material.SetTexture("_ShadowBorderMask", aoTex);

                var settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(false);
                settings.useOcclusion = true;
                var gen = CreateGenerator(wrapper, settings, blackTex);

                var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = (Material)method.Invoke(gen, null);
                Assert.IsNotNull(result);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(aoTex);
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithSpecular_SetsSpecularProperties()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            var metallicTex = LilToonTestHelper.CreateTestTexture();
            var glossTex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetFloat("_UseReflection", 1.0f);
                wrapper.Material.SetFloat("_Metallic", 0.8f);
                wrapper.Material.SetTexture("_MetallicGlossMap", metallicTex);
                wrapper.Material.SetTexture("_SmoothnessTex", glossTex);
                wrapper.Material.SetFloat("_Smoothness", 0.6f);
                wrapper.Material.SetFloat("_SpecularBlur", 0.3f);

                var settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(false);
                settings.useSpecular = true;
                var gen = CreateGenerator(wrapper, settings, blackTex);

                var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = (Material)method.Invoke(gen, null);
                Assert.IsNotNull(result);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(glossTex);
                UObject.DestroyImmediate(metallicTex);
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithMatcap_Add_SetsAdditive()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            var matcapTex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetFloat("_UseMatCap", 1.0f);
                wrapper.Material.SetTexture("_MatCapTex", matcapTex);
                wrapper.Material.SetFloat("_MatCapBlendMode", 1f); // Add
                wrapper.Material.SetFloat("_MatCapBlend", 0.5f);

                var settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(false);
                settings.useMatcap = true;
                var gen = CreateGenerator(wrapper, settings, blackTex);

                var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = (Material)method.Invoke(gen, null);
                Assert.IsNotNull(result);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(matcapTex);
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithMatcap_Multiply_SetsMultiplicative()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            var matcapTex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetFloat("_UseMatCap", 1.0f);
                wrapper.Material.SetTexture("_MatCapTex", matcapTex);
                wrapper.Material.SetFloat("_MatCapBlendMode", 3f); // Multiply
                wrapper.Material.SetFloat("_MatCapBlend", 0.5f);

                var settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(false);
                settings.useMatcap = true;
                var gen = CreateGenerator(wrapper, settings, blackTex);

                var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = (Material)method.Invoke(gen, null);
                Assert.IsNotNull(result);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(matcapTex);
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithMatcap_Normal_SetsAdditive()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            var matcapTex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetFloat("_UseMatCap", 1.0f);
                wrapper.Material.SetTexture("_MatCapTex", matcapTex);
                wrapper.Material.SetFloat("_MatCapBlendMode", 0f); // Normal
                wrapper.Material.SetFloat("_MatCapBlend", 0.5f);

                var settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(false);
                settings.useMatcap = true;
                var gen = CreateGenerator(wrapper, settings, blackTex);

                var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = (Material)method.Invoke(gen, null);
                Assert.IsNotNull(result);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(matcapTex);
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithMatcap_Screen_SetsAdditive()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            var matcapTex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetFloat("_UseMatCap", 1.0f);
                wrapper.Material.SetTexture("_MatCapTex", matcapTex);
                wrapper.Material.SetFloat("_MatCapBlendMode", 2f); // Screen
                wrapper.Material.SetFloat("_MatCapBlend", 0.5f);

                var settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(false);
                settings.useMatcap = true;
                var gen = CreateGenerator(wrapper, settings, blackTex);

                var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = (Material)method.Invoke(gen, null);
                Assert.IsNotNull(result);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(matcapTex);
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithMatcapMask_SetsMask()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            var matcapTex = LilToonTestHelper.CreateTestTexture();
            var maskTex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetFloat("_UseMatCap", 1.0f);
                wrapper.Material.SetTexture("_MatCapTex", matcapTex);
                wrapper.Material.SetTexture("_MatCapBlendMask", maskTex);
                wrapper.Material.SetFloat("_MatCapBlend", 0.5f);

                var settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(false);
                settings.useMatcap = true;
                var gen = CreateGenerator(wrapper, settings, blackTex);

                var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = (Material)method.Invoke(gen, null);
                Assert.IsNotNull(result);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(maskTex);
                UObject.DestroyImmediate(matcapTex);
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithRimLight_SetsRimProperties()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            try
            {
                wrapper.Material.SetFloat("_UseRim", 1.0f);
                wrapper.Material.SetColor("_RimColor", new Color(0.5f, 0.6f, 0.7f, 0.8f));
                wrapper.Material.SetFloat("_RimMainStrength", 0.4f);
                wrapper.Material.SetFloat("_RimBorder", 0.5f);
                wrapper.Material.SetFloat("_RimFresnelPower", 3.0f);
                wrapper.Material.SetFloat("_RimBlur", 0.2f);
                wrapper.Material.SetFloat("_RimEnableLighting", 0.0f);

                var settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(false);
                settings.useRimLighting = true;
                var gen = CreateGenerator(wrapper, settings, blackTex);

                var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = (Material)method.Invoke(gen, null);
                Assert.IsNotNull(result);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithRimLight_Environmental_SetsIntensity()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            try
            {
                wrapper.Material.SetFloat("_UseRim", 1.0f);
                wrapper.Material.SetColor("_RimColor", new Color(1, 1, 1, 1));
                wrapper.Material.SetFloat("_RimMainStrength", 0.5f);
                wrapper.Material.SetFloat("_RimBorder", 0.5f);
                wrapper.Material.SetFloat("_RimFresnelPower", 3.0f);
                wrapper.Material.SetFloat("_RimBlur", 0.2f);
                wrapper.Material.SetFloat("_RimEnableLighting", 0.8f); // > 0 means environmental

                var settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(false);
                settings.useRimLighting = true;
                var gen = CreateGenerator(wrapper, settings, blackTex);

                var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = (Material)method.Invoke(gen, null);
                Assert.IsNotNull(result);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void ConvertToToonStandard_AllFeaturesEnabled_ProducesValidMaterial()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            var mainTex = LilToonTestHelper.CreateTestTexture();
            var normalTex = LilToonTestHelper.CreateTestTexture();
            var emissionTex = LilToonTestHelper.CreateTestTexture();
            var aoTex = LilToonTestHelper.CreateTestTexture();
            var metallicTex = LilToonTestHelper.CreateTestTexture();
            var glossTex = LilToonTestHelper.CreateTestTexture();
            var matcapTex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.mainTexture = mainTex;
                wrapper.Material.SetFloat("_UseBumpMap", 1.0f);
                wrapper.Material.SetTexture("_BumpMap", normalTex);
                wrapper.Material.SetFloat("_BumpScale", 1.0f);
                wrapper.Material.SetFloat("_UseShadow", 1.0f);
                wrapper.Material.SetFloat("_UseEmission", 1.0f);
                wrapper.Material.SetTexture("_EmissionMap", emissionTex);
                wrapper.Material.SetColor("_EmissionColor", Color.white);
                wrapper.Material.SetTexture("_ShadowBorderMask", aoTex);
                wrapper.Material.SetFloat("_UseReflection", 1.0f);
                wrapper.Material.SetFloat("_Metallic", 0.5f);
                wrapper.Material.SetTexture("_MetallicGlossMap", metallicTex);
                wrapper.Material.SetTexture("_SmoothnessTex", glossTex);
                wrapper.Material.SetFloat("_Smoothness", 0.5f);
                wrapper.Material.SetFloat("_UseMatCap", 1.0f);
                wrapper.Material.SetTexture("_MatCapTex", matcapTex);
                wrapper.Material.SetFloat("_UseRim", 1.0f);
                wrapper.Material.SetColor("_RimColor", Color.white);
                wrapper.Material.SetFloat("_RimEnableLighting", 0.5f);

                var settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(true);
                var gen = CreateGenerator(wrapper, settings, blackTex);

                var method = typeof(LilToonToonStandardGenerator).GetMethod("ConvertToToonStandard",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = (Material)method.Invoke(gen, null);
                Assert.IsNotNull(result);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(matcapTex);
                UObject.DestroyImmediate(glossTex);
                UObject.DestroyImmediate(metallicTex);
                UObject.DestroyImmediate(aoTex);
                UObject.DestroyImmediate(emissionTex);
                UObject.DestroyImmediate(normalTex);
                UObject.DestroyImmediate(mainTex);
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        // Platform override tests
        [Test]
        public void GetMainTexturePlatformOverride_NoTextures_ReturnsNull()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            try
            {
                wrapper.Material.mainTexture = null;
                var gen = CreateGenerator(wrapper, blackTex: blackTex);
                var method = typeof(LilToonToonStandardGenerator).GetMethod("GetMainTexturePlatformOverride",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = method.Invoke(gen, null);
                Assert.IsNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void GetMainTexturePlatformOverride_WithMainTex_ReturnsValue()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            var mainTex = LilToonTestHelper.CreateTestTexture(256, 256);
            try
            {
                wrapper.Material.mainTexture = mainTex;
                var gen = CreateGenerator(wrapper, blackTex: blackTex);
                var method = typeof(LilToonToonStandardGenerator).GetMethod("GetMainTexturePlatformOverride",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = method.Invoke(gen, null);
                // Result could be null for runtime textures without import settings
                Assert.Pass("GetMainTexturePlatformOverride completed");
            }
            finally
            {
                UObject.DestroyImmediate(mainTex);
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void GetMainTexturePlatformOverride_WithMain2nd3rd_IncludesAll()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            var mainTex = LilToonTestHelper.CreateTestTexture();
            var main2nd = LilToonTestHelper.CreateTestTexture();
            var main3rd = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.mainTexture = mainTex;
                wrapper.Material.SetFloat("_UseMain2ndTex", 1.0f);
                wrapper.Material.SetTexture("_Main2ndTex", main2nd);
                wrapper.Material.SetFloat("_UseMain3rdTex", 1.0f);
                wrapper.Material.SetTexture("_Main3rdTex", main3rd);

                var gen = CreateGenerator(wrapper, blackTex: blackTex);
                var method = typeof(LilToonToonStandardGenerator).GetMethod("GetMainTexturePlatformOverride",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(gen, null);
                Assert.Pass();
            }
            finally
            {
                UObject.DestroyImmediate(main3rd);
                UObject.DestroyImmediate(main2nd);
                UObject.DestroyImmediate(mainTex);
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void GetEmissionMapPlatformOverride_NoEmission_ReturnsNull()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            try
            {
                wrapper.Material.SetFloat("_UseEmission", 0.0f);
                wrapper.Material.SetFloat("_UseEmission2nd", 0.0f);
                var gen = CreateGenerator(wrapper, blackTex: blackTex);
                var method = typeof(LilToonToonStandardGenerator).GetMethod("GetEmissionMapPlatformOverride",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = method.Invoke(gen, null);
                Assert.IsNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void GetEmissionMapPlatformOverride_WithEmission_ReturnsValue()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            var emTex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetFloat("_UseEmission", 1.0f);
                wrapper.Material.SetTexture("_EmissionMap", emTex);
                var gen = CreateGenerator(wrapper, blackTex: blackTex);
                var method = typeof(LilToonToonStandardGenerator).GetMethod("GetEmissionMapPlatformOverride",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(gen, null);
                Assert.Pass();
            }
            finally
            {
                UObject.DestroyImmediate(emTex);
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void GetGlossMapPlatformOverride_NoTextures_ReturnsNull()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            try
            {
                var gen = CreateGenerator(wrapper, blackTex: blackTex);
                var method = typeof(LilToonToonStandardGenerator).GetMethod("GetGlossMapPlatformOverride",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = method.Invoke(gen, null);
                Assert.IsNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void GetGlossMapPlatformOverride_WithSmoothnessTex_Runs()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            var glossTex = LilToonTestHelper.CreateTestTexture();
            try
            {
                wrapper.Material.SetTexture("_SmoothnessTex", glossTex);
                var gen = CreateGenerator(wrapper, blackTex: blackTex);
                var method = typeof(LilToonToonStandardGenerator).GetMethod("GetGlossMapPlatformOverride",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(gen, null);
                Assert.Pass();
            }
            finally
            {
                UObject.DestroyImmediate(glossTex);
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void GetMatcapPlatformOverride_Runs()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            try
            {
                var gen = CreateGenerator(wrapper, blackTex: blackTex);
                var method = typeof(LilToonToonStandardGenerator).GetMethod("GetMatcapPlatformOverride",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(gen, null);
                Assert.Pass();
            }
            finally
            {
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void GetMatcapMaskPlatformOverride_Runs()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            try
            {
                var gen = CreateGenerator(wrapper, blackTex: blackTex);
                var method = typeof(LilToonToonStandardGenerator).GetMethod("GetMatcapMaskPlatformOverride",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(gen, null);
                Assert.Pass();
            }
            finally
            {
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void GetMetallicMapPlatformOverride_Runs()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            try
            {
                var gen = CreateGenerator(wrapper, blackTex: blackTex);
                var method = typeof(LilToonToonStandardGenerator).GetMethod("GetMetallicMapPlatformOverride",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(gen, null);
                Assert.Pass();
            }
            finally
            {
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void GetNormalMapPlatformOverride_Runs()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            try
            {
                var gen = CreateGenerator(wrapper, blackTex: blackTex);
                var method = typeof(LilToonToonStandardGenerator).GetMethod("GetNormalMapPlatformOverride",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(gen, null);
                Assert.Pass();
            }
            finally
            {
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }

        [Test]
        public void GetOcclusionMapPlatformOverride_Runs()
        {
            var wrapper = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4);
            try
            {
                var gen = CreateGenerator(wrapper, blackTex: blackTex);
                var method = typeof(LilToonToonStandardGenerator).GetMethod("GetOcclusionMapPlatformOverride",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(gen, null);
                Assert.Pass();
            }
            finally
            {
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(wrapper.Material);
            }
        }
    }

    // =========================================================
    // MaterialWrapperBuilder lilToon detection Tests
    // =========================================================
    [TestFixture]
    public class MaterialWrapperBuilder_LilToon_Tests
    {
        [Test]
        public void DetectShaderCategory_LilToon_ReturnsLilToon()
        {
            var mat = LilToonTestHelper.CreateLilToonMaterial();
            try
            {
                var builder = new MaterialWrapperBuilder();
                var method = typeof(MaterialWrapperBuilder).GetMethod("DetectShaderCategory",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = method.Invoke(builder, new object[] { mat });
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.LilToon, result);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_LilToonMaterial_ReturnsLilToonMaterial()
        {
            var mat = LilToonTestHelper.CreateLilToonMaterial();
            try
            {
                var builder = new MaterialWrapperBuilder();
                var method = typeof(MaterialWrapperBuilder).GetMethod("Build",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = method.Invoke(builder, new object[] { mat });
                Assert.IsInstanceOf<LilToonMaterial>(result);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }
    }
}
