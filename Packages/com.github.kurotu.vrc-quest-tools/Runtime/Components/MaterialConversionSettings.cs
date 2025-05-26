using KRT.VRCQuestTools.Models;
using UnityEngine;
using VRC.SDKBase;

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// Component to configure material conversion.
    /// </summary>
    [AddComponentMenu("VRCQuestTools/VQT Material Conversion Settings")]

    [HelpURL("https://kurotu.github.io/VRCQuestTools/docs/references/components/material-conversion-settings?lang=auto")]
    public class MaterialConversionSettings : VRCQuestToolsEditorOnly, IMaterialConversionComponent, IPlatformDependentComponent, INdmfComponent
    {
        /// <summary>
        /// Default material convert setting. The default value is <see cref="ToonLitConvertSettings"/>.
        /// </summary>
        [SerializeReference]
        public IMaterialConvertSettings defaultMaterialConvertSettings = new ToonLitConvertSettings();

        /// <summary>
        /// Additional material convert settings.
        /// </summary>
        [SerializeReference]
        public AdditionalMaterialConvertSettings[] additionalMaterialConvertSettings = { };

        /// <summary>
        /// Whether to remove redundant material slots that are greater than the number of submeshes.
        /// </summary>
        [SerializeField]
        public bool removeExtraMaterialSlots = true;

        /// <summary>
        /// NDMF phase to convert the avatar.
        /// </summary>
        [SerializeField]
        public AvatarConverterNdmfPhase ndmfPhase = AvatarConverterNdmfPhase.Auto;

        /// <inheritdoc/>
        public IMaterialConvertSettings DefaultMaterialConvertSettings => defaultMaterialConvertSettings;

        /// <inheritdoc/>
        public AdditionalMaterialConvertSettings[] AdditionalMaterialConvertSettings
        {
            get => additionalMaterialConvertSettings;
            set => additionalMaterialConvertSettings = value;
        }

        /// <inheritdoc/>
        public bool RemoveExtraMaterialSlots => removeExtraMaterialSlots;

        /// <inheritdoc/>
        public AvatarConverterNdmfPhase NdmfPhase => ndmfPhase;

        /// <inheritdoc/>
        public bool IsPrimaryRoot
        {
            get
            {
                var isRoot = gameObject.GetComponent<VRC_AvatarDescriptor>() != null;
                var hasAvatarConverterSettings = gameObject.GetComponent<AvatarConverterSettings>() != null;
                return isRoot && !hasAvatarConverterSettings;
            }
        }

        private void Reset()
        {
            defaultMaterialConvertSettings.LoadDefaultAssets();
        }
    }
}
