Shader "Hidden/VRCQuestTools/MetallicSmoothnes"
{
    Properties
    {
        _MetallicMap("Metallic", 2D) = "white" {}
        _InvertMetallic("Invert Metallic", Int) = 0

        _SmoothnessMap("Smoothness", 2D) = "white" {}
        _InvertSmoothness("Invert Smoothness", Int) = 0
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

            sampler2D _MetallicMap;
            float4 _MetallicMap_ST;
            float _InvertMetallic;

            sampler2D _SmoothnessMap;
            float _InvertSmoothness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MetallicMap);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 metal = tex2D(_MetallicMap, i.uv);
                fixed4 smooth = tex2D(_SmoothnessMap, i.uv);
                fixed4 OUT = (0.0f, 0.0f, 0.0f, 0.0f);
                OUT.r = _InvertMetallic ? 1.0f - metal : metal;
                OUT.a = _InvertSmoothness ? 1.0f - smooth : smooth;
                return OUT;
            }
            ENDCG
        }
    }
}
