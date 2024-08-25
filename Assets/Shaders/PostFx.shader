Shader "CustomShaders/PostFX"
{
    SubShader
    {
        Cull Off
        ZTest Always
        ZWrite Off

        CGINCLUDE
        #include "Common.hlsl"
        #include "PostFxPass.hlsl"
        ENDCG

        Pass
        {
            Name "Copy"

            CGPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag

            TEXTURE2D(g_PostFxSrc);
            SAMPLER(samplerg_PostFxSrc);

            
            fixed4 frag(v2f i) : SV_Target
            {
                // DX uv start from left top
                if (_ProjectionParams.x < 0.0)
                {
                    i.uv.y = 1 - i.uv.y;
                }
                return SAMPLE_TEXTURE2D(g_PostFxSrc, samplerg_PostFxSrc, i.uv);
            }
            ENDCG

        }

    }
}