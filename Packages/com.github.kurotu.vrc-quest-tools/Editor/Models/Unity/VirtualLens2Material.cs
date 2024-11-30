// <copyright file="VirtualLens2Material.cs" company="kurotu">
// Copyright (c) kurotu.
// See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// VirtualLens2 materials.
    /// </summary>
    internal class VirtualLens2Material : MaterialBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualLens2Material"/> class.
        /// </summary>
        /// <param name="material">Material to wrap.</param>
        internal VirtualLens2Material(Material material)
            : base(material)
        {
        }

        /// <inheritdoc/>
        internal override Shader ToonLitBakeShader => null;

        /// <inheritdoc/>
        internal override Shader StandardLiteMainBakeShader => null;

        /// <inheritdoc/>
        internal override Shader StandardLiteMetallicSmoothnessBakeShader => null;

        /// <inheritdoc/>
        internal override Texture2D GenerateToonLitImage(IToonLitConvertSettings settings)
        {
            return GenerateTexture();
        }

        /// <inheritdoc/>
        internal override Texture2D GenerateStandardLiteMainImage(StandardLiteConvertSettings settings)
        {
            return GenerateTexture();
        }

        private Texture2D GenerateTexture()
        {
            if (Material.shader.name.ToLower().Contains("/UnlitPreview".ToLower()))
            {
                return Texture2D.blackTexture;
            }
            else
            {
                return null;
            }
        }
    }
}
