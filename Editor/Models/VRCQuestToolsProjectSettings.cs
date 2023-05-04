using System;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Settings which should be stored in ProjectSettings folder for source control.
    /// </summary>
    [Serializable]
    internal class VRCQuestToolsProjectSettings
    {
        /// <summary>
        /// Enable Auto Remove Vertex Colors.
        /// </summary>
        public bool AutoRemoveVertexColors = true;
    }
}
