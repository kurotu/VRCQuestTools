// <copyright file="NormalMapMipUtilityTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Tests for NormalMapMipUtility's pure downsample/re-normalize logic. Independent of astcenc availability
    /// (no process is spawned), unlike the rest of the astcenc test suite.
    /// </summary>
    public class NormalMapMipUtilityTests
    {
        /// <summary>
        /// Verifies that a 2x2 block averages in decoded vector space and re-normalizes: two opposing X normals
        /// cancel out, leaving the average of the two (0,0,1) normals, itself already unit length.
        /// </summary>
        [Test]
        public void DownsampleNormalMap_OpposingXNormals_AveragesAndNormalizes()
        {
            var pixels = new[]
            {
                EncodeNormal(new Vector3(1f, 0f, 0f)),
                EncodeNormal(new Vector3(-1f, 0f, 0f)),
                EncodeNormal(new Vector3(0f, 0f, 1f)),
                EncodeNormal(new Vector3(0f, 0f, 1f)),
            };

            var result = NormalMapMipUtility.DownsampleNormalMap(pixels, 2, 2, out var newWidth, out var newHeight);

            Assert.AreEqual(1, newWidth);
            Assert.AreEqual(1, newHeight);
            Assert.AreEqual(1, result.Length);

            var decoded = DecodeNormal(result[0]);
            Assert.Less(Vector3.Distance(decoded, new Vector3(0f, 0f, 1f)), 0.02f, $"Expected (0,0,1) but got {decoded}.");
            Assert.AreEqual(255, (int)result[0].a);
        }

        /// <summary>
        /// Verifies the zero-length guard: when a block's decoded vectors sum to exactly zero (no dominant
        /// direction), the result falls back to the flat (0,0,1) normal instead of producing a NaN from
        /// normalizing a zero vector. Pairs are constructed as exact byte complements (channel value v paired
        /// with 255 - v) rather than via <see cref="EncodeNormal"/> on opposing unit vectors: decode(v) and
        /// decode(255 - v) are exact negatives of each other algebraically, whereas independently encoding e.g.
        /// (1,0,0) and (-1,0,0) both quantize their zero components to the same rounded byte (127.5 always rounds
        /// to 128), which does not cancel and defeated an earlier version of this test.
        /// </summary>
        [Test]
        public void DownsampleNormalMap_ZeroSumBlock_FallsBackToFlatNormal()
        {
            var pixels = new[]
            {
                new Color32(200, 50, 10, 255),
                new Color32(55, 205, 245, 255), // 255 - (200, 50, 10): cancels pixel 0 exactly.
                new Color32(80, 180, 30, 255),
                new Color32(175, 75, 225, 255), // 255 - (80, 180, 30): cancels pixel 2 exactly.
            };

            var result = NormalMapMipUtility.DownsampleNormalMap(pixels, 2, 2, out _, out _);

            var decoded = DecodeNormal(result[0]);
            Assert.Less(Vector3.Distance(decoded, new Vector3(0f, 0f, 1f)), 0.02f, $"Expected the zero-length guard's (0,0,1) fallback but got {decoded}.");
            Assert.AreEqual(255, (int)result[0].a);
        }

        /// <summary>
        /// Verifies alpha is always forced to 255 in the output, regardless of the input alpha values (which are
        /// never meaningful for a normal map -- Unity's own normal map ASTC encoder always writes alpha = 1.0).
        /// </summary>
        [Test]
        public void DownsampleNormalMap_AlwaysForcesOpaqueAlpha()
        {
            var pixels = new[]
            {
                new Color32(200, 10, 10, 0),
                new Color32(10, 200, 10, 30),
                new Color32(10, 10, 200, 60),
                new Color32(200, 200, 200, 90),
            };

            var result = NormalMapMipUtility.DownsampleNormalMap(pixels, 2, 2, out _, out _);

            foreach (var p in result)
            {
                Assert.AreEqual(255, (int)p.a);
            }
        }

        /// <summary>
        /// Verifies output dimensions are a floor division of the input, for even, odd, and already-minimal (1
        /// on one or both axes) source sizes.
        /// </summary>
        /// <param name="width">Source width.</param>
        /// <param name="height">Source height.</param>
        /// <param name="expectedWidth">Expected output width.</param>
        /// <param name="expectedHeight">Expected output height.</param>
        [TestCase(4, 4, 2, 2)]
        [TestCase(5, 3, 2, 1)]
        [TestCase(1, 1, 1, 1)]
        [TestCase(2, 1, 1, 1)]
        [TestCase(1, 2, 1, 1)]
        [TestCase(3, 1, 1, 1)]
        public void DownsampleNormalMap_ComputesFloorDivisionSize(int width, int height, int expectedWidth, int expectedHeight)
        {
            var pixels = new Color32[width * height];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = EncodeNormal(new Vector3(0f, 0f, 1f));
            }

            var result = NormalMapMipUtility.DownsampleNormalMap(pixels, width, height, out var newWidth, out var newHeight);

            Assert.AreEqual(expectedWidth, newWidth);
            Assert.AreEqual(expectedHeight, newHeight);
            Assert.AreEqual(expectedWidth * expectedHeight, result.Length);
        }

        /// <summary>
        /// Verifies the tail-of-chain case (source already 1x1): the 2x2 sampling window clamps all four samples
        /// onto the same single input texel, so the direction is preserved rather than distorted.
        /// </summary>
        [Test]
        public void DownsampleNormalMap_TailCollapseToSinglePixel_PreservesDirection()
        {
            var direction = new Vector3(0.6f, -0.3f, 0.7f).normalized;
            var pixels = new[] { EncodeNormal(direction) };

            var result = NormalMapMipUtility.DownsampleNormalMap(pixels, 1, 1, out var newWidth, out var newHeight);

            Assert.AreEqual(1, newWidth);
            Assert.AreEqual(1, newHeight);
            var decoded = DecodeNormal(result[0]);
            Assert.Less(Vector3.Distance(decoded, direction), 0.02f, $"Expected {direction} but got {decoded}.");
        }

        /// <summary>
        /// Verifies the null-argument guard.
        /// </summary>
        [Test]
        public void DownsampleNormalMap_NullPixels_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => NormalMapMipUtility.DownsampleNormalMap(null, 2, 2, out _, out _));
        }

        /// <summary>
        /// Verifies the pixel-count/dimension mismatch guard.
        /// </summary>
        [Test]
        public void DownsampleNormalMap_MismatchedLength_Throws()
        {
            var pixels = new Color32[3];
            Assert.Throws<ArgumentException>(() => NormalMapMipUtility.DownsampleNormalMap(pixels, 2, 2, out _, out _));
        }

        private static Color32 EncodeNormal(Vector3 normal)
        {
            var r = (byte)Mathf.Clamp(Mathf.RoundToInt(((normal.x + 1f) / 2f) * 255f), 0, 255);
            var g = (byte)Mathf.Clamp(Mathf.RoundToInt(((normal.y + 1f) / 2f) * 255f), 0, 255);
            var b = (byte)Mathf.Clamp(Mathf.RoundToInt(((normal.z + 1f) / 2f) * 255f), 0, 255);
            return new Color32(r, g, b, 255);
        }

        private static Vector3 DecodeNormal(Color32 c)
        {
            return new Vector3(((c.r * 2f) / 255f) - 1f, ((c.g * 2f) / 255f) - 1f, ((c.b * 2f) / 255f) - 1f);
        }
    }
}
