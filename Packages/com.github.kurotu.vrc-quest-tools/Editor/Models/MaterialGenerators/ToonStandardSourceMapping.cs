// <copyright file="ToonStandardSourceMapping.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using UnityEngine;
using UnityEngine.Rendering;
using static KRT.VRCQuestTools.Models.ToonStandardGenerator;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Maps a source material to Toon Standard specific values.
    /// Shader-neutral values are read through the source interfaces (<see cref="IAlbedoSource"/> etc.) instead.
    /// </summary>
    internal abstract class ToonStandardSourceMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToonStandardSourceMapping"/> class.
        /// </summary>
        /// <param name="settings">Convert settings.</param>
        /// <param name="sharedBlackTexture">Shared black texture to disable emission.</param>
        protected ToonStandardSourceMapping(ToonStandardConvertSettings settings, Texture2D sharedBlackTexture)
        {
            Settings = settings;
            SharedBlackTexture = sharedBlackTexture;
        }

        /// <summary>
        /// Gets the settings for the material conversion.
        /// </summary>
        protected ToonStandardConvertSettings Settings { get; }

        /// <summary>
        /// Gets the shared black texture to disable emission.
        /// </summary>
        protected Texture2D SharedBlackTexture { get; }

        /// <summary>
        /// Creates the Toon Standard mapping for the material.
        /// </summary>
        /// <param name="material">Source material.</param>
        /// <param name="settings">Convert settings.</param>
        /// <param name="sharedBlackTexture">Shared black texture to disable emission.</param>
        /// <returns>The mapping, or null when the material has no Toon Standard mapping.</returns>
        internal static ToonStandardSourceMapping Create(MaterialBase material, ToonStandardConvertSettings settings, Texture2D sharedBlackTexture)
        {
            if (material is LilToonMaterial lilMaterial)
            {
                return new LilToonToonStandardMapping(lilMaterial, settings, sharedBlackTexture);
            }

            if (material is PoiyomiMaterial poiyomiMaterial)
            {
                return new PoiyomiToonStandardMapping(poiyomiMaterial, settings, sharedBlackTexture);
            }

            return null;
        }

        /// <summary>
        /// Converts internal material to Toon Standard.
        /// </summary>
        /// <returns>Toon Standard Material.</returns>
        internal abstract Material ConvertToToonStandard();

        /// <summary>
        /// Gets the main color of the material.
        /// </summary>
        /// <returns>Main color.</returns>
        internal abstract Color GetMainColor();

        /// <summary>
        /// Gets the normal map texture scale and offset of the material.
        /// </summary>
        /// <returns>Scale and offset.</returns>
        internal abstract (Vector2 Scale, Vector2 Offset) GetNormalMapST();

        /// <summary>
        /// Gets the culling mode of the material.
        /// </summary>
        /// <returns>Culling mode.</returns>
        internal abstract CullMode GetCulling();

        /// <summary>
        /// Gets the material should use shadow.
        /// </summary>
        /// <returns>True if the material should use shadow ramp.</returns>
        internal abstract bool GetUseShadowRamp();

        /// <summary>
        /// Gets the min brightness of the material.
        /// </summary>
        /// <returns>Minimum brightness.</returns>
        internal abstract float GetMinBrightness();

        /// <summary>
        /// Gets the occulusion map texture scale and offset of the material.
        /// </summary>
        /// <returns>Scale and offset.</returns>
        internal abstract (Vector2 Scale, Vector2 Offset) GetOcculusionMapST();

        /// <summary>
        /// Gets the material should use specular.
        /// </summary>
        /// <returns>True if the material should use specular.</returns>
        internal abstract bool GetUseSpecular();

        /// <summary>
        /// Gets the material should use metallic map.
        /// </summary>
        /// <returns>True if the material should use metallic map.</returns>
        internal abstract bool GetUseMetallicMap();

        /// <summary>
        /// Gets the metallic map texture scale and offset of the material.
        /// </summary>
        /// <returns>Scale and offset.</returns>
        internal abstract (Vector2 Scale, Vector2 Offset) GetMetallicMapST();

        /// <summary>
        /// Gets the metallic strength of the material.
        /// </summary>
        /// <returns>Metallic strength.</returns>
        internal abstract float GetMetallicStrength();

        /// <summary>
        /// Gets the material should use gloss map.
        /// </summary>
        /// <returns>True if the material should use gloss map.</returns>
        internal abstract bool GetUseGlossMap();

        /// <summary>
        /// Gets the gloss map texture scale and offset of the material.
        /// </summary>
        /// <returns>Scale and offset.</returns>
        internal abstract (Vector2 Scale, Vector2 Offset) GetGlossMapST();

        /// <summary>
        /// Gets the gloss strength of the material.
        /// </summary>
        /// <returns>Gloss strength.</returns>
        internal abstract float GetGlossStrength();

        /// <summary>
        /// Gets the sharpness of the material.
        /// </summary>
        /// <returns>Sharpness.</returns>
        internal abstract float GetSharpness();

        /// <summary>
        /// Gets the reflectance of the material.
        /// </summary>
        /// <returns>Reflectance.</returns>
        internal abstract float GetReflectance();

        /// <summary>
        /// Gets the material should use matcap.
        /// </summary>
        /// <returns>True if the material should use matcap.</returns>
        internal abstract bool GetUseMatcap();

        /// <summary>
        /// Gets the matcap texture of the material.
        /// </summary>
        /// <returns>Matcap texture.</returns>
        internal abstract Texture GetMatcap();

        /// <summary>
        /// Gets the material should use matcap mask.
        /// </summary>
        /// <returns>True if the material should use matcap mask.</returns>
        internal abstract bool GetUseMatcapMask();

        /// <summary>
        /// Gets the matcap mask texture scale and offset of the material.
        /// </summary>
        /// <returns>Scale and offset.</returns>
        internal abstract (Vector2 Scale, Vector2 Offset) GetMatcapMaskST();

        /// <summary>
        /// Gets the matcap mask strength of the material.
        /// </summary>
        /// <returns>Matcap mask strength.</returns>
        internal abstract float GetMatcapMaskStrength();

        /// <summary>
        /// Gets the material should use rim lighting.
        /// </summary>
        /// <returns>True if the material should use rim lighting.</returns>
        internal abstract bool GetUseRimLighting();

        /// <summary>
        /// Gets the rim color of the material.
        /// </summary>
        /// <returns>Rim color.</returns>
        internal abstract Color GetRimColor();

        /// <summary>
        /// Gets the rim albedo tint of the material.
        /// </summary>
        /// <returns>Rim Albedo Tint.</returns>
        internal abstract float GetRimAlbedoTint();

        /// <summary>
        /// Gets the rim intensity of the material.
        /// </summary>
        /// <returns>Rim Intensity.</returns>
        internal abstract float GetRimIntensity();

        /// <summary>
        /// Gets the rim range of the material.
        /// </summary>
        /// <returns>Rim range.</returns>
        internal abstract float GetRimRange();

        /// <summary>
        /// Gets the rim softness of the material.
        /// </summary>
        /// <returns>Rim softness.</returns>
        internal abstract float GetRimSoftness();

        /// <summary>
        /// Gets the rim environmental of the material.
        /// </summary>
        /// <returns>Rim environment.</returns>
        internal abstract bool GetRimEnvironmental();

        /// <summary>
        /// Gets the matcap type of the material.
        /// </summary>
        /// <returns>Matcap type.</returns>
        internal abstract ToonStandardMaterialWrapper.MatcapTypeMode GetMapcapType();

        /// <summary>
        /// Generates the shadow ramp of the material.
        /// </summary>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        internal abstract AsyncCallbackRequest GenerateShadowRamp(Action<Texture2D> completion);

        /// <summary>
        /// Generates the metallic map of the material.
        /// </summary>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        internal abstract AsyncCallbackRequest GenerateMetallicMap(Action<Texture2D> completion);

        /// <summary>
        /// Generates the gloss map of the material.
        /// </summary>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        internal abstract AsyncCallbackRequest GenerateGlossMap(Action<Texture2D> completion);

        /// <summary>
        /// Generates the matcap of the material.
        /// </summary>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        internal abstract AsyncCallbackRequest GenerateMatcap(Action<Texture2D> completion);

        /// <summary>
        /// Generates the matcap mask of the material.
        /// </summary>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        internal abstract AsyncCallbackRequest GenerateMatcapMask(Action<Texture2D> completion);

        /// <summary>
        /// Generates a packed mask texture from the given texture pack.
        /// </summary>
        /// <param name="pack">Texture pack configuration.</param>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        internal abstract AsyncCallbackRequest GeneratePackedMask(TexturePack pack, Action<Texture2D> completion);

        /// <summary>
        /// Gets the platform override settings for matcap.
        /// </summary>
        /// <returns>Platform override settings, or null if none.</returns>
        internal abstract (int MaxTextureSize, TextureFormat Format)? GetMatcapPlatformOverride();

        /// <summary>
        /// Gets the platform override settings for matcap mask.
        /// </summary>
        /// <returns>Platform override settings, or null if none.</returns>
        internal abstract (int MaxTextureSize, TextureFormat Format)? GetMatcapMaskPlatformOverride();

        /// <summary>
        /// Gets the platform override settings for metallic map.
        /// </summary>
        /// <returns>Platform override settings, or null if none.</returns>
        internal abstract (int MaxTextureSize, TextureFormat Format)? GetMetallicMapPlatformOverride();

        /// <summary>
        /// Gets the platform override settings for gloss map.
        /// </summary>
        /// <returns>Platform override settings, or null if none.</returns>
        internal abstract (int MaxTextureSize, TextureFormat Format)? GetGlossMapPlatformOverride();

        /// <summary>
        /// Gets the platform override settings for packed mask.
        /// </summary>
        /// <param name="pack">The texture pack for which to get overrides.</param>
        /// <returns>Platform override settings, or null if none.</returns>
        internal abstract (int MaxTextureSize, TextureFormat Format)? GetPackedMaskPlatformOverride(TexturePack pack);
    }
}
