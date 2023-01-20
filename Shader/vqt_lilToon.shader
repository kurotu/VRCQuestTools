Shader "Hidden/VRCQuestTools/lilToon"
{
    Properties
    {
        _LIL_FEATURE_EMISSION_1ST("LIL_FEATURE_EMISSION_1ST", Int) = 0
        _LIL_FEATURE_EMISSION_2ND("LIL_FEATURE_EMISSION_2ND", Int) = 0
        _LIL_FEATURE_ANIMATE_EMISSION_UV("LIL_FEATURE_ANIMATE_EMISSION_UV", Int) = 0
        _LIL_FEATURE_ANIMATE_EMISSION_MASK_UV("LIL_FEATURE_ANIMATE_EMISSION_MASK_UV", Int) = 0
        _LIL_FEATURE_EMISSION_GRADATION("LIL_FEATURE_EMISSION_GRADATION", Int) = 0

        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_Color("Color", Color) = (1,1,1,1)

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

        _VQT_MainTexBrightness("VQT Main Texture Brightness", Range(0, 1)) = 1
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
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
                float2 uv : TEXCOORD0;
                float2 uv_EmissionMap : TEXCOORD1;
                float2 uv_EmissionBlendMask : TEXCOORD2;
                float2 uv_Emission2ndMap : TEXCOORD4;
                float2 uv_Emission2ndBlendMask : TEXCOORD5;
                float4 vertex : SV_POSITION;
            };

            uint _LIL_FEATURE_EMISSION_1ST;
            uint _LIL_FEATURE_EMISSION_2ND;
            uint _LIL_FEATURE_ANIMATE_EMISSION_UV;
            uint _LIL_FEATURE_ANIMATE_EMISSION_MASK_UV;
            uint _LIL_FEATURE_EMISSION_GRADATION;

            sampler2D _MainTex;
            float4 _MainTex_ST;

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

            float _VQT_MainTexBrightness;

            float4 sampleTex2D(sampler2D tex, float2 uv, float angle) {
                half angleCos           = cos(angle);
                half angleSin           = sin(angle);
                half2x2 rotateMatrix    = half2x2(angleCos, -angleSin, angleSin, angleCos);
                float2 newUV = mul(rotateMatrix, uv - 0.5) + 0.5;
                return tex2D(tex, newUV);
            }

            float3 emissionRGB(fixed4 emiColor, float4 emi, float emiBlend, float4 emiMask) {
                return emi.rgb * emiColor.rgb * emi.a * emiColor.a * emiBlend * emiMask.rgb * emiMask.a;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv_EmissionMap = TRANSFORM_TEX(v.uv, _EmissionMap);
                o.uv_EmissionBlendMask = TRANSFORM_TEX(v.uv, _EmissionBlendMask);
                o.uv_Emission2ndMap = TRANSFORM_TEX(v.uv, _Emission2ndMap);
                o.uv_Emission2ndBlendMask = TRANSFORM_TEX(v.uv, _Emission2ndBlendMask);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb *= _VQT_MainTexBrightness;

                if (_LIL_FEATURE_EMISSION_1ST && _UseEmission) {
                    float angle;
                    angle = _LIL_FEATURE_ANIMATE_EMISSION_UV ? _EmissionMap_ScrollRotate.b : 0.0f;
                    float4 emi = sampleTex2D(_EmissionMap, i.uv_EmissionMap, angle);
                    angle = _LIL_FEATURE_ANIMATE_EMISSION_MASK_UV ? _EmissionBlendMask_ScrollRotate.b : 0.0f;
                    float4 emiMask = sampleTex2D(_EmissionBlendMask, i.uv_EmissionBlendMask, angle);
                    emi.rgb = emissionRGB(_EmissionColor, emi, _EmissionBlend, emiMask);
                    if (_LIL_FEATURE_EMISSION_GRADATION && _EmissionUseGrad) {
                        // Use first color
                        fixed4 c = _EmissionGradTex.Sample(sampler_EmissionGradTex, 0);
                        emi.rgb *= c.rgb;
                    }
                    emi.rgb = lerp(emi.rgb, 0, _EmissionFluorescence);
                    if (_EmissionBlendMode == 0) col.rgb = emi.rgb;
                    if (_EmissionBlendMode == 1) col.rgb += emi.rgb;
                    if (_EmissionBlendMode == 2) col.rgb = col.rgb + emi.rgb - col.rgb * emi.rgb;
                    if (_EmissionBlendMode == 3) col.rgb *= emi.rgb;
                }

                if (_LIL_FEATURE_EMISSION_2ND && _UseEmission2nd) {
                    float angle;
                    angle = _LIL_FEATURE_ANIMATE_EMISSION_UV ? _Emission2ndMap_ScrollRotate.b : 0.0f;
                    float4 emi = sampleTex2D(_Emission2ndMap, i.uv_Emission2ndMap, angle);
                    angle = _LIL_FEATURE_ANIMATE_EMISSION_MASK_UV ? _Emission2ndBlendMask_ScrollRotate.b : 0.0f;
                    float4 emiMask = sampleTex2D(_Emission2ndBlendMask, i.uv_Emission2ndBlendMask, angle);
                    emi.rgb = emissionRGB(_Emission2ndColor, emi, _Emission2ndBlend, emiMask);
                    if (_LIL_FEATURE_EMISSION_GRADATION && _Emission2ndUseGrad) {
                        // Use first color
                        fixed4 c = _Emission2ndGradTex.Sample(sampler_Emission2ndGradTex, 0);
                        emi.rgb *= c.rgb;
                    }
                    emi.rgb = lerp(emi.rgb, 0, _Emission2ndFluorescence);
                    if (_Emission2ndBlendMode == 0) col.rgb = emi.rgb;
                    if (_Emission2ndBlendMode == 1) col.rgb += emi.rgb;
                    if (_Emission2ndBlendMode == 2) col.rgb = col.rgb + emi.rgb - col.rgb * emi.rgb;
                    if (_Emission2ndBlendMode == 3) col.rgb *= emi.rgb;
                }
                return col;
            }

            ENDCG
        }
    }
}
