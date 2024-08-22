#ifndef __CUSTOM_SHADOW
#define __CUSTOM_SHADOW

#include "Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"

#define MAX_DIR_LIGHT_SHADOW_COUNT 4
#define MAX_CASCADE_COUNT 4

TEXTURE2D_SHADOW(g_dirLightShadowMap);
SAMPLER(samplerg_dirLightShadowMap);
SAMPLER_CMP(sampler_linear_clamp_compare);


#ifdef _DIR_LIGHT_PCF2x2
#define DIR_LIGHT_FILTER_SAMPLES 1
#define GET_FILTER_SAMPLE_WEIGHT(t, p, ow, op) ow[0] = 1; op[0] = p;
#elif defined(_DIR_LIGHT_PCF3x3)
#define DIR_LIGHT_FILTER_SAMPLES 4
#define GET_FILTER_SAMPLE_WEIGHT(t, p, ow, op) SampleShadow_ComputeSamples_Tent_3x3(t, p, ow, op)
#else 
#define DIR_LIGHT_FILTER_SAMPLES 1
#define GET_FILTER_SAMPLE_WEIGHT(t, p, ow, op)
#endif

CBUFFER_START(_CustomShadow)
    // sampler2D g_dirLightShadowMap;
    fixed4x4 g_worldToDirLightShadowMatrix[MAX_DIR_LIGHT_SHADOW_COUNT * MAX_CASCADE_COUNT];
    // x: strength, y: normalBias
    fixed4 g_dirLightShadowDataPacked[MAX_DIR_LIGHT_SHADOW_COUNT];
    // xyz: positon, w: r ^ 2 
    fixed4 g_cascadeBoundingSphere[MAX_CASCADE_COUNT];
    // x: 1 / shadowMaxDistance, y: 1 / fadeDistance, z: 1 / (1 - (1 - fadeCascade) ^ 2)
    fixed4 g_shadowFadeDistancePacked;
    // x: 1 / sphereRadius, y: texel size * sqrt 2
    fixed4 g_cascadeDataPacked[MAX_CASCADE_COUNT];
    // x: textureSize, y: texelSize
    fixed4 g_shadowTextureDataPacked;
    int g_dirLightShadowCount;
CBUFFER_END


bool invalid_uv(fixed2 uv)
{
    return 0 < uv.x && uv.x < 1 && 0 < uv.y && uv.y < 1;
}

fixed sample_hard_shadow(fixed3 pos_sts)
{
    return UNITY_SAMPLE_TEX2D(g_dirLightShadowMap, pos_sts.xy) < pos_sts.z ? 1.0 : 0.0;
}

fixed sample_pcf_shadow(fixed3 pos_sts)
{
    float weight[DIR_LIGHT_FILTER_SAMPLES];
    float2 sample_pos[DIR_LIGHT_FILTER_SAMPLES];
    GET_FILTER_SAMPLE_WEIGHT(g_shadowTextureDataPacked.yyxx, pos_sts.xy, weight, sample_pos);
    float shadow = 0;
    for (int i = 0; i < DIR_LIGHT_FILTER_SAMPLES; i++)
    {
        shadow += weight[i] * SAMPLE_TEXTURE2D_SHADOW(g_dirLightShadowMap, sampler_linear_clamp_compare,
                                                      fixed3(sample_pos[i].xy, pos_sts.z));
    }
    return shadow;
}

int get_cascade_level(fixed3 pos_ws)
{
    for (int i = 0; i < MAX_CASCADE_COUNT; i++)
    {
        if (distance_squared(pos_ws, g_cascadeBoundingSphere[i]) < g_cascadeBoundingSphere[i].w)
        {
            return i;
        }
    }
    return MAX_CASCADE_COUNT;
}

// fade at the max distance and last cascade, (1 - distance / maxDistance) / fadeFactor(fadeAreaLength)
fixed fade_shadow_strength(fixed d, fixed m, fixed f)
{
    return saturate((1 - d * m) * f);
}

// CALCULATE SHADOW ATTENUATION FOR EACH LIGHT SEPARATELY, NOT **AVERAGE** THEM!!!
fixed get_shadow_attenuation(int light_index, fixed4 pos_ws, fixed3 N)
{
    fixed depth = -UnityWorldToViewPos(pos_ws).z;
    int level = get_cascade_level(pos_ws);
    if (level == MAX_CASCADE_COUNT)
    {
        return 1.0;
    }
    // sts: shadow texture space
    // along normal direction bias texel size
    fixed4 bias = fixed4(normalize(N) * g_cascadeDataPacked[level].y * g_dirLightShadowDataPacked[light_index].y, 0);
    // fixed4 bias = 0;
    fixed4 pos_sts = mul(g_worldToDirLightShadowMatrix[light_index * MAX_CASCADE_COUNT + level], pos_ws + bias);
    pos_sts.xyz /= pos_sts.w;
    pos_sts.z = pos_sts.z * 0.5 + 0.5;
    // if (!invalid_uv(pos_SS.xy))
    // {
    //     shadow_attenuation = 0.0f;
    // }

    fixed shadow_strength = g_dirLightShadowDataPacked[light_index].x;
    // max distance fade: (1 - depth / maxShadowDistance) / fadeDistance
    shadow_strength *= fade_shadow_strength(depth, g_shadowFadeDistancePacked.x, g_shadowFadeDistancePacked.y);
    // last cascade fade: (1 - d ^ 2 / r ^ 2) / (1 - (1 - fadeCascade) ^ 2)
    if (level == MAX_CASCADE_COUNT - 1)
    {
        shadow_strength *= fade_shadow_strength(
            distance_squared(pos_ws, g_cascadeBoundingSphere[MAX_CASCADE_COUNT - 1]),
            1.0f / g_cascadeBoundingSphere[MAX_CASCADE_COUNT - 1].w, g_shadowFadeDistancePacked.z);
    }

#if defined(_DIR_LIGHT_PCF2x2) || defined(_DIR_LIGHT_PCF3x3)
    fixed shadow_attenuation = lerp(1.0f, sample_pcf_shadow(pos_sts), shadow_strength);
#else
    fixed shadow_attenuation = lerp(1.0f, sample_hard_shadow(pos_sts), shadow_strength);
#endif
    return shadow_attenuation;
}

#endif
