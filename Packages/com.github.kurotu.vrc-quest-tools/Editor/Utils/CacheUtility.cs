// <copyright file="CacheUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

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
        /// <remarks>
        /// Entries are persisted as a small binary header followed by the texture's raw bytes verbatim (see
        /// <see cref="WriteTo"/>), deliberately not as JSON with base64-encoded pixel data. Base64 inflated every
        /// entry by 4/3 on disk, and encoding/decoding it meant a multi-megabyte string existing alongside the
        /// byte array for every single texture, on top of the JSON string built around it.
        /// </remarks>
        internal class TextureCache
        {
            /// <summary>
            /// Revision of the binary layout written by <see cref="WriteTo"/>. Bump this whenever the layout
            /// changes: it is part of the texture cache's stamp (see <see cref="CacheManager.Texture"/>), so a
            /// bump discards every existing entry at once instead of making <see cref="ReadFrom"/> reject them
            /// one by one as they are looked up.
            /// </summary>
            internal const int FormatVersion = 1;

            /// <summary>
            /// Magic number at the head of every entry, so an unrelated or truncated file is rejected before
            /// its contents are interpreted as texture attributes.
            /// </summary>
            private static readonly byte[] MagicBytes = { (byte)'V', (byte)'Q', (byte)'T', (byte)'C' };

            private readonly int width;
            private readonly int height;
            private readonly TextureFormat format;
            private readonly bool linear;
            private readonly bool normalMap;
            private readonly BuildTarget buildTarget;
            private readonly bool mipmap;
            private readonly byte[] rawData;

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

                // The byte[] overload, not GetRawTextureData<byte>(): the NativeArray version throws for a
                // non-readable texture while this one succeeds, and compressed results are not readable.
                rawData = texture.GetRawTextureData();
            }

            private TextureCache(int width, int height, TextureFormat format, bool linear, bool normalMap, BuildTarget buildTarget, bool mipmap, byte[] rawData)
            {
                this.width = width;
                this.height = height;
                this.format = format;
                this.linear = linear;
                this.normalMap = normalMap;
                this.buildTarget = buildTarget;
                this.mipmap = mipmap;
                this.rawData = rawData;
            }

            /// <summary>
            /// Reads an entry previously written by <see cref="WriteTo"/>.
            /// </summary>
            /// <param name="stream">Stream positioned at the head of an entry.</param>
            /// <returns>Restored cache entry.</returns>
            /// <exception cref="InvalidDataException">The stream does not hold an entry of the current format.</exception>
            internal static TextureCache ReadFrom(Stream stream)
            {
                // leaveOpen: true -- the stream belongs to the caller (CacheManager), which closes it itself.
                using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
                {
                    var magic = reader.ReadBytes(MagicBytes.Length);
                    if (magic.Length != MagicBytes.Length)
                    {
                        throw new InvalidDataException("Not a texture cache entry: the file is shorter than its magic number.");
                    }
                    for (int i = 0; i < MagicBytes.Length; i++)
                    {
                        if (magic[i] != MagicBytes[i])
                        {
                            throw new InvalidDataException("Not a texture cache entry: magic number mismatch.");
                        }
                    }

                    var formatVersion = reader.ReadInt32();
                    if (formatVersion != FormatVersion)
                    {
                        throw new InvalidDataException($"Unsupported texture cache format version {formatVersion} (expected {FormatVersion}).");
                    }

                    var width = reader.ReadInt32();
                    var height = reader.ReadInt32();
                    var format = (TextureFormat)reader.ReadInt32();
                    var buildTarget = (BuildTarget)reader.ReadInt32();
                    var linear = reader.ReadBoolean();
                    var normalMap = reader.ReadBoolean();
                    var mipmap = reader.ReadBoolean();

                    var dataLength = reader.ReadInt32();
                    if (dataLength < 0)
                    {
                        throw new InvalidDataException($"Invalid texture cache data length: {dataLength}.");
                    }

                    // Compared against what the file actually holds *before* ReadBytes below, which allocates a
                    // buffer of exactly this size upfront: a corrupt length field would otherwise ask for an
                    // arbitrarily large allocation, and the post-read truncation check would only notice once
                    // that allocation had already been attempted. BinaryReader consumes exactly the bytes it is
                    // asked for (it never reads ahead), so the stream position here is the real read position.
                    // Streams passed in by CacheManager are always seekable (FileStream); for anything else the
                    // truncation check below remains the only guard.
                    if (stream.CanSeek)
                    {
                        var remaining = stream.Length - stream.Position;
                        if (dataLength > remaining)
                        {
                            throw new InvalidDataException($"Truncated texture cache entry: the header declares {dataLength} bytes of texture data but only {remaining} bytes remain.");
                        }
                    }

                    var rawData = reader.ReadBytes(dataLength);
                    if (rawData.Length != dataLength)
                    {
                        throw new InvalidDataException($"Truncated texture cache entry: expected {dataLength} bytes of texture data but found {rawData.Length}.");
                    }

                    return new TextureCache(width, height, format, linear, normalMap, buildTarget, mipmap, rawData);
                }
            }

            /// <summary>
            /// Writes this entry to <paramref name="stream"/> in the layout <see cref="ReadFrom"/> expects.
            /// </summary>
            /// <param name="stream">Stream to write to.</param>
            internal void WriteTo(Stream stream)
            {
                // leaveOpen: true -- the stream belongs to the caller (CacheManager), which closes it itself.
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                {
                    writer.Write(MagicBytes);
                    writer.Write(FormatVersion);
                    writer.Write(width);
                    writer.Write(height);
                    writer.Write((int)format);
                    writer.Write((int)buildTarget);
                    writer.Write(linear);
                    writer.Write(normalMap);
                    writer.Write(mipmap);
                    writer.Write(rawData.Length);
                    writer.Write(rawData);
                }
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
                tex.LoadRawTextureData(rawData);
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
