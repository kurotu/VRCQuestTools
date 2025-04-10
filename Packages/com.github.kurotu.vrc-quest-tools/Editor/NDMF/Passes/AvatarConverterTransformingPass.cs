using nadena.dev.ndmf;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Convert the avatar with AvatarConverterSettings component in NDMF.
    /// </summary>
    internal class AvatarConverterTransformingPass : Pass<AvatarConverterTransformingPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Convert avatar for Android in transforming phase";

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            if (!NdmfPluginUtility.HasMaterialConversionComponents(context.AvatarRootObject))
            {
                return;
            }

            if (NdmfPluginUtility.ResolveAvatarConverterNdmfPhase(context.AvatarRootObject) == Models.AvatarConverterNdmfPhase.Transforming)
            {
                NdmfPluginUtility.ConvertAvatarInPass(context);
            }
        }
    }
}
