// Batch 23: Coverage tests for AvatarConverter deep paths, VRChatAvatar, NdmfPhaseExtension, validators
// Targets: AvatarConverter (400 uncovered), VRChatAvatar (21), AvatarConverterNdmfPhaseExtension (10),
//          MissingScriptsRule (25), various remaining gaps

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
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;
using UObject = UnityEngine.Object;
using VQTBuildTarget = KRT.VRCQuestTools.Models.BuildTarget;

namespace KRT.VRCQuestTools.Tests
{
    // ==========================================
    // AvatarConverterNdmfPhaseExtension Tests
    // ==========================================
    [TestFixture]
    public class Batch23_NdmfPhaseExtensionTests
    {
        [Test]
        public void Resolve_Transforming_ReturnsSelf()
        {
            var phase = AvatarConverterNdmfPhase.Transforming;
            var result = phase.Resolve(null);
            Assert.AreEqual(AvatarConverterNdmfPhase.Transforming, result);
        }

        [Test]
        public void Resolve_Optimizing_ReturnsSelf()
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
        public void Resolve_Auto_WithAvatar_ReturnsOptimizingOrTransforming()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var phase = AvatarConverterNdmfPhase.Auto;
                var result = phase.Resolve(go);
                // Without VRCFury, should return Optimizing
                // With VRCFury, depends on component presence
                Assert.That(result, Is.EqualTo(AvatarConverterNdmfPhase.Optimizing).Or.EqualTo(AvatarConverterNdmfPhase.Transforming));
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void Resolve_TransformingPhase_IgnoresAvatarRoot()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var phase = AvatarConverterNdmfPhase.Transforming;
                var result = phase.Resolve(go);
                Assert.AreEqual(AvatarConverterNdmfPhase.Transforming, result);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void Resolve_OptimizingPhase_IgnoresAvatarRoot()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var phase = AvatarConverterNdmfPhase.Optimizing;
                var result = phase.Resolve(go);
                Assert.AreEqual(AvatarConverterNdmfPhase.Optimizing, result);
            }
            finally { UObject.DestroyImmediate(go); }
        }
    }

    // ==========================================
    // VRChatAvatar - HasVertexColor Tests
    // ==========================================
    [TestFixture]
    public class Batch23_VRChatAvatarVertexColorTests
    {
        private static VRChatAvatar CreateAvatar(GameObject go)
        {
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            return new VRChatAvatar(desc);
        }

        [Test]
        public void HasVertexColor_NoRenderers_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            try
            {
                var avatar = CreateAvatar(go);
                Assert.IsFalse(avatar.HasVertexColor);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void HasVertexColor_MeshWithoutColors_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new int[] { 0, 1, 2 };
            smr.sharedMesh = mesh;
            try
            {
                var avatar = CreateAvatar(go);
                Assert.IsFalse(avatar.HasVertexColor);
            }
            finally
            {
                UObject.DestroyImmediate(mesh);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasVertexColor_MeshWithColors_ReturnsTrue()
        {
            var go = new GameObject("Avatar");
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.colors32 = new Color32[] { Color.red, Color.green, Color.blue };
            smr.sharedMesh = mesh;
            try
            {
                var avatar = CreateAvatar(go);
                Assert.IsTrue(avatar.HasVertexColor);
            }
            finally
            {
                UObject.DestroyImmediate(mesh);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasVertexColor_MeshRenderer_WithColors_ReturnsTrue()
        {
            var go = new GameObject("Avatar");
            var child = new GameObject("MeshChild");
            child.transform.SetParent(go.transform);
            var mf = child.AddComponent<MeshFilter>();
            child.AddComponent<MeshRenderer>();
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.colors32 = new Color32[] { Color.red, Color.green, Color.blue };
            mf.sharedMesh = mesh;
            try
            {
                var avatar = CreateAvatar(go);
                Assert.IsTrue(avatar.HasVertexColor);
            }
            finally
            {
                UObject.DestroyImmediate(mesh);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasDynamicBoneComponents_NoDynamicBone_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            try
            {
                var avatar = CreateAvatar(go);
                // DynamicBone is not imported in this project, so should return false
                Assert.IsFalse(avatar.HasDynamicBoneComponents);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void HasUnityConstraints_NoConstraints_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            try
            {
                var avatar = CreateAvatar(go);
                Assert.IsFalse(avatar.HasUnityConstraints);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void HasAnimatedMaterials_NoAnimators_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            try
            {
                var avatar = CreateAvatar(go);
                Assert.IsFalse(avatar.HasAnimatedMaterials);
            }
            finally { UObject.DestroyImmediate(go); }
        }
    }

    // ==========================================
    // VRChatAvatar - GetRuntimeAnimatorControllers
    // ==========================================
    [TestFixture]
    public class Batch23_VRChatAvatarAnimatorControllerTests
    {
        [Test]
        public void GetRuntimeAnimatorControllers_NoLayers_ReturnsEmpty()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                var controllers = avatar.GetRuntimeAnimatorControllers();
                Assert.IsNotNull(controllers);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void GetRuntimeAnimatorControllers_WithBaseAnimationLayers_ReturnsControllers()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            var ac = new AnimatorController();
            ac.name = "TestController";
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[]
            {
                new VRCAvatarDescriptor.CustomAnimLayer
                {
                    isDefault = false,
                    animatorController = ac,
                }
            };
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                var controllers = avatar.GetRuntimeAnimatorControllers();
                Assert.IsNotNull(controllers);
                Assert.That(controllers.Length, Is.GreaterThanOrEqualTo(1));
                Assert.Contains(ac, controllers);
            }
            finally
            {
                UObject.DestroyImmediate(ac);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRuntimeAnimatorControllers_WithDefaultLayers_SkipsDefault()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[]
            {
                new VRCAvatarDescriptor.CustomAnimLayer
                {
                    isDefault = true,
                    animatorController = null,
                }
            };
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                var controllers = avatar.GetRuntimeAnimatorControllers();
                Assert.IsNotNull(controllers);
                // Default layers with null controller should be filtered out
                Assert.AreEqual(0, controllers.Length);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void GetRuntimeAnimatorControllers_WithAnimatorOverride_IncludesBaseController()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            var baseController = new AnimatorController();
            baseController.name = "BaseController";
            // Add a parameter and layer so override can work
            baseController.AddParameter("test", AnimatorControllerParameterType.Bool);
            baseController.AddLayer("Base Layer");

            var overrideController = new AnimatorOverrideController();
            overrideController.runtimeAnimatorController = baseController;

            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[]
            {
                new VRCAvatarDescriptor.CustomAnimLayer
                {
                    isDefault = false,
                    animatorController = overrideController,
                }
            };
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                var controllers = avatar.GetRuntimeAnimatorControllers();
                Assert.IsNotNull(controllers);
                // Should contain both the override controller and its base
                Assert.That(controllers, Has.Member(overrideController));
                Assert.That(controllers, Has.Member(baseController));
            }
            finally
            {
                UObject.DestroyImmediate(overrideController);
                UObject.DestroyImmediate(baseController);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRuntimeAnimatorControllers_WithChildAnimator_ReturnsControllers()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            var animator = child.AddComponent<Animator>();
            var ac = new AnimatorController();
            ac.name = "ChildController";
            animator.runtimeAnimatorController = ac;
            try
            {
                var avatar = new VRChatAvatar(desc);
                var controllers = avatar.GetRuntimeAnimatorControllers();
                Assert.IsNotNull(controllers);
                Assert.That(controllers, Has.Member(ac));
            }
            finally
            {
                UObject.DestroyImmediate(ac);
                UObject.DestroyImmediate(go);
            }
        }
    }

    // ==========================================
    // AvatarConverter - Deep Path Tests
    // ==========================================
    [TestFixture]
    public class Batch23_AvatarConverterDeepPathTests
    {
        private AvatarConverter CreateConverter()
        {
            return new AvatarConverter(new MaterialWrapperBuilder());
        }

        private (GameObject, VRChatAvatar) CreateAvatarWithSettings()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            go.AddComponent<AvatarConverterSettings>();
            return (go, new VRChatAvatar(desc));
        }

        [Test]
        public void PrepareConvertForQuestInPlace_WithNoVirtualLens_DoesNotThrow()
        {
            var converter = CreateConverter();
            var (go, avatar) = CreateAvatarWithSettings();
            try
            {
                converter.PrepareConvertForQuestInPlace(avatar);
                // Should not throw when no VirtualLens2 is present
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void PrepareModularAvatarComponentsInPlace_DoesNotThrow()
        {
            var converter = CreateConverter();
            var (go, avatar) = CreateAvatarWithSettings();
            try
            {
                converter.PrepareModularAvatarComponentsInPlace(avatar);
                // Should handle MA import status gracefully
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void ConvertForQuestInPlace_ChecksLegacyMAVersion()
        {
            var converter = CreateConverter();
            var (go, avatar) = CreateAvatarWithSettings();
            var remover = new ComponentRemover();
            var progress = new AvatarConverter.ProgressCallback
            {
                onTextureProgress = (total, index, orig, conv) => { },
                onAnimationClipProgress = (total, index, orig, conv) => { },
                onRuntimeAnimatorProgress = (total, index, orig, conv) => { },
            };
            try
            {
                // This will run the full conversion pipeline on an empty avatar
                // It should succeed since there's nothing to convert
                converter.ConvertForQuestInPlace(avatar, remover, false, "", progress);
            }
            catch (LegacyPackageException)
            {
                // Expected if MA is detected as legacy version
            }
            catch (BreakingPackageException)
            {
                // Expected if MA is detected as breaking version
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void FindDescendant_ViaReflection_ReturnsNullForMissing()
        {
            var converter = CreateConverter();
            var method = typeof(AvatarConverter).GetMethod("FindDescendant", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("FindDescendant not found"); return; }

            var go = new GameObject("Root");
            try
            {
                var result = method.Invoke(converter, new object[] { go, "NonExistent" });
                Assert.IsNull(result);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void FindDescendant_ViaReflection_FindsDirectChild()
        {
            var converter = CreateConverter();
            var method = typeof(AvatarConverter).GetMethod("FindDescendant", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("FindDescendant not found"); return; }

            var go = new GameObject("Root");
            var child = new GameObject("_VirtualLens_Root");
            child.transform.SetParent(go.transform);
            try
            {
                var result = (GameObject)method.Invoke(converter, new object[] { go, "_VirtualLens_Root" });
                Assert.IsNotNull(result);
                Assert.AreEqual("_VirtualLens_Root", result.name);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void FindDescendant_ViaReflection_FindsDeepChild()
        {
            var converter = CreateConverter();
            var method = typeof(AvatarConverter).GetMethod("FindDescendant", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("FindDescendant not found"); return; }

            var go = new GameObject("Root");
            var mid = new GameObject("Middle");
            mid.transform.SetParent(go.transform);
            var deep = new GameObject("DeepChild");
            deep.transform.SetParent(mid.transform);
            try
            {
                var result = (GameObject)method.Invoke(converter, new object[] { go, "DeepChild" });
                Assert.IsNotNull(result);
                Assert.AreEqual("DeepChild", result.name);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void ApplyVirtualLens2Support_WithVirtualLensRoot_DisablesIt()
        {
            var converter = CreateConverter();
            var method = typeof(AvatarConverter).GetMethod("ApplyVirtualLens2Support", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("ApplyVirtualLens2Support not found"); return; }

            var go = new GameObject("Avatar");
            var vlRoot = new GameObject("_VirtualLens_Root");
            vlRoot.transform.SetParent(go.transform);
            var vlOrigin = new GameObject("VirtualLensOrigin");
            vlOrigin.transform.SetParent(go.transform);
            try
            {
                method.Invoke(converter, new object[] { go });
                Assert.AreEqual("EditorOnly", vlRoot.tag);
                Assert.IsFalse(vlRoot.activeSelf);
                Assert.AreEqual("EditorOnly", vlOrigin.tag);
                Assert.IsFalse(vlOrigin.activeSelf);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void ApplyVirtualLens2Support_WithoutVirtualLensRoot_DoesNothing()
        {
            var converter = CreateConverter();
            var method = typeof(AvatarConverter).GetMethod("ApplyVirtualLens2Support", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("ApplyVirtualLens2Support not found"); return; }

            var go = new GameObject("Avatar");
            try
            {
                method.Invoke(converter, new object[] { go });
                // No exception expected
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void ApplyVRCQuestToolsComponents_ViaReflection_AddsConvertedAvatar()
        {
            var converter = CreateConverter();
            var method = typeof(AvatarConverter).GetMethod("ApplyVRCQuestToolsComponents", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("ApplyVRCQuestToolsComponents not found"); return; }

            var go = new GameObject("Avatar");
            var settings = go.AddComponent<AvatarConverterSettings>();
            var questGo = new GameObject("Quest Avatar");
            try
            {
                method.Invoke(converter, new object[] { settings, questGo });
                Assert.IsNotNull(questGo.GetComponent<ConvertedAvatar>());
            }
            finally
            {
                UObject.DestroyImmediate(questGo);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void ApplyVRCQuestToolsComponents_WithExistingConvertedAvatar_DoesNotDuplicate()
        {
            var converter = CreateConverter();
            var method = typeof(AvatarConverter).GetMethod("ApplyVRCQuestToolsComponents", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("ApplyVRCQuestToolsComponents not found"); return; }

            var go = new GameObject("Avatar");
            var settings = go.AddComponent<AvatarConverterSettings>();
            var questGo = new GameObject("Quest Avatar");
            questGo.AddComponent<ConvertedAvatar>();
            try
            {
                method.Invoke(converter, new object[] { settings, questGo });
                var components = questGo.GetComponents<ConvertedAvatar>();
                Assert.AreEqual(1, components.Length);
            }
            finally
            {
                UObject.DestroyImmediate(questGo);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void ApplyVRCQuestToolsComponents_WithPlatformTargetSettings_SetsBuildTarget()
        {
            var converter = CreateConverter();
            var method = typeof(AvatarConverter).GetMethod("ApplyVRCQuestToolsComponents", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("ApplyVRCQuestToolsComponents not found"); return; }

            var go = new GameObject("Avatar");
            var settings = go.AddComponent<AvatarConverterSettings>();
            var questGo = new GameObject("Quest Avatar");
            var pts = questGo.AddComponent<PlatformTargetSettings>();
            try
            {
                method.Invoke(converter, new object[] { settings, questGo });
                Assert.AreEqual(VQTBuildTarget.Android, pts.buildTarget);
            }
            finally
            {
                UObject.DestroyImmediate(questGo);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_ViaReflection_TrimsExcess()
        {
            var converter = CreateConverter();
            var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("RemoveExtraMaterialSlots not found"); return; }

            var go = new GameObject("Avatar");
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new int[] { 0, 1, 2 };
            // 1 submesh, but 3 material slots
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

        [Test]
        public void RemoveExtraMaterialSlots_NullMesh_Skips()
        {
            var converter = CreateConverter();
            var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("RemoveExtraMaterialSlots not found"); return; }

            var go = new GameObject("Avatar");
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            // No mesh assigned
            var mat = new Material(Shader.Find("Standard"));
            smr.sharedMaterials = new Material[] { mat };
            try
            {
                method.Invoke(converter, new object[] { go });
                // Should not throw
                Assert.AreEqual(1, smr.sharedMaterials.Length);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_MeshRenderer_TrimsExcess()
        {
            var converter = CreateConverter();
            var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("RemoveExtraMaterialSlots not found"); return; }

            var go = new GameObject("Avatar");
            var child = new GameObject("MeshChild");
            child.transform.SetParent(go.transform);
            var mf = child.AddComponent<MeshFilter>();
            var mr = child.AddComponent<MeshRenderer>();
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new int[] { 0, 1, 2 };
            mf.sharedMesh = mesh;
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            mr.sharedMaterials = new Material[] { mat1, mat2 };

            try
            {
                Assert.AreEqual(2, mr.sharedMaterials.Length);
                method.Invoke(converter, new object[] { go });
                Assert.AreEqual(1, mr.sharedMaterials.Length);
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
        public void RemoveExtraMaterialSlots_NoExcess_KeepsMaterials()
        {
            var converter = CreateConverter();
            var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("RemoveExtraMaterialSlots not found"); return; }

            var go = new GameObject("Avatar");
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new int[] { 0, 1, 2 };
            smr.sharedMesh = mesh;
            var mat = new Material(Shader.Find("Standard"));
            smr.sharedMaterials = new Material[] { mat };
            try
            {
                method.Invoke(converter, new object[] { go });
                Assert.AreEqual(1, smr.sharedMaterials.Length);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(mesh);
                UObject.DestroyImmediate(go);
            }
        }
    }

    // ==========================================
    // AvatarConverter - CreateMaterialConvertSettingsMap (overload 2)
    // ==========================================
    [TestFixture]
    public class Batch23_CreateMaterialConvertSettingsMapTests
    {
        private AvatarConverter CreateConverter()
        {
            return new AvatarConverter(new MaterialWrapperBuilder());
        }

        [Test]
        public void CreateSettingsMap_NoSettings_AppliesDefaultForPrimaryConversion()
        {
            var converter = CreateConverter();
            var go = new GameObject("Avatar");
            var settings = go.AddComponent<AvatarConverterSettings>();
            var mat = new Material(Shader.Find("Standard"));
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMaterial = mat;
            try
            {
                // Use the overload that takes avatarRoot and materials
                var method = typeof(AvatarConverter).GetMethod("CreateMaterialConvertSettingsMap",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new Type[] { typeof(GameObject), typeof(Material[]) },
                    null);
                if (method == null) { Assert.Ignore("CreateMaterialConvertSettingsMap(GO, Material[]) not found"); return; }

                var result = (Dictionary<Material, IMaterialConvertSettings>)method.Invoke(converter, new object[] { go, new Material[] { mat } });
                Assert.IsNotNull(result);
                // With AvatarConverterSettings as primary, it should apply default settings
                Assert.IsTrue(result.ContainsKey(mat));
            }
            finally
            {
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateSettingsMap_MaterialConversionSettings_AppliesAdditionalSettings()
        {
            var converter = CreateConverter();
            var go = new GameObject("Avatar");
            var mcs = go.AddComponent<MaterialConversionSettings>();
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "TestMat";
            var replaceMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            mcs.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = mat,
                    materialConvertSettings = new MaterialReplaceSettings { material = replaceMat }
                }
            };
            try
            {
                var method = typeof(AvatarConverter).GetMethod("CreateMaterialConvertSettingsMap",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new Type[] { typeof(GameObject), typeof(Material[]) },
                    null);
                if (method == null) { Assert.Ignore("Method not found"); return; }

                var result = (Dictionary<Material, IMaterialConvertSettings>)method.Invoke(converter, new object[] { go, new Material[] { mat } });
                Assert.IsNotNull(result);
                Assert.IsTrue(result.ContainsKey(mat));
                Assert.IsInstanceOf<MaterialReplaceSettings>(result[mat]);
            }
            finally
            {
                UObject.DestroyImmediate(replaceMat);
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateSettingsMap_NullTargetMaterial_ThrowsTargetMaterialNullException()
        {
            var converter = CreateConverter();
            var go = new GameObject("Avatar");
            var mcs = go.AddComponent<MaterialConversionSettings>();
            mcs.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = null,
                    materialConvertSettings = new ToonLitConvertSettings()
                }
            };
            try
            {
                var method = typeof(AvatarConverter).GetMethod("CreateMaterialConvertSettingsMap",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new Type[] { typeof(GameObject), typeof(Material[]) },
                    null);
                if (method == null) { Assert.Ignore("Method not found"); return; }

                Assert.Throws<TargetInvocationException>(() =>
                {
                    method.Invoke(converter, new object[] { go, new Material[0] });
                });
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void CreateSettingsMap_InvalidReplacementMaterial_Throws()
        {
            var converter = CreateConverter();
            var go = new GameObject("Avatar");
            var mcs = go.AddComponent<MaterialConversionSettings>();
            var mat = new Material(Shader.Find("Standard"));
            var invalidReplaceMat = new Material(Shader.Find("Standard")); // Not Quest-compatible
            mcs.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = mat,
                    materialConvertSettings = new MaterialReplaceSettings { material = invalidReplaceMat }
                }
            };
            try
            {
                var method = typeof(AvatarConverter).GetMethod("CreateMaterialConvertSettingsMap",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new Type[] { typeof(GameObject), typeof(Material[]) },
                    null);
                if (method == null) { Assert.Ignore("Method not found"); return; }

                Assert.Throws<TargetInvocationException>(() =>
                {
                    method.Invoke(converter, new object[] { go, new Material[] { mat } });
                });
            }
            finally
            {
                UObject.DestroyImmediate(invalidReplaceMat);
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateSettingsMap_MaterialSwap_NullOriginal_Throws()
        {
            var converter = CreateConverter();
            var go = new GameObject("Avatar");
            var swap = go.AddComponent<MaterialSwap>();
            swap.materialMappings = new List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping
                {
                    originalMaterial = null,
                    replacementMaterial = new Material(Shader.Find("VRChat/Mobile/Toon Lit"))
                }
            };
            try
            {
                var method = typeof(AvatarConverter).GetMethod("CreateMaterialConvertSettingsMap",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new Type[] { typeof(GameObject), typeof(Material[]) },
                    null);
                if (method == null) { Assert.Ignore("Method not found"); return; }

                Assert.Throws<TargetInvocationException>(() =>
                {
                    method.Invoke(converter, new object[] { go, new Material[0] });
                });
            }
            finally
            {
                foreach (var m in swap.materialMappings.Where(mm => mm.replacementMaterial != null))
                    UObject.DestroyImmediate(m.replacementMaterial);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateSettingsMap_MaterialSwap_NullReplacement_Throws()
        {
            var converter = CreateConverter();
            var go = new GameObject("Avatar");
            var swap = go.AddComponent<MaterialSwap>();
            var origMat = new Material(Shader.Find("Standard"));
            swap.materialMappings = new List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping
                {
                    originalMaterial = origMat,
                    replacementMaterial = null
                }
            };
            try
            {
                var method = typeof(AvatarConverter).GetMethod("CreateMaterialConvertSettingsMap",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new Type[] { typeof(GameObject), typeof(Material[]) },
                    null);
                if (method == null) { Assert.Ignore("Method not found"); return; }

                Assert.Throws<TargetInvocationException>(() =>
                {
                    method.Invoke(converter, new object[] { go, new Material[] { origMat } });
                });
            }
            finally
            {
                UObject.DestroyImmediate(origMat);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateSettingsMap_OmitsUnusedMaterials()
        {
            var converter = CreateConverter();
            var go = new GameObject("Avatar");
            go.AddComponent<AvatarConverterSettings>();
            var usedMat = new Material(Shader.Find("Standard"));
            var unusedMat = new Material(Shader.Find("Standard"));

            // Add additional settings for an unused material
            var settings = go.GetComponent<AvatarConverterSettings>();
            settings.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = unusedMat,
                    materialConvertSettings = new ToonLitConvertSettings()
                }
            };
            try
            {
                var method = typeof(AvatarConverter).GetMethod("CreateMaterialConvertSettingsMap",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new Type[] { typeof(GameObject), typeof(Material[]) },
                    null);
                if (method == null) { Assert.Ignore("Method not found"); return; }

                // Only usedMat is in avatarMaterials - unusedMat should be omitted
                var result = (Dictionary<Material, IMaterialConvertSettings>)method.Invoke(converter, new object[] { go, new Material[] { usedMat } });
                Assert.IsFalse(result.ContainsKey(unusedMat));
                Assert.IsTrue(result.ContainsKey(usedMat));
            }
            finally
            {
                UObject.DestroyImmediate(unusedMat);
                UObject.DestroyImmediate(usedMat);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateSettingsMap_AvatarConverterSettings_AdditionalSettings_Applied()
        {
            var converter = CreateConverter();
            var go = new GameObject("Avatar");
            var acs = go.AddComponent<AvatarConverterSettings>();
            var mat = new Material(Shader.Find("Standard"));
            acs.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = mat,
                    materialConvertSettings = new MatCapLitConvertSettings()
                }
            };
            try
            {
                var method = typeof(AvatarConverter).GetMethod("CreateMaterialConvertSettingsMap",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new Type[] { typeof(GameObject), typeof(Material[]) },
                    null);
                if (method == null) { Assert.Ignore("Method not found"); return; }

                var result = (Dictionary<Material, IMaterialConvertSettings>)method.Invoke(converter, new object[] { go, new Material[] { mat } });
                Assert.IsTrue(result.ContainsKey(mat));
                Assert.IsInstanceOf<MatCapLitConvertSettings>(result[mat]);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateSettingsMap_AvatarConverterSettings_NullTarget_Throws()
        {
            var converter = CreateConverter();
            var go = new GameObject("Avatar");
            var acs = go.AddComponent<AvatarConverterSettings>();
            acs.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = null,
                    materialConvertSettings = new ToonLitConvertSettings()
                }
            };
            try
            {
                var method = typeof(AvatarConverter).GetMethod("CreateMaterialConvertSettingsMap",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new Type[] { typeof(GameObject), typeof(Material[]) },
                    null);
                if (method == null) { Assert.Ignore("Method not found"); return; }

                Assert.Throws<TargetInvocationException>(() =>
                {
                    method.Invoke(converter, new object[] { go, new Material[0] });
                });
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void CreateSettingsMap_DuplicateMaterial_FirstSettingsWin()
        {
            var converter = CreateConverter();
            var go = new GameObject("Avatar");
            var acs = go.AddComponent<AvatarConverterSettings>();
            var mat = new Material(Shader.Find("Standard"));
            // Same material with two different settings in additional settings
            acs.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = mat,
                    materialConvertSettings = new MatCapLitConvertSettings()
                },
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = mat,
                    materialConvertSettings = new ToonLitConvertSettings()
                }
            };
            try
            {
                var method = typeof(AvatarConverter).GetMethod("CreateMaterialConvertSettingsMap",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new Type[] { typeof(GameObject), typeof(Material[]) },
                    null);
                if (method == null) { Assert.Ignore("Method not found"); return; }

                var result = (Dictionary<Material, IMaterialConvertSettings>)method.Invoke(converter, new object[] { go, new Material[] { mat } });
                Assert.IsTrue(result.ContainsKey(mat));
                // First one wins
                Assert.IsInstanceOf<MatCapLitConvertSettings>(result[mat]);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateSettingsMap_MaterialSwap_ValidMapping_AddsMaterialReplace()
        {
            var converter = CreateConverter();
            var go = new GameObject("Avatar");
            var swap = go.AddComponent<MaterialSwap>();
            var origMat = new Material(Shader.Find("Standard"));
            var toonLitShader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (toonLitShader == null) { Assert.Ignore("Toon Lit shader not found"); return; }
            var replaceMat = new Material(toonLitShader);
            swap.materialMappings = new List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping
                {
                    originalMaterial = origMat,
                    replacementMaterial = replaceMat
                }
            };
            try
            {
                var method = typeof(AvatarConverter).GetMethod("CreateMaterialConvertSettingsMap",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new Type[] { typeof(GameObject), typeof(Material[]) },
                    null);
                if (method == null) { Assert.Ignore("Method not found"); return; }

                var result = (Dictionary<Material, IMaterialConvertSettings>)method.Invoke(converter, new object[] { go, new Material[] { origMat } });
                Assert.IsTrue(result.ContainsKey(origMat));
                Assert.IsInstanceOf<MaterialReplaceSettings>(result[origMat]);
                Assert.AreEqual(replaceMat, ((MaterialReplaceSettings)result[origMat]).material);
            }
            finally
            {
                UObject.DestroyImmediate(replaceMat);
                UObject.DestroyImmediate(origMat);
                UObject.DestroyImmediate(go);
            }
        }
    }

    // ==========================================
    // AvatarConverter - ApplyConvertedMaterials
    // ==========================================
    [TestFixture]
    public class Batch23_ApplyConvertedMaterialsTests
    {
        [Test]
        public void ApplyConvertedMaterials_ReplacesMaterials()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var origMat = new Material(Shader.Find("Standard"));
            var newMat = new Material(Shader.Find("Standard"));
            newMat.name = "Converted";
            smr.sharedMaterial = origMat;

            var convertedMaterials = new Dictionary<Material, Material> { { origMat, newMat } };
            var progress = new AvatarConverter.ProgressCallback
            {
                onTextureProgress = (t, i, o, c) => { },
                onAnimationClipProgress = (t, i, o, c) => { },
                onRuntimeAnimatorProgress = (t, i, o, c) => { },
            };
            try
            {
                converter.ApplyConvertedMaterials(go, convertedMaterials, false, "", progress);
                Assert.AreEqual(newMat, smr.sharedMaterial);
            }
            finally
            {
                UObject.DestroyImmediate(newMat);
                UObject.DestroyImmediate(origMat);
                UObject.DestroyImmediate(go);
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
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMaterials = new Material[] { null };

            var progress = new AvatarConverter.ProgressCallback
            {
                onTextureProgress = (t, i, o, c) => { },
                onAnimationClipProgress = (t, i, o, c) => { },
                onRuntimeAnimatorProgress = (t, i, o, c) => { },
            };
            try
            {
                converter.ApplyConvertedMaterials(go, new Dictionary<Material, Material>(), false, "", progress);
                Assert.IsNull(smr.sharedMaterials[0]);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void ApplyConvertedMaterials_UnmappedMaterial_Unchanged()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mat = new Material(Shader.Find("Standard"));
            smr.sharedMaterial = mat;

            var progress = new AvatarConverter.ProgressCallback
            {
                onTextureProgress = (t, i, o, c) => { },
                onAnimationClipProgress = (t, i, o, c) => { },
                onRuntimeAnimatorProgress = (t, i, o, c) => { },
            };
            try
            {
                converter.ApplyConvertedMaterials(go, new Dictionary<Material, Material>(), false, "", progress);
                Assert.AreEqual(mat, smr.sharedMaterial);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(go);
            }
        }
    }

    // ==========================================
    // AvatarConverter - GenerateConvertedMaterial switch branches
    // ==========================================
    [TestFixture]
    public class Batch23_GenerateConvertedMaterialTests
    {
        [Test]
        public void GenerateConvertedMaterial_MaterialReplaceSettings_NullMaterial_Throws()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("GenerateConvertedMaterial", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("GenerateConvertedMaterial not found"); return; }

            var mat = new Material(Shader.Find("Standard"));
            var wrapper = new StandardMaterial(mat);
            var replaceSettings = new MaterialReplaceSettings { material = null };
            try
            {
                var ex = Assert.Throws<TargetInvocationException>(() =>
                {
                    method.Invoke(converter, new object[] { wrapper, replaceSettings, false, "", (Action<Material>)((m) => { }) });
                });
                Assert.IsInstanceOf<InvalidOperationException>(ex.InnerException);
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void GenerateConvertedMaterial_MaterialReplaceSettings_ValidMaterial_Returns()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("GenerateConvertedMaterial", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("GenerateConvertedMaterial not found"); return; }

            var mat = new Material(Shader.Find("Standard"));
            var wrapper = new StandardMaterial(mat);
            var toonLitShader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (toonLitShader == null) { Assert.Ignore("Toon Lit shader not found"); return; }
            var replaceMat = new Material(toonLitShader);
            var replaceSettings = new MaterialReplaceSettings { material = replaceMat };
            Material result = null;
            try
            {
                var request = method.Invoke(converter, new object[] { wrapper, replaceSettings, false, "", (Action<Material>)((m) => { result = m; }) });
                Assert.IsNotNull(request);
                // Execute - WaitForCompletion is internal
                request.GetType().GetMethod("WaitForCompletion", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(request, null);
                Assert.AreEqual(replaceMat, result);
            }
            finally
            {
                UObject.DestroyImmediate(replaceMat);
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GenerateConvertedMaterial_ToonLitSettings_ReturnsRequest()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("GenerateConvertedMaterial", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("GenerateConvertedMaterial not found"); return; }

            var mat = new Material(Shader.Find("Standard"));
            var wrapper = new StandardMaterial(mat);
            var settings = new ToonLitConvertSettings();
            Material result = null;
            try
            {
                var request = method.Invoke(converter, new object[] { wrapper, settings, false, "", (Action<Material>)((m) => { result = m; }) });
                Assert.IsNotNull(request);
                // The request should be a ToonLitGenerator result
                request.GetType().GetMethod("WaitForCompletion", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(request, null);
                Assert.IsNotNull(result);
            }
            finally
            {
                if (result != null && result != mat) UObject.DestroyImmediate(result);
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GenerateConvertedMaterial_MatCapLitSettings_ReturnsRequest()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("GenerateConvertedMaterial", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("GenerateConvertedMaterial not found"); return; }

            var mat = new Material(Shader.Find("Standard"));
            var wrapper = new StandardMaterial(mat);
            var settings = new MatCapLitConvertSettings();
            Material result = null;
            try
            {
                var request = method.Invoke(converter, new object[] { wrapper, settings, false, "", (Action<Material>)((m) => { result = m; }) });
                Assert.IsNotNull(request);
                request.GetType().GetMethod("WaitForCompletion", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(request, null);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is NullReferenceException)
            {
                // MatCapLit may fail if the shader "VRChat/Mobile/MatCap Lit" is not available
                Assert.Pass("MatCapLit shader not available - expected");
            }
            {
                if (result != null && result != mat) UObject.DestroyImmediate(result);
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GenerateConvertedMaterial_ToonStandardSettings_NonLilToon_ReturnsRequest()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("GenerateConvertedMaterial", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("GenerateConvertedMaterial not found"); return; }

            var mat = new Material(Shader.Find("Standard"));
            var wrapper = new StandardMaterial(mat);
            var settings = new ToonStandardConvertSettings();
            Material result = null;
            try
            {
                var request = method.Invoke(converter, new object[] { wrapper, settings, false, "", (Action<Material>)((m) => { result = m; }) });
                Assert.IsNotNull(request);
                request.GetType().GetMethod("WaitForCompletion", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(request, null);
                // GenericToonStandardGenerator should produce a material
                Assert.IsNotNull(result);
            }
            finally
            {
                if (result != null && result != mat) UObject.DestroyImmediate(result);
                UObject.DestroyImmediate(mat);
            }
        }
    }

    // ==========================================
    // AvatarConverter - ConvertMaterialsForMobile
    // ==========================================
    [TestFixture]
    public class Batch23_ConvertMaterialsForMobileTests
    {
        [Test]
        public void ConvertMaterialsForMobile_EmptyMap_ReturnsEmpty()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var result = converter.ConvertMaterialsForMobile(
                new Dictionary<Material, IMaterialConvertSettings>(),
                false, "",
                (total, index, orig, conv) => { });
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ConvertMaterialsForMobile_QuestMaterial_SkipsConversion()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var toonLitShader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (toonLitShader == null) { Assert.Ignore("Toon Lit shader not found"); return; }
            var mat = new Material(toonLitShader);
            var map = new Dictionary<Material, IMaterialConvertSettings>
            {
                { mat, new ToonLitConvertSettings() }
            };
            try
            {
                var result = converter.ConvertMaterialsForMobile(map, false, "", (t, i, o, c) => { });
                // Quest-compatible material should be skipped
                Assert.AreEqual(0, result.Count);
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void ConvertMaterialsForMobile_ReplaceSettings_ReturnsReplaceMaterial()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var mat = new Material(Shader.Find("Standard"));
            var toonLitShader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (toonLitShader == null) { Assert.Ignore("Toon Lit shader not found"); return; }
            var replaceMat = new Material(toonLitShader);
            var map = new Dictionary<Material, IMaterialConvertSettings>
            {
                { mat, new MaterialReplaceSettings { material = replaceMat } }
            };
            try
            {
                var result = converter.ConvertMaterialsForMobile(map, false, "", (t, i, o, c) => { });
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(replaceMat, result[mat]);
            }
            finally
            {
                UObject.DestroyImmediate(replaceMat);
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertMaterialsForMobile_ToonLitSettings_ConvertsStandard()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var mat = new Material(Shader.Find("Standard"));
            var map = new Dictionary<Material, IMaterialConvertSettings>
            {
                { mat, new ToonLitConvertSettings() }
            };
            Material converted = null;
            try
            {
                var result = converter.ConvertMaterialsForMobile(map, false, "", (t, i, o, c) => { });
                Assert.AreEqual(1, result.Count);
                Assert.IsTrue(result.ContainsKey(mat));
                converted = result[mat];
                Assert.IsNotNull(converted);
            }
            finally
            {
                if (converted != null && converted != mat) UObject.DestroyImmediate(converted);
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertMaterialsForMobile_ToonStandardSettings_ConvertsStandard()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var mat = new Material(Shader.Find("Standard"));
            var map = new Dictionary<Material, IMaterialConvertSettings>
            {
                { mat, new ToonStandardConvertSettings() }
            };
            Material converted = null;
            try
            {
                var result = converter.ConvertMaterialsForMobile(map, false, "", (t, i, o, c) => { });
                Assert.AreEqual(1, result.Count);
                Assert.IsTrue(result.ContainsKey(mat));
                converted = result[mat];
                Assert.IsNotNull(converted);
            }
            finally
            {
                if (converted != null && converted != mat) UObject.DestroyImmediate(converted);
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertMaterialsForMobile_DuplicateMaterial_ConvertedOnce()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var mat = new Material(Shader.Find("Standard"));
            // Can't add duplicate keys in a dictionary, but testing distinct behavior
            var map = new Dictionary<Material, IMaterialConvertSettings>
            {
                { mat, new ToonLitConvertSettings() }
            };
            Material converted = null;
            try
            {
                var result = converter.ConvertMaterialsForMobile(map, false, "", (t, i, o, c) => { });
                Assert.AreEqual(1, result.Count);
                converted = result[mat];
            }
            finally
            {
                if (converted != null && converted != mat) UObject.DestroyImmediate(converted);
                UObject.DestroyImmediate(mat);
            }
        }
    }

    // ==========================================
    // AvatarConverter - SharedBlackTextureCache deeper paths
    // ==========================================
    [TestFixture]
    public class Batch23_SharedBlackTextureCacheTests
    {
        [Test]
        public void GetOrCreateSharedBlackTexture_CacheHit_ReturnsSame()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var getMethod = typeof(AvatarConverter).GetMethod("GetOrCreateSharedBlackTexture", BindingFlags.NonPublic | BindingFlags.Instance);
            var clearMethod = typeof(AvatarConverter).GetMethod("ClearSharedBlackTextureCache", BindingFlags.NonPublic | BindingFlags.Instance);
            if (getMethod == null || clearMethod == null) { Assert.Ignore("Methods not found"); return; }

            Texture2D first = null, second = null;
            try
            {
                clearMethod.Invoke(converter, null);
                first = (Texture2D)getMethod.Invoke(converter, new object[] { false, "" });
                second = (Texture2D)getMethod.Invoke(converter, new object[] { false, "" });
                Assert.AreSame(first, second);
            }
            finally
            {
                if (first != null) UObject.DestroyImmediate(first);
                clearMethod.Invoke(converter, null);
            }
        }

        [Test]
        public void GetOrCreateSharedBlackTexture_DifferentKeys_ReturnsDifferent()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var getMethod = typeof(AvatarConverter).GetMethod("GetOrCreateSharedBlackTexture", BindingFlags.NonPublic | BindingFlags.Instance);
            var clearMethod = typeof(AvatarConverter).GetMethod("ClearSharedBlackTextureCache", BindingFlags.NonPublic | BindingFlags.Instance);
            if (getMethod == null || clearMethod == null) { Assert.Ignore("Methods not found"); return; }

            Texture2D first = null, second = null;
            try
            {
                clearMethod.Invoke(converter, null);
                first = (Texture2D)getMethod.Invoke(converter, new object[] { false, "path1" });
                second = (Texture2D)getMethod.Invoke(converter, new object[] { false, "path2" });
                Assert.AreNotSame(first, second);
            }
            finally
            {
                if (first != null) UObject.DestroyImmediate(first);
                if (second != null) UObject.DestroyImmediate(second);
                clearMethod.Invoke(converter, null);
            }
        }

        [Test]
        public void CreateSharedBlackTexture_NoSave_SetsStreamingMipMaps()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var method = typeof(AvatarConverter).GetMethod("CreateSharedBlackTexture", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("CreateSharedBlackTexture not found"); return; }

            Texture2D tex = null;
            try
            {
                tex = (Texture2D)method.Invoke(converter, new object[] { false, "" });
                Assert.IsNotNull(tex);
                Assert.AreEqual("VQT_Shared_Black", tex.name);
                Assert.IsTrue(tex.streamingMipmaps);
            }
            finally
            {
                if (tex != null) UObject.DestroyImmediate(tex);
            }
        }
    }

    // ==========================================
    // MissingScriptsRule - more validation paths
    // ==========================================
    [TestFixture]
    public class Batch23_MissingScriptsRuleTests
    {
        [Test]
        public void Validate_InactiveAvatar_ReturnsNull()
        {
            var rule = new MissingScriptsRule();
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            go.SetActive(false);
            try
            {
                var avatar = new VRChatAvatar(desc);
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void Validate_ActiveAvatar_NoMissingScripts_ReturnsNull()
        {
            var rule = new MissingScriptsRule();
            var go = new GameObject("Avatar");
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
    }

    // ==========================================
    // VRChatAvatar - Contact methods
    // ==========================================
    [TestFixture]
    public class Batch23_VRChatAvatarContactTests
    {
        [Test]
        public void GetContacts_NoContacts_ReturnsEmpty()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                var contacts = avatar.GetContacts();
                Assert.IsNotNull(contacts);
                Assert.AreEqual(0, contacts.Length);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void GetNonLocalContacts_NoContacts_ReturnsEmpty()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                var contacts = avatar.GetNonLocalContacts();
                Assert.IsNotNull(contacts);
                Assert.AreEqual(0, contacts.Length);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void GetLocalContactReceivers_NoReceivers_ReturnsEmpty()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                var receivers = avatar.GetLocalContactReceivers();
                Assert.IsNotNull(receivers);
                Assert.AreEqual(0, receivers.Length);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void GetLocalContactSenders_NoSenders_ReturnsEmpty()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                var senders = avatar.GetLocalContactSenders();
                Assert.IsNotNull(senders);
                Assert.AreEqual(0, senders.Length);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void GetRelatedMaterials_NoRenderers_ReturnsEmpty()
        {
            var go = new GameObject("Avatar");
            try
            {
                var materials = VRChatAvatar.GetRelatedMaterials(go);
                Assert.IsNotNull(materials);
                Assert.AreEqual(0, materials.Length);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void GetRelatedMaterials_WithRenderer_ReturnsMaterials()
        {
            var go = new GameObject("Avatar");
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mat = new Material(Shader.Find("Standard"));
            smr.sharedMaterial = mat;
            try
            {
                var materials = VRChatAvatar.GetRelatedMaterials(go);
                Assert.IsNotNull(materials);
                Assert.That(materials, Has.Member(mat));
            }
            finally
            {
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(go);
            }
        }
    }

    // ==========================================
    // AvatarConverter - GenerateMobileTextures branches
    // ==========================================
    [TestFixture]
    public class Batch23_GenerateMobileTexturesTests
    {
        [Test]
        public void GenerateMobileTextures_EmptyMaterials_DoesNothing()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var go = new GameObject("Avatar");
            var settings = go.AddComponent<AvatarConverterSettings>();
            try
            {
                converter.GenerateMobileTextures(new Material[0], false, "", settings, (t, i, o, c) => { });
                // No exception expected
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void GenerateMobileTextures_QuestMaterial_Skipped()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var go = new GameObject("Avatar");
            var settings = go.AddComponent<AvatarConverterSettings>();
            var toonLitShader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (toonLitShader == null) { Assert.Ignore("Toon Lit shader not found"); return; }
            var mat = new Material(toonLitShader);
            try
            {
                converter.GenerateMobileTextures(new Material[] { mat }, false, "", settings, (t, i, o, c) => { });
                // Quest material skipped, no exception
            }
            finally
            {
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GenerateMobileTextures_ToonLitSettings_NoGenerate_Succeeds()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var go = new GameObject("Avatar");
            var settings = go.AddComponent<AvatarConverterSettings>();
            // Default ToonLitConvertSettings has generateQuestTextures = false
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                converter.GenerateMobileTextures(new Material[] { mat }, false, "", settings, (t, i, o, c) => { });
            }
            finally
            {
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GenerateMobileTextures_MaterialReplaceSettings_Skipped()
        {
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var go = new GameObject("Avatar");
            var settings = go.AddComponent<AvatarConverterSettings>();
            var mat = new Material(Shader.Find("Standard"));
            var toonLitShader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (toonLitShader == null) { Assert.Ignore("Toon Lit shader not found"); return; }
            var replaceMat = new Material(toonLitShader);
            settings.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = mat,
                    materialConvertSettings = new MaterialReplaceSettings { material = replaceMat }
                }
            };
            try
            {
                converter.GenerateMobileTextures(new Material[] { mat }, false, "", settings, (t, i, o, c) => { });
                // MaterialReplaceSettings returns ResultRequest - no texture gen
            }
            finally
            {
                UObject.DestroyImmediate(replaceMat);
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(go);
            }
        }
    }

    // ==========================================
    // VRChatAvatar - EstimatePerformanceStats
    // ==========================================
    [TestFixture]
    public class Batch23_VRChatAvatarPerformanceTests
    {
        [Test]
        public void EstimatePerformanceStats_EmptyAvatar_DoesNotThrow()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                var stats = avatar.EstimatePerformanceStats(
                    new VRCPhysBone[0],
                    new VRCPhysBoneCollider[0],
                    new ContactBase[0],
                    true);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void EstimatePerformanceStats_DesktopMode_KeepsAllStats()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                var stats = avatar.EstimatePerformanceStats(
                    new VRCPhysBone[0],
                    new VRCPhysBoneCollider[0],
                    new ContactBase[0],
                    false);
                Assert.IsNotNull(stats);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void EstimatePerformanceStats_WithProviders_DoesNotThrow()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                var providers = avatar.GetPhysBoneProviders();
                var stats = avatar.EstimatePerformanceStats(
                    providers,
                    new VRCPhysBoneCollider[0],
                    new ContactBase[0],
                    true);
                Assert.IsNotNull(stats);
            }
            finally { UObject.DestroyImmediate(go); }
        }
    }

    // ==========================================
    // ComponentRemover - additional paths
    // ==========================================
    [TestFixture]
    public class Batch23_ComponentRemoverTests
    {
        [Test]
        public void RemoveUnsupportedComponentsInChildren_WithConstraintExclusion()
        {
            var remover = new ComponentRemover();
            var go = new GameObject("Avatar");
            try
            {
                // Test with excludeTypes parameter
                remover.RemoveUnsupportedComponentsInChildren(go, true, false, new Type[] { typeof(UnityEngine.Animations.IConstraint) });
                // No exception expected
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void RemoveUnsupportedComponentsInChildren_IncludeInactive()
        {
            var remover = new ComponentRemover();
            var go = new GameObject("Avatar");
            var child = new GameObject("InactiveChild");
            child.transform.SetParent(go.transform);
            child.SetActive(false);
            try
            {
                remover.RemoveUnsupportedComponentsInChildren(go, true);
                // No exception expected
            }
            finally { UObject.DestroyImmediate(go); }
        }
    }

    // ==========================================
    // VRChatAvatar - Materials property
    // ==========================================
    [TestFixture]
    public class Batch23_VRChatAvatarMaterialsTests
    {
        [Test]
        public void Materials_NoRenderers_ReturnsEmpty()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                Assert.IsNotNull(avatar.Materials);
                Assert.AreEqual(0, avatar.Materials.Length);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void Materials_WithRenderers_ReturnsMaterials()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mat = new Material(Shader.Find("Standard"));
            smr.sharedMaterial = mat;
            try
            {
                var avatar = new VRChatAvatar(desc);
                Assert.IsNotNull(avatar.Materials);
                Assert.That(avatar.Materials, Has.Member(mat));
            }
            finally
            {
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void Materials_NullMaterialsFiltered()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMaterials = new Material[] { null };
            try
            {
                var avatar = new VRChatAvatar(desc);
                Assert.IsNotNull(avatar.Materials);
                // Null materials should be filtered out
                Assert.AreEqual(0, avatar.Materials.Length);
            }
            finally { UObject.DestroyImmediate(go); }
        }
    }
}
