// <copyright file="StandardMaterialTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using KRT.VRCQuestTools.Models;
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
#if UNITY_EDITOR_WIN
        private const float Threshold = 0.1f;
#else
        private const float Threshold = 0.2f;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardMaterialTests"/> class.
        /// </summary>
        public StandardMaterialTests()
        {
            CacheManager.Texture.Clear();
        }

        /// <summary>
        /// Test standard without emission.
        /// </summary>
        [Test]
        public void StandardNoEmission()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("Standard_NoEmission.mat");
            Assert.AreEqual(typeof(StandardMaterial), wrapper.GetType());
            var setting = new ToonLitConvertSettings
            {
                mainTextureBrightness = 1.0f,
            };
            Texture2D texObj = null;
            wrapper.GenerateToonLitImage(setting, (t) => { texObj = t; }).WaitForCompletion();
            using (var tex = DisposableObject.New(texObj))
            using (var original = DisposableObject.New(TestUtils.LoadUncompressedTexture("albedo_1024px_png.png")))
            {
                Assert.Less(TestUtils.MaxDifference(tex.Object, original.Object), Threshold);
            }
        }

        /// <summary>
        /// Test standard material variant.
        /// </summary>
        [Test]
        public void StandardNpEmission_EmissionVariant()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("Standard_NoEmission EmissionVariant.mat");
            Assert.AreEqual(typeof(StandardMaterial), wrapper.GetType());
            var setting = new ToonLitConvertSettings
            {
                mainTextureBrightness = 1.0f,
            };
            Texture2D texObj = null;
            wrapper.GenerateToonLitImage(setting, (t) => { texObj = t; }).WaitForCompletion();
            using (var tex = DisposableObject.New(texObj))
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
                Assert.Less(TestUtils.MaxDifference(tex.Object, composed.Object), Threshold);
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
            var setting = new ToonLitConvertSettings
            {
                mainTextureBrightness = 1.0f,
            };
            Texture2D texObj = null;
            wrapper.GenerateToonLitImage(setting, (t) => { texObj = t; }).WaitForCompletion();
            using (var tex = DisposableObject.New(texObj))
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
                Assert.Less(TestUtils.MaxDifference(tex.Object, composed.Object), Threshold);
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
            var setting = new ToonLitConvertSettings
            {
                mainTextureBrightness = 1.0f,
            };
            Texture2D texObj = null;
            wrapper.GenerateToonLitImage(setting, (t) => { texObj = t; }).WaitForCompletion();
            using (var tex = DisposableObject.New(texObj))
            using (var original = DisposableObject.New(TestUtils.LoadUncompressedTexture("alpha_test.png")))
            {
                Assert.Less(TestUtils.MaxDifference(tex.Object, original.Object), Threshold);
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
            var setting = new ToonLitConvertSettings
            {
                mainTextureBrightness = 1.0f,
            };
            Texture2D texObj = null;
            wrapper.GenerateToonLitImage(setting, (t) => { texObj = t; }).WaitForCompletion();
            using (var tex = DisposableObject.New(texObj))
            using (var original = DisposableObject.New(TextureUtility.CreateColorTexture(Color.red)))
            {
                Assert.Less(TestUtils.MaxDifference(tex.Object, original.Object), Threshold);
            }
        }

        /// <summary>
        /// Test render texture.
        /// </summary>
        [Test]
        public void RenderTexture()
        {
#if !UNITY_EDITOR_WIN
            Assert.Ignore("The result is different on Linux.");
            return;
#endif
            var wrapper = TestUtils.LoadMaterialWrapper("render_texture.mat");
            Assert.AreEqual(typeof(StandardMaterial), wrapper.GetType());
            var setting = new ToonLitConvertSettings
            {
                mainTextureBrightness = 1.0f,
            };
            Texture2D texObj = null;
            wrapper.GenerateToonLitImage(setting, (t) => { texObj = t; }).WaitForCompletion();
            using (var tex = DisposableObject.New(texObj))
            using (var original = DisposableObject.New(TextureUtility.CreateColorTexture(new Color32(0, 0, 0, 0), 256, 256)))
            {
                Assert.Less(TestUtils.MaxDifference(tex.Object, original.Object), Threshold);
            }
        }
    }
}
