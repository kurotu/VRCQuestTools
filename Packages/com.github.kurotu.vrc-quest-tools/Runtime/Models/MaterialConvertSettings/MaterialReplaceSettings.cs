// <copyright file="MaterialReplaceSettings.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

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
#pragma warning disable CS0414
        private int serializedVersion = 1;
#pragma warning restore CS0414

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
            if (material == null)
            {
                return $"{nameof(MaterialReplaceSettings)}_null";
            }

#if UNITY_6000_4_OR_NEWER
            var materialId = EntityId.ToULong(material.GetEntityId());
#else
            var materialId = unchecked((uint)material.GetInstanceID());
#endif

            return $"{nameof(MaterialReplaceSettings)}_{materialId}_{AdditionalMaterialConvertSettings.GetContentCacheKey(material)}";
        }
    }
}
