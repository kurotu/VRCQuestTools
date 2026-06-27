Shader "Hidden/VRCQuestTools/Particle"
{
    // Bake shader for particle material conversion.
    // Unlike Hidden/VRCQuestTools/Standard (Toon Lit, generates shadow from normal map),
    // this shader keeps the alpha channel so that the converted material can use
    // VRChat/Mobile/Particles/Additive (SrcAlpha) and Cutout punch-through.
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VQT_Tint ("VQT Tint", Color) = (1,1,1,1)
        _VQT_Invert ("VQT Invert RGB", Float) = 0
        _VQT_CutoffEnabled ("VQT Cutoff Enabled", Float) = 0
        _VQT_Cutoff ("VQT Cutoff", Range(0, 1)) = 0
        _VQT_AlphaToOne ("VQT Alpha To One", Float) = 0
        _VQT_Emission ("VQT Emission", Float) = 0
        _EmissionMap ("Emission", 2D) = "black" {}
        [HDR]_EmissionColor ("EmissionColor", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
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
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _VQT_Tint;
            float _VQT_Invert;
            float _VQT_CutoffEnabled;
            float _VQT_Cutoff;
            float _VQT_AlphaToOne;
            float _VQT_Emission;

            sampler2D _EmissionMap;
            fixed4 _EmissionColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _VQT_Tint;

                // Additive emission (Additive destination only).
                if (_VQT_Emission > 0.5)
                {
                    col.rgb += tex2D(_EmissionMap, i.uv).rgb * _EmissionColor.rgb;
                }

                // Cutout: punch alpha to {0, 1} by threshold.
                if (_VQT_CutoffEnabled > 0.5)
                {
                    col.a = col.a >= _VQT_Cutoff ? 1.0 : 0.0;
                }

                // Transparent (premultiplied alpha): bake alpha = 1 to avoid double alpha multiply.
                if (_VQT_AlphaToOne > 0.5)
                {
                    col.a = 1.0;
                }

                // Subtractive: invert RGB so that Multiply destination yields dst * (1 - src).
                if (_VQT_Invert > 0.5)
                {
                    col.rgb = 1.0 - col.rgb;
                }

                col = saturate(col);
                return col;
            }
            ENDCG
        }
    }
}
