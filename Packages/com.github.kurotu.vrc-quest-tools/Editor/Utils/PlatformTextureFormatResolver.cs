// <copyright file="PlatformTextureFormatResolver.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using KRT.VRCQuestTools.Models;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Implementation of texture format resolver that prioritizes platform overrides.
    /// </summary>
    internal class PlatformTextureFormatResolver : ITextureFormatResolver
    {
        /// <inheritdoc/>
        public TextureFormatResult ResolveTextureFormat(IEnumerable<Texture> sourceTextures, IMaterialConvertSettings fallbackSettings)
        {
            // First try to get platform override settings
            var overrideResult = TextureOverrideUtility.GetBestPlatformOverride(sourceTextures);
            
            if (overrideResult.hasOverride)
            {
                return new TextureFormatResult
                {
                    mobileTextureFormat = overrideResult.textureFormat,
                    maxTextureSize = overrideResult.maxTextureSize,
                    fromOverride = true
                };
            }

            // Fall back to material convert settings
            var maxSize = 0;
            if (fallbackSettings is ToonStandardConvertSettings toonStandardSettings)
            {
                maxSize = (int)toonStandardSettings.maxTextureSize;
            }
            else if (fallbackSettings is IToonLitConvertSettings toonLitSettings)
            {
                maxSize = (int)toonLitSettings.MaxTextureSize;
            }

            return new TextureFormatResult
            {
                mobileTextureFormat = fallbackSettings.MobileTextureFormat,
                maxTextureSize = maxSize,
                fromOverride = false
            };
        }
    }
}