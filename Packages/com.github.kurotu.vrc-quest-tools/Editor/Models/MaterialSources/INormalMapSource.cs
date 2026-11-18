// <copyright file="INormalMapSource.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Source material which has a tangent space normal map.
    /// </summary>
    internal interface INormalMapSource
    {
        /// <summary>
        /// Gets a value indicating whether the source renders a normal map.
        /// </summary>
        bool UseNormalMap { get; }

        /// <summary>
        /// Gets the original normal map texture.
        /// </summary>
        Texture NormalMap { get; }

        /// <summary>
        /// Gets how the source samples the normal map.
        /// </summary>
        TextureUV NormalMapUV { get; }

        /// <summary>
        /// Gets the normal map scale (bump scale).
        /// </summary>
        float NormalMapScale { get; }

        /// <summary>
        /// Generates the normal map in the original normal map's texture space.
        /// </summary>
        /// <param name="outputRGB">Whether to output RGB encoding instead of AG.</param>
        /// <param name="maxTextureSize">Max texture size used when the source has no platform override.</param>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        AsyncCallbackRequest GenerateNormalMap(bool outputRGB, TextureSizeLimit maxTextureSize, Action<Texture2D> completion);

        /// <summary>
        /// Gets the platform override settings of the normal map.
        /// </summary>
        /// <returns>Platform override settings, or null if none.</returns>
        (int MaxTextureSize, TextureFormat Format)? GetNormalMapPlatformOverride();
    }
}
