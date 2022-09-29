// <copyright file="AssetUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.IO;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

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
        internal static Type DynamicBoneType = SystemUtility.GetTypeByName("DynamicBone");

        private const string LilToonPackageJsonGUID = "397d2fa9e93fb5d44a9540d5f01437fc";

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
        internal static Texture2D LoadUncompressedTexture(Texture texture)
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

            var tex2 = LoadUncompressedTexture(path);
            tex2.wrapMode = texture.wrapMode;
            return tex2;
        }

        /// <summary>
        /// Loads uncompressed image as Texture2D.
        /// </summary>
        /// <see href="https://github.com/lilxyzw/lilToon/issues/17">lilxyzw/lilToon#17.</see>
        /// <param name="path">path to image.</param>
        /// <returns>Loaded texture.</returns>
        internal static Texture2D LoadUncompressedTexture(string path)
        {
            var extension = Path.GetExtension(path).ToLower();
            if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
            {
                var tex = new Texture2D(2, 2);
                var bytes = File.ReadAllBytes(Path.GetFullPath(path));
                tex.LoadImage(bytes);
                tex.filterMode = FilterMode.Bilinear;
                return tex;
            }

            const string AndroidPlatform = "Android";
            const string StandalonePlatform = "Standalone";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
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
        /// Resizes a texture to desired size.
        /// </summary>
        /// <param name="texture">Texture to resize.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <returns>Resized texture.</returns>
        internal static Texture2D ResizeTexture(Texture2D texture, int width, int height)
        {
            using (var rt = DisposableObject.New(new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)))
            {
                var activeRT = RenderTexture.active;

                RenderTexture.active = rt.Object;
                Graphics.Blit(texture, rt.Object);
                var result = new Texture2D(width, height);
                result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                result.Apply();

                RenderTexture.active = activeRT;
                return result;
            }
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
