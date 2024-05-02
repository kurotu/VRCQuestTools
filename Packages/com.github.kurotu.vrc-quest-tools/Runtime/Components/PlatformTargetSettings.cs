using KRT.VRCQuestTools.Models;
using UnityEngine;

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// PlatformTargetSettings is a component to control VQT Platform components.
    /// </summary>
    [AddComponentMenu("VRCQuestTools/VQT Platform Target Settings")]
    [HelpURL("https://kurotu.github.io/VRCQuestTools/docs/references/components/platform-target-settings?lang=auto")]
    [DisallowMultipleComponent]
    public class PlatformTargetSettings : VRCQuestToolsEditorOnly, INdmfComponent
    {
        /// <summary>
        /// Build target to control VQT Platform components.
        /// </summary>
        public BuildTarget buildTarget = BuildTarget.Auto;
    }
}
