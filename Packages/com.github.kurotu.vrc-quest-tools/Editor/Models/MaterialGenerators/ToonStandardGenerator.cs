// <copyright file="ToonStandardGenerator.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Material generator for Toon Standard.
    /// Shader-neutral values are read from the source interfaces and Toon Standard specific values from <see cref="ToonStandardSourceMapping"/>.
    /// </summary>
    internal class ToonStandardGenerator : IMaterialGenerator
    {
        /// <summary>
        /// Settings for the material conversion.
        /// </summary>
        protected readonly ToonStandardConvertSettings Settings;

        /// <summary>
        /// Shared black texture to disable emission.
        /// </summary>
        protected readonly Texture2D sharedBlackTexture;

        /// <summary>
        /// Whether the conversion is for the NDMF editor preview. When true, the generated normal map is
        /// re-uploaded so it displays correctly in the editor.
        /// </summary>
        protected readonly bool forEditorPreview;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToonStandardGenerator"/> class.
        /// </summary>
        /// <param name="settings">Convert settings.</param>
        /// <param name="sharedBlackTexture">Shared black texture to disable emission.</param>
        /// <param name="forEditorPreview">Whether the conversion is for the NDMF editor preview.</param>
        internal ToonStandardGenerator(ToonStandardConvertSettings settings, Texture2D sharedBlackTexture, bool forEditorPreview)
        {
            this.Settings = settings;
            this.sharedBlackTexture = sharedBlackTexture;
            this.forEditorPreview = forEditorPreview;
        }

        /// <summary>
        /// Mask types for the texture pack.
        /// </summary>
        internal enum MaskType
        {
            /// <summary>
            /// No mask.
            /// </summary>
            None,

            /// <summary>
            /// Detail mask.
            /// </summary>
            DetailMask,

            /// <summary>
            /// Metallic map.
            /// </summary>
            MetallicMap,

            /// <summary>
            /// Matcap mask.
            /// </summary>
            MatcapMask,

            /// <summary>
            /// Occulusion map.
            /// </summary>
            OcculusionMap,

            /// <summary>
            /// Gloss map.
            /// </summary>
            GlossMap,
        }

        /// <inheritdoc/>
        public AsyncCallbackRequest GenerateMaterial(MaterialBase material, UnityEditor.BuildTarget buildTarget, bool saveTextureAsPng, string texturesPath, Action<Material> completion)
        {
            var mapping = ToonStandardSourceMapping.Create(material, Settings, sharedBlackTexture);
            if (mapping == null)
            {
                var toonLitConvertSettings = new ToonLitConvertSettings
                {
                    generateQuestTextures = Settings.generateQuestTextures,
                    maxTextureSize = Settings.maxTextureSize,
                    mobileTextureFormat = Settings.mobileTextureFormat,
                    mainTextureBrightness = 1.0f,
                    generateShadowFromNormalMap = true,
                };
                return new ToonLitGenerator(toonLitConvertSettings).GenerateMaterial(material, buildTarget, saveTextureAsPng, texturesPath, (newMat) =>
                {
                    var newMaterial = new ToonStandardMaterialWrapper();
                    newMaterial.Name = material.Material.name;
                    newMaterial.MainTexture = newMat.mainTexture;
                    newMaterial.MainTextureScale = newMat.mainTextureScale;
                    newMaterial.MainTextureOffset = newMat.mainTextureOffset;
                    newMaterial.ShadowRamp = Settings.fallbackShadowRamp;
                    completion?.Invoke(newMaterial);
                });
            }

            var albedoSource = (IAlbedoSource)material;
            var normalMapSource = (INormalMapSource)material;
            var emissionSource = (IEmissionSource)material;
            var occlusionSource = (IOcclusionSource)material;

            ToonStandardMaterialWrapper newMaterial;
            if (Settings.generateQuestTextures)
            {
                newMaterial = new ToonStandardMaterialWrapper();
                newMaterial.Name = material.Material.name;
                var masks = new List<MaskType>();

                var albedoRequest = CreateAlbedoBakeRequest(Settings);
                if (albedoSource.RequiresAlbedoTexture(albedoRequest))
                {
                    var mainPlatformOverride = albedoSource.GetAlbedoPlatformOverride(albedoRequest);
                    MaterialGeneratorUtility.GenerateTexture(material.Material, Settings, "main", saveTextureAsPng, texturesPath, (compl) => albedoSource.GenerateAlbedo(albedoRequest, compl), (t) =>
                    {
                        newMaterial.MainTexture = t;
                        newMaterial.MainColor = new Color(1, 1, 1, 1);
                        (newMaterial.MainTextureScale, newMaterial.MainTextureOffset) = albedoSource.MainTextureST;
                    }, mainPlatformOverride, forEditorPreview).WaitForCompletion();
                }
                else
                {
                    newMaterial.MainColor = mapping.GetMainColor();
                }

                if (normalMapSource.UseNormalMap && Settings.useNormalMap)
                {
                    newMaterial.UseNormalMap = true;
                    var isMobile = buildTarget == UnityEditor.BuildTarget.Android || buildTarget == UnityEditor.BuildTarget.iOS;
                    var outputRGB = saveTextureAsPng || isMobile;
                    var normalPlatformOverride = normalMapSource.GetNormalMapPlatformOverride();
                    MaterialGeneratorUtility.GenerateNormalMap(material.Material, Settings, "normal", saveTextureAsPng, texturesPath, (compl) => normalMapSource.GenerateNormalMap(outputRGB, Settings.maxTextureSize, compl), (t) =>
                    {
                        newMaterial.NormalMap = t;
                        (newMaterial.NormalMapTextureScale, newMaterial.NormalMapTextureOffset) = mapping.GetNormalMapST();
                        newMaterial.NormalMapScale = normalMapSource.NormalMapScale;
                    }, normalPlatformOverride, forEditorPreview).WaitForCompletion();
                }

                newMaterial.Culling = mapping.GetCulling();

                if (mapping.GetUseShadowRamp())
                {
                    if (Settings.generateShadowRamp)
                    {
                        MaterialGeneratorUtility.GenerateTexture(material.Material, Settings, "shadowRamp", saveTextureAsPng, texturesPath, (compl) => mapping.GenerateShadowRamp(compl), (t) =>
                        {
                            newMaterial.ShadowRamp = t;
                            newMaterial.ShadowBoost = 0.0f;
                            newMaterial.ShadowTint = 0.0f;
                        }, null, forEditorPreview).WaitForCompletion();
                    }
                    else
                    {
                        newMaterial.ShadowRamp = Settings.fallbackShadowRamp;
                        newMaterial.ShadowBoost = 0.0f;
                        newMaterial.ShadowTint = 0.5f;
                    }
                }
                else
                {
                    newMaterial.ShadowRamp = ToonStandardMaterialWrapper.RampTexture.Flat;
                }

                newMaterial.MinBrightness = mapping.GetMinBrightness();

                if (emissionSource.UseEmission && Settings.useEmission)
                {
                    if (emissionSource.UseEmissionMap)
                    {
                        var emissionPlatformOverride = emissionSource.GetEmissionMapPlatformOverride();
                        MaterialGeneratorUtility.GenerateTexture(material.Material, Settings, "emission", saveTextureAsPng, texturesPath, (compl) => emissionSource.GenerateEmissionMap(CreateAlbedoBakeRequest(Settings), Settings.maxTextureSize, compl), (t) =>
                        {
                            newMaterial.EmissionMap = t;
                            newMaterial.EmissionColor = new Color(1, 1, 1, 1);
                        }, emissionPlatformOverride, forEditorPreview).WaitForCompletion();
                    }
                    else
                    {
                        newMaterial.EmissionColor = emissionSource.EmissionColor;
                        newMaterial.EmissionStrength = emissionSource.EmissionStrength;
                    }
                }
                else
                {
                    newMaterial.EmissionMap = sharedBlackTexture;
                    newMaterial.EmissionColor = Color.black;
                }

                if (occlusionSource.UseOcclusionMap && Settings.useOcclusion)
                {
                    newMaterial.UseOcclusion = true;
                    newMaterial.OcclusionStrength = occlusionSource.OcclusionStrength;
                    masks.Add(MaskType.OcculusionMap);
                }

                if (mapping.GetUseSpecular() && Settings.useSpecular)
                {
                    newMaterial.UseSpecular = true;
                    if (mapping.GetUseMetallicMap())
                    {
                        masks.Add(MaskType.MetallicMap);
                    }
                    newMaterial.MetallicStrength = mapping.GetMetallicStrength();

                    if (mapping.GetUseGlossMap())
                    {
                        masks.Add(MaskType.GlossMap);
                    }
                    newMaterial.GlossStrength = mapping.GetGlossStrength();

                    newMaterial.Sharpness = mapping.GetSharpness();
                    newMaterial.Reflectance = mapping.GetReflectance();
                }

                if (mapping.GetUseMatcap() && Settings.useMatcap)
                {
                    newMaterial.UseMatcap = true;
                    var sourceMatcapPlatformOverride = mapping.GetMatcapPlatformOverride();
                    var matcapPlatformOverride = BuildEffectiveMatcapFormatOverride(sourceMatcapPlatformOverride);
                    MaterialGeneratorUtility.GenerateTexture(material.Material, Settings, "matcap", saveTextureAsPng, texturesPath, (compl) => mapping.GenerateMatcap(compl), (t) =>
                    {
                        newMaterial.Matcap = t;
                    }, matcapPlatformOverride, forEditorPreview).WaitForCompletion();

                    if (mapping.GetUseMatcapMask())
                    {
                        masks.Add(MaskType.MatcapMask);
                    }
                    newMaterial.MatcapStrength = mapping.GetMatcapMaskStrength();
                    newMaterial.MatcapType = mapping.GetMapcapType();
                }

                if (mapping.GetUseRimLighting() && Settings.useRimLighting)
                {
                    newMaterial.UseRimLighting = true;
                    newMaterial.RimColor = mapping.GetRimColor();
                    newMaterial.RimAlbedoTint = mapping.GetRimAlbedoTint();
                    newMaterial.RimIntensity = mapping.GetRimIntensity();
                    newMaterial.RimRange = mapping.GetRimRange();
                    newMaterial.RimSoftness = mapping.GetRimSoftness();
                    newMaterial.RimEnvironmental = mapping.GetRimEnvironmental();
                }

                if (masks.Count > 0)
                {
                    var texturePacks = new List<TexturePack>();

                    // per 4 masks, generate a texture pack.
                    for (int i = 0; i < masks.Count; i += 4)
                    {
                        var pack = new TexturePack
                        {
                            // R and A has higher quality in DXT5.
                            R = masks.ElementAtOrDefault(i),
                            A = masks.ElementAtOrDefault(i + 1),
                            G = masks.ElementAtOrDefault(i + 2),
                            B = masks.ElementAtOrDefault(i + 3),
                        };
                        texturePacks.Add(pack);
                    }

                    foreach (var pack in texturePacks)
                    {
                        var name = $"mask_{pack.R}_{pack.G}_{pack.B}_{pack.A}";
                        var sourcePackedMaskPlatformOverride = mapping.GetPackedMaskPlatformOverride(pack);
                        var packedMaskPlatformOverride = BuildEffectiveMaskFormatOverride(sourcePackedMaskPlatformOverride);
                        // Masks hold linear values, so they must not be stored as sRGB textures.
                        MaterialGeneratorUtility.GenerateParameterTexture(material.Material, Settings, name, saveTextureAsPng, texturesPath, (compl) => mapping.GeneratePackedMask(pack, compl), (t) =>
                        {
                            foreach (var mask in pack.GetMasks())
                            {
                                switch (mask.MaskType)
                                {
                                    case MaskType.None:
                                        break;
                                    case MaskType.DetailMask:
                                        newMaterial.DetailMask = t;
                                        newMaterial.DetailMaskChannel = mask.Channel;

                                        // TODO: DetailMask ST.
                                        break;
                                    case MaskType.MetallicMap:
                                        newMaterial.MetallicMap = t;
                                        newMaterial.MetallicMapChannel = mask.Channel;
                                        (newMaterial.MetallicMapTextureScale, newMaterial.MetallicMapTextureOffset) = mapping.GetMetallicMapST();
                                        break;
                                    case MaskType.MatcapMask:
                                        newMaterial.MatcapMask = t;
                                        newMaterial.MatcapMaskChannel = mask.Channel;
                                        (newMaterial.MatcapMaskTextureScale, newMaterial.MatcapMaskTextureOffset) = mapping.GetMatcapMaskST();
                                        break;
                                    case MaskType.OcculusionMap:
                                        newMaterial.OcclusionMap = t;
                                        newMaterial.OcclusionMapChannel = mask.Channel;
                                        (newMaterial.OcclusionMapTextureScale, newMaterial.OcclusionMapTextureOffset) = mapping.GetOcculusionMapST();
                                        break;
                                    case MaskType.GlossMap:
                                        newMaterial.GlossMap = t;
                                        newMaterial.GlossMapChannel = mask.Channel;
                                        (newMaterial.GlossMapTextureScale, newMaterial.GlossMapTextureOffset) = mapping.GetGlossMapST();
                                        break;
                                    default:
                                        throw new InvalidProgramException($"Unhandled mask type: {mask.MaskType}");
                                }
                            }
                        }, packedMaskPlatformOverride, forEditorPreview).WaitForCompletion();
                    }
                }
            }
            else
            {
                newMaterial = new ToonStandardMaterialWrapper(mapping.ConvertToToonStandard());
            }

            return new ResultRequest<Material>(newMaterial, completion);
        }

        /// <inheritdoc/>
        public AsyncCallbackRequest GenerateTextures(MaterialBase material, UnityEditor.BuildTarget buildTarget, bool saveTextureAsPng, string texturesPath, Action completion)
        {
            return GenerateMaterial(material, buildTarget, saveTextureAsPng, texturesPath, (newMaterial) =>
            {
                completion?.Invoke();
            });
        }

        /// <summary>
        /// Creates the albedo bake request which reproduces the main texture for Toon Standard.
        /// </summary>
        /// <param name="settings">Convert settings.</param>
        /// <returns>Albedo bake request.</returns>
        internal static AlbedoBakeRequest CreateAlbedoBakeRequest(ToonStandardConvertSettings settings)
        {
            return new AlbedoBakeRequest
            {
                GenerateQuestTextures = settings.generateQuestTextures,
                MaxTextureSize = settings.maxTextureSize,
                MobileTextureFormat = settings.mobileTextureFormat,
                Brightness = 1.0f,
                ShadeFromNormalMap = !settings.useNormalMap,
                FoldEmission = !settings.useEmission,
                TargetRendersMatcap = settings.useMatcap,
            };
        }

        /// <summary>
        /// Builds the effective platform override for packed mask textures, incorporating mask format settings.
        /// </summary>
        /// <param name="sourceOverride">Platform override derived from source textures.</param>
        /// <returns>Effective platform override for mask textures.</returns>
        private (int MaxTextureSize, TextureFormat Format)? BuildEffectiveMaskFormatOverride((int MaxTextureSize, TextureFormat Format)? sourceOverride)
        {
            if (Settings.maskMobileTextureFormat != MobileTextureFormat.NoOverride)
            {
                var format = (TextureFormat)(int)Settings.maskMobileTextureFormat;
                var sourceMaxSize = TextureUtility.NormalizeMaxTextureSize(sourceOverride?.MaxTextureSize);
                var maskLimit = TextureUtility.NormalizeMaxTextureSize((int)Settings.maskMaxTextureSize);
                var mainLimit = TextureUtility.NormalizeMaxTextureSize((int)Settings.maxTextureSize);
                var settingsMaxSize = maskLimit ?? mainLimit;
                var maxSize = TextureUtility.MinDefinedMaxTextureSize(sourceMaxSize, settingsMaxSize) ?? 0;
                return (maxSize, format);
            }

            return sourceOverride;
        }

        /// <summary>
        /// Builds the effective platform override for matcap textures, incorporating matcap format settings.
        /// </summary>
        /// <param name="sourceOverride">Platform override derived from source textures.</param>
        /// <returns>Effective platform override for matcap textures.</returns>
        private (int MaxTextureSize, TextureFormat Format)? BuildEffectiveMatcapFormatOverride((int MaxTextureSize, TextureFormat Format)? sourceOverride)
        {
            if (Settings.matcapMobileTextureFormat != MobileTextureFormat.NoOverride)
            {
                var format = (TextureFormat)(int)Settings.matcapMobileTextureFormat;
                var sourceMaxSize = TextureUtility.NormalizeMaxTextureSize(sourceOverride?.MaxTextureSize);
                var matcapLimit = TextureUtility.NormalizeMaxTextureSize((int)Settings.matcapMaxTextureSize);
                var mainLimit = TextureUtility.NormalizeMaxTextureSize((int)Settings.maxTextureSize);
                var settingsMaxSize = matcapLimit ?? mainLimit;
                var maxSize = TextureUtility.MinDefinedMaxTextureSize(sourceMaxSize, settingsMaxSize) ?? 0;
                return (maxSize, format);
            }

            return sourceOverride;
        }

        /// <summary>
        /// Represents a texture pack for Toon Standard materials, containing different mask types.
        /// </summary>
        internal class TexturePack
        {
            /// <summary>
            /// Red channel mask type.
            /// </summary>
            public MaskType R;

            /// <summary>
            /// Green channel mask type.
            /// </summary>
            public MaskType G;

            /// <summary>
            /// Blue channel mask type.
            /// </summary>
            public MaskType B;

            /// <summary>
            /// Alpha channel mask type.
            /// </summary>
            public MaskType A;

            /// <summary>
            /// Enumerates the masks in this texture pack with their corresponding channels.
            /// </summary>
            /// <returns>IEnumerable for mask type and channel.</returns>
            internal IEnumerable<(MaskType MaskType, ToonStandardMaterialWrapper.MaskChannel Channel)> GetMasks()
            {
                if (R != MaskType.None)
                {
                    yield return (R, ToonStandardMaterialWrapper.MaskChannel.R);
                }
                if (G != MaskType.None)
                {
                    yield return (G, ToonStandardMaterialWrapper.MaskChannel.G);
                }
                if (B != MaskType.None)
                {
                    yield return (B, ToonStandardMaterialWrapper.MaskChannel.B);
                }
                if (A != MaskType.None)
                {
                    yield return (A, ToonStandardMaterialWrapper.MaskChannel.A);
                }
            }
        }
    }
}
