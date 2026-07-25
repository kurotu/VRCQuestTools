using System;
using System.IO;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Utility class for material generator.
    /// </summary>
    internal static class MaterialGeneratorUtility
    {
        /// <summary>
        /// Convert MobileTextureFormat to nullable TextureFormat, handling NoOverride case.
        /// </summary>
        /// <param name="format">Mobile texture format to convert.</param>
        /// <returns>Nullable TextureFormat, or null if NoOverride.</returns>
        private static TextureFormat? ConvertToNullableTextureFormat(MobileTextureFormat format)
        {
            return format == MobileTextureFormat.NoOverride ? null : (TextureFormat?)format;
        }

        /// <summary>
        /// Generate a texture for material.
        /// </summary>
        /// <param name="material">Original material.</param>
        /// <param name="settings">Convert settings.</param>
        /// <param name="textureType">Texture type name.</param>
        /// <param name="saveAsPng">Whether to save as PNG.</param>
        /// <param name="texturesPath">Textures directory to save PNG.</param>
        /// <param name="requestGenerateImageFunc">Function to generate Texture2D.</param>
        /// <param name="completion">Completion callback.</param>
        /// <param name="platformOverride">Optional platform override settings (MaxTextureSize and Format) from source textures.</param>
        /// <param name="forEditorPreview">Whether the conversion is for the NDMF editor preview, which trades a little compression quality for speed.</param>
        /// <returns>Async callback request.</returns>
        internal static AsyncCallbackRequest GenerateTexture(Material material, IMaterialConvertSettings settings, string textureType, bool saveAsPng, string texturesPath, Func<Action<Texture2D>, AsyncCallbackRequest> requestGenerateImageFunc, Action<Texture2D> completion, (int MaxTextureSize, TextureFormat Format)? platformOverride, bool forEditorPreview = false)
        {
            return GenerateTexture(material, settings, textureType, saveAsPng, texturesPath, TextureConfig.SRGB, requestGenerateImageFunc, completion, platformOverride, forEditorPreview);
        }

        /// <summary>
        /// Generate a parameter texture for material.
        /// </summary>
        /// <param name="material">Original material.</param>
        /// <param name="settings">Convert settings.</param>
        /// <param name="textureType">Texture type name.</param>
        /// <param name="saveAsPng">Whether to save as PNG.</param>
        /// <param name="texturesPath">Textures directory to save PNG.</param>
        /// <param name="requestGenerateImageFunc">Function to generate Texture2D.</param>
        /// <param name="completion">Completion callback.</param>
        /// <param name="platformOverride">Optional platform override settings (MaxTextureSize and Format) from source textures.</param>
        /// <param name="forEditorPreview">Whether the conversion is for the NDMF editor preview, which trades a little compression quality for speed.</param>
        /// <returns>Async callback request.</returns>
        internal static AsyncCallbackRequest GenerateParameterTexture(Material material, IMaterialConvertSettings settings, string textureType, bool saveAsPng, string texturesPath, Func<Action<Texture2D>, AsyncCallbackRequest> requestGenerateImageFunc, Action<Texture2D> completion, (int MaxTextureSize, TextureFormat Format)? platformOverride, bool forEditorPreview = false)
        {
            return GenerateTexture(material, settings, textureType, saveAsPng, texturesPath, TextureConfig.Parameter, requestGenerateImageFunc, completion, platformOverride, forEditorPreview);
        }

        /// <summary>
        /// Generate a normal map texture for material.
        /// </summary>
        /// <param name="material">Original material.</param>
        /// <param name="settings">Convert settings.</param>
        /// <param name="textureType">Texture type name.</param>
        /// <param name="saveAsPng">Whether to save as PNG.</param>
        /// <param name="texturesPath">Textures directory to save PNG.</param>
        /// <param name="requestGenerateImageFunc">Function to generate Texture2D.</param>
        /// <param name="completion">Completion callback.</param>
        /// <param name="platformOverride">Optional platform override settings (MaxTextureSize and Format) from source textures.</param>
        /// <param name="forEditorPreview">Whether the conversion is for the NDMF editor preview; re-uploads the generated normal map so it displays.</param>
        /// <returns>Async callback request.</returns>
        internal static AsyncCallbackRequest GenerateNormalMap(Material material, IMaterialConvertSettings settings, string textureType, bool saveAsPng, string texturesPath, Func<Action<Texture2D>, AsyncCallbackRequest> requestGenerateImageFunc, Action<Texture2D> completion, (int MaxTextureSize, TextureFormat Format)? platformOverride, bool forEditorPreview)
        {
            return GenerateTexture(material, settings, textureType, saveAsPng, texturesPath, TextureConfig.NormalMap, requestGenerateImageFunc, completion, platformOverride, forEditorPreview);
        }

        private static AsyncCallbackRequest GenerateTexture(Material material, IMaterialConvertSettings settings, string textureType, bool saveAsPng, string texturesPath, TextureConfig config, Func<Action<Texture2D>, AsyncCallbackRequest> requestGenerateImageFunc, Action<Texture2D> completion, (int MaxTextureSize, TextureFormat Format)? platformOverride, bool forEditorPreview)
        {
            var assetHash = Hash128.Compute(CacheUtility.GetContentCacheKey(material) + settings.GetCacheKey());

            // Only the in-code compression path (saveAsPng == false) actually invokes an ITextureCompressor;
            // include its identifier so switching between astcenc and Unity compression (or astcenc presets)
            // doesn't reuse a stale cache entry produced by a different encoder. Note this identifies the encoder
            // that was *available* when the key was computed, not necessarily the one that produced the bytes:
            // AstcencTextureCompressor can fall back to UnityTextureCompressor mid-compression (e.g. the astcenc
            // process fails for this particular texture) without that fallback being reflected here. That's
            // harmless -- both encoders emit valid ASTC data, so nothing renders incorrectly -- it just means the
            // cache key doesn't always perfectly identify which encoder actually wrote a given cache entry.
            var compressorKeyComponent = string.Empty;

            // Resolved and captured here (rather than only inside the `if (!saveAsPng)` block below) so the
            // progressive-preview branch further down -- inside the requestGenerateImageFunc completion closure --
            // can reuse the exact same resolution instead of re-deriving it, keeping the two in sync by construction.
            TextureFormat? compressionFormat = null;
            if (!saveAsPng)
            {
                // Mirrors the format resolution actually used by the compression path (TextureUtility.CompressTextureForBuildTarget
                // / CompressNormalMap via ResolveEffectiveCompressionFormat) so the cache key never diverges from what gets compressed.
                var mobileFormat = platformOverride?.Format ?? TextureUtility.GetCompressionFormat(settings.MobileTextureFormat);
                compressionFormat = TextureUtility.ResolveEffectiveCompressionFormat(EditorUserBuildSettings.activeBuildTarget, mobileFormat, config.isNormalMap);
                // forEditorPreview selects a faster astcenc preset, which produces different bytes than the final
                // one, so it belongs in the key: preview and final results must not share a cache entry.
                compressorKeyComponent = "_" + TextureCompressorProvider.GetCompressor(compressionFormat, forEditorPreview).CacheKeyComponent;
            }

            var cacheFile = $"texture_{VRCQuestTools.Version}_{settings.GetType()}_{textureType}_{EditorUserBuildSettings.activeBuildTarget}{compressorKeyComponent}_{assetHash}" + (saveAsPng ? ".png" : ".json");
            var texName = $"{material.name}_{textureType}";
            string outFile = null;
            if (saveAsPng)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(material, out string guid, out long localId);
                outFile = $"{texturesPath}/{texName}_from_{guid}.png";
            }

            var cacheTexture = TryLoadCacheTexture(material, settings, saveAsPng, texturesPath, config, cacheFile, outFile, platformOverride);
            if (cacheTexture)
            {
                cacheTexture.name = texName;
                return new ResultRequest<Texture2D>(cacheTexture, completion);
            }

            var request = requestGenerateImageFunc((texToWrite) =>
            {
                if (texToWrite)
                {
                    texToWrite.name = texName;

                    // Progressive NDMF preview: on a cache miss, forEditorPreview, and an astcenc-compatible
                    // format, skip the (main-thread-stalling) synchronous compression below entirely. The baked,
                    // uncompressed texToWrite becomes the immediate on-screen placeholder while the same texture
                    // is handed to PreviewTextureCompressionQueue for background compression, disk cache save,
                    // and eventual material texture replacement. TryEnqueueProgressiveCompression returns false
                    // (falling through to the normal synchronous path below) when astcenc is unavailable for this
                    // format or the queue's pending-bytes safety valve is full.
                    if (!saveAsPng && forEditorPreview && TryEnqueueProgressiveCompression(compressionFormat, config, cacheFile, platformOverride, ref texToWrite))
                    {
                        completion?.Invoke(texToWrite);
                        return;
                    }

                    texToWrite = SaveTexture(settings.MobileTextureFormat, saveAsPng, texturesPath, config, texToWrite, cacheFile, outFile, platformOverride, forEditorPreview);

                    // A freshly generated normal map is not uploaded to the GPU by TextureGenerator; re-upload it
                    // for the NDMF preview (preview only) so it renders. See TextureUtility.ReuploadForEditorDisplay.
                    // Not needed on the progressive branch above: its placeholder is an ordinary baked Texture2D
                    // (from Graphics.Blit/ReadPixels), already GPU-uploaded, not TextureGenerator output.
                    if (config.isNormalMap && forEditorPreview)
                    {
                        var reuploaded = TextureUtility.ReuploadForEditorDisplay(texToWrite);
                        TextureUtility.DestroyTexture(texToWrite);
                        texToWrite = reuploaded;
                    }
                }
                completion?.Invoke(texToWrite);
            });
            return request;
        }

        /// <summary>
        /// For the NDMF editor preview's progressive texture replacement: when astcenc is available for
        /// <paramref name="compressionFormat"/>, hands the baked, uncompressed <paramref name="texToWrite"/> back
        /// as-is (or resized, for the color path -- see below) as the immediate placeholder, and enqueues the
        /// same texture in <see cref="PreviewTextureCompressionQueue"/> for background compression. The eventual
        /// compressed result and disk cache entry end up indistinguishable from what <see cref="SaveTexture"/>'s
        /// non-PNG branch would have produced synchronously -- just later, without stalling the editor meanwhile.
        /// </summary>
        /// <param name="compressionFormat">Resolved compression format from <see cref="TextureUtility.ResolveEffectiveCompressionFormat"/>; null when the texture is left uncompressed for a non-mobile normal map (never astcenc-compatible, so this method returns false immediately in that case).</param>
        /// <param name="config">Texture config (isNormalMap/isSRGB) for the texture being generated.</param>
        /// <param name="cacheFile">Disk cache file name the compressed result should eventually be saved under.</param>
        /// <param name="platformOverride">Optional platform override; only its MaxTextureSize is used here (its Format already went into <paramref name="compressionFormat"/>).</param>
        /// <param name="texToWrite">The freshly baked, uncompressed texture. For color textures with a maxTextureSize override, replaced in place with a resized instance (mirroring <see cref="TextureUtility.CompressTextureForBuildTarget"/>'s own maxTextureSize handling) before being enqueued as the placeholder; normal maps are left untouched here since <see cref="AstcencTextureCompressor.CompressNormalMapAsync"/> applies its own maxTextureSize shrink internally, mirroring <see cref="TextureUtility.CompressNormalMap"/>.</param>
        /// <returns>True when the texture was successfully enqueued for background compression (the caller must treat <paramref name="texToWrite"/> as the new placeholder and do nothing further to it). False when astcenc is unavailable for <paramref name="compressionFormat"/>, or the queue's pending-bytes cap was reached, in which case the caller must fall back to synchronous compression.</returns>
        private static bool TryEnqueueProgressiveCompression(TextureFormat? compressionFormat, TextureConfig config, string cacheFile, (int MaxTextureSize, TextureFormat Format)? platformOverride, ref Texture2D texToWrite)
        {
            if (!(TextureCompressorProvider.GetCompressor(compressionFormat, forEditorPreview: true) is AstcencTextureCompressor compressor))
            {
                return false;
            }

            var overrideMaxTextureSize = TextureUtility.NormalizeMaxTextureSize(platformOverride?.MaxTextureSize);

            if (!config.isNormalMap && overrideMaxTextureSize.HasValue)
            {
                // Mirrors CompressTextureForBuildTarget's maxTextureSize resize step. Its DXT5 4-multiple guard is
                // irrelevant here: TextureCompressorProvider only resolves to AstcencTextureCompressor for ASTC formats.
                var (w, h) = TextureUtility.AspectFitReduction(texToWrite.width, texToWrite.height, overrideMaxTextureSize.Value);
                if (w != texToWrite.width || h != texToWrite.height)
                {
                    texToWrite = TextureUtility.ResizeTextureImmediate(texToWrite, w, h);
                }
            }

            return PreviewTextureCompressionQueue.TryEnqueue(texToWrite, compressor, compressionFormat, config.isNormalMap, readable: false, overrideMaxTextureSize, cacheFile, config.isSRGB);
        }

        private static Texture2D TryLoadCacheTexture(Material material, IMaterialConvertSettings settings, bool saveAsPng, string texturesPath, TextureConfig config, string cacheFile, string outFile, (int MaxTextureSize, TextureFormat Format)? platformOverride)
        {
            // Convert MobileTextureFormat to TextureFormat?, handling NoOverride case
            // Use platform override format if provided, otherwise fall back to settings
            TextureFormat? mobileTextureFormatNullable = platformOverride?.Format ?? ConvertToNullableTextureFormat(settings.MobileTextureFormat);
            int? overrideMaxTextureSize = TextureUtility.NormalizeMaxTextureSize(platformOverride?.MaxTextureSize);

            if (CacheManager.Texture.Exists(cacheFile))
            {
                try
                {
                    if (saveAsPng)
                    {
                        Directory.CreateDirectory(texturesPath);
                        CacheManager.Texture.CopyFromCache(cacheFile, outFile);
                        AssetDatabase.ImportAsset(outFile);
                        if (config.isNormalMap)
                        {
                            TextureUtility.ConfigureNormalMapImporter(outFile, mobileTextureFormatNullable, overrideMaxTextureSize);
                        }
                        else
                        {
                            TextureUtility.ConfigureTextureImporter(outFile, mobileTextureFormatNullable, config.isSRGB, overrideMaxTextureSize);
                        }
                        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(outFile);
                        return tex;
                    }
                    else
                    {
                        var cache = JsonUtility.FromJson<CacheUtility.TextureCache>(CacheManager.Texture.LoadString(cacheFile));
                        var tex = cache.ToTexture2D();
                        TextureUtility.SetStreamingMipMaps(tex, true);
                        return tex;
                    }
                }
                catch (Exception e)
                {
                    // Recoverable error, just log and continue.
                    Logger.LogException(e);
                    Logger.LogWarning($"Failed to load cache file {cacheFile} for {material.name}");
                }
            }
            return null;
        }

        private static Texture2D SaveTexture(MobileTextureFormat mobileTextureFormat, bool saveAsPng, string texturesPath, TextureConfig config, Texture2D texToWrite, string cacheFile, string outFile, (int MaxTextureSize, TextureFormat Format)? platformOverride, bool forEditorPreview)
        {
            // Convert MobileTextureFormat to TextureFormat?, handling NoOverride case
            // Use platform override format if provided, otherwise fall back to settings
            TextureFormat? mobileTextureFormatNullable = platformOverride?.Format ?? ConvertToNullableTextureFormat(mobileTextureFormat);
            int? overrideMaxTextureSize = TextureUtility.NormalizeMaxTextureSize(platformOverride?.MaxTextureSize);

            // For in-code compression, use override format if provided, otherwise fall back to settings
            TextureFormat mobileTextureFormatForCompression = platformOverride?.Format ?? TextureUtility.GetCompressionFormat(mobileTextureFormat);

            if (saveAsPng)
            {
                Directory.CreateDirectory(texturesPath);

                // When the texture is added into another asset, "/" is acceptable as name.
                if (texToWrite.name.Contains("/"))
                {
                    var dir = Path.GetDirectoryName(outFile);
                    Directory.CreateDirectory(dir);
                }
                texToWrite = TextureUtility.SaveUncompressedTexture(outFile, texToWrite, mobileTextureFormatNullable, config.isSRGB, overrideMaxTextureSize);
                if (config.isNormalMap)
                {
                    TextureUtility.ConfigureNormalMapImporter(outFile, mobileTextureFormatNullable, overrideMaxTextureSize);
                }
                CacheManager.Texture.CopyToCache(outFile, cacheFile);
            }
            else
            {
                if (config.isNormalMap)
                {
                    texToWrite = TextureUtility.CompressNormalMap(texToWrite, EditorUserBuildSettings.activeBuildTarget, mobileTextureFormatForCompression, maxTextureSize: overrideMaxTextureSize, forEditorPreview: forEditorPreview);
                }
                else
                {
                    texToWrite = TextureUtility.CompressTextureForBuildTarget(texToWrite, EditorUserBuildSettings.activeBuildTarget, mobileTextureFormatForCompression, overrideMaxTextureSize, forEditorPreview);
                }

                // Must run after compression, not before: compression backends may replace texToWrite with a new
                // instance (e.g. the astcenc path always returns a new Texture2D on success), which would silently
                // drop a flag set on the pre-compression instance.
                TextureUtility.SetStreamingMipMaps(texToWrite, true);
                CacheManager.Texture.Save(cacheFile, JsonUtility.ToJson(new CacheUtility.TextureCache(texToWrite, !config.isSRGB, config.isNormalMap, EditorUserBuildSettings.activeBuildTarget)));
            }

            return texToWrite;
        }

        private struct TextureConfig
        {
            public bool isSRGB;
            public bool isNormalMap;
            public bool alphaIsTransparency;

            public static TextureConfig SRGB => new TextureConfig()
            {
                isSRGB = true,
                isNormalMap = false,
                alphaIsTransparency = true,
            };

            public static TextureConfig Parameter => new TextureConfig()
            {
                isSRGB = false,
                isNormalMap = false,
                alphaIsTransparency = false,
            };

            public static TextureConfig NormalMap => new TextureConfig()
            {
                isSRGB = false,
                isNormalMap = true,
                alphaIsTransparency = false,
            };
        }
    }
}
