#ifndef __CUSTOM_SHADOW
#define __CUSTOM_SHADOW

#define MAX_DIR_LIGHT_SHADOW_COUNT 4


CBUFFER_START(_CustomShadow)
    sampler2D g_dirLightShadowMap;
    fixed4x4 g_worldToDirLightClipSpaceMatrix[MAX_DIR_LIGHT_SHADOW_COUNT];
    int blockSize = 1;
CBUFFER_END

bool invalid_uv(fixed2 uv)
{
    return 0 < uv.x && uv.x < 1 && 0 < uv.y && uv.y < 1;
}

fixed get_shadow_attenuation(fixed4 pos_WS)
{
    // Shadow space
    fixed4 pos_SS = mul(g_worldToDirLightClipSpaceMatrix[0], pos_WS);
    pos_SS.xyz /= pos_SS.w;
    pos_SS.z = pos_SS.z * 0.5 + 0.5;
    if (!invalid_uv(pos_SS.xy))
    {
        return 0.0;
    }
    return tex2D(g_dirLightShadowMap, pos_SS.xy) - 0.002 < pos_SS.z ? 1.0 : 0.0;
}

#endif
