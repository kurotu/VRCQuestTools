Shader "Hidden/VRCQuestTools/StandardLite/lilToon_metallic_smoothness"
{
    Properties
    {
        _MetallicGlossMap ("Metallic Gloss Map", 2D) = "white" {}
        _SmoothnessTex ("Smoothness Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

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

            sampler2D _MetallicGlossMap;
            sampler2D _SmoothnessTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 metallicGloss = tex2D(_MetallicGlossMap, i.uv);
                float4 smoothness = tex2D(_SmoothnessTex, i.uv);

                float4 output;
                output.r = metallicGloss.r;
                output.g = 1.0;
                output.b = 1.0;
                output.a = smoothness.r;

                return output;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
