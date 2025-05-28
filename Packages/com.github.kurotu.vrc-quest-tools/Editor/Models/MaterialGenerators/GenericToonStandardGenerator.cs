using System;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Material generator for GenericToon to ToonStandard conversion.
    /// </summary>
    internal class GenericToonStandardGenerator : ToonStandardGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericToonStandardGenerator"/> class.
        /// </summary>
        /// <param name="settings">Convert settings.</param>
        public GenericToonStandardGenerator(ToonStandardConvertSettings settings)
            : base(settings)
        {
        }

        /// <inheritdoc/>
        protected override Material ConvertToToonStandard()
        {
            var newMaterial = new ToonStandardMaterialWrapper();
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
        protected override AsyncCallbackRequest GenerateMatcap(Action<Texture2D> completion)
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
        protected override AsyncCallbackRequest GenerateNormalMap(bool outputRGB, Action<Texture2D> completion)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override AsyncCallbackRequest GenerateOcclusionMap(Action<Texture2D> completion)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override AsyncCallbackRequest GeneratePackedMask(TexturePack pack, Action<Texture2D> completion)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override AsyncCallbackRequest GenerateShadowRamp(Action<Texture2D> completion)
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
        protected override (Vector2 Scale, Vector2 Offset) GetGlossMapST()
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
        protected override (Vector2 Scale, Vector2 Offset) GetMatcapMaskST()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override float GetMatcapMaskStrength()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override (Vector2 Scale, Vector2 Offset) GetMetallicMapST()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override float GetMetallicStrength()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override float GetMinBrightness()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override float GetNormalMapScale()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override (Vector2 Scale, Vector2 Offset) GetNormalMapST()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override (Vector2 Scale, Vector2 Offset) GetOcculusionMapST()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override float GetReflectance()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override float GetRimAlbedoTint()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override Color GetRimColor()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override bool GetRimEnvironmental()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override float GetRimIntensity()
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
        protected override bool GetUseOcclusionMap()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override bool GetUseRimLighting()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override bool GetUseShadowRamp()
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
