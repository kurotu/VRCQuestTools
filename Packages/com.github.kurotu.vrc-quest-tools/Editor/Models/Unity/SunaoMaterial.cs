// <copyright file="SunaoMaterial.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Sunao Shader material.
    /// </summary>
    internal class SunaoMaterial : MaterialBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SunaoMaterial"/> class.
        /// </summary>
        /// <param name="material">Material.</param>
        internal SunaoMaterial(Material material)
            : base(material)
        {
        }

        /// <inheritdoc/>
        internal override Shader ToonLitBakeShader => Shader.Find("Hidden/VRCQuestTools/Sunao");
    }
}
