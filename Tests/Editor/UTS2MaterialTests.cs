// <copyright file="UTS2MaterialTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using ImageMagick;
using NUnit.Framework;

namespace KRT.VRCQuestTools
{
    public class UTS2MaterialTests
    {
        [Test]
        public void UTS2()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("UTS2.mat");
            Assert.AreEqual(typeof(UTS2Material), wrapper.GetType());
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
