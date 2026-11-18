// <copyright file="LilToonMaterial.NormalMap.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <content>
    /// Normal map source implementation of lilToon material.
    /// </content>
    internal partial class LilToonMaterial : INormalMapSource
    {
        /// <inheritdoc/>
        bool INormalMapSource.UseNormalMap => UseNormalMap && NormalMap != null;

        /// <inheritdoc/>
        Texture INormalMapSource.NormalMap => NormalMap;

        /// <inheritdoc/>
        /// <remarks>lilToon samples the normal map with its tiling and offset applied to the main texture UV.</remarks>
        TextureUV INormalMapSource.NormalMapUV => new TextureUV(TextureUVBasis.MainUV, NormalMapTextureScale, NormalMapTextureOffset);

        /// <inheritdoc/>
        float INormalMapSource.NormalMapScale => NormalMapScale;

        /// <inheritdoc/>
        AsyncCallbackRequest INormalMapSource.GenerateNormalMap(bool outputRGB, TextureSizeLimit maxTextureSize, Action<Texture2D> completion)
        {
            var normal = (Texture2D)NormalMap;

            // Check platform override settings from source texture
            var platformOverride = TextureUtility.GetBestPlatformOverrideSettings(normal);
            var maxSize = platformOverride?.MaxTextureSize ?? (int)maxTextureSize;

            var (width, height) = TextureUtility.AspectFitReduction(normal.width, normal.height, maxSize);
            var newTex = TextureUtility.DownscaleNormalMap(normal, outputRGB, width, height);
            return new ResultRequest<Texture2D>(newTex, completion);
        }

        /// <inheritdoc/>
        (int MaxTextureSize, TextureFormat Format)? INormalMapSource.GetNormalMapPlatformOverride()
        {
            return TextureUtility.GetBestPlatformOverrideSettings(NormalMap);
        }
    }
}
