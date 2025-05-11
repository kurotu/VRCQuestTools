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
                newMaterial.ShadowRamp = ToonStandardMaterialWrapper.RampTexture.RealisticVerySoft;
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
                newMaterial.RimRange = Mathf.Pow(1.0f - lilMaterial.RimLightBorder, lilMaterial.RimFresnelPower);
                newMaterial.RimSoftness = lilMaterial.RimLightBlur;
            }

            return newMaterial;
        }

        /// <inheritdoc/>
        protected override AsyncCallbackRequest GenerateEmissionMap(Action<Texture2D> completion)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override AsyncCallbackRequest GenerateGlossMap(Action<Texture2D> completion)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override AsyncCallbackRequest GenerateMainTexture(Action<Texture2D> completion)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override AsyncCallbackRequest GenerateMatcapMask(Action<Texture2D> completion)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override AsyncCallbackRequest GenerateMetallicMap(Action<Texture2D> completion)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override AsyncCallbackRequest GenerateNormalMap(Action<Texture2D> completion)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override AsyncCallbackRequest GenerateShadowRamp(Action<Texture2D> completion)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override float GetAnisotropy()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override CullMode GetCulling()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override Color GetEmissionColor()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override float GetGlossStrength()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override Color GetMainColor()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override ToonStandardMaterialWrapper.MatcapTypeMode GetMapcapType()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override Texture GetMatcap()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override float GetMatcapMaskStrength()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override float GetMetallicStrength()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override float GetNormalMapScale()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override float GetReflectance()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override Color GetRimColor()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override float GetRimRange()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override float GetRimSoftness()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override float GetSharpness()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override bool GetUseEmissionMap()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override bool GetUseGlossMap()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override bool GetUseMainTexture()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override bool GetUseMatcap()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override bool GetUseMatcapMask()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override bool GetUseMetallicMap()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override bool GetUseNormalMap()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override bool GetUseRimLighting()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override bool GetUseSpecular()
        {
            throw new NotImplementedException();
        }
    }
}
