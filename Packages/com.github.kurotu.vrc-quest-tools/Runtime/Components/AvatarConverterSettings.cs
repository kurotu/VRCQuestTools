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
        /// Destination avatar.
        /// </summary>
        [SerializeField]
        public VRC_AvatarDescriptor destinationAvatar;

        /// <summary>
        /// Default material convert setting. The default value is <see cref="ToonLitConvertSettings"/>.
        /// </summary>
        [SerializeReference]
        public IMaterialConvertSettings defaultMaterialConvertSetting = new ToonLitConvertSettings();

        /// <summary>
        /// Additional material convert settings.
        /// </summary>
        [SerializeField]
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
        /// Whether to remove existing avatar game object: destinationAvatar.
        /// </summary>
        [SerializeField]
        public bool overwriteDestinationAvatar = true;

        /// <summary>
        /// Gets avatar descriptor of the avatar root object.
        /// </summary>
        public VRC_AvatarDescriptor AvatarDescriptor => gameObject.GetComponent<VRC_AvatarDescriptor>();

        private void Reset()
        {
            var descriptor = AvatarDescriptor;
            physBonesToKeep = descriptor ? descriptor.gameObject.GetComponentsInChildren<VRCPhysBone>() : new VRCPhysBone[] { };
            physBoneCollidersToKeep = descriptor ? descriptor.gameObject.GetComponentsInChildren<VRCPhysBoneCollider>() : new VRCPhysBoneCollider[] { };
            contactsToKeep = descriptor ? descriptor.gameObject.GetComponentsInChildren<ContactBase>() : new ContactBase[] { };
        }

        private void OnValidate()
        {
            var e = Event.current;
            if (e != null && e.type != EventType.ExecuteCommand && e.commandName != "Duplicate")
            {
                destinationAvatar = null;
            }

            if (destinationAvatar != null && destinationAvatar == AvatarDescriptor)
            {
                destinationAvatar = null;
            }
        }
    }
}
