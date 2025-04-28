using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Process MeshFlipper components after other polygon reduction tools.
    /// </summary>
    public class MeshFlipperAfterPolygonReductionPass : Pass<MeshFlipperAfterPolygonReductionPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Flip meshes after polygon reduction";

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            MeshFlipperPassUtility.Execute(context, MeshFlipperProcessingPhase.AfterPolygonReduction);
        }
    }
}
