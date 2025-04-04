using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Process MeshFlipper components after other decimation tools.
    /// </summary>
    public class MeshFlipperAfterDecimationPass : Pass<MeshFlipperAfterDecimationPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Flip meshes after decimation";

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            MeshFlipperPassUtility.Execute(context, MeshFlipperProcessingPhase.AfterDecimation);
        }
    }
}
