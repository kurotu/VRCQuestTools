using System;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
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
        /// <param name="buildTarget">Build target.</param>
        /// <param name="saveTextureAsPng">Whether to save textures as png.</param>
        /// <param name="texturesPath">Path to save textures.</param>
        /// <param name="completion">Completion action.</param>
        /// <returns>Callback request to wait.</returns>
        AsyncCallbackRequest GenerateMaterial(MaterialBase material, UnityEditor.BuildTarget buildTarget, bool saveTextureAsPng, string texturesPath, Action<Material> completion);

        /// <summary>
        /// Generates textures.
        /// </summary>
        /// <param name="material">Material to convert.</param>
        /// <param name="buildTarget">Build target.</param>
        /// <param name="saveAsPng">Whether to save textures as png.</param>
        /// <param name="texturesPath">Path to save textures.</param>
        /// <param name="completion">Completion action.</param>
        /// <returns>Callback request to wait.</returns>
        AsyncCallbackRequest GenerateTextures(MaterialBase material, UnityEditor.BuildTarget buildTarget, bool saveAsPng, string texturesPath, Action completion);
    }
}
