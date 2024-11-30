// <copyright file="ArktoonMaterial.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Represents arctoon-Shaders material.
    /// </summary>
    internal class ArktoonMaterial : StandardMaterial
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArktoonMaterial"/> class.
        /// </summary>
        /// <param name="material">Material.</param>
        internal ArktoonMaterial(Material material)
            : base(material)
        {
        }

        /// <inheritdoc/>
        internal override Shader ToonLitBakeShader => Material.shader.name.Contains("EmissiveFreak/")
            ? Shader.Find("Hidden/VRCQuestTools/arktoon/EmissiveFreak")
            : Shader.Find("Hidden/VRCQuestTools/arktoon/Opaque");
    }
}
