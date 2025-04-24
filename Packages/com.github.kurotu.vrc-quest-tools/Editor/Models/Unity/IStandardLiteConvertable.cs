using System;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Interface for classes that can be converted to Standard Lite.
    /// </summary>
    internal interface IStandardLiteConvertable
    {
        /// <summary>
        /// Gets a value indicating whether to use standard lite metallic smoothness.
        /// </summary>
        bool UseStandardLiteMetallicSmoothness { get; }

        /// <summary>
        /// Gets a value indicating whether to use standard lite emission.
        /// </summary>
        bool UseStandardLiteEmission { get; }

        /// <summary>
        /// Gets a value indicating whether to use standard lite normal map.
        /// </summary>
        bool UseStandardLiteNormalMap { get; }

        /// <summary>
        /// Gets a value for minimum brightness.
        /// </summary>
        float MinimumBrightness { get; }

        /// <summary>
        /// Convert internal material to Standard Lite.
        /// </summary>
        /// <returns>Converted material.</returns>
        Material ConvertToStandardLite();

        /// <summary>
        /// Generate Standard Lite main texture.
        /// </summary>
        /// <param name="settings">Convert settings.</param>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Callback request.</returns>
        AsyncCallbackRequest GenerateStandardLiteMain(StandardLiteConvertSettings settings, Action<Texture2D> completion);

        /// <summary>
        /// Generate Standard Lite metallic texture.
        /// </summary>
        /// <param name="settings">Convert settings.</param>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Callback request.</returns>
        AsyncCallbackRequest GenerateStandardLiteMetallicSmoothness(StandardLiteConvertSettings settings, Action<Texture2D> completion);

        /// <summary>
        /// Generate Standard Lite normal map.
        /// </summary>
        /// <param name="settings">Convert settings.</param>
        /// <param name="inputRGB">Whether to use RGB input.</param>
        /// <param name="outputRGB">Whether to use RGB output.</param>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Callback request.</returns>
        AsyncCallbackRequest GenerateStandardLiteNormalMap(StandardLiteConvertSettings settings, bool inputRGB, bool outputRGB, Action<Texture2D> completion);

        /// <summary>
        /// Generate Standard Lite emission texture.
        /// </summary>
        /// <param name="settings">Convert settings.</param>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Callback request.</returns>
        AsyncCallbackRequest GenerateStandardLiteEmission(StandardLiteConvertSettings settings, Action<Texture2D> completion);
    }
}
