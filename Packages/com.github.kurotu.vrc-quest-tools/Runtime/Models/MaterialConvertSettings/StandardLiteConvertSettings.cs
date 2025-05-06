using System;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Settings for standard lite material conversion.
    /// </summary>
    [Serializable]
    public class StandardLiteConvertSettings : IMaterialConvertSettings
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
        /// Whether to use minimum brightness for albedo color.
        /// </summary>
        public bool useMinimumBrightness = true;

        /// <summary>
        /// Whether to use auto minimum brightness.
        /// </summary>
        public bool autoMinimumBrightness = true;

        /// <summary>
        /// Minimum brightness for albedo color.
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float minimumBrightness = 0.05f;

        /// <inheritdoc/>
        public MobileTextureFormat MobileTextureFormat => mobileTextureFormat;

        /// <inheritdoc/>
        public string GetCacheKey()
        {
            return JsonUtility.ToJson(this);
        }
    }
}
