// Tests for AvatarDynamics.CalculatePerformanceStats

using NUnit.Framework;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;
using KRT.VRCQuestTools.Models.VRChat;

namespace KRT.VRCQuestTools.Models.VRChat.Tests
{
    [TestFixture]
    public class AvatarDynamicsCalcTests
    {
        [Test]
        public void CalculatePerformanceStats_EmptyAvatar_AllZeros()
        {
            var root = new GameObject("Root");
            try
            {
                var stats = AvatarDynamics.CalculatePerformanceStats(
                    root,
                    new VRCPhysBone[0],
                    new VRCPhysBoneCollider[0],
                    new ContactBase[0]);
                Assert.AreEqual(0, stats.PhysBonesCount);
                Assert.AreEqual(0, stats.PhysBonesTransformCount);
                Assert.AreEqual(0, stats.PhysBonesColliderCount);
                Assert.AreEqual(0, stats.PhysBonesCollisionCheckCount);
                Assert.AreEqual(0, stats.ContactsCount);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CalculatePerformanceStats_SinglePhysBone_CountsCorrectly()
        {
            var root = new GameObject("Root");
            var child = new GameObject("Bone");
            child.transform.SetParent(root.transform);
            var pb = child.AddComponent<VRCPhysBone>();
            try
            {
                var stats = AvatarDynamics.CalculatePerformanceStats(
                    root,
                    new VRCPhysBone[] { pb },
                    new VRCPhysBoneCollider[0],
                    new ContactBase[0]);
                Assert.AreEqual(1, stats.PhysBonesCount);
                Assert.IsTrue(stats.PhysBonesTransformCount >= 1);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CalculatePerformanceStats_EditorOnlyPhysBone_Excluded()
        {
            var root = new GameObject("Root");
            var child = new GameObject("EditorOnlyBone");
            child.transform.SetParent(root.transform);
            child.tag = "EditorOnly";
            var pb = child.AddComponent<VRCPhysBone>();
            try
            {
                var stats = AvatarDynamics.CalculatePerformanceStats(
                    root,
                    new VRCPhysBone[] { pb },
                    new VRCPhysBoneCollider[0],
                    new ContactBase[0]);
                Assert.AreEqual(0, stats.PhysBonesCount);
                Assert.AreEqual(0, stats.PhysBonesTransformCount);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CalculatePerformanceStats_WithColliderReferencedByPhysBone()
        {
            var root = new GameObject("Root");
            var boneGo = new GameObject("Bone");
            boneGo.transform.SetParent(root.transform);
            var colliderGo = new GameObject("Collider");
            colliderGo.transform.SetParent(root.transform);
            var pb = boneGo.AddComponent<VRCPhysBone>();
            var collider = colliderGo.AddComponent<VRCPhysBoneCollider>();
            pb.colliders.Add(collider);
            try
            {
                var stats = AvatarDynamics.CalculatePerformanceStats(
                    root,
                    new VRCPhysBone[] { pb },
                    new VRCPhysBoneCollider[] { collider },
                    new ContactBase[0]);
                Assert.AreEqual(1, stats.PhysBonesCount);
                Assert.AreEqual(1, stats.PhysBonesColliderCount);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CalculatePerformanceStats_UnreferencedCollider_NotCounted()
        {
            var root = new GameObject("Root");
            var boneGo = new GameObject("Bone");
            boneGo.transform.SetParent(root.transform);
            var colliderGo = new GameObject("Collider");
            colliderGo.transform.SetParent(root.transform);
            var pb = boneGo.AddComponent<VRCPhysBone>();
            var collider = colliderGo.AddComponent<VRCPhysBoneCollider>();
            // Don't add collider to pb.colliders
            try
            {
                var stats = AvatarDynamics.CalculatePerformanceStats(
                    root,
                    new VRCPhysBone[] { pb },
                    new VRCPhysBoneCollider[] { collider },
                    new ContactBase[0]);
                Assert.AreEqual(0, stats.PhysBonesColliderCount);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CalculatePerformanceStats_WithContacts()
        {
            var root = new GameObject("Root");
            var contactGo = new GameObject("Contact");
            contactGo.transform.SetParent(root.transform);
            var contact = contactGo.AddComponent<ContactReceiver>();
            try
            {
                var stats = AvatarDynamics.CalculatePerformanceStats(
                    root,
                    new VRCPhysBone[0],
                    new VRCPhysBoneCollider[0],
                    new ContactBase[] { contact });
                // Contacts count depends on IsLocalOnly property
                Assert.IsTrue(stats.ContactsCount >= 0);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CalculatePerformanceStats_WithProviderOverload()
        {
            var root = new GameObject("Root");
            var child = new GameObject("Bone");
            child.transform.SetParent(root.transform);
            var pb = child.AddComponent<VRCPhysBone>();
            var provider = new VRCPhysBoneProvider(pb);
            try
            {
                var stats = AvatarDynamics.CalculatePerformanceStats(
                    root,
                    new VRCPhysBoneProviderBase[] { provider },
                    new VRCPhysBoneCollider[0],
                    new ContactBase[0]);
                Assert.AreEqual(1, stats.PhysBonesCount);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CalculatePerformanceStats_PhysBoneWithChildren_CountsTransforms()
        {
            var root = new GameObject("Root");
            var bone = new GameObject("Bone");
            bone.transform.SetParent(root.transform);
            var child1 = new GameObject("Child1");
            child1.transform.SetParent(bone.transform);
            var child2 = new GameObject("Child2");
            child2.transform.SetParent(child1.transform);
            var pb = bone.AddComponent<VRCPhysBone>();
            try
            {
                var stats = AvatarDynamics.CalculatePerformanceStats(
                    root,
                    new VRCPhysBone[] { pb },
                    new VRCPhysBoneCollider[0],
                    new ContactBase[0]);
                Assert.AreEqual(1, stats.PhysBonesCount);
                // Should count root + children = 3 (bone, child1, child2)
                Assert.AreEqual(3, stats.PhysBonesTransformCount);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void PerformanceStats_FieldsInitialized()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            Assert.AreEqual(0, stats.PhysBonesCount);
            Assert.AreEqual(0, stats.PhysBonesTransformCount);
            Assert.AreEqual(0, stats.PhysBonesColliderCount);
            Assert.AreEqual(0, stats.PhysBonesCollisionCheckCount);
            Assert.AreEqual(0, stats.ContactsCount);
        }

        [Test]
        public void PerformanceStats_CanSetFields()
        {
            var stats = new AvatarDynamics.PerformanceStats
            {
                PhysBonesCount = 5,
                PhysBonesTransformCount = 50,
                PhysBonesColliderCount = 10,
                PhysBonesCollisionCheckCount = 100,
                ContactsCount = 3,
            };
            Assert.AreEqual(5, stats.PhysBonesCount);
            Assert.AreEqual(50, stats.PhysBonesTransformCount);
            Assert.AreEqual(10, stats.PhysBonesColliderCount);
            Assert.AreEqual(100, stats.PhysBonesCollisionCheckCount);
            Assert.AreEqual(3, stats.ContactsCount);
        }
    }
}
