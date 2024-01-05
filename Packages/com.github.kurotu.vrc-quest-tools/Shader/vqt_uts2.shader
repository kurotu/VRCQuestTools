Shader "Hidden/VRCQuestTools/UTS2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _BaseColor("BaseColor", Color) = (1,1,1,1)

        [Normal] _NormalMap ("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Strength", Range(0, 1)) = 1
        _Is_NormalMapToBase("Is NormalMap to Base", Int) = 0

        _Set_SystemShadowsToBase ("Set_SystemShadowsToBase", Int ) = 1
        _Tweak_SystemShadowsLevel ("Tweak_SystemShadowsLevel", Range(-0.5, 0.5)) = 0

        _1st_ShadeMap ("1st ShadeMap", 2D) = "white" {}
        _1st_ShadeColor ("1st ShadeColor", Color) = (1,1,1,1)
        _Use_BaseAs1st ("Use Base as 1st", Int) = 0
        _2nd_ShadeMap ("2nd ShadeMap", 2D) = "white" {}
        _2nd_ShadeColor ("2nd ShadeColor", Color) = (1,1,1,1)
        _Use_1stAs2nd ("Use 1st as 2nd", Int) = 0

        _BaseColor_Step ("BaseColor_Step", Range(0, 1)) = 0.5
        _BaseShade_Feather ("Base/Shade_Feather", Range(0.0001, 1)) = 0.0001
        _ShadeColor_Step ("ShadeColor_Step", Range(0, 1)) = 0
        _1st_ShadeColor_Feather ("1st ShadeColor Feather", Range(0.0001, 1)) = 0.0001
        _1st_ShadeColor_Step ("1st ShadeColor Step", Range(0, 1)) = 0.5
        _2nd_ShadeColor_Feather ("2nd ShadeColor Feather", Range(0.0001, 1)) = 0.0001
        _1st2nd_Shades_Feather ("1st2nd ShadeColor Feather", Range(0.0001, 1)) = 0.0001 // == 2nd Feather
        _2nd_ShadeColor_Step ("2nd ShadeColor Step", Range(0, 1)) = 0

        _Set_1st_ShadePosition ("Set_1st_ShadePosition", 2D) = "white" {}
        _Set_2nd_ShadePosition ("Set_2nd_ShadePosition", 2D) = "white" {}

        _Emissive_Tex("Emissive_Tex", 2D) = "white" {}
        [HDR]_Emissive_Color("Emissive_Color", Color) = (1,1,1,1)

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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv_Emissive_Tex : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _BaseColor;

            sampler2D _NormalMap;
            float _BumpScale;
            uint _Is_NormalMapToBase;

            uint _Set_SystemShadowsToBase;
            float _Tweak_SystemShadowsLevel;

            sampler2D _1st_ShadeMap;
            float4 _1st_ShadeMap_ST;
            float4 _1st_ShadeColor;
            uint _Use_BaseAs1st;
            sampler2D _2nd_ShadeMap;
            float4 _2nd_ShadeMap_ST;
            float4 _2nd_ShadeColor;
            uint _Use_1stAs2nd;

            float _BaseColor_Step;
            float _BaseShade_Feather;
            float _ShadeColor_Step;
            float _1st_ShadeColor_Feather;
            float _1st_ShadeColor_Step;
            float _2nd_ShadeColor_Feather;
            float _1st2nd_Shades_Feather;
            float _2nd_ShadeColor_Step;

            sampler2D _Set_1st_ShadePosition;
            float4 _Set_1st_ShadePosition_ST;
            sampler2D _Set_2nd_ShadePosition;
            float4 _Set_2nd_ShadePosition_ST;

            sampler2D _Emissive_Tex;
            float4 _Emissive_Tex_ST;
            fixed4 _Emissive_Color;

            float _VQT_MainTexBrightness;
            uint _VQT_GenerateShadow;

            float4 sampleTex2D(sampler2D tex, float2 uv, float angle) {
              half angleCos = cos(angle);
              half angleSin = sin(angle);
              half2x2 rotateMatrix = half2x2(angleCos, -angleSin, angleSin, angleCos);
              float2 newUV = mul(rotateMatrix, uv - 0.5) + 0.5;
              return tex2D(tex, newUV);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv_Emissive_Tex = TRANSFORM_TEX(v.uv, _Emissive_Tex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;

                if (_VQT_GenerateShadow && _Is_NormalMapToBase) {
                    // Reference: UTS 2.0.9, UCTS_DoubleShadeWithFeather.cginc
                    //Unitychan Toon Shader ver.2.0
                    //v.2.0.9
                    //nobuyuki@unity3d.com
                    //https://github.com/unity3d-jp/UnityChanToonShaderVer2_Project
                    //(C)Unity Technologies Japan/UCL
                    fixed4 _MainTex_var = tex2D(_MainTex, i.uv);
                    float3 normal = UnpackScaleNormal(tex2D(_NormalMap, i.uv), _BumpScale);
                    float Set_LightColor = 1;
                    float _Is_LightColor_Base = 0;
                    float _Is_LightColor_1st_Shade = 0;
                    float _Is_LightColor_2nd_Shade = 0;
                    float2 Set_UV0 = i.uv;

                    float3 Set_BaseColor = lerp( (_BaseColor.rgb*_MainTex_var.rgb), ((_BaseColor.rgb*_MainTex_var.rgb)*Set_LightColor), _Is_LightColor_Base );

                    float4 _1st_ShadeMap_var = lerp(tex2D(_1st_ShadeMap,TRANSFORM_TEX(Set_UV0, _1st_ShadeMap)),_MainTex_var,_Use_BaseAs1st);
                    float3 Set_1st_ShadeColor = lerp( (_1st_ShadeColor.rgb*_1st_ShadeMap_var.rgb), ((_1st_ShadeColor.rgb*_1st_ShadeMap_var.rgb)*Set_LightColor), _Is_LightColor_1st_Shade );

                    float4 _2nd_ShadeMap_var = lerp(tex2D(_2nd_ShadeMap,TRANSFORM_TEX(Set_UV0, _2nd_ShadeMap)),_1st_ShadeMap_var,_Use_1stAs2nd);
                    float3 Set_2nd_ShadeColor = lerp( (_2nd_ShadeColor.rgb*_2nd_ShadeMap_var.rgb), ((_2nd_ShadeColor.rgb*_2nd_ShadeMap_var.rgb)*Set_LightColor), _Is_LightColor_2nd_Shade );

                    float _HalfLambert_var = 0.5*dot(normal,VQT_DUMMY_LIGHT_DIRECTION)+0.5;
                    float4 _Set_2nd_ShadePosition_var = tex2D(_Set_2nd_ShadePosition,TRANSFORM_TEX(Set_UV0, _Set_2nd_ShadePosition));
                    float4 _Set_1st_ShadePosition_var = tex2D(_Set_1st_ShadePosition,TRANSFORM_TEX(Set_UV0, _Set_1st_ShadePosition));
                    float attenuation = 1;
                    float _SystemShadowsLevel_var = (attenuation*0.5)+0.5+_Tweak_SystemShadowsLevel > 0.001 ? (attenuation*0.5)+0.5+_Tweak_SystemShadowsLevel : 0.0001;
                    float Set_FinalShadowMask = saturate((1.0 + ( (lerp( _HalfLambert_var, _HalfLambert_var*saturate(_SystemShadowsLevel_var), _Set_SystemShadowsToBase ) - (_BaseColor_Step-_BaseShade_Feather)) * ((1.0 - _Set_1st_ShadePosition_var.rgb).r - 1.0) ) / (_BaseColor_Step - (_BaseColor_Step-_BaseShade_Feather))));

                    float3 Set_FinalBaseColor = lerp(Set_BaseColor,lerp(Set_1st_ShadeColor,Set_2nd_ShadeColor,saturate((1.0 + ( (_HalfLambert_var - (_ShadeColor_Step-_1st2nd_Shades_Feather)) * ((1.0 - _Set_2nd_ShadePosition_var.rgb).r - 1.0) ) / (_ShadeColor_Step - (_ShadeColor_Step-_1st2nd_Shades_Feather))))),Set_FinalShadowMask); // Final Color
                    col.rgb = Set_FinalBaseColor.rgb;
                }

                col.rgb *= _VQT_MainTexBrightness;
                float4 emi = sampleTex2D(_Emissive_Tex, i.uv_Emissive_Tex, 0.0f);
                col = clamp(col + emi * _Emissive_Color, 0, 1);
                return col;
            }
            ENDCG
        }
    }
}
