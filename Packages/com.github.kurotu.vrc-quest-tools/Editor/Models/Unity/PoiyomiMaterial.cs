// <copyright file="PoiyomiMaterial.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEngine;
using UnityEngine.Rendering;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Represents Poiyomi shader material.
    /// </summary>
    internal class PoiyomiMaterial : StandardMaterial, IToonStandardConvertable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PoiyomiMaterial"/> class.
        /// </summary>
        /// <param name="material">Material.</param>
        internal PoiyomiMaterial(Material material)
            : base(material)
        {
        }

        /// <inheritdoc/>
        internal override Shader ToonLitBakeShader => Shader.Find("Hidden/VRCQuestTools/Poiyomi");

        /// <inheritdoc/>
        internal override Vector2 MainTextureScale
        {
            get
            {
                var st = GetMainTexST();
                return new Vector2(st.x, st.y);
            }
        }

        /// <inheritdoc/>
        internal override Vector2 MainTextureOffset
        {
            get
            {
                var st = GetMainTexST();
                return new Vector2(st.z, st.w);
            }
        }

        /// <summary>
        /// Gets a value indicating whether shading is enabled.
        /// </summary>
        internal bool UseShadow => GetBoolPropertyWithFallback("_ShadingEnabled", "_LightingMode");

        /// <summary>
        /// Gets cull mode.
        /// </summary>
        internal CullMode CullMode => (CullMode)(int)Material.GetFloat("_Cull");

        /// <summary>
        /// Gets normal map.
        /// </summary>
        internal Texture NormalMap => GetTextureProperty("_BumpMap");

        /// <summary>
        /// Gets normal map scale.
        /// </summary>
        internal float NormalMapScale => GetFloatProperty("_BumpScale", 1.0f);

        /// <summary>
        /// Gets normal map texture scale.
        /// </summary>
        internal Vector2 NormalMapTextureScale => GetTextureScaleProperty("_BumpMap");

        /// <summary>
        /// Gets normal map texture offset.
        /// </summary>
        internal Vector2 NormalMapTextureOffset => GetTextureOffsetProperty("_BumpMap");

        /// <summary>
        /// Gets a value indicating whether any emission channel is enabled.
        /// </summary>
        internal bool UseEmission => EnableEmission0 || EnableEmission1 || EnableEmission2 || EnableEmission3;

        /// <summary>
        /// Gets or sets a value indicating whether emission channel 0 is enabled.
        /// </summary>
        internal bool EnableEmission0
        {
            get => GetBoolPropertyWithFallback("_EnableEmission", "_EmissionStrength");
            set => SetFloatPropertyIfExists("_EnableEmission", value ? 1.0f : 0.0f);
        }

        /// <summary>
        /// Gets or sets a value indicating whether emission channel 1 is enabled.
        /// </summary>
        internal bool EnableEmission1
        {
            get => GetBoolPropertyWithFallback("_EnableEmission1", "_EmissionStrength1");
            set => SetFloatPropertyIfExists("_EnableEmission1", value ? 1.0f : 0.0f);
        }

        /// <summary>
        /// Gets or sets a value indicating whether emission channel 2 is enabled.
        /// </summary>
        internal bool EnableEmission2
        {
            get => GetBoolPropertyWithFallback("_EnableEmission2", "_EmissionStrength2");
            set => SetFloatPropertyIfExists("_EnableEmission2", value ? 1.0f : 0.0f);
        }

        /// <summary>
        /// Gets or sets a value indicating whether emission channel 3 is enabled.
        /// </summary>
        internal bool EnableEmission3
        {
            get => GetBoolPropertyWithFallback("_EnableEmission3", "_EmissionStrength3");
            set => SetFloatPropertyIfExists("_EnableEmission3", value ? 1.0f : 0.0f);
        }

        /// <summary>
        /// Gets emission map 0.
        /// </summary>
        internal Texture EmissionMap0 => GetTextureProperty("_EmissionMap");

        /// <summary>
        /// Gets emission map 1.
        /// </summary>
        internal Texture EmissionMap1 => GetTextureProperty("_EmissionMap1");

        /// <summary>
        /// Gets emission map 2.
        /// </summary>
        internal Texture EmissionMap2 => GetTextureProperty("_EmissionMap2");

        /// <summary>
        /// Gets emission map 3.
        /// </summary>
        internal Texture EmissionMap3 => GetTextureProperty("_EmissionMap3");

        /// <summary>
        /// Gets emission color 0.
        /// </summary>
        internal Color EmissionColor0 => GetColorProperty("_EmissionColor");

        /// <summary>
        /// Gets emission color 1.
        /// </summary>
        internal Color EmissionColor1 => GetColorProperty("_EmissionColor1");

        /// <summary>
        /// Gets emission color 2.
        /// </summary>
        internal Color EmissionColor2 => GetColorProperty("_EmissionColor2");

        /// <summary>
        /// Gets emission color 3.
        /// </summary>
        internal Color EmissionColor3 => GetColorProperty("_EmissionColor3");

        /// <summary>
        /// Gets emission mask 0.
        /// </summary>
        internal Texture EmissionMask0 => GetTextureProperty("_EmissionMask");

        /// <summary>
        /// Gets emission mask 1.
        /// </summary>
        internal Texture EmissionMask1 => GetTextureProperty("_EmissionMask1");

        /// <summary>
        /// Gets emission mask 2.
        /// </summary>
        internal Texture EmissionMask2 => GetTextureProperty("_EmissionMask2");

        /// <summary>
        /// Gets emission mask 3.
        /// </summary>
        internal Texture EmissionMask3 => GetTextureProperty("_EmissionMask3");

        /// <summary>
        /// Gets emission mask channel 0.
        /// </summary>
        internal float EmissionMaskChannel0 => GetFloatProperty("_EmissionMaskChannel");

        /// <summary>
        /// Gets emission mask channel 1.
        /// </summary>
        internal float EmissionMaskChannel1 => GetFloatWithFallback("_EmissionMaskChannel1", "_EmissionMask1Channel");

        /// <summary>
        /// Gets emission mask channel 2.
        /// </summary>
        internal float EmissionMaskChannel2 => GetFloatWithFallback("_EmissionMaskChannel2", "_EmissionMask2Channel");

        /// <summary>
        /// Gets emission mask channel 3.
        /// </summary>
        internal float EmissionMaskChannel3 => GetFloatWithFallback("_EmissionMaskChannel3", "_EmissionMask3Channel");

        /// <summary>
        /// Gets emission strength 0.
        /// </summary>
        internal float EmissionStrength0 => GetFloatProperty("_EmissionStrength");

        /// <summary>
        /// Gets emission strength 1.
        /// </summary>
        internal float EmissionStrength1 => GetFloatProperty("_EmissionStrength1");

        /// <summary>
        /// Gets emission strength 2.
        /// </summary>
        internal float EmissionStrength2 => GetFloatProperty("_EmissionStrength2");

        /// <summary>
        /// Gets emission strength 3.
        /// </summary>
        internal float EmissionStrength3 => GetFloatProperty("_EmissionStrength3");

        /// <summary>
        /// Gets a value indicating whether base color is used as map for emission channel 0.
        /// </summary>
        internal bool EmissionBaseColorAsMap0 => GetBoolProperty("_EmissionBaseColorAsMap");

        /// <summary>
        /// Gets a value indicating whether base color is used as map for emission channel 1.
        /// </summary>
        internal bool EmissionBaseColorAsMap1 => GetBoolProperty("_EmissionBaseColorAsMap1");

        /// <summary>
        /// Gets a value indicating whether base color is used as map for emission channel 2.
        /// </summary>
        internal bool EmissionBaseColorAsMap2 => GetBoolProperty("_EmissionBaseColorAsMap2");

        /// <summary>
        /// Gets a value indicating whether base color is used as map for emission channel 3.
        /// </summary>
        internal bool EmissionBaseColorAsMap3 => GetBoolProperty("_EmissionBaseColorAsMap3");

        /// <summary>
        /// Gets emission map 0 texture scale.
        /// </summary>
        internal Vector2 EmissionMapTextureScale0 => GetTextureScaleProperty("_EmissionMap");

        /// <summary>
        /// Gets emission map 0 texture offset.
        /// </summary>
        internal Vector2 EmissionMapTextureOffset0 => GetTextureOffsetProperty("_EmissionMap");

        /// <summary>
        /// Gets emission map 1 texture scale.
        /// </summary>
        internal Vector2 EmissionMapTextureScale1 => GetTextureScaleProperty("_EmissionMap1");

        /// <summary>
        /// Gets emission map 1 texture offset.
        /// </summary>
        internal Vector2 EmissionMapTextureOffset1 => GetTextureOffsetProperty("_EmissionMap1");

        /// <summary>
        /// Gets emission map 2 texture scale.
        /// </summary>
        internal Vector2 EmissionMapTextureScale2 => GetTextureScaleProperty("_EmissionMap2");

        /// <summary>
        /// Gets emission map 2 texture offset.
        /// </summary>
        internal Vector2 EmissionMapTextureOffset2 => GetTextureOffsetProperty("_EmissionMap2");

        /// <summary>
        /// Gets emission map 3 texture scale.
        /// </summary>
        internal Vector2 EmissionMapTextureScale3 => GetTextureScaleProperty("_EmissionMap3");

        /// <summary>
        /// Gets emission map 3 texture offset.
        /// </summary>
        internal Vector2 EmissionMapTextureOffset3 => GetTextureOffsetProperty("_EmissionMap3");

        /// <summary>
        /// Gets emission mask 0 texture scale.
        /// </summary>
        internal Vector2 EmissionMaskTextureScale0 => GetTextureScaleProperty("_EmissionMask");

        /// <summary>
        /// Gets emission mask 0 texture offset.
        /// </summary>
        internal Vector2 EmissionMaskTextureOffset0 => GetTextureOffsetProperty("_EmissionMask");

        /// <summary>
        /// Gets emission mask 1 texture scale.
        /// </summary>
        internal Vector2 EmissionMaskTextureScale1 => GetTextureScaleProperty("_EmissionMask1");

        /// <summary>
        /// Gets emission mask 1 texture offset.
        /// </summary>
        internal Vector2 EmissionMaskTextureOffset1 => GetTextureOffsetProperty("_EmissionMask1");

        /// <summary>
        /// Gets emission mask 2 texture scale.
        /// </summary>
        internal Vector2 EmissionMaskTextureScale2 => GetTextureScaleProperty("_EmissionMask2");

        /// <summary>
        /// Gets emission mask 2 texture offset.
        /// </summary>
        internal Vector2 EmissionMaskTextureOffset2 => GetTextureOffsetProperty("_EmissionMask2");

        /// <summary>
        /// Gets emission mask 3 texture scale.
        /// </summary>
        internal Vector2 EmissionMaskTextureScale3 => GetTextureScaleProperty("_EmissionMask3");

        /// <summary>
        /// Gets emission mask 3 texture offset.
        /// </summary>
        internal Vector2 EmissionMaskTextureOffset3 => GetTextureOffsetProperty("_EmissionMask3");

        /// <summary>
        /// Gets a value indicating whether matcap is enabled.
        /// </summary>
        internal bool UseMatcap => GetBoolPropertyWithFallback("_MatcapEnable", "_Matcap");

        /// <summary>
        /// Gets matcap texture.
        /// </summary>
        internal Texture MatcapTexture => GetTextureProperty("_Matcap");

        /// <summary>
        /// Gets matcap color.
        /// </summary>
        internal Color MatcapColor => GetColorProperty("_MatcapColor");

        /// <summary>
        /// Gets matcap intensity.
        /// </summary>
        internal float MatcapIntensity => GetFloatProperty("_MatcapIntensity");

        /// <summary>
        /// Gets matcap mask texture.
        /// </summary>
        internal Texture MatcapMask => GetTextureProperty("_MatcapMask");

        /// <summary>
        /// Gets matcap mask texture scale.
        /// </summary>
        internal Vector2 MatcapMaskTextureScale => GetTextureScaleProperty("_MatcapMask");

        /// <summary>
        /// Gets matcap mask texture offset.
        /// </summary>
        internal Vector2 MatcapMaskTextureOffset => GetTextureOffsetProperty("_MatcapMask");

        /// <summary>
        /// Gets matcap mask channel.
        /// </summary>
        internal float MatcapMaskChannel => GetFloatProperty("_MatcapMaskChannel");

        /// <summary>
        /// Gets matcap replace blend weight.
        /// </summary>
        internal float MatcapReplace => GetFloatProperty("_MatcapReplace");

        /// <summary>
        /// Gets matcap multiply blend weight.
        /// </summary>
        internal float MatcapMultiply => GetFloatProperty("_MatcapMultiply");

        /// <summary>
        /// Gets matcap add blend weight.
        /// </summary>
        internal float MatcapAdd => GetFloatProperty("_MatcapAdd");

        /// <summary>
        /// Gets matcap screen blend weight.
        /// </summary>
        internal float MatcapScreen => GetFloatProperty("_MatcapScreen");

        /// <summary>
        /// Gets a value indicating whether rim lighting is enabled.
        /// </summary>
        internal bool UseRimLighting => GetBoolPropertyWithFallback("_EnableRimLighting", "_RimStyle");

        /// <summary>
        /// Gets the rim lighting style (0=Poiyomi, 1=UTS2, 2=LilToon).
        /// </summary>
        internal int RimStyle => (int)GetFloatProperty("_RimStyle");

        /// <summary>
        /// Gets rim light color (styles 0 and 1).
        /// </summary>
        internal Color RimLightColor => GetColorProperty("_RimLightColor");

        // ---- Style 0 (Poiyomi) ----

        /// <summary>
        /// Gets rim strength (style 0 only, Range 0-20).
        /// </summary>
        internal float RimStrength => GetFloatProperty("_RimStrength");

        /// <summary>
        /// Gets rim width (style 0 only, Range 0-1).
        /// </summary>
        internal float RimWidth => GetFloatProperty("_RimWidth");

        /// <summary>
        /// Gets rim sharpness (style 0 only, Range 0-1).
        /// </summary>
        internal float RimSharpness => GetFloatProperty("_RimSharpness");

        // ---- Style 1 (UTS2) ----

        /// <summary>
        /// Gets rim light power (style 1 only, Range 0-1).
        /// </summary>
        internal float RimLightPower => GetFloatProperty("_RimLight_Power");

        /// <summary>
        /// Gets whether rim feathering is disabled (style 1 only).
        /// </summary>
        internal bool RimLightFeatherOff => GetBoolProperty("_RimLight_FeatherOff");

        /// <summary>
        /// Gets rim environmental lighting factor (style 1 only, Range 0-1).
        /// </summary>
        internal float RimLightingEnvironmental => GetFloatProperty("_Is_LightColor_RimLight");

        // ---- Style 2 (LilToon) ----

        /// <summary>
        /// Gets rim color for LilToon style (style 2 only, HDR).
        /// </summary>
        internal Color RimColorLilToon => GetColorProperty("_RimColor");

        /// <summary>
        /// Gets rim border (style 2 only, Range 0-1).
        /// </summary>
        internal float RimBorder => GetFloatProperty("_RimBorder");

        /// <summary>
        /// Gets rim blur/softness (style 2 only, Range 0-1).
        /// </summary>
        internal float RimBlur => GetFloatProperty("_RimBlur");

        /// <summary>
        /// Gets rim Fresnel power (style 2 only, Range 0.01-50).
        /// </summary>
        internal float RimFresnelPower => GetFloatProperty("_RimFresnelPower", 1.0f);

        /// <summary>
        /// Gets rim enable lighting factor (style 2 only, Range 0-1).
        /// </summary>
        internal float RimEnableLighting => GetFloatProperty("_RimEnableLighting");

        /// <summary>
        /// Gets rim main color blend (style 2 albedo tint, Range 0-1).
        /// </summary>
        internal float RimMainStrength => GetFloatProperty("_RimMainStrength");

        /// <summary>
        /// Gets a value indicating whether specular/reflections are enabled.
        /// </summary>
        internal bool UseSpecular => GetBoolPropertyWithFallback("_MochieBRDF", "_MochieMetallicMaps");

        /// <summary>
        /// Gets a value indicating whether vertex coloring should be applied.
        /// True when vertex coloring is enabled and blend amount exceeds 50%.
        /// </summary>
        internal bool UseVertexColor => GetBoolPropertyWithFallback("_MainVertexColoringEnabled", "_MainVertexColoring")
            && GetBoolProperty("_MainVertexColoring");

        /// <summary>
        /// Gets the minimum light brightness (Range 0-1).
        /// ToonStandard clamps this to its own Range(0, 0.1) at assignment time.
        /// </summary>
        internal float LightingMinLightBrightness => GetFloatProperty("_LightingMinLightBrightness");

        /// <summary>
        /// Gets metallic multiplier.
        /// </summary>
        internal float MetallicMultiplier => GetFloatProperty("_MochieMetallicMultiplier");

        /// <summary>
        /// Gets roughness multiplier.
        /// </summary>
        internal float RoughnessMultiplier => GetFloatProperty("_MochieRoughnessMultiplier");

        /// <summary>
        /// Gets metallic packed texture.
        /// </summary>
        internal Texture MetallicMaps => GetTextureProperty("_MochieMetallicMaps");

        /// <summary>
        /// Gets metallic packed texture scale.
        /// </summary>
        internal Vector2 MetallicMapsTextureScale => GetTextureScaleProperty("_MochieMetallicMaps");

        /// <summary>
        /// Gets metallic packed texture offset.
        /// </summary>
        internal Vector2 MetallicMapsTextureOffset => GetTextureOffsetProperty("_MochieMetallicMaps");

        /// <summary>
        /// Gets metallic source channel index.
        /// </summary>
        internal float MetallicMapsMetallicChannel => GetFloatProperty("_MochieMetallicMapsMetallicChannel");

        /// <summary>
        /// Gets roughness source channel index.
        /// </summary>
        internal float MetallicMapsRoughnessChannel => GetFloatProperty("_MochieMetallicMapsRoughnessChannel");

        /// <summary>
        /// Gets the AO (Ambient Occlusion) packed texture.
        /// Poiyomi packs up to 4 AO channels (R/G/B/A) into one texture.
        /// </summary>
        internal Texture AOMap => GetTextureProperty("_LightingAOMaps");

        /// <summary>
        /// Gets the AO map texture scale.
        /// </summary>
        internal Vector2 AOMapTextureScale => GetTextureScaleProperty("_LightingAOMaps");

        /// <summary>
        /// Gets the AO map texture offset.
        /// </summary>
        internal Vector2 AOMapTextureOffset => GetTextureOffsetProperty("_LightingAOMaps");

        /// <summary>
        /// Gets the AO R-channel strength (Range 0-1). Defaults to 1 (R is the primary channel).
        /// </summary>
        internal float AOStrengthR => GetFloatProperty("_LightDataAOStrengthR", 1.0f);

        /// <summary>
        /// Gets the AO G-channel strength (Range 0-1).
        /// In locked shaders the property may be renamed with a material-name suffix;
        /// if absent it returns 0.
        /// </summary>
        internal float AOStrengthG => GetFloatProperty("_LightDataAOStrengthG");

        /// <summary>
        /// Gets the AO B-channel strength (Range 0-1).
        /// </summary>
        internal float AOStrengthB => GetFloatProperty("_LightDataAOStrengthB");

        /// <summary>
        /// Gets the AO A-channel strength (Range 0-1).
        /// </summary>
        internal float AOStrengthA => GetFloatProperty("_LightDataAOStrengthA");

        /// <summary>
        /// Gets the main texture scale and offset as a Vector4 from _MainTex_ST property.
        /// Poiyomi shaders may store UV tiling as a separate Vector4 property rather than
        /// using Unity's standard texture scale/offset metadata.
        /// </summary>
        /// <returns>Vector4 with (scaleX, scaleY, offsetX, offsetY).</returns>
        private Vector4 GetMainTexST()
        {
            if (Material.HasProperty("_MainTex_ST"))
            {
                return Material.GetVector("_MainTex_ST");
            }

            var scale = Material.mainTextureScale;
            var offset = Material.mainTextureOffset;
            return new Vector4(scale.x, scale.y, offset.x, offset.y);
        }

        /// <summary>
        /// Returns true when the named property exists and its float value exceeds 0.5.
        /// Returns false when the property is absent, e.g. stripped from a locked Poiyomi shader.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <returns>True when the property exists and its value is greater than 0.5.</returns>
        private bool GetBoolProperty(string name)
            => Material.HasProperty(name) && Material.GetFloat(name) > 0.5f;

        /// <summary>
        /// Determines whether a boolean feature is active, using a two-step check to handle
        /// locked Poiyomi shaders correctly.
        /// <para>
        /// When a feature is <em>disabled</em> and the shader is locked, Poiyomi removes both the
        /// toggle property and all feature-specific value properties.  When a feature is
        /// <em>enabled</em> and the shader is locked, Poiyomi may remove the toggle property (it is
        /// baked in as permanently on), but the feature-specific value properties are preserved.
        /// </para>
        /// <para>
        /// Check order:
        /// 1. If <paramref name="toggleProp"/> exists, use its value (&gt; 0.5 = true).
        /// 2. Otherwise, if <paramref name="fallbackProp"/> exists the feature was enabled and baked;
        ///    return true.
        /// 3. If neither exists, the feature was disabled and stripped; return false.
        /// </para>
        /// </summary>
        /// <param name="toggleProp">The float toggle property (e.g. "_EnableEmission").</param>
        /// <param name="fallbackProp">A value property that is only present when the feature is
        /// active (e.g. "_EmissionMap" for emission channel 0).</param>
        /// <returns>Whether the feature is considered active.</returns>
        private bool GetBoolPropertyWithFallback(string toggleProp, string fallbackProp)
        {
            if (Material.HasProperty(toggleProp))
            {
                return Material.GetFloat(toggleProp) > 0.5f;
            }

            // Toggle was stripped from locked shader.
            // Feature is on when its primary value property still exists.
            return Material.HasProperty(fallbackProp);
        }

        /// <summary>
        /// Gets a Texture property from the material, or null when the property is absent.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <returns>Texture or null.</returns>
        private Texture GetTextureProperty(string name)
            => Material.HasProperty(name) ? Material.GetTexture(name) : null;

        /// <summary>
        /// Gets the tiling of a texture property, or Vector2.one when the property is absent.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <returns>Tiling scale or Vector2.one.</returns>
        private Vector2 GetTextureScaleProperty(string name)
            => Material.HasProperty(name) ? Material.GetTextureScale(name) : Vector2.one;

        /// <summary>
        /// Gets the offset of a texture property, or Vector2.zero when the property is absent.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <returns>Texture offset or Vector2.zero.</returns>
        private Vector2 GetTextureOffsetProperty(string name)
            => Material.HasProperty(name) ? Material.GetTextureOffset(name) : Vector2.zero;

        /// <summary>
        /// Gets a float property from the material, or a default value when the property is absent.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="defaultValue">Value to return when the property is missing, e.g. stripped from a locked Poiyomi shader.</param>
        /// <returns>Property value or <paramref name="defaultValue"/>.</returns>
        private float GetFloatProperty(string name, float defaultValue = 0.0f)
            => Material.HasProperty(name) ? Material.GetFloat(name) : defaultValue;

        /// <summary>
        /// Gets a Color property from the material, or default(Color) when the property is absent.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <returns>Property value or default(Color) when absent.</returns>
        private Color GetColorProperty(string name)
            => Material.HasProperty(name) ? Material.GetColor(name) : default;

        /// <summary>
        /// Sets a float property only when it exists on the material.
        /// No-ops on locked Poiyomi shaders where the property has been stripped.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="value">Value to set.</param>
        private void SetFloatPropertyIfExists(string name, float value)
        {
            if (Material.HasProperty(name))
            {
                Material.SetFloat(name, value);
            }
        }

        private float GetFloatWithFallback(string primaryName, string fallbackName)
        {
            if (Material.HasProperty(primaryName))
            {
                return Material.GetFloat(primaryName);
            }

            if (Material.HasProperty(fallbackName))
            {
                return Material.GetFloat(fallbackName);
            }

            return 0.0f;
        }
    }
}
