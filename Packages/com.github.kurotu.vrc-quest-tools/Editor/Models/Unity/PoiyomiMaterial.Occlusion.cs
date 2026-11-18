// <copyright file="PoiyomiMaterial.Occlusion.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <content>
    /// Occlusion source implementation of Poiyomi material.
    /// </content>
    internal partial class PoiyomiMaterial : IOcclusionSource
    {
        /// <inheritdoc/>
        bool IOcclusionSource.UseOcclusionMap
        {
            get
            {
                if (AOMap == null)
                {
                    return false;
                }

                var (_, strength) = GetPrimaryAOChannelAndStrength();
                return strength > 0;
            }
        }

        /// <inheritdoc/>
        TextureUV IOcclusionSource.OcclusionMapUV => new TextureUV(TextureUVBasis.MeshUV0, AOMapTextureScale, AOMapTextureOffset);

        /// <inheritdoc/>
        float IOcclusionSource.OcclusionStrength
        {
            get
            {
                var (_, strength) = GetPrimaryAOChannelAndStrength();
                return strength;
            }
        }

        /// <inheritdoc/>
        AsyncCallbackRequest IOcclusionSource.GenerateOcclusionMap(TextureSizeLimit maxTextureSize, Action<Texture2D> completion)
        {
            var aoMap = (Texture2D)AOMap;
            var (channel, _) = GetPrimaryAOChannelAndStrength();

            var platformOverride = TextureUtility.GetBestPlatformOverrideSettings(aoMap);
            var maxSize = platformOverride?.MaxTextureSize ?? (int)maxTextureSize;
            var (width, height) = TextureUtility.AspectFitReduction(aoMap.width, aoMap.height, maxSize);

            var swizzleMat = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));
            swizzleMat.SetTexture("_Texture0", aoMap);
            swizzleMat.SetFloat("_Texture0Input", channel);
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

        /// <summary>
        /// Gets the first AO channel which has a positive strength, and its strength.
        /// </summary>
        /// <returns>Channel index (0-3) and strength. Defaults to R channel at full strength.</returns>
        internal (int Channel, float Strength) GetPrimaryAOChannelAndStrength()
        {
            var strengths = new float[]
            {
                AOStrengthR,
                AOStrengthG,
                AOStrengthB,
                AOStrengthA,
            };
            for (int i = 0; i < strengths.Length; i++)
            {
                if (strengths[i] > 0)
                {
                    return (i, strengths[i]);
                }
            }

            return (0, 1.0f); // Default to R channel at full strength
        }
    }
}
