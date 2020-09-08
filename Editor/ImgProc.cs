using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ImageMagick;

namespace KRTQuestTools
{
    using QuantumType = System.UInt16;

    static class ImgProc
    {
        internal static MagickImage Multiply(MagickImage image, Color32 color)
        {
            using (var colorTex = new MagickImage(GetMagickColor(color), image.Width, image.Height))
            {
                var newImage = new MagickImage(image);
                newImage.Composite(colorTex, CompositeOperator.Multiply);
                return newImage;
            }
        }

        internal static MagickImage Add(MagickImage image0, MagickImage image1)
        {
            var newImage = new MagickImage(image0);
            newImage.Composite(image1, CompositeOperator.Plus);
            return newImage;
        }

        internal static MagickImage Screen(MagickImage image0, MagickImage image1)
        {
            var newImage = new MagickImage(image0);
            newImage.Composite(image1, CompositeOperator.Screen);
            return newImage;
        }

        internal static MagickColor GetMagickColor(Color32 color)
        {
            var r = GetMagickColor(color.r);
            var g = GetMagickColor(color.g);
            var b = GetMagickColor(color.b);
            return new MagickColor(r, g, b);
        }

        private static QuantumType GetMagickColor(byte color)
        {
            return (QuantumType)(QuantumType.MaxValue * color / byte.MaxValue);
        }
    }
}
