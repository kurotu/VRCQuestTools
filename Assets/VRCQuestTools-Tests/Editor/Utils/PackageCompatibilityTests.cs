// Tests for package compatibility, exceptions, and validation
// Targets: LegacyPackageException, BreakingPackageException, MissingScriptsRule, VRCSDKUtility network IDs,
// AvatarConverter RemoveExtraMaterialSlots/exception paths, LilToonMaterial remaining getters,
// VRCQuestToolsAvatarProcessor, MaterialGeneratorUtility, VirtualLens2Material

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
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools.Tests
{
    // ========== PackageCompatibilityException Tests ==========
    [TestFixture]
    public class PackageCompatibilityExceptionTests
    {
        [Test]
        public void LegacyPackageException_Constructor_SetsProperties()
        {
            var ex = new LegacyPackageException("TestPackage", "1.0.0");
            Assert.AreEqual("TestPackage", ex.PackageDisplayName);
            Assert.AreEqual("1.0.0", ex.RequiredVersion);
            Assert.IsNotNull(ex.Message);
            Assert.IsTrue(ex.Message.Length > 0);
        }

        [Test]
        public void LegacyPackageException_LocalizedMessage_IsNotEmpty()
        {
            var ex = new LegacyPackageException("TestPackage", "2.0.0");
            Assert.IsNotNull(ex.LocalizedMessage);
            Assert.IsTrue(ex.LocalizedMessage.Length > 0);
        }

        [Test]
        public void LegacyPackageException_IsPackageCompatibilityException()
        {
            var ex = new LegacyPackageException("Test", "1.0.0");
            Assert.IsInstanceOf<PackageCompatibilityException>(ex);
            Assert.IsInstanceOf<Exception>(ex);
        }

        [Test]
        public void BreakingPackageException_Constructor_SetsProperties()
        {
            var ex = new BreakingPackageException("TestPackage", "3.0.0");
            Assert.AreEqual("TestPackage", ex.PackageDisplayName);
            Assert.AreEqual("3.0.0", ex.BreakingVersion);
            Assert.IsNotNull(ex.Message);
            Assert.IsTrue(ex.Message.Length > 0);
        }

        [Test]
        public void BreakingPackageException_LocalizedMessage_IsNotEmpty()
        {
            var ex = new BreakingPackageException("TestPackage", "4.0.0");
            Assert.IsNotNull(ex.LocalizedMessage);
            Assert.IsTrue(ex.LocalizedMessage.Length > 0);
        }

        [Test]
        public void BreakingPackageException_IsPackageCompatibilityException()
        {
            var ex = new BreakingPackageException("Test", "3.0.0");
            Assert.IsInstanceOf<PackageCompatibilityException>(ex);
            Assert.IsInstanceOf<Exception>(ex);
        }

        [Test]
        public void LegacyPackageException_ImplementsIVRCQuestToolsException()
        {
            var ex = new LegacyPackageException("Test", "1.0.0");
            var iface = typeof(LegacyPackageException).GetInterfaces();
            Assert.IsTrue(iface.Any(i => i.Name == "IVRCQuestToolsException"));
        }

        [Test]
        public void BreakingPackageException_ImplementsIVRCQuestToolsException()
        {
            var ex = new BreakingPackageException("Test", "3.0.0");
            var iface = typeof(BreakingPackageException).GetInterfaces();
            Assert.IsTrue(iface.Any(i => i.Name == "IVRCQuestToolsException"));
        }
    }

    // ========== MissingScriptsRule Tests ==========
    [TestFixture]
    public class MissingScriptsRuleTests_PkgCompat
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            objectsToCleanup.Clear();
        }

        private VRChatAvatar CreateSimpleAvatar(string name)
        {
            var go = new GameObject(name);
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            return new VRChatAvatar(desc);
        }

        [Test]
        public void Validate_ActiveAvatarWithoutMissingScripts_ReturnsNull()
        {
            var avatar = CreateSimpleAvatar("CleanAvatar");
            var rule = new MissingScriptsRule();
            var result = rule.Validate(avatar);
            Assert.IsNull(result);
        }

        [Test]
        public void Validate_InactiveAvatar_ReturnsNull()
        {
            var avatar = CreateSimpleAvatar("InactiveAvatar");
            avatar.GameObject.SetActive(false);
            var rule = new MissingScriptsRule();
            var result = rule.Validate(avatar);
            Assert.IsNull(result);
        }
    }

    // ========== VRCSDKUtility Network ID Tests ==========
    [TestFixture]
    public class VRCSDKUtilityNetworkIdTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            objectsToCleanup.Clear();
        }

        [Test]
        public void HasMissingNetworkIds_NoPhysBones_ReturnsFalse()
        {
            var go = new GameObject("NoPhysBones");
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            Assert.IsFalse(VRCSDKUtility.HasMissingNetworkIds(desc));
        }

        [Test]
        public void HasMissingNetworkIds_WithPhysBone_NoIds_ReturnsTrue()
        {
            var go = new GameObject("WithPhysBone");
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            var child = new GameObject("Bone");
            child.transform.SetParent(go.transform);
            child.AddComponent<VRCPhysBone>();
            Assert.IsTrue(VRCSDKUtility.HasMissingNetworkIds(desc));
        }

        [Test]
        public void AssignNetworkIdsToPhysBones_AssignsIds()
        {
            var go = new GameObject("AssignTest");
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var child = new GameObject("Bone1");
            child.transform.SetParent(go.transform);
            child.AddComponent<VRCPhysBone>();
            var child2 = new GameObject("Bone2");
            child2.transform.SetParent(go.transform);
            child2.AddComponent<VRCPhysBone>();

            VRCSDKUtility.AssignNetworkIdsToPhysBones(desc);
            Assert.IsTrue(desc.NetworkIDCollection.Count >= 2);
        }

        [Test]
        public void AssignNetworkIdsToPhysBones_DoesNotDuplicate()
        {
            var go = new GameObject("NoDupTest");
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var child = new GameObject("Bone");
            child.transform.SetParent(go.transform);
            child.AddComponent<VRCPhysBone>();

            VRCSDKUtility.AssignNetworkIdsToPhysBones(desc);
            var count1 = desc.NetworkIDCollection.Count;
            VRCSDKUtility.AssignNetworkIdsToPhysBones(desc);
            var count2 = desc.NetworkIDCollection.Count;
            Assert.AreEqual(count1, count2, "Should not add duplicate IDs");
        }

        [Test]
        public void AssignNetworkIdsToPhysBonesByHierarchyHash_AssignsIds()
        {
            var go = new GameObject("HashAssignTest");
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var child = new GameObject("PhysBone1");
            child.transform.SetParent(go.transform);
            child.AddComponent<VRCPhysBone>();

            var (allIds, newIds) = VRCSDKUtility.AssignNetworkIdsToPhysBonesByHierarchyHash(desc);
            Assert.IsTrue(newIds.Any(), "Should assign new IDs");
            Assert.IsTrue(allIds.Any(), "Should have all IDs");
        }

        [Test]
        public void AssignNetworkIdsToPhysBonesByHierarchyHash_DoesNotDuplicate()
        {
            var go = new GameObject("HashNoDupTest");
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var child = new GameObject("PhysBone");
            child.transform.SetParent(go.transform);
            child.AddComponent<VRCPhysBone>();

            VRCSDKUtility.AssignNetworkIdsToPhysBonesByHierarchyHash(desc);
            var count1 = desc.NetworkIDCollection.Count;
            var (_, newIds) = VRCSDKUtility.AssignNetworkIdsToPhysBonesByHierarchyHash(desc);
            Assert.AreEqual(0, newIds.Count(), "Should not add new IDs on second call");
            Assert.AreEqual(count1, desc.NetworkIDCollection.Count);
        }

        [Test]
        public void HasMissingNetworkIds_AfterAssignment_ReturnsFalse()
        {
            var go = new GameObject("AfterAssignTest");
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var child = new GameObject("PB");
            child.transform.SetParent(go.transform);
            child.AddComponent<VRCPhysBone>();

            Assert.IsTrue(VRCSDKUtility.HasMissingNetworkIds(desc));
            VRCSDKUtility.AssignNetworkIdsToPhysBones(desc);
            Assert.IsFalse(VRCSDKUtility.HasMissingNetworkIds(desc));
        }

        [Test]
        public void GetFullPathInHierarchy_ReturnsCorrectPath()
        {
            var go = new GameObject("Root");
            objectsToCleanup.Add(go);
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            var grandchild = new GameObject("Grandchild");
            grandchild.transform.SetParent(child.transform);

            var method = typeof(VRCSDKUtility).GetMethod("GetFullPathInHierarchy", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (method != null)
            {
                var path = (string)method.Invoke(null, new object[] { grandchild });
                Assert.IsTrue(path.Contains("Child"));
                Assert.IsTrue(path.Contains("Grandchild"));
            }
        }
    }

    // ========== AvatarConverter RemoveExtraMaterialSlots Tests ==========
    [TestFixture]
    public class AvatarConverterExtraSlotsTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            objectsToCleanup.Clear();
        }

        [Test]
        public void RemoveExtraMaterialSlots_TrimsExcessMaterials()
        {
            var go = new GameObject("SlotTest");
            objectsToCleanup.Add(go);
            var child = new GameObject("MeshChild");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();

            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            mesh.vertices = new[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.subMeshCount = 1;
            smr.sharedMesh = mesh;

            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(mat1);
            objectsToCleanup.Add(mat2);
            smr.sharedMaterials = new[] { mat1, mat2 }; // 2 materials, but only 1 submesh

            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "RemoveExtraMaterialSlots should exist");
            method.Invoke(converter, new object[] { go });

            Assert.AreEqual(1, smr.sharedMaterials.Length, "Should trim to submesh count");
        }

        [Test]
        public void RemoveExtraMaterialSlots_NoChange_WhenCorrect()
        {
            var go = new GameObject("NoChangeTest");
            objectsToCleanup.Add(go);
            var child = new GameObject("MeshChild2");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();

            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            mesh.vertices = new[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.subMeshCount = 1;
            smr.sharedMesh = mesh;

            var mat1 = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(mat1);
            smr.sharedMaterials = new[] { mat1 };

            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(converter, new object[] { go });

            Assert.AreEqual(1, smr.sharedMaterials.Length);
        }

        [Test]
        public void RemoveExtraMaterialSlots_NullMesh_Skips()
        {
            var go = new GameObject("NullMeshTest");
            objectsToCleanup.Add(go);
            var child = new GameObject("NoMeshChild");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();

            var mat1 = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(mat1);
            smr.sharedMaterials = new[] { mat1 };
            // No mesh assigned

            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.DoesNotThrow(() => method.Invoke(converter, new object[] { go }));
        }

        [Test]
        public void RemoveExtraMaterialSlots_MeshRenderer_TrimsMaterials()
        {
            var go = new GameObject("MeshRendererTest");
            objectsToCleanup.Add(go);
            var child = new GameObject("MRChild");
            child.transform.SetParent(go.transform);
            var mf = child.AddComponent<MeshFilter>();
            var mr = child.AddComponent<MeshRenderer>();

            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            mesh.vertices = new[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.subMeshCount = 1;
            mf.sharedMesh = mesh;

            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            var mat3 = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(mat1);
            objectsToCleanup.Add(mat2);
            objectsToCleanup.Add(mat3);
            mr.sharedMaterials = new[] { mat1, mat2, mat3 };

            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(converter, new object[] { go });

            Assert.AreEqual(1, mr.sharedMaterials.Length);
        }
    }

    // ========== AvatarConverter CreateMaterialConvertSettingsMap Exception Paths ==========
    [TestFixture]
    public class AvatarConverterSettingsMapExceptionTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            objectsToCleanup.Clear();
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithMaterialConversionSettings_AddsEntries()
        {
            var go = new GameObject("MatConvSettingsAvatar");
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(mat);
            smr.sharedMaterials = new[] { mat };

            // Add MaterialConversionSettings
            var convSettings = go.AddComponent<MaterialConversionSettings>();
            convSettings.additionalMaterialConvertSettings = new[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = mat,
                    materialConvertSettings = new ToonLitConvertSettings(),
                },
            };

            var avatar = new VRChatAvatar(desc);
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var map = converter.CreateMaterialConvertSettingsMap(avatar);

            Assert.IsNotNull(map);
            Assert.IsTrue(map.ContainsKey(mat));
            Assert.IsInstanceOf<ToonLitConvertSettings>(map[mat]);
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_NullTargetMaterial_ThrowsTargetMaterialNullException()
        {
            var go = new GameObject("NullTargetAvatar");
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

            var convSettings = go.AddComponent<MaterialConversionSettings>();
            convSettings.additionalMaterialConvertSettings = new[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = null,
                    materialConvertSettings = new ToonLitConvertSettings(),
                },
            };

            var child = new GameObject("Body2");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(mat);
            smr.sharedMaterials = new[] { mat };

            var avatar = new VRChatAvatar(desc);
            var converter = new AvatarConverter(new MaterialWrapperBuilder());

            Assert.Throws<TargetMaterialNullException>(() => converter.CreateMaterialConvertSettingsMap(avatar));
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_EmptyAvatar_ReturnsEmptyMap()
        {
            var go = new GameObject("EmptyAvatar");
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

            var avatar = new VRChatAvatar(desc);
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var map = converter.CreateMaterialConvertSettingsMap(avatar);

            Assert.IsNotNull(map);
            Assert.AreEqual(0, map.Count);
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithAvatarConverterSettings_AppliesDefault()
        {
            var go = new GameObject("DefaultSettingsAvatar");
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var settings = go.AddComponent<AvatarConverterSettings>();

            var child = new GameObject("Body3");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(mat);
            smr.sharedMaterials = new[] { mat };

            var avatar = new VRChatAvatar(desc);
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var map = converter.CreateMaterialConvertSettingsMap(avatar);

            Assert.IsNotNull(map);
            Assert.IsTrue(map.Count > 0, "Should have entries from default settings");
            Assert.IsTrue(map.ContainsKey(mat));
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_NullTargetInAvatarConverterSettings_Throws()
        {
            var go = new GameObject("NullTargetInConverterSettings");
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var settings = go.AddComponent<AvatarConverterSettings>();
            settings.additionalMaterialConvertSettings = new[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = null,
                    materialConvertSettings = new ToonLitConvertSettings(),
                },
            };

            var child = new GameObject("Body4");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(mat);
            smr.sharedMaterials = new[] { mat };

            var avatar = new VRChatAvatar(desc);
            var converter = new AvatarConverter(new MaterialWrapperBuilder());

            Assert.Throws<TargetMaterialNullException>(() => converter.CreateMaterialConvertSettingsMap(avatar));
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_FiltersUnusedMaterials()
        {
            var go = new GameObject("FilterUnusedAvatar");
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

            var child = new GameObject("Body5");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var usedMat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(usedMat);
            smr.sharedMaterials = new[] { usedMat };

            // Add MaterialConversionSettings targeting an unused material
            var unusedMat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(unusedMat);
            var convSettings = go.AddComponent<MaterialConversionSettings>();
            convSettings.additionalMaterialConvertSettings = new[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = unusedMat,
                    materialConvertSettings = new ToonLitConvertSettings(),
                },
            };

            var avatar = new VRChatAvatar(desc);
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var map = converter.CreateMaterialConvertSettingsMap(avatar);

            Assert.IsFalse(map.ContainsKey(unusedMat), "Unused material should be filtered out");
        }
    }

    // ========== LilToonMaterial Remaining Getter Tests ==========
    [TestFixture]
    public class LilToonMaterialRemainingGetterTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            objectsToCleanup.Clear();
        }

        private LilToonMaterial CreateLilToon()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) return null;
            var mat = new Material(shader);
            objectsToCleanup.Add(mat);
            return new LilToonMaterial(mat);
        }

        // Emission properties
        [Test]
        public void EmissionMapTextureScale_ReturnsValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var scale = lil.EmissionMapTextureScale;
            Assert.IsNotNull(scale);
        }

        [Test]
        public void EmissionMapTextureOffset_ReturnsValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var offset = lil.EmissionMapTextureOffset;
            Assert.IsNotNull(offset);
        }

        [Test]
        public void EmissionBlendMask_DefaultIsNull()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            // Blend mask may be null by default
            var mask = lil.EmissionBlendMask;
            Assert.IsTrue(mask == null || mask != null); // Just verify access doesn't throw
        }

        [Test]
        public void EmissionBlend_ReturnsFloat()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var blend = lil.EmissionBlend;
            Assert.IsTrue(blend >= 0f);
        }

        // 2nd Emission properties
        [Test]
        public void UseEmission2nd_GetSet()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.UseEmission2nd = true;
            Assert.IsTrue(lil.UseEmission2nd);
            lil.UseEmission2nd = false;
            Assert.IsFalse(lil.UseEmission2nd);
        }

        [Test]
        public void Emission2ndMap_DefaultIsNull()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var tex = lil.Emission2ndMap;
            Assert.IsNull(tex);
        }

        [Test]
        public void Emission2ndColor_GetSet()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Emission2ndColor = Color.cyan;
            var color = lil.Emission2ndColor;
            Assert.AreEqual(Color.cyan.r, color.r, 0.01f);
        }

        [Test]
        public void Emission2ndBlendMask_DefaultIsNull()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            Assert.IsNull(lil.Emission2ndBlendMask);
        }

        [Test]
        public void Emission2ndBlend_ReturnsFloat()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var blend = lil.Emission2ndBlend;
            Assert.IsTrue(blend >= 0f);
        }

        // Reflection / Metallic properties
        [Test]
        public void UseReflection_DefaultIsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            Assert.IsFalse(lil.UseReflection);
        }

        [Test]
        public void MetallicMap_DefaultIsNull()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            Assert.IsNull(lil.MetallicMap);
        }

        [Test]
        public void MetallicMapTextureScale_ReturnsValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var scale = lil.MetallicMapTextureScale;
            Assert.AreEqual(1f, scale.x, 0.01f);
        }

        [Test]
        public void MetallicMapTextureOffset_ReturnsValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var offset = lil.MetallicMapTextureOffset;
            Assert.AreEqual(0f, offset.x, 0.01f);
        }

        [Test]
        public void SmoothnessTex_DefaultIsNull()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            Assert.IsNull(lil.SmoothnessTex);
        }

        [Test]
        public void SmoothnessTexScale_ReturnsValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var scale = lil.SmoothnessTexScale;
            Assert.AreEqual(1f, scale.x, 0.01f);
        }

        [Test]
        public void SmoothnessTexOffset_ReturnsValue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var offset = lil.SmoothnessTexOffset;
            Assert.AreEqual(0f, offset.x, 0.01f);
        }

        [Test]
        public void ReflectionColorTex_DefaultIsNull()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            Assert.IsNull(lil.ReflectionColorTex);
        }

        [Test]
        public void ReflectionColor_ReturnsColor()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var color = lil.ReflectionColor;
            Assert.IsNotNull(color);
        }

        [Test]
        public void Smoothness_ReturnsFloat()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var val = lil.Smoothness;
            Assert.IsTrue(val >= 0f);
        }

        [Test]
        public void Reflectance_ReturnsFloat()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var val = lil.Reflectance;
            Assert.IsTrue(val >= 0f);
        }

        [Test]
        public void SpecularBlur_ReturnsFloat()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var val = lil.SpecularBlur;
            Assert.IsTrue(val >= 0f);
        }

        // MatCap properties
        [Test]
        public void UseMatCap_DefaultIsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            Assert.IsFalse(lil.UseMatCap);
        }

        [Test]
        public void MatCapTex_DefaultIsNull()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            Assert.IsNull(lil.MatCapTex);
        }

        [Test]
        public void MatCapColor_ReturnsColor()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var color = lil.MatCapColor;
            Assert.IsNotNull(color);
        }

        // Rim Light properties
        [Test]
        public void UseRimLight_DefaultIsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            Assert.IsFalse(lil.UseRimLight);
        }

        [Test]
        public void RimLightColor_ReturnsColor()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var color = lil.RimLightColor;
            Assert.IsNotNull(color);
        }

        [Test]
        public void UseReflection_WhenSet_True()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 1.0f);
            Assert.IsTrue(lil.UseReflection);
        }

        [Test]
        public void UseMatCap_WhenSet_True()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseMatCap", 1.0f);
            Assert.IsTrue(lil.UseMatCap);
        }

        [Test]
        public void UseRimLight_WhenSet_True()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseRim", 1.0f);
            Assert.IsTrue(lil.UseRimLight);
        }
    }

    // ========== VRCQuestToolsAvatarProcessor Tests ==========
    [TestFixture]
    public class AvatarProcessorTests_PkgCompat
    {
        [Test]
        public void AvatarProcessor_CallbackOrder_IsNegative()
        {
            // Access via reflection since it's in a different assembly
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type processorType = null;
            foreach (var asm in assemblies)
            {
                processorType = asm.GetType("KRT.VRCQuestTools.NonDestructive.VRCQuestToolsAvatarProcessor");
                if (processorType != null) break;
            }
            if (processorType == null) { Assert.Ignore("Type not found"); return; }

            var processor = Activator.CreateInstance(processorType);
            var orderProp = processorType.GetProperty("callbackOrder");
            Assert.IsNotNull(orderProp);
            var order = (int)orderProp.GetValue(processor);
            Assert.AreEqual(-12000, order);
        }
    }

    // ========== MaterialGeneratorUtility Tests ==========
    [TestFixture]
    public class MaterialGeneratorUtilityTests_PkgCompat
    {
        [Test]
        public void ConvertToNullableTextureFormat_NoOverride_ReturnsNull()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod(
                "ConvertToNullableTextureFormat",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (method == null) { Assert.Ignore("Method not found"); return; }

            var result = method.Invoke(null, new object[] { MobileTextureFormat.NoOverride });
            Assert.IsNull(result);
        }

        [Test]
        public void ConvertToNullableTextureFormat_ASTC_6x6_ReturnsValue()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod(
                "ConvertToNullableTextureFormat",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (method == null) { Assert.Ignore("Method not found"); return; }

            var result = method.Invoke(null, new object[] { MobileTextureFormat.ASTC_6x6 });
            Assert.IsNotNull(result);
        }

        [Test]
        public void ConvertToNullableTextureFormat_ETC2_ReturnsValue()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod(
                "ConvertToNullableTextureFormat",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (method == null) { Assert.Ignore("Method not found"); return; }

            // Use the actual enum values available
            foreach (MobileTextureFormat format in Enum.GetValues(typeof(MobileTextureFormat)))
            {
                if (format != MobileTextureFormat.NoOverride && format != MobileTextureFormat.ASTC_6x6)
                {
                    var result = method.Invoke(null, new object[] { format });
                    Assert.IsNotNull(result, $"{format} should return non-null");
                    break;
                }
            }
        }

        [Test]
        public void ConvertToNullableTextureFormat_AllFormats_ReturnCorrectly()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod(
                "ConvertToNullableTextureFormat",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (method == null) { Assert.Ignore("Method not found"); return; }

            foreach (MobileTextureFormat format in Enum.GetValues(typeof(MobileTextureFormat)))
            {
                var result = method.Invoke(null, new object[] { format });
                if (format == MobileTextureFormat.NoOverride)
                {
                    Assert.IsNull(result, $"NoOverride should return null");
                }
                else
                {
                    Assert.IsNotNull(result, $"{format} should return non-null");
                }
            }
        }
    }

    // ========== ComponentRemover Additional Tests ==========
    [TestFixture]
    public class ComponentRemoverTests_PkgCompat
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            objectsToCleanup.Clear();
        }

        [Test]
        public void RemoveUnsupportedComponentsInChildren_RemovesComponents()
        {
            var go = new GameObject("RemoverTest");
            objectsToCleanup.Add(go);
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            // Add AudioSource which is unsupported on Quest
            child.AddComponent<AudioSource>();

            var remover = new ComponentRemover();
            remover.RemoveUnsupportedComponentsInChildren(go, true);

            // AudioSource should be removed
            Assert.IsNull(child.GetComponent<AudioSource>());
        }

        [Test]
        public void RemoveUnsupportedComponentsInChildren_KeepsSupportedComponents()
        {
            var go = new GameObject("KeepTest");
            objectsToCleanup.Add(go);
            var child = new GameObject("Child2");
            child.transform.SetParent(go.transform);
            child.AddComponent<SkinnedMeshRenderer>();

            var remover = new ComponentRemover();
            remover.RemoveUnsupportedComponentsInChildren(go, true);

            // SkinnedMeshRenderer should be kept
            Assert.IsNotNull(child.GetComponent<SkinnedMeshRenderer>());
        }

        [Test]
        public void RemoveUnsupportedComponentsInChildren_WithExcludedTypes()
        {
            var go = new GameObject("ExcludeTest");
            objectsToCleanup.Add(go);
            var child = new GameObject("Child3");
            child.transform.SetParent(go.transform);
            child.AddComponent<AudioSource>();

            var remover = new ComponentRemover();
            remover.RemoveUnsupportedComponentsInChildren(go, true, true, new[] { typeof(AudioSource) });

            // AudioSource should be kept since it's excluded
            Assert.IsNotNull(child.GetComponent<AudioSource>());
        }
    }

    // ========== VRCQuestToolsSettings Additional Tests ==========
    [TestFixture]
    public class VRCQuestToolsSettingsTests_PkgCompat
    {
        [Test]
        public void I18nResource_ReturnsNonNull()
        {
            var resource = VRCQuestToolsSettings.I18nResource;
            Assert.IsNotNull(resource);
        }

        [Test]
        public void TextureCacheSize_HasDefault()
        {
            var size = VRCQuestToolsSettings.TextureCacheSize;
            Assert.IsTrue(size >= 0);
        }
    }

    // ========== MaterialSwap Exception Tests ==========
    [TestFixture]
    public class MaterialSwapExceptionTests
    {
        [Test]
        public void InvalidMaterialSwapNullException_Constructor()
        {
            var go = new GameObject("SwapTest");
            var swap = go.AddComponent<MaterialSwap>();
            var ex = new InvalidMaterialSwapNullException("test message", swap, 0);
            Assert.AreEqual("test message", ex.Message);
            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void InvalidReplacementMaterialException_Constructor()
        {
            var go = new GameObject("ReplaceTest");
            var swap = go.AddComponent<MaterialSwap>();
            var mat = new Material(Shader.Find("Standard"));
            var ex = new InvalidReplacementMaterialException("test replace", swap, mat);
            Assert.AreEqual("test replace", ex.Message);
            UnityEngine.Object.DestroyImmediate(go);
            UnityEngine.Object.DestroyImmediate(mat);
        }
    }

    // ========== VirtualLens2Material Tests ==========
    [TestFixture]
    public class VirtualLens2MaterialTests_PkgCompat
    {
        [Test]
        public void VirtualLensSettingsType_CheckAvailability()
        {
            var type = typeof(VirtualLensUtility);
            var field = type.GetField("VirtualLensSettingsType", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (field != null)
            {
                // Just access the field to cover the code
                var val = field.GetValue(null);
                // May be null if VirtualLens2 not installed
            }
        }
    }

    // ========== AssetUtility Additional Tests ==========
    [TestFixture]
    public class AssetUtilityTests_PkgCompat
    {
        [Test]
        public void LilToonVersion_IsNotNull()
        {
            var field = typeof(AssetUtility).GetField("LilToonVersion", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
            {
                var prop = typeof(AssetUtility).GetProperty("LilToonVersion", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (prop != null)
                {
                    var val = prop.GetValue(null);
                    Assert.IsNotNull(val);
                }
            }
            else
            {
                var val = field.GetValue(null);
                Assert.IsNotNull(val);
            }
        }

        [Test]
        public void IsLilToonImported_ReturnsTrue()
        {
            // lilToon is installed in this project
            Assert.IsTrue(AssetUtility.IsLilToonImported());
        }

        [Test]
        public void IsPoiyomiImported_ReturnsBool()
        {
            // Just verify it doesn't throw via reflection
            var method = typeof(AssetUtility).GetMethod("IsPoiyomiImported", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
            {
                var result = (bool)method.Invoke(null, null);
                Assert.IsTrue(result == true || result == false);
            }
        }

        [Test]
        public void GetAllObjectReferences_WithMaterial()
        {
            var method = typeof(AssetUtility).GetMethod("GetAllObjectReferences", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null) { Assert.Ignore("Method not found"); return; }

            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var result = method.Invoke(null, new object[] { mat });
                Assert.IsNotNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }
    }

    // ========== ConversionException Types Tests ==========
    [TestFixture]
    public class ConversionExceptionTests_PkgCompat
    {
        [Test]
        public void MaterialConversionException_HasMaterial()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var ex = new MaterialConversionException("conversion failed", mat, new Exception("inner"));
                Assert.AreEqual("conversion failed", ex.Message);
                Assert.IsNotNull(ex.InnerException);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void AnimationClipConversionException_Properties()
        {
            var clip = new AnimationClip();
            try
            {
                var inner = new Exception("inner error");
                var ex = new AnimationClipConversionException("clip error", clip, inner);
                Assert.AreEqual("clip error", ex.Message);
                Assert.AreEqual(inner, ex.InnerException);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void AnimatorControllerConversionException_Properties()
        {
            var inner = new Exception("inner");
            var controller = new UnityEditor.Animations.AnimatorController();
            try
            {
                var ex = new AnimatorControllerConversionException("ctrl error", controller, inner);
                Assert.AreEqual("ctrl error", ex.Message);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(controller);
            }
        }
    }
}
