using System.Collections;
using System.Collections.Generic;
using ImageMagick;
using UnityEditor;
using UnityEngine;

namespace KRTQuestTools
{
    public class UTS2Material : GenericMaterial
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
