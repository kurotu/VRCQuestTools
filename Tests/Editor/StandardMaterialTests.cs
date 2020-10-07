// <copyright file="StandardMaterial.cs" company="kurotu">
// Copyright (c) kurotu.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

using ImageMagick;
using NUnit.Framework;

namespace KRTQuestTools
{
    public class StandardMaterialTests
    {
        [Test]
        public void StandardNoEmission()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("Standard_NoEmission.mat");
            Assert.AreEqual(typeof(StandardMaterial), wrapper.GetType());
            using (var image = wrapper.CompositeLayers())
            using (var original = TestUtils.LoadMagickImage("albedo_1024px.png"))
            {
                var result = image.Compare(original);
                Assert.AreEqual(0.0, result.MeanErrorPerPixel);
            }
        }

        [Test]
        public void StandardEmission()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("Standard_Emission.mat");
            Assert.AreEqual(typeof(StandardMaterial), wrapper.GetType());
            using (var image = wrapper.CompositeLayers())
            using (var main = TestUtils.LoadMagickImage("albedo_1024px.png"))
            using (var emission = TestUtils.LoadMagickImage("emission_1024px.png"))
            {
                main.Composite(emission, CompositeOperator.Screen);
                var result = image.Compare(main);
                Assert.AreEqual(0.0, result.MeanErrorPerPixel);
            }
        }
    }
}
