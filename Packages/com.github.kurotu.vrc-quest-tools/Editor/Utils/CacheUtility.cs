using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
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
                        var hash = TextureUtility.GetImageContentsHash(tex);
                        sb.Append($"{name}_{hash}");
                        break;
                    case ShaderPropertyType.Int:
                        sb.Append($"{name}_{material.GetInteger(name)}");
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Hashes a file name using MD5 and preserves the extension (if any).
        /// Example: "very/long/name.png" -> "<md5hex>.png"
        /// </summary>
        /// <param name="fileName">Original file name.</param>
        /// <returns>Hashed file name with extension.</returns>
        internal static string HashFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return fileName;
            }
            var ext = Path.GetExtension(fileName);
            // Use full input (including path) to reduce collisions across different paths.
            var input = fileName;
            using (var md5 = MD5.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(bytes);
                var sbHash = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes)
                {
                    sbHash.Append(b.ToString("x2"));
                }
                var hashed = sbHash.ToString();
                return string.IsNullOrEmpty(ext) ? hashed : (hashed + ext);
            }
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
                        // The blank normal map asset's .meta only overrides the import format to ASTC for the
                        // Android/iOS platforms. When the active build target differs (e.g. Standalone/Linux in
                        // CI), Unity imports the same asset with an Automatic (non-ASTC) format instead, so its
                        // format/dimensions/mip layout no longer match what this TextureCache recorded at save
                        // time. Using such an asset as a raw-byte container would make the LoadRawTextureData
                        // call in ToTexture2D() over/underread the stored buffer. Only use the asset as a
                        // container when it actually matches; otherwise fall back to building the container
                        // directly below. This mismatch is an expected, environment-dependent situation (not a
                        // missing asset), so no warning is logged for it.
                        if (normal.format == format && normal.width == width && normal.height == height && (normal.mipmapCount > 1) == mipmap)
                        {
                            // Restore the cached color space (normal maps are linear); CopyAsReadable's bool is the Texture2D "linear" flag.
                            return TextureUtility.CopyAsReadable(normal, linear);
                        }
                    }
                    else
                    {
                        Logger.LogWarning($"Failed to load normal map from {path}. Creating normal map from uncompressed one.");
                    }
                }
                // Build the container directly instead of going through TextureUtility.CompressNormalMap /
                // TextureCompressorProvider. This texture's fields (format/mipmap/linear/dimensions) are a
                // one-to-one match for the values recorded in TextureCache at save time, and ToTexture2D()
                // immediately replaces the entire content via LoadRawTextureData -- no compression step is
                // needed to produce it. Routing through the compressor facade would resolve the format via
                // the active build target (TextureUtility.ResolveEffectiveCompressionFormat), which only
                // honors the ASTC mobileFormat when the active build target is Android/iOS; on any other
                // active build target (e.g. Linux/Windows standalone in CI) it would silently produce a
                // container in a different format than the one actually stored in the cache, causing
                // LoadRawTextureData to fail on a byte-size mismatch. Building the container directly avoids
                // this active-build-target dependency entirely.
                return new Texture2D(width, height, format, mipmap, linear);
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
