// <copyright file="PoiyomiMaterial.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Represents Poiyomi shader material.
    /// </summary>
    internal class PoiyomiMaterial : StandardMaterial
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PoiyomiMaterial"/> class.
        /// </summary>
        /// <param name="material">Material.</param>
        internal PoiyomiMaterial(Material material)
            : base(material)
        {
        }

        /// <inheritdoc/>
        internal override Shader ToonLitBakeShader => Shader.Find("Hidden/VRCQuestTools/Poiyomi");

        /// <inheritdoc/>
        internal override Vector2 MainTextureScale
        {
            get
            {
                var st = GetMainTexST();
                return new Vector2(st.x, st.y);
            }
        }

        /// <inheritdoc/>
        internal override Vector2 MainTextureOffset
        {
            get
            {
                var st = GetMainTexST();
                return new Vector2(st.z, st.w);
            }
        }

        /// <summary>
        /// Gets the main texture scale and offset as a Vector4 from _MainTex_ST property.
        /// Poiyomi shaders may store UV tiling as a separate Vector4 property rather than
        /// using Unity's standard texture scale/offset metadata.
        /// </summary>
        /// <returns>Vector4 with (scaleX, scaleY, offsetX, offsetY).</returns>
        private Vector4 GetMainTexST()
        {
            if (Material.HasProperty("_MainTex_ST"))
            {
                return Material.GetVector("_MainTex_ST");
            }

            var scale = Material.mainTextureScale;
            var offset = Material.mainTextureOffset;
            return new Vector4(scale.x, scale.y, offset.x, offset.y);
        }
    }
}
