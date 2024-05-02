using UnityEngine;
using VRC.SDKBase;

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// Base class for editor only components.
    /// </summary>
#if UNITY_2021_2_OR_NEWER
    [Icon("Packages/com.github.kurotu.vrc-quest-tools/Runtime/Images/VRCQuestTools-Icon.png")]
#endif
    public abstract class VRCQuestToolsEditorOnly : MonoBehaviour, IEditorOnly
    {
    }
}
