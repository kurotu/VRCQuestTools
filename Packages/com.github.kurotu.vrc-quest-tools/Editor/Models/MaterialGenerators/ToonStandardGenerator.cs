using System;
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

        /// <inheritdoc/>
        public AsyncCallbackRequest GenerateMaterial(MaterialBase material, UnityEditor.BuildTarget buildTarget, bool saveTextureAsPng, string texturesPath, Action<Material> completion)
        {
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
                        newMaterial.NormalMapScale = GetNormalMapScale();
                    }).WaitForCompletion();
                }

                newMaterial.Culling = GetCulling();

                MaterialGeneratorUtility.GenerateTexture(material.Material, settings, "shadowRamp", saveTextureAsPng, texturesPath, (compl) => GenerateShadowRamp(compl), (t) =>
                {
                    newMaterial.ShadowRamp = t;
                    newMaterial.ShadowBoost = 0.0f;
                    newMaterial.ShadowTint = 0.0f;
                    newMaterial.MinBrightness = GetMinBrightness();
                }).WaitForCompletion();

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

                var texturePack = new TexturePack();
                if (GetUseSpecular())
                {
                    newMaterial.UseSpecular = true;
                    if (GetUseMetallicMap())
                    {
                        MaterialGeneratorUtility.GenerateParameterTexture(material.Material, settings, "metallic", saveTextureAsPng, texturesPath, (compl) => GenerateMetallicMap(compl), (t) =>
                        {
                            if (texturePack.R == null)
                            {
                                texturePack.R = new Tuple<TexturePack.RedChannelType, Texture2D>(TexturePack.RedChannelType.MetallicMap, t);
                            }
                            newMaterial.MetallicMap = t;
                        }).WaitForCompletion();
                    }
                    newMaterial.MetallicStrength = GetMetallicStrength();

                    if (GetUseGlossMap())
                    {
                        MaterialGeneratorUtility.GenerateParameterTexture(material.Material, settings, "gloss", saveTextureAsPng, texturesPath, (compl) => GenerateGlossMap(compl), (t) =>
                        {
                            if (texturePack.A == null)
                            {
                                texturePack.A = new Tuple<TexturePack.AlphaChannelType, Texture2D>(TexturePack.AlphaChannelType.GlossMap, t);
                            }
                            newMaterial.GlossMap = t;
                        }).WaitForCompletion();
                    }
                    newMaterial.GlossStrength = GetGlossStrength();

                    newMaterial.Sharpness = GetSharpness();
                    newMaterial.Reflectance = GetReflectance();
                    newMaterial.Anisotropy = GetAnisotropy();
                }

                if (GetUseMatcap())
                {
                    newMaterial.UseMatcap = true;
                    newMaterial.Matcap = GetMatcap();
                    if (GetUseMatcapMask())
                    {
                        MaterialGeneratorUtility.GenerateTexture(material.Material, settings, "matcapMask", saveTextureAsPng, texturesPath, (compl) => GenerateMatcapMask(compl), (t) =>
                        {
                            if (texturePack.R == null)
                            {
                                texturePack.R = new Tuple<TexturePack.RedChannelType, Texture2D>(TexturePack.RedChannelType.MatcapMask, t);
                            }
                            newMaterial.MatcapMask = t;
                        }).WaitForCompletion();
                    }
                    newMaterial.MatcapStrength = GetMatcapMaskStrength();
                    newMaterial.MatcapType = GetMapcapType();
                }

                if (GetUseRimLighting())
                {
                    newMaterial.UseRimLighting = true;
                    newMaterial.RimColor = GetRimColor();
                    newMaterial.RimIntensity = GetRimIntensity();
                    newMaterial.RimRange = GetRimRange();
                    newMaterial.RimSoftness = GetRimSoftness();
                    newMaterial.RimEnvironmental = GetRimEnvironmental();
                }

                if (texturePack.NeedsPacking)
                {
                    Debug.LogWarning("Pack textures are not supported yet.");
                }
            }
            else
            {
                newMaterial = new ToonStandardMaterialWrapper(ConvertToToonStandard());
            }

            return new ResultRequest<Material>(newMaterial, completion);
        }

        /// <inheritdoc/>
        public AsyncCallbackRequest GenerateTextures(MaterialBase material, UnityEditor.BuildTarget buildTarget, bool saveTextureAsPng, string texturesPath, Action completion)
        {
            if (!(material is IToonStandardConvertable))
            {
                Debug.LogWarning("ToonStandardGenerator only supports LilToonMaterial.");
                return new ToonLitGenerator(new ToonLitConvertSettings()).GenerateTextures(material, buildTarget, saveTextureAsPng, texturesPath, completion);
            }

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
        /// Gets the anisotropy of the material.
        /// </summary>
        /// <returns>Anisotropy.</returns>
        protected abstract float GetAnisotropy();

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
        /// Generates the occulusion map of the material.
        /// </summary>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        // protected abstract AsyncCallbackRequest GenerateOcculusionMap(Action<Texture2D> completion);

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
        // protected abstract AsyncCallbackRequest GenerateMatcap(Action<Texture2D> completion);

        /// <summary>
        /// Generates the matcap mask of the material.
        /// </summary>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Async callback request.</returns>
        protected abstract AsyncCallbackRequest GenerateMatcapMask(Action<Texture2D> completion);

        private class TexturePack
        {
            public Tuple<RedChannelType, Texture2D> R;
            public Tuple<GreenChannelType, Texture2D> G;
            public Tuple<BlueChannelType, Texture2D> B;
            public Tuple<AlphaChannelType, Texture2D> A;

            internal enum RedChannelType
            {
                DetailMask,
                MetallicMap,
                MatcapMask,
            }

            internal enum GreenChannelType
            {
                OcculusionMap,
            }

            internal enum BlueChannelType
            {
            }

            internal enum AlphaChannelType
            {
                GlossMap,
            }

            public bool NeedsPacking
            {
                get
                {
                    var count = 0;
                    if (R != null)
                    {
                        count++;
                    }
                    if (G != null)
                    {
                        count++;
                    }
                    if (B != null)
                    {
                        count++;
                    }
                    if (A != null)
                    {
                        count++;
                    }
                    return count > 1;
                }
            }
        }
    }
}
