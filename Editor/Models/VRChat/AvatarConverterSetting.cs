using UnityEngine;

namespace KRT.VRCQuestTools.Models.VRChat
{
    /// <summary>
    /// Parameters for AvatarConverter.
    /// </summary>
    internal class AvatarConverterSetting
    {
        /// <summary>
        /// Whether a converter geenrates textures for Quest.
        /// </summary>
        internal bool generateQuestTextures;

        /// <summary>
        /// Maximum textures size when generating.
        /// </summary>
        internal int maxTextureSize;

        /// <summary>
        /// Main texture brightness, [0:1].
        /// </summary>
        internal float mainTextureBrightness;

        /// <summary>
        /// Animator Override Controllers to apply.
        /// </summary>
        internal AnimatorOverrideController[] overrideControllers;
    }
}
