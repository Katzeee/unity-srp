#ifndef __CUSTOM_FRAGMENT
# define __CUSTOM_FRAGMENT

struct Surface
{
    fixed4 albedo;
    fixed4 metallic;
    // fixed3 F0;
    fixed roughness;
    
};

struct FragValue
{
    fixed3 N;
    fixed NoL;
    fixed3 L;
    fixed NoV;
    fixed3 V;
    fixed NoH;
    fixed3 H;
    fixed HoV;
};

#endif
