using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Utils;
using nadena.dev.ndmf;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Remove transparent parts from face.
    /// </summary>
    internal class RemoveTransparentFacePartsPass : Pass<RemoveTransparentFacePartsPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Remove transparent face parts";

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            var converterSettings = context.AvatarRootObject.GetComponent<AvatarConverterSettings>();
            if (converterSettings == null)
            {
                return;
            }

            if (!converterSettings.removeTransparentFaceParts)
            {
                return;
            }

            var renderers = converterSettings.GetComponentsInChildren<Renderer>(true);
            var objectRegistry = context.GetState<NdmfObjectRegistry>();
            foreach (var renderer in renderers)
            {
                if (VRCSDKUtility.IsFaceSkinnedMeshRenderer(context.AvatarDescriptor, renderer))
                {
                    var sharedMesh = RendererUtility.GetSharedMesh(renderer);
                    var m = MeshUtility.RemoveTransparentPart(sharedMesh, renderer.sharedMaterials);
                    if (m != null)
                    {
                        RendererUtility.SetSharedMesh(renderer, m);
                        objectRegistry.RegisterReplacedObject(sharedMesh, m);
                    }
                }
            }
        }
    }
}
