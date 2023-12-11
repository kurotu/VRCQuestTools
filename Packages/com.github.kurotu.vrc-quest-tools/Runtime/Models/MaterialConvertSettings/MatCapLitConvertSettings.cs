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
        /// Texture brightness for quest. [0-1].
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float mainTextureBrightness = 1.0f;

        /// <summary>
        /// MatCap texture.
        /// </summary>
        public Texture matCapTexture;

        /// <inheritdoc/>
        public bool GenerateQuestTextures => generateQuestTextures;

        /// <inheritdoc/>
        public TextureSizeLimit MaxTextureSize => maxTextureSize;

        /// <inheritdoc/>
        public float MainTextureBrightness => mainTextureBrightness;
    }
}
