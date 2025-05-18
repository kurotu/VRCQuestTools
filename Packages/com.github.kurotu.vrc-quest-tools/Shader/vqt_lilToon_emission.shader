Shader "Hidden/VRCQuestTools/lilToon/Emission"
{
    Properties
    {
        _VQT_AlbedoTex("Albedo Texture", 2D) = "black" {}
        _VQT_AlbedoBrightness("Albedo Brightness", Range(0, 1)) = 1

        _LIL_FEATURE_EMISSION_1ST("LIL_FEATURE_EMISSION_1ST", Int) = 1
        _LIL_FEATURE_EMISSION_2ND("LIL_FEATURE_EMISSION_2ND", Int) = 1
        _LIL_FEATURE_ANIMATE_EMISSION_UV("LIL_FEATURE_ANIMATE_EMISSION_UV", Int) = 1
        _LIL_FEATURE_ANIMATE_EMISSION_MASK_UV("LIL_FEATURE_ANIMATE_EMISSION_MASK_UV", Int) = 1
        _LIL_FEATURE_EMISSION_GRADATION("LIL_FEATURE_EMISSION_GRADATION", Int) = 1

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
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityStandardUtils.cginc"

            sampler2D _VQT_AlbedoTex;
            float4 _VQT_AlbedoTex_ST;
            float _VQT_AlbedoBrightness;

            uint _LIL_FEATURE_EMISSION_1ST;
            uint _LIL_FEATURE_EMISSION_2ND;
            uint _LIL_FEATURE_ANIMATE_EMISSION_UV;
            uint _LIL_FEATURE_ANIMATE_EMISSION_MASK_UV;
            uint _LIL_FEATURE_EMISSION_GRADATION;

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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;

                float2 uv_EmissionMap : TEXCOORD2;
                float2 uv_EmissionBlendMask : TEXCOORD3;
                float2 uv_Emission2ndMap : TEXCOORD4;
                float2 uv_Emission2ndBlendMask : TEXCOORD5;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                o.uv_EmissionMap = TRANSFORM_TEX(v.uv, _EmissionMap);
                o.uv_EmissionBlendMask = TRANSFORM_TEX(v.uv, _EmissionBlendMask);
                o.uv_Emission2ndMap = TRANSFORM_TEX(v.uv, _Emission2ndMap);
                o.uv_Emission2ndBlendMask = TRANSFORM_TEX(v.uv, _Emission2ndBlendMask);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 albedo = sampleTex2D(_VQT_AlbedoTex, i.uv, 0);
                fixed4 col = albedo;
                col.rgb *= _VQT_AlbedoBrightness;

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
