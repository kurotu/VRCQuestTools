Shader "Hidden/VRCQuestTools/Multiply"
{
    Properties
    {
        _Texture0 ("Texture 0", 2D) = "white" {}
        _Texture0Color ("Texture 0 Color", Color) = (1,1,1,1)
        _Texture1 ("Texture 1", 2D) = "white" {}
        _Texture1Color ("Texture 1 Color", Color) = (1,1,1,1)
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

            sampler2D _Texture0;
            float4 _Texture0_ST;
            fixed4 _Texture0Color;

            sampler2D _Texture1;
            float4 _Texture1_ST;
            fixed4 _Texture1Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _Texture0);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 output = tex2D(_Texture0, i.uv) * _Texture0Color * tex2D(_Texture1, i.uv) * _Texture1Color;

                return output;
            }
            ENDCG
        }
    }
}
