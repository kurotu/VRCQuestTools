using nadena.dev.ndmf;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Convert the avatar with AvatarConverterSettings component in NDMF.
    /// </summary>
    internal class AvatarConverterTransformingPass : Pass<AvatarConverterTransformingPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Convert avatar for Mobile in transforming phase";

        /// <summary>
        /// Runs this pass directly for EditMode tests, bypassing the NDMF pass pipeline.
        /// </summary>
        /// <param name="context">BuildContext.</param>
        internal void RunForTest(BuildContext context)
        {
            Execute(context);
        }

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
