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
    public class AvatarConverterSettings : MonoBehaviour, IEditorOnly
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
        public AnimatorOverrideController[] animatorOverrideControllers = { };

        /// <summary>
        /// Whether to remove vertex color.
        /// </summary>
        [SerializeField]
        public bool removeVertexColor = true;

        /// <summary>
        /// Gets avatar descriptor of the avatar root object.
        /// </summary>
        public VRC_AvatarDescriptor AvatarDescriptor => gameObject.GetComponent<VRC_AvatarDescriptor>();

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
            var descriptor = AvatarDescriptor;
            physBonesToKeep = descriptor ? descriptor.gameObject.GetComponentsInChildren<VRCPhysBone>() : new VRCPhysBone[] { };
            physBoneCollidersToKeep = descriptor ? descriptor.gameObject.GetComponentsInChildren<VRCPhysBoneCollider>() : new VRCPhysBoneCollider[] { };
            contactsToKeep = descriptor ? descriptor.gameObject.GetComponentsInChildren<ContactBase>() : new ContactBase[] { };
        }
    }
}
