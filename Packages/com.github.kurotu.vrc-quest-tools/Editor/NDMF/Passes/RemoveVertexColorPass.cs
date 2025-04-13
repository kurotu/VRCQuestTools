using System.Linq;
using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Remove vertex color from the avatar.
    /// </summary>
    internal class RemoveVertexColorPass : Pass<RemoveVertexColorPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Remove vertex color";

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            var settings = context.AvatarRootObject.GetComponent<AvatarConverterSettings>();
            if (settings == null)
            {
                return;
            }

            var buildTarget = NdmfHelper.ResolveBuildTarget(context.AvatarRootObject);
            if (buildTarget != Models.BuildTarget.Android)
            {
                return;
            }

            if (!settings.removeVertexColor)
            {
                return;
            }

            var objectRegistry = context.GetState<NdmfObjectRegistry>();
            var smrs = context.AvatarRootObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (var i = 0; i < smrs.Length; i++)
            {
                var smr = smrs[i];
                if (!ShouldRemoveVertexColor(smr.sharedMesh))
                {
                    continue;
                }
                var newMesh = Object.Instantiate(smr.sharedMesh);
                newMesh.colors32 = null;
                objectRegistry.RegisterReplacedObject(smr.sharedMesh, newMesh);
                smr.sharedMesh = newMesh;
            }

            var mfs = context.AvatarRootObject.GetComponentsInChildren<MeshFilter>(true);
            for (var i = 0; i < mfs.Length; i++)
            {
                var mf = mfs[i];
                if (!ShouldRemoveVertexColor(mf.sharedMesh))
                {
                    continue;
                }
                var newMesh = Object.Instantiate(mf.sharedMesh);
                newMesh.colors32 = null;
                objectRegistry.RegisterReplacedObject(mf.sharedMesh, newMesh);
                mf.sharedMesh = newMesh;
            }
        }

        private bool ShouldRemoveVertexColor(Mesh mesh)
        {
            if (mesh == null)
            {
                return false;
            }
            if (mesh.colors32 == null || mesh.colors32.Length == 0)
            {
                return false;
            }
            if (mesh.colors32.All(c => c.r == 255 && c.g == 255 && c.b == 255 && c.a == 255))
            {
                return false;
            }
            return true;
        }
    }
}
