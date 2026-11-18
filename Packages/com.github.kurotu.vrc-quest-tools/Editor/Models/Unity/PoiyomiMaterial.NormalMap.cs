// <copyright file="PoiyomiMaterial.NormalMap.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <content>
    /// Normal map source implementation of Poiyomi material.
    /// </content>
    internal partial class PoiyomiMaterial : INormalMapSource
    {
        /// <inheritdoc/>
        bool INormalMapSource.UseNormalMap => NormalMap != null;

        /// <inheritdoc/>
        Texture INormalMapSource.NormalMap => NormalMap;

        /// <inheritdoc/>
        TextureUV INormalMapSource.NormalMapUV => new TextureUV(TextureUVBasis.MeshUV0, NormalMapTextureScale, NormalMapTextureOffset);

        /// <inheritdoc/>
        float INormalMapSource.NormalMapScale => NormalMapScale;

        /// <inheritdoc/>
        AsyncCallbackRequest INormalMapSource.GenerateNormalMap(bool outputRGB, TextureSizeLimit maxTextureSize, Action<Texture2D> completion)
        {
            var normal = (Texture2D)NormalMap;
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
