﻿using UnityEngine;

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

        /// <inheritdoc/>
        public string GetCacheKey()
        {
            throw new System.InvalidProgramException("MaterialReplaceSettings doesn't generate textures.");
        }
    }
}
