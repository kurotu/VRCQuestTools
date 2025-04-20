using System;

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

        /// <inheritdoc/>
        public string GetCacheKey()
        {
            return $"{generateQuestTextures}_{maxTextureSize}";
        }
    }
}
