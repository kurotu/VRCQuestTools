using System;
using System.IO;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Class for Toon Lit material generator.
    /// </summary>
    internal class ToonLitGenerator : IMaterialGenerator
    {
        private readonly IToonLitConvertSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToonLitGenerator"/> class.
        /// </summary>
        /// <param name="settings">Convert settings.</param>
        internal ToonLitGenerator(IToonLitConvertSettings settings)
        {
            this.settings = settings;
        }

        /// <inheritdoc/>
        public AsyncCallbackRequest GenerateMaterial(MaterialBase material, bool saveTextureAsPng, string texturesPath, Action<Material> completion)
        {
            var newMaterial = material.ConvertToToonLit();
            if (settings.GenerateQuestTextures)
            {
                return GenerateToonLitTexture(material, settings, saveTextureAsPng, texturesPath, (texture) =>
                {
                    newMaterial.mainTexture = texture;
                    completion?.Invoke(newMaterial);
                });
            }
            return new ResultRequest<UnityEngine.Object>(null, (_) =>
            {
                completion?.Invoke(newMaterial);
            });
        }

        /// <inheritdoc/>
        public AsyncCallbackRequest GenerateTextures(MaterialBase material, bool saveTextureAsPng, string texturesPath, Action completion)
        {
            return GenerateToonLitTexture(material, settings, saveTextureAsPng, texturesPath, (_) =>
            {
                completion?.Invoke();
            });
        }

        private AsyncCallbackRequest GenerateToonLitTexture(MaterialBase material, IToonLitConvertSettings settings, bool saveAsPng, string texturesPath, Action<Texture2D> completion)
        {
            var assetHash = Hash128.Compute(CacheUtility.GetContentCacheKey(material.Material) + settings.GetCacheKey());
            var cacheFile = $"texture_{VRCQuestTools.Version}_toonlit_main_{EditorUserBuildSettings.activeBuildTarget}_{assetHash}" + (saveAsPng ? ".png" : ".json");
            string outFile = null;
            if (saveAsPng)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(material.Material, out string guid, out long localId);
                outFile = $"{texturesPath}/{material.Material.name}_from_{guid}.png";
            }

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
                                return new ResultRequest<Texture2D>(tex, completion);
                            }
                            else
                            {
                                var cache = JsonUtility.FromJson<CacheUtility.TextureCache>(CacheManager.Texture.LoadString(cacheFile));
                                var tex = cache.ToTexture2D();
                                AssetUtility.SetStreamingMipMaps(tex, true);
                                tex.name = material.Material.name;
                                return new ResultRequest<Texture2D>(tex, completion);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                            Debug.LogWarning($"[{VRCQuestTools.Name}] Failed to load cache file {cacheFile} for {material.Material.name}");
                        }
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }

            return material.GenerateToonLitImage(settings, (texToWrite) =>
            {
                if (texToWrite)
                {
                    if (saveAsPng)
                    {
                        Directory.CreateDirectory(texturesPath);

                        // When the texture is added into another asset, "/" is acceptable as name.
                        if (material.Material.name.Contains("/"))
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
                        texToWrite.name = material.Material.name;
                        CacheManager.Texture.Save(cacheFile, JsonUtility.ToJson(new CacheUtility.TextureCache(texToWrite)));
                    }
                }
                completion?.Invoke(texToWrite);
            });
        }
    }
}
