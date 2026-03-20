// Tests for AvatarConverter private helper methods via reflection.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests.Models.VRChat
{
    [TestFixture]
    internal class AvatarConverterFindDescendantTests
    {
        private AvatarConverter converter;
        private MethodInfo findDescendantMethod;

        [SetUp]
        public void SetUp()
        {
            converter = new AvatarConverter(new MaterialWrapperBuilder());
            findDescendantMethod = typeof(AvatarConverter).GetMethod(
                "FindDescendant",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(findDescendantMethod, "FindDescendant method should exist");
        }

        [Test]
        public void FindDescendant_DirectChild_ReturnsChild()
        {
            var root = new GameObject("Root");
            var child = new GameObject("TargetChild");
            child.transform.SetParent(root.transform);
            try
            {
                var result = (GameObject)findDescendantMethod.Invoke(converter, new object[] { root, "TargetChild" });
                Assert.IsNotNull(result);
                Assert.AreEqual("TargetChild", result.name);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void FindDescendant_DeepChild_ReturnsChild()
        {
            var root = new GameObject("Root");
            var level1 = new GameObject("Level1");
            level1.transform.SetParent(root.transform);
            var level2 = new GameObject("Level2");
            level2.transform.SetParent(level1.transform);
            var target = new GameObject("DeepTarget");
            target.transform.SetParent(level2.transform);
            try
            {
                var result = (GameObject)findDescendantMethod.Invoke(converter, new object[] { root, "DeepTarget" });
                Assert.IsNotNull(result);
                Assert.AreEqual("DeepTarget", result.name);
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
            var child = new GameObject("SomeChild");
            child.transform.SetParent(root.transform);
            try
            {
                var result = (GameObject)findDescendantMethod.Invoke(converter, new object[] { root, "NonExistent" });
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void FindDescendant_EmptyHierarchy_ReturnsNull()
        {
            var root = new GameObject("Root");
            try
            {
                var result = (GameObject)findDescendantMethod.Invoke(converter, new object[] { root, "Any" });
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }

    [TestFixture]
    internal class AvatarConverterRemoveExtraMaterialSlotsTests
    {
        private AvatarConverter converter;
        private MethodInfo removeExtraMaterialSlotsMethod;

        [SetUp]
        public void SetUp()
        {
            converter = new AvatarConverter(new MaterialWrapperBuilder());
            removeExtraMaterialSlotsMethod = typeof(AvatarConverter).GetMethod(
                "RemoveExtraMaterialSlots",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(removeExtraMaterialSlotsMethod, "RemoveExtraMaterialSlots method should exist");
        }

        private static Mesh CreateSimpleMesh(int subMeshCount = 1)
        {
            var mesh = new Mesh();
            var verts = new Vector3[subMeshCount * 3];
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = new Vector3(i, 0, 0);
            }
            mesh.vertices = verts;
            mesh.subMeshCount = subMeshCount;
            for (int i = 0; i < subMeshCount; i++)
            {
                mesh.SetTriangles(new int[] { i * 3, i * 3 + 1, i * 3 + 2 }, i);
            }
            return mesh;
        }

        [Test]
        public void RemoveExtraMaterialSlots_SkinnedMeshRenderer_ExtraSlotsRemoved()
        {
            var root = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = CreateSimpleMesh(1);
            smr.sharedMesh = mesh;
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            smr.sharedMaterials = new Material[] { mat1, mat2 }; // 2 materials but only 1 submesh
            try
            {
                Assert.AreEqual(2, smr.sharedMaterials.Length);
                removeExtraMaterialSlotsMethod.Invoke(converter, new object[] { root });
                Assert.AreEqual(1, smr.sharedMaterials.Length);
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(mesh);
                Object.DestroyImmediate(mat1);
                Object.DestroyImmediate(mat2);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_MeshRenderer_ExtraSlotsRemoved()
        {
            var root = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            var filter = child.AddComponent<MeshFilter>();
            var mr = child.AddComponent<MeshRenderer>();
            var mesh = CreateSimpleMesh(1);
            filter.sharedMesh = mesh;
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            var mat3 = new Material(Shader.Find("Standard"));
            mr.sharedMaterials = new Material[] { mat1, mat2, mat3 }; // 3 materials but only 1 submesh
            try
            {
                Assert.AreEqual(3, mr.sharedMaterials.Length);
                removeExtraMaterialSlotsMethod.Invoke(converter, new object[] { root });
                Assert.AreEqual(1, mr.sharedMaterials.Length);
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(mesh);
                Object.DestroyImmediate(mat1);
                Object.DestroyImmediate(mat2);
                Object.DestroyImmediate(mat3);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_MatchingSlotCount_NoChange()
        {
            var root = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = CreateSimpleMesh(2);
            smr.sharedMesh = mesh;
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            smr.sharedMaterials = new Material[] { mat1, mat2 };
            try
            {
                removeExtraMaterialSlotsMethod.Invoke(converter, new object[] { root });
                Assert.AreEqual(2, smr.sharedMaterials.Length);
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(mesh);
                Object.DestroyImmediate(mat1);
                Object.DestroyImmediate(mat2);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_NoMesh_Skipped()
        {
            var root = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            child.AddComponent<SkinnedMeshRenderer>();
            try
            {
                // Should not throw when mesh is null
                removeExtraMaterialSlotsMethod.Invoke(converter, new object[] { root });
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }

    [TestFixture]
    internal class AvatarConverterCacheTests
    {
        private AvatarConverter converter;

        [SetUp]
        public void SetUp()
        {
            converter = new AvatarConverter(new MaterialWrapperBuilder());
        }

        [Test]
        public void ClearSharedBlackTextureCache_DoesNotThrow()
        {
            var method = typeof(AvatarConverter).GetMethod(
                "ClearSharedBlackTextureCache",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "ClearSharedBlackTextureCache should exist");
            method.Invoke(converter, null);
        }

        [Test]
        public void GetOrCreateSharedBlackTexture_NoSaveAsFile_ReturnsCachedTexture()
        {
            var method = typeof(AvatarConverter).GetMethod(
                "GetOrCreateSharedBlackTexture",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "GetOrCreateSharedBlackTexture should exist");

            // First call creates the texture
            var tex1 = (Texture2D)method.Invoke(converter, new object[] { false, "" });
            Assert.IsNotNull(tex1);

            // Second call should return cached texture
            var tex2 = (Texture2D)method.Invoke(converter, new object[] { false, "" });
            Assert.AreSame(tex1, tex2);

            Object.DestroyImmediate(tex1);
        }

        [Test]
        public void GetOrCreateSharedBlackTexture_DifferentKeys_ReturnsDifferentTextures()
        {
            var method = typeof(AvatarConverter).GetMethod(
                "GetOrCreateSharedBlackTexture",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method);

            var tex1 = (Texture2D)method.Invoke(converter, new object[] { false, "pathA" });
            var tex2 = (Texture2D)method.Invoke(converter, new object[] { false, "pathB" });
            Assert.IsNotNull(tex1);
            Assert.IsNotNull(tex2);
            Assert.AreNotSame(tex1, tex2);

            Object.DestroyImmediate(tex1);
            Object.DestroyImmediate(tex2);
        }

        [Test]
        public void GetOrCreateSharedBlackTexture_AfterClear_CreatesNewTexture()
        {
            var getMethod = typeof(AvatarConverter).GetMethod(
                "GetOrCreateSharedBlackTexture",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var clearMethod = typeof(AvatarConverter).GetMethod(
                "ClearSharedBlackTextureCache",
                BindingFlags.Instance | BindingFlags.NonPublic);

            var tex1 = (Texture2D)getMethod.Invoke(converter, new object[] { false, "" });
            clearMethod.Invoke(converter, null);
            var tex2 = (Texture2D)getMethod.Invoke(converter, new object[] { false, "" });

            Assert.IsNotNull(tex1);
            Assert.IsNotNull(tex2);
            Assert.AreNotSame(tex1, tex2);

            Object.DestroyImmediate(tex1);
            Object.DestroyImmediate(tex2);
        }
    }

    [TestFixture]
    internal class AvatarConverterApplyVirtualLens2SupportTests
    {
        private AvatarConverter converter;
        private MethodInfo applyVirtualLens2Method;

        [SetUp]
        public void SetUp()
        {
            converter = new AvatarConverter(new MaterialWrapperBuilder());
            applyVirtualLens2Method = typeof(AvatarConverter).GetMethod(
                "ApplyVirtualLens2Support",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(applyVirtualLens2Method, "ApplyVirtualLens2Support should exist");
        }

        [Test]
        public void ApplyVirtualLens2Support_WithVirtualLensRoot_SetsEditorOnly()
        {
            var avatar = new GameObject("Avatar");
            var vlRoot = new GameObject("_VirtualLens_Root");
            vlRoot.transform.SetParent(avatar.transform);
            try
            {
                applyVirtualLens2Method.Invoke(converter, new object[] { avatar });
                Assert.AreEqual("EditorOnly", vlRoot.tag);
                Assert.IsFalse(vlRoot.activeSelf);
            }
            finally
            {
                Object.DestroyImmediate(avatar);
            }
        }

        [Test]
        public void ApplyVirtualLens2Support_WithVirtualLensRootAndOrigin_BothSetEditorOnly()
        {
            var avatar = new GameObject("Avatar");
            var vlRoot = new GameObject("_VirtualLens_Root");
            vlRoot.transform.SetParent(avatar.transform);
            var origin = new GameObject("VirtualLensOrigin");
            origin.transform.SetParent(avatar.transform);
            try
            {
                applyVirtualLens2Method.Invoke(converter, new object[] { avatar });
                Assert.AreEqual("EditorOnly", vlRoot.tag);
                Assert.IsFalse(vlRoot.activeSelf);
                Assert.AreEqual("EditorOnly", origin.tag);
                Assert.IsFalse(origin.activeSelf);
            }
            finally
            {
                Object.DestroyImmediate(avatar);
            }
        }

        [Test]
        public void ApplyVirtualLens2Support_NoVirtualLensRoot_NoChange()
        {
            var avatar = new GameObject("Avatar");
            var child = new GameObject("SomeChild");
            child.transform.SetParent(avatar.transform);
            try
            {
                applyVirtualLens2Method.Invoke(converter, new object[] { avatar });
                Assert.AreEqual("Untagged", child.tag);
                Assert.IsTrue(child.activeSelf);
            }
            finally
            {
                Object.DestroyImmediate(avatar);
            }
        }

        [Test]
        public void ApplyVirtualLens2Support_VirtualLensRootDeep_SetsEditorOnly()
        {
            var avatar = new GameObject("Avatar");
            var level1 = new GameObject("Body");
            level1.transform.SetParent(avatar.transform);
            var vlRoot = new GameObject("_VirtualLens_Root");
            vlRoot.transform.SetParent(level1.transform);
            try
            {
                applyVirtualLens2Method.Invoke(converter, new object[] { avatar });
                Assert.AreEqual("EditorOnly", vlRoot.tag);
                Assert.IsFalse(vlRoot.activeSelf);
            }
            finally
            {
                Object.DestroyImmediate(avatar);
            }
        }
    }
}
