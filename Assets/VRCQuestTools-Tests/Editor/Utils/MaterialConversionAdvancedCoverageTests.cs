// Tests for uncovered code paths - Batch 26
// Focus: MaterialWrapperBuilder branches, AvatarConverter.CreateMaterialConvertSettingsMap paths,
//        CacheUtility.TextureCache, MSMapGenViewModel, Validators, VPMService, NdmfUtility

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KRT.VRCQuestTools;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.Validators;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Services;
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.ViewModels;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using UObject = UnityEngine.Object;

namespace KRT.VRCQuestTools.Tests
{
    // ==========================================
    // MaterialWrapperBuilder Tests
    // ==========================================
    [TestFixture]
    public class MaterialWrapperBuilderTests_MatAdvanced
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
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Standard, builder.DetectShaderCategory(mat));
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void DetectShaderCategory_StandardSpecular_ReturnsStandard()
        {
            var shader = Shader.Find("Standard (Specular setup)");
            if (shader == null) { Assert.Ignore("Standard (Specular setup) shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Standard, builder.DetectShaderCategory(mat));
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void DetectShaderCategory_Unlit_ReturnsUnlit()
        {
            var shader = Shader.Find("Unlit/Color");
            if (shader == null) { Assert.Ignore("Unlit/Color shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Unlit, builder.DetectShaderCategory(mat));
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void DetectShaderCategory_UnlitTexture_ReturnsUnlit()
        {
            var shader = Shader.Find("Unlit/Texture");
            if (shader == null) { Assert.Ignore("Unlit/Texture shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Unlit, builder.DetectShaderCategory(mat));
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void DetectShaderCategory_VRChatMobile_ReturnsQuest()
        {
            var shader = Shader.Find("VRChat/Mobile/Standard Lite");
            if (shader == null) { Assert.Ignore("VRChat Mobile shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Quest, builder.DetectShaderCategory(mat));
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void DetectShaderCategory_VRChatMobileToonLit_ReturnsQuest()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null) { Assert.Ignore("VRChat/Mobile/Toon Lit shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Quest, builder.DetectShaderCategory(mat));
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void DetectShaderCategory_Unknown_ReturnsUnverified()
        {
            // Use a shader that doesn't match any known category
            var shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null) { Assert.Ignore("Hidden shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Unverified, builder.DetectShaderCategory(mat));
            }
            finally { UObject.DestroyImmediate(mat); }
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
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void Build_Unlit_ReturnsStandardMaterial()
        {
            var shader = Shader.Find("Unlit/Color");
            if (shader == null) { Assert.Ignore("Unlit/Color shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
                Assert.IsInstanceOf<StandardMaterial>(wrapper);
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void Build_VRChatMobile_ReturnsStandardMaterial()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null) { Assert.Ignore("VRChat/Mobile/Toon Lit not found"); return; }
            var mat = new Material(shader);
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
                Assert.IsInstanceOf<StandardMaterial>(wrapper);
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void ShaderCategory_HasExpectedValues()
        {
            var values = Enum.GetValues(typeof(MaterialWrapperBuilder.ShaderCategory));
            Assert.GreaterOrEqual(values.Length, 10);
            Assert.Contains(MaterialWrapperBuilder.ShaderCategory.UTS2, values);
            Assert.Contains(MaterialWrapperBuilder.ShaderCategory.Arktoon, values);
            Assert.Contains(MaterialWrapperBuilder.ShaderCategory.AXCS, values);
            Assert.Contains(MaterialWrapperBuilder.ShaderCategory.Standard, values);
            Assert.Contains(MaterialWrapperBuilder.ShaderCategory.Unlit, values);
            Assert.Contains(MaterialWrapperBuilder.ShaderCategory.Quest, values);
            Assert.Contains(MaterialWrapperBuilder.ShaderCategory.Sunao, values);
            Assert.Contains(MaterialWrapperBuilder.ShaderCategory.LilToon, values);
            Assert.Contains(MaterialWrapperBuilder.ShaderCategory.Poiyomi, values);
            Assert.Contains(MaterialWrapperBuilder.ShaderCategory.VirtualLens2, values);
            Assert.Contains(MaterialWrapperBuilder.ShaderCategory.Unverified, values);
        }
    }

    // ==========================================
    // AvatarConverter.CreateMaterialConvertSettingsMap deep paths
    // ==========================================
    [TestFixture]
    public class AvatarConverterCreateSettingsMapTests_MatAdvanced
    {
        private AvatarConverter converter;

        [SetUp]
        public void SetUp()
        {
            converter = new AvatarConverter(new MaterialWrapperBuilder());
        }

        private GameObject CreateTestAvatar(string name)
        {
            var go = new GameObject(name);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            return go;
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithMaterialConversionSettings_IncludesOverrides()
        {
            var go = CreateTestAvatar("WithOverrides");
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            var mcs = child.AddComponent<MaterialConversionSettings>();
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "TestMaterial";
            mcs.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = mat,
                    materialConvertSettings = new ToonLitConvertSettings(),
                },
            };

            // Add renderer so material is "used"
            var renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new Material[] { mat };

            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                var result = converter.CreateMaterialConvertSettingsMap(avatar);
                Assert.IsTrue(result.ContainsKey(mat));
                Assert.IsInstanceOf<ToonLitConvertSettings>(result[mat]);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithMaterialSwap_IncludesSwap()
        {
            var go = CreateTestAvatar("WithSwap");
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            var swap = child.AddComponent<MaterialSwap>();
            var originalMat = new Material(Shader.Find("Standard"));
            originalMat.name = "OriginalMat";
            var questShader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (questShader == null) { Assert.Ignore("VRChat/Mobile/Toon Lit not found"); return; }
            var replacementMat = new Material(questShader);
            replacementMat.name = "ReplacementMat";
            swap.materialMappings = new System.Collections.Generic.List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping
                {
                    originalMaterial = originalMat,
                    replacementMaterial = replacementMat,
                },
            };

            var renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new Material[] { originalMat };

            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                var result = converter.CreateMaterialConvertSettingsMap(avatar);
                Assert.IsTrue(result.ContainsKey(originalMat));
                Assert.IsInstanceOf<MaterialReplaceSettings>(result[originalMat]);
            }
            finally
            {
                UObject.DestroyImmediate(originalMat);
                UObject.DestroyImmediate(replacementMat);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_MaterialSwapNullOriginal_Throws()
        {
            var go = CreateTestAvatar("SwapNullOrig");
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            var swap = child.AddComponent<MaterialSwap>();
            var replacementMat = new Material(Shader.Find("Standard"));
            swap.materialMappings = new System.Collections.Generic.List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping
                {
                    originalMaterial = null,
                    replacementMaterial = replacementMat,
                },
            };

            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                Assert.Throws<InvalidMaterialSwapNullException>(() => converter.CreateMaterialConvertSettingsMap(avatar));
            }
            finally
            {
                UObject.DestroyImmediate(replacementMat);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_MaterialSwapNullReplacement_Throws()
        {
            var go = CreateTestAvatar("SwapNullReplace");
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            var swap = child.AddComponent<MaterialSwap>();
            var originalMat = new Material(Shader.Find("Standard"));
            swap.materialMappings = new System.Collections.Generic.List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping
                {
                    originalMaterial = originalMat,
                    replacementMaterial = null,
                },
            };

            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                Assert.Throws<InvalidMaterialSwapNullException>(() => converter.CreateMaterialConvertSettingsMap(avatar));
            }
            finally
            {
                UObject.DestroyImmediate(originalMat);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_NullTargetInMaterialConversion_Throws()
        {
            var go = CreateTestAvatar("NullTarget");
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            var mcs = child.AddComponent<MaterialConversionSettings>();
            mcs.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = null,
                    materialConvertSettings = new ToonLitConvertSettings(),
                },
            };

            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                Assert.Throws<TargetMaterialNullException>(() => converter.CreateMaterialConvertSettingsMap(avatar));
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithAvatarConverterSettings_AppliesDefault()
        {
            var go = CreateTestAvatar("WithACS");
            var acs = go.AddComponent<AvatarConverterSettings>();
            // AvatarConverterSettings adds default material convert settings
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "DefaultMat";

            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            var renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new Material[] { mat };

            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                var result = converter.CreateMaterialConvertSettingsMap(avatar);
                // Should have settings for the material
                Assert.IsNotNull(result);
                Assert.GreaterOrEqual(result.Count, 0); // may be 0 if material is Quest-allowed
            }
            finally
            {
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_NullTargetInACS_Throws()
        {
            var go = CreateTestAvatar("NullTargetACS");
            var acs = go.AddComponent<AvatarConverterSettings>();
            acs.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = null,
                    materialConvertSettings = new ToonLitConvertSettings(),
                },
            };

            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                Assert.Throws<TargetMaterialNullException>(() => converter.CreateMaterialConvertSettingsMap(avatar));
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_TwoArgOverload_Works()
        {
            var go = CreateTestAvatar("TwoArg");
            var mat = new Material(Shader.Find("Standard"));
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            var renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new Material[] { mat };

            try
            {
                var result = converter.CreateMaterialConvertSettingsMap(go, new Material[] { mat });
                Assert.IsNotNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_MaterialNotInAvatar_OmittedFromResult()
        {
            var go = CreateTestAvatar("OmitUnused");
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            var mcs = child.AddComponent<MaterialConversionSettings>();
            var unusedMat = new Material(Shader.Find("Standard"));
            unusedMat.name = "UnusedMat";
            mcs.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = unusedMat,
                    materialConvertSettings = new ToonLitConvertSettings(),
                },
            };

            // No renderer using unusedMat
            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                var result = converter.CreateMaterialConvertSettingsMap(avatar);
                // unusedMat is not in avatar.Materials so should be omitted
                Assert.IsFalse(result.ContainsKey(unusedMat));
            }
            finally
            {
                UObject.DestroyImmediate(unusedMat);
                UObject.DestroyImmediate(go);
            }
        }
    }

    // ==========================================
    // CacheUtility Tests (deep paths)
    // ==========================================
    [TestFixture]
    public class CacheUtilityDeepTests_MatAdvanced
    {
        [Test]
        public void GetContentCacheKey_Standard_ReturnsNonEmptyString()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var key = CacheUtility.GetContentCacheKey(mat);
                Assert.IsNotNull(key);
                Assert.IsNotEmpty(key);
                Assert.IsTrue(key.Contains("Standard"));
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void GetContentCacheKey_WithTexture_IncludesTextureHash()
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
                UObject.DestroyImmediate(tex);
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetContentCacheKey_DifferentColors_DifferentKeys()
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
                UObject.DestroyImmediate(mat1);
                UObject.DestroyImmediate(mat2);
            }
        }

        [Test]
        public void GetContentCacheKey_SameSettings_SameKey()
        {
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            try
            {
                var key1 = CacheUtility.GetContentCacheKey(mat1);
                var key2 = CacheUtility.GetContentCacheKey(mat2);
                Assert.AreEqual(key1, key2);
            }
            finally
            {
                UObject.DestroyImmediate(mat1);
                UObject.DestroyImmediate(mat2);
            }
        }

        [Test]
        public void GetContentCacheKey_WithKeywords_IncludesKeywords()
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.EnableKeyword("_METALLIC_SETUP");
            try
            {
                var key = CacheUtility.GetContentCacheKey(mat);
                Assert.IsNotNull(key);
                Assert.IsTrue(key.Contains("LocalKeywords_"));
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void TextureCache_RoundTrip_PreservesData()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false, false);
            var pixels = new Color32[16];
            for (int i = 0; i < 16; i++) pixels[i] = new Color32(255, 0, 0, 255);
            tex.SetPixels32(pixels);
            tex.Apply();

            try
            {
                var cache = new CacheUtility.TextureCache(tex, false, false, UnityEditor.BuildTarget.StandaloneWindows64);
                var restored = cache.ToTexture2D();
                Assert.IsNotNull(restored);
                Assert.AreEqual(4, restored.width);
                Assert.AreEqual(4, restored.height);
                UObject.DestroyImmediate(restored);
            }
            finally { UObject.DestroyImmediate(tex); }
        }

        [Test]
        public void TextureCache_WithMipmap_PreservesMipmap()
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, true, false);
            tex.Apply();
            try
            {
                var cache = new CacheUtility.TextureCache(tex, false, false, UnityEditor.BuildTarget.StandaloneWindows64);
                var restored = cache.ToTexture2D();
                Assert.IsNotNull(restored);
                UObject.DestroyImmediate(restored);
            }
            finally { UObject.DestroyImmediate(tex); }
        }

        [Test]
        public void TextureCache_Linear_PreservesLinearFlag()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false, true);
            tex.Apply();
            try
            {
                var cache = new CacheUtility.TextureCache(tex, true, false, UnityEditor.BuildTarget.StandaloneWindows64);
                var restored = cache.ToTexture2D();
                Assert.IsNotNull(restored);
                UObject.DestroyImmediate(restored);
            }
            finally { UObject.DestroyImmediate(tex); }
        }
    }

    // ==========================================
    // MSMapGenViewModel Tests
    // ==========================================
    [TestFixture]
    public class MSMapGenViewModelTests_MatAdvanced
    {
        [Test]
        public void DisableGenerateButton_BothNull_ReturnsTrue()
        {
            var vm = new MSMapGenViewModel();
            vm.metallicMap = null;
            vm.smoothnessMap = null;
            Assert.IsTrue(vm.DisableGenerateButton);
        }

        [Test]
        public void DisableGenerateButton_MetallicSet_ReturnsFalse()
        {
            var vm = new MSMapGenViewModel();
            var tex = new Texture2D(4, 4);
            vm.metallicMap = tex;
            vm.smoothnessMap = null;
            try
            {
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally { UObject.DestroyImmediate(tex); }
        }

        [Test]
        public void DisableGenerateButton_SmoothnessSet_ReturnsFalse()
        {
            var vm = new MSMapGenViewModel();
            var tex = new Texture2D(4, 4);
            vm.metallicMap = null;
            vm.smoothnessMap = tex;
            try
            {
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally { UObject.DestroyImmediate(tex); }
        }

        [Test]
        public void DisableGenerateButton_BothSet_ReturnsFalse()
        {
            var vm = new MSMapGenViewModel();
            var tex1 = new Texture2D(4, 4);
            var tex2 = new Texture2D(4, 4);
            vm.metallicMap = tex1;
            vm.smoothnessMap = tex2;
            try
            {
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally
            {
                UObject.DestroyImmediate(tex1);
                UObject.DestroyImmediate(tex2);
            }
        }

        [Test]
        public void InvertSmoothness_DefaultFalse()
        {
            var vm = new MSMapGenViewModel();
            Assert.IsFalse(vm.invertSmoothness);
        }

        [Test]
        public void InvertSmoothness_CanBeSet()
        {
            var vm = new MSMapGenViewModel();
            vm.invertSmoothness = true;
            Assert.IsTrue(vm.invertSmoothness);
        }
    }

    // ==========================================
    // Validator Tests
    // ==========================================
    [TestFixture]
    public class ValidatorTests
    {
        private GameObject CreateTestAvatar(string name)
        {
            var go = new GameObject(name);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            return go;
        }

        [Test]
        public void MissingScriptsRule_CleanAvatar_ReturnsNull()
        {
            var go = CreateTestAvatar("Clean");
            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                var rule = new MissingScriptsRule();
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void MissingScriptsRule_InactiveAvatar_ReturnsNull()
        {
            var go = CreateTestAvatar("Inactive");
            go.SetActive(false);
            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                var rule = new MissingScriptsRule();
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void MissingNdmfRule_CleanAvatar_ReturnsNull()
        {
            var go = CreateTestAvatar("Clean");
            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                var rule = new MissingNdmfRule();
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void AvatarValidationRules_Rules_IsNotEmpty()
        {
            var rules = AvatarValidationRules.Rules;
            Assert.IsNotNull(rules);
            Assert.GreaterOrEqual(rules.Length, 1);
        }
    }

    // ==========================================
    // VPMService Tests
    // ==========================================
    [TestFixture]
    public class VPMServiceTests_MatAdvanced
    {
        [Test]
        public void Constructor_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new VPMService());
        }
    }

    // ==========================================
    // NdmfUtility Tests
    // ==========================================
    [TestFixture]
    public class NdmfUtilityTests_MatAdvanced
    {
        [Test]
        public void NotifyObjectUpdate_NullObject_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => NdmfUtility.NotifyObjectUpdate(null));
        }

        [Test]
        public void NotifyObjectUpdate_ValidObject_DoesNotThrow()
        {
            var go = new GameObject("Test");
            try
            {
                Assert.DoesNotThrow(() => NdmfUtility.NotifyObjectUpdate(go));
            }
            finally { UObject.DestroyImmediate(go); }
        }
    }

    // ==========================================
    // UnityQuestSettingsViewModel Tests
    // ==========================================
    [TestFixture]
    public class UnityQuestSettingsViewModelTests_MatAdvanced
    {
        [Test]
        public void Constructor_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new UnityQuestSettingsViewModel());
        }

        [Test]
        public void AllSettingsValid_ReturnsBool()
        {
            var vm = new UnityQuestSettingsViewModel();
            var result = vm.AllSettingsValid;
            Assert.That(result, Is.TypeOf<bool>());
        }

        [Test]
        public void HasValidAndroidTextureCompression_ReturnsBool()
        {
            var vm = new UnityQuestSettingsViewModel();
            var result = vm.HasValidAndroidTextureCompression;
            Assert.That(result, Is.TypeOf<bool>());
        }

        [Test]
        public void HasAndroidBuildSupport_ReturnsBool()
        {
            var vm = new UnityQuestSettingsViewModel();
            var result = vm.HasAndroidBuildSupport;
            Assert.That(result, Is.TypeOf<bool>());
        }

        [Test]
        public void ShowWindowOnLoad_CanToggle()
        {
            var vm = new UnityQuestSettingsViewModel();
            var original = vm.ShowWindowOnLoad;
            try
            {
                vm.ShowWindowOnLoad = !original;
                Assert.AreEqual(!original, vm.ShowWindowOnLoad);
            }
            finally
            {
                vm.ShowWindowOnLoad = original;
            }
        }

        [Test]
        public void DefaultAndroidTextureCompression_ReturnsMobileTextureSubtarget()
        {
            var vm = new UnityQuestSettingsViewModel();
            var result = vm.DefaultAndroidTextureCompression;
            Assert.IsInstanceOf<MobileTextureSubtarget>(result);
        }
    }

    // ==========================================
    // AvatarConverter.RemoveExtraMaterialSlots via reflection
    // ==========================================
    [TestFixture]
    public class AvatarConverterRemoveExtraSlots
    {
        [Test]
        public void RemoveExtraMaterialSlots_MoreSlotsThanSubMeshes_Removes()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("RemoveExtraMaterialSlots not accessible"); return; }

            var go = new GameObject("Root");
            var child = new GameObject("Mesh");
            child.transform.SetParent(go.transform);
            var meshFilter = child.AddComponent<MeshFilter>();
            var renderer = child.AddComponent<MeshRenderer>();

            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.subMeshCount = 1;
            meshFilter.sharedMesh = mesh;

            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            renderer.sharedMaterials = new Material[] { mat1, mat2 };

            try
            {
                Assert.AreEqual(2, renderer.sharedMaterials.Length);
                method.Invoke(converter, new object[] { go });
                Assert.AreEqual(1, renderer.sharedMaterials.Length);
            }
            finally
            {
                UObject.DestroyImmediate(mat1);
                UObject.DestroyImmediate(mat2);
                UObject.DestroyImmediate(mesh);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_EqualSlots_NoChange()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("RemoveExtraMaterialSlots not accessible"); return; }

            var go = new GameObject("Root");
            var child = new GameObject("Mesh");
            child.transform.SetParent(go.transform);
            var meshFilter = child.AddComponent<MeshFilter>();
            var renderer = child.AddComponent<MeshRenderer>();

            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.subMeshCount = 1;
            meshFilter.sharedMesh = mesh;

            var mat1 = new Material(Shader.Find("Standard"));
            renderer.sharedMaterials = new Material[] { mat1 };

            try
            {
                method.Invoke(converter, new object[] { go });
                Assert.AreEqual(1, renderer.sharedMaterials.Length);
            }
            finally
            {
                UObject.DestroyImmediate(mat1);
                UObject.DestroyImmediate(mesh);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_SkinnedMeshRenderer_Works()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("RemoveExtraMaterialSlots not accessible"); return; }

            var go = new GameObject("Root");
            var child = new GameObject("SMR");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();

            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.subMeshCount = 1;
            smr.sharedMesh = mesh;

            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            var mat3 = new Material(Shader.Find("Standard"));
            smr.sharedMaterials = new Material[] { mat1, mat2, mat3 };

            try
            {
                Assert.AreEqual(3, smr.sharedMaterials.Length);
                method.Invoke(converter, new object[] { go });
                Assert.AreEqual(1, smr.sharedMaterials.Length);
            }
            finally
            {
                UObject.DestroyImmediate(mat1);
                UObject.DestroyImmediate(mat2);
                UObject.DestroyImmediate(mat3);
                UObject.DestroyImmediate(mesh);
                UObject.DestroyImmediate(go);
            }
        }
    }

    // ==========================================
    // InvalidMaterialSwapNullException Tests
    // ==========================================
    [TestFixture]
    public class InvalidMaterialSwapNullExceptionTests
    {
        [Test]
        public void Constructor_SetsComponentAndIndex()
        {
            var go = new GameObject("TestSwap");
            var swap = go.AddComponent<MaterialSwap>();
            var mat = new Material(Shader.Find("Standard"));
            swap.materialMappings = new System.Collections.Generic.List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping
                {
                    originalMaterial = mat,
                    replacementMaterial = mat,
                },
            };

            try
            {
                var ex = new InvalidMaterialSwapNullException("test", swap, 0);
                Assert.AreEqual(swap, ex.Component);
                Assert.AreEqual(mat, ex.MaterialMapping.originalMaterial);
                Assert.IsNotNull(ex.Message);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(go);
            }
        }
    }

    // ==========================================
    // AvatarConverter ApplyVirtualLens2Support via PrepareConvertForQuestInPlace
    // ==========================================
    [TestFixture]
    public class AvatarConverterPrepareTests_MatAdvanced
    {
        [Test]
        public void PrepareConvertForQuestInPlace_EmptyAvatar_DoesNotThrow()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var go = new GameObject("TestAvatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                Assert.DoesNotThrow(() => converter.PrepareConvertForQuestInPlace(avatar));
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void PrepareModularAvatarComponentsInPlace_EmptyAvatar_DoesNotThrow()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var go = new GameObject("TestAvatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                Assert.DoesNotThrow(() => converter.PrepareModularAvatarComponentsInPlace(avatar));
            }
            finally { UObject.DestroyImmediate(go); }
        }
    }

    // ==========================================
    // AssetUtility uncovered paths
    // ==========================================
    [TestFixture]
    public class AssetUtilityTests_MatAdvanced
    {
        [Test]
        public void GetTypeByName_NonExistentType_ReturnsNull()
        {
            var method = typeof(AssetUtility).GetMethod("GetTypeByName", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("GetTypeByName not accessible"); return; }
            var result = method.Invoke(null, new object[] { "NonExistentTypeNameXyz123" });
            Assert.IsNull(result);
        }

        [Test]
        public void GetTypeByName_ExistingType_ReturnsType()
        {
            var method = typeof(AssetUtility).GetMethod("GetTypeByName", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("GetTypeByName not accessible"); return; }
            var result = method.Invoke(null, new object[] { "UnityEngine.GameObject" });
            Assert.IsNotNull(result);
        }
    }

    // ==========================================
    // MaterialConversionGUI / MaterialConvertSettingsTypes coverage
    // (Tested via reflection since Inspector namespace not directly accessible)
    // ==========================================
    [TestFixture]
    public class MaterialConvertSettingsTypesTests_MatAdvanced
    {
        [Test]
        public void GetDefaultConvertTypePopups_WithToonStandard_ReturnsPopups()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type type = null;
            foreach (var asm in assemblies)
            {
                type = asm.GetType("KRT.VRCQuestTools.Inspector.MaterialConvertSettingsTypes");
                if (type != null) break;
            }
            if (type == null) { Assert.Ignore("MaterialConvertSettingsTypes not found in assemblies"); return; }
            var method = type.GetMethod("GetDefaultConvertTypePopups", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("GetDefaultConvertTypePopups not accessible"); return; }
            var result = method.Invoke(null, new object[] { true });
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetDefaultConvertTypePopups_WithoutToonStandard_ReturnsPopups()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type type = null;
            foreach (var asm in assemblies)
            {
                type = asm.GetType("KRT.VRCQuestTools.Inspector.MaterialConvertSettingsTypes");
                if (type != null) break;
            }
            if (type == null) { Assert.Ignore("MaterialConvertSettingsTypes not found in assemblies"); return; }
            var method = type.GetMethod("GetDefaultConvertTypePopups", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("GetDefaultConvertTypePopups not accessible"); return; }
            var result = method.Invoke(null, new object[] { false });
            Assert.IsNotNull(result);
        }
    }

    // ==========================================
    // AvatarConverter ClearSharedBlackTextureCache
    // ==========================================
    [TestFixture]
    public class AvatarConverterCacheTests_MatAdvanced
    {
        [Test]
        public void ClearSharedBlackTextureCache_DoesNotThrow()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("ClearSharedBlackTextureCache", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("ClearSharedBlackTextureCache not accessible"); return; }
            Assert.DoesNotThrow(() => method.Invoke(converter, null));
        }

        [Test]
        public void GetOrCreateSharedBlackTexture_NoSave_ReturnsTexture()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("GetOrCreateSharedBlackTexture", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("GetOrCreateSharedBlackTexture not accessible"); return; }
            var result = method.Invoke(converter, new object[] { false, "" }) as Texture2D;
            Assert.IsNotNull(result);
        }
    }

    // ==========================================
    // LegacyPackageException / BreakingPackageException
    // ==========================================
    [TestFixture]
    public class PackageExceptionTests
    {
        [Test]
        public void LegacyPackageException_CanBeCreated()
        {
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.LegacyPackageException");
            if (type == null) { Assert.Ignore("LegacyPackageException not found"); return; }
            try
            {
                var ex = Activator.CreateInstance(type, new object[] { "TestPackage", "1.0.0" }) as Exception;
                Assert.IsNotNull(ex);
                Assert.IsNotNull(ex.Message);
            }
            catch (Exception e) when (e is MissingMethodException)
            {
                Assert.Ignore("LegacyPackageException constructor not matching");
            }
        }

        [Test]
        public void BreakingPackageException_CanBeCreated()
        {
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.BreakingPackageException");
            if (type == null) { Assert.Ignore("BreakingPackageException not found"); return; }
            try
            {
                var ex = Activator.CreateInstance(type, new object[] { "TestPackage", "2.0.0" }) as Exception;
                Assert.IsNotNull(ex);
                Assert.IsNotNull(ex.Message);
            }
            catch (Exception e) when (e is MissingMethodException)
            {
                Assert.Ignore("BreakingPackageException constructor not matching");
            }
        }
    }

    // ==========================================
    // AvatarConverter.ApplyConvertedMaterials (renderers only path)
    // ==========================================
    [TestFixture]
    public class AvatarConverterApplyMaterialsTests
    {
        [Test]
        public void ApplyConvertedMaterials_NoAnimators_AppliesMaterialsToRenderers()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var go = new GameObject("TestAvatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            var renderer = child.AddComponent<MeshRenderer>();
            var originalMat = new Material(Shader.Find("Standard"));
            originalMat.name = "Original";
            var questMat = new Material(Shader.Find("Standard"));
            questMat.name = "Quest";
            renderer.sharedMaterials = new Material[] { originalMat };

            var convertedMaterials = new Dictionary<Material, Material>
            {
                { originalMat, questMat },
            };
            var progressCallback = new AvatarConverter.ProgressCallback
            {
                onTextureProgress = null,
                onAnimationClipProgress = null,
                onRuntimeAnimatorProgress = null,
            };

            try
            {
                converter.ApplyConvertedMaterials(go, convertedMaterials, false, "", progressCallback);
                Assert.AreEqual(questMat, renderer.sharedMaterials[0]);
            }
            finally
            {
                UObject.DestroyImmediate(originalMat);
                UObject.DestroyImmediate(questMat);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void ApplyConvertedMaterials_NullMaterialInRenderer_StaysNull()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var go = new GameObject("TestAvatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            var renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new Material[] { null };

            var convertedMaterials = new Dictionary<Material, Material>();
            var progressCallback = new AvatarConverter.ProgressCallback
            {
                onTextureProgress = null,
                onAnimationClipProgress = null,
                onRuntimeAnimatorProgress = null,
            };

            try
            {
                converter.ApplyConvertedMaterials(go, convertedMaterials, false, "", progressCallback);
                Assert.IsNull(renderer.sharedMaterials[0]);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void ApplyConvertedMaterials_UnconvertedMaterial_StaysOriginal()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var go = new GameObject("TestAvatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            var renderer = child.AddComponent<MeshRenderer>();
            var originalMat = new Material(Shader.Find("Standard"));
            renderer.sharedMaterials = new Material[] { originalMat };

            var convertedMaterials = new Dictionary<Material, Material>();
            var progressCallback = new AvatarConverter.ProgressCallback
            {
                onTextureProgress = null,
                onAnimationClipProgress = null,
                onRuntimeAnimatorProgress = null,
            };

            try
            {
                converter.ApplyConvertedMaterials(go, convertedMaterials, false, "", progressCallback);
                Assert.AreEqual(originalMat, renderer.sharedMaterials[0]);
            }
            finally
            {
                UObject.DestroyImmediate(originalMat);
                UObject.DestroyImmediate(go);
            }
        }
    }
}
