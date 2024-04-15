using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
using UnityEditor;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Configure build target platform in NDMF.
    /// </summary>
    internal class BuildTargetConfigurationPass : Pass<BuildTargetConfigurationPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Configure build target platform";

        /// <inheritdoc/>
        protected override void Execute(BuildContext ctx)
        {
            var targetSettings = ctx.AvatarRootObject.GetComponent<PlatformTargetSettings>();
            if (targetSettings == null)
            {
                targetSettings = ctx.AvatarRootObject.AddComponent<PlatformTargetSettings>();
            }

            if (NdmfSessionState.BuildTarget != Models.BuildTarget.Auto)
            {
                targetSettings.buildTarget = NdmfSessionState.BuildTarget;
            }

            if (targetSettings.buildTarget == Models.BuildTarget.Auto)
            {
                switch (EditorUserBuildSettings.activeBuildTarget)
                {
                    case BuildTarget.StandaloneWindows:
                    case BuildTarget.StandaloneWindows64:
                        targetSettings.buildTarget = Models.BuildTarget.PC;
                        break;
                    case BuildTarget.Android:
                        targetSettings.buildTarget = Models.BuildTarget.Android;
                        break;
                    default:
                        throw new System.InvalidOperationException("Unsupported unity build target: " + EditorUserBuildSettings.activeBuildTarget);
                }
            }

            // Enforce build target to Android if unity build target is Android.
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                targetSettings.buildTarget = Models.BuildTarget.Android;
            }
        }
    }
}
