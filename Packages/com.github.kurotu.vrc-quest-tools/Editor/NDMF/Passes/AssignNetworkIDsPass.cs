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

        /// <summary>
        /// Runs this pass directly for EditMode tests, bypassing the NDMF pass pipeline.
        /// </summary>
        /// <param name="context">BuildContext.</param>
        internal void RunForTest(BuildContext context)
        {
            Execute(context);
        }

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            var avatarDescriptor = context.AvatarRootObject.GetComponent<VRCAvatarDescriptor>();
            if (avatarDescriptor == null)
            {
                Logger.LogWarning($"No VRCAvatarDescriptor found in the avatar root object. Skipping network ID assignment.");
                return;
            }

            var assigner = avatarDescriptor.GetComponent<NetworkIDAssigner>();
            var settings = avatarDescriptor.GetComponent<AvatarConverterSettings>();
            var shouldAssignNetworkIds = assigner != null || (settings != null && settings.assignNetworkIds);
            if (!shouldAssignNetworkIds)
            {
                if (settings != null && VRCSDKUtility.HasMissingNetworkIds(avatarDescriptor))
                {
                    NdmfErrorReport.ReportError(new MissingNetworkIDAssignerWarning());
                }
                return;
            }

            VRCSDKUtility.AssignNetworkIdsToPhysBonesByHierarchyHash(avatarDescriptor);
        }
    }
}
