// Referenced work from Sunao Shader.
// Sunao Shader is licensed under the MIT License.
// https://agenasu.booth.pm/items/1723985

// Referenced from SunaoShader 1.6.2 : SunaoShader_Function.cginc
inline float DiffuseCalc(float3 normal, float3 ldir, float gradient, float width)
{
    float Diffuse;
    Diffuse = ((dot(normal, ldir) - 0.5f) * (gradient + 0.000001f)) + 1.5f - width;

    return saturate(Diffuse);
}

// Referenced from SunaoShader 1.6.2 : SunaoShader_Function.cginc
inline float4 Toon(uint toon, float gradient)
{
    float4 otoon;
    otoon.x = max(float(11 - toon), 2.0f);
    otoon.y = 1.0f / (otoon.x - 1.0f);
    otoon.z = 0.5f / otoon.x;
    otoon.w = pow(1.0f + (gradient * gradient * gradient), 10.0f);

    return otoon;
}

// Referenced from SunaoShader 1.6.2 : SunaoShader_Function.cginc
inline float ToonCalc(float diffuse, float4 toon)
{
    float Diffuse;
    float Gradient;

    diffuse = max(diffuse, 0.000001f);
    Gradient = frac((diffuse + toon.z - 0.0000001f) * toon.x) - 0.5f;
    Gradient = saturate(Gradient * toon.w + 0.5f) + 0.5f;
    Gradient = (frac(Gradient) - 0.5f) * toon.y;
    Diffuse = floor(diffuse * toon.x) * toon.y + Gradient;

    return saturate(Diffuse);
}

// Referenced from SunaoShader 1.6.2 : SunaoShader_Function.cginc
inline float3 LightingCalc(float3 light, float diffuse, float3 shadecol, float shademask)
{
    float3 ocol;
    ocol = lerp(light * shadecol, light, diffuse);
    ocol = lerp(light, ocol, shademask);

    return ocol;
}
