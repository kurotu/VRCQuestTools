// <copyright file="LilToonToonStandardMapping.cs" company="kurotu">
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
    /// Toon Standard mapping for LilToon materials.
    /// </summary>
    internal class LilToonToonStandardMapping : ToonStandardSourceMapping
    {
        private readonly LilToonMaterial lilMaterial;

        /// <summary>
        /// Initializes a new instance of the <see cref="LilToonToonStandardMapping"/> class.
        /// </summary>
        /// <param name="material">LilToon material.</param>
        /// <param name="settings">Convert settings.</param>
        /// <param name="sharedBlackTexture">Shared black texture to disable emission.</param>
        internal LilToonToonStandardMapping(LilToonMaterial material, ToonStandardConvertSettings settings, Texture2D sharedBlackTexture)
            : base(settings, sharedBlackTexture)
        {
            this.lilMaterial = material;
        }

        /// <inheritdoc/>
        internal override Material ConvertToToonStandard()
        {
            var newMaterial = new ToonStandardMaterialWrapper();

            newMaterial.Name = lilMaterial.Material.name;

            newMaterial.MainTexture = lilMaterial.Material.mainTexture;
            newMaterial.MainTextureScale = lilMaterial.MainTextureScale;
            newMaterial.MainTextureOffset = lilMaterial.MainTextureOffset;
            newMaterial.MainColor = Utils.ColorUtility.HdrToLdr(lilMaterial.Material.color);

            if (lilMaterial.UseNormalMap && Settings.useNormalMap)
            {
                newMaterial.UseNormalMap = true;
                newMaterial.NormalMap = lilMaterial.NormalMap;
                newMaterial.NormalMapTextureScale = lilMaterial.NormalMapTextureScale;
                newMaterial.NormalMapTextureOffset = lilMaterial.NormalMapTextureOffset;
                newMaterial.NormalMapScale = lilMaterial.NormalMapScale;
            }

            newMaterial.Culling = lilMaterial.CullMode;

            if (lilMaterial.UseShadow)
            {
                newMaterial.ShadowRamp = Settings.fallbackShadowRamp;
            }
            else
            {
                newMaterial.ShadowRamp = ToonStandardMaterialWrapper.RampTexture.Flat;
            }

            newMaterial.MinBrightness = lilMaterial.LightMinLimit;

            if (lilMaterial.UseEmission && Settings.useEmission)
            {
                newMaterial.EmissionMap = lilMaterial.EmissionMap;
                newMaterial.EmissionMapTextureScale = lilMaterial.EmissionMapTextureScale;
                newMaterial.EmissionMapTextureOffset = lilMaterial.EmissionMapTextureOffset;
                newMaterial.EmissionColor = Utils.ColorUtility.HdrToLdr(lilMaterial.EmissionColor);
            }
            else
            {
                newMaterial.EmissionMap = SharedBlackTexture;
                newMaterial.EmissionColor = Color.black;
            }

            if (lilMaterial.UseShadow && lilMaterial.AOMap != null && Settings.useOcclusion)
            {
                newMaterial.UseOcclusion = true;

                // The original texture is assigned directly, so _ShadowAOShift remapping is not applied here.
                newMaterial.OcclusionMap = lilMaterial.AOMap;
                newMaterial.OcclusionMapTextureScale = lilMaterial.AOMapTextureScale;
                newMaterial.OcclusionMapTextureOffset = lilMaterial.AOMapTextureOffset;
                newMaterial.OcclusionStrength = ((IOcclusionSource)lilMaterial).OcclusionStrength;
            }

            if (lilMaterial.UseReflection && Settings.useSpecular)
            {
                newMaterial.UseSpecular = true;
                newMaterial.MetallicMap = lilMaterial.MetallicMap;
                newMaterial.MetallicMapTextureScale = lilMaterial.MetallicMapTextureScale;
                newMaterial.MetallicMapTextureOffset = lilMaterial.MetallicMapTextureOffset;
                newMaterial.MetallicStrength = lilMaterial.Metallic;

                newMaterial.GlossMap = lilMaterial.SmoothnessTex;
                newMaterial.GlossMapTextureScale = lilMaterial.SmoothnessTexScale;
                newMaterial.GlossMapTextureOffset = lilMaterial.SmoothnessTexOffset;
                newMaterial.GlossStrength = lilMaterial.Smoothness;
                newMaterial.Sharpness = 1.0f - lilMaterial.SpecularBlur;
            }

            if (lilMaterial.UseMatCap && Settings.useMatcap)
            {
                newMaterial.UseMatcap = true;
                newMaterial.Matcap = lilMaterial.MatCapTex;
                newMaterial.MatcapMask = lilMaterial.MatCapMask;
                newMaterial.MatcapMaskTextureScale = lilMaterial.MatCapMaskTextureScale;
                newMaterial.MatcapMaskTextureOffset = lilMaterial.MatCapMaskTextureOffset;
                newMaterial.MatcapStrength = lilMaterial.MatCapBlend;
                switch (lilMaterial.MatCapBlendingMode)
                {
                    case LilToonMaterial.MatCapBlendMode.Normal:
                    case LilToonMaterial.MatCapBlendMode.Add:
                    case LilToonMaterial.MatCapBlendMode.Screen:
                        newMaterial.MatcapType = ToonStandardMaterialWrapper.MatcapTypeMode.Additive;
                        break;
                    case LilToonMaterial.MatCapBlendMode.Multiply:
                        newMaterial.MatcapType = ToonStandardMaterialWrapper.MatcapTypeMode.Multiplicative;
                        break;
                }
            }

            if (lilMaterial.UseRimLight && Settings.useRimLighting)
            {
                newMaterial.UseRimLighting = true;
                newMaterial.RimColor = Utils.ColorUtility.HdrToLdr(lilMaterial.RimLightColor);
                newMaterial.RimAlbedoTint = lilMaterial.RimMainStrength;
                newMaterial.RimRange = Mathf.Pow(1.0f - lilMaterial.RimLightBorder, lilMaterial.RimFresnelPower);
                newMaterial.RimSoftness = lilMaterial.RimLightBlur;
                newMaterial.RimEnvironmental = lilMaterial.RimEnableLighting > 0.0f;
                if (newMaterial.RimEnvironmental)
                {
                    newMaterial.RimIntensity *= lilMaterial.RimEnableLighting;
                }
            }

            return newMaterial;
        }

        /// <summary>
        /// Gets the platform override settings for gloss map textures.
        /// </summary>
        /// <returns>Platform override settings, or null if none.</returns>
        internal override (int MaxTextureSize, TextureFormat Format)? GetGlossMapPlatformOverride()
        {
            var textures = new List<Texture>(2);

            if (lilMaterial.SmoothnessTex != null)
            {
                textures.Add(lilMaterial.SmoothnessTex);
            }

            if (lilMaterial.ReflectionColorTex != null)
            {
                textures.Add(lilMaterial.ReflectionColorTex);
            }

            if (textures.Count > 0)
            {
                return TextureUtility.GetBestPlatformOverrideSettings(textures.ToArray());
            }

            return null;
        }

        /// <summary>
        /// Gets the platform override settings for matcap textures.
        /// </summary>
        /// <returns>Platform override settings, or null if none.</returns>
        internal override (int MaxTextureSize, TextureFormat Format)? GetMatcapPlatformOverride()
        {
            return TextureUtility.GetBestPlatformOverrideSettings(lilMaterial.MatCapTex);
        }

        /// <summary>
        /// Gets the platform override settings for matcap mask textures.
        /// </summary>
        /// <returns>Platform override settings, or null if none.</returns>
        internal override (int MaxTextureSize, TextureFormat Format)? GetMatcapMaskPlatformOverride()
        {
            return TextureUtility.GetBestPlatformOverrideSettings(lilMaterial.MatCapMask);
        }

        /// <summary>
        /// Gets the platform override settings for metallic map textures.
        /// </summary>
        /// <returns>Platform override settings, or null if none.</returns>
        internal override (int MaxTextureSize, TextureFormat Format)? GetMetallicMapPlatformOverride()
        {
            return TextureUtility.GetBestPlatformOverrideSettings(lilMaterial.MetallicMap, lilMaterial.ReflectionColorTex);
        }

        /// <summary>
        /// Gets the platform override settings for packed mask textures.
        /// </summary>
        /// <param name="pack">The texture pack for which to get overrides.</param>
        /// <returns>Platform override settings, or null if none.</returns>
        internal override (int MaxTextureSize, TextureFormat Format)? GetPackedMaskPlatformOverride(TexturePack pack)
        {
            var sourceTextures = new List<Texture>();
            foreach (var mask in pack.GetMasks())
            {
                Texture tex = null;
                switch (mask.MaskType)
                {
                    case MaskType.DetailMask:
                        break; // lilToon doesn't have detail mask
                    case MaskType.MetallicMap:
                        tex = lilMaterial.MetallicMap;
                        break;
                    case MaskType.MatcapMask:
                        tex = lilMaterial.MatCapMask;
                        break;
                    case MaskType.OcculusionMap:
                        tex = lilMaterial.AOMap;
                        break;
                    case MaskType.GlossMap:
                        tex = lilMaterial.SmoothnessTex;
                        break;
                }
                if (tex != null)
                {
                    sourceTextures.Add(tex);
                }
            }
            return TextureUtility.GetBestPlatformOverrideSettings(sourceTextures.ToArray());
        }

        /// <inheritdoc/>
        internal override AsyncCallbackRequest GenerateGlossMap(Action<Texture2D> completion)
        {
            var gloss = (Texture2D)lilMaterial.SmoothnessTex;
            var reflectionColorTex = (Texture2D)lilMaterial.ReflectionColorTex;
            var reflectionColorTexWidth = reflectionColorTex ? reflectionColorTex.width : 4;
            var reflectionColorTexHeight = reflectionColorTex ? reflectionColorTex.height : 4;
            var sourceWidth = Math.Max(gloss ? gloss.width : 4, reflectionColorTexWidth);
            var sourceHeight = Math.Max(gloss ? gloss.height : 4, reflectionColorTexHeight);

            // Check platform override settings from source textures
            var platformOverride = TextureUtility.GetBestPlatformOverrideSettings(gloss, reflectionColorTex);
            var maxTextureSize = platformOverride?.MaxTextureSize ?? (int)Settings.maxTextureSize;

            var (targetWidth, targetHeight) = TextureUtility.AspectFitReduction(sourceWidth, sourceHeight, maxTextureSize);

            var alphaGlossMap = RenderTexture.GetTemporary(sourceWidth, sourceHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var mat = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));
            mat.SetTexture("_Texture0", gloss);
            mat.SetFloat("_Texture0Input", 0); // R
            mat.SetFloat("_Texture0Output", 3); // A
            Graphics.Blit(null, alphaGlossMap, mat);

            // ReflectionColor is authored in sRGB (gamma) space in the Inspector; linearize before
            // computing luminance so it matches how lilToon composes it in the shader
            // (same treatment as GetOcclusionStrength's shadow colors).
            var reflectionColor = lilMaterial.ReflectionColor;
            var reflectionColorGrayscale = Utils.ColorUtility.GetRec709Grayscale(reflectionColor.linear) * reflectionColor.a;
            var reflectionGrayscaleTex = RenderTexture.GetTemporary(reflectionColorTexWidth, reflectionColorTexHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var mat0 = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));
            mat0.SetTexture("_Texture0", reflectionColorTex);
            mat0.SetFloat("_Texture0Input", 4); // Grayscale
            mat0.SetFloat("_Texture0Output", 3); // A
            mat0.SetFloat("_Texture0Scale", reflectionColorGrayscale); // Bake ReflectionColor tint (Float property avoids Color gamma auto-conversion)
            Graphics.Blit(null, reflectionGrayscaleTex, mat0);

            var rt1 = RenderTexture.GetTemporary(sourceWidth, sourceHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var mat1 = new Material(Shader.Find("Hidden/VRCQuestTools/Multiply"));
            mat1.SetTexture("_Texture0", alphaGlossMap);
            mat1.SetTexture("_Texture1", reflectionGrayscaleTex);
            Graphics.Blit(null, rt1, mat1);

            var rt2 = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            TextureUtility.DownscaleBlit(rt1, false, rt2);

            return TextureUtility.RequestReadbackRenderTexture(rt2, true, true, (tex) =>
            {
                RenderTexture.ReleaseTemporary(alphaGlossMap);
                RenderTexture.ReleaseTemporary(reflectionGrayscaleTex);
                RenderTexture.ReleaseTemporary(rt1);
                RenderTexture.ReleaseTemporary(rt2);
                UnityEngine.Object.DestroyImmediate(mat);
                UnityEngine.Object.DestroyImmediate(mat0);
                UnityEngine.Object.DestroyImmediate(mat1);
                completion?.Invoke(tex);
            });
        }

        /// <inheritdoc/>
        internal override AsyncCallbackRequest GenerateMatcap(Action<Texture2D> completion)
        {
            var mat = new Material(Shader.Find("Hidden/VRCQuestTools/Multiply"));
            mat.SetTexture("_Texture0", lilMaterial.MatCapTex);
            var color = lilMaterial.MatCapColor;
            color = Utils.ColorUtility.HdrToLdr(color);
            switch (lilMaterial.MatCapBlendingMode)
            {
                case LilToonMaterial.MatCapBlendMode.Add:
                case LilToonMaterial.MatCapBlendMode.Screen:
                    var attenuation = 0.8f;
                    color.r *= Mathf.Lerp(color.r, 0.0f, lilMaterial.MatCapMainStrength * attenuation);
                    color.g *= Mathf.Lerp(color.g, 0.0f, lilMaterial.MatCapMainStrength * attenuation);
                    color.b *= Mathf.Lerp(color.b, 0.0f, lilMaterial.MatCapMainStrength * attenuation);
                    break;
                case LilToonMaterial.MatCapBlendMode.Normal:
                case LilToonMaterial.MatCapBlendMode.Multiply:
                    color.r *= Mathf.Lerp(color.r, 1.0f, lilMaterial.MatCapMainStrength);
                    color.g *= Mathf.Lerp(color.g, 1.0f, lilMaterial.MatCapMainStrength);
                    color.b *= Mathf.Lerp(color.b, 1.0f, lilMaterial.MatCapMainStrength);
                    break;
            }
            color.a = 1.0f;
            mat.SetColor("_Texture0Color", color);

            var matcapWidth = lilMaterial.MatCapTex ? lilMaterial.MatCapTex.width : 4;
            var matcapHeight = lilMaterial.MatCapTex ? lilMaterial.MatCapTex.height : 4;

            // Check platform override settings from source texture
            var platformOverride = TextureUtility.GetBestPlatformOverrideSettings(lilMaterial.MatCapTex);
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
            var matcapMask = (Texture2D)lilMaterial.MatCapMask;

            // Check platform override settings from source texture
            var platformOverride = TextureUtility.GetBestPlatformOverrideSettings(matcapMask);
            var maxTextureSize = platformOverride?.MaxTextureSize ?? (int)Settings.maxTextureSize;

            var (width, height) = TextureUtility.AspectFitReduction(matcapMask.width, matcapMask.height, maxTextureSize);

            var rt = RenderTexture.GetTemporary(matcapMask.width, matcapMask.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var mat = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));
            mat.SetTexture("_Texture0", matcapMask);
            mat.SetFloat("_Texture0Input", 4); // Grayscale
            mat.SetFloat("_Texture0Output", 0); // R

            Graphics.Blit(null, rt, mat);

            var rt2 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            TextureUtility.DownscaleBlit(rt, false, rt2);

            return TextureUtility.RequestReadbackRenderTexture(rt2, true, true, (tex) =>
            {
                RenderTexture.ReleaseTemporary(rt);
                RenderTexture.ReleaseTemporary(rt2);
                UnityEngine.Object.DestroyImmediate(mat);
                completion?.Invoke(tex);
            });
        }

        /// <inheritdoc/>
        internal override AsyncCallbackRequest GenerateMetallicMap(Action<Texture2D> completion)
        {
            var metallic = (Texture2D)lilMaterial.MetallicMap;
            var metallicWidth = metallic ? metallic.width : 4;
            var metallicHeight = metallic ? metallic.height : 4;
            var reflectionColorTex = (Texture2D)lilMaterial.ReflectionColorTex;
            var reflectionColorWidth = reflectionColorTex ? reflectionColorTex.width : 4;
            var reflectionColorHeight = reflectionColorTex ? reflectionColorTex.height : 4;

            var originalWidth = Math.Max(metallicWidth, reflectionColorWidth);
            var originalHeight = Math.Max(metallicHeight, reflectionColorHeight);

            // Check platform override settings from source textures
            var platformOverride = TextureUtility.GetBestPlatformOverrideSettings(metallic, reflectionColorTex);
            var maxTextureSize = platformOverride?.MaxTextureSize ?? (int)Settings.maxTextureSize;

            var (width, height) = TextureUtility.AspectFitReduction(originalWidth, originalHeight, maxTextureSize);

            // ReflectionColor is authored in sRGB (gamma) space in the Inspector; linearize before
            // computing luminance so it matches how lilToon composes it in the shader
            // (same treatment as GetOcclusionStrength's shadow colors).
            var reflectionColor = lilMaterial.ReflectionColor;
            var reflectionGrayscale = Utils.ColorUtility.GetRec709Grayscale(reflectionColor.linear) * reflectionColor.a;
            var reflectionGrayscaleTex = RenderTexture.GetTemporary(reflectionColorWidth, reflectionColorHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var mat0 = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));
            mat0.SetTexture("_Texture0", reflectionColorTex);
            mat0.SetFloat("_Texture0Input", 4); // Grayscale
            mat0.SetFloat("_Texture0Output", 0); // R
            mat0.SetFloat("_Texture0Scale", reflectionGrayscale); // Bake ReflectionColor tint (Float property avoids Color gamma auto-conversion)
            Graphics.Blit(null, reflectionGrayscaleTex, mat0);

            var rt = RenderTexture.GetTemporary(originalWidth, originalHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var mat = new Material(Shader.Find("Hidden/VRCQuestTools/Multiply"));
            mat.SetTexture("_Texture0", reflectionGrayscaleTex);
            mat.SetTexture("_Texture1", metallic);
            Graphics.Blit(null, rt, mat);

            var rt2 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            TextureUtility.DownscaleBlit(rt, false, rt2);

            return TextureUtility.RequestReadbackRenderTexture(rt2, true, true, (tex) =>
            {
                RenderTexture.ReleaseTemporary(reflectionGrayscaleTex);
                RenderTexture.ReleaseTemporary(rt);
                RenderTexture.ReleaseTemporary(rt2);
                UnityEngine.Object.DestroyImmediate(mat0);
                UnityEngine.Object.DestroyImmediate(mat);
                completion?.Invoke(tex);
            });
        }

        /// <inheritdoc/>
        internal override AsyncCallbackRequest GeneratePackedMask(TexturePack pack, Action<Texture2D> completion)
        {
            var swizzleMat = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));

            int maxWidth = 0;
            int maxHeight = 0;
            var sourceTextures = new System.Collections.Generic.List<Texture>();

            foreach (var (mask, index) in pack.GetMasks().Select((mask, index) => (mask, index)))
            {
                switch (mask.MaskType)
                {
                    case MaskType.None:
                        break;
                    case MaskType.DetailMask:
                        break;
                    case MaskType.MetallicMap:
                        // Collect source textures for platform override
                        if (lilMaterial.MetallicMap)
                        {
                            sourceTextures.Add(lilMaterial.MetallicMap);
                        }
                        if (lilMaterial.ReflectionColorTex)
                        {
                            sourceTextures.Add(lilMaterial.ReflectionColorTex);
                        }

                        GenerateMetallicMap((tex) =>
                        {
                            if (tex)
                            {
                                swizzleMat.SetTexture($"_Texture{index}", tex);
                                swizzleMat.SetFloat($"_Texture{index}Input", 0); // R
                                swizzleMat.SetFloat($"_Texture{index}Output", (int)mask.Channel);
                                maxHeight = Math.Max(maxHeight, tex.height);
                                maxWidth = Math.Max(maxWidth, tex.width);
                            }
                        }).WaitForCompletion();
                        break;
                    case MaskType.MatcapMask:
                        // Collect source textures for platform override
                        if (lilMaterial.MatCapMask)
                        {
                            sourceTextures.Add(lilMaterial.MatCapMask);
                        }

                        GenerateMatcapMask((tex) =>
                        {
                            if (tex)
                            {
                                swizzleMat.SetTexture($"_Texture{index}", tex);
                                swizzleMat.SetFloat($"_Texture{index}Input", 0); // R
                                swizzleMat.SetFloat($"_Texture{index}Output", (int)mask.Channel);
                                maxHeight = Math.Max(maxHeight, tex.height);
                                maxWidth = Math.Max(maxWidth, tex.width);
                            }
                        }).WaitForCompletion();
                        break;
                    case MaskType.OcculusionMap:
                        // Collect source textures for platform override
                        if (lilMaterial.AOMap)
                        {
                            sourceTextures.Add(lilMaterial.AOMap);
                        }

                        ((IOcclusionSource)lilMaterial).GenerateOcclusionMap(Settings.maxTextureSize, (tex) =>
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
                    case MaskType.GlossMap:
                        // Collect source textures for platform override
                        if (lilMaterial.SmoothnessTex)
                        {
                            sourceTextures.Add(lilMaterial.SmoothnessTex);
                        }
                        if (lilMaterial.ReflectionColorTex)
                        {
                            sourceTextures.Add(lilMaterial.ReflectionColorTex);
                        }

                        GenerateGlossMap((tex) =>
                        {
                            if (tex)
                            {
                                swizzleMat.SetTexture($"_Texture{index}", tex);
                                swizzleMat.SetFloat($"_Texture{index}Input", 3); // A
                                swizzleMat.SetFloat($"_Texture{index}Output", (int)mask.Channel);
                                maxHeight = Math.Max(maxHeight, tex.height);
                                maxWidth = Math.Max(maxWidth, tex.width);
                            }
                        }).WaitForCompletion();
                        break;
                }
            }

            // Check platform override settings from source textures
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
            var material = new Material(lilMaterial.Material);
            material.parent = null;

            if (AssetUtility.CanLilToonBakeShadowRamp())
            {
                material.shader = AssetUtility.GetLilToon2Ramp();
            }
            else
            {
                throw new InvalidOperationException("lilToon 1.10.0 or later is required to bake shadow ramp.");
            }

            var width = 128;
            var height = 16;
            var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(null, rt, material);

            return TextureUtility.RequestReadbackRenderTexture(rt, true, false, (tex) =>
            {
                tex.wrapMode = TextureWrapMode.Clamp;
                RenderTexture.ReleaseTemporary(rt);
                UnityEngine.Object.DestroyImmediate(material);
                completion?.Invoke(tex);
            });
        }

        /// <inheritdoc/>
        internal override CullMode GetCulling()
        {
            return lilMaterial.CullMode;
        }

        /// <inheritdoc/>
        internal override (Vector2 Scale, Vector2 Offset) GetGlossMapST()
        {
            return (lilMaterial.SmoothnessTexScale, lilMaterial.SmoothnessTexOffset);
        }

        /// <inheritdoc/>
        internal override float GetGlossStrength()
        {
            return lilMaterial.Smoothness;
        }

        /// <inheritdoc/>
        internal override Color GetMainColor()
        {
            if (Settings.useMatcap && lilMaterial.UseMatCap && lilMaterial.MatCapBlendingMode == LilToonMaterial.MatCapBlendMode.Normal)
            {
                if (!((IAlbedoSource)lilMaterial).RequiresAlbedoTexture(ToonStandardGenerator.CreateAlbedoBakeRequest(Settings)))
                {
                    return Color.black;
                }
            }
            return lilMaterial.Material.color;
        }

        /// <inheritdoc/>
        internal override ToonStandardMaterialWrapper.MatcapTypeMode GetMapcapType()
        {
            switch (lilMaterial.MatCapBlendingMode)
            {
                case LilToonMaterial.MatCapBlendMode.Normal:
                case LilToonMaterial.MatCapBlendMode.Add:
                case LilToonMaterial.MatCapBlendMode.Screen:
                    return ToonStandardMaterialWrapper.MatcapTypeMode.Additive;
                case LilToonMaterial.MatCapBlendMode.Multiply:
                    return ToonStandardMaterialWrapper.MatcapTypeMode.Multiplicative;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc/>
        internal override Texture GetMatcap()
        {
            return lilMaterial.MatCapTex;
        }

        /// <inheritdoc/>
        internal override (Vector2 Scale, Vector2 Offset) GetMatcapMaskST()
        {
            return (lilMaterial.MatCapMaskTextureScale, lilMaterial.MatCapMaskTextureOffset);
        }

        /// <inheritdoc/>
        internal override float GetMatcapMaskStrength()
        {
            return lilMaterial.MatCapBlend * lilMaterial.MatCapColor.a;
        }

        /// <inheritdoc/>
        internal override (Vector2 Scale, Vector2 Offset) GetMetallicMapST()
        {
            return (lilMaterial.MetallicMapTextureScale, lilMaterial.MetallicMapTextureOffset);
        }

        /// <inheritdoc/>
        internal override float GetMetallicStrength()
        {
            return lilMaterial.Metallic;
        }

        /// <inheritdoc/>
        internal override float GetMinBrightness() => ((IMinimumBrightnessSource)lilMaterial).MinimumBrightness;

        /// <inheritdoc/>
        internal override (Vector2 Scale, Vector2 Offset) GetNormalMapST()
        {
            return (lilMaterial.NormalMapTextureScale, lilMaterial.NormalMapTextureOffset);
        }

        /// <inheritdoc/>
        internal override float GetReflectance()
        {
            return lilMaterial.Reflectance;
        }

        /// <inheritdoc/>
        internal override float GetRimAlbedoTint()
        {
            return lilMaterial.RimMainStrength;
        }

        /// <inheritdoc/>
        internal override Color GetRimColor()
        {
            var color = lilMaterial.RimLightColor;
            color.a = 1.0f;
            return color;
        }

        /// <inheritdoc/>
        internal override bool GetRimEnvironmental()
        {
            return lilMaterial.RimEnableLighting > 0.0f;
        }

        /// <inheritdoc/>
        internal override float GetRimIntensity()
        {
            var intensity = GetRimEnvironmental()
                ? 0.5f * lilMaterial.RimEnableLighting
                : 0.5f;
            intensity *= lilMaterial.RimLightColor.a;
            return intensity;
        }

        /// <inheritdoc/>
        internal override float GetRimRange()
        {
            return Mathf.Pow(1.0f - lilMaterial.RimLightBorder, lilMaterial.RimFresnelPower);
        }

        /// <inheritdoc/>
        internal override float GetRimSoftness()
        {
            return lilMaterial.RimLightBlur;
        }

        /// <inheritdoc/>
        internal override float GetSharpness()
        {
            return 1.0f - lilMaterial.SpecularBlur;
        }

        /// <inheritdoc/>
        internal override bool GetUseGlossMap()
        {
            if (!lilMaterial.UseReflection)
            {
                return false;
            }
            return lilMaterial.SmoothnessTex != null || lilMaterial.ReflectionColorTex != null || lilMaterial.ReflectionColor != Color.white;
        }

        /// <inheritdoc/>
        internal override bool GetUseMatcap()
        {
            return lilMaterial.UseMatCap;
        }

        /// <inheritdoc/>
        internal override bool GetUseMatcapMask()
        {
            return lilMaterial.UseMatCap && lilMaterial.MatCapMask != null;
        }

        /// <inheritdoc/>
        internal override bool GetUseMetallicMap()
        {
            if (!lilMaterial.UseReflection)
            {
                return false;
            }
            return lilMaterial.MetallicMap != null || lilMaterial.ReflectionColorTex != null || lilMaterial.ReflectionColor != Color.white;
        }

        /// <inheritdoc/>
        internal override (Vector2 Scale, Vector2 Offset) GetOcculusionMapST()
        {
            return (lilMaterial.AOMapTextureScale, lilMaterial.AOMapTextureOffset);
        }

        /// <inheritdoc/>
        internal override bool GetUseRimLighting()
        {
            return lilMaterial.UseRimLight;
        }

        /// <inheritdoc/>
        internal override bool GetUseShadowRamp()
        {
            return lilMaterial.UseShadow;
        }

        /// <inheritdoc/>
        internal override bool GetUseSpecular()
        {
            return lilMaterial.UseReflection;
        }
    }
}
