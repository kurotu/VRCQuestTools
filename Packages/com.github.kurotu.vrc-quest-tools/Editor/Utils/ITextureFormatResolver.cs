// <copyright file="ITextureFormatResolver.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using KRT.VRCQuestTools.Models;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Interface for resolving texture format with platform override support.
    /// </summary>
    internal interface ITextureFormatResolver
    {
        /// <summary>
        /// Resolves the best texture format and size from material settings and platform overrides.
        /// </summary>
        /// <param name="sourceTextures">Source textures to check for platform overrides.</param>
        /// <param name="fallbackSettings">Fallback material convert settings if no overrides found.</param>
        /// <returns>Resolved texture format settings.</returns>
        TextureFormatResult ResolveTextureFormat(IEnumerable<Texture> sourceTextures, IMaterialConvertSettings fallbackSettings);
    }

    /// <summary>
    /// Result of texture format resolution containing format and size information.
    /// </summary>
    internal struct TextureFormatResult
    {
        /// <summary>
        /// Resolved mobile texture format.
        /// </summary>
        public MobileTextureFormat mobileTextureFormat;

        /// <summary>
        /// Maximum texture size (0 means no limit).
        /// </summary>
        public int maxTextureSize;

        /// <summary>
        /// Whether the result came from platform overrides (true) or fallback settings (false).
        /// </summary>
        public bool fromOverride;
    }
}