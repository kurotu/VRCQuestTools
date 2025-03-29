using System.Collections.Generic;
using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
using UnityEngine;

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
            var objectRegistry = context.GetState<NdmfObjectRegistry>();

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
                if (originalMesh == null)
                {
                    continue;
                }

                var key = originalMesh;

                if (flippedMeshes.TryGetValue(key, out var flippedMesh))
                {
                    meshFlipper.SetSharedMesh(flippedMesh);
                }
                else
                {
                    try
                    {
                        var mesh = meshFlipper.CreateMesh();
                        flippedMeshes[key] = mesh;
                        meshFlipper.SetSharedMesh(mesh);
                        objectRegistry.RegisterReplacedObject(originalMesh, mesh);
                    }
                    catch (MeshFlipperMaskMissingException)
                    {
                        NdmfErrorReport.ReportError(new MeshFlipperMaskMissingError(meshFlipper));
                    }
                    catch (MeshFlipperMaskNotReadableException e)
                    {
                        var obj = NdmfObjectRegistry.GetReference(e.texture);
                        NdmfErrorReport.ReportError(new MeshFlipperMaskNotReadableError(meshFlipper, obj));
                    }
                }
            }
        }
    }
}
