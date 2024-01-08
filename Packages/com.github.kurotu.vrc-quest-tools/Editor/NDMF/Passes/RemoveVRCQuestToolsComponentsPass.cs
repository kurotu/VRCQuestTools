using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Remove VRCQuestTools components in NDMF.
    /// </summary>
    internal class RemoveVRCQuestToolsComponentsPass : Pass<RemoveVRCQuestToolsComponentsPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Remove VRCQuestTools components";

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            var components = context.AvatarRootObject.GetComponentsInChildren<VRCQuestToolsEditorOnly>(true);
            foreach (var component in components)
            {
                Object.DestroyImmediate(component);
            }
        }
    }
}
