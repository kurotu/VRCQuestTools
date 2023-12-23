Shader "Hidden/VRCQuestTools/lilToon"
{
    Properties
    {
        _LIL_FEATURE_NORMAL_1ST("LIL_FEATURE_NORMAL_1ST", Int) = 1
        _LIL_FEATURE_EMISSION_1ST("LIL_FEATURE_EMISSION_1ST", Int) = 1
        _LIL_FEATURE_EMISSION_2ND("LIL_FEATURE_EMISSION_2ND", Int) = 1
        _LIL_FEATURE_ANIMATE_EMISSION_UV("LIL_FEATURE_ANIMATE_EMISSION_UV", Int) = 1
        _LIL_FEATURE_ANIMATE_EMISSION_MASK_UV("LIL_FEATURE_ANIMATE_EMISSION_MASK_UV", Int) = 1
        _LIL_FEATURE_EMISSION_GRADATION("LIL_FEATURE_EMISSION_GRADATION", Int) = 1

        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_Color("Color", Color) = (1,1,1,1)

        _UseShadow("Use Shadow", Int) = 0
        _ShadowStrength("sStrength", Range(0, 1)) = 1
        _ShadowStrengthMask("sStrength", 2D) = "white" {}
        _ShadowColor("Shadow 1st Color", Color) = (0.5, 0.5, 0.5, 1)
        _ShadowColorTex("Shadow Color", 2D) = "black" {}
        _ShadowBorder("Shadow 1st Border", Range(0, 1)) = 0.5
        _ShadowBlur("Shadow 1st Blur", Range(0, 1)) = 0.5
        _Shadow2ndColor("Shadow 2nd Color", Color) = (0.3, 0.3, 0.3, 1)
        _Shadow2ndColorTex("2nd Color", 2D) = "black" {}
        _Shadow2ndBorder("Shadow 2nd Border", Range(0, 1)) = 0.3
        _Shadow2ndBlur("Shadow 2nd Blur", Range(0, 1)) = 0.3
        _Shadow3rdColor("Shadow 3rd Color", Color) = (0, 0, 0, 1)
        _Shadow3rdColorTex("2nd Color", 2D) = "black" {}
        _Shadow3rdBorder("Shadow 3rd Border", Range(0, 1)) = 0.1
        _Shadow3rdBlur("Shadow 3rd Blur", Range(0, 1)) = 0.1
        // _ShadowBorderColor("Shadow Border Color", Color) = (1, 0, 0, 1)
        // _ShadowBorderRange("Shadow Border Range", Range(0, 1)) = 0.1

        _UseBumpMap("Use Normal Map", Int) = 0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1

        _UseEmission("Use Emission", Int) = 0
        [HDR]_EmissionColor("Color", Color) = (1,1,1,1)
        _EmissionMap("Texture", 2D) = "white" {}
        _EmissionMap_ScrollRotate("Angle|UV Animation|Scroll|Rotate", Vector) = (0,0,0,0)
        // _EmissionMap_UVMode("UV Mode|UV0|UV1|UV2|UV3|Rim", Int) = 0
        _EmissionBlend("Blend", Range(0,1)) = 1
        _EmissionBlendMask("Mask", 2D) = "white" {}
        _EmissionBlendMask_ScrollRotate("Angle|UV Animation|Scroll|Rotate", Vector) = (0,0,0,0)
        _EmissionBlendMode("Blend Mode|Normal|Add|Screen|Multiply", Int) = 1
        // _EmissionBlink("Blink Strength|Blink Type|Blink Speed|Blink Offset", Vector) = (0,0,3.141593,0)
        _EmissionUseGrad("Use Gradation", Int) = 0
        _EmissionGradTex("Gradation Texture", 2D) = "white" {}
        _EmissionGradSpeed("Gradation Speed", Float) = 1
        // _EmissionParallaxDepth("Parallax Depth", float) = 0
        _EmissionFluorescence("Fluorescence", Range(0,1)) = 0
        _EmissionMainStrength("EmissionMainStrength", Range(0,1)) = 0

        _UseEmission2nd("Use Emission", Int) = 0
        [HDR]_Emission2ndColor("Color", Color) = (1,1,1,1)
        _Emission2ndMap("Texture", 2D) = "white" {}
        _Emission2ndMap_ScrollRotate("Angle|UV Animation|Scroll|Rotate", Vector) = (0,0,0,0)
        // _Emission2ndMap_UVMode("UV Mode|UV0|UV1|UV2|UV3|Rim", Int) = 0
        _Emission2ndBlend("Blend", Range(0,1)) = 1
        _Emission2ndBlendMask("Mask", 2D) = "white" {}
        _Emission2ndBlendMask_ScrollRotate("Angle|UV Animation|Scroll|Rotate", Vector) = (0,0,0,0)
        _Emission2ndBlendMode("Blend Mode|Normal|Add|Screen|Multiply", Int) = 1
        // _Emission2ndBlink("Blink Strength|Blink Type|Blink Speed|Blink Offset", Vector) = (0,0,3.141593,0)
        _Emission2ndUseGrad("Use Gradation", Int) = 0
        _Emission2ndGradTex("Gradation Texture", 2D) = "white" {}
        _Emission2ndGradSpeed("Gradation Speed", Float) = 1
        // _Emission2ndParallaxDepth("Parallax Depth", float) = 0
        _Emission2ndFluorescence("Fluorescence", Range(0,1)) = 0
        _Emission2ndMainStrength("Emission2ndMainStrength", Range(0,1)) = 0

        _VQT_MainTexBrightness("VQT Main Texture Brightness", Range(0, 1)) = 1
        _VQT_GenerateShadow("VQT Generate Shadow", Int) = 1
    }
    SubShader
    {
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
                float2 uv_BumpMap : TEXCOORD1;
                float2 uv_EmissionMap : TEXCOORD2;
                float2 uv_EmissionBlendMask : TEXCOORD3;
                float2 uv_Emission2ndMap : TEXCOORD4;
                float2 uv_Emission2ndBlendMask : TEXCOORD5;
                float4 vertex : SV_POSITION;
            };

            uint _LIL_FEATURE_NORMAL_1ST;
            uint _LIL_FEATURE_EMISSION_1ST;
            uint _LIL_FEATURE_EMISSION_2ND;
            uint _LIL_FEATURE_ANIMATE_EMISSION_UV;
            uint _LIL_FEATURE_ANIMATE_EMISSION_MASK_UV;
            uint _LIL_FEATURE_EMISSION_GRADATION;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            uint _UseShadow;
            float _ShadowStrength;
            sampler2D _ShadowStrengthMask;
            fixed4 _ShadowColor;
            sampler2D _ShadowColorTex;
            float4 _ShadowColorTex_ST;
            float _ShadowBorder;
            float _ShadowBlur;
            fixed4 _Shadow2ndColor;
            sampler2D _Shadow2ndColorTex;
            float4 _Shadow2ndColorTex_ST;
            float _Shadow2ndBorder;
            float _Shadow2ndBlur;
            fixed4 _Shadow3rdColor;
            sampler2D _Shadow3rdColorTex;
            float4 _Shadow3rdColorTex_ST;
            float _Shadow3rdBorder;
            float _Shadow3rdBlur;

            uint _UseBumpMap;
            sampler2D _BumpMap;
            float4 _BumpMap_ST;
            float _BumpScale;

            uint _UseEmission;
            fixed4 _EmissionColor;
            sampler2D _EmissionMap;
            float4 _EmissionMap_ST;
            fixed4 _EmissionMap_ScrollRotate;
            float _EmissionBlend;
            sampler2D _EmissionBlendMask;
            float4 _EmissionBlendMask_ST;
            fixed4 _EmissionBlendMask_ScrollRotate;
            uint _EmissionBlendMode;
            uint _EmissionUseGrad;
            Texture2D _EmissionGradTex;
            SamplerState sampler_EmissionGradTex;
            float _EmissionFluorescence;
            float _EmissionMainStrength;

            uint _UseEmission2nd;
            fixed4 _Emission2ndColor;
            sampler2D _Emission2ndMap;
            float4 _Emission2ndMap_ST;
            fixed4 _Emission2ndMap_ScrollRotate;
            float _Emission2ndBlend;
            sampler2D _Emission2ndBlendMask;
            float4 _Emission2ndBlendMask_ST;
            fixed4 _Emission2ndBlendMask_ScrollRotate;
            uint _Emission2ndBlendMode;
            uint _Emission2ndUseGrad;
            Texture2D _Emission2ndGradTex;
            SamplerState sampler_Emission2ndGradTex;
            float _Emission2ndFluorescence;
            float _Emission2ndMainStrength;

            float _VQT_MainTexBrightness;
            uint _VQT_GenerateShadow;

            float4 sampleTex2D(sampler2D tex, float2 uv, float angle) {
                half angleCos           = cos(angle);
                half angleSin           = sin(angle);
                half2x2 rotateMatrix    = half2x2(angleCos, -angleSin, angleSin, angleCos);
                float2 newUV = mul(rotateMatrix, uv - 0.5) + 0.5;
                return tex2D(tex, newUV);
            }

            float4 emissionRGB(fixed4 emiColor, float4 emi, float emiBlend, float4 emiMask, float4 albedo, float mainStrength) {
                float4 emi_tmp = emi * emiColor;
                float4 ret = lerp(emi_tmp, emi_tmp * albedo, mainStrength) * emiMask;
                ret.rgb *= emiBlend;
                return ret;
            }

            float4 compose(float4 base, float4 foreground, uint mode) {
                float3 add = base.rgb + foreground.rgb;
                float3 mul = base.rgb * foreground.rgb;
                float3 outCol;
                if (mode == 0) outCol = foreground.rgb;
                if (mode == 1) outCol = add;
                if (mode == 2) outCol = add - mul;
                if (mode == 3) outCol = mul;
                float4 ret;
                ret.rgb = lerp(base.rgb, outCol.rgb, foreground.a);
                ret.a = base.a;
                return ret;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv_BumpMap = TRANSFORM_TEX(v.uv, _BumpMap);
                o.uv_EmissionMap = TRANSFORM_TEX(v.uv, _EmissionMap);
                o.uv_EmissionBlendMask = TRANSFORM_TEX(v.uv, _EmissionBlendMask);
                o.uv_Emission2ndMap = TRANSFORM_TEX(v.uv, _Emission2ndMap);
                o.uv_Emission2ndBlendMask = TRANSFORM_TEX(v.uv, _Emission2ndBlendMask);
                return o;
            }

            fixed4 lilShadow(float lightValue, float2 uv) {
                fixed4 white = fixed4(1, 1, 1, 1);

                float shadowBorderMax = _ShadowBorder + _ShadowBlur / 2;
                float shadowBorderMin = _ShadowBorder - _ShadowBlur / 2;
                if (lightValue >= shadowBorderMax) {
                    return white;
                }

                fixed4 shadow = fixed4(1, 1, 1, 1);

                // shadow1
                if (lightValue < shadowBorderMax) {
                    float rate = (lightValue - shadowBorderMin) / (shadowBorderMax - shadowBorderMin);
                    fixed4 shadowColor = lerp(_ShadowColor, white, saturate(rate));
                    shadow.rgb = lerp(shadow.rgb, shadow.rgb * shadowColor.rgb, shadowColor.a);
                }

                // shadow2
                float shadow2BorderMax = _Shadow2ndBorder + _Shadow2ndBlur / 2;
                float shadow2BorderMin = _Shadow2ndBorder - _Shadow2ndBlur / 2;
                if (lightValue < shadow2BorderMax) {
                    float rate = (lightValue - shadow2BorderMin) / (shadow2BorderMax - shadow2BorderMin);
                    fixed4 shadow2Color = lerp(_Shadow2ndColor, white, saturate(rate));
                    shadow.rgb = lerp(shadow.rgb, shadow.rgb * shadow2Color.rgb, shadow2Color.a);
                }

                // shadow3
                float shadow3BorderMax = _Shadow3rdBorder + _Shadow3rdBlur / 2;
                float shadow3BorderMin = _Shadow3rdBorder - _Shadow3rdBlur / 2;
                if (lightValue < shadow3BorderMax) {
                    float rate = (lightValue - shadow3BorderMin) / (shadow3BorderMax - shadow3BorderMin);
                    fixed4 shadow3Color = lerp(_Shadow3rdColor, white, saturate(rate));
                    shadow.rgb = lerp(shadow.rgb, shadow.rgb * shadow3Color.rgb, shadow3Color.a);
                }

                float4 strengthMask = tex2D(_ShadowStrengthMask, uv);
                float strength = strengthMask.rgb * _ShadowStrength;
                shadow.rgb = lerp(white.rgb, shadow.rgb, strength);

                return shadow;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 albedo = tex2D(_MainTex, i.uv);
                if (_VQT_GenerateShadow && _LIL_FEATURE_NORMAL_1ST && _UseShadow && _UseBumpMap) {
                    half3 normal = UnpackScaleNormal(tex2D(_BumpMap, i.uv_BumpMap), _BumpScale);
                    half4 normalCol = vqt_normalToGrayScale(normal);

                    fixed4 shadow = lilShadow(normalCol, i.uv);

                    albedo.rgb *= shadow.rgb;
                }

                albedo.rgb *= _VQT_MainTexBrightness;
                fixed4 col = albedo;

                if (_LIL_FEATURE_EMISSION_1ST && _UseEmission) {
                    float angle;
                    angle = _LIL_FEATURE_ANIMATE_EMISSION_UV ? _EmissionMap_ScrollRotate.b : 0.0f;
                    float4 emi = sampleTex2D(_EmissionMap, i.uv_EmissionMap, angle);
                    angle = _LIL_FEATURE_ANIMATE_EMISSION_MASK_UV ? _EmissionBlendMask_ScrollRotate.b : 0.0f;
                    float4 emiMask = sampleTex2D(_EmissionBlendMask, i.uv_EmissionBlendMask, angle);
                    emi = emissionRGB(_EmissionColor, emi, _EmissionBlend, emiMask, albedo, _EmissionMainStrength);
                    if (_LIL_FEATURE_EMISSION_GRADATION && _EmissionUseGrad) {
                        // Use first color
                        fixed4 c = _EmissionGradTex.Sample(sampler_EmissionGradTex, 0);
                        emi.rgb *= c.rgb;
                    }
                    emi.rgb = lerp(emi.rgb, 0, _EmissionFluorescence);
                    col = compose(col, emi, _EmissionBlendMode);
                }

                if (_LIL_FEATURE_EMISSION_2ND && _UseEmission2nd) {
                    float angle;
                    angle = _LIL_FEATURE_ANIMATE_EMISSION_UV ? _Emission2ndMap_ScrollRotate.b : 0.0f;
                    float4 emi = sampleTex2D(_Emission2ndMap, i.uv_Emission2ndMap, angle);
                    angle = _LIL_FEATURE_ANIMATE_EMISSION_MASK_UV ? _Emission2ndBlendMask_ScrollRotate.b : 0.0f;
                    float4 emiMask = sampleTex2D(_Emission2ndBlendMask, i.uv_Emission2ndBlendMask, angle);
                    emi.rgb = emissionRGB(_Emission2ndColor, emi, _Emission2ndBlend, emiMask, albedo, _Emission2ndMainStrength);
                    if (_LIL_FEATURE_EMISSION_GRADATION && _Emission2ndUseGrad) {
                        // Use first color
                        fixed4 c = _Emission2ndGradTex.Sample(sampler_Emission2ndGradTex, 0);
                        emi.rgb *= c.rgb;
                    }
                    emi.rgb = lerp(emi.rgb, 0, _Emission2ndFluorescence);
                    col = compose(col, emi, _Emission2ndBlendMode);
                }
                return col;
            }

            ENDCG
        }
    }
}
