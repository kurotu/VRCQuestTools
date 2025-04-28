using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Process MeshFlipper components before other polygon reduction tools.
    /// </summary>
    public class MeshFlipperPass : Pass<MeshFlipperPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Flip meshes before polygon reduction";

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            MeshFlipperPassUtility.Execute(context, MeshFlipperProcessingPhase.BeforePolygonReduction);
        }
    }
}
