// <copyright file="AXCSMaterialTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Tests for AXCS.
    /// </summary>
    public class AXCSMaterialTests
    {
        /// <summary>
        /// Set up tests.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            TestUtils.AssertIgnoreOnMissingShader("ArxCharacterShaders/Opaque");
        }

        /// <summary>
        /// AXCS with emission.
        /// </summary>
        [Test]
        public void EmissionColor()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("axcs.mat");
            Assert.AreEqual(typeof(ArktoonMaterial), wrapper.GetType());
            var setting = new TextureGeneratorSetting
            {
                MainTextureBrightness = 1.0f,
            };
            using (var tex = DisposableObject.New(wrapper.GenerateToonLitImage(setting)))
            using (var main = DisposableObject.New(TestUtils.LoadUncompressedTexture("albedo_1024px_png.png")))
            using (var computed = DisposableObject.New(new Texture2D(main.Object.width, main.Object.height)))
            {
                var emission = new Color32(0x62, 0x62, 0x62, 0xff);
                var pixels = main.Object.GetPixels32().Select(p =>
                {
                    return new Color32(
                        (byte)System.Math.Min(p.r + emission.r, 255),
                        (byte)System.Math.Min(p.g + emission.g, 255),
                        (byte)System.Math.Min(p.b + emission.b, 255),
                        p.a);
                }).ToArray();
                computed.Object.SetPixels32(pixels);

                Assert.Less(TestUtils.Difference(tex.Object, computed.Object), 1e-4);
            }
        }

        /// <summary>
        /// AXCS with emissive freak.
        /// </summary>
        [Test]
        public void EmissiveFreak()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("axcs_EmissiveFreak.mat");
            Assert.AreEqual(typeof(ArktoonMaterial), wrapper.GetType());
            var setting = new TextureGeneratorSetting
            {
                MainTextureBrightness = 1.0f,
            };
            using (var tex = DisposableObject.New(wrapper.GenerateToonLitImage(setting)))
            using (var main = DisposableObject.New(TestUtils.LoadUncompressedTexture("albedo_1024px_png.png")))
            using (var emission = DisposableObject.New(TestUtils.LoadUncompressedTexture("emission_1024px.png")))
            using (var ef1 = DisposableObject.New(TestUtils.LoadUncompressedTexture("emissive_freak_1_1024px.png")))
            using (var ef2 = DisposableObject.New(TestUtils.LoadUncompressedTexture("emissive_freak_2_1024px.png")))
            using (var computed = DisposableObject.New(new Texture2D(main.Object.width, main.Object.height)))
            {
                var e = emission.Object.GetPixels32();
                var e1 = ef1.Object.GetPixels32();
                var e2 = ef2.Object.GetPixels32();
                var pixels = main.Object.GetPixels32().Select((p, i) =>
                {
                    return new Color32(
                        (byte)System.Math.Min(e[i].r + e1[i].r + e2[i].r, 255),
                        (byte)System.Math.Min(e[i].g + e1[i].g + e2[i].g, 255),
                        (byte)System.Math.Min(e[i].b + e1[i].b + e2[i].b, 255),
                        p.a);
                }).ToArray();
                computed.Object.SetPixels32(pixels);

                Assert.Less(TestUtils.Difference(tex.Object, computed.Object), 1e-2);
            }
        }
    }
}