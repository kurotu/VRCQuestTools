// <copyright file="UTS2Material.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Utils;
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
        protected override bool HasEmission()
        {
            return true;
        }

        /// <inheritdoc/>
        protected override Layer GetEmissionLayer()
        {
            return new Layer
            {
                image = MagickImageUtility.GetMagickImage(Material.GetTexture("_Emissive_Tex")),
                color = Material.GetColor("_Emissive_Color"),
            };
        }
    }
}
