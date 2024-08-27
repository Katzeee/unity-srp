#ifndef __CUSTOM_POST_FX_PASS
#define __CUSTOM_POST_FX_PASS

struct v2f
{
    float4 pos_cs: SV_POSITION;
    float2 uv: VAR_SCREEN_UV;
};

v2f vert(uint vertex_id: SV_VertexID)
{
    v2f o;
    o.pos_cs = float4(vertex_id <= 1 ? -1.0 : 3.0, vertex_id == 1 ? 3.0 : -1.0, 0.0, 1.0);
    o.uv = float2(vertex_id <= 1 ? 0.0 : 2.0, vertex_id == 1 ? 2.0 : 0.0);
    return o;
}



#endif
