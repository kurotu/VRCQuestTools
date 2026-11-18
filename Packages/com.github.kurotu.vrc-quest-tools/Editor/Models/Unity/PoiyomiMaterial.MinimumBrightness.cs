// <copyright file="PoiyomiMaterial.MinimumBrightness.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <content>
    /// Minimum brightness source implementation of Poiyomi material.
    /// </content>
    internal partial class PoiyomiMaterial : IMinimumBrightnessSource
    {
        /// <inheritdoc/>
        float IMinimumBrightnessSource.MinimumBrightness => LightingMinLightBrightness;
    }
}
