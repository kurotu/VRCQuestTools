// <copyright file="MSMapGenViewModel.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using ImageMagick;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.ViewModels
{
    /// <summary>
    /// ViewModel for MSMapGenWindow.
    /// </summary>
    internal class MSMapGenViewModel : Object
    {
        /// <summary>
        /// Source metallic map.
        /// </summary>
        internal Texture2D metallicMap;

        /// <summary>
        /// Source smoothness / roughness map.
        /// </summary>
        internal Texture2D smoothnessMap;

        /// <summary>
        /// Should invert smoothness map.
        /// </summary>
        internal bool invertSmoothness;

        /// <summary>
        /// Gets a value indicating whether a window shows a generate button.
        /// </summary>
        internal bool DisableGenerateButton => (smoothnessMap == null) && (metallicMap == null);

        /// <summary>
        /// Generate and save MSMap.
        /// </summary>
        /// <param name="destPath">File path to save.</param>
        internal void GenerateMetallicSmoothness(string destPath)
        {
            using (var metallic = metallicMap ? MagickImageUtility.GetMagickImage(metallicMap) : new MagickImage(MagickColors.White, 2, 2))
            using (var smoothness = smoothnessMap ? MagickImageUtility.GetMagickImage(smoothnessMap) : new MagickImage(MagickColors.White, 2, 2))
            using (var msMap = MagickImageUtility.GenerateMetallicSmoothness(metallic, false, smoothness, invertSmoothness && smoothnessMap))
            {
                MagickImageUtility.SaveAsAsset(destPath, msMap, MagickFormat.Png32, false);
            }
        }
    }
}
