using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Process MeshFlipper components before other decimation tools.
    /// </summary>
    public class MeshFlipperPass : Pass<MeshFlipperPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Flip meshes before decimation";

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            MeshFlipperPassUtility.Execute(context, MeshFlipperProcessingPhase.BeforeDecimation);
        }
    }
}
