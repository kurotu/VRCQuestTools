using KRT.VRCQuestTools.Models;
using UnityEngine;

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// NetworkIdAssigner assigns network IDs to physbones which don't have ID.
    /// This would be useful to keep physbones to be synchronized between PC and Android.
    /// </summary>
    [AddComponentMenu("VRCQuestTools/VQT Network ID Assigner")]
    [HelpURL("https://kurotu.github.io/VRCQuestTools/docs/references/components/network-id-assigner?lang=auto")]
    [DisallowMultipleComponent]
    public class NetworkIDAssigner : VRCQuestToolsEditorOnly
    {
        // no members.
    }
}
