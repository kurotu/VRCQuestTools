using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase.Editor.BuildPipeline;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Callback to calculate actual performance.
    /// </summary>
    internal class VRCQuestToolsActualPerformanceCallback : IVRCSDKPreprocessAvatarCallback
    {
#pragma warning disable SA1300
        /// <summary>
        /// Gets execution order for last processing.
        /// </summary>
        public int callbackOrder => int.MaxValue;
#pragma warning restore SA1300

        /// <summary>
        /// Calculate actual performance rating.
        /// </summary>
        /// <param name="avatarGameObject">Target avatar object.</param>
        /// <returns>always true.</returns>
        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            // Callback is called also on play mode by "Apply on Play" of non-destructive context.
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return true;
            }
            var pipelineManager = avatarGameObject.GetComponent<PipelineManager>();
            if (string.IsNullOrEmpty(pipelineManager.blueprintId))
            {
                return true;
            }

            var isMobile = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android || EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
            var stats = VRCSDKUtility.CalculatePerformanceStats(avatarGameObject, isMobile);
            var rating = stats.GetPerformanceRatingForCategory(VRC.SDKBase.Validation.Performance.AvatarPerformanceCategory.Overall);
            NdmfSessionState.LastActualPerformanceRating[pipelineManager.blueprintId] = rating;
            return true;
        }
    }
}
