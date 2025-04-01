using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Material convert setting for MatCap Lit shader.
    /// </summary>
    [Serializable]
    public class MatCapLitConvertSettings : IMaterialConvertSettings, IToonLitConvertSettings
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
        [Range(0.0f, 1.0f)]
        public float mainTextureBrightness = MaterialConvertSettingsDefaults.DefaultBrightness[MaterialConvertSettingsDefaults.DefaultBrightnessMode];

        /// <summary>
        /// Whether to generate shadow from normal map.
        /// </summary>
        public bool generateShadowFromNormalMap = true;

        /// <summary>
        /// MatCap texture.
        /// </summary>
        public Texture matCapTexture;

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
#if UNITY_EDITOR
            return $"{generateQuestTextures}_{maxTextureSize}_{mainTextureBrightnessMode}_{mainTextureBrightness}_{generateShadowFromNormalMap}_{matCapTexture.imageContentsHash}";
#else
            throw new System.NotSupportedException("GetCacheKey is only available for UnityEditor.");
#endif
        }
    }
}
