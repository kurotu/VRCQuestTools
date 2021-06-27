// <copyright file="MaterialUtils.cs" company="kurotu">
// Copyright (c) kurotu.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

using ImageMagick;
using System;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    public class Layer : IDisposable
    {
        public MagickImage image = null;
        public Color color = Color.white;

        public MagickImage GetMagickImage()
        {
            if (image == null)
            {
                return new MagickImage(ImgProc.GetMagickColor(color), 1, 1);
            }
            return ImgProc.Multiply(image, color);
        }

        public void Dispose()
        {
            if (image != null && !image.IsDisposed)
            {
                image.Dispose();
            }
        }
    }

    public abstract class MaterialWrapper
    {
        public abstract MagickImage CompositeLayers();
    }

    public static class MaterialUtils
    {
        public static MaterialWrapper CreateWrapper(Material material)
        {
            switch (DetectShaderType(material))
            {
                case ShaderCategory.UTS2:
                    return new UTS2Material(material);
                case ShaderCategory.Arktoon:
                    return new ArktoonMaterial(material);
                case ShaderCategory.Sunao:
                    return new SunaoMaterial(material);
                default:
                    return new StandardMaterial(material);
            }
        }

        internal static MagickImage GetMagickImage(Material material, string texturePropertyName)
        {
            var texture = material.GetTexture(texturePropertyName);
            return GetMagickImage(texture);
        }

        internal static MagickImage GetMagickImage(Texture texture)
        {
            if (texture == null) return null;
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

        internal static ShaderCategory DetectShaderType(Material material)
        {
            var shaderName = material.shader.name;
            if (shaderName == "Standard" || shaderName == "Standard (Specular setup)" || shaderName.StartsWith("Standard/"))
            {
                return ShaderCategory.Standard;
            }
            if (shaderName.StartsWith("UnityChanToonShader"))
            {
                return ShaderCategory.UTS2;
            }
            if (shaderName.StartsWith("arktoon/"))
            {
                return ShaderCategory.Arktoon;
            }
            if (shaderName.StartsWith("Unlit/"))
            {
                return ShaderCategory.Unlit;
            }
            if (shaderName.StartsWith("VRChat/Mobile/"))
            {
                return ShaderCategory.Quest;
            }
            if (shaderName.StartsWith("Sunao Shader/"))
            {
                return ShaderCategory.Sunao;
            }
            return ShaderCategory.Unverified;
        }

        private static MagickImage UnityBuiltinExtraToMagickImage(Texture2D texture)
        {
            var copy = new Texture2D(texture.width, texture.height);
            copy.LoadRawTextureData(texture.GetRawTextureData());
            var image = new MagickImage(copy.EncodeToPNG());
            return image;
        }
    }

    internal enum ShaderCategory
    {
        UTS2, Arktoon, Standard, Unlit, Quest, Sunao, Unverified
    }
}
