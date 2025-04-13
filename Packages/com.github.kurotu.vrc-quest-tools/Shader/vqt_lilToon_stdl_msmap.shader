Shader "Hidden/VRCQuestTools/StandardLite/lilToon_metallic_smoothness"
{
    Properties
    {
        _MetallicGlossMap ("Metallic Gloss Map", 2D) = "white" {}
        _ReflectionColorTex ("Reflection Color Tex", 2D) = "white" {}
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

            sampler2D _MetallicGlossMap;
            sampler2D _ReflectionColorTex;
            sampler2D _SmoothnessTex;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 metallicGloss = tex2D(_MetallicGlossMap, i.uv).rgb;
                float3 reflectionColor = tex2D(_ReflectionColorTex, i.uv).rgb;
                float3 smoothness = tex2D(_SmoothnessTex, i.uv).rgb;

                float reflectionColorBrightness = dot(reflectionColor, float3(0.299, 0.587, 0.114));

                fixed4 output;
                output.r = metallicGloss.r * reflectionColorBrightness;
                output.g = 1.0;
                output.b = 1.0;
                output.a = smoothness.r;

                return output;
            }
            ENDCG
        }
    }
}
