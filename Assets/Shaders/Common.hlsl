#ifndef __CUSTOM_COMMON
#define __CUSTOM_COMMON

fixed distance_squared(fixed3 a, fixed3 b)
{
    fixed3 c = a - b;
    return dot(c, c);
}

#endif
