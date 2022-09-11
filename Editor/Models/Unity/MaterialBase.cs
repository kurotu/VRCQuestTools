// <copyright file="MaterialBase.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using ImageMagick;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Abstract class for material wrapper.
    /// </summary>
    internal abstract class MaterialBase
    {
        /// <summary>
        /// Internal material.
        /// </summary>
        protected readonly Material Material;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialBase"/> class.
        /// </summary>
        /// <param name="material">Material.</param>
        internal MaterialBase(Material material)
        {
            Material = material;
        }

        /// <summary>
        /// Convert internal material to Toon Lit.
        /// </summary>
        /// <returns>Converted material.</returns>
        internal Material ConvertToToonLit()
        {
            var newShader = Shader.Find("VRChat/Mobile/Toon Lit");
            return new Material(newShader)
            {
                color = Material.color,
                doubleSidedGI = Material.doubleSidedGI,
                enableInstancing = true, // https://docs.vrchat.com/docs/quest-content-optimization#avatars-and-worlds
                globalIlluminationFlags = Material.globalIlluminationFlags,
                hideFlags = Material.hideFlags,
                mainTexture = Material.mainTexture ?? Material.GetTexture("_MainTex"), // mainTexture may return null in some cases (e.g. After upgrading lilToon).
                mainTextureOffset = Material.mainTextureOffset,
                mainTextureScale = Material.mainTextureScale,
                name = $"{Material.name}_{newShader.name.Split('/').Last()}",
                renderQueue = Material.renderQueue,
                shader = newShader,
                shaderKeywords = null,
            };
        }

        /// <summary>
        /// Generates an image for Toon Lit main texture.
        /// </summary>
        /// <returns>Generated image.</returns>
        internal abstract Texture2D GenerateToonLitImage();
    }
}
