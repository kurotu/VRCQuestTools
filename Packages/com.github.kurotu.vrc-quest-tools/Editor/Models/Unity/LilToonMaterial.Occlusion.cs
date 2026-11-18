// <copyright file="LilToonMaterial.Occlusion.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <content>
    /// Occlusion source implementation of lilToon material.
    /// </content>
    internal partial class LilToonMaterial : IOcclusionSource
    {
        /// <inheritdoc/>
        bool IOcclusionSource.UseOcclusionMap => UseShadow && AOMap != null;

        /// <inheritdoc/>
        /// <remarks>lilToon samples _ShadowBorderMask with the main texture UV and ignores its own tiling and offset.</remarks>
        TextureUV IOcclusionSource.OcclusionMapUV => TextureUV.MainUVIdentity;

        /// <inheritdoc/>
        float IOcclusionSource.OcclusionStrength
        {
            get
            {
                // lilToon's AO map only shifts shadow borders, so the darkest achievable color is bounded by
                // the shadow color chain. Derive the strength so that Toon Standard's lerp(1, sample, strength)
                // never darkens below the luminance of the fully shadowed color.
                // Shadow colors are composed in linear space by the shader, so convert sRGB inspector values first.
                var floor = Color.white;
                var c1 = ShadowColor;
                floor = Color.Lerp(floor, floor * c1.linear, c1.a);
                if (UseShadow2nd)
                {
                    var c2 = Shadow2ndColor;
                    floor = Color.Lerp(floor, floor * c2.linear, c2.a);
                }
                if (UseShadow3rd)
                {
                    var c3 = Shadow3rdColor;
                    floor = Color.Lerp(floor, floor * c3.linear, c3.a);
                }
                floor = Color.Lerp(Color.white, floor, ShadowStrength);
                var floorBrightness = Utils.ColorUtility.GetRec709Grayscale(floor);
                return Mathf.Clamp01(1.0f - floorBrightness);
            }
        }

        /// <inheritdoc/>
        AsyncCallbackRequest IOcclusionSource.GenerateOcclusionMap(TextureSizeLimit maxTextureSize, Action<Texture2D> completion)
        {
            var aoMap = (Texture2D)AOMap;

            // Check platform override settings from source texture
            var platformOverride = TextureUtility.GetBestPlatformOverrideSettings(aoMap);
            var maxSize = platformOverride?.MaxTextureSize ?? (int)maxTextureSize;

            var (width, height) = TextureUtility.AspectFitReduction(aoMap.width, aoMap.height, maxSize);

            var swizzleMat = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));
            swizzleMat.SetTexture("_Texture0", aoMap);

            // Bake the _ShadowAOShift remap (saturate(v * scale + offset)) of the selected channel into the mask.
            if (UseShadow3rd)
            {
                swizzleMat.SetFloat("_Texture0Input", 2);
                swizzleMat.SetFloat("_Texture0Scale", ShadowAOShift2.x);
                swizzleMat.SetFloat("_Texture0Offset", ShadowAOShift2.y);
            }
            else if (UseShadow2nd)
            {
                swizzleMat.SetFloat("_Texture0Input", 1);
                swizzleMat.SetFloat("_Texture0Scale", ShadowAOShift.z);
                swizzleMat.SetFloat("_Texture0Offset", ShadowAOShift.w);
            }
            else
            {
                swizzleMat.SetFloat("_Texture0Input", 0);
                swizzleMat.SetFloat("_Texture0Scale", ShadowAOShift.x);
                swizzleMat.SetFloat("_Texture0Offset", ShadowAOShift.y);
            }
            swizzleMat.SetFloat("_Texture0Output", 1); // G

            var rt = RenderTexture.GetTemporary(aoMap.width, aoMap.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(null, rt, swizzleMat);

            var rt2 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            TextureUtility.DownscaleBlit(rt, false, rt2);
            return TextureUtility.RequestReadbackRenderTexture(rt2, true, true, (tex) =>
            {
                RenderTexture.ReleaseTemporary(rt);
                RenderTexture.ReleaseTemporary(rt2);
                UnityEngine.Object.DestroyImmediate(swizzleMat);
                completion?.Invoke(tex);
            });
        }

        /// <inheritdoc/>
        (int MaxTextureSize, TextureFormat Format)? IOcclusionSource.GetOcclusionMapPlatformOverride()
        {
            return TextureUtility.GetBestPlatformOverrideSettings(AOMap);
        }
    }
}
