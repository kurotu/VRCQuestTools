Shader "Hidden/VRCQuestTools/Poiyomi"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)

        [Normal] _BumpMap ("Normalmap", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1

        [ToggleUI]_ShadingEnabled ("Enable Shading", Float) = 0
        [KeywordEnum(TextureRamp, Multilayer Math, Wrapped, Skin, ShadeMap, Flat, Realistic, Cloth, SDF)] _LightingMode ("Lighting Type", Float) = 5
        _LightingShadowColor ("Shadow Tint", Color) = (1, 1, 1)

        // _LightingMode == 0
        [sRGBWarning(true)]_ToonRamp ("Lighting Ramp", 2D) = "white" { }
      	_ShadowOffset ("Ramp Offset", Range(-1, 1)) = 0

        // _LightingMode == 1
        [sRGBWarning(true)]_ShadowColorTex ("Color Tex", 2D) = "black" { }
        _ShadowColor ("Shadow Color", Color) = (0.7, 0.75, 0.85, 1.0)
        _ShadowBorder ("Shadow Border", Range(0, 1)) = 0.5
        _ShadowBlur ("Shadow Blur", Range(0, 1)) = 0.1
        [sRGBWarning(true)]_Shadow2ndColorTex ("Color Tex", 2D) = "black" { }
        _Shadow2ndColor ("Shadow 2nd Color", Color) = (0, 0, 0, 0)
        _Shadow2ndBorder ("Shadow 2nd Border", Range(0, 1)) = 0.5
        _Shadow2ndBlur ("Shadow 2nd Blur", Range(0, 1)) = 0.1
        [sRGBWarning(true)]_Shadow3rdColorTex ("Color Tex", 2D) = "black" { }
        _Shadow3rdColor ("Shadow 3rd Color", Color) = (0, 0, 0, 0)
        _Shadow3rdBorder ("Shadow 3rd Border", Range(0, 1)) = 0.5
        _Shadow3rdBlur ("Shadow 3rd Blur", Range(0, 1)) = 0.1

        // _LightingMode == 2
        // apply realistic and tint

        // _LightingMode == 3
        // apply realistic and tint

        // _LightingMode == 4
        _1st_ShadeColor ("1st Shade Color", Color) = (1, 1, 1)
    		[sRGBWarning(true)]_1st_ShadeMap ("1st ShadeMap", 2D) = "white" { }
        [ToggleUI] _Use_BaseAs1st ("Use BaseMap as 1st ShadeMap", Float) = 0
        _2nd_ShadeColor ("2nd Shade Color", Color) = (1, 1, 1)
        [sRGBWarning(true)]_2nd_ShadeMap ("2nd ShadeMap", 2D) = "white" { }
        [ToggleUI] _Use_1stAs2nd ("Use BaseMap as 2nd ShadeMap", Float) = 0
		    _BaseColor_Step ("BaseColor_Step", Range(0.01, 1)) = 0.5
		    _BaseShade_Feather ("Base/Shade_Feather", Range(0.0001, 1)) = 0.0001
		    _ShadeColor_Step ("ShadeColor_Step", Range(0, 1)) = 0
		    _1st2nd_Shades_Feather ("1st/2nd_Shades_Feather", Range(0.0001, 1)) = 0.0001
    		[Enum(Replace, 0, Multiply, 1)]_ShadingShadeMapBlendType ("Blend Mode", Int) = 0

        // _LightingMode == 5
        // apply no shadow (flat)

        // _LightingMode == 6
        // apply realistic

        // _LightingMode == 7
        // apply realistic

        // _LightingMode == 8
        // apply flat

        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 1

        [ToggleUI]_EnableEmission ("Enable Emission", Float) = 0
        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,0)
        _EmissionMap ("Emission 0", 2D) = "black" {}
        [ToggleUI]_EmissionBaseColorAsMap ("Base Color as Map", Float) = 0
        [sRGBWarning]_EmissionMask ("Emission Mask", 2D) = "white" {}
        [Enum(R, 0, G, 1, B, 2, A, 3)]_EmissionMaskChannel ("Channel", Float) = 0
        // [ToggleUI]_EmissionMaskInvert ("Invert", Float) = 0
        _EmissionStrength ("Emission Strength", Range(0, 20)) = 0

        [ToggleUI]_EnableEmission1 ("Enable Emission 1", Float) = 0
        [HDR] _EmissionColor1 ("Emission Color 1", Color) = (0,0,0,0)
        _EmissionMap1 ("Emission 1", 2D) = "black" {}
        [ToggleUI]_EmissionBaseColorAsMap1 ("Base Color as Map", Float) = 0
        [sRGBWarning]_EmissionMask1 ("Emission Mask 1", 2D) = "white" {}
        [Enum(R, 0, G, 1, B, 2, A, 3)]_EmissionMask1Channel ("Channel 1", Float) = 0
        // [ToggleUI]_EmissionMaskInvert1 ("Invert 1", Float) = 0
        _EmissionStrength1 ("Emission Strength 1", Range(0, 20)) = 0

        [ToggleUI]_EnableEmission2 ("Enable Emission 2", Float) = 0
        [HDR] _EmissionColor2 ("Emission Color 2", Color) = (0,0,0,0)
        _EmissionMap2 ("Emission 2", 2D) = "black" {}
        [ToggleUI]_EmissionBaseColorAsMap2 ("Base Color as Map", Float) = 0
        [sRGBWarning]_EmissionMask2 ("Emission Mask 2", 2D) = "white" {}
        [Enum(R, 0, G, 1, B, 2, A, 3)]_EmissionMask2Channel ("Channel 2", Float) = 0
        // [ToggleUI]_EmissionMaskInvert2 ("Invert 2", Float) = 0
        _EmissionStrength2 ("Emission Strength 2", Range(0, 20)) = 0

        [ToggleUI]_EnableEmission3 ("Enable Emission 3", Float) = 0
        [HDR] _EmissionColor3 ("Emission Color 3", Color) = (0,0,0,0)
        _EmissionMap3 ("Emission 3", 2D) = "black" {}
        [ToggleUI]_EmissionBaseColorAsMap3 ("Base Color as Map", Float) = 0
        [sRGBWarning]_EmissionMask3 ("Emission Mask 3", 2D) = "white" {}
        [Enum(R, 0, G, 1, B, 2, A, 3)]_EmissionMask3Channel ("Channel 3", Float) = 0
        // [ToggleUI]_EmissionMaskInvert3 ("Invert 3", Float) = 0
        _EmissionStrength3 ("Emission Strength 3", Range(0, 20)) = 0

        _VQT_MainTexBrightness("VQT Main Texture Brightness", Range(0, 1)) = 1
        _VQT_GenerateShadow("VQT Generate Shadow", Int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityStandardUtils.cginc"
            #include "cginc/vqt_common.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                // float2 uv_EmissionMap : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            sampler2D _BumpMap;
            float _BumpScale;

            float _ShadingEnabled;
            float _LightingMode;
            float3 _LightingShadowColor;

            sampler2D _ToonRamp;
            float4 _ToonRamp_ST;
            float _ShadowOffset;

            sampler2D _ShadowColorTex;
            float4 _ShadowColorTex_ST;
            float4 _ShadowColor;
            float _ShadowBorder;
            float _ShadowBlur;
            sampler2D _Shadow2ndColorTex;
            float4 _Shadow2ndColorTex_ST;
            float4 _Shadow2ndColor;
            float _Shadow2ndBorder;
            float _Shadow2ndBlur;
            sampler2D _Shadow3rdColorTex;
            float4 _Shadow3rdColorTex_ST;
            float4 _Shadow3rdColor;
            float _Shadow3rdBorder;
            float _Shadow3rdBlur;

            float3 _1st_ShadeColor;
            sampler2D _1st_ShadeMap;
            float _Use_BaseAs1st;
            float3 _2nd_ShadeColor;
            sampler2D _2nd_ShadeMap;
            float _Use_1stAs2nd;
            float _BaseColor_Step;
            float _BaseShade_Feather;
            float _ShadeColor_Step;
            float _1st2nd_Shades_Feather;
            float _ShadingShadeMapBlendType;

            float _ShadowStrength;

            float _EnableEmission;
            float4 _EmissionColor;
            sampler2D _EmissionMap;
            float _EmissionBaseColorAsMap;
            sampler2D _EmissionMask;
            float _EmissionMaskChannel;
            // float _EmissionMaskInvert;
            float _EmissionStrength;

            float _EnableEmission1;
            float4 _EmissionColor1;
            sampler2D _EmissionMap1;
            float _EmissionBaseColorAsMap1;
            sampler2D _EmissionMask1;
            float _EmissionMask1Channel;
            // float _EmissionMaskInvert1;
            float _EmissionStrength1;

            float _EnableEmission2;
            float4 _EmissionColor2;
            sampler2D _EmissionMap2;
            float _EmissionBaseColorAsMap2;
            sampler2D _EmissionMask2;
            float _EmissionMask2Channel;
            // float _EmissionMaskInvert2;
            float _EmissionStrength2;

            float _EnableEmission3;
            float4 _EmissionColor3;
            sampler2D _EmissionMap3;
            float _EmissionBaseColorAsMap3;
            sampler2D _EmissionMask3;
            float _EmissionMask3Channel;
            // float _EmissionMaskInvert3;
            float _EmissionStrength3;

            float _VQT_MainTexBrightness;
            uint _VQT_GenerateShadow;

            float4 sampleTex2D(sampler2D tex, float2 uv, float angle) {
              half angleCos = cos(angle);
              half angleSin = sin(angle);
              half2x2 rotateMatrix = half2x2(angleCos, -angleSin, angleSin, angleCos);
              float2 newUV = mul(rotateMatrix, uv - 0.5) + 0.5;
              return tex2D(tex, newUV);
            }

            float4 poiMultiLayerMath(float4 baseColor, float2 uv, float light) {
                float4 shadow = 1;
                float shadow1BorderMax = _ShadowBorder + _ShadowBlur / 2;
                float shadow1BorderMin = _ShadowBorder - _ShadowBlur / 2;
                if (light > shadow1BorderMax) {
                    return baseColor;
                }

                if (_ShadowColor.a > 0) {
                    float4 shadowTex = tex2D(_ShadowColorTex, uv);
                    float3 color = _ShadowColor.rgb * shadowTex.rgb;
                    shadow.rgb = lerp(color, shadow.rgb, saturate((light - shadow1BorderMin) / _ShadowBlur));
                }

                if (_Shadow2ndColor.a > 0) {
                    float shadow2BorderMax = _Shadow2ndBorder + _Shadow2ndBlur / 2;
                    float shadow2BorderMin = _Shadow2ndBorder - _Shadow2ndBlur / 2;
                    float4 shadowTex = tex2D(_Shadow2ndColorTex, uv);
                    float3 color = _Shadow2ndColor.rgb * shadowTex.rgb;
                    shadow.rgb = lerp(color, shadow.rgb, saturate((light - shadow2BorderMin) / _Shadow2ndBlur));
                }

                if (_Shadow3rdColor.a > 0) {
                    float shadow3BorderMax = _Shadow3rdBorder + _Shadow3rdBlur / 2;
                    float shadow3BorderMin = _Shadow3rdBorder - _Shadow3rdBlur / 2;
                    float4 shadowTex = tex2D(_Shadow3rdColorTex, uv);
                    float3 color = _Shadow3rdColor.rgb * shadowTex.rgb;
                    shadow.rgb = lerp(color, shadow.rgb, saturate((light - shadow3BorderMin) / _Shadow3rdBlur));
                }

                float4 result = baseColor;
                result.rgb *= shadow.rgb;
                return result;
            }

            float4 poiShadeMap(float4 baseColor, float2 uv, float light) {
                float4 result = baseColor;
                if (light >= _BaseColor_Step) {
                    return baseColor;
                }

                float4 shade1Map = sampleTex2D(_1st_ShadeMap, uv, 0.0f);
                float4 shade1 = shade1Map;
                if (_Use_BaseAs1st == 1) {
                    shade1.rgb = baseColor.rgb;
                }
                shade1.rgb *= _1st_ShadeColor.rgb;
                if (_ShadingShadeMapBlendType == 1) {
                    shade1.rgb *= baseColor.rgb;
                }
                float baseStepMin = _BaseColor_Step - _BaseShade_Feather;
                result.rgb = lerp(shade1.rgb, result.rgb, saturate((light - baseStepMin) / _BaseShade_Feather));

                float4 shade2Map = sampleTex2D(_2nd_ShadeMap, uv, 0.0f);
                float4 shade2 = shade2Map;
                if (_Use_1stAs2nd == 1) {
                    shade2.rgb = shade1Map.rgb;
                }
                shade2.rgb *= _2nd_ShadeColor.rgb;
                if (_ShadingShadeMapBlendType == 1) {
                    shade2.rgb *= baseColor.rgb;
                }
                float shadeStepMin = _ShadeColor_Step - _1st2nd_Shades_Feather;
                result.rgb = lerp(shade2.rgb, result.rgb, saturate((light - shadeStepMin) / _1st2nd_Shades_Feather));

                return result;
            }

            float4 poiApplyShadow(float4 baseColor, float2 uv) {
                half3 normal = UnpackScaleNormal(tex2D(_BumpMap, uv), _BumpScale);
                half4 normalCol = vqt_normalToGrayScale(normal);

                // texture ramp
                if (_LightingMode == 0) {
                    float2 coord = normalCol.rg;
                    coord.r = saturate(coord.r + _ShadowOffset);
                    float4 shadow = tex2D(_ToonRamp, coord);
                    shadow.rgb = lerp(float3(1, 1, 1), shadow.rgb, _ShadowStrength);
                    float4 result = baseColor;
                    result.rgb *= shadow.rgb;
                    return result;
                }

                // multilayer math
                if (_LightingMode == 1) {
                    return poiMultiLayerMath(baseColor, uv, normalCol.r);
                }

                // realistic
                if (_LightingMode == 2 || _LightingMode == 3 || _LightingMode == 6 || _LightingMode == 7) {
                    if (_LightingMode == 2 || _LightingMode == 3) {
                        normalCol.rgb = lerp(_LightingShadowColor.rgb, float3(1, 1, 1), normalCol.r);
                        normalCol.rgb = lerp(float3(1, 1, 1), normalCol.rgb, _ShadowStrength);
                    } else  {
                        normalCol.rgb /= 0.83;
                    }
                    fixed4 result = baseColor;
                    result.rgb *= normalCol.rgb;
                    return result;
                }

                // shade map
                if (_LightingMode == 4) {
                    return poiShadeMap(baseColor, uv, normalCol.r);
                }

                // flat
                if (_LightingMode == 5 || _LightingMode == 8) {
                    return baseColor;
                }

                normalCol.rgb /= 0.83;
                baseColor.rgb *= normalCol.rgb;
                return baseColor;
            }

            float emissionMask(float4 mask, float channel) {
                if (channel == 1) {
                    return mask.r;
                }
                if (channel == 2) {
                    return mask.g;
                }
                if (channel == 3) {
                    return mask.b;
                }
                return mask.a;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // o.uv_EmissionMap = TRANSFORM_TEX(v.uv, _EmissionMap);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;

                if (_VQT_GenerateShadow && _ShadingEnabled) {
                    col = poiApplyShadow(col, i.uv);
                }

                col.rgb *= _VQT_MainTexBrightness;

                if (_EnableEmission) {
                    float4 emi = sampleTex2D(_EmissionMap, i.uv, 0.0f);
                    float4 maskTex = sampleTex2D(_EmissionMask, i.uv, 0.0f);
                    float mask = emissionMask(maskTex, _EmissionMaskChannel);
                    float strength = saturate(mask * _EmissionStrength3); // limit too strong emission.
                    float3 base = lerp(1, emi.rgb, _EmissionBaseColorAsMap);
                    col.rgb += emi.rgb * _EmissionColor.rgb * _EmissionColor.a * strength * base;
                }

                if (_EnableEmission1) {
                    float4 emi = sampleTex2D(_EmissionMap1, i.uv, 0.0f);
                    float4 maskTex = sampleTex2D(_EmissionMask1, i.uv, 0.0f);
                    float mask = emissionMask(maskTex, _EmissionMask1Channel);
                    float strength = saturate(mask * _EmissionStrength3); // limit too strong emission.
                    float3 base = lerp(1, emi.rgb, _EmissionBaseColorAsMap1);
                    col.rgb += emi.rgb * _EmissionColor1.rgb * _EmissionColor1.a * strength * base;
                }

                if (_EnableEmission2) {
                    float4 emi = sampleTex2D(_EmissionMap2, i.uv, 0.0f);
                    float4 maskTex = sampleTex2D(_EmissionMask2, i.uv, 0.0f);
                    float mask = emissionMask(maskTex, _EmissionMask2Channel);
                    float strength = saturate(mask * _EmissionStrength3); // limit too strong emission.
                    float3 base = lerp(1, emi.rgb, _EmissionBaseColorAsMap2);
                    col.rgb += emi.rgb * _EmissionColor2.rgb * _EmissionColor2.a * strength * base;
                }

                if (_EnableEmission3) {
                    float4 emi = sampleTex2D(_EmissionMap3, i.uv, 0.0f);
                    float4 maskTex = sampleTex2D(_EmissionMask3, i.uv, 0.0f);
                    float mask = emissionMask(maskTex, _EmissionMask3Channel);
                    float strength = saturate(mask * _EmissionStrength3); // limit too strong emission.
                    float3 base = lerp(1, emi.rgb, _EmissionBaseColorAsMap3);
                    col.rgb += emi.rgb * _EmissionColor3.rgb * _EmissionColor3.a * strength * base;
                }

                return saturate(col);
            }
            ENDCG
        }
    }
}
