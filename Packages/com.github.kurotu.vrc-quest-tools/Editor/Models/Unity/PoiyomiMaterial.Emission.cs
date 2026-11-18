// <copyright file="PoiyomiMaterial.Emission.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <content>
    /// Emission source implementation of Poiyomi material.
    /// </content>
    internal partial class PoiyomiMaterial : IEmissionSource
    {
        /// <inheritdoc/>
        bool IEmissionSource.UseEmission => UseEmission;

        /// <inheritdoc/>
        bool IEmissionSource.UseEmissionMap =>
            IsEmissionChannelTexturized(EnableEmission0, EmissionMap0, EmissionMask0, EmissionBaseColorAsMap0)
                || IsEmissionChannelTexturized(EnableEmission1, EmissionMap1, EmissionMask1, EmissionBaseColorAsMap1)
                || IsEmissionChannelTexturized(EnableEmission2, EmissionMap2, EmissionMask2, EmissionBaseColorAsMap2)
                || IsEmissionChannelTexturized(EnableEmission3, EmissionMap3, EmissionMask3, EmissionBaseColorAsMap3);

        /// <inheritdoc/>
        Color IEmissionSource.EmissionColor
        {
            get
            {
                var enabledChannels = GetEnabledEmissionChannels().ToList();
                if (enabledChannels.Count > 1)
                {
                    return Color.white;
                }

                return enabledChannels.Count == 1
                    ? GetEmissionColorByIndex(enabledChannels[0])
                    : Color.black;
            }
        }

        /// <inheritdoc/>
        float IEmissionSource.EmissionStrength
        {
            get
            {
                var channels = GetEnabledEmissionChannels().ToArray();
                if (channels.Length != 1)
                {
                    return 1.0f;
                }

                switch (channels[0])
                {
                    case 0:
                        return EmissionStrength0;
                    case 1:
                        return EmissionStrength1;
                    case 2:
                        return EmissionStrength2;
                    case 3:
                        return EmissionStrength3;
                    default:
                        throw new InvalidOperationException("Unknown emission channel.");
                }
            }
        }

        /// <inheritdoc/>
        TextureUV IEmissionSource.EmissionMapUV => TextureUV.MeshUV0Identity;

        /// <inheritdoc/>
        AsyncCallbackRequest IEmissionSource.GenerateEmissionMap(AlbedoBakeRequest albedo, TextureSizeLimit maxTextureSize, Action<Texture2D> completion)
        {
            Texture2D main = null;
            ((IAlbedoSource)this).GenerateAlbedo(albedo, (tex) =>
            {
                main = tex;
            }).WaitForCompletion();

            var shader = Shader.Find("Hidden/VRCQuestTools/Poiyomi/Emission");
            if (shader == null)
            {
                throw new InvalidOperationException("Shader not found: Hidden/VRCQuestTools/Poiyomi/Emission");
            }

            var bakeMat = new Material(shader);
            bakeMat.parent = null;
            bakeMat.SetTexture("_VQT_AlbedoTex", main);
            SetEmissionBakeProperties(bakeMat);

            var (sourceWidth, sourceHeight) = GetEmissionSourceSize();
            var platformOverride = ((IEmissionSource)this).GetEmissionMapPlatformOverride();
            var maxSize = platformOverride?.MaxTextureSize ?? (int)maxTextureSize;
            var (targetWidth, targetHeight) = TextureUtility.AspectFitReduction(sourceWidth, sourceHeight, maxSize);

            var rt = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(null, rt, bakeMat);
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
            return TextureUtility.GetBestPlatformOverrideSettings(
                EmissionMap0,
                EmissionMask0,
                EmissionMap1,
                EmissionMask1,
                EmissionMap2,
                EmissionMask2,
                EmissionMap3,
                EmissionMask3);
        }

        /// <summary>
        /// Enumerates the enabled emission channels in ascending order.
        /// </summary>
        /// <returns>Enabled emission channel indices (0-3).</returns>
        internal IEnumerable<int> GetEnabledEmissionChannels()
        {
            if (EnableEmission0)
            {
                yield return 0;
            }

            if (EnableEmission1)
            {
                yield return 1;
            }

            if (EnableEmission2)
            {
                yield return 2;
            }

            if (EnableEmission3)
            {
                yield return 3;
            }
        }

        /// <summary>
        /// Gets the emission map of the emission channel.
        /// </summary>
        /// <param name="index">Emission channel index (0-3).</param>
        /// <returns>Emission map.</returns>
        internal Texture GetEmissionMapByIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return EmissionMap0;
                case 1:
                    return EmissionMap1;
                case 2:
                    return EmissionMap2;
                case 3:
                    return EmissionMap3;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        private static Color NormalizeHdrColor(Color color)
        {
            color = Utils.ColorUtility.HdrToLdr(color);
            color.a = Mathf.Min(color.a, 1.0f);
            return color;
        }

        private static bool IsEmissionChannelTexturized(bool enabled, Texture map, Texture mask, bool baseColorAsMap)
            => enabled && (map != null || mask != null || baseColorAsMap);

        private void SetEmissionBakeProperties(Material bakeMat)
        {
            bakeMat.SetFloat("_EnableEmission", EnableEmission0 ? 1.0f : 0.0f);
            bakeMat.SetTexture("_EmissionMap", EmissionMap0);
            bakeMat.SetTextureScale("_EmissionMap", EmissionMapTextureScale0);
            bakeMat.SetTextureOffset("_EmissionMap", EmissionMapTextureOffset0);
            bakeMat.SetColor("_EmissionColor", NormalizeHdrColor(EmissionColor0));
            bakeMat.SetTexture("_EmissionMask", EmissionMask0);
            bakeMat.SetTextureScale("_EmissionMask", EmissionMaskTextureScale0);
            bakeMat.SetTextureOffset("_EmissionMask", EmissionMaskTextureOffset0);
            bakeMat.SetFloat("_EmissionMaskChannel", EmissionMaskChannel0);
            bakeMat.SetFloat("_EmissionStrength", EmissionStrength0);
            bakeMat.SetFloat("_EmissionBaseColorAsMap", EmissionBaseColorAsMap0 ? 1.0f : 0.0f);

            bakeMat.SetFloat("_EnableEmission1", EnableEmission1 ? 1.0f : 0.0f);
            bakeMat.SetTexture("_EmissionMap1", EmissionMap1);
            bakeMat.SetTextureScale("_EmissionMap1", EmissionMapTextureScale1);
            bakeMat.SetTextureOffset("_EmissionMap1", EmissionMapTextureOffset1);
            bakeMat.SetColor("_EmissionColor1", NormalizeHdrColor(EmissionColor1));
            bakeMat.SetTexture("_EmissionMask1", EmissionMask1);
            bakeMat.SetTextureScale("_EmissionMask1", EmissionMaskTextureScale1);
            bakeMat.SetTextureOffset("_EmissionMask1", EmissionMaskTextureOffset1);
            bakeMat.SetFloat("_EmissionMaskChannel1", EmissionMaskChannel1);
            bakeMat.SetFloat("_EmissionStrength1", EmissionStrength1);
            bakeMat.SetFloat("_EmissionBaseColorAsMap1", EmissionBaseColorAsMap1 ? 1.0f : 0.0f);

            bakeMat.SetFloat("_EnableEmission2", EnableEmission2 ? 1.0f : 0.0f);
            bakeMat.SetTexture("_EmissionMap2", EmissionMap2);
            bakeMat.SetTextureScale("_EmissionMap2", EmissionMapTextureScale2);
            bakeMat.SetTextureOffset("_EmissionMap2", EmissionMapTextureOffset2);
            bakeMat.SetColor("_EmissionColor2", NormalizeHdrColor(EmissionColor2));
            bakeMat.SetTexture("_EmissionMask2", EmissionMask2);
            bakeMat.SetTextureScale("_EmissionMask2", EmissionMaskTextureScale2);
            bakeMat.SetTextureOffset("_EmissionMask2", EmissionMaskTextureOffset2);
            bakeMat.SetFloat("_EmissionMaskChannel2", EmissionMaskChannel2);
            bakeMat.SetFloat("_EmissionStrength2", EmissionStrength2);
            bakeMat.SetFloat("_EmissionBaseColorAsMap2", EmissionBaseColorAsMap2 ? 1.0f : 0.0f);

            bakeMat.SetFloat("_EnableEmission3", EnableEmission3 ? 1.0f : 0.0f);
            bakeMat.SetTexture("_EmissionMap3", EmissionMap3);
            bakeMat.SetTextureScale("_EmissionMap3", EmissionMapTextureScale3);
            bakeMat.SetTextureOffset("_EmissionMap3", EmissionMapTextureOffset3);
            bakeMat.SetColor("_EmissionColor3", NormalizeHdrColor(EmissionColor3));
            bakeMat.SetTexture("_EmissionMask3", EmissionMask3);
            bakeMat.SetTextureScale("_EmissionMask3", EmissionMaskTextureScale3);
            bakeMat.SetTextureOffset("_EmissionMask3", EmissionMaskTextureOffset3);
            bakeMat.SetFloat("_EmissionMaskChannel3", EmissionMaskChannel3);
            bakeMat.SetFloat("_EmissionStrength3", EmissionStrength3);
            bakeMat.SetFloat("_EmissionBaseColorAsMap3", EmissionBaseColorAsMap3 ? 1.0f : 0.0f);
        }

        private (int Width, int Height) GetEmissionSourceSize()
        {
            var width = 4;
            var height = 4;

            foreach (var idx in GetEnabledEmissionChannels())
            {
                var map = GetEmissionMapByIndex(idx);
                if (map)
                {
                    width = Mathf.Max(width, map.width);
                    height = Mathf.Max(height, map.height);
                }

                var mask = GetEmissionMaskByIndex(idx);
                if (mask)
                {
                    width = Mathf.Max(width, mask.width);
                    height = Mathf.Max(height, mask.height);
                }
            }

            return (width, height);
        }

        private Texture GetEmissionMaskByIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return EmissionMask0;
                case 1:
                    return EmissionMask1;
                case 2:
                    return EmissionMask2;
                case 3:
                    return EmissionMask3;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        private Color GetEmissionColorByIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return EmissionColor0;
                case 1:
                    return EmissionColor1;
                case 2:
                    return EmissionColor2;
                case 3:
                    return EmissionColor3;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }
}
