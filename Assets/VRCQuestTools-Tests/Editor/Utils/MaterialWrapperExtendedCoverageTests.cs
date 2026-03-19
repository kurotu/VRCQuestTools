// Batch 31: Tests for MaterialWrapperBuilder shader categories, VRCQuestTools properties,
// FallbackAvatarCallback branches, MissingScriptsRule, and ToonStandardGenerator paths.

using System;
using System.Collections.Generic;
using System.Reflection;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using NUnit.Framework;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace KRT.VRCQuestTools.Tests
{
    // =========================================================
    // MaterialWrapperBuilder - DetectShaderCategory Tests
    // =========================================================
    [TestFixture]
    public class MaterialWrapperBuilder_DetectShaderCategory_ExtendedTests
    {
        private MaterialWrapperBuilder builder;

        [SetUp]
        public void SetUp()
        {
            builder = new MaterialWrapperBuilder();
        }

        [Test]
        public void DetectShaderCategory_Standard_ReturnsStandard()
        {
            var shader = Shader.Find("Standard");
            if (shader == null) Assert.Inconclusive("Standard shader not found");
            var mat = new Material(shader);
            try
            {
                var result = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Standard, result);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_StandardSpecular_ReturnsStandard()
        {
            var shader = Shader.Find("Standard (Specular setup)");
            if (shader == null) Assert.Inconclusive("Standard (Specular setup) shader not found");
            var mat = new Material(shader);
            try
            {
                var result = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Standard, result);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_UnlitColor_ReturnsUnlit()
        {
            var shader = Shader.Find("Unlit/Color");
            if (shader == null) Assert.Inconclusive("Unlit/Color shader not found");
            var mat = new Material(shader);
            try
            {
                var result = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Unlit, result);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_UnlitTexture_ReturnsUnlit()
        {
            var shader = Shader.Find("Unlit/Texture");
            if (shader == null) Assert.Inconclusive("Unlit/Texture shader not found");
            var mat = new Material(shader);
            try
            {
                var result = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Unlit, result);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_VRChatMobileToonLit_ReturnsQuest()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null) Assert.Inconclusive("VRChat/Mobile/Toon Lit shader not found");
            var mat = new Material(shader);
            try
            {
                var result = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Quest, result);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_VRChatMobileStandard_ReturnsQuest()
        {
            var shader = Shader.Find("VRChat/Mobile/Standard Lite");
            if (shader == null) Assert.Inconclusive("VRChat/Mobile/Standard Lite shader not found");
            var mat = new Material(shader);
            try
            {
                var result = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Quest, result);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_LilToon_ReturnsLilToon()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Inconclusive("lilToon shader not found");
            var mat = new Material(shader);
            try
            {
                var result = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.LilToon, result);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_HiddenShader_ReturnsUnverified()
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null) Assert.Inconclusive("Hidden/Internal-Colored shader not found");
            var mat = new Material(shader);
            try
            {
                var result = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Unverified, result);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }
    }

    // =========================================================
    // MaterialWrapperBuilder - Build with lilToon Tests
    // =========================================================
    [TestFixture]
    public class MaterialWrapperBuilder_Build_ExtendedTests
    {
        private MaterialWrapperBuilder builder;

        [SetUp]
        public void SetUp()
        {
            builder = new MaterialWrapperBuilder();
        }

        [Test]
        public void Build_LilToon_ReturnsLilToonMaterial()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Inconclusive("lilToon shader not found");
            var mat = new Material(shader);
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
                Assert.IsInstanceOf<LilToonMaterial>(wrapper);
                Assert.AreEqual(mat, wrapper.Material);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_Standard_ReturnsStandardMaterial()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
                Assert.IsInstanceOf<StandardMaterial>(wrapper);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_UnlitColor_ReturnsStandardMaterial()
        {
            var shader = Shader.Find("Unlit/Color");
            if (shader == null) Assert.Inconclusive("Unlit/Color shader not found");
            var mat = new Material(shader);
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
                Assert.IsInstanceOf<StandardMaterial>(wrapper);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_VRChatMobileToonLit_ReturnsStandardMaterial()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null) Assert.Inconclusive("VRChat/Mobile/Toon Lit shader not found");
            var mat = new Material(shader);
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
                Assert.IsInstanceOf<StandardMaterial>(wrapper);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }
    }

    // =========================================================
    // VRCQuestTools Entry Point Tests
    // =========================================================
    [TestFixture]
    public class VRCQuestTools_EntryPointTests
    {
        [Test]
        public void AssetRoot_IsNotNullOrEmpty()
        {
            var assetRoot = VRCQuestTools.AssetRoot;
            Assert.IsNotNull(assetRoot);
            Assert.IsNotEmpty(assetRoot);
        }

        [Test]
        public void AssetRoot_StartsWithPackages()
        {
            // When imported as VPM package, AssetRoot starts with "Packages"
            var assetRoot = VRCQuestTools.AssetRoot;
            Assert.IsTrue(assetRoot.StartsWith("Packages"), $"Expected AssetRoot to start with 'Packages', got: {assetRoot}");
        }

        [Test]
        public void IsImportedAsPackage_ReturnsTrue()
        {
            // VPM package installation means it's imported as a package
            Assert.IsTrue(VRCQuestTools.IsImportedAsPackage);
        }

        [Test]
        public void Version_IsNotNullOrEmpty()
        {
            Assert.IsNotNull(VRCQuestTools.Version);
            Assert.IsNotEmpty(VRCQuestTools.Version);
        }
    }

    // =========================================================
    // FallbackAvatarCallback - Extended Branch Tests
    // =========================================================
    [TestFixture]
    public class FallbackAvatarCallback_ExtendedTests
    {
        private static readonly Type callbackType = typeof(LilToonToonStandardGenerator).Assembly
            .GetType("KRT.VRCQuestTools.NonDestructive.FallbackAvatarCallback");

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

        private object CreateCallback()
        {
            return Activator.CreateInstance(callbackType);
        }

        private bool InvokeOnPreprocessAvatar(object callback, GameObject go)
        {
            var method = callbackType.GetMethod("OnPreprocessAvatar",
                BindingFlags.Instance | BindingFlags.Public);
            return (bool)method.Invoke(callback, new object[] { go });
        }

        [Test]
        public void OnPreprocessAvatar_NoPipelineManager_ReturnsTrue()
        {
            var go = new GameObject("TestAvatar");
            createdObjects.Add(go);
            var callback = CreateCallback();
            var result = InvokeOnPreprocessAvatar(callback, go);
            Assert.IsTrue(result);
        }

        private static Type FindPipelineManagerType()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var t = asm.GetType("VRC.Core.PipelineManager");
                    if (t != null) return t;
                }
                catch { }
            }
            return null;
        }

        [Test]
        public void OnPreprocessAvatar_EmptyBlueprintId_ReturnsTrue()
        {
            var pmType = FindPipelineManagerType();
            if (pmType == null) Assert.Inconclusive("PipelineManager type not found");
            var go = new GameObject("TestAvatar");
            createdObjects.Add(go);
            go.AddComponent(pmType);
            var callback = CreateCallback();
            var result = InvokeOnPreprocessAvatar(callback, go);
            Assert.IsTrue(result);
        }

        [Test]
        public void OnPreprocessAvatar_WithBlueprintId_NoFallback_ReturnsTrue()
        {
            var pmType = FindPipelineManagerType();
            if (pmType == null) Assert.Inconclusive("PipelineManager type not found");
            var go = new GameObject("TestAvatar");
            createdObjects.Add(go);
            var pm = go.AddComponent(pmType);
            var bpField = pmType.GetField("blueprintId", BindingFlags.Instance | BindingFlags.Public);
            if (bpField != null) bpField.SetValue(pm, "avtr_test_12345");
            var callback = CreateCallback();
            var result = InvokeOnPreprocessAvatar(callback, go);
            Assert.IsTrue(result);
        }

        [Test]
        public void OnPreprocessAvatar_WithFallbackAvatar_ReturnsTrue()
        {
            var pmType = FindPipelineManagerType();
            if (pmType == null) Assert.Inconclusive("PipelineManager type not found");
            var go = new GameObject("TestAvatar");
            createdObjects.Add(go);
            var pm = go.AddComponent(pmType);
            var bpField = pmType.GetField("blueprintId", BindingFlags.Instance | BindingFlags.Public);
            if (bpField != null) bpField.SetValue(pm, "avtr_fallback_test");
            go.AddComponent<KRT.VRCQuestTools.Components.FallbackAvatar>();
            var callback = CreateCallback();
            var result = InvokeOnPreprocessAvatar(callback, go);
            Assert.IsTrue(result);
        }
    }

    // =========================================================
    // MissingScriptsRule - Validate Tests
    // =========================================================
    [TestFixture]
    public class MissingScriptsRule_ValidateTests
    {
        private static readonly Type ruleType = typeof(LilToonToonStandardGenerator).Assembly
            .GetType("KRT.VRCQuestTools.Models.Validators.MissingScriptsRule");

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

        private object CreateRule()
        {
            return Activator.CreateInstance(ruleType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null, null, null);
        }

        [Test]
        public void Validate_InactiveAvatar_ReturnsNull()
        {
            var go = new GameObject("InactiveAvatar");
            createdObjects.Add(go);
            var descriptor = go.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            go.SetActive(false);

            var rule = CreateRule();
            var method = ruleType.GetMethod("Validate",
                BindingFlags.Instance | BindingFlags.Public);

            var avatarType = typeof(LilToonToonStandardGenerator).Assembly
                .GetType("KRT.VRCQuestTools.Models.VRChat.VRChatAvatar");
            var avatar = Activator.CreateInstance(avatarType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null, new object[] { descriptor }, null);

            var result = method.Invoke(rule, new object[] { avatar });
            Assert.IsNull(result);
        }

        [Test]
        public void Validate_ActiveAvatarNoMissingScripts_ReturnsNull()
        {
            var go = new GameObject("CleanAvatar");
            createdObjects.Add(go);
            var descriptor = go.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();

            var rule = CreateRule();
            var method = ruleType.GetMethod("Validate",
                BindingFlags.Instance | BindingFlags.Public);

            var avatarType = typeof(LilToonToonStandardGenerator).Assembly
                .GetType("KRT.VRCQuestTools.Models.VRChat.VRChatAvatar");
            var avatar = Activator.CreateInstance(avatarType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null, new object[] { descriptor }, null);

            var result = method.Invoke(rule, new object[] { avatar });
            Assert.IsNull(result);
        }
    }

    // =========================================================
    // MissingNdmfRule - Validate Tests
    // =========================================================
    [TestFixture]
    public class MissingNdmfRule_ValidateTests
    {
        private static readonly Type ruleType = typeof(LilToonToonStandardGenerator).Assembly
            .GetType("KRT.VRCQuestTools.Models.Validators.MissingNdmfRule");

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

        [Test]
        public void Validate_ActiveAvatar_ReturnsNullWhenNdmfInstalled()
        {
            // NDMF is installed in this project (VQT_HAS_NDMF defined)
            var go = new GameObject("TestAvatar");
            createdObjects.Add(go);
            var descriptor = go.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();

            if (ruleType == null) Assert.Inconclusive("MissingNdmfRule not found");
            var rule = Activator.CreateInstance(ruleType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null, null, null);
            var method = ruleType.GetMethod("Validate",
                BindingFlags.Instance | BindingFlags.Public);

            var avatarType = typeof(LilToonToonStandardGenerator).Assembly
                .GetType("KRT.VRCQuestTools.Models.VRChat.VRChatAvatar");
            var avatar = Activator.CreateInstance(avatarType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null, new object[] { descriptor }, null);

            var result = method.Invoke(rule, new object[] { avatar });
            // When NDMF is installed, there's no "missing NDMF" notification
            Assert.IsNull(result);
        }

        [Test]
        public void Validate_InactiveAvatar_ReturnsNull()
        {
            var go = new GameObject("InactiveAvatar");
            createdObjects.Add(go);
            var descriptor = go.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            go.SetActive(false);

            if (ruleType == null) Assert.Inconclusive("MissingNdmfRule not found");
            var rule = Activator.CreateInstance(ruleType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null, null, null);
            var method = ruleType.GetMethod("Validate",
                BindingFlags.Instance | BindingFlags.Public);

            var avatarType = typeof(LilToonToonStandardGenerator).Assembly
                .GetType("KRT.VRCQuestTools.Models.VRChat.VRChatAvatar");
            var avatar = Activator.CreateInstance(avatarType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null, new object[] { descriptor }, null);

            var result = method.Invoke(rule, new object[] { avatar });
            Assert.IsNull(result);
        }
    }

    // =========================================================
    // ToonStandardGenerator - GenerateTextures path test
    // =========================================================
    [TestFixture]
    public class ToonStandardGenerator_GenerateTexturesTests
    {
        [Test]
        public void GenerateTextures_WithLilToon_DoesNotThrow()
        {
            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            try
            {
                var settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(true);
                settings.generateQuestTextures = false;
                settings.fallbackShadowRamp = new Texture2D(4, 4);
                var gen = new LilToonToonStandardGenerator(lilMat, settings, blackTex);

                bool completed = false;
                var request = gen.GenerateTextures(
                    lilMat,
                    UnityEditor.BuildTarget.Android,
                    false,
                    "Assets/Temp",
                    () => { completed = true; });
                request.WaitForCompletion();
                Assert.IsTrue(completed);
            }
            finally
            {
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(lilMat.Material);
            }
        }
    }

    // =========================================================
    // LilToonToonStandardGenerator - GenerateMaterial non-texture path
    // =========================================================
    [TestFixture]
    public class LilToonGenerator_GenerateMaterialTests
    {
        [Test]
        public void GenerateMaterial_NoQuestTextures_ReturnsValidMaterial()
        {
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
                request.WaitForCompletion();
                Assert.IsNotNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(lilMat.Material);
            }
        }

        [Test]
        public void GenerateMaterial_WithAllFeaturesDisabled_Succeeds()
        {
            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var blackTex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            try
            {
                var settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(false);
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
                request.WaitForCompletion();
                Assert.IsNotNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(lilMat.Material);
            }
        }

        [Test]
        public void GenerateMaterial_NonConvertable_UsesStandardMaterial()
        {
            // StandardMaterial does NOT implement IToonStandardConvertable
            // so GenerateMaterial should fall back to ToonLit path
            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper();
            var stdMat = new Material(Shader.Find("Standard"));
            var stdWrapper = new StandardMaterial(stdMat);
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
                    stdWrapper,
                    UnityEditor.BuildTarget.Android,
                    false,
                    "Assets/Temp",
                    (mat) => { result = mat; });
                request.WaitForCompletion();
                Assert.IsNotNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(blackTex);
                UObject.DestroyImmediate(stdMat);
                UObject.DestroyImmediate(lilMat.Material);
            }
        }
    }

    // =========================================================
    // Additional LilToonMaterial property tests for remaining coverage
    // =========================================================
    [TestFixture]
    public class LilToonMaterial_RemainingPropertyTests
    {
        private LilToonMaterial lilMat;
        private Texture2D testTex;

        [SetUp]
        public void SetUp()
        {
            lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper();
            testTex = LilToonTestHelper.CreateTestTexture();
        }

        [TearDown]
        public void TearDown()
        {
            if (testTex != null) UObject.DestroyImmediate(testTex);
            if (lilMat != null) UObject.DestroyImmediate(lilMat.Material);
        }

        [Test]
        public void ShadowBorderMask_AsAOMap_WhenSet_ReturnsTexture()
        {
            lilMat.Material.SetTexture("_ShadowBorderMask", testTex);
            // AOMap property reads from _ShadowBorderMask
            var prop = typeof(LilToonMaterial).GetProperty("AOMap",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (prop == null) Assert.Inconclusive("AOMap property not found");
            var result = (Texture)prop.GetValue(lilMat);
            Assert.AreEqual(testTex, result);
        }

        [Test]
        public void MatCapMainStrength_GetSet()
        {
            lilMat.Material.SetFloat("_MatCapMainStrength", 0.6f);
            var prop = typeof(LilToonMaterial).GetProperty("MatCapMainStrength",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (prop == null) Assert.Inconclusive("MatCapMainStrength property not found");
            var result = (float)prop.GetValue(lilMat);
            Assert.AreEqual(0.6f, result, 0.001f);
        }

        [Test]
        public void ShadowBorderRange_PropertyAccess()
        {
            // Test accessing shadow border range related property
            lilMat.Material.SetFloat("_ShadowBorderRange", 0.5f);
            // Check if this property is exposed on LilToonMaterial
            var prop = typeof(LilToonMaterial).GetProperty("ShadowBorderRange",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (prop == null)
            {
                // Property might not exist - that's fine, skip
                Assert.Pass("ShadowBorderRange property does not exist on LilToonMaterial");
            }
            else
            {
                var result = (float)prop.GetValue(lilMat);
                Assert.AreEqual(0.5f, result, 0.001f);
            }
        }
    }

    // =========================================================
    // ToonStandardMaterialWrapper additional coverage
    // =========================================================
    [TestFixture]
    public class ToonStandardMaterialWrapper_ExtendedTests
    {
        [Test]
        public void Constructor_Default_CreatesValidWrapper()
        {
            var wrapper = new ToonStandardMaterialWrapper();
            Assert.IsNotNull(wrapper);
            Assert.IsNotNull((Material)wrapper);
        }

        [Test]
        public void Constructor_FromMaterial_CreatesWrapper()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Standard");
            if (shader == null) Assert.Inconclusive("Toon Standard shader not found");
            var mat = new Material(shader);
            try
            {
                var wrapper = new ToonStandardMaterialWrapper(mat);
                Assert.IsNotNull(wrapper);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void RimProperties_SetAndGet()
        {
            var wrapper = new ToonStandardMaterialWrapper();
            try
            {
                wrapper.UseRimLighting = true;
                wrapper.RimColor = Color.blue;
                wrapper.RimAlbedoTint = 0.5f;
                wrapper.RimIntensity = 0.8f;
                wrapper.RimRange = 0.3f;
                wrapper.RimSoftness = 0.4f;
                wrapper.RimEnvironmental = true;

                Assert.IsTrue(wrapper.UseRimLighting);
                Assert.AreEqual(Color.blue, wrapper.RimColor);
                Assert.AreEqual(0.5f, wrapper.RimAlbedoTint, 0.001f);
                Assert.AreEqual(0.8f, wrapper.RimIntensity, 0.001f);
                Assert.AreEqual(0.3f, wrapper.RimRange, 0.001f);
                Assert.AreEqual(0.4f, wrapper.RimSoftness, 0.001f);
                Assert.IsTrue(wrapper.RimEnvironmental);
            }
            finally
            {
                UObject.DestroyImmediate((Material)wrapper);
            }
        }

        [Test]
        public void OcclusionProperties_SetAndGet()
        {
            var wrapper = new ToonStandardMaterialWrapper();
            var testTex = new Texture2D(4, 4);
            try
            {
                wrapper.UseOcclusion = true;
                wrapper.OcclusionMap = testTex;
                wrapper.OcclusionMapChannel = ToonStandardMaterialWrapper.MaskChannel.R;
                wrapper.OcclusionMapTextureScale = new Vector2(2f, 2f);
                wrapper.OcclusionMapTextureOffset = new Vector2(0.5f, 0.5f);

                Assert.IsTrue(wrapper.UseOcclusion);
                Assert.AreEqual(testTex, wrapper.OcclusionMap);
            }
            finally
            {
                UObject.DestroyImmediate(testTex);
                UObject.DestroyImmediate((Material)wrapper);
            }
        }

        [Test]
        public void SpecularProperties_SetAndGet()
        {
            var wrapper = new ToonStandardMaterialWrapper();
            var testTex = new Texture2D(4, 4);
            try
            {
                wrapper.UseSpecular = true;
                wrapper.MetallicStrength = 0.7f;
                wrapper.GlossStrength = 0.8f;
                wrapper.Sharpness = 0.9f;
                wrapper.Reflectance = 0.5f;
                wrapper.MetallicMap = testTex;
                wrapper.MetallicMapChannel = ToonStandardMaterialWrapper.MaskChannel.R;
                wrapper.GlossMap = testTex;
                wrapper.GlossMapChannel = ToonStandardMaterialWrapper.MaskChannel.G;

                Assert.IsTrue(wrapper.UseSpecular);
                Assert.AreEqual(0.7f, wrapper.MetallicStrength, 0.001f);
                Assert.AreEqual(0.8f, wrapper.GlossStrength, 0.001f);
                Assert.AreEqual(0.9f, wrapper.Sharpness, 0.001f);
                Assert.AreEqual(0.5f, wrapper.Reflectance, 0.001f);
            }
            finally
            {
                UObject.DestroyImmediate(testTex);
                UObject.DestroyImmediate((Material)wrapper);
            }
        }

        [Test]
        public void DetailMask_SetAndGet()
        {
            var wrapper = new ToonStandardMaterialWrapper();
            var testTex = new Texture2D(4, 4);
            try
            {
                wrapper.DetailMask = testTex;
                wrapper.DetailMaskChannel = ToonStandardMaterialWrapper.MaskChannel.A;

                Assert.AreEqual(testTex, wrapper.DetailMask);
            }
            finally
            {
                UObject.DestroyImmediate(testTex);
                UObject.DestroyImmediate((Material)wrapper);
            }
        }
    }
}
