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
        /// Texture brightness mode for quest.
        /// </summary>
        public BrightnessMode mainTextureBrightnessMode = MaterialConvertSettingsDefaults.DefaultBrightnessMode;

        /// <summary>
        /// Texture brightness for quest. [0-1].
        /// </summary>
        /// <remarks>
        /// Toon Lit shader adds indirect light to main texture, but final color will be about 150% of texture color.
        /// So 1.0 is too bright for light colors. (1 / 1.5 => 0.67 is a little dark.)
        /// </remarks>
        [Range(0.0f, 1.0f)]
        public float mainTextureBrightness = MaterialConvertSettingsDefaults.DefaultBrightness[MaterialConvertSettingsDefaults.DefaultBrightnessMode];

        /// <summary>
        /// Whether to generate shadow from normal map.
        /// </summary>
        public bool generateShadowFromNormalMap = true;

        /// <inheritdoc/>
        public bool GenerateQuestTextures => generateQuestTextures;

        /// <inheritdoc/>
        public TextureSizeLimit MaxTextureSize => maxTextureSize;

        /// <inheritdoc/>
        public BrightnessMode MainTextureBrightnessMode => mainTextureBrightnessMode;

        /// <inheritdoc/>
        public float MainTextureBrightness => mainTextureBrightness;

        /// <inheritdoc/>
        public bool GenerateShadowFromNormalMap => generateShadowFromNormalMap;

        /// <inheritdoc/>
        public string GetCacheKey()
        {
            return $"{generateQuestTextures}_{maxTextureSize}_{mainTextureBrightnessMode}_{mainTextureBrightness}_{generateShadowFromNormalMap}";
        }
    }
}
