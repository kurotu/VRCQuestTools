// <copyright file="UTS2Material.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEngine;

namespace KRT.VRCQuestTools
{
    public class UTS2Material : StandardMaterial
    {
        internal UTS2Material(Material material) : base(material) { }

        public override bool HasEmission()
        {
            return true;
        }

        public override Layer GetEmissionLayer()
        {
            return new Layer
            {
                image = MaterialUtils.GetMagickImage(material, "_Emissive_Tex"),
                color = material.GetColor("_Emissive_Color")
            };
        }

    }
}
