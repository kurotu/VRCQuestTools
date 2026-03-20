// Tests for VRCSDKUtility methods that don't require VRChat SDK pipeline.

using System;
using NUnit.Framework;
using UnityEngine;
using VRC.SDKBase.Validation.Performance;
using KRT.VRCQuestTools.Utils;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    internal class VRCSDKUtilityHierarchyTests
    {
        [Test]
        public void GetFullPathInHierarchy_SingleRoot_ReturnsSlashName()
        {
            var go = new GameObject("Root");
            try
            {
                var path = VRCSDKUtility.GetFullPathInHierarchy(go);
                Assert.That(path, Does.Contain("Root"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetFullPathInHierarchy_ChildObject_ContainsParentName()
        {
            var root = new GameObject("Avatar");
            var child = new GameObject("Body");
            child.transform.SetParent(root.transform);
            try
            {
                var path = VRCSDKUtility.GetFullPathInHierarchy(child);
                Assert.That(path, Does.Contain("Avatar"));
                Assert.That(path, Does.Contain("Body"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GetFullPathInHierarchy_DeepHierarchy_ContainsAllNames()
        {
            var root = new GameObject("Root");
            var mid = new GameObject("Mid");
            var leaf = new GameObject("Leaf");
            mid.transform.SetParent(root.transform);
            leaf.transform.SetParent(mid.transform);
            try
            {
                var path = VRCSDKUtility.GetFullPathInHierarchy(leaf);
                Assert.That(path, Does.Contain("Root"));
                Assert.That(path, Does.Contain("Mid"));
                Assert.That(path, Does.Contain("Leaf"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void IsAvatarRoot_NullObject_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsAvatarRoot(null));
        }

        [Test]
        public void IsAvatarRoot_PlainGameObject_ReturnsFalse()
        {
            var go = new GameObject("NotAnAvatar");
            try
            {
                Assert.IsFalse(VRCSDKUtility.IsAvatarRoot(go));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetAvatarRoot_NullObject_ReturnsNull()
        {
            Assert.IsNull(VRCSDKUtility.GetAvatarRoot(null));
        }

        [Test]
        public void GetAvatarRoot_PlainGameObject_ReturnsNull()
        {
            var go = new GameObject("NotAnAvatar");
            try
            {
                Assert.IsNull(VRCSDKUtility.GetAvatarRoot(go));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetAvatarRoot_ChildOfPlainHierarchy_ReturnsNull()
        {
            var root = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            try
            {
                Assert.IsNull(VRCSDKUtility.GetAvatarRoot(child));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }

    [TestFixture]
    internal class VRCSDKUtilityMissingComponentTests
    {
        [Test]
        public void CountMissingComponentsInChildren_CleanHierarchy_ReturnsZero()
        {
            var root = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            try
            {
                Assert.AreEqual(0, VRCSDKUtility.CountMissingComponentsInChildren(root, true));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CountMissingComponentsInChildren_SingleObject_ReturnsZero()
        {
            var go = new GameObject("Single");
            try
            {
                Assert.AreEqual(0, VRCSDKUtility.CountMissingComponentsInChildren(go, false));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetGameObjectsWithMissingComponents_CleanHierarchy_ReturnsEmpty()
        {
            var root = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            try
            {
                var result = VRCSDKUtility.GetGameObjectsWithMissingComponents(root, true);
                Assert.AreEqual(0, result.Length);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GetGameObjectsWithMissingComponents_SingleObject_ReturnsEmpty()
        {
            var go = new GameObject("Single");
            try
            {
                var result = VRCSDKUtility.GetGameObjectsWithMissingComponents(go, false);
                Assert.AreEqual(0, result.Length);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RemoveMissingComponentsInChildren_CleanObject_NoError()
        {
            var root = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            try
            {
                VRCSDKUtility.RemoveMissingComponentsInChildren(root, true);
                Assert.AreEqual(0, VRCSDKUtility.CountMissingComponentsInChildren(root, true));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }

    [TestFixture]
    internal class VRCSDKUtilityContentTagTests
    {
        [Test]
        public void GetContentTagLabel_Sex_ReturnsNuditySexuality()
        {
            Assert.AreEqual("Nudity/Sexuality", VRCSDKUtility.GetContentTagLabel("content_sex"));
        }

        [Test]
        public void GetContentTagLabel_Violence_ReturnsRealisticViolence()
        {
            Assert.AreEqual("Realistic Violence", VRCSDKUtility.GetContentTagLabel("content_violence"));
        }

        [Test]
        public void GetContentTagLabel_Gore_ReturnsBloodGore()
        {
            Assert.AreEqual("Blood/Gore", VRCSDKUtility.GetContentTagLabel("content_gore"));
        }

        [Test]
        public void GetContentTagLabel_Other_ReturnsOtherNSFW()
        {
            Assert.AreEqual("Other NSFW", VRCSDKUtility.GetContentTagLabel("content_other"));
        }

        [Test]
        public void GetContentTagLabel_Fallback_ReturnsFallback()
        {
            Assert.AreEqual("Fallback", VRCSDKUtility.GetContentTagLabel("author_quest_fallback"));
        }

        [Test]
        public void GetContentTagLabel_UnknownTag_ReturnsTagItself()
        {
            Assert.AreEqual("custom_tag", VRCSDKUtility.GetContentTagLabel("custom_tag"));
        }
    }

    [TestFixture]
    internal class VRCSDKUtilityPerformanceTests
    {
        [Test]
        public void IsAllowedForFallbackAvatar_Excellent_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsAllowedForFallbackAvatar(PerformanceRating.Excellent));
        }

        [Test]
        public void IsAllowedForFallbackAvatar_Good_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsAllowedForFallbackAvatar(PerformanceRating.Good));
        }

        [Test]
        public void IsAllowedForFallbackAvatar_Medium_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsAllowedForFallbackAvatar(PerformanceRating.Medium));
        }

        [Test]
        public void IsAllowedForFallbackAvatar_Poor_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsAllowedForFallbackAvatar(PerformanceRating.Poor));
        }

        [Test]
        public void IsAllowedForFallbackAvatar_VeryPoor_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsAllowedForFallbackAvatar(PerformanceRating.VeryPoor));
        }

        [Test]
        public void IsAllowedForFallbackAvatar_None_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsAllowedForFallbackAvatar(PerformanceRating.None));
        }
    }

    [TestFixture]
    internal class VRCSDKUtilityComponentTypeTests
    {
        [Test]
        public void IsUnsupportedComponentType_DynamicBone_ReturnsTrue()
        {
            var dynamicBoneType = System.Type.GetType("DynamicBone, Assembly-CSharp");
            if (dynamicBoneType == null)
            {
                Assert.Ignore("DynamicBone not available in project");
            }
            Assert.IsTrue(VRCSDKUtility.IsUnsupportedComponentType(dynamicBoneType));
        }

        [Test]
        public void IsUnsupportedComponentType_Transform_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsUnsupportedComponentType(typeof(Transform)));
        }

        [Test]
        public void IsUnsupportedComponentType_GameObject_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsUnsupportedComponentType(typeof(GameObject)));
        }

        [Test]
        public void IsUnsupportedComponentType_MeshRenderer_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsUnsupportedComponentType(typeof(MeshRenderer)));
        }

        [Test]
        public void IsUnsupportedComponentType_Cloth_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsUnsupportedComponentType(typeof(Cloth)));
        }

        [Test]
        public void IsUnsupportedComponentType_Camera_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsUnsupportedComponentType(typeof(Camera)));
        }
    }

    [TestFixture]
    internal class VRCSDKUtilityExampleAssetTests
    {
        [Test]
        public void IsExampleAsset_VpmSdk3DemoPath_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsExampleAsset("Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/SomeFile.fbx"));
        }

        [Test]
        public void IsExampleAsset_Sdk3ExamplesPath_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsExampleAsset("Assets/VRCSDK/Examples3/SomeFile.asset"));
        }

        [Test]
        public void IsExampleAsset_RegularAssetPath_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsExampleAsset("Assets/MyAvatar/Materials/Body.mat"));
        }

        [Test]
        public void IsExampleAsset_EmptyPath_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsExampleAsset(string.Empty));
        }

        [Test]
        public void IsExampleAsset_PackagePath_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsExampleAsset("Packages/com.vrchat.avatars/Runtime/Something.cs"));
        }
    }

    [TestFixture]
    internal class VRCSDKUtilityRootTransformTests
    {
        [Test]
        public void GetRootTransform_NonPhysBoneComponent_ReturnsNull()
        {
            var go = new GameObject("Root");
            var renderer = go.AddComponent<MeshRenderer>();
            try
            {
                var root = VRCSDKUtility.GetRootTransform(renderer);
                Assert.IsNull(root);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
