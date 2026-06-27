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

        /// <summary>
        /// Texture cache size in bytes.
        /// Default: 128MB.
        /// </summary>
        public ulong TextureCacheSize = 128 * 1024 * 1024;
    }
}
