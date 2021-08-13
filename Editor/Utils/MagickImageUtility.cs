// <copyright file="MagickImageUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using ImageMagick;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// MagickImage manipulation utility.
    /// </summary>
    public static class MagickImageUtility
    {
        /// <summary>
        /// Generates Metallic(R) Smoothness(A) texture.
        /// </summary>
        /// <param name="metallic">Metallic map.</param>
        /// <param name="invertMetallic">Whether invert metallic map.</param>
        /// <param name="smoothness">Smoothness or roughness map.</param>
        /// <param name="invertSmoothness">Whether invert smoothness map.</param>
        /// <returns>Generated MSMap.</returns>
        public static MagickImage GenerateMetallicSmoothness(MagickImage metallic, bool invertMetallic, MagickImage smoothness, bool invertSmoothness)
        {
            var width = Math.Max(metallic.Width, smoothness.Width);
            var height = Math.Max(metallic.Height, smoothness.Height);
            using (var m = new MagickImage(metallic))
            using (var s = new MagickImage(smoothness))
            using (var green = new MagickImage(MagickColors.Black, metallic.Width, metallic.Height))
            using (var blue = new MagickImage(MagickColors.Black, metallic.Width, metallic.Height))
            using (var rgb = new MagickImageCollection())
            {
                m.ColorSpace = ImageMagick.ColorSpace.Gray;
                m.Resize(width, height);
                if (invertMetallic)
                {
                    m.Negate();
                }

                s.ColorSpace = ImageMagick.ColorSpace.Gray;
                s.Resize(width, height);
                if (invertSmoothness)
                {
                    s.Negate();
                }

                rgb.Add(m);
                rgb.Add(green);
                rgb.Add(blue);
                var rgba = (MagickImage)rgb.Combine(ImageMagick.ColorSpace.sRGB);
                rgba.Alpha(AlphaOption.On);
                rgba.Composite(s, CompositeOperator.CopyAlpha);
                return rgba;
            }
        }

        /// <summary>
        /// Save MagickImage as Unity asset.
        /// </summary>
        /// <param name="path">Path to save.</param>
        /// <param name="image">Image to save.</param>
        /// <param name="format">Format to write.</param>
        /// <param name="isSRGB">Whether the image is sRGB.</param>
        internal static void SaveAsAsset(string path, MagickImage image, MagickFormat format, bool isSRGB)
        {
            image.Write(path, format);
            AssetDatabase.Refresh();
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.sRGBTexture = isSRGB;
            importer.alphaIsTransparency = isSRGB;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.SaveAndReimport();
        }

        /// <summary>
        /// Get MagickImage from Unity Texture.
        /// </summary>
        /// <param name="texture">Unity Texture.</param>
        /// <returns>Loaded MagickImage.</returns>
        internal static MagickImage GetMagickImage(Texture texture)
        {
            if (texture == null)
            {
                return null;
            }

            if (texture.GetType() == typeof(RenderTexture))
            {
                return new MagickImage(MagickColors.Black, 1, 1);
            }

            var path = AssetDatabase.GetAssetPath(texture);
            try
            {
                if (path == "Resources/unity_builtin_extra")
                {
                    return UnityBuiltinExtraToMagickImage(texture as Texture2D);
                }
                return new MagickImage(path);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to load \"{path}\" as MagickImage: {e.Message}");
            }
        }

        private static MagickImage UnityBuiltinExtraToMagickImage(Texture2D texture)
        {
            var copy = new Texture2D(texture.width, texture.height);
            copy.LoadRawTextureData(texture.GetRawTextureData());
            var image = new MagickImage(copy.EncodeToPNG());
            return image;
        }
    }
}
