using System;
using KRT.VRCQuestTools.Ndmf;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;

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

#if VQT_HAS_NDMF_ERROR_REPORT
        private static readonly Lazy<Texture2D> logoTexture = new Lazy<Texture2D>(() => AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("e6a14816c3530ec498d3a7f1aad45a5a")));
        /// <inheritdoc/>
        public override Texture2D LogoTexture => logoTexture.Value;
#endif

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

            InPhase(BuildPhase.Generating)
                .Run(AssignNetworkIDsPass.Instance);

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
                .Then.Run(RemoveVRCQuestToolsComponentsPass.Instance);

            InPhase(BuildPhase.Optimizing)
                .AfterPlugin("com.anatawa12.avatar-optimizer")
                .Run(CheckTextureFormatPass.Instance);
        }
    }
}
