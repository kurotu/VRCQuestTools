using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Validation.Performance;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Session state for VRCQuestTools NDMF.
    /// </summary>
    internal static class NdmfSessionState
    {
        /// <summary>
        /// Gets the last actual performance rating.
        /// This is now a reference to the shared session state.
        /// </summary>
        internal static Dictionary<string, PerformanceRating> LastActualPerformanceRating => Models.VRCQuestToolsSessionState.LastActualPerformanceRating;

        private const string BaseKey = "KRT.VRCQuestTools.Ndmf.";
        private const string BuildTargetKey = BaseKey + "BuildTarget";

        /// <summary>
        /// Gets or sets the build target platform.
        /// </summary>
        internal static Models.BuildTarget BuildTarget
        {
            get => (Models.BuildTarget)SessionState.GetInt(BuildTargetKey, (int)Models.BuildTarget.Auto);
            set => SessionState.SetInt(BuildTargetKey, (int)value);
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            BuildTarget = Models.BuildTarget.Auto;
        }
    }
}
