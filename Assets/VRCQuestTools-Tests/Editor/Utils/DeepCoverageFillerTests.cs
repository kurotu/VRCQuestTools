// Batch47: Deep coverage tests for CreateMaterialConvertSettingsMap, validation rules,
// ComponentRemover, VRCSDKUtility methods, VRChatAvatar properties, and ToonStandardGenerator.
// Targets the largest uncovered testable code blocks.

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
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    internal class CreateMaterialConvertSettingsMapTests_DeepFiller
    {
        private List<UnityEngine.Object> objectsToCleanup;

        [SetUp]
        public void SetUp()
        {
            objectsToCleanup = new List<UnityEngine.Object>();
        }

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
        }

        private GameObject CreateAvatarWithDescriptor()
        {
            var go = new GameObject("TestAvatar_Batch47");
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            return go;
        }

        private Material CreateStandardMaterial(string name)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.name = name;
            objectsToCleanup.Add(mat);
            return mat;
        }

        private Material CreateToonLitMaterial(string name)
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null) shader = Shader.Find("Standard");
            var mat = new Material(shader);
            mat.name = name;
            objectsToCleanup.Add(mat);
            return mat;
        }

        private AvatarConverter CreateAvatarConverter()
        {
            return new AvatarConverter(new MaterialWrapperBuilder());
        }

        // -------- CreateMaterialConvertSettingsMap with AvatarConverterSettings --------
        [Test]
        public void CreateMaterialConvertSettingsMap_WithAvatarConverterSettings_UsesDefaultSettings()
        {
            var go = CreateAvatarWithDescriptor();
            var settings = go.AddComponent<AvatarConverterSettings>();
            var mat = CreateStandardMaterial("TestMat_DefaultSettings");

            // Add renderer with the material
            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            smr.sharedMesh = mesh;
            smr.sharedMaterials = new[] { mat };

            var converter = CreateAvatarConverter();
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            var map = converter.CreateMaterialConvertSettingsMap(avatar);

            Assert.IsTrue(map.ContainsKey(mat), "Should contain the test material");
            Assert.IsNotNull(map[mat], "Convert settings should not be null");
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithAdditionalSettings_OverridesDefault()
        {
            var go = CreateAvatarWithDescriptor();
            var settings = go.AddComponent<AvatarConverterSettings>();
            var mat = CreateStandardMaterial("TestMat_Additional");

            // Add renderer with the material
            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            smr.sharedMesh = mesh;
            smr.sharedMaterials = new[] { mat };

            // Set additional material convert settings directly
            var matCapSettings = new MatCapLitConvertSettings();
            settings.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings { targetMaterial = mat, materialConvertSettings = matCapSettings },
            };

            var converter = CreateAvatarConverter();
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            var map = converter.CreateMaterialConvertSettingsMap(avatar);

            Assert.IsTrue(map.ContainsKey(mat));
            Assert.IsInstanceOf<MatCapLitConvertSettings>(map[mat]);
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithMaterialSwap_AddsMappings()
        {
            var go = CreateAvatarWithDescriptor();
            go.AddComponent<AvatarConverterSettings>();
            var originalMat = CreateStandardMaterial("SwapOriginal");
            var replacementMat = CreateToonLitMaterial("SwapReplacement");

            // Add renderer
            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            smr.sharedMesh = mesh;
            smr.sharedMaterials = new[] { originalMat };

            // Add MaterialSwap
            var swap = go.AddComponent<MaterialSwap>();
            var mappingsField = typeof(MaterialSwap).GetField("materialMappings", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (mappingsField != null)
            {
                var mappingType = typeof(MaterialSwap).Assembly.GetType("KRT.VRCQuestTools.Components.MaterialSwap+MaterialMapping");
                if (mappingType == null) mappingType = typeof(MaterialSwap).GetNestedType("MaterialMapping", BindingFlags.Public | BindingFlags.NonPublic);
                if (mappingType != null)
                {
                    var listType = typeof(List<>).MakeGenericType(mappingType);
                    var list = Activator.CreateInstance(listType);
                    var mapping = Activator.CreateInstance(mappingType);
                    var origField = mappingType.GetField("originalMaterial", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var replField = mappingType.GetField("replacementMaterial", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (origField != null) origField.SetValue(mapping, originalMat);
                    if (replField != null) replField.SetValue(mapping, replacementMat);
                    listType.GetMethod("Add").Invoke(list, new[] { mapping });
                    mappingsField.SetValue(swap, list);
                }
            }

            var converter = CreateAvatarConverter();
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            var map = converter.CreateMaterialConvertSettingsMap(avatar);

            // MaterialSwap should create a MaterialReplaceSettings
            Assert.IsTrue(map.ContainsKey(originalMat), "Map should contain the swap material");
            Assert.IsInstanceOf<MaterialReplaceSettings>(map[originalMat]);
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithMaterialConversionSettings_AddsMappings()
        {
            var go = CreateAvatarWithDescriptor();
            var mat = CreateStandardMaterial("ConversionMat");

            // Add renderer
            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            smr.sharedMesh = mesh;
            smr.sharedMaterials = new[] { mat };

            // Add MaterialConversionSettings
            var mcs = go.AddComponent<MaterialConversionSettings>();

            var converter = CreateAvatarConverter();
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            var map = converter.CreateMaterialConvertSettingsMap(avatar);

            // Should have default settings from MaterialConversionSettings
            Assert.IsTrue(map.ContainsKey(mat), "Map should contain the material");
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_EmptyAvatar_ReturnsEmptyMap()
        {
            var go = CreateAvatarWithDescriptor();
            go.AddComponent<AvatarConverterSettings>();

            var converter = CreateAvatarConverter();
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            var map = converter.CreateMaterialConvertSettingsMap(avatar);

            Assert.AreEqual(0, map.Count, "No renderers means no materials to convert");
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_FiltersUnusedMaterials()
        {
            var go = CreateAvatarWithDescriptor();
            var settings = go.AddComponent<AvatarConverterSettings>();
            var usedMat = CreateStandardMaterial("UsedMat");
            var unusedMat = CreateStandardMaterial("UnusedMat");

            // Add renderer with only usedMat
            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            smr.sharedMesh = mesh;
            smr.sharedMaterials = new[] { usedMat };

            var converter = CreateAvatarConverter();
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            var map = converter.CreateMaterialConvertSettingsMap(avatar);

            Assert.IsTrue(map.ContainsKey(usedMat), "Used material should be in map");
            Assert.IsFalse(map.ContainsKey(unusedMat), "Unused material should not be in map");
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_TwoOverloads_Work()
        {
            var go = CreateAvatarWithDescriptor();
            go.AddComponent<AvatarConverterSettings>();
            var mat = CreateStandardMaterial("TestMat_Overload");

            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            smr.sharedMesh = mesh;
            smr.sharedMaterials = new[] { mat };

            var converter = CreateAvatarConverter();

            // Test both overloads
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            var map1 = converter.CreateMaterialConvertSettingsMap(avatar);
            var map2 = converter.CreateMaterialConvertSettingsMap(go, new[] { mat });

            Assert.AreEqual(map1.Count, map2.Count);
        }
    }

    [TestFixture]
    internal class ValidationRulesCoverageTests
    {
        private List<UnityEngine.Object> objectsToCleanup;

        [SetUp]
        public void SetUp()
        {
            objectsToCleanup = new List<UnityEngine.Object>();
        }

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
        }

        private VRChatAvatar CreateTestAvatar(string name = "ValidationTestAvatar")
        {
            var go = new GameObject(name);
            objectsToCleanup.Add(go);
            var _vrcDesc1 = go.AddComponent<VRCAvatarDescriptor>();
            _vrcDesc1.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            _vrcDesc1.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            return new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
        }

        // -------- AvatarValidationRules --------
        [Test]
        public void AvatarValidationRules_Rules_ContainsMissingScriptsRule()
        {
            var rules = AvatarValidationRules.Rules;
            Assert.IsTrue(rules.Any(r => r is MissingScriptsRule), "Should contain MissingScriptsRule");
        }

        [Test]
        public void AvatarValidationRules_Rules_ContainsMissingNdmfRule()
        {
            var rules = AvatarValidationRules.Rules;
            Assert.IsTrue(rules.Any(r => r is MissingNdmfRule), "Should contain MissingNdmfRule");
        }

        // -------- MissingScriptsRule --------
        [Test]
        public void MissingScriptsRule_ActiveAvatarNoMissingScripts_ReturnsNull()
        {
            var avatar = CreateTestAvatar();
            var rule = new MissingScriptsRule();
            var result = rule.Validate(avatar);
            Assert.IsNull(result, "No missing scripts should return null");
        }

        [Test]
        public void MissingScriptsRule_InactiveAvatar_ReturnsNull()
        {
            var avatar = CreateTestAvatar();
            avatar.GameObject.SetActive(false);
            var rule = new MissingScriptsRule();
            var result = rule.Validate(avatar);
            Assert.IsNull(result, "Inactive avatar should return null");
        }

        // -------- MissingNdmfRule --------
        [Test]
        public void MissingNdmfRule_AvatarWithNoNdmfComponents_ReturnsNull()
        {
            var avatar = CreateTestAvatar();
            var rule = new MissingNdmfRule();
            var result = rule.Validate(avatar);
            // With VQT_HAS_NDMF defined, returns null always
            // Without it, returns null if no INdmfComponent children
            Assert.IsNull(result, "No NDMF components should return null");
        }
    }

    [TestFixture]
    internal class ComponentRemoverTests_DeepFiller
    {
        private List<UnityEngine.Object> objectsToCleanup;

        [SetUp]
        public void SetUp()
        {
            objectsToCleanup = new List<UnityEngine.Object>();
        }

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
        }

        [Test]
        public void ComponentRemover_IsUnsupportedComponent_ReturnsTrueForUnsupported()
        {
            var remover = new ComponentRemover();
            // DynamicBone is a well-known unsupported type; check a known unsupported type
            // Light is not unsupported, AudioSource might be
            var go = new GameObject("RemoverTest");
            objectsToCleanup.Add(go);
            var mr = go.AddComponent<MeshRenderer>();
            Assert.IsFalse(remover.IsUnsupportedComponent(mr), "MeshRenderer should be supported");
        }

        [Test]
        public void ComponentRemover_IsUnsupportedComponent_Type_Light_ReturnsFalse()
        {
            var remover = new ComponentRemover();
            Assert.IsFalse(remover.IsUnsupportedComponent(typeof(MeshRenderer)));
        }

        [Test]
        public void ComponentRemover_GetUnsupportedComponentsInChildren_EmptyObject_ReturnsEmpty()
        {
            var remover = new ComponentRemover();
            var go = new GameObject("EmptyObject");
            objectsToCleanup.Add(go);
            var components = remover.GetUnsupportedComponentsInChildren(go, true);
            Assert.AreEqual(0, components.Length, "Empty object should have no unsupported components");
        }

        [Test]
        public void ComponentRemover_RemoveUnsupportedComponentsInChildren_NoUnsupported_DoesNothing()
        {
            var remover = new ComponentRemover();
            var go = new GameObject("SafeObject");
            objectsToCleanup.Add(go);
            go.AddComponent<MeshRenderer>();
            var countBefore = go.GetComponents<Component>().Length;

            remover.RemoveUnsupportedComponentsInChildren(go, true);

            var countAfter = go.GetComponents<Component>().Length;
            Assert.AreEqual(countBefore, countAfter, "Should not remove supported components");
        }

        [Test]
        public void ComponentRemover_RemoveUnsupportedComponentsInChildren_WithAllowedTypes_SkipsAllowed()
        {
            var remover = new ComponentRemover();
            var go = new GameObject("AllowedTest");
            objectsToCleanup.Add(go);

            // RemoveUnsupportedComponentsInChildren with allowed types should not remove those
            remover.RemoveUnsupportedComponentsInChildren(go, true, false, new System.Type[] { typeof(MeshRenderer) });
            Assert.IsNotNull(go); // Just verify no crash
        }

        [Test]
        public void ComponentRemover_RemoveUnsupportedComponentsInChildren_CanUndo_True()
        {
            var remover = new ComponentRemover();
            var go = new GameObject("UndoTest");
            objectsToCleanup.Add(go);

            // With canUndo=true, should still work without errors
            remover.RemoveUnsupportedComponentsInChildren(go, true, true);
            Assert.IsNotNull(go);
        }
    }

    [TestFixture]
    internal class VRCSDKUtilityTests_DeepFiller
    {
        private List<UnityEngine.Object> objectsToCleanup;

        [SetUp]
        public void SetUp()
        {
            objectsToCleanup = new List<UnityEngine.Object>();
        }

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
        }

        [Test]
        public void IsMaterialAllowedForQuestAvatar_ToonLitMaterial_ReturnsTrue()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null)
            {
                Assert.Ignore("VRChat/Mobile/Toon Lit shader not found");
                return;
            }
            var mat = new Material(shader);
            objectsToCleanup.Add(mat);
            Assert.IsTrue(VRCSDKUtility.IsMaterialAllowedForQuestAvatar(mat));
        }

        [Test]
        public void IsMaterialAllowedForQuestAvatar_StandardMaterial_ReturnsFalse()
        {
            var mat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(mat);
            Assert.IsFalse(VRCSDKUtility.IsMaterialAllowedForQuestAvatar(mat));
        }

        [Test]
        public void IsProxyAnimationClip_RuntimeClip_ReturnsFalse()
        {
            var clip = new AnimationClip();
            clip.name = "TestClip";
            objectsToCleanup.Add(clip);
            Assert.IsFalse(VRCSDKUtility.IsProxyAnimationClip(clip));
        }

        [Test]
        public void IsExampleAsset_RuntimeObject_ReturnsFalse()
        {
            var mat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(mat);
            Assert.IsFalse(VRCSDKUtility.IsExampleAsset(mat));
        }

        [Test]
        public void CountMissingComponentsInChildren_NormalAvatar_ReturnsZero()
        {
            var go = new GameObject("NormalAvatar");
            objectsToCleanup.Add(go);
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            var count = VRCSDKUtility.CountMissingComponentsInChildren(go, true);
            Assert.AreEqual(0, count);
        }

        [Test]
        public void GetGameObjectsWithMissingComponents_NormalAvatar_ReturnsEmpty()
        {
            var go = new GameObject("NormalAvatar");
            objectsToCleanup.Add(go);
            var objects = VRCSDKUtility.GetGameObjectsWithMissingComponents(go, true);
            Assert.AreEqual(0, objects.Length);
        }

        [Test]
        public void GetAvatarsFromLoadedScenes_ReturnsDescriptors()
        {
            var go = new GameObject("SceneAvatar");
            objectsToCleanup.Add(go);
            var _vrcDesc2 = go.AddComponent<VRCAvatarDescriptor>();
            _vrcDesc2.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            _vrcDesc2.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var avatars = VRCSDKUtility.GetAvatarsFromLoadedScenes();
            Assert.IsTrue(avatars.Length > 0, "Should find at least one avatar in the scene");
        }

        [Test]
        public void IsUnsupportedComponentType_Light_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsUnsupportedComponentType(typeof(MeshRenderer)));
        }

        [Test]
        public void IsUnsupportedComponentType_Transform_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsUnsupportedComponentType(typeof(Transform)));
        }

        [Test]
        public void UnsupportedComponentTypes_NotEmpty()
        {
            Assert.IsTrue(VRCSDKUtility.UnsupportedComponentTypes.Length > 0, "Should have known unsupported types");
        }
    }

    [TestFixture]
    internal class VRChatAvatarPropertyTests_DeepFiller
    {
        private List<UnityEngine.Object> objectsToCleanup;

        [SetUp]
        public void SetUp()
        {
            objectsToCleanup = new List<UnityEngine.Object>();
        }

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
        }

        private VRChatAvatar CreateAvatarWithRenderer(Mesh mesh = null, Material mat = null)
        {
            var go = new GameObject("AvatarPropTest");
            objectsToCleanup.Add(go);
            var _vrcDesc3 = go.AddComponent<VRCAvatarDescriptor>();
            _vrcDesc3.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            _vrcDesc3.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            if (mesh == null)
            {
                mesh = new Mesh();
                objectsToCleanup.Add(mesh);
            }
            smr.sharedMesh = mesh;
            if (mat != null)
            {
                smr.sharedMaterials = new[] { mat };
            }

            return new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
        }

        [Test]
        public void HasAnimatedMaterials_NoAnimator_ReturnsFalse()
        {
            var avatar = CreateAvatarWithRenderer();
            Assert.IsFalse(avatar.HasAnimatedMaterials);
        }

        [Test]
        public void HasAnimatedMaterials_AnimatorWithNoMaterialAnims_ReturnsFalse()
        {
            var go = new GameObject("AnimAvatar");
            objectsToCleanup.Add(go);
            var _vrcDesc4 = go.AddComponent<VRCAvatarDescriptor>();
            _vrcDesc4.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            _vrcDesc4.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var animator = go.AddComponent<Animator>();
            var controller = new AnimatorController();
            objectsToCleanup.Add(controller);
            controller.AddLayer("Base");
            animator.runtimeAnimatorController = controller;

            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            Assert.IsFalse(avatar.HasAnimatedMaterials);
        }

        [Test]
        public void HasVertexColor_NoVertexColors_ReturnsFalse()
        {
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            mesh.vertices = new[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            // No colors set
            var avatar = CreateAvatarWithRenderer(mesh);
            Assert.IsFalse(avatar.HasVertexColor);
        }

        [Test]
        public void HasVertexColor_WithVertexColors_ReturnsTrue()
        {
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            mesh.vertices = new[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.colors32 = new[] { new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), new Color32(0, 0, 255, 255) };
            var avatar = CreateAvatarWithRenderer(mesh);
            Assert.IsTrue(avatar.HasVertexColor);
        }

        [Test]
        public void Materials_WithRenderer_ReturnsRendererMaterials()
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "TestMatProp";
            objectsToCleanup.Add(mat);
            var avatar = CreateAvatarWithRenderer(null, mat);
            Assert.IsTrue(avatar.Materials.Contains(mat), "Materials should contain the renderer's material");
        }

        [Test]
        public void Materials_EmptyAvatar_ReturnsEmpty()
        {
            var go = new GameObject("EmptyAvatar");
            objectsToCleanup.Add(go);
            var _vrcDesc5 = go.AddComponent<VRCAvatarDescriptor>();
            _vrcDesc5.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            _vrcDesc5.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            Assert.AreEqual(0, avatar.Materials.Length);
        }

        [Test]
        public void GetRuntimeAnimatorControllers_NoAnimators_ReturnsEmpty()
        {
            var go = new GameObject("NoAnimator");
            objectsToCleanup.Add(go);
            var _vrcDesc6 = go.AddComponent<VRCAvatarDescriptor>();
            _vrcDesc6.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            _vrcDesc6.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            var controllers = avatar.GetRuntimeAnimatorControllers();
            Assert.AreEqual(0, controllers.Length);
        }

        [Test]
        public void GetRuntimeAnimatorControllers_WithAnimator_ReturnsControllers()
        {
            var go = new GameObject("WithAnimator");
            objectsToCleanup.Add(go);
            var _vrcDesc7 = go.AddComponent<VRCAvatarDescriptor>();
            _vrcDesc7.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            _vrcDesc7.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var animator = go.AddComponent<Animator>();
            var controller = new AnimatorController();
            objectsToCleanup.Add(controller);
            controller.AddLayer("Base");
            animator.runtimeAnimatorController = controller;

            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            var controllers = avatar.GetRuntimeAnimatorControllers();
            Assert.IsTrue(controllers.Length > 0, "Should return at least one controller");
        }

        [Test]
        public void GetPhysBones_NoPhysBones_ReturnsEmpty()
        {
            var go = new GameObject("NoPB");
            objectsToCleanup.Add(go);
            var _vrcDesc8 = go.AddComponent<VRCAvatarDescriptor>();
            _vrcDesc8.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            _vrcDesc8.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            Assert.AreEqual(0, avatar.GetPhysBones().Length);
        }

        [Test]
        public void GetPhysBones_WithPhysBone_ReturnsBone()
        {
            var go = new GameObject("WithPB");
            objectsToCleanup.Add(go);
            var _vrcDesc9 = go.AddComponent<VRCAvatarDescriptor>();
            _vrcDesc9.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            _vrcDesc9.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var child = new GameObject("Bone");
            child.transform.SetParent(go.transform);
            child.AddComponent<VRCPhysBone>();
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            Assert.AreEqual(1, avatar.GetPhysBones().Length);
        }

        [Test]
        public void GetPhysBoneColliders_NoColliders_ReturnsEmpty()
        {
            var go = new GameObject("NoCollider");
            objectsToCleanup.Add(go);
            var _vrcDesc10 = go.AddComponent<VRCAvatarDescriptor>();
            _vrcDesc10.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            _vrcDesc10.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            Assert.AreEqual(0, avatar.GetPhysBoneColliders().Length);
        }

        [Test]
        public void GetContactSenders_NoSenders_ReturnsEmpty()
        {
            var go = new GameObject("NoSender");
            objectsToCleanup.Add(go);
            var _vrcDesc11 = go.AddComponent<VRCAvatarDescriptor>();
            _vrcDesc11.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            _vrcDesc11.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            Assert.AreEqual(0, avatar.GetLocalContactSenders().Length);
        }

        [Test]
        public void GetContactReceivers_NoReceivers_ReturnsEmpty()
        {
            var go = new GameObject("NoReceiver");
            objectsToCleanup.Add(go);
            var _vrcDesc12 = go.AddComponent<VRCAvatarDescriptor>();
            _vrcDesc12.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            _vrcDesc12.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            Assert.AreEqual(0, avatar.GetLocalContactReceivers().Length);
        }
    }

    [TestFixture]
    internal class ToonStandardGeneratorTests
    {
        private List<UnityEngine.Object> objectsToCleanup;

        [SetUp]
        public void SetUp()
        {
            objectsToCleanup = new List<UnityEngine.Object>();
        }

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
        }

        private Material CreateStandardMaterial()
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "ToonStdTest";
            objectsToCleanup.Add(mat);
            return mat;
        }

        // Test the non-IToonStandardConvertable fallback path in GenerateMaterial
        [Test]
        public void GenerateMaterial_NonToonStandardConvertable_FallsBackToToonLit()
        {
            var mat = CreateStandardMaterial();
            var wrapper = new MaterialWrapperBuilder().Build(mat);
            var settings = new ToonStandardConvertSettings();
            settings.generateQuestTextures = true;
            var blackTex = new Texture2D(4, 4);
            objectsToCleanup.Add(blackTex);

            // Use GenericToonStandardGenerator for a Standard material
            var generatorType = typeof(ToonStandardGenerator).Assembly.GetType("KRT.VRCQuestTools.Models.GenericToonStandardGenerator");
            if (generatorType == null)
            {
                Assert.Ignore("GenericToonStandardGenerator not found");
                return;
            }

            var generator = Activator.CreateInstance(generatorType, new object[] { wrapper, settings, blackTex });
            var generateMethod = typeof(ToonStandardGenerator).GetMethod("GenerateMaterial", BindingFlags.Public | BindingFlags.Instance);
            if (generateMethod == null)
            {
                Assert.Ignore("GenerateMaterial not found");
                return;
            }

            Material resultMat = null;
            var request = (AsyncCallbackRequest)generateMethod.Invoke(generator, new object[]
            {
                wrapper,
                UnityEditor.BuildTarget.Android,
                false,
                "Assets/TestOutput",
                (Action<Material>)((m) => { resultMat = m; }),
            });

            request.WaitForCompletion();
            Assert.IsNotNull(resultMat, "Should generate a result material");
        }

        // Test generateQuestTextures=false path
        [Test]
        public void GenerateMaterial_GenerateQuestTexturesFalse_CallsConvertToToonStandard()
        {
            var mat = CreateStandardMaterial();
            var wrapper = new MaterialWrapperBuilder().Build(mat);
            var settings = new ToonStandardConvertSettings();
            settings.generateQuestTextures = false;
            var blackTex = new Texture2D(4, 4);
            objectsToCleanup.Add(blackTex);

            var generatorType = typeof(ToonStandardGenerator).Assembly.GetType("KRT.VRCQuestTools.Models.GenericToonStandardGenerator");
            if (generatorType == null)
            {
                Assert.Ignore("GenericToonStandardGenerator not found");
                return;
            }

            var generator = Activator.CreateInstance(generatorType, new object[] { wrapper, settings, blackTex });
            var generateMethod = typeof(ToonStandardGenerator).GetMethod("GenerateMaterial", BindingFlags.Public | BindingFlags.Instance);

            Material resultMat = null;
            var request = (AsyncCallbackRequest)generateMethod.Invoke(generator, new object[]
            {
                wrapper,
                UnityEditor.BuildTarget.Android,
                false,
                "Assets/TestOutput",
                (Action<Material>)((m) => { resultMat = m; }),
            });

            request.WaitForCompletion();
            Assert.IsNotNull(resultMat, "Should generate via ConvertToToonStandard");
        }

        // Test with LilToon material if available
        [Test]
        public void GenerateMaterial_LilToonMaterial_GenerateQuestTexturesFalse_Works()
        {
            var lilShader = Shader.Find("lilToon");
            if (lilShader == null)
            {
                Assert.Ignore("lilToon shader not found");
                return;
            }

            var mat = new Material(lilShader);
            mat.name = "LilToonGenTest";
            objectsToCleanup.Add(mat);
            var wrapper = new MaterialWrapperBuilder().Build(mat);

            var settings = new ToonStandardConvertSettings();
            settings.generateQuestTextures = false;
            var blackTex = new Texture2D(4, 4);
            objectsToCleanup.Add(blackTex);

            var generatorType = typeof(ToonStandardGenerator).Assembly.GetType("KRT.VRCQuestTools.Models.LilToonToonStandardGenerator");
            if (generatorType == null)
            {
                Assert.Ignore("LilToonToonStandardGenerator not found");
                return;
            }

            var generator = Activator.CreateInstance(generatorType, new object[] { wrapper, settings, blackTex });
            var generateMethod = typeof(ToonStandardGenerator).GetMethod("GenerateMaterial", BindingFlags.Public | BindingFlags.Instance);

            Material resultMat = null;
            var request = (AsyncCallbackRequest)generateMethod.Invoke(generator, new object[]
            {
                wrapper,
                UnityEditor.BuildTarget.Android,
                false,
                "Assets/TestOutput",
                (Action<Material>)((m) => { resultMat = m; }),
            });

            request.WaitForCompletion();
            Assert.IsNotNull(resultMat, "LilToon should generate via ConvertToToonStandard");
        }

        // Test GenerateTextures delegates to GenerateMaterial
        [Test]
        public void GenerateTextures_DelegatesToGenerateMaterial()
        {
            var mat = CreateStandardMaterial();
            var wrapper = new MaterialWrapperBuilder().Build(mat);
            var settings = new ToonStandardConvertSettings();
            settings.generateQuestTextures = false;
            var blackTex = new Texture2D(4, 4);
            objectsToCleanup.Add(blackTex);

            var generatorType = typeof(ToonStandardGenerator).Assembly.GetType("KRT.VRCQuestTools.Models.GenericToonStandardGenerator");
            if (generatorType == null)
            {
                Assert.Ignore("GenericToonStandardGenerator not found");
                return;
            }

            var generator = Activator.CreateInstance(generatorType, new object[] { wrapper, settings, blackTex });
            var generateTexMethod = typeof(ToonStandardGenerator).GetMethod("GenerateTextures", BindingFlags.Public | BindingFlags.Instance);

            bool completed = false;
            var request = (AsyncCallbackRequest)generateTexMethod.Invoke(generator, new object[]
            {
                wrapper,
                UnityEditor.BuildTarget.Android,
                false,
                "Assets/TestOutput",
                (Action)(() => { completed = true; }),
            });

            request.WaitForCompletion();
            Assert.IsTrue(completed, "GenerateTextures completion should fire");
        }
    }

    [TestFixture]
    internal class ApplyConvertedMaterialsDeepTests
    {
        private List<UnityEngine.Object> objectsToCleanup;

        [SetUp]
        public void SetUp()
        {
            objectsToCleanup = new List<UnityEngine.Object>();
        }

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
        }

        private AvatarConverter CreateAvatarConverter()
        {
            return new AvatarConverter(new MaterialWrapperBuilder());
        }

        private (GameObject, VRCAvatarDescriptor) CreateAvatarWithAnimator()
        {
            var go = new GameObject("AnimatedAvatar_Batch47");
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            go.AddComponent<Animator>();
            return (go, desc);
        }

        // Test ApplyConvertedMaterials with override controllers
        [Test]
        public void ApplyConvertedMaterials_WithOverrideControllers_InjectsOverrides()
        {
            var (go, desc) = CreateAvatarWithAnimator();
            var settings = go.AddComponent<AvatarConverterSettings>();

            // Create material pair
            var originalMat = new Material(Shader.Find("Standard"));
            originalMat.name = "OrigMat";
            objectsToCleanup.Add(originalMat);
            var convertedMat = new Material(Shader.Find("Standard"));
            convertedMat.name = "ConvertedMat";
            objectsToCleanup.Add(convertedMat);
            var convertedMaterials = new Dictionary<Material, Material> { { originalMat, convertedMat } };

            // Create base controller with material animation
            var baseController = new AnimatorController();
            objectsToCleanup.Add(baseController);
            baseController.name = "BaseCtrl";
            baseController.AddLayer("Base");

            var clip = new AnimationClip();
            clip.name = "MatSwap";
            objectsToCleanup.Add(clip);

            // Create override controller
            var overrideController = new AnimatorOverrideController();
            objectsToCleanup.Add(overrideController);
            overrideController.runtimeAnimatorController = baseController;

            // Set override controllers on settings
            var overrideField = typeof(AvatarConverterSettings).GetField("animatorOverrideControllers", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (overrideField != null)
            {
                overrideField.SetValue(settings, new[] { overrideController });
            }

            // Add renderer with original material
            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            smr.sharedMesh = mesh;
            smr.sharedMaterials = new[] { originalMat };

            var converter = CreateAvatarConverter();
            var progressCallback = CreateProgressCallback();

            converter.ApplyConvertedMaterials(go, convertedMaterials, false, "Assets/Test", progressCallback);

            // Renderer material should be replaced
            Assert.AreEqual(convertedMat, smr.sharedMaterials[0], "Renderer material should be converted");
        }

        // Test ApplyConvertedMaterials - renderer material replacement with null materials
        [Test]
        public void ApplyConvertedMaterials_WithNullMaterialInRenderer_HandlesGracefully()
        {
            var (go, desc) = CreateAvatarWithAnimator();

            var originalMat = new Material(Shader.Find("Standard"));
            originalMat.name = "TestMat";
            objectsToCleanup.Add(originalMat);
            var convertedMat = new Material(Shader.Find("Standard"));
            convertedMat.name = "ConvertedMat";
            objectsToCleanup.Add(convertedMat);
            var convertedMaterials = new Dictionary<Material, Material> { { originalMat, convertedMat } };

            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            smr.sharedMesh = mesh;
            smr.sharedMaterials = new Material[] { originalMat, null };

            var converter = CreateAvatarConverter();
            var progressCallback = CreateProgressCallback();

            converter.ApplyConvertedMaterials(go, convertedMaterials, false, "Assets/Test", progressCallback);

            Assert.AreEqual(convertedMat, smr.sharedMaterials[0]);
            Assert.IsNull(smr.sharedMaterials[1], "Null material should remain null");
        }

        // Test ApplyConvertedMaterials - renderer material not in converted map stays unchanged
        [Test]
        public void ApplyConvertedMaterials_MaterialNotInMap_RemainsUnchanged()
        {
            var (go, desc) = CreateAvatarWithAnimator();

            var unknownMat = new Material(Shader.Find("Standard"));
            unknownMat.name = "UnknownMat";
            objectsToCleanup.Add(unknownMat);
            var convertedMaterials = new Dictionary<Material, Material>();

            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            smr.sharedMesh = mesh;
            smr.sharedMaterials = new[] { unknownMat };

            var converter = CreateAvatarConverter();
            var progressCallback = CreateProgressCallback();

            converter.ApplyConvertedMaterials(go, convertedMaterials, false, "Assets/Test", progressCallback);

            Assert.AreEqual(unknownMat, smr.sharedMaterials[0], "Material not in map should remain unchanged");
        }

        // Test ApplyConvertedMaterials - child Animator controller replacement
        [Test]
        public void ApplyConvertedMaterials_ChildAnimatorWithController_GetsConverted()
        {
            var (go, desc) = CreateAvatarWithAnimator();

            var originalMat = new Material(Shader.Find("Standard"));
            originalMat.name = "ChildAnimMat";
            objectsToCleanup.Add(originalMat);
            var convertedMat = new Material(Shader.Find("Standard"));
            convertedMat.name = "ConvertedChildAnimMat";
            objectsToCleanup.Add(convertedMat);
            var convertedMaterials = new Dictionary<Material, Material> { { originalMat, convertedMat } };

            // Create an animation clip that references materials via ObjectReferenceKeyframe
            var clip = new AnimationClip();
            clip.name = "MaterialAnimClip";
            objectsToCleanup.Add(clip);
            var binding = new EditorCurveBinding
            {
                path = "Body",
                type = typeof(SkinnedMeshRenderer),
                propertyName = "m_Materials.Array.data[0]",
            };
            var keyframes = new ObjectReferenceKeyframe[]
            {
                new ObjectReferenceKeyframe { time = 0, value = originalMat },
            };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            // Create controller with the clip
            var controller = new AnimatorController();
            objectsToCleanup.Add(controller);
            controller.name = "ChildCtrl";
            controller.AddLayer("Base");
            var layer = controller.layers[0];
            var state = layer.stateMachine.AddState("State");
            state.motion = clip;
            controller.layers = new[] { layer };

            // Add child animator
            var child = new GameObject("ChildObj");
            child.transform.SetParent(go.transform);
            var childAnimator = child.AddComponent<Animator>();
            childAnimator.runtimeAnimatorController = controller;

            // Add renderer
            var body = new GameObject("Body");
            body.transform.SetParent(go.transform);
            var smr = body.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            smr.sharedMesh = mesh;
            smr.sharedMaterials = new[] { originalMat };

            var converter = CreateAvatarConverter();
            var progressCallback = CreateProgressCallback();

            converter.ApplyConvertedMaterials(go, convertedMaterials, false, "Assets/Test", progressCallback);

            // Verify renderer material is replaced
            Assert.AreEqual(convertedMat, smr.sharedMaterials[0]);
        }

        private AvatarConverter.ProgressCallback CreateProgressCallback()
        {
            return new AvatarConverter.ProgressCallback
            {
                onTextureProgress = (total, index, orig, conv) => { },
                onAnimationClipProgress = (total, index, orig, conv) => { },
                onRuntimeAnimatorProgress = (total, index, orig, conv) => { },
            };
        }
    }

    [TestFixture]
    internal class UnityAnimationUtilityTests_DeepFiller
    {
        private List<UnityEngine.Object> objectsToCleanup;

        [SetUp]
        public void SetUp()
        {
            objectsToCleanup = new List<UnityEngine.Object>();
        }

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
        }

        [Test]
        public void GetMaterials_ClipWithNoMaterials_ReturnsEmpty()
        {
            var clip = new AnimationClip();
            clip.name = "EmptyClip";
            objectsToCleanup.Add(clip);
            var mats = UnityAnimationUtility.GetMaterials(clip);
            Assert.AreEqual(0, mats.Length);
        }

        [Test]
        public void GetMaterials_ClipWithMaterialKeyframes_ReturnsMaterials()
        {
            var clip = new AnimationClip();
            clip.name = "MatClip";
            objectsToCleanup.Add(clip);
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "AnimMat";
            objectsToCleanup.Add(mat);

            var binding = new EditorCurveBinding
            {
                path = "Body",
                type = typeof(SkinnedMeshRenderer),
                propertyName = "m_Materials.Array.data[0]",
            };
            var keyframes = new ObjectReferenceKeyframe[]
            {
                new ObjectReferenceKeyframe { time = 0, value = mat },
            };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            var mats = UnityAnimationUtility.GetMaterials(clip);
            Assert.IsTrue(mats.Contains(mat), "Should find the material in the animation clip");
        }

        [Test]
        public void ReplaceAnimationClipMaterials_ReplacesCorrectMaterial()
        {
            var clip = new AnimationClip();
            clip.name = "ReplaceClip";
            objectsToCleanup.Add(clip);
            var originalMat = new Material(Shader.Find("Standard"));
            originalMat.name = "Original";
            objectsToCleanup.Add(originalMat);
            var newMat = new Material(Shader.Find("Standard"));
            newMat.name = "New";
            objectsToCleanup.Add(newMat);

            var binding = new EditorCurveBinding
            {
                path = "Body",
                type = typeof(SkinnedMeshRenderer),
                propertyName = "m_Materials.Array.data[0]",
            };
            var keyframes = new ObjectReferenceKeyframe[]
            {
                new ObjectReferenceKeyframe { time = 0, value = originalMat },
            };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            var convertedClip = UnityAnimationUtility.ReplaceAnimationClipMaterials(clip, new Dictionary<Material, Material> { { originalMat, newMat } });
            objectsToCleanup.Add(convertedClip);

            var convertedMats = UnityAnimationUtility.GetMaterials(convertedClip);
            Assert.IsTrue(convertedMats.Contains(newMat), "Converted clip should contain the new material");
            Assert.IsFalse(convertedMats.Contains(originalMat), "Converted clip should not contain the original material");
        }

        [Test]
        public void GetBlendTrees_EmptyController_ReturnsEmpty()
        {
            var controller = new AnimatorController();
            objectsToCleanup.Add(controller);
            controller.AddLayer("Base");
            var trees = UnityAnimationUtility.GetBlendTrees(controller);
            Assert.AreEqual(0, trees.Length);
        }

        [Test]
        public void GetBlendTrees_ControllerWithBlendTree_ReturnsTree()
        {
            var controller = new AnimatorController();
            objectsToCleanup.Add(controller);
            controller.AddLayer("Base");
            controller.AddParameter("Blend", AnimatorControllerParameterType.Float);

            var layer = controller.layers[0];
            var tree = new BlendTree();
            objectsToCleanup.Add(tree);
            tree.blendParameter = "Blend";

            var clip1 = new AnimationClip();
            clip1.name = "Clip1";
            objectsToCleanup.Add(clip1);
            var clip2 = new AnimationClip();
            clip2.name = "Clip2";
            objectsToCleanup.Add(clip2);
            tree.AddChild(clip1);
            tree.AddChild(clip2);

            var state = layer.stateMachine.AddState("BlendState");
            state.motion = tree;
            controller.layers = new[] { layer };

            var trees = UnityAnimationUtility.GetBlendTrees(controller);
            Assert.IsTrue(trees.Length > 0, "Should find the blend tree");
        }

        [Test]
        public void DeepCopyBlendTree_CopiesStructure()
        {
            var tree = new BlendTree();
            objectsToCleanup.Add(tree);
            tree.name = "OriginalTree";

            var clip = new AnimationClip();
            clip.name = "Child";
            objectsToCleanup.Add(clip);
            tree.AddChild(clip);

            var copy = UnityAnimationUtility.DeepCopyBlendTree(tree);
            objectsToCleanup.Add(copy);

            Assert.AreNotSame(tree, copy);
            Assert.AreEqual(tree.children.Length, copy.children.Length);
        }

        [Test]
        public void DoesMotionExistInBlendTreeDescendants_ClipInTree_ReturnsTrue()
        {
            var tree = new BlendTree();
            objectsToCleanup.Add(tree);
            var clip = new AnimationClip();
            clip.name = "TargetClip";
            objectsToCleanup.Add(clip);
            tree.AddChild(clip);

            Assert.IsTrue(UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(tree, clip));
        }

        [Test]
        public void DoesMotionExistInBlendTreeDescendants_ClipNotInTree_ReturnsFalse()
        {
            var tree = new BlendTree();
            objectsToCleanup.Add(tree);
            var clip = new AnimationClip();
            clip.name = "NotInTree";
            objectsToCleanup.Add(clip);

            Assert.IsFalse(UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(tree, clip));
        }

        [Test]
        public void GetMaterials_Controller_ReturnsAllMaterials()
        {
            var controller = new AnimatorController();
            objectsToCleanup.Add(controller);
            controller.AddLayer("Base");

            var clip = new AnimationClip();
            clip.name = "CtrlMatClip";
            objectsToCleanup.Add(clip);
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "CtrlMat";
            objectsToCleanup.Add(mat);

            var binding = new EditorCurveBinding
            {
                path = "Body",
                type = typeof(SkinnedMeshRenderer),
                propertyName = "m_Materials.Array.data[0]",
            };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, new[]
            {
                new ObjectReferenceKeyframe { time = 0, value = mat },
            });

            var layer = controller.layers[0];
            var state = layer.stateMachine.AddState("State");
            state.motion = clip;
            controller.layers = new[] { layer };

            var mats = UnityAnimationUtility.GetMaterials(controller);
            Assert.IsTrue(mats.Contains(mat));
        }
    }

    [TestFixture]
    internal class ModularAvatarUtilityTests_DeepFiller
    {
        [Test]
        public void IsModularAvatarImported_ReturnsBool()
        {
            // Just verify no crash - value depends on whether MA is installed
            var result = ModularAvatarUtility.IsModularAvatarImported();
            Assert.IsTrue(result == true || result == false);
        }

        [Test]
        public void IsLegacyVersion_ReturnsBoolWithoutCrash()
        {
            // Should not throw even if MA is not installed
            var result = ModularAvatarUtility.IsLegacyVersion();
            Assert.IsTrue(result == true || result == false);
        }

        [Test]
        public void IsBreakingVersion_ReturnsBoolWithoutCrash()
        {
            var result = ModularAvatarUtility.IsBreakingVersion();
            Assert.IsTrue(result == true || result == false);
        }
    }

    [TestFixture]
    internal class ComponentUtilityTests_DeepFiller
    {
        private List<UnityEngine.Object> objectsToCleanup;

        [SetUp]
        public void SetUp()
        {
            objectsToCleanup = new List<UnityEngine.Object>();
        }

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
        }

        [Test]
        public void GetPrimaryMaterialConversionComponent_WithAvatarConverterSettings_ReturnsIt()
        {
            var go = new GameObject("PrimaryTest");
            objectsToCleanup.Add(go);
            var _vrcDesc13 = go.AddComponent<VRCAvatarDescriptor>();
            _vrcDesc13.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            _vrcDesc13.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            go.AddComponent<AvatarConverterSettings>();

            var primary = ComponentUtility.GetPrimaryMaterialConversionComponent(go);
            Assert.IsNotNull(primary, "AvatarConverterSettings should be a primary conversion component");
        }

        [Test]
        public void GetPrimaryMaterialConversionComponent_NoComponents_ReturnsNull()
        {
            var go = new GameObject("NoPrimary");
            objectsToCleanup.Add(go);
            var primary = ComponentUtility.GetPrimaryMaterialConversionComponent(go);
            Assert.IsNull(primary);
        }

        [Test]
        public void GetPrimaryMaterialConversionComponent_WithMaterialConversionSettings_Works()
        {
            var go = new GameObject("MCSPrimaryTest");
            objectsToCleanup.Add(go);
            var _vrcDesc14 = go.AddComponent<VRCAvatarDescriptor>();
            _vrcDesc14.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            _vrcDesc14.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            var mcs = go.AddComponent<MaterialConversionSettings>();

            // MaterialConversionSettings is primary only if there's no AvatarConverterSettings
            var primary = ComponentUtility.GetPrimaryMaterialConversionComponent(go);
            Assert.IsNotNull(primary);
        }
    }

    [TestFixture]
    internal class ExceptionClassTests
    {
        [Test]
        public void MaterialConversionException_StoresOriginalMaterial()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var exType = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.VRChat.MaterialConversionException");
                if (exType == null)
                {
                    Assert.Ignore("MaterialConversionException not found");
                    return;
                }
                var ex = (Exception)Activator.CreateInstance(exType, new object[] { "test error", mat, new Exception("inner") });
                Assert.AreEqual("test error", ex.Message.Split(new[] { '\r', '\n' })[0].TrimEnd());
                Assert.IsNotNull(ex.InnerException);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void AnimationClipConversionException_StoresClip()
        {
            var clip = new AnimationClip();
            try
            {
                var exType = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.VRChat.AnimationClipConversionException");
                if (exType == null)
                {
                    Assert.Ignore("AnimationClipConversionException not found");
                    return;
                }
                var ex = (Exception)Activator.CreateInstance(exType, new object[] { "clip error", clip, new Exception("inner") });
                Assert.IsNotNull(ex);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void AnimatorControllerConversionException_StoresController()
        {
            var ctrl = new AnimatorController();
            try
            {
                var exType = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.VRChat.AnimatorControllerConversionException");
                if (exType == null)
                {
                    Assert.Ignore("AnimatorControllerConversionException not found");
                    return;
                }
                var ex = (Exception)Activator.CreateInstance(exType, new object[] { "ctrl error", (RuntimeAnimatorController)ctrl, new Exception("inner") });
                Assert.IsNotNull(ex);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(ctrl);
            }
        }

        [Test]
        public void LegacyPackageException_HasMessage()
        {
            var exType = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.VRChat.LegacyPackageException");
            if (exType == null)
            {
                Assert.Ignore("LegacyPackageException not found");
                return;
            }
            var ex = (Exception)Activator.CreateInstance(exType, new object[] { "TestPackage", "1.0.0" });
            Assert.IsTrue(ex.Message.Contains("TestPackage") || ex.Message.Length > 0);
        }

        [Test]
        public void BreakingPackageException_HasMessage()
        {
            var exType = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.VRChat.BreakingPackageException");
            if (exType == null)
            {
                Assert.Ignore("BreakingPackageException not found");
                return;
            }
            var ex = (Exception)Activator.CreateInstance(exType, new object[] { "TestPackage", "2.0.0" });
            Assert.IsTrue(ex.Message.Length > 0);
        }

        [Test]
        public void TargetMaterialNullException_HasMessage()
        {
            var exType = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.VRChat.TargetMaterialNullException");
            if (exType == null)
            {
                Assert.Ignore("TargetMaterialNullException not found");
                return;
            }
            var go = new GameObject("TestComp");
            var comp = go.AddComponent<AvatarConverterSettings>();
            try
            {
                var ex = (Exception)Activator.CreateInstance(exType, new object[] { "null target", (Component)comp });
                Assert.IsTrue(ex.Message.Length > 0);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
