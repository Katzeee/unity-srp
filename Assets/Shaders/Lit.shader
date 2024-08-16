Shader "CustomShaders/Lit"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _MainTex ("Texture", 2D) = "white" {}
        _Metallic("Metallic", Float) = 0.5
        _Roughness("Roughness", Float) = 0.5
        _SpecularPower("Specular Power", Float) = 32
    }
    SubShader
    {
        Tags { "LightMode"="CustomLit" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Light.hlsl"
            #include "BRDF.hlsl"

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
                UNITY_DEFINE_INSTANCED_PROP(float, _SpecularPower)
            UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

            v2f vert (appdata_tan v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v)
                UNITY_TRANSFER_INSTANCE_ID(v, o)
                o.pos = UnityObjectToClipPos(v.vertex);
                o.pos_W = mul(UNITY_MATRIX_M, v.vertex);
                o.normal = normalize(UnityObjectToWorldNormal(v.normal));
                float4 cur_tex_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ST);
                o.uv = TRANSFORM_TEX(v.texcoord, cur_tex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i)
                fixed4 col = tex2D(_MainTex, i.uv) * UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
                fixed3 diffuse = 0;
                fixed3 specular = 0;
                fixed3 F0 = 0.04f;
                F0 = lerp(F0, col, UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Metallic));
                for (int j = 0; j < g_DirectionalLightCount; j++)
                {
                    fixed3 L = normalize(g_DirectionalLightDirs[j]);
                    fixed NoL = saturate(dot(i.normal, L));
                    fixed3 V = normalize(_WorldSpaceCameraPos - i.pos_W);
                    fixed3 H = normalize(V + L);
                    fixed NoH = dot(i.normal, H);
                    fixed HoV = dot(H, V);
                    fixed NoV = dot(i.normal, V);

                    fixed3 F = fresnel_schlick(saturate(HoV), F0);
                    fixed Kd = 1.0 - F;
                    Kd *= 1.0 - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Metallic);
                    fixed3 D = distribution_ggx(NoH, UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Roughness));
                    fixed3 G = geometry_smith(i.normal, V, L, UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Roughness));
                    fixed3 nom = D * F * G;
                    fixed denom = 4.0 * saturate(NoV) * saturate(NoL) + 0.001;
                    diffuse += Kd * g_DirectionalLightColors[j] / _PI * NoL;
                    specular += nom * g_DirectionalLightColors[j] * NoL / denom;
                }
                // fixed4 specular = 0.0;
                return fixed4(col.rgb * (diffuse + specular), col.a);
            }
            ENDCG
        }
    }
}
