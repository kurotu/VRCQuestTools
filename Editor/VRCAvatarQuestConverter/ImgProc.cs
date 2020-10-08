// <copyright file="ImgProc.cs" company="kurotu">
// Copyright (c) kurotu.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

using ImageMagick;
using UnityEngine;

namespace VRCQuestTools
{
    using QuantumType = System.UInt16;

    static class ImgProc
    {
        internal static MagickImage Multiply(MagickImage image, Color32 color)
        {
            return Multiply(image, GetMagickColor(color));
        }

        internal static MagickImage Multiply(MagickImage image, MagickColor color)
        {
            using (var colorTex = new MagickImage(color, image.Width, image.Height))
            {
                var newImage = new MagickImage(image);
                newImage.HasAlpha = false;
                newImage.Composite(colorTex, CompositeOperator.Multiply);
                if (image.HasAlpha)
                {
                    newImage.HasAlpha = true;
                    newImage.CopyPixels(image, Channels.Alpha);
                }
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
