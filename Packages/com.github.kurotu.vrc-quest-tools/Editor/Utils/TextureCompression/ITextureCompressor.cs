// <copyright file="ITextureCompressor.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Interface for texture compression backends.
    /// </summary>
    internal interface ITextureCompressor
    {
        /// <summary>
        /// Gets the identifier to embed into cache file names (e.g. "unity").
        /// </summary>
        string CacheKeyComponent { get; }

        /// <summary>
        /// Compresses a texture to the specified format.
        /// </summary>
        /// <param name="texture">Texture to compress.</param>
        /// <param name="format">Format to compress to.</param>
        /// <param name="completion">Completion action which receives the compressed texture.</param>
        /// <returns>Request to wait.</returns>
        AsyncCallbackRequest CompressTexture(Texture2D texture, TextureFormat format, Action<Texture2D> completion);

        /// <summary>
        /// Compresses a normal map texture with mipmap generation.
        /// </summary>
        /// <param name="texture">Normal map texture (RGB).</param>
        /// <param name="format">Format to compress to. When null, the platform format is left unset and Unity decides the format.</param>
        /// <param name="readable">Whether to make output texture readable.</param>
        /// <param name="maxTextureSize">Optional max texture size override.</param>
        /// <param name="completion">Completion action which receives the compressed normal map.</param>
        /// <returns>Request to wait.</returns>
        AsyncCallbackRequest CompressNormalMap(Texture2D texture, TextureFormat? format, bool readable, int? maxTextureSize, Action<Texture2D> completion);
    }
}
