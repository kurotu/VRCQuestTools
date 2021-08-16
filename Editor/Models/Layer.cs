// <copyright file="Layer.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using ImageMagick;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Represents a set of image and color.
    /// </summary>
    internal class Layer : IDisposable
    {
        /// <summary>
        /// Image.
        /// </summary>
        internal MagickImage image = null;

        /// <summary>
        /// Color which should be multiplied.
        /// </summary>
        internal Color color = Color.white;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (image != null && !image.IsDisposed)
            {
                image.Dispose();
            }
        }

        /// <summary>
        /// Get as MagickImage.
        /// </summary>
        /// <returns>Composed MagickImage.</returns>
        internal MagickImage GetMagickImage()
        {
            if (image == null)
            {
                return new MagickImage(MagickImageUtility.GetMagickColor(color), 1, 1);
            }
            return MagickImageUtility.Multiply(image, color);
        }
    }
}
