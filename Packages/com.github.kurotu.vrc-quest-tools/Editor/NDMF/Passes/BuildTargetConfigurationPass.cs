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
            if (NdmfSessionState.BuildTarget != Models.BuildTarget.Auto)
            {
                var targetSettings = ctx.AvatarRootObject.GetComponent<PlatformTargetSettings>()
                    ?? ctx.AvatarRootObject.AddComponent<PlatformTargetSettings>();
                targetSettings.buildTarget = NdmfSessionState.BuildTarget;
            }
            NdmfSessionState.BuildTarget = Models.BuildTarget.Auto;
        }
    }
}
