#if VQT_HAS_NDMF
using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    class RemoveVRCQuestToolsComponentsPass : Pass<RemoveVRCQuestToolsComponentsPass>
    {
        public override string DisplayName => "Remove VRCQuestTools components";

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
#endif
