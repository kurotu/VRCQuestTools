// <copyright file="UTS2Material.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Represents UnityChanToonShader material.
    /// </summary>
    internal class UTS2Material : StandardMaterial
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UTS2Material"/> class.
        /// </summary>
        /// <param name="material">Material.</param>
        internal UTS2Material(Material material)
            : base(material)
        {
        }

        /// <inheritdoc/>
        internal override Shader ToonLitBakeShader => Shader.Find("Hidden/VRCQuestTools/UTS2");
    }
}
