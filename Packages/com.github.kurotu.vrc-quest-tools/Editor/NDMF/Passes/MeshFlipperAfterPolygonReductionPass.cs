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
            MeshFlipperPassUtility.Execute(context, MeshFlipperProcessingPhase.AfterPolygonReduction);
        }
    }
}
