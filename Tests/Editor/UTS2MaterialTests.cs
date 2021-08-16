// <copyright file="UTS2MaterialTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using ImageMagick;
using KRT.VRCQuestTools.Models.Unity;
using NUnit.Framework;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for UTS2.
    /// </summary>
    public class UTS2MaterialTests
    {
        /// <summary>
        /// Test UTS2 with emission.
        /// </summary>
        [Test]
        public void UTS2()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("UTS2.mat");
            Assert.AreEqual(typeof(UTS2Material), wrapper.GetType());
            using (var image = wrapper.GenerateToonLitImage())
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
