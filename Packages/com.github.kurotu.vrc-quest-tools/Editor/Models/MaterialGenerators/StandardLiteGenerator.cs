using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using Microsoft.SqlServer.Server;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.iOS;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Class for Standard Lite material generator.
    /// </summary>
    internal class StandardLiteGenerator : IMaterialGenerator
    {
        private readonly StandardLiteConvertSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardLiteGenerator"/> class.
        /// </summary>
        /// <param name="settings"></param>
        internal StandardLiteGenerator(StandardLiteConvertSettings settings)
        {
            this.settings = settings;
        }

        /// <inheritdoc/>
        public Material GenerateMaterial(MaterialBase material, bool saveTextureAsPng, string texturesPath)
        {
            if (material is not LilToonMaterial)
            {
                Debug.LogWarning("StandardLiteGenerator only supports LilToonMaterial.");
                return new ToonLitGenerator(new ToonLitConvertSettings()).GenerateMaterial(material, saveTextureAsPng, texturesPath);
            }
            var newMaterial = material.ConvertToStandardLite();
            if (settings.generateQuestTextures)
            {
                var main = GenerateMainTexture(material, settings, saveTextureAsPng, texturesPath);
                newMaterial.mainTexture = main;
                
                var emi = newMaterial.GetTexture("_EmissionMap");
                if (emi)
                {
                    var tex = DuplicateTexture(emi);
                    var e2 = AssetUtility.ResizeTexture(tex, main.width, main.height);
                    //tex.Reinitialize(main.width, main.height);
                    EditorUtility.CompressTexture(e2, TextureFormat.ASTC_6x6, TextureCompressionQuality.Best);
                    newMaterial.SetTexture("_EmissionMap", e2);
                }

                var normal = newMaterial.GetTexture("_BumpMap");
                if (normal)
                {
                    var tex =@DuplicateNormalTexture(normal);
                    var n2 = AssetUtility.ResizeTexture(tex, main.width, main.height);
                    EditorUtility.CompressTexture(n2, TextureFormat.ASTC_6x6, TextureCompressionQuality.Best);
                    newMaterial.SetTexture("_BumpMap", n2);
                }

                var useMetallicSmoothness = material.Material.GetFloat("_UseReflection") > 0.0f;
                if(useMetallicSmoothness)
                {
                    var metallic = material.Material.GetTexture("_MetallicGlossMap");
                    var smoothness = material.Material.GetTexture("_SmoothnessTex");
                    if (metallic || smoothness) {
                        var tex = GenerateMetallicSmoothnessTexture(material, settings, saveTextureAsPng, texturesPath);
                        //var m2 = AssetUtility.ResizeTexture(tex, main.width, main.height);
                        newMaterial.SetTexture("_MetallicGlossMap", tex);
                    }
                }
            }
            return newMaterial;
        }

        Texture2D DuplicateTexture(Texture source)
        {
            var width = source.width;
            var height = source.height;

            var dstTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            // Remember active render texture
            var activeRenderTexture = RenderTexture.active;
            var mat = new Material(Shader.Find("Unlit/Texture"));
            mat.SetTexture("_MainTex", source);
            Graphics.Blit(source, dstTexture);

            Texture2D outTexture = new Texture2D(width, height);
            outTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            outTexture.Apply();

            // Restore active render texture
            RenderTexture.active = activeRenderTexture;
            return outTexture;
        }

        Texture2D DuplicateNormalTexture(Texture source)
        {
            var width = source.width;
            var height = source.height;

            var dstTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            // Remember active render texture
            var activeRenderTexture = RenderTexture.active;
            var mat = new Material(Shader.Find("VRCQuestTools/NormalToColor"));
            mat.SetTexture("_BumpMap", source);
            Graphics.Blit(source, dstTexture, mat);

            Texture2D outTexture = new Texture2D(width, height, TextureFormat.ARGB32, true, false);
            outTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            outTexture.Apply();

            // Restore active render texture
            RenderTexture.active = activeRenderTexture;
            return outTexture;
        }

        /// <inheritdoc/>
        public void GenerateTextures(MaterialBase material, bool saveAsPng, string texturesPath)
        {
            if (material is not LilToonMaterial)
            {
                Debug.LogWarning("StandardLiteGenerator only supports LilToonMaterial.");
                new ToonLitGenerator(new ToonLitConvertSettings()).GenerateTextures(material, saveAsPng, texturesPath);
            }
            GenerateMainTexture(material, settings, saveAsPng, texturesPath);
        }

        private Texture2D GenerateMainTexture(MaterialBase material, StandardLiteConvertSettings settings, bool saveAsPng, string texturesPath)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(material.Material, out string guid, out long localId);
            using (var tex = DisposableObject.New(material.GenerateStandardLiteMainImage(settings)))
            {
                Texture2D texture = null;
                if (tex.Object != null)
                {
                    using (var disposables = new CompositeDisposable())
                    {
                        var texToWrite = tex.Object;
                        var maxTextureSize = (int)settings.maxTextureSize;
                        if (maxTextureSize > 0 && Math.Max(tex.Object.width, tex.Object.height) > maxTextureSize)
                        {
                            var resized = AssetUtility.ResizeTexture(tex.Object, maxTextureSize, maxTextureSize);
                            disposables.Add(DisposableObject.New(resized));
                            texToWrite = resized;
                        }

                        if (saveAsPng)
                        {
                            Directory.CreateDirectory(texturesPath);
                            var outFile = $"{texturesPath}/{material.Material.name}_main_from_{guid}.png";

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

        private Texture2D GenerateMetallicSmoothnessTexture(MaterialBase material, StandardLiteConvertSettings settings, bool saveAsPng, string texturesPath)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(material.Material, out string guid, out long localId);
            using (var tex = DisposableObject.New(material.GenerateStandardLiteMetallicSmoothnessImage(settings)))
            {
                Texture2D texture = null;
                if (tex.Object != null)
                {
                    using (var disposables = new CompositeDisposable())
                    {
                        var texToWrite = tex.Object;
                        var maxTextureSize = (int)settings.maxTextureSize;
                        if (maxTextureSize > 0 && Math.Max(tex.Object.width, tex.Object.height) > maxTextureSize)
                        {
                            var resized = AssetUtility.ResizeTexture(tex.Object, maxTextureSize, maxTextureSize);
                            disposables.Add(DisposableObject.New(resized));
                            texToWrite = resized;
                        }

                        if (saveAsPng)
                        {
                            Directory.CreateDirectory(texturesPath);
                            var outFile = $"{texturesPath}/{material.Material.name}_metallic_from_{guid}.png";

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
