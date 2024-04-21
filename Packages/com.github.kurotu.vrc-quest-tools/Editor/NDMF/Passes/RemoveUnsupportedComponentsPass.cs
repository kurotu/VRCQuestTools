using System.Linq;
using System.Threading;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using nadena.dev.ndmf;
#if !VQT_HAS_NDMF_ERROR_REPORT
using KRT.VRCQuestTools.Ndmf.Dummy;
#endif

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
            var buildTarget = NdmfHelper.ResolveBuildTarget(context.AvatarRootObject);

            var shouldRemove = buildTarget == BuildTarget.Android
                || context.AvatarRootObject.GetComponent<ConvertedAvatar>() != null;

            if (shouldRemove)
            {
                var i18n = VRCQuestToolsSettings.I18nResource;
                var remover = VRCQuestTools.ComponentRemover;
                var components = remover.GetUnsupportedComponentsInChildren(context.AvatarRootObject, true);

                foreach (var component in components)
                {
                    var obj = ObjectRegistry.GetReference(component);
                    var e = new NdmfComponentRemoverWarning(component.GetType(), obj);
                    ErrorReport.ReportError(e);
                }

                remover.RemoveUnsupportedComponentsInChildren(context.AvatarRootObject, true, false);
            }
        }
    }
}
