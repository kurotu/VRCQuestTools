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
            var buildTarget = Models.BuildTarget.Auto;
            var targetSettings = context.AvatarRootObject.GetComponent<PlatformTargetSettings>();
            if (targetSettings != null)
            {
                buildTarget = targetSettings.buildTarget;
            }

            if (buildTarget == Models.BuildTarget.Auto)
            {
                switch (EditorUserBuildSettings.activeBuildTarget)
                {
                    case BuildTarget.StandaloneWindows:
                    case BuildTarget.StandaloneWindows64:
                        buildTarget = Models.BuildTarget.PC;
                        break;
                    case BuildTarget.Android:
                        buildTarget = Models.BuildTarget.Android;
                        break;
                    default:
                        throw new System.InvalidOperationException("Unsupported build target: " + EditorUserBuildSettings.activeBuildTarget);
                }
            }

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
