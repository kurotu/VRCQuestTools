#if VQT_HAS_NDMF
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Ndmf;
using KRT.VRCQuestTools.Views;
using nadena.dev.ndmf;
using System.Linq;
using UnityEditor;
using UnityEngine;

[assembly: ExportsPlugin(typeof(VRCQuestToolsNdmfPlugin))]

namespace KRT.VRCQuestTools.Ndmf
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
                .Run(RemoveUnsupportedComponentsPass.Instance)
                .Then.Run(RemoveVRCQuestToolsComponentPass.Instance);
        }
    }
}
#endif
