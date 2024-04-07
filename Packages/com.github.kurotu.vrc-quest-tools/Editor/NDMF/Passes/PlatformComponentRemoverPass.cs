using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Remove components by PlatformComponentRemover in NDMF.
    /// </summary>
    internal class PlatformComponentRemoverPass : Pass<PlatformComponentRemoverPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Remove platform components";

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

            var components = context.AvatarRootObject.GetComponentsInChildren<PlatformComponentRemover>(true);
            foreach (var component in components)
            {
                foreach (var settings in component.componentSettings)
                {
                    if (settings.component == null)
                    {
                        continue;
                    }
                    if (!IsDecendant(settings.component.gameObject, context.AvatarRootObject))
                    {
                        continue;
                    }

                    if (buildTarget == Models.BuildTarget.PC && settings.removeOnPC)
                    {
                        Debug.Log($"Remove {settings.component.GetType().Name} for PC platform", settings.component.gameObject);
                        Object.DestroyImmediate(settings.component);
                    }
                    if (buildTarget == Models.BuildTarget.Android && settings.removeOnAndroid)
                    {
                        Debug.Log($"Remove {settings.component.GetType().Name} for Android platform", settings.component.gameObject);
                        Object.DestroyImmediate(settings.component);
                    }
                }
            }
        }

        private bool IsDecendant(GameObject decendant, GameObject root)
        {
            if (decendant == null)
            {
                return false;
            }
            if (root == null)
            {
                return false;
            }
            if (decendant == root)
            {
                return true;
            }
            var parent = decendant.transform.parent;
            if (parent == null)
            {
                return false;
            }
            return IsDecendant(parent.gameObject, root);
        }
    }
}
