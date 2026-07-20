using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models.VRChat;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Prepare the avatar with AvatarConverterSettings component in NDMF.
    /// </summary>
    internal class AvatarConverterResolvingPass : Pass<AvatarConverterResolvingPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Prepare converter and build target";

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
                Logger.LogWarning($"No VRCAvatarDescriptor found in the avatar root object. Skipping avatar conversion preparation.");
                return;
            }

            var converterSettings = avatarDescriptor.GetComponent<AvatarConverterSettings>();
            if (converterSettings == null)
            {
                return;
            }

            var buildTarget = NdmfHelper.ResolveBuildTarget(context.AvatarRootObject);
            var avatar = new VRChatAvatar(avatarDescriptor);
            switch (buildTarget)
            {
                case Models.BuildTarget.PC:
                    // do nothing
                    break;
                case Models.BuildTarget.Android:
                    VRCQuestTools.AvatarConverter.PrepareConvertForQuestInPlace(avatar);
                    VRCQuestTools.AvatarConverter.PrepareModularAvatarComponentsInPlace(avatar);
                    break;
                default:
                    throw new System.InvalidProgramException($"Unsupported build target: {buildTarget}");
            }
        }
    }
}
