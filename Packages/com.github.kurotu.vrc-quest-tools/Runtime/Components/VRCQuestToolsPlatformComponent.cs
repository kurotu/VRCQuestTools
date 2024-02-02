using KRT.VRCQuestTools.Models;

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// Editor only component which depends on build target.
    /// </summary>
    public class VRCQuestToolsPlatformComponent : VRCQuestToolsEditorOnly
    {
        /// <summary>
        /// Build target to control this component.
        /// </summary>
        public BuildTarget buildTarget = BuildTarget.Auto;
    }
}
