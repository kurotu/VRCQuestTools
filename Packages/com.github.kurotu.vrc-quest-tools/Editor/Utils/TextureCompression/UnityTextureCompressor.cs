// <copyright file="UnityTextureCompressor.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using Unity.Collections;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Texture compressor which uses Unity's built-in texture compression.
    /// </summary>
    internal class UnityTextureCompressor : ITextureCompressor
    {
        /// <inheritdoc/>
        public string CacheKeyComponent => "unity";

        /// <inheritdoc/>
        /// <remarks>
        /// This is also the path used for the PC/Standalone DXT5 fallback (see
        /// <see cref="TextureUtility.ResolveEffectiveCompressionFormat"/>): unlike ASTC, which
        /// <see cref="AstcencTextureCompressor"/> instead runs out-of-process/async because Unity's built-in ASTC
        /// encoder is slow enough to stall NDMF preview, Unity's built-in DXT encoder was measured
        /// (<c>DxtBenchmarkTests</c>) at low tens of milliseconds even at 4096px -- no async/external-CLI path is
        /// warranted for it.
        /// </remarks>
        public AsyncCallbackRequest CompressTexture(Texture2D texture, TextureFormat format, Action<Texture2D> completion)
        {
            EditorUtility.CompressTexture(texture, format, TextureCompressionQuality.Best);
            return new ResultRequest<Texture2D>(texture, completion);
        }

        /// <inheritdoc/>
        public AsyncCallbackRequest CompressNormalMap(Texture2D texture, TextureFormat? format, bool readable, int? maxTextureSize, Action<Texture2D> completion)
        {
            var pixels = texture.GetPixels32(0);
            using (var colors = new NativeArray<Color32>(pixels, Allocator.Temp))
            {
                var settings = new TextureGenerationSettings(TextureImporterType.NormalMap);
                settings.textureImporterSettings.readable = readable;
                settings.textureImporterSettings.mipmapEnabled = true;
                settings.textureImporterSettings.streamingMipmaps = true;
                settings.textureImporterSettings.wrapMode = texture.wrapMode;
                settings.textureImporterSettings.filterMode = texture.filterMode;
                settings.textureImporterSettings.aniso = texture.anisoLevel;
                var currentMaxSize = Math.Max(texture.width, texture.height);
                settings.platformSettings.maxTextureSize = maxTextureSize.HasValue ? Math.Min(maxTextureSize.Value, currentMaxSize) : currentMaxSize;
                settings.sourceTextureInformation.width = texture.width;
                settings.sourceTextureInformation.height = texture.height;
                settings.sourceTextureInformation.containsAlpha = true;
                settings.sourceTextureInformation.hdr = false;
                if (format.HasValue)
                {
                    settings.platformSettings.format = (TextureImporterFormat)format.Value;
                }

                var output = TextureGenerator.GenerateTexture(settings, colors);
                output.texture.name = texture.name;
                return new ResultRequest<Texture2D>(output.texture, completion);
            }
        }
    }
}
