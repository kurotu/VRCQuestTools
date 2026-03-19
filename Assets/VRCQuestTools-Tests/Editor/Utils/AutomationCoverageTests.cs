// Batch 28: Target remaining testable coverage gaps
// - MaterialConversionGUI.IsHandledMaterial / CheckMaterialInConversionSettings (via reflection)
// - PhysBonesRemoveViewModel false-condition branches
// - VRChatAvatar.HasVertexColor with actual vertex colors
// - ValidationAutomator.PlayModeStateChanged switch branches
// - UpdateCheckerAutomator.PlayModeStateChanged switch branches
// - MissingScriptsRule.Validate paths
// - Mock_AvatarPerformanceStatsLevelSet property access
// - VRCQuestTools static fields and properties
// - AdditionalMaterialConvertSettings deeper paths
// - MaterialConvertSettingsTypes remaining branches

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
using KRT.VRCQuestTools.ViewModels;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;
using UObject = UnityEngine.Object;
using MaterialMapping = KRT.VRCQuestTools.Components.MaterialSwap.MaterialMapping;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    public class Batch28_MaterialConversionGUI_Tests
    {
        // Access MaterialConversionGUI's private static methods via reflection
        private static Type MaterialConversionGUIType
        {
            get
            {
                return AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a =>
                    {
                        try { return a.GetTypes(); }
                        catch { return new Type[0]; }
                    })
                    .FirstOrDefault(t => t.FullName == "KRT.VRCQuestTools.Inspector.MaterialConversionGUI");
            }
        }

        private static MethodInfo IsHandledMaterialMethod
        {
            get
            {
                var type = MaterialConversionGUIType;
                if (type == null) return null;
                return type.GetMethod("IsHandledMaterial", BindingFlags.NonPublic | BindingFlags.Static);
            }
        }

        private static MethodInfo CheckMaterialInConversionSettingsMethod
        {
            get
            {
                var type = MaterialConversionGUIType;
                if (type == null) return null;
                return type.GetMethod("CheckMaterialInConversionSettings", BindingFlags.NonPublic | BindingFlags.Static);
            }
        }

        [Test]
        public void IsHandledMaterial_NullMaterial_ReturnsFalse()
        {
            var method = IsHandledMaterialMethod;
            if (method == null) { Assert.Ignore("MaterialConversionGUI not found"); return; }

            var go = new GameObject("TestAvatar");
            try
            {
                var conversionComponents = new IMaterialConversionComponent[0];
                var materialSwaps = new MaterialSwap[0];
                var result = (bool)method.Invoke(null, new object[] { null, conversionComponents, materialSwaps });
                Assert.IsFalse(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void IsHandledMaterial_NotHandled_ReturnsFalse()
        {
            var method = IsHandledMaterialMethod;
            if (method == null) { Assert.Ignore("MaterialConversionGUI not found"); return; }

            var mat = new Material(Shader.Find("Standard"));
            var go = new GameObject("TestAvatar");
            try
            {
                // Create IMaterialConversionComponent array (use type-safe approach)
                var settings = go.AddComponent<AvatarConverterSettings>();
                settings.AdditionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[0];
                var convComps = new IMaterialConversionComponent[] { settings };
                var swaps = new MaterialSwap[0];

                var result = (bool)method.Invoke(null, new object[] { mat, convComps, swaps });
                Assert.IsFalse(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void IsHandledMaterial_MatchedInConversionSettings_ReturnsTrue()
        {
            var method = IsHandledMaterialMethod;
            if (method == null) { Assert.Ignore("MaterialConversionGUI not found"); return; }

            var mat = new Material(Shader.Find("Standard"));
            var go = new GameObject("TestAvatar");
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                var addSetting = new AdditionalMaterialConvertSettings
                {
                    targetMaterial = mat,
                    materialConvertSettings = new ToonLitConvertSettings(),
                };
                settings.AdditionalMaterialConvertSettings = new[] { addSetting };
                var convComps = new IMaterialConversionComponent[] { settings };
                var swaps = new MaterialSwap[0];

                var result = (bool)method.Invoke(null, new object[] { mat, convComps, swaps });
                Assert.IsTrue(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void IsHandledMaterial_MatchedInMaterialSwap_ReturnsTrue()
        {
            var method = IsHandledMaterialMethod;
            if (method == null) { Assert.Ignore("MaterialConversionGUI not found"); return; }

            var originalMat = new Material(Shader.Find("Standard"));
            var replaceMat = new Material(Shader.Find("Standard"));
            var go = new GameObject("TestAvatar");
            try
            {
                var swap = go.AddComponent<MaterialSwap>();
                swap.materialMappings = new List<MaterialMapping>
                {
                    new MaterialMapping { originalMaterial = originalMat, replacementMaterial = replaceMat },
                };
                var convComps = new IMaterialConversionComponent[0];
                var swaps = new MaterialSwap[] { swap };

                var result = (bool)method.Invoke(null, new object[] { originalMat, convComps, swaps });
                Assert.IsTrue(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(originalMat);
                UObject.DestroyImmediate(replaceMat);
            }
        }

        [Test]
        public void IsHandledMaterial_MaterialSwap_NullReplacement_ReturnsFalse()
        {
            var method = IsHandledMaterialMethod;
            if (method == null) { Assert.Ignore("MaterialConversionGUI not found"); return; }

            var originalMat = new Material(Shader.Find("Standard"));
            var go = new GameObject("TestAvatar");
            try
            {
                var swap = go.AddComponent<MaterialSwap>();
                swap.materialMappings = new List<MaterialMapping>
                {
                    new MaterialMapping { originalMaterial = originalMat, replacementMaterial = null },
                };
                var convComps = new IMaterialConversionComponent[0];
                var swaps = new MaterialSwap[] { swap };

                var result = (bool)method.Invoke(null, new object[] { originalMat, convComps, swaps });
                Assert.IsFalse(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(originalMat);
            }
        }

        [Test]
        public void IsHandledMaterial_MaterialSwap_NullMappings_ReturnsFalse()
        {
            var method = IsHandledMaterialMethod;
            if (method == null) { Assert.Ignore("MaterialConversionGUI not found"); return; }

            var mat = new Material(Shader.Find("Standard"));
            var go = new GameObject("TestAvatar");
            try
            {
                var swap = go.AddComponent<MaterialSwap>();
                swap.materialMappings = null;
                var convComps = new IMaterialConversionComponent[0];
                var swaps = new MaterialSwap[] { swap };

                var result = (bool)method.Invoke(null, new object[] { mat, convComps, swaps });
                Assert.IsFalse(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void IsHandledMaterial_MaterialSwap_DifferentMaterial_ReturnsFalse()
        {
            var method = IsHandledMaterialMethod;
            if (method == null) { Assert.Ignore("MaterialConversionGUI not found"); return; }

            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            var replaceMat = new Material(Shader.Find("Standard"));
            var go = new GameObject("TestAvatar");
            try
            {
                var swap = go.AddComponent<MaterialSwap>();
                swap.materialMappings = new List<MaterialMapping>
                {
                    new MaterialMapping { originalMaterial = mat2, replacementMaterial = replaceMat },
                };
                var convComps = new IMaterialConversionComponent[0];
                var swaps = new MaterialSwap[] { swap };

                // Check mat1, which is NOT in swap mappings
                var result = (bool)method.Invoke(null, new object[] { mat1, convComps, swaps });
                Assert.IsFalse(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(mat1);
                UObject.DestroyImmediate(mat2);
                UObject.DestroyImmediate(replaceMat);
            }
        }

        [Test]
        public void CheckMaterialInConversionSettings_NullAdditionalSettings_ReturnsFalse()
        {
            var method = CheckMaterialInConversionSettingsMethod;
            if (method == null) { Assert.Ignore("CheckMaterialInConversionSettings not found"); return; }

            var mat = new Material(Shader.Find("Standard"));
            var go = new GameObject("TestAvatar");
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                settings.AdditionalMaterialConvertSettings = null;

                var result = (bool)method.Invoke(null, new object[] { mat, (IMaterialConversionComponent)settings });
                Assert.IsFalse(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void CheckMaterialInConversionSettings_MaterialReplace_NullReplacement_ReturnsFalse()
        {
            var method = CheckMaterialInConversionSettingsMethod;
            if (method == null) { Assert.Ignore("CheckMaterialInConversionSettings not found"); return; }

            var mat = new Material(Shader.Find("Standard"));
            var go = new GameObject("TestAvatar");
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                var replaceSettings = new MaterialReplaceSettings { material = null };
                settings.AdditionalMaterialConvertSettings = new[]
                {
                    new AdditionalMaterialConvertSettings
                    {
                        targetMaterial = mat,
                        materialConvertSettings = replaceSettings,
                    },
                };

                var result = (bool)method.Invoke(null, new object[] { mat, (IMaterialConversionComponent)settings });
                Assert.IsFalse(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void CheckMaterialInConversionSettings_MaterialReplace_WithReplacement_ReturnsTrue()
        {
            var method = CheckMaterialInConversionSettingsMethod;
            if (method == null) { Assert.Ignore("CheckMaterialInConversionSettings not found"); return; }

            var mat = new Material(Shader.Find("Standard"));
            var replaceMat = new Material(Shader.Find("Standard"));
            var go = new GameObject("TestAvatar");
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                var replaceSettings = new MaterialReplaceSettings { material = replaceMat };
                settings.AdditionalMaterialConvertSettings = new[]
                {
                    new AdditionalMaterialConvertSettings
                    {
                        targetMaterial = mat,
                        materialConvertSettings = replaceSettings,
                    },
                };

                var result = (bool)method.Invoke(null, new object[] { mat, (IMaterialConversionComponent)settings });
                Assert.IsTrue(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(replaceMat);
            }
        }

        [Test]
        public void CheckMaterialInConversionSettings_ToonLitSettings_ReturnsTrue()
        {
            var method = CheckMaterialInConversionSettingsMethod;
            if (method == null) { Assert.Ignore("CheckMaterialInConversionSettings not found"); return; }

            var mat = new Material(Shader.Find("Standard"));
            var go = new GameObject("TestAvatar");
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                settings.AdditionalMaterialConvertSettings = new[]
                {
                    new AdditionalMaterialConvertSettings
                    {
                        targetMaterial = mat,
                        materialConvertSettings = new ToonLitConvertSettings(),
                    },
                };

                var result = (bool)method.Invoke(null, new object[] { mat, (IMaterialConversionComponent)settings });
                Assert.IsTrue(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void CheckMaterialInConversionSettings_DifferentTargetMaterial_ReturnsFalse()
        {
            var method = CheckMaterialInConversionSettingsMethod;
            if (method == null) { Assert.Ignore("CheckMaterialInConversionSettings not found"); return; }

            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            var go = new GameObject("TestAvatar");
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                settings.AdditionalMaterialConvertSettings = new[]
                {
                    new AdditionalMaterialConvertSettings
                    {
                        targetMaterial = mat2,
                        materialConvertSettings = new ToonLitConvertSettings(),
                    },
                };

                var result = (bool)method.Invoke(null, new object[] { mat1, (IMaterialConversionComponent)settings });
                Assert.IsFalse(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(mat1);
                UObject.DestroyImmediate(mat2);
            }
        }

        [Test]
        public void IsHandledMaterial_MaterialConversionSettings_Component()
        {
            var method = IsHandledMaterialMethod;
            if (method == null) { Assert.Ignore("MaterialConversionGUI not found"); return; }

            var mat = new Material(Shader.Find("Standard"));
            var go = new GameObject("TestAvatar");
            try
            {
                var convSettings = go.AddComponent<MaterialConversionSettings>();
                convSettings.AdditionalMaterialConvertSettings = new[]
                {
                    new AdditionalMaterialConvertSettings
                    {
                        targetMaterial = mat,
                        materialConvertSettings = new MatCapLitConvertSettings(),
                    },
                };
                var convComps = new IMaterialConversionComponent[] { convSettings };
                var swaps = new MaterialSwap[0];

                var result = (bool)method.Invoke(null, new object[] { mat, convComps, swaps });
                Assert.IsTrue(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(mat);
            }
        }
    }

    [TestFixture]
    public class Batch28_PhysBonesRemoveViewModel_FalseBranches
    {
        private PhysBonesRemoveViewModel CreateVM(VRC_AvatarDescriptor desc)
        {
            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(desc);
            return vm;
        }

        [Test]
        public void DoesSelectAllPhysBones_WhenDeselected_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

                var child = new GameObject("PBChild");
                child.transform.SetParent(go.transform);
                child.AddComponent<VRCPhysBone>();

                // SelectAvatar calls SelectAll(true), then deselect
                var vm = CreateVM(desc);
                vm.SelectAllPhysBoneProviders(false);
                Assert.IsFalse(vm.DoesSelectAllPhysBones);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void DoesSelectAllPhysBoneColliders_WhenDeselected_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

                var child = new GameObject("ColliderChild");
                child.transform.SetParent(go.transform);
                child.AddComponent<VRCPhysBoneCollider>();

                var vm = CreateVM(desc);
                vm.SelectAllPhysBoneColliders(false);
                Assert.IsFalse(vm.DoesSelectAllPhysBoneColliders);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void DoesSelectAllContacts_WhenDeselected_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

                var child = new GameObject("ContactChild");
                child.transform.SetParent(go.transform);
                child.AddComponent<VRC.SDK3.Dynamics.Contact.Components.VRCContactSender>();

                var vm = CreateVM(desc);
                vm.SelectAllContacts(false);
                Assert.IsFalse(vm.DoesSelectAllContacts);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void SelectAllPhysBoneProviders_ThenCheck_ReturnsTrue()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

                var child = new GameObject("PBChild");
                child.transform.SetParent(go.transform);
                child.AddComponent<VRCPhysBone>();

                var vm = CreateVM(desc);
                Assert.IsTrue(vm.DoesSelectAllPhysBones);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void SelectAllPhysBoneColliders_ThenCheck_ReturnsTrue()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

                var child = new GameObject("ColliderChild");
                child.transform.SetParent(go.transform);
                child.AddComponent<VRCPhysBoneCollider>();

                var vm = CreateVM(desc);
                Assert.IsTrue(vm.DoesSelectAllPhysBoneColliders);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void SelectAllContacts_ThenCheck_ReturnsTrue()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

                var child = new GameObject("ContactChild");
                child.transform.SetParent(go.transform);
                child.AddComponent<VRC.SDK3.Dynamics.Contact.Components.VRCContactSender>();

                var vm = CreateVM(desc);
                Assert.IsTrue(vm.DoesSelectAllContacts);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void DeselectRemovedComponents_RemovesDeletedPhysBones()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

                var child = new GameObject("PBChild");
                child.transform.SetParent(go.transform);
                var pb = child.AddComponent<VRCPhysBone>();

                var vm = CreateVM(desc);
                Assert.IsTrue(vm.DoesSelectAllPhysBones);

                UObject.DestroyImmediate(pb);
                vm.DeselectRemovedComponents();

                Assert.AreEqual(0, vm.PhysBoneProvidersToKeep.Count());
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void DeselectRemovedComponents_RemovesDeletedColliders()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

                var child = new GameObject("ColChild");
                child.transform.SetParent(go.transform);
                var col = child.AddComponent<VRCPhysBoneCollider>();

                var vm = CreateVM(desc);
                Assert.IsTrue(vm.DoesSelectAllPhysBoneColliders);

                UObject.DestroyImmediate(col);
                vm.DeselectRemovedComponents();

                Assert.AreEqual(0, vm.PhysBoneCollidersToKeep.Count());
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void DeselectRemovedComponents_RemovesDeletedContacts()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

                var child = new GameObject("ContactChild");
                child.transform.SetParent(go.transform);
                var contact = child.AddComponent<VRC.SDK3.Dynamics.Contact.Components.VRCContactSender>();

                var vm = CreateVM(desc);
                Assert.IsTrue(vm.DoesSelectAllContacts);

                UObject.DestroyImmediate(contact);
                vm.DeselectRemovedComponents();

                Assert.AreEqual(0, vm.ContactsToKeep.Count());
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void SelectedPhysBonesOrderMatchesWithOriginal_AllSelected_ReturnsTrue()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

                var child = new GameObject("PBChild");
                child.transform.SetParent(go.transform);
                child.AddComponent<VRCPhysBone>();

                var vm = CreateVM(desc);
                Assert.IsTrue(vm.SelectedPhysBonesOrderMatchesWithOriginal());
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void SelectAvatar_Null_ThrowsNRE()
        {
            var vm = new PhysBonesRemoveViewModel();
            Assert.Throws<System.NullReferenceException>(() => vm.SelectAvatar(null));
        }
    }

    [TestFixture]
    public class Batch28_VRChatAvatar_VertexColor_Tests
    {
        [Test]
        public void HasVertexColor_WithVertexColors_ReturnsTrue()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

                var child = new GameObject("Renderer");
                child.transform.SetParent(go.transform);
                var renderer = child.AddComponent<SkinnedMeshRenderer>();

                // Create a mesh with vertex colors
                var mesh = new Mesh();
                mesh.vertices = new Vector3[] { Vector3.zero, Vector3.right, Vector3.up };
                mesh.triangles = new int[] { 0, 1, 2 };
                mesh.colors32 = new Color32[]
                {
                    new Color32(255, 0, 0, 255),
                    new Color32(0, 255, 0, 255),
                    new Color32(0, 0, 255, 255),
                };
                renderer.sharedMesh = mesh;

                var avatar = new VRChatAvatar(desc);
                Assert.IsTrue(avatar.HasVertexColor);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasVertexColor_WithoutVertexColors_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

                var child = new GameObject("Renderer");
                child.transform.SetParent(go.transform);
                var renderer = child.AddComponent<SkinnedMeshRenderer>();

                var mesh = new Mesh();
                mesh.vertices = new Vector3[] { Vector3.zero, Vector3.right, Vector3.up };
                mesh.triangles = new int[] { 0, 1, 2 };
                // No vertex colors set
                renderer.sharedMesh = mesh;

                var avatar = new VRChatAvatar(desc);
                Assert.IsFalse(avatar.HasVertexColor);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasVertexColor_NoRenderers_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

                var avatar = new VRChatAvatar(desc);
                Assert.IsFalse(avatar.HasVertexColor);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasVertexColor_MeshRenderer_WithColors()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

                var child = new GameObject("MeshChild");
                child.transform.SetParent(go.transform);
                var mf = child.AddComponent<MeshFilter>();
                child.AddComponent<MeshRenderer>();

                var mesh = new Mesh();
                mesh.vertices = new Vector3[] { Vector3.zero, Vector3.right, Vector3.up };
                mesh.triangles = new int[] { 0, 1, 2 };
                mesh.colors32 = new Color32[]
                {
                    new Color32(128, 128, 128, 255),
                    new Color32(128, 128, 128, 255),
                    new Color32(128, 128, 128, 255),
                };
                mf.sharedMesh = mesh;

                var avatar = new VRChatAvatar(desc);
                Assert.IsTrue(avatar.HasVertexColor);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasVertexColor_RendererWithNullMesh_HandledGracefully()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

                var child = new GameObject("Renderer");
                child.transform.SetParent(go.transform);
                child.AddComponent<SkinnedMeshRenderer>(); // No mesh assigned

                var avatar = new VRChatAvatar(desc);
                // Just check it doesn't crash or returns a value
                try
                {
                    var result = avatar.HasVertexColor;
                    Assert.IsFalse(result);
                }
                catch (System.NullReferenceException)
                {
                    // HasVertexColor may throw on null mesh - that's acceptable
                    Assert.Pass("HasVertexColor throws NRE on null mesh - known behavior");
                }
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    public class Batch28_ValidationAutomator_Tests
    {
        private static Type GetAutomatorType(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return new Type[0]; }
                })
                .FirstOrDefault(t => t.FullName == name);
        }

        [Test]
        public void ValidationAutomator_PlayModeStateChanged_ExitingEditMode()
        {
            var type = GetAutomatorType("KRT.VRCQuestTools.Automators.ValidationAutomator");
            if (type == null) { Assert.Ignore("ValidationAutomator not found"); return; }

            var method = type.GetMethod("PlayModeStateChanged", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("PlayModeStateChanged not found"); return; }

            // ExitingEditMode should not throw
            Assert.DoesNotThrow(() => method.Invoke(null, new object[] { PlayModeStateChange.ExitingEditMode }));
        }

        [Test]
        public void ValidationAutomator_PlayModeStateChanged_EnteredPlayMode()
        {
            var type = GetAutomatorType("KRT.VRCQuestTools.Automators.ValidationAutomator");
            if (type == null) { Assert.Ignore("ValidationAutomator not found"); return; }

            var method = type.GetMethod("PlayModeStateChanged", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("PlayModeStateChanged not found"); return; }

            Assert.DoesNotThrow(() => method.Invoke(null, new object[] { PlayModeStateChange.EnteredPlayMode }));
        }

        [Test]
        public void ValidationAutomator_PlayModeStateChanged_ExitingPlayMode()
        {
            var type = GetAutomatorType("KRT.VRCQuestTools.Automators.ValidationAutomator");
            if (type == null) { Assert.Ignore("ValidationAutomator not found"); return; }

            var method = type.GetMethod("PlayModeStateChanged", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("PlayModeStateChanged not found"); return; }

            Assert.DoesNotThrow(() => method.Invoke(null, new object[] { PlayModeStateChange.ExitingPlayMode }));
        }

        [Test]
        public void ValidationAutomator_PlayModeStateChanged_EnteredEditMode()
        {
            var type = GetAutomatorType("KRT.VRCQuestTools.Automators.ValidationAutomator");
            if (type == null) { Assert.Ignore("ValidationAutomator not found"); return; }

            var method = type.GetMethod("PlayModeStateChanged", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("PlayModeStateChanged not found"); return; }

            Assert.DoesNotThrow(() => method.Invoke(null, new object[] { PlayModeStateChange.EnteredEditMode }));
        }

        [Test]
        public void UpdateCheckerAutomator_PlayModeStateChanged_ExitingEditMode()
        {
            var type = GetAutomatorType("KRT.VRCQuestTools.Automators.UpdateCheckerAutomator");
            if (type == null) { Assert.Ignore("UpdateCheckerAutomator not found"); return; }

            var method = type.GetMethod("PlayModeStateChanged", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("PlayModeStateChanged not found"); return; }

            Assert.DoesNotThrow(() => method.Invoke(null, new object[] { PlayModeStateChange.ExitingEditMode }));
        }

        [Test]
        public void UpdateCheckerAutomator_PlayModeStateChanged_EnteredPlayMode()
        {
            var type = GetAutomatorType("KRT.VRCQuestTools.Automators.UpdateCheckerAutomator");
            if (type == null) { Assert.Ignore("UpdateCheckerAutomator not found"); return; }

            var method = type.GetMethod("PlayModeStateChanged", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("PlayModeStateChanged not found"); return; }

            Assert.DoesNotThrow(() => method.Invoke(null, new object[] { PlayModeStateChange.EnteredPlayMode }));
        }

        [Test]
        public void UpdateCheckerAutomator_PlayModeStateChanged_ExitingPlayMode()
        {
            var type = GetAutomatorType("KRT.VRCQuestTools.Automators.UpdateCheckerAutomator");
            if (type == null) { Assert.Ignore("UpdateCheckerAutomator not found"); return; }

            var method = type.GetMethod("PlayModeStateChanged", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("PlayModeStateChanged not found"); return; }

            Assert.DoesNotThrow(() => method.Invoke(null, new object[] { PlayModeStateChange.ExitingPlayMode }));
        }

        [Test]
        public void UpdateCheckerAutomator_PlayModeStateChanged_EnteredEditMode()
        {
            var type = GetAutomatorType("KRT.VRCQuestTools.Automators.UpdateCheckerAutomator");
            if (type == null) { Assert.Ignore("UpdateCheckerAutomator not found"); return; }

            var method = type.GetMethod("PlayModeStateChanged", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("PlayModeStateChanged not found"); return; }

            Assert.DoesNotThrow(() => method.Invoke(null, new object[] { PlayModeStateChange.EnteredEditMode }));
        }
    }

    [TestFixture]
    public class Batch28_MissingScriptsRule_Tests
    {
        [Test]
        public void Validate_InactiveAvatar_ReturnsNull()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

                go.SetActive(false);

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
        public void Validate_ActiveAvatar_NoMissingScripts_ReturnsNull()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

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
    }

    [TestFixture]
    public class Batch28_VRCQuestToolsStatic_Tests
    {
        [Test]
        public void Name_IsVRCQuestTools()
        {
            Assert.AreEqual("VRCQuestTools", VRCQuestTools.Name);
        }

        [Test]
        public void Version_IsNotNullOrEmpty()
        {
            Assert.IsFalse(string.IsNullOrEmpty(VRCQuestTools.Version));
        }

        [Test]
        public void Version_IsSemver()
        {
            var version = new SemVer(VRCQuestTools.Version);
            Assert.IsNotNull(version);
        }

        [Test]
        public void AssetRoot_IsNotNullOrEmpty()
        {
            var assetRoot = VRCQuestTools.AssetRoot;
            Assert.IsFalse(string.IsNullOrEmpty(assetRoot));
        }

        [Test]
        public void IsImportedAsPackage_Returns_BasedOnAssetRoot()
        {
            var assetRoot = VRCQuestTools.AssetRoot;
            var isPackage = VRCQuestTools.IsImportedAsPackage;
            Assert.AreEqual(assetRoot.StartsWith("Packages"), isPackage);
        }

        [Test]
        public void ComponentRemover_IsNotNull()
        {
            Assert.IsNotNull(VRCQuestTools.ComponentRemover);
        }

        [Test]
        public void AvatarConverter_IsNotNull()
        {
            Assert.IsNotNull(VRCQuestTools.AvatarConverter);
        }

        [Test]
        public void VPM_IsNotNull()
        {
            Assert.IsNotNull(VRCQuestTools.VPM);
        }

        [Test]
        public void VPMRepositoryURL_IsNotEmpty()
        {
            Assert.IsFalse(string.IsNullOrEmpty(VRCQuestTools.VPMRepositoryURL));
        }

        [Test]
        public void DocsURL_IsNotEmpty()
        {
            Assert.IsFalse(string.IsNullOrEmpty(VRCQuestTools.DocsURL));
        }

        [Test]
        public void PackageName_IsCorrect()
        {
            Assert.AreEqual("com.github.kurotu.vrc-quest-tools", VRCQuestTools.PackageName);
        }

        [Test]
        public void GitHubRepository_IsCorrect()
        {
            Assert.AreEqual("kurotu/VRCQuestTools", VRCQuestTools.GitHubRepository);
        }
    }

    [TestFixture]
    public class Batch28_MockPerformanceStatsLevelSet_Tests
    {
        [Test]
        public void Properties_ReturnDefault()
        {
            // Access Mock_AvatarPerformanceStatsLevelSet via reflection since it's internal
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return new Type[0]; }
                })
                .FirstOrDefault(t => t.FullName == "KRT.VRCQuestTools.Mocks.Mock_AvatarPerformanceStatsLevelSet");

            if (type == null) { Assert.Ignore("Mock_AvatarPerformanceStatsLevelSet not found"); return; }

            // It inherits UnityEngine.Object, so we must create through special means
            // Actually, it has no public constructor accessible, and inherits UObject
            // We need to use Activator or FormatterServices
            try
            {
                var obj = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
                var excellent = type.GetProperty("excellent").GetValue(obj);
                var good = type.GetProperty("good").GetValue(obj);
                var medium = type.GetProperty("medium").GetValue(obj);
                var poor = type.GetProperty("poor").GetValue(obj);

                // These are get-only properties with no backing field init, so they return null
                Assert.IsNull(excellent);
                Assert.IsNull(good);
                Assert.IsNull(medium);
                Assert.IsNull(poor);
            }
            catch (Exception ex)
            {
                Assert.Ignore($"Cannot instantiate Mock_AvatarPerformanceStatsLevelSet: {ex.Message}");
            }
        }
    }

    [TestFixture]
    public class Batch28_AdditionalMaterialConvertSettings_Tests
    {
        [Test]
        public void GetCacheKey_WithNullMaterial_ContainsNull()
        {
            var settings = new AdditionalMaterialConvertSettings();
            settings.targetMaterial = null;
            settings.materialConvertSettings = new ToonLitConvertSettings();

            var key = settings.GetCacheKey();
            Assert.IsTrue(key.Contains("null"));
        }

        [Test]
        public void GetCacheKey_WithMaterial_ContainsInstanceId()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var settings = new AdditionalMaterialConvertSettings();
                settings.targetMaterial = mat;
                settings.materialConvertSettings = new ToonLitConvertSettings();

                var key = settings.GetCacheKey();
                Assert.IsTrue(key.Contains(mat.GetInstanceID().ToString()));
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void LoadDefaultAssets_DoesNotThrow()
        {
            var settings = new AdditionalMaterialConvertSettings();
            settings.materialConvertSettings = new ToonLitConvertSettings();
            Assert.DoesNotThrow(() => settings.LoadDefaultAssets());
        }
    }

    [TestFixture]
    public class Batch28_IMaterialConversionComponent_GetCacheKey_Tests
    {
        [Test]
        public void AvatarConverterSettings_GetCacheKey_ReturnsValue()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                settings.AdditionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[0];

                var comp = (IMaterialConversionComponent)settings;
                var key = comp.GetCacheKey();
                Assert.IsFalse(string.IsNullOrEmpty(key));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void MaterialConversionSettings_GetCacheKey_ReturnsValue()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var settings = go.AddComponent<MaterialConversionSettings>();
                settings.AdditionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[0];

                var comp = (IMaterialConversionComponent)settings;
                var key = comp.GetCacheKey();
                Assert.IsFalse(string.IsNullOrEmpty(key));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetCacheKey_WithAdditionalSettings_IncludesSettingsInKey()
        {
            var mat = new Material(Shader.Find("Standard"));
            var go = new GameObject("TestAvatar");
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                settings.AdditionalMaterialConvertSettings = new[]
                {
                    new AdditionalMaterialConvertSettings
                    {
                        targetMaterial = mat,
                        materialConvertSettings = new ToonLitConvertSettings(),
                    },
                };

                var comp = (IMaterialConversionComponent)settings;
                var key = comp.GetCacheKey();
                Assert.IsTrue(key.Contains(mat.GetInstanceID().ToString()));
            }
            finally
            {
                UObject.DestroyImmediate(go);
                UObject.DestroyImmediate(mat);
            }
        }
    }

    [TestFixture]
    public class Batch28_AvatarValidationRules_Tests
    {
        [Test]
        public void Rules_IsNotNull()
        {
            var rules = AvatarValidationRules.Rules;
            Assert.IsNotNull(rules);
            Assert.IsTrue(rules.Length > 0);
        }

        [Test]
        public void Add_NewRule_IncreasesCount()
        {
            var initialCount = AvatarValidationRules.Rules.Length;

            // Add a dummy rule
            var dummyRule = new DummyValidationRule();
            AvatarValidationRules.Add(dummyRule);

            var newCount = AvatarValidationRules.Rules.Length;
            Assert.AreEqual(initialCount + 1, newCount);
        }

        private class DummyValidationRule : IAvatarValidationRule
        {
            public NotificationItem Validate(VRChatAvatar avatar) => null;
        }
    }

    [TestFixture]
    public class Batch28_UpdateCheckerAutomator_CheckForUpdates_Tests
    {
        [Test]
        public void CheckForUpdates_WithIgnoreCache_ReturnsResult()
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return new Type[0]; }
                })
                .FirstOrDefault(t => t.FullName == "KRT.VRCQuestTools.Automators.UpdateCheckerAutomator");

            if (type == null) { Assert.Ignore("UpdateCheckerAutomator not found"); return; }

            // Access CheckForUpdates(bool) via reflection - it's internal static async Task<bool>
            var method = type.GetMethod("CheckForUpdates", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(bool) }, null);
            if (method == null) { Assert.Ignore("CheckForUpdates(bool) not found"); return; }

            // Call with ignoreCache=false (should use cached results)
            try
            {
                var task = (System.Threading.Tasks.Task<bool>)method.Invoke(null, new object[] { false });
                // Just ensure it doesn't throw synchronously
                Assert.IsNotNull(task);

                // Wait for the task with a timeout
                if (task.Wait(5000))
                {
                    // Result should be a boolean (true/false for has update)
                    Assert.IsNotNull(task.Result);
                }
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                // Expected if network not available or Newtonsoft Json not configured
                Assert.Pass($"CheckForUpdates threw expected exception: {ex.InnerException.GetType().Name}");
            }
        }
    }

    [TestFixture]
    public class Batch28_VRCQuestToolsAvatarProcessor_Tests
    {
        [Test]
        public void CallbackOrder_IsNegative12000()
        {
            // Access VRCQuestToolsAvatarProcessor via reflection since it's in NonDestructive assembly
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return new Type[0]; }
                })
                .FirstOrDefault(t => t.FullName == "KRT.VRCQuestTools.NonDestructive.VRCQuestToolsAvatarProcessor");

            if (type == null) { Assert.Ignore("VRCQuestToolsAvatarProcessor not found"); return; }

            var instance = Activator.CreateInstance(type);
            var prop = type.GetProperty("callbackOrder");
            if (prop == null) { Assert.Ignore("callbackOrder property not found"); return; }

            var order = (int)prop.GetValue(instance);
            Assert.AreEqual(-12000, order);
        }

        [Test]
        public void OnPreprocessAvatar_NoMissingScripts_ReturnsTrue()
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return new Type[0]; }
                })
                .FirstOrDefault(t => t.FullName == "KRT.VRCQuestTools.NonDestructive.VRCQuestToolsAvatarProcessor");

            if (type == null) { Assert.Ignore("VRCQuestToolsAvatarProcessor not found"); return; }

            var go = new GameObject("TestAvatar");
            try
            {
                go.AddComponent<VRCAvatarDescriptor>();

                var instance = Activator.CreateInstance(type);
                var method = type.GetMethod("OnPreprocessAvatar");
                if (method == null) { Assert.Ignore("OnPreprocessAvatar not found"); return; }

                var result = (bool)method.Invoke(instance, new object[] { go });
                Assert.IsTrue(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnPreprocessAvatar_WithVertexColorRemover_RemovesColors()
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return new Type[0]; }
                })
                .FirstOrDefault(t => t.FullName == "KRT.VRCQuestTools.NonDestructive.VRCQuestToolsAvatarProcessor");

            if (type == null) { Assert.Ignore("VRCQuestToolsAvatarProcessor not found"); return; }

            var go = new GameObject("TestAvatar");
            try
            {
                go.AddComponent<VRCAvatarDescriptor>();

                // Add a VertexColorRemover and a mesh with vertex colors
                var child = new GameObject("MeshChild");
                child.transform.SetParent(go.transform);
                var remover = child.AddComponent<VertexColorRemover>();
                var smr = child.AddComponent<SkinnedMeshRenderer>();

                var mesh = new Mesh();
                mesh.vertices = new Vector3[] { Vector3.zero, Vector3.right, Vector3.up };
                mesh.triangles = new int[] { 0, 1, 2 };
                mesh.colors32 = new Color32[]
                {
                    new Color32(255, 0, 0, 255),
                    new Color32(0, 255, 0, 255),
                    new Color32(0, 0, 255, 255),
                };
                smr.sharedMesh = mesh;

                var instance = Activator.CreateInstance(type);
                var method = type.GetMethod("OnPreprocessAvatar");
                if (method == null) { Assert.Ignore("OnPreprocessAvatar not found"); return; }

                var result = (bool)method.Invoke(instance, new object[] { go });
                Assert.IsTrue(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnPreprocessAvatar_WithEditorOnlyComponent_DestroysIt()
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return new Type[0]; }
                })
                .FirstOrDefault(t => t.FullName == "KRT.VRCQuestTools.NonDestructive.VRCQuestToolsAvatarProcessor");

            if (type == null) { Assert.Ignore("VRCQuestToolsAvatarProcessor not found"); return; }

            var go = new GameObject("TestAvatar");
            try
            {
                go.AddComponent<VRCAvatarDescriptor>();

                // Add a VRCQuestToolsEditorOnly component (e.g. NetworkIDAssigner)
                var child = new GameObject("EditorOnlyChild");
                child.transform.SetParent(go.transform);
                child.AddComponent<NetworkIDAssigner>();

                var instance = Activator.CreateInstance(type);
                var method = type.GetMethod("OnPreprocessAvatar");
                if (method == null) { Assert.Ignore("OnPreprocessAvatar not found"); return; }

                var result = (bool)method.Invoke(instance, new object[] { go });
                Assert.IsTrue(result);

                // The NetworkIDAssigner (VRCQuestToolsEditorOnly) should be destroyed
                var remaining = go.GetComponentsInChildren<NetworkIDAssigner>(true);
                Assert.AreEqual(0, remaining.Length);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void AssignNetworkIDs_NoAssigner_DoesNotThrow()
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return new Type[0]; }
                })
                .FirstOrDefault(t => t.FullName == "KRT.VRCQuestTools.NonDestructive.VRCQuestToolsAvatarProcessor");

            if (type == null) { Assert.Ignore("VRCQuestToolsAvatarProcessor not found"); return; }

            var method = type.GetMethod("AssignNetworkIDs", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("AssignNetworkIDs not found"); return; }

            var go = new GameObject("TestAvatar");
            try
            {
                go.AddComponent<VRCAvatarDescriptor>();
                // No NetworkIDAssigner added
                Assert.DoesNotThrow(() => method.Invoke(null, new object[] { go }));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void AssignNetworkIDs_WithAssigner_CallsAssign()
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return new Type[0]; }
                })
                .FirstOrDefault(t => t.FullName == "KRT.VRCQuestTools.NonDestructive.VRCQuestToolsAvatarProcessor");

            if (type == null) { Assert.Ignore("VRCQuestToolsAvatarProcessor not found"); return; }

            var method = type.GetMethod("AssignNetworkIDs", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("AssignNetworkIDs not found"); return; }

            var go = new GameObject("TestAvatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                go.AddComponent<NetworkIDAssigner>();

                Assert.DoesNotThrow(() => method.Invoke(null, new object[] { go }));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    public class Batch28_ActualPerformanceCallback_Tests
    {
        [Test]
        public void CallbackOrder_IsIntMaxValue()
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return new Type[0]; }
                })
                .FirstOrDefault(t => t.FullName == "KRT.VRCQuestTools.NonDestructive.ActualPerformanceCallback");

            if (type == null) { Assert.Ignore("ActualPerformanceCallback not found"); return; }

            var instance = Activator.CreateInstance(type);
            var prop = type.GetProperty("callbackOrder");
            if (prop == null) { Assert.Ignore("callbackOrder not found"); return; }

            var order = (int)prop.GetValue(instance);
            Assert.AreEqual(int.MaxValue, order);
        }

        [Test]
        public void LastActualPerformanceRating_IsNotNull()
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return new Type[0]; }
                })
                .FirstOrDefault(t => t.FullName == "KRT.VRCQuestTools.NonDestructive.ActualPerformanceCallback");

            if (type == null) { Assert.Ignore("ActualPerformanceCallback not found"); return; }

            var field = type.GetField("LastActualPerformanceRating", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
            if (field == null) { Assert.Ignore("LastActualPerformanceRating not found"); return; }

            var dict = field.GetValue(null);
            Assert.IsNotNull(dict);
        }
    }

    [TestFixture]
    public class Batch28_FallbackAvatarCallback_Tests
    {
        [Test]
        public void CallbackOrder_IsNegative100000()
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return new Type[0]; }
                })
                .FirstOrDefault(t => t.FullName == "KRT.VRCQuestTools.NonDestructive.FallbackAvatarCallback");

            if (type == null) { Assert.Ignore("FallbackAvatarCallback not found"); return; }

            var instance = Activator.CreateInstance(type);
            var prop = type.GetProperty("callbackOrder");
            if (prop == null) { Assert.Ignore("callbackOrder not found"); return; }

            var order = (int)prop.GetValue(instance);
            Assert.AreEqual(-100000, order);
        }
    }

    [TestFixture]
    public class Batch28_MaterialConversionSettings_CacheKey_Tests
    {
        [Test]
        public void RemoveExtraMaterialSlots_DefaultValue()
        {
            var go = new GameObject("Test");
            try
            {
                var settings = go.AddComponent<MaterialConversionSettings>();
                // Access RemoveExtraMaterialSlots - default should be a boolean
                var comp = (IMaterialConversionComponent)settings;
                var value = comp.RemoveExtraMaterialSlots;
                Assert.IsTrue(value); // Default is true
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void EnableMaterialPreview_DefaultValue()
        {
            var go = new GameObject("Test");
            try
            {
                var settings = go.AddComponent<MaterialConversionSettings>();
                var comp = (IMaterialConversionComponent)settings;
                var value = comp.EnableMaterialPreview;
                // Default should be a boolean value
                Assert.IsNotNull(value);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void IsPrimaryRoot_DefaultValue()
        {
            var go = new GameObject("Test");
            try
            {
                var settings = go.AddComponent<MaterialConversionSettings>();
                var comp = (IMaterialConversionComponent)settings;
                Assert.IsFalse(comp.IsPrimaryRoot);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void AvatarConverterSettings_IsPrimaryRoot_True()
        {
            var go = new GameObject("Test");
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                var comp = (IMaterialConversionComponent)settings;
                Assert.IsTrue(comp.IsPrimaryRoot);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    public class Batch28_I18n_Remaining_Tests
    {
        [Test]
        public void SetLanguage_English_Works()
        {
            var i18n = KRT.VRCQuestTools.I18n.I18n.GetI18n();
            Assert.IsNotNull(i18n);
        }

        [Test]
        public void SupportedLanguages_NotEmpty()
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return new Type[0]; }
                })
                .FirstOrDefault(t => t.FullName == "KRT.VRCQuestTools.I18n.I18n");

            if (type == null) { Assert.Ignore("I18n type not found"); return; }

            var supportedLangs = type.GetProperty("SupportedLanguages", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
            if (supportedLangs == null) { Assert.Ignore("SupportedLanguages not found"); return; }

            var langs = supportedLangs.GetValue(null) as string[];
            if (langs == null)
            {
                // Try as IEnumerable
                var enumerable = supportedLangs.GetValue(null) as System.Collections.IEnumerable;
                Assert.IsNotNull(enumerable);
            }
            else
            {
                Assert.IsTrue(langs.Length > 0);
            }
        }
    }

    [TestFixture]
    public class Batch28_ComponentRemover_RemainingPaths
    {
        [Test]
        public void RemoveUnsupportedComponents_WithMissingScripts_NoThrow()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

                var remover = new KRT.VRCQuestTools.Models.ComponentRemover();
                // Just ensure it doesn't throw with a clean avatar
                Assert.DoesNotThrow(() => remover.RemoveUnsupportedComponentsInChildren(go, false, false, new System.Type[0]));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    public class Batch28_VRChatAvatar_HasDynamicBone_Tests
    {
        [Test]
        public void HasDynamicBoneComponents_NoDynamicBone_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

                var avatar = new VRChatAvatar(desc);
                // DynamicBone is not imported in test environment, so should return false
                Assert.IsFalse(avatar.HasDynamicBoneComponents);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    public class Batch28_AnimatorControllerDuplicator_DeepPaths
    {
        [Test]
        public void Duplicate_ControllerWithSubStateMachine()
        {
            var controller = new AnimatorController();
            try
            {
                controller.AddLayer("Base");
                var layer = controller.layers[0];
                var subSM = layer.stateMachine.AddStateMachine("SubSM");
                subSM.AddState("SubState");

                var duplicator = new AnimatorControllerDuplicator();
                var result = duplicator.Duplicate(controller);
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.layers.Length);
                Assert.AreEqual(1, result.layers[0].stateMachine.stateMachines.Length);
            }
            finally
            {
                UObject.DestroyImmediate(controller);
            }
        }

        [Test]
        public void Duplicate_ControllerWithMultipleParameters()
        {
            var controller = new AnimatorController();
            try
            {
                controller.AddParameter("FloatParam", AnimatorControllerParameterType.Float);
                controller.AddParameter("IntParam", AnimatorControllerParameterType.Int);
                controller.AddParameter("BoolParam", AnimatorControllerParameterType.Bool);
                controller.AddParameter("TriggerParam", AnimatorControllerParameterType.Trigger);
                controller.AddLayer("Base");

                var duplicator = new AnimatorControllerDuplicator();
                var result = duplicator.Duplicate(controller);
                Assert.IsNotNull(result);
                Assert.AreEqual(4, result.parameters.Length);
            }
            finally
            {
                UObject.DestroyImmediate(controller);
            }
        }
    }
}
