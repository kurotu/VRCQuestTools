using System;
using System.IO;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    internal static class MaterialGeneratorUtility
    {
        internal static AsyncCallbackRequest GenerateTexture(Material material, IMaterialConvertSettings settings, string textureType, bool saveAsPng, string texturesPath, Func<Action<Texture2D>, AsyncCallbackRequest> requestGenerateImageFunc, Action<Texture2D> completion)
        {
            return GenerateTexture(material, settings, textureType, saveAsPng, texturesPath, TextureConfig.SRGB, requestGenerateImageFunc, completion);
        }

        internal static AsyncCallbackRequest GenerateParameterTexture(Material material, IMaterialConvertSettings settings, string textureType, bool saveAsPng, string texturesPath, Func<Action<Texture2D>, AsyncCallbackRequest> requestGenerateImageFunc, Action<Texture2D> completion)
        {
            return GenerateTexture(material, settings, textureType, saveAsPng, texturesPath, TextureConfig.Parameter, requestGenerateImageFunc, completion);
        }

        internal static AsyncCallbackRequest GenerateNormalMap(Material material, IMaterialConvertSettings settings, string textureType, bool saveAsPng, string texturesPath, Func<Action<Texture2D>, AsyncCallbackRequest> requestGenerateImageFunc, Action<Texture2D> completion)
        {
            return GenerateTexture(material, settings, textureType, saveAsPng, texturesPath, TextureConfig.NormalMap, requestGenerateImageFunc, completion);
        }

        private static AsyncCallbackRequest GenerateTexture(Material material, IMaterialConvertSettings settings, string textureType, bool saveAsPng, string texturesPath, TextureConfig config, Func<Action<Texture2D>, AsyncCallbackRequest> requestGenerateImageFunc, Action<Texture2D> completion)
        {
            var assetHash = Hash128.Compute(CacheUtility.GetContentCacheKey(material) + settings.GetCacheKey());
            var cacheFile = $"texture_{VRCQuestTools.Version}_{settings.GetType()}_{textureType}_{EditorUserBuildSettings.activeBuildTarget}_{assetHash}" + (saveAsPng ? ".png" : ".json");
            var texName = $"{material.name}_{textureType}";
            string outFile = null;
            if (saveAsPng)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(material, out string guid, out long localId);
                outFile = $"{texturesPath}/{texName}_from_{guid}.png";
            }

            var cacheTexture = TryLoadCacheTexture(material, saveAsPng, texturesPath, config, cacheFile, outFile);
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
                    texToWrite = SaveTexture(saveAsPng, texturesPath, config, texToWrite, cacheFile, outFile);
                }
                completion?.Invoke(texToWrite);
            });
            return request;
        }

        private static Texture2D TryLoadCacheTexture(Material material, bool saveAsPng, string texturesPath, TextureConfig config, string cacheFile, string outFile)
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
                                    AssetUtility.CongigureNormalMapImporter(outFile);
                                }
                                else
                                {
                                    AssetUtility.ConfigureTextureImporter(outFile, config.isSRGB);
                                }
                                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(outFile);
                                return tex;
                            }
                            else
                            {
                                var cache = JsonUtility.FromJson<CacheUtility.TextureCache>(CacheManager.Texture.LoadString(cacheFile));
                                var tex = cache.ToTexture2D();
                                AssetUtility.SetStreamingMipMaps(tex, true);
                                return tex;
                            }
                        }
                        catch (Exception e)
                        {
                            // Recoverable error, just log and continue.
                            Debug.LogException(e);
                            Debug.LogWarning($"[{VRCQuestTools.Name}] Failed to load cache file {cacheFile} for {material.name}");
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

        private static Texture2D SaveTexture(bool saveAsPng, string texturesPath, TextureConfig config, Texture2D texToWrite, string cacheFile, string outFile)
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
                texToWrite = AssetUtility.SaveUncompressedTexture(outFile, texToWrite, config.isSRGB);
                if (config.isNormalMap)
                {
                    AssetUtility.CongigureNormalMapImporter(outFile);
                }
                CacheManager.Texture.CopyToCache(outFile, cacheFile);
            }
            else
            {
                AssetUtility.SetStreamingMipMaps(texToWrite, true);
                if (config.isNormalMap)
                {
                    texToWrite = AssetUtility.CompressNormalMap(texToWrite);
                }
                else
                {
                    AssetUtility.CompressTextureForBuildTarget(texToWrite, EditorUserBuildSettings.activeBuildTarget);
                }
                CacheManager.Texture.Save(cacheFile, JsonUtility.ToJson(new CacheUtility.TextureCache(texToWrite)));
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
                alphaIsTransparency = true
            };

            public static TextureConfig Parameter => new TextureConfig()
            {
                isSRGB = false,
                isNormalMap = false,
                alphaIsTransparency = false
            };

            public static TextureConfig NormalMap => new TextureConfig()
            {
                isSRGB = false,
                isNormalMap = true,
                alphaIsTransparency = false
            };
        }
    }
}
