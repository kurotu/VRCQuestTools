using System;
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
        public AsyncCallbackRequest GenerateMaterial(MaterialBase material, bool saveTextureAsPng, string texturesPath, Action<Material> completion)
        {
            return toonLitGenerator.GenerateMaterial(material, saveTextureAsPng, texturesPath, newMaterial =>
            {
                newMaterial.shader = Shader.Find("VRChat/Mobile/MatCap Lit");
                newMaterial.SetTexture("_MatCap", matCapLitConvertSettings.matCapTexture);
            });
        }

        /// <inheritdoc/>
        public AsyncCallbackRequest GenerateTextures(MaterialBase material, bool saveAsPng, string texturesPath, Action completion)
        {
            return toonLitGenerator.GenerateTextures(material, saveAsPng, texturesPath, completion);
        }
    }
}
