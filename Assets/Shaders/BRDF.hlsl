#ifndef __CUSTOM_BRDF
#define __CUSTOM_BRDF

// F part
fixed3 fresnel_schlick(fixed cos_theta, fixed3 F0)
{
    return F0 + (1.0f - F0) * pow(saturate(1.0 - cos_theta), 5.0f);
}

// GGX D part
fixed distribution_ggx(fixed NoH, fixed roughness)
{
    fixed a = roughness * roughness;
    fixed a_square = a * a;
    NoH = saturate(NoH);
    fixed denom = NoH * NoH * (a_square - 1.0) + 1.00001;
    denom = UNITY_PI * (denom * denom);
    return a_square / denom;
}

// help function
fixed geometry_schilck_ggx(fixed3 N, fixed3 dir, fixed roughness)
{
    fixed NoD = saturate(dot(N, dir));
    fixed k = (roughness + 1.0) * (roughness + 1.0) / 2;
    fixed denom = NoD * (1.0 - k) + k;
    return NoD / denom;
    
}

// Smith G part
fixed geometry_smith(fixed3 N, fixed3 V, fixed3 L, fixed roughtness)
{
    return geometry_schilck_ggx(N, V, roughtness) * geometry_schilck_ggx(N, L, roughtness);
}

#endif
