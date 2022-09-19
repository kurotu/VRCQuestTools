Shader "Hidden/VRCQuestTools/arktoon/EmissiveFreak"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)

        _EmissionMap("EmissionMap", 2D) = "white" {}
        [HDR]_EmissionColor("EmissionColor", Color) = (1,1,1,1)

        _EmissiveFreak1Tex("EmissiveFreak1Tex", 2D) = "white" {}
        [HDR]_EmissiveFreak1Color("EmissiveFreak1Color", Color) = (1,1,1,1)

        _EmissiveFreak2Tex("EmissiveFreak2Tex", 2D) = "white" {}
        [HDR]_EmissiveFreak2Color("EmissiveFreak2Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                float2 uv_EmissiveFreak1Tex : TEXCOORD2;
                float2 uv_EmissiveFreak2Tex : TEXCOORD3;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            sampler2D _EmissionMap;
            float4 _EmissionMap_ST;
            fixed4 _EmissionColor;

            sampler2D _EmissiveFreak1Tex;
            float4 _EmissiveFreak1Tex_ST;
            fixed4 _EmissiveFreak1Color;

            sampler2D _EmissiveFreak2Tex;
            float4 _EmissiveFreak2Tex_ST;
            fixed4 _EmissiveFreak2Color;

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
                o.uv_EmissiveFreak1Tex = TRANSFORM_TEX(v.uv, _EmissiveFreak1Tex);
                o.uv_EmissiveFreak2Tex = TRANSFORM_TEX(v.uv, _EmissiveFreak2Tex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;
                float4 emi = sampleTex2D(_EmissionMap, i.uv_EmissionMap, 0.0f);
                col = clamp(col + emi * _EmissionColor, 0, 1);

                float4 ef1 = sampleTex2D(_EmissiveFreak1Tex, i.uv_EmissiveFreak1Tex, 0.0f);
                col = clamp(col + ef1 * _EmissiveFreak1Color, 0, 1);

                float4 ef2 = sampleTex2D(_EmissiveFreak2Tex, i.uv_EmissiveFreak2Tex, 0.0f);
                col = clamp(col + ef2 * _EmissiveFreak2Color, 0, 1);

                return col;
            }
            ENDCG
        }
    }
}
