Shader "Hidden/VRCQuestTools/Standard"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)

        [Normal] _BumpMap ("Normalmap", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1

        _EmissionMap("Emission", 2D) = "white" {}
        [HDR]_EmissionColor("EmissionColor", Color) = (1,1,1,1)

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
            #pragma multi_compile _ _EMISSION

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

            float _VQT_MainTexBrightness;
            uint _VQT_GenerateShadow;

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

                if (_VQT_GenerateShadow == 1)
                {
                    half3 normal = UnpackScaleNormal(tex2D(_BumpMap, i.uv), _BumpScale);
                    half4 normalCol = vqt_normalToGrayScale(normal);
                    col.rgb *= normalCol.rgb / 0.83; // In standard shading, normalCol multiplies 0.83 to most of the main texture. So we need to undo that.
                }

                col.rgb *= _VQT_MainTexBrightness;
#ifdef _EMISSION
                fixed4 emi = tex2D(_EmissionMap, i.uv);
                col = clamp(col + emi * _EmissionColor, 0, 1);
#endif
                col.rgb = saturate(col.rgb);
                return col;
            }
            ENDCG
        }
    }
}
