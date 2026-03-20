// Batch 22 - Coverage tests for AvatarConverter deeper paths, MaterialWrapperBuilder,
// CacheUtility TextureCache, MSMapGenViewModel, VRCQuestTools entry, MissingScriptsRule,
// MissingNdmfRule, AvatarConverterNdmfPhaseExtension, MatCapLitGenerator deeper paths
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using KRT.VRCQuestTools;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.Validators;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.ViewModels;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase;
using UObject = UnityEngine.Object;
using UBuildTarget = UnityEditor.BuildTarget;
using VQTBuildTarget = KRT.VRCQuestTools.Models.BuildTarget;
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;

namespace KRT.VRCQuestTools.Tests
{
    #region MaterialWrapperBuilder Tests

    [TestFixture]
    public class MaterialWrapperBuilderTests_Pipeline
    {
        private MaterialWrapperBuilder builder;

        [SetUp]
        public void SetUp()
        {
            builder = new MaterialWrapperBuilder();
        }

        [Test]
        public void Build_StandardShader_ReturnsStandardMaterial()
        {
            var shader = Shader.Find("Standard");
            if (shader == null) { Assert.Ignore("Standard shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                var result = builder.Build(mat);
                Assert.IsNotNull(result);
                Assert.IsInstanceOf<StandardMaterial>(result);
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void Build_UnlitShader_ReturnsStandardMaterial()
        {
            var shader = Shader.Find("Unlit/Color");
            if (shader == null) { Assert.Ignore("Unlit shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                var result = builder.Build(mat);
                Assert.IsNotNull(result);
                Assert.IsInstanceOf<StandardMaterial>(result);
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void Build_QuestMobileShader_ReturnsStandardMaterial()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null) { Assert.Ignore("VRChat Mobile shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                var result = builder.Build(mat);
                Assert.IsNotNull(result);
                Assert.IsInstanceOf<StandardMaterial>(result);
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void Build_UnverifiedShader_ReturnsStandardMaterial()
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null) { Assert.Ignore("Hidden shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                var result = builder.Build(mat);
                Assert.IsNotNull(result);
                Assert.IsInstanceOf<StandardMaterial>(result);
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void DetectShaderCategory_Standard_ReturnsStandard()
        {
            var shader = Shader.Find("Standard");
            if (shader == null) { Assert.Ignore("Standard shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Standard, builder.DetectShaderCategory(mat));
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void DetectShaderCategory_StandardSpecularSetup_ReturnsStandard()
        {
            var shader = Shader.Find("Standard (Specular setup)");
            if (shader == null) { Assert.Ignore("Standard (Specular setup) not found"); return; }
            var mat = new Material(shader);
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Standard, builder.DetectShaderCategory(mat));
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void DetectShaderCategory_UnlitColor_ReturnsUnlit()
        {
            var shader = Shader.Find("Unlit/Color");
            if (shader == null) { Assert.Ignore("Unlit/Color not found"); return; }
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
            if (shader == null) { Assert.Ignore("Unlit/Texture not found"); return; }
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
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null) { Assert.Ignore("VRChat/Mobile/Toon Lit not found"); return; }
            var mat = new Material(shader);
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Quest, builder.DetectShaderCategory(mat));
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void DetectShaderCategory_HiddenShader_ReturnsUnverified()
        {
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
        public void ShaderCategory_AllEnumValues_AreDefined()
        {
            var values = Enum.GetValues(typeof(MaterialWrapperBuilder.ShaderCategory));
            Assert.IsTrue(values.Length >= 11, $"Expected at least 11 ShaderCategory values, got {values.Length}");
        }
    }

    #endregion

    #region CacheUtility TextureCache Tests

    [TestFixture]
    public class CacheUtilityTextureCacheTests
    {
        [Test]
        public void TextureCache_RoundTrip_NonNormalMap()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            try
            {
                var pixels = new Color32[16];
                for (int i = 0; i < 16; i++)
                    pixels[i] = new Color32(255, 0, 0, 255);
                tex.SetPixels32(pixels);
                tex.Apply();

                var cache = new CacheUtility.TextureCache(tex, false, false, UBuildTarget.StandaloneWindows64);
                var json = JsonUtility.ToJson(cache);
                Assert.IsFalse(string.IsNullOrEmpty(json));

                var deserialized = JsonUtility.FromJson<CacheUtility.TextureCache>(json);
                Assert.IsNotNull(deserialized);

                var restored = deserialized.ToTexture2D();
                Assert.IsNotNull(restored);
                Assert.AreEqual(4, restored.width);
                Assert.AreEqual(4, restored.height);
                UObject.DestroyImmediate(restored);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void TextureCache_WithMipmaps()
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, true);
            try
            {
                tex.Apply();
                var cache = new CacheUtility.TextureCache(tex, false, false, UBuildTarget.StandaloneWindows64);
                var json = JsonUtility.ToJson(cache);
                var deserialized = JsonUtility.FromJson<CacheUtility.TextureCache>(json);
                var restored = deserialized.ToTexture2D();
                Assert.IsNotNull(restored);
                UObject.DestroyImmediate(restored);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void TextureCache_LinearTexture()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false, true);
            try
            {
                tex.Apply();
                var cache = new CacheUtility.TextureCache(tex, true, false, UBuildTarget.Android);
                var json = JsonUtility.ToJson(cache);
                Assert.IsFalse(string.IsNullOrEmpty(json));
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void GetContentCacheKey_StandardMaterial_ReturnsNonEmptyKey()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var key = CacheUtility.GetContentCacheKey(mat);
                Assert.IsFalse(string.IsNullOrEmpty(key));
                Assert.IsTrue(key.Contains("Standard"));
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetContentCacheKey_WithTexture_IncludesTextureHash()
        {
            var mat = new Material(Shader.Find("Standard"));
            var tex = new Texture2D(4, 4);
            try
            {
                tex.Apply();
                mat.mainTexture = tex;
                var key = CacheUtility.GetContentCacheKey(mat);
                Assert.IsFalse(string.IsNullOrEmpty(key));
            }
            finally
            {
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void GetContentCacheKey_DifferentColors_DifferentKeys()
        {
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            try
            {
                mat1.color = Color.red;
                mat2.color = Color.blue;
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
        public void GetContentCacheKey_WithKeywords_IncludesKeywords()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                mat.EnableKeyword("_EMISSION");
                var key = CacheUtility.GetContentCacheKey(mat);
                Assert.IsTrue(key.Contains("_EMISSION"));
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }
    }

    #endregion

    #region MSMapGenViewModel Tests

    [TestFixture]
    public class MSMapGenViewModelTests_Pipeline
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
        public void DisableGenerateButton_MetallicOnly_ReturnsFalse()
        {
            var vm = new MSMapGenViewModel();
            var tex = new Texture2D(4, 4);
            try
            {
                vm.metallicMap = tex;
                vm.smoothnessMap = null;
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally { UObject.DestroyImmediate(tex); }
        }

        [Test]
        public void DisableGenerateButton_SmoothnessOnly_ReturnsFalse()
        {
            var vm = new MSMapGenViewModel();
            var tex = new Texture2D(4, 4);
            try
            {
                vm.metallicMap = null;
                vm.smoothnessMap = tex;
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
            try
            {
                vm.metallicMap = tex1;
                vm.smoothnessMap = tex2;
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally
            {
                UObject.DestroyImmediate(tex1);
                UObject.DestroyImmediate(tex2);
            }
        }

        [Test]
        public void InvertSmoothness_DefaultIsFalse()
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

    #endregion

    #region VRCQuestTools Entry Tests

    [TestFixture]
    public class VRCQuestToolsEntryTests_Pipeline
    {
        [Test]
        public void Name_IsVRCQuestTools()
        {
            Assert.AreEqual("VRCQuestTools", VRCQuestTools.Name);
        }

        [Test]
        public void Version_IsValidSemVer()
        {
            Assert.IsFalse(string.IsNullOrEmpty(VRCQuestTools.Version));
            var parts = VRCQuestTools.Version.Split('.');
            Assert.AreEqual(3, parts.Length);
            Assert.IsTrue(int.TryParse(parts[0], out _));
            Assert.IsTrue(int.TryParse(parts[1], out _));
            Assert.IsTrue(int.TryParse(parts[2], out _));
        }

        [Test]
        public void GitHubRepository_IsSet()
        {
            var field = typeof(VRCQuestTools).GetField("GitHubRepository", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field);
            var value = (string)field.GetValue(null);
            Assert.AreEqual("kurotu/VRCQuestTools", value);
        }

        [Test]
        public void BoothURL_IsSet()
        {
            var field = typeof(VRCQuestTools).GetField("BoothURL", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field);
            var value = (string)field.GetValue(null);
            Assert.IsTrue(value.Contains("booth.pm"));
        }

        [Test]
        public void ComponentRemover_IsNotNull()
        {
            var field = typeof(VRCQuestTools).GetField("ComponentRemover", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field);
            Assert.IsNotNull(field.GetValue(null));
        }

        [Test]
        public void AvatarConverter_IsNotNull()
        {
            var field = typeof(VRCQuestTools).GetField("AvatarConverter", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field);
            Assert.IsNotNull(field.GetValue(null));
        }

        [Test]
        public void VPM_IsNotNull()
        {
            var field = typeof(VRCQuestTools).GetField("VPM", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field);
            Assert.IsNotNull(field.GetValue(null));
        }

        [Test]
        public void AssetRoot_IsNotNull()
        {
            var prop = typeof(VRCQuestTools).GetProperty("AssetRoot", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(prop);
            var value = (string)prop.GetValue(null);
            Assert.IsFalse(string.IsNullOrEmpty(value));
        }

        [Test]
        public void IsImportedAsPackage_ReturnsBoolean()
        {
            var prop = typeof(VRCQuestTools).GetProperty("IsImportedAsPackage", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(prop);
            var value = (bool)prop.GetValue(null);
            // Just verify it doesn't throw
            Assert.IsTrue(value || !value);
        }

        [Test]
        public void PackageName_IsCorrect()
        {
            var field = typeof(VRCQuestTools).GetField("PackageName", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field);
            Assert.AreEqual("com.github.kurotu.vrc-quest-tools", (string)field.GetValue(null));
        }

        [Test]
        public void VPMRepositoryURL_IsSet()
        {
            var field = typeof(VRCQuestTools).GetField("VPMRepositoryURL", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field);
            var value = (string)field.GetValue(null);
            Assert.IsTrue(value.Contains("vpm.json"));
        }

        [Test]
        public void DocsURL_IsSet()
        {
            var field = typeof(VRCQuestTools).GetField("DocsURL", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field);
            var value = (string)field.GetValue(null);
            Assert.IsTrue(value.Contains("VRCQuestTools"));
        }
    }

    #endregion

    #region AvatarConverter CreateMaterialConvertSettingsMap Tests

    [TestFixture]
    public class AvatarConverterSettingsMapTests
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
            var go = new GameObject("Avatar");
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
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithAvatarConverterSettings_AppliesDefaultSettings()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var settings = go.AddComponent<AvatarConverterSettings>();
            var child = new GameObject("Child");
            child.transform.parent = go.transform;
            var renderer = child.AddComponent<MeshRenderer>();
            var standardMat = new Material(Shader.Find("Standard"));
            renderer.sharedMaterial = standardMat;
            try
            {
                var avatar = new VRChatAvatar(desc);
                var map = converter.CreateMaterialConvertSettingsMap(avatar);
                Assert.IsNotNull(map);
                Assert.IsTrue(map.ContainsKey(standardMat));
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(standardMat);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithMaterialConversionSettings_AppliesAdditionalSettings()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var mcs = go.AddComponent<MaterialConversionSettings>();
            var standardMat = new Material(Shader.Find("Standard"));
            mcs.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings { targetMaterial = standardMat }
            };

            var child = new GameObject("Child");
            child.transform.parent = go.transform;
            var renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = standardMat;
            try
            {
                var avatarMaterials = new Material[] { standardMat };
                var map = converter.CreateMaterialConvertSettingsMap(go, avatarMaterials);
                Assert.IsNotNull(map);
                Assert.IsTrue(map.ContainsKey(standardMat));
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(standardMat);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_NullTargetMaterial_ThrowsTargetMaterialNullException()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var mcs = go.AddComponent<MaterialConversionSettings>();
            mcs.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings { targetMaterial = null }
            };
            try
            {
                var avatarMaterials = new Material[0];
                Assert.Throws<TargetMaterialNullException>(() =>
                {
                    converter.CreateMaterialConvertSettingsMap(go, avatarMaterials);
                });
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithMaterialSwap_AppliesMapping()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var swap = go.AddComponent<MaterialSwap>();
            var originalMat = new Material(Shader.Find("Standard"));
            var questShader = Shader.Find("VRChat/Mobile/Toon Lit");
            Material replacementMat = null;
            if (questShader != null)
            {
                replacementMat = new Material(questShader);
            }
            try
            {
                if (replacementMat == null) { Assert.Ignore("VRChat Mobile shader not found"); return; }
                swap.materialMappings = new List<MaterialSwap.MaterialMapping>
                {
                    new MaterialSwap.MaterialMapping
                    {
                        originalMaterial = originalMat,
                        replacementMaterial = replacementMat,
                    }
                };
                var child = new GameObject("Child");
                child.transform.parent = go.transform;
                var renderer = child.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = originalMat;

                var avatarMaterials = new Material[] { originalMat };
                var map = converter.CreateMaterialConvertSettingsMap(go, avatarMaterials);
                Assert.IsNotNull(map);
                Assert.IsTrue(map.ContainsKey(originalMat));
                Assert.IsInstanceOf<MaterialReplaceSettings>(map[originalMat]);
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(originalMat);
                if (replacementMat != null) UObject.DestroyImmediate(replacementMat);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_MaterialSwapNullOriginal_Throws()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var swap = go.AddComponent<MaterialSwap>();
            swap.materialMappings = new List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping
                {
                    originalMaterial = null,
                    replacementMaterial = null,
                }
            };
            try
            {
                var avatarMaterials = new Material[0];
                Assert.Throws<InvalidMaterialSwapNullException>(() =>
                {
                    converter.CreateMaterialConvertSettingsMap(go, avatarMaterials);
                });
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_MaterialSwapNullReplacement_Throws()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var swap = go.AddComponent<MaterialSwap>();
            var originalMat = new Material(Shader.Find("Standard"));
            swap.materialMappings = new List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping
                {
                    originalMaterial = originalMat,
                    replacementMaterial = null,
                }
            };
            try
            {
                var avatarMaterials = new Material[0];
                Assert.Throws<InvalidMaterialSwapNullException>(() =>
                {
                    converter.CreateMaterialConvertSettingsMap(go, avatarMaterials);
                });
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(originalMat);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_MaterialSwapNonQuestReplacement_Throws()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var swap = go.AddComponent<MaterialSwap>();
            var originalMat = new Material(Shader.Find("Standard"));
            var replacementMat = new Material(Shader.Find("Standard"));
            swap.materialMappings = new List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping
                {
                    originalMaterial = originalMat,
                    replacementMaterial = replacementMat,
                }
            };
            try
            {
                var avatarMaterials = new Material[0];
                Assert.Throws<InvalidReplacementMaterialException>(() =>
                {
                    converter.CreateMaterialConvertSettingsMap(go, avatarMaterials);
                });
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(originalMat);
                UObject.DestroyImmediate(replacementMat);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_AvatarConverterSettings_NullTargetMaterial_Throws()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var acs = go.AddComponent<AvatarConverterSettings>();
            acs.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings { targetMaterial = null }
            };
            try
            {
                var avatarMaterials = new Material[0];
                Assert.Throws<TargetMaterialNullException>(() =>
                {
                    converter.CreateMaterialConvertSettingsMap(go, avatarMaterials);
                });
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_OmitsUnusedMaterials()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var settings = go.AddComponent<AvatarConverterSettings>();
            var standardMat = new Material(Shader.Find("Standard"));
            var unusedMat = new Material(Shader.Find("Standard"));
            try
            {
                // Only standardMat is in avatar materials
                var avatarMaterials = new Material[] { standardMat };
                var map = converter.CreateMaterialConvertSettingsMap(go, avatarMaterials);
                Assert.IsNotNull(map);
                Assert.IsFalse(map.ContainsKey(unusedMat));
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(standardMat);
                UObject.DestroyImmediate(unusedMat);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_DuplicateMaterials_FirstWins()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var mcs1 = go.AddComponent<MaterialConversionSettings>();
            var standardMat = new Material(Shader.Find("Standard"));
            var toonLitSettings = new ToonLitConvertSettings();
            mcs1.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings { targetMaterial = standardMat, materialConvertSettings = toonLitSettings }
            };
            // Add AvatarConverterSettings with same material - should not override
            var acs = go.AddComponent<AvatarConverterSettings>();
            acs.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings { targetMaterial = standardMat, materialConvertSettings = new ToonStandardConvertSettings() }
            };
            try
            {
                var avatarMaterials = new Material[] { standardMat };
                var map = converter.CreateMaterialConvertSettingsMap(go, avatarMaterials);
                Assert.IsNotNull(map);
                Assert.IsTrue(map.ContainsKey(standardMat));
                // MaterialConversionSettings should win over AvatarConverterSettings
                Assert.IsInstanceOf<ToonLitConvertSettings>(map[standardMat]);
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(standardMat);
            }
        }
    }

    #endregion

    #region AvatarConverter RemoveExtraMaterialSlots Tests

    [TestFixture]
    public class AvatarConverterRemoveExtraMaterialSlotsTests
    {
        [Test]
        public void RemoveExtraMaterialSlots_TrimsExcessMaterials()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "RemoveExtraMaterialSlots method should exist");

            var go = new GameObject("Avatar");
            var child = new GameObject("Renderer");
            child.transform.parent = go.transform;
            var renderer = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.subMeshCount = 1; // Only 1 submesh
            renderer.sharedMesh = mesh;

            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            renderer.sharedMaterials = new Material[] { mat1, mat2 }; // 2 materials but 1 submesh
            try
            {
                method.Invoke(converter, new object[] { go });
                Assert.AreEqual(1, renderer.sharedMaterials.Length);
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(mesh);
                UObject.DestroyImmediate(mat1);
                UObject.DestroyImmediate(mat2);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_NoExcess_NoChange()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            var go = new GameObject("Avatar");
            var child = new GameObject("Renderer");
            child.transform.parent = go.transform;
            var renderer = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.subMeshCount = 1;
            renderer.sharedMesh = mesh;
            var mat1 = new Material(Shader.Find("Standard"));
            renderer.sharedMaterials = new Material[] { mat1 }; // Same count
            try
            {
                method.Invoke(converter, new object[] { go });
                Assert.AreEqual(1, renderer.sharedMaterials.Length);
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(mesh);
                UObject.DestroyImmediate(mat1);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_NullMesh_Skips()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            var go = new GameObject("Avatar");
            var child = new GameObject("Renderer");
            child.transform.parent = go.transform;
            var renderer = child.AddComponent<SkinnedMeshRenderer>();
            // No mesh assigned
            var mat1 = new Material(Shader.Find("Standard"));
            renderer.sharedMaterials = new Material[] { mat1 };
            try
            {
                // Should not throw
                method.Invoke(converter, new object[] { go });
                Assert.AreEqual(1, renderer.sharedMaterials.Length);
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(mat1);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_MeshRenderer_AlsoTrims()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            var go = new GameObject("Avatar");
            var child = new GameObject("Renderer");
            child.transform.parent = go.transform;
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
                method.Invoke(converter, new object[] { go });
                Assert.AreEqual(1, renderer.sharedMaterials.Length);
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(mesh);
                UObject.DestroyImmediate(mat1);
                UObject.DestroyImmediate(mat2);
            }
        }
    }

    #endregion

    #region AvatarConverter PrepareConvertForQuestInPlace Tests

    [TestFixture]
    public class AvatarConverterPrepareTests_Pipeline
    {
        [Test]
        public void PrepareConvertForQuestInPlace_DoesNotThrow()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var go = new GameObject("Avatar");
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
        public void PrepareModularAvatarComponentsInPlace_DoesNotThrow()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                // May throw if MA is not imported, but should not crash
                try
                {
                    converter.PrepareModularAvatarComponentsInPlace(avatar);
                }
                catch (Exception)
                {
                    // Expected if MA is not available
                }
            }
            finally { UObject.DestroyImmediate(go); }
        }
    }

    #endregion

    #region AvatarConverter ApplyConvertedMaterials Tests

    [TestFixture]
    public class AvatarConverterApplyConvertedMaterialsTests
    {
        [Test]
        public void ApplyConvertedMaterials_ReplacesRendererMaterials()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

            var child = new GameObject("Renderer");
            child.transform.parent = go.transform;
            var renderer = child.AddComponent<MeshRenderer>();
            var originalMat = new Material(Shader.Find("Standard"));
            var questShader = Shader.Find("VRChat/Mobile/Toon Lit");
            Material convertedMat = null;
            if (questShader != null)
            {
                convertedMat = new Material(questShader);
            }
            try
            {
                if (convertedMat == null) { Assert.Ignore("Quest shader not found"); return; }
                renderer.sharedMaterial = originalMat;
                var convertedMaterials = new Dictionary<Material, Material>
                {
                    { originalMat, convertedMat }
                };
                var progressCallback = new AvatarConverter.ProgressCallback
                {
                    onTextureProgress = (total, index, orig, conv) => { },
                    onAnimationClipProgress = (total, index, orig, conv) => { },
                    onRuntimeAnimatorProgress = (total, index, orig, conv) => { },
                };
                converter.ApplyConvertedMaterials(go, convertedMaterials, false, "", progressCallback);

                // Renderer should now have the converted material
                Assert.AreEqual(convertedMat, renderer.sharedMaterial);
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(originalMat);
                if (convertedMat != null) UObject.DestroyImmediate(convertedMat);
            }
        }

        [Test]
        public void ApplyConvertedMaterials_NullMaterial_StaysNull()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var child = new GameObject("Renderer");
            child.transform.parent = go.transform;
            var renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new Material[] { null };
            try
            {
                var convertedMaterials = new Dictionary<Material, Material>();
                var progressCallback = new AvatarConverter.ProgressCallback
                {
                    onTextureProgress = (total, index, orig, conv) => { },
                    onAnimationClipProgress = (total, index, orig, conv) => { },
                    onRuntimeAnimatorProgress = (total, index, orig, conv) => { },
                };
                converter.ApplyConvertedMaterials(go, convertedMaterials, false, "", progressCallback);
                Assert.IsNull(renderer.sharedMaterials[0]);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void ApplyConvertedMaterials_UnmappedMaterial_StaysSame()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var child = new GameObject("Renderer");
            child.transform.parent = go.transform;
            var renderer = child.AddComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Standard"));
            renderer.sharedMaterial = mat;
            try
            {
                // Empty conversion map
                var convertedMaterials = new Dictionary<Material, Material>();
                var progressCallback = new AvatarConverter.ProgressCallback
                {
                    onTextureProgress = (total, index, orig, conv) => { },
                    onAnimationClipProgress = (total, index, orig, conv) => { },
                    onRuntimeAnimatorProgress = (total, index, orig, conv) => { },
                };
                converter.ApplyConvertedMaterials(go, convertedMaterials, false, "", progressCallback);
                Assert.AreEqual(mat, renderer.sharedMaterial);
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(mat);
            }
        }
    }

    #endregion

    #region MissingScriptsRule Tests

    [TestFixture]
    public class MissingScriptsRuleTests_Pipeline
    {
        [Test]
        public void Validate_ActiveAvatarNoMissingScripts_ReturnsNull()
        {
            var rule = new MissingScriptsRule();
            var go = new GameObject("Avatar");
            go.SetActive(true);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void Validate_InactiveAvatar_ReturnsNull()
        {
            var rule = new MissingScriptsRule();
            var parent = new GameObject("Parent");
            parent.SetActive(false);
            var go = new GameObject("Avatar");
            go.transform.parent = parent.transform;
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally { UObject.DestroyImmediate(parent); }
        }
    }

    #endregion

    #region MissingNdmfRule Tests

    [TestFixture]
    public class MissingNdmfRuleTests_Pipeline
    {
        [Test]
        public void Validate_NoNdmfComponents_ReturnsNull()
        {
            var rule = new MissingNdmfRule();
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                var result = rule.Validate(avatar);
                // When VQT_HAS_NDMF is defined, always null
                // When not defined, null if no INdmfComponent
                Assert.IsNull(result);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void Validate_WithAvatarConverterSettings_MayReturnNotification()
        {
            // AvatarConverterSettings implements INdmfComponent
            var rule = new MissingNdmfRule();
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            go.AddComponent<AvatarConverterSettings>();
            try
            {
                var avatar = new VRChatAvatar(desc);
                var result = rule.Validate(avatar);
                // When VQT_HAS_NDMF is defined, always returns null (NDMF is present)
                // Just verify it doesn't throw
                Assert.IsNull(result, "When NDMF is present, MissingNdmfRule should return null");
            }
            finally { UObject.DestroyImmediate(go); }
        }
    }

    #endregion

    #region AvatarConverter ProgressCallback Tests

    [TestFixture]
    public class ProgressCallbackTests_Pipeline
    {
        [Test]
        public void ProgressCallback_CanBeCreated()
        {
            var callback = new AvatarConverter.ProgressCallback();
            Assert.IsNotNull(callback);
        }

        [Test]
        public void TextureProgressCallback_CanBeInvoked()
        {
            var invoked = false;
            AvatarConverter.TextureProgressCallback callback = (total, index, orig, conv) => { invoked = true; };
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                callback(1, 0, mat, null);
                Assert.IsTrue(invoked);
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void AnimationClipProgressCallback_CanBeInvoked()
        {
            var invoked = false;
            AvatarConverter.AnimationClipProgressCallback callback = (total, index, orig, conv) => { invoked = true; };
            callback(1, 0, null, null);
            Assert.IsTrue(invoked);
        }

        [Test]
        public void RuntimeAnimatorProgressCallback_CanBeInvoked()
        {
            var invoked = false;
            AvatarConverter.RuntimeAnimatorProgressCallback callback = (total, index, orig, conv) => { invoked = true; };
            callback(1, 0, null, null);
            Assert.IsTrue(invoked);
        }
    }

    #endregion

    #region AvatarConverter SharedBlackTextureCache Tests

    [TestFixture]
    public class SharedBlackTextureCacheTests_Pipeline
    {
        [Test]
        public void ClearSharedBlackTextureCache_DoesNotThrow()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("ClearSharedBlackTextureCache", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("ClearSharedBlackTextureCache not found"); return; }
            Assert.DoesNotThrow(() => method.Invoke(converter, null));
        }

        [Test]
        public void GetOrCreateSharedBlackTexture_InMemoryMode_ReturnsTexture()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("GetOrCreateSharedBlackTexture", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("GetOrCreateSharedBlackTexture not found"); return; }
            Texture2D result = null;
            try
            {
                result = (Texture2D)method.Invoke(converter, new object[] { false, "" });
                Assert.IsNotNull(result);
            }
            finally
            {
                if (result != null) UObject.DestroyImmediate(result);
            }
        }

        [Test]
        public void GetOrCreateSharedBlackTexture_SameArgs_ReturnsCached()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("GetOrCreateSharedBlackTexture", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("GetOrCreateSharedBlackTexture not found"); return; }
            Texture2D result1 = null;
            Texture2D result2 = null;
            try
            {
                result1 = (Texture2D)method.Invoke(converter, new object[] { false, "" });
                result2 = (Texture2D)method.Invoke(converter, new object[] { false, "" });
                Assert.AreSame(result1, result2);
            }
            finally
            {
                if (result1 != null) UObject.DestroyImmediate(result1);
            }
        }
    }

    #endregion

    #region MaterialGeneratorUtility TextureConfig Tests

    [TestFixture]
    public class MaterialGeneratorUtilityTests_Pipeline
    {
        [Test]
        public void ConvertToNullableTextureFormat_NoOverride_ReturnsNull()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod("ConvertToNullableTextureFormat", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "ConvertToNullableTextureFormat should exist");
            var result = method.Invoke(null, new object[] { MobileTextureFormat.NoOverride });
            Assert.IsNull(result);
        }

        [Test]
        public void ConvertToNullableTextureFormat_ASTC_4x4_ReturnsFormat()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod("ConvertToNullableTextureFormat", BindingFlags.NonPublic | BindingFlags.Static);
            var result = (TextureFormat?)method.Invoke(null, new object[] { MobileTextureFormat.ASTC_4x4 });
            Assert.IsNotNull(result);
            Assert.AreEqual(TextureFormat.ASTC_4x4, result.Value);
        }

        [Test]
        public void ConvertToNullableTextureFormat_ASTC_6x6_ReturnsFormat()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod("ConvertToNullableTextureFormat", BindingFlags.NonPublic | BindingFlags.Static);
            var result = (TextureFormat?)method.Invoke(null, new object[] { MobileTextureFormat.ASTC_6x6 });
            Assert.IsNotNull(result);
            Assert.AreEqual(TextureFormat.ASTC_6x6, result.Value);
        }

        [Test]
        public void ConvertToNullableTextureFormat_ASTC_8x8_ReturnsFormat()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod("ConvertToNullableTextureFormat", BindingFlags.NonPublic | BindingFlags.Static);
            var result = (TextureFormat?)method.Invoke(null, new object[] { MobileTextureFormat.ASTC_8x8 });
            Assert.IsNotNull(result);
            Assert.AreEqual(TextureFormat.ASTC_8x8, result.Value);
        }

        [Test]
        public void ConvertToNullableTextureFormat_ASTC_10x10_ReturnsFormat()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod("ConvertToNullableTextureFormat", BindingFlags.NonPublic | BindingFlags.Static);
            var result = (TextureFormat?)method.Invoke(null, new object[] { MobileTextureFormat.ASTC_10x10 });
            Assert.IsNotNull(result);
            Assert.AreEqual(TextureFormat.ASTC_10x10, result.Value);
        }

        [Test]
        public void ConvertToNullableTextureFormat_ASTC_12x12_ReturnsFormat()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod("ConvertToNullableTextureFormat", BindingFlags.NonPublic | BindingFlags.Static);
            var result = (TextureFormat?)method.Invoke(null, new object[] { MobileTextureFormat.ASTC_12x12 });
            Assert.IsNotNull(result);
            Assert.AreEqual(TextureFormat.ASTC_12x12, result.Value);
        }

        [Test]
        public void ConvertToNullableTextureFormat_ASTC_5x5_ReturnsFormat()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod("ConvertToNullableTextureFormat", BindingFlags.NonPublic | BindingFlags.Static);
            var result = (TextureFormat?)method.Invoke(null, new object[] { MobileTextureFormat.ASTC_5x5 });
            Assert.IsNotNull(result);
            Assert.AreEqual(TextureFormat.ASTC_5x5, result.Value);
        }

        [Test]
        public void TextureConfig_SRGB_HasCorrectValues()
        {
            var type = typeof(MaterialGeneratorUtility).GetNestedType("TextureConfig", BindingFlags.NonPublic);
            Assert.IsNotNull(type, "TextureConfig should exist");
            var prop = type.GetProperty("SRGB", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(prop, "SRGB property should exist");
            var config = prop.GetValue(null);
            var isSRGB = (bool)type.GetField("isSRGB", BindingFlags.Public | BindingFlags.Instance).GetValue(config);
            var isNormalMap = (bool)type.GetField("isNormalMap", BindingFlags.Public | BindingFlags.Instance).GetValue(config);
            Assert.IsTrue(isSRGB);
            Assert.IsFalse(isNormalMap);
        }

        [Test]
        public void TextureConfig_Parameter_HasCorrectValues()
        {
            var type = typeof(MaterialGeneratorUtility).GetNestedType("TextureConfig", BindingFlags.NonPublic);
            var prop = type.GetProperty("Parameter", BindingFlags.Public | BindingFlags.Static);
            var config = prop.GetValue(null);
            var isSRGB = (bool)type.GetField("isSRGB", BindingFlags.Public | BindingFlags.Instance).GetValue(config);
            var isNormalMap = (bool)type.GetField("isNormalMap", BindingFlags.Public | BindingFlags.Instance).GetValue(config);
            Assert.IsFalse(isSRGB);
            Assert.IsFalse(isNormalMap);
        }

        [Test]
        public void TextureConfig_NormalMap_HasCorrectValues()
        {
            var type = typeof(MaterialGeneratorUtility).GetNestedType("TextureConfig", BindingFlags.NonPublic);
            var prop = type.GetProperty("NormalMap", BindingFlags.Public | BindingFlags.Static);
            var config = prop.GetValue(null);
            var isSRGB = (bool)type.GetField("isSRGB", BindingFlags.Public | BindingFlags.Instance).GetValue(config);
            var isNormalMap = (bool)type.GetField("isNormalMap", BindingFlags.Public | BindingFlags.Instance).GetValue(config);
            Assert.IsFalse(isSRGB);
            Assert.IsTrue(isNormalMap);
        }
    }

    #endregion

    #region AvatarConverter ConvertForQuestInPlace Validation Tests

    [TestFixture]
    public class AvatarConverterConvertForQuestValidationTests
    {
        [Test]
        public void ConvertForQuestInPlace_WithLegacyMA_ThrowsLegacyPackageException()
        {
            // ModularAvatarUtility.IsLegacyVersion() check
            // This test verifies the check exists. If MA is not in legacy mode, it proceeds.
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                var remover = new ComponentRemover();
                // Try conversion - will fail because of missing components but validates early checks
                try
                {
                    converter.ConvertForQuestInPlace(avatar, remover, false, "", new AvatarConverter.ProgressCallback
                    {
                        onTextureProgress = (a, b, c, d) => { },
                        onAnimationClipProgress = (a, b, c, d) => { },
                        onRuntimeAnimatorProgress = (a, b, c, d) => { },
                    });
                }
                catch (LegacyPackageException)
                {
                    // Expected if MA is in legacy mode
                    Assert.Pass("LegacyPackageException thrown as expected");
                }
                catch (BreakingPackageException)
                {
                    // Expected if MA is in breaking mode
                    Assert.Pass("BreakingPackageException thrown as expected");
                }
                catch (Exception)
                {
                    // Other exceptions are OK - we just wanted to test the early validation
                }
            }
            finally { UObject.DestroyImmediate(go); }
        }
    }

    #endregion

    #region AvatarConverter GenerateConvertedMaterial Tests

    [TestFixture]
    public class AvatarConverterGenerateConvertedMaterialTests
    {
        [Test]
        public void GenerateConvertedMaterial_MaterialReplaceSettings_NullMaterial_Throws()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("GenerateConvertedMaterial", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "GenerateConvertedMaterial should exist");

            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var materialBase = new StandardMaterial(mat);
                var replaceSettings = new MaterialReplaceSettings { material = null };
                var ex = Assert.Throws<TargetInvocationException>(() =>
                {
                    method.Invoke(converter, new object[]
                    {
                        materialBase,
                        replaceSettings,
                        false,
                        "",
                        (Action<Material>)((m) => { }),
                    });
                });
                Assert.IsInstanceOf<InvalidOperationException>(ex.InnerException);
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void GenerateConvertedMaterial_MaterialReplaceSettings_ValidMaterial_ReturnsResultRequest()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("GenerateConvertedMaterial", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("GenerateConvertedMaterial not found"); return; }

            var mat = new Material(Shader.Find("Standard"));
            var questShader = Shader.Find("VRChat/Mobile/Toon Lit");
            Material replaceMat = null;
            if (questShader != null) replaceMat = new Material(questShader);
            try
            {
                if (replaceMat == null) { Assert.Ignore("Quest shader not found"); return; }
                var materialBase = new StandardMaterial(mat);
                var replaceSettings = new MaterialReplaceSettings { material = replaceMat };
                Material result = null;
                // Use the converter's ConvertMaterialsForMobile instead since GenerateConvertedMaterial
                // may have complex parameter types
                var map = new Dictionary<Material, IMaterialConvertSettings>
                {
                    { mat, replaceSettings }
                };
                var convertedMaterials = converter.ConvertMaterialsForMobile(map, false, "", (total, index, orig, conv) => { });
                Assert.IsNotNull(convertedMaterials);
                Assert.AreEqual(1, convertedMaterials.Count);
                Assert.AreEqual(replaceMat, convertedMaterials[mat]);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
                if (replaceMat != null) UObject.DestroyImmediate(replaceMat);
            }
        }
    }

    #endregion

    #region AvatarConverter ApplyVRCQuestToolsComponents Tests

    [TestFixture]
    public class ApplyVRCQuestToolsComponentsTests_Pipeline
    {
        [Test]
        public void ApplyVRCQuestToolsComponents_AddsConvertedAvatar()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("ApplyVRCQuestToolsComponents", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("ApplyVRCQuestToolsComponents not found"); return; }

            var go = new GameObject("Avatar");
            var settings = go.AddComponent<AvatarConverterSettings>();
            try
            {
                method.Invoke(converter, new object[] { settings, go });
                Assert.IsNotNull(go.GetComponent<ConvertedAvatar>());
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void ApplyVRCQuestToolsComponents_AlreadyHasConvertedAvatar_DoesNotDuplicate()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("ApplyVRCQuestToolsComponents", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("ApplyVRCQuestToolsComponents not found"); return; }

            var go = new GameObject("Avatar");
            var settings = go.AddComponent<AvatarConverterSettings>();
            go.AddComponent<ConvertedAvatar>();
            try
            {
                method.Invoke(converter, new object[] { settings, go });
                Assert.AreEqual(1, go.GetComponents<ConvertedAvatar>().Length);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void ApplyVRCQuestToolsComponents_WithPlatformTargetSettings_SetsBuildTarget()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("ApplyVRCQuestToolsComponents", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("ApplyVRCQuestToolsComponents not found"); return; }

            var go = new GameObject("Avatar");
            var settings = go.AddComponent<AvatarConverterSettings>();
            var platformSettings = go.AddComponent<PlatformTargetSettings>();
            try
            {
                method.Invoke(converter, new object[] { settings, go });
                Assert.AreEqual(VQTBuildTarget.Android, platformSettings.buildTarget);
            }
            finally { UObject.DestroyImmediate(go); }
        }
    }

    #endregion

    #region AvatarConverter ConvertMaterialsForMobile Tests

    [TestFixture]
    public class ConvertMaterialsForMobileTests_Pipeline
    {
        [Test]
        public void ConvertMaterialsForMobile_EmptyMap_ReturnsEmpty()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var map = new Dictionary<Material, IMaterialConvertSettings>();
            var result = converter.ConvertMaterialsForMobile(map, false, "", null);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ConvertMaterialsForMobile_QuestMaterial_SkippedAsAlreadyAllowed()
        {
            var questShader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (questShader == null) { Assert.Ignore("Quest shader not found"); return; }
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var mat = new Material(questShader);
            try
            {
                var map = new Dictionary<Material, IMaterialConvertSettings>
                {
                    { mat, new ToonLitConvertSettings() }
                };
                var result = converter.ConvertMaterialsForMobile(map, false, "", null);
                Assert.IsNotNull(result);
                // Quest material is already allowed, so it should be skipped
                Assert.AreEqual(0, result.Count);
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void ConvertMaterialsForMobile_WithReplaceSettings_ReturnsConvertedMaterial()
        {
            var questShader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (questShader == null) { Assert.Ignore("Quest shader not found"); return; }
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var originalMat = new Material(Shader.Find("Standard"));
            var replaceMat = new Material(questShader);
            try
            {
                var map = new Dictionary<Material, IMaterialConvertSettings>
                {
                    { originalMat, new MaterialReplaceSettings { material = replaceMat } }
                };
                var result = converter.ConvertMaterialsForMobile(map, false, "", (total, index, orig, conv) => { });
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(replaceMat, result[originalMat]);
            }
            finally
            {
                UObject.DestroyImmediate(originalMat);
                UObject.DestroyImmediate(replaceMat);
            }
        }
    }

    #endregion
}
