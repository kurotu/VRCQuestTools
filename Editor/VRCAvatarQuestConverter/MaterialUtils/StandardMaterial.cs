// <copyright file="StandardMaterial.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using ImageMagick;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    public class StandardMaterial : MaterialWrapper
    {
        protected readonly Material material;

        internal StandardMaterial(Material material)
        {
            this.material = material;
        }

        public virtual Layer GetMainLayer()
        {
            return new Layer
            {
                image = MaterialUtils.GetMagickImage(material.mainTexture),
                color = material.HasProperty("_Color") ? material.color : Color.white
            };
        }

        public virtual bool HasEmission()
        {
            return material.shaderKeywords.Contains("_EMISSION");
        }

        public virtual Layer GetEmissionLayer()
        {
            if (!HasEmission()) return null;
            return new Layer
            {
                image = MaterialUtils.GetMagickImage(material, "_EmissionMap"),
                color = material.GetColor("_EmissionColor")
            };
        }

        public override MagickImage CompositeLayers()
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

        private Tuple<int, int> DecideCompositionSize(Layer main, Layer emission)
        {
            var layers = new List<Layer>
            {
                main,
                emission
            };
            foreach (var l in layers)
            {
                if (l == null) continue;
                if (l.image != null)
                {
                    return new Tuple<int, int>(l.image.Width, l.image.Height);
                }
            }
            return new Tuple<int, int>(1, 1);
        }
    }
}
