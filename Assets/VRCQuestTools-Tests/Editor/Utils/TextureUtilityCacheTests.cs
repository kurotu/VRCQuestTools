// Tests for TextureUtility, CacheUtility - using System;
using System.Reflection;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using VQTBuildTarget = KRT.VRCQuestTools.Models.BuildTarget;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    public class TextureUtilityCacheIntegrationTests
    {
        [Test]
        public void CompressTextureForBuildTarget_Mobile_ASTC()
        {
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            try
            {
                var result = TextureUtility.CompressTextureForBuildTarget(tex, UnityEditor.BuildTarget.Android, TextureFormat.ASTC_4x4);
                Assert.IsNotNull(result);
                Assert.AreEqual(TextureFormat.ASTC_4x4, result.format);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressTextureForBuildTarget_Desktop_DXT5()
        {
            // DXT5 requires dimensions multiple of 4
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            try
            {
                var result = TextureUtility.CompressTextureForBuildTarget(tex, UnityEditor.BuildTarget.StandaloneWindows64, TextureFormat.ASTC_4x4);
                Assert.IsNotNull(result);
                Assert.AreEqual(TextureFormat.DXT5, result.format);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressTextureForBuildTarget_DXT5_NonMultipleOf4_SkipsCompression()
        {
            // Non-multiple-of-4 texture should skip DXT5 compression
            var tex = new Texture2D(65, 65, TextureFormat.RGBA32, false);
            try
            {
                var result = TextureUtility.CompressTextureForBuildTarget(tex, UnityEditor.BuildTarget.StandaloneWindows64, TextureFormat.ASTC_4x4);
                Assert.IsNotNull(result);
                // Should remain RGBA32 since DXT5 requires multiple of 4
                Assert.AreEqual(TextureFormat.RGBA32, result.format);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressTextureForBuildTarget_WithMaxTextureSize_Resizes()
        {
            var tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            try
            {
                var result = TextureUtility.CompressTextureForBuildTarget(tex, UnityEditor.BuildTarget.Android, TextureFormat.ASTC_4x4, maxTextureSize: 64);
                Assert.IsNotNull(result);
                Assert.AreEqual(64, result.width);
                Assert.AreEqual(64, result.height);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressTextureForBuildTarget_MaxTextureSizeSameAsSize_NoResize()
        {
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            try
            {
                var result = TextureUtility.CompressTextureForBuildTarget(tex, UnityEditor.BuildTarget.Android, TextureFormat.ASTC_4x4, maxTextureSize: 64);
                Assert.IsNotNull(result);
                Assert.AreEqual(64, result.width);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressTextureForBuildTarget_iOS_Mobile()
        {
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            try
            {
                var result = TextureUtility.CompressTextureForBuildTarget(tex, UnityEditor.BuildTarget.iOS, TextureFormat.ASTC_6x6);
                Assert.IsNotNull(result);
                Assert.AreEqual(TextureFormat.ASTC_6x6, result.format);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressNormalMap_Mobile()
        {
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            try
            {
                // Fill with normal map colors (0.5, 0.5, 1.0)
                var pixels = tex.GetPixels32();
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = new Color32(128, 128, 255, 255);
                }
                tex.SetPixels32(pixels);
                tex.Apply();

                var result = TextureUtility.CompressNormalMap(tex, UnityEditor.BuildTarget.Android, TextureFormat.ASTC_4x4);
                Assert.IsNotNull(result);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressNormalMap_Desktop()
        {
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            try
            {
                var pixels = tex.GetPixels32();
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = new Color32(128, 128, 255, 255);
                }
                tex.SetPixels32(pixels);
                tex.Apply();

                var result = TextureUtility.CompressNormalMap(tex, UnityEditor.BuildTarget.StandaloneWindows64, TextureFormat.ASTC_4x4);
                Assert.IsNotNull(result);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressNormalMap_WithMaxTextureSize()
        {
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            try
            {
                var pixels = tex.GetPixels32();
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = new Color32(128, 128, 255, 255);
                }
                tex.SetPixels32(pixels);
                tex.Apply();

                var result = TextureUtility.CompressNormalMap(tex, UnityEditor.BuildTarget.Android, TextureFormat.ASTC_4x4, maxTextureSize: 32);
                Assert.IsNotNull(result);
                Assert.LessOrEqual(Math.Max(result.width, result.height), 32);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void LoadUncompressedTexture_NullReturnsNull()
        {
            var result = TextureUtility.LoadUncompressedTexture((Texture)null);
            Assert.IsNull(result);
        }

        [Test]
        public void LoadUncompressedTexture_UnsavedTexture_ReturnsSame()
        {
            // An unsaved texture (no asset path) returns itself
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            try
            {
                var result = TextureUtility.LoadUncompressedTexture((Texture)tex);
                Assert.IsNotNull(result);
                Assert.AreEqual(tex, result);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CreateColorTexture_ReturnsCorrectSize()
        {
            var color = new Color32(255, 0, 0, 255);
            var tex = TextureUtility.CreateColorTexture(color, 8, 8);
            try
            {
                Assert.AreEqual(8, tex.width);
                Assert.AreEqual(8, tex.height);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CreateColorTexture_DefaultSize()
        {
            var color = new Color32(0, 255, 0, 255);
            var tex = TextureUtility.CreateColorTexture(color);
            try
            {
                Assert.AreEqual(4, tex.width);
                Assert.AreEqual(4, tex.height);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void IsKnownTextureFormat_RGBA32_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.RGBA32));
        }

        [Test]
        public void IsKnownTextureFormat_DXT5_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.DXT5));
        }

        [Test]
        public void IsKnownTextureFormat_ASTC4x4_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.ASTC_4x4));
        }

        [Test]
        public void IsSupportedTextureFormat_Android_ASTC()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.ASTC_4x4, UnityEditor.BuildTarget.Android));
        }

        [Test]
        public void IsSupportedTextureFormat_Windows_DXT5()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.DXT5, UnityEditor.BuildTarget.StandaloneWindows64));
        }

        [Test]
        public void IsSupportedTextureFormat_iOS_ASTC()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.ASTC_4x4, UnityEditor.BuildTarget.iOS));
        }

        [Test]
        public void IsSupportedTextureFormat_Windows_RGBA32()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.RGBA32, UnityEditor.BuildTarget.StandaloneWindows));
        }

        [Test]
        public void IsUncompressedFormat_RGBA32_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsUncompressedFormat(TextureFormat.RGBA32));
        }

        [Test]
        public void IsUncompressedFormat_DXT5_ReturnsFalse()
        {
            Assert.IsFalse(TextureUtility.IsUncompressedFormat(TextureFormat.DXT5));
        }

        [Test]
        public void IsNormalMapAsset_NonAsset_ReturnsFalse()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                Assert.IsFalse(TextureUtility.IsNormalMapAsset(tex));
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void SetStreamingMipMaps_SetsProperty()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, true);
            try
            {
                TextureUtility.SetStreamingMipMaps(tex, true);
                var so = new SerializedObject(tex);
                so.Update();
                var prop = so.FindProperty("m_StreamingMipmaps");
                Assert.IsTrue(prop.boolValue);

                TextureUtility.SetStreamingMipMaps(tex, false);
                so.Update();
                prop = so.FindProperty("m_StreamingMipmaps");
                Assert.IsFalse(prop.boolValue);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CopyAsReadable_CreatesReadableCopy()
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            try
            {
                var pixels = tex.GetPixels32();
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = new Color32(255, 128, 64, 255);
                }
                tex.SetPixels32(pixels);
                tex.Apply();

                var copy = TextureUtility.CopyAsReadable(tex, true);
                try
                {
                    Assert.AreEqual(tex.width, copy.width);
                    Assert.AreEqual(tex.height, copy.height);
                }
                finally
                {
                    Object.DestroyImmediate(copy);
                }
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void DestroyTexture_NullDoesNotThrow()
        {
            Assert.DoesNotThrow(() => TextureUtility.DestroyTexture(null));
        }

        [Test]
        public void BakeTexture_BasicBake()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            try
            {
                var pixels = tex.GetPixels32();
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = new Color32(200, 100, 50, 255);
                }
                tex.SetPixels32(pixels);
                tex.Apply();

                Texture2D result = null;
                var request = TextureUtility.BakeTexture(tex, true, 8, 8, false, null, (outTex) =>
                {
                    result = outTex;
                });
                request.WaitForCompletion();
                Assert.IsNotNull(result);
                Assert.AreEqual(8, result.width);
                Assert.AreEqual(8, result.height);
                Object.DestroyImmediate(result);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void ResizeTexture_ReducesSize()
        {
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            try
            {
                var pixels = tex.GetPixels32();
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = new Color32(100, 100, 100, 255);
                }
                tex.SetPixels32(pixels);
                tex.Apply();

                Texture2D result = null;
                var request = TextureUtility.ResizeTexture(tex, true, 16, 16, (outTex) =>
                {
                    result = outTex;
                });
                request.WaitForCompletion();
                Assert.IsNotNull(result);
                Assert.AreEqual(16, result.width);
                Assert.AreEqual(16, result.height);
                Object.DestroyImmediate(result);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void GetImageContentsHash_NonNull()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                var hash = TextureUtility.GetImageContentsHash(tex);
                Assert.IsNotNull(hash);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void GetImageContentsHash_Null_ReturnsValue()
        {
            var hash = TextureUtility.GetImageContentsHash(null);
            Assert.IsNotNull(hash);
        }

        [Test]
        public void DownscaleBlit_ReducesSize()
        {
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            var desc = new RenderTextureDescriptor(16, 16, RenderTextureFormat.ARGB32, 0);
            var output = RenderTexture.GetTemporary(desc);
            try
            {
                var pixels = tex.GetPixels32();
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = new Color32(128, 128, 128, 255);
                }
                tex.SetPixels32(pixels);
                tex.Apply();

                TextureUtility.DownscaleBlit(tex, true, output);
                Assert.AreEqual(16, output.width);
                Assert.AreEqual(16, output.height);
            }
            finally
            {
                RenderTexture.ReleaseTemporary(output);
                Object.DestroyImmediate(tex);
            }
        }

        // Test AspectFitReduction via reflection
        [Test]
        public void AspectFitReduction_Square()
        {
            var method = typeof(TextureUtility).GetMethod("AspectFitReduction", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
            {
                Assert.Ignore("AspectFitReduction method not found.");
                return;
            }
            var result = method.Invoke(null, new object[] { 256, 256, 128 });
            var tuple = ((int, int))result;
            Assert.AreEqual(128, tuple.Item1);
            Assert.AreEqual(128, tuple.Item2);
        }

        [Test]
        public void AspectFitReduction_Landscape()
        {
            var method = typeof(TextureUtility).GetMethod("AspectFitReduction", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
            {
                Assert.Ignore("AspectFitReduction method not found.");
                return;
            }
            var result = method.Invoke(null, new object[] { 512, 256, 256 });
            var tuple = ((int, int))result;
            Assert.LessOrEqual(tuple.Item1, 256);
            Assert.LessOrEqual(tuple.Item2, 256);
        }

        [Test]
        public void AspectFitReduction_AlreadySmaller()
        {
            var method = typeof(TextureUtility).GetMethod("AspectFitReduction", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
            {
                Assert.Ignore("AspectFitReduction method not found.");
                return;
            }
            var result = method.Invoke(null, new object[] { 64, 64, 256 });
            var tuple = ((int, int))result;
            Assert.AreEqual(64, tuple.Item1);
            Assert.AreEqual(64, tuple.Item2);
        }
    }

    [TestFixture]
    public class CacheUtilityCacheIntegrationTests
    {
        [Test]
        public void GetContentCacheKey_NonNull()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var key = CacheUtility.GetContentCacheKey(mat);
                Assert.IsNotNull(key);
                Assert.IsNotEmpty(key);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetContentCacheKey_DifferentMaterials_DifferentKeys()
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
        public void GetContentCacheKey_SameMaterial_SameKey()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                mat.color = Color.green;
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
        public void TextureCache_Constructor_AndToTexture2D_RoundTrip()
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            try
            {
                var pixels = tex.GetPixels32();
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = new Color32(128, 64, 32, 255);
                }
                tex.SetPixels32(pixels);
                tex.Apply();

                var cache = new CacheUtility.TextureCache(tex, false, false, UnityEditor.BuildTarget.StandaloneWindows64);
                var restored = cache.ToTexture2D();
                try
                {
                    Assert.IsNotNull(restored);
                    Assert.AreEqual(8, restored.width);
                    Assert.AreEqual(8, restored.height);
                }
                finally
                {
                    Object.DestroyImmediate(restored);
                }
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void TextureCache_Constructor_WithLinear()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false, true);
            try
            {
                tex.Apply();
                var cache = new CacheUtility.TextureCache(tex, true, false, UnityEditor.BuildTarget.Android);
                var restored = cache.ToTexture2D();
                try
                {
                    Assert.IsNotNull(restored);
                }
                finally
                {
                    Object.DestroyImmediate(restored);
                }
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }
    }
}
