// Tests for VRChatAvatar additional methods
using KRT.VRCQuestTools.Models.VRChat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Animations;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDK3.Dynamics.Contact.Components;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    internal class VRChatAvatarExtendedTests
    {
        private GameObject CreateAvatarRoot()
        {
            var go = new GameObject("AvatarRoot");
            go.AddComponent<VRCAvatarDescriptor>();
            return go;
        }

        private VRChatAvatar CreateAvatar(GameObject go)
        {
            return new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
        }

        [Test]
        public void GetPhysBones_EmptyAvatar_ReturnsEmpty()
        {
            var go = CreateAvatarRoot();
            try
            {
                var avatar = CreateAvatar(go);
                var result = avatar.GetPhysBones();
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetPhysBones_WithPhysBones_ReturnsThem()
        {
            var go = CreateAvatarRoot();
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            child.AddComponent<VRCPhysBone>();
            try
            {
                var avatar = CreateAvatar(go);
                var result = avatar.GetPhysBones();
                Assert.AreEqual(1, result.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetPhysBoneColliders_EmptyAvatar_ReturnsEmpty()
        {
            var go = CreateAvatarRoot();
            try
            {
                var avatar = CreateAvatar(go);
                var result = avatar.GetPhysBoneColliders();
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetPhysBoneColliders_WithColliders_ReturnsThem()
        {
            var go = CreateAvatarRoot();
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            child.AddComponent<VRCPhysBoneCollider>();
            try
            {
                var avatar = CreateAvatar(go);
                var result = avatar.GetPhysBoneColliders();
                Assert.AreEqual(1, result.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetContacts_EmptyAvatar_ReturnsEmpty()
        {
            var go = CreateAvatarRoot();
            try
            {
                var avatar = CreateAvatar(go);
                var result = avatar.GetContacts();
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetContacts_WithContacts_ReturnsThem()
        {
            var go = CreateAvatarRoot();
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            child.AddComponent<VRCContactReceiver>();
            try
            {
                var avatar = CreateAvatar(go);
                var result = avatar.GetContacts();
                Assert.IsTrue(result.Length >= 1);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetNonLocalContacts_EmptyAvatar_ReturnsEmpty()
        {
            var go = CreateAvatarRoot();
            try
            {
                var avatar = CreateAvatar(go);
                var result = avatar.GetNonLocalContacts();
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetLocalContactReceivers_EmptyAvatar_ReturnsEmpty()
        {
            var go = CreateAvatarRoot();
            try
            {
                var avatar = CreateAvatar(go);
                var result = avatar.GetLocalContactReceivers();
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetLocalContactSenders_EmptyAvatar_ReturnsEmpty()
        {
            var go = CreateAvatarRoot();
            try
            {
                var avatar = CreateAvatar(go);
                var result = avatar.GetLocalContactSenders();
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasUnityConstraints_NoConstraints_ReturnsFalse()
        {
            var go = CreateAvatarRoot();
            try
            {
                var avatar = CreateAvatar(go);
                Assert.IsFalse(avatar.HasUnityConstraints);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasUnityConstraints_WithConstraint_ReturnsTrue()
        {
            var go = CreateAvatarRoot();
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            child.AddComponent<PositionConstraint>();
            try
            {
                var avatar = CreateAvatar(go);
                Assert.IsTrue(avatar.HasUnityConstraints);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasDynamicBoneComponents_NoDynamicBone_ReturnsFalse()
        {
            var go = CreateAvatarRoot();
            try
            {
                var avatar = CreateAvatar(go);
                // DynamicBone not imported in test project
                Assert.IsFalse(avatar.HasDynamicBoneComponents);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasVertexColor_NoMeshes_ReturnsFalse()
        {
            var go = CreateAvatarRoot();
            try
            {
                var avatar = CreateAvatar(go);
                Assert.IsFalse(avatar.HasVertexColor);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRelatedMaterials_EmptyHierarchy_ThrowsDueToNullBaseAnimationLayers()
        {
            var go = new GameObject("Root");
            go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                Assert.Throws<System.ArgumentNullException>(() =>
                {
                    VRChatAvatar.GetRelatedMaterials(go);
                });
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRelatedMaterials_WithRenderer_ThrowsDueToNullBaseAnimationLayers()
        {
            var go = new GameObject("Root");
            go.AddComponent<VRCAvatarDescriptor>();
            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            var renderer = child.AddComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Standard"));
            renderer.sharedMaterial = mat;
            try
            {
                Assert.Throws<System.ArgumentNullException>(() =>
                {
                    VRChatAvatar.GetRelatedMaterials(go);
                });
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Materials_Property_ThrowsDueToNullBaseAnimationLayers()
        {
            var go = new GameObject("Root");
            go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                var avatar = CreateAvatar(go);
                Assert.Throws<System.ArgumentNullException>(() =>
                {
                    var _ = avatar.Materials;
                });
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetPhysBoneProviders_WithPhysBones_ReturnsProviders()
        {
            var go = CreateAvatarRoot();
            var child = new GameObject("Bone");
            child.transform.SetParent(go.transform);
            child.AddComponent<VRCPhysBone>();
            try
            {
                var avatar = CreateAvatar(go);
                var providers = avatar.GetPhysBoneProviders();
                Assert.IsNotNull(providers);
                Assert.IsTrue(providers.Length >= 1);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
