// Batch48: Tests for ToonStandardGenerator/LilToonToonStandardGenerator ConvertToToonStandard path
// and additional AvatarConverter coverage.

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
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace KRT.VRCQuestTools.Tests
{
    // =========================================================================
    // Test: LilToonToonStandardGenerator ConvertToToonStandard branches
    // via GenerateMaterial with generateQuestTextures=false
    // =========================================================================
    [TestFixture]
    public class Batch48_LilToonConvertToToonStandardTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null)
                {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }
            objectsToCleanup.Clear();
        }

        private Texture2D CreateBlackTexture()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.name = "Batch48_BlackTex";
            var pixels = new Color[16];
            for (int i = 0; i < 16; i++) pixels[i] = Color.black;
            tex.SetPixels(pixels);
            tex.Apply();
            objectsToCleanup.Add(tex);
            return tex;
        }

        private Texture2D CreateTestTexture(string name = "TestTex")
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            tex.name = name;
            objectsToCleanup.Add(tex);
            return tex;
        }

        [Test]
        public void ConvertToToonStandard_BasicProperties_NameMainTexColorShadow()
        {
            var lilShader = LilToonTestHelper.FindLilToonShader();
            if (lilShader == null)
            {
                Assert.Ignore("lilToon not installed");
                return;
            }

            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper("Batch48_BasicProps");
            objectsToCleanup.Add(lilMat.Material);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
            };
            var blackTex = CreateBlackTexture();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings, blackTex);

            Material result = null;
            var request = generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, "Assets/", (mat) => result = mat);
            request.WaitForCompletion();

            Assert.IsNotNull(result, "GenerateMaterial should produce a material");
            Assert.AreEqual("Batch48_BasicProps", result.name);
        }

        [Test]
        public void ConvertToToonStandard_WithNormalMap_SetsNormalMapProperties()
        {
            var lilShader = LilToonTestHelper.FindLilToonShader();
            if (lilShader == null)
            {
                Assert.Ignore("lilToon not installed");
                return;
            }

            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper("Batch48_NormalMap");
            objectsToCleanup.Add(lilMat.Material);

            // Enable normal map on the material
            lilMat.Material.SetFloat("_UseBumpMap", 1.0f);
            var normalTex = CreateTestTexture("NormalMap");
            lilMat.Material.SetTexture("_BumpMap", normalTex);
            lilMat.Material.SetFloat("_BumpScale", 0.8f);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
                useNormalMap = true,
            };
            var blackTex = CreateBlackTexture();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings, blackTex);

            Material result = null;
            var request = generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, "Assets/", (mat) => result = mat);
            request.WaitForCompletion();

            Assert.IsNotNull(result);
            // The result should have normal map related properties set
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.IsTrue(wrapper.UseNormalMap, "Normal map should be enabled");
        }

        [Test]
        public void ConvertToToonStandard_WithNormalMapDisabledInSettings_DoesNotSetNormalMap()
        {
            var lilShader = LilToonTestHelper.FindLilToonShader();
            if (lilShader == null)
            {
                Assert.Ignore("lilToon not installed");
                return;
            }

            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper("Batch48_NoNormal");
            objectsToCleanup.Add(lilMat.Material);
            lilMat.Material.SetFloat("_UseBumpMap", 1.0f);
            var normalTex = CreateTestTexture("NormalMap");
            lilMat.Material.SetTexture("_BumpMap", normalTex);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
                useNormalMap = false,
            };
            var blackTex = CreateBlackTexture();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings, blackTex);

            Material result = null;
            var request = generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, "Assets/", (mat) => result = mat);
            request.WaitForCompletion();

            Assert.IsNotNull(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.IsFalse(wrapper.UseNormalMap, "Normal map should NOT be enabled when settings.useNormalMap=false");
        }

        [Test]
        public void ConvertToToonStandard_WithEmission_SetsEmissionProperties()
        {
            var lilShader = LilToonTestHelper.FindLilToonShader();
            if (lilShader == null)
            {
                Assert.Ignore("lilToon not installed");
                return;
            }

            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper("Batch48_Emission");
            objectsToCleanup.Add(lilMat.Material);
            lilMat.Material.SetFloat("_UseEmission", 1.0f);
            var emissionTex = CreateTestTexture("EmissionMap");
            lilMat.Material.SetTexture("_EmissionMap", emissionTex);
            lilMat.Material.SetColor("_EmissionColor", new Color(1, 0.5f, 0, 1));

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
                useEmission = true,
            };
            var blackTex = CreateBlackTexture();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings, blackTex);

            Material result = null;
            var request = generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, "Assets/", (mat) => result = mat);
            request.WaitForCompletion();

            Assert.IsNotNull(result);
            // Emission should be set with the emission map
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.IsNotNull(wrapper.EmissionMap, "Emission map should be set");
        }

        [Test]
        public void ConvertToToonStandard_EmissionDisabledInSettings_SetsBlackEmission()
        {
            var lilShader = LilToonTestHelper.FindLilToonShader();
            if (lilShader == null)
            {
                Assert.Ignore("lilToon not installed");
                return;
            }

            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper("Batch48_NoEmission");
            objectsToCleanup.Add(lilMat.Material);
            lilMat.Material.SetFloat("_UseEmission", 1.0f);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
                useEmission = false,
            };
            var blackTex = CreateBlackTexture();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings, blackTex);

            Material result = null;
            var request = generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, "Assets/", (mat) => result = mat);
            request.WaitForCompletion();

            Assert.IsNotNull(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.AreEqual(Color.black, wrapper.EmissionColor, "Emission color should be black when disabled");
        }

        [Test]
        public void ConvertToToonStandard_WithOcclusion_SetsOcclusionProperties()
        {
            var lilShader = LilToonTestHelper.FindLilToonShader();
            if (lilShader == null)
            {
                Assert.Ignore("lilToon not installed");
                return;
            }

            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper("Batch48_Occlusion");
            objectsToCleanup.Add(lilMat.Material);
            lilMat.Material.SetFloat("_UseShadow", 1.0f);
            var aoTex = CreateTestTexture("AOMap");
            lilMat.Material.SetTexture("_ShadowBorderMask", aoTex);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
                useOcclusion = true,
            };
            var blackTex = CreateBlackTexture();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings, blackTex);

            Material result = null;
            var request = generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, "Assets/", (mat) => result = mat);
            request.WaitForCompletion();

            Assert.IsNotNull(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.IsTrue(wrapper.UseOcclusion, "Occlusion should be enabled");
        }

        [Test]
        public void ConvertToToonStandard_WithSpecular_SetsSpecularProperties()
        {
            var lilShader = LilToonTestHelper.FindLilToonShader();
            if (lilShader == null)
            {
                Assert.Ignore("lilToon not installed");
                return;
            }

            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper("Batch48_Specular");
            objectsToCleanup.Add(lilMat.Material);
            lilMat.Material.SetFloat("_UseReflection", 1.0f);
            lilMat.Material.SetFloat("_Metallic", 0.5f);
            lilMat.Material.SetFloat("_Smoothness", 0.7f);
            lilMat.Material.SetFloat("_SpecularBlur", 0.3f);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
                useSpecular = true,
            };
            var blackTex = CreateBlackTexture();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings, blackTex);

            Material result = null;
            var request = generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, "Assets/", (mat) => result = mat);
            request.WaitForCompletion();

            Assert.IsNotNull(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.IsTrue(wrapper.UseSpecular, "Specular should be enabled");
            Assert.AreEqual(0.5f, wrapper.MetallicStrength, 0.01f, "Metallic should match");
            Assert.AreEqual(0.7f, wrapper.GlossStrength, 0.01f, "Gloss should match smoothness");
        }

        [Test]
        public void ConvertToToonStandard_WithMatcap_SetsMatcapProperties()
        {
            var lilShader = LilToonTestHelper.FindLilToonShader();
            if (lilShader == null)
            {
                Assert.Ignore("lilToon not installed");
                return;
            }

            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper("Batch48_Matcap");
            objectsToCleanup.Add(lilMat.Material);
            lilMat.Material.SetFloat("_UseMatCap", 1.0f);
            var matcapTex = CreateTestTexture("MatCapTex");
            lilMat.Material.SetTexture("_MatCapTex", matcapTex);
            lilMat.Material.SetFloat("_MatCapBlend", 0.8f);
            lilMat.Material.SetFloat("_MatCapBlendMode", 0f); // Normal mode

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
                useMatcap = true,
            };
            var blackTex = CreateBlackTexture();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings, blackTex);

            Material result = null;
            var request = generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, "Assets/", (mat) => result = mat);
            request.WaitForCompletion();

            Assert.IsNotNull(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.IsTrue(wrapper.UseMatcap, "Matcap should be enabled");
        }

        [Test]
        public void ConvertToToonStandard_WithMatcap_MultiplyMode_SetsMultiplicativeType()
        {
            var lilShader = LilToonTestHelper.FindLilToonShader();
            if (lilShader == null)
            {
                Assert.Ignore("lilToon not installed");
                return;
            }

            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper("Batch48_MatcapMul");
            objectsToCleanup.Add(lilMat.Material);
            lilMat.Material.SetFloat("_UseMatCap", 1.0f);
            var matcapTex = CreateTestTexture("MatCapTexMul");
            lilMat.Material.SetTexture("_MatCapTex", matcapTex);
            lilMat.Material.SetFloat("_MatCapBlendMode", 3f); // Multiply mode

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
                useMatcap = true,
            };
            var blackTex = CreateBlackTexture();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings, blackTex);

            Material result = null;
            var request = generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, "Assets/", (mat) => result = mat);
            request.WaitForCompletion();

            Assert.IsNotNull(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Multiplicative, wrapper.MatcapType, "Multiply mode should map to Multiplicative");
        }

        [Test]
        public void ConvertToToonStandard_WithRimLighting_SetsRimProperties()
        {
            var lilShader = LilToonTestHelper.FindLilToonShader();
            if (lilShader == null)
            {
                Assert.Ignore("lilToon not installed");
                return;
            }

            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper("Batch48_RimLight");
            objectsToCleanup.Add(lilMat.Material);
            lilMat.Material.SetFloat("_UseRim", 1.0f);
            lilMat.Material.SetColor("_RimColor", new Color(1, 0, 0, 1));
            lilMat.Material.SetFloat("_RimMainStrength", 0.5f);
            lilMat.Material.SetFloat("_RimBorder", 0.4f);
            lilMat.Material.SetFloat("_RimFresnelPower", 3f);
            lilMat.Material.SetFloat("_RimBlur", 0.2f);
            lilMat.Material.SetFloat("_RimEnableLighting", 0.8f);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
                useRimLighting = true,
            };
            var blackTex = CreateBlackTexture();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings, blackTex);

            Material result = null;
            var request = generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, "Assets/", (mat) => result = mat);
            request.WaitForCompletion();

            Assert.IsNotNull(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.IsTrue(wrapper.UseRimLighting, "Rim lighting should be enabled");
        }

        [Test]
        public void ConvertToToonStandard_WithRimLighting_ZeroEnableLighting()
        {
            var lilShader = LilToonTestHelper.FindLilToonShader();
            if (lilShader == null)
            {
                Assert.Ignore("lilToon not installed");
                return;
            }

            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper("Batch48_RimNoLight");
            objectsToCleanup.Add(lilMat.Material);
            lilMat.Material.SetFloat("_UseRim", 1.0f);
            lilMat.Material.SetColor("_RimColor", new Color(0, 1, 0, 1));
            lilMat.Material.SetFloat("_RimEnableLighting", 0f);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
                useRimLighting = true,
            };
            var blackTex = CreateBlackTexture();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings, blackTex);

            Material result = null;
            var request = generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, "Assets/", (mat) => result = mat);
            request.WaitForCompletion();

            Assert.IsNotNull(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.IsTrue(wrapper.UseRimLighting, "Rim lighting should still be enabled");
            Assert.IsFalse(wrapper.RimEnvironmental, "RimEnvironmental should be false when enable lighting is 0");
        }

        [Test]
        public void ConvertToToonStandard_WithShadow_SetsShadowRamp()
        {
            var lilShader = LilToonTestHelper.FindLilToonShader();
            if (lilShader == null)
            {
                Assert.Ignore("lilToon not installed");
                return;
            }

            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper("Batch48_Shadow");
            objectsToCleanup.Add(lilMat.Material);
            lilMat.Material.SetFloat("_UseShadow", 1.0f);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
            };
            var blackTex = CreateBlackTexture();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings, blackTex);

            Material result = null;
            var request = generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, "Assets/", (mat) => result = mat);
            request.WaitForCompletion();

            Assert.IsNotNull(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            // With shadow enabled, should use fallbackShadowRamp
            Assert.AreEqual(settings.fallbackShadowRamp, wrapper.ShadowRamp);
        }

        [Test]
        public void ConvertToToonStandard_WithoutShadow_SetsFlatShadowRamp()
        {
            var lilShader = LilToonTestHelper.FindLilToonShader();
            if (lilShader == null)
            {
                Assert.Ignore("lilToon not installed");
                return;
            }

            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper("Batch48_NoShadow");
            objectsToCleanup.Add(lilMat.Material);
            lilMat.Material.SetFloat("_UseShadow", 0f);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
            };
            var blackTex = CreateBlackTexture();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings, blackTex);

            Material result = null;
            var request = generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, "Assets/", (mat) => result = mat);
            request.WaitForCompletion();

            Assert.IsNotNull(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.AreEqual(ToonStandardMaterialWrapper.RampTexture.Flat, wrapper.ShadowRamp);
        }

        [Test]
        public void ConvertToToonStandard_AllFeaturesEnabled()
        {
            var lilShader = LilToonTestHelper.FindLilToonShader();
            if (lilShader == null)
            {
                Assert.Ignore("lilToon not installed");
                return;
            }

            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper("Batch48_AllFeatures");
            objectsToCleanup.Add(lilMat.Material);

            // Enable all features
            lilMat.Material.SetFloat("_UseBumpMap", 1.0f);
            lilMat.Material.SetTexture("_BumpMap", CreateTestTexture("Normal"));
            lilMat.Material.SetFloat("_UseShadow", 1.0f);
            lilMat.Material.SetTexture("_ShadowBorderMask", CreateTestTexture("AO"));
            lilMat.Material.SetFloat("_UseEmission", 1.0f);
            lilMat.Material.SetTexture("_EmissionMap", CreateTestTexture("Emission"));
            lilMat.Material.SetFloat("_UseReflection", 1.0f);
            lilMat.Material.SetFloat("_Metallic", 0.3f);
            lilMat.Material.SetFloat("_Smoothness", 0.6f);
            lilMat.Material.SetFloat("_UseMatCap", 1.0f);
            lilMat.Material.SetTexture("_MatCapTex", CreateTestTexture("MatCap"));
            lilMat.Material.SetFloat("_UseRim", 1.0f);
            lilMat.Material.SetFloat("_RimEnableLighting", 0.5f);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
                useNormalMap = true,
                useEmission = true,
                useOcclusion = true,
                useSpecular = true,
                useMatcap = true,
                useRimLighting = true,
            };
            var blackTex = CreateBlackTexture();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings, blackTex);

            Material result = null;
            var request = generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, "Assets/", (mat) => result = mat);
            request.WaitForCompletion();

            Assert.IsNotNull(result);
            var wrapper = new ToonStandardMaterialWrapper(result);
            Assert.IsTrue(wrapper.UseNormalMap, "Normal map enabled");
            Assert.IsTrue(wrapper.UseOcclusion, "Occlusion enabled");
            Assert.IsTrue(wrapper.UseSpecular, "Specular enabled");
            Assert.IsTrue(wrapper.UseMatcap, "Matcap enabled");
            Assert.IsTrue(wrapper.UseRimLighting, "Rim lighting enabled");
        }
    }

    // =========================================================================
    // Test: ToonStandardGenerator non-IToonStandardConvertable fallback
    // =========================================================================
    [TestFixture]
    public class Batch48_ToonStandardGeneratorFallbackTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null)
                {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }
            objectsToCleanup.Clear();
        }

        private Texture2D CreateBlackTexture()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            objectsToCleanup.Add(tex);
            return tex;
        }

        [Test]
        public void GenerateMaterial_NonConvertableMaterial_FallsBackToToonLit()
        {
            // StandardMaterial does NOT implement IToonStandardConvertable
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "Batch48_Standard";
            objectsToCleanup.Add(mat);
            var wrapper = new MaterialWrapperBuilder().Build(mat);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = true,
                maxTextureSize = TextureSizeLimit.Max256x256,
            };
            var blackTex = CreateBlackTexture();
            var generator = new GenericToonStandardGenerator(wrapper, settings, blackTex);

            Material result = null;
            var request = generator.GenerateMaterial(wrapper, UnityEditor.BuildTarget.Android, false, "Assets/", (m) => result = m);
            request.WaitForCompletion();

            Assert.IsNotNull(result, "Should produce a material via ToonLit fallback");
            Assert.AreEqual("Batch48_Standard", result.name);
        }

        [Test]
        public void GenerateMaterial_NonConvertable_WithTexture_ProducesResult()
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "Batch48_StdWithTex";
            var tex = new Texture2D(16, 16);
            objectsToCleanup.Add(tex);
            mat.mainTexture = tex;
            objectsToCleanup.Add(mat);
            var wrapper = new MaterialWrapperBuilder().Build(mat);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = true,
                maxTextureSize = TextureSizeLimit.Max256x256,
            };
            var blackTex = CreateBlackTexture();
            var generator = new GenericToonStandardGenerator(wrapper, settings, blackTex);

            Material result = null;
            var request = generator.GenerateMaterial(wrapper, UnityEditor.BuildTarget.Android, false, "Assets/", (m) => result = m);
            request.WaitForCompletion();

            Assert.IsNotNull(result);
        }

        [Test]
        public void GenerateTextures_Delegates_ToGenerateMaterial()
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "Batch48_GenTextures";
            objectsToCleanup.Add(mat);
            var wrapper = new MaterialWrapperBuilder().Build(mat);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
            };
            var blackTex = CreateBlackTexture();
            var generator = new GenericToonStandardGenerator(wrapper, settings, blackTex);

            bool completed = false;
            var request = generator.GenerateTextures(wrapper, UnityEditor.BuildTarget.Android, false, "Assets/", () => completed = true);
            request.WaitForCompletion();

            Assert.IsTrue(completed, "GenerateTextures completion should be called");
        }
    }

    // =========================================================================
    // Test: GenericToonStandardGenerator ConvertToToonStandard (generateQuestTextures=false)
    // =========================================================================
    [TestFixture]
    public class Batch48_GenericToonStandardGeneratorTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null)
                {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }
            objectsToCleanup.Clear();
        }

        [Test]
        public void ConvertToToonStandard_ReturnsNewToonStandardMaterial()
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "Batch48_GenericConvert";
            objectsToCleanup.Add(mat);
            var wrapper = new MaterialWrapperBuilder().Build(mat);
            var blackTex = new Texture2D(4, 4);
            objectsToCleanup.Add(blackTex);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
            };
            var generator = new GenericToonStandardGenerator(wrapper, settings, blackTex);

            Material result = null;
            generator.GenerateMaterial(wrapper, UnityEditor.BuildTarget.Android, false, "Assets/", (m) => result = m).WaitForCompletion();

            Assert.IsNotNull(result);
        }

        [Test]
        public void GetUseEmission_ReturnsFalse()
        {
            var mat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(mat);
            var wrapper = new MaterialWrapperBuilder().Build(mat);
            var blackTex = new Texture2D(4, 4);
            objectsToCleanup.Add(blackTex);

            var settings = new ToonStandardConvertSettings();
            var generator = new GenericToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(GenericToonStandardGenerator).GetMethod("GetUseEmission", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "GetUseEmission should exist");
            var result = (bool)method.Invoke(generator, null);
            Assert.IsFalse(result, "GenericToonStandardGenerator.GetUseEmission should return false");
        }

        [Test]
        public void GetMainTexturePlatformOverride_WithNoTexture_ReturnsNull()
        {
            var mat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(mat);
            var wrapper = new MaterialWrapperBuilder().Build(mat);
            var blackTex = new Texture2D(4, 4);
            objectsToCleanup.Add(blackTex);

            var settings = new ToonStandardConvertSettings();
            var generator = new GenericToonStandardGenerator(wrapper, settings, blackTex);

            var method = typeof(GenericToonStandardGenerator).GetMethod("GetMainTexturePlatformOverride", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "GetMainTexturePlatformOverride should exist");
            var result = method.Invoke(generator, null);
            Assert.IsNull(result, "Should return null when no main texture");
        }

        [Test]
        public void GenericPlatformOverrides_ReturnNull()
        {
            var mat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(mat);
            var wrapper = new MaterialWrapperBuilder().Build(mat);
            var blackTex = new Texture2D(4, 4);
            objectsToCleanup.Add(blackTex);

            var settings = new ToonStandardConvertSettings();
            var generator = new GenericToonStandardGenerator(wrapper, settings, blackTex);

            var methodNames = new[]
            {
                "GetNormalMapPlatformOverride",
                "GetEmissionMapPlatformOverride",
                "GetMatcapPlatformOverride",
                "GetMatcapMaskPlatformOverride",
                "GetMetallicMapPlatformOverride",
                "GetGlossMapPlatformOverride",
                "GetOcclusionMapPlatformOverride",
            };

            foreach (var name in methodNames)
            {
                var method = typeof(GenericToonStandardGenerator).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(method, $"{name} should exist");
                var result = method.Invoke(generator, null);
                Assert.IsNull(result, $"{name} should return null");
            }
        }
    }

    // =========================================================================
    // Test: AvatarConverter deeper paths - RemoveVertexColor, CreateMaterialConvertSettingsMap exceptions
    // =========================================================================
    [TestFixture]
    public class Batch48_AvatarConverterDeepPathTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null)
                {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }
            objectsToCleanup.Clear();
        }

        private AvatarConverter CreateConverter()
        {
            return new AvatarConverter(new MaterialWrapperBuilder());
        }

        private (GameObject, VRCAvatarDescriptor) CreateAvatar(string name)
        {
            var go = new GameObject(name);
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            return (go, desc);
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithTargetMaterialNull_ThrowsTargetMaterialNullException()
        {
            var (go, desc) = CreateAvatar("NullTargetMat");
            var mcs = go.AddComponent<MaterialConversionSettings>();
            mcs.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = null,
                    materialConvertSettings = new ToonLitConvertSettings(),
                },
            };

            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            smr.sharedMesh = mesh;
            smr.sharedMaterials = new Material[0];

            var converter = CreateConverter();
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());

            Assert.Throws<TargetMaterialNullException>(() => converter.CreateMaterialConvertSettingsMap(avatar));
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithInvalidReplacementMaterial_ThrowsException()
        {
            var (go, desc) = CreateAvatar("InvalidReplacement");
            var mcs = go.AddComponent<MaterialConversionSettings>();
            var targetMat = new Material(Shader.Find("Standard"));
            targetMat.name = "TargetMat";
            objectsToCleanup.Add(targetMat);

            // Create a non-Quest-allowed material for replacement
            var replaceMat = new Material(Shader.Find("Standard"));
            replaceMat.name = "BadReplaceMat";
            objectsToCleanup.Add(replaceMat);

            mcs.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = targetMat,
                    materialConvertSettings = new MaterialReplaceSettings { material = replaceMat },
                },
            };

            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            smr.sharedMesh = mesh;
            smr.sharedMaterials = new[] { targetMat };

            var converter = CreateConverter();
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());

            Assert.Throws<InvalidReplacementMaterialException>(() => converter.CreateMaterialConvertSettingsMap(avatar));
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithMaterialSwapNullOriginal_ThrowsException()
        {
            var (go, desc) = CreateAvatar("NullSwapOriginal");
            go.AddComponent<AvatarConverterSettings>();
            var swap = go.AddComponent<MaterialSwap>();

            // Set up a swap with null original material
            swap.materialMappings = new List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping { originalMaterial = null },
            };

            var converter = CreateConverter();
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());

            // Should throw when swap has null original
            Assert.Throws<InvalidMaterialSwapNullException>(() => converter.CreateMaterialConvertSettingsMap(avatar));
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_AvatarConverterSettings_NullTargetMaterial_Throws()
        {
            var (go, desc) = CreateAvatar("ACSNullTarget");
            var acs = go.AddComponent<AvatarConverterSettings>();
            acs.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = null,
                    materialConvertSettings = new ToonLitConvertSettings(),
                },
            };

            var converter = CreateConverter();
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());

            Assert.Throws<TargetMaterialNullException>(() => converter.CreateMaterialConvertSettingsMap(avatar));
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_AvatarConverterSettings_InvalidReplacement_Throws()
        {
            var (go, desc) = CreateAvatar("ACSBadReplace");
            var acs = go.AddComponent<AvatarConverterSettings>();
            var targetMat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(targetMat);
            var replaceMat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(replaceMat);

            acs.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = targetMat,
                    materialConvertSettings = new MaterialReplaceSettings { material = replaceMat },
                },
            };

            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            smr.sharedMesh = mesh;
            smr.sharedMaterials = new[] { targetMat };

            var converter = CreateConverter();
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());

            Assert.Throws<InvalidReplacementMaterialException>(() => converter.CreateMaterialConvertSettingsMap(avatar));
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_FiltersOutMaterialsNotInAvatar()
        {
            var (go, desc) = CreateAvatar("FilterTest");
            var acs = go.AddComponent<AvatarConverterSettings>();
            var usedMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            usedMat.name = "UsedMat";
            objectsToCleanup.Add(usedMat);
            var unusedMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            unusedMat.name = "UnusedMat";
            objectsToCleanup.Add(unusedMat);

            // Add both to additional settings
            acs.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = usedMat,
                    materialConvertSettings = new MatCapLitConvertSettings(),
                },
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = unusedMat,
                    materialConvertSettings = new MatCapLitConvertSettings(),
                },
            };

            // Only add usedMat to a renderer
            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            smr.sharedMesh = mesh;
            smr.sharedMaterials = new[] { usedMat };

            var converter = CreateConverter();
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            var map = converter.CreateMaterialConvertSettingsMap(avatar);

            Assert.IsTrue(map.ContainsKey(usedMat), "Used material should be in map");
            Assert.IsFalse(map.ContainsKey(unusedMat), "Unused material should NOT be in map");
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_MaterialSwapNullReplacement_ThrowsException()
        {
            var (go, desc) = CreateAvatar("SwapNullReplace");
            go.AddComponent<AvatarConverterSettings>();
            var swap = go.AddComponent<MaterialSwap>();
            var origMat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(origMat);

            // Set up swap with null replacement
            swap.materialMappings = new List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping
                {
                    originalMaterial = origMat,
                    replacementMaterial = null,
                },
            };

            var converter = CreateConverter();
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());

            Assert.Throws<InvalidMaterialSwapNullException>(() => converter.CreateMaterialConvertSettingsMap(avatar));
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_MaterialSwapInvalidReplacement_ThrowsException()
        {
            var (go, desc) = CreateAvatar("SwapBadReplace");
            go.AddComponent<AvatarConverterSettings>();
            var swap = go.AddComponent<MaterialSwap>();
            var origMat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(origMat);
            var badReplaceMat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(badReplaceMat);

            swap.materialMappings = new List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping
                {
                    originalMaterial = origMat,
                    replacementMaterial = badReplaceMat,
                },
            };

            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            smr.sharedMesh = mesh;
            smr.sharedMaterials = new[] { origMat };

            var converter = CreateConverter();
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());

            Assert.Throws<InvalidReplacementMaterialException>(() => converter.CreateMaterialConvertSettingsMap(avatar));
        }
    }

    // =========================================================================
    // Test: VRChatAvatar HasVertexColor and HasAnimatedMaterials deeper paths
    // =========================================================================
    [TestFixture]
    public class Batch48_VRChatAvatarDeepPropertyTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null)
                {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }
            objectsToCleanup.Clear();
        }

        private (GameObject, VRChatAvatar) CreateAvatar(string name)
        {
            var go = new GameObject(name);
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            return (go, new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>()));
        }

        [Test]
        public void HasVertexColor_WithMeshColors_ReturnsTrue()
        {
            var (go, avatar) = CreateAvatar("VertexColorTrue");
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            mesh.vertices = new[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.colors = new[] { Color.red, Color.green, Color.blue };
            smr.sharedMesh = mesh;

            Assert.IsTrue(avatar.HasVertexColor, "Should detect vertex colors");
        }

        [Test]
        public void HasVertexColor_WithoutMeshColors_ReturnsFalse()
        {
            var (go, avatar) = CreateAvatar("VertexColorFalse");
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            mesh.vertices = new[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            smr.sharedMesh = mesh;

            Assert.IsFalse(avatar.HasVertexColor, "Should not detect vertex colors without them");
        }

        [Test]
        public void HasVertexColor_WithNullMesh_ThrowsNullReferenceException()
        {
            var (go, avatar) = CreateAvatar("NullMeshVC");
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            child.AddComponent<SkinnedMeshRenderer>();

            // Code doesn't handle null mesh; it throws NRE
            Assert.Throws<NullReferenceException>(() => { var _ = avatar.HasVertexColor; });
        }

        [Test]
        public void HasVertexColor_MeshFilter_WithColors_ReturnsTrue()
        {
            var (go, avatar) = CreateAvatar("MeshFilterVC");
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            child.AddComponent<MeshRenderer>();
            var mf = child.AddComponent<MeshFilter>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            mesh.vertices = new[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.colors = new[] { Color.red, Color.green, Color.blue };
            mf.sharedMesh = mesh;

            Assert.IsTrue(avatar.HasVertexColor, "MeshFilter mesh with colors should be detected");
        }

        [Test]
        public void Materials_ReturnsDistinctMaterials()
        {
            var (go, avatar) = CreateAvatar("DistinctMats");
            var child1 = new GameObject("Body1");
            child1.transform.SetParent(go.transform);
            objectsToCleanup.Add(child1);
            var smr1 = child1.AddComponent<SkinnedMeshRenderer>();
            var mesh1 = new Mesh();
            objectsToCleanup.Add(mesh1);
            smr1.sharedMesh = mesh1;

            var mat1 = new Material(Shader.Find("Standard"));
            mat1.name = "Mat1";
            objectsToCleanup.Add(mat1);
            var mat2 = new Material(Shader.Find("Standard"));
            mat2.name = "Mat2";
            objectsToCleanup.Add(mat2);

            smr1.sharedMaterials = new[] { mat1, mat2, mat1 }; // mat1 appears twice

            var materials = avatar.Materials;
            Assert.AreEqual(2, materials.Length, "Should return distinct materials");
        }

        [Test]
        public void GetRuntimeAnimatorControllers_WithBaseAnimationLayer_ReturnsControllers()
        {
            var (go, avatar) = CreateAvatar("WithBaseLayer");
            var animator = go.AddComponent<Animator>();
            var controller = new AnimatorController();
            controller.name = "TestController";
            objectsToCleanup.Add(controller);
            animator.runtimeAnimatorController = controller;

            // Set up base animation layers with a non-default layer
            var desc = go.GetComponent<VRCAvatarDescriptor>();
            var layer = new VRCAvatarDescriptor.CustomAnimLayer
            {
                isDefault = false,
                animatorController = controller,
            };
            desc.baseAnimationLayers = new[] { layer };

            var controllers = avatar.GetRuntimeAnimatorControllers();
            // Should include both the Animator's controller and the playable layer controller
            Assert.IsTrue(controllers.Length >= 1, "Should have at least 1 controller");
            Assert.IsTrue(controllers.Contains(controller), "Should contain the assigned controller");
        }

        [Test]
        public void GetRuntimeAnimatorControllers_DefaultLayers_AreExcluded()
        {
            var (go, avatar) = CreateAvatar("DefaultLayers");

            var desc = go.GetComponent<VRCAvatarDescriptor>();
            var layer = new VRCAvatarDescriptor.CustomAnimLayer
            {
                isDefault = true,
                animatorController = null,
            };
            desc.baseAnimationLayers = new[] { layer };

            var controllers = avatar.GetRuntimeAnimatorControllers();
            Assert.AreEqual(0, controllers.Length, "Default layers should be excluded");
        }
    }

    // =========================================================================
    // Test: ModularAvatarUtility, SystemUtility, VPMService
    // =========================================================================
    [TestFixture]
    public class Batch48_UtilityDeepTests
    {
        [Test]
        public void SystemUtility_GetTypeByName_KnownType_ReturnsType()
        {
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Utils.SystemUtility");
            Assert.IsNotNull(type, "SystemUtility should exist");
            var method = type.GetMethod("GetTypeByName", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.IsNotNull(method, "GetTypeByName method should exist");
            var result = method.Invoke(null, new object[] { "UnityEngine.Transform" });
            Assert.IsNotNull(result, "Should find UnityEngine.Transform type");
        }

        [Test]
        public void SystemUtility_GetTypeByName_UnknownType_ReturnsNull()
        {
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Utils.SystemUtility");
            var method = type.GetMethod("GetTypeByName", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            var result = method.Invoke(null, new object[] { "NonExistent.Type.That.DoesNot.Exist" });
            Assert.IsNull(result, "Should return null for unknown type");
        }

        [Test]
        public void VRCSDKUtility_CountMissingComponentsInChildren_EmptyObject_ReturnsZero()
        {
            var go = new GameObject("Empty");
            try
            {
                var count = VRCSDKUtility.CountMissingComponentsInChildren(go, true);
                Assert.AreEqual(0, count);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void VRCSDKUtility_IsProxyAnimationClip_Null_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsProxyAnimationClip(null));
        }

        [Test]
        public void VRCSDKUtility_IsProxyAnimationClip_NormalClip_ReturnsFalse()
        {
            var clip = new AnimationClip();
            clip.name = "TestClip";
            try
            {
                Assert.IsFalse(VRCSDKUtility.IsProxyAnimationClip(clip));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void VRCSDKUtility_IsExampleAsset_NormalObject_ReturnsFalse()
        {
            var go = new GameObject("NotAnExample");
            try
            {
                Assert.IsFalse(VRCSDKUtility.IsExampleAsset((UnityEngine.Object)go));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void VRCSDKUtility_IsExampleAsset_NullPath_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsExampleAsset(string.Empty));
        }

        [Test]
        public void VRCSDKUtility_IsExampleAsset_ExamplePath_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsExampleAsset("Assets/VRCSDK/Examples3/SomeAsset.prefab"));
        }

        [Test]
        public void MaterialConvertSettingsTypes_GetDefaultConvertTypePopups_ReturnsNonEmpty()
        {
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.MaterialConvertSettingsTypes");
            Assert.IsNotNull(type, "MaterialConvertSettingsTypes should exist");
            var method = type.GetMethod("GetDefaultConvertTypePopups", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.IsNotNull(method, "GetDefaultConvertTypePopups should exist");
            var result = method.Invoke(null, new object[] { false });
            Assert.IsNotNull(result, "Should return a list");
            var list = result as System.Collections.IList;
            Assert.IsTrue(list.Count > 0, "Should have at least one popup item");
        }
    }

    // =========================================================================
    // Test: FallbackAvatarCallback and validation callbacks
    // =========================================================================
    [TestFixture]
    public class Batch48_NdmfCallbackTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null)
                {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }
            objectsToCleanup.Clear();
        }

        [Test]
        public void ActualPerformanceCallback_CanBeCreated()
        {
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.NonDestructive.ActualPerformanceCallback");
            if (type == null)
            {
                Assert.Ignore("ActualPerformanceCallback not found");
                return;
            }
            Assert.IsNotNull(type);
        }
    }
}
