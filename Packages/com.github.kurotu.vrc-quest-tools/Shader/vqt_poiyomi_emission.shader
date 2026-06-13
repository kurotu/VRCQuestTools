Shader "Hidden/VRCQuestTools/Poiyomi/Emission"
{
    Properties
    {
        _VQT_AlbedoTex("Albedo Texture", 2D) = "black" {}

        [ToggleUI]_EnableEmission("Enable Emission", Float) = 0
        _EmissionMap("Emission 0", 2D) = "white" {}
        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0,0)
        _EmissionMask("Emission Mask", 2D) = "white" {}
        _EmissionMaskChannel("Emission Mask Channel", Float) = 0
        _EmissionStrength("Emission Strength", Range(0, 20)) = 0
        [ToggleUI]_EmissionBaseColorAsMap("Base Color as Map", Float) = 0

        [ToggleUI]_EnableEmission1("Enable Emission 1", Float) = 0
        _EmissionMap1("Emission 1", 2D) = "white" {}
        [HDR] _EmissionColor1("Emission Color 1", Color) = (0,0,0,0)
        _EmissionMask1("Emission Mask 1", 2D) = "white" {}
        _EmissionMaskChannel1("Emission Mask Channel 1", Float) = 0
        _EmissionStrength1("Emission Strength 1", Range(0, 20)) = 0
        [ToggleUI]_EmissionBaseColorAsMap1("Base Color as Map 1", Float) = 0

        [ToggleUI]_EnableEmission2("Enable Emission 2", Float) = 0
        _EmissionMap2("Emission 2", 2D) = "white" {}
        [HDR] _EmissionColor2("Emission Color 2", Color) = (0,0,0,0)
        _EmissionMask2("Emission Mask 2", 2D) = "white" {}
        _EmissionMaskChannel2("Emission Mask Channel 2", Float) = 0
        _EmissionStrength2("Emission Strength 2", Range(0, 20)) = 0
        [ToggleUI]_EmissionBaseColorAsMap2("Base Color as Map 2", Float) = 0

        [ToggleUI]_EnableEmission3("Enable Emission 3", Float) = 0
        _EmissionMap3("Emission 3", 2D) = "white" {}
        [HDR] _EmissionColor3("Emission Color 3", Color) = (0,0,0,0)
        _EmissionMask3("Emission Mask 3", 2D) = "white" {}
        _EmissionMaskChannel3("Emission Mask Channel 3", Float) = 0
        _EmissionStrength3("Emission Strength 3", Range(0, 20)) = 0
        [ToggleUI]_EmissionBaseColorAsMap3("Base Color as Map 3", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
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
                float4 vertex : SV_POSITION;
            };

            sampler2D _VQT_AlbedoTex;
            float4 _VQT_AlbedoTex_ST;

            float _EnableEmission;
            sampler2D _EmissionMap;
            float4 _EmissionMap_ST;
            float4 _EmissionColor;
            sampler2D _EmissionMask;
            float4 _EmissionMask_ST;
            float _EmissionMaskChannel;
            float _EmissionStrength;
            float _EmissionBaseColorAsMap;

            float _EnableEmission1;
            sampler2D _EmissionMap1;
            float4 _EmissionMap1_ST;
            float4 _EmissionColor1;
            sampler2D _EmissionMask1;
            float4 _EmissionMask1_ST;
            float _EmissionMaskChannel1;
            float _EmissionStrength1;
            float _EmissionBaseColorAsMap1;

            float _EnableEmission2;
            sampler2D _EmissionMap2;
            float4 _EmissionMap2_ST;
            float4 _EmissionColor2;
            sampler2D _EmissionMask2;
            float4 _EmissionMask2_ST;
            float _EmissionMaskChannel2;
            float _EmissionStrength2;
            float _EmissionBaseColorAsMap2;

            float _EnableEmission3;
            sampler2D _EmissionMap3;
            float4 _EmissionMap3_ST;
            float4 _EmissionColor3;
            sampler2D _EmissionMask3;
            float4 _EmissionMask3_ST;
            float _EmissionMaskChannel3;
            float _EmissionStrength3;
            float _EmissionBaseColorAsMap3;

            float emissionMask(float4 mask, float channel)
            {
                if (channel == 1)
                {
                    return mask.g;
                }

                if (channel == 2)
                {
                    return mask.b;
                }

                if (channel == 3)
                {
                    return mask.a;
                }

                return mask.r;
            }

            float3 applyEmission(
                float enable,
                sampler2D mapTex,
                float2 mapUv,
                float4 color,
                sampler2D maskTex,
                float2 maskUv,
                float maskChannel,
                float strength,
                float baseColorAsMap,
                float3 albedo)
            {
                if (enable < 0.5)
                {
                    return 0;
                }

                float3 emi = tex2D(mapTex, mapUv).rgb;
                float4 maskTexColor = tex2D(maskTex, maskUv);
                float mask = emissionMask(maskTexColor, maskChannel);
                float3 baseColor = lerp(1, albedo, baseColorAsMap);
                return emi * color.rgb * mask * strength * baseColor;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _VQT_AlbedoTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 albedo = tex2D(_VQT_AlbedoTex, i.uv).rgb;
                float3 col = 0;

                col += applyEmission(_EnableEmission, _EmissionMap, TRANSFORM_TEX(i.uv, _EmissionMap), _EmissionColor, _EmissionMask, TRANSFORM_TEX(i.uv, _EmissionMask), _EmissionMaskChannel, _EmissionStrength, _EmissionBaseColorAsMap, albedo);
                col += applyEmission(_EnableEmission1, _EmissionMap1, TRANSFORM_TEX(i.uv, _EmissionMap1), _EmissionColor1, _EmissionMask1, TRANSFORM_TEX(i.uv, _EmissionMask1), _EmissionMaskChannel1, _EmissionStrength1, _EmissionBaseColorAsMap1, albedo);
                col += applyEmission(_EnableEmission2, _EmissionMap2, TRANSFORM_TEX(i.uv, _EmissionMap2), _EmissionColor2, _EmissionMask2, TRANSFORM_TEX(i.uv, _EmissionMask2), _EmissionMaskChannel2, _EmissionStrength2, _EmissionBaseColorAsMap2, albedo);
                col += applyEmission(_EnableEmission3, _EmissionMap3, TRANSFORM_TEX(i.uv, _EmissionMap3), _EmissionColor3, _EmissionMask3, TRANSFORM_TEX(i.uv, _EmissionMask3), _EmissionMaskChannel3, _EmissionStrength3, _EmissionBaseColorAsMap3, albedo);

                return fixed4(saturate(col), 1);
            }
            ENDCG
        }
    }
}
