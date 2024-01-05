Shader "VRCQuestTools/NormalMap/Gray"
{
    Properties
    {
        _Level("Level", Range(0, 1)) = 0.5
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv_NormalMap : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            float _Level;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
              fixed4 color = 1;
              color.rgb = _Level;
              return color;
            }

            ENDCG
        }
    }
}
