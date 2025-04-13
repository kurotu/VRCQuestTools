
using UnityEngine;
using VRC.SDKBase;

namespace KRT.VRCQuestTools.Components
{
    [AddComponentMenu("VRCQuestTools/VQT Avatar Proxy")]
    [DisallowMultipleComponent]
    [HelpURL("https://kurotu.github.io/VRCQuestTools/docs/references/components/avatar-proxy?lang=auto")]
    public class AvatarProxy : VRCQuestToolsEditorOnly, IPlatformDependentComponent
    {
        /// <summary>
        /// The avatar descriptor to use as a proxy.
        /// </summary>
        public VRC_AvatarDescriptor mobileAvatar;
    }
}
