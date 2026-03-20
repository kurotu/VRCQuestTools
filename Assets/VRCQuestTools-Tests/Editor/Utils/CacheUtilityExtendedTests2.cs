// <copyright file="CacheUtilityExtendedTests2.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests.Utils
{
    [TestFixture]
    internal class CacheUtilityExtendedTests2
    {
        // ---- TextureCache tests ----

        [Test]
        public void TextureCache_Constructor_CachesTextureProperties()
        {
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false, false);
            try
            {
                var cache = new CacheUtility.TextureCache(tex, false, false, UnityEditor.BuildTarget.StandaloneWindows64);
                Assert.IsNotNull(cache);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void TextureCache_ToTexture2D_RestoresDimensions()
        {
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false, false);
            var pixels = new Color32[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(255, 0, 0, 255);
            }
            tex.SetPixels32(pixels);
            tex.Apply();

            Texture2D restored = null;
            try
            {
                var cache = new CacheUtility.TextureCache(tex, false, false, UnityEditor.BuildTarget.StandaloneWindows64);
                restored = cache.ToTexture2D();
                Assert.AreEqual(32, restored.width);
                Assert.AreEqual(32, restored.height);
            }
            finally
            {
                Object.DestroyImmediate(tex);
                if (restored != null) Object.DestroyImmediate(restored);
            }
        }

        [Test]
        public void TextureCache_ToTexture2D_PreservesFormat()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false, false);
            var pixels = new Color32[16 * 16];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(0, 255, 0, 255);
            }
            tex.SetPixels32(pixels);
            tex.Apply();

            Texture2D restored = null;
            try
            {
                var cache = new CacheUtility.TextureCache(tex, false, false, UnityEditor.BuildTarget.StandaloneWindows64);
                restored = cache.ToTexture2D();
                Assert.AreEqual(TextureFormat.RGBA32, restored.format);
            }
            finally
            {
                Object.DestroyImmediate(tex);
                if (restored != null) Object.DestroyImmediate(restored);
            }
        }

        [Test]
        public void TextureCache_Linear_RestoresCorrectly()
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false, true); // linear
            var pixels = new Color32[8 * 8];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(128, 128, 128, 255);
            }
            tex.SetPixels32(pixels);
            tex.Apply();

            Texture2D restored = null;
            try
            {
                var cache = new CacheUtility.TextureCache(tex, true, false, UnityEditor.BuildTarget.StandaloneWindows64);
                restored = cache.ToTexture2D();
                Assert.AreEqual(8, restored.width);
                Assert.AreEqual(8, restored.height);
            }
            finally
            {
                Object.DestroyImmediate(tex);
                if (restored != null) Object.DestroyImmediate(restored);
            }
        }

        [Test]
        public void TextureCache_WithMipMap_RestoresCorrectly()
        {
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, true, false); // mipmap = true
            var pixels = new Color32[64 * 64];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(0, 0, 255, 255);
            }
            tex.SetPixels32(pixels);
            tex.Apply();

            Texture2D restored = null;
            try
            {
                var cache = new CacheUtility.TextureCache(tex, false, false, UnityEditor.BuildTarget.StandaloneWindows64);
                restored = cache.ToTexture2D();
                Assert.AreEqual(64, restored.width);
            }
            finally
            {
                Object.DestroyImmediate(tex);
                if (restored != null) Object.DestroyImmediate(restored);
            }
        }

        // ---- GetContentCacheKey tests ----

        [Test]
        public void GetContentCacheKey_SameMaterial_SameKey()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var key1 = CacheUtility.GetContentCacheKey(mat);
                var key2 = CacheUtility.GetContentCacheKey(mat);
                Assert.AreEqual(key1, key2);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetContentCacheKey_DifferentColor_DifferentKey()
        {
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            try
            {
                mat1.color = Color.red;
                mat2.color = Color.blue;
                var key1 = CacheUtility.GetContentCacheKey(mat1);
                var key2 = CacheUtility.GetContentCacheKey(mat2);
                Assert.AreNotEqual(key1, key2);
            }
            finally
            {
                Object.DestroyImmediate(mat1);
                Object.DestroyImmediate(mat2);
            }
        }

        [Test]
        public void GetContentCacheKey_ContainsShaderName()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var key = CacheUtility.GetContentCacheKey(mat);
                Assert.IsTrue(key.StartsWith("Standard"), $"Key should start with shader name, got: {key}");
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetContentCacheKey_DifferentFloat_DifferentKey()
        {
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            try
            {
                mat1.SetFloat("_Metallic", 0.0f);
                mat2.SetFloat("_Metallic", 1.0f);
                var key1 = CacheUtility.GetContentCacheKey(mat1);
                var key2 = CacheUtility.GetContentCacheKey(mat2);
                Assert.AreNotEqual(key1, key2);
            }
            finally
            {
                Object.DestroyImmediate(mat1);
                Object.DestroyImmediate(mat2);
            }
        }
    }
}
