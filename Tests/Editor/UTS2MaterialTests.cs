// <copyright file="UTS2MaterialTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

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
            using (var main = DisposableObject.New(TestUtils.LoadUncompressedTexture("albedo_1024px_png.png")))
            using (var emission = DisposableObject.New(TestUtils.LoadUncompressedTexture("emission_1024px.png")))
            using (var computed = DisposableObject.New(new Texture2D(main.Object.width, main.Object.height)))
            {
                var e = emission.Object.GetPixels32();
                var pixels = main.Object.GetPixels32().Select((p, i) =>
                {
                    return new Color32(
                        (byte)System.Math.Min(p.r + e[i].r, 255),
                        (byte)System.Math.Min(p.g + e[i].g, 255),
                        (byte)System.Math.Min(p.b + e[i].b, 255),
                        p.a);
                }).ToArray();
                computed.Object.SetPixels32(pixels);

                Assert.Less(TestUtils.Difference(tex.Object, computed.Object), 1e-4);
            }
        }
    }
}
