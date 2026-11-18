// <copyright file="IMinimumBrightnessSource.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Source material which keeps a minimum light intensity.
    /// </summary>
    internal interface IMinimumBrightnessSource
    {
        /// <summary>
        /// Gets the minimum light intensity the source applies to the albedo, as authored (0-1).
        /// </summary>
        float MinimumBrightness { get; }
    }
}
