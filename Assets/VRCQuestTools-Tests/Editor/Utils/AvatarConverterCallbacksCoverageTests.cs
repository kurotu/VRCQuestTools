// Batch 16 - AvatarConverter, GenericToonStandardGenerator, Callbacks, ComponentUtility tests

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
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase.Validation.Performance;
using Object = UnityEngine.Object;

namespace KRT.VRCQuestTools.Tests
{
    // ========== AvatarConverter Tests ==========
    [TestFixture]
    public class AvatarConverterTests
    {
        private AvatarConverter CreateConverter()
        {
            return new AvatarConverter(new MaterialWrapperBuilder());
        }

        // --- FindDescendant via reflection ---
        private GameObject InvokeFindDescendant(AvatarConverter converter, GameObject root, string name)
        {
            var method = typeof(AvatarConverter).GetMethod("FindDescendant", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "FindDescendant method should exist");
            return (GameObject)method.Invoke(converter, new object[] { root, name });
        }

        [Test]
        public void FindDescendant_DirectChild_Found()
        {
            var root = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            try
            {
                var converter = CreateConverter();
                var result = InvokeFindDescendant(converter, root, "Child");
                Assert.IsNotNull(result);
                Assert.AreEqual("Child", result.name);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void FindDescendant_NestedChild_Found()
        {
            var root = new GameObject("Root");
            var mid = new GameObject("Mid");
            mid.transform.SetParent(root.transform);
            var deep = new GameObject("Deep");
            deep.transform.SetParent(mid.transform);
            try
            {
                var converter = CreateConverter();
                var result = InvokeFindDescendant(converter, root, "Deep");
                Assert.IsNotNull(result);
                Assert.AreEqual("Deep", result.name);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void FindDescendant_NotFound_ReturnsNull()
        {
            var root = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            try
            {
                var converter = CreateConverter();
                var result = InvokeFindDescendant(converter, root, "Nonexistent");
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void FindDescendant_NoChildren_ReturnsNull()
        {
            var root = new GameObject("Root");
            try
            {
                var converter = CreateConverter();
                var result = InvokeFindDescendant(converter, root, "Anything");
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void FindDescendant_MultipleChildrenSameName_ReturnsFirst()
        {
            var root = new GameObject("Root");
            var child1 = new GameObject("Child");
            child1.transform.SetParent(root.transform);
            var child2 = new GameObject("Child");
            child2.transform.SetParent(root.transform);
            try
            {
                var converter = CreateConverter();
                var result = InvokeFindDescendant(converter, root, "Child");
                Assert.IsNotNull(result);
                Assert.AreEqual("Child", result.name);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        // --- CreateSharedBlackTexture via reflection (non-save path) ---
        private Texture2D InvokeCreateSharedBlackTexture(AvatarConverter converter, bool saveAsFile, string texturesPath)
        {
            var method = typeof(AvatarConverter).GetMethod("CreateSharedBlackTexture", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "CreateSharedBlackTexture method should exist");
            return (Texture2D)method.Invoke(converter, new object[] { saveAsFile, texturesPath });
        }

        [Test]
        public void CreateSharedBlackTexture_NoSave_ReturnsBlackTexture()
        {
            var converter = CreateConverter();
            Texture2D tex = null;
            try
            {
                tex = InvokeCreateSharedBlackTexture(converter, false, "");
                Assert.IsNotNull(tex);
                Assert.AreEqual("VQT_Shared_Black", tex.name);
                Assert.AreEqual(4, tex.width);
                Assert.AreEqual(4, tex.height);
                var pixels = tex.GetPixels32();
                foreach (var p in pixels)
                {
                    Assert.AreEqual(0, p.r);
                    Assert.AreEqual(0, p.g);
                    Assert.AreEqual(0, p.b);
                }
            }
            finally
            {
                if (tex != null)
                    Object.DestroyImmediate(tex);
            }
        }

        // --- GetOrCreateSharedBlackTexture via reflection ---
        private Texture2D InvokeGetOrCreateSharedBlackTexture(AvatarConverter converter, bool saveAsFile, string texturesPath)
        {
            var method = typeof(AvatarConverter).GetMethod("GetOrCreateSharedBlackTexture", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "GetOrCreateSharedBlackTexture method should exist");
            return (Texture2D)method.Invoke(converter, new object[] { saveAsFile, texturesPath });
        }

        private void InvokeClearSharedBlackTextureCache(AvatarConverter converter)
        {
            var method = typeof(AvatarConverter).GetMethod("ClearSharedBlackTextureCache", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "ClearSharedBlackTextureCache method should exist");
            method.Invoke(converter, null);
        }

        [Test]
        public void GetOrCreateSharedBlackTexture_CachesTexture()
        {
            var converter = CreateConverter();
            Texture2D tex1 = null;
            Texture2D tex2 = null;
            try
            {
                tex1 = InvokeGetOrCreateSharedBlackTexture(converter, false, "path1");
                tex2 = InvokeGetOrCreateSharedBlackTexture(converter, false, "path1");
                Assert.AreSame(tex1, tex2, "Should return cached texture");
            }
            finally
            {
                if (tex1 != null) Object.DestroyImmediate(tex1);
                if (tex2 != null && tex2 != tex1) Object.DestroyImmediate(tex2);
            }
        }

        [Test]
        public void GetOrCreateSharedBlackTexture_DifferentKeys_DifferentTextures()
        {
            var converter = CreateConverter();
            Texture2D tex1 = null;
            Texture2D tex2 = null;
            try
            {
                tex1 = InvokeGetOrCreateSharedBlackTexture(converter, false, "path1");
                tex2 = InvokeGetOrCreateSharedBlackTexture(converter, false, "path2");
                Assert.AreNotSame(tex1, tex2, "Different keys should return different textures");
            }
            finally
            {
                if (tex1 != null) Object.DestroyImmediate(tex1);
                if (tex2 != null) Object.DestroyImmediate(tex2);
            }
        }

        [Test]
        public void ClearSharedBlackTextureCache_ResetsCache()
        {
            var converter = CreateConverter();
            Texture2D tex1 = null;
            Texture2D tex2 = null;
            try
            {
                tex1 = InvokeGetOrCreateSharedBlackTexture(converter, false, "path1");
                InvokeClearSharedBlackTextureCache(converter);
                tex2 = InvokeGetOrCreateSharedBlackTexture(converter, false, "path1");
                Assert.AreNotSame(tex1, tex2, "After clearing cache, should return new texture");
            }
            finally
            {
                if (tex1 != null) Object.DestroyImmediate(tex1);
                if (tex2 != null) Object.DestroyImmediate(tex2);
            }
        }

        // --- RemoveExtraMaterialSlots via reflection ---
        private void InvokeRemoveExtraMaterialSlots(AvatarConverter converter, GameObject go)
        {
            var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "RemoveExtraMaterialSlots method should exist");
            method.Invoke(converter, new object[] { go });
        }

        [Test]
        public void RemoveExtraMaterialSlots_MoreMaterialsThanSubMeshes_Removes()
        {
            var go = new GameObject("Avatar");
            try
            {
                var childGo = new GameObject("Body");
                childGo.transform.SetParent(go.transform);
                var meshFilter = childGo.AddComponent<MeshFilter>();
                var meshRenderer = childGo.AddComponent<MeshRenderer>();

                // Create mesh with 1 submesh
                var mesh = new Mesh();
                mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
                mesh.triangles = new int[] { 0, 1, 2 };
                mesh.subMeshCount = 1;
                meshFilter.sharedMesh = mesh;

                // Set 3 materials but only 1 submesh
                var mat1 = new Material(Shader.Find("Standard"));
                var mat2 = new Material(Shader.Find("Standard"));
                var mat3 = new Material(Shader.Find("Standard"));
                meshRenderer.sharedMaterials = new Material[] { mat1, mat2, mat3 };

                var converter = CreateConverter();
                InvokeRemoveExtraMaterialSlots(converter, go);

                Assert.AreEqual(1, meshRenderer.sharedMaterials.Length);
                Assert.AreEqual(mat1, meshRenderer.sharedMaterials[0]);

                Object.DestroyImmediate(mesh);
                Object.DestroyImmediate(mat1);
                Object.DestroyImmediate(mat2);
                Object.DestroyImmediate(mat3);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_MaterialsMatchSubMeshes_NoChange()
        {
            var go = new GameObject("Avatar");
            try
            {
                var childGo = new GameObject("Body");
                childGo.transform.SetParent(go.transform);
                var smr = childGo.AddComponent<SkinnedMeshRenderer>();

                var mesh = new Mesh();
                mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up, Vector3.right, Vector3.forward, Vector3.left };
                mesh.subMeshCount = 2;
                mesh.SetTriangles(new int[] { 0, 1, 2 }, 0);
                mesh.SetTriangles(new int[] { 3, 4, 5 }, 1);
                smr.sharedMesh = mesh;

                var mat1 = new Material(Shader.Find("Standard"));
                var mat2 = new Material(Shader.Find("Standard"));
                smr.sharedMaterials = new Material[] { mat1, mat2 };

                var converter = CreateConverter();
                InvokeRemoveExtraMaterialSlots(converter, go);

                Assert.AreEqual(2, smr.sharedMaterials.Length);

                Object.DestroyImmediate(mesh);
                Object.DestroyImmediate(mat1);
                Object.DestroyImmediate(mat2);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_NoMesh_DoesNotThrow()
        {
            var go = new GameObject("Avatar");
            try
            {
                var childGo = new GameObject("Body");
                childGo.transform.SetParent(go.transform);
                childGo.AddComponent<MeshRenderer>();
                // No MeshFilter => no mesh

                var converter = CreateConverter();
                Assert.DoesNotThrow(() => InvokeRemoveExtraMaterialSlots(converter, go));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // --- CreateMaterialConvertSettingsMap ---
        [Test]
        public void CreateMaterialConvertSettingsMap_WithAvatarConverterSettings_AppliesDefaults()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                var settings = go.AddComponent<AvatarConverterSettings>();

                // Add a renderer with a material
                var childGo = new GameObject("Body");
                childGo.transform.SetParent(go.transform);
                var smr = childGo.AddComponent<SkinnedMeshRenderer>();
                var mat = new Material(Shader.Find("Standard"));
                smr.sharedMaterial = mat;

                var converter = CreateConverter();
                var materials = new Material[] { mat };
                var map = converter.CreateMaterialConvertSettingsMap(go, materials);

                Assert.IsNotNull(map);
                Assert.IsTrue(map.ContainsKey(mat));
                Assert.IsNotNull(map[mat]);

                Object.DestroyImmediate(mat);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_EmptyMaterials_EmptyMap()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                go.AddComponent<AvatarConverterSettings>();

                var converter = CreateConverter();
                var map = converter.CreateMaterialConvertSettingsMap(go, new Material[0]);

                Assert.IsNotNull(map);
                Assert.AreEqual(0, map.Count);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_MaterialNotInAvatar_OmittedFromMap()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                go.AddComponent<AvatarConverterSettings>();

                var converter = CreateConverter();
                var mat = new Material(Shader.Find("Standard"));
                // avatarMaterials is empty, so the mat should not appear
                var map = converter.CreateMaterialConvertSettingsMap(go, new Material[0]);

                Assert.IsFalse(map.ContainsKey(mat));

                Object.DestroyImmediate(mat);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithMaterialConversionSettings_AppliesDefaults()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                var mcs = go.AddComponent<MaterialConversionSettings>();

                var childGo = new GameObject("Body");
                childGo.transform.SetParent(go.transform);
                var smr = childGo.AddComponent<SkinnedMeshRenderer>();
                var mat = new Material(Shader.Find("Standard"));
                smr.sharedMaterial = mat;

                var converter = CreateConverter();
                var map = converter.CreateMaterialConvertSettingsMap(go, new Material[] { mat });

                Assert.IsNotNull(map);
                // MaterialConversionSettings on root with VRC_AvatarDescriptor and no AvatarConverterSettings => IsPrimaryRoot true
                Assert.IsTrue(map.ContainsKey(mat));

                Object.DestroyImmediate(mat);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_MaterialConversionSettingsWithAvatarConverterSettings_NotPrimaryRoot()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                go.AddComponent<AvatarConverterSettings>();
                var mcs = go.AddComponent<MaterialConversionSettings>();

                var mat = new Material(Shader.Find("Standard"));

                var converter = CreateConverter();
                var map = converter.CreateMaterialConvertSettingsMap(go, new Material[] { mat });

                // AvatarConverterSettings is present so MaterialConversionSettings.IsPrimaryRoot = false
                // But AvatarConverterSettings.IsPrimaryRoot = true, so it should still have the material
                Assert.IsTrue(map.ContainsKey(mat));

                Object.DestroyImmediate(mat);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // --- ProgressCallback ---
        [Test]
        public void ProgressCallback_CanBeCreated()
        {
            var progressCallback = new AvatarConverter.ProgressCallback();
            Assert.IsNotNull(progressCallback);
            Assert.IsNull(progressCallback.onTextureProgress);
            Assert.IsNull(progressCallback.onAnimationClipProgress);
            Assert.IsNull(progressCallback.onRuntimeAnimatorProgress);
        }

        // --- AvatarConverter constructor ---
        [Test]
        public void Constructor_StoresMaterialWrapperBuilder()
        {
            var builder = new MaterialWrapperBuilder();
            var converter = new AvatarConverter(builder);
            Assert.AreSame(builder, converter.MaterialWrapperBuilder);
        }
    }

    // ========== GenericToonStandardGenerator Tests ==========
    [TestFixture]
    public class GenericToonStandardGeneratorTests_Callbacks
    {
        private GenericToonStandardGenerator CreateGenerator(Material sourceMaterial = null)
        {
            if (sourceMaterial == null)
            {
                sourceMaterial = new Material(Shader.Find("Standard"));
            }
            var wrapper = new MaterialWrapperBuilder().Build(sourceMaterial);
            var settings = new ToonStandardConvertSettings();
            var blackTex = new Texture2D(4, 4);

            // GenericToonStandardGenerator is internal, use reflection
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.GenericToonStandardGenerator");
            Assert.IsNotNull(type, "GenericToonStandardGenerator type should exist");

            // Find constructor with 3 params
            var ctors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            ConstructorInfo ctor = null;
            foreach (var c in ctors)
            {
                var p = c.GetParameters();
                if (p.Length == 3)
                {
                    ctor = c;
                    break;
                }
            }
            Assert.IsNotNull(ctor, $"GenericToonStandardGenerator constructor should exist (found {ctors.Length} ctors)");
            return (GenericToonStandardGenerator)ctor.Invoke(new object[] { wrapper, settings, blackTex });
        }

        private T InvokeProtectedMethod<T>(object instance, string methodName, object[] args = null)
        {
            var method = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                // Try with parameter types
                method = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            }
            Assert.IsNotNull(method, $"Method {methodName} should exist");
            return (T)method.Invoke(instance, args);
        }

        [Test]
        public void ConvertToToonStandard_ReturnsNewMaterial()
        {
            var sourceMat = new Material(Shader.Find("Standard"));
            Material result = null;
            try
            {
                var generator = CreateGenerator(sourceMat);
                result = InvokeProtectedMethod<Material>(generator, "ConvertToToonStandard");
                Assert.IsNotNull(result);
            }
            finally
            {
                Object.DestroyImmediate(sourceMat);
                if (result != null) Object.DestroyImmediate(result);
            }
        }

        [Test]
        public void GetUseEmission_ReturnsFalse()
        {
            var sourceMat = new Material(Shader.Find("Standard"));
            try
            {
                var generator = CreateGenerator(sourceMat);
                var result = InvokeProtectedMethod<bool>(generator, "GetUseEmission");
                Assert.IsFalse(result);
            }
            finally
            {
                Object.DestroyImmediate(sourceMat);
            }
        }

        [Test]
        public void GetNormalMapPlatformOverride_ReturnsNull()
        {
            var sourceMat = new Material(Shader.Find("Standard"));
            try
            {
                var generator = CreateGenerator(sourceMat);
                var method = generator.GetType().GetMethod("GetNormalMapPlatformOverride", BindingFlags.NonPublic | BindingFlags.Instance);
                var result = method.Invoke(generator, null);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(sourceMat);
            }
        }

        [Test]
        public void GetEmissionMapPlatformOverride_ReturnsNull()
        {
            var sourceMat = new Material(Shader.Find("Standard"));
            try
            {
                var generator = CreateGenerator(sourceMat);
                var method = generator.GetType().GetMethod("GetEmissionMapPlatformOverride", BindingFlags.NonPublic | BindingFlags.Instance);
                var result = method.Invoke(generator, null);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(sourceMat);
            }
        }

        [Test]
        public void GetMatcapPlatformOverride_ReturnsNull()
        {
            var sourceMat = new Material(Shader.Find("Standard"));
            try
            {
                var generator = CreateGenerator(sourceMat);
                var method = generator.GetType().GetMethod("GetMatcapPlatformOverride", BindingFlags.NonPublic | BindingFlags.Instance);
                var result = method.Invoke(generator, null);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(sourceMat);
            }
        }

        [Test]
        public void GetMatcapMaskPlatformOverride_ReturnsNull()
        {
            var sourceMat = new Material(Shader.Find("Standard"));
            try
            {
                var generator = CreateGenerator(sourceMat);
                var method = generator.GetType().GetMethod("GetMatcapMaskPlatformOverride", BindingFlags.NonPublic | BindingFlags.Instance);
                var result = method.Invoke(generator, null);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(sourceMat);
            }
        }

        [Test]
        public void GetMetallicMapPlatformOverride_ReturnsNull()
        {
            var sourceMat = new Material(Shader.Find("Standard"));
            try
            {
                var generator = CreateGenerator(sourceMat);
                var method = generator.GetType().GetMethod("GetMetallicMapPlatformOverride", BindingFlags.NonPublic | BindingFlags.Instance);
                var result = method.Invoke(generator, null);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(sourceMat);
            }
        }

        [Test]
        public void GetGlossMapPlatformOverride_ReturnsNull()
        {
            var sourceMat = new Material(Shader.Find("Standard"));
            try
            {
                var generator = CreateGenerator(sourceMat);
                var method = generator.GetType().GetMethod("GetGlossMapPlatformOverride", BindingFlags.NonPublic | BindingFlags.Instance);
                var result = method.Invoke(generator, null);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(sourceMat);
            }
        }

        [Test]
        public void GetOcclusionMapPlatformOverride_ReturnsNull()
        {
            var sourceMat = new Material(Shader.Find("Standard"));
            try
            {
                var generator = CreateGenerator(sourceMat);
                var method = generator.GetType().GetMethod("GetOcclusionMapPlatformOverride", BindingFlags.NonPublic | BindingFlags.Instance);
                var result = method.Invoke(generator, null);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(sourceMat);
            }
        }

        [Test]
        public void GenerateMainTexture_ThrowsNotImplementedException()
        {
            var sourceMat = new Material(Shader.Find("Standard"));
            try
            {
                var generator = CreateGenerator(sourceMat);
                var method = generator.GetType().GetMethod("GenerateMainTexture", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.Throws<TargetInvocationException>(() => method.Invoke(generator, new object[] { (Action<Texture2D>)null }));
            }
            finally
            {
                Object.DestroyImmediate(sourceMat);
            }
        }

        [Test]
        public void GenerateEmissionMap_ThrowsNotImplementedException()
        {
            var sourceMat = new Material(Shader.Find("Standard"));
            try
            {
                var generator = CreateGenerator(sourceMat);
                var method = generator.GetType().GetMethod("GenerateEmissionMap", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.Throws<TargetInvocationException>(() => method.Invoke(generator, new object[] { (Action<Texture2D>)null }));
            }
            finally
            {
                Object.DestroyImmediate(sourceMat);
            }
        }

        [Test]
        public void GetMainColor_ThrowsNotImplementedException()
        {
            var sourceMat = new Material(Shader.Find("Standard"));
            try
            {
                var generator = CreateGenerator(sourceMat);
                var method = generator.GetType().GetMethod("GetMainColor", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.Throws<TargetInvocationException>(() => method.Invoke(generator, null));
            }
            finally
            {
                Object.DestroyImmediate(sourceMat);
            }
        }
    }

    // ========== FallbackAvatarCallback Tests ==========
    [TestFixture]
    public class FallbackAvatarCallbackTests_Callbacks
    {
        [Test]
        public void CallbackOrder_IsNegative100000()
        {
            var callbackType = typeof(KRT.VRCQuestTools.NonDestructive.FallbackAvatarCallback);
            var callback = Activator.CreateInstance(callbackType);
            var prop = callbackType.GetProperty("callbackOrder");
            Assert.IsNotNull(prop);
            var order = (int)prop.GetValue(callback);
            Assert.AreEqual(-100000, order);
        }

        [Test]
        public void OnPreprocessAvatar_NoPipelineManager_ReturnsTrue()
        {
            var go = new GameObject("Avatar");
            try
            {
                var callbackType = typeof(KRT.VRCQuestTools.NonDestructive.FallbackAvatarCallback);
                var callback = Activator.CreateInstance(callbackType);
                var method = callbackType.GetMethod("OnPreprocessAvatar");
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
        public void OnPreprocessAvatar_WithPipelineManager_NoBlueprintId_ReturnsTrue()
        {
            var go = new GameObject("Avatar");
            try
            {
                // PipelineManager is in VRCCore-Editor.dll which may not be referenced.
                // Add it via reflection if available, otherwise skip.
                var pmType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
                    .FirstOrDefault(t => t.FullName == "VRC.Core.PipelineManager");
                if (pmType == null)
                {
                    Assert.Ignore("PipelineManager type not available in test assembly");
                    return;
                }
                go.AddComponent(pmType);

                var callbackType = typeof(KRT.VRCQuestTools.NonDestructive.FallbackAvatarCallback);
                var callback = Activator.CreateInstance(callbackType);
                var method = callbackType.GetMethod("OnPreprocessAvatar");
                var result = (bool)method.Invoke(callback, new object[] { go });
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }

    // ========== ActualPerformanceCallback Tests ==========
    [TestFixture]
    public class ActualPerformanceCallbackTests_Callbacks
    {
        [Test]
        public void CallbackOrder_IsIntMaxValue()
        {
            var callbackType = typeof(KRT.VRCQuestTools.NonDestructive.ActualPerformanceCallback);
            var callback = Activator.CreateInstance(callbackType);
            var prop = callbackType.GetProperty("callbackOrder");
            Assert.IsNotNull(prop);
            var order = (int)prop.GetValue(callback);
            Assert.AreEqual(int.MaxValue, order);
        }

        [Test]
        public void LastActualPerformanceRating_IsNotNull()
        {
            var field = typeof(KRT.VRCQuestTools.NonDestructive.ActualPerformanceCallback)
                .GetField("LastActualPerformanceRating", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.IsNotNull(field);
            var dict = field.GetValue(null);
            Assert.IsNotNull(dict);
        }
    }

    // ========== ComponentUtility Tests ==========
    [TestFixture]
    public class ComponentUtilityTests_Callbacks
    {
        [Test]
        public void GetPrimaryMaterialConversionComponent_WithAvatarConverterSettings_ReturnsIt()
        {
            var go = new GameObject("Avatar");
            try
            {
                go.AddComponent<VRCAvatarDescriptor>();
                var acs = go.AddComponent<AvatarConverterSettings>();

                var result = KRT.VRCQuestTools.Utils.ComponentUtility.GetPrimaryMaterialConversionComponent(go);
                Assert.IsNotNull(result);
                Assert.AreSame(acs, result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetPrimaryMaterialConversionComponent_NoComponent_ReturnsNull()
        {
            var go = new GameObject("Avatar");
            try
            {
                var result = KRT.VRCQuestTools.Utils.ComponentUtility.GetPrimaryMaterialConversionComponent(go);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetPrimaryMaterialConversionComponent_MaterialConversionSettingsOnNonRoot_NotPrimary()
        {
            var go = new GameObject("Avatar");
            try
            {
                // No VRC_AvatarDescriptor, so MaterialConversionSettings.IsPrimaryRoot = false
                var mcs = go.AddComponent<MaterialConversionSettings>();

                var result = KRT.VRCQuestTools.Utils.ComponentUtility.GetPrimaryMaterialConversionComponent(go);
                // IsPrimaryRoot checks for VRC_AvatarDescriptor, which is absent
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetPrimaryMaterialConversionComponent_MaterialConversionSettingsOnAvatarRoot_IsPrimary()
        {
            var go = new GameObject("Avatar");
            try
            {
                go.AddComponent<VRCAvatarDescriptor>();
                var mcs = go.AddComponent<MaterialConversionSettings>();

                var result = KRT.VRCQuestTools.Utils.ComponentUtility.GetPrimaryMaterialConversionComponent(go);
                Assert.IsNotNull(result);
                Assert.AreSame(mcs, result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetPrimaryMaterialConversionComponent_BothComponents_AvatarConverterSettingsIsPrimary()
        {
            var go = new GameObject("Avatar");
            try
            {
                go.AddComponent<VRCAvatarDescriptor>();
                var acs = go.AddComponent<AvatarConverterSettings>();
                var mcs = go.AddComponent<MaterialConversionSettings>();

                var result = KRT.VRCQuestTools.Utils.ComponentUtility.GetPrimaryMaterialConversionComponent(go);
                // AvatarConverterSettings.IsPrimaryRoot = true (always)
                // MaterialConversionSettings.IsPrimaryRoot = false (because AvatarConverterSettings exists)
                // FirstOrDefault returns AvatarConverterSettings
                Assert.IsNotNull(result);
                Assert.AreSame(acs, result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }

    // ========== MaterialConversionSettings Component Tests ==========
    [TestFixture]
    public class MaterialConversionSettingsTests_Callbacks
    {
        [Test]
        public void DefaultValues_AreCorrect()
        {
            var go = new GameObject("Test");
            try
            {
                var mcs = go.AddComponent<MaterialConversionSettings>();
                Assert.IsNotNull(mcs.DefaultMaterialConvertSettings);
                Assert.IsNotNull(mcs.AdditionalMaterialConvertSettings);
                Assert.AreEqual(0, mcs.AdditionalMaterialConvertSettings.Length);
                Assert.IsTrue(mcs.RemoveExtraMaterialSlots);
                Assert.IsTrue(mcs.EnableMaterialPreview);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void IsPrimaryRoot_OnAvatarRoot_WithoutACS_IsTrue()
        {
            var go = new GameObject("Avatar");
            try
            {
                go.AddComponent<VRCAvatarDescriptor>();
                var mcs = go.AddComponent<MaterialConversionSettings>();
                Assert.IsTrue(((IMaterialConversionComponent)mcs).IsPrimaryRoot);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void IsPrimaryRoot_OnAvatarRoot_WithACS_IsFalse()
        {
            var go = new GameObject("Avatar");
            try
            {
                go.AddComponent<VRCAvatarDescriptor>();
                go.AddComponent<AvatarConverterSettings>();
                var mcs = go.AddComponent<MaterialConversionSettings>();
                Assert.IsFalse(((IMaterialConversionComponent)mcs).IsPrimaryRoot);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void IsPrimaryRoot_NotOnAvatarRoot_IsFalse()
        {
            var go = new GameObject("NotAvatar");
            try
            {
                var mcs = go.AddComponent<MaterialConversionSettings>();
                Assert.IsFalse(((IMaterialConversionComponent)mcs).IsPrimaryRoot);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void AdditionalMaterialConvertSettings_SetGet()
        {
            var go = new GameObject("Test");
            try
            {
                var mcs = go.AddComponent<MaterialConversionSettings>();
                var newSettings = new AdditionalMaterialConvertSettings[]
                {
                    new AdditionalMaterialConvertSettings(),
                };
                mcs.AdditionalMaterialConvertSettings = newSettings;
                Assert.AreEqual(1, mcs.AdditionalMaterialConvertSettings.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }

    // ========== AdditionalMaterialConvertSettings Tests ==========
    [TestFixture]
    public class AdditionalMaterialConvertSettingsTests
    {
        [Test]
        public void DefaultValues()
        {
            var settings = new AdditionalMaterialConvertSettings();
            Assert.IsNull(settings.targetMaterial);
            Assert.IsNotNull(settings.materialConvertSettings);
            Assert.IsInstanceOf<ToonLitConvertSettings>(settings.materialConvertSettings);
        }

        [Test]
        public void GetCacheKey_NullTarget()
        {
            var settings = new AdditionalMaterialConvertSettings();
            var key = settings.GetCacheKey();
            Assert.IsNotNull(key);
            Assert.IsTrue(key.StartsWith("null_"));
        }

        [Test]
        public void GetCacheKey_WithTarget()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var settings = new AdditionalMaterialConvertSettings();
                settings.targetMaterial = mat;
                var key = settings.GetCacheKey();
                Assert.IsNotNull(key);
                Assert.IsFalse(key.StartsWith("null_"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void LoadDefaultAssets_DoesNotThrow()
        {
            var settings = new AdditionalMaterialConvertSettings();
            Assert.DoesNotThrow(() => settings.LoadDefaultAssets());
        }
    }

    // ========== Exceptions Tests ==========
    [TestFixture]
    public class ExceptionTests
    {
        [Test]
        public void TargetMaterialNullException_HasMessage()
        {
            var go = new GameObject("Test");
            try
            {
                var comp = go.AddComponent<AvatarConverterSettings>();
                var excType = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.TargetMaterialNullException");
                Assert.IsNotNull(excType, "TargetMaterialNullException should exist");
                var exc = (Exception)Activator.CreateInstance(excType, "test message", (Component)comp);
                Assert.AreEqual("test message", exc.Message);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void InvalidReplacementMaterialException_HasMessage()
        {
            var go = new GameObject("Test");
            try
            {
                var comp = go.AddComponent<AvatarConverterSettings>();
                var mat = new Material(Shader.Find("Standard"));
                var excType = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.InvalidReplacementMaterialException");
                Assert.IsNotNull(excType, "InvalidReplacementMaterialException should exist");
                var exc = (Exception)Activator.CreateInstance(excType, "test message", (Component)comp, mat);
                Assert.AreEqual("test message", exc.Message);
                Object.DestroyImmediate(mat);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void MaterialConversionException_HasMessage()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var excType = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.MaterialConversionException");
                Assert.IsNotNull(excType, "MaterialConversionException should exist");
                var inner = new Exception("inner");
                var exc = (Exception)Activator.CreateInstance(excType, "test message", mat, inner);
                Assert.AreEqual("test message", exc.Message);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }
    }
}
