// <copyright file="MSMapGenTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using ImageMagick;
using NUnit.Framework;
using System.IO;

namespace KRT.VRCQuestTools
{
    public class MSMapGenTests
    {
        readonly string LinearGradationPath = Path.Combine(TestUtils.TexturesFolder, "linear_gradation.png");
        readonly string MSMapPath = Path.GetTempFileName() + ".png";

        [SetUp]
        public void Setup()
        {
            if (File.Exists(MSMapPath))
            {
                File.Delete(MSMapPath);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(MSMapPath))
            {
                File.Delete(MSMapPath);
            }
        }

        [Test]
        public void LinearGradation()
        {
            using (var gradation = new MagickImage(LinearGradationPath))
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

        [Test]
        public void Invert()
        {
            using (var gradation = new MagickImage(LinearGradationPath))
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

        [Test]
        public void MetallicSmoothness()
        {
            using (var gradation = new MagickImage(LinearGradationPath))
            using (var msmap = MSMapGen.GenerateMetallicSmoothness(gradation, false, gradation, false))
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
                msmap.Write(MSMapPath, MagickFormat.Png32);
            }
            using (var msmap = new MagickImage(MSMapPath))
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

        [Test]
        public void MetallicSmoothnessInvert()
        {
            using (var gradation = new MagickImage(LinearGradationPath))
            using (var msmap = MSMapGen.GenerateMetallicSmoothness(gradation, true, gradation, true))
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
                msmap.Write(MSMapPath, MagickFormat.Png32);
            }
            using (var msmap = new MagickImage(MSMapPath))
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
