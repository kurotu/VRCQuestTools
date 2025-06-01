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
    /// Base class for Toon Standard material generator.
    /// </summary>
    internal abstract class ToonStandardGenerator : IMaterialGenerator
    {
        /// <summary>
        /// Settings for the material conversion.
        /// </summary>
        protected readonly ToonStandardConvertSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToonStandardGenerator"/> class.
        /// </summary>
        /// <param name="settings">Convert settings.</param>
        internal ToonStandardGenerator(ToonStandardConvertSettings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Mask types for the texture pack.
        /// </summary>
        protected enum MaskType
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
#if VQT_HAS_VRCSDK_TOON_STANDARD
            if (!(material is IToonStandardConvertable))
            {
                var toonLitConvertSettings = new ToonLitConvertSettings
                {
                    generateQuestTextures = settings.generateQuestTextures,
                    maxTextureSize = settings.maxTextureSize,
                    mobileTextureFormat = settings.mobileTextureFormat,
                    mainTextureBrightness = 1.0f,
                    generateShadowFromNormalMap = true,
                };
                return new ToonLitGenerator(toonLitConvertSettings).GenerateMaterial(material, buildTarget, saveTextureAsPng, texturesPath, (newMat) =>
                {
                    var newMaterial = new ToonStandardMaterialWrapper();
                    newMaterial.Name = material.Material.name;
                    newMaterial.MainTexture = newMat.mainTexture;
                    newMaterial.ShadowRamp = settings.fallbackShadowRamp;
                    completion?.Invoke(newMaterial);
                });
            }

            ToonStandardMaterialWrapper newMaterial;
            if (settings.generateQuestTextures)
            {
                newMaterial = new ToonStandardMaterialWrapper();
                newMaterial.Name = material.Material.name;
                var masks = new List<MaskType>();

                if (GetUseMainTexture())
                {
                    MaterialGeneratorUtility.GenerateTexture(material.Material, settings, "main", saveTextureAsPng, texturesPath, (compl) => GenerateMainTexture(compl), (t) =>
                    {
                        newMaterial.MainTexture = t;
                        newMaterial.MainColor = new Color(1, 1, 1, 1);
                    }).WaitForCompletion();
                }
                else
                {
                    newMaterial.MainColor = GetMainColor();
                }

                if (GetUseNormalMap())
                {
                    newMaterial.UseNormalMap = true;
                    var isMobile = buildTarget == UnityEditor.BuildTarget.Android || buildTarget == UnityEditor.BuildTarget.iOS;
                    var outputRGB = saveTextureAsPng || isMobile;
                    MaterialGeneratorUtility.GenerateNormalMap(material.Material, settings, "normal", saveTextureAsPng, texturesPath, (compl) => GenerateNormalMap(outputRGB, compl), (t) =>
                    {
                        newMaterial.NormalMap = t;
                        (newMaterial.NormalMapTextureScale, newMaterial.NormalMapTextureOffset) = GetNormalMapST();
                        newMaterial.NormalMapScale = GetNormalMapScale();
                    }).WaitForCompletion();
                }

                newMaterial.Culling = GetCulling();

                if (GetUseShadowRamp())
                {
                    MaterialGeneratorUtility.GenerateTexture(material.Material, settings, "shadowRamp", saveTextureAsPng, texturesPath, (compl) => GenerateShadowRamp(compl), (t) =>
                    {
                        newMaterial.ShadowRamp = t;
                        newMaterial.ShadowBoost = 0.0f;
                        newMaterial.ShadowTint = 0.0f;
                    }).WaitForCompletion();
                }
                else
                {
                    newMaterial.ShadowRamp = ToonStandardMaterialWrapper.RampTexture.Flat;
                }

                newMaterial.MinBrightness = GetMinBrightness();

                if (GetUseEmissionMap())
                {
                    MaterialGeneratorUtility.GenerateTexture(material.Material, settings, "emission", saveTextureAsPng, texturesPath, (compl) => GenerateEmissionMap(compl), (t) =>
                    {
                        newMaterial.EmissionMap = t;
                        newMaterial.EmissionColor = new Color(1, 1, 1, 1);
                    }).WaitForCompletion();
                }
                else
                {
                    newMaterial.EmissionColor = GetEmissionColor();
                }

                if (GetUseOcclusionMap())
                {
                    newMaterial.UseOcclusion = true;
                    masks.Add(MaskType.OcculusionMap);
                }

                if (GetUseSpecular())
                {
                    newMaterial.UseSpecular = true;
                    if (GetUseMetallicMap())
                    {
                        masks.Add(MaskType.MetallicMap);
                    }
                    newMaterial.MetallicStrength = GetMetallicStrength();

                    if (GetUseGlossMap())
                    {
                        masks.Add(MaskType.GlossMap);
                    }
                    newMaterial.GlossStrength = GetGlossStrength();

                    newMaterial.Sharpness = GetSharpness();
                    newMaterial.Reflectance = GetReflectance();
                }

                if (GetUseMatcap())
                {
                    newMaterial.UseMatcap = true;
                    MaterialGeneratorUtility.GenerateTexture(material.Material, settings, "matcap", saveTextureAsPng, texturesPath, (compl) => GenerateMatcap(compl), (t) =>
                    {
                        newMaterial.Matcap = t;
                    }).WaitForCompletion();

                    if (GetUseMatcapMask())
                    {
                        masks.Add(MaskType.MatcapMask);
                    }
                    newMaterial.MatcapStrength = GetMatcapMaskStrength();
                    newMaterial.MatcapType = GetMapcapType();
                }

                if (GetUseRimLighting())
                {
                    newMaterial.UseRimLighting = true;
                    newMaterial.RimColor = GetRimColor();
                    newMaterial.RimAlbedoTint = GetRimAlbedoTint();
                    newMaterial.RimIntensity = GetRimIntensity();
                    newMaterial.RimRange = GetRimRange();
                    newMaterial.RimSoftness = GetRimSoftness();
                    newMaterial.RimEnvironmental = GetRimEnvironmental();
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
                        MaterialGeneratorUtility.GenerateTexture(material.Material, settings, name, saveTextureAsPng, texturesPath, (compl) => GeneratePackedMask(pack, compl), (t) =>
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
                                        (newMaterial.MetallicMapTextureScale, newMaterial.MetallicMapTextureOffset) = GetMetallicMapST();
                                        break;
                                    case MaskType.MatcapMask:
                                        newMaterial.MatcapMask = t;
                                        newMaterial.MatcapMaskChannel = mask.Channel;
                                        (newMaterial.MatcapMaskTextureScale, newMaterial.MatcapMaskTextureOffset) = GetMatcapMaskST();
                                        break;
                                    case MaskType.OcculusionMap:
                                        newMaterial.OcclusionMap = t;
                                        newMaterial.OcclusionMapChannel = mask.Channel;
                                        (newMaterial.OcclusionMapTextureScale, newMaterial.OcclusionMapTextureOffset) = GetOcculusionMapST();
                                        break;
                                    case MaskType.GlossMap:
                                        newMaterial.GlossMap = t;
                                        newMaterial.GlossMapChannel = mask.Channel;
                                        (newMaterial.GlossMapTextureScale, newMaterial.GlossMapTextureOffset) = GetGlossMapST();
                                        break;
                                    default:
                                        throw new InvalidProgramException($"Unhandled mask type: {mask.MaskType}");
                                }
                            }
                        }).WaitForCompletion();
                    }
                }
            }
            else
            {
                newMaterial = new ToonStandardMaterialWrapper(ConvertToToonStandard());
            }

            return new ResultRequest<Material>(newMaterial, completion);
#else
            throw new InvalidOperationException("VRCSDK 3.8.1 or later is required for Toon Standard.");
#endif
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
        /// Converts internal material to Toon Standard.
        /// </summary>
        /// <returns>Toon Standard Material.</returns>
        protected abstract Material ConvertToToonStandard();

        /// <summary>
        /// Gets the material should use main texture.
        /// </summary>
        /// <returns>True if the material should use main texture.</returns>
        protected abstract bool GetUseMainTexture();

        /// <summary>
        /// Gets the main color of the material.
        /// </summary>
        /// <returns>Main color.</returns>
        protected abstract Color GetMainColor();

        /// <summary>
        /// Gets the material should use normal map.
        /// </summary>
        /// <returns>True if the material should use normal map.</returns>
        protected abstract bool GetUseNormalMap();

        /// <summary>
        /// Gets the normal map texture scale and offset of the material.
        /// </summary>
        /// <returns>Scale and offset.</returns>
        protected abstract (Vector2 Scale, Vector2 Offset) GetNormalMapST();

        /// <summary>
        /// Gets the normal map scale.
        /// </summary>
        /// <returns>Normal map scale.</returns>
        protected abstract float GetNormalMapScale();

        /// <summary>
        /// Gets the culling mode of the material.
        /// </summary>
        /// <returns>Culling mode.</returns>
        protected abstract CullMode GetCulling();

        /// <summary>
        /// Gets the material should use shadow.
        /// </summary>
        /// <returns>True if the material should use shadow ramp.</returns>
        protected abstract bool GetUseShadowRamp();

        /// <summary>
        /// Gets the min brightness of the material.
        /// </summary>
        /// <returns>Minimum brightness.</returns>
        protected abstract float GetMinBrightness();

        /// <summary>
        /// Gets the material should use emission map.
        /// </summary>
        /// <returns>True if the material should use emission map.</returns>
        protected abstract bool GetUseEmissionMap();

        /// <summary>
        /// Gets the emission color of the material.
        /// </summary>
        /// <returns>Emission color.</returns>
        protected abstract Color GetEmissionColor();

        /// <summary>
        /// Gets the material should use occulusion map.
        /// </summary>
        /// <returns>True if the material should use occlusion map.</returns>
        protected abstract bool GetUseOcclusionMap();

        /// <summary>
        /// Gets the occulusion map texture scale and offset of the material.
        /// </summary>
        /// <returns>Scale and offset.</returns>
        protected abstract (Vector2 Scale, Vector2 Offset) GetOcculusionMapST();

        /// <summary>
        /// Gets the material should use occulusion map.
        /// </summary>
        /// <returns>True if the material should use occulusion map.</returns>
        // protected abstract bool GetUseOcculusionMap();

        /// <summary>
        /// Gets the material should use specular.
        /// </summary>
        /// <returns>True if the material should use specular.</returns>
        protected abstract bool GetUseSpecular();

        /// <summary>
        /// Gets the material should use metallic map.
        /// </summary>
        /// <returns>True if the material should use metallic map.</returns>
        protected abstract bool GetUseMetallicMap();

        /// <summary>
        /// Gets the metallic map texture scale and offset of the material.
        /// </summary>
        /// <returns>Scale and offset.</returns>
        protected abstract (Vector2 Scale, Vector2 Offset) GetMetallicMapST();

        /// <summary>
        /// Gets the metallic strength of the material.
        /// </summary>
        /// <returns>Metallic strength.</returns>
        protected abstract float GetMetallicStrength();

        /// <summary>
        /// Gets the material should use gloss map.
        /// </summary>
        /// <returns>True if the material should use gloss map.</returns>
        protected abstract bool GetUseGlossMap();

        /// <summary>
        /// Gets the gloss map texture scale and offset of the material.
        /// </summary>
        /// <returns>Scale and offset.</returns>
        protected abstract (Vector2 Scale, Vector2 Offset) GetGlossMapST();

        /// <summary>
        /// Gets the gloss strength of the material.
        /// </summary>
        /// <returns>Gloss strength.</returns>
        protected abstract float GetGlossStrength();

        /// <summary>
        /// Gets the sharpness of the material.
        /// </summary>
        /// <returns>Sharpness.</returns>
        protected abstract float GetSharpness();

        /// <summary>
        /// Gets the reflectance of the material.
        /// </summary>
        /// <returns>Reflectance.</returns>
        protected abstract float GetReflectance();

        /// <summary>
        /// Gets the material should use matcap.
        /// </summary>
        /// <returns>True if the material should use matcap.</returns>
        protected abstract bool GetUseMatcap();

        /// <summary>
        /// Gets the matcap texture of the material.
        /// </summary>
        /// <returns>Matcap texture.</returns>
        protected abstract Texture GetMatcap();

        /// <summary>
        /// Gets the material should use matcap mask.
        /// </summary>
        /// <returns>True if the material should use matcap mask.</returns>
        protected abstract bool GetUseMatcapMask();

        /// <summary>
        /// Gets the matcap mask texture scale and offset of the material.
        /// </summary>
        /// <returns>Scale and offset.</returns>
        protected abstract (Vector2 Scale, Vector2 Offset) GetMatcapMaskST();

        /// <summary>
        /// Gets the matcap mask strength of the material.
        /// </summary>
        /// <returns>Matcap mask strength.</returns>
        protected abstract float GetMatcapMaskStrength();

        /// <summary>
        /// Gets the material should use rim lighting.
        /// </summary>
        /// <returns>True if the material should use rim lighting.</returns>
        protected abstract bool GetUseRimLighting();

        /// <summary>
        /// Gets the rim color of the material.
        /// </summary>
        /// <returns>Rim color.</returns>
        protected abstract Color GetRimColor();

        /// <summary>
        /// Gets the rim albedo tint of the material.
        /// </summary>
        /// <returns>Rim Albedo Tint.</returns>
        protected abstract float GetRimAlbedoTint();

        /// <summary>
        /// Gets the rim intensity of the material.
        /// </summary>
        /// <returns>Rim Intensity.</returns>
        protected abstract float GetRimIntensity();

        /// <summary>
        /// Gets the rim range of the material.
        /// </summary>
        /// <returns>Rim range.</returns>
        protected abstract float GetRimRange();

        /// <summary>
        /// Gets the rim softness of the material.
        /// </summary>
        /// <returns>Rim softness.</returns>
        protected abstract float GetRimSoftness();

        /// <summary>
        /// Gets the rim environmental of the material.
        /// </summary>
        /// <returns>Rim environment.</returns>
        protected abstract bool GetRimEnvironmental();

        /// <summary>
        /// Gets the matcap type of the material.
        /// </summary>
        /// <returns>Matcap type.</returns>
        protected abstract ToonStandardMaterialWrapper.MatcapTypeMode GetMapcapType();

        /// <summary>
        /// Generates the main texture of the material.
        /// </summary>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        protected abstract AsyncCallbackRequest GenerateMainTexture(Action<Texture2D> completion);

        /// <summary>
        /// Generates the normal map of the material.
        /// </summary>
        /// <param name="outputRGB">Whether to output normal map as RGB texture.</param>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        protected abstract AsyncCallbackRequest GenerateNormalMap(bool outputRGB, Action<Texture2D> completion);

        /// <summary>
        /// Generates the shadow ramp of the material.
        /// </summary>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        protected abstract AsyncCallbackRequest GenerateShadowRamp(Action<Texture2D> completion);

        /// <summary>
        /// Generates the emission map of the material.
        /// </summary>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        protected abstract AsyncCallbackRequest GenerateEmissionMap(Action<Texture2D> completion);

        /// <summary>
        /// Generates the occlusion map of the material.
        /// </summary>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        protected abstract AsyncCallbackRequest GenerateOcclusionMap(Action<Texture2D> completion);

        /// <summary>
        /// Generates the detail mask of the material.
        /// </summary>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        // protected abstract AsyncCallbackRequest GenerateDetailMask(Action<Texture2D> completion);

        /// <summary>
        /// Generates the detail texture of the material.
        /// </summary>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        // protected abstract AsyncCallbackRequest GenerateDetailTexture(Action<Texture2D> completion);

        /// <summary>
        /// Generates the detail normal map of the material.
        /// </summary>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        // protected abstract AsyncCallbackRequest GenerateDetailNormalMap(Action<Texture2D> completion);

        /// <summary>
        /// Generates the metallic map of the material.
        /// </summary>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        protected abstract AsyncCallbackRequest GenerateMetallicMap(Action<Texture2D> completion);

        /// <summary>
        /// Generates the gloss map of the material.
        /// </summary>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        protected abstract AsyncCallbackRequest GenerateGlossMap(Action<Texture2D> completion);

        /// <summary>
        /// Generates the matcap of the material.
        /// </summary>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        protected abstract AsyncCallbackRequest GenerateMatcap(Action<Texture2D> completion);

        /// <summary>
        /// Generates the matcap mask of the material.
        /// </summary>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        protected abstract AsyncCallbackRequest GenerateMatcapMask(Action<Texture2D> completion);

        /// <summary>
        /// Generates a packed mask texture from the given texture pack.
        /// </summary>
        /// <param name="pack">Texture pack configuration.</param>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        protected abstract AsyncCallbackRequest GeneratePackedMask(TexturePack pack, Action<Texture2D> completion);

        /// <summary>
        /// Represents a texture pack for Toon Standard materials, containing different mask types.
        /// </summary>
        protected class TexturePack
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
