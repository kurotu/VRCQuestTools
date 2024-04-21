using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Remove GameObjects by PlatformGameObjectRemover in NDMF.
    /// </summary>
    internal class PlatformGameObjectRemoverPass : Pass<PlatformGameObjectRemoverPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Remove platform game objects";

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            var buildTarget = NdmfHelper.ResolveBuildTarget(context.AvatarRootObject);
            var components = context.AvatarRootObject.GetComponentsInChildren<PlatformGameObjectRemover>(true);
            foreach (var c in components)
            {
                if (c == null)
                {
                    continue;
                }

                if (c.gameObject == null)
                {
                    continue;
                }

                if (buildTarget == Models.BuildTarget.PC && c.removeOnPC)
                {
                    Object.DestroyImmediate(c.gameObject);
                }
                if (buildTarget == Models.BuildTarget.Android && c.removeOnAndroid)
                {
                    Object.DestroyImmediate(c.gameObject);
                }
            }
        }
    }
}
