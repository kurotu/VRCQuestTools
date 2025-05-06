using System;
using System.Linq;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Material generator for MatCap Lit shader.
    /// </summary>
    internal class MatCapLitGenerator : IMaterialGenerator
    {
        private MatCapLitConvertSettings matCapLitConvertSettings;
        private ToonLitGenerator toonLitGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatCapLitGenerator"/> class.
        /// </summary>
        /// <param name="matCapLitConvertSettings">Convert setttings.</param>
        public MatCapLitGenerator(MatCapLitConvertSettings matCapLitConvertSettings)
        {
            this.matCapLitConvertSettings = matCapLitConvertSettings;
            toonLitGenerator = new ToonLitGenerator(matCapLitConvertSettings);
        }

        /// <inheritdoc/>
        public AsyncCallbackRequest GenerateMaterial(MaterialBase material, UnityEditor.BuildTarget buildTarget, bool saveTextureAsPng, string texturesPath, Action<Material> completion)
        {
            var originalName = material.Material.name;
            return toonLitGenerator.GenerateMaterial(material, buildTarget, saveTextureAsPng, texturesPath, newMaterial =>
            {
                var shader = Shader.Find("VRChat/Mobile/MatCap Lit");
                newMaterial.name = $"{originalName}_{shader.name.Split('/').Last()}";
                newMaterial.shader = shader;
                newMaterial.SetTexture("_MatCap", matCapLitConvertSettings.matCapTexture);
                completion?.Invoke(newMaterial);
            });
        }

        /// <inheritdoc/>
        public AsyncCallbackRequest GenerateTextures(MaterialBase material, UnityEditor.BuildTarget buildTarget, bool saveAsPng, string texturesPath, Action completion)
        {
            return toonLitGenerator.GenerateTextures(material, buildTarget, saveAsPng, texturesPath, completion);
        }
    }
}
