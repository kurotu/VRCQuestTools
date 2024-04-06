using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using nadena.dev.ndmf;
using UnityEditor;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// CheckDependenciesPass is a pass to check dependencies.
    /// </summary>
    internal class CheckDependenciesPass : Pass<CheckDependenciesPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Check dependencies";

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
#if !VQT_HAS_NDMF_ERROR_REPORT
            var needsNdmf = context.AvatarRootObject.GetComponentInChildren<IVRCQuestToolsNdmfComponent>() != null;
            if (needsNdmf)
            {
                var version = "1.3.0";
                var i18n = VRCQuestToolsSettings.I18nResource;
                EditorUtility.DisplayDialog("VRCQuestTools NDMF", i18n.NdmfPluginRequiresNdmfUpdate(version), "OK");
                throw new System.InvalidOperationException($"Non-Destructive Modular Framework (NDMF) {version} or later is required. Please update NDMF.");
            }
#endif
        }
    }
}
