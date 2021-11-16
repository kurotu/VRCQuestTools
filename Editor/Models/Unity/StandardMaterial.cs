// <copyright file="StandardMaterial.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using ImageMagick;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Represents Unity Standard shader material.
    /// </summary>
    internal class StandardMaterial : MaterialBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StandardMaterial"/> class.
        /// </summary>
        /// <param name="material">Material.</param>
        internal StandardMaterial(Material material)
            : base(material)
        {
        }

        /// <inheritdoc/>
        internal override MagickImage GenerateToonLitImage()
        {
            using (var main = GetMainLayer())
            using (var emission = GetEmissionLayer())
            {
                var (width, height) = DecideCompositionSize(main, emission);
                var newImage = new MagickImage(MagickColors.Black, width, height);
                using (var mainImage = main.GetMagickImage())
                {
                    mainImage.HasAlpha = false;
                    mainImage.Resize(width, height);
                    newImage.Composite(mainImage, CompositeOperator.Plus);
                }
                if (HasEmission())
                {
                    using (var emissionImage = emission.GetMagickImage())
                    {
                        emissionImage.Resize(width, height);
                        emissionImage.HasAlpha = false;
                        newImage.Composite(emissionImage, CompositeOperator.Screen);
                    }
                }
                if (main.image != null && main.image.HasAlpha)
                {
                    newImage.HasAlpha = true;
                    newImage.CopyPixels(main.image, Channels.Alpha);
                }
                return newImage;
            }
        }

        /// <summary>
        /// Whether the material uses emission.
        /// </summary>
        /// <returns>true when using emission.</returns>
        protected virtual bool HasEmission()
        {
            return Material.shaderKeywords.Contains("_EMISSION");
        }

        /// <summary>
        /// Return emission layer.
        /// </summary>
        /// <returns>Emission layer.</returns>
        protected virtual Layer GetEmissionLayer()
        {
            if (!HasEmission())
            {
                return null;
            }

            return new Layer
            {
                image = MagickImageUtility.GetMagickImage(Material.GetTexture("_EmissionMap")),
                color = Material.GetColor("_EmissionColor"),
            };
        }

        private Layer GetMainLayer()
        {
            return new Layer
            {
                // mainTexture may return null in some cases (e.g. After upgrading lilToon)
                image = MagickImageUtility.GetMagickImage(Material.mainTexture ?? Material.GetTexture("_MainTex")),
                color = Material.HasProperty("_Color") ? Material.color : Color.white,
            };
        }

        private Tuple<int, int> DecideCompositionSize(Layer main, Layer emission)
        {
            var layers = new List<Layer>
            {
                main,
                emission,
            };
            foreach (var l in layers)
            {
                if (l == null)
                {
                    continue;
                }

                if (l.image != null)
                {
                    return new Tuple<int, int>(l.image.Width, l.image.Height);
                }
            }
            return new Tuple<int, int>(1, 1);
        }
    }
}
