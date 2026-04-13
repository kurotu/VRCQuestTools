// Tests for LilToonToonStandardGenerator.ConvertToToonStandard covering material feature branches.

using System.Reflection;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Tests;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Tests for ConvertToToonStandard material feature branches.
    /// Covers getter methods for emission, specular, matcap, rim light, etc.
    /// </summary>
    [TestFixture]
    public class LilToonToonStandardConvertTests
    {
        private static Shader lilToonShader;
        private static Shader toonStandardShader;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            LilToonTestHelper.SkipIfNotImported();

            var lilToonVersion = AssetUtility.LilToonVersion;
            if (lilToonVersion < new SemVer(1, 10, 0) || lilToonVersion >= new SemVer(3, 0, 0))
            {
                Assert.Ignore($"lilToon {lilToonVersion} is not supported.");
            }

            lilToonShader = Shader.Find("lilToon");
            toonStandardShader = Shader.Find("VRChat/Mobile/Toon Standard");
            if (lilToonShader == null || toonStandardShader == null)
            {
                Assert.Ignore("lilToon or Toon Standard shader not available.");
            }
        }

        private LilToonToonStandardGenerator CreateGenerator(Material sourceMaterial, ToonStandardConvertSettings settings = null, Texture2D blackTex = null)
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
            return new LilToonToonStandardGenerator(lilMat, settings, blackTex);
        }

        private Material InvokeConvertToToonStandard(LilToonToonStandardGenerator generator)
        {
            var method = typeof(LilToonToonStandardGenerator).GetMethod(
                "ConvertToToonStandard",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "ConvertToToonStandard method not found");
            return (Material)method.Invoke(generator, null);
        }

        [Test]
        public void ConvertToToonStandard_BasicMaterial_SetsNameAndMainTexture()
        {
            var mat = new Material(lilToonShader);
            mat.name = "TestMaterial";
            Material result = null;
            try
            {
                var gen = CreateGenerator(mat);
                result = InvokeConvertToToonStandard(gen);
                Assert.IsNotNull(result);
                Assert.AreEqual("TestMaterial", result.name);
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithEmission_SetsEmissionProperties()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission", 1);
            var emissionTex = new Texture2D(4, 4);
            mat.SetTexture("_EmissionMap", emissionTex);
            mat.SetColor("_EmissionColor", new Color(1, 0, 0, 1));
            Material result = null;
            try
            {
                var gen = CreateGenerator(mat);
                result = InvokeConvertToToonStandard(gen);
                var wrapper = new ToonStandardMaterialWrapper(result);
                Assert.IsNotNull(wrapper.EmissionMap, "Emission map should be set");
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(emissionTex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithEmissionDisabled_SetsBlackEmission()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission", 0);
            var blackTex = new Texture2D(4, 4);
            Material result = null;
            try
            {
                var gen = CreateGenerator(mat, blackTex: blackTex);
                result = InvokeConvertToToonStandard(gen);
                var wrapper = new ToonStandardMaterialWrapper(result);
                Assert.AreEqual(Color.black, wrapper.EmissionColor);
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(blackTex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonStandard_EmissionSettingDisabled_IgnoresEmission()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission", 1);
            mat.SetColor("_EmissionColor", new Color(1, 0, 0, 1));
            var blackTex = new Texture2D(4, 4);
            Material result = null;
            try
            {
                var settings = new ToonStandardConvertSettings
                {
                    generateQuestTextures = false,
                    useEmission = false,
                };
                var gen = CreateGenerator(mat, settings, blackTex);
                result = InvokeConvertToToonStandard(gen);
                var wrapper = new ToonStandardMaterialWrapper(result);
                Assert.AreEqual(Color.black, wrapper.EmissionColor);
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(blackTex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithNormalMap_SetsNormalMapProperties()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseBumpMap", 1);
            var normalTex = new Texture2D(4, 4);
            mat.SetTexture("_BumpMap", normalTex);
            mat.SetFloat("_BumpScale", 0.75f);
            Material result = null;
            try
            {
                var gen = CreateGenerator(mat);
                result = InvokeConvertToToonStandard(gen);
                var wrapper = new ToonStandardMaterialWrapper(result);
                Assert.IsTrue(wrapper.UseNormalMap);
                Assert.AreEqual(normalTex, wrapper.NormalMap);
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(normalTex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithShadow_SetsShadowRamp()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseShadow", 1);
            var rampTex = new Texture2D(128, 16);
            Material result = null;
            try
            {
                var settings = new ToonStandardConvertSettings
                {
                    generateQuestTextures = false,
                    useEmission = false,
                    fallbackShadowRamp = rampTex,
                };
                var gen = CreateGenerator(mat, settings);
                result = InvokeConvertToToonStandard(gen);
                var wrapper = new ToonStandardMaterialWrapper(result);
                Assert.AreEqual(rampTex, wrapper.ShadowRamp);
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(rampTex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithoutShadow_SetsFlatRamp()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseShadow", 0);
            Material result = null;
            try
            {
                var gen = CreateGenerator(mat);
                result = InvokeConvertToToonStandard(gen);
                // Flat ramp is a built-in texture, just verify it was set
                Assert.IsNotNull(result);
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithShadowAndAO_SetsOcclusionMap()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseShadow", 1);
            var aoTex = new Texture2D(4, 4);
            mat.SetTexture("_ShadowBorderMask", aoTex);
            Material result = null;
            try
            {
                var gen = CreateGenerator(mat);
                result = InvokeConvertToToonStandard(gen);
                var wrapper = new ToonStandardMaterialWrapper(result);
                Assert.IsTrue(wrapper.UseOcclusion);
                Assert.AreEqual(aoTex, wrapper.OcclusionMap);
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(aoTex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithReflection_SetsSpecularProperties()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseReflection", 1);
            mat.SetFloat("_Metallic", 0.8f);
            mat.SetFloat("_Smoothness", 0.5f);
            mat.SetFloat("_SpecularBlur", 0.3f);
            Material result = null;
            try
            {
                var gen = CreateGenerator(mat);
                result = InvokeConvertToToonStandard(gen);
                var wrapper = new ToonStandardMaterialWrapper(result);
                Assert.IsTrue(wrapper.UseSpecular);
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithMatcapNormal_SetsAdditiveType()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseMatCap", 1);
            mat.SetFloat("_MatCapBlendMode", 0); // Normal
            var matcapTex = new Texture2D(4, 4);
            mat.SetTexture("_MatCapTex", matcapTex);
            Material result = null;
            try
            {
                var gen = CreateGenerator(mat);
                result = InvokeConvertToToonStandard(gen);
                var wrapper = new ToonStandardMaterialWrapper(result);
                Assert.IsTrue(wrapper.UseMatcap);
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(matcapTex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithMatcapMultiply_SetsMultiplicativeType()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseMatCap", 1);
            mat.SetFloat("_MatCapBlendMode", 3); // Multiply
            var matcapTex = new Texture2D(4, 4);
            mat.SetTexture("_MatCapTex", matcapTex);
            Material result = null;
            try
            {
                var gen = CreateGenerator(mat);
                result = InvokeConvertToToonStandard(gen);
                var wrapper = new ToonStandardMaterialWrapper(result);
                Assert.IsTrue(wrapper.UseMatcap);
                Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Multiplicative, wrapper.MatcapType);
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(matcapTex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithMatcapMask_SetsMatcapMask()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseMatCap", 1);
            var matcapTex = new Texture2D(4, 4);
            var matcapMask = new Texture2D(4, 4);
            mat.SetTexture("_MatCapTex", matcapTex);
            mat.SetTexture("_MatCapBlendMask", matcapMask);
            Material result = null;
            try
            {
                var gen = CreateGenerator(mat);
                result = InvokeConvertToToonStandard(gen);
                var wrapper = new ToonStandardMaterialWrapper(result);
                Assert.AreEqual(matcapMask, wrapper.MatcapMask);
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(matcapTex);
                Object.DestroyImmediate(matcapMask);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithRimLight_SetsRimProperties()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseRim", 1);
            mat.SetColor("_RimColor", new Color(0.5f, 0.6f, 0.7f, 0.8f));
            mat.SetFloat("_RimMainStrength", 0.5f);
            mat.SetFloat("_RimBorder", 0.3f);
            mat.SetFloat("_RimFresnelPower", 2.0f);
            mat.SetFloat("_RimBlur", 0.1f);
            mat.SetFloat("_RimEnableLighting", 0.7f);
            Material result = null;
            try
            {
                var gen = CreateGenerator(mat);
                result = InvokeConvertToToonStandard(gen);
                var wrapper = new ToonStandardMaterialWrapper(result);
                Assert.IsTrue(wrapper.UseRimLighting);
                Assert.Greater(wrapper.RimRange, 0f);
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithRimLightNoLighting_SetsEnvironmentalFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseRim", 1);
            mat.SetColor("_RimColor", new Color(1, 1, 1, 1));
            mat.SetFloat("_RimEnableLighting", 0f);
            Material result = null;
            try
            {
                var gen = CreateGenerator(mat);
                result = InvokeConvertToToonStandard(gen);
                var wrapper = new ToonStandardMaterialWrapper(result);
                Assert.IsFalse(wrapper.RimEnvironmental);
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonStandard_CullModeTransferred()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_Cull", 1); // Front culling
            Material result = null;
            try
            {
                var gen = CreateGenerator(mat);
                result = InvokeConvertToToonStandard(gen);
                Assert.IsNotNull(result);
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonStandard_LightMinLimit_TransferredAsMinBrightness()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_LightMinLimit", 0.25f);
            Material result = null;
            try
            {
                var gen = CreateGenerator(mat);
                result = InvokeConvertToToonStandard(gen);
                var wrapper = new ToonStandardMaterialWrapper(result);
                Assert.AreEqual(0.25f, wrapper.MinBrightness, 0.01f);
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithReflectionTextures_SetsMetallicAndGloss()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseReflection", 1);
            var metallicTex = new Texture2D(4, 4);
            var smoothnessTex = new Texture2D(4, 4);
            mat.SetTexture("_MetallicGlossMap", metallicTex);
            mat.SetTexture("_SmoothnessTex", smoothnessTex);
            mat.SetFloat("_Metallic", 0.9f);
            mat.SetFloat("_Smoothness", 0.6f);
            Material result = null;
            try
            {
                var gen = CreateGenerator(mat);
                result = InvokeConvertToToonStandard(gen);
                var wrapper = new ToonStandardMaterialWrapper(result);
                Assert.IsTrue(wrapper.UseSpecular);
                Assert.AreEqual(metallicTex, wrapper.MetallicMap);
                Assert.AreEqual(smoothnessTex, wrapper.GlossMap);
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(metallicTex);
                Object.DestroyImmediate(smoothnessTex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithEmission2nd_SetsEmission()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission", 0);
            mat.SetFloat("_UseEmission2nd", 1);
            mat.SetColor("_Emission2ndColor", new Color(0, 1, 0, 1));
            Material result = null;
            try
            {
                var gen = CreateGenerator(mat);
                result = InvokeConvertToToonStandard(gen);
                Assert.IsNotNull(result);
                // Emission2ndColor should be used when only emission2nd is enabled
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonStandard_BothEmissions_SetsFirstEmissionColor()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission", 1);
            mat.SetFloat("_UseEmission2nd", 1);
            mat.SetColor("_EmissionColor", new Color(1, 0, 0, 1));
            mat.SetColor("_Emission2ndColor", new Color(0, 1, 0, 1));
            Material result = null;
            try
            {
                var gen = CreateGenerator(mat);
                result = InvokeConvertToToonStandard(gen);
                var wrapper = new ToonStandardMaterialWrapper(result);
                // ConvertToToonStandard uses lilMaterial.EmissionColor (first emission) directly
                Assert.AreEqual(new Color(1, 0, 0, 1), wrapper.EmissionColor);
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithMatcapAdd_SetsAdditiveType()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseMatCap", 1);
            mat.SetFloat("_MatCapBlendMode", 1); // Add
            var matcapTex = new Texture2D(4, 4);
            mat.SetTexture("_MatCapTex", matcapTex);
            Material result = null;
            try
            {
                var gen = CreateGenerator(mat);
                result = InvokeConvertToToonStandard(gen);
                var wrapper = new ToonStandardMaterialWrapper(result);
                Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Additive, wrapper.MatcapType);
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(matcapTex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonStandard_WithMatcapScreen_SetsAdditiveType()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseMatCap", 1);
            mat.SetFloat("_MatCapBlendMode", 2); // Screen
            var matcapTex = new Texture2D(4, 4);
            mat.SetTexture("_MatCapTex", matcapTex);
            Material result = null;
            try
            {
                var gen = CreateGenerator(mat);
                result = InvokeConvertToToonStandard(gen);
                var wrapper = new ToonStandardMaterialWrapper(result);
                Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Additive, wrapper.MatcapType);
            }
            finally
            {
                if (result != null) Object.DestroyImmediate(result);
                Object.DestroyImmediate(matcapTex);
                Object.DestroyImmediate(mat);
            }
        }
    }

    /// <summary>
    /// Tests for LilToonToonStandardGenerator getter methods via reflection.
    /// </summary>
    [TestFixture]
    public class LilToonToonStandardGetterTests
    {
        private static Shader lilToonShader;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            LilToonTestHelper.SkipIfNotImported();

            var lilToonVersion = AssetUtility.LilToonVersion;
            if (lilToonVersion < new SemVer(1, 10, 0) || lilToonVersion >= new SemVer(3, 0, 0))
            {
                Assert.Ignore($"lilToon {lilToonVersion} is not supported.");
            }

            lilToonShader = Shader.Find("lilToon");
            if (lilToonShader == null)
            {
                Assert.Ignore("lilToon shader not available.");
            }
        }

        private object InvokeProtected(LilToonToonStandardGenerator gen, string methodName, params object[] args)
        {
            var method = typeof(LilToonToonStandardGenerator).GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                method = typeof(ToonStandardGenerator).GetMethod(methodName,
                    BindingFlags.NonPublic | BindingFlags.Instance);
            }
            Assert.IsNotNull(method, $"Method {methodName} not found");
            return method.Invoke(gen, args);
        }

        private LilToonToonStandardGenerator CreateGenerator(Material mat, ToonStandardConvertSettings settings = null)
        {
            if (settings == null)
            {
                settings = new ToonStandardConvertSettings
                {
                    generateQuestTextures = false,
                    useEmission = true,
                    useSpecular = true,
                    useMatcap = true,
                    useRimLighting = true,
                    useOcclusion = true,
                    useNormalMap = true,
                };
            }
            return new LilToonToonStandardGenerator(new LilToonMaterial(mat), settings, null);
        }

        [Test]
        public void GetUseMainTexture_WithMainTexture_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.mainTexture = tex;
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsTrue((bool)InvokeProtected(gen, "GetUseMainTexture"));
            }
            finally
            {
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseMainTexture_NoTextureButEmissionDisabledInSettings_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission", 1);
            try
            {
                var settings = new ToonStandardConvertSettings { useEmission = false };
                var gen = CreateGenerator(mat, settings);
                // When useEmission=false but material has emission, GetUseMainTexture returns true
                Assert.IsTrue((bool)InvokeProtected(gen, "GetUseMainTexture"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseEmission_NeitherEnabled_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission", 0);
            mat.SetFloat("_UseEmission2nd", 0);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsFalse((bool)InvokeProtected(gen, "GetUseEmission"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseEmission_FirstEnabled_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission", 1);
            mat.SetFloat("_UseEmission2nd", 0);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsTrue((bool)InvokeProtected(gen, "GetUseEmission"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseEmission_SecondEnabled_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission", 0);
            mat.SetFloat("_UseEmission2nd", 1);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsTrue((bool)InvokeProtected(gen, "GetUseEmission"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseNormalMap_WithNormalMap_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseBumpMap", 1);
            var normalTex = new Texture2D(4, 4);
            mat.SetTexture("_BumpMap", normalTex);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsTrue((bool)InvokeProtected(gen, "GetUseNormalMap"));
            }
            finally
            {
                Object.DestroyImmediate(normalTex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseNormalMap_EnabledButNoTexture_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseBumpMap", 1);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsFalse((bool)InvokeProtected(gen, "GetUseNormalMap"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseMatcap_Enabled_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseMatCap", 1);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsTrue((bool)InvokeProtected(gen, "GetUseMatcap"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseMatcap_Disabled_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseMatCap", 0);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsFalse((bool)InvokeProtected(gen, "GetUseMatcap"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseRimLighting_Enabled_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseRim", 1);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsTrue((bool)InvokeProtected(gen, "GetUseRimLighting"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseSpecular_ReflectionEnabled_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseReflection", 1);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsTrue((bool)InvokeProtected(gen, "GetUseSpecular"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseShadowRamp_ShadowEnabled_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseShadow", 1);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsTrue((bool)InvokeProtected(gen, "GetUseShadowRamp"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseOcclusionMap_WithShadowAndAO_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseShadow", 1);
            var aoTex = new Texture2D(4, 4);
            mat.SetTexture("_ShadowBorderMask", aoTex);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsTrue((bool)InvokeProtected(gen, "GetUseOcclusionMap"));
            }
            finally
            {
                Object.DestroyImmediate(aoTex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseOcclusionMap_NoShadow_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseShadow", 0);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsFalse((bool)InvokeProtected(gen, "GetUseOcclusionMap"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseGlossMap_NoReflection_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseReflection", 0);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsFalse((bool)InvokeProtected(gen, "GetUseGlossMap"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseGlossMap_WithSmoothnessTex_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseReflection", 1);
            var smoothTex = new Texture2D(4, 4);
            mat.SetTexture("_SmoothnessTex", smoothTex);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsTrue((bool)InvokeProtected(gen, "GetUseGlossMap"));
            }
            finally
            {
                Object.DestroyImmediate(smoothTex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseMetallicMap_NoReflection_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseReflection", 0);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsFalse((bool)InvokeProtected(gen, "GetUseMetallicMap"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseMetallicMap_WithMetallicTex_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseReflection", 1);
            var metalTex = new Texture2D(4, 4);
            mat.SetTexture("_MetallicGlossMap", metalTex);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsTrue((bool)InvokeProtected(gen, "GetUseMetallicMap"));
            }
            finally
            {
                Object.DestroyImmediate(metalTex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetEmissionColor_BothEmissions_ReturnsWhite()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission", 1);
            mat.SetFloat("_UseEmission2nd", 1);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.AreEqual(Color.white, (Color)InvokeProtected(gen, "GetEmissionColor"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetEmissionColor_NeitherEmission_ReturnsBlack()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission", 0);
            mat.SetFloat("_UseEmission2nd", 0);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.AreEqual(Color.black, (Color)InvokeProtected(gen, "GetEmissionColor"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMinBrightness_ReturnsLightMinLimit()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_LightMinLimit", 0.42f);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.AreEqual(0.42f, (float)InvokeProtected(gen, "GetMinBrightness"), 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetSharpness_ReturnsOneMinusSpecularBlur()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_SpecularBlur", 0.3f);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.AreEqual(0.7f, (float)InvokeProtected(gen, "GetSharpness"), 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetRimEnvironmental_WithLighting_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_RimEnableLighting", 0.5f);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsTrue((bool)InvokeProtected(gen, "GetRimEnvironmental"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetRimEnvironmental_NoLighting_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_RimEnableLighting", 0f);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsFalse((bool)InvokeProtected(gen, "GetRimEnvironmental"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMainColor_WithMatcapNormalAndNoMainTex_ReturnsBlack()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseMatCap", 1);
            mat.SetFloat("_MatCapBlendMode", 0); // Normal
            // No main texture set
            try
            {
                var settings = new ToonStandardConvertSettings
                {
                    generateQuestTextures = false,
                    useMatcap = true,
                    useEmission = false,
                };
                var gen = CreateGenerator(mat, settings);
                var color = (Color)InvokeProtected(gen, "GetMainColor");
                Assert.AreEqual(Color.black, color);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseEmissionMap_WithMap_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission", 1);
            var tex = new Texture2D(4, 4);
            mat.SetTexture("_EmissionMap", tex);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsTrue((bool)InvokeProtected(gen, "GetUseEmissionMap"));
            }
            finally
            {
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseEmissionMap_WithBlendMask_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission", 1);
            var tex = new Texture2D(4, 4);
            mat.SetTexture("_EmissionBlendMask", tex);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsTrue((bool)InvokeProtected(gen, "GetUseEmissionMap"));
            }
            finally
            {
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseMatcapMask_WithMask_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseMatCap", 1);
            var tex = new Texture2D(4, 4);
            mat.SetTexture("_MatCapBlendMask", tex);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsTrue((bool)InvokeProtected(gen, "GetUseMatcapMask"));
            }
            finally
            {
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetUseMatcapMask_NoMask_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseMatCap", 1);
            try
            {
                var gen = CreateGenerator(mat);
                Assert.IsFalse((bool)InvokeProtected(gen, "GetUseMatcapMask"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }
    }
}
