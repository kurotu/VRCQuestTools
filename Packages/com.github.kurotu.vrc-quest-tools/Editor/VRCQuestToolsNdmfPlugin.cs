#if VQT_HAS_NDMF
using KRT.VRCQuestTools;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Ndmf;
using KRT.VRCQuestTools.Views;
using nadena.dev.ndmf;
using System.Linq;
using UnityEditor;
using UnityEngine;

[assembly: ExportsPlugin(typeof(VRCQuestToolsNdmfPlugin))]

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// NDMF plugin of VRCQuestTools.
    /// </summary>
    internal class VRCQuestToolsNdmfPlugin : Plugin<VRCQuestToolsNdmfPlugin>
    {
        /// <summary>
        /// Gets the display name of the plugin.
        /// </summary>
        public override string DisplayName => "VRCQuestTools";

        protected override void Configure()
        {
            var mainContext = System.Threading.SynchronizationContext.Current;

#if !VQT_HAS_NDMF_ERROR_REPORT
            InPhase(BuildPhase.Resolving)
                .Run("Clear report window", ctx =>
                {
                    if (EditorWindow.HasOpenInstances<NdmfReportWindow>())
                    {
                        NdmfReportWindow.Clear();
                    }
                });
#endif

            InPhase(BuildPhase.Optimizing)
                .BeforePlugin("com.anatawa12.avatar-optimizer")
                .Run("Remove unsupported components", ctx =>
                {
                    if (ctx.AvatarRootObject.GetComponent<ConvertedAvatar>() != null)
                    {
                        var i18n = VRCQuestToolsSettings.I18nResource;
                        var remover = VRCQuestTools.ComponentRemover;
                        var components = remover.GetUnsupportedComponentsInChildren(ctx.AvatarRootObject, true);
#if VQT_HAS_NDMF_ERROR_REPORT
                        foreach (var component in components)
                        {
                            var obj = ObjectRegistry.GetReference(component);
                            var e = new NdmfComponentRemoverWarning(component.GetType(), obj);
                            ErrorReport.ReportError(e);
                        }
#else
                        var messages = components.Select(c => new NdmfReportWindow.ReportItem
                        {
                            type = MessageType.Warning,
                            message = i18n.NdmfPluginRemovedUnsupportedComponent(c.GetType().Name, c.name),
                            gameObject = c.gameObject,
                        }).ToArray();

                        if (messages.Length > 0)
                        {
                            mainContext.Post(_ => NdmfReportWindow.ShowWindow(messages), null);
                        }
#endif
                        remover.RemoveUnsupportedComponentsInChildren(ctx.AvatarRootObject, true, false);
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
