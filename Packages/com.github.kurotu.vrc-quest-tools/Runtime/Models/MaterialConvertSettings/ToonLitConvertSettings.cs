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
        /// Texture format for android.
        /// </summary>
        public MobileTextureFormat mobileTextureFormat = MobileTextureFormat.ASTC_6x6;

        /// <summary>
        /// Texture brightness for quest. [0-1].
        /// </summary>
        /// <remarks>
        /// Toon Lit shader adds indirect light to main texture, but final color will be about 150% of texture color.
        /// So 1.0 is too bright for light colors. (1 / 1.5 => 0.67 is a little dark.)
        /// </remarks>
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
        public MobileTextureFormat MobileTextureFormat => mobileTextureFormat;

        /// <inheritdoc/>
        public float MainTextureBrightness => mainTextureBrightness;

        /// <inheritdoc/>
        public bool GenerateShadowFromNormalMap => generateShadowFromNormalMap;

        /// <inheritdoc/>
        public void LoadDefaultAssets()
        {
            // nothing.
        }

        /// <inheritdoc/>
        public string GetCacheKey()
        {
            return JsonUtility.ToJson(this);
        }
    }
}
