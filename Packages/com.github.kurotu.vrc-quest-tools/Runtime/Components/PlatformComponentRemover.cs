using System.Linq;
using KRT.VRCQuestTools.Models;
using UnityEngine;

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// PlatformComponentRemover removes components depending on the target platform.
    /// Selected components are automatically removed on build.
    /// </summary>
    [AddComponentMenu("VRCQuestTools/VQT Platform Component Remover")]
    [HelpURL("https://kurotu.github.io/VRCQuestTools/docs/references/components/platform-component-remover?lang=auto")]
    [DisallowMultipleComponent]
    public class PlatformComponentRemover : VRCQuestToolsEditorOnly, INdmfComponent
    {
        /// <summary>
        /// PlatformComponentRemoverItems to manage components.
        /// </summary>
        public PlatformComponentRemoverItem[] componentSettings = new PlatformComponentRemoverItem[0];

        /// <summary>
        /// Remove non-existing components then add unregistered components.
        /// </summary>
        public void UpdateComponentSettings()
        {
            var components = gameObject.GetComponents<Component>()
                .Where(c => !(c is Transform))
                .Where(c => !(c is PlatformComponentRemover))
                .ToArray();

            var keep = componentSettings.Where(s => components.Contains(s.component));
            var add = components.Where(c => componentSettings.Any(s => s.component == c) == false)
                .Select(c => new PlatformComponentRemoverItem { component = c });

            componentSettings = keep.Concat(add).ToArray();
        }
    }
}
