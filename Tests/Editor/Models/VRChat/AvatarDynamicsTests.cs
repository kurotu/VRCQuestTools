using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

#if VQT_HAS_VRCSDK
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase.Validation.Performance;
using VRC.SDKBase.Validation.Performance.Stats;
#endif

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
#if VQT_HAS_VRCSDK
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

            // https://creators.vrchat.com/avatars/avatar-performance-ranking-system/#quest-limits
            Assert.AreEqual(PerformanceRating.Good, stats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysBoneComponentCount));
            Assert.AreEqual(PerformanceRating.Good, stats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysBoneTransformCount));
            Assert.AreEqual(PerformanceRating.Good, stats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysBoneColliderCount));
            Assert.AreEqual(PerformanceRating.Good, stats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysBoneCollisionCheckCount));

            // Test AvatarPerformanceStatsLevelSet
            var questStatsLevelSetPath = AssetDatabase.GUIDToAssetPath("f0f530dea3891c04e8ab37831627e702"); // AvatarPerformanceStatLevels_Quest.asset
            var questStatsLevelSet = AssetDatabase.LoadAssetAtPath<AvatarPerformanceStatsLevelSet>(questStatsLevelSetPath);
            Assert.AreEqual(8, questStatsLevelSet.poor.physBone.componentCount);
            Assert.AreEqual(64, questStatsLevelSet.poor.physBone.transformCount);
            Assert.AreEqual(16, questStatsLevelSet.poor.physBone.colliderCount);
            Assert.AreEqual(64, questStatsLevelSet.poor.physBone.collisionCheckCount);
#else
            Assert.Ignore("VRCSDK is not installed.");
#endif
        }

        /// <summary>
        /// Test components are children of EditorOnly.
        /// </summary>
        [Test]
        public void TestVRCSDK_ChildOfEditorOnly()
        {
#if VQT_HAS_VRCSDK
            var scene = OpenTestScene();
            var root = scene.GetRootGameObjects().First((obj) => obj.name == "ChildOfEditorOnly");
            var stats = new AvatarPerformanceStats(true);
            AvatarPerformance.CalculatePerformanceStats(root.name, root, stats, true);

            Assert.AreEqual(1, root.GetComponentsInChildren<VRCPhysBone>(true).Length);
            Assert.AreEqual(1, root.GetComponentsInChildren<VRCPhysBoneCollider>(true).Length);
            Assert.IsNull(stats.physBone); // Children components of EditorOnly are ignored.

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
#else
            Assert.Ignore("VRCSDK is not installed.");
#endif
        }

        private Scene OpenTestScene()
        {
            EditorSceneManager.OpenScene(TestUtils.FixturesFolder + "/Scenes/AvatarDynamicsTests.unity");
            return SceneManager.GetActiveScene();
        }
    }
}
