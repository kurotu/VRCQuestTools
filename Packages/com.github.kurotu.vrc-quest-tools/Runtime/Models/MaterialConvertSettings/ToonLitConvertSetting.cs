using System;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Material convert setting for Toon Lit shader.
    /// </summary>
    [Serializable]
    public class ToonLitConvertSetting : IMaterialConvertSetting
    {
        /// <summary>
        /// Whether to generate quest textures.
        /// </summary>
        public bool generateQuestTextures = true;

        /// <summary>
        /// Max texture size for quest.
        /// </summary>
        public int maxTextureSize = 1024;

        /// <summary>
        /// Texture brightness for quest. [0-1].
        /// </summary>
        public float mainTextureBrightness = 0.83f;
    }
}
