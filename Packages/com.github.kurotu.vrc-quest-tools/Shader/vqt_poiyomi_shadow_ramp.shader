Shader "Hidden/VRCQuestTools/Poiyomi/ShadowRamp"
{
    // Generates a ToonStandard shadow ramp (128x16) from Poiyomi shading parameters.
    // The x UV coordinate (0=dark, 1=lit) represents the light intensity, and the
    // fragment outputs the shadow tint multiplier to apply to the albedo.
    Properties
    {
        _ShadingEnabled("Enable Shading", Float) = 1
        [KeywordEnum(TextureRamp, Multilayer Math, Wrapped, Skin, ShadeMap, Flat, Realistic, Cloth, SDF)] _LightingMode("Lighting Type", Float) = 5
        _LightingShadowColor("Shadow Tint", Color) = (1, 1, 1)

        // Mode 0 - TextureRamp
        _ToonRamp("Lighting Ramp", 2D) = "white" {}
        _ShadowOffset("Ramp Offset", Range(-1, 1)) = 0

        // Mode 1 - Multilayer Math
        _ShadowColor("Shadow Color", Color) = (0.7, 0.75, 0.85, 1.0)
        _ShadowBorder("Shadow Border", Range(0, 1)) = 0.5
        _ShadowBlur("Shadow Blur", Range(0, 1)) = 0.1
        _Shadow2ndColor("Shadow 2nd Color", Color) = (0, 0, 0, 0)
        _Shadow2ndBorder("Shadow 2nd Border", Range(0, 1)) = 0.5
        _Shadow2ndBlur("Shadow 2nd Blur", Range(0, 1)) = 0.3
        _Shadow3rdColor("Shadow 3rd Color", Color) = (0, 0, 0, 0)
        _Shadow3rdBorder("Shadow 3rd Border", Range(0, 1)) = 0.25
        _Shadow3rdBlur("Shadow 3rd Blur", Range(0, 1)) = 0.1

        // Mode 4 - ShadeMap (texture sampling omitted; pure color values used for ramp)
        _1st_ShadeColor("1st Shade Color", Color) = (1, 1, 1)
        _2nd_ShadeColor("2nd Shade Color", Color) = (1, 1, 1)
        _BaseColor_Step("BaseColor_Step", Range(0.01, 1)) = 0.5
        _BaseShade_Feather("Base/Shade_Feather", Range(0.0001, 1)) = 0.0001
        _ShadeColor_Step("ShadeColor_Step", Range(0, 1)) = 0
        _1st2nd_Shades_Feather("1st/2nd_Shades_Feather", Range(0.0001, 1)) = 0.0001

        _ShadowStrength("Shadow Strength", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _ShadingEnabled;
            float _LightingMode;
            float3 _LightingShadowColor;

            sampler2D _ToonRamp;
            float _ShadowOffset;

            float4 _ShadowColor;
            float _ShadowBorder;
            float _ShadowBlur;
            float4 _Shadow2ndColor;
            float _Shadow2ndBorder;
            float _Shadow2ndBlur;
            float4 _Shadow3rdColor;
            float _Shadow3rdBorder;
            float _Shadow3rdBlur;

            float3 _1st_ShadeColor;
            float3 _2nd_ShadeColor;
            float _BaseColor_Step;
            float _BaseShade_Feather;
            float _ShadeColor_Step;
            float _1st2nd_Shades_Feather;

            float _ShadowStrength;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // UV x-axis: 0 = fully in shadow, 1 = fully lit
                float light = i.uv.x;
                float3 shadowTint = float3(1, 1, 1);

                if (_ShadingEnabled > 0.5)
                {
                    if (_LightingMode < 0.5)
                    {
                        // Mode 0: TextureRamp - sample ramp directly with offset
                        float2 coord = float2(saturate(light + _ShadowOffset), 0.5);
                        float3 ramp = tex2D(_ToonRamp, coord).rgb;
                        shadowTint = lerp(float3(1, 1, 1), ramp, _ShadowStrength);
                    }
                    else if (_LightingMode < 1.5)
                    {
                        // Mode 1: Multilayer Math
                        float3 shadow = float3(1, 1, 1);

                        float s1Max = _ShadowBorder + _ShadowBlur / 2;
                        float s1Min = _ShadowBorder - _ShadowBlur / 2;
                        if (light < s1Max && _ShadowColor.a > 0)
                        {
                            float blend = saturate((light - s1Min) / max(_ShadowBlur, 0.0001));
                            shadow = lerp(_ShadowColor.rgb, shadow, blend);
                        }

                        if (_Shadow2ndColor.a > 0)
                        {
                            float b2Min = _Shadow2ndBorder - _Shadow2ndBlur / 2;
                            float blend2 = saturate((light - b2Min) / max(_Shadow2ndBlur, 0.0001));
                            shadow = lerp(_Shadow2ndColor.rgb, shadow, blend2);
                        }

                        if (_Shadow3rdColor.a > 0)
                        {
                            float b3Min = _Shadow3rdBorder - _Shadow3rdBlur / 2;
                            float blend3 = saturate((light - b3Min) / max(_Shadow3rdBlur, 0.0001));
                            shadow = lerp(_Shadow3rdColor.rgb, shadow, blend3);
                        }

                        shadowTint = lerp(float3(1, 1, 1), shadow, _ShadowStrength);
                    }
                    else if (_LightingMode < 4.5)
                    {
                        if (_LightingMode < 3.5)
                        {
                            // Modes 2/3: Wrapped/Skin - tint-based shadow
                            float3 tint = lerp(_LightingShadowColor.rgb, float3(1, 1, 1), light);
                            shadowTint = lerp(float3(1, 1, 1), tint, _ShadowStrength);
                        }
                        else
                        {
                            // Mode 4: ShadeMap - use shade colors (textures approximated as white for ramp)
                            float3 result = float3(1, 1, 1);

                            float3 shade1 = _1st_ShadeColor.rgb;
                            float baseStepMin = _BaseColor_Step - _BaseShade_Feather;
                            result = lerp(shade1, result, saturate((light - baseStepMin) / max(_BaseShade_Feather, 0.0001)));

                            float3 shade2 = _2nd_ShadeColor.rgb;
                            float shadeStepMin = _ShadeColor_Step - _1st2nd_Shades_Feather;
                            result = lerp(shade2, result, saturate((light - shadeStepMin) / max(_1st2nd_Shades_Feather, 0.0001)));

                            shadowTint = result;
                        }
                    }
                    else if (_LightingMode > 5.5)
                    {
                        // Modes 6/7/8: Realistic/Cloth/SDF - linear brightness ramp
                        shadowTint = float3(min(light / 0.83, 1.0), min(light / 0.83, 1.0), min(light / 0.83, 1.0));
                    }
                    // Mode 5 (Flat): white - no shadow modification
                }

                return fixed4(shadowTint, 1);
            }
            ENDCG
        }
    }
}
