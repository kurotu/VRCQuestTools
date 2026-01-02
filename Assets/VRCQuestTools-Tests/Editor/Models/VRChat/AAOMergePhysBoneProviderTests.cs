using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase.Validation.Performance;
using VRC.SDKBase.Validation.Performance.Stats;

namespace KRT.VRCQuestTools.Models.VRChat
{
    /// <summary>
    /// Integration tests for AAOMergePhysBoneProvider.
    /// </summary>
    public class AAOMergePhysBoneProviderTests
    {
        [Test]
        public void SimpleCase_MatchesManualBakePerformance()
        {
            var scene = OpenTestScene();
            var root = scene.GetRootGameObjects().First(obj => obj.name == "SimpleCase");

            var mergePhysBone = FindMergePhysBoneComponent(root);
            Assert.NotNull(mergePhysBone, "MergePhysBone component is missing in the test scene.");

            var provider = new AAOMergePhysBoneProvider(mergePhysBone);
            var estimatedStats = EstimateMergePhysBonePerformance(root, provider);

            var bakedRoot = nadena.dev.ndmf.AvatarProcessor.ProcessAvatarUI(root);
            Assert.NotNull(bakedRoot, "Manual bake failed to produce an avatar.");

            try
            {
                var bakedPerformance = CalculatePerformanceStats(bakedRoot);
                var bakedDynamicsStats = new AvatarDynamics.PerformanceStats
                {
                    PhysBonesCount = bakedPerformance.physBone?.componentCount ?? 0,
                    PhysBonesTransformCount = bakedPerformance.physBone?.transformCount ?? 0,
                    PhysBonesColliderCount = bakedPerformance.physBone?.colliderCount ?? 0,
                    PhysBonesCollisionCheckCount = bakedPerformance.physBone?.collisionCheckCount ?? 0,
                    ContactsCount = bakedPerformance.contactCount ?? 0,
                };

                Assert.AreEqual(1, bakedDynamicsStats.PhysBonesCount, "Manual bake PhysBone count differs from expectation.");
                Assert.AreEqual(5, bakedDynamicsStats.PhysBonesTransformCount, "Manual bake PhysBone transform count differs from expectation.");

                Assert.AreEqual(bakedDynamicsStats.PhysBonesCount, estimatedStats.PhysBonesCount, "Provider estimate should match baked PhysBone count.");
                Assert.AreEqual(bakedDynamicsStats.PhysBonesTransformCount, estimatedStats.PhysBonesTransformCount, "Provider estimate should match baked PhysBone transform count.");

                var questLevelSet = LoadQuestStatsLevelSet();
                Assert.AreEqual(
                    AvatarPerformanceCalculator.GetPerformanceRating(estimatedStats, questLevelSet, AvatarPerformanceCategory.PhysBoneComponentCount),
                    AvatarPerformanceCalculator.GetPerformanceRating(bakedDynamicsStats, questLevelSet, AvatarPerformanceCategory.PhysBoneComponentCount),
                    "PhysBone component count rating should match manual bake.");
                Assert.AreEqual(
                    AvatarPerformanceCalculator.GetPerformanceRating(estimatedStats, questLevelSet, AvatarPerformanceCategory.PhysBoneTransformCount),
                    AvatarPerformanceCalculator.GetPerformanceRating(bakedDynamicsStats, questLevelSet, AvatarPerformanceCategory.PhysBoneTransformCount),
                    "PhysBone transform count rating should match manual bake.");
            }
            finally
            {
                if (bakedRoot != null)
                {
                    Object.DestroyImmediate(bakedRoot);
                }
            }
        }

        private static AvatarDynamics.PerformanceStats EstimateMergePhysBonePerformance(GameObject avatarRoot, AAOMergePhysBoneProvider provider)
        {
            var physBones = provider.GetPhysBones().Where(pb => pb != null).ToArray();
            var transforms = new HashSet<Transform>();
            foreach (var pb in physBones)
            {
                var pbRoot = pb.rootTransform == null ? pb.transform : pb.rootTransform;
                foreach (var t in pbRoot.GetComponentsInChildren<Transform>(true))
                {
                    transforms.Add(t);
                }
            }

            if (provider.RootTransform != null)
            {
                transforms.Add(provider.RootTransform);
            }

            var colliders = provider.Colliders.OfType<VRCPhysBoneCollider>().Where(c => c != null).Distinct().Count();

            return new AvatarDynamics.PerformanceStats
            {
                PhysBonesCount = 1,
                PhysBonesTransformCount = transforms.Count,
                PhysBonesColliderCount = colliders,
                PhysBonesCollisionCheckCount = 0,
                ContactsCount = 0,
            };
        }

        private static Component FindMergePhysBoneComponent(GameObject root)
        {
            return root.GetComponentsInChildren<Component>(true)
                .FirstOrDefault(c => c != null && c.GetType().FullName == "Anatawa12.AvatarOptimizer.MergePhysBone");
        }

        private static AvatarPerformanceStats CalculatePerformanceStats(GameObject root)
        {
            var stats = new AvatarPerformanceStats(true);
            AvatarPerformance.CalculatePerformanceStats(root.name, root, stats, true);
            return stats;
        }

        private static AvatarPerformanceStatsLevelSet LoadQuestStatsLevelSet()
        {
            var questStatsLevelSetPath = AssetDatabase.GUIDToAssetPath("f0f530dea3891c04e8ab37831627e702");
            return AssetDatabase.LoadAssetAtPath<AvatarPerformanceStatsLevelSet>(questStatsLevelSetPath);
        }

        private static Scene OpenTestScene()
        {
            EditorSceneManager.OpenScene(TestUtils.FixturesFolder + "/Scenes/AAOMergePhysBoneProviderTests.unity");
            return SceneManager.GetActiveScene();
        }
    }
}
