// <copyright file="ArktoonMaterialTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using ImageMagick;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for Arktoon.
    /// </summary>
    public class ArktoonMaterialTests
    {
        /// <summary>
        /// Set up tests.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            TestUtils.AssertIgnoreOnMissingShader("arktoon/Opaque");
        }

        /// <summary>
        /// Arctoon with emission.
        /// </summary>
        [Test]
        public void EmissionColor()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("arktoon.mat");
            Assert.AreEqual(typeof(ArktoonMaterial), wrapper.GetType());
            using (var tex = DisposableObject.New(wrapper.GenerateToonLitImage()))
            using (var image = MagickImageUtility.Texture2DToMagickImage(tex.Object))
            using (var original = TestUtils.LoadMagickImage("albedo_1024px_png.png"))
            using (var emission = new MagickImage(new MagickColorFactory().Create("#1F1F1F"), original.Width, original.Height))
            {
                original.Composite(emission, CompositeOperator.Screen);
                var result = image.Compare(original);
                Assert.AreEqual(0.0, result.MeanErrorPerPixel);
            }
        }

        /// <summary>
        /// Arctoon with emissive freak.
        /// </summary>
        [Test]
        public void EmissiveFreak()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("arktoon_EmissiveFreak.mat");
            Assert.AreEqual(typeof(ArktoonMaterial), wrapper.GetType());
            using (var tex = DisposableObject.New(wrapper.GenerateToonLitImage()))
            using (var image = MagickImageUtility.Texture2DToMagickImage(tex.Object))
            using (var main = TestUtils.LoadMagickImage("albedo_1024px_png.png"))
            using (var emission = TestUtils.LoadMagickImage("emission_1024px.png"))
            using (var ef1 = TestUtils.LoadMagickImage("emissive_freak_1_1024px.png"))
            using (var ef2 = TestUtils.LoadMagickImage("emissive_freak_2_1024px.png"))
            {
                main.Composite(emission, CompositeOperator.Screen);
                main.Composite(ef1, CompositeOperator.Screen);
                main.Composite(ef2, CompositeOperator.Screen);
                var result = image.Compare(main);
                Assert.AreEqual(0.0, result.MeanErrorPerPixel);
            }
        }
    }
}
