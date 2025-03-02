using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using System;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Interface for material generator.
    /// </summary>
    internal interface IMaterialGenerator
    {
        /// <summary>
        /// Generates a material.
        /// </summary>
        /// <param name="material">Material to convert.</param>
        /// <param name="saveTextureAsPng">Whether to save textures as png.</param>
        /// <param name="texturesPath">Path to save textures.</param>
        /// <returns>Generated material.</returns>
        TextureReadbackRequest GenerateMaterial(MaterialBase material, bool saveTextureAsPng, string texturesPath, Action<Material> completion);

        /// <summary>
        /// Generates textures.
        /// </summary>
        /// <param name="material">Material to convert.</param>
        /// <param name="saveAsPng">Whether to save textures as png.</param>
        /// <param name="texturesPath">Path to save textures.</param>
        TextureReadbackRequest GenerateTextures(MaterialBase material, bool saveAsPng, string texturesPath, Action completion);
    }
}
