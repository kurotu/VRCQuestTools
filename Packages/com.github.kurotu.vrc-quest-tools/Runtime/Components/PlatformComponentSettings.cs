using System.Linq;
using KRT.VRCQuestTools.Models;
using UnityEngine;

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// PlatformComponentSettings is a component that manages components depending on the target platform.
    /// Disabled components are automatically removed on build.
    /// </summary>
    [AddComponentMenu("VRCQuestTools/VQT Platform Component Settings")]
    [HelpURL("https://kurotu.github.io/VRCQuestTools/ja/docs/references/components/platform-component-settings")]
    [DisallowMultipleComponent]
    public class PlatformComponentSettings : VRCQuestToolsEditorOnly, IVRCQuestToolsNdmfComponent
    {
        /// <summary>
        /// Build target to enable components.
        /// </summary>
        public BuildTarget buildTarget = BuildTarget.Auto;

        /// <summary>
        /// PlatformComponentSettingsItems to manage components.
        /// </summary>
        public PlatformComponentSettingsItem[] componentSettings = new PlatformComponentSettingsItem[0];

        /// <summary>
        /// Remove non-existing components then add unregistered components.
        /// </summary>
        public void UpdateComponentSettings()
        {
            var components = gameObject.GetComponents<Component>()
                .Where(c => !(c is Transform))
                .Where(c => !(c is PlatformComponentSettings))
                .ToArray();

            var keep = componentSettings.Where(s => components.Contains(s.component));
            var add = components.Where(c => componentSettings.Any(s => s.component == c) == false)
                .Select(c => new PlatformComponentSettingsItem { component = c, enabledOnPC = true, enabledOnAndroid = true });

            componentSettings = keep.Concat(add).ToArray();
        }
    }
}
