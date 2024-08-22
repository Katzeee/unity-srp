Shader "CustomShaders/Lit"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _MainTex ("Texture", 2D) = "white" {}
        _Metallic("Metallic", Float) = 0.5
        _Roughness("Roughness", Float) = 0.5
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 0
        [Enum(Off, 0, On, 1)] _ZWrite("Z Write", Float) = 1
        _CutOff("Alpha Cutoff", Range(0.0, 1.0)) = 0.8
        [Toggle(_CLIPPING)] _Clipping("Alpha Clipping", Float) = 0
        [Toggle(_PREMUL_ALPHA)] _PremulAlpha("Premultiply Alpha", Float) = 0
        [Toggle(_RECEIVE_SHADOW)] _ReceiveShadow("Receive Shadow", Float) = 1
    }
    SubShader
    {
        Tags { "LightMode"="CustomLit" }
        Pass
        {
            Name "LitPass"
            
            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            CGPROGRAM
            #pragma target 3.5
            #pragma multi_compile_instancing
            #pragma shader_feature _CLIPPING
            #pragma shader_feature _PREMUL_ALPHA
            #pragma shader_feature _RECEIVE_SHADOW
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Fragment.hlsl"
            #include "Light.hlsl"
            #include "Lighting.hlsl"
            #include "Shadow.hlsl"
            
            struct v2f
            {
                float4 pos: SV_POSITION;
                float4 pos_WS: TEXCOORD0;
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
                UNITY_DEFINE_INSTANCED_PROP(float, _CutOff)
            UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

            v2f vert (appdata_tan v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.pos_WS = mul(UNITY_MATRIX_M, v.vertex);
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
                f.V = normalize(_WorldSpaceCameraPos - i.pos_WS);
                f.NoV = dot(f.N, f.V);
                
                for (int light_index = 0; light_index < g_DirectionalLightCount; light_index++)
                {
                    f.L = normalize(g_DirectionalLightDirs[light_index]);
                    f.NoL = dot(f.N, f.L);
                    f.H = normalize(f.V + f.L);
                    f.NoH = dot(f.N, f.H);
                    f.HoV = dot(f.H, f.V);

                    // calculate direct light lighting
                    fixed3 temp_direct_lighting = 0;
#ifdef _PREMUL_ALPHA     
                    temp_direct_lighting = cook_torrance_brdf(f, s, true) * g_DirectionalLightColors[light_index];
#else                    
                    temp_direct_lighting  = cook_torrance_brdf(f, s) * g_DirectionalLightColors[light_index];
#endif
#ifdef _RECEIVE_SHADOW
                    temp_direct_lighting *= get_shadow_attenuation(light_index, i.pos_WS, f.N);
#endif
                    direct_lighting += temp_direct_lighting;
                    
                }
#ifdef _CLIPPING
                clip(s.albedo.a - UNITY_ACCESS_INSTANCED_PROP(UnityPreMaterial, _CutOff));
#endif
                return fixed4(direct_lighting, s.albedo.a);
            }
            ENDCG
        }

        Pass 
        {
            Name "ShadowMapPass"
            Tags { "LightMode" = "ShadowCaster" }
            Cull Front
            
            ColorMask 0
            
            CGPROGRAM
            #pragma target 3.5
            #pragma multi_compile_instancing
            #pragma shader_feature _CLIPPING
            #pragma shader_feature _PREMUL_ALPHA
            #pragma vertex vert
            #pragma fragment frag
            
            
            #include "UnityCG.cginc"

            struct v2f
            {
                fixed4 pos: SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(appdata_tan v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            void frag(v2f i)
            {
                
            }
            ENDCG
        }
    }
    CustomEditor "CustomShaderGUI"
}
