#ifndef __CUSTOM_COMMON
#define __CUSTOM_COMMON

#include "UnityCG.cginc"
#define BUILTIN_TARGET_API
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

fixed distance_squared(fixed3 a, fixed3 b)
{
    fixed3 c = a - b;
    return dot(c, c);
}

#endif
