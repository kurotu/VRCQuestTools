// <copyright file="PoiyomiToonStandardGenerator.cs" company="kurotu">
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

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Material generator for Poiyomi to ToonStandard conversion.
    /// </summary>
    internal class PoiyomiToonStandardGenerator : ToonStandardGenerator
    {
        private readonly PoiyomiMaterial poiyomiMaterial;

        /// <summary>
        /// Initializes a new instance of the <see cref="PoiyomiToonStandardGenerator"/> class.
        /// </summary>
        /// <param name="material">Poiyomi material.</param>
        /// <param name="settings">Convert settings.</param>
        /// <param name="sharedBlackTexture">Shared black texture to disable emission.</param>
        /// <param name="forEditorPreview">Whether the conversion is for the NDMF editor preview.</param>
        public PoiyomiToonStandardGenerator(PoiyomiMaterial material, ToonStandardConvertSettings settings, Texture2D sharedBlackTexture, bool forEditorPreview)
            : base(settings, sharedBlackTexture, forEditorPreview)
        {
            this.poiyomiMaterial = material;
        }

        /// <inheritdoc/>
        protected override Material ConvertToToonStandard()
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

            if (GetUseNormalMap() && Settings.useNormalMap)
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

            if (GetUseEmission() && Settings.useEmission)
            {
                if (GetUseEmissionMap())
                {
                    newMaterial.EmissionMap = GetPrimaryEmissionMap();
                    newMaterial.EmissionMapTextureScale = poiyomiMaterial.EmissionMapTextureScale0;
                    newMaterial.EmissionMapTextureOffset = poiyomiMaterial.EmissionMapTextureOffset0;
                }

                newMaterial.EmissionColor = Utils.ColorUtility.HdrToLdr(GetEmissionColor());
            }
            else
            {
                newMaterial.EmissionMap = sharedBlackTexture;
                newMaterial.EmissionColor = Color.black;
            }

            // NOT SUPPORTED: _Decal / _Decal1 / _Decal2 / _Decal3 - ToonStandard has no decal system.
            // NOT SUPPORTED: _EnableOutlines - Outline is a separate shader (ToonStandardOutline) and
            //   cannot be merged into this material conversion.
            // NOT SUPPORTED: _Mode (transparency/fade/cutout) - ToonStandard is Opaque-only.
            //   Non-opaque blend modes (Fade, Transparent, Additive, etc.) have no equivalent.
            // NOT SUPPORTED: Color grading (_Saturation, _MainColorAdjustTexture R/G/B mask, _Value) -
            //   ToonStandard has no color grading post-processing.

            if (GetUseOcclusionMap() && Settings.useOcclusion)
            {
                var (aoChannel, aoStrength) = GetPrimaryAOChannelAndStrength();
                newMaterial.UseOcclusion = true;
                newMaterial.OcclusionMap = poiyomiMaterial.AOMap;
                newMaterial.OcclusionMapTextureScale = poiyomiMaterial.AOMapTextureScale;
                newMaterial.OcclusionMapTextureOffset = poiyomiMaterial.AOMapTextureOffset;
                newMaterial.OcclusionMapChannel = ChannelIndexToMaskChannel(aoChannel);
                newMaterial.OcclusionStrength = aoStrength;
            }

            if (GetUseSpecular() && Settings.useSpecular)
            {
                newMaterial.UseSpecular = true;
                newMaterial.MetallicMap = poiyomiMaterial.MetallicMaps;
                newMaterial.MetallicMapTextureScale = poiyomiMaterial.MetallicMapsTextureScale;
                newMaterial.MetallicMapTextureOffset = poiyomiMaterial.MetallicMapsTextureOffset;
                newMaterial.MetallicMapChannel = ChannelIndexToMaskChannel(poiyomiMaterial.MetallicMapsMetallicChannel);
                newMaterial.MetallicStrength = poiyomiMaterial.MetallicMultiplier;

                newMaterial.GlossMap = poiyomiMaterial.MetallicMaps;
                newMaterial.GlossMapTextureScale = poiyomiMaterial.MetallicMapsTextureScale;
                newMaterial.GlossMapTextureOffset = poiyomiMaterial.MetallicMapsTextureOffset;
                newMaterial.GlossMapChannel = ChannelIndexToMaskChannel(poiyomiMaterial.MetallicMapsRoughnessChannel);
                newMaterial.GlossStrength = poiyomiMaterial.RoughnessMultiplier;
                newMaterial.Sharpness = 0.5f;
                newMaterial.Reflectance = 0.5f;
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

                // NOT SUPPORTED: 2nd MatCap (_Matcap2Enable, _Matcap2, _Matcap2Color, etc.)
                // ToonStandard provides only one MatCap slot. The second MatCap layer is lost.
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

                // NOT SUPPORTED: 2nd Rim Lighting (_EnableRim2Lighting) - ToonStandard has only
                // one rim slot. The second rim layer is lost.
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
        protected override (int MaxTextureSize, TextureFormat Format)? GetMainTexturePlatformOverride()
        {
            return TextureUtility.GetBestPlatformOverrideSettings(poiyomiMaterial.Material.mainTexture);
        }

        /// <inheritdoc/>
        protected override (int MaxTextureSize, TextureFormat Format)? GetNormalMapPlatformOverride()
        {
            return TextureUtility.GetBestPlatformOverrideSettings(poiyomiMaterial.NormalMap);
        }

        /// <inheritdoc/>
        protected override (int MaxTextureSize, TextureFormat Format)? GetEmissionMapPlatformOverride()
        {
            return TextureUtility.GetBestPlatformOverrideSettings(
                poiyomiMaterial.EmissionMap0,
                poiyomiMaterial.EmissionMask0,
                poiyomiMaterial.EmissionMap1,
                poiyomiMaterial.EmissionMask1,
                poiyomiMaterial.EmissionMap2,
                poiyomiMaterial.EmissionMask2,
                poiyomiMaterial.EmissionMap3,
                poiyomiMaterial.EmissionMask3);
        }

        /// <inheritdoc/>
        protected override (int MaxTextureSize, TextureFormat Format)? GetMatcapPlatformOverride()
        {
            return TextureUtility.GetBestPlatformOverrideSettings(poiyomiMaterial.MatcapTexture);
        }

        /// <inheritdoc/>
        protected override (int MaxTextureSize, TextureFormat Format)? GetMatcapMaskPlatformOverride()
        {
            return TextureUtility.GetBestPlatformOverrideSettings(poiyomiMaterial.MatcapMask);
        }

        /// <inheritdoc/>
        protected override (int MaxTextureSize, TextureFormat Format)? GetMetallicMapPlatformOverride()
        {
            return TextureUtility.GetBestPlatformOverrideSettings(poiyomiMaterial.MetallicMaps);
        }

        /// <inheritdoc/>
        protected override (int MaxTextureSize, TextureFormat Format)? GetGlossMapPlatformOverride()
        {
            return TextureUtility.GetBestPlatformOverrideSettings(poiyomiMaterial.MetallicMaps);
        }

        /// <inheritdoc/>
        protected override (int MaxTextureSize, TextureFormat Format)? GetOcclusionMapPlatformOverride()
        {
            return TextureUtility.GetBestPlatformOverrideSettings(poiyomiMaterial.AOMap);
        }

        /// <inheritdoc/>
        protected override (int MaxTextureSize, TextureFormat Format)? GetPackedMaskPlatformOverride(TexturePack pack)
        {
            var textures = new List<Texture>();
            foreach (var mask in pack.GetMasks())
            {
                switch (mask.MaskType)
                {
                    case MaskType.MetallicMap:
                    case MaskType.GlossMap:
                        if (poiyomiMaterial.MetallicMaps)
                        {
                            textures.Add(poiyomiMaterial.MetallicMaps);
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
        protected override AsyncCallbackRequest GenerateEmissionMap(Action<Texture2D> completion)
        {
            Texture2D main = null;
            GenerateMainTexture((tex) =>
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
            var platformOverride = GetEmissionMapPlatformOverride();
            var maxTextureSize = platformOverride?.MaxTextureSize ?? (int)Settings.maxTextureSize;
            var (targetWidth, targetHeight) = TextureUtility.AspectFitReduction(sourceWidth, sourceHeight, maxTextureSize);

            var rt = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(null, rt, bakeMat);
            return TextureUtility.RequestReadbackRenderTexture(rt, true, false, (tex) =>
            {
                RenderTexture.ReleaseTemporary(rt);
                UnityEngine.Object.DestroyImmediate(bakeMat);
                completion?.Invoke(tex);
            });
        }

        /// <inheritdoc/>
        protected override AsyncCallbackRequest GenerateGlossMap(Action<Texture2D> completion)
        {
            var packed = (Texture2D)poiyomiMaterial.MetallicMaps;
            var platformOverride = TextureUtility.GetBestPlatformOverrideSettings(packed);
            var maxTextureSize = platformOverride?.MaxTextureSize ?? (int)Settings.maxTextureSize;
            var (width, height) = TextureUtility.AspectFitReduction(packed.width, packed.height, maxTextureSize);

            var swizzleMat = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));
            swizzleMat.SetTexture("_Texture0", packed);
            swizzleMat.SetFloat("_Texture0Input", poiyomiMaterial.MetallicMapsRoughnessChannel);
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
        protected override AsyncCallbackRequest GenerateMainTexture(Action<Texture2D> completion)
        {
            var toonLitConvertSettings = new ToonLitConvertSettings
            {
                generateQuestTextures = Settings.generateQuestTextures,
                maxTextureSize = Settings.maxTextureSize,
                mobileTextureFormat = Settings.mobileTextureFormat,
                mainTextureBrightness = 1.0f,
                generateShadowFromNormalMap = !Settings.useNormalMap,
            };

            var toonLitBakeMat = new PoiyomiMaterial(new Material(poiyomiMaterial.Material));
            if (Settings.useEmission)
            {
                toonLitBakeMat.EnableEmission0 = false;
                toonLitBakeMat.EnableEmission1 = false;
                toonLitBakeMat.EnableEmission2 = false;
                toonLitBakeMat.EnableEmission3 = false;
            }

            if (Settings.useMatcap && toonLitBakeMat.Material.HasProperty("_MatcapEnable"))
            {
                toonLitBakeMat.Material.SetFloat("_MatcapEnable", 0.0f);
            }

            return toonLitBakeMat.GenerateToonLitImage(toonLitConvertSettings, completion);
        }

        /// <inheritdoc/>
        protected override AsyncCallbackRequest GenerateMatcap(Action<Texture2D> completion)
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
            var maxTextureSize = platformOverride?.MaxTextureSize ?? (int)Settings.maxTextureSize;
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
        protected override AsyncCallbackRequest GenerateMatcapMask(Action<Texture2D> completion)
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
        protected override AsyncCallbackRequest GenerateMetallicMap(Action<Texture2D> completion)
        {
            var packed = (Texture2D)poiyomiMaterial.MetallicMaps;
            var platformOverride = TextureUtility.GetBestPlatformOverrideSettings(packed);
            var maxTextureSize = platformOverride?.MaxTextureSize ?? (int)Settings.maxTextureSize;
            var (width, height) = TextureUtility.AspectFitReduction(packed.width, packed.height, maxTextureSize);

            var swizzleMat = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));
            swizzleMat.SetTexture("_Texture0", packed);
            swizzleMat.SetFloat("_Texture0Input", poiyomiMaterial.MetallicMapsMetallicChannel);
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
        protected override AsyncCallbackRequest GenerateNormalMap(bool outputRGB, Action<Texture2D> completion)
        {
            var normal = (Texture2D)poiyomiMaterial.NormalMap;
            var platformOverride = TextureUtility.GetBestPlatformOverrideSettings(normal);
            var maxTextureSize = platformOverride?.MaxTextureSize ?? (int)Settings.maxTextureSize;
            var (width, height) = TextureUtility.AspectFitReduction(normal.width, normal.height, maxTextureSize);
            var newTex = TextureUtility.DownscaleNormalMap(normal, outputRGB, width, height);
            return new ResultRequest<Texture2D>(newTex, completion);
        }

        /// <inheritdoc/>
        protected override AsyncCallbackRequest GenerateOcclusionMap(Action<Texture2D> completion)
        {
            var aoMap = (Texture2D)poiyomiMaterial.AOMap;
            var (channel, _) = GetPrimaryAOChannelAndStrength();

            var platformOverride = TextureUtility.GetBestPlatformOverrideSettings(aoMap);
            var maxTextureSize = platformOverride?.MaxTextureSize ?? (int)Settings.maxTextureSize;
            var (width, height) = TextureUtility.AspectFitReduction(aoMap.width, aoMap.height, maxTextureSize);

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
        protected override AsyncCallbackRequest GeneratePackedMask(TexturePack pack, Action<Texture2D> completion)
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

                        GenerateOcclusionMap((tex) =>
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
                        if (poiyomiMaterial.MetallicMaps)
                        {
                            sourceTextures.Add(poiyomiMaterial.MetallicMaps);
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
                        if (poiyomiMaterial.MetallicMaps)
                        {
                            sourceTextures.Add(poiyomiMaterial.MetallicMaps);
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
        protected override AsyncCallbackRequest GenerateShadowRamp(Action<Texture2D> completion)
        {
            var bakeMat = new Material(poiyomiMaterial.Material);
            bakeMat.parent = null;
            bakeMat.shader = Shader.Find("Hidden/VRCQuestTools/Poiyomi/ShadowRamp");

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
        protected override CullMode GetCulling()
        {
            return poiyomiMaterial.CullMode;
        }

        /// <inheritdoc/>
        protected override Color GetEmissionColor()
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

        /// <inheritdoc/>
        protected override (Vector2 Scale, Vector2 Offset) GetGlossMapST()
        {
            return (poiyomiMaterial.MetallicMapsTextureScale, poiyomiMaterial.MetallicMapsTextureOffset);
        }

        /// <inheritdoc/>
        protected override float GetGlossStrength()
        {
            return poiyomiMaterial.RoughnessMultiplier;
        }

        /// <inheritdoc/>
        protected override Color GetMainColor()
        {
            if (Settings.useMatcap &&
                poiyomiMaterial.UseMatcap &&
                poiyomiMaterial.MatcapReplace > 0.0f &&
                poiyomiMaterial.MatcapAdd <= 0.0f &&
                poiyomiMaterial.MatcapScreen <= 0.0f &&
                poiyomiMaterial.MatcapMultiply <= 0.0f &&
                !GetUseMainTexture())
            {
                return Color.black;
            }

            return poiyomiMaterial.Material.color;
        }

        /// <inheritdoc/>
        protected override (Vector2 Scale, Vector2 Offset) GetMainTextureST()
        {
            return (poiyomiMaterial.MainTextureScale, poiyomiMaterial.MainTextureOffset);
        }

        /// <inheritdoc/>
        protected override ToonStandardMaterialWrapper.MatcapTypeMode GetMapcapType()
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
        protected override Texture GetMatcap()
        {
            return poiyomiMaterial.MatcapTexture;
        }

        /// <inheritdoc/>
        protected override (Vector2 Scale, Vector2 Offset) GetMatcapMaskST()
        {
            return (poiyomiMaterial.MatcapMaskTextureScale, poiyomiMaterial.MatcapMaskTextureOffset);
        }

        /// <inheritdoc/>
        protected override float GetMatcapMaskStrength()
        {
            return poiyomiMaterial.MatcapIntensity / 5.0f;
        }

        /// <inheritdoc/>
        protected override (Vector2 Scale, Vector2 Offset) GetMetallicMapST()
        {
            return (poiyomiMaterial.MetallicMapsTextureScale, poiyomiMaterial.MetallicMapsTextureOffset);
        }

        /// <inheritdoc/>
        protected override float GetMetallicStrength()
        {
            return poiyomiMaterial.MetallicMultiplier;
        }

        /// <inheritdoc/>
        protected override float GetMinBrightness()
        {
            // Poiyomi _LightingMinLightBrightness is Range(0,1), but ToonStandard _MinBrightness
            // is Range(0, 0.1). Clamp to ToonStandard's upper bound.
            return Mathf.Min(poiyomiMaterial.LightingMinLightBrightness, 0.1f);
        }

        /// <inheritdoc/>
        protected override (Vector2 Scale, Vector2 Offset) GetNormalMapST()
        {
            return (poiyomiMaterial.NormalMapTextureScale, poiyomiMaterial.NormalMapTextureOffset);
        }

        /// <inheritdoc/>
        protected override float GetNormalMapScale()
        {
            return poiyomiMaterial.NormalMapScale;
        }

        /// <inheritdoc/>
        protected override (Vector2 Scale, Vector2 Offset) GetOcculusionMapST()
        {
            return (poiyomiMaterial.AOMapTextureScale, poiyomiMaterial.AOMapTextureOffset);
        }

        /// <inheritdoc/>
        protected override float GetReflectance()
        {
            return 0.5f;
        }

        /// <inheritdoc/>
        protected override float GetRimAlbedoTint()
        {
            // Style 2 (LilToon) has explicit albedo tint; other styles have none.
            return poiyomiMaterial.RimStyle == 2 ? poiyomiMaterial.RimMainStrength : 0.0f;
        }

        /// <inheritdoc/>
        protected override Color GetRimColor()
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
        protected override bool GetRimEnvironmental()
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
        protected override float GetRimIntensity()
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
        protected override float GetRimRange()
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
        protected override float GetRimSoftness()
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
        protected override float GetSharpness()
        {
            return 0.5f;
        }

        /// <inheritdoc/>
        protected override bool GetUseEmission()
        {
            return poiyomiMaterial.UseEmission;
        }

        /// <inheritdoc/>
        protected override bool GetUseEmissionMap()
        {
            return IsEmissionChannelTexturized(poiyomiMaterial.EnableEmission0, poiyomiMaterial.EmissionMap0, poiyomiMaterial.EmissionMask0, poiyomiMaterial.EmissionBaseColorAsMap0)
                || IsEmissionChannelTexturized(poiyomiMaterial.EnableEmission1, poiyomiMaterial.EmissionMap1, poiyomiMaterial.EmissionMask1, poiyomiMaterial.EmissionBaseColorAsMap1)
                || IsEmissionChannelTexturized(poiyomiMaterial.EnableEmission2, poiyomiMaterial.EmissionMap2, poiyomiMaterial.EmissionMask2, poiyomiMaterial.EmissionBaseColorAsMap2)
                || IsEmissionChannelTexturized(poiyomiMaterial.EnableEmission3, poiyomiMaterial.EmissionMap3, poiyomiMaterial.EmissionMask3, poiyomiMaterial.EmissionBaseColorAsMap3);
        }

        /// <inheritdoc/>
        protected override bool GetUseGlossMap()
        {
            return poiyomiMaterial.UseSpecular && poiyomiMaterial.MetallicMaps != null;
        }

        /// <inheritdoc/>
        protected override bool GetUseMainTexture()
        {
            return poiyomiMaterial.Material.mainTexture != null || (poiyomiMaterial.UseEmission && !Settings.useEmission);
        }

        /// <inheritdoc/>
        protected override bool GetUseMatcap()
        {
            return poiyomiMaterial.UseMatcap;
        }

        /// <inheritdoc/>
        protected override bool GetUseMatcapMask()
        {
            return poiyomiMaterial.UseMatcap && poiyomiMaterial.MatcapMask != null;
        }

        /// <inheritdoc/>
        protected override bool GetUseMetallicMap()
        {
            return poiyomiMaterial.UseSpecular && poiyomiMaterial.MetallicMaps != null;
        }

        /// <inheritdoc/>
        protected override bool GetUseNormalMap()
        {
            return poiyomiMaterial.NormalMap != null;
        }

        /// <inheritdoc/>
        protected override bool GetUseOcclusionMap()
        {
            if (poiyomiMaterial.AOMap == null)
            {
                return false;
            }

            var (_, strength) = GetPrimaryAOChannelAndStrength();
            return strength > 0;
        }

        /// <inheritdoc/>
        protected override bool GetUseRimLighting()
        {
            return poiyomiMaterial.UseRimLighting;
        }

        /// <inheritdoc/>
        protected override bool GetUseShadowRamp()
        {
            return poiyomiMaterial.UseShadow;
        }

        /// <inheritdoc/>
        protected override bool GetUseSpecular()
        {
            return poiyomiMaterial.UseSpecular;
        }

        private static Color NormalizeHdrColor(Color color)
        {
            color = Utils.ColorUtility.HdrToLdr(color);
            color.a = Mathf.Min(color.a, 1.0f);
            return color;
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

        private IEnumerable<int> GetEnabledEmissionChannels()
        {
            if (poiyomiMaterial.EnableEmission0)
            {
                yield return 0;
            }

            if (poiyomiMaterial.EnableEmission1)
            {
                yield return 1;
            }

            if (poiyomiMaterial.EnableEmission2)
            {
                yield return 2;
            }

            if (poiyomiMaterial.EnableEmission3)
            {
                yield return 3;
            }
        }

        private void SetEmissionBakeProperties(Material bakeMat)
        {
            bakeMat.SetFloat("_EnableEmission", poiyomiMaterial.EnableEmission0 ? 1.0f : 0.0f);
            bakeMat.SetTexture("_EmissionMap", poiyomiMaterial.EmissionMap0);
            bakeMat.SetTextureScale("_EmissionMap", poiyomiMaterial.EmissionMapTextureScale0);
            bakeMat.SetTextureOffset("_EmissionMap", poiyomiMaterial.EmissionMapTextureOffset0);
            bakeMat.SetColor("_EmissionColor", NormalizeHdrColor(poiyomiMaterial.EmissionColor0));
            bakeMat.SetTexture("_EmissionMask", poiyomiMaterial.EmissionMask0);
            bakeMat.SetTextureScale("_EmissionMask", poiyomiMaterial.EmissionMaskTextureScale0);
            bakeMat.SetTextureOffset("_EmissionMask", poiyomiMaterial.EmissionMaskTextureOffset0);
            bakeMat.SetFloat("_EmissionMaskChannel", poiyomiMaterial.EmissionMaskChannel0);
            bakeMat.SetFloat("_EmissionStrength", poiyomiMaterial.EmissionStrength0);
            bakeMat.SetFloat("_EmissionBaseColorAsMap", poiyomiMaterial.EmissionBaseColorAsMap0 ? 1.0f : 0.0f);

            bakeMat.SetFloat("_EnableEmission1", poiyomiMaterial.EnableEmission1 ? 1.0f : 0.0f);
            bakeMat.SetTexture("_EmissionMap1", poiyomiMaterial.EmissionMap1);
            bakeMat.SetTextureScale("_EmissionMap1", poiyomiMaterial.EmissionMapTextureScale1);
            bakeMat.SetTextureOffset("_EmissionMap1", poiyomiMaterial.EmissionMapTextureOffset1);
            bakeMat.SetColor("_EmissionColor1", NormalizeHdrColor(poiyomiMaterial.EmissionColor1));
            bakeMat.SetTexture("_EmissionMask1", poiyomiMaterial.EmissionMask1);
            bakeMat.SetTextureScale("_EmissionMask1", poiyomiMaterial.EmissionMaskTextureScale1);
            bakeMat.SetTextureOffset("_EmissionMask1", poiyomiMaterial.EmissionMaskTextureOffset1);
            bakeMat.SetFloat("_EmissionMaskChannel1", poiyomiMaterial.EmissionMaskChannel1);
            bakeMat.SetFloat("_EmissionStrength1", poiyomiMaterial.EmissionStrength1);
            bakeMat.SetFloat("_EmissionBaseColorAsMap1", poiyomiMaterial.EmissionBaseColorAsMap1 ? 1.0f : 0.0f);

            bakeMat.SetFloat("_EnableEmission2", poiyomiMaterial.EnableEmission2 ? 1.0f : 0.0f);
            bakeMat.SetTexture("_EmissionMap2", poiyomiMaterial.EmissionMap2);
            bakeMat.SetTextureScale("_EmissionMap2", poiyomiMaterial.EmissionMapTextureScale2);
            bakeMat.SetTextureOffset("_EmissionMap2", poiyomiMaterial.EmissionMapTextureOffset2);
            bakeMat.SetColor("_EmissionColor2", NormalizeHdrColor(poiyomiMaterial.EmissionColor2));
            bakeMat.SetTexture("_EmissionMask2", poiyomiMaterial.EmissionMask2);
            bakeMat.SetTextureScale("_EmissionMask2", poiyomiMaterial.EmissionMaskTextureScale2);
            bakeMat.SetTextureOffset("_EmissionMask2", poiyomiMaterial.EmissionMaskTextureOffset2);
            bakeMat.SetFloat("_EmissionMaskChannel2", poiyomiMaterial.EmissionMaskChannel2);
            bakeMat.SetFloat("_EmissionStrength2", poiyomiMaterial.EmissionStrength2);
            bakeMat.SetFloat("_EmissionBaseColorAsMap2", poiyomiMaterial.EmissionBaseColorAsMap2 ? 1.0f : 0.0f);

            bakeMat.SetFloat("_EnableEmission3", poiyomiMaterial.EnableEmission3 ? 1.0f : 0.0f);
            bakeMat.SetTexture("_EmissionMap3", poiyomiMaterial.EmissionMap3);
            bakeMat.SetTextureScale("_EmissionMap3", poiyomiMaterial.EmissionMapTextureScale3);
            bakeMat.SetTextureOffset("_EmissionMap3", poiyomiMaterial.EmissionMapTextureOffset3);
            bakeMat.SetColor("_EmissionColor3", NormalizeHdrColor(poiyomiMaterial.EmissionColor3));
            bakeMat.SetTexture("_EmissionMask3", poiyomiMaterial.EmissionMask3);
            bakeMat.SetTextureScale("_EmissionMask3", poiyomiMaterial.EmissionMaskTextureScale3);
            bakeMat.SetTextureOffset("_EmissionMask3", poiyomiMaterial.EmissionMaskTextureOffset3);
            bakeMat.SetFloat("_EmissionMaskChannel3", poiyomiMaterial.EmissionMaskChannel3);
            bakeMat.SetFloat("_EmissionStrength3", poiyomiMaterial.EmissionStrength3);
            bakeMat.SetFloat("_EmissionBaseColorAsMap3", poiyomiMaterial.EmissionBaseColorAsMap3 ? 1.0f : 0.0f);
        }

        private static bool IsEmissionChannelTexturized(bool enabled, Texture map, Texture mask, bool baseColorAsMap)
            => enabled && (map != null || mask != null || baseColorAsMap);

        private (int Channel, float Strength) GetPrimaryAOChannelAndStrength()
        {
            var strengths = new float[]
            {
                poiyomiMaterial.AOStrengthR,
                poiyomiMaterial.AOStrengthG,
                poiyomiMaterial.AOStrengthB,
                poiyomiMaterial.AOStrengthA,
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

        private Texture GetPrimaryEmissionMap()
        {
            foreach (var idx in GetEnabledEmissionChannels())
            {
                var map = GetEmissionMapByIndex(idx);
                if (map != null)
                {
                    return map;
                }
            }

            return poiyomiMaterial.EmissionMap0
                ?? poiyomiMaterial.EmissionMap1
                ?? poiyomiMaterial.EmissionMap2
                ?? poiyomiMaterial.EmissionMap3;
        }

        private Texture GetEmissionMapByIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return poiyomiMaterial.EmissionMap0;
                case 1:
                    return poiyomiMaterial.EmissionMap1;
                case 2:
                    return poiyomiMaterial.EmissionMap2;
                case 3:
                    return poiyomiMaterial.EmissionMap3;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        private Texture GetEmissionMaskByIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return poiyomiMaterial.EmissionMask0;
                case 1:
                    return poiyomiMaterial.EmissionMask1;
                case 2:
                    return poiyomiMaterial.EmissionMask2;
                case 3:
                    return poiyomiMaterial.EmissionMask3;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        private Color GetEmissionColorByIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return poiyomiMaterial.EmissionColor0;
                case 1:
                    return poiyomiMaterial.EmissionColor1;
                case 2:
                    return poiyomiMaterial.EmissionColor2;
                case 3:
                    return poiyomiMaterial.EmissionColor3;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }
}
