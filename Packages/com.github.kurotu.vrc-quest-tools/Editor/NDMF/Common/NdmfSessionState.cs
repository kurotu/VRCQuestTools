using UnityEditor;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Session state for VRCQuestTools NDMF.
    /// </summary>
    internal static class NdmfSessionState
    {
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
