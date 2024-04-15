using KRT.VRCQuestTools.Models.Unity;
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
        public Material GenerateMaterial(MaterialBase material, bool saveTextureAsPng, string texturesPath)
        {
            var newMaterial = toonLitGenerator.GenerateMaterial(material, saveTextureAsPng, texturesPath);
            newMaterial.shader = Shader.Find("VRChat/Mobile/MatCap Lit");
            newMaterial.SetTexture("_MatCap", matCapLitConvertSettings.matCapTexture);
            return newMaterial;
        }

        /// <summary>
        /// Generate textures for MatCap Lit shader.
        /// </summary>
        /// <param name="material">Material to convert.</param>
        /// <param name="saveAsPng">Whether to save textures as png.</param>
        /// <param name="texturesPath">Path to save textures.</param>
        public void GenerateTextures(MaterialBase material, bool saveAsPng, string texturesPath)
        {
            toonLitGenerator.GenerateTextures(material, saveAsPng, texturesPath);
        }
    }
}
