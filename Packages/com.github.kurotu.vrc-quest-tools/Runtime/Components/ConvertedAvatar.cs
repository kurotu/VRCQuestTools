using UnityEngine;
using VRC.SDKBase;

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// Component to mark converted avatar.
    /// </summary>
    [AddComponentMenu("VRCQuestTools/VQT Converted Avatar")]
    [DisallowMultipleComponent]
    [HelpURL("https://kurotu.github.io/VRCQuestTools/docs/references/components/converted-avatar?lang=auto")]
    public class ConvertedAvatar : VRCQuestToolsEditorOnly
    {
        /// <summary>
        /// Serialized schema version for forward compatibility.
        /// </summary>
        [SerializeField]
        private int serializedVersion = 1;
    }
}
