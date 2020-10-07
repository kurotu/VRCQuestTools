// <copyright file="ArktoonMaterialTests.cs" company="kurotu">
// Copyright (c) kurotu.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

using ImageMagick;
using NUnit.Framework;

namespace KRTQuestTools
{
    public class ArktoonMaterialTests
    {
        [Test]
        public void EmissionColor()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("arktoon.mat");
            Assert.AreEqual(typeof(ArktoonMaterial), wrapper.GetType());
            using (var image = wrapper.CompositeLayers())
            using (var original = TestUtils.LoadMagickImage("albedo_1024px.png"))
            using (var emission = new MagickImage(new MagickColorFactory().Create("#1F1F1F"), original.Width, original.Height))
            {
                original.Composite(emission, CompositeOperator.Screen);
                var result = image.Compare(original);
                Assert.AreEqual(0.0, result.MeanErrorPerPixel);
            }
        }

        [Test]
        public void EmissiveFreak()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("arktoon_EmissiveFreak.mat");
            Assert.AreEqual(typeof(ArktoonMaterial), wrapper.GetType());
            using (var image = wrapper.CompositeLayers())
            using (var main = TestUtils.LoadMagickImage("albedo_1024px.png"))
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
