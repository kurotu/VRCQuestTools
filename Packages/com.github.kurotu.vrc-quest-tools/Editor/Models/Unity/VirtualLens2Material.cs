// <copyright file="VirtualLens2Material.cs" company="kurotu">
// Copyright (c) kurotu.
// See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Utils;
using System;
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
        internal override Shader BakeShader => null;

        /// <inheritdoc/>
        internal override TextureReadbackRequest GenerateToonLitImage(IToonLitConvertSettings settings, Action<Texture2D> completion)
        {
            if (Material.shader.name.ToLower().Contains("/UnlitPreview".ToLower()))
            {
                return new TextureResultReadbackRequest(Texture2D.blackTexture, completion);
            }
            else
            {
                return new TextureResultReadbackRequest(null, completion);
            }
        }
    }
}
