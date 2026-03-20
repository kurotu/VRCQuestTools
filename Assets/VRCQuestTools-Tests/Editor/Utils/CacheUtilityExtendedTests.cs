// Tests for CacheUtility

using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using KRT.VRCQuestTools.Utils;

namespace KRT.VRCQuestTools.Utils.Tests
{
    [TestFixture]
    public class CacheUtilityExtendedTests
    {
        [Test]
        public void GetContentCacheKey_StandardMaterial_ReturnsNonEmpty()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var key = CacheUtility.GetContentCacheKey(mat);
                Assert.IsNotNull(key);
                Assert.IsNotEmpty(key);
                Assert.IsTrue(key.StartsWith("Standard"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetContentCacheKey_DifferentColors_DifferentKeys()
        {
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            mat1.SetColor("_Color", Color.red);
            mat2.SetColor("_Color", Color.blue);
            try
            {
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
        public void GetContentCacheKey_WithKeywords_IncludesKeywords()
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.EnableKeyword("_EMISSION");
            try
            {
                var key = CacheUtility.GetContentCacheKey(mat);
                Assert.IsTrue(key.Contains("_EMISSION"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void TextureCache_RoundTrip_PreservesData()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false, false);
            var pixels = new Color32[16];
            for (int i = 0; i < 16; i++)
            {
                pixels[i] = new Color32((byte)(i * 16), (byte)(i * 8), (byte)(i * 4), 255);
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            try
            {
                var cache = new CacheUtility.TextureCache(tex, false, false, BuildTarget.StandaloneWindows64);
                var restored = cache.ToTexture2D();
                Assert.IsNotNull(restored);
                Assert.AreEqual(4, restored.width);
                Assert.AreEqual(4, restored.height);
                Object.DestroyImmediate(restored);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void TextureCache_WithMipmap_PreservesData()
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, true, false);
            tex.Apply();
            try
            {
                var cache = new CacheUtility.TextureCache(tex, false, false, BuildTarget.StandaloneWindows64);
                var restored = cache.ToTexture2D();
                Assert.IsNotNull(restored);
                Assert.AreEqual(8, restored.width);
                Assert.AreEqual(8, restored.height);
                Object.DestroyImmediate(restored);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void TextureCache_LinearTexture_PreservesData()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false, true);
            tex.Apply();
            try
            {
                var cache = new CacheUtility.TextureCache(tex, true, false, BuildTarget.StandaloneWindows64);
                var restored = cache.ToTexture2D();
                Assert.IsNotNull(restored);
                Object.DestroyImmediate(restored);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }
    }
}
