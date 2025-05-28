using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Utility class for color operations.
    /// </summary>
    internal static class ColorUtility
    {
        /// <summary>
        /// Calculates Rec.709 grayscale value from a color.
        /// </summary>
        /// <param name="color">Input color.</param>
        /// <returns>Grayscale value.</returns>
        internal static float GetRec709Grayscale(Color color)
        {
            return color.r * 0.2126f + color.g * 0.7152f + color.b * 0.0722f;
        }

        /// <summary>
        /// Converts HDR color to LDR color.
        /// </summary>
        /// <param name="color">HDR color.</param>
        /// <returns>Converted LDR color if the color is HDR.</returns>
        internal static Color HdrToLdr(Color color)
        {
            var maxColorComponent = color.maxColorComponent;
            if (maxColorComponent > 1.0f)
            {
                var ldr = color / maxColorComponent;
                ldr.a = color.a; // Preserve alpha
                ldr.r = Mathf.Pow(ldr.r, 1.0f / 2.2f);
                ldr.g = Mathf.Pow(ldr.g, 1.0f / 2.2f);
                ldr.b = Mathf.Pow(ldr.b, 1.0f / 2.2f);
                return ldr;
            }
            return color;
        }
    }
}