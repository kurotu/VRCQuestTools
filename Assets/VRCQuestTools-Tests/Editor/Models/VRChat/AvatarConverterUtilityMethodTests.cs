// Tests for AvatarConverter private utility methods:
// FindDescendant, GetOrCreateSharedBlackTexture, ClearSharedBlackTextureCache.

using System.Reflection;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    public class AvatarConverterUtilityMethodTests
    {
        private AvatarConverter converter;

        [SetUp]
        public void SetUp()
        {
            converter = new AvatarConverter(new MaterialWrapperBuilder());
        }

        // --- FindDescendant ---

        [Test]
        public void FindDescendant_DirectChild_ReturnsChild()
        {
            var parent = new GameObject("Parent");
            var child = new GameObject("Child");
            child.transform.SetParent(parent.transform);
            try
            {
                var result = InvokeFindDescendant(parent, "Child");
                Assert.That(result, Is.EqualTo(child));
            }
            finally
            {
                Object.DestroyImmediate(parent);
            }
        }

        [Test]
        public void FindDescendant_NestedChild_ReturnsChild()
        {
            var root = new GameObject("Root");
            var mid = new GameObject("Mid");
            var deep = new GameObject("Deep");
            mid.transform.SetParent(root.transform);
            deep.transform.SetParent(mid.transform);
            try
            {
                var result = InvokeFindDescendant(root, "Deep");
                Assert.That(result, Is.EqualTo(deep));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void FindDescendant_NotFound_ReturnsNull()
        {
            var parent = new GameObject("Parent");
            var child = new GameObject("Child");
            child.transform.SetParent(parent.transform);
            try
            {
                var result = InvokeFindDescendant(parent, "NonExistent");
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(parent);
            }
        }

        [Test]
        public void FindDescendant_NoChildren_ReturnsNull()
        {
            var parent = new GameObject("Parent");
            try
            {
                var result = InvokeFindDescendant(parent, "Child");
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(parent);
            }
        }

        [Test]
        public void FindDescendant_MultipleChildren_FindsCorrectOne()
        {
            var parent = new GameObject("Parent");
            var child1 = new GameObject("Alpha");
            var child2 = new GameObject("Beta");
            var child3 = new GameObject("Gamma");
            child1.transform.SetParent(parent.transform);
            child2.transform.SetParent(parent.transform);
            child3.transform.SetParent(parent.transform);
            try
            {
                var result = InvokeFindDescendant(parent, "Beta");
                Assert.That(result, Is.EqualTo(child2));
            }
            finally
            {
                Object.DestroyImmediate(parent);
            }
        }

        [Test]
        public void FindDescendant_DeepNesting_FindsAtThirdLevel()
        {
            var root = new GameObject("Root");
            var l1 = new GameObject("L1");
            var l2 = new GameObject("L2");
            var target = new GameObject("Target");
            l1.transform.SetParent(root.transform);
            l2.transform.SetParent(l1.transform);
            target.transform.SetParent(l2.transform);
            try
            {
                var result = InvokeFindDescendant(root, "Target");
                Assert.That(result, Is.EqualTo(target));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        // --- GetOrCreateSharedBlackTexture ---

        [Test]
        public void GetOrCreateSharedBlackTexture_InMemory_ReturnsBlackTexture()
        {
            Texture2D tex = null;
            try
            {
                tex = InvokeGetOrCreateSharedBlackTexture(false, "");
                Assert.IsNotNull(tex);
                Assert.That(tex.name, Is.EqualTo("VQT_Shared_Black"));
                Assert.That(tex.width, Is.EqualTo(4));
                Assert.That(tex.height, Is.EqualTo(4));
            }
            finally
            {
                InvokeClearSharedBlackTextureCache();
                if (tex != null)
                {
                    Object.DestroyImmediate(tex);
                }
            }
        }

        [Test]
        public void GetOrCreateSharedBlackTexture_CalledTwice_ReturnsSameInstance()
        {
            Texture2D tex1 = null;
            Texture2D tex2 = null;
            try
            {
                tex1 = InvokeGetOrCreateSharedBlackTexture(false, "");
                tex2 = InvokeGetOrCreateSharedBlackTexture(false, "");
                Assert.That(tex1, Is.SameAs(tex2), "Should return cached instance");
            }
            finally
            {
                InvokeClearSharedBlackTextureCache();
                if (tex1 != null)
                {
                    Object.DestroyImmediate(tex1);
                }
            }
        }

        [Test]
        public void ClearSharedBlackTextureCache_AfterClear_CreatesNewInstance()
        {
            Texture2D tex1 = null;
            Texture2D tex2 = null;
            try
            {
                tex1 = InvokeGetOrCreateSharedBlackTexture(false, "");
                InvokeClearSharedBlackTextureCache();
                tex2 = InvokeGetOrCreateSharedBlackTexture(false, "");
                Assert.That(tex1, Is.Not.SameAs(tex2), "Should create new instance after cache clear");
            }
            finally
            {
                InvokeClearSharedBlackTextureCache();
                if (tex1 != null)
                {
                    Object.DestroyImmediate(tex1);
                }
                if (tex2 != null)
                {
                    Object.DestroyImmediate(tex2);
                }
            }
        }

        private GameObject InvokeFindDescendant(GameObject gameObject, string name)
        {
            var method = typeof(AvatarConverter).GetMethod(
                "FindDescendant",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(GameObject), typeof(string) },
                null);
            return (GameObject)method.Invoke(converter, new object[] { gameObject, name });
        }

        private Texture2D InvokeGetOrCreateSharedBlackTexture(bool saveAsFile, string texturesPath)
        {
            var method = typeof(AvatarConverter).GetMethod(
                "GetOrCreateSharedBlackTexture",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(bool), typeof(string) },
                null);
            return (Texture2D)method.Invoke(converter, new object[] { saveAsFile, texturesPath });
        }

        private void InvokeClearSharedBlackTextureCache()
        {
            var method = typeof(AvatarConverter).GetMethod(
                "ClearSharedBlackTextureCache",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                System.Type.EmptyTypes,
                null);
            method.Invoke(converter, null);
        }
    }
}
