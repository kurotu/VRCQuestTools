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
        /// Gets the index of the active matcap slot, or -1 when no matcap is enabled.
        /// Poiyomi has two matcap slots (slot 0 = "_Matcap*", slot 1 = "_Matcap2*"), but
        /// ToonStandard provides only one. The lowest-numbered enabled slot is selected.
        /// </summary>
        internal int ActiveMatcapSlot
        {
            get
            {
                if (GetBoolPropertyWithFallback("_MatcapEnable", "_Matcap"))
                {
                    return 0;
                }

                if (GetBoolPropertyWithFallback("_Matcap2Enable", "_Matcap2"))
                {
                    return 1;
                }

                return -1;
            }
        }

        /// <summary>
        /// Gets a value indicating whether matcap is enabled.
        /// </summary>
        internal bool UseMatcap => ActiveMatcapSlot >= 0;

        /// <summary>
        /// Gets matcap texture.
        /// </summary>
        internal Texture MatcapTexture => GetTextureProperty(MatcapProp(string.Empty));

        /// <summary>
        /// Gets matcap color.
        /// </summary>
        internal Color MatcapColor => GetColorProperty(MatcapProp("Color"));

        /// <summary>
        /// Gets matcap intensity.
        /// </summary>
        internal float MatcapIntensity => GetFloatProperty(MatcapProp("Intensity"));

        /// <summary>
        /// Gets matcap mask texture.
        /// </summary>
        internal Texture MatcapMask => GetTextureProperty(MatcapProp("Mask"));

        /// <summary>
        /// Gets matcap mask texture scale.
        /// </summary>
        internal Vector2 MatcapMaskTextureScale => GetTextureScaleProperty(MatcapProp("Mask"));

        /// <summary>
        /// Gets matcap mask texture offset.
        /// </summary>
        internal Vector2 MatcapMaskTextureOffset => GetTextureOffsetProperty(MatcapProp("Mask"));

        /// <summary>
        /// Gets matcap mask channel.
        /// </summary>
        internal float MatcapMaskChannel => GetFloatProperty(MatcapProp("MaskChannel"));

        /// <summary>
        /// Gets matcap replace blend weight.
        /// </summary>
        internal float MatcapReplace => GetFloatProperty(MatcapProp("Replace"));

        /// <summary>
        /// Gets matcap multiply blend weight.
        /// </summary>
        internal float MatcapMultiply => GetFloatProperty(MatcapProp("Multiply"));

        /// <summary>
        /// Gets matcap add blend weight.
        /// </summary>
        internal float MatcapAdd => GetFloatProperty(MatcapProp("Add"));

        /// <summary>
        /// Gets matcap screen blend weight.
        /// </summary>
        internal float MatcapScreen => GetFloatProperty(MatcapProp("Screen"));

        /// <summary>
        /// Gets the index of the active rim lighting slot, or -1 when no rim is enabled.
        /// Poiyomi has two rim slots (slot 0 = "_Rim*", slot 1 = "_Rim2*"), but ToonStandard
        /// provides only one. The lowest-numbered enabled slot is selected.
        /// </summary>
        internal int ActiveRimSlot
        {
            get
            {
                if (GetBoolPropertyWithFallback("_EnableRimLighting", "_RimStyle"))
                {
                    return 0;
                }

                if (GetBoolPropertyWithFallback("_EnableRim2Lighting", "_Rim2Style"))
                {
                    return 1;
                }

                return -1;
            }
        }

        /// <summary>
        /// Gets a value indicating whether rim lighting is enabled.
        /// </summary>
        internal bool UseRimLighting => ActiveRimSlot >= 0;

        /// <summary>
        /// Gets the rim lighting style (0=Poiyomi, 1=UTS2, 2=LilToon).
        /// </summary>
        internal int RimStyle => (int)GetFloatProperty(RimProp("Style"));

        /// <summary>
        /// Gets rim light color (styles 0 and 1).
        /// </summary>
        internal Color RimLightColor => GetColorProperty(RimProp("LightColor"));

        // ---- Style 0 (Poiyomi) ----

        /// <summary>
        /// Gets rim strength (style 0 only, Range 0-20).
        /// </summary>
        internal float RimStrength => GetFloatProperty(RimProp("Strength"));

        /// <summary>
        /// Gets rim width (style 0 only, Range 0-1).
        /// </summary>
        internal float RimWidth => GetFloatProperty(RimProp("Width"));

        /// <summary>
        /// Gets rim sharpness (style 0 only, Range 0-1).
        /// </summary>
        internal float RimSharpness => GetFloatProperty(RimProp("Sharpness"));

        // ---- Style 1 (UTS2) ----

        /// <summary>
        /// Gets rim light power (style 1 only, Range 0-1).
        /// </summary>
        internal float RimLightPower => GetFloatProperty(RimProp("Light_Power"));

        /// <summary>
        /// Gets whether rim feathering is disabled (style 1 only).
        /// </summary>
        internal bool RimLightFeatherOff => GetBoolProperty(RimProp("Light_FeatherOff"));

        /// <summary>
        /// Gets rim environmental lighting factor (style 1 only, Range 0-1).
        /// </summary>
        internal float RimLightingEnvironmental => GetFloatProperty(ActiveRimSlot == 1 ? "_Is_LightColor_Rim2Light" : "_Is_LightColor_RimLight");

        // ---- Style 2 (LilToon) ----

        /// <summary>
        /// Gets rim color for LilToon style (style 2 only, HDR).
        /// </summary>
        internal Color RimColorLilToon => GetColorProperty(RimProp("Color"));

        /// <summary>
        /// Gets rim border (style 2 only, Range 0-1).
        /// </summary>
        internal float RimBorder => GetFloatProperty(RimProp("Border"));

        /// <summary>
        /// Gets rim blur/softness (style 2 only, Range 0-1).
        /// </summary>
        internal float RimBlur => GetFloatProperty(RimProp("Blur"));

        /// <summary>
        /// Gets rim Fresnel power (style 2 only, Range 0.01-50).
        /// </summary>
        internal float RimFresnelPower => GetFloatProperty(RimProp("FresnelPower"), 1.0f);

        /// <summary>
        /// Gets rim enable lighting factor (style 2 only, Range 0-1).
        /// </summary>
        internal float RimEnableLighting => GetFloatProperty(RimProp("EnableLighting"));

        /// <summary>
        /// Gets rim main color blend (style 2 albedo tint, Range 0-1).
        /// </summary>
        internal float RimMainStrength => GetFloatProperty(RimProp("MainStrength"));

        /// <summary>
        /// Gets a value indicating whether specular/reflections (Mochie BRDF "Reflections &amp; Specular") are enabled.
        /// </summary>
        internal bool UseSpecular => GetBoolPropertyWithFallback("_MochieBRDF", "_MochieMetallicMaps");

        /// <summary>
        /// Gets a value indicating whether Stylized Reflections are enabled.
        /// When enabled, Stylized Reflections take priority over the Mochie BRDF reflections/specular.
        /// </summary>
        internal bool UseStylizedReflections => GetBoolPropertyWithFallback("_StylizedSpecular", "_StylizedReflectionMode");

        /// <summary>
        /// Gets the Stylized Reflection mode (0=UnityChan, 1=lilToon).
        /// </summary>
        internal int StylizedReflectionMode => (int)GetFloatProperty("_StylizedReflectionMode");

        // ---- Stylized Reflections mode 0 (UnityChan toon specular) ----

        /// <summary>
        /// Gets the stylized specular tint color (mode 0).
        /// </summary>
        internal Color StylizedHighColor => GetColorProperty("_HighColor");

        /// <summary>
        /// Gets the stylized specular map texture (mode 0).
        /// </summary>
        internal Texture StylizedHighColorTex => GetTextureProperty("_HighColor_Tex");

        /// <summary>
        /// Gets the stylized specular mask texture (mode 0).
        /// </summary>
        internal Texture StylizedHighColorMask => GetTextureProperty("_Set_HighColorMask");

        /// <summary>
        /// Gets the stylized specular mask texture scale (mode 0).
        /// </summary>
        internal Vector2 StylizedHighColorMaskTextureScale => GetTextureScaleProperty("_Set_HighColorMask");

        /// <summary>
        /// Gets the stylized specular mask texture offset (mode 0).
        /// </summary>
        internal Vector2 StylizedHighColorMaskTextureOffset => GetTextureOffsetProperty("_Set_HighColorMask");

        /// <summary>
        /// Gets the stylized specular mask channel (mode 0).
        /// </summary>
        internal float StylizedHighColorMaskChannel => GetFloatProperty("_Set_HighColorMaskChannel", 1.0f);

        /// <summary>
        /// Gets the stylized specular strength (mode 0).
        /// </summary>
        internal float StylizedSpecularStrength => GetFloatProperty("_StylizedSpecularStrength", 1.0f);

        /// <summary>
        /// Gets the stylized specular highlight size (mode 0, Range 0-1, "_HighColor_Power").
        /// </summary>
        internal float StylizedSpecularSize => GetFloatProperty("_HighColor_Power");

        /// <summary>
        /// Gets the stylized specular feather (mode 0, Range 0-1).
        /// </summary>
        internal float StylizedSpecularFeather => GetFloatProperty("_StylizedSpecularFeather");

        // ---- Stylized Reflections mode 1 (lilToon reflections) ----

        /// <summary>
        /// Gets the stylized lilToon metallic multiplier (mode 1).
        /// </summary>
        internal float StylizedMetallic => GetFloatProperty("_Metallic");

        /// <summary>
        /// Gets the stylized lilToon metallic map (mode 1).
        /// </summary>
        internal Texture StylizedMetallicMap => GetTextureProperty("_MetallicGlossMap");

        /// <summary>
        /// Gets the stylized lilToon metallic map texture scale (mode 1).
        /// </summary>
        internal Vector2 StylizedMetallicMapTextureScale => GetTextureScaleProperty("_MetallicGlossMap");

        /// <summary>
        /// Gets the stylized lilToon metallic map texture offset (mode 1).
        /// </summary>
        internal Vector2 StylizedMetallicMapTextureOffset => GetTextureOffsetProperty("_MetallicGlossMap");

        /// <summary>
        /// Gets the stylized lilToon smoothness multiplier (mode 1).
        /// </summary>
        internal float StylizedSmoothness => GetFloatProperty("_Smoothness", 1.0f);

        /// <summary>
        /// Gets the stylized lilToon smoothness map (mode 1).
        /// </summary>
        internal Texture StylizedSmoothnessTex => GetTextureProperty("_SmoothnessTex");

        /// <summary>
        /// Gets the stylized lilToon smoothness map texture scale (mode 1).
        /// </summary>
        internal Vector2 StylizedSmoothnessTexTextureScale => GetTextureScaleProperty("_SmoothnessTex");

        /// <summary>
        /// Gets the stylized lilToon smoothness map texture offset (mode 1).
        /// </summary>
        internal Vector2 StylizedSmoothnessTexTextureOffset => GetTextureOffsetProperty("_SmoothnessTex");

        /// <summary>
        /// Gets the stylized lilToon reflectance (mode 1).
        /// </summary>
        internal float StylizedReflectance => GetFloatProperty("_Reflectance", 0.04f);

        /// <summary>
        /// Gets the stylized lilToon specular blur (mode 1, Range 0-1).
        /// </summary>
        internal float StylizedSpecularBlur => GetFloatProperty("_SpecularBlur");

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

        /// <summary>
        /// Resolves a rim lighting property name for the active rim slot.
        /// Slot 0 uses the "_Rim" prefix, slot 1 uses the "_Rim2" prefix.
        /// </summary>
        /// <param name="suffix">The property name part after the prefix (e.g. "LightColor", "Strength").</param>
        /// <returns>The resolved property name.</returns>
        private string RimProp(string suffix)
            => (ActiveRimSlot == 1 ? "_Rim2" : "_Rim") + suffix;

        /// <summary>
        /// Resolves a matcap property name for the active matcap slot.
        /// Slot 0 uses the "_Matcap" prefix, slot 1 uses the "_Matcap2" prefix.
        /// </summary>
        /// <param name="suffix">The property name part after the prefix (e.g. "Color", "Mask"); empty for the base texture.</param>
        /// <returns>The resolved property name.</returns>
        private string MatcapProp(string suffix)
            => (ActiveMatcapSlot == 1 ? "_Matcap2" : "_Matcap") + suffix;

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
