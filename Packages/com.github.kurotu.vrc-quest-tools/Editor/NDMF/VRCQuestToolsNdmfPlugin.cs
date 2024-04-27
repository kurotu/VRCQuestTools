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
#if !VQT_HAS_NDMF_ERROR_REPORT
            InPhase(BuildPhase.Resolving)
                .Run("Clear report window", ctx =>
                {
                    if (UnityEditor.EditorWindow.HasOpenInstances<NdmfReportWindow>())
                    {
                        NdmfReportWindow.Clear();
                    }
                });
#endif

            InPhase(BuildPhase.Resolving)
                .BeforePlugin("dev.logilabo.virtuallens2.apply-non-destructive") // need to configure vlens2.
                .Run(BuildTargetConfigurationPass.Instance)
                .Then.Run(PlatformGameObjectRemoverPass.Instance)
                .Then.Run(PlatformComponentRemoverPass.Instance)
                .Then.Run(AvatarConverterResolvingPass.Instance);

            InPhase(BuildPhase.Transforming)
                .AfterPlugin("net.rs64.tex-trans-tool") // needs generated textures
                .AfterPlugin("nadena.dev.modular-avatar") // convert built avatar
                .Run(AvatarConverterTransformingPass.Instance);

            InPhase(BuildPhase.Transforming)
                .AfterPlugin("MantisLODEditor.ndmf") // needs vertex color to control decimation
                .Run(RemoveVertexColorPass.Instance);

            InPhase(BuildPhase.Optimizing)
                .BeforePlugin("com.anatawa12.avatar-optimizer")
                .Run(RemoveUnsupportedComponentsPass.Instance)
                .Then.Run(RemoveVRCQuestToolsComponentsPass.Instance)
                .Then.Run(CheckTextureFormatPass.Instance);
        }
    }
}
