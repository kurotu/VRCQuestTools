using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
#if !VQT_HAS_NDMF_ERROR_REPORT
using KRT.VRCQuestTools.Ndmf.Dummy;
#endif

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Convert the avatar with AvatarConverterSettings component in NDMF.
    /// </summary>
    internal class AvatarConverterOptimizingPass : Pass<AvatarConverterOptimizingPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Convert avatar for Android in optimizing phase";

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            var settings = context.AvatarRootObject.GetComponent<AvatarConverterSettings>();
            if (settings == null)
            {
                return;
            }
            if (settings.ndmfPhase == Models.AvatarConverterNdmfPhase.Optimizing)
            {
                NdmfPluginUtility.ConvertAvatarInPass(context, settings);
            }
        }
    }
}
