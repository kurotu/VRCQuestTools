// <copyright file="UTS2Material.cs" company="kurotu">
// Copyright (c) kurotu.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

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
