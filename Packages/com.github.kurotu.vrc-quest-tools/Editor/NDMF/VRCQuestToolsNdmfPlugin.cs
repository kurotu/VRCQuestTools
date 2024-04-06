using KRT.VRCQuestTools.Ndmf;
using nadena.dev.ndmf;

[assembly: ExportsPlugin(typeof(VRCQuestToolsNdmfPlugin))]

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// NDMF plugin of VRCQuestTools.
    /// </summary>
    internal class VRCQuestToolsNdmfPlugin : Plugin<VRCQuestToolsNdmfPlugin>
    {
        /// <summary>
        /// Gets the display name of the plugin.
        /// </summary>
        public override string DisplayName => "VRCQuestTools";

        /// <inheritdoc/>
        public override string QualifiedName => "com.github.kurotu.vrc-quest-tools";

        /// <inheritdoc/>
        protected override void Configure()
        {
            InPhase(BuildPhase.Resolving)
                .Run(CheckDependenciesPass.Instance)
                .Then.Run(PlatformGameObjectRemoverPass.Instance)
                .Then.Run(PlatformComponentRemoverPass.Instance);

            InPhase(BuildPhase.Optimizing)
                .BeforePlugin("com.anatawa12.avatar-optimizer")
                .Run(RemoveUnsupportedComponentsPass.Instance)
                .Then.Run(RemoveVRCQuestToolsComponentsPass.Instance);
        }
    }
}
