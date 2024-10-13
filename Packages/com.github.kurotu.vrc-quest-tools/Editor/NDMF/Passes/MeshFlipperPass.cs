using System.Collections.Generic;
using nadena.dev.ndmf;
using UnityEngine;
#if !VQT_HAS_NDMF_ERROR_REPORT
using KRT.VRCQuestTools.Ndmf.Dummy;
#endif

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Process MeshFlipper components.
    /// </summary>
    public class MeshFlipperPass : Pass<MeshFlipperPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Flip meshes";

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            var buildTarget = NdmfHelper.ResolveBuildTarget(context.AvatarRootObject);

            Dictionary<Mesh, Mesh> flippedMeshes = new Dictionary<Mesh, Mesh>();
            foreach (var meshFlipper in context.AvatarRootObject.GetComponentsInChildren<Components.MeshFlipper>(true))
            {
                if (buildTarget == Models.BuildTarget.PC && !meshFlipper.enabledOnPC)
                {
                    continue;
                }
                if (buildTarget == Models.BuildTarget.Android && !meshFlipper.enabledOnAndroid)
                {
                    continue;
                }

                var originalMesh = meshFlipper.GetSharedMesh();
                var key = originalMesh;

                if (flippedMeshes.TryGetValue(key, out var flippedMesh))
                {
                    meshFlipper.SetSharedMesh(flippedMesh);
                }
                else
                {
                    var mesh = meshFlipper.CreateMesh();
                    flippedMeshes[key] = mesh;
                    meshFlipper.SetSharedMesh(mesh);
                    ObjectRegistry.RegisterReplacedObject(originalMesh, mesh);
                }
            }
        }
    }
}
