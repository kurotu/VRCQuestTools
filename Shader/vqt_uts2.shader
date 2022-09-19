Shader "Hidden/VRCQuestTools/UTS2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)

        _Emissive_Tex("Emissive_Tex", 2D) = "white" {}
        [HDR]_Emissive_Color("Emissive_Color", Color) = (1,1,1,1)
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
                float2 uv_Emissive_Tex : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            sampler2D _Emissive_Tex;
            float4 _Emissive_Tex_ST;
            fixed4 _Emissive_Color;

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
                o.uv_Emissive_Tex = TRANSFORM_TEX(v.uv, _Emissive_Tex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;
                float4 emi = sampleTex2D(_Emissive_Tex, i.uv_Emissive_Tex, 0.0f);
                col = clamp(col + emi * _Emissive_Color, 0, 1);
                return col;
            }
            ENDCG
        }
    }
}
