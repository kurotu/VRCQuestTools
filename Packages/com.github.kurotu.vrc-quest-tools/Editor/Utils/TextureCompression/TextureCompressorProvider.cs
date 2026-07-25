// <copyright file="TextureCompressorProvider.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Provides a texture compressor for a texture format.
    /// </summary>
    internal static class TextureCompressorProvider
    {
        private static readonly UnityTextureCompressor UnityCompressor = new UnityTextureCompressor();

        /// <summary>
        /// Gets a texture compressor to use for the format.
        /// </summary>
        /// <param name="format">Format to compress to. Null when the format is left unset for normal map compression.</param>
        /// <returns>Texture compressor.</returns>
        internal static ITextureCompressor GetCompressor(TextureFormat? format)
        {
            return UnityCompressor;
        }
    }
}
