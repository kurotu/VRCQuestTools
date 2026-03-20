// <copyright file="ColorUtilityTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using NUnit.Framework;
using UnityEngine;
using VQTColorUtility = KRT.VRCQuestTools.Utils.ColorUtility;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="VQTColorUtility"/>.
    /// </summary>
    public class ColorUtilityTests
    {
        /// <summary>
        /// Test Rec.709 grayscale for pure red.
        /// </summary>
        [Test]
        public void GetRec709Grayscale_Red_ReturnsCorrectValue()
        {
            var result = VQTColorUtility.GetRec709Grayscale(Color.red);
            Assert.AreEqual(0.2126f, result, 0.001f);
        }

        /// <summary>
        /// Test Rec.709 grayscale for pure green.
        /// </summary>
        [Test]
        public void GetRec709Grayscale_Green_ReturnsCorrectValue()
        {
            var result = VQTColorUtility.GetRec709Grayscale(Color.green);
            Assert.AreEqual(0.7152f, result, 0.001f);
        }

        /// <summary>
        /// Test Rec.709 grayscale for pure blue.
        /// </summary>
        [Test]
        public void GetRec709Grayscale_Blue_ReturnsCorrectValue()
        {
            var result = VQTColorUtility.GetRec709Grayscale(Color.blue);
            Assert.AreEqual(0.0722f, result, 0.001f);
        }

        /// <summary>
        /// Test Rec.709 grayscale for white.
        /// </summary>
        [Test]
        public void GetRec709Grayscale_White_ReturnsOne()
        {
            var result = VQTColorUtility.GetRec709Grayscale(Color.white);
            Assert.AreEqual(1.0f, result, 0.001f);
        }

        /// <summary>
        /// Test Rec.709 grayscale for black.
        /// </summary>
        [Test]
        public void GetRec709Grayscale_Black_ReturnsZero()
        {
            var result = VQTColorUtility.GetRec709Grayscale(Color.black);
            Assert.AreEqual(0.0f, result, 0.001f);
        }

        /// <summary>
        /// Test HdrToLdr with LDR color returns same color.
        /// </summary>
        [Test]
        public void HdrToLdr_LdrColor_ReturnsSameColor()
        {
            var color = new Color(0.5f, 0.3f, 0.8f, 1.0f);
            var result = VQTColorUtility.HdrToLdr(color);
            Assert.AreEqual(color.r, result.r, 0.001f);
            Assert.AreEqual(color.g, result.g, 0.001f);
            Assert.AreEqual(color.b, result.b, 0.001f);
            Assert.AreEqual(color.a, result.a, 0.001f);
        }

        /// <summary>
        /// Test HdrToLdr with HDR color converts correctly.
        /// </summary>
        [Test]
        public void HdrToLdr_HdrColor_ConvertsToLdr()
        {
            var color = new Color(2.0f, 1.0f, 0.5f, 0.9f);
            var result = VQTColorUtility.HdrToLdr(color);

            // All color components should be <= 1.0
            Assert.LessOrEqual(result.r, 1.0f);
            Assert.LessOrEqual(result.g, 1.0f);
            Assert.LessOrEqual(result.b, 1.0f);

            // Alpha should be preserved
            Assert.AreEqual(0.9f, result.a, 0.001f);
        }

        /// <summary>
        /// Test HdrToLdr preserves alpha.
        /// </summary>
        [Test]
        public void HdrToLdr_PreservesAlpha()
        {
            var color = new Color(3.0f, 2.0f, 1.5f, 0.5f);
            var result = VQTColorUtility.HdrToLdr(color);
            Assert.AreEqual(0.5f, result.a, 0.001f);
        }

        /// <summary>
        /// Test HdrToLdr with color at exactly 1.0 max component.
        /// </summary>
        [Test]
        public void HdrToLdr_ExactlyOne_ReturnsSameColor()
        {
            var color = new Color(1.0f, 0.5f, 0.3f, 1.0f);
            var result = VQTColorUtility.HdrToLdr(color);
            Assert.AreEqual(color.r, result.r, 0.001f);
            Assert.AreEqual(color.g, result.g, 0.001f);
            Assert.AreEqual(color.b, result.b, 0.001f);
        }
    }
}
