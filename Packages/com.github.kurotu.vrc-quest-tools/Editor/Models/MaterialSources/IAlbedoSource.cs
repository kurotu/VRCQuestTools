// <copyright file="IAlbedoSource.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Source material which can bake its albedo (main texture).
    /// </summary>
    internal interface IAlbedoSource
    {
        /// <summary>
        /// Gets the tiling and offset of the main texture on mesh UV0. The baked albedo is in the main texture's space.
        /// </summary>
        (Vector2 Scale, Vector2 Offset) MainTextureST { get; }

        /// <summary>
        /// Gets whether the albedo needs a texture for the request. When false, the main color alone represents the albedo.
        /// </summary>
        /// <param name="request">Bake request.</param>
        /// <returns>True when a texture is needed.</returns>
        bool RequiresAlbedoTexture(AlbedoBakeRequest request);

        /// <summary>
        /// Bakes the albedo.
        /// </summary>
        /// <param name="request">Bake request.</param>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        AsyncCallbackRequest GenerateAlbedo(AlbedoBakeRequest request, Action<Texture2D> completion);

        /// <summary>
        /// Gets the platform override settings of the source textures which the albedo is baked from.
        /// </summary>
        /// <returns>Platform override settings, or null if none.</returns>
        (int MaxTextureSize, TextureFormat Format)? GetAlbedoPlatformOverride();
    }
}
