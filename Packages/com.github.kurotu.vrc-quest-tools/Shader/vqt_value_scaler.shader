Shader "Hidden/VRCQuestTools/ValueScaler"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScaleR ("Scale R", Range(0, 1)) = 1
        _ScaleG ("Scale G", Range(0, 1)) = 1
        _ScaleB ("Scale B", Range(0, 1)) = 1
        _ScaleA ("Scale A", Range(0, 1)) = 1
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
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ScaleR;
            float _ScaleG;
            float _ScaleB;
            float _ScaleA;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col.r *= _ScaleR;
                col.g *= _ScaleG;
                col.b *= _ScaleB;
                col.a *= _ScaleA;
                return col;
            }
            ENDCG
        }
    }
}
