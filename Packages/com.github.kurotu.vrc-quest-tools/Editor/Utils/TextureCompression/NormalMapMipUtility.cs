// <copyright file="NormalMapMipUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Pure CPU downsampling for tangent-space normal map mip generation, used by
    /// <see cref="AstcencTextureCompressor"/>'s normal map path (astcenc itself only encodes; it does not
    /// generate mipmaps). A plain box filter over the encoded [0, 255] bytes would shrink the resulting
    /// vector's length below 1 wherever the four source normals disagree, which reads as flattened / duller
    /// shading at a distance. This instead decodes each texel to a signed vector, averages in that linear
    /// vector space, and re-normalizes, mirroring how Unity's own normal map mip generation behaves
    /// (mip0/mip1 both average very close to unit length there too).
    /// </summary>
    internal static class NormalMapMipUtility
    {
        /// <summary>
        /// Produces one half-resolution mip level from <paramref name="pixels"/> by averaging each 2x2 block of
        /// decoded normal vectors and re-normalizing the result. At odd dimensions the last row/column of source
        /// texels is dropped (output size is a floor division, matching how texture mip chains are sized), and at
        /// the tail of a mip chain (source width or height already 1) the sampling collapses to 1x2, 2x1, or 1x1
        /// by clamping the second sample of the affected axis to the first.
        /// </summary>
        /// <param name="pixels">Source pixels; RGB is the encoded normal ((v + 1) / 2 * 255 per channel). Alpha is ignored on input.</param>
        /// <param name="width">Source width in pixels.</param>
        /// <param name="height">Source height in pixels.</param>
        /// <param name="newWidth">Output width: <c>max(1, width / 2)</c>.</param>
        /// <param name="newHeight">Output height: <c>max(1, height / 2)</c>.</param>
        /// <returns>Downsampled, re-normalized pixels of size <paramref name="newWidth"/> x <paramref name="newHeight"/>. Alpha is always 255.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pixels"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="pixels"/>.Length does not match width * height.</exception>
        internal static Color32[] DownsampleNormalMap(Color32[] pixels, int width, int height, out int newWidth, out int newHeight)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }
            if (pixels.Length != width * height)
            {
                throw new ArgumentException($"Pixel count {pixels.Length} does not match {width}x{height}", nameof(pixels));
            }

            newWidth = Math.Max(1, width >> 1);
            newHeight = Math.Max(1, height >> 1);
            var result = new Color32[newWidth * newHeight];

            for (var y = 0; y < newHeight; y++)
            {
                var srcY0 = Math.Min(height - 1, y * 2);
                var srcY1 = Math.Min(height - 1, (y * 2) + 1);
                var rowOffset0 = srcY0 * width;
                var rowOffset1 = srcY1 * width;
                for (var x = 0; x < newWidth; x++)
                {
                    var srcX0 = Math.Min(width - 1, x * 2);
                    var srcX1 = Math.Min(width - 1, (x * 2) + 1);

                    var sum = Decode(pixels[rowOffset0 + srcX0])
                        + Decode(pixels[rowOffset0 + srcX1])
                        + Decode(pixels[rowOffset1 + srcX0])
                        + Decode(pixels[rowOffset1 + srcX1]);

                    var normal = sum.sqrMagnitude > 0f ? sum.normalized : new Vector3(0f, 0f, 1f);
                    result[(y * newWidth) + x] = Encode(normal);
                }
            }
            return result;
        }

        private static Vector3 Decode(Color32 c)
        {
            return new Vector3(((c.r * 2f) / 255f) - 1f, ((c.g * 2f) / 255f) - 1f, ((c.b * 2f) / 255f) - 1f);
        }

        private static Color32 Encode(Vector3 normal)
        {
            var r = (byte)Mathf.Clamp(Mathf.RoundToInt(((normal.x + 1f) / 2f) * 255f), 0, 255);
            var g = (byte)Mathf.Clamp(Mathf.RoundToInt(((normal.y + 1f) / 2f) * 255f), 0, 255);
            var b = (byte)Mathf.Clamp(Mathf.RoundToInt(((normal.z + 1f) / 2f) * 255f), 0, 255);
            return new Color32(r, g, b, 255);
        }
    }
}
