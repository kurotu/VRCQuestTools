// <copyright file="PoiyomiToonStandardMapping.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using UnityEngine;
using UnityEngine.Rendering;
using static KRT.VRCQuestTools.Models.ToonStandardGenerator;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Toon Standard mapping for Poiyomi materials.
    /// </summary>
    internal class PoiyomiToonStandardMapping : ToonStandardSourceMapping
    {
        private readonly PoiyomiMaterial poiyomiMaterial;

        /// <summary>
        /// Initializes a new instance of the <see cref="PoiyomiToonStandardMapping"/> class.
        /// </summary>
        /// <param name="material">Poiyomi material.</param>
        /// <param name="settings">Convert settings.</param>
        /// <param name="sharedBlackTexture">Shared black texture to disable emission.</param>
        internal PoiyomiToonStandardMapping(PoiyomiMaterial material, ToonStandardConvertSettings settings, Texture2D sharedBlackTexture)
            : base(settings, sharedBlackTexture)
        {
            this.poiyomiMaterial = material;
        }

        /// <summary>
        /// The Poiyomi feature that provides specular/reflection inputs for the conversion.
        /// </summary>
        private enum SpecularSource
        {
            /// <summary>No specular/reflection feature is enabled.</summary>
            None,

            /// <summary>Mochie BRDF "Reflections &amp; Specular" (_MochieBRDF).</summary>
            Mochie,

            /// <summary>Stylized Reflections, UnityChan mode (_StylizedSpecular, _StylizedReflectionMode==0).</summary>
            StylizedUnityChan,

            /// <summary>Stylized Reflections, lilToon mode (_StylizedSpecular, _StylizedReflectionMode==1).</summary>
            StylizedLilToon,
        }

        /// <inheritdoc/>
        internal override Material ConvertToToonStandard()
        {
            var newMaterial = new ToonStandardMaterialWrapper();
            newMaterial.Name = poiyomiMaterial.Material.name;

            newMaterial.MainTexture = poiyomiMaterial.Material.mainTexture;
            newMaterial.MainTextureScale = poiyomiMaterial.MainTextureScale;
            newMaterial.MainTextureOffset = poiyomiMaterial.MainTextureOffset;
            newMaterial.MainColor = Utils.ColorUtility.HdrToLdr(poiyomiMaterial.Material.color);

            // Vertex color: Poiyomi blends vertex color by amount (_MainVertexColoring Range 0-1),
            // but ToonStandard only has a boolean toggle. Enable when blend > 50%.
            // Partial blend amounts and per-channel vertex color (_MainColorAdjustTexture R/G/B/A packing)
            // cannot be reproduced in ToonStandard.
            newMaterial.UseVertexColor = poiyomiMaterial.UseVertexColor;

            if (((INormalMapSource)poiyomiMaterial).UseNormalMap && Settings.useNormalMap)
            {
                newMaterial.UseNormalMap = true;
                newMaterial.NormalMap = poiyomiMaterial.NormalMap;
                newMaterial.NormalMapTextureScale = poiyomiMaterial.NormalMapTextureScale;
                newMaterial.NormalMapTextureOffset = poiyomiMaterial.NormalMapTextureOffset;
                newMaterial.NormalMapScale = poiyomiMaterial.NormalMapScale;
            }

            // NOT SUPPORTED: _DetailNormalMap - Poiyomi's detail normal system (UV selectors,
            // stochastic sampling) differs structurally from ToonStandard's USE_DETAIL_MAPS.
            // ToonStandard does support detail maps, but mapping without a detail albedo/mask would
            // produce incomplete results. Skipped intentionally.

            newMaterial.Culling = poiyomiMaterial.CullMode;
            newMaterial.ShadowRamp = poiyomiMaterial.UseShadow
                ? Settings.fallbackShadowRamp
                : ToonStandardMaterialWrapper.RampTexture.Flat;

            // NOT SUPPORTED: _LightingDetailShadowMaps (detail shadow), _LightingShadowMasks (shadow mask layers)
            // - Poiyomi's detail shadow system uses a packed 4-channel texture with per-channel strength and
            //   blending modes. ToonStandard has no equivalent detail shadow layer.

            // Map Poiyomi's minimum brightness to ToonStandard's MinBrightness (clamped to 0-0.1 range).
            newMaterial.MinBrightness = Mathf.Min(poiyomiMaterial.LightingMinLightBrightness, 0.1f);

            // NOTE: _LightingCapEnabled ("Limit Brightness") maps to ToonStandard's _LimitBrightness.
            // ToonStandard's default is 1 (enabled), which is a safe default. Disabling it is not exposed
            // through ToonStandardMaterialWrapper and is intentionally left at the default.

            if (((IEmissionSource)poiyomiMaterial).UseEmission && Settings.useEmission)
            {
                if (((IEmissionSource)poiyomiMaterial).UseEmissionMap)
                {
                    // Use the texture scale/offset of the channel the primary map actually comes from.
                    // Poiyomi has four emission channels; the chosen map may be from channel 1/2/3, in
                    // which case channel 0's UV transform would sample it incorrectly.
                    var emissionChannel = GetPrimaryEmissionChannel();
                    newMaterial.EmissionMap = poiyomiMaterial.GetEmissionMapByIndex(emissionChannel);
                    newMaterial.EmissionMapTextureScale = GetEmissionMapTextureScaleByIndex(emissionChannel);
                    newMaterial.EmissionMapTextureOffset = GetEmissionMapTextureOffsetByIndex(emissionChannel);
                }

                newMaterial.EmissionColor = Utils.ColorUtility.HdrToLdr(((IEmissionSource)poiyomiMaterial).EmissionColor);
                newMaterial.EmissionStrength = ((IEmissionSource)poiyomiMaterial).EmissionStrength;
            }
            else
            {
                newMaterial.EmissionMap = SharedBlackTexture;
                newMaterial.EmissionColor = Color.black;
            }

            // NOT SUPPORTED: _Decal / _Decal1 / _Decal2 / _Decal3 - ToonStandard has no decal system.
            // NOT SUPPORTED: _EnableOutlines - Outline is a separate shader (ToonStandardOutline) and
            //   cannot be merged into this material conversion.
            // NOT SUPPORTED: _Mode (transparency/fade/cutout) - ToonStandard is Opaque-only.
            //   Non-opaque blend modes (Fade, Transparent, Additive, etc.) have no equivalent.
            // NOT SUPPORTED: Color grading (_Saturation, _MainColorAdjustTexture R/G/B mask, _Value) -
            //   ToonStandard has no color grading post-processing.

            if (((IOcclusionSource)poiyomiMaterial).UseOcclusionMap && Settings.useOcclusion)
            {
                var (aoChannel, aoStrength) = poiyomiMaterial.GetPrimaryAOChannelAndStrength();
                newMaterial.UseOcclusion = true;
                newMaterial.OcclusionMap = poiyomiMaterial.AOMap;
                newMaterial.OcclusionMapTextureScale = poiyomiMaterial.AOMapTextureScale;
                newMaterial.OcclusionMapTextureOffset = poiyomiMaterial.AOMapTextureOffset;
                newMaterial.OcclusionMapChannel = ChannelIndexToMaskChannel(aoChannel);
                newMaterial.OcclusionStrength = aoStrength;
            }

            // Specular/reflections: Stylized Reflections (_StylizedSpecular) take priority over the
            // Mochie BRDF "Reflections & Specular" (_MochieBRDF). The active source is resolved by
            // GetSpecularSource() and read through the unified accessors below.
            if (GetUseSpecular() && Settings.useSpecular)
            {
                var (metallicScale, metallicOffset) = GetSpecularMetallicMapST();
                var (glossScale, glossOffset) = GetSpecularGlossMapST();
                newMaterial.UseSpecular = true;
                newMaterial.MetallicMap = GetSpecularMetallicTexture();
                newMaterial.MetallicMapTextureScale = metallicScale;
                newMaterial.MetallicMapTextureOffset = metallicOffset;
                newMaterial.MetallicMapChannel = ChannelIndexToMaskChannel(GetSpecularMetallicChannel());
                newMaterial.MetallicStrength = GetMetallicStrength();

                newMaterial.GlossMap = GetSpecularGlossTexture();
                newMaterial.GlossMapTextureScale = glossScale;
                newMaterial.GlossMapTextureOffset = glossOffset;
                newMaterial.GlossMapChannel = ChannelIndexToMaskChannel(GetSpecularGlossChannel());
                newMaterial.GlossStrength = GetGlossStrength();
                newMaterial.Sharpness = GetSharpness();
                newMaterial.Reflectance = GetReflectance();
            }

            if (GetUseMatcap() && Settings.useMatcap)
            {
                newMaterial.UseMatcap = true;
                newMaterial.Matcap = poiyomiMaterial.MatcapTexture;
                newMaterial.MatcapMask = poiyomiMaterial.MatcapMask;
                newMaterial.MatcapMaskTextureScale = poiyomiMaterial.MatcapMaskTextureScale;
                newMaterial.MatcapMaskTextureOffset = poiyomiMaterial.MatcapMaskTextureOffset;
                newMaterial.MatcapMaskChannel = ChannelIndexToMaskChannel(poiyomiMaterial.MatcapMaskChannel);
                newMaterial.MatcapStrength = GetMatcapMaskStrength();
                newMaterial.MatcapType = GetMapcapType();

                // ToonStandard provides only one MatCap slot. Poiyomi has two (_MatcapEnable / _Matcap2Enable);
                // PoiyomiMaterial selects the lowest-numbered enabled slot (PoiyomiMaterial.ActiveMatcapSlot),
                // so the other MatCap layer is dropped.
            }

            if (GetUseRimLighting() && Settings.useRimLighting)
            {
                newMaterial.UseRimLighting = true;
                newMaterial.RimColor = Utils.ColorUtility.HdrToLdr(GetRimColor());
                newMaterial.RimAlbedoTint = GetRimAlbedoTint();
                newMaterial.RimIntensity = GetRimIntensity();
                newMaterial.RimRange = GetRimRange();
                newMaterial.RimSoftness = GetRimSoftness();
                newMaterial.RimEnvironmental = GetRimEnvironmental();

                // ToonStandard has only one rim slot. Poiyomi has two (_EnableRimLighting / _EnableRim2Lighting);
                // PoiyomiMaterial selects the lowest-numbered enabled slot (PoiyomiMaterial.ActiveRimSlot),
                // so the other rim layer is dropped.
            }

            // NOT SUPPORTED features with no ToonStandard equivalent:
            // - Dissolve / TPS Dissolve
            // - AudioLink effects (emission/rim modulation via AudioLink)
            // - Light Direction Masking for shadows
            // - Vertex Color partial blend (_MainVertexColoring as float) - ToonStandard is boolean only
            // - UV animation / panning (_MainTexPan, _EmissionMapPan, etc.) - no runtime animation support
            // - Panosphere / World Pos / Local Pos UV projections

            return newMaterial;
        }

        /// <inheritdoc/>
        internal override (int MaxTextureSize, TextureFormat Format)? GetMatcapPlatformOverride()
        {
            return TextureUtility.GetBestPlatformOverrideSettings(poiyomiMaterial.MatcapTexture);
        }

        /// <inheritdoc/>
        internal override (int MaxTextureSize, TextureFormat Format)? GetPackedMaskPlatformOverride(TexturePack pack)
        {
            var textures = new List<Texture>();
            foreach (var mask in pack.GetMasks())
            {
                switch (mask.MaskType)
                {
                    case MaskType.MetallicMap:
                        if (GetSpecularMetallicTexture())
                        {
                            textures.Add(GetSpecularMetallicTexture());
                        }

                        break;
                    case MaskType.GlossMap:
                        if (GetSpecularGlossTexture())
                        {
                            textures.Add(GetSpecularGlossTexture());
                        }

                        break;
                    case MaskType.MatcapMask:
                        if (poiyomiMaterial.MatcapMask)
                        {
                            textures.Add(poiyomiMaterial.MatcapMask);
                        }

                        break;
                    case MaskType.OcculusionMap:
                        if (poiyomiMaterial.AOMap)
                        {
                            textures.Add(poiyomiMaterial.AOMap);
                        }

                        break;
                }
            }

            return textures.Count > 0
                ? TextureUtility.GetBestPlatformOverrideSettings(textures.ToArray())
                : null;
        }

        /// <inheritdoc/>
        internal override AsyncCallbackRequest GenerateGlossMap(Action<Texture2D> completion)
        {
            var packed = (Texture2D)GetSpecularGlossTexture();
            var platformOverride = TextureUtility.GetBestPlatformOverrideSettings(packed);
            var maxTextureSize = platformOverride?.MaxTextureSize ?? (int)Settings.maxTextureSize;
            var (width, height) = TextureUtility.AspectFitReduction(packed.width, packed.height, maxTextureSize);

            var swizzleMat = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));
            swizzleMat.SetTexture("_Texture0", packed);
            swizzleMat.SetFloat("_Texture0Input", GetSpecularGlossChannel());
            swizzleMat.SetFloat("_Texture0Output", 3); // A

            var rt = RenderTexture.GetTemporary(packed.width, packed.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
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
        internal override AsyncCallbackRequest GenerateMatcap(Action<Texture2D> completion)
        {
            var mat = new Material(Shader.Find("Hidden/VRCQuestTools/Multiply"));
            mat.SetTexture("_Texture0", poiyomiMaterial.MatcapTexture);

            var color = Utils.ColorUtility.HdrToLdr(poiyomiMaterial.MatcapColor);
            if (poiyomiMaterial.MatcapAdd > 0.0f || poiyomiMaterial.MatcapScreen > 0.0f || poiyomiMaterial.MatcapMultiply <= 0.0f)
            {
                var attenuation = 0.8f;
                var blend = Mathf.Clamp01(Mathf.Max(poiyomiMaterial.MatcapReplace, Mathf.Max(poiyomiMaterial.MatcapAdd, poiyomiMaterial.MatcapScreen)));
                color.r *= Mathf.Lerp(color.r, 0.0f, blend * attenuation);
                color.g *= Mathf.Lerp(color.g, 0.0f, blend * attenuation);
                color.b *= Mathf.Lerp(color.b, 0.0f, blend * attenuation);
            }
            else
            {
                var blend = Mathf.Clamp01(poiyomiMaterial.MatcapMultiply);
                color.r *= Mathf.Lerp(color.r, 1.0f, blend);
                color.g *= Mathf.Lerp(color.g, 1.0f, blend);
                color.b *= Mathf.Lerp(color.b, 1.0f, blend);
            }

            color.a = 1.0f;
            mat.SetColor("_Texture0Color", color);

            var matcapWidth = poiyomiMaterial.MatcapTexture ? poiyomiMaterial.MatcapTexture.width : 4;
            var matcapHeight = poiyomiMaterial.MatcapTexture ? poiyomiMaterial.MatcapTexture.height : 4;
            var platformOverride = TextureUtility.GetBestPlatformOverrideSettings(poiyomiMaterial.MatcapTexture);
            var matcapLimit = TextureUtility.NormalizeMaxTextureSize((int)Settings.matcapMaxTextureSize) ?? (int)Settings.maxTextureSize;
            var maxTextureSize = platformOverride?.MaxTextureSize ?? matcapLimit;
            var (width, height) = TextureUtility.AspectFitReduction(matcapWidth, matcapHeight, maxTextureSize);

            var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(null, rt, mat);

            return TextureUtility.RequestReadbackRenderTexture(rt, true, false, (tex) =>
            {
                UnityEngine.Object.DestroyImmediate(mat);
                RenderTexture.ReleaseTemporary(rt);
                completion?.Invoke(tex);
            });
        }

        /// <inheritdoc/>
        internal override AsyncCallbackRequest GenerateMatcapMask(Action<Texture2D> completion)
        {
            var matcapMask = (Texture2D)poiyomiMaterial.MatcapMask;
            var platformOverride = TextureUtility.GetBestPlatformOverrideSettings(matcapMask);
            var maxTextureSize = platformOverride?.MaxTextureSize ?? (int)Settings.maxTextureSize;
            var (width, height) = TextureUtility.AspectFitReduction(matcapMask.width, matcapMask.height, maxTextureSize);

            var rt = RenderTexture.GetTemporary(matcapMask.width, matcapMask.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var swizzleMat = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));
            swizzleMat.SetTexture("_Texture0", matcapMask);
            swizzleMat.SetFloat("_Texture0Input", poiyomiMaterial.MatcapMaskChannel);
            swizzleMat.SetFloat("_Texture0Output", 0); // R
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
        internal override AsyncCallbackRequest GenerateMetallicMap(Action<Texture2D> completion)
        {
            var packed = (Texture2D)GetSpecularMetallicTexture();
            var platformOverride = TextureUtility.GetBestPlatformOverrideSettings(packed);
            var maxTextureSize = platformOverride?.MaxTextureSize ?? (int)Settings.maxTextureSize;
            var (width, height) = TextureUtility.AspectFitReduction(packed.width, packed.height, maxTextureSize);

            var swizzleMat = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));
            swizzleMat.SetTexture("_Texture0", packed);
            swizzleMat.SetFloat("_Texture0Input", GetSpecularMetallicChannel());
            swizzleMat.SetFloat("_Texture0Output", 0); // R

            var rt = RenderTexture.GetTemporary(packed.width, packed.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
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
        internal override AsyncCallbackRequest GeneratePackedMask(TexturePack pack, Action<Texture2D> completion)
        {
            var swizzleMat = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));
            int maxWidth = 0;
            int maxHeight = 0;
            var sourceTextures = new List<Texture>();

            foreach (var (mask, index) in pack.GetMasks().Select((mask, index) => (mask, index)))
            {
                switch (mask.MaskType)
                {
                    case MaskType.None:
                    case MaskType.DetailMask:
                        break;
                    case MaskType.OcculusionMap:
                        if (poiyomiMaterial.AOMap)
                        {
                            sourceTextures.Add(poiyomiMaterial.AOMap);
                        }

                        ((IOcclusionSource)poiyomiMaterial).GenerateOcclusionMap(Settings.maxTextureSize, (tex) =>
                        {
                            if (tex)
                            {
                                swizzleMat.SetTexture($"_Texture{index}", tex);
                                swizzleMat.SetFloat($"_Texture{index}Input", 1); // G
                                swizzleMat.SetFloat($"_Texture{index}Output", (int)mask.Channel);
                                maxHeight = Math.Max(maxHeight, tex.height);
                                maxWidth = Math.Max(maxWidth, tex.width);
                            }
                        }).WaitForCompletion();
                        break;
                    case MaskType.MetallicMap:
                        if (GetSpecularMetallicTexture())
                        {
                            sourceTextures.Add(GetSpecularMetallicTexture());
                        }

                        GenerateMetallicMap((tex) =>
                        {
                            if (tex)
                            {
                                swizzleMat.SetTexture($"_Texture{index}", tex);
                                swizzleMat.SetFloat($"_Texture{index}Input", 0);
                                swizzleMat.SetFloat($"_Texture{index}Output", (int)mask.Channel);
                                maxHeight = Math.Max(maxHeight, tex.height);
                                maxWidth = Math.Max(maxWidth, tex.width);
                            }
                        }).WaitForCompletion();
                        break;
                    case MaskType.MatcapMask:
                        if (poiyomiMaterial.MatcapMask)
                        {
                            sourceTextures.Add(poiyomiMaterial.MatcapMask);
                        }

                        GenerateMatcapMask((tex) =>
                        {
                            if (tex)
                            {
                                swizzleMat.SetTexture($"_Texture{index}", tex);
                                swizzleMat.SetFloat($"_Texture{index}Input", 0);
                                swizzleMat.SetFloat($"_Texture{index}Output", (int)mask.Channel);
                                maxHeight = Math.Max(maxHeight, tex.height);
                                maxWidth = Math.Max(maxWidth, tex.width);
                            }
                        }).WaitForCompletion();
                        break;
                    case MaskType.GlossMap:
                        if (GetSpecularGlossTexture())
                        {
                            sourceTextures.Add(GetSpecularGlossTexture());
                        }

                        GenerateGlossMap((tex) =>
                        {
                            if (tex)
                            {
                                swizzleMat.SetTexture($"_Texture{index}", tex);
                                swizzleMat.SetFloat($"_Texture{index}Input", 3);
                                swizzleMat.SetFloat($"_Texture{index}Output", (int)mask.Channel);
                                maxHeight = Math.Max(maxHeight, tex.height);
                                maxWidth = Math.Max(maxWidth, tex.width);
                            }
                        }).WaitForCompletion();
                        break;
                }
            }

            var platformOverride = TextureUtility.GetBestPlatformOverrideSettings(sourceTextures.ToArray());
            var sourceMaxSize = TextureUtility.NormalizeMaxTextureSize(platformOverride?.MaxTextureSize);
            var maskMaxSize = TextureUtility.NormalizeMaxTextureSize((int)Settings.maskMaxTextureSize);
            var mainMaxSize = TextureUtility.NormalizeMaxTextureSize((int)Settings.maxTextureSize);
            var settingsMaxSize = maskMaxSize ?? mainMaxSize;
            var maxTextureSize = TextureUtility.MinDefinedMaxTextureSize(sourceMaxSize, settingsMaxSize);

            var width = Math.Max(1, maxWidth);
            var height = Math.Max(1, maxHeight);
            if (maxTextureSize.HasValue)
            {
                width = Math.Min(width, maxTextureSize.Value);
                height = Math.Min(height, maxTextureSize.Value);
            }

            var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(null, rt, swizzleMat);
            return TextureUtility.RequestReadbackRenderTexture(rt, true, true, (tex) =>
            {
                RenderTexture.ReleaseTemporary(rt);
                UnityEngine.Object.DestroyImmediate(swizzleMat);
                completion?.Invoke(tex);
            });
        }

        /// <inheritdoc/>
        internal override AsyncCallbackRequest GenerateShadowRamp(Action<Texture2D> completion)
        {
            var shader = Shader.Find("Hidden/VRCQuestTools/Poiyomi/ShadowRamp");
            if (shader == null)
            {
                throw new InvalidOperationException("Shader not found: Hidden/VRCQuestTools/Poiyomi/ShadowRamp");
            }

            var bakeMat = new Material(poiyomiMaterial.Material);
            bakeMat.parent = null;
            bakeMat.shader = shader;

            var width = 128;
            var height = 16;
            var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(null, rt, bakeMat);

            return TextureUtility.RequestReadbackRenderTexture(rt, true, false, (tex) =>
            {
                tex.wrapMode = TextureWrapMode.Clamp;
                RenderTexture.ReleaseTemporary(rt);
                UnityEngine.Object.DestroyImmediate(bakeMat);
                completion?.Invoke(tex);
            });
        }

        /// <inheritdoc/>
        internal override CullMode GetCulling()
        {
            return poiyomiMaterial.CullMode;
        }

        /// <inheritdoc/>
        internal override (Vector2 Scale, Vector2 Offset) GetGlossMapST()
        {
            return GetSpecularGlossMapST();
        }

        /// <inheritdoc/>
        internal override float GetGlossStrength()
        {
            switch (GetSpecularSource())
            {
                case SpecularSource.Mochie:
                    return poiyomiMaterial.RoughnessMultiplier;
                case SpecularSource.StylizedLilToon:
                    return poiyomiMaterial.StylizedSmoothness;
                case SpecularSource.StylizedUnityChan:
                    // UnityChan toon specular has no PBR smoothness; approximate from highlight size
                    // (larger highlight ≈ rougher ≈ lower gloss).
                    return Mathf.Clamp01(1.0f - poiyomiMaterial.StylizedSpecularSize);
                default:
                    return 1.0f;
            }
        }

        /// <inheritdoc/>
        internal override Color GetMainColor()
        {
            if (Settings.useMatcap &&
                poiyomiMaterial.UseMatcap &&
                poiyomiMaterial.MatcapReplace > 0.0f &&
                poiyomiMaterial.MatcapAdd <= 0.0f &&
                poiyomiMaterial.MatcapScreen <= 0.0f &&
                poiyomiMaterial.MatcapMultiply <= 0.0f &&
                !((IAlbedoSource)poiyomiMaterial).RequiresAlbedoTexture(ToonStandardGenerator.CreateAlbedoBakeRequest(Settings)))
            {
                return Color.black;
            }

            return poiyomiMaterial.Material.color;
        }

        /// <inheritdoc/>
        internal override ToonStandardMaterialWrapper.MatcapTypeMode GetMapcapType()
        {
            if (poiyomiMaterial.MatcapAdd > 0.0f || poiyomiMaterial.MatcapScreen > 0.0f)
            {
                return ToonStandardMaterialWrapper.MatcapTypeMode.Additive;
            }

            return poiyomiMaterial.MatcapMultiply > 0.0f
                ? ToonStandardMaterialWrapper.MatcapTypeMode.Multiplicative
                : ToonStandardMaterialWrapper.MatcapTypeMode.Additive;
        }

        /// <inheritdoc/>
        internal override (Vector2 Scale, Vector2 Offset) GetMatcapMaskST()
        {
            return (poiyomiMaterial.MatcapMaskTextureScale, poiyomiMaterial.MatcapMaskTextureOffset);
        }

        /// <inheritdoc/>
        internal override float GetMatcapMaskStrength()
        {
            return poiyomiMaterial.MatcapIntensity / 5.0f;
        }

        /// <inheritdoc/>
        internal override (Vector2 Scale, Vector2 Offset) GetMetallicMapST()
        {
            return GetSpecularMetallicMapST();
        }

        /// <inheritdoc/>
        internal override float GetMetallicStrength()
        {
            switch (GetSpecularSource())
            {
                case SpecularSource.Mochie:
                    return poiyomiMaterial.MetallicMultiplier;
                case SpecularSource.StylizedLilToon:
                    return poiyomiMaterial.StylizedMetallic;
                default:
                    // UnityChan toon specular has no metallic.
                    return 0.0f;
            }
        }

        /// <inheritdoc/>
        internal override float GetMinBrightness()
        {
            // Poiyomi _LightingMinLightBrightness is Range(0,1), but ToonStandard _MinBrightness
            // is Range(0, 0.1). Clamp to ToonStandard's upper bound.
            return Mathf.Min(((IMinimumBrightnessSource)poiyomiMaterial).MinimumBrightness, 0.1f);
        }

        /// <inheritdoc/>
        internal override (Vector2 Scale, Vector2 Offset) GetNormalMapST()
        {
            return (poiyomiMaterial.NormalMapTextureScale, poiyomiMaterial.NormalMapTextureOffset);
        }

        /// <inheritdoc/>
        internal override (Vector2 Scale, Vector2 Offset) GetOcculusionMapST()
        {
            return (poiyomiMaterial.AOMapTextureScale, poiyomiMaterial.AOMapTextureOffset);
        }

        /// <inheritdoc/>
        internal override float GetReflectance()
        {
            switch (GetSpecularSource())
            {
                case SpecularSource.StylizedLilToon:
                    return poiyomiMaterial.StylizedReflectance;
                case SpecularSource.StylizedUnityChan:
                    return Mathf.Clamp01(poiyomiMaterial.StylizedSpecularStrength);
                default:
                    return 0.5f;
            }
        }

        /// <inheritdoc/>
        internal override float GetRimAlbedoTint()
        {
            // Style 2 (LilToon) has explicit albedo tint; other styles have none.
            return poiyomiMaterial.RimStyle == 2 ? poiyomiMaterial.RimMainStrength : 0.0f;
        }

        /// <inheritdoc/>
        internal override Color GetRimColor()
        {
            Color color;
            if (poiyomiMaterial.RimStyle == 2)
            {
                // Style 2 (LilToon) uses separate _RimColor property (HDR).
                color = Utils.ColorUtility.HdrToLdr(poiyomiMaterial.RimColorLilToon);
            }
            else
            {
                // Styles 0 and 1 share _RimLightColor.
                color = poiyomiMaterial.RimLightColor;
                color.a = 1.0f;
            }

            return color;
        }

        /// <inheritdoc/>
        internal override bool GetRimEnvironmental()
        {
            switch (poiyomiMaterial.RimStyle)
            {
                case 1:
                    // UTS2 style: _Is_LightColor_RimLight controls scene light mixing.
                    return poiyomiMaterial.RimLightingEnvironmental > 0.5f;
                case 2:
                    // LilToon style: _RimEnableLighting controls scene light mixing.
                    return poiyomiMaterial.RimEnableLighting > 0.0f;
                default:
                    // Poiyomi style 0: emission-based, not affected by scene lighting.
                    return false;
            }
        }

        /// <inheritdoc/>
        internal override float GetRimIntensity()
        {
            switch (poiyomiMaterial.RimStyle)
            {
                case 1:
                {
                    // UTS2 style: environmental rim with moderate intensity.
                    var intensity = GetRimEnvironmental() ? 0.5f * poiyomiMaterial.RimLightingEnvironmental : 0.5f;
                    return intensity;
                }

                case 2:
                {
                    // LilToon style: mirrors LilToon mapping, modulated by color alpha.
                    var ldrColor = Utils.ColorUtility.HdrToLdr(poiyomiMaterial.RimColorLilToon);
                    var intensity = GetRimEnvironmental() ? 0.5f * poiyomiMaterial.RimEnableLighting : 0.5f;
                    return intensity * ldrColor.a;
                }

                default:
                    // Poiyomi style 0: map 0-20 emission strength to 0-1.
                    return Mathf.Clamp01(poiyomiMaterial.RimStrength / 20.0f);
            }
        }

        /// <inheritdoc/>
        internal override float GetRimRange()
        {
            switch (poiyomiMaterial.RimStyle)
            {
                case 1:
                    // UTS2 style: _RimLight_Power controls rim width (0-1, smaller = narrower).
                    return poiyomiMaterial.RimLightPower;
                case 2:
                    // LilToon style: Fresnel-based range (mirrors LilToon mapping).
                    return Mathf.Pow(1.0f - poiyomiMaterial.RimBorder, poiyomiMaterial.RimFresnelPower);
                default:
                    // Poiyomi style 0: direct width mapping.
                    return poiyomiMaterial.RimWidth;
            }
        }

        /// <inheritdoc/>
        internal override float GetRimSoftness()
        {
            switch (poiyomiMaterial.RimStyle)
            {
                case 1:
                    // UTS2 style: feather off = hard edge (0), feather on = moderate soft (0.3).
                    return poiyomiMaterial.RimLightFeatherOff ? 0.0f : 0.3f;
                case 2:
                    // LilToon style: _RimBlur maps directly to softness.
                    return poiyomiMaterial.RimBlur;
                default:
                    // Poiyomi style 0: invert sharpness → softness.
                    return 1.0f - poiyomiMaterial.RimSharpness;
            }
        }

        /// <inheritdoc/>
        internal override float GetSharpness()
        {
            switch (GetSpecularSource())
            {
                case SpecularSource.StylizedLilToon:
                    return Mathf.Clamp01(1.0f - poiyomiMaterial.StylizedSpecularBlur);
                case SpecularSource.StylizedUnityChan:
                    return Mathf.Clamp01(1.0f - poiyomiMaterial.StylizedSpecularFeather);
                default:
                    return 0.5f;
            }
        }

        /// <inheritdoc/>
        internal override bool GetUseGlossMap()
        {
            return GetUseSpecular() && GetSpecularGlossTexture() != null;
        }

        /// <inheritdoc/>
        internal override bool GetUseMatcap()
        {
            return poiyomiMaterial.UseMatcap;
        }

        /// <inheritdoc/>
        internal override bool GetUseMatcapMask()
        {
            return poiyomiMaterial.UseMatcap && poiyomiMaterial.MatcapMask != null;
        }

        /// <inheritdoc/>
        internal override bool GetUseMetallicMap()
        {
            return GetUseSpecular() && GetSpecularMetallicTexture() != null;
        }

        /// <inheritdoc/>
        internal override bool GetUseRimLighting()
        {
            return poiyomiMaterial.UseRimLighting;
        }

        /// <inheritdoc/>
        internal override bool GetUseShadowRamp()
        {
            return poiyomiMaterial.UseShadow;
        }

        /// <inheritdoc/>
        internal override bool GetUseSpecular()
        {
            return poiyomiMaterial.UseSpecular || poiyomiMaterial.UseStylizedReflections;
        }

        /// <summary>
        /// Resolves which Poiyomi feature provides the specular/reflection inputs.
        /// Stylized Reflections (_StylizedSpecular) take priority over the Mochie BRDF (_MochieBRDF).
        /// </summary>
        /// <returns>The active specular source.</returns>
        private SpecularSource GetSpecularSource()
        {
            if (poiyomiMaterial.UseStylizedReflections)
            {
                return poiyomiMaterial.StylizedReflectionMode == 1
                    ? SpecularSource.StylizedLilToon
                    : SpecularSource.StylizedUnityChan;
            }

            if (poiyomiMaterial.UseSpecular)
            {
                return SpecularSource.Mochie;
            }

            return SpecularSource.None;
        }

        private Texture GetSpecularMetallicTexture()
        {
            switch (GetSpecularSource())
            {
                case SpecularSource.Mochie:
                    return poiyomiMaterial.MetallicMaps;
                case SpecularSource.StylizedLilToon:
                    return poiyomiMaterial.StylizedMetallicMap;
                default:
                    // UnityChan toon specular has no metallic map.
                    return null;
            }
        }

        private float GetSpecularMetallicChannel()
        {
            switch (GetSpecularSource())
            {
                case SpecularSource.Mochie:
                    return poiyomiMaterial.MetallicMapsMetallicChannel;
                default:
                    // lilToon _MetallicGlossMap stores metallic in R.
                    return 0;
            }
        }

        private (Vector2 Scale, Vector2 Offset) GetSpecularMetallicMapST()
        {
            switch (GetSpecularSource())
            {
                case SpecularSource.Mochie:
                    return (poiyomiMaterial.MetallicMapsTextureScale, poiyomiMaterial.MetallicMapsTextureOffset);
                case SpecularSource.StylizedLilToon:
                    return (poiyomiMaterial.StylizedMetallicMapTextureScale, poiyomiMaterial.StylizedMetallicMapTextureOffset);
                default:
                    return (Vector2.one, Vector2.zero);
            }
        }

        private Texture GetSpecularGlossTexture()
        {
            switch (GetSpecularSource())
            {
                case SpecularSource.Mochie:
                    return poiyomiMaterial.MetallicMaps;
                case SpecularSource.StylizedLilToon:
                    return poiyomiMaterial.StylizedSmoothnessTex;
                case SpecularSource.StylizedUnityChan:
                    // UnityChan toon specular has no PBR gloss map; approximate using the specular mask.
                    return poiyomiMaterial.StylizedHighColorMask;
                default:
                    return null;
            }
        }

        private float GetSpecularGlossChannel()
        {
            switch (GetSpecularSource())
            {
                case SpecularSource.Mochie:
                    return poiyomiMaterial.MetallicMapsRoughnessChannel;
                case SpecularSource.StylizedUnityChan:
                    return poiyomiMaterial.StylizedHighColorMaskChannel;
                default:
                    // lilToon _SmoothnessTex stores smoothness in R.
                    return 0;
            }
        }

        private (Vector2 Scale, Vector2 Offset) GetSpecularGlossMapST()
        {
            switch (GetSpecularSource())
            {
                case SpecularSource.Mochie:
                    return (poiyomiMaterial.MetallicMapsTextureScale, poiyomiMaterial.MetallicMapsTextureOffset);
                case SpecularSource.StylizedLilToon:
                    return (poiyomiMaterial.StylizedSmoothnessTexTextureScale, poiyomiMaterial.StylizedSmoothnessTexTextureOffset);
                case SpecularSource.StylizedUnityChan:
                    return (poiyomiMaterial.StylizedHighColorMaskTextureScale, poiyomiMaterial.StylizedHighColorMaskTextureOffset);
                default:
                    return (Vector2.one, Vector2.zero);
            }
        }

        private static ToonStandardMaterialWrapper.MaskChannel ChannelIndexToMaskChannel(float channelIndex)
        {
            switch ((int)channelIndex)
            {
                case 1: return ToonStandardMaterialWrapper.MaskChannel.G;
                case 2: return ToonStandardMaterialWrapper.MaskChannel.B;
                case 3: return ToonStandardMaterialWrapper.MaskChannel.A;
                default: return ToonStandardMaterialWrapper.MaskChannel.R;
            }
        }

        private int GetPrimaryEmissionChannel()
        {
            foreach (var idx in poiyomiMaterial.GetEnabledEmissionChannels())
            {
                if (poiyomiMaterial.GetEmissionMapByIndex(idx) != null)
                {
                    return idx;
                }
            }

            for (int idx = 0; idx <= 3; idx++)
            {
                if (poiyomiMaterial.GetEmissionMapByIndex(idx) != null)
                {
                    return idx;
                }
            }

            return 0;
        }

        private Vector2 GetEmissionMapTextureScaleByIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return poiyomiMaterial.EmissionMapTextureScale0;
                case 1:
                    return poiyomiMaterial.EmissionMapTextureScale1;
                case 2:
                    return poiyomiMaterial.EmissionMapTextureScale2;
                case 3:
                    return poiyomiMaterial.EmissionMapTextureScale3;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        private Vector2 GetEmissionMapTextureOffsetByIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return poiyomiMaterial.EmissionMapTextureOffset0;
                case 1:
                    return poiyomiMaterial.EmissionMapTextureOffset1;
                case 2:
                    return poiyomiMaterial.EmissionMapTextureOffset2;
                case 3:
                    return poiyomiMaterial.EmissionMapTextureOffset3;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }
}
