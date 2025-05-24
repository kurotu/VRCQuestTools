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
        /// Whether to remove avatar dynamics components.
        /// </summary>
        [SerializeField]
        public bool removeAvatarDynamics = true;

        /// <summary>
        /// PhysBones to keep while conversion.
        /// </summary>
        [SerializeField]
        public VRCPhysBone[] physBonesToKeep = { };

        /// <summary>
        /// PhysBone colliders to keep while conversion.
        /// </summary>
        [SerializeField]
        public VRCPhysBoneCollider[] physBoneCollidersToKeep = { };

        /// <summary>
        /// Contact senders and receivers to keep while conversion.
        /// </summary>
        [SerializeField]
        public ContactBase[] contactsToKeep = { };

        /// <summary>
        /// Animator override controllers to apply while conversion.
        /// </summary>
        [SerializeField]
#if UNITY_2020_2_OR_NEWER
        [NonReorderable] // somehow reorderable list doesn't work well
#endif
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
        /// NDMF phase to convert the avatar.
        /// </summary>
        [SerializeField]
        public AvatarConverterNdmfPhase ndmfPhase = AvatarConverterNdmfPhase.Auto;

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

            var descriptor = AvatarDescriptor;
            physBonesToKeep = descriptor ? descriptor.gameObject.GetComponentsInChildren<VRCPhysBone>(true) : new VRCPhysBone[] { };
            physBoneCollidersToKeep = descriptor ? descriptor.gameObject.GetComponentsInChildren<VRCPhysBoneCollider>(true) : new VRCPhysBoneCollider[] { };
            contactsToKeep = descriptor ? descriptor.GetComponentsInChildren<ContactBase>(true)
                .Where(c =>
                {
                    switch (c)
                    {
#if VQT_HAS_VRCSDK_LOCAL_CONTACT_RECEIVER
                        case ContactReceiver receiver:
                            return !receiver.IsLocalOnly;
#endif
#if VQT_HAS_VRCSDK_LOCAL_CONTACT_SENDER
                        case ContactSender sender:
                            return !sender.IsLocalOnly;
#endif
                        default:
                            return true;
                    }
                })
                .ToArray()
                : new ContactBase[] { };
        }
    }
}
