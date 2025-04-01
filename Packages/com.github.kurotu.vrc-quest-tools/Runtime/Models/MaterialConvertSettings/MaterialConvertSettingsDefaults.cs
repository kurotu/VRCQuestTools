using System.Collections.Generic;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Default values for material convert settings.
    /// </summary>
    public static class MaterialConvertSettingsDefaults
    {
        /// <summary>
        /// Default brightness mode for main texture.
        /// </summary>
        public const BrightnessMode DefaultBrightnessMode = BrightnessMode.Linear;

        /// <summary>
        /// Default brightness for main texture. [0-1].
        /// </summary>
        public static readonly IReadOnlyDictionary<BrightnessMode, float> DefaultBrightness = new Dictionary<BrightnessMode, float>
        {
            { BrightnessMode.Linear, 0.83f },
            { BrightnessMode.Lab, 0.9f },
        };
    }
}
