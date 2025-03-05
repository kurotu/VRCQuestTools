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
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(material.Material, out string guid, out long localId);
            return material.GenerateToonLitImage(settings, (tex) =>
            {
                var texToWrite = tex;
                var texture = tex;
                if (saveAsPng)
                {
                    Directory.CreateDirectory(texturesPath);
                    var outFile = $"{texturesPath}/{material.Material.name}_from_{guid}.png";

                    // When the texture is added into another asset, "/" is acceptable as name.
                    if (material.Material.name.Contains("/"))
                    {
                        var dir = Path.GetDirectoryName(outFile);
                        Directory.CreateDirectory(dir);
                    }
                    texture = AssetUtility.SaveUncompressedTexture(outFile, texToWrite);
                }
                else
                {
                    AssetUtility.SetStreamingMipMaps(texToWrite, true);
                    AssetUtility.CompressTextureForBuildTarget(texToWrite, EditorUserBuildSettings.activeBuildTarget);
                    texture = UnityEngine.Object.Instantiate(texToWrite);
                    texture.name = material.Material.name;
                }
                completion?.Invoke(texture);
            });
        }
    }
}
