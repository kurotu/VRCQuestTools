// <copyright file="ArktoonMaterial.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using ImageMagick;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Represents arctoon-Shaders material.
    /// </summary>
    internal class ArktoonMaterial : StandardMaterial
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArktoonMaterial"/> class.
        /// </summary>
        /// <param name="material">Material.</param>
        internal ArktoonMaterial(Material material)
            : base(material)
        {
        }

        /// <inheritdoc/>
        internal override Texture2D GenerateToonLitImage()
        {
            using (var image = GenerateToonLitMagickImage())
            {
                return MagickImageUtility.MagickImageToTexture2D(image);
            }
        }

        private MagickImage GenerateToonLitMagickImage()
        {
            if (HasEmissiveFreak())
            {
                using (var baseTexture = DisposableObject.New(base.GenerateToonLitImage()))
                using (var baseImage = new MagickImage(baseTexture.Object.EncodeToPNG()))
                {
                    var image = new MagickImage(baseImage)
                    {
                        HasAlpha = false,
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

            using (var baseTexture = DisposableObject.New(base.GenerateToonLitImage()))
            {
                return MagickImageUtility.Texture2DToMagickImage(baseTexture.Object);
            }
        }

        /// <inheritdoc/>
        protected override bool HasEmission()
        {
            return true;
        }

        private bool HasEmissiveFreak()
        {
            return Material.shader.name.Contains("EmissiveFreak/");
        }

        private Layer GetEmissiveFreakLayer(int index)
        {
            var num = index + 1;
            return new Layer
            {
                image = MagickImageUtility.GetMagickImage(Material.GetTexture($"_EmissiveFreak{num}Tex")),
                color = Material.GetColor($"_EmissiveFreak{num}Color"),
            };
        }
    }
}
