using System;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
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
        /// <param name="settings">Convert settings.</param>
        internal StandardLiteGenerator(StandardLiteConvertSettings settings)
        {
            this.settings = settings;
        }

        /// <inheritdoc/>
        public AsyncCallbackRequest GenerateMaterial(MaterialBase material, bool saveTextureAsPng, string texturesPath, Action<Material> completion)
        {
            if (!(material is IStandardLiteConvertable))
            {
                Debug.LogWarning("StandardLiteGenerator only supports LilToonMaterial.");
                return new ToonLitGenerator(new ToonLitConvertSettings()).GenerateMaterial(material, saveTextureAsPng, texturesPath, completion);
            }

            var standardLiteConvertable = material as IStandardLiteConvertable;
            var newMaterial = standardLiteConvertable.ConvertToStandardLite();
            if (settings.generateQuestTextures)
            {
                var request = GenerateMainTexture(material, settings, saveTextureAsPng, texturesPath, (t) =>
                {
                    newMaterial.mainTexture = t;
                });
                request.WaitForCompletion();

                if (standardLiteConvertable.UseStandardLiteEmission)
                {
                    request = GenerateEmissionTexture(material, settings, saveTextureAsPng, texturesPath, (t) =>
                    {
                        newMaterial.SetTexture("_EmissionMap", t);
                        newMaterial.SetColor("_EmissionColor", new Color(1, 1, 1, 1));
                    });
                    request.WaitForCompletion();
                }

                if (standardLiteConvertable.UseStandardLiteNormalMap)
                {
                    var inputRGB = EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android || EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS;
                    var outputRGB = saveTextureAsPng || inputRGB;
                    request = GenerateNormalTexture(material, settings, inputRGB, outputRGB, saveTextureAsPng, texturesPath, (t) =>
                    {
                        newMaterial.SetTexture("_BumpMap", t);
                    });
                    request.WaitForCompletion();
                }

                if (standardLiteConvertable.UseStandardLiteMetallicSmoothness)
                {
                    request = GenerateMetallicSmoothnessTexture(material, settings, saveTextureAsPng, texturesPath, (t) =>
                    {
                        newMaterial.SetTexture("_MetallicGlossMap", t);
                    });
                    request.WaitForCompletion();
                }
            }
            return new ResultRequest<Material>(newMaterial, completion);
        }

        /// <inheritdoc/>
        public AsyncCallbackRequest GenerateTextures(MaterialBase material, bool saveAsPng, string texturesPath, Action completion)
        {
            if (!(material is IStandardLiteConvertable))
            {
                Debug.LogWarning("StandardLiteGenerator only supports LilToonMaterial.");
                return new ToonLitGenerator(new ToonLitConvertSettings()).GenerateTextures(material, saveAsPng, texturesPath, completion);
            }

            // TODO: Generate all textures
            throw new System.NotImplementedException("StandardLiteGenerator.GenerateTextures not implemented");
        }

        private AsyncCallbackRequest GenerateMainTexture(MaterialBase material, StandardLiteConvertSettings settings, bool saveAsPng, string texturesPath, Action<Texture2D> completion)
        {
            return MaterialGeneratorUtility.GenerateTexture(material.Material, settings, "main", saveAsPng, texturesPath, (compl) => (material as IStandardLiteConvertable).GenerateStandardLiteMain(settings, compl), completion);
        }

        private AsyncCallbackRequest GenerateMetallicSmoothnessTexture(MaterialBase material, StandardLiteConvertSettings settings, bool saveAsPng, string texturesPath, Action<Texture2D> completion)
        {
            return MaterialGeneratorUtility.GenerateParameterTexture(material.Material, settings, "metallicSmoothness", saveAsPng, texturesPath, (compl) => (material as IStandardLiteConvertable).GenerateStandardLiteMetallicSmoothness(settings, compl), completion);
        }

        private AsyncCallbackRequest GenerateEmissionTexture(MaterialBase material, StandardLiteConvertSettings settings, bool saveAsPng, string texturesPath, Action<Texture2D> completion)
        {
            return MaterialGeneratorUtility.GenerateTexture(material.Material, settings, "emission", saveAsPng, texturesPath, (compl) => (material as IStandardLiteConvertable).GenerateStandardLiteEmission(settings, compl), completion);
        }

        private AsyncCallbackRequest GenerateNormalTexture(MaterialBase material, StandardLiteConvertSettings settings, bool inputRGB, bool outputRGB, bool saveAsPng, string texturesPath, Action<Texture2D> completion)
        {
            return MaterialGeneratorUtility.GenerateNormalMap(material.Material, settings, "normal", saveAsPng, texturesPath, (compl) => (material as IStandardLiteConvertable).GenerateStandardLiteNormalMap(settings, inputRGB, outputRGB, compl), completion);
        }
    }
}
