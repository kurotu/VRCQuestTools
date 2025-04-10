using KRT.VRCQuestTools.Models;
using UnityEngine;

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// Component to configure material conversion.
    /// </summary>
    [AddComponentMenu("VRCQuestTools/VQT Material Conversion Settings")]
    [HelpURL("https://kurotu.github.io/VRCQuestTools/docs/references/components/material-conversion-settings?lang=auto")]
    public class MaterialConversionSettings : VRCQuestToolsEditorOnly, IPlatformDependentComponent, INdmfComponent, IExperimentalComponent
    {
        /// <summary>
        /// Additional material convert settings.
        /// </summary>
        [SerializeReference]
        public AdditionalMaterialConvertSettings[] materialConvertSettings = { };
    }
}
