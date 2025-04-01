float3 vqt_RGBtoXYZ(float3 rgb)
{
    return mul(float3x3(
        0.4124564, 0.3575761, 0.1804375,
        0.2126729, 0.7151522, 0.0721750,
        0.0193339, 0.1191920, 0.9503041
    ), rgb);
}

float3 vqt_XYZtoLAB(float3 xyz)
{
    // D65基準白色 (Xn, Yn, Zn)
    float3 ref_white = float3(0.95047, 1.00000, 1.08883);
    xyz /= ref_white; // 正規化
    
    // f(t) 関数
    float epsilon = 0.008856; // (6/29)^3
    float kappa = 903.3; // (29/3)^3
    float3 f = xyz > epsilon ? pow(xyz, 1.0 / 3.0) : (kappa * xyz + 16.0) / 116.0;
    
    float L = (116.0 * f.y) - 16.0;
    float a = 500.0 * (f.x - f.y);
    float b = 200.0 * (f.y - f.z);
    
    return float3(L, a, b);
}

float3 vqt_AdjustBrightnessLAB(float3 lab, float brightness)
{
    lab.x *= brightness; // L チャンネルのスケーリング
    return lab;
}

float3 vqt_LABtoXYZ(float3 lab)
{
    float3 ref_white = float3(0.95047, 1.00000, 1.08883);
    
    float fy = (lab.x + 16.0) / 116.0;
    float fx = fy + lab.y / 500.0;
    float fz = fy - lab.z / 200.0;
    
    float epsilon = 0.008856;
    float kappa = 903.3;
    
    float3 xyz;
    xyz.x = fx > (6.0 / 29.0) ? pow(fx, 3.0) : (fx - 16.0 / 116.0) * (3.0 * (6.0 / 29.0) * (6.0 / 29.0));
    xyz.y = lab.x > kappa * epsilon ? pow((lab.x + 16.0) / 116.0, 3.0) : lab.x / kappa;
    xyz.z = fz > (6.0 / 29.0) ? pow(fz, 3.0) : (fz - 16.0 / 116.0) * (3.0 * (6.0 / 29.0) * (6.0 / 29.0));
    
    return xyz * ref_white;
}

float3 vqt_XYZtoRGB(float3 xyz)
{
    return mul(float3x3(
        3.2404542, -1.5371385, -0.4985314,
        -0.9692660, 1.8760108, 0.0415560,
        0.0556434, -0.2040259, 1.0572252
    ), xyz);
}
