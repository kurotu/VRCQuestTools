#define VQT_DUMMY_LIGHT_DIRECTION float3(-0.5, 0.5, 0.7071068)
//#define VQT_DUMMY_LIGHT_DIRECTION float3(-0.5, 0.5, 0.5)

float4 vqt_lambert(half3 normal)
{
    float3 light = normalize(VQT_DUMMY_LIGHT_DIRECTION);
    float t = dot(normal, light);
    t = max(0, t);
    
    float3 diffuse = t;
    float4 finalColor = float4(1, 1, 1, 1);
    finalColor.xyz = diffuse;
    return finalColor;
}

float4 vqt_phong(half3 normal)
{
    float3 light = normalize(VQT_DUMMY_LIGHT_DIRECTION);
    float3 refVec = reflect(-light, normal);
    float3 toEye = normalize(float3(0, 0, 1));
    
    float t = dot(refVec, toEye);
    t = max(0, t);
    t = pow(t, 1);
    
    float3 specular = t;
    float4 finalColor = float4(1, 1, 1, 1);
    finalColor.xyz = specular;
    return finalColor;
}

half4 vqt_normalToGrayScale(half3 normal)
{
    float4 lambert = vqt_lambert(normal);
    float4 phong = pow(vqt_phong(normal), 100);
    float4 finalColor = float4(1, 1, 1, 1);
    finalColor.rgb = saturate(lambert.rgb + phong.rgb);
    return finalColor;
}
