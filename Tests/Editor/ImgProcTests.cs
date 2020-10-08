// <copyright file="ImgProcTests.cs" company="kurotu">
// Copyright (c) kurotu.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

using ImageMagick;
using NUnit.Framework;

namespace KRT.VRCQuestTools
{
    public class ImgProcTests
    {
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
