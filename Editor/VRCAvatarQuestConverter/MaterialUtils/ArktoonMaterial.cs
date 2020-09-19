using System.Collections;
using System.Collections.Generic;
using ImageMagick;
using UnityEditor;
using UnityEngine;

namespace KRTQuestTools
{
    public class ArktoonMaterial : GenericMaterial
    {
        internal ArktoonMaterial(Material material) : base(material) { }

        public override bool HasEmission()
        {
            return true;
        }

        public override MagickImage CompositeLayers()
        {
            var image = base.CompositeLayers();
            if (HasEmissiveFreak())
            {
                for (var i = 0; i < 2; i++)
                {
                    using (var ef = GetEmissiveFreakLayer(i))
                    using (var efImage = ef.GetMagickImage())
                    {
                        efImage.Resize(image.Width, image.Height);
                        image.Composite(efImage, CompositeOperator.Screen);
                    }
                }
            }
            return image;
        }

        private bool HasEmissiveFreak()
        {
            return material.shader.name.Contains("/EmissiveFreak/");
        }

        private Layer GetEmissiveFreakLayer(int index)
        {
            var num = index + 1;
            return new Layer
            {
                image = MaterialUtils.GetMagickImage(material, $"_EmissiveFreak{num}Tex"),
                color = material.GetColor($"_EmissiveFreak{num}Color")
            };
        }
    }
}
