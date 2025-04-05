using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Utils;
using nadena.dev.ndmf;

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
            var assigner = context.AvatarRootObject.GetComponent<NetworkIDAssigner>();
            if (assigner == null)
            {
                if (context.AvatarRootObject.GetComponent<AvatarConverterSettings>() != null && VRCSDKUtility.HasMissingNetworkIds(context.AvatarDescriptor))
                {
                    NdmfErrorReport.ReportError(new MissingNetworkIDAssignerWarning());
                }
                return;
            }

            VRCSDKUtility.AssignNetworkIdsToPhysBonesByHierarchyHash(context.AvatarDescriptor);
        }
    }
}
