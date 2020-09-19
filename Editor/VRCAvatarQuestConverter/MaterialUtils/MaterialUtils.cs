using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using ImageMagick;

namespace KRTQuestTools
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
        public abstract Layer GetMainLayer();
        public abstract Layer GetEmissionLayer();
        public abstract MagickImage CompositeLayers();
    }

    public static class MaterialUtils
    {
        public static MaterialWrapper CreateWrapper(Material material)
        {
            if (material.shader.name.StartsWith("UnityChanToonShader")) {
                return new UTS2Material(material);
            }
            if (material.shader.name.StartsWith("arktoon/"))
            {
                return new ArktoonMaterial(material);
            }
            return new GenericMaterial(material);
        }

        internal static MagickImage GetMagickImage(Material material, string texturePropertyName)
        {
            var texture = material.GetTexture(texturePropertyName);
            return GetMagickImage(texture);
        }

        internal static MagickImage GetMagickImage(Texture texture)
        {
            if (texture == null) return null;
            var path = AssetDatabase.GetAssetPath(texture);
            return new MagickImage(path);
        }
    }
}
