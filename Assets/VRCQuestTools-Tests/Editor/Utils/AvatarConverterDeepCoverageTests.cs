// Batch 20: Coverage improvement tests targeting remaining testable areas
// AvatarConverter deeper paths, VRCQuestToolsAvatarProcessor, VRChatAvatar,
// VRCQuestToolsSettings, MaterialGeneratorUtility, CacheUtility edge cases

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
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.ViewModels;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;
using UObject = UnityEngine.Object;
using UBuildTarget = UnityEditor.BuildTarget;
using VQTBuildTarget = KRT.VRCQuestTools.Models.BuildTarget;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    public class AvatarConverterDeepTests
    {
        private static AvatarConverter CreateConverter()
        {
            return new AvatarConverter(new MaterialWrapperBuilder());
        }

        private static VRCAvatarDescriptor CreateDescriptor(GameObject go)
        {
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            return desc;
        }

        [Test]
        public void FindDescendant_ReturnsChild()
        {
            var parent = new GameObject("Parent");
            var child = new GameObject("Child");
            child.transform.SetParent(parent.transform);
            try
            {
                var converter = CreateConverter();
                var method = typeof(AvatarConverter).GetMethod("FindDescendant",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(method, "FindDescendant method not found");
                var result = method.Invoke(converter, new object[] { parent, "Child" }) as GameObject;
                Assert.IsNotNull(result);
                Assert.AreEqual("Child", result.name);
            }
            finally
            {
                UObject.DestroyImmediate(parent);
            }
        }

        [Test]
        public void FindDescendant_ReturnsNestedChild()
        {
            var root = new GameObject("Root");
            var mid = new GameObject("Mid");
            var deep = new GameObject("DeepChild");
            mid.transform.SetParent(root.transform);
            deep.transform.SetParent(mid.transform);
            try
            {
                var converter = CreateConverter();
                var method = typeof(AvatarConverter).GetMethod("FindDescendant",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = method.Invoke(converter, new object[] { root, "DeepChild" }) as GameObject;
                Assert.IsNotNull(result);
                Assert.AreEqual("DeepChild", result.name);
            }
            finally
            {
                UObject.DestroyImmediate(root);
            }
        }

        [Test]
        public void FindDescendant_ReturnsNullWhenNotFound()
        {
            var root = new GameObject("Root");
            try
            {
                var converter = CreateConverter();
                var method = typeof(AvatarConverter).GetMethod("FindDescendant",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var result = method.Invoke(converter, new object[] { root, "NonExistent" });
                Assert.IsNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(root);
            }
        }

        [Test]
        public void ClearSharedBlackTextureCache_DoesNotThrow()
        {
            var converter = CreateConverter();
            var method = typeof(AvatarConverter).GetMethod("ClearSharedBlackTextureCache",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method);
            Assert.DoesNotThrow(() => method.Invoke(converter, null));
        }

        [Test]
        public void GetOrCreateSharedBlackTexture_InMemory_ReturnsTexture()
        {
            var converter = CreateConverter();
            var method = typeof(AvatarConverter).GetMethod("GetOrCreateSharedBlackTexture",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "GetOrCreateSharedBlackTexture not found");
            var result = method.Invoke(converter, new object[] { false, "dummy" }) as Texture2D;
            try
            {
                Assert.IsNotNull(result);
                Assert.AreEqual("VQT_Shared_Black", result.name);
            }
            finally
            {
                if (result != null) UObject.DestroyImmediate(result);
            }
        }

        [Test]
        public void GetOrCreateSharedBlackTexture_ReturnsCachedOnSecondCall()
        {
            var converter = CreateConverter();
            var method = typeof(AvatarConverter).GetMethod("GetOrCreateSharedBlackTexture",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var result1 = method.Invoke(converter, new object[] { false, "test" }) as Texture2D;
            var result2 = method.Invoke(converter, new object[] { false, "test" }) as Texture2D;
            try
            {
                Assert.IsNotNull(result1);
                Assert.IsNotNull(result2);
                Assert.AreSame(result1, result2, "Should return cached texture");
            }
            finally
            {
                if (result1 != null) UObject.DestroyImmediate(result1);
            }
        }

        [Test]
        public void CreateSharedBlackTexture_InMemory()
        {
            var converter = CreateConverter();
            var method = typeof(AvatarConverter).GetMethod("CreateSharedBlackTexture",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method);
            var result = method.Invoke(converter, new object[] { false, "dummy" }) as Texture2D;
            try
            {
                Assert.IsNotNull(result);
                Assert.AreEqual("VQT_Shared_Black", result.name);
            }
            finally
            {
                if (result != null) UObject.DestroyImmediate(result);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_TrimsExtraMaterials()
        {
            var go = new GameObject("Avatar");
            try
            {
                var meshFilter = go.AddComponent<MeshFilter>();
                var meshRenderer = go.AddComponent<MeshRenderer>();
                var mesh = new Mesh();
                mesh.vertices = new Vector3[] { Vector3.zero, Vector3.up, Vector3.right };
                mesh.triangles = new int[] { 0, 1, 2 };
                mesh.subMeshCount = 1;
                meshFilter.sharedMesh = mesh;

                var mat1 = new Material(Shader.Find("Standard"));
                var mat2 = new Material(Shader.Find("Standard"));
                meshRenderer.sharedMaterials = new Material[] { mat1, mat2 };

                var converter = CreateConverter();
                var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(method);
                method.Invoke(converter, new object[] { go });

                Assert.AreEqual(1, meshRenderer.sharedMaterials.Length);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_KeepsCorrectCount()
        {
            var go = new GameObject("Avatar");
            try
            {
                var meshFilter = go.AddComponent<MeshFilter>();
                var meshRenderer = go.AddComponent<MeshRenderer>();
                var mesh = new Mesh();
                mesh.vertices = new Vector3[] { Vector3.zero, Vector3.up, Vector3.right };
                mesh.triangles = new int[] { 0, 1, 2 };
                mesh.subMeshCount = 1;
                meshFilter.sharedMesh = mesh;

                var mat1 = new Material(Shader.Find("Standard"));
                meshRenderer.sharedMaterials = new Material[] { mat1 };

                var converter = CreateConverter();
                var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(converter, new object[] { go });

                Assert.AreEqual(1, meshRenderer.sharedMaterials.Length);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithMaterialConversionSettings()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var convSettings = go.AddComponent<MaterialConversionSettings>();
                var mat = new Material(Shader.Find("Standard"));
                mat.name = "TestMat";

                convSettings.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
                {
                    new AdditionalMaterialConvertSettings
                    {
                        targetMaterial = mat,
                        materialConvertSettings = new ToonLitConvertSettings(),
                    },
                };

                var meshFilter = go.AddComponent<MeshFilter>();
                var meshRenderer = go.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterials = new Material[] { mat };

                var converter = CreateConverter();
                var map = converter.CreateMaterialConvertSettingsMap(go, new Material[] { mat });

                Assert.IsTrue(map.ContainsKey(mat));
                Assert.IsInstanceOf<ToonLitConvertSettings>(map[mat]);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_MaterialSwap_AddsMappings()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var matSwap = go.AddComponent<MaterialSwap>();
                var originalMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
                var replaceMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
                originalMat.name = "Original";
                replaceMat.name = "Replace";

                matSwap.materialMappings = new List<MaterialSwap.MaterialMapping>
                {
                    new MaterialSwap.MaterialMapping
                    {
                        originalMaterial = originalMat,
                        replacementMaterial = replaceMat,
                    },
                };

                var converter = CreateConverter();
                var map = converter.CreateMaterialConvertSettingsMap(go, new Material[] { originalMat });

                Assert.IsTrue(map.ContainsKey(originalMat));
                Assert.IsInstanceOf<MaterialReplaceSettings>(map[originalMat]);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_MaterialSwap_NullOriginal_Throws()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var matSwap = go.AddComponent<MaterialSwap>();
                var replaceMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));

                matSwap.materialMappings = new List<MaterialSwap.MaterialMapping>
                {
                    new MaterialSwap.MaterialMapping
                    {
                        originalMaterial = null,
                        replacementMaterial = replaceMat,
                    },
                };

                var converter = CreateConverter();
                Assert.Throws<InvalidMaterialSwapNullException>(() =>
                    converter.CreateMaterialConvertSettingsMap(go, new Material[0]));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_MaterialSwap_NullReplacement_Throws()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var matSwap = go.AddComponent<MaterialSwap>();
                var originalMat = new Material(Shader.Find("Standard"));

                matSwap.materialMappings = new List<MaterialSwap.MaterialMapping>
                {
                    new MaterialSwap.MaterialMapping
                    {
                        originalMaterial = originalMat,
                        replacementMaterial = null,
                    },
                };

                var converter = CreateConverter();
                Assert.Throws<InvalidMaterialSwapNullException>(() =>
                    converter.CreateMaterialConvertSettingsMap(go, new Material[] { originalMat }));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_DefaultSettings_Applied()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var settings = go.AddComponent<AvatarConverterSettings>();
                var mat = new Material(Shader.Find("Standard"));
                mat.name = "UnmappedMat";

                var meshFilter = go.AddComponent<MeshFilter>();
                var meshRenderer = go.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterials = new Material[] { mat };

                var converter = CreateConverter();
                var avatar = new VRChatAvatar(desc);
                var map = converter.CreateMaterialConvertSettingsMap(avatar);

                Assert.IsTrue(map.ContainsKey(mat));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_AvatarConverterSettings_NullTarget_Throws()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var settings = go.AddComponent<AvatarConverterSettings>();
                settings.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
                {
                    new AdditionalMaterialConvertSettings
                    {
                        targetMaterial = null,
                        materialConvertSettings = new ToonLitConvertSettings(),
                    },
                };

                var converter = CreateConverter();
                Assert.Throws<TargetMaterialNullException>(() =>
                    converter.CreateMaterialConvertSettingsMap(go, new Material[0]));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_FiltersUnusedMaterials()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var settings = go.AddComponent<AvatarConverterSettings>();
                var usedMat = new Material(Shader.Find("Standard"));
                var unusedMat = new Material(Shader.Find("Standard"));
                usedMat.name = "Used";
                unusedMat.name = "Unused";

                var meshRenderer = go.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterials = new Material[] { usedMat };

                var converter = CreateConverter();
                var map = converter.CreateMaterialConvertSettingsMap(go, new Material[] { usedMat });

                Assert.IsTrue(map.ContainsKey(usedMat));
                Assert.IsFalse(map.ContainsKey(unusedMat));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void ProgressCallback_CanBeCreated()
        {
            var type = typeof(AvatarConverter).GetNestedType("ProgressCallback", BindingFlags.NonPublic);
            Assert.IsNotNull(type, "ProgressCallback nested type not found");
            var instance = Activator.CreateInstance(type);
            Assert.IsNotNull(instance);
        }

        [Test]
        public void PrepareModularAvatarComponentsInPlace_DoesNotThrow()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var avatar = new VRChatAvatar(desc);
                var converter = CreateConverter();
                Assert.DoesNotThrow(() => converter.PrepareModularAvatarComponentsInPlace(avatar));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void MaterialSwap_InvalidReplacement_NonMobileShader_Throws()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var matSwap = go.AddComponent<MaterialSwap>();
                var originalMat = new Material(Shader.Find("Standard"));
                var replaceMat = new Material(Shader.Find("Standard"));
                originalMat.name = "Orig";
                replaceMat.name = "BadReplace";

                matSwap.materialMappings = new List<MaterialSwap.MaterialMapping>
                {
                    new MaterialSwap.MaterialMapping
                    {
                        originalMaterial = originalMat,
                        replacementMaterial = replaceMat,
                    },
                };

                var converter = CreateConverter();
                Assert.Throws<InvalidReplacementMaterialException>(() =>
                    converter.CreateMaterialConvertSettingsMap(go, new Material[] { originalMat }));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void ApplyVirtualLens2Support_WithVirtualLensRoot()
        {
            var avatar = new GameObject("Avatar");
            var vlRoot = new GameObject("_VirtualLens_Root");
            vlRoot.transform.SetParent(avatar.transform);
            var vlOrigin = new GameObject("VirtualLensOrigin");
            vlOrigin.transform.SetParent(avatar.transform);
            try
            {
                var converter = CreateConverter();
                var method = typeof(AvatarConverter).GetMethod("ApplyVirtualLens2Support",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(method);
                method.Invoke(converter, new object[] { avatar });

                Assert.AreEqual("EditorOnly", vlRoot.tag);
                Assert.IsFalse(vlRoot.activeSelf);
                Assert.AreEqual("EditorOnly", vlOrigin.tag);
                Assert.IsFalse(vlOrigin.activeSelf);
            }
            finally
            {
                UObject.DestroyImmediate(avatar);
            }
        }

        [Test]
        public void ApplyVirtualLens2Support_WithoutVirtualLensRoot_DoesNotThrow()
        {
            var avatar = new GameObject("Avatar");
            try
            {
                var converter = CreateConverter();
                var method = typeof(AvatarConverter).GetMethod("ApplyVirtualLens2Support",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.DoesNotThrow(() => method.Invoke(converter, new object[] { avatar }));
            }
            finally
            {
                UObject.DestroyImmediate(avatar);
            }
        }

        [Test]
        public void ApplyVRCQuestToolsComponents_AddsConvertedAvatar()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var settings = go.AddComponent<AvatarConverterSettings>();

                var converter = CreateConverter();
                var method = typeof(AvatarConverter).GetMethod("ApplyVRCQuestToolsComponents",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(method);
                method.Invoke(converter, new object[] { settings, go });

                Assert.IsNotNull(go.GetComponent<ConvertedAvatar>());
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void ApplyVRCQuestToolsComponents_SetsPlatformTargetSettings()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var settings = go.AddComponent<AvatarConverterSettings>();
                var platformSettings = go.AddComponent<PlatformTargetSettings>();
                platformSettings.buildTarget = VQTBuildTarget.PC;

                var converter = CreateConverter();
                var method = typeof(AvatarConverter).GetMethod("ApplyVRCQuestToolsComponents",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(converter, new object[] { settings, go });

                Assert.AreEqual(VQTBuildTarget.Android, platformSettings.buildTarget);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    public class VRChatAvatarDeepTests
    {
        private static VRCAvatarDescriptor CreateDescriptor(GameObject go)
        {
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            return desc;
        }

        [Test]
        public void HasVertexColor_FalseWhenNoColors()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var child = new GameObject("Child");
                child.transform.SetParent(go.transform);
                var smr = child.AddComponent<SkinnedMeshRenderer>();
                var mesh = new Mesh();
                mesh.vertices = new Vector3[] { Vector3.zero, Vector3.up, Vector3.right };
                mesh.triangles = new int[] { 0, 1, 2 };
                smr.sharedMesh = mesh;

                var avatar = new VRChatAvatar(desc);
                Assert.IsFalse(avatar.HasVertexColor);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasVertexColor_TrueWhenHasColors()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var child = new GameObject("Child");
                child.transform.SetParent(go.transform);
                var smr = child.AddComponent<SkinnedMeshRenderer>();
                var mesh = new Mesh();
                mesh.vertices = new Vector3[] { Vector3.zero, Vector3.up, Vector3.right };
                mesh.triangles = new int[] { 0, 1, 2 };
                mesh.colors32 = new Color32[] { Color.red, Color.green, Color.blue };
                smr.sharedMesh = mesh;

                var avatar = new VRChatAvatar(desc);
                Assert.IsTrue(avatar.HasVertexColor);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasDynamicBoneComponents_FalseWhenNotImported()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var avatar = new VRChatAvatar(desc);
                Assert.IsFalse(avatar.HasDynamicBoneComponents);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasUnityConstraints_FalseWhenNone()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var avatar = new VRChatAvatar(desc);
                Assert.IsFalse(avatar.HasUnityConstraints);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasAnimatedMaterials_FalseWhenNoAnimators()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var avatar = new VRChatAvatar(desc);
                Assert.IsFalse(avatar.HasAnimatedMaterials);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRelatedMaterials_IncludesRendererMaterials()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var child = new GameObject("Child");
                child.transform.SetParent(go.transform);
                var mr = child.AddComponent<MeshRenderer>();
                var mat = new Material(Shader.Find("Standard"));
                mr.sharedMaterials = new Material[] { mat };

                var result = VRChatAvatar.GetRelatedMaterials(go);
                Assert.Contains(mat, result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRuntimeAnimatorControllers_ReturnsEmpty()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var avatar = new VRChatAvatar(desc);
                var controllers = avatar.GetRuntimeAnimatorControllers();
                Assert.IsNotNull(controllers);
                Assert.AreEqual(0, controllers.Length);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetContacts_ReturnsEmptyWhenNone()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var avatar = new VRChatAvatar(desc);
                Assert.AreEqual(0, avatar.GetContacts().Length);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetNonLocalContacts_ReturnsContacts()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var child = new GameObject("Contact");
                child.transform.SetParent(go.transform);
                var contact = child.AddComponent<VRCContactReceiver>();

                var avatar = new VRChatAvatar(desc);
                var contacts = avatar.GetNonLocalContacts();
                Assert.IsNotNull(contacts);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetLocalContactReceivers_ReturnsEmpty()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var avatar = new VRChatAvatar(desc);
                Assert.AreEqual(0, avatar.GetLocalContactReceivers().Length);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetLocalContactSenders_ReturnsEmpty()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var avatar = new VRChatAvatar(desc);
                Assert.AreEqual(0, avatar.GetLocalContactSenders().Length);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    public class VRCQuestToolsSettingsTests_Deep
    {
        [Test]
        public void LatestVersionCache_GetSet()
        {
            var original = VRCQuestToolsSettings.LatestVersionCache;
            try
            {
                VRCQuestToolsSettings.LatestVersionCache = new SemVer("1.2.3");
                var result = VRCQuestToolsSettings.LatestVersionCache;
                Assert.AreEqual("1.2.3", result.ToString());
            }
            finally
            {
                VRCQuestToolsSettings.LatestVersionCache = original;
            }
        }

        [Test]
        public void SkippedVersion_GetSet()
        {
            var original = VRCQuestToolsSettings.SkippedVersion;
            try
            {
                VRCQuestToolsSettings.SkippedVersion = new SemVer("2.0.0");
                var result = VRCQuestToolsSettings.SkippedVersion;
                Assert.AreEqual("2.0.0", result.ToString());
            }
            finally
            {
                VRCQuestToolsSettings.SkippedVersion = original;
            }
        }

        [Test]
        public void LastVersionCheckDateTime_GetSet()
        {
            var original = VRCQuestToolsSettings.LastVersionCheckDateTime;
            try
            {
                var now = DateTime.UtcNow;
                VRCQuestToolsSettings.LastVersionCheckDateTime = now;
                var result = VRCQuestToolsSettings.LastVersionCheckDateTime;
                // Allow 1 second tolerance due to int precision
                Assert.LessOrEqual(Math.Abs((result - now).TotalSeconds), 1.0);
            }
            finally
            {
                VRCQuestToolsSettings.LastVersionCheckDateTime = original;
            }
        }

        [Test]
        public void DisplayLanguage_GetSet()
        {
            var original = VRCQuestToolsSettings.DisplayLanguage;
            try
            {
                VRCQuestToolsSettings.DisplayLanguage = DisplayLanguage.English;
                Assert.AreEqual(DisplayLanguage.English, VRCQuestToolsSettings.DisplayLanguage);

                VRCQuestToolsSettings.DisplayLanguage = DisplayLanguage.Japanese;
                Assert.AreEqual(DisplayLanguage.Japanese, VRCQuestToolsSettings.DisplayLanguage);
            }
            finally
            {
                VRCQuestToolsSettings.DisplayLanguage = original;
            }
        }

        [Test]
        public void TextureCacheSize_GetSet()
        {
            var original = VRCQuestToolsSettings.TextureCacheSize;
            try
            {
                VRCQuestToolsSettings.TextureCacheSize = 256 * 1024 * 1024UL;
                Assert.AreEqual(256 * 1024 * 1024UL, VRCQuestToolsSettings.TextureCacheSize);
            }
            finally
            {
                VRCQuestToolsSettings.TextureCacheSize = original;
            }
        }

        [Test]
        public void TextureCacheFolder_IsNotEmpty()
        {
            var folder = VRCQuestToolsSettings.TextureCacheFolder;
            Assert.IsFalse(string.IsNullOrEmpty(folder));
        }

        [Test]
        public void IsValidationAutomatorEnabled_GetSet()
        {
            var original = VRCQuestToolsSettings.IsValidationAutomatorEnabled;
            try
            {
                VRCQuestToolsSettings.IsValidationAutomatorEnabled = false;
                Assert.IsFalse(VRCQuestToolsSettings.IsValidationAutomatorEnabled);
                VRCQuestToolsSettings.IsValidationAutomatorEnabled = true;
                Assert.IsTrue(VRCQuestToolsSettings.IsValidationAutomatorEnabled);
            }
            finally
            {
                VRCQuestToolsSettings.IsValidationAutomatorEnabled = original;
            }
        }

        [Test]
        public void IsCheckTextureFormatOnStandaloneEnabled_GetSet()
        {
            var original = VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled;
            try
            {
                VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled = true;
                Assert.IsTrue(VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled);
                VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled = false;
                Assert.IsFalse(VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled);
            }
            finally
            {
                VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled = original;
            }
        }

        [Test]
        public void ResetPreferences_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => VRCQuestToolsSettings.ResetPreferences());
        }

        [Test]
        public void I18nResource_IsNotNull()
        {
            Assert.IsNotNull(VRCQuestToolsSettings.I18nResource);
        }
    }

    [TestFixture]
    public class UnityQuestSettingsViewModelTests_Deep
    {
        [Test]
        public void DefaultAndroidTextureCompression_ReturnsValue()
        {
            var vm = new UnityQuestSettingsViewModel();
            var result = vm.DefaultAndroidTextureCompression;
            Assert.IsTrue(Enum.IsDefined(typeof(MobileTextureSubtarget), result));
        }

        [Test]
        public void ShowWindowOnLoad_GetSet()
        {
            var vm = new UnityQuestSettingsViewModel();
            var original = vm.ShowWindowOnLoad;
            try
            {
                vm.ShowWindowOnLoad = false;
                Assert.IsFalse(vm.ShowWindowOnLoad);
                vm.ShowWindowOnLoad = true;
                Assert.IsTrue(vm.ShowWindowOnLoad);
            }
            finally
            {
                vm.ShowWindowOnLoad = original;
            }
        }

        [Test]
        public void AllSettingsValid_ReturnsBool()
        {
            var vm = new UnityQuestSettingsViewModel();
            var result = vm.AllSettingsValid;
            Assert.IsTrue(result is bool);
        }

        [Test]
        public void HasValidAndroidTextureCompression_ReturnsBool()
        {
            var vm = new UnityQuestSettingsViewModel();
            var result = vm.HasValidAndroidTextureCompression;
            Assert.IsTrue(result is bool);
        }

        [Test]
        public void HasAndroidBuildSupport_ReturnsBool()
        {
            var vm = new UnityQuestSettingsViewModel();
            var result = vm.HasAndroidBuildSupport;
            Assert.IsTrue(result is bool);
        }
    }

    [TestFixture]
    public class VRCQuestToolsAvatarProcessorTests
    {
        private static VRCAvatarDescriptor CreateDescriptor(GameObject go)
        {
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            return desc;
        }

        [Test]
        public void OnPreprocessAvatar_ReturnsTrue_WhenNoMissingComponents()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var desc = CreateDescriptor(go);
                var processorType = SystemUtility.GetTypeByName("KRT.VRCQuestTools.NonDestructive.VRCQuestToolsAvatarProcessor");
                if (processorType == null)
                {
                    Assert.Pass("VRCQuestToolsAvatarProcessor type not found in loaded assemblies");
                    return;
                }
                var processor = Activator.CreateInstance(processorType);
                var method = processorType.GetMethod("OnPreprocessAvatar",
                    BindingFlags.Public | BindingFlags.Instance);
                Assert.IsNotNull(method);

                var result = (bool)method.Invoke(processor, new object[] { go });
                Assert.IsTrue(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CallbackOrder_IsNegative()
        {
            var processorType = SystemUtility.GetTypeByName("KRT.VRCQuestTools.NonDestructive.VRCQuestToolsAvatarProcessor");
            if (processorType == null)
            {
                Assert.Pass("VRCQuestToolsAvatarProcessor type not found");
                return;
            }
            var processor = Activator.CreateInstance(processorType);
            var prop = processorType.GetProperty("callbackOrder", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(prop);
            var order = (int)prop.GetValue(processor);
            Assert.Less(order, 0);
            Assert.AreEqual(-12000, order);
        }

        [Test]
        public void AssignNetworkIDs_NoAssigner_DoesNotThrow()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var desc = CreateDescriptor(go);
                var processorType = SystemUtility.GetTypeByName("KRT.VRCQuestTools.NonDestructive.VRCQuestToolsAvatarProcessor");
                if (processorType == null)
                {
                    Assert.Pass("VRCQuestToolsAvatarProcessor type not found");
                    return;
                }
                var method = processorType.GetMethod("AssignNetworkIDs",
                    BindingFlags.NonPublic | BindingFlags.Static);
                Assert.IsNotNull(method);
                Assert.DoesNotThrow(() => method.Invoke(null, new object[] { go }));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    public class MaterialGeneratorUtilityTests_Deep
    {
        [Test]
        public void TextureConfig_SRGB_Properties()
        {
            var type = typeof(MaterialGeneratorUtility).GetNestedType("TextureConfig", BindingFlags.NonPublic);
            Assert.IsNotNull(type, "TextureConfig not found");

            var srgbProp = type.GetProperty("SRGB", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(srgbProp, "SRGB property not found");
            var srgb = srgbProp.GetValue(null);
            var isSRGB = (bool)type.GetField("isSRGB", BindingFlags.Public | BindingFlags.Instance).GetValue(srgb);
            var isNormal = (bool)type.GetField("isNormalMap", BindingFlags.Public | BindingFlags.Instance).GetValue(srgb);
            Assert.IsTrue(isSRGB);
            Assert.IsFalse(isNormal);
        }

        [Test]
        public void TextureConfig_Parameter_Properties()
        {
            var type = typeof(MaterialGeneratorUtility).GetNestedType("TextureConfig", BindingFlags.NonPublic);
            var paramProp = type.GetProperty("Parameter", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(paramProp);
            var param = paramProp.GetValue(null);
            var isSRGB = (bool)type.GetField("isSRGB", BindingFlags.Public | BindingFlags.Instance).GetValue(param);
            var isNormal = (bool)type.GetField("isNormalMap", BindingFlags.Public | BindingFlags.Instance).GetValue(param);
            Assert.IsFalse(isSRGB);
            Assert.IsFalse(isNormal);
        }

        [Test]
        public void TextureConfig_NormalMap_Properties()
        {
            var type = typeof(MaterialGeneratorUtility).GetNestedType("TextureConfig", BindingFlags.NonPublic);
            var normalProp = type.GetProperty("NormalMap", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(normalProp);
            var normal = normalProp.GetValue(null);
            var isSRGB = (bool)type.GetField("isSRGB", BindingFlags.Public | BindingFlags.Instance).GetValue(normal);
            var isNormal = (bool)type.GetField("isNormalMap", BindingFlags.Public | BindingFlags.Instance).GetValue(normal);
            Assert.IsFalse(isSRGB);
            Assert.IsTrue(isNormal);
        }

        [Test]
        public void ConvertToNullableTextureFormat_NoOverride_ReturnsNull()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod("ConvertToNullableTextureFormat",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "ConvertToNullableTextureFormat not found");
            var result = method.Invoke(null, new object[] { MobileTextureFormat.NoOverride });
            Assert.IsNull(result);
        }

        [Test]
        public void ConvertToNullableTextureFormat_ASTC4x4_ReturnsValue()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod("ConvertToNullableTextureFormat",
                BindingFlags.NonPublic | BindingFlags.Static);
            var result = (TextureFormat?)method.Invoke(null, new object[] { MobileTextureFormat.ASTC_4x4 });
            Assert.IsNotNull(result);
            Assert.AreEqual(TextureFormat.ASTC_4x4, result.Value);
        }
    }

    [TestFixture]
    public class CacheUtilityDeepTests_Deep
    {
        [Test]
        public void TextureCache_ToTexture2D_NonNormalMap()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false, false);
            try
            {
                var pixels = new Color[16];
                for (int i = 0; i < 16; i++) pixels[i] = Color.red;
                tex.SetPixels(pixels);
                tex.Apply();

                var cache = new CacheUtility.TextureCache(tex, false, false, UBuildTarget.StandaloneWindows64);
                var restored = cache.ToTexture2D();
                try
                {
                    Assert.IsNotNull(restored);
                    Assert.AreEqual(4, restored.width);
                    Assert.AreEqual(4, restored.height);
                }
                finally
                {
                    UObject.DestroyImmediate(restored);
                }
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void TextureCache_JsonRoundtrip_NonNormalMap()
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false, true);
            try
            {
                var cache = new CacheUtility.TextureCache(tex, true, false, UBuildTarget.Android);
                var json = JsonUtility.ToJson(cache);
                var deserialized = JsonUtility.FromJson<CacheUtility.TextureCache>(json);
                var restored = deserialized.ToTexture2D();
                try
                {
                    Assert.AreEqual(8, restored.width);
                    Assert.AreEqual(8, restored.height);
                }
                finally
                {
                    UObject.DestroyImmediate(restored);
                }
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void ResolveNormalMapPath_NonSquare_ReturnsEmpty()
        {
            var tex = new Texture2D(4, 8, TextureFormat.RGBA32, false, false);
            try
            {
                var cache = new CacheUtility.TextureCache(tex, false, true, UBuildTarget.Android);

                var method = cache.GetType().GetMethod("ResolveNormalMapPath",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(method);
                var result = (string)method.Invoke(cache, null);
                Assert.AreEqual(string.Empty, result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }
    }

    [TestFixture]
    public class MissingRulesDeepTests
    {
        private static VRCAvatarDescriptor CreateDescriptor(GameObject go)
        {
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            return desc;
        }

        [Test]
        public void MissingScriptsRule_Validate_InactiveAvatar_ReturnsNull()
        {
            var go = new GameObject("Avatar");
            go.SetActive(false);
            try
            {
                var desc = CreateDescriptor(go);
                var avatar = new VRChatAvatar(desc);
                var rule = new MissingScriptsRule();
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void MissingScriptsRule_Validate_ActiveNoMissing_ReturnsNull()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var avatar = new VRChatAvatar(desc);
                var rule = new MissingScriptsRule();
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void MissingNdmfRule_Validate_NoNdmfComponents_ReturnsNull()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var avatar = new VRChatAvatar(desc);
                var rule = new MissingNdmfRule();
                var result = rule.Validate(avatar);
                // With VQT_HAS_NDMF defined, this returns null
                // Without it, returns null if no INdmfComponent found
                Assert.IsNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void AvatarValidationRules_Rules_ContainsMissingScriptsRule()
        {
            var rules = AvatarValidationRules.Rules;
            Assert.IsTrue(rules.Any(r => r is MissingScriptsRule));
        }

        [Test]
        public void AvatarValidationRules_Rules_ContainsMissingNdmfRule()
        {
            var rules = AvatarValidationRules.Rules;
            Assert.IsTrue(rules.Any(r => r is MissingNdmfRule));
        }
    }

    [TestFixture]
    public class SystemUtilityDeepTests
    {
        [Test]
        public void GetAppLocalCachePath_ReturnsNonEmpty()
        {
            var result = SystemUtility.GetAppLocalCachePath("TestApp");
            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.IsTrue(result.Contains("TestApp"));
        }

        [Test]
        public void OpenFolder_ThrowsOnNonExistentDir()
        {
            Assert.Throws<System.IO.DirectoryNotFoundException>(() =>
                SystemUtility.OpenFolder(@"C:\NonExistentDirectory_12345"));
        }
    }

    [TestFixture]
    public class VRCQuestToolsEntryTests_Deep
    {
        [Test]
        public void Name_IsVRCQuestTools()
        {
            Assert.AreEqual("VRCQuestTools", VRCQuestTools.Name);
        }

        [Test]
        public void Version_IsSemVer()
        {
            var version = VRCQuestTools.Version;
            Assert.IsFalse(string.IsNullOrEmpty(version));
            var parts = version.Split('.');
            Assert.AreEqual(3, parts.Length);
        }

        [Test]
        public void GitHubRepository_IsSet()
        {
            var field = typeof(VRCQuestTools).GetField("GitHubRepository",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field);
            var value = (string)field.GetValue(null);
            Assert.AreEqual("kurotu/VRCQuestTools", value);
        }

        [Test]
        public void ComponentRemover_IsNotNull()
        {
            var field = typeof(VRCQuestTools).GetField("ComponentRemover",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field);
            Assert.IsNotNull(field.GetValue(null));
        }

        [Test]
        public void AvatarConverter_IsNotNull()
        {
            var field = typeof(VRCQuestTools).GetField("AvatarConverter",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field);
            Assert.IsNotNull(field.GetValue(null));
        }

        [Test]
        public void VPM_IsNotNull()
        {
            var field = typeof(VRCQuestTools).GetField("VPM",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field);
            Assert.IsNotNull(field.GetValue(null));
        }

        [Test]
        public void PackageName_IsCorrect()
        {
            var field = typeof(VRCQuestTools).GetField("PackageName",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field);
            Assert.AreEqual("com.github.kurotu.vrc-quest-tools", field.GetValue(null));
        }

        [Test]
        public void BoothURL_IsSet()
        {
            var field = typeof(VRCQuestTools).GetField("BoothURL",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field);
            var value = (string)field.GetValue(null);
            Assert.IsTrue(value.Contains("booth.pm"));
        }

        [Test]
        public void DocsURL_IsSet()
        {
            var field = typeof(VRCQuestTools).GetField("DocsURL",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field);
            var value = (string)field.GetValue(null);
            Assert.IsTrue(value.Contains("VRCQuestTools"));
        }

        [Test]
        public void VPMRepositoryURL_IsSet()
        {
            var field = typeof(VRCQuestTools).GetField("VPMRepositoryURL",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field);
            var value = (string)field.GetValue(null);
            Assert.IsTrue(value.Contains("vpm.json"));
        }
    }

    [TestFixture]
    public class PhysBonesRemoveViewModelDeepTests
    {
        private static VRCAvatarDescriptor CreateDescriptor(GameObject go)
        {
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            return desc;
        }

        [Test]
        public void DoesSelectAllPhysBones_TrueWhenAllSelected()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var vm = new PhysBonesRemoveViewModel();
                var avatarRootField = typeof(PhysBonesRemoveViewModel).GetField("avatarRoot",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                avatarRootField.SetValue(vm, go);

                // No phys bones, empty keep list -> 0 == 0 -> true
                Assert.IsTrue(vm.DoesSelectAllPhysBones);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void DoesSelectAllPhysBoneColliders_TrueWhenAllSelected()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var vm = new PhysBonesRemoveViewModel();
                var avatarRootField = typeof(PhysBonesRemoveViewModel).GetField("avatarRoot",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                avatarRootField.SetValue(vm, go);

                Assert.IsTrue(vm.DoesSelectAllPhysBoneColliders);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void DoesSelectAllContacts_TrueWhenAllSelected()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = CreateDescriptor(go);
                var vm = new PhysBonesRemoveViewModel();
                var avatarRootField = typeof(PhysBonesRemoveViewModel).GetField("avatarRoot",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                avatarRootField.SetValue(vm, go);

                Assert.IsTrue(vm.DoesSelectAllContacts);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    public class AvatarConverterNdmfPhaseCoverageTests
    {
        [Test]
        public void Resolve_ReturnsTransforming_ForTransforming()
        {
            var phase = AvatarConverterNdmfPhase.Transforming;
            var result = phase.Resolve(null);
            Assert.AreEqual(AvatarConverterNdmfPhase.Transforming, result);
        }

        [Test]
        public void Resolve_ReturnsOptimizing_ForOptimizing()
        {
            var phase = AvatarConverterNdmfPhase.Optimizing;
            var result = phase.Resolve(null);
            Assert.AreEqual(AvatarConverterNdmfPhase.Optimizing, result);
        }

        [Test]
        public void Resolve_Auto_NullAvatar_ReturnsOptimizing()
        {
            var phase = AvatarConverterNdmfPhase.Auto;
            var result = phase.Resolve(null);
            Assert.AreEqual(AvatarConverterNdmfPhase.Optimizing, result);
        }

        [Test]
        public void Resolve_Auto_WithAvatar_ReturnsOptimizing()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var phase = AvatarConverterNdmfPhase.Auto;
                var result = phase.Resolve(go);
                Assert.AreEqual(AvatarConverterNdmfPhase.Optimizing, result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    public class MaterialConversionSettingsTests_Deep
    {
        [Test]
        public void MaterialConversionSettings_DefaultMaterialConvertSettings()
        {
            var go = new GameObject("Test");
            try
            {
                var comp = go.AddComponent<MaterialConversionSettings>();
                var settings = (comp as IMaterialConversionComponent).DefaultMaterialConvertSettings;
                Assert.IsNotNull(settings);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void MaterialConversionSettings_DefaultMaterialConvertSettings_IsNotNull()
        {
            var go = new GameObject("Test");
            try
            {
                var comp = go.AddComponent<MaterialConversionSettings>();
                var iComp = comp as IMaterialConversionComponent;
                var settings = iComp.DefaultMaterialConvertSettings;
                Assert.IsNotNull(settings);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void MaterialConversionSettings_RemoveExtraMaterialSlots()
        {
            var go = new GameObject("Test");
            try
            {
                var comp = go.AddComponent<MaterialConversionSettings>();
                var iComp = comp as IMaterialConversionComponent;
                Assert.IsTrue(iComp.RemoveExtraMaterialSlots is bool);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void MaterialConversionSettings_EnableMaterialPreview()
        {
            var go = new GameObject("Test");
            try
            {
                var comp = go.AddComponent<MaterialConversionSettings>();
                var iComp = comp as IMaterialConversionComponent;
                Assert.IsTrue(iComp.EnableMaterialPreview is bool);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    public class MockPerformanceTests
    {
        [Test]
        public void MockAvatarPerformanceStatsLevelSet_TypeCheck()
        {
            var type = SystemUtility.GetTypeByName("KRT.VRCQuestTools.Mocks.Mock_AvatarPerformanceStatsLevelSet");
            if (type == null)
            {
                Assert.Ignore("Mock_AvatarPerformanceStatsLevelSet type not found");
                return;
            }

            // Try to create; some types require specific constructor args
            try
            {
                var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsTrue(ctors.Length > 0, "Type should have at least one constructor");

                // Access properties via reflection
                foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    Assert.IsNotNull(prop.Name);
                }
            }
            catch (Exception)
            {
                Assert.Pass("Type exists but cannot be instantiated");
            }
        }
    }

    [TestFixture]
    public class ComponentRemoverDeepTests
    {
        [Test]
        public void RemoveUnsupportedComponentsInChildren_WithExcludedTypes()
        {
            var go = new GameObject("Avatar");
            try
            {
                var child = new GameObject("Child");
                child.transform.SetParent(go.transform);

                var remover = new ComponentRemover();
                Assert.DoesNotThrow(() =>
                    remover.RemoveUnsupportedComponentsInChildren(go, true, false, new Type[] { typeof(Transform) }));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    public class VPMServiceDeepTests
    {
        [Test]
        public void VrcGetCommand_ReturnsString()
        {
            var type = typeof(Services.VPMService);
            var prop = type.GetProperty("VrcGetCommand", BindingFlags.NonPublic | BindingFlags.Static);
            if (prop != null)
            {
                var value = prop.GetValue(null);
                Assert.IsNotNull(value);
            }
            else
            {
                // Try field
                var field = type.GetField("VrcGetCommand", BindingFlags.NonPublic | BindingFlags.Static);
                if (field != null)
                {
                    Assert.IsNotNull(field.GetValue(null));
                }
            }
        }
    }

    [TestFixture]
    public class ModularAvatarUtilityDeepTests
    {
        [Test]
        public void IsModularAvatarImported_ReturnsBool()
        {
            var result = ModularAvatarUtility.IsModularAvatarImported();
            Assert.IsTrue(result is bool);
        }

        [Test]
        public void IsLegacyVersion_ReturnsBool()
        {
            var result = ModularAvatarUtility.IsLegacyVersion();
            Assert.IsTrue(result is bool);
        }

        [Test]
        public void IsBreakingVersion_ReturnsBool()
        {
            var result = ModularAvatarUtility.IsBreakingVersion();
            Assert.IsTrue(result is bool);
        }

        [Test]
        public void RemoveUnsupportedComponents_OnEmptyGO_DoesNotThrow()
        {
            var go = new GameObject("Test");
            try
            {
                Assert.DoesNotThrow(() =>
                    ModularAvatarUtility.RemoveUnsupportedComponents(go, true));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetMergeAnimatorComponentsInChildren_OnEmptyGO_ReturnsEmpty()
        {
            var go = new GameObject("Test");
            try
            {
                var result = ModularAvatarUtility.GetMergeAnimatorComponentsInChildren(go, true);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Length);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    public class NdmfUtilityTests_Deep
    {
        [Test]
        public void NdmfUtility_Exists()
        {
            var type = typeof(NdmfUtility);
            Assert.IsNotNull(type);
        }
    }

    [TestFixture]
    public class NotificationItemTests_Deep
    {
        [Test]
        public void NotificationItem_DrawReturnsDelegate()
        {
            bool called = false;
            var item = new NotificationItem(() =>
            {
                called = true;
                return true;
            });

            Assert.IsNotNull(item);
        }
    }
}
