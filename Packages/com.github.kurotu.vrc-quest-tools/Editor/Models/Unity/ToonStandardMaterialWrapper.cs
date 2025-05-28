using System;
using KRT.VRCQuestTools.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Wrapper for Toon Standard material.
    /// </summary>
    internal class ToonStandardMaterialWrapper
    {
        private readonly Material material;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToonStandardMaterialWrapper"/> class.
        /// </summary>
        /// <param name="material">Material to wrap.</param>
        internal ToonStandardMaterialWrapper(Material material)
        {
            this.material = material;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToonStandardMaterialWrapper"/> class with a default material.
        /// </summary>
        internal ToonStandardMaterialWrapper()
        {
            material = new Material(Shader.Find("VRChat/Mobile/Toon Standard"));
        }

        /// <summary>
        /// UV map mode.
        /// </summary>
        internal enum UVMapMode
        {
            /// <summary>
            /// UV0.
            /// </summary>
            UV0 = 0,

            /// <summary>
            /// UV1.
            /// </summary>
            UV1 = 1,
        }

        /// <summary>
        /// Mask channel enumeration.
        /// </summary>
        internal enum MaskChannel
        {
            /// <summary>
            /// Red channel.
            /// </summary>
            R,

            /// <summary>
            /// Green channel.
            /// </summary>
            G,

            /// <summary>
            /// Blue channel.
            /// </summary>
            B,

            /// <summary>
            /// Alpha channel.
            /// </summary>
            A,
        }

        /// <summary>
        /// Detail map mode.
        /// </summary>
        internal enum DetailMapMode
        {
            /// <summary>
            /// Alpha blended detail map.
            /// </summary>
            AlphaBlended = 0,

            /// <summary>
            /// Additive detail map.
            /// </summary>
            Additive = 1,

            /// <summary>
            /// Multiply detail map.
            /// </summary>
            Multiply = 2,

            /// <summary>
            /// Masked detail map.
            /// </summary>
            Mask = 3,
        }

        /// <summary>
        /// Matcap type mode.
        /// </summary>
        internal enum MatcapTypeMode
        {
            /// <summary>
            /// Additive matcap.
            /// </summary>
            Additive = 0,

            /// <summary>
            /// Multiplicative matcap.
            /// </summary>
            Multiplicative = 1,
        }

        /// <summary>
        /// Gets or sets the material name.
        /// </summary>
        internal string Name
        {
            get => material.name;
            set => material.name = value;
        }

        /// <summary>
        /// Gets or sets the albedo texture.
        /// </summary>
        internal Texture MainTexture
        {
            get => material.GetTexture("_MainTex");
            set => material.SetTexture("_MainTex", value);
        }

        /// <summary>
        /// Gets or sets the albedo color.
        /// </summary>
        internal Color MainColor
        {
            get => material.GetColor("_Color");
            set => material.SetColor("_Color", value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use normal maps.
        /// </summary>
        internal bool UseNormalMap
        {
            get => material.IsKeywordEnabled("USE_NORMAL_MAPS");
            set => SetKeyword("USE_NORMAL_MAPS", value);
        }

        /// <summary>
        /// Gets or sets the normal map texture.
        /// </summary>
        internal Texture NormalMap
        {
            get => material.GetTexture("_BumpMap");
            set => material.SetTexture("_BumpMap", value);
        }

        /// <summary>
        /// Gets or sets the normal map texture scale.
        /// </summary>
        internal Vector2 NormalMapTextureScale
        {
            get => material.GetTextureScale("_BumpMap");
            set => material.SetTextureScale("_BumpMap", value);
        }

        /// <summary>
        /// Gets or sets the normal map texture offset.
        /// </summary>
        internal Vector2 NormalMapTextureOffset
        {
            get => material.GetTextureOffset("_BumpMap");
            set => material.SetTextureOffset("_BumpMap", value);
        }

        /// <summary>
        /// Gets or sets the normal map scale.
        /// </summary>
        internal float NormalMapScale
        {
            get => material.GetFloat("_BumpScale");
            set => material.SetFloat("_BumpScale", value);
        }

        /// <summary>
        /// Gets or sets the culling mode.
        /// </summary>
        internal CullMode Culling
        {
            get => (CullMode)material.GetFloat("_Culling");
            set => material.SetFloat("_Culling", (float)value);
        }

        /// <summary>
        /// Gets or sets shadow ramp texture.
        /// </summary>
        internal Texture ShadowRamp
        {
            get => material.GetTexture("_Ramp");
            set => material.SetTexture("_Ramp", value);
        }

        /// <summary>
        /// Gets or sets the shadow boost.
        /// </summary>
        internal float ShadowBoost
        {
            get => material.GetFloat("_ShadowBoost");
            set => material.SetFloat("_ShadowBoost", value);
        }

        /// <summary>
        /// Gets or sets the shadow tint.
        /// </summary>
        internal float ShadowTint
        {
            get => material.GetFloat("_ShadowAlbedo");
            set => material.SetFloat("_ShadowAlbedo", value);
        }

        /// <summary>
        /// Gets or sets the minimum brightness.
        /// </summary>
        internal float MinBrightness
        {
            get => material.GetFloat("_MinBrightness");
            set => material.SetFloat("_MinBrightness", value);
        }

        /// <summary>
        /// Gets or sets the emission map texture.
        /// </summary>
        internal Texture EmissionMap
        {
            get => material.GetTexture("_EmissionMap");
            set => material.SetTexture("_EmissionMap", value);
        }

        /// <summary>
        /// Gets or sets the emission color.
        /// </summary>
        internal Color EmissionColor
        {
            get => material.GetColor("_EmissionColor");
            set => material.SetColor("_EmissionColor", value);
        }

        /// <summary>
        /// Gets or sets the emission UV map.
        /// </summary>
        internal UVMapMode EmissionUVMap
        {
            get => (UVMapMode)material.GetFloat("_EmissionUV");
            set => material.SetFloat("_EmissionUV", (float)value);
        }

        /// <summary>
        /// Gets or sets the emission strength.
        /// </summary>
        internal float EmissionStrength
        {
            get => material.GetFloat("_EmissionStrength");
            set => material.SetFloat("_EmissionStrength", value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use occlusion.
        /// </summary>
        internal bool UseOcclusion
        {
            get => material.IsKeywordEnabled("USE_OCCLUSION_MAP");
            set => SetKeyword("USE_OCCLUSION_MAP", value);
        }

        /// <summary>
        /// Gets or sets the occlusion map texture.
        /// </summary>
        internal Texture OcclusionMap
        {
            get => material.GetTexture("_OcclusionMap");
            set => material.SetTexture("_OcclusionMap", value);
        }

        /// <summary>
        /// Gets or sets the occlusion map texture scale.
        /// </summary>
        internal Vector2 OcclusionMapTextureScale
        {
            get => material.GetTextureScale("_OcclusionMap");
            set => material.SetTextureScale("_OcclusionMap", value);
        }

        /// <summary>
        /// Gets or sets the occlusion map texture offset.
        /// </summary>
        internal Vector2 OcclusionMapTextureOffset
        {
            get => material.GetTextureOffset("_OcclusionMap");
            set => material.SetTextureOffset("_OcclusionMap", value);
        }

        /// <summary>
        /// Gets or sets the occlusion map channel.
        /// </summary>
        internal MaskChannel OcclusionMapChannel
        {
            get => (MaskChannel)material.GetFloat("_OcclusionMapChannel");
            set => material.SetFloat("_OcclusionMapChannel", (float)value);
        }

        /// <summary>
        /// Gets or sets the occlusion strength.
        /// </summary>
        internal float OcclusionStrength
        {
            get => material.GetFloat("_OcclusionStrength");
            set => material.SetFloat("_OcclusionStrength", value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use detail maps.
        /// </summary>
        internal bool UseDetail
        {
            get => material.IsKeywordEnabled("USE_DETAIL_MAPS");
            set => SetKeyword("USE_DETAIL_MAPS", value);
        }

        /// <summary>
        /// Gets or sets the detail map mode.
        /// </summary>
        internal DetailMapMode DetailMode
        {
            get => (DetailMapMode)material.GetFloat("_DetailMode");
            set => material.SetFloat("_DetailMode", (float)value);
        }

        /// <summary>
        /// Gets or sets the detail mask.
        /// </summary>
        internal Texture DetailMask
        {
            get => material.GetTexture("_DetailMask");
            set => material.SetTexture("_DetailMask", value);
        }

        /// <summary>
        /// Gets or sets the detail mask channel.
        /// </summary>
        internal MaskChannel DetailMaskChannel
        {
            get => (MaskChannel)material.GetFloat("_DetailMaskChannel");
            set => material.SetFloat("_DetailMaskChannel", (float)value);
        }

        /// <summary>
        /// Gets or sets the detail map texture.
        /// </summary>
        internal Texture DetailTexture
        {
            get => material.GetTexture("_DetailAlbedoMap");
            set => material.SetTexture("_DetailAlbedoMap", value);
        }

        /// <summary>
        /// Gets or sets the detail normal map texture.
        /// </summary>
        internal Texture DetailNormalMap
        {
            get => material.GetTexture("_DetailNormalMap");
            set => material.SetTexture("_DetailNormalMap", value);
        }

        /// <summary>
        /// Gets or sets the detail normal map scale.
        /// </summary>
        internal float DetailNormalMapScale
        {
            get => material.GetFloat("_DetailNormalMapScale");
            set => material.SetFloat("_DetailNormalMapScale", value);
        }

        /// <summary>
        /// Gets or sets the detail UV map mode.
        /// </summary>
        internal UVMapMode DetailUVMap
        {
            get => (UVMapMode)material.GetFloat("_DetailUV");
            set => material.SetFloat("_DetailUV", (float)value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use specular.
        /// </summary>
        internal bool UseSpecular
        {
            get => material.IsKeywordEnabled("USE_SPECULAR");
            set => SetKeyword("USE_SPECULAR", value);
        }

        /// <summary>
        /// Gets or sets the metallic map texture.
        /// </summary>
        internal Texture MetallicMap
        {
            get => material.GetTexture("_MetallicMap");
            set => material.SetTexture("_MetallicMap", value);
        }

        /// <summary>
        /// Gets or sets the metallic map texture scale.
        /// </summary>
        internal Vector2 MetallicMapTextureScale
        {
            get => material.GetTextureScale("_MetallicMap");
            set => material.SetTextureScale("_MetallicMap", value);
        }

        /// <summary>
        /// Gets or sets the metallic map texture offset.
        /// </summary>
        internal Vector2 MetallicMapTextureOffset
        {
            get => material.GetTextureOffset("_MetallicMap");
            set => material.SetTextureOffset("_MetallicMap", value);
        }

        /// <summary>
        /// Gets or sets the metallic map channel.
        /// </summary>
        internal MaskChannel MetallicMapChannel
        {
            get => (MaskChannel)material.GetFloat("_MetallicMapChannel");
            set => material.SetFloat("_MetallicMapChannel", (float)value);
        }

        /// <summary>
        /// Gets or sets the metallic strength.
        /// </summary>
        internal float MetallicStrength
        {
            get => material.GetFloat("_MetallicStrength");
            set => material.SetFloat("_MetallicStrength", value);
        }

        /// <summary>
        /// Gets or sets the gloss map texture.
        /// </summary>
        internal Texture GlossMap
        {
            get => material.GetTexture("_GlossMap");
            set => material.SetTexture("_GlossMap", value);
        }

        /// <summary>
        /// Gets or sets the gloss map texture scale.
        /// </summary>
        internal Vector2 GlossMapTextureScale
        {
            get => material.GetTextureScale("_GlossMap");
            set => material.SetTextureScale("_GlossMap", value);
        }

        /// <summary>
        /// Gets or sets the gloss map texture offset.
        /// </summary>
        internal Vector2 GlossMapTextureOffset
        {
            get => material.GetTextureOffset("_GlossMap");
            set => material.SetTextureOffset("_GlossMap", value);
        }

        /// <summary>
        /// Gets or sets the gloss map channel.
        /// </summary>
        internal MaskChannel GlossMapChannel
        {
            get => (MaskChannel)material.GetFloat("_GlossMapChannel");
            set => material.SetFloat("_GlossMapChannel", (float)value);
        }

        /// <summary>
        /// Gets or sets the gloss strength.
        /// </summary>
        internal float GlossStrength
        {
            get => material.GetFloat("_GlossStrength");
            set => material.SetFloat("_GlossStrength", value);
        }

        /// <summary>
        /// Gets or sets the specular sharpness.
        /// </summary>
        internal float Sharpness
        {
            get => material.GetFloat("_SpecularSharpness");
            set => material.SetFloat("_SpecularSharpness", value);
        }

        /// <summary>
        /// Gets or sets the specular reflectance.
        /// </summary>
        internal float Reflectance
        {
            get => material.GetFloat("_Reflectance");
            set => material.SetFloat("_Reflectance", value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use matcap.
        /// </summary>
        internal bool UseMatcap
        {
            get => material.IsKeywordEnabled("USE_MATCAP");
            set => SetKeyword("USE_MATCAP", value);
        }

        /// <summary>
        /// Gets or sets the matcap texture.
        /// </summary>
        internal Texture Matcap
        {
            get => material.GetTexture("_Matcap");
            set => material.SetTexture("_Matcap", value);
        }

        /// <summary>
        /// Gets or sets the matcap mask texture.
        /// </summary>
        internal Texture MatcapMask
        {
            get => material.GetTexture("_MatcapMask");
            set => material.SetTexture("_MatcapMask", value);
        }

        /// <summary>
        /// Gets or sets the matcap mask texture scale.
        /// </summary>
        internal Vector2 MatcapMaskTextureScale
        {
            get => material.GetTextureScale("_MatcapMask");
            set => material.SetTextureScale("_MatcapMask", value);
        }

        /// <summary>
        /// Gets or sets the matcap mask texture offset.
        /// </summary>
        internal Vector2 MatcapMaskTextureOffset
        {
            get => material.GetTextureOffset("_MatcapMask");
            set => material.SetTextureOffset("_MatcapMask", value);
        }

        /// <summary>
        /// Gets or sets the matcap mask channel.
        /// </summary>
        internal MaskChannel MatcapMaskChannel
        {
            get => (MaskChannel)material.GetFloat("_MatcapMaskChannel");
            set => material.SetFloat("_MatcapMaskChannel", (float)value);
        }

        /// <summary>
        /// Gets or sets the matcap strength.
        /// </summary>
        internal float MatcapStrength
        {
            get => material.GetFloat("_MatcapStrength");
            set => material.SetFloat("_MatcapStrength", value);
        }

        /// <summary>
        /// Gets or sets the matcap type mode.
        /// </summary>
        internal MatcapTypeMode MatcapType
        {
            get => (MatcapTypeMode)material.GetFloat("_MatcapType");
            set => material.SetFloat("_MatcapType", (float)value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether whether to use rim lighting.
        /// </summary>
        internal bool UseRimLighting
        {
            get => material.IsKeywordEnabled("USE_RIMLIGHT");
            set => SetKeyword("USE_RIMLIGHT", value);
        }

        /// <summary>
        /// Gets or sets the rim lighting color.
        /// </summary>
        internal Color RimColor
        {
            get => material.GetColor("_RimColor");
            set => material.SetColor("_RimColor", value);
        }

        /// <summary>
        /// Gets or sets the rim lighting albedo tint.
        /// </summary>
        internal float RimAlbedoTint
        {
            get => material.GetFloat("_RimAlbedoTint");
            set => material.SetFloat("_RimAlbedoTint", value);
        }

        /// <summary>
        /// Gets or sets the rim lighting intensity.
        /// </summary>
        internal float RimIntensity
        {
            get => material.GetFloat("_RimIntensity");
            set => material.SetFloat("_RimIntensity", value);
        }

        /// <summary>
        /// Gets or sets the rim lighting range.
        /// </summary>
        internal float RimRange
        {
            get => material.GetFloat("_RimRange");
            set => material.SetFloat("_RimRange", value);
        }

        /// <summary>
        /// Gets or sets the rim lighting softness.
        /// </summary>
        internal float RimSoftness
        {
            get => material.GetFloat("_RimSharpness");
            set => material.SetFloat("_RimSharpness", value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use rim environmental lighting.
        /// </summary>
        internal bool RimEnvironmental
        {
            get => material.GetFloat("_RimEnvironmental") > 0.5f;
            set => material.SetFloat("_RimEnvironmental", value ? 1.0f : 0.0f);
        }

        /// <summary>
        /// Implicitly converts a ToonStandardMaterialWrapper to a Material.
        /// </summary>
        /// <param name="m">This material wrapper.</param>
        public static implicit operator Material(ToonStandardMaterialWrapper m) => m.material;

        private void SetKeyword(string keyword, bool value)
        {
            if (value)
            {
                material.EnableKeyword(keyword);
            }
            else
            {
                material.DisableKeyword(keyword);
            }
        }

        /// <summary>
        /// Ramp texture.
        /// </summary>
        internal static class RampTexture
        {
            private static Lazy<Texture2D> flat = new Lazy<Texture2D>(() => AssetUtility.LoadAssetByGUID<Texture2D>("8ed41581528c4fa4fa11970aca4edb8d"));
            private static Lazy<Texture2D> realistic = new Lazy<Texture2D>(() => AssetUtility.LoadAssetByGUID<Texture2D>("348500adef1d2da428abc7b720b8b699"));
            private static Lazy<Texture2D> realisticSoft = new Lazy<Texture2D>(() => AssetUtility.LoadAssetByGUID<Texture2D>("636cf1b5dfca6f54b94ca3d2ff8216c9"));
            private static Lazy<Texture2D> realisticVerySoft = new Lazy<Texture2D>(() => AssetUtility.LoadAssetByGUID<Texture2D>("5f304bf7a07313d43b8562d9eabce646"));
            private static Lazy<Texture2D> toon2band = new Lazy<Texture2D>(() => AssetUtility.LoadAssetByGUID<Texture2D>("dfafc89321615114fb6dbecdba0c8214"));
            private static Lazy<Texture2D> toon3band = new Lazy<Texture2D>(() => AssetUtility.LoadAssetByGUID<Texture2D>("5d1b50be612cf1248b6e101f8d1c5b53"));
            private static Lazy<Texture2D> toon4band = new Lazy<Texture2D>(() => AssetUtility.LoadAssetByGUID<Texture2D>("0d6a7a9ec31ab7448a777f0e2daef4af"));

            /// <summary>
            /// Gets the flat ramp texture.
            /// </summary>
            internal static Texture2D Flat => flat.Value;

            /// <summary>
            /// Gets the realistic ramp texture.
            /// </summary>
            internal static Texture2D Realistic => realistic.Value;

            /// <summary>
            /// Gets the realistic soft ramp texture.
            /// </summary>
            internal static Texture2D RealisticSoft => realisticSoft.Value;

            /// <summary>
            /// Gets the realistic very soft ramp texture.
            /// </summary>
            internal static Texture2D RealisticVerySoft => realisticVerySoft.Value;

            /// <summary>
            /// Gets the 2 band toon ramp texture.
            /// </summary>
            internal static Texture2D Toon2Band => toon2band.Value;

            /// <summary>
            /// Gets the 3 band toon ramp texture.
            /// </summary>
            internal static Texture2D Toon3Band => toon3band.Value;

            /// <summary>
            /// Gets the 4 band toon ramp texture.
            /// </summary>
            internal static Texture2D Toon4Band => toon4band.Value;
        }
    }
}
