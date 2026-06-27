using UnityEngine;
using VRC.SDKBase;

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// Base class for editor only components.
    /// </summary>
    [Icon("Packages/com.github.kurotu.vrc-quest-tools/Runtime/Images/VRCQuestTools-Icon.png")]
    public abstract class VRCQuestToolsEditorOnly : MonoBehaviour, IEditorOnly
    {
    }
}
