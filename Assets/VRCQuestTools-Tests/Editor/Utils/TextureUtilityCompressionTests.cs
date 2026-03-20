// <copyright file="TextureUtilityCompressionTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;
using VQTBuildTarget = KRT.VRCQuestTools.Models.BuildTarget;

namespace KRT.VRCQuestTools.Tests.Utils
{
    [TestFixture]
    internal class TextureUtilityCompressionTests
    {
        // ---- CompressTextureForBuildTarget ----

        [Test]
        public void CompressTextureForBuildTarget_Android_ASTC6x6()
        {
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            FillTexture(tex, Color.red);
            try
            {
                var result = TextureUtility.CompressTextureForBuildTarget(tex, UnityEditor.BuildTarget.Android, TextureFormat.ASTC_6x6);
                Assert.IsNotNull(result);
                Assert.AreEqual(TextureFormat.ASTC_6x6, result.format);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressTextureForBuildTarget_Windows_DXT5()
        {
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            FillTexture(tex, Color.green);
            try
            {
                var result = TextureUtility.CompressTextureForBuildTarget(tex, UnityEditor.BuildTarget.StandaloneWindows64, TextureFormat.DXT5);
                Assert.IsNotNull(result);
                Assert.AreEqual(TextureFormat.DXT5, result.format);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressTextureForBuildTarget_WithMaxSize()
        {
            var tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            FillTexture(tex, Color.blue);
            try
            {
                var result = TextureUtility.CompressTextureForBuildTarget(tex, UnityEditor.BuildTarget.Android, TextureFormat.ASTC_6x6, 64);
                Assert.IsNotNull(result);
                Assert.AreEqual(64, result.width);
                Assert.AreEqual(64, result.height);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        // ---- CompressNormalMap ----

        [Test]
        public void CompressNormalMap_Android_ASTC6x6()
        {
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false, true);
            FillNormalMap(tex);
            try
            {
                var result = TextureUtility.CompressNormalMap(tex, UnityEditor.BuildTarget.Android, TextureFormat.ASTC_6x6, true);
                Assert.IsNotNull(result);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressNormalMap_WithMaxSize()
        {
            var tex = new Texture2D(128, 128, TextureFormat.RGBA32, false, true);
            FillNormalMap(tex);
            try
            {
                var result = TextureUtility.CompressNormalMap(tex, UnityEditor.BuildTarget.Android, TextureFormat.ASTC_6x6, true, 64);
                Assert.IsNotNull(result);
                Assert.AreEqual(64, result.width);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        // ---- LoadUncompressedTexture ----

        [Test]
        public void LoadUncompressedTexture_RuntimeTexture_ReturnsReadable()
        {
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            FillTexture(tex, Color.white);
            try
            {
                var result = TextureUtility.LoadUncompressedTexture(tex);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.isReadable);
                if (result != tex)
                {
                    Object.DestroyImmediate(result);
                }
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        // ---- DownscaleNormalMap ----

        [Test]
        public void DownscaleNormalMap_HalvesSize()
        {
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false, true);
            FillNormalMap(tex);
            try
            {
                var result = TextureUtility.DownscaleNormalMap(tex, true, 32, 32);
                Assert.IsNotNull(result);
                Assert.AreEqual(32, result.width);
                Assert.AreEqual(32, result.height);
                Object.DestroyImmediate(result);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        // ---- IsSupportedTextureFormat extended ----

        [Test]
        public void IsSupportedTextureFormat_ASTC_Android_True()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.ASTC_6x6, UnityEditor.BuildTarget.Android));
        }

        [Test]
        public void IsSupportedTextureFormat_DXT5_Windows_True()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.DXT5, UnityEditor.BuildTarget.StandaloneWindows64));
        }

        [Test]
        public void IsSupportedTextureFormat_DXT5_Android_False()
        {
            Assert.IsFalse(TextureUtility.IsSupportedTextureFormat(TextureFormat.DXT5, UnityEditor.BuildTarget.Android));
        }

        [Test]
        public void IsSupportedTextureFormat_ASTC_Windows_False()
        {
            Assert.IsFalse(TextureUtility.IsSupportedTextureFormat(TextureFormat.ASTC_6x6, UnityEditor.BuildTarget.StandaloneWindows64));
        }

        // ---- CreateColorTexture extended ----

        [Test]
        public void CreateColorTexture_CustomSize_CorrectDimensions()
        {
            var tex = TextureUtility.CreateColorTexture(new Color32(128, 128, 128, 255), 256, 128);
            try
            {
                Assert.AreEqual(256, tex.width);
                Assert.AreEqual(128, tex.height);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        // ---- SetStreamingMipMaps ----

        [Test]
        public void SetStreamingMipMaps_Enable_SetsFlag()
        {
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, true); // mipmap = true
            FillTexture(tex, Color.white);
            try
            {
                TextureUtility.SetStreamingMipMaps(tex, true);
                Assert.IsTrue(tex.streamingMipmaps);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void SetStreamingMipMaps_Disable_ClearsFlag()
        {
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, true);
            FillTexture(tex, Color.white);
            try
            {
                TextureUtility.SetStreamingMipMaps(tex, false);
                Assert.IsFalse(tex.streamingMipmaps);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        // ---- Helper methods ----

        private static void FillTexture(Texture2D tex, Color color)
        {
            var pixels = new Color32[tex.width * tex.height];
            var color32 = (Color32)color;
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color32;
            }
            tex.SetPixels32(pixels);
            tex.Apply();
        }

        private static void FillNormalMap(Texture2D tex)
        {
            var pixels = new Color32[tex.width * tex.height];
            // Default normal: (0.5, 0.5, 1.0, 1.0) = flat normal
            var normalColor = new Color32(128, 128, 255, 255);
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = normalColor;
            }
            tex.SetPixels32(pixels);
            tex.Apply();
        }
    }
}
