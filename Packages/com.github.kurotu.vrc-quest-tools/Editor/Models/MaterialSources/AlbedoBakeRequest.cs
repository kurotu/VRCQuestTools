// <copyright file="AlbedoBakeRequest.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Request to bake the albedo (main texture) of a source material.
    /// The flags describe what the target shader renders by itself; everything else is folded into the albedo.
    /// </summary>
    internal class AlbedoBakeRequest
    {
        /// <summary>
        /// Whether textures are generated for mobile.
        /// </summary>
        internal bool GenerateQuestTextures = true;

        /// <summary>
        /// Max texture size.
        /// </summary>
        internal TextureSizeLimit MaxTextureSize = TextureSizeLimit.Max1024x1024;

        /// <summary>
        /// Texture format for mobile.
        /// </summary>
        internal MobileTextureFormat MobileTextureFormat = MobileTextureFormat.NoOverride;

        /// <summary>
        /// Brightness multiplied to the albedo.
        /// </summary>
        internal float Brightness = 1.0f;

        /// <summary>
        /// Whether to fake shading from the normal map, for targets which do not render the normal map.
        /// </summary>
        internal bool ShadeFromNormalMap;

        /// <summary>
        /// Whether to fold emission into the albedo, for targets which do not render the source's emission.
        /// </summary>
        internal bool FoldEmission = true;

        /// <summary>
        /// Whether the target renders the source's matcap by itself, so the albedo is prepared for it instead of including it.
        /// </summary>
        internal bool TargetRendersMatcap;

        /// <summary>
        /// Creates a request for Toon Lit style targets, which fold emission into the albedo and do not render matcap.
        /// </summary>
        /// <param name="settings">Toon Lit convert settings.</param>
        /// <returns>Bake request.</returns>
        internal static AlbedoBakeRequest FromToonLitSettings(IToonLitConvertSettings settings)
        {
            return new AlbedoBakeRequest
            {
                GenerateQuestTextures = settings.GenerateQuestTextures,
                MaxTextureSize = settings.MaxTextureSize,
                MobileTextureFormat = settings.MobileTextureFormat,
                Brightness = settings.MainTextureBrightness,
                ShadeFromNormalMap = settings.GenerateShadowFromNormalMap,
                FoldEmission = true,
                TargetRendersMatcap = false,
            };
        }
    }
}
