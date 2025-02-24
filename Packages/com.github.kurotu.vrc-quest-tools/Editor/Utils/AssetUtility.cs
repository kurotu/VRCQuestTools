// <copyright file="AssetUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Utility for assets.
    /// </summary>
    internal static class AssetUtility
    {
        /// <summary>
        /// Gets version of lilToon.
        /// </summary>
        internal static readonly SemVer LilToonVersion;

        /// <summary>
        /// Type object of DynamicBone.
        /// </summary>
        internal static readonly Type DynamicBoneType = SystemUtility.GetTypeByName("DynamicBone");

        /// <summary>
        /// Type object of DynamicBoneCollider.
        /// </summary>
        internal static readonly Type DynamicBoneColliderType = SystemUtility.GetTypeByName("DynamicBoneCollider");

        private const string LilToonPackageJsonGUID = "397d2fa9e93fb5d44a9540d5f01437fc";

        private const int MinimumTextureSize = 4; // needs 4x4 for DXT.

        private static readonly HashSet<TextureFormat> UncompressedFormats = new HashSet<TextureFormat>
        {
            TextureFormat.Alpha8,
            TextureFormat.ARGB32,
            TextureFormat.ARGB4444,
            TextureFormat.BGRA32,
            TextureFormat.R16,
            TextureFormat.R8,
            TextureFormat.RFloat,
            TextureFormat.RG16,
            TextureFormat.RG32,
            TextureFormat.RGB24,
            TextureFormat.RGB48,
            TextureFormat.RGB565,
            TextureFormat.RGB9e5Float,
            TextureFormat.RGBA32,
            TextureFormat.RGBA4444,
            TextureFormat.RGBA64,
            TextureFormat.RGBAFloat,
            TextureFormat.RGBAHalf,
            TextureFormat.RGFloat,
            TextureFormat.RGHalf,
            TextureFormat.RHalf,
            TextureFormat.YUY2,
        };

        // https://docs.unity3d.com/2022.3/Documentation/Manual/class-TextureImporterOverride.html
        private static readonly HashSet<TextureFormat> WindowsFormats = new HashSet<TextureFormat>
        {
            TextureFormat.BC4,
            TextureFormat.BC5,
            TextureFormat.BC6H,
            TextureFormat.BC7,
            TextureFormat.DXT1,
            TextureFormat.DXT1Crunched,
            TextureFormat.DXT5,
            TextureFormat.DXT5Crunched,
        };

        // https://docs.unity3d.com/2022.3/Documentation/Manual/class-TextureImporterOverride.html
        // Contains "partial" formats to simplify.
        private static readonly HashSet<TextureFormat> AndroidFormats = new HashSet<TextureFormat>
        {
            TextureFormat.ASTC_4x4,
            TextureFormat.ASTC_5x5,
            TextureFormat.ASTC_6x6,
            TextureFormat.ASTC_8x8,
            TextureFormat.ASTC_10x10,
            TextureFormat.ASTC_12x12,
            TextureFormat.ASTC_HDR_4x4,
            TextureFormat.ASTC_HDR_5x5,
            TextureFormat.ASTC_HDR_6x6,
            TextureFormat.ASTC_HDR_8x8,
            TextureFormat.ASTC_HDR_10x10,
            TextureFormat.ASTC_HDR_12x12,
            TextureFormat.ETC2_RGB,
            TextureFormat.ETC2_RGBA1,
            TextureFormat.ETC2_RGBA8,
            TextureFormat.ETC2_RGBA8Crunched,
            TextureFormat.ETC_RGB4,
            TextureFormat.ETC_RGB4Crunched,
            TextureFormat.EAC_R,
            TextureFormat.EAC_R_SIGNED,
            TextureFormat.EAC_RG,
            TextureFormat.EAC_RG_SIGNED,
        };

        // https://docs.unity3d.com/2022.3/Documentation/Manual/class-TextureImporterOverride.html
        // Contains "partial" formats to simplify.
        private static readonly HashSet<TextureFormat> IosFormats = new HashSet<TextureFormat>
        {
            TextureFormat.ASTC_4x4,
            TextureFormat.ASTC_5x5,
            TextureFormat.ASTC_6x6,
            TextureFormat.ASTC_8x8,
            TextureFormat.ASTC_10x10,
            TextureFormat.ASTC_12x12,
            TextureFormat.ASTC_HDR_4x4,
            TextureFormat.ASTC_HDR_5x5,
            TextureFormat.ASTC_HDR_6x6,
            TextureFormat.ASTC_HDR_8x8,
            TextureFormat.ASTC_HDR_10x10,
            TextureFormat.ASTC_HDR_12x12,
            TextureFormat.ETC2_RGB,
            TextureFormat.ETC2_RGBA1,
            TextureFormat.ETC2_RGBA8,
            TextureFormat.ETC2_RGBA8Crunched,
            TextureFormat.ETC_RGB4,
            TextureFormat.ETC_RGB4Crunched,
            TextureFormat.EAC_R,
            TextureFormat.EAC_R_SIGNED,
            TextureFormat.EAC_RG,
            TextureFormat.EAC_RG_SIGNED,
            TextureFormat.PVRTC_RGB2,
            TextureFormat.PVRTC_RGB4,
            TextureFormat.PVRTC_RGBA2,
            TextureFormat.PVRTC_RGBA4,
        };

        static AssetUtility()
        {
            if (IsLilToonImported())
            {
                try
                {
                    var path = AssetDatabase.GUIDToAssetPath(LilToonPackageJsonGUID);
                    var str = File.ReadAllText(path);
                    var package = JsonUtility.FromJson<PackageJson>(str);
                    LilToonVersion = new SemVer(package.version);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    EditorUtility.DisplayDialog(VRCQuestTools.Name, $"Error occurred when detecting lilToon version.\nPlease report this message and the console error log.\n\n{e.GetType().Name}: {e.Message}", "OK");
                    LilToonVersion = new SemVer("0.0.0");
                }
            }
            else
            {
                LilToonVersion = new SemVer("0.0.0");
            }
        }

        /// <summary>
        /// Gets whether Dynamic Bone is imported.
        /// </summary>
        /// <returns>true when Dynamic Bone is imported.</returns>
        internal static bool IsDynamicBoneImported()
        {
            return DynamicBoneType != null;
        }

        /// <summary>
        /// Gets whether lilToon is imported.
        /// </summary>
        /// <returns>true when lilToon shader and lilToonInspector are imported.</returns>
        internal static bool IsLilToonImported()
        {
            var shader = Shader.Find("lilToon");
            var inspector = SystemUtility.GetTypeByName("lilToon.lilToonInspector");
            return (shader != null) && (inspector != null);
        }

        /// <summary>
        /// Creates a minimum empty texture.
        /// </summary>
        /// <returns>Created texture object.</returns>
        internal static Texture2D CreateMinimumEmptyTexture()
        {
            return new Texture2D(MinimumTextureSize, MinimumTextureSize);
        }

        /// <summary>
        /// Saves Texture2D as png asset.
        /// </summary>
        /// <param name="path">Path to save.</param>
        /// <param name="texture">Texture to save.</param>
        /// <param name="isSRGB">Texture is sRGB.</param>
        /// <returns>Saved texture asset.</returns>
        internal static Texture2D SaveUncompressedTexture(string path, Texture2D texture, bool isSRGB = true)
        {
            var png = texture.EncodeToPNG();
            File.WriteAllBytes(path, png);
            AssetDatabase.Refresh();
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = isSRGB;
            importer.sRGBTexture = isSRGB;
            if (importer.mipmapEnabled)
            {
                importer.streamingMipmaps = true;
            }
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        /// <summary>
        /// Loads uncompressed image as Texture2D.
        /// </summary>
        /// <param name="texture">original texture.</param>
        /// <returns>Loaded texture.</returns>
        internal static Texture LoadUncompressedTexture(Texture texture)
        {
            if (texture == null)
            {
                return null;
            }

            if (texture.GetType() == typeof(RenderTexture))
            {
                return CreateColorTexture(Color.black);
            }

            var path = AssetDatabase.GetAssetPath(texture);
            if (path == "Resources/unity_builtin_extra")
            {
                return (Texture2D)UnityEngine.Object.Instantiate(texture);
            }

            if (!(texture is Texture2D))
            {
                return UnityEngine.Object.Instantiate(texture);
            }

            // unsaved asset
            if (string.IsNullOrEmpty(path))
            {
                return texture;
            }

            // already saved as an asset file
            var extension = Path.GetExtension(path).ToLower();
            if (extension == ".asset")
            {
                return texture;
            }

            var tex2 = LoadUncompressedTexture(path, false);
            tex2.wrapMode = texture.wrapMode;
            return tex2;
        }

        /// <summary>
        /// Loads uncompressed image as Texture2D.
        /// </summary>
        /// <see href="https://github.com/lilxyzw/lilToon/issues/17">lilxyzw/lilToon#17.</see>
        /// <param name="path">path to image.</param>
        /// <param name="makeReadable">instanciate a readable one.</param>
        /// <returns>Loaded texture.</returns>
        internal static Texture2D LoadUncompressedTexture(string path, bool makeReadable)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            var extension = Path.GetExtension(path).ToLower();
            if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
            {
                var isLinear = !importer.sRGBTexture;
                var tex = new Texture2D(MinimumTextureSize, MinimumTextureSize, TextureFormat.RGBA32, Texture.GenerateAllMips, linear: isLinear);
                var bytes = File.ReadAllBytes(Path.GetFullPath(path));
                tex.LoadImage(bytes);
                tex.filterMode = FilterMode.Bilinear;
                return tex;
            }

            if (!makeReadable)
            {
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }

            const string AndroidPlatform = "Android";
            const string StandalonePlatform = "Standalone";
            var isReadable = importer.isReadable;
            var textureCompression = importer.textureCompression;
            var standaloneTextureSettings = importer.GetPlatformTextureSettings(StandalonePlatform);
            var androidTextureSettings = importer.GetPlatformTextureSettings(AndroidPlatform);

            // Set uncompressed settings.
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            if (standaloneTextureSettings.overridden)
            {
                var tmp = new TextureImporterPlatformSettings();
                standaloneTextureSettings.CopyTo(tmp);
                tmp.overridden = false;
                importer.SetPlatformTextureSettings(tmp);
            }
            if (androidTextureSettings.overridden)
            {
                var tmp = new TextureImporterPlatformSettings();
                androidTextureSettings.CopyTo(tmp);
                tmp.overridden = false;
                importer.SetPlatformTextureSettings(tmp);
            }
            importer.SaveAndReimport();

            var psd = AssetDatabase.LoadAssetAtPath<Texture>(path);
            var ret = UnityEngine.Object.Instantiate(psd);

            // Restore compression settings.
            importer.isReadable = isReadable;
            importer.textureCompression = textureCompression;
            if (standaloneTextureSettings.overridden)
            {
                importer.SetPlatformTextureSettings(standaloneTextureSettings);
            }
            if (androidTextureSettings.overridden)
            {
                importer.SetPlatformTextureSettings(androidTextureSettings);
            }
            importer.SaveAndReimport();

            if (!(ret is Texture2D))
            {
                throw new ArgumentException($"{path} is {ret.GetType().Name}");
            }

            return (Texture2D)ret;
        }

        /// <summary>
        /// Creates a single color texture.
        /// </summary>
        /// <param name="color">Color to use.</param>
        /// <param name="width">Texture width.</param>
        /// <param name="height">Texture height.</param>
        /// <returns>Created texture.</returns>
        internal static Texture2D CreateColorTexture(Color32 color, int width, int height)
        {
            var tex = new Texture2D(width, height);
            var pixels = tex.GetPixels32();
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            tex.SetPixels32(pixels);
            return tex;
        }

        /// <summary>
        /// Bake a texture to a new one.
        /// </summary>
        /// <param name="input">Input texture.</param>
        /// <param name="width">Desired width.</param>
        /// <param name="height">Desired height.</param>
        /// <param name="useMipmap">Use mip map.</param>
        /// <returns>New texture.</returns>
        internal static Texture2D BakeTexture(Texture input, int width, int height, bool useMipmap)
        {
            return BakeTexture(input, null, width, height, useMipmap);
        }

        /// <summary>
        /// Bake a texture to a new one.
        /// </summary>
        /// <param name="input">Input texture.</param>
        /// <param name="material">Material to bake.</param>
        /// <param name="width">Desired width.</param>
        /// <param name="height">Desired height.</param>
        /// <param name="useMipmap">Use mip map.</param>
        /// <returns>New texture.</returns>
        internal static Texture2D BakeTexture(Texture input, Material material, int width, int height, bool useMipmap)
        {
            var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            var activeRT = RenderTexture.active;
            try
            {
                if (material)
                {
                    Graphics.Blit(input, rt, material);
                }
                else
                {
                    Graphics.Blit(input, rt);
                }

                var result = new Texture2D(width, height, TextureFormat.RGBA32, useMipmap);
                if (SystemInfo.supportsAsyncGPUReadback)
                {
                    var request = AsyncGPUReadback.Request(rt, 0, TextureFormat.RGBA32);
                    request.WaitForCompletion();
                    if (useMipmap)
                    {
                        var mipmapCount = rt.mipmapCount;
                        for (int i = 0; i < mipmapCount; i++)
                        {
                            using (var data = request.GetData<Color32>(i))
                            {
                                result.SetPixelData(data, i);
                            }
                        }
                    }
                    else
                    {
                        using (var data = request.GetData<Color32>())
                        {
                            result.LoadRawTextureData(data);
                        }
                    }
                }
                else
                {
                    result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                }
                result.Apply();
                return result;
            }
            finally
            {
                RenderTexture.active = activeRT;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        /// <summary>
        /// Resizes a texture to desired size.
        /// </summary>
        /// <param name="texture">Texture to resize.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <returns>Resized texture.</returns>
        internal static Texture2D ResizeTexture(Texture2D texture, int width, int height)
        {
            return BakeTexture(texture, width, height, texture.mipmapCount > 1);
        }

        /// <summary>
        /// Compresses a texture for the build target.
        /// </summary>
        /// <param name="texture">Texture to compress.</param>
        /// <param name="buildTarget">Build target. Usually it's EditorUserBuildSettings.activeBuildTarget.</param>
        internal static void CompressTextureForBuildTarget(Texture2D texture, UnityEditor.BuildTarget buildTarget)
        {
            var isMobile = buildTarget == UnityEditor.BuildTarget.Android || buildTarget == UnityEditor.BuildTarget.iOS;
            var format = isMobile ? TextureFormat.ASTC_6x6 : TextureFormat.DXT5;
            EditorUtility.CompressTexture(texture, format, TextureCompressionQuality.Best);
        }

        /// <summary>
        /// Creates a single color 4x4 texture.
        /// </summary>
        /// <param name="color">Color to use.</param>
        /// <returns>Created texture.</returns>
        internal static Texture2D CreateColorTexture(Color32 color)
        {
            return CreateColorTexture(color, 4, 4);
        }

        /// <summary>
        /// Returns whether the texture format is known by the tool.
        /// </summary>
        /// <param name="format">Texture format.</param>
        /// <returns>true when the format is known by the tool.</returns>
        internal static bool IsKnownTextureFormat(TextureFormat format)
        {
            return UncompressedFormats.Contains(format) || WindowsFormats.Contains(format) || AndroidFormats.Contains(format) || IosFormats.Contains(format);
        }

        /// <summary>
        /// Returns whether the texture format is supported by the build target.
        /// </summary>
        /// <param name="format">Texture format.</param>
        /// <param name="buildTarget">Build target.</param>
        /// <returns>true when the format is supported.</returns>
        internal static bool IsSupportedTextureFormat(TextureFormat format, UnityEditor.BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case UnityEditor.BuildTarget.StandaloneWindows:
                case UnityEditor.BuildTarget.StandaloneWindows64:
                    return UncompressedFormats.Contains(format) || WindowsFormats.Contains(format);
                case UnityEditor.BuildTarget.Android:
                    return UncompressedFormats.Contains(format) || AndroidFormats.Contains(format);
                case UnityEditor.BuildTarget.iOS:
                    return UncompressedFormats.Contains(format) || IosFormats.Contains(format);
                default:
                    throw new NotSupportedException($"Unsupported build target: {buildTarget}");
            }
        }

        /// <summary>
        /// Returns whether the texture format is uncompressed.
        /// </summary>
        /// <param name="format">Texture format.</param>
        /// <returns>true when the format is for uncompressed.</returns>
        internal static bool IsUncompressedFormat(TextureFormat format)
        {
            return UncompressedFormats.Contains(format);
        }

        /// <summary>
        /// Sets whether the texture uses streaming mipmaps.
        /// </summary>
        /// <param name="texture">Texture.</param>
        /// <param name="useStreamingMipmaps">Streaming Mipmaps.</param>
        internal static void SetStreamingMipMaps(Texture2D texture, bool useStreamingMipmaps)
        {
            var so = new SerializedObject(texture);
            so.Update();
            var streamingMipmaps = so.FindProperty("m_StreamingMipmaps");
            streamingMipmaps.boolValue = useStreamingMipmaps;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Gets whether the texture is a normal map asset.
        /// </summary>
        /// <param name="texture">Texture.</param>
        /// <returns>true when the texture is a normal map. false when it's not an asset.</returns>
        internal static bool IsNormalMapAsset(Texture texture)
        {
            var path = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return false;
            }
            return importer.textureType == TextureImporterType.NormalMap;
        }

        /// <summary>
        /// Destroys a texture if it's not an asset.
        /// </summary>
        /// <param name="texture">Texture.</param>
        internal static void DestroyTexture(Texture texture)
        {
            if (texture == null)
            {
                return;
            }

            if (AssetDatabase.GetAssetPath(texture) != null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(texture);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        /// <summary>
        /// Create a new asset. If the path already exists, it will be overwritten.
        /// </summary>
        /// <typeparam name="T">Type of asset.</typeparam>
        /// <param name="asset">Asset to save.</param>
        /// <param name="path">Path to save.</param>
        /// <param name="postCreateAction">Action to execute after AssetDatabase.CreateAsset() for further objects.</param>
        /// <returns>Created asset.</returns>
        /// <remarks>Do not use `asset` object after this method. Use the returned object.</remarks>
        internal static T CreateAsset<T>(T asset, string path, Action<T> postCreateAction = null)
            where T : UnityEngine.Object
        {
            if (!File.Exists(path))
            {
                AssetDatabase.CreateAsset(asset, path);
                postCreateAction?.Invoke(asset);
                AssetDatabase.SaveAssets();
                return AssetDatabase.LoadAssetAtPath<T>(path);
            }

            var file = Path.GetFileName(path);
            var tmpDir = $"Assets/tmp_vqt";
            var tmpPath = $"{tmpDir}/{file}";

            try
            {
                if (!Directory.Exists(tmpDir))
                {
                    Directory.CreateDirectory(tmpDir);
                }
                AssetDatabase.CreateAsset(asset, tmpPath);
                postCreateAction?.Invoke(asset);
                AssetDatabase.SaveAssets();
                File.Copy(tmpPath, path, true);
            }
            catch (Exception e)
            {
                ExceptionDispatchInfo.Capture(e).Throw();
            }
            finally
            {
                if (File.Exists(tmpPath))
                {
                    File.Delete(tmpPath);
                }
                if (File.Exists($"{tmpPath}.meta"))
                {
                    File.Delete($"{tmpPath}.meta");
                }
                if (Directory.Exists(tmpDir))
                {
                    Directory.Delete(tmpDir, true);
                }
                if (File.Exists($"{tmpDir}.meta"))
                {
                    File.Delete($"{tmpDir}.meta");
                }
            }

            AssetDatabase.Refresh();
            var newAsset = AssetDatabase.LoadAssetAtPath<T>(path);
            return newAsset;
        }

        /// <summary>
        /// Get all object references in the object.
        /// </summary>
        /// <param name="o">Object.</param>
        /// <returns>All referenced objects.</returns>
        internal static UnityEngine.Object[] GetAllObjectReferences(UnityEngine.Object o)
        {
            var list = new HashSet<UnityEngine.Object>();
            GetAllObjectReferencesImpl(o, list);
            return list.ToArray();
        }

        private static void GetAllObjectReferencesImpl(UnityEngine.Object o, HashSet<UnityEngine.Object> list)
        {
            var so = new SerializedObject(o);
            var itr = so.GetIterator();
            while (itr.Next(true))
            {
                if (itr.propertyType == SerializedPropertyType.ObjectReference)
                {
                    var obj = itr.objectReferenceValue;
                    if (obj != null && !list.Contains(obj))
                    {
                        list.Add(obj);
                        GetAllObjectReferencesImpl(obj, list);
                    }
                }
            }
        }

        [Serializable]
        private class PackageJson
        {
            /// <summary>
            /// package version.
            /// </summary>
            public string version;
        }
    }
}
