// <copyright file="VirtualLens2Material.cs" company="kurotu">
// Copyright (c) kurotu.
// See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using KRT.VRCQuestTools.Utils;
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
        internal override AsyncCallbackRequest GenerateToonLitImage(IToonLitConvertSettings settings, Action<Texture2D> completion)
        {
            if (Material.shader.name.ToLower().Contains("/UnlitPreview".ToLower()))
            {
                var newTex = new Texture2D(Texture2D.blackTexture.width, Texture2D.blackTexture.height);
                newTex.SetPixels32(Texture2D.blackTexture.GetPixels32());
                return new ResultRequest<Texture2D>(newTex, completion);
            }
            else
            {
                return new ResultRequest<Texture2D>(null, completion);
            }
        }
    }
}
