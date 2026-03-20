// <copyright file="VRCSDKUtilityExtendedTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;
using VRC.Core;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase.Validation.Performance;
using VRC.SDKBase.Validation.Performance.Stats;
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;

namespace KRT.VRCQuestTools.Tests.Utils
{
    [TestFixture]
    internal class VRCSDKUtilityExtendedTests
    {
        // ---- LoadAvatarPerformanceStatsLevelSet ----

        [Test]
        public void LoadAvatarPerformanceStatsLevelSet_Mobile_ReturnsNonNull()
        {
            var set = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(true);
            Assert.IsNotNull(set);
        }

        [Test]
        public void LoadAvatarPerformanceStatsLevelSet_PC_ReturnsNonNull()
        {
            var set = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(false);
            Assert.IsNotNull(set);
        }

        [Test]
        public void LoadAvatarPerformanceStatsLevelSet_Mobile_HasExcellentLevel()
        {
            var set = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(true);
            Assert.IsNotNull(set.excellent);
        }

        [Test]
        public void LoadAvatarPerformanceStatsLevelSet_Mobile_HasGoodLevel()
        {
            var set = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(true);
            Assert.IsNotNull(set.good);
        }

        [Test]
        public void LoadAvatarPerformanceStatsLevelSet_Mobile_HasMediumLevel()
        {
            var set = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(true);
            Assert.IsNotNull(set.medium);
        }

        [Test]
        public void LoadAvatarPerformanceStatsLevelSet_Mobile_HasPoorLevel()
        {
            var set = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(true);
            Assert.IsNotNull(set.poor);
        }

        // ---- LoadPerformanceIcon ----

        [Test]
        public void LoadPerformanceIcon_Excellent_ReturnsTexture()
        {
            var icon = VRCSDKUtility.LoadPerformanceIcon(PerformanceRating.Excellent);
            Assert.IsNotNull(icon);
        }

        [Test]
        public void LoadPerformanceIcon_Good_ReturnsTexture()
        {
            var icon = VRCSDKUtility.LoadPerformanceIcon(PerformanceRating.Good);
            Assert.IsNotNull(icon);
        }

        [Test]
        public void LoadPerformanceIcon_Medium_ReturnsTexture()
        {
            var icon = VRCSDKUtility.LoadPerformanceIcon(PerformanceRating.Medium);
            Assert.IsNotNull(icon);
        }

        [Test]
        public void LoadPerformanceIcon_Poor_ReturnsTexture()
        {
            var icon = VRCSDKUtility.LoadPerformanceIcon(PerformanceRating.Poor);
            Assert.IsNotNull(icon);
        }

        [Test]
        public void LoadPerformanceIcon_VeryPoor_ReturnsTexture()
        {
            var icon = VRCSDKUtility.LoadPerformanceIcon(PerformanceRating.VeryPoor);
            Assert.IsNotNull(icon);
        }

        [Test]
        public void LoadPerformanceIcon_None_ThrowsInvalidOperation()
        {
            Assert.Throws<System.InvalidOperationException>(() =>
            {
                VRCSDKUtility.LoadPerformanceIcon(PerformanceRating.None);
            });
        }

        // ---- GetContentTagLabel ----

        [Test]
        public void GetContentTagLabel_Sex_ReturnsNudity()
        {
            var label = VRCSDKUtility.GetContentTagLabel("content_sex");
            Assert.AreEqual("Nudity/Sexuality", label);
        }

        [Test]
        public void GetContentTagLabel_Violence_ReturnsRealisticViolence()
        {
            var label = VRCSDKUtility.GetContentTagLabel("content_violence");
            Assert.AreEqual("Realistic Violence", label);
        }

        [Test]
        public void GetContentTagLabel_Gore_ReturnsBloodGore()
        {
            var label = VRCSDKUtility.GetContentTagLabel("content_gore");
            Assert.AreEqual("Blood/Gore", label);
        }

        [Test]
        public void GetContentTagLabel_Other_ReturnsOtherNSFW()
        {
            var label = VRCSDKUtility.GetContentTagLabel("content_other");
            Assert.AreEqual("Other NSFW", label);
        }

        [Test]
        public void GetContentTagLabel_Fallback_ReturnsFallback()
        {
            var label = VRCSDKUtility.GetContentTagLabel("author_quest_fallback");
            Assert.AreEqual("Fallback", label);
        }

        [Test]
        public void GetContentTagLabel_Unknown_ReturnsTag()
        {
            var label = VRCSDKUtility.GetContentTagLabel("custom_tag");
            Assert.AreEqual("custom_tag", label);
        }

        // ---- StripeUnusedNetworkIds ----

        [Test]
        public void StripeUnusedNetworkIds_EmptyCollection_NoError()
        {
            var go = new GameObject("Avatar");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                VRCSDKUtility.StripeUnusedNetworkIds(descriptor);
                Assert.AreEqual(0, descriptor.NetworkIDCollection.Count);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void StripeUnusedNetworkIds_RemovesOrphanedIds()
        {
            var go = new GameObject("Avatar");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                // Add an orphaned network ID (gameObject exists but no PhysBone)
                var child = new GameObject("Child");
                child.transform.parent = go.transform;
                descriptor.NetworkIDCollection.Add(new VRC.SDKBase.Network.NetworkIDPair
                {
                    gameObject = child,
                    ID = 10,
                });
                Assert.AreEqual(1, descriptor.NetworkIDCollection.Count);

                VRCSDKUtility.StripeUnusedNetworkIds(descriptor);
                Assert.AreEqual(0, descriptor.NetworkIDCollection.Count);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void StripeUnusedNetworkIds_KeepsPhysBoneIds()
        {
            var go = new GameObject("Avatar");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                var child = new GameObject("PBChild");
                child.transform.parent = go.transform;
                child.AddComponent<VRCPhysBone>();
                descriptor.NetworkIDCollection.Add(new VRC.SDKBase.Network.NetworkIDPair
                {
                    gameObject = child,
                    ID = 10,
                });
                Assert.AreEqual(1, descriptor.NetworkIDCollection.Count);

                VRCSDKUtility.StripeUnusedNetworkIds(descriptor);
                Assert.AreEqual(1, descriptor.NetworkIDCollection.Count);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void StripeUnusedNetworkIds_RemovesNullGameObject()
        {
            var go = new GameObject("Avatar");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                descriptor.NetworkIDCollection.Add(new VRC.SDKBase.Network.NetworkIDPair
                {
                    gameObject = null,
                    ID = 10,
                });
                Assert.AreEqual(1, descriptor.NetworkIDCollection.Count);

                VRCSDKUtility.StripeUnusedNetworkIds(descriptor);
                Assert.AreEqual(0, descriptor.NetworkIDCollection.Count);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // ---- AssignNetworkIdsToPhysBones ----

        [Test]
        public void AssignNetworkIdsToPhysBones_NoPhysBones_NoIds()
        {
            var go = new GameObject("Avatar");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                VRCSDKUtility.AssignNetworkIdsToPhysBones(descriptor);
                Assert.AreEqual(0, descriptor.NetworkIDCollection.Count);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void AssignNetworkIdsToPhysBones_SinglePhysBone_AssignsId()
        {
            var go = new GameObject("Avatar");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                var child = new GameObject("PBone");
                child.transform.parent = go.transform;
                child.AddComponent<VRCPhysBone>();

                VRCSDKUtility.AssignNetworkIdsToPhysBones(descriptor);
                Assert.AreEqual(1, descriptor.NetworkIDCollection.Count);
                Assert.AreEqual(child, descriptor.NetworkIDCollection[0].gameObject);
                Assert.IsTrue(descriptor.NetworkIDCollection[0].ID >= 10);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void AssignNetworkIdsToPhysBones_MultiplePhysBones_AssignsUniqueIds()
        {
            var go = new GameObject("Avatar");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    var child = new GameObject($"PBone_{i}");
                    child.transform.parent = go.transform;
                    child.AddComponent<VRCPhysBone>();
                }

                VRCSDKUtility.AssignNetworkIdsToPhysBones(descriptor);
                Assert.AreEqual(5, descriptor.NetworkIDCollection.Count);

                var ids = descriptor.NetworkIDCollection.Select(p => p.ID).ToArray();
                Assert.AreEqual(ids.Distinct().Count(), ids.Length, "All IDs should be unique");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void AssignNetworkIdsToPhysBones_AlreadyAssigned_SkipsDuplicate()
        {
            var go = new GameObject("Avatar");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                var child = new GameObject("PBone");
                child.transform.parent = go.transform;
                child.AddComponent<VRCPhysBone>();
                descriptor.NetworkIDCollection.Add(new VRC.SDKBase.Network.NetworkIDPair
                {
                    gameObject = child,
                    ID = 10,
                });

                VRCSDKUtility.AssignNetworkIdsToPhysBones(descriptor);
                // Should not duplicate the ID
                Assert.AreEqual(1, descriptor.NetworkIDCollection.Count);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // ---- AssignNetworkIdsToPhysBonesByHierarchyHash ----

        [Test]
        public void AssignNetworkIdsToPhysBonesByHierarchyHash_NoPhysBones_EmptyResult()
        {
            var go = new GameObject("Avatar");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                var (allIDs, newIDs) = VRCSDKUtility.AssignNetworkIdsToPhysBonesByHierarchyHash(descriptor);
                Assert.AreEqual(0, allIDs.Count());
                Assert.AreEqual(0, newIDs.Count());
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void AssignNetworkIdsToPhysBonesByHierarchyHash_SinglePhysBone_AssignsId()
        {
            var go = new GameObject("Avatar");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                var child = new GameObject("Hair");
                child.transform.parent = go.transform;
                child.AddComponent<VRCPhysBone>();

                var (allIDs, newIDs) = VRCSDKUtility.AssignNetworkIdsToPhysBonesByHierarchyHash(descriptor);
                Assert.AreEqual(1, allIDs.Count());
                Assert.AreEqual(1, newIDs.Count());
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void AssignNetworkIdsToPhysBonesByHierarchyHash_DeterministicIds()
        {
            // Two avatars with same hierarchy should get same IDs
            var go1 = new GameObject("Avatar1");
            var desc1 = go1.AddComponent<VRCAvatarDescriptor>();
            var go2 = new GameObject("Avatar2");
            var desc2 = go2.AddComponent<VRCAvatarDescriptor>();
            try
            {
                var child1 = new GameObject("Hair");
                child1.transform.parent = go1.transform;
                child1.AddComponent<VRCPhysBone>();

                var child2 = new GameObject("Hair");
                child2.transform.parent = go2.transform;
                child2.AddComponent<VRCPhysBone>();

                var (allIDs1, _) = VRCSDKUtility.AssignNetworkIdsToPhysBonesByHierarchyHash(desc1);
                var (allIDs2, _) = VRCSDKUtility.AssignNetworkIdsToPhysBonesByHierarchyHash(desc2);

                var id1 = allIDs1.First().ID;
                var id2 = allIDs2.First().ID;
                Assert.AreEqual(id1, id2, "Same hierarchy path should produce same hash-based ID");
            }
            finally
            {
                Object.DestroyImmediate(go1);
                Object.DestroyImmediate(go2);
            }
        }

        // ---- IsExampleAsset ----

        [Test]
        public void IsExampleAsset_VpmSdk3DemoPath_ReturnsTrue()
        {
            var path = "Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/SomeFile.fbx";
            Assert.IsTrue(VRCSDKUtility.IsExampleAsset(path));
        }

        [Test]
        public void IsExampleAsset_VpmSdk3DemoSubfolder_ReturnsTrue()
        {
            var path = "Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/Animation/ProxyAnim/proxy_stand_still.anim";
            Assert.IsTrue(VRCSDKUtility.IsExampleAsset(path));
        }

        [Test]
        public void IsExampleAsset_VpmBetaSdk3DemoPath_ReturnsTrue()
        {
            var path = "Assets/Samples/VRChat SDK - Avatars/3.5.0/AV3 Demo Assets/SomeFile.controller";
            Assert.IsTrue(VRCSDKUtility.IsExampleAsset(path));
        }

        [Test]
        public void IsExampleAsset_LegacySdk3Path_ReturnsTrue()
        {
            var path = "Assets/VRCSDK/Examples3/SomeFile.mat";
            Assert.IsTrue(VRCSDKUtility.IsExampleAsset(path));
        }

        [Test]
        public void IsExampleAsset_UserAssetPath_ReturnsFalse()
        {
            var path = "Assets/MyAvatar/Materials/Body.mat";
            Assert.IsFalse(VRCSDKUtility.IsExampleAsset(path));
        }

        [Test]
        public void IsExampleAsset_EmptyPath_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsExampleAsset(string.Empty));
        }

        [Test]
        public void IsExampleAsset_PackagesPathNotDemo_ReturnsFalse()
        {
            var path = "Packages/com.vrchat.avatars/Runtime/SomeScript.cs";
            Assert.IsFalse(VRCSDKUtility.IsExampleAsset(path));
        }

        // ---- GetRootTransform ----

        [Test]
        public void GetRootTransform_PhysBone_ReturnsPhysBoneRoot()
        {
            var go = new GameObject("TestPB");
            var pb = go.AddComponent<VRCPhysBone>();
            var rootGo = new GameObject("Root");
            pb.rootTransform = rootGo.transform;
            try
            {
                var result = VRCSDKUtility.GetRootTransform(pb);
                Assert.That(result, Is.EqualTo(rootGo.transform));
            }
            finally
            {
                Object.DestroyImmediate(rootGo);
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRootTransform_PhysBoneCollider_ReturnsColliderRoot()
        {
            var go = new GameObject("TestCollider");
            var collider = go.AddComponent<VRCPhysBoneCollider>();
            var rootGo = new GameObject("Root");
            collider.rootTransform = rootGo.transform;
            try
            {
                var result = VRCSDKUtility.GetRootTransform(collider);
                Assert.That(result, Is.EqualTo(rootGo.transform));
            }
            finally
            {
                Object.DestroyImmediate(rootGo);
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRootTransform_ContactSender_ReturnsContactRoot()
        {
            var go = new GameObject("TestContact");
            var contact = go.AddComponent<VRC.SDK3.Dynamics.Contact.Components.VRCContactSender>();
            var rootGo = new GameObject("Root");
            contact.rootTransform = rootGo.transform;
            try
            {
                var result = VRCSDKUtility.GetRootTransform(contact);
                Assert.That(result, Is.EqualTo(rootGo.transform));
            }
            finally
            {
                Object.DestroyImmediate(rootGo);
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRootTransform_OtherComponent_ReturnsNull()
        {
            var go = new GameObject("TestOther");
            var comp = go.AddComponent<BoxCollider>();
            try
            {
                var result = VRCSDKUtility.GetRootTransform(comp);
                Assert.That(result, Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // ---- AvatarDynamicsPerformanceCategories ----

        [Test]
        public void AvatarDynamicsPerformanceCategories_HasFiveCategories()
        {
            Assert.AreEqual(5, VRCSDKUtility.AvatarDynamicsPerformanceCategories.Length);
        }

        [Test]
        public void AvatarDynamicsPerformanceCategories_ContainsPhysBoneComponent()
        {
            Assert.Contains(AvatarPerformanceCategory.PhysBoneComponentCount, VRCSDKUtility.AvatarDynamicsPerformanceCategories);
        }

        [Test]
        public void AvatarDynamicsPerformanceCategories_ContainsContactCount()
        {
            Assert.Contains(AvatarPerformanceCategory.ContactCount, VRCSDKUtility.AvatarDynamicsPerformanceCategories);
        }

        // ---- Constants ----

        [Test]
        public void PoorPhysBonesCountLimit_Is8()
        {
            Assert.AreEqual(8, VRCSDKUtility.PoorPhysBonesCountLimit);
        }

        [Test]
        public void PoorPhysBoneCollidersCountLimit_Is16()
        {
            Assert.AreEqual(16, VRCSDKUtility.PoorPhysBoneCollidersCountLimit);
        }

        [Test]
        public void PoorContactsCountLimit_Is16()
        {
            Assert.AreEqual(16, VRCSDKUtility.PoorContactsCountLimit);
        }

        // ---- CalculatePerformanceStats ----

        [Test]
        public void CalculatePerformanceStats_EmptyAvatar_ReturnsStats()
        {
            var go = new GameObject("Avatar");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                var stats = VRCSDKUtility.CalculatePerformanceStats(go, true);
                Assert.IsNotNull(stats);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
