#define MAX_DIRECTIONAL_LIGHT_COUNT 4
CBUFFER_START(_CustomLight)
    int g_DirectionalLightCount;
    float4 g_DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 g_DirectionalLightDirs[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END
