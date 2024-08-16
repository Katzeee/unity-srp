Shader "CustomShaders/Lit"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _MainTex ("Texture", 2D) = "white" {}
        _Metallic("Metallic", Float) = 0.5
        _Roughness("Roughness", Float) = 0.5
    }
    SubShader
    {
        Tags { "LightMode"="CustomLit" }

        Pass
        {
            CGPROGRAM
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Fragment.hlsl"
            #include "Light.hlsl"
            #include "Lighting.hlsl"
            
            struct v2f
            {
                float4 pos: SV_POSITION;
                float4 pos_W: TEXCOORD0;
                float3 normal: TEXCOORD1;
                float2 uv: TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
                UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
                UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
                UNITY_DEFINE_INSTANCED_PROP(float, _Metallic)
                UNITY_DEFINE_INSTANCED_PROP(float, _Roughness)
            UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

            v2f vert (appdata_tan v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.pos_W = mul(UNITY_MATRIX_M, v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                float4 cur_tex_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ST);
                o.uv = TRANSFORM_TEX(v.texcoord, cur_tex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i)
                // prepare surface
                Surface s;
                s.metallic = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Metallic);
                s.roughness = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Roughness);
                s.albedo = tex2D(_MainTex, i.uv) * UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
                
                fixed3 direct_lighting = 0;
                
                // prepare vectors and dot products
                FragValue f;
                f.N = normalize(i.normal);
                f.V = normalize(_WorldSpaceCameraPos - i.pos_W);
                f.NoV = dot(f.N, f.V);
                
                for (int j = 0; j < g_DirectionalLightCount; j++)
                {
                    f.L = normalize(g_DirectionalLightDirs[j]);
                    f.NoL = dot(f.N, f.L);
                    f.H = normalize(f.V + f.L);
                    f.NoH = dot(f.N, f.H);
                    f.HoV = dot(f.H, f.V);

                    // calculate direct light lighting
                    direct_lighting += cook_torrance_brdf(f, s) * g_DirectionalLightColors[j];
                    
                }
                return fixed4(direct_lighting, s.albedo.a);
            }
            ENDCG
        }
    }
}
