using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Settings for Material Replacement.
    /// </summary>
    public class MaterialReplaceSettings : IMaterialConvertSettings
    {
        /// <summary>
        /// Material to replace.
        /// </summary>
        [SerializeField]
        public Material material;

        /// <summary>Serialized schema version for forward compatibility.</summary>
        [SerializeField]
        private int serializedVersion = 1;

        /// <inheritdoc/>
        public MobileTextureFormat MobileTextureFormat => throw new System.InvalidProgramException("MaterialReplaceSettings doesn't generate textures.");

        /// <inheritdoc/>
        public void LoadDefaultAssets()
        {
            // nothing.
        }

        /// <inheritdoc/>
        public string GetCacheKey()
        {
            throw new System.InvalidProgramException("MaterialReplaceSettings doesn't generate textures.");
        }
    }
}
