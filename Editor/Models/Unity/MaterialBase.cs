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
        /// Shader categories.
        /// </summary>
        internal enum ShaderCategory
        {
#pragma warning disable SA1136 // Enum values should be on separate lines
#pragma warning disable SA1602 // Enumeration items should be documented
            UTS2, Arktoon, Standard, Unlit, Quest, Sunao, Unverified,
#pragma warning restore SA1602 // Enumeration items should be documented
#pragma warning restore SA1136 // Enum values should be on separate lines
        }

        /// <summary>
        /// Create an instance of appropriate MaterialBase object by shader category.
        /// </summary>
        /// <param name="material">Material.</param>
        /// <returns>Material wrapper object.</returns>
        internal static MaterialBase Create(Material material)
        {
            switch (DetectShaderCategory(material))
            {
                case ShaderCategory.UTS2:
                    return new UTS2Material(material);
                case ShaderCategory.Arktoon:
                    return new ArktoonMaterial(material);
                case ShaderCategory.Sunao:
                    return new SunaoMaterial(material);
                default:
                    return new StandardMaterial(material);
            }
        }

        /// <summary>
        /// Detects shader category for a material.
        /// </summary>
        /// <param name="material">Material.</param>
        /// <returns>Detected shader category.</returns>
        internal static ShaderCategory DetectShaderCategory(Material material)
        {
            var shaderName = material.shader.name;
            if (shaderName == "Standard" || shaderName == "Standard (Specular setup)" || shaderName.StartsWith("Standard/"))
            {
                return ShaderCategory.Standard;
            }
            if (shaderName.StartsWith("UnityChanToonShader"))
            {
                return ShaderCategory.UTS2;
            }
            if (shaderName.StartsWith("arktoon/"))
            {
                return ShaderCategory.Arktoon;
            }
            if (shaderName.StartsWith("Unlit/"))
            {
                return ShaderCategory.Unlit;
            }
            if (shaderName.StartsWith("VRChat/Mobile/"))
            {
                return ShaderCategory.Quest;
            }
            if (shaderName.StartsWith("Sunao Shader/"))
            {
                return ShaderCategory.Sunao;
            }
            return ShaderCategory.Unverified;
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
                mainTexture = Material.mainTexture,
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
        internal abstract MagickImage GenerateToonLitImage();
    }
}
