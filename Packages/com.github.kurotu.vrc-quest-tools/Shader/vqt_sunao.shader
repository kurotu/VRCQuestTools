Shader "Hidden/VRCQuestTools/Sunao"
{
    Properties
    {
        // Main texture
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _Bright("Brightness", Range(0, 1)) = 1
        _UVAnimation("Animation Speed", Range(0, 10)) = 0
        _UVAnimX("Animation X Size", Int) = 1
        _UVAnimY("Animation Y Size", Int) = 1
        _UVAnimOtherTex("Animation Other Texture Maps", Float) = 0

        [Normal] _BumpMap ("Normalmap", 2D) = "bump" {}
        _BumpScale ("Normal Map Scale", Range(-2.0,  2.0)) = 1.0

        _ShadeMask("Shade Mask", 2D) = "white" {}
        _Shade ("Shade Strength", Range( 0.0,  1.0)) = 0.3
        _ShadeWidth ("Shade Width", Range( 0.0,  2.0)) = 0.75
        _ShadeGradient ("Shade Gradient", Range( 0.0,  2.0)) = 0.75
        _ShadeColor ("Shade Color", Range( 0.0,  1.0)) = 0.5
        _CustomShadeColor ("Custom Shade Color", Color) = (0,0,0,0)

        _ToonEnable ("Enable Toon Shading", int) = 0
        _Toon ("Toon", Range( 0.0,  9.0)) = 9.0
        _ToonSharpness ("Toon Sharpness", Range( 0.0,  1.0)) = 1.0

        // Decal
        _DecalEnable("Enable Decal", Int) = 0
        _DecalTex("Decal Texture", 2D) = "white" {}
        _DecalColor("Decal Color", Color) = (1,1,1,1)
        _DecalPosX("Decal Position X", Range(0, 1)) = 0.5
        _DecalPosY("Decal Position Y", Range(0, 1)) = 0.5
        _DecalSizeX("Decal Scale X", Range(0, 1)) = 0.5
        _DecalSizeY("Decal Scale Y", Range(0, 1)) = 0.5
        _DecalRotation("Decal Rotation", Range(-180, 180)) = 0 // degree, clockwise
        _DecalMode("Decal Mode", Int) = 0
        _DecalMirror("Decal Mirror Mode", Int) = 0
        _DecalBright("Decal Brightness Offset", Range(-1, 1)) = 0 // for Multiply(Mono)
        _DecalEmission("Decal Emission Intensity", Range(0, 10)) = 1 // for Emissive(Add), Emisive(Override)
        _DecalAnimation("Decal Animation Speed", Range(0, 10)) = 0
        _DecalAnimX("Decal Animation X Size", Int) = 1
        _DecalAnimY("Decal Animation Y Size", Int) = 1

        // Emission
        _EmissionEnable("Enable Emission", Int) = 0
        _EmissionMap("Emission Mask", 2D) = "white" {}
        [HDR]_EmissionColor("Emission Color", Color) = (0,0,0,0)
        _Emission("Emission Intensity", Range(0, 2)) = 1
        _EmissionMap2("2nd Emission Mask", 2D) = "white" {}
        _EmissionMode("Emission Mode", Int) = 0
        _EmissionAnimation("Emission Animation Speed", Range(0, 10)) = 0
        _EmissionAnimX("Emission Animation X Size", Int) = 1
        _EmissionAnimY("Emission Animation Y Size", Int) = 1

        // Gamma Fix
        _EnableGammaFix("Enable Gamma Fix", Int) = 0
        _GammaR("Gamma R", Range(0, 5)) = 1
        _GammaG("Gamma G", Range(0, 5)) = 1
        _GammaB("Gamma B", Range(0, 5)) = 1

        // Blightness Fix
        _EnableBlightFix("Enable Blightness Fix", Int) = 0
        _BlightOutput("Output Blightness", Range(0, 5)) = 1
        _BlightOffset("Blightness Offset", Range(-5, 5)) = 0

        // Output Limitter
        _LimitterEnable("Enable Output Limitter", Int) = 0
        _LimitterMax("Limitter Max", Range(0, 5)) = 1

        _VQT_MainTexBrightness("VQT Main Texture Brightness", Range(0, 1)) = 1
        _VQT_GenerateShadow("VQT Generate Shadow", Int) = 1
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
            #include "UnityStandardUtils.cginc"
            #include "cginc/vqt_common.cginc"
            #include "cginc/vqt_sunao_function.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv_EmissionMap : TEXCOORD1;
                float2 uv_EmissionMap2 : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Bright;
            float _UVAnimation;
            float _UVAnimX;
            float _UVAnimY;
            float _UVAnimOtherTex;

            sampler2D _BumpMap;
            float _BumpScale;

            sampler2D _ShadeMask;
            float4 _ShadeMask_ST;
            float _Shade;
            float _ShadeWidth;
            float _ShadeGradient;
            float _ShadeColor;
            float4 _CustomShadeColor;
            bool _ToonEnable;
            uint _Toon;
            float _ToonSharpness;

            float _DecalEnable;
            sampler2D _DecalTex;
            fixed4 _DecalColor;
            float _DecalPosX;
            float _DecalPosY;
            float _DecalSizeX;
            float _DecalSizeY;
            float _DecalRotation;
            float _DecalMode;
            float _DecalMirror;
            float _DecalBright;
            float _DecalEmission;
            float _DecalAnimation;
            float _DecalAnimX;
            float _DecalAnimY;

            float _EmissionEnable;
            sampler2D _EmissionMap;
            float4 _EmissionMap_ST;
            fixed4 _EmissionColor;
            float _Emission;
            sampler2D _EmissionMap2;
            float4 _EmissionMap2_ST;
            float _EmissionMode;
            float _EmissionAnimation;
            float _EmissionAnimX;
            float _EmissionAnimY;

            float _EnableGammaFix;
            float _GammaR;
            float _GammaG;
            float _GammaB;

            float _EnableBlightFix;
            float _BlightOutput;
            float _BlightOffset;

            float _LimitterEnable;
            float _LimitterMax;

            float _VQT_MainTexBrightness;
            uint _VQT_GenerateShadow;

            float4 sampleTex2D(sampler2D tex, float2 uv, float angle) {
              half angleCos = cos(angle);
              half angleSin = sin(angle);
              half2x2 rotateMatrix = half2x2(angleCos, -angleSin, angleSin, angleCos);
              float2 newUV = mul(rotateMatrix, uv - 0.5) + 0.5;
              return tex2D(tex, newUV);
            }

            float2 animateUV(float2 uv, float animX, float animY) {
                float2 newUV;
                newUV.x = uv.x / animX;
                newUV.y = (uv.y / animY) + (animY - 1) / animY;
                return newUV;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv_EmissionMap = TRANSFORM_TEX(v.uv, _EmissionMap);
                o.uv_EmissionMap2 = TRANSFORM_TEX(v.uv, _EmissionMap2);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 mainUV = _UVAnimation
                    ? animateUV(i.uv, _UVAnimX, _UVAnimY)
                    : i.uv;
                fixed4 col = tex2D(_MainTex, mainUV) * _Color * _Bright;

                if (_VQT_GenerateShadow) {
                    half3 normal = UnpackScaleNormal(tex2D(_BumpMap, i.uv), _BumpScale);
                    float diffuse = DiffuseCalc(normal, VQT_DUMMY_LIGHT_DIRECTION, _ShadeGradient , _ShadeWidth);
                    if (_ToonEnable) {
                        float4 toon = Toon(_Toon, _ToonSharpness);
                        diffuse = ToonCalc(diffuse, toon);
                    }
                    float3 shadeColor = saturate(col.rgb * 3.0f - 1.5f) * _ShadeColor;
                    shadeColor = lerp(shadeColor, _CustomShadeColor.rgb, _CustomShadeColor.a);
                    float3 diffColor = LightingCalc(float3(1, 1, 1) , diffuse , shadeColor , 1.0);

                    float shadeMap = tex2D(_ShadeMask, i.uv).r;
                    float shade = _Shade * shadeMap;
                    diffColor = lerp(float3(1, 1, 1), diffColor, shade);

                    col.rgb *= diffColor;
                }

                col.rgb *= _VQT_MainTexBrightness;
                fixed4 OUT = col;

                float4 decal = float4(0.0f, 0.0f, 0.0f, 0.0f);
                if (_DecalEnable) {
                    float angle = _DecalRotation * 3.1415926535f / 180.0f;
                    half angleCos = cos(angle);
                    half angleSin = sin(angle);
                    half2x2 rot = half2x2(angleCos, -angleSin, angleSin, angleCos);
                    float2 decalUV = i.uv;
                    if ((_DecalMirror == 4 || _DecalMirror == 5) && _DecalPosX >= 0.5f) { // Copy(Mirror), Copy(Fixed)
                        decalUV.x = 1.0f - decalUV.x;
                    }
                    decalUV -= float2(_DecalPosX, _DecalPosY);
                    if (_DecalMirror == 3) { // Mirror2
                        decalUV.x = - decalUV.x;
                    }
                    decalUV = mul(rot, decalUV) / float2(_DecalSizeX, _DecalSizeY) + 0.5f;

                    if (decalUV.x >= 0.0f && decalUV.x <= 1.0f && decalUV.y >= 0.0f && decalUV.y <= 1.0f) {
                        if (_DecalAnimation) {
                            decalUV = animateUV(decalUV, _DecalAnimX, _DecalAnimY);
                        }
                        decal = tex2D(_DecalTex, decalUV) * _DecalColor;
                    }

                    if (_DecalMirror == 4) { // Copy(Mirror)
                        if (i.uv.x >= 0.5f) {
                            decal = float4(0.0f, 0.0f, 0.0f, 0.0f);
                        }
                        decalUV = i.uv;
                        if (_DecalPosX < 0.5f) {
                            decalUV.x = 1.0f - decalUV.x;
                        }
                        decalUV -= float2(_DecalPosX, _DecalPosY);
                        decalUV = mul(rot, decalUV) / float2(_DecalSizeX, _DecalSizeY) + 0.5f;
                        if (i.uv.x >= 0.5f && decalUV.x >= 0.0f && decalUV.x <= 1.0f && decalUV.y >= 0.0f && decalUV.y <= 1.0f) {
                            if (_DecalAnimation) {
                                decalUV = animateUV(decalUV, _DecalAnimX, _DecalAnimY);
                            }
                            decal = tex2D(_DecalTex, decalUV) * _DecalColor;
                        }
                    }

                    if (_DecalMirror == 5) {
                        if (i.uv.x >= 0.5f) {
                            decal = float4(0.0f, 0.0f, 0.0f, 0.0f);
                        }
                        decalUV = i.uv;
                        if (_DecalPosX >= 0.5f) {
                            decalUV.x = 1.0f - decalUV.x;
                        }
                        decalUV -= float2(1.0f - _DecalPosX, _DecalPosY);
                        decalUV = mul(rot, decalUV) / float2(_DecalSizeX, _DecalSizeY) + 0.5f;
                        if (i.uv.x >= 0.5f && decalUV.x >= 0.0f && decalUV.x <= 1.0f && decalUV.y >= 0.0f && decalUV.y <= 1.0f) {
                            if (_DecalAnimation) {
                                decalUV = animateUV(decalUV, _DecalAnimX, _DecalAnimY);
                            }
                            decal = tex2D(_DecalTex, decalUV) * _DecalColor;
                        }
                    }

                    switch (_DecalMode) { // Copy(Fixed)
                        case 0: // Override
                        case 5: // Emissive(Override)
                            OUT.rgb = lerp(OUT.rgb, decal.rgb, decal.a);
                            break;
                        case 1: // Add
                            OUT.rgb = saturate(OUT.rgb + decal.rgb * decal.a);
                            break;
                        case 2: // Multiply
                            OUT.rgb = lerp(OUT.rgb, OUT.rgb * decal.rgb, decal.a);
                            break;
                        case 3: // Multiply(Mono)
                            float mix = saturate(max(OUT.r, max(OUT.g, OUT.b))) + _DecalBright;
                            OUT.rgb = lerp(OUT.rgb, mix * decal.rgb, decal.a);
                            break;
                        case 4: // Emissive(Add)
                            // nothing
                            break;
                    }
                }

                if (_DecalEnable) {
                    if (_DecalMode == 4 || _DecalMode == 5) { // Emissive(Add), Emissive(Override)
                        OUT.rgb += decal.rgb * decal.a * _DecalEmission;
                    }
                }

                if (_EmissionEnable) {
                    float2 emiUV = _EmissionAnimation
                        ? animateUV(i.uv_EmissionMap, _EmissionAnimX, _EmissionAnimY)
                        : i.uv_EmissionMap;
                    float4 emi = sampleTex2D(_EmissionMap, emiUV, 0.0f);

                    float2 emi2UV = ((_UVAnimation > 0.0f) && (_UVAnimOtherTex > 0.0f))
                        ? animateUV(i.uv_EmissionMap2, _UVAnimX, _UVAnimY)
                        : i.uv_EmissionMap2;
                    float4 emi2 = sampleTex2D(_EmissionMap2, emi2UV, 0.0f);

                    emi.rgb *= emi.a * emi2.rgb * emi2.a;

                    switch (_EmissionMode) {
                        case 0: // Add
                            OUT.rgb = saturate(OUT.rgb + emi.rgb * _EmissionColor);
                            break;
                        case 1: // Multiply
                            OUT.rgb *= saturate(1 - emi.rgb * col.a);
                            OUT.rgb = saturate(OUT.rgb + lerp(col.rgb, 1.0f, 0.05f) * emi.rgb * col.a);
                            break;
                        case 2: // Minus
                            OUT.rgb = saturate(OUT.rgb - emi.rgb * _EmissionColor);
                            break;
                    }
                }

                if (_EnableGammaFix) {
                    _GammaR = max(_GammaR, 0.00001f);
                    _GammaG = max(_GammaG, 0.00001f);
                    _GammaB = max(_GammaB, 0.00001f);
                    OUT.r = pow(OUT.r, 1.0f / (1.0f / _GammaR));
                    OUT.g = pow(OUT.g, 1.0f / (1.0f / _GammaG));
                    OUT.b = pow(OUT.b, 1.0f / (1.0f / _GammaB));
                }

                if (_EnableBlightFix) {
                    OUT.rgb *= _BlightOutput;
                    OUT.rgb = max(OUT.rgb + _BlightOffset, 0.0f);
                }

                if (_LimitterEnable) {
                    OUT.rgb = min(OUT.rgb, _LimitterMax);
                }

                return OUT;
            }
            ENDCG
        }
    }
}
