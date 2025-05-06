Shader "VRCQuestTools/NormalToColor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BumpMap ("Bump Map", 2D) = "bump" {}
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

            struct appdata_t
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
            sampler2D _BumpMap;
            float4 _MainTex_ST;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 bumpColor = tex2D(_BumpMap, i.uv);
                half3 bump = UnpackNormal(bumpColor);
                half4 color;
                color.r = (bump.r + 1.0) / 2.0;
                color.g = (bump.g + 1.0) / 2.0;
                color.b = (bump.b + 1.0) / 2.0;
                color.a = 1;
                /*
                color.r = 0.5;
                color.g = 0.5;
                color.b = 1;*/
                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
