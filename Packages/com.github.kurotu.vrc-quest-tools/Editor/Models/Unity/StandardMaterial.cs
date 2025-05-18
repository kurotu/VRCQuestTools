// <copyright file="StandardMaterial.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Represents Unity Standard shader material.
    /// </summary>
    internal class StandardMaterial : MaterialBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StandardMaterial"/> class.
        /// </summary>
        /// <param name="material">Material.</param>
        internal StandardMaterial(Material material)
            : base(material)
        {
        }

        /// <inheritdoc/>
        internal override Shader ToonLitBakeShader => Shader.Find("Hidden/VRCQuestTools/Standard");
    }
}
