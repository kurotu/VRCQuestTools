// <copyright file="IOcclusionSource.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Source material which has an ambient occlusion map.
    /// </summary>
    internal interface IOcclusionSource
    {
        /// <summary>
        /// Gets a value indicating whether the source renders an occlusion map.
        /// </summary>
        bool UseOcclusionMap { get; }

        /// <summary>
        /// Gets how the source samples the occlusion map.
        /// </summary>
        TextureUV OcclusionMapUV { get; }

        /// <summary>
        /// Gets the strength k which approximates the source's occlusion as <c>lerp(1, occlusion, k)</c>.
        /// </summary>
        float OcclusionStrength { get; }

        /// <summary>
        /// Generates the occlusion map with the value in the G channel, in the original occlusion map's texture space.
        /// </summary>
        /// <param name="maxTextureSize">Max texture size used when the source has no platform override.</param>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        AsyncCallbackRequest GenerateOcclusionMap(TextureSizeLimit maxTextureSize, Action<Texture2D> completion);

        /// <summary>
        /// Gets the platform override settings of the occlusion map.
        /// </summary>
        /// <returns>Platform override settings, or null if none.</returns>
        (int MaxTextureSize, TextureFormat Format)? GetOcclusionMapPlatformOverride();
    }
}
