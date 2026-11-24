// <copyright file="LilToonMaterial.Emission.cs" company="kurotu">
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
    /// Emission source implementation of lilToon material.
    /// </content>
    internal partial class LilToonMaterial : IEmissionSource
    {
        /// <inheritdoc/>
        bool IEmissionSource.UseEmission => UseEmission || UseEmission2nd;

        /// <inheritdoc/>
        bool IEmissionSource.UseEmissionMap
        {
            get
            {
                if (UseEmission && (EmissionMap != null || EmissionBlendMask != null || EmissionBlend < 1.0f))
                {
                    return true;
                }
                if (UseEmission2nd && (Emission2ndMap != null || Emission2ndBlendMask != null || Emission2ndBlend < 1.0f))
                {
                    return true;
                }
                return UseEmission && UseEmission2nd;
            }
        }

        /// <inheritdoc/>
        Color IEmissionSource.EmissionColor
        {
            get
            {
                if (UseEmission && UseEmission2nd)
                {
                    return Color.white;
                }
                if (UseEmission)
                {
                    return EmissionColor;
                }
                if (UseEmission2nd)
                {
                    return Emission2ndColor;
                }
                return Color.black;
            }
        }

        /// <inheritdoc/>
        float IEmissionSource.EmissionStrength => 1.0f;

        /// <inheritdoc/>
        TextureUV IEmissionSource.EmissionMapUV => TextureUV.MeshUV0Identity;

        /// <inheritdoc/>
        AsyncCallbackRequest IEmissionSource.GenerateEmissionMap(AlbedoBakeRequest albedo, TextureSizeLimit maxTextureSize, Action<Texture2D> completion)
        {
            Texture2D main = null;
            var mTask = ((IAlbedoSource)this).GenerateAlbedo(albedo, (tex) =>
            {
                main = tex;
            });
            mTask.WaitForCompletion();

            var bakeMat = new Material(Material);
            bakeMat.parent = null;
            bakeMat.shader = Shader.Find("Hidden/VRCQuestTools/lilToon/Emission");
            var bakeMatWrapper = new LilToonMaterial(bakeMat);
            bakeMatWrapper.Material.SetTexture("_VQT_AlbedoTex", main);
            bakeMatWrapper.EmissionColor = Utils.ColorUtility.HdrToLdr(bakeMatWrapper.EmissionColor);
            bakeMatWrapper.Emission2ndColor = Utils.ColorUtility.HdrToLdr(bakeMatWrapper.Emission2ndColor);

            var sourceWidth = Mathf.Max(
                EmissionMap ? EmissionMap.width : 4,
                EmissionBlendMask ? EmissionBlendMask.width : 4,
                Emission2ndMap ? Emission2ndMap.width : 4,
                Emission2ndBlendMask ? Emission2ndBlendMask.width : 4);
            var sourceHeight = Mathf.Max(
                EmissionMap ? EmissionMap.height : 4,
                EmissionBlendMask ? EmissionBlendMask.height : 4,
                Emission2ndMap ? Emission2ndMap.height : 4,
                Emission2ndBlendMask ? Emission2ndBlendMask.height : 4);

            // Check platform override settings from source textures used in emission shader
            var platformOverride = TextureUtility.GetBestPlatformOverrideSettings(
                EmissionMap,
                EmissionBlendMask,
                Emission2ndMap,
                Emission2ndBlendMask);
            var maxSize = platformOverride?.MaxTextureSize ?? (int)maxTextureSize;

            var (targetWidth, targetHeight) = TextureUtility.AspectFitReduction(sourceWidth, sourceHeight, maxSize);
            var rt = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(EmissionMap, rt, bakeMat);
            return TextureUtility.RequestReadbackRenderTexture(rt, true, false, (tex) =>
            {
                RenderTexture.ReleaseTemporary(rt);
                UnityEngine.Object.DestroyImmediate(bakeMat);
                if (main != null)
                {
                    UnityEngine.Object.DestroyImmediate(main);
                }
                completion?.Invoke(tex);
            });
        }

        /// <inheritdoc/>
        (int MaxTextureSize, TextureFormat Format)? IEmissionSource.GetEmissionMapPlatformOverride()
        {
            // Only use color textures, and only if the feature is actually enabled
            var textures = new List<Texture>(2);

            if (UseEmission && EmissionMap != null)
            {
                textures.Add(EmissionMap);
            }

            if (UseEmission2nd && Emission2ndMap != null)
            {
                textures.Add(Emission2ndMap);
            }

            if (textures.Count > 0)
            {
                return TextureUtility.GetBestPlatformOverrideSettings(textures.ToArray());
            }

            return null;
        }
    }
}
