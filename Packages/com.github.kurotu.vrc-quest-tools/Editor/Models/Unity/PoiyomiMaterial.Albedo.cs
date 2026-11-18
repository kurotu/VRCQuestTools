// <copyright file="PoiyomiMaterial.Albedo.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <content>
    /// Albedo source implementation of Poiyomi material.
    /// </content>
    internal partial class PoiyomiMaterial : IAlbedoSource
    {
        /// <inheritdoc/>
        (Vector2 Scale, Vector2 Offset) IAlbedoSource.MainTextureST => (MainTextureScale, MainTextureOffset);

        /// <inheritdoc/>
        bool IAlbedoSource.RequiresAlbedoTexture(AlbedoBakeRequest request)
        {
            return Material.mainTexture != null || (((IEmissionSource)this).UseEmission && request.FoldEmission);
        }

        /// <inheritdoc/>
        AsyncCallbackRequest IAlbedoSource.GenerateAlbedo(AlbedoBakeRequest request, Action<Texture2D> completion)
        {
            var toonLitConvertSettings = request.ToToonLitConvertSettings();

            var toonLitBakeMat = new PoiyomiMaterial(new Material(Material));
            if (!request.FoldEmission)
            {
                // Emission is baked separately into the emission map, so it must not be baked into the
                // main (ToonLit) texture. The EnableEmissionN setter no-ops on locked Poiyomi shaders where
                // the toggle property was stripped, which would leave emission enabled after the shader is
                // swapped to the bake shader (Hidden/VRCQuestTools/Poiyomi declares _EnableEmission*),
                // double-counting emission. Write 0 directly to the bake shader's properties instead so the
                // value persists through the swap regardless of whether the source shader is locked.
                toonLitBakeMat.Material.SetFloat("_EnableEmission", 0.0f);
                toonLitBakeMat.Material.SetFloat("_EnableEmission1", 0.0f);
                toonLitBakeMat.Material.SetFloat("_EnableEmission2", 0.0f);
                toonLitBakeMat.Material.SetFloat("_EnableEmission3", 0.0f);
            }

            if (request.TargetRendersMatcap)
            {
                // Disable both matcap slots so neither is baked into the main (ToonLit) texture,
                // regardless of which slot is the active one.
                if (toonLitBakeMat.Material.HasProperty("_MatcapEnable"))
                {
                    toonLitBakeMat.Material.SetFloat("_MatcapEnable", 0.0f);
                }

                if (toonLitBakeMat.Material.HasProperty("_Matcap2Enable"))
                {
                    toonLitBakeMat.Material.SetFloat("_Matcap2Enable", 0.0f);
                }
            }

            return toonLitBakeMat.GenerateToonLitImage(toonLitConvertSettings, completion);
        }

        /// <inheritdoc/>
        (int MaxTextureSize, TextureFormat Format)? IAlbedoSource.GetAlbedoPlatformOverride()
        {
            return TextureUtility.GetBestPlatformOverrideSettings(Material.mainTexture);
        }
    }
}
