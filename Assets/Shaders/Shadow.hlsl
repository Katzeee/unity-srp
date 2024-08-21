#ifndef __CUSTOM_SHADOW
#define __CUSTOM_SHADOW

#define MAX_DIR_LIGHT_SHADOW_COUNT 4
#define MAX_DIR_LIGHT_SHADOW_CASCADE_COUNT 4

CBUFFER_START(_CustomShadow)
sampler2D g_dirLightShadowMap;
fixed4x4 g_worldToDirLightShadowMatrix[MAX_DIR_LIGHT_SHADOW_COUNT * MAX_DIR_LIGHT_SHADOW_CASCADE_COUNT];
// x: strength
fixed4 g_dirLightShadowDataPacked[MAX_DIR_LIGHT_SHADOW_COUNT];
fixed4 g_cascadeBoundingSphere[MAX_DIR_LIGHT_SHADOW_CASCADE_COUNT];
// x: 1 / shadowMaxDistance, y: 1 / fadeDistance, z: 1 / (1 - (1 - fadeCascade) ^ 2)
fixed4 g_shadowFadeDistancePacked;
int g_dirLightShadowCount;
CBUFFER_END

#include "Common.hlsl"

bool invalid_uv(fixed2 uv)
{
    return 0 < uv.x && uv.x < 1 && 0 < uv.y && uv.y < 1;
}

fixed sample_hard_shadow(fixed3 pos_sts)
{
    return tex2D(g_dirLightShadowMap, pos_sts.xy) < pos_sts.z ? 1.0 : 0.0;
}

fixed sample_pcf_shadow(fixed3 pos_SS)
{
    return 1.0;
}

int get_cascade_level(fixed3 pos_ws)
{
    for (int i = 0; i < MAX_DIR_LIGHT_SHADOW_CASCADE_COUNT; i++)
    {
        if (distance_squared(pos_ws, g_cascadeBoundingSphere[i]) < g_cascadeBoundingSphere[i].w)
        {
            return i;
        }
    }
    return MAX_DIR_LIGHT_SHADOW_CASCADE_COUNT;
}

// fade at the max distance and last cascade, (1 - distance / maxDistance) / fadeFactor(fadeAreaLength)
fixed fade_shadow_strength(fixed d, fixed m, fixed f)
{
    return saturate((1 - d * m) * f);
}


fixed get_shadow_attenuation(fixed4 pos_ws)
{
    fixed depth = -UnityWorldToViewPos(pos_ws).z;
    float shadow_attenuation = 0.0;
    [unroll(MAX_DIR_LIGHT_SHADOW_COUNT)]
    for (int i = 0; i < g_dirLightShadowCount; i++)
    {
        int level = get_cascade_level(pos_ws);
        if (level == MAX_DIR_LIGHT_SHADOW_CASCADE_COUNT)
        {
            return 1.0;
        }
        // Shadow texture space
        fixed4 pos_sts = mul(g_worldToDirLightShadowMatrix[i * MAX_DIR_LIGHT_SHADOW_CASCADE_COUNT + level], pos_ws);
        pos_sts.xyz /= pos_sts.w;
        pos_sts.z = pos_sts.z * 0.5 + 0.5;
        // if (!invalid_uv(pos_SS.xy))
        // {
        //     shadow_attenuation = 0.0f;
        // }

        fixed shadow_strength = g_dirLightShadowDataPacked[i].x;
        // max distance fade: (1 - depth / maxShadowDistance) / fadeDistance
        shadow_strength *= fade_shadow_strength(depth, g_shadowFadeDistancePacked.x, g_shadowFadeDistancePacked.y);
        // last cascade fade: (1 - d ^ 2 / r ^ 2) / (1 - (1 - fadeCascade) ^ 2)
        if (level == MAX_DIR_LIGHT_SHADOW_CASCADE_COUNT - 1)
        {
            shadow_strength *= fade_shadow_strength(
                distance_squared(pos_ws, g_cascadeBoundingSphere[MAX_DIR_LIGHT_SHADOW_CASCADE_COUNT - 1]),
                1.0f / g_cascadeBoundingSphere[MAX_DIR_LIGHT_SHADOW_CASCADE_COUNT - 1].w, g_shadowFadeDistancePacked.z);
        }

        shadow_attenuation += lerp(1.0f, sample_hard_shadow(pos_sts), shadow_strength);
    }
    // return depth;
    return shadow_attenuation / g_dirLightShadowCount;
}

#endif
