// <copyright file="LilToonMaterial.Albedo.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <content>
    /// Albedo source implementation of lilToon material.
    /// </content>
    internal partial class LilToonMaterial : IAlbedoSource
    {
        /// <inheritdoc/>
        (Vector2 Scale, Vector2 Offset) IAlbedoSource.MainTextureST => (MainTextureScale, MainTextureOffset);

        /// <inheritdoc/>
        bool IAlbedoSource.RequiresAlbedoTexture(AlbedoBakeRequest request)
        {
            if (request.FoldEmission && (UseEmission || UseEmission2nd))
            {
                return true;
            }
            return Material.mainTexture != null;
        }

        /// <inheritdoc/>
        AsyncCallbackRequest IAlbedoSource.GenerateAlbedo(AlbedoBakeRequest request, Action<Texture2D> completion)
        {
            var toonLitConvertSettings = request.ToToonLitConvertSettings();
            var toonLitBakeMat = new LilToonMaterial(new Material(Material));
            if (!request.FoldEmission)
            {
                toonLitBakeMat.UseEmission = false;
                toonLitBakeMat.UseEmission2nd = false;
            }
            if (request.TargetRendersMatcap)
            {
                toonLitBakeMat.Material.SetFloat("_VQT_UseToonStandardMatCap", 1);
            }
            return toonLitBakeMat.GenerateToonLitImage(toonLitConvertSettings, completion);
        }

        /// <inheritdoc/>
        (int MaxTextureSize, TextureFormat Format)? IAlbedoSource.GetAlbedoPlatformOverride()
        {
            // Only use color textures, not mask textures, and only from enabled features
            var textures = new List<Texture>(3);

            if (Material.mainTexture != null)
            {
                textures.Add(Material.mainTexture);
            }

            if (UseMain2ndTex && Main2ndTex != null)
            {
                textures.Add(Main2ndTex);
            }

            if (UseMain3rdTex && Main3rdTex != null)
            {
                textures.Add(Main3rdTex);
            }

            if (textures.Count > 0)
            {
                return TextureUtility.GetBestPlatformOverrideSettings(textures.ToArray());
            }

            return null;
        }
    }
}
