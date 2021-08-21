// <copyright file="MSMapGenTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.IO;
using ImageMagick;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for MSMapGen.
    /// </summary>
    public class MSMapGenTests
    {
        private readonly string linearGradationPath = Path.Combine(TestUtils.TexturesFolder, "linear_gradation.png");
        private readonly string msMapPath = Path.GetTempFileName() + ".png";

        /// <summary>
        /// Setup params.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            if (File.Exists(msMapPath))
            {
                File.Delete(msMapPath);
            }
        }

        /// <summary>
        /// TearDown params.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (File.Exists(msMapPath))
            {
                File.Delete(msMapPath);
            }
        }

        /// <summary>
        /// Test gradation fixture.
        /// </summary>
        [Test]
        public void LinearGradation()
        {
            using (var gradation = new MagickImage(linearGradationPath))
            {
                Assert.AreEqual(256, gradation.Width);
                Assert.AreEqual(256, gradation.Height);
                Assert.AreEqual(ColorSpace.sRGB, gradation.ColorSpace);
                var pixels = gradation.GetPixels();
                for (int x = 0; x < 256; x++)
                {
                    var v = pixels.GetValue(x, 0);
                    Assert.AreEqual(3, v.Length);
                    Assert.AreEqual(257 * x, v[0]); // 257 * 255 = 65535
                }
            }
        }

        /// <summary>
        /// Test Negate() as invert.
        /// </summary>
        [Test]
        public void Invert()
        {
            using (var gradation = new MagickImage(linearGradationPath))
            {
                gradation.Negate();
                Assert.AreEqual(ColorSpace.sRGB, gradation.ColorSpace);
                var pixels = gradation.GetPixels();
                for (int x = 0; x < 256; x++)
                {
                    var v = pixels.GetValue(x, 0);
                    Assert.AreEqual(3, v.Length);
                    Assert.AreEqual(257 * (255 - x), v[0]); // 257 * 255 = 65535
                }
            }
        }

        /// <summary>
        /// Test generating MSMap.
        /// </summary>
        [Test]
        public void MetallicSmoothness()
        {
            using (var gradation = new MagickImage(linearGradationPath))
            using (var msmap = MagickImageUtility.GenerateMetallicSmoothness(gradation, false, gradation, false))
            {
                Assert.AreEqual(ColorSpace.sRGB, msmap.ColorSpace);
                Assert.True(msmap.HasAlpha);
                var pixels = msmap.GetPixels();
                for (int x = 0; x < 256; x++)
                {
                    var v = pixels.GetValue(x, 0);
                    Assert.AreEqual(4, v.Length);
                    Assert.AreEqual(257 * x, v[0]); // 257 * 255 = 65535
                    Assert.AreEqual(0, v[1]);
                    Assert.AreEqual(0, v[2]);
                    Assert.AreEqual(257 * x, v[3]);
                }
                msmap.Write(msMapPath, MagickFormat.Png32);
            }
            using (var msmap = new MagickImage(msMapPath))
            {
                Assert.AreEqual(ColorSpace.sRGB, msmap.ColorSpace);
                Assert.True(msmap.HasAlpha);
                var pixels = msmap.GetPixels();
                for (int x = 0; x < 256; x++)
                {
                    var v = pixels.GetValue(x, 0);
                    Assert.AreEqual(4, v.Length);
                    Assert.AreEqual(257 * x, v[0]); // 257 * 255 = 65535
                    Assert.AreEqual(0, v[1]);
                    Assert.AreEqual(0, v[2]);
                    Assert.AreEqual(257 * x, v[3]);
                }
            }
        }

        /// <summary>
        /// Test inverted input maps.
        /// </summary>
        [Test]
        public void MetallicSmoothnessInvert()
        {
            using (var gradation = new MagickImage(linearGradationPath))
            using (var msmap = MagickImageUtility.GenerateMetallicSmoothness(gradation, true, gradation, true))
            {
                Assert.AreEqual(ColorSpace.sRGB, msmap.ColorSpace);
                Assert.True(msmap.HasAlpha);
                var pixels = msmap.GetPixels();
                for (int x = 0; x < 256; x++)
                {
                    var v = pixels.GetValue(x, 0);
                    Assert.AreEqual(4, v.Length);
                    Assert.AreEqual(257 * (255 - x), v[0]); // 257 * 255 = 65535
                    Assert.AreEqual(0, v[1]);
                    Assert.AreEqual(0, v[2]);
                    Assert.AreEqual(257 * (255 - x), v[3]);
                }
                msmap.Write(msMapPath, MagickFormat.Png32);
            }
            using (var msmap = new MagickImage(msMapPath))
            {
                Assert.AreEqual(ColorSpace.sRGB, msmap.ColorSpace);
                Assert.True(msmap.HasAlpha);
                var pixels = msmap.GetPixels();
                for (int x = 0; x < 256; x++)
                {
                    var v = pixels.GetValue(x, 0);
                    Assert.AreEqual(4, v.Length);
                    Assert.AreEqual(257 * (255 - x), v[0]); // 257 * 255 = 65535
                    Assert.AreEqual(0, v[1]);
                    Assert.AreEqual(0, v[2]);
                    Assert.AreEqual(257 * (255 - x), v[3]);
                }
            }
        }
    }
}
