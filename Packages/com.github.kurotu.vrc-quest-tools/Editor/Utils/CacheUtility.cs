using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Utility class for cache.
    /// </summary>
    internal static class CacheUtility
    {
        /// <summary>
        /// Get content cache key for material.
        /// </summary>
        /// <param name="material">Target material.</param>
        /// <returns>Cache key.</returns>
        internal static string GetContentCacheKey(Material material)
        {
            var sb = new StringBuilder(material.shader.name);

            sb.Append("LocalKeywords_");
            foreach (var keyword in material.shaderKeywords)
            {
                sb.Append(keyword);
            }

            var propertyCount = material.shader.GetPropertyCount();
            for (int i = 0; i < propertyCount; i++)
            {
                var type = material.shader.GetPropertyType(i);
                var name = material.shader.GetPropertyName(i);
                switch (type)
                {
                    case ShaderPropertyType.Color:
                        sb.Append($"{name}_{material.GetColor(name)}");
                        break;
                    case ShaderPropertyType.Vector:
                        sb.Append($"{name}_{material.GetVector(name)}");
                        break;
                    case ShaderPropertyType.Float:
                        sb.Append($"{name}_{material.GetFloat(name)}");
                        break;
                    case ShaderPropertyType.Range:
                        sb.Append($"{name}__{material.GetFloat(name)}");
                        break;
                    case ShaderPropertyType.Texture:
                        var tex = material.GetTexture(name);
                        if (tex != null)
                        {
                            sb.Append($"{name}_{tex.imageContentsHash}");
                        }
                        else
                        {
                            sb.Append($"{name}_null");
                        }
                        break;
#if UNITY_2021_1_OR_NEWER
                    case ShaderPropertyType.Int:
                        sb.Append($"{name}_{material.GetInteger(name)}");
                        break;
#endif
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Content cache for texture.
        /// </summary>
        [Serializable]
        internal class TextureCache
        {
            [SerializeField]
            private int width;

            [SerializeField]
            private int height;

            [SerializeField]
            private TextureFormat format;

            [SerializeField]
            private bool linear;

            [SerializeField]
            private bool normalMap;

            [SerializeField]
            private bool mipmap;

            [SerializeField]
            private string base64Data;

            /// <summary>
            /// Initializes a new instance of the <see cref="TextureCache"/> class.
            /// </summary>
            /// <param name="texture">Texture to cache.</param>
            /// <param name="linear"="linear">Texture is linear.</param>
            /// <param name="normalMap"="normalMap">Texture is normal map.</param>
            internal TextureCache(Texture2D texture, bool linear, bool normalMap = false)
            {
                width = texture.width;
                height = texture.height;
                format = texture.format;
                this.linear = linear;
                this.normalMap = normalMap;
                mipmap = texture.mipmapCount > 1;
                base64Data = Convert.ToBase64String(texture.GetRawTextureData());
            }

            /// <summary>
            /// Convert to Texture2D.
            /// </summary>
            /// <returns>Restored texture.</returns>
            internal Texture2D ToTexture2D()
            {
                var tex = normalMap ?
                    CreateCompressedNormalMap(width, height) :
                    new Texture2D(width, height, format, mipmap, linear);
                tex.LoadRawTextureData(Convert.FromBase64String(base64Data));
                tex.Apply(true, true);
                return tex;
            }

            private Texture2D CreateCompressedNormalMap(int width, int height)
            {
                var tex = new Texture2D(width, height, format, mipmap, linear);
                return TextureUtility.CompressNormalMap(tex, EditorUserBuildSettings.activeBuildTarget, format, true);
            }
        }
    }
}
