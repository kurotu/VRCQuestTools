using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using System;
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

        /// <summary>
        /// Generate material for MatCap Lit shader.
        /// </summary>
        /// <param name="material">Material to convert.</param>
        /// <param name="saveTextureAsPng">Whether to save textures as png.</param>
        /// <param name="texturesPath">Path to save textures.</param>
        /// <returns>Generated material.</returns>
        public TextureReadbackRequest GenerateMaterial(MaterialBase material, bool saveTextureAsPng, string texturesPath, Action<Material> completion)
        {
            return toonLitGenerator.GenerateMaterial(material, saveTextureAsPng, texturesPath, newMaterial =>
            {
                newMaterial.shader = Shader.Find("VRChat/Mobile/MatCap Lit");
                newMaterial.SetTexture("_MatCap", matCapLitConvertSettings.matCapTexture);
            });
        }

        /// <summary>
        /// Generate textures for MatCap Lit shader.
        /// </summary>
        /// <param name="material">Material to convert.</param>
        /// <param name="saveAsPng">Whether to save textures as png.</param>
        /// <param name="texturesPath">Path to save textures.</param>
        public TextureReadbackRequest GenerateTextures(MaterialBase material, bool saveAsPng, string texturesPath, Action completion)
        {
            return toonLitGenerator.GenerateTextures(material, saveAsPng, texturesPath, completion);
        }
    }
}
