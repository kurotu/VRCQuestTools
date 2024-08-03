#if VQT_HAS_VRCSDK_BASE
using VRC.SDKBase.Validation.Performance;
using VRC.SDKBase.Validation.Performance.Stats;
#else
using AvatarPerformanceCategory = KRT.VRCQuestTools.Mocks.Mock_AvatarPerformanceCategory;
using AvatarPerformanceStatsLevel = KRT.VRCQuestTools.Mocks.Mock_AvatarPerformanceStatsLevel;
using AvatarPerformanceStatsLevelSet = KRT.VRCQuestTools.Mocks.Mock_AvatarPerformanceStatsLevelSet;
using PerformanceRating = KRT.VRCQuestTools.Mocks.Mock_PerformanceRating;
#endif

namespace KRT.VRCQuestTools.Models.VRChat
{
    /// <summary>
    /// Avatar performance rating.
    /// </summary>
    internal static class AvatarPerformanceCalculator
    {
        /// <summary>
        /// Gets the rating value.
        /// </summary>
        /// <param name="stats">Avatar Dynamics stats.</param>
        /// <param name="statsLevelSet">StatsLevelSet.</param>
        /// <param name="category">Performance category.</param>
        /// <returns>Performance rating.</returns>
        internal static PerformanceRating GetPerformanceRating(AvatarDynamics.PerformanceStats stats, AvatarPerformanceStatsLevelSet statsLevelSet, AvatarPerformanceCategory category)
        {
            switch (category)
            {
                case AvatarPerformanceCategory.PhysBoneComponentCount:
                    return GetRating(stats.PhysBonesCount, statsLevelSet, category);
                case AvatarPerformanceCategory.PhysBoneTransformCount:
                    return GetRating(stats.PhysBonesTransformCount, statsLevelSet, category);
                case AvatarPerformanceCategory.PhysBoneColliderCount:
                    return GetRating(stats.PhysBonesColliderCount, statsLevelSet, category);
                case AvatarPerformanceCategory.PhysBoneCollisionCheckCount:
                    return GetRating(stats.PhysBonesCollisionCheckCount, statsLevelSet, category);
                case AvatarPerformanceCategory.ContactCount:
                    return GetRating(stats.ContactsCount, statsLevelSet, category);
                default:
                    throw new System.InvalidProgramException();
            }
        }

        private static PerformanceRating GetRating(int statsValue, AvatarPerformanceStatsLevelSet statsLevelSet, AvatarPerformanceCategory category)
        {
            if (statsValue <= GetRatingValue(statsLevelSet, category, PerformanceRating.Excellent))
            {
                return PerformanceRating.Excellent;
            }
            if (statsValue <= GetRatingValue(statsLevelSet, category, PerformanceRating.Good))
            {
                return PerformanceRating.Good;
            }
            if (statsValue <= GetRatingValue(statsLevelSet, category, PerformanceRating.Medium))
            {
                return PerformanceRating.Medium;
            }
            if (statsValue <= GetRatingValue(statsLevelSet, category, PerformanceRating.Poor))
            {
                return PerformanceRating.Poor;
            }
            return PerformanceRating.VeryPoor;
        }

        private static int GetRatingValue(AvatarPerformanceStatsLevelSet statsLevelSet, AvatarPerformanceCategory category, PerformanceRating rating)
        {
#if VQT_HAS_VRCSDK_BASE
            var statsLevel = GetStatsLevel(statsLevelSet, rating);
            switch (category)
            {
                case AvatarPerformanceCategory.PhysBoneComponentCount:
                    return statsLevel.physBone.componentCount;
                case AvatarPerformanceCategory.PhysBoneTransformCount:
                    return statsLevel.physBone.transformCount;
                case AvatarPerformanceCategory.PhysBoneColliderCount:
                    return statsLevel.physBone.colliderCount;
                case AvatarPerformanceCategory.PhysBoneCollisionCheckCount:
                    return statsLevel.physBone.collisionCheckCount;
                case AvatarPerformanceCategory.ContactCount:
                    return statsLevel.contactCount;
                default:
                    throw new System.NotImplementedException();
            }
#else
            throw new System.InvalidProgramException();
#endif
        }

        private static AvatarPerformanceStatsLevel GetStatsLevel(AvatarPerformanceStatsLevelSet statsLevelSet, PerformanceRating rating)
        {
            switch (rating)
            {
                case PerformanceRating.None:
                    throw new System.InvalidProgramException();
                case PerformanceRating.Excellent:
                    return statsLevelSet.excellent;
                case PerformanceRating.Good:
                    return statsLevelSet.good;
                case PerformanceRating.Medium:
                    return statsLevelSet.medium;
                case PerformanceRating.Poor:
                    return statsLevelSet.poor;
                case PerformanceRating.VeryPoor:
                    throw new System.InvalidProgramException();
                default:
                    throw new System.NotImplementedException();
            }
        }
    }
}
