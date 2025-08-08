using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Utils;
using nadena.dev.ndmf;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Assign network IDs to the avatar.
    /// </summary>
    internal class AssignNetworkIDsPass : Pass<AssignNetworkIDsPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Assign network IDs";

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            var avatarDescriptor = context.AvatarRootObject.GetComponent<VRCAvatarDescriptor>();
            if (avatarDescriptor == null)
            {
                Debug.LogWarning($"[{VRCQuestTools.Name}] No VRCAvatarDescriptor found in the avatar root object. Skipping network ID assignment.");
                return;
            }

            var assigner = avatarDescriptor.GetComponent<NetworkIDAssigner>();
            if (assigner == null)
            {
                if (avatarDescriptor.GetComponent<AvatarConverterSettings>() != null && VRCSDKUtility.HasMissingNetworkIds(avatarDescriptor))
                {
                    NdmfErrorReport.ReportError(new MissingNetworkIDAssignerWarning());
                }
                return;
            }

            VRCSDKUtility.AssignNetworkIdsToPhysBonesByHierarchyHash(avatarDescriptor);
        }
    }
}
