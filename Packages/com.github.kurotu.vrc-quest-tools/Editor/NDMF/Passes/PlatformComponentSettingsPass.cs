using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Remove non-active platform components by PlatformComponentSettings in NDMF.
    /// </summary>
    internal class PlatformComponentSettingsPass : Pass<PlatformComponentSettingsPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Remove non-active platform components";

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            var components = context.AvatarRootObject.GetComponentsInChildren<PlatformComponentSettings>(true);
            foreach (var component in components)
            {
                var buildTarget = component.buildTarget;
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
                            throw new System.InvalidOperationException("Unknown build target: " + EditorUserBuildSettings.activeBuildTarget);
                    }
                }

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

                    if (buildTarget == Models.BuildTarget.PC && !settings.enabledOnPC)
                    {
                        Debug.Log($"Remove {settings.component.GetType().Name} for PC platform", settings.component.gameObject);
                        Object.DestroyImmediate(settings.component);
                    }
                    if (buildTarget == Models.BuildTarget.Android && !settings.enabledOnAndroid)
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
