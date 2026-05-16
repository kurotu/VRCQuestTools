using System.Linq;
using KRT.VRCQuestTools.Models;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase;

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// Component to configure avatar conversion.
    /// </summary>
    [AddComponentMenu("VRCQuestTools/VQT Avatar Converter Settings")]
    [HelpURL("https://kurotu.github.io/VRCQuestTools/docs/references/components/avatar-converter-settings?lang=auto")]
    public class AvatarConverterSettings : VRCQuestToolsEditorOnly, IMaterialConversionComponent
    {
        /// <summary>
        /// Default material convert setting.
        /// </summary>
        [SerializeReference]
        public IMaterialConvertSettings defaultMaterialConvertSettings =
            ToonStandardConvertSettings.SimpleFeatures;

        /// <summary>
        /// Additional material convert settings.
        /// </summary>
        [SerializeReference]
        public AdditionalMaterialConvertSettings[] additionalMaterialConvertSettings = { };

        /// <summary>
        /// Whether to remove avatar dynamics components.
        /// </summary>
        [SerializeField]
        public bool removeAvatarDynamics = true;

        /// <summary>
        /// PhysBones to keep while conversion.
        /// </summary>
        [System.Obsolete("Use PlatformComponentRemover on the avatar dynamics component's GameObject instead.")]
        [SerializeField]
        public VRCPhysBone[] physBonesToKeep = { };

        /// <summary>
        /// PhysBone colliders to keep while conversion.
        /// </summary>
        [System.Obsolete("Use PlatformComponentRemover on the avatar dynamics component's GameObject instead.")]
        [SerializeField]
        public VRCPhysBoneCollider[] physBoneCollidersToKeep = { };

        /// <summary>
        /// Contact senders and receivers to keep while conversion.
        /// </summary>
        [System.Obsolete("Use PlatformComponentRemover on the avatar dynamics component's GameObject instead.")]
        [SerializeField]
        public ContactBase[] contactsToKeep = { };

        /// <summary>
        /// Animator override controllers to apply while conversion.
        /// </summary>
        [SerializeField]
        [NonReorderable] // somehow reorderable list doesn't work well
        public AnimatorOverrideController[] animatorOverrideControllers = { };

        /// <summary>
        /// Whether to remove vertex color.
        /// </summary>
        [SerializeField]
        public bool removeVertexColor = true;

        /// <summary>
        /// Whether to remove redundant material slots that are greater than the number of submeshes.
        /// </summary>
        [SerializeField]
        public bool removeExtraMaterialSlots = true;

        /// <summary>
        /// Whether to compress existing expressions menu icons if they'are uncompressed.
        /// </summary>
        [SerializeField]
        public bool compressExpressionsMenuIcons = true;

        /// <summary>
        /// Whether to assign network IDs to PhysBones during build.
        /// </summary>
        [SerializeField]
        public bool assignNetworkIds = true;

        /// <summary>
        /// NDMF phase to convert the avatar.
        /// </summary>
        [SerializeField]
        public AvatarConverterNdmfPhase ndmfPhase = AvatarConverterNdmfPhase.Auto;

        /// <summary>
        /// Whether to enable material preview in the editor.
        /// </summary>
        [SerializeField]
        public bool enableMaterialPreview = true;

        /// <summary>
        /// Gets avatar descriptor of the avatar root object.
        /// </summary>
        public VRC_AvatarDescriptor AvatarDescriptor => gameObject.GetComponent<VRC_AvatarDescriptor>();

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
        public bool IsPrimaryRoot => true;

        /// <inheritdoc/>
        public bool EnableMaterialPreview => enableMaterialPreview;

        /// <summary>
        /// Gets a value indicating whether any legacy avatar dynamics settings (<see cref="physBonesToKeep"/>, <see cref="physBoneCollidersToKeep"/>, or <see cref="contactsToKeep"/>) contain non-null entries.
        /// </summary>
#pragma warning disable CS0618
        public bool HasLegacyAvatarDynamicsSettings =>
            physBonesToKeep.Any(x => x != null) ||
            physBoneCollidersToKeep.Any(x => x != null) ||
            contactsToKeep.Any(x => x != null);
#pragma warning restore CS0618

        /// <summary>
        /// Gets the material convert settings for the specified material.
        /// </summary>
        /// <param name="material">Material to convert.</param>
        /// <returns>Resolved IMaterialConvertSettings.</returns>
        public IMaterialConvertSettings GetMaterialConvertSettings(Material material)
        {
            foreach (var setting in additionalMaterialConvertSettings)
            {
                if (setting.targetMaterial == material)
                {
                    return setting.materialConvertSettings;
                }
            }
            return defaultMaterialConvertSettings;
        }

        private void Reset()
        {
            defaultMaterialConvertSettings.LoadDefaultAssets();
        }
    }
}
