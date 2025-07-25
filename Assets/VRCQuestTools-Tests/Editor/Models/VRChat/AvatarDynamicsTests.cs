using System.Linq;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase.Validation.Performance;
using VRC.SDKBase.Validation.Performance.Stats;

namespace KRT.VRCQuestTools.Models.VRChat
{
    /// <summary>
    /// Tests for AvatarDynamics.
    /// </summary>
    public class AvatarDynamicsTests
    {
        /// <summary>
        /// Test AvatarDynamics performance rating of VRCSDK.
        /// </summary>
        [Test]
        public void TestVRCSDK_Basic()
        {
            var scene = OpenTestScene();
            var root = scene.GetRootGameObjects().First((obj) => obj.name == "SimplePhysBone");

            // Test AvatarPerformance.CalculatePerformanceStats
            var isMobile = true;
            var stats = new AvatarPerformanceStats(isMobile);
            AvatarPerformance.CalculatePerformanceStats(root.name, root, stats, isMobile);

            Assert.AreEqual(2, root.GetComponentsInChildren<VRCPhysBone>(true).Length);
            Assert.AreEqual(1, stats.physBone.Value.componentCount); // EditorOnly PhysBones are ignored.
            Assert.AreEqual(6, stats.physBone.Value.transformCount); // EditorOnly PhysBones are ignored. EditorOnly children are counted.

            Assert.AreEqual(2, root.GetComponentsInChildren<VRCPhysBoneCollider>(true).Length);
            Assert.AreEqual(2, stats.physBone.Value.colliderCount); // EditorOnly colliders are counted.
            Assert.AreEqual(10, stats.physBone.Value.collisionCheckCount); // EditorOnly PhysBones are ignored. EditorOnly colliders are counted.

            Assert.AreEqual(2, root.GetComponentsInChildren<ContactBase>(true).Length);
            Assert.AreEqual(2, stats.contactCount.Value); // EditorOnly contacts are counted. Children of EditorOnly are counted.

            // https://creators.vrchat.com/avatars/avatar-performance-ranking-system/#quest-limits
            Assert.AreEqual(PerformanceRating.Good, stats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysBoneComponentCount));
            Assert.AreEqual(PerformanceRating.Good, stats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysBoneTransformCount));
            Assert.AreEqual(PerformanceRating.Good, stats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysBoneColliderCount));
            Assert.AreEqual(PerformanceRating.Good, stats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysBoneCollisionCheckCount));
            Assert.AreEqual(PerformanceRating.Excellent, stats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ContactCount));

            // Test AvatarPerformanceStatsLevelSet
            AvatarPerformanceStatsLevelSet questStatsLevelSet = LoadQuestStatsLevelSet();
            Assert.AreEqual(8, questStatsLevelSet.poor.physBone.componentCount);
            Assert.AreEqual(64, questStatsLevelSet.poor.physBone.transformCount);
            Assert.AreEqual(16, questStatsLevelSet.poor.physBone.colliderCount);
            Assert.AreEqual(64, questStatsLevelSet.poor.physBone.collisionCheckCount);
        }

        /// <summary>
        /// Test components are children of EditorOnly.
        /// </summary>
        [Test]
        public void TestVRCSDK_ChildOfEditorOnly()
        {
            var scene = OpenTestScene();
            var root = scene.GetRootGameObjects().First((obj) => obj.name == "ChildOfEditorOnly");
            var stats = new AvatarPerformanceStats(true);
            AvatarPerformance.CalculatePerformanceStats(root.name, root, stats, true);

            Assert.AreEqual(1, root.GetComponentsInChildren<VRCPhysBone>(true).Length);
            Assert.AreEqual(1, root.GetComponentsInChildren<VRCPhysBoneCollider>(true).Length);
            Assert.AreEqual(0, root.GetComponentsInChildren<ContactBase>(true).Length);
            Assert.True(stats.physBone.HasValue);
            Assert.AreEqual(0, stats.physBone.Value.componentCount);
            Assert.AreEqual(0, stats.physBone.Value.colliderCount);
        }

        /// <summary>
        /// Test components are children of EditorOnly.
        /// </summary>
        [Test]
        public void TestVRCSDK_ChildOfEditorOnly_WithNewPB()
        {
            var scene = OpenTestScene();
            var root = scene.GetRootGameObjects().First((obj) => obj.name == "ChildOfEditorOnly");
            var stats = new AvatarPerformanceStats(true);

            // Append new pb object to root.
            var newPB = new GameObject("NewPB");
            newPB.AddComponent<VRCPhysBone>();
            newPB.transform.SetParent(root.transform, true);
            newPB.GetComponent<VRCPhysBone>().colliders.Add(root.GetComponentInChildren<VRCPhysBoneCollider>());
            AvatarPerformance.CalculatePerformanceStats(root.name, root, stats, true);

            Assert.AreEqual(2, root.GetComponentsInChildren<VRCPhysBone>(true).Length);
            Assert.AreEqual(1, root.GetComponentsInChildren<VRCPhysBoneCollider>(true).Length);

            Assert.AreEqual(1, stats.physBone.Value.componentCount); // children pb of EditorOnly are ignored.
            Assert.AreEqual(1, stats.physBone.Value.transformCount); // children pb of EditorOnly are ignored.
            Assert.AreEqual(1, stats.physBone.Value.colliderCount); // referenced children colliders of EditorOnly are counted
            Assert.AreEqual(0, stats.physBone.Value.collisionCheckCount); // children colliders of EditorOnly are ignored
        }

        /// <summary>
        /// Test basic case.
        /// </summary>
        [Test]
        public void TestBasic()
        {
            var scene = OpenTestScene();
            var root = scene.GetRootGameObjects().First((obj) => obj.name == "SimplePhysBone");
            var pbs = root.GetComponentsInChildren<VRCPhysBone>(true).Select(c => new VRCSDKUtility.Reflection.PhysBone(c)).ToArray();
            var colliders = root.GetComponentsInChildren<VRCPhysBoneCollider>(true).Select(c => new VRCSDKUtility.Reflection.PhysBoneCollider(c)).ToArray();
            var contacts = root.GetComponentsInChildren<ContactBase>(true).Select(c => new VRCSDKUtility.Reflection.ContactBase(c)).ToArray();
            var perfs = AvatarDynamics.CalculatePerformanceStats(root, pbs, colliders, contacts);

            var sdkStats = new AvatarPerformanceStats(true);
            AvatarPerformance.CalculatePerformanceStats(root.name, root, sdkStats, true);

            Assert.AreEqual(sdkStats.physBone.Value.componentCount, perfs.PhysBonesCount, "PhysBones count is different from SDK.");
            Assert.AreEqual(sdkStats.physBone.Value.transformCount, perfs.PhysBonesTransformCount, "PhysBones Transform count is different from SDK.");
            Assert.AreEqual(sdkStats.physBone.Value.colliderCount, perfs.PhysBonesColliderCount, "PhysBoneColliders count is different from SDK.");
            Assert.AreEqual(sdkStats.physBone.Value.collisionCheckCount, perfs.PhysBonesCollisionCheckCount, "PhysBones collision check count is different from SDK.");
            Assert.AreEqual(sdkStats.contactCount.Value, perfs.ContactsCount, "Contacts count is different from SDK.");

            // https://creators.vrchat.com/avatars/avatar-performance-ranking-system/#quest-limits
            var questStatsLevelSet = LoadQuestStatsLevelSet();
            Debug.Log(questStatsLevelSet);
            Assert.AreEqual(PerformanceRating.Good, AvatarPerformanceCalculator.GetPerformanceRating(perfs, questStatsLevelSet, AvatarPerformanceCategory.PhysBoneComponentCount));
            Assert.AreEqual(PerformanceRating.Good, AvatarPerformanceCalculator.GetPerformanceRating(perfs, questStatsLevelSet, AvatarPerformanceCategory.PhysBoneTransformCount));
            Assert.AreEqual(PerformanceRating.Good, AvatarPerformanceCalculator.GetPerformanceRating(perfs, questStatsLevelSet, AvatarPerformanceCategory.PhysBoneColliderCount));
            Assert.AreEqual(PerformanceRating.Good, AvatarPerformanceCalculator.GetPerformanceRating(perfs, questStatsLevelSet, AvatarPerformanceCategory.PhysBoneCollisionCheckCount));
            Assert.AreEqual(PerformanceRating.Excellent, AvatarPerformanceCalculator.GetPerformanceRating(perfs, questStatsLevelSet, AvatarPerformanceCategory.ContactCount));
        }

        /// <summary>
        /// Test EditorOnly case.
        /// </summary>
        [Test]
        public void TestEditorOnly()
        {
            var scene = OpenTestScene();
            var root = scene.GetRootGameObjects().First((obj) => obj.name == "ChildOfEditorOnly");
            var pbs = root.GetComponentsInChildren<VRCPhysBone>(true).Select(c => new VRCSDKUtility.Reflection.PhysBone(c)).ToArray();
            var colliders = root.GetComponentsInChildren<VRCPhysBoneCollider>(true).Select(c => new VRCSDKUtility.Reflection.PhysBoneCollider(c)).ToArray();
            var contacts = root.GetComponentsInChildren<ContactBase>(true).Select(c => new VRCSDKUtility.Reflection.ContactBase(c)).ToArray();
            var perfs = AvatarDynamics.CalculatePerformanceStats(root, pbs, colliders, contacts);

            var sdkStats = new AvatarPerformanceStats(true);
            AvatarPerformance.CalculatePerformanceStats(root.name, root, sdkStats, true);

            Assert.AreEqual(0, perfs.PhysBonesCount, "PhysBones count is wrong.");
            Assert.AreEqual(0, perfs.PhysBonesTransformCount, "PhysBones transform count is wrong.");
            Assert.AreEqual(0, perfs.PhysBonesColliderCount, "PhysBoneColliders count is wrong.");
            Assert.AreEqual(0, perfs.PhysBonesCollisionCheckCount, "PhysBone collision check count is wrong.");
            Assert.AreEqual(0, perfs.ContactsCount, "Contacts count is wrong.");
        }

        /// <summary>
        /// Test EditorOnly case.
        /// </summary>
        [Test]
        public void TestEditorOnly_WithNewPB()
        {
            var scene = OpenTestScene();
            var root = scene.GetRootGameObjects().First((obj) => obj.name == "ChildOfEditorOnly");

            // Append new pb object to root.
            var newPB = new GameObject("NewPB");
            newPB.AddComponent<VRCPhysBone>();
            newPB.transform.SetParent(root.transform, true);
            newPB.GetComponent<VRCPhysBone>().colliders.Add(root.GetComponentInChildren<VRCPhysBoneCollider>());

            var pbs = root.GetComponentsInChildren<VRCPhysBone>(true).Select(c => new VRCSDKUtility.Reflection.PhysBone(c)).ToArray();
            var colliders = root.GetComponentsInChildren<VRCPhysBoneCollider>(true).Select(c => new VRCSDKUtility.Reflection.PhysBoneCollider(c)).ToArray();
            var contacts = root.GetComponentsInChildren<ContactBase>(true).Select(c => new VRCSDKUtility.Reflection.ContactBase(c)).ToArray();
            var perfs = AvatarDynamics.CalculatePerformanceStats(root, pbs, colliders, contacts);

            var sdkStats = new AvatarPerformanceStats(true);
            AvatarPerformance.CalculatePerformanceStats(root.name, root, sdkStats, true);

            Assert.AreEqual(2, root.GetComponentsInChildren<VRCPhysBone>(true).Length);
            Assert.AreEqual(1, root.GetComponentsInChildren<VRCPhysBoneCollider>(true).Length);
            Assert.AreEqual(0, root.GetComponentsInChildren<ContactBase>(true).Length);

            Assert.AreEqual(sdkStats.physBone.Value.componentCount, perfs.PhysBonesCount, "PhysBones count is different from SDK.");
            Assert.AreEqual(sdkStats.physBone.Value.transformCount, perfs.PhysBonesTransformCount, "PhysBones Transform count is different from SDK.");
            Assert.AreEqual(sdkStats.physBone.Value.colliderCount, perfs.PhysBonesColliderCount, "PhysBoneColliders count is different from SDK.");
            Assert.AreEqual(sdkStats.physBone.Value.collisionCheckCount, perfs.PhysBonesCollisionCheckCount, "PhysBones collision check count is different from SDK.");
            Assert.AreEqual(sdkStats.contactCount.Value, perfs.ContactsCount, "Contacts count is different from SDK.");
        }

        /// <summary>
        /// Test MultiChild case.
        /// </summary>
        [Test]
        public void TestMultiChilIgnore()
        {
            var scene = OpenTestScene();
            var root = scene.GetRootGameObjects().First((obj) => obj.name == "MultiChildIgnore");

            var pbs = root.GetComponentsInChildren<VRCPhysBone>(true).Select(c => new VRCSDKUtility.Reflection.PhysBone(c)).ToArray();
            var colliders = root.GetComponentsInChildren<VRCPhysBoneCollider>(true).Select(c => new VRCSDKUtility.Reflection.PhysBoneCollider(c)).ToArray();
            var contacts = new VRCSDKUtility.Reflection.ContactBase[0];
            var perfs = AvatarDynamics.CalculatePerformanceStats(root, pbs, colliders, contacts);

            var sdkStats = new AvatarPerformanceStats(true);
            AvatarPerformance.CalculatePerformanceStats(root.name, root, sdkStats, true);

            Assert.AreEqual(1, root.GetComponentsInChildren<VRCPhysBone>(true).Length);
            Assert.AreEqual(1, root.GetComponentsInChildren<VRCPhysBoneCollider>(true).Length);
            Assert.AreEqual(0, root.GetComponentsInChildren<ContactBase>(true).Length);

            Assert.AreEqual(sdkStats.physBone.Value.componentCount, perfs.PhysBonesCount, "PhysBones count is different from SDK.");
            Assert.AreEqual(sdkStats.physBone.Value.transformCount, perfs.PhysBonesTransformCount, "PhysBones Transform count is different from SDK.");
            Assert.AreEqual(sdkStats.physBone.Value.colliderCount, perfs.PhysBonesColliderCount, "PhysBoneColliders count is different from SDK.");
            Assert.AreEqual(sdkStats.physBone.Value.collisionCheckCount, perfs.PhysBonesCollisionCheckCount, "PhysBones collision check count is different from SDK.");
            Assert.AreEqual(sdkStats.contactCount.Value, perfs.ContactsCount, "Contacts count is different from SDK.");

            perfs = AvatarDynamics.CalculatePerformanceStats(root, pbs, new VRCSDKUtility.Reflection.PhysBoneCollider[0], contacts);
            Assert.AreEqual(0, perfs.PhysBonesCollisionCheckCount, "PhysBones collision check count should be 0 when colliders are missing.");
        }

        /// <summary>
        /// Test MultiChild case.
        /// </summary>
        [Test]
        public void TestMultiChildFirst()
        {
            var scene = OpenTestScene();
            var root = scene.GetRootGameObjects().First((obj) => obj.name == "MultiChildFirst");

            var pbs = root.GetComponentsInChildren<VRCPhysBone>(true).Select(c => new VRCSDKUtility.Reflection.PhysBone(c)).ToArray();
            var colliders = root.GetComponentsInChildren<VRCPhysBoneCollider>(true).Select(c => new VRCSDKUtility.Reflection.PhysBoneCollider(c)).ToArray();
            var contacts = new VRCSDKUtility.Reflection.ContactBase[0];
            var perfs = AvatarDynamics.CalculatePerformanceStats(root, pbs, colliders, contacts);

            var sdkStats = new AvatarPerformanceStats(true);
            AvatarPerformance.CalculatePerformanceStats(root.name, root, sdkStats, true);

            Assert.AreEqual(1, root.GetComponentsInChildren<VRCPhysBone>(true).Length);
            Assert.AreEqual(1, root.GetComponentsInChildren<VRCPhysBoneCollider>(true).Length);
            Assert.AreEqual(0, root.GetComponentsInChildren<ContactBase>(true).Length);

            Assert.AreEqual(sdkStats.physBone.Value.componentCount, perfs.PhysBonesCount, "PhysBones count is different from SDK.");
            Assert.AreEqual(sdkStats.physBone.Value.transformCount, perfs.PhysBonesTransformCount, "PhysBones Transform count is different from SDK.");
            Assert.AreEqual(sdkStats.physBone.Value.colliderCount, perfs.PhysBonesColliderCount, "PhysBoneColliders count is different from SDK.");
            Assert.AreEqual(sdkStats.physBone.Value.collisionCheckCount, perfs.PhysBonesCollisionCheckCount, "PhysBones collision check count is different from SDK.");
            Assert.AreEqual(sdkStats.contactCount.Value, perfs.ContactsCount, "Contacts count is different from SDK.");

            perfs = AvatarDynamics.CalculatePerformanceStats(root, pbs, new VRCSDKUtility.Reflection.PhysBoneCollider[0], contacts);
            Assert.AreEqual(0, perfs.PhysBonesCollisionCheckCount, "PhysBones collision check count should be 0 when colliders are missing.");
        }

        /// <summary>
        /// Test EndpointPosition case.
        /// </summary>
        [Test]
        public void TestEndpointPosition()
        {
            var scene = OpenTestScene();
            var root = scene.GetRootGameObjects().First((obj) => obj.name == "EndpointPosition");

            var pbs = root.GetComponentsInChildren<VRCPhysBone>(true).Select(c => new VRCSDKUtility.Reflection.PhysBone(c)).ToArray();
            var colliders = root.GetComponentsInChildren<VRCPhysBoneCollider>(true).Select(c => new VRCSDKUtility.Reflection.PhysBoneCollider(c)).ToArray();
            var contacts = new VRCSDKUtility.Reflection.ContactBase[0];
            var perfs = AvatarDynamics.CalculatePerformanceStats(root, pbs, colliders, contacts);

            var sdkStats = new AvatarPerformanceStats(true);
            AvatarPerformance.CalculatePerformanceStats(root.name, root, sdkStats, true);

            Assert.AreEqual(1, root.GetComponentsInChildren<VRCPhysBone>(true).Length);
            Assert.AreEqual(1, root.GetComponentsInChildren<VRCPhysBoneCollider>(true).Length);
            Assert.AreEqual(0, root.GetComponentsInChildren<ContactBase>(true).Length);

            Assert.AreEqual(sdkStats.physBone.Value.componentCount, perfs.PhysBonesCount, "PhysBones count is different from SDK.");
            Assert.AreEqual(sdkStats.physBone.Value.transformCount, perfs.PhysBonesTransformCount, "PhysBones Transform count is different from SDK.");
            Assert.AreEqual(sdkStats.physBone.Value.colliderCount, perfs.PhysBonesColliderCount, "PhysBoneColliders count is different from SDK.");
            Assert.AreEqual(sdkStats.physBone.Value.collisionCheckCount, perfs.PhysBonesCollisionCheckCount, "PhysBones collision check count is different from SDK.");
            Assert.AreEqual(sdkStats.contactCount.Value, perfs.ContactsCount, "Contacts count is different from SDK.");

            perfs = AvatarDynamics.CalculatePerformanceStats(root, pbs, new VRCSDKUtility.Reflection.PhysBoneCollider[0], contacts);
            Assert.AreEqual(0, perfs.PhysBonesCollisionCheckCount, "PhysBones collision check count should be 0 when colliders are missing.");
        }

        private static AvatarPerformanceStatsLevelSet LoadQuestStatsLevelSet()
        {
            var questStatsLevelSetPath = AssetDatabase.GUIDToAssetPath("f0f530dea3891c04e8ab37831627e702"); // AvatarPerformanceStatLevels_Quest.asset
            var questStatsLevelSet = AssetDatabase.LoadAssetAtPath<AvatarPerformanceStatsLevelSet>(questStatsLevelSetPath);
            return questStatsLevelSet;
        }

        private Scene OpenTestScene()
        {
            EditorSceneManager.OpenScene(TestUtils.FixturesFolder + "/Scenes/AvatarDynamicsTests.unity");
            return SceneManager.GetActiveScene();
        }
    }
}
