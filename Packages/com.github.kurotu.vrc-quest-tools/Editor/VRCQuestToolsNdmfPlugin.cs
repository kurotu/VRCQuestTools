#if VQT_HAS_NDMF
using KRT.VRCQuestTools;
using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
using UnityEngine;

[assembly: ExportsPlugin(typeof(VRCQuestToolsNdmfPlugin))]

namespace KRT.VRCQuestTools
{
    internal class VRCQuestToolsNdmfPlugin : Plugin<VRCQuestToolsNdmfPlugin>
    {
        public override string DisplayName => "VRCQuestTools";

        protected override void Configure()
        {
            InPhase(BuildPhase.Optimizing)
                .BeforePlugin("com.anatawa12.avatar-optimizer")
                .Run("Remove unsupported components", ctx =>
                {
                    if (ctx.AvatarRootObject.GetComponent<ConvertedAvatar>() != null)
                    {
                        var remover = VRCQuestTools.ComponentRemover;
                        remover.RemoveUnsupportedComponentsInChildren(ctx.AvatarRootObject, true);
                    }
                })
                .Then.Run("Remove VRCQuestTools components", ctx =>
                {
                    Component[] components = ctx.AvatarRootObject.GetComponentsInChildren<ConvertedAvatar>(true);
                    foreach (var component in components)
                    {
                        Object.DestroyImmediate(component);
                    }

                    components = ctx.AvatarRootObject.GetComponentsInChildren<VRCQuestToolsEditorOnly>(true);
                    foreach (var component in components)
                    {
                        Object.DestroyImmediate(component);
                    }

                });
        }
    }
}
#endif
