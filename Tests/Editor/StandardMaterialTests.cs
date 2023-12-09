// <copyright file="StandardMaterialTests.cs" company="kurotu">
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
    /// Tests for StandardMaterial.
    /// </summary>
    public class StandardMaterialTests
    {
        private const float Threshold = 1e-10f;

        /// <summary>
        /// Test standard without emission.
        /// </summary>
        [Test]
        public void StandardNoEmission()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("Standard_NoEmission.mat");
            Assert.AreEqual(typeof(StandardMaterial), wrapper.GetType());
            var setting = new TextureGeneratorSetting
            {
                MainTextureBrightness = 1.0f,
            };
            using (var tex = DisposableObject.New(wrapper.GenerateToonLitImage(setting)))
            using (var original = DisposableObject.New(TestUtils.LoadUncompressedTexture("albedo_1024px_png.png")))
            {
                Assert.Less(TestUtils.Difference(tex.Object, original.Object), Threshold);
            }
        }

        /// <summary>
        /// Test standard material variant.
        /// </summary>
        [Test]
        public void StandardNpEmission_EmissionVariant()
        {
#if UNITY_2022_1_OR_NEWER
            var wrapper = TestUtils.LoadMaterialWrapper("Standard_NoEmission EmissionVariant.mat");
            Assert.AreEqual(typeof(StandardMaterial), wrapper.GetType());
            var setting = new TextureGeneratorSetting
            {
                MainTextureBrightness = 1.0f,
            };
            using (var tex = DisposableObject.New(wrapper.GenerateToonLitImage(setting)))
            using (var main = DisposableObject.New(TestUtils.LoadUncompressedTexture("albedo_1024px_png.png")))
            using (var emission = DisposableObject.New(TestUtils.LoadUncompressedTexture("emission_1024px.png")))
            using (var composed = DisposableObject.New(new Texture2D(main.Object.width, main.Object.height)))
            {
                var mainPixels = main.Object.GetPixels32();
                var emissionPixels = emission.Object.GetPixels32();
                var compose = mainPixels.Select((p, i) =>
                {
                    var e = emissionPixels[i];
                    var r = (byte)System.Math.Min(p.r + e.r, 255);
                    var g = (byte)System.Math.Min(p.g + e.g, 255);
                    var b = (byte)System.Math.Min(p.b + e.b, 255);
                    var a = (byte)System.Math.Min(p.a + e.a, 255);
                    return new Color32(r, g, b, a);
                }).ToArray();
                composed.Object.SetPixels32(compose);
                Assert.Less(TestUtils.Difference(tex.Object, composed.Object), Threshold);
            }
#else
            Assert.Ignore("This test is not supported on Unity 2021.2 or older.");
#endif
        }

        /// <summary>
        /// Test standard with emission.
        /// </summary>
        [Test]
        public void StandardEmission()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("Standard_Emission.mat");
            Assert.AreEqual(typeof(StandardMaterial), wrapper.GetType());
            var setting = new TextureGeneratorSetting
            {
                MainTextureBrightness = 1.0f,
            };
            using (var tex = DisposableObject.New(wrapper.GenerateToonLitImage(setting)))
            using (var main = DisposableObject.New(TestUtils.LoadUncompressedTexture("albedo_1024px_png.png")))
            using (var emission = DisposableObject.New(TestUtils.LoadUncompressedTexture("emission_1024px.png")))
            using (var composed = DisposableObject.New(new Texture2D(main.Object.width, main.Object.height)))
            {
                var mainPixels = main.Object.GetPixels32();
                var emissionPixels = emission.Object.GetPixels32();
                var compose = mainPixels.Select((p, i) =>
                {
                    var e = emissionPixels[i];
                    var r = (byte)System.Math.Min(p.r + e.r, 255);
                    var g = (byte)System.Math.Min(p.g + e.g, 255);
                    var b = (byte)System.Math.Min(p.b + e.b, 255);
                    var a = (byte)System.Math.Min(p.a + e.a, 255);
                    return new Color32(r, g, b, a);
                }).ToArray();
                composed.Object.SetPixels32(compose);
                Assert.Less(TestUtils.Difference(tex.Object, composed.Object), Threshold);
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
            var setting = new TextureGeneratorSetting
            {
                MainTextureBrightness = 1.0f,
            };
            using (var tex = DisposableObject.New(wrapper.GenerateToonLitImage(setting)))
            using (var original = DisposableObject.New(TestUtils.LoadUncompressedTexture("alpha_test.png")))
            {
                Assert.Less(TestUtils.Difference(tex.Object, original.Object), Threshold);
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
            var setting = new TextureGeneratorSetting
            {
                MainTextureBrightness = 1.0f,
            };
            using (var tex = DisposableObject.New(wrapper.GenerateToonLitImage(setting)))
            using (var original = DisposableObject.New(AssetUtility.CreateColorTexture(Color.red)))
            {
                Assert.Less(TestUtils.Difference(tex.Object, original.Object), Threshold);
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
            var setting = new TextureGeneratorSetting
            {
                MainTextureBrightness = 1.0f,
            };
            using (var tex = DisposableObject.New(wrapper.GenerateToonLitImage(setting)))
            using (var original = DisposableObject.New(AssetUtility.CreateColorTexture(new Color32(205, 205, 205, 205), 256, 256)))
            {
                Assert.Less(TestUtils.Difference(tex.Object, original.Object), Threshold);
            }
        }
    }
}
