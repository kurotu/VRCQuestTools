using KRT.VRCQuestTools.Models.Unity;
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
        /// <param name="texturesPath">Path to save textures.</param>
        /// <returns>Generated material.</returns>
        Material GenerateMaterial(MaterialBase material, string texturesPath);

        /// <summary>
        /// Generates textures.
        /// </summary>
        /// <param name="material">Material to convert.</param>
        /// <param name="texturesPath">Path to save textures.</param>
        void GenerateTextures(MaterialBase material, string texturesPath);
    }
}
