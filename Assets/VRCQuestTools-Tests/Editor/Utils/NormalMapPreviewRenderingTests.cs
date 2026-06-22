// <copyright file="NormalMapPreviewRenderingTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Tests for the in-memory normal map preview rendering issue and its countermeasure.
    ///
    /// Root cause: a normal map produced by TextureGenerator (CompressNormalMap) is not uploaded to the GPU
    /// for display, so a freshly generated in-memory normal map renders incorrectly in the NDMF preview. The
    /// same data renders correctly once re-uploaded (the editor displays the compressed normal map - DXT5nm on
    /// desktop, ASTC on Android/iOS - correctly after a Texture2D.Apply). Cache-restored textures are already
    /// uploaded via Apply. The actual mobile build is unaffected.
    ///
    /// Countermeasure: <see cref="TextureUtility.ReuploadForEditorDisplay"/> re-uploads a freshly generated
    /// normal map (keeping its format) so that the editor preview renders the same result as the asset-imported
    /// (baked) texture.
    ///
    /// NDMF preview path:  CompressNormalMap -> in-memory Texture2D -> ReuploadForEditorDisplay -> material.
    /// NDMF bake path:     CompressNormalMap -> AssetDatabase.CreateAsset -> .asset -> loaded back -> material.
    /// </summary>
    public class NormalMapPreviewRenderingTests
    {
#if UNITY_EDITOR_WIN
        private const float Threshold = 0.1f;
#else
        private const float Threshold = 0.2f;
#endif

        private string tempPath;

        /// <summary>
        /// Sets up a temporary directory for baked texture assets.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            tempPath = $"Assets/VRCQuestTools-Tests/Temp/{System.Guid.NewGuid():N}";
            Directory.CreateDirectory(tempPath);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Removes the temporary directory and its assets, and resets the global log-assert flag.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            // Always reset the global flag so an exception mid-test cannot leave it enabled and mask
            // failures in subsequent tests.
            LogAssert.ignoreFailingMessages = false;
            if (string.IsNullOrEmpty(tempPath))
            {
                return;
            }

            if (AssetDatabase.IsValidFolder(tempPath))
            {
                AssetDatabase.DeleteAsset(tempPath);
            }
            else if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }

        /// <summary>
        /// Verifies that a freshly generated normal map (no cache) re-uploaded for editor display renders
        /// correctly for the current active build target. This reproduces the NDMF preview first-time generation
        /// path: the CompressNormalMap output is not uploaded to the GPU for display until re-uploaded. A failure
        /// indicates the fresh normal map is not reflected in the preview.
        /// </summary>
        [Test]
        public void FreshNormalMap_PreparedForEditorDisplay_RendersLikeSource()
        {
            var sourceNormalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/VRCQuestTools-Tests/Fixtures/Textures/NormalMapSample01.png");
            Assert.IsNotNull(sourceNormalMap, "Failed to load NormalMapSample01.png.");

            LogAssert.ignoreFailingMessages = true;
            var downscaled = TextureUtility.DownscaleNormalMap(
                sourceNormalMap, true, sourceNormalMap.width, sourceNormalMap.height);

            // Generate for the active build target (ASTC for mobile, DXT5nm for desktop) like the preview does.
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var fresh = TextureUtility.CompressNormalMap(downscaled, buildTarget, TextureFormat.ASTC_8x8);

            // Prepare for editor display the same way MaterialGeneratorUtility does for a freshly generated texture.
            var prepared = TextureUtility.ReuploadForEditorDisplay(fresh);

            var sourceRendered = RenderWithNormalMap(sourceNormalMap);
            var preparedRendered = RenderWithNormalMap(prepared);
            LogAssert.ignoreFailingMessages = false;

            var diff = TestUtils.MaxDifference(sourceRendered, preparedRendered);
            Assert.Less(diff, Threshold, $"Freshly generated normal map re-uploaded for editor display doesn't match source rendering (diff={diff:F4}, target={buildTarget}).");
        }

        /// <summary>
        /// Verifies that the re-uploaded in-memory normal map (NDMF preview path with countermeasure)
        /// and the .asset-serialized normal map (NDMF bake path) render equivalently to the source
        /// normal map when applied to a Standard material.
        /// </summary>
        [Test]
        public void PreviewNormalMap_And_BakedNormalMap_RenderSimilarly()
        {
            // Load source normal map as an asset (may be DXT5nm on Windows).
            var sourceNormalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/VRCQuestTools-Tests/Fixtures/Textures/NormalMapSample01.png");
            Assert.IsNotNull(sourceNormalMap, "Failed to load NormalMapSample01.png.");

            // Apply DownscaleNormalMap as ToonStandard does for mobile (outputRGB=true when isMobile=true).
            // This decodes the source (e.g. DXT5nm) into a linear RGB Texture2D suitable for CompressNormalMap.
            LogAssert.ignoreFailingMessages = true;
            var downscaled = TextureUtility.DownscaleNormalMap(
                sourceNormalMap, true, sourceNormalMap.width, sourceNormalMap.height);

            // NDMF preview path: a freshly generated CompressNormalMap output is re-uploaded for editor display.
            var previewCompressed = TextureUtility.CompressNormalMap(
                downscaled, UnityEditor.BuildTarget.Android, TextureFormat.ASTC_8x8);
            var previewNormal = TextureUtility.ReuploadForEditorDisplay(previewCompressed);

            // NDMF bake path: same CompressNormalMap output is serialized to .asset and reloaded.
            var inMemoryNormal = TextureUtility.CompressNormalMap(
                downscaled, UnityEditor.BuildTarget.Android, TextureFormat.ASTC_8x8);
            LogAssert.ignoreFailingMessages = false;

            var assetPath = $"{tempPath}/normal_baked.asset";
            AssetDatabase.CreateAsset(inMemoryNormal, assetPath);
            AssetDatabase.SaveAssets();
            var bakedNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            Assert.IsNotNull(bakedNormal, "Failed to load baked normal map .asset.");

            // Render all three: source (reference), preview, and baked.
            LogAssert.ignoreFailingMessages = true;
            var sourceRendered = RenderWithNormalMap(sourceNormalMap);
            var previewRendered = RenderWithNormalMap(previewNormal);
            var bakedRendered = RenderWithNormalMap(bakedNormal);
            LogAssert.ignoreFailingMessages = false;

            // Compare each result against the source rendering (ground truth).
            var diffSourceVsPreview = TestUtils.MaxDifference(sourceRendered, previewRendered);
            var diffSourceVsBaked = TestUtils.MaxDifference(sourceRendered, bakedRendered);
            var diffPreviewVsBaked = TestUtils.MaxDifference(previewRendered, bakedRendered);

            var msgPreview = $"Re-uploaded preview normal map doesn't match source rendering (diff={diffSourceVsPreview:F4}). Baked diff from source: {diffSourceVsBaked:F4}.";
            Assert.Less(diffSourceVsPreview, Threshold, msgPreview);
            var msgBaked = $"Baked normal map doesn't match source rendering (diff={diffSourceVsBaked:F4}). Preview diff from source: {diffSourceVsPreview:F4}.";
            Assert.Less(diffSourceVsBaked, Threshold, msgBaked);
            var msgPreviewVsBaked = $"Re-uploaded preview and baked normal maps should render the same (diff={diffPreviewVsBaked:F4}).";
            Assert.Less(diffPreviewVsBaked, Threshold, msgPreviewVsBaked);
        }

        /// <summary>
        /// Verifies that a cached/restored normal map renders equivalently to the source rendering.
        /// This reproduces the NDMF preview cache reuse path: CompressNormalMap -> TextureCache serialize ->
        /// TextureCache.ToTexture2D. The cache-restored texture is already uploaded via Apply, so it is used
        /// directly, while the freshly generated preview is re-uploaded.
        /// </summary>
        [Test]
        public void CachedPreviewNormalMap_RenderSimilarlyToSource()
        {
            var sourceNormalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/VRCQuestTools-Tests/Fixtures/Textures/NormalMapSample01.png");
            Assert.IsNotNull(sourceNormalMap, "Failed to load NormalMapSample01.png.");

            LogAssert.ignoreFailingMessages = true;
            var downscaled = TextureUtility.DownscaleNormalMap(
                sourceNormalMap, true, sourceNormalMap.width, sourceNormalMap.height);
            var previewCompressed = TextureUtility.CompressNormalMap(
                downscaled, UnityEditor.BuildTarget.Android, TextureFormat.ASTC_8x8);
            var cache = new CacheUtility.TextureCache(previewCompressed, true, true, UnityEditor.BuildTarget.Android);

            // Freshly generated preview is re-uploaded; cache-restored texture is already uploaded (used directly).
            var previewNormal = TextureUtility.ReuploadForEditorDisplay(previewCompressed);
            var cachedNormal = cache.ToTexture2D();
            LogAssert.ignoreFailingMessages = false;

            LogAssert.ignoreFailingMessages = true;
            var sourceRendered = RenderWithNormalMap(sourceNormalMap);
            var previewRendered = RenderWithNormalMap(previewNormal);
            var cachedRendered = RenderWithNormalMap(cachedNormal);
            LogAssert.ignoreFailingMessages = false;

            var diffSourceVsPreview = TestUtils.MaxDifference(sourceRendered, previewRendered);
            var diffSourceVsCached = TestUtils.MaxDifference(sourceRendered, cachedRendered);
            var diffPreviewVsCached = TestUtils.MaxDifference(previewRendered, cachedRendered);

            var msgCached = $"Cached normal map doesn't match source rendering (diff={diffSourceVsCached:F4}). Preview diff from source: {diffSourceVsPreview:F4}, preview-vs-cached diff: {diffPreviewVsCached:F4}.";
            Assert.Less(diffSourceVsCached, Threshold, msgCached);
            var msgPreviewVsCached = $"Cached normal map should match preview rendering (diff={diffPreviewVsCached:F4}). Preview diff from source: {diffSourceVsPreview:F4}, cached diff from source: {diffSourceVsCached:F4}.";
            Assert.Less(diffPreviewVsCached, Threshold, msgPreviewVsCached);
        }

        /// <summary>
        /// Verifies order-dependent behavior where generated preview texture is used first and
        /// cache-restored texture is used second. The freshly generated one is re-uploaded; the cache-restored
        /// one is used directly.
        /// </summary>
        [Test]
        public void CacheReuseOrder_GeneratedThenReused_RenderSimilarlyToSource()
        {
            var sourceNormalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/VRCQuestTools-Tests/Fixtures/Textures/NormalMapSample01.png");
            Assert.IsNotNull(sourceNormalMap, "Failed to load NormalMapSample01.png.");

            LogAssert.ignoreFailingMessages = true;
            var downscaled = TextureUtility.DownscaleNormalMap(
                sourceNormalMap, true, sourceNormalMap.width, sourceNormalMap.height);
            var generatedFirst = TextureUtility.ReuploadForEditorDisplay(TextureUtility.CompressNormalMap(
                downscaled, UnityEditor.BuildTarget.Android, TextureFormat.ASTC_8x8));
            var cacheSource = TextureUtility.CompressNormalMap(
                downscaled, UnityEditor.BuildTarget.Android, TextureFormat.ASTC_8x8);
            var cache = new CacheUtility.TextureCache(cacheSource, true, true, UnityEditor.BuildTarget.Android);
            var reusedSecond = cache.ToTexture2D();

            var sourceRendered = RenderWithNormalMap(sourceNormalMap);
            var generatedRendered = RenderWithNormalMap(generatedFirst);
            var reusedRendered = RenderWithNormalMap(reusedSecond);
            LogAssert.ignoreFailingMessages = false;

            var diffSourceVsGenerated = TestUtils.MaxDifference(sourceRendered, generatedRendered);
            var diffSourceVsReused = TestUtils.MaxDifference(sourceRendered, reusedRendered);
            var diffGeneratedVsReused = TestUtils.MaxDifference(generatedRendered, reusedRendered);

            Assert.Less(diffSourceVsGenerated, Threshold, $"Generated-first normal map doesn't match source rendering (diff={diffSourceVsGenerated:F4}).");
            Assert.Less(diffSourceVsReused, Threshold, $"Reused-second normal map doesn't match source rendering (diff={diffSourceVsReused:F4}).");
            Assert.Less(diffGeneratedVsReused, Threshold, $"Generated-first and reused-second renderings should match (diff={diffGeneratedVsReused:F4}).");
        }

        /// <summary>
        /// Verifies order-dependent behavior where cache-restored texture is used first and
        /// generated preview texture is used second. The cache-restored one is used directly; the freshly
        /// generated one is re-uploaded.
        /// </summary>
        [Test]
        public void CacheReuseOrder_ReusedThenGenerated_RenderSimilarlyToSource()
        {
            var sourceNormalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/VRCQuestTools-Tests/Fixtures/Textures/NormalMapSample01.png");
            Assert.IsNotNull(sourceNormalMap, "Failed to load NormalMapSample01.png.");

            LogAssert.ignoreFailingMessages = true;
            var downscaled = TextureUtility.DownscaleNormalMap(
                sourceNormalMap, true, sourceNormalMap.width, sourceNormalMap.height);
            var cacheSource = TextureUtility.CompressNormalMap(
                downscaled, UnityEditor.BuildTarget.Android, TextureFormat.ASTC_8x8);
            var cache = new CacheUtility.TextureCache(cacheSource, true, true, UnityEditor.BuildTarget.Android);
            var reusedFirst = cache.ToTexture2D();
            var generatedSecond = TextureUtility.ReuploadForEditorDisplay(TextureUtility.CompressNormalMap(
                downscaled, UnityEditor.BuildTarget.Android, TextureFormat.ASTC_8x8));

            var sourceRendered = RenderWithNormalMap(sourceNormalMap);
            var reusedRendered = RenderWithNormalMap(reusedFirst);
            var generatedRendered = RenderWithNormalMap(generatedSecond);
            LogAssert.ignoreFailingMessages = false;

            var diffSourceVsReused = TestUtils.MaxDifference(sourceRendered, reusedRendered);
            var diffSourceVsGenerated = TestUtils.MaxDifference(sourceRendered, generatedRendered);
            var diffReusedVsGenerated = TestUtils.MaxDifference(reusedRendered, generatedRendered);

            Assert.Less(diffSourceVsReused, Threshold, $"Reused-first normal map doesn't match source rendering (diff={diffSourceVsReused:F4}).");
            Assert.Less(diffSourceVsGenerated, Threshold, $"Generated-second normal map doesn't match source rendering (diff={diffSourceVsGenerated:F4}).");
            Assert.Less(diffReusedVsGenerated, Threshold, $"Reused-first and generated-second renderings should match (diff={diffReusedVsGenerated:F4}).");
        }

        /// <summary>
        /// Compares channel decode assumptions (RGB vs AG) and verifies that the preview/cache textures follow
        /// the same decode model (RGB) as the baked texture.
        /// </summary>
        [Test]
        public void ChannelDecodeModel_PreviewAndCacheMatchBaked()
        {
            var sourceNormalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/VRCQuestTools-Tests/Fixtures/Textures/NormalMapSample01.png");
            Assert.IsNotNull(sourceNormalMap, "Failed to load NormalMapSample01.png.");

            LogAssert.ignoreFailingMessages = true;
            var downscaled = TextureUtility.DownscaleNormalMap(
                sourceNormalMap, true, sourceNormalMap.width, sourceNormalMap.height);
            var previewCompressed = TextureUtility.CompressNormalMap(
                downscaled, UnityEditor.BuildTarget.Android, TextureFormat.ASTC_8x8);
            var inMemoryNormal = TextureUtility.CompressNormalMap(
                downscaled, UnityEditor.BuildTarget.Android, TextureFormat.ASTC_8x8);
            var cache = new CacheUtility.TextureCache(previewCompressed, true, true, UnityEditor.BuildTarget.Android);
            var previewNormal = TextureUtility.ReuploadForEditorDisplay(previewCompressed);
            var cachedNormal = cache.ToTexture2D();
            LogAssert.ignoreFailingMessages = false;

            var assetPath = $"{tempPath}/normal_model_check.asset";
            AssetDatabase.CreateAsset(inMemoryNormal, assetPath);
            AssetDatabase.SaveAssets();
            var bakedNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            Assert.IsNotNull(bakedNormal, "Failed to load baked normal map .asset.");

            LogAssert.ignoreFailingMessages = true;
            var downscaledRaw = RenderAsColorTexture(downscaled);
            var previewRaw = RenderAsColorTexture(previewNormal);
            var cachedRaw = RenderAsColorTexture(cachedNormal);
            var bakedRaw = RenderAsColorTexture(bakedNormal);
            LogAssert.ignoreFailingMessages = false;

            var previewRgbError = DecodedNormalErrorFromDownscaled(downscaledRaw, previewRaw, decodeRgb: true);
            var previewAgError = DecodedNormalErrorFromDownscaled(downscaledRaw, previewRaw, decodeRgb: false);
            var cachedRgbError = DecodedNormalErrorFromDownscaled(downscaledRaw, cachedRaw, decodeRgb: true);
            var cachedAgError = DecodedNormalErrorFromDownscaled(downscaledRaw, cachedRaw, decodeRgb: false);
            var bakedRgbError = DecodedNormalErrorFromDownscaled(downscaledRaw, bakedRaw, decodeRgb: true);
            var bakedAgError = DecodedNormalErrorFromDownscaled(downscaledRaw, bakedRaw, decodeRgb: false);

            var bakedUsesRgb = bakedRgbError <= bakedAgError;
            var previewUsesRgb = previewRgbError <= previewAgError;
            var cachedUsesRgb = cachedRgbError <= cachedAgError;
            Debug.Log($"[NormalMapPreviewRenderingTests] decode errors: preview RGB/AG={previewRgbError:F4}/{previewAgError:F4}, cached RGB/AG={cachedRgbError:F4}/{cachedAgError:F4}, baked RGB/AG={bakedRgbError:F4}/{bakedAgError:F4}");

            Assert.IsTrue(bakedUsesRgb, $"Baked normal map should follow RGB decode model but RGB/AG error={bakedRgbError:F4}/{bakedAgError:F4}.");
            var msgPreview = $"Preview decode model mismatch. Preview RGB/AG error={previewRgbError:F4}/{previewAgError:F4}, Baked RGB/AG error={bakedRgbError:F4}/{bakedAgError:F4}.";
            Assert.AreEqual(bakedUsesRgb, previewUsesRgb, msgPreview);
            var msgCached = $"Cached decode model mismatch. Cached RGB/AG error={cachedRgbError:F4}/{cachedAgError:F4}, Baked RGB/AG error={bakedRgbError:F4}/{bakedAgError:F4}.";
            Assert.AreEqual(bakedUsesRgb, cachedUsesRgb, msgCached);
        }

        /// <summary>
        /// Renders a sphere with the given normal map texture using a camera and returns the captured pixels.
        /// </summary>
        /// <param name="normalMap">Normal map texture to apply to the Standard material's BumpMap slot.</param>
        /// <returns>Rendered Texture2D containing captured pixels.</returns>
        private static Texture2D RenderWithNormalMap(Texture2D normalMap)
        {
            GameObject sphere = null;
            Material mat = null;
            GameObject lightGo = null;
            GameObject camGo = null;
            RenderTexture rt = null;
            var prevRT = RenderTexture.active;
            try
            {
                // Sphere with Standard material using the normal map.
                sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = Vector3.zero;

                mat = new Material(Shader.Find("Standard"));
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", normalMap);
                mat.SetFloat("_BumpScale", 1f);
                sphere.GetComponent<MeshRenderer>().sharedMaterial = mat;

                // Directional light to make normal map shading visible.
                lightGo = new GameObject("TestLight");
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.transform.rotation = Quaternion.Euler(45f, 45f, 0f);
                light.intensity = 1f;

                // Camera facing the sphere from the front.
                camGo = new GameObject("TestCamera");
                var cam = camGo.AddComponent<Camera>();
                cam.transform.position = new Vector3(0f, 0f, -3f);
                cam.transform.LookAt(sphere.transform);
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;

                // Render to a 256x256 RenderTexture and read back pixels.
                rt = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
                cam.targetTexture = rt;
                cam.Render();

                RenderTexture.active = rt;
                var result = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                result.ReadPixels(new Rect(0f, 0f, 256f, 256f), 0, 0);
                result.Apply();
                cam.targetTexture = null;
                return result;
            }
            finally
            {
                RenderTexture.active = prevRT;
                if (rt != null) { Object.DestroyImmediate(rt); }
                if (mat != null) { Object.DestroyImmediate(mat); }
                if (sphere != null) { Object.DestroyImmediate(sphere); }
                if (lightGo != null) { Object.DestroyImmediate(lightGo); }
                if (camGo != null) { Object.DestroyImmediate(camGo); }
            }
        }

        private static Texture2D RenderAsColorTexture(Texture2D texture)
        {
            GameObject quad = null;
            Material mat = null;
            GameObject camGo = null;
            RenderTexture rt = null;
            var prevRT = RenderTexture.active;
            try
            {
                quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.position = Vector3.zero;

                mat = new Material(Shader.Find("Unlit/Texture"));
                mat.SetTexture("_MainTex", texture);
                quad.GetComponent<MeshRenderer>().sharedMaterial = mat;

                camGo = new GameObject("RawTextureCamera");
                var cam = camGo.AddComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 0.5f;
                cam.transform.position = new Vector3(0f, 0f, -3f);
                cam.transform.LookAt(quad.transform);
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;

                rt = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
                cam.targetTexture = rt;
                cam.Render();

                RenderTexture.active = rt;
                var result = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                result.ReadPixels(new Rect(0f, 0f, 256f, 256f), 0, 0);
                result.Apply();
                cam.targetTexture = null;
                return result;
            }
            finally
            {
                RenderTexture.active = prevRT;
                if (rt != null) { Object.DestroyImmediate(rt); }
                if (mat != null) { Object.DestroyImmediate(mat); }
                if (quad != null) { Object.DestroyImmediate(quad); }
                if (camGo != null) { Object.DestroyImmediate(camGo); }
            }
        }

        private static float DecodedNormalErrorFromDownscaled(Texture2D downscaledRaw, Texture2D targetRaw, bool decodeRgb)
        {
            var xs = new int[] { 64, 128, 192 };
            var ys = new int[] { 64, 128, 192 };
            float errorSum = 0f;
            int count = 0;
            foreach (var x in xs)
            {
                foreach (var y in ys)
                {
                    var reference = DecodeRgbNormal(downscaledRaw.GetPixel(x, y));
                    var target = decodeRgb ? DecodeRgbNormal(targetRaw.GetPixel(x, y)) : DecodeAgNormal(targetRaw.GetPixel(x, y));
                    errorSum += Vector3.Distance(reference, target);
                    count++;
                }
            }
            return errorSum / count;
        }

        private static Vector3 DecodeRgbNormal(Color color)
        {
            var normal = new Vector3((color.r * 2f) - 1f, (color.g * 2f) - 1f, (color.b * 2f) - 1f);
            return normal.sqrMagnitude > 0f ? normal.normalized : Vector3.forward;
        }

        private static Vector3 DecodeAgNormal(Color color)
        {
            var x = (color.a * 2f) - 1f;
            var y = (color.g * 2f) - 1f;
            var z = Mathf.Sqrt(Mathf.Clamp01(1f - (x * x) - (y * y)));
            return new Vector3(x, y, z);
        }
    }
}
