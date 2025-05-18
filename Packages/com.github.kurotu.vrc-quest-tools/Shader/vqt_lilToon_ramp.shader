Shader "Hidden/VRCQuestTools/lilToon/Ramp"
{
    Properties
    {
        _ShadowStrength("sStrength", Range(0, 1)) = 1
        _ShadowStrengthMask("sStrength", 2D) = "white" {}
        _ShadowColor("Shadow 1st Color", Color) = (0.5, 0.5, 0.5, 1)
        _ShadowBorder("Shadow 1st Border", Range(0, 1)) = 0.5
        _ShadowBlur("Shadow 1st Blur", Range(0, 1)) = 0.5
        _Shadow2ndColor("Shadow 2nd Color", Color) = (0.3, 0.3, 0.3, 1)
        _Shadow2ndBorder("Shadow 2nd Border", Range(0, 1)) = 0.3
        _Shadow2ndBlur("Shadow 2nd Blur", Range(0, 1)) = 0.3
        _Shadow3rdColor("Shadow 3rd Color", Color) = (0, 0, 0, 1)
        _Shadow3rdBorder("Shadow 3rd Border", Range(0, 1)) = 0.1
        _Shadow3rdBlur("Shadow 3rd Blur", Range(0, 1)) = 0.1
        // _ShadowBorderColor("Shadow Border Color", Color) = (1, 0, 0, 1)
        // _ShadowBorderRange("Shadow Border Range", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityStandardUtils.cginc"
            #include "cginc/vqt_common.cginc"

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

            float _ShadowStrength;
            fixed4 _ShadowColor;
            float _ShadowBorder;
            float _ShadowBlur;
            fixed4 _Shadow2ndColor;
            float _Shadow2ndBorder;
            float _Shadow2ndBlur;
            fixed4 _Shadow3rdColor;
            float _Shadow3rdBorder;
            float _Shadow3rdBlur;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // ToonLit用の影生成を転用
            fixed4 lilShadow(float lightValue) {
                fixed4 white = fixed4(1, 1, 1, 1);

                float shadowBorderMax = _ShadowBorder + _ShadowBlur / 2;
                float shadowBorderMin = _ShadowBorder - _ShadowBlur / 2;
                if (lightValue >= shadowBorderMax) {
                    return white;
                }

                fixed4 shadow = fixed4(1, 1, 1, 1);

                // shadow1
                if (lightValue < shadowBorderMax) {
                    float rate = (lightValue - shadowBorderMin) / (shadowBorderMax - shadowBorderMin);
                    fixed4 shadowColor = lerp(_ShadowColor, white, saturate(rate));
                    shadow.rgb = lerp(shadow.rgb, shadow.rgb * shadowColor.rgb, shadowColor.a);
                }

                // shadow2
                float shadow2BorderMax = _Shadow2ndBorder + _Shadow2ndBlur / 2;
                float shadow2BorderMin = _Shadow2ndBorder - _Shadow2ndBlur / 2;
                if (lightValue < shadow2BorderMax) {
                    float rate = (lightValue - shadow2BorderMin) / (shadow2BorderMax - shadow2BorderMin);
                    fixed4 shadow2Color = lerp(_Shadow2ndColor, white, saturate(rate));
                    shadow.rgb = lerp(shadow.rgb, shadow.rgb * shadow2Color.rgb, shadow2Color.a);
                }

                // shadow3
                float shadow3BorderMax = _Shadow3rdBorder + _Shadow3rdBlur / 2;
                float shadow3BorderMin = _Shadow3rdBorder - _Shadow3rdBlur / 2;
                if (lightValue < shadow3BorderMax) {
                    float rate = (lightValue - shadow3BorderMin) / (shadow3BorderMax - shadow3BorderMin);
                    fixed4 shadow3Color = lerp(_Shadow3rdColor, white, saturate(rate));
                    shadow.rgb = lerp(shadow.rgb, shadow.rgb * shadow3Color.rgb, shadow3Color.a);
                }

                float strength = _ShadowStrength;
                shadow.rgb = lerp(white.rgb, shadow.rgb, strength);

                return shadow;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed3 rgb = lilShadow(i.uv.x).rgb;
                // 雑に色を柔らかくしてlilToon2Rampに近づける
                rgb = LinearToGammaSpace(rgb);
                return fixed4(rgb, 1.0);
            }

            ENDCG
        }
    }
}
