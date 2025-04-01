using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Utility for mesh flipper pass.
    /// </summary>
    internal static class MeshFlipperPassUtility
    {
        /// <summary>
        /// Execute mesh flipper pass.
        /// </summary>
        /// <param name="context">Build context.</param>
        /// <param name="targetPhase">Target processing phase.</param>
        internal static void Execute(BuildContext context, MeshFlipperProcessingPhase targetPhase)
        {
            var buildTarget = NdmfHelper.ResolveBuildTarget(context.AvatarRootObject);
            var objectRegistry = context.GetState<NdmfObjectRegistry>();

            Dictionary<Mesh, Mesh> flippedMeshes = new Dictionary<Mesh, Mesh>();
            var meshFlippers = context.AvatarRootObject.GetComponentsInChildren<MeshFlipper>(true)
                .Where(mf => mf.processingPhase == targetPhase);
            foreach (var meshFlipper in meshFlippers)
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
