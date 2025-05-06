using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Enumeration for mobile texture formats. Its values are based on Unity's TextureFormat enum.
    /// </summary>
    public enum MobileTextureFormat
    {
#pragma warning disable SA1602 // Disable SA1602 for enum documentation
        ASTC_4x4 = TextureFormat.ASTC_4x4,
        ASTC_5x5 = TextureFormat.ASTC_5x5,
        ASTC_6x6 = TextureFormat.ASTC_6x6,
        ASTC_8x8 = TextureFormat.ASTC_8x8,
        ASTC_10x10 = TextureFormat.ASTC_10x10,
        ASTC_12x12 = TextureFormat.ASTC_12x12,
#pragma warning restore SA1602
    }
}
