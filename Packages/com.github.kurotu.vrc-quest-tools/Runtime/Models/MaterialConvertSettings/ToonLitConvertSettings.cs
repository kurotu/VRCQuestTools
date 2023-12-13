using System;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Material convert setting for Toon Lit shader.
    /// </summary>
    [Serializable]
    public class ToonLitConvertSettings : IToonLitConvertSettings
    {
        /// <summary>
        /// Whether to generate quest textures.
        /// </summary>
        public bool generateQuestTextures = true;

        /// <summary>
        /// Max texture size for quest.
        /// </summary>
        public TextureSizeLimit maxTextureSize = TextureSizeLimit.Max1024x1024;

        /// <summary>
        /// Texture brightness for quest. [0-1].
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float mainTextureBrightness = 0.83f;

        /// <summary>
        /// Whether to generate shadow from normal map.
        /// </summary>
        public bool generateShadowFromNormalMap = true;

        /// <inheritdoc/>
        public bool GenerateQuestTextures => generateQuestTextures;

        /// <inheritdoc/>
        public TextureSizeLimit MaxTextureSize => maxTextureSize;

        /// <inheritdoc/>
        public float MainTextureBrightness => mainTextureBrightness;

        /// <inheritdoc/>
        public bool GenerateShadowFromNormalMap => generateShadowFromNormalMap;
    }
}
