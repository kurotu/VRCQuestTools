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
            if (!AvatarConverterPassUtility.HasMaterialOperatorComponents(context.AvatarRootObject))
            {
                return;
            }

            if (AvatarConverterPassUtility.ResolveAvatarConverterNdmfPhase(context.AvatarRootObject) == Models.AvatarConverterNdmfPhase.Transforming)
            {
                AvatarConverterPassUtility.ConvertAvatarInPass(context);
            }
        }
    }
}
