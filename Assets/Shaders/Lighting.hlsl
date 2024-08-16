#ifndef __CUSTOM_LIGHTING
#define __CUSTOM_LIGHTING

#include "BRDF.hlsl"
#include "Fragment.hlsl"

fixed3 blinn_phong(FragValue f, Surface s)
{
    fixed3 diffuse = saturate(f.NoL) * s.albedo;
    fixed3 specular = pow(saturate(f.NoH), 16.0);
    return diffuse + specular; 
}

fixed3 cook_torrance_brdf(FragValue f, Surface s)
{
    // specular
    
    fixed3 F0 = lerp(DEFAULT_F0, s.albedo, s.metallic);
    fixed3 D = distribution_ggx(f.NoH, s.roughness);
    fixed3 F = fresnel_schlick(f.HoV, F0);
    fixed3 G = geometry_smith(f.N, f.V, f.L, s.roughness);
    fixed3 nom = D * F * G;
    // fixed3 nom = F;
    fixed denom = 4.0 * saturate(f.NoV) * saturate(f.NoL) + 0.001;
    fixed3 specular = nom * f.NoL / denom;
    
    // diffuse
    fixed3 Kd = 1.0 - F;
    Kd *= 1 - s.metallic;
    fixed3 diffuse = Kd * saturate(f.NoL) / UNITY_PI;
    
    return (diffuse + specular) * s.albedo;
}

#endif