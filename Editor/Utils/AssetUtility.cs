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

        static AssetUtility()
        {
            if (IsLilToonImported())
            {
                var str = File.ReadAllText("Assets/lilToon/package.json");
                var package = JsonUtility.FromJson<PackageJson>(str);
                LilToonVersion = new SemVer(package.version);
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
                var tex = new Texture2D(2, 2);
                var pixels = tex.GetPixels32();
                for (var i = 0; i < pixels.Length; i++)
                {
                    pixels[i].r = 0;
                    pixels[i].g = 0;
                    pixels[i].b = 0;
                }
                tex.SetPixels32(pixels);
                return tex;
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

            var psd = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
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

            return ret;
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
