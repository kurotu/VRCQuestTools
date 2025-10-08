using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// Generate a texture for material with platform override support.
        /// </summary>
        /// <param name="material">Original material.</param>
        /// <param name="settings">Convert settings.</param>
        /// <param name="textureType">Texture type name.</param>
        /// <param name="saveAsPng">Whether to save as PNG.</param>
        /// <param name="texturesPath">Textures directory to save PNG.</param>
        /// <param name="requestGenerateImageFunc">Function to generate Texture2D.</param>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        internal static AsyncCallbackRequest GenerateTexture(Material material, IMaterialConvertSettings settings, string textureType, bool saveAsPng, string texturesPath, Func<Action<Texture2D>, AsyncCallbackRequest> requestGenerateImageFunc, Action<Texture2D> completion)
        {
            return GenerateTexture(material, settings, textureType, saveAsPng, texturesPath, TextureConfig.SRGB, requestGenerateImageFunc, completion);
        }

        /// <summary>
        /// Generate a parameter texture for material with platform override support.
        /// </summary>
        /// <param name="material">Original material.</param>
        /// <param name="settings">Convert settings.</param>
        /// <param name="textureType">Texture type name.</param>
        /// <param name="saveAsPng">Whether to save as PNG.</param>
        /// <param name="texturesPath">Textures directory to save PNG.</param>
        /// <param name="requestGenerateImageFunc">Function to generate Texture2D.</param>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        internal static AsyncCallbackRequest GenerateParameterTexture(Material material, IMaterialConvertSettings settings, string textureType, bool saveAsPng, string texturesPath, Func<Action<Texture2D>, AsyncCallbackRequest> requestGenerateImageFunc, Action<Texture2D> completion)
        {
            return GenerateTexture(material, settings, textureType, saveAsPng, texturesPath, TextureConfig.Parameter, requestGenerateImageFunc, completion);
        }

        /// <summary>
        /// Generate a normal map texture for material with platform override support.
        /// </summary>
        /// <param name="material">Original material.</param>
        /// <param name="settings">Convert settings.</param>
        /// <param name="textureType">Texture type name.</param>
        /// <param name="saveAsPng">Whether to save as PNG.</param>
        /// <param name="texturesPath">Textures directory to save PNG.</param>
        /// <param name="requestGenerateImageFunc">Function to generate Texture2D.</param>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        internal static AsyncCallbackRequest GenerateNormalMap(Material material, IMaterialConvertSettings settings, string textureType, bool saveAsPng, string texturesPath, Func<Action<Texture2D>, AsyncCallbackRequest> requestGenerateImageFunc, Action<Texture2D> completion)
        {
            return GenerateTexture(material, settings, textureType, saveAsPng, texturesPath, TextureConfig.NormalMap, requestGenerateImageFunc, completion);
        }

        private static AsyncCallbackRequest GenerateTexture(Material material, IMaterialConvertSettings settings, string textureType, bool saveAsPng, string texturesPath, TextureConfig config, Func<Action<Texture2D>, AsyncCallbackRequest> requestGenerateImageFunc, Action<Texture2D> completion)
        {
            // Extract source textures from the material
            var sourceTextures = ExtractSourceTextures(material);
            
            // Create fallback settings from material settings
            var fallbackSettings = new TextureOverrideUtility.FallbackTextureSettings
            {
                mobileTextureFormat = settings.MobileTextureFormat,
                maxTextureSize = GetMaxTextureSizeFromSettings(settings)
            };
            
            // Resolve texture format using platform overrides or fallback to material settings
            var formatResult = TextureOverrideUtility.ResolveTextureFormat(sourceTextures, EditorUserBuildSettings.activeBuildTarget, fallbackSettings);
            
            var assetHash = Hash128.Compute(CacheUtility.GetContentCacheKey(material) + settings.GetCacheKey() + formatResult.GetCacheKey());
            var cacheFile = $"texture_{VRCQuestTools.Version}_{settings.GetType()}_{textureType}_{EditorUserBuildSettings.activeBuildTarget}_{assetHash}" + (saveAsPng ? ".png" : ".json");
            var texName = $"{material.name}_{textureType}";
            string outFile = null;
            if (saveAsPng)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(material, out string guid, out long localId);
                outFile = $"{texturesPath}/{texName}_from_{guid}.png";
            }

            var cacheTexture = TryLoadCacheTexture(material, formatResult.mobileTextureFormat, saveAsPng, texturesPath, config, cacheFile, outFile);
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
                    
                    // Pass maxTextureSize to SaveTexture for platform override configuration
                    var maxSize = formatResult.fromOverride && formatResult.maxTextureSize > 0 ? formatResult.maxTextureSize : 0;
                    texToWrite = SaveTexture(formatResult.mobileTextureFormat, saveAsPng, texturesPath, config, texToWrite, cacheFile, outFile, maxSize);
                }
                completion?.Invoke(texToWrite);
            });
            return request;
        }

        /// <summary>
        /// Extracts textures from a material for platform override analysis.
        /// </summary>
        /// <param name="material">Material to extract textures from.</param>
        /// <param name="texturePropertyNames">Optional array of specific texture property names to extract. If null, extracts all textures.</param>
        /// <returns>Collection of textures found in the material.</returns>
        private static IEnumerable<Texture> ExtractSourceTextures(Material material, string[] texturePropertyNames = null)
        {
            var textures = new List<Texture>();
            var propertyNames = texturePropertyNames ?? material.GetTexturePropertyNames();
            foreach (var propertyName in propertyNames)
            {
                var texture = material.GetTexture(propertyName);
                if (texture != null)
                {
                    textures.Add(texture);
                }
            }
            return textures;
        }

        private static Texture2D TryLoadCacheTexture(Material material, MobileTextureFormat mobileTextureFormat, bool saveAsPng, string texturesPath, TextureConfig config, string cacheFile, string outFile)
        {
            using (var mutex = CacheManager.Texture.CreateMutex())
            {
                mutex.WaitOne();
                try
                {
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
                                    TextureUtility.ConfigureNormalMapImporter(outFile, (TextureFormat)mobileTextureFormat);
                                }
                                else
                                {
                                    TextureUtility.ConfigureTextureImporter(outFile, (TextureFormat)mobileTextureFormat, config.isSRGB);
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
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            return null;
        }
        private static Texture2D SaveTexture(MobileTextureFormat mobileTextureFormat, bool saveAsPng, string texturesPath, TextureConfig config, Texture2D texToWrite, string cacheFile, string outFile, int maxTextureSize = 0)
        {
            if (saveAsPng)
            {
                Directory.CreateDirectory(texturesPath);

                // When the texture is added into another asset, "/" is acceptable as name.
                if (texToWrite.name.Contains("/"))
                {
                    var dir = Path.GetDirectoryName(outFile);
                    Directory.CreateDirectory(dir);
                }
                texToWrite = TextureUtility.SaveUncompressedTexture(outFile, texToWrite, (TextureFormat)mobileTextureFormat, config.isSRGB);
                if (config.isNormalMap)
                {
                    TextureUtility.ConfigureNormalMapImporter(outFile, (TextureFormat)mobileTextureFormat, maxTextureSize);
                }
                else
                {
                    TextureUtility.ConfigureTextureImporter(outFile, (TextureFormat)mobileTextureFormat, config.isSRGB, maxTextureSize);
                }
                CacheManager.Texture.CopyToCache(outFile, cacheFile);
            }
            else
            {
                TextureUtility.SetStreamingMipMaps(texToWrite, true);
                if (config.isNormalMap)
                {
                    texToWrite = TextureUtility.CompressNormalMap(texToWrite, EditorUserBuildSettings.activeBuildTarget, (TextureFormat)mobileTextureFormat);
                }
                else
                {
                    TextureUtility.CompressTextureForBuildTarget(texToWrite, EditorUserBuildSettings.activeBuildTarget, (TextureFormat)mobileTextureFormat);
                }
                CacheManager.Texture.Save(cacheFile, JsonUtility.ToJson(new CacheUtility.TextureCache(texToWrite, !config.isSRGB, config.isNormalMap, EditorUserBuildSettings.activeBuildTarget)));
            }

            return texToWrite;
        }

        /// <summary>
        /// Extracts maximum texture size from material convert settings.
        /// </summary>
        /// <param name="settings">Material convert settings.</param>
        /// <returns>Maximum texture size value.</returns>
        private static int GetMaxTextureSizeFromSettings(IMaterialConvertSettings settings)
        {
            if (settings is ToonStandardConvertSettings toonStandardSettings)
            {
                return (int)toonStandardSettings.maxTextureSize;
            }
            else if (settings is IToonLitConvertSettings toonLitSettings)
            {
                return (int)toonLitSettings.MaxTextureSize;
            }
            
            return 0; // No limit if unknown settings type
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
