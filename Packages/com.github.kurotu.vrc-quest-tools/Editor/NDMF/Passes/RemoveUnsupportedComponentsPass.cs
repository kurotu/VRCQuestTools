using System.Threading;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using nadena.dev.ndmf;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Remove unsupported components in NDMF.
    /// </summary>
    internal class RemoveUnsupportedComponentsPass : Pass<RemoveUnsupportedComponentsPass>
    {
        private SynchronizationContext mainContext = SynchronizationContext.Current;

        /// <inheritdoc/>
        public override string DisplayName => "Remove unsupported components";

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            if (context.AvatarRootObject.GetComponent<ConvertedAvatar>() != null)
            {
                var i18n = VRCQuestToolsSettings.I18nResource;
                var remover = VRCQuestTools.ComponentRemover;
                var components = remover.GetUnsupportedComponentsInChildren(context.AvatarRootObject, true);
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
                remover.RemoveUnsupportedComponentsInChildren(context.AvatarRootObject, true, false);
            }
        }
    }
}
