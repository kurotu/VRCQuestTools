using nadena.dev.ndmf;

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
            if (!AvatarConverterPassUtility.HasMaterialOperatorComponents(context.AvatarRootObject))
            {
                return;
            }

            if (AvatarConverterPassUtility.ResolveAvatarConverterNdmfPhase(context.AvatarRootObject) == Models.AvatarConverterNdmfPhase.Optimizing)
            {
                AvatarConverterPassUtility.ConvertAvatarInPass(context);
            }
        }
    }
}
