// <copyright file="LilToonMaterial.MinimumBrightness.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <content>
    /// Minimum brightness source implementation of lilToon material.
    /// </content>
    internal partial class LilToonMaterial : IMinimumBrightnessSource
    {
        /// <inheritdoc/>
        float IMinimumBrightnessSource.MinimumBrightness => LightMinLimit;
    }
}
