// <copyright file="StandardMaterialTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using ImageMagick;
using KRT.VRCQuestTools.Models.Unity;
using NUnit.Framework;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for StandardMaterial.
    /// </summary>
    public class StandardMaterialTests
    {
        /// <summary>
        /// Test standard without emission.
        /// </summary>
        [Test]
        public void StandardNoEmission()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("Standard_NoEmission.mat");
            Assert.AreEqual(typeof(StandardMaterial), wrapper.GetType());
            using (var image = wrapper.GenerateToonLitImage())
            using (var original = TestUtils.LoadMagickImage("albedo_1024px_png.png"))
            {
                var result = image.Compare(original);
                Assert.AreEqual(0.0, result.MeanErrorPerPixel);
            }
        }

        /// <summary>
        /// Test standard with emission.
        /// </summary>
        [Test]
        public void StandardEmission()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("Standard_Emission.mat");
            Assert.AreEqual(typeof(StandardMaterial), wrapper.GetType());
            using (var image = wrapper.GenerateToonLitImage())
            using (var main = TestUtils.LoadMagickImage("albedo_1024px_png.png"))
            using (var emission = TestUtils.LoadMagickImage("emission_1024px.png"))
            {
                main.Composite(emission, CompositeOperator.Screen);
                var result = image.Compare(main);
                Assert.AreEqual(0.0, result.MeanErrorPerPixel);
            }
        }

        /// <summary>
        /// Test unlit transparent.
        /// </summary>
        [Test]
        public void UnlitTransparent()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("Unlit_Transparent.mat");
            Assert.AreEqual(typeof(StandardMaterial), wrapper.GetType());
            using (var image = wrapper.GenerateToonLitImage())
            using (var main = TestUtils.LoadMagickImage("alpha_test.png"))
            {
                var result = image.Compare(main);
                Assert.AreEqual(0.0, result.MeanErrorPerPixel);
            }
        }

        /// <summary>
        /// Test unlit color.
        /// </summary>
        [Test]
        public void UnlitColor()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("Unlit_Color.mat");
            Assert.AreEqual(typeof(StandardMaterial), wrapper.GetType());
            using (var image = wrapper.GenerateToonLitImage())
            using (var main = new MagickImage(MagickColor.FromRgb(255, 0, 0), 1, 1))
            {
                var result = image.Compare(main);
                Assert.AreEqual(0.0, result.MeanErrorPerPixel);
            }
        }

        /// <summary>
        /// Test render texture.
        /// </summary>
        [Test]
        public void RenderTexture()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("render_texture.mat");
            Assert.AreEqual(typeof(StandardMaterial), wrapper.GetType());
            using (var image = wrapper.GenerateToonLitImage())
            using (var main = new MagickImage(MagickColor.FromRgb(0, 0, 0), 1, 1))
            {
                var result = image.Compare(main);
                Assert.AreEqual(0.0, result.MeanErrorPerPixel);
            }
        }
    }
}
