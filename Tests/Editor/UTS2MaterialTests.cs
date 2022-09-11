// <copyright file="UTS2MaterialTests.cs" company="kurotu">
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
    /// Tests for UTS2.
    /// </summary>
    public class UTS2MaterialTests
    {
        /// <summary>
        /// Set up tests.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            TestUtils.AssertIgnoreOnMissingShader("UnityChanToonShader/Toon_DoubleShadeWithFeather");
        }

        /// <summary>
        /// Test UTS2 with emission.
        /// </summary>
        [Test]
        public void UTS2()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("UTS2.mat");
            Assert.AreEqual(typeof(UTS2Material), wrapper.GetType());
            using (var tex = DisposableObject.New(wrapper.GenerateToonLitImage()))
            using (var image = MagickImageUtility.Texture2DToMagickImage(tex.Object))
            using (var main = TestUtils.LoadMagickImage("albedo_1024px_png.png"))
            using (var emission = TestUtils.LoadMagickImage("emission_1024px.png"))
            {
                main.Composite(emission, CompositeOperator.Screen);
                var result = image.Compare(main);
                Assert.AreEqual(0.0, result.MeanErrorPerPixel);
            }
        }
    }
}
