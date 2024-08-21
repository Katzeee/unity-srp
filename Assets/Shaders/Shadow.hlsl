#ifndef __CUSTOM_SHADOW
#define __CUSTOM_SHADOW

#define MAX_DIR_LIGHT_SHADOW_COUNT 4
#define MAX_DIR_LIGHT_SHADOW_CASCADE_COUNT 4

CBUFFER_START(_CustomShadow)
sampler2D g_dirLightShadowMap;
int g_dirLightShadowCount;
fixed4x4 g_worldToDirLightShadowMatrix[MAX_DIR_LIGHT_SHADOW_COUNT * MAX_DIR_LIGHT_SHADOW_CASCADE_COUNT];
// x: strength
fixed4 g_dirLightShadowDataPacked[MAX_DIR_LIGHT_SHADOW_COUNT];
fixed4 g_cascadeSphere[MAX_DIR_LIGHT_SHADOW_CASCADE_COUNT];
CBUFFER_END

#include "Common.hlsl"

bool invalid_uv(fixed2 uv)
{
    return 0 < uv.x && uv.x < 1 && 0 < uv.y && uv.y < 1;
}

fixed sample_hard_shadow(fixed3 pos_SS)
{
    return tex2D(g_dirLightShadowMap, pos_SS.xy) - 0.002 < pos_SS.z ? 1.0 : 0.0;
}

fixed sample_pcf_shadow(fixed3 pos_SS)
{
    return 1.0;
}

int get_cascade_level(fixed3 pos_WS)
{
    for (int i = 0; i < MAX_DIR_LIGHT_SHADOW_CASCADE_COUNT; i++)
    {
        if (distance_squared(pos_WS, g_cascadeSphere[i]) < g_cascadeSphere[i].w)
        {
            return i;
        }
    }
    return MAX_DIR_LIGHT_SHADOW_CASCADE_COUNT;
}


fixed get_shadow_attenuation(fixed4 pos_WS)
{
    float shadow_attenuation = 0.0;
    for (int i = 0; i < g_dirLightShadowCount; i++)
    {
        int level = get_cascade_level(pos_WS);
        // Shadow space
        fixed4 pos_SS = mul(g_worldToDirLightShadowMatrix[i * MAX_DIR_LIGHT_SHADOW_CASCADE_COUNT + level], pos_WS);
        pos_SS.xyz /= pos_SS.w;
        pos_SS.z = pos_SS.z * 0.5 + 0.5;
        // if (!invalid_uv(pos_SS.xy))
        // {
        //     shadow_attenuation = 0.0f;
        // }
        shadow_attenuation += lerp(1.0f, sample_hard_shadow(pos_SS), g_dirLightShadowDataPacked[i].x);
    }
    return shadow_attenuation / g_dirLightShadowCount;
}

#endif
