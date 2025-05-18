using System;
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
            newMaterial.MainColor = lilMaterial.Material.color;

            if (lilMaterial.UseNormalMap)
            {
                newMaterial.UseNormalMap = true;
                newMaterial.NormalMap = lilMaterial.NormalMap;
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
                newMaterial.EmissionColor = lilMaterial.EmissionColor;
            }

            if (lilMaterial.UseReflection)
            {
                newMaterial.UseSpecular = true;
                newMaterial.MetallicMap = lilMaterial.MetallicMap;
                newMaterial.MetallicStrength = lilMaterial.Metallic;

                newMaterial.GlossMap = lilMaterial.SmoothnessTex;
                newMaterial.GlossStrength = lilMaterial.Smoothness;
                newMaterial.Sharpness = 1.0f - lilMaterial.SpecularBlur;
            }

            if (lilMaterial.UseMatCap)
            {
                newMaterial.UseMatcap = true;
                newMaterial.Matcap = lilMaterial.MatCapTex;
                newMaterial.MatcapMask = lilMaterial.MatCapMask;
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
                newMaterial.RimColor = lilMaterial.RimLightColor;
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

            var targetSize = Math.Min(lilMaterial.EmissionMap.width, (int)settings.maxTextureSize);
            var rt = RenderTexture.GetTemporary(targetSize, targetSize, 0, RenderTextureFormat.ARGB32);
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
            var targetSize = Math.Min(gloss.width, (int)settings.maxTextureSize);

            var rt = RenderTexture.GetTemporary(gloss.width, gloss.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var mat = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));
            mat.SetTexture("_Texture0", gloss);
            mat.SetFloat("_Texture0Input", 0); // R
            mat.SetFloat("_Texture0Output", 3); // A
            mat.SetFloat("_Texture1Output", -1);
            mat.SetFloat("_Texture2Output", -1);
            mat.SetFloat("_Texture3Output", -1);

            Graphics.Blit(null, rt, mat);

            var rt2 = RenderTexture.GetTemporary(targetSize, targetSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
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
        protected override AsyncCallbackRequest GenerateMainTexture(Action<Texture2D> completion)
        {
            var rt = lilMaterial.BakeMain();
            var textureSize = Math.Min(rt.width, (int)settings.maxTextureSize);
            var rt2 = RenderTexture.GetTemporary(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
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
            mat.SetColor("_Texture0Color", lilMaterial.MatCapColor);

            var matcapSize = lilMaterial.MatCapTex ? lilMaterial.MatCapTex.width : 4;
            var targetSize = Math.Min(matcapSize, (int)settings.maxTextureSize);
            var rt = RenderTexture.GetTemporary(targetSize, targetSize, 0, RenderTextureFormat.ARGB32);
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
            var targetSize = Math.Min(matcapMask.width, (int)settings.maxTextureSize);

            var rt = RenderTexture.GetTemporary(matcapMask.width, matcapMask.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var mat = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));
            mat.SetTexture("_Texture0", matcapMask);
            mat.SetFloat("_Texture0Input", 4); // Grayscale
            mat.SetFloat("_Texture0Output", 0); // R
            mat.SetFloat("_Texture1Output", -1);
            mat.SetFloat("_Texture2Output", -1);
            mat.SetFloat("_Texture3Output", -1);

            Graphics.Blit(null, rt, mat);

            var rt2 = RenderTexture.GetTemporary(targetSize, targetSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
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
            var metallicSize = metallic ? metallic.width : 0;
            var reflectionColor = (Texture2D)lilMaterial.ReflectionColorTex;
            var reflectionColorSize = reflectionColor ? reflectionColor.width : 0;

            var originalSize = Math.Max(metallicSize, reflectionColorSize);
            var targetSize = Math.Min(originalSize, (int)settings.maxTextureSize);

            var rt0 = RenderTexture.GetTemporary(originalSize, originalSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var mat0 = new Material(Shader.Find("Hidden/VRCQuestTools/Swizzle"));
            mat0.SetTexture("_Texture0", reflectionColor);
            mat0.SetFloat("_Texture0Input", 4); // Grayscale
            mat0.SetFloat("_Texture0Output", 0); // R
            mat0.SetFloat("_Texture1Output", -1);
            mat0.SetFloat("_Texture2Output", -1);
            mat0.SetFloat("_Texture3Output", -1);
            Graphics.Blit(null, rt0, mat0);

            var rt = RenderTexture.GetTemporary(originalSize, originalSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var mat = new Material(Shader.Find("Hidden/VRCQuestTools/Multiply"));
            mat.SetTexture("_Texture0", rt0);
            mat.SetTexture("_Texture1", metallic);
            Graphics.Blit(null, rt, mat);

            var rt2 = RenderTexture.GetTemporary(targetSize, targetSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            TextureUtility.DownscaleBlit(rt, false, rt2);

            return TextureUtility.RequestReadbackRenderTexture(rt2, true, true, (tex) =>
            {
                RenderTexture.ReleaseTemporary(rt0);
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
            var targetSize = Math.Min(normal.width, (int)settings.maxTextureSize);
            var newTex = TextureUtility.DownscaleNormalMap(normal, outputRGB, targetSize, targetSize);
            return new ResultRequest<Texture2D>(newTex, completion);
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
        protected override Texture GetMatcap()
        {
            return lilMaterial.MatCapTex;
        }

        /// <inheritdoc/>
        protected override float GetMatcapMaskStrength()
        {
            return lilMaterial.MatCapBlend;
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
            return lilMaterial.RimLightColor;
        }

        /// <inheritdoc/>
        protected override bool GetRimEnvironmental()
        {
            return lilMaterial.RimEnableLighting > 0.0f;
        }

        /// <inheritdoc/>
        protected override float GetRimIntensity()
        {
            return GetRimEnvironmental()
                ? 0.5f * lilMaterial.RimEnableLighting
                : 0.5f;
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
            if (lilMaterial.UseEmission && lilMaterial.EmissionMap != null)
            {
                return true;
            }
            if (lilMaterial.UseEmission2nd && lilMaterial.Emission2ndMap != null)
            {
                return true;
            }
            return lilMaterial.UseEmission && lilMaterial.UseEmission2nd;
        }

        /// <inheritdoc/>
        protected override bool GetUseGlossMap()
        {
            return lilMaterial.UseReflection && lilMaterial.SmoothnessTex != null;
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
            return lilMaterial.UseReflection
                && (lilMaterial.MetallicMap != null || lilMaterial.ReflectionColorTex != null);
        }

        /// <inheritdoc/>
        protected override bool GetUseNormalMap()
        {
            return lilMaterial.UseNormalMap && lilMaterial.NormalMap != null;
        }

        /// <inheritdoc/>
        protected override bool GetUseRimLighting()
        {
            return lilMaterial.UseRimLight;
        }

        /// <inheritdoc/>
        protected override bool GetUseSpecular()
        {
            return lilMaterial.UseReflection;
        }
    }
}
