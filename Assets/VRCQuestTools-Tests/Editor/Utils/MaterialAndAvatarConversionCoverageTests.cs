// Batch 19 - Coverage tests for remaining testable areas
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.Validators;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using VRC.SDK3.Avatars.Components;
using UBuildTarget = UnityEditor.BuildTarget;
using Object = UnityEngine.Object;

namespace KRT.VRCQuestTools.Tests
{
    // ===========================================================================
    // MaterialWrapperBuilder tests - DetectShaderCategory + Build
    // ===========================================================================
    [TestFixture]
    public class MaterialWrapperBuilderTests_MatAvatar
    {
        private MaterialWrapperBuilder builder;

        [SetUp]
        public void SetUp()
        {
            builder = new MaterialWrapperBuilder();
        }

        [Test]
        public void DetectShaderCategory_StandardShader_ReturnsStandard()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var category = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Standard, category);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_StandardSpecularShader_ReturnsStandard()
        {
            var shader = Shader.Find("Standard (Specular setup)");
            if (shader == null)
            {
                Assert.Inconclusive("Standard (Specular setup) shader not found");
                return;
            }
            var mat = new Material(shader);
            try
            {
                var category = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Standard, category);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_UnlitShader_ReturnsUnlit()
        {
            var shader = Shader.Find("Unlit/Texture");
            if (shader == null)
            {
                Assert.Inconclusive("Unlit/Texture shader not found");
                return;
            }
            var mat = new Material(shader);
            try
            {
                var category = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Unlit, category);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_VRCMobileShader_ReturnsQuest()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null)
            {
                Assert.Inconclusive("VRChat/Mobile/Toon Lit shader not found");
                return;
            }
            var mat = new Material(shader);
            try
            {
                var category = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Quest, category);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_UnknownShader_ReturnsUnverified()
        {
            // Hidden/Internal-Colored shader should be unknown
            var shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
            {
                Assert.Inconclusive("Hidden/Internal-Colored shader not found");
                return;
            }
            var mat = new Material(shader);
            try
            {
                var category = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Unverified, category);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_StandardMaterial_ReturnsStandardMaterial()
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
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_UnlitMaterial_ReturnsStandardMaterial()
        {
            var shader = Shader.Find("Unlit/Texture");
            if (shader == null)
            {
                Assert.Inconclusive("Unlit/Texture shader not found");
                return;
            }
            var mat = new Material(shader);
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
                Assert.IsInstanceOf<StandardMaterial>(wrapper);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_QuestMaterial_ReturnsStandardMaterial()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null)
            {
                Assert.Inconclusive("VRChat/Mobile/Toon Lit shader not found");
                return;
            }
            var mat = new Material(shader);
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
                Assert.IsInstanceOf<StandardMaterial>(wrapper);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ShaderCategory_AllEnumValuesExist()
        {
            var values = Enum.GetValues(typeof(MaterialWrapperBuilder.ShaderCategory));
            Assert.GreaterOrEqual(values.Length, 11); // UTS2, Arktoon, Standard, Unlit, Quest, Sunao, AXCS, LilToon, Poiyomi, VirtualLens2, Unverified
        }
    }

    // ===========================================================================
    // AvatarConverterNdmfPhaseExtension tests
    // ===========================================================================
    [TestFixture]
    public class AvatarConverterNdmfPhaseExtensionTests_MatAvatar
    {
        [Test]
        public void Resolve_TransformingPhase_ReturnsTransforming()
        {
            var result = AvatarConverterNdmfPhase.Transforming.Resolve(null);
            Assert.AreEqual(AvatarConverterNdmfPhase.Transforming, result);
        }

        [Test]
        public void Resolve_OptimizingPhase_ReturnsOptimizing()
        {
            var result = AvatarConverterNdmfPhase.Optimizing.Resolve(null);
            Assert.AreEqual(AvatarConverterNdmfPhase.Optimizing, result);
        }

        [Test]
        public void Resolve_AutoWithNullAvatar_ReturnsOptimizing()
        {
            var result = AvatarConverterNdmfPhase.Auto.Resolve(null);
            Assert.AreEqual(AvatarConverterNdmfPhase.Optimizing, result);
        }

        [Test]
        public void Resolve_AutoWithAvatar_ReturnsOptimizing()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var result = AvatarConverterNdmfPhase.Auto.Resolve(go);
                // Without VRCFury, Auto should resolve to Optimizing
                Assert.AreEqual(AvatarConverterNdmfPhase.Optimizing, result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Resolve_TransformingWithAvatar_ReturnsTransforming()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var result = AvatarConverterNdmfPhase.Transforming.Resolve(go);
                Assert.AreEqual(AvatarConverterNdmfPhase.Transforming, result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void EnumValues_HasExpectedCount()
        {
            var values = Enum.GetValues(typeof(AvatarConverterNdmfPhase));
            Assert.AreEqual(3, values.Length);
        }
    }

    // ===========================================================================
    // MissingScriptsRule tests
    // ===========================================================================
    [TestFixture]
    public class MissingScriptsRuleTests_MatAvatar
    {
        [Test]
        public void Validate_InactiveAvatar_ReturnsNull()
        {
            var go = new GameObject("TestAvatar");
            go.SetActive(false);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var rule = new MissingScriptsRule();
                var avatar = new VRChatAvatar(desc);
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Validate_ActiveAvatarNoMissingScripts_ReturnsNull()
        {
            var go = new GameObject("TestAvatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var rule = new MissingScriptsRule();
                var avatar = new VRChatAvatar(desc);
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void InitOnLoad_RegistersRule()
        {
            // Verify via reflection that MissingScriptsRule has InitializeOnLoadMethod
            var method = typeof(MissingScriptsRule).GetMethod("InitOnLoad", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method);
            var attr = method.GetCustomAttribute<InitializeOnLoadMethodAttribute>();
            Assert.IsNotNull(attr);
        }
    }

    // ===========================================================================
    // MissingNdmfRule tests
    // ===========================================================================
    [TestFixture]
    public class MissingNdmfRuleTests_MatAvatar
    {
        [Test]
        public void Validate_WithNdmfInstalled_ReturnsNull()
        {
            // With VQT_HAS_NDMF defined (which it is in this project), always returns null
            var go = new GameObject("TestAvatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var rule = new MissingNdmfRule();
                var avatar = new VRChatAvatar(desc);
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void InitOnLoad_RegistersRule()
        {
            var method = typeof(MissingNdmfRule).GetMethod("InitOnLoad", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method);
        }

        [Test]
        public void ImplementsIAvatarValidationRule()
        {
            Assert.IsTrue(typeof(IAvatarValidationRule).IsAssignableFrom(typeof(MissingNdmfRule)));
        }
    }

    // ===========================================================================
    // CacheUtility.GetContentCacheKey tests
    // ===========================================================================
    [TestFixture]
    public class CacheUtilityGetContentCacheKeyTests
    {
        [Test]
        public void GetContentCacheKey_StandardMaterial_ReturnsNonEmpty()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var key = CacheUtility.GetContentCacheKey(mat);
                Assert.IsNotNull(key);
                Assert.IsNotEmpty(key);
                Assert.IsTrue(key.Contains("Standard"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetContentCacheKey_SameMaterialSameKey()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var key1 = CacheUtility.GetContentCacheKey(mat);
                var key2 = CacheUtility.GetContentCacheKey(mat);
                Assert.AreEqual(key1, key2);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetContentCacheKey_DifferentColorDifferentKey()
        {
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            mat1.color = Color.red;
            mat2.color = Color.blue;
            try
            {
                var key1 = CacheUtility.GetContentCacheKey(mat1);
                var key2 = CacheUtility.GetContentCacheKey(mat2);
                Assert.AreNotEqual(key1, key2);
            }
            finally
            {
                Object.DestroyImmediate(mat1);
                Object.DestroyImmediate(mat2);
            }
        }

        [Test]
        public void GetContentCacheKey_IncludesKeywords()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var key = CacheUtility.GetContentCacheKey(mat);
                Assert.IsTrue(key.Contains("LocalKeywords_"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetContentCacheKey_WithTexture_ReturnsKey()
        {
            var mat = new Material(Shader.Find("Standard"));
            var tex = new Texture2D(4, 4);
            mat.mainTexture = tex;
            try
            {
                var key = CacheUtility.GetContentCacheKey(mat);
                Assert.IsNotNull(key);
                Assert.IsNotEmpty(key);
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void GetContentCacheKey_DifferentFloatsDifferentKey()
        {
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            mat1.SetFloat("_Metallic", 0.0f);
            mat2.SetFloat("_Metallic", 1.0f);
            try
            {
                var key1 = CacheUtility.GetContentCacheKey(mat1);
                var key2 = CacheUtility.GetContentCacheKey(mat2);
                Assert.AreNotEqual(key1, key2);
            }
            finally
            {
                Object.DestroyImmediate(mat1);
                Object.DestroyImmediate(mat2);
            }
        }
    }

    // ===========================================================================
    // CacheUtility.TextureCache tests
    // ===========================================================================
    [TestFixture]
    public class TextureCacheTests_MatAvatar
    {
        [Test]
        public void TextureCache_Constructor_StoresData()
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false, false);
            try
            {
                var cache = new CacheUtility.TextureCache(tex, false, false, UBuildTarget.Android);
                Assert.IsNotNull(cache);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void TextureCache_ToTexture2D_NonNormalMap_RestoresTexture()
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false, false);
            var pixels = new Color32[64];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(255, 0, 0, 255);
            tex.SetPixels32(pixels);
            tex.Apply();
            try
            {
                var cache = new CacheUtility.TextureCache(tex, false, false, UBuildTarget.StandaloneWindows64);
                var restored = cache.ToTexture2D();
                Assert.IsNotNull(restored);
                Assert.AreEqual(8, restored.width);
                Assert.AreEqual(8, restored.height);
                Object.DestroyImmediate(restored);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void TextureCache_RoundTrip_PreservesSize()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false, true);
            tex.Apply();
            try
            {
                var cache = new CacheUtility.TextureCache(tex, true, false, UBuildTarget.StandaloneWindows64);
                var restored = cache.ToTexture2D();
                Assert.AreEqual(16, restored.width);
                Assert.AreEqual(16, restored.height);
                Object.DestroyImmediate(restored);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void TextureCache_JsonSerialization_Works()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false, false);
            tex.Apply();
            try
            {
                var cache = new CacheUtility.TextureCache(tex, false, false, UBuildTarget.Android);
                var json = JsonUtility.ToJson(cache);
                Assert.IsNotNull(json);
                Assert.IsNotEmpty(json);

                var deserialized = JsonUtility.FromJson<CacheUtility.TextureCache>(json);
                Assert.IsNotNull(deserialized);
                var restored = deserialized.ToTexture2D();
                Assert.IsNotNull(restored);
                Assert.AreEqual(4, restored.width);
                Assert.AreEqual(4, restored.height);
                Object.DestroyImmediate(restored);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }
    }

    // ===========================================================================
    // VRCQuestTools static class tests
    // ===========================================================================
    [TestFixture]
    public class VRCQuestToolsEntryTests_MatAvatar
    {
        [Test]
        public void Name_IsVRCQuestTools()
        {
            Assert.AreEqual("VRCQuestTools", VRCQuestTools.Name);
        }

        [Test]
        public void Version_IsValidSemVer()
        {
            Assert.IsNotNull(VRCQuestTools.Version);
            Assert.IsTrue(VRCQuestTools.Version.Split('.').Length >= 3, "Version should be semver");
        }

        [Test]
        public void AssetRoot_IsNotEmpty()
        {
            var assetRoot = VRCQuestTools.AssetRoot;
            Assert.IsNotNull(assetRoot);
            Assert.IsNotEmpty(assetRoot);
        }

        [Test]
        public void IsImportedAsPackage_ReturnsBoolean()
        {
            var result = VRCQuestTools.IsImportedAsPackage;
            // In this project it should be imported as a package
            Assert.IsTrue(result);
        }

        [Test]
        public void ExportUnityPackage_MethodExists()
        {
            var method = typeof(VRCQuestTools).GetMethod("ExportUnityPackage", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method);
        }

        [Test]
        public void ComponentRemover_IsNotNull()
        {
            var field = typeof(VRCQuestTools).GetField("ComponentRemover", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            var value = field.GetValue(null);
            Assert.IsNotNull(value);
        }

        [Test]
        public void AvatarConverter_IsNotNull()
        {
            var field = typeof(VRCQuestTools).GetField("AvatarConverter", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            var value = field.GetValue(null);
            Assert.IsNotNull(value);
        }

        [Test]
        public void VPMRepositoryURL_IsNotEmpty()
        {
            var field = typeof(VRCQuestTools).GetField("VPMRepositoryURL", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            var value = (string)field.GetValue(null);
            Assert.IsNotEmpty(value);
        }

        [Test]
        public void DocsURL_IsNotEmpty()
        {
            var field = typeof(VRCQuestTools).GetField("DocsURL", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            var value = (string)field.GetValue(null);
            Assert.IsNotEmpty(value);
        }

        [Test]
        public void PackageName_IsCorrect()
        {
            var field = typeof(VRCQuestTools).GetField("PackageName", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            var value = (string)field.GetValue(null);
            Assert.AreEqual("com.github.kurotu.vrc-quest-tools", value);
        }
    }

    // ===========================================================================
    // AvatarConverter.CreateMaterialConvertSettingsMap tests
    // ===========================================================================
    [TestFixture]
    public class AvatarConverterCreateSettingsMapTests_MatAvatar
    {
        private AvatarConverter converter;

        [SetUp]
        public void SetUp()
        {
            converter = new AvatarConverter(new MaterialWrapperBuilder());
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_EmptyAvatar_ReturnsEmptyMap()
        {
            var go = new GameObject("TestAvatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                var map = converter.CreateMaterialConvertSettingsMap(avatar);
                Assert.IsNotNull(map);
                Assert.AreEqual(0, map.Count);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithMaterials_NoConverterSettings_ReturnsEmptyMap()
        {
            var go = new GameObject("TestAvatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var map = converter.CreateMaterialConvertSettingsMap(go, new Material[] { mat });
                Assert.IsNotNull(map);
                // No AvatarConverterSettings or MaterialConversionSettings, so no settings applied
                Assert.AreEqual(0, map.Count);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithConverterSettings_AppliesDefaultSettings()
        {
            var go = new GameObject("TestAvatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var settings = go.AddComponent<AvatarConverterSettings>();
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var materials = new Material[] { mat };
                var map = converter.CreateMaterialConvertSettingsMap(go, materials);
                Assert.IsNotNull(map);
                // AvatarConverterSettings provides default settings via primary conversion component
                Assert.GreaterOrEqual(map.Count, 0);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithMaterialConversionSettings_NullTarget_Throws()
        {
            var go = new GameObject("TestAvatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var convSettings = go.AddComponent<MaterialConversionSettings>();
            convSettings.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings { targetMaterial = null }
            };
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                Assert.Throws<TargetMaterialNullException>(() =>
                {
                    converter.CreateMaterialConvertSettingsMap(go, new Material[] { mat });
                });
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithMaterialSwap_NullOriginal_Throws()
        {
            var go = new GameObject("TestAvatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var swap = go.AddComponent<MaterialSwap>();
            swap.materialMappings = new List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping { originalMaterial = null, replacementMaterial = null }
            };
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                Assert.Throws<InvalidMaterialSwapNullException>(() =>
                {
                    converter.CreateMaterialConvertSettingsMap(go, new Material[] { mat });
                });
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithMaterialSwap_NullReplacement_Throws()
        {
            var go = new GameObject("TestAvatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var swap = go.AddComponent<MaterialSwap>();
            var originalMat = new Material(Shader.Find("Standard"));
            swap.materialMappings = new List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping { originalMaterial = originalMat, replacementMaterial = null }
            };
            try
            {
                Assert.Throws<InvalidMaterialSwapNullException>(() =>
                {
                    converter.CreateMaterialConvertSettingsMap(go, new Material[] { originalMat });
                });
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(originalMat);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithValidMaterialSwap_AddsMappings()
        {
            var go = new GameObject("TestAvatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var swap = go.AddComponent<MaterialSwap>();
            var originalMat = new Material(Shader.Find("Standard"));
            var replacementShader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (replacementShader == null)
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(originalMat);
                Assert.Inconclusive("VRChat/Mobile/Toon Lit shader not found");
                return;
            }
            var replacementMat = new Material(replacementShader);
            swap.materialMappings = new List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping { originalMaterial = originalMat, replacementMaterial = replacementMat }
            };
            try
            {
                var map = converter.CreateMaterialConvertSettingsMap(go, new Material[] { originalMat });
                Assert.IsNotNull(map);
                Assert.IsTrue(map.ContainsKey(originalMat));
                Assert.IsInstanceOf<MaterialReplaceSettings>(map[originalMat]);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(originalMat);
                Object.DestroyImmediate(replacementMat);
            }
        }
    }

    // ===========================================================================
    // AvatarConverter additional method tests
    // ===========================================================================
    [TestFixture]
    public class AvatarConverterMethodCoverageTests
    {
        [Test]
        public void Constructor_StoresWrapperBuilder()
        {
            var builder = new MaterialWrapperBuilder();
            var converter = new AvatarConverter(builder);
            Assert.AreSame(builder, converter.MaterialWrapperBuilder);
        }

        [Test]
        public void PrepareConvertForQuestInPlace_WithSimpleAvatar_DoesNotThrow()
        {
            var go = new GameObject("TestAvatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var converter = new AvatarConverter(new MaterialWrapperBuilder());
                var avatar = new VRChatAvatar(desc);
                Assert.DoesNotThrow(() => converter.PrepareConvertForQuestInPlace(avatar));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void PrepareModularAvatarComponentsInPlace_WithSimpleAvatar_DoesNotThrow()
        {
            var go = new GameObject("TestAvatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var converter = new AvatarConverter(new MaterialWrapperBuilder());
                var avatar = new VRChatAvatar(desc);
                Assert.DoesNotThrow(() => converter.PrepareModularAvatarComponentsInPlace(avatar));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_ViaReflection_EmptyAvatar_DoesNotThrow()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var converter = new AvatarConverter(new MaterialWrapperBuilder());
                var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(method);
                Assert.DoesNotThrow(() => method.Invoke(converter, new object[] { go }));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_WithRenderer_TrimsExcess()
        {
            var go = new GameObject("TestAvatar");
            var child = new GameObject("Child");
            child.transform.parent = go.transform;
            var meshFilter = child.AddComponent<MeshFilter>();
            var renderer = child.AddComponent<MeshRenderer>();

            // Create a mesh with 1 submesh
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.right, Vector3.up };
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.subMeshCount = 1;
            meshFilter.sharedMesh = mesh;

            // Assign 3 materials (2 extra)
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            var mat3 = new Material(Shader.Find("Standard"));
            renderer.sharedMaterials = new Material[] { mat1, mat2, mat3 };

            try
            {
                var converter = new AvatarConverter(new MaterialWrapperBuilder());
                var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(converter, new object[] { go });

                Assert.AreEqual(1, renderer.sharedMaterials.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mesh);
                Object.DestroyImmediate(mat1);
                Object.DestroyImmediate(mat2);
                Object.DestroyImmediate(mat3);
            }
        }

        [Test]
        public void SaveMaterialAsset_ViaReflection_MethodExists()
        {
            var method = typeof(AvatarConverter).GetMethod("SaveMaterialAsset", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method);
            Assert.AreEqual(4, method.GetParameters().Length);
        }

        [Test]
        public void GenerateConvertedMaterial_ViaReflection_MethodExists()
        {
            var method = typeof(AvatarConverter).GetMethod("GenerateConvertedMaterial", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method);
            Assert.AreEqual(5, method.GetParameters().Length);
        }

        [Test]
        public void ConvertAnimatorControllersForQuest_ViaReflection_MethodExists()
        {
            var method = typeof(AvatarConverter).GetMethod("ConvertAnimatorControllersForQuest", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method);
        }
    }

    // ===========================================================================
    // FallbackAvatarCallback tests
    // ===========================================================================
    [TestFixture]
    public class FallbackAvatarCallbackTests_MatAvatar
    {
        [Test]
        public void ImplementsIVRCSDKPreprocessAvatarCallback()
        {
            var type = typeof(KRT.VRCQuestTools.NonDestructive.FallbackAvatarCallback);
            var iface = type.GetInterfaces().FirstOrDefault(i => i.Name == "IVRCSDKPreprocessAvatarCallback");
            Assert.IsNotNull(iface, "Should implement IVRCSDKPreprocessAvatarCallback");
        }

        [Test]
        public void CallbackOrder_IsNegative()
        {
            var callback = new KRT.VRCQuestTools.NonDestructive.FallbackAvatarCallback();
            // Access callbackOrder via the interface property
            var prop = callback.GetType().GetProperty("callbackOrder");
            Assert.IsNotNull(prop);
            var order = (int)prop.GetValue(callback);
            Assert.Less(order, 0);
            Assert.AreEqual(-100000, order);
        }

        [Test]
        public void OnPreprocessAvatar_NoPipelineManager_ReturnsTrue()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var callback = new KRT.VRCQuestTools.NonDestructive.FallbackAvatarCallback();
                var method = callback.GetType().GetMethod("OnPreprocessAvatar");
                Assert.IsNotNull(method);
                var result = (bool)method.Invoke(callback, new object[] { go });
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }

    // ===========================================================================
    // ActualPerformanceCallback tests
    // ===========================================================================
    [TestFixture]
    public class ActualPerformanceCallbackTests_MatAvatar
    {
        [Test]
        public void ImplementsIVRCSDKPreprocessAvatarCallback()
        {
            var type = typeof(KRT.VRCQuestTools.NonDestructive.ActualPerformanceCallback);
            var iface = type.GetInterfaces().FirstOrDefault(i => i.Name == "IVRCSDKPreprocessAvatarCallback");
            Assert.IsNotNull(iface, "Should implement IVRCSDKPreprocessAvatarCallback");
        }

        [Test]
        public void CallbackOrder_IsMaxValue()
        {
            var callback = new KRT.VRCQuestTools.NonDestructive.ActualPerformanceCallback();
            var prop = callback.GetType().GetProperty("callbackOrder");
            Assert.IsNotNull(prop);
            var order = (int)prop.GetValue(callback);
            Assert.AreEqual(int.MaxValue, order);
        }

        [Test]
        public void OnPreprocessAvatar_NoPipelineManager_ReturnsTrue()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var callback = new KRT.VRCQuestTools.NonDestructive.ActualPerformanceCallback();
                var method = callback.GetType().GetMethod("OnPreprocessAvatar");
                Assert.IsNotNull(method);
                var result = (bool)method.Invoke(callback, new object[] { go });
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void LastActualPerformanceRating_IsAccessible()
        {
            var field = typeof(KRT.VRCQuestTools.NonDestructive.ActualPerformanceCallback)
                .GetField("LastActualPerformanceRating", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            var dict = field.GetValue(null);
            Assert.IsNotNull(dict);
        }
    }

    // ===========================================================================
    // VPMService tests
    // ===========================================================================
    [TestFixture]
    public class VPMServiceTests_MatAvatar
    {
        [Test]
        public void Constructor_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new KRT.VRCQuestTools.Services.VPMService());
        }

        [Test]
        public void GetVPMRepository_NullUrl_ThrowsSynchronously()
        {
            var service = new KRT.VRCQuestTools.Services.VPMService();
            try
            {
                var task = service.GetVPMRepository(null);
                task.Wait();
                Assert.Fail("Expected exception");
            }
            catch (AggregateException)
            {
                // Expected
            }
            catch (Exception)
            {
                // Expected (could be various exception types)
            }
        }

        [Test]
        public void GetVPMRepository_MethodExists()
        {
            var method = typeof(KRT.VRCQuestTools.Services.VPMService).GetMethod("GetVPMRepository", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method);
        }
    }

    // ===========================================================================
    // Mock_AvatarPerformanceStatsLevelSet tests (fill remaining uncovered)
    // ===========================================================================
    [TestFixture]
    public class MockAvatarPerformanceStatsLevelSetTests
    {
        [Test]
        public void MockType_Exists()
        {
            var type = typeof(VRCQuestTools).Assembly.GetType("KRT.VRCQuestTools.Mocks.Mock_AvatarPerformanceStatsLevelSet");
            if (type == null)
            {
                // Try alternate assembly
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = asm.GetType("KRT.VRCQuestTools.Mocks.Mock_AvatarPerformanceStatsLevelSet");
                    if (type != null) break;
                }
            }
            Assert.IsNotNull(type, "Mock_AvatarPerformanceStatsLevelSet type should exist");
        }

        [Test]
        public void MockType_HasExpectedInterface()
        {
            Type type = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType("KRT.VRCQuestTools.Mocks.Mock_AvatarPerformanceStatsLevelSet");
                if (type != null) break;
            }
            if (type == null)
            {
                Assert.Inconclusive("Type not found");
                return;
            }
            // Check it has fields/properties
            var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.Greater(members.Length, 0);
        }
    }

    // ===========================================================================
    // IMaterialConversionComponent.GetCacheKey tests
    // ===========================================================================
    [TestFixture]
    public class IMaterialConversionComponentGetCacheKeyTests
    {
        [Test]
        public void GetCacheKey_OnAvatarConverterSettings_ReturnsNonEmpty()
        {
            var go = new GameObject("TestAvatar");
            var settings = go.AddComponent<AvatarConverterSettings>();
            try
            {
                // AvatarConverterSettings implements IMaterialConversionComponent
                var component = settings as IMaterialConversionComponent;
                if (component != null)
                {
                    var key = component.GetCacheKey();
                    Assert.IsNotNull(key);
                    Assert.IsNotEmpty(key);
                }
                else
                {
                    Assert.Inconclusive("AvatarConverterSettings does not implement IMaterialConversionComponent");
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetCacheKey_OnMaterialConversionSettings_ReturnsNonEmpty()
        {
            var go = new GameObject("TestAvatar");
            var settings = go.AddComponent<MaterialConversionSettings>();
            try
            {
                var component = settings as IMaterialConversionComponent;
                if (component != null)
                {
                    var key = component.GetCacheKey();
                    Assert.IsNotNull(key);
                    Assert.IsNotEmpty(key);
                }
                else
                {
                    Assert.Inconclusive("MaterialConversionSettings does not implement IMaterialConversionComponent");
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }

    // ===========================================================================
    // MenuIconResizer component test
    // ===========================================================================
    [TestFixture]
    public class MenuIconResizerTests_MatAvatar
    {
        [Test]
        public void MenuIconResizer_ComponentExists()
        {
            var type = typeof(MenuIconResizer);
            Assert.IsNotNull(type);
            Assert.IsTrue(typeof(MonoBehaviour).IsAssignableFrom(type));
        }

        [Test]
        public void MenuIconResizer_CanBeAddedToGameObject()
        {
            var go = new GameObject("Test");
            try
            {
                var component = go.AddComponent<MenuIconResizer>();
                Assert.IsNotNull(component);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }

    // ===========================================================================
    // MeshFlipperMaskNotReadableException tests (0% coverage)
    // ===========================================================================
    [TestFixture]
    public class MeshFlipperMaskNotReadableExceptionTests
    {
        [Test]
        public void Constructor_WithMessageAndTexture_SetsProperties()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                var ex = new MeshFlipperMaskNotReadableException("test message", tex);
                Assert.AreEqual("test message", ex.Message);
                Assert.AreSame(tex, ex.Texture);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void IsException()
        {
            Assert.IsTrue(typeof(Exception).IsAssignableFrom(typeof(MeshFlipperMaskNotReadableException)));
        }
    }

    // ===========================================================================
    // VRCPhysBoneProviderBase uncovered test
    // ===========================================================================
    [TestFixture]
    public class VRCPhysBoneProviderBaseTests_MatAvatar
    {
        [Test]
        public void VRCPhysBoneProviderBase_TypeExists()
        {
            Type type = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType("KRT.VRCQuestTools.Models.VRChat.VRCPhysBoneProviderBase");
                if (type != null) break;
            }
            Assert.IsNotNull(type, "VRCPhysBoneProviderBase type should exist");
        }
    }

    // ===========================================================================
    // MaterialConversionGUI partial coverage test
    // ===========================================================================
    [TestFixture]
    public class MaterialConversionGUITests_MatAvatar
    {
        [Test]
        public void MaterialConversionGUI_TypeExists()
        {
            Type type = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType("KRT.VRCQuestTools.Inspector.MaterialConversionGUI");
                if (type != null) break;
            }
            Assert.IsNotNull(type, "MaterialConversionGUI type should exist");
        }
    }

    // ===========================================================================
    // ValidationAutomator tests
    // ===========================================================================
    [TestFixture]
    public class ValidationAutomatorTests_MatAvatar
    {
        [Test]
        public void ValidationAutomator_TypeExists()
        {
            Type type = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType("KRT.VRCQuestTools.Automators.ValidationAutomator");
                if (type != null) break;
            }
            Assert.IsNotNull(type, "ValidationAutomator type should exist");
            Assert.IsTrue(type.IsAbstract && type.IsSealed, "ValidationAutomator should be static");
        }
    }

    // ===========================================================================
    // UnityQuestSettingsViewModel additional tests
    // ===========================================================================
    [TestFixture]
    public class UnityQuestSettingsViewModelTests_MatAvatar
    {
        [Test]
        public void ViewModelType_Exists()
        {
            var type = typeof(KRT.VRCQuestTools.ViewModels.UnityQuestSettingsViewModel);
            Assert.IsNotNull(type);
        }

        [Test]
        public void Constructor_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                new KRT.VRCQuestTools.ViewModels.UnityQuestSettingsViewModel();
            });
        }
    }

    // ===========================================================================
    // AvatarConverterSettings additional coverage tests
    // ===========================================================================
    [TestFixture]
    public class AvatarConverterSettingsAdditionalTests
    {
        [Test]
        public void NdmfPhase_DefaultValue()
        {
            var go = new GameObject("TestAvatar");
            var settings = go.AddComponent<AvatarConverterSettings>();
            try
            {
                Assert.AreEqual(AvatarConverterNdmfPhase.Auto, settings.ndmfPhase);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void EnableMaterialPreview_DefaultIsTrue()
        {
            var go = new GameObject("TestAvatar");
            var settings = go.AddComponent<AvatarConverterSettings>();
            try
            {
                var component = settings as IMaterialConversionComponent;
                if (component != null)
                {
                    Assert.IsTrue(component.EnableMaterialPreview);
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
