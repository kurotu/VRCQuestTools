Shader "Hidden/VRCQuestTools/arktoon/Opaque"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)

        [Normal] _BumpMap ("Normalmap", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1

        _EmissionMap("EmissionMap", 2D) = "white" {}
        [HDR]_EmissionColor("EmissionColor", Color) = (1,1,1,1)

        _Shadowborder ("[Shadow] border ", Range(0, 1)) = 0.6
        _ShadowborderBlur ("[Shadow] border Blur", Range(0, 1)) = 0.05
        _ShadowStrength ("[Shadow] Strength", Range(0, 1)) = 0.5
        _ShadowStrengthMask ("[Shadow] Strength Mask", 2D) = "white" {}

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
                float2 uv_EmissionMap : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            sampler2D _BumpMap;
            float _BumpScale;

            sampler2D _EmissionMap;
            float4 _EmissionMap_ST;
            fixed4 _EmissionColor;

            float _Shadowborder;
            float _ShadowborderBlur;
            float _ShadowStrength;
            sampler2D _ShadowStrengthMask;
            fixed4 _ShadowStrengthMask_ST;

            float _VQT_MainTexBrightness;
            uint _VQT_GenerateShadow;

            float4 sampleTex2D(sampler2D tex, float2 uv, float angle) {
              half angleCos = cos(angle);
              half angleSin = sin(angle);
              half2x2 rotateMatrix = half2x2(angleCos, -angleSin, angleSin, angleCos);
              float2 newUV = mul(rotateMatrix, uv - 0.5) + 0.5;
              return tex2D(tex, newUV);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv_EmissionMap = TRANSFORM_TEX(v.uv, _EmissionMap);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;

                if (_VQT_GenerateShadow) {
                    half3 normal = UnpackScaleNormal(tex2D(_BumpMap, i.uv), _BumpScale);
                    half4 normalCol = vqt_normalToGrayScale(normal);
                    float4 mask = tex2D(_ShadowStrengthMask, i.uv);
                    float strength = mask * _ShadowStrength;
                    fixed4 shadow = arktoonShadow(normalCol, _Shadowborder, _ShadowborderBlur, strength);
                    col.rgb *= shadow.rgb;
                }

                col.rgb *= _VQT_MainTexBrightness;
                float4 emi = sampleTex2D(_EmissionMap, i.uv_EmissionMap, 0.0f);
                col = clamp(col + emi * _EmissionColor, 0, 1);
                return col;
            }
            ENDCG
        }
    }
}
