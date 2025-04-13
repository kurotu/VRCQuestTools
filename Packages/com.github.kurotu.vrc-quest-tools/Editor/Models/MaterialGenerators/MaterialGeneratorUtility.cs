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
            var assetHash = Hash128.Compute(CacheUtility.GetContentCacheKey(material) + settings.GetCacheKey());
            var cacheFile = $"texture_{VRCQuestTools.Version}_{settings.GetType()}_{textureType}_{EditorUserBuildSettings.activeBuildTarget}_{assetHash}" + (saveAsPng ? ".png" : ".json");
            string outFile = null;
            if (saveAsPng)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(material, out string guid, out long localId);
                outFile = $"{texturesPath}/{material.name}_{textureType}_from_{guid}.png";
            }

            var cacheTexture = TryLoadCacheTexture(material, saveAsPng, texturesPath, cacheFile, outFile);
            if (cacheTexture)
            {
                return new ResultRequest<Texture2D>(cacheTexture, completion);
            }

            var request = requestGenerateImageFunc((texToWrite) =>
            {
                if (texToWrite)
                {
                    texToWrite = SaveTexture(material, saveAsPng, texturesPath, texToWrite, cacheFile, outFile);
                }
                completion?.Invoke(texToWrite);
            });
            return request;
        }

        private static Texture2D TryLoadCacheTexture(Material material, bool saveAsPng, string texturesPath, string cacheFile, string outFile)
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
                                AssetUtility.ConfigureTextureImporter(outFile);
                                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(outFile);
                                return tex;
                            }
                            else
                            {
                                var cache = JsonUtility.FromJson<CacheUtility.TextureCache>(CacheManager.Texture.LoadString(cacheFile));
                                var tex = cache.ToTexture2D();
                                AssetUtility.SetStreamingMipMaps(tex, true);
                                tex.name = material.name;
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

        private static Texture2D SaveTexture(Material material, bool saveAsPng, string texturesPath, Texture2D texToWrite, string cacheFile, string outFile)
        {
            if (saveAsPng)
            {
                Directory.CreateDirectory(texturesPath);

                // When the texture is added into another asset, "/" is acceptable as name.
                if (material.name.Contains("/"))
                {
                    var dir = Path.GetDirectoryName(outFile);
                    Directory.CreateDirectory(dir);
                }
                texToWrite = AssetUtility.SaveUncompressedTexture(outFile, texToWrite);
                CacheManager.Texture.CopyToCache(outFile, cacheFile);
            }
            else
            {
                AssetUtility.SetStreamingMipMaps(texToWrite, true);
                AssetUtility.CompressTextureForBuildTarget(texToWrite, EditorUserBuildSettings.activeBuildTarget);
                texToWrite.name = material.name;
                CacheManager.Texture.Save(cacheFile, JsonUtility.ToJson(new CacheUtility.TextureCache(texToWrite)));
            }

            return texToWrite;
        }
    }
}
