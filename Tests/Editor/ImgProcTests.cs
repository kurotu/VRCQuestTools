// <copyright file="ImgProcTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using ImageMagick;
using NUnit.Framework;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for MagickImage manipulation.
    /// </summary>
    public class ImgProcTests
    {
        /// <summary>
        /// Test alpha channel after screen composition.
        /// </summary>
        [Test]
        public void TestAlphaChannel()
        {
            using (var image = new MagickImage(TestUtils.LoadMagickImage("alpha_test.png")))
            using (var dest = new MagickImage(image))
            using (var emission = new MagickImage(MagickColors.Black, image.Width, image.Height))
            {
                Assert.True(image.HasAlpha);

                dest.HasAlpha = false;
                dest.Composite(emission, CompositeOperator.Screen);
                dest.HasAlpha = true;
                dest.CopyPixels(image, Channels.Alpha);

                var result = image.Compare(dest, ErrorMetric.MeanErrorPerPixel, Channels.All);
                Assert.AreEqual(0.0, result);
            }
        }
    }
}
