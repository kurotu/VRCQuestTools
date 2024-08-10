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
        public Material GenerateMaterial(MaterialBase material, bool saveTextureAsPng, string texturesPath)
        {
            var newMaterial = material.ConvertToToonLit();
            if (settings.GenerateQuestTextures)
            {
                var texture = GenerateToonLitTexture(material, settings, saveTextureAsPng, texturesPath);
                newMaterial.mainTexture = texture;
            }
            return newMaterial;
        }

        /// <inheritdoc/>
        public void GenerateTextures(MaterialBase material, bool saveTextureAsPng, string texturesPath)
        {
            GenerateToonLitTexture(material, settings, saveTextureAsPng, texturesPath);
        }

        private Texture2D GenerateToonLitTexture(MaterialBase material, IToonLitConvertSettings settings, bool saveAsPng, string texturesPath)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(material.Material, out string guid, out long localId);
            using (var tex = DisposableObject.New(material.GenerateToonLitImage(settings)))
            {
                Texture2D texture = null;
                if (tex.Object != null)
                {
                    using (var disposables = new CompositeDisposable())
                    {
                        var texToWrite = tex.Object;
                        var maxTextureSize = (int)settings.MaxTextureSize;
                        if (maxTextureSize > 0 && Math.Max(tex.Object.width, tex.Object.height) > maxTextureSize)
                        {
                            var resized = AssetUtility.ResizeTexture(tex.Object, maxTextureSize, maxTextureSize);
                            disposables.Add(DisposableObject.New(resized));
                            texToWrite = resized;
                        }

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
                            var format = (EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android || EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS) ? TextureFormat.ASTC_6x6 : TextureFormat.DXT5;
                            EditorUtility.CompressTexture(texToWrite, format, TextureCompressionQuality.Best);
                            texture = UnityEngine.Object.Instantiate(texToWrite);
                            texture.name = material.Material.name;
                        }
                    }
                }
                return texture;
            }
        }
    }
}
