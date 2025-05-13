Shader "Hidden/VRCQuestTools/Grayscale"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutputToR ("Output To R", Range(0, 1)) = 0
        _OutputToG ("Output To G", Range(0, 1)) = 0
        _OutputToB ("Output To B", Range(0, 1)) = 0
        _OutputToA ("Output To A", Range(0, 1)) = 0
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
            float _OutputToR;
            float _OutputToG;
            float _OutputToB;
            float _OutputToA;

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

                // Rec.709
                float grayscale = dot(col.rgb, float3(0.2126, 0.7152, 0.0722));

                fixed4 output = fixed4(1, 1, 1, 1);
                if (_OutputToR > 0) {
                    output.r = grayscale;
                }
                if (_OutputToG > 0) {
                    output.g = grayscale;
                }
                if (_OutputToB > 0) {
                    output.b = grayscale;
                }
                if (_OutputToA > 0) {
                    output.a = grayscale;
                }
                return output;
            }
            ENDCG
        }
    }
}
