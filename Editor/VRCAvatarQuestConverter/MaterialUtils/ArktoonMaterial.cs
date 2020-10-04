using ImageMagick;
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
            if (HasEmissiveFreak())
            {
                using (var baseImage = base.CompositeLayers())
                {
                    var image = new MagickImage(baseImage)
                    {
                        HasAlpha = false
                    };
                    for (var i = 0; i < 2; i++)
                    {
                        using (var ef = GetEmissiveFreakLayer(i))
                        using (var efImage = ef.GetMagickImage())
                        {
                            efImage.HasAlpha = false;
                            efImage.Resize(image.Width, image.Height);
                            image.Composite(efImage, CompositeOperator.Screen);
                        }
                    }
                    if (baseImage.HasAlpha)
                    {
                        image.HasAlpha = true;
                        image.CopyPixels(baseImage, Channels.Alpha);
                    }
                    return image;
                }
            }
            return base.CompositeLayers();
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
