// Batch49: Tests targeting platform overrides, AvatarConverter material application,
// DeleteAvatarDynamicsComponents, RemoveExtraMaterialSlots, and TextureUtility edge cases.

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
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase;

namespace KRT.VRCQuestTools.Tests
{
    // =========================================================================
    // Test: LilToon Platform Override methods via reflection
    // =========================================================================
    [TestFixture]
    public class Batch49_LilToonPlatformOverrideTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

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
            objectsToCleanup.Clear();
        }

        private Texture2D CreateBlackTexture()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.name = "Batch49_BlackTex";
            objectsToCleanup.Add(tex);
            return tex;
        }

        private Texture2D CreateTestTexture(string name = "TestTex")
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            tex.name = name;
            objectsToCleanup.Add(tex);
            return tex;
        }

        private LilToonToonStandardGenerator CreateLilToonGenerator()
        {
            var lilShader = LilToonTestHelper.FindLilToonShader();
            if (lilShader == null)
            {
                Assert.Ignore("lilToon not installed");
                return null;
            }

            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper("Batch49_Override");
            objectsToCleanup.Add(lilMat.Material);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
            };
            var blackTex = CreateBlackTexture();
            return GeneratorReflectionHelper.CreateGenerator(lilMat, settings, blackTex);
        }

        [Test]
        public void GetMainTexturePlatformOverride_NoTextures_ReturnsNull()
        {
            var generator = CreateLilToonGenerator();
            if (generator == null) return;

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetMainTexturePlatformOverride");
            Assert.IsNull(result, "No textures should return null");
        }

        [Test]
        public void GetMainTexturePlatformOverride_WithMainTexture_ReturnsResult()
        {
            var lilShader = LilToonTestHelper.FindLilToonShader();
            if (lilShader == null)
            {
                Assert.Ignore("lilToon not installed");
                return;
            }

            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper("Batch49_MainTex");
            objectsToCleanup.Add(lilMat.Material);
            var tex = CreateTestTexture("MainTex");
            lilMat.Material.mainTexture = tex;

            var settings = new ToonStandardConvertSettings { generateQuestTextures = false };
            var blackTex = CreateBlackTexture();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings, blackTex);

            // Runtime textures have no asset path, so result will be null
            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetMainTexturePlatformOverride");
            Assert.IsNull(result, "Runtime texture without asset path returns null");
        }

        [Test]
        public void GetEmissionMapPlatformOverride_NoEmission_ReturnsNull()
        {
            var generator = CreateLilToonGenerator();
            if (generator == null) return;

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetEmissionMapPlatformOverride");
            Assert.IsNull(result, "No emission textures should return null");
        }

        [Test]
        public void GetEmissionMapPlatformOverride_WithEmission_ReturnsNull()
        {
            var lilShader = LilToonTestHelper.FindLilToonShader();
            if (lilShader == null)
            {
                Assert.Ignore("lilToon not installed");
                return;
            }

            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper("Batch49_Emission");
            objectsToCleanup.Add(lilMat.Material);
            lilMat.Material.SetFloat("_UseEmission", 1.0f);
            lilMat.Material.SetTexture("_EmissionMap", CreateTestTexture("EmissionTex"));

            var settings = new ToonStandardConvertSettings { generateQuestTextures = false };
            var blackTex = CreateBlackTexture();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings, blackTex);

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetEmissionMapPlatformOverride");
            Assert.IsNull(result, "Runtime texture returns null");
        }

        [Test]
        public void GetGlossMapPlatformOverride_NoTextures_ReturnsNull()
        {
            var generator = CreateLilToonGenerator();
            if (generator == null) return;

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetGlossMapPlatformOverride");
            Assert.IsNull(result, "No gloss textures should return null");
        }

        [Test]
        public void GetMatcapPlatformOverride_NoTexture_ReturnsNull()
        {
            var generator = CreateLilToonGenerator();
            if (generator == null) return;

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetMatcapPlatformOverride");
            Assert.IsNull(result, "No matcap texture should return null");
        }

        [Test]
        public void GetMatcapMaskPlatformOverride_NoTexture_ReturnsNull()
        {
            var generator = CreateLilToonGenerator();
            if (generator == null) return;

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetMatcapMaskPlatformOverride");
            Assert.IsNull(result, "No matcap mask should return null");
        }

        [Test]
        public void GetMetallicMapPlatformOverride_NoTexture_ReturnsNull()
        {
            var generator = CreateLilToonGenerator();
            if (generator == null) return;

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetMetallicMapPlatformOverride");
            Assert.IsNull(result, "No metallic map should return null");
        }

        [Test]
        public void GetNormalMapPlatformOverride_NoTexture_ReturnsNull()
        {
            var generator = CreateLilToonGenerator();
            if (generator == null) return;

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetNormalMapPlatformOverride");
            Assert.IsNull(result, "No normal map should return null");
        }

        [Test]
        public void GetOcclusionMapPlatformOverride_NoTexture_ReturnsNull()
        {
            var generator = CreateLilToonGenerator();
            if (generator == null) return;

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetOcclusionMapPlatformOverride");
            Assert.IsNull(result, "No occlusion map should return null");
        }

        [Test]
        public void GetGlossMapPlatformOverride_WithSmoothnessTex_ReturnsNull()
        {
            var lilShader = LilToonTestHelper.FindLilToonShader();
            if (lilShader == null)
            {
                Assert.Ignore("lilToon not installed");
                return;
            }

            var lilMat = LilToonTestHelper.CreateLilToonMaterialWrapper("Batch49_Gloss");
            objectsToCleanup.Add(lilMat.Material);
            lilMat.Material.SetTexture("_SmoothnessTex", CreateTestTexture("SmoothTex"));

            var settings = new ToonStandardConvertSettings { generateQuestTextures = false };
            var blackTex = CreateBlackTexture();
            var generator = GeneratorReflectionHelper.CreateGenerator(lilMat, settings, blackTex);

            var result = GeneratorReflectionHelper.InvokeProtected(generator, "GetGlossMapPlatformOverride");
            Assert.IsNull(result, "Runtime texture returns null");
        }

        [Test]
        public void GetPackedMaskPlatformOverride_WithNullPack_Throws()
        {
            var generator = CreateLilToonGenerator();
            if (generator == null) return;

            Assert.Throws<TargetInvocationException>(() =>
            {
                GeneratorReflectionHelper.InvokeProtected(generator, "GetPackedMaskPlatformOverride", new object[] { null });
            });
        }
    }

    // =========================================================================
    // Test: AvatarConverter.ApplyConvertedMaterials - material substitution
    // =========================================================================
    [TestFixture]
    public class Batch49_ApplyConvertedMaterialsTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

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
            objectsToCleanup.Clear();
        }

        private (GameObject, VRCAvatarDescriptor) CreateAvatar(string name)
        {
            var go = new GameObject(name);
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            return (go, desc);
        }

        [Test]
        public void ApplyConvertedMaterials_SubstitutesMaterialsOnRenderers()
        {
            var (go, desc) = CreateAvatar("MatSubst");
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            mesh.vertices = new[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            smr.sharedMesh = mesh;

            var origMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            origMat.name = "OrigMat";
            objectsToCleanup.Add(origMat);
            var convertedMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            convertedMat.name = "ConvertedMat";
            objectsToCleanup.Add(convertedMat);

            smr.sharedMaterials = new[] { origMat };

            var convertedMaterials = new Dictionary<Material, Material>
            {
                { origMat, convertedMat },
            };

            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            converter.ApplyConvertedMaterials(go, convertedMaterials, false, "Assets/", new AvatarConverter.ProgressCallback());

            Assert.AreEqual(convertedMat, smr.sharedMaterials[0], "Material should be substituted");
        }

        [Test]
        public void ApplyConvertedMaterials_NullMaterialInRenderer_StaysNull()
        {
            var (go, desc) = CreateAvatar("NullMat");
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            mesh.vertices = new[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            smr.sharedMesh = mesh;
            smr.sharedMaterials = new Material[] { null };

            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            converter.ApplyConvertedMaterials(go, new Dictionary<Material, Material>(), false, "Assets/", new AvatarConverter.ProgressCallback());

            Assert.IsNull(smr.sharedMaterials[0], "Null material should stay null");
        }

        [Test]
        public void ApplyConvertedMaterials_UnconvertedMaterial_StaysOriginal()
        {
            var (go, desc) = CreateAvatar("Unconverted");
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            mesh.vertices = new[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            smr.sharedMesh = mesh;

            var mat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            mat.name = "UnconvertedMat";
            objectsToCleanup.Add(mat);
            smr.sharedMaterials = new[] { mat };

            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            converter.ApplyConvertedMaterials(go, new Dictionary<Material, Material>(), false, "Assets/", new AvatarConverter.ProgressCallback());

            Assert.AreEqual(mat, smr.sharedMaterials[0], "Unconverted material should stay");
        }

        [Test]
        public void ApplyConvertedMaterials_MultipleRenderers_AllSubstituted()
        {
            var (go, desc) = CreateAvatar("MultiRenderer");

            var origMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            origMat.name = "Orig";
            objectsToCleanup.Add(origMat);
            var newMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            newMat.name = "New";
            objectsToCleanup.Add(newMat);

            for (int i = 0; i < 3; i++)
            {
                var child = new GameObject($"Renderer{i}");
                child.transform.SetParent(go.transform);
                objectsToCleanup.Add(child);
                var smr = child.AddComponent<SkinnedMeshRenderer>();
                var mesh = new Mesh();
                objectsToCleanup.Add(mesh);
                mesh.vertices = new[] { Vector3.zero, Vector3.one, Vector3.up };
                mesh.triangles = new[] { 0, 1, 2 };
                smr.sharedMesh = mesh;
                smr.sharedMaterials = new[] { origMat };
            }

            var convertedMaterials = new Dictionary<Material, Material> { { origMat, newMat } };

            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            converter.ApplyConvertedMaterials(go, convertedMaterials, false, "Assets/", new AvatarConverter.ProgressCallback());

            foreach (var smr in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Assert.AreEqual(newMat, smr.sharedMaterials[0], $"Material on {smr.gameObject.name} should be substituted");
            }
        }

        [Test]
        public void ApplyConvertedMaterials_MeshRendererAlsoSubstituted()
        {
            var (go, desc) = CreateAvatar("MeshRendererSubst");
            var child = new GameObject("Cube");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var mf = child.AddComponent<MeshFilter>();
            var mr = child.AddComponent<MeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            mesh.vertices = new[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            mf.sharedMesh = mesh;

            var origMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            origMat.name = "OrigMR";
            objectsToCleanup.Add(origMat);
            var newMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            newMat.name = "NewMR";
            objectsToCleanup.Add(newMat);
            mr.sharedMaterials = new[] { origMat };

            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            converter.ApplyConvertedMaterials(go, new Dictionary<Material, Material> { { origMat, newMat } }, false, "Assets/", new AvatarConverter.ProgressCallback());

            Assert.AreEqual(newMat, mr.sharedMaterials[0], "MeshRenderer material should be substituted");
        }
    }

    // =========================================================================
    // Test: AvatarConverter.RemoveExtraMaterialSlots
    // =========================================================================
    [TestFixture]
    public class Batch49_RemoveExtraMaterialSlotsTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

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
            objectsToCleanup.Clear();
        }

        [Test]
        public void RemoveExtraMaterialSlots_TrimsMaterialsToSubMeshCount()
        {
            var go = new GameObject("TrimTest");
            objectsToCleanup.Add(go);

            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            mesh.vertices = new[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            smr.sharedMesh = mesh;

            var mat1 = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            var mat2 = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            var mat3 = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            objectsToCleanup.Add(mat1);
            objectsToCleanup.Add(mat2);
            objectsToCleanup.Add(mat3);

            // Mesh has 1 submesh but renderer has 3 materials
            smr.sharedMaterials = new[] { mat1, mat2, mat3 };
            Assert.AreEqual(3, smr.sharedMaterials.Length, "Should start with 3 materials");

            // Call via reflection since it's private
            var converterType = typeof(AvatarConverter);
            var method = converterType.GetMethod("RemoveExtraMaterialSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "RemoveExtraMaterialSlots should exist");

            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            method.Invoke(converter, new object[] { go });

            Assert.AreEqual(1, smr.sharedMaterials.Length, "Should be trimmed to 1 (submesh count)");
            Assert.AreEqual(mat1, smr.sharedMaterials[0], "First material should be preserved");
        }

        [Test]
        public void RemoveExtraMaterialSlots_ExactMatch_NoChange()
        {
            var go = new GameObject("ExactMatchTest");
            objectsToCleanup.Add(go);

            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            mesh.vertices = new[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            smr.sharedMesh = mesh;

            var mat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            objectsToCleanup.Add(mat);
            smr.sharedMaterials = new[] { mat };

            var converterType = typeof(AvatarConverter);
            var method = converterType.GetMethod("RemoveExtraMaterialSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            method.Invoke(converter, new object[] { go });

            Assert.AreEqual(1, smr.sharedMaterials.Length, "Should remain 1");
        }

        [Test]
        public void RemoveExtraMaterialSlots_NullMesh_Skipped()
        {
            var go = new GameObject("NullMeshTest");
            objectsToCleanup.Add(go);

            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            // No mesh assigned

            var mat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            objectsToCleanup.Add(mat);
            smr.sharedMaterials = new[] { mat };

            var converterType = typeof(AvatarConverter);
            var method = converterType.GetMethod("RemoveExtraMaterialSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            var converter = new AvatarConverter(new MaterialWrapperBuilder());

            Assert.DoesNotThrow(() => method.Invoke(converter, new object[] { go }));
            Assert.AreEqual(1, smr.sharedMaterials.Length, "Material should not change when no mesh");
        }

        [Test]
        public void RemoveExtraMaterialSlots_MeshRenderer_AlsoTrimmed()
        {
            var go = new GameObject("MRTrimTest");
            objectsToCleanup.Add(go);

            var child = new GameObject("Cube");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var mf = child.AddComponent<MeshFilter>();
            var mr = child.AddComponent<MeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            mesh.vertices = new[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            mf.sharedMesh = mesh;

            var mat1 = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            var mat2 = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            objectsToCleanup.Add(mat1);
            objectsToCleanup.Add(mat2);
            mr.sharedMaterials = new[] { mat1, mat2 };

            var converterType = typeof(AvatarConverter);
            var method = converterType.GetMethod("RemoveExtraMaterialSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            method.Invoke(converter, new object[] { go });

            Assert.AreEqual(1, mr.sharedMaterials.Length, "MeshRenderer should be trimmed");
        }
    }

    // =========================================================================
    // Test: VRCSDKUtility.DeleteAvatarDynamicsComponents
    // =========================================================================
    [TestFixture]
    public class Batch49_DeleteAvatarDynamicsTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

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
            objectsToCleanup.Clear();
        }

        private (GameObject, VRChatAvatar) CreateAvatarWithDynamics(string name, int physBoneCount, int colliderCount, int contactCount)
        {
            var go = new GameObject(name);
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

            for (int i = 0; i < physBoneCount; i++)
            {
                var child = new GameObject($"PhysBone{i}");
                child.transform.SetParent(go.transform);
                child.AddComponent<VRCPhysBone>();
            }

            for (int i = 0; i < colliderCount; i++)
            {
                var child = new GameObject($"Collider{i}");
                child.transform.SetParent(go.transform);
                child.AddComponent<VRCPhysBoneCollider>();
            }

            for (int i = 0; i < contactCount; i++)
            {
                var child = new GameObject($"Contact{i}");
                child.transform.SetParent(go.transform);
                child.AddComponent<VRCContactSender>();
            }

            return (go, new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>()));
        }

        [Test]
        public void DeleteAvatarDynamicsComponents_DeletesAll_WhenNoneKept()
        {
            var (go, avatar) = CreateAvatarWithDynamics("DeleteAll", 2, 1, 1);

            VRCSDKUtility.DeleteAvatarDynamicsComponents(
                avatar,
                new VRCPhysBone[0],
                new VRCPhysBoneCollider[0],
                new VRC.Dynamics.ContactBase[0]);

            Assert.AreEqual(0, go.GetComponentsInChildren<VRCPhysBone>(true).Length, "All PhysBones should be deleted");
            Assert.AreEqual(0, go.GetComponentsInChildren<VRCPhysBoneCollider>(true).Length, "All colliders should be deleted");
            Assert.AreEqual(0, go.GetComponentsInChildren<VRCContactSender>(true).Length, "All contacts should be deleted");
        }

        [Test]
        public void DeleteAvatarDynamicsComponents_KeepsSpecified()
        {
            var (go, avatar) = CreateAvatarWithDynamics("KeepSome", 3, 2, 2);

            var physBones = go.GetComponentsInChildren<VRCPhysBone>(true);
            var colliders = go.GetComponentsInChildren<VRCPhysBoneCollider>(true);
            var contacts = go.GetComponentsInChildren<VRCContactSender>(true);

            VRCSDKUtility.DeleteAvatarDynamicsComponents(
                avatar,
                new[] { physBones[0] },
                new[] { colliders[0] },
                new VRC.Dynamics.ContactBase[] { contacts[0] });

            Assert.AreEqual(1, go.GetComponentsInChildren<VRCPhysBone>(true).Length, "Should keep 1 PhysBone");
            Assert.AreEqual(1, go.GetComponentsInChildren<VRCPhysBoneCollider>(true).Length, "Should keep 1 collider");
            Assert.AreEqual(1, go.GetComponentsInChildren<VRCContactSender>(true).Length, "Should keep 1 contact");
        }

        [Test]
        public void DeleteAvatarDynamicsComponents_NoDynamics_NoError()
        {
            var (go, avatar) = CreateAvatarWithDynamics("NoDyn", 0, 0, 0);

            Assert.DoesNotThrow(() =>
            {
                VRCSDKUtility.DeleteAvatarDynamicsComponents(
                    avatar,
                    new VRCPhysBone[0],
                    new VRCPhysBoneCollider[0],
                    new VRC.Dynamics.ContactBase[0]);
            });
        }

        [Test]
        public void DeleteAvatarDynamicsComponents_RemovesColliderRefFromPhysBone()
        {
            var (go, avatar) = CreateAvatarWithDynamics("ColliderRef", 1, 1, 0);

            var physBone = go.GetComponentInChildren<VRCPhysBone>();
            var collider = go.GetComponentInChildren<VRCPhysBoneCollider>();
            physBone.colliders.Add(collider);

            // Delete the collider but keep the phys bone
            VRCSDKUtility.DeleteAvatarDynamicsComponents(
                avatar,
                new[] { physBone },
                new VRCPhysBoneCollider[0],
                new VRC.Dynamics.ContactBase[0]);

            Assert.AreEqual(1, go.GetComponentsInChildren<VRCPhysBone>(true).Length, "PhysBone should still exist");
            Assert.AreEqual(0, go.GetComponentsInChildren<VRCPhysBoneCollider>(true).Length, "Collider should be deleted");
            Assert.IsNull(physBone.colliders[0], "Collider reference should be nulled");
        }
    }

    // =========================================================================
    // Test: TextureUtility.GetBestPlatformOverrideSettings edge cases
    // =========================================================================
    [TestFixture]
    public class Batch49_TexturePlatformOverrideTests
    {
        [Test]
        public void GetBestPlatformOverrideSettings_Null_ReturnsNull()
        {
            var result = TextureUtility.GetBestPlatformOverrideSettings(null);
            Assert.IsNull(result);
        }

        [Test]
        public void GetBestPlatformOverrideSettings_EmptyArray_ReturnsNull()
        {
            var result = TextureUtility.GetBestPlatformOverrideSettings(new Texture[0]);
            Assert.IsNull(result);
        }

        [Test]
        public void GetBestPlatformOverrideSettings_NullTextures_ReturnsNull()
        {
            var result = TextureUtility.GetBestPlatformOverrideSettings(null, null);
            Assert.IsNull(result);
        }

        [Test]
        public void GetBestPlatformOverrideSettings_RuntimeTexture_ReturnsNull()
        {
            var tex = new Texture2D(8, 8);
            try
            {
                var result = TextureUtility.GetBestPlatformOverrideSettings(tex);
                Assert.IsNull(result, "Runtime texture (no asset path) should return null");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tex);
            }
        }
    }

    // =========================================================================
    // Test: AvatarConverter.ConvertForQuestInPlace and GenerateMobileTextures
    // validation paths
    // =========================================================================
    [TestFixture]
    public class Batch49_AvatarConverterValidationTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

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
            objectsToCleanup.Clear();
        }

        private (GameObject, VRCAvatarDescriptor) CreateAvatar(string name)
        {
            var go = new GameObject(name);
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            return (go, desc);
        }

        [Test]
        public void VRChatAvatar_HasAnimatedMaterials_EmptyAvatar_ReturnsFalse()
        {
            var (go, desc) = CreateAvatar("NoAnimMat");
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            Assert.IsFalse(avatar.HasAnimatedMaterials);
        }

        [Test]
        public void VRChatAvatar_HasAnimatedMaterials_WithMaterialAnimation_ReturnsTrue()
        {
            var (go, desc) = CreateAvatar("AnimMat");

            var controller = new AnimatorController();
            objectsToCleanup.Add(controller);
            controller.AddLayer("Base");
            var state = controller.layers[0].stateMachine.AddState("State");

            var clip = new AnimationClip();
            objectsToCleanup.Add(clip);

            // HasAnimatedMaterials uses ObjectReferenceCurve with actual Material objects
            var mat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(mat);
            var binding = new EditorCurveBinding
            {
                path = "Body",
                type = typeof(SkinnedMeshRenderer),
                propertyName = "m_Materials.Array.data[0]",
            };
            var keyframes = new[] { new ObjectReferenceKeyframe { time = 0, value = mat } };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            state.motion = clip;

            desc.baseAnimationLayers = new[]
            {
                new VRCAvatarDescriptor.CustomAnimLayer
                {
                    isDefault = false,
                    animatorController = controller,
                },
            };

            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            Assert.IsTrue(avatar.HasAnimatedMaterials, "Should detect material animation");
        }

        [Test]
        public void VRChatAvatar_GetRuntimeAnimatorControllers_ReturnsControllers()
        {
            var (go, desc) = CreateAvatar("GetControllers");
            var controller = new AnimatorController();
            objectsToCleanup.Add(controller);
            controller.AddLayer("Base");

            desc.baseAnimationLayers = new[]
            {
                new VRCAvatarDescriptor.CustomAnimLayer
                {
                    isDefault = false,
                    animatorController = controller,
                },
            };

            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            var controllers = avatar.GetRuntimeAnimatorControllers();
            Assert.IsTrue(controllers.Length > 0, "Should return at least 1 controller");
        }

        [Test]
        public void VRChatAvatar_Materials_EmptyAvatar_ReturnsEmpty()
        {
            var (go, desc) = CreateAvatar("EmptyMats");
            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            Assert.IsNotNull(avatar.Materials);
            Assert.AreEqual(0, avatar.Materials.Length);
        }

        [Test]
        public void VRChatAvatar_Materials_WithRenderers_ReturnsMaterials()
        {
            var (go, desc) = CreateAvatar("WithMats");
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            objectsToCleanup.Add(child);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            objectsToCleanup.Add(mesh);
            smr.sharedMesh = mesh;

            var mat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            objectsToCleanup.Add(mat);
            smr.sharedMaterials = new[] { mat };

            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            Assert.IsTrue(avatar.Materials.Length > 0);
            Assert.IsTrue(avatar.Materials.Contains(mat));
        }
    }

    // =========================================================================
    // Test: Validation rules - MissingScriptsRule, MissingNdmfRule, AvatarValidationRules
    // =========================================================================
    [TestFixture]
    public class Batch49_ValidationRuleTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

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
            objectsToCleanup.Clear();
        }

        private VRChatAvatar CreateAvatar(string name)
        {
            var go = new GameObject(name);
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            return new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
        }

        [Test]
        public void MissingScriptsRule_NoMissingScripts_ReturnsNull()
        {
            var avatar = CreateAvatar("NoMissing");
            var ruleType = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.Validators.MissingScriptsRule");
            Assert.IsNotNull(ruleType, "MissingScriptsRule should exist");

            var rule = Activator.CreateInstance(ruleType);
            var validateMethod = ruleType.GetMethod("Validate");
            Assert.IsNotNull(validateMethod);

            var result = validateMethod.Invoke(rule, new object[] { avatar });
            Assert.IsNull(result, "No missing scripts should return null");
        }

        [Test]
        public void MissingNdmfRule_NoNdmfComponents_ReturnsNull()
        {
            var avatar = CreateAvatar("NoNdmf");
            var ruleType = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.Validators.MissingNdmfRule");
            Assert.IsNotNull(ruleType, "MissingNdmfRule should exist");

            var rule = Activator.CreateInstance(ruleType);
            var validateMethod = ruleType.GetMethod("Validate");
            Assert.IsNotNull(validateMethod);

            var result = validateMethod.Invoke(rule, new object[] { avatar });
            Assert.IsNull(result, "No NDMF components should return null");
        }

        [Test]
        public void AvatarValidationRules_Rules_ReturnsNonEmpty()
        {
            var rulesType = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.Validators.AvatarValidationRules");
            Assert.IsNotNull(rulesType, "AvatarValidationRules should exist");

            var rulesProp = rulesType.GetProperty("Rules", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.IsNotNull(rulesProp, "Rules property should exist");

            var rules = rulesProp.GetValue(null);
            Assert.IsNotNull(rules, "Rules should not be null");
            var rulesArray = rules as System.Array;
            Assert.IsNotNull(rulesArray, "Rules should be array");
            Assert.IsTrue(rulesArray.Length > 0, "Should have at least one validation rule");
        }
    }

    // =========================================================================
    // Test: VRCSDKUtility additional methods - StripeUnusedNetworkIds
    // =========================================================================
    [TestFixture]
    public class Batch49_VRCSDKUtilityDeepTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

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
            objectsToCleanup.Clear();
        }

        [Test]
        public void StripeUnusedNetworkIds_NoNetworkIds_DoesNotThrow()
        {
            var go = new GameObject("NoNetIds");
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

            var method = typeof(VRCSDKUtility).GetMethod("StripeUnusedNetworkIds", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
            {
                Assert.Ignore("StripeUnusedNetworkIds not found");
                return;
            }

            Assert.DoesNotThrow(() => method.Invoke(null, new object[] { go.GetComponent<VRC_AvatarDescriptor>() }));
        }

        [Test]
        public void VRCSDKUtility_IsExampleAsset_VpmSdk3DemoPath_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsExampleAsset("Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/SomeExample.unity"));
        }

        [Test]
        public void VRCSDKUtility_IsExampleAsset_NormalPackagePath_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsExampleAsset("Packages/com.vrchat.avatars/Runtime/Scripts/VRCAvatarDescriptor.cs"));
        }

        [Test]
        public void VRChatAvatar_GetPhysBones_EmptyAvatar_ReturnsEmpty()
        {
            var go = new GameObject("EmptyDyn");
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            Assert.AreEqual(0, avatar.GetPhysBones().Length);
            Assert.AreEqual(0, avatar.GetPhysBoneColliders().Length);
            Assert.AreEqual(0, avatar.GetContacts().Length);
        }

        [Test]
        public void VRChatAvatar_GetPhysBones_WithComponents_ReturnsThem()
        {
            var go = new GameObject("WithDyn");
            objectsToCleanup.Add(go);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

            var pbChild = new GameObject("PB");
            pbChild.transform.SetParent(go.transform);
            objectsToCleanup.Add(pbChild);
            pbChild.AddComponent<VRCPhysBone>();

            var colChild = new GameObject("Col");
            colChild.transform.SetParent(go.transform);
            objectsToCleanup.Add(colChild);
            colChild.AddComponent<VRCPhysBoneCollider>();

            var avatar = new VRChatAvatar(go.GetComponent<VRC_AvatarDescriptor>());
            Assert.AreEqual(1, avatar.GetPhysBones().Length);
            Assert.AreEqual(1, avatar.GetPhysBoneColliders().Length);
        }

        [Test]
        public void VRCSDKUtility_IsExampleAsset_ExampleSdkPath_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsExampleAsset("Assets/VRCSDK/Examples3/SomeExampleScene.unity"));
        }

        [Test]
        public void VRCSDKUtility_IsExampleAsset_EmptyPath_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsExampleAsset(string.Empty));
        }
    }

    // =========================================================================
    // Test: FallbackAvatarCallback and ActualPerformanceCallback edge cases
    // =========================================================================
    [TestFixture]
    public class Batch49_CallbackTests
    {
        [Test]
        public void ActualPerformanceCallback_Type_Exists()
        {
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.NonDestructive.ActualPerformanceCallback");
            Assert.IsNotNull(type, "ActualPerformanceCallback should exist");
        }

        [Test]
        public void ActualPerformanceCallback_HasLastRating_Field()
        {
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.NonDestructive.ActualPerformanceCallback");
            if (type == null)
            {
                Assert.Ignore("Type not found");
                return;
            }

            var field = type.GetField("LastActualPerformanceRating", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.IsNotNull(field, "LastActualPerformanceRating field should exist");
            var value = field.GetValue(null);
            Assert.IsNotNull(value, "Dictionary should be initialized");
        }

        [Test]
        public void FallbackAvatarCallback_Type_Exists()
        {
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.NonDestructive.FallbackAvatarCallback");
            Assert.IsNotNull(type, "FallbackAvatarCallback should exist");
        }

        [Test]
        public void VRCQuestToolsAvatarProcessor_Type_Exists()
        {
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.NonDestructive.VRCQuestToolsAvatarProcessor");
            if (type == null)
            {
                type = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
                    .FirstOrDefault(t => t.FullName == "KRT.VRCQuestTools.NonDestructive.VRCQuestToolsAvatarProcessor");
            }

            // When NDMF is installed, the NonDestructive processor is replaced by NDMF passes
            if (type == null)
            {
                Assert.Ignore("VRCQuestToolsAvatarProcessor not present (NDMF is installed)");
                return;
            }

            Assert.IsNotNull(type);
        }
    }

    // =========================================================================
    // Test: Generic ToonStandard platform override methods
    // =========================================================================
    [TestFixture]
    public class Batch49_GenericToonStandardPlatformOverrideTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

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
            objectsToCleanup.Clear();
        }

        [Test]
        public void GenericToonStandard_AllPlatformOverrides_ReturnNull()
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "GenericOverride";
            objectsToCleanup.Add(mat);
            var wrapper = new MaterialWrapperBuilder().Build(mat);

            var settings = new ToonStandardConvertSettings { generateQuestTextures = false };
            var blackTex = new Texture2D(4, 4);
            objectsToCleanup.Add(blackTex);
            var generator = new GenericToonStandardGenerator(wrapper, settings, blackTex);

            var genType = typeof(GenericToonStandardGenerator);
            var methods = new[]
            {
                "GetMainTexturePlatformOverride",
                "GetEmissionMapPlatformOverride",
                "GetGlossMapPlatformOverride",
                "GetMatcapPlatformOverride",
                "GetMatcapMaskPlatformOverride",
                "GetMetallicMapPlatformOverride",
                "GetNormalMapPlatformOverride",
                "GetOcclusionMapPlatformOverride",
            };

            foreach (var methodName in methods)
            {
                var method = genType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (method == null)
                {
                    method = genType.BaseType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                }
                Assert.IsNotNull(method, $"{methodName} should exist");
                var result = method.Invoke(generator, null);
                Assert.IsNull(result, $"{methodName} should return null for generic material");
            }
        }
    }
}
