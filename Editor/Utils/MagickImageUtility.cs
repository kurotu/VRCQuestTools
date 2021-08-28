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
#pragma warning disable SA1135 // Using directives should be qualified
    using QuantumType = System.UInt16;
#pragma warning restore SA1135 // Using directives should be qualified

    /// <summary>
    /// MagickImage manipulation utility.
    /// </summary>
    internal static class MagickImageUtility
    {
        /// <summary>
        /// Multiply a image and a color.
        /// </summary>
        /// <param name="image">Base image.</param>
        /// <param name="color">Color to multiply.</param>
        /// <returns>Composed image.</returns>
        internal static MagickImage Multiply(MagickImage image, Color32 color)
        {
            return Multiply(image, GetMagickColor(color));
        }

        /// <summary>
        /// Multiply a image and a color.
        /// </summary>
        /// <param name="image">Base image.</param>
        /// <param name="color">Color to multiply.</param>
        /// <returns>Composed image.</returns>
        internal static MagickImage Multiply(MagickImage image, MagickColor color)
        {
            using (var colorTex = new MagickImage(color, image.Width, image.Height))
            {
                var newImage = new MagickImage(image);
                newImage.HasAlpha = false;
                newImage.Composite(colorTex, CompositeOperator.Multiply);
                if (image.HasAlpha)
                {
                    newImage.HasAlpha = true;
                    newImage.CopyPixels(image, Channels.Alpha);
                }
                return newImage;
            }
        }

        /// <summary>
        /// Adds two images.
        /// </summary>
        /// <param name="image0">Base image.</param>
        /// <param name="image1">Image to add.</param>
        /// <returns>Composed image.</returns>
        internal static MagickImage Add(MagickImage image0, MagickImage image1)
        {
            var newImage = new MagickImage(image0);
            newImage.Composite(image1, CompositeOperator.Plus);
            return newImage;
        }

        /// <summary>
        /// Apply screen composition.
        /// </summary>
        /// <param name="image0">Base image.</param>
        /// <param name="image1">Image to apply screen composition.</param>
        /// <returns>Composed image.</returns>
        internal static MagickImage Screen(MagickImage image0, MagickImage image1)
        {
            var newImage = new MagickImage(image0);
            newImage.Composite(image1, CompositeOperator.Screen);
            return newImage;
        }

        /// <summary>
        /// Resize image to square shape by interpolation.
        /// </summary>
        /// <param name="image">Image to resize.</param>
        internal static void ResizeToSquare(MagickImage image)
        {
            var size = System.Math.Max(image.Width, image.Height);
            image.InterpolativeResize(size, size, PixelInterpolateMethod.Bilinear);
        }

        /// <summary>
        /// Convert color to MagickColor.
        /// </summary>
        /// <param name="color">Original color.</param>
        /// <returns>Converted color.</returns>
        internal static MagickColor GetMagickColor(Color32 color)
        {
            var r = GetMagickColor(color.r);
            var g = GetMagickColor(color.g);
            var b = GetMagickColor(color.b);
            return new MagickColor(r, g, b);
        }

        /// <summary>
        /// Generates Metallic(R) Smoothness(A) texture.
        /// </summary>
        /// <param name="metallic">Metallic map.</param>
        /// <param name="invertMetallic">Whether invert metallic map.</param>
        /// <param name="smoothness">Smoothness or roughness map.</param>
        /// <param name="invertSmoothness">Whether invert smoothness map.</param>
        /// <returns>Generated MSMap.</returns>
        internal static MagickImage GenerateMetallicSmoothness(MagickImage metallic, bool invertMetallic, MagickImage smoothness, bool invertSmoothness)
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
        /// Save color MagickImage as Unity asset.
        /// </summary>
        /// <param name="path">Path to save.</param>
        /// <param name="image">Image to save.</param>
        /// <returns>Saved Texture2D asset.</returns>
        internal static Texture2D SaveAsAsset(string path, MagickImage image)
        {
            var format = image.HasAlpha ? MagickFormat.Png32 : MagickFormat.Png24;
            return SaveAsAsset(path, image, format, true);
        }

        /// <summary>
        /// Save MagickImage as Unity asset.
        /// </summary>
        /// <param name="path">Path to save.</param>
        /// <param name="image">Image to save.</param>
        /// <param name="format">Format to write.</param>
        /// <param name="isSRGB">Whether the image is sRGB.</param>
        /// <returns>Saved Texture2D asset.</returns>
        internal static Texture2D SaveAsAsset(string path, MagickImage image, MagickFormat format, bool isSRGB)
        {
            image.Write(path, format);
            AssetDatabase.Refresh();
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.sRGBTexture = isSRGB;
            importer.alphaIsTransparency = isSRGB;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            if (importer.mipmapEnabled)
            {
                importer.streamingMipmaps = true;
            }
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
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

#pragma warning disable SA1121 // Use built-in type alias
        private static QuantumType GetMagickColor(byte color)
        {
            return (QuantumType)(QuantumType.MaxValue * color / byte.MaxValue);
        }
#pragma warning restore SA1121 // Use built-in type alias
    }
}
