#pragma warning disable SA1300,SA1600,SA1602

namespace KRT.VRCQuestTools.Mocks
{
    internal class Mock_AvatarPerformanceStats
    {
        public Mock_AvatarPerformanceStats(bool isMobile)
        {
        }

        public Mock_PerformanceRating GetPerformanceRatingForCategory(Mock_AvatarPerformanceCategory category)
        {
            return Mock_PerformanceRating.None;
        }
    }
}
