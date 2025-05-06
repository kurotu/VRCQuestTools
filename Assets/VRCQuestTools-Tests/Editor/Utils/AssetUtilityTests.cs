// <copyright file="AssetUtilityTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Test MagickImage and Utility.
    /// </summary>
    public class AssetUtilityTests
    {
        private string albedo1024pxPngPath = TestUtils.TexturesFolder + "/albedo_1024px_png.png";
        private string albedo1024pxPsdPath = TestUtils.TexturesFolder + "/albedo_1024px_psd.psd";
        private string originLeftBottomTgaPath = TestUtils.TexturesFolder + "/albedo_1024px_tga_origin_left_bottom.tga";
        private string originLeftTopTgaPath = TestUtils.TexturesFolder + "/albedo_1024px_tga_origin_left_top.tga";

        /// <summary>
        /// Test PNG texture.
        /// </summary>
        [Test]
        public void LoadUncompressedPng()
        {
            using (var png = new DisposableObject<Texture2D>(TextureUtility.LoadUncompressedTexture(albedo1024pxPngPath, true)))
            {
                Assert.IsEmpty(AssetDatabase.GetAssetPath(png.Object));
                Assert.AreEqual(1024, png.Object.width);
                Assert.AreEqual(1024, png.Object.height);
            }
        }

        /// <summary>
        /// Test PSD pixels.
        /// </summary>
        [Test]
        public void LoadUncompressedPsd()
        {
            using (var png = new DisposableObject<Texture2D>(TextureUtility.LoadUncompressedTexture(albedo1024pxPngPath, true)))
            using (var psd = new DisposableObject<Texture2D>(TextureUtility.LoadUncompressedTexture(albedo1024pxPsdPath, true)))
            {
                var pngPixels = png.Object.GetPixels32();
                var psdPixels = psd.Object.GetPixels32();
                Assert.AreEqual(pngPixels.Length, psdPixels.Length);
                Assert.AreEqual(pngPixels, psdPixels);
            }
        }

        /// <summary>
        /// Test tga origin and AutoOrient().
        /// </summary>
        [Test]
        public void LoadUncompressedTgaOrigin()
        {
            using (var originLeftBottom = new DisposableObject<Texture2D>(TextureUtility.LoadUncompressedTexture(originLeftBottomTgaPath, true)))
            using (var originLeftTop = new DisposableObject<Texture2D>(TextureUtility.LoadUncompressedTexture(originLeftTopTgaPath, true)))
            {
                Assert.AreEqual(originLeftBottom.Object.GetPixels32(), originLeftTop.Object.GetPixels32());
            }
        }

        /// <summary>
        /// Test loading tga.
        /// </summary>
        [Test]
        public void LoadUncompressedTga()
        {
            using (var png = new DisposableObject<Texture2D>(TextureUtility.LoadUncompressedTexture(albedo1024pxPngPath, true)))
            using (var tga = new DisposableObject<Texture2D>(TextureUtility.LoadUncompressedTexture(originLeftBottomTgaPath, true)))
            {
                var pngPixels = png.Object.GetPixels32();
                var tgaPixels = tga.Object.GetPixels32();
                Assert.AreEqual(pngPixels.Length, tgaPixels.Length);
                Assert.AreEqual(pngPixels, tgaPixels);
            }
        }

        /// <summary>
        /// Test AssetUtility.CreateAsset().
        /// </summary>
        [Test]
        public void CreateAsset()
        {
            var guid = GUID.Generate();
            var path = $"Assets/test_tmp_{guid}.mat";
            Assert.IsFalse(System.IO.File.Exists(path));

            var material = new Material(Shader.Find("Standard"));
            material.color = Color.red;
            var origMat = AssetUtility.CreateAsset(material, path);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(origMat, out var guid_orig, out long _);
            Assert.AreEqual(Color.red, origMat.color);

            var updated = new Material(Shader.Find("Standard"));
            updated.color = Color.blue;
            var newMat = AssetUtility.CreateAsset(updated, path);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(newMat, out var guid_new, out long _);
            Assert.AreEqual(Color.blue, newMat.color);

            Assert.AreEqual(guid_orig, guid_new);

            AssetDatabase.DeleteAsset(path);
        }
    }
}
