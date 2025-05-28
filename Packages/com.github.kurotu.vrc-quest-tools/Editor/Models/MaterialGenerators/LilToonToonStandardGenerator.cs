using System;
using System.Linq;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Material generator for LilToon to ToonStandard conversion.
    /// </summary>
    internal class LilToonToonStandardGenerator : ToonStandardGenerator
    {
        private readonly LilToonMaterial lilMaterial;

        /// <summary>
        /// Initializes a new instance of the <see cref="LilToonToonStandardGenerator"/> class.
        /// </summary>
        /// <param name="material">LilToon material.</param>
        /// <param name="settings">Convert settings.</param>
        public LilToonToonStandardGenerator(LilToonMaterial material, ToonStandardConvertSettings settings)
            : base(settings)
        {
            this.lilMaterial = material;
        }

        /// <inheritdoc/>
        protected override Material ConvertToToonStandard()
        {
            var newMaterial = new ToonStandardMaterialWrapper();

            newMaterial.Name = lilMaterial.Material.name;

            newMaterial.MainTexture = lilMaterial.Material.mainTexture;
            newMaterial.MainColor = Utils.ColorUtility.HdrToLdr(lilMaterial.Material.color);

            if (lilMaterial.UseNormalMap)
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
                newMaterial.ShadowRamp = settings.fallbackShadowRamp;
            }
            else
            {
                newMaterial.ShadowRamp = ToonStandardMaterialWrapper.RampTexture.Flat;
            }

            newMaterial.MinBrightness = lilMaterial.LightMinLimit;

            if (lilMaterial.UseEmission)
            {
                newMaterial.EmissionMap = lilMaterial.EmissionMap;
                newMaterial.EmissionColor = Utils.ColorUtility.HdrToLdr(lilMaterial.EmissionColor);
            }

            if (lilMaterial.UseShadow && lilMaterial.AOMap != null)
            {
                newMaterial.UseOcclusion = true;
                newMaterial.OcclusionMap = lilMaterial.AOMap;
            }

            if (lilMaterial.UseReflection)
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

            if (lilMaterial.UseMatCap)
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

            if (lilMaterial.UseRimLight)
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

        /// <inheritdoc/>
        protected override AsyncCallbackRequest GenerateEmissionMap(Action<Texture2D> completion)
        {
            var bakeMat = new Material(lilMaterial.Material);
#if UNITY_2022_1_OR_NEWER
            bakeMat.parent = null;
#endif
            bakeMat.shader = Shader.Find("Hidden/VRCQuestTools/lilToon/Emission");
            var bakeMatWrapper = new LilToonMaterial(bakeMat);
            bakeMatWrapper.EmissionColor = Utils.ColorUtility.HdrToLdr(bakeMatWrapper.EmissionColor);
            bakeMatWrapper.Emission2ndColor = Utils.ColorUtility.HdrToLdr(bakeMatWrapper.Emission2ndColor);

            var sourceWidth = Mathf.Max(
                lilMaterial.EmissionMap ? lilMaterial.EmissionMap.width : 4,
                lilMaterial.EmissionBlendMask ? lilMaterial.EmissionBlendMask.width : 4,
                lilMaterial.Emission2ndMap ? lilMaterial.Emission2ndMap.width : 4,
                lilMaterial.Emission2ndBlendMask ? lilMaterial.Emission2ndBlendMask.width : 4);
            var sourceHeight = Mathf.Max(
                lilMaterial.EmissionMap ? lilMaterial.EmissionMap.height : 4,
                lilMaterial.EmissionBlendMask ? lilMaterial.EmissionBlendMask.height : 4,
                lilMaterial.Emission2ndMap ? lilMaterial.Emission2ndMap.height : 4,
                lilMaterial.Emission2ndBlendMask ? lilMaterial.Emission2ndBlendMask.height : 4);

            var (targetWidth, targetHeight) = TextureUtility.AspectFitReduction(sourceWidth, sourceHeight, (int)settings.maxTextureSize);
            var rt = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(lilMaterial.EmissionMap, rt, bakeMat);
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
            var gloss = (Texture2D)lilMaterial.SmoothnessTex;
            var reflectionColorTex = (Texture2D)lilMaterial.ReflectionColorTex;
            var reflectionColorTexWidth = reflectionColorTex ? reflectionColorTex.width : 4;
            var reflectionColorTexHeight = reflectionColorTex ? reflectionColorTex.height : 4;
            var sourceWidth = Math.Max(gloss ? gloss.width : 4, reflectionColorTexWidth);
            var sourceHeight = Math.Max(gloss ? gloss.height : 4, reflectionColorTexHeight);
            var (targetWidth, targetHeight) = TextureUtility.AspectFitReduction(sourceWidth, sourceHeight, (int)settings.maxTextureSize);

            var alphaGlossMap = RenderTexture.GetTemporary(sourceWidth, sourceHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var mat = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));
            mat.SetTexture("_Texture0", gloss);
            mat.SetFloat("_Texture0Input", 0); // R
            mat.SetFloat("_Texture0Output", 3); // A
            Graphics.Blit(null, alphaGlossMap, mat);

            var reflectionGrayscaleTex = RenderTexture.GetTemporary(reflectionColorTexWidth, reflectionColorTexHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var mat0 = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));
            mat0.SetTexture("_Texture0", reflectionColorTex);
            mat0.SetFloat("_Texture0Input", 4); // Grayscale
            mat0.SetFloat("_Texture0Output", 3); // A
            Graphics.Blit(null, reflectionGrayscaleTex, mat0);

            var rt1 = RenderTexture.GetTemporary(sourceWidth, sourceHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var mat1 = new Material(Shader.Find("Hidden/VRCQuestTools/Multiply"));
            mat1.SetTexture("_Texture0", alphaGlossMap);
            var reflectionColorGrayscale = Utils.ColorUtility.GetRec709Grayscale(lilMaterial.ReflectionColor) * lilMaterial.ReflectionColor.a;
            var reflectionColor = new Color(1.0f, 1.0f, 1.0f, reflectionColorGrayscale);
            mat1.SetColor("_Texture0Color", reflectionColor);
            mat1.SetTexture("_Texture1", reflectionGrayscaleTex);
            Graphics.Blit(null, rt1, mat1);

            var rt2 = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            TextureUtility.DownscaleBlit(rt1, false, rt2);

            return TextureUtility.RequestReadbackRenderTexture(rt2, true, true, (tex) =>
            {
                RenderTexture.ReleaseTemporary(alphaGlossMap);
                RenderTexture.ReleaseTemporary(rt1);
                RenderTexture.ReleaseTemporary(rt2);
                UnityEngine.Object.DestroyImmediate(mat);
                UnityEngine.Object.DestroyImmediate(mat1);
                completion?.Invoke(tex);
            });
        }

        /// <inheritdoc/>
        protected override AsyncCallbackRequest GenerateMainTexture(Action<Texture2D> completion)
        {
            var rt = lilMaterial.BakeMain();
            var (width, height) = TextureUtility.AspectFitReduction(rt.width, rt.height, (int)settings.maxTextureSize);
            var rt2 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            TextureUtility.DownscaleBlit(rt, true, rt2);
            return TextureUtility.RequestReadbackRenderTexture(rt2, true, (tex) =>
            {
                UnityEngine.Object.DestroyImmediate(rt);
                RenderTexture.ReleaseTemporary(rt2);
                completion?.Invoke(tex);
            });
        }

        /// <inheritdoc/>
        protected override AsyncCallbackRequest GenerateMatcap(Action<Texture2D> completion)
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
            var (width, height) = TextureUtility.AspectFitReduction(matcapWidth, matcapHeight, (int)settings.maxTextureSize);
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
            var matcapMask = (Texture2D)lilMaterial.MatCapMask;
            var (width, height) = TextureUtility.AspectFitReduction(matcapMask.width, matcapMask.height, (int)settings.maxTextureSize);

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
        protected override AsyncCallbackRequest GenerateMetallicMap(Action<Texture2D> completion)
        {
            var metallic = (Texture2D)lilMaterial.MetallicMap;
            var metallicWidth = metallic ? metallic.width : 4;
            var metallicHeight = metallic ? metallic.height : 4;
            var reflectionColorTex = (Texture2D)lilMaterial.ReflectionColorTex;
            var reflectionColorWidth = reflectionColorTex ? reflectionColorTex.width : 4;
            var reflectionColorHeight = reflectionColorTex ? reflectionColorTex.height : 4;

            var originalWidth = Math.Max(metallicWidth, reflectionColorWidth);
            var originalHeight = Math.Max(metallicHeight, reflectionColorHeight);
            var (width, height) = TextureUtility.AspectFitReduction(originalWidth, originalHeight, (int)settings.maxTextureSize);

            var reflectionGrayscaleTex = RenderTexture.GetTemporary(reflectionColorWidth, reflectionColorHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var mat0 = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));
            mat0.SetTexture("_Texture0", reflectionColorTex);
            mat0.SetFloat("_Texture0Input", 4); // Grayscale
            mat0.SetFloat("_Texture0Output", 0); // R
            Graphics.Blit(null, reflectionGrayscaleTex, mat0);

            var rt = RenderTexture.GetTemporary(originalWidth, originalHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var mat = new Material(Shader.Find("Hidden/VRCQuestTools/Multiply"));
            var reflectionGrayscale = Utils.ColorUtility.GetRec709Grayscale(lilMaterial.ReflectionColor) * lilMaterial.ReflectionColor.a;
            var reflectionColor = new Color(reflectionGrayscale, 1.0f, 1.0f, 1.0f);
            mat.SetTexture("_Texture0", reflectionGrayscaleTex);
            mat.SetColor("_Texture0Color", reflectionColor);
            mat.SetTexture("_Texture1", metallic);
            Graphics.Blit(null, rt, mat);

            var rt2 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            TextureUtility.DownscaleBlit(rt, false, rt2);

            return TextureUtility.RequestReadbackRenderTexture(rt2, true, true, (tex) =>
            {
                RenderTexture.ReleaseTemporary(reflectionGrayscaleTex);
                RenderTexture.ReleaseTemporary(rt);
                RenderTexture.ReleaseTemporary(rt2);
                UnityEngine.Object.DestroyImmediate(mat);
                completion?.Invoke(tex);
            });
        }

        /// <inheritdoc/>
        protected override AsyncCallbackRequest GenerateNormalMap(bool outputRGB, Action<Texture2D> completion)
        {
            var normal = (Texture2D)lilMaterial.NormalMap;
            var (width, height) = TextureUtility.AspectFitReduction(normal.width, normal.height, (int)settings.maxTextureSize);
            var newTex = TextureUtility.DownscaleNormalMap(normal, outputRGB, width, height);
            return new ResultRequest<Texture2D>(newTex, completion);
        }

        /// <inheritdoc/>
        protected override AsyncCallbackRequest GenerateOcclusionMap(Action<Texture2D> completion)
        {
            var aoMap = (Texture2D)lilMaterial.AOMap;
            var (width, height) = TextureUtility.AspectFitReduction(aoMap.width, aoMap.height, (int)settings.maxTextureSize);

            var swizzleMat = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));
            swizzleMat.SetTexture("_Texture0", aoMap);
            if (lilMaterial.UseShadow3rd)
            {
                swizzleMat.SetFloat("_Texture0Input", 2);
            }
            else if (lilMaterial.UseShadow2nd)
            {
                swizzleMat.SetFloat("_Texture0Input", 1);
            }
            else
            {
                swizzleMat.SetFloat("_Texture0Input", 0);
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
        protected override AsyncCallbackRequest GeneratePackedMask(TexturePack pack, Action<Texture2D> completion)
        {
            var swizzleMat = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));

            int maxWidth = 0;
            int maxHeight = 0;
            foreach (var (mask, index) in pack.GetMasks().Select((mask, index) => (mask, index)))
            {
                switch (mask.MaskType)
                {
                    case MaskType.None:
                        break;
                    case MaskType.DetailMask:
                        break;
                    case MaskType.MetallicMap:
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
                    case MaskType.GlossMap:
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

            var width = Math.Min(maxWidth, (int)settings.maxTextureSize);
            var height = Math.Min(maxHeight, (int)settings.maxTextureSize);
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
            var material = new Material(lilMaterial.Material);
#if UNITY_2022_1_OR_NEWER
            material.parent = null;
#endif

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
        protected override CullMode GetCulling()
        {
            return lilMaterial.CullMode;
        }

        /// <inheritdoc/>
        protected override Color GetEmissionColor()
        {
            if (lilMaterial.UseEmission && lilMaterial.UseEmission2nd)
            {
                return Color.white;
            }
            if (lilMaterial.UseEmission)
            {
                return lilMaterial.EmissionColor;
            }
            if (lilMaterial.UseEmission2nd)
            {
                return lilMaterial.Emission2ndColor;
            }
            return Color.black;
        }

        /// <inheritdoc/>
        protected override (Vector2 Scale, Vector2 Offset) GetGlossMapST()
        {
            return (lilMaterial.SmoothnessTexScale, lilMaterial.SmoothnessTexOffset);
        }

        /// <inheritdoc/>
        protected override float GetGlossStrength()
        {
            return lilMaterial.Smoothness;
        }

        /// <inheritdoc/>
        protected override Color GetMainColor()
        {
            return lilMaterial.Material.color;
        }

        /// <inheritdoc/>
        protected override ToonStandardMaterialWrapper.MatcapTypeMode GetMapcapType()
        {
            switch (lilMaterial.MatCapBlendingMode)
            {
                case LilToonMaterial.MatCapBlendMode.Add:
                case LilToonMaterial.MatCapBlendMode.Screen:
                    return ToonStandardMaterialWrapper.MatcapTypeMode.Additive;
                case LilToonMaterial.MatCapBlendMode.Normal:
                case LilToonMaterial.MatCapBlendMode.Multiply:
                    return ToonStandardMaterialWrapper.MatcapTypeMode.Multiplicative;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc/>
        protected override Texture GetMatcap()
        {
            return lilMaterial.MatCapTex;
        }

        /// <inheritdoc/>
        protected override (Vector2 Scale, Vector2 Offset) GetMatcapMaskST()
        {
            return (lilMaterial.MatCapMaskTextureScale, lilMaterial.MatCapMaskTextureOffset);
        }

        /// <inheritdoc/>
        protected override float GetMatcapMaskStrength()
        {
            return lilMaterial.MatCapBlend * lilMaterial.MatCapColor.a;
        }

        /// <inheritdoc/>
        protected override (Vector2 Scale, Vector2 Offset) GetMetallicMapST()
        {
            return (lilMaterial.MetallicMapTextureScale, lilMaterial.MetallicMapTextureOffset);
        }

        /// <inheritdoc/>
        protected override float GetMetallicStrength()
        {
            return lilMaterial.Metallic;
        }

        /// <inheritdoc/>
        protected override float GetMinBrightness()
        {
            return lilMaterial.LightMinLimit;
        }

        /// <inheritdoc/>
        protected override (Vector2 Scale, Vector2 Offset) GetNormalMapST()
        {
            return (lilMaterial.NormalMapTextureScale, lilMaterial.NormalMapTextureOffset);
        }

        /// <inheritdoc/>
        protected override float GetNormalMapScale()
        {
            return lilMaterial.NormalMapScale;
        }

        /// <inheritdoc/>
        protected override float GetReflectance()
        {
            return lilMaterial.Reflectance;
        }

        /// <inheritdoc/>
        protected override float GetRimAlbedoTint()
        {
            return lilMaterial.RimMainStrength;
        }

        /// <inheritdoc/>
        protected override Color GetRimColor()
        {
            var color = lilMaterial.RimLightColor;
            color.a = 1.0f;
            return color;
        }

        /// <inheritdoc/>
        protected override bool GetRimEnvironmental()
        {
            return lilMaterial.RimEnableLighting > 0.0f;
        }

        /// <inheritdoc/>
        protected override float GetRimIntensity()
        {
            var intensity = GetRimEnvironmental()
                ? 0.5f * lilMaterial.RimEnableLighting
                : 0.5f;
            intensity *= lilMaterial.RimLightColor.a;
            return intensity;
        }

        /// <inheritdoc/>
        protected override float GetRimRange()
        {
            return Mathf.Pow(1.0f - lilMaterial.RimLightBorder, lilMaterial.RimFresnelPower);
        }

        /// <inheritdoc/>
        protected override float GetRimSoftness()
        {
            return lilMaterial.RimLightBlur;
        }

        /// <inheritdoc/>
        protected override float GetSharpness()
        {
            return 1.0f - lilMaterial.SpecularBlur;
        }

        /// <inheritdoc/>
        protected override bool GetUseEmissionMap()
        {
            if (lilMaterial.UseEmission && (lilMaterial.EmissionMap != null || lilMaterial.EmissionBlendMask != null || lilMaterial.EmissionBlend < 1.0f))
            {
                return true;
            }
            if (lilMaterial.UseEmission2nd && (lilMaterial.Emission2ndMap != null || lilMaterial.Emission2ndBlendMask != null || lilMaterial.Emission2ndBlend < 1.0f))
            {
                return true;
            }
            return lilMaterial.UseEmission && lilMaterial.UseEmission2nd;
        }

        /// <inheritdoc/>
        protected override bool GetUseGlossMap()
        {
            if (!lilMaterial.UseReflection)
            {
                return false;
            }
            return lilMaterial.SmoothnessTex != null || lilMaterial.ReflectionColorTex != null || lilMaterial.ReflectionColor != Color.white;
        }

        /// <inheritdoc/>
        protected override bool GetUseMainTexture()
        {
            return lilMaterial.Material.mainTexture != null;
        }

        /// <inheritdoc/>
        protected override bool GetUseMatcap()
        {
            return lilMaterial.UseMatCap;
        }

        /// <inheritdoc/>
        protected override bool GetUseMatcapMask()
        {
            return lilMaterial.UseMatCap && lilMaterial.MatCapMask != null;
        }

        /// <inheritdoc/>
        protected override bool GetUseMetallicMap()
        {
            if (!lilMaterial.UseReflection)
            {
                return false;
            }
            return lilMaterial.MetallicMap != null || lilMaterial.ReflectionColorTex != null || lilMaterial.ReflectionColor != Color.white;
        }

        /// <inheritdoc/>
        protected override bool GetUseNormalMap()
        {
            return lilMaterial.UseNormalMap && lilMaterial.NormalMap != null;
        }

        /// <inheritdoc/>
        protected override bool GetUseOcclusionMap()
        {
            return lilMaterial.UseShadow && lilMaterial.AOMap != null;
        }

        /// <inheritdoc/>
        protected override (Vector2 Scale, Vector2 Offset) GetOcculusionMapST()
        {
            return (lilMaterial.AOMapTextureScale, lilMaterial.AOMapTextureOffset);
        }

        /// <inheritdoc/>
        protected override bool GetUseRimLighting()
        {
            return lilMaterial.UseRimLight;
        }

        /// <inheritdoc/>
        protected override bool GetUseShadowRamp()
        {
            return lilMaterial.UseShadow;
        }

        /// <inheritdoc/>
        protected override bool GetUseSpecular()
        {
            return lilMaterial.UseReflection;
        }
    }
}
