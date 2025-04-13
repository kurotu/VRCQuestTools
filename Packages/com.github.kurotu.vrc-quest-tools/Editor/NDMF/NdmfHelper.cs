using KRT.VRCQuestTools.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Helper functions for NDMF.
    /// </summary>
    internal static class NdmfHelper
    {
        /// <summary>
        /// Resolve the build target from the avatar root object.
        /// </summary>
        /// <param name="avatarRootObject">Avatar root object.</param>
        /// <returns>Resolved build target, PC or Android.</returns>
        public static Models.BuildTarget ResolveBuildTarget(GameObject avatarRootObject)
        {
            var targetSettings = avatarRootObject.GetComponent<PlatformTargetSettings>();
            if (targetSettings != null)
            {
                return ResolveBuildTarget(targetSettings.buildTarget);
            }

            return ResolveBuildTarget(Models.BuildTarget.Auto);
        }

        private static Models.BuildTarget ResolveBuildTarget(Models.BuildTarget buildTarget)
        {
            if (buildTarget != Models.BuildTarget.Auto)
            {
                return buildTarget;
            }

            Models.BuildTarget target;
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    target = Models.BuildTarget.PC;
                    break;
                case BuildTarget.Android:
                case BuildTarget.iOS:
                    target = Models.BuildTarget.Android;
                    break;
                default:
                    target = Models.BuildTarget.PC;
                    Debug.LogWarning($"[{VRCQuestTools.Name}] Unsupported unity build target: {EditorUserBuildSettings.activeBuildTarget}. Fallback to PC configuration.");
                    break;
            }
            Assert.IsTrue(target != Models.BuildTarget.Auto);
            return target;
        }
    }
}
