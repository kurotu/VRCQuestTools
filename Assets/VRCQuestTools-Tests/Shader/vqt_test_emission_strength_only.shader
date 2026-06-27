// Minimal test fixture: has _EmissionStrength* but NOT _EnableEmission*, simulating a locked Poiyomi shader.
Shader "Hidden/VRCQuestTools/Test/EmissionStrengthOnly"
{
    Properties
    {
        _EmissionStrength ("Emission Strength", Range(0, 20)) = 1
        _EmissionStrength1 ("Emission Strength 1", Range(0, 20)) = 1
        _EmissionStrength2 ("Emission Strength 2", Range(0, 20)) = 1
        _EmissionStrength3 ("Emission Strength 3", Range(0, 20)) = 1
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
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return 0;
            }

            ENDCG
        }
    }
}
