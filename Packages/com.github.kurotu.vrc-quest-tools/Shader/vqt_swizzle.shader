Shader "Hidden/VRCQuestTools/Swizzle"
{
    Properties
    {
        _Texture0 ("Texture 0", 2D) = "white" {}
        [Enum(R, 0, G, 1, B, 2, A, 3, Grayscale, 4)] _Texture0Input ("Texture 0 Input", Int) = 0
        [Enum(R, 0, G, 1, B, 2, A, 3)] _Texture0Output ("Texture 0 Output", Int) = -1

        _Texture1 ("Texture 1", 2D) = "white" {}
        [Enum(R, 0, G, 1, B, 2, A, 3, Grayscale, 4)] _Texture1Input ("Texture 1 Input", Int) = 1
        [Enum(R, 0, G, 1, B, 2, A, 3)] _Texture1Output ("Texture 1 Output", Int) = -1

        _Texture2 ("Texture 2", 2D) = "white" {}
        [Enum(R, 0, G, 1, B, 2, A, 3, Grayscale, 4)] _Texture2Input ("Texture 2 Input", Int) = 2
        [Enum(R, 0, G, 1, B, 2, A, 3)] _Texture2Output ("Texture 2 Output", Int) = -1

        _Texture3 ("Texture 3", 2D) = "white" {}
        [Enum(R, 0, G, 1, B, 2, A, 3, Grayscale, 4)] _Texture3Input ("Texture 3 Input", Int) = 3
        [Enum(R, 0, G, 1, B, 2, A, 3)] _Texture3Output ("Texture 3 Output", Int) = -1
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
            float _Texture0Input;
            float _Texture0Output;

            sampler2D _Texture1;
            float4 _Texture1_ST;
            float _Texture1Input;
            float _Texture1Output;

            sampler2D _Texture2;
            float4 _Texture2_ST;
            float _Texture2Input;
            float _Texture2Output;

            sampler2D _Texture3;
            float4 _Texture3_ST;
            float _Texture3Input;
            float _Texture3Output;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _Texture0);
                return o;
            }

            float swizzleInput(fixed4 input, float channelInput) {
                if (channelInput == 0) {
                    return input.r;
                } else if (channelInput == 1) {
                    return input.g;
                } else if (channelInput == 2) {
                    return input.b;
                } else if (channelInput == 3) {
                    return input.a;
                } else if (channelInput == 4) {
                    // Rec.709
                    return dot(input.rgb, float3(0.2126, 0.7152, 0.0722));
                }
                return 0;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 output = fixed4(1, 1, 1, 1);

                fixed4 input0 = tex2D(_Texture0, i.uv);
                float value0 = swizzleInput(input0, _Texture0Input);
                if (_Texture0Output == 0) {
                    output.r = value0;
                } else if (_Texture0Output == 1) {
                    output.g = value0;
                } else if (_Texture0Output == 2) {
                    output.b = value0;
                } else if (_Texture0Output == 3) {
                    output.a = value0;
                }

                fixed4 input1 = tex2D(_Texture1, i.uv);
                float value1 = swizzleInput(input1, _Texture1Input);
                if (_Texture1Output == 0) {
                    output.r = value1;
                } else if (_Texture1Output == 1) {
                    output.g = value1;
                } else if (_Texture1Output == 2) {
                    output.b = value1;
                } else if (_Texture1Output == 3) {
                    output.a = value1;
                }

                fixed4 input2 = tex2D(_Texture2, i.uv);
                float value2 = swizzleInput(input2, _Texture2Input);
                if (_Texture2Output == 0) {
                    output.r = value2;
                } else if (_Texture2Output == 1) {
                    output.g = value2;
                } else if (_Texture2Output == 2) {
                    output.b = value2;
                } else if (_Texture2Output == 3) {
                    output.a = value2;
                }

                fixed4 input3 = tex2D(_Texture3, i.uv);
                float value3 = swizzleInput(input3, _Texture3Input);
                if (_Texture3Output == 0) {
                    output.r = value3;
                } else if (_Texture3Output == 1) {
                    output.g = value3;
                } else if (_Texture3Output == 2) {
                    output.b = value3;
                } else if (_Texture3Output == 3) {
                    output.a = value3;
                }

                return output;
            }
            ENDCG
        }
    }
}
