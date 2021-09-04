// <copyright file="MagickImageUtilityTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using ImageMagick;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Test MagickImage and Utility.
    /// </summary>
    public class MagickImageUtilityTests
    {
        private string albedo1024pxPngPath = TestUtils.TexturesFolder + "/albedo_1024px.png";
        private string originLeftBottomTgaPath = TestUtils.TexturesFolder + "/albedo_1024px_tga_origin_left_bottom.tga";
        private string originLeftTopTgaPath = TestUtils.TexturesFolder + "/albedo_1024px_tga_origin_left_top.tga";

        /// <summary>
        /// Test tga origin and AutoOrient().
        /// </summary>
        [Test]
        public void MagickImageTgaOrigin()
        {
            using (var originLeftBottom = new MagickImage(originLeftBottomTgaPath))
            using (var originLeftTop = new MagickImage(originLeftTopTgaPath))
            using (var png = new MagickImage(albedo1024pxPngPath))
            {
                // Check loaded orientation
                Assert.AreNotEqual(originLeftBottom.Orientation, originLeftTop.Orientation);
                Assert.AreEqual(OrientationType.BottomLeft, originLeftBottom.Orientation);
                Assert.AreEqual(OrientationType.TopLeft, originLeftTop.Orientation);

                // Check format
                Assert.AreEqual(MagickFormat.Tga, originLeftBottom.Format);
                Assert.AreEqual(MagickFormat.Tga, originLeftTop.Format);

                // Compare pixels with png
                Assert.AreNotEqual(0.0, png.Compare(originLeftBottom).MeanErrorPerPixel);
                originLeftBottom.AutoOrient();
                Assert.AreEqual(0.0, png.Compare(originLeftBottom).MeanErrorPerPixel);

                Assert.AreEqual(0.0, png.Compare(originLeftTop).MeanErrorPerPixel);
                originLeftTop.AutoOrient();
                Assert.AreEqual(0.0, png.Compare(originLeftTop).MeanErrorPerPixel);
            }
        }

        /// <summary>
        /// Test loading tga.
        /// </summary>
        [Test]
        public void GetTgaMagickImage()
        {
            using (var png = new MagickImage(albedo1024pxPngPath))
            {
                var originLeftBottomTex = AssetDatabase.LoadAssetAtPath<Texture2D>(originLeftBottomTgaPath);
                using (var tga = MagickImageUtility.GetMagickImage(originLeftBottomTex))
                {
                    Assert.AreEqual(0.0, png.Compare(tga).MeanErrorPerPixel);
                }

                var originLeftTopTex = AssetDatabase.LoadAssetAtPath<Texture2D>(originLeftTopTgaPath);
                using (var tga = MagickImageUtility.GetMagickImage(originLeftTopTex))
                {
                    Assert.AreEqual(0.0, png.Compare(tga).MeanErrorPerPixel);
                }
            }
        }
    }
}
