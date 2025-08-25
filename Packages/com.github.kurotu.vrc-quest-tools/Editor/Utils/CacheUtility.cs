using System;
using System.IO;
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
                    case ShaderPropertyType.Int:
                        sb.Append($"{name}_{material.GetInteger(name)}");
                        break;
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
            private BuildTarget buildTarget;

            [SerializeField]
            private bool mipmap;

            [SerializeField]
            private string base64Data;

            /// <summary>
            /// Initializes a new instance of the <see cref="TextureCache"/> class.
            /// </summary>
            /// <param name="texture">Texture to cache.</param>
            /// <param name="linear">Texture is linear.</param>
            /// <param name="normalMap">Texture is normal map.</param>
            /// <param name="buildTarget">Build target for the texture.</param>
            internal TextureCache(Texture2D texture, bool linear, bool normalMap, BuildTarget buildTarget)
            {
                width = texture.width;
                height = texture.height;
                format = texture.format;
                this.linear = linear;
                this.normalMap = normalMap;
                this.buildTarget = buildTarget;
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
                if (buildTarget == BuildTarget.Android || buildTarget == BuildTarget.iOS)
                {
                    var path = ResolveNormalMapPath();
                    var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    if (normal != null)
                    {
                        return TextureUtility.CopyAsReadable(normal, false);
                    }
                    Debug.LogWarning($"Failed to load normal map from {path}. Creating normal map from uncompressed one.");
                }
                var tex = new Texture2D(width, height, format, mipmap, linear);
                return TextureUtility.CompressNormalMap(tex, buildTarget, format, true);
            }

            private string ResolveNormalMapPath()
            {
                if (width != height)
                {
                    return string.Empty;
                }

                // Package/-/Assets/BlankNormalMaps
                var folder = AssetDatabase.GUIDToAssetPath("17d9dbede49f19943a367a284154f9d4");
                if (string.IsNullOrEmpty(folder))
                {
                    throw new InvalidOperationException("Failed to resolve normal map folder.");
                }

                string formatName;
                switch (format)
                {
                    case TextureFormat.ASTC_4x4:
                        formatName = nameof(TextureFormat.ASTC_4x4);
                        break;
                    case TextureFormat.ASTC_5x5:
                        formatName = nameof(TextureFormat.ASTC_5x5);
                        break;
                    case TextureFormat.ASTC_6x6:
                        formatName = nameof(TextureFormat.ASTC_6x6);
                        break;
                    case TextureFormat.ASTC_8x8:
                        formatName = nameof(TextureFormat.ASTC_8x8);
                        break;
                    case TextureFormat.ASTC_10x10:
                        formatName = nameof(TextureFormat.ASTC_10x10);
                        break;
                    case TextureFormat.ASTC_12x12:
                        formatName = nameof(TextureFormat.ASTC_12x12);
                        break;
                    default:
                        return string.Empty;
                }
                return Path.Join(folder, $"VQT_Normal_{width}px_{formatName}.png");
            }
        }
    }
}
