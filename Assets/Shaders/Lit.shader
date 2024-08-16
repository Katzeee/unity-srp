Shader "CustomShaders/Lit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Kd("Kd", Float) = 0.5
        _Ks("Ks", Float) = 0.3
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

            struct v2f
            {
                float4 pos: SV_POSITION;
                float4 pos_W: TEXCOORD0;
                float3 normal: TEXCOORD1;
                float2 uv: TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Kd;
            float _Ks;
            float _SpecularPower;

            v2f vert (appdata_tan v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.pos_W = mul(UNITY_MATRIX_M, v.vertex);
                // o.normal = normalize(mul(v.normal, (float3x3)unity_WorldToObject));
                o.normal = normalize(UnityObjectToWorldNormal(v.normal));
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 diffuse = fixed4(0, 0, 0, 0);
                fixed4 specular = fixed4(0, 0, 0, 0);
                for (int j = 0; j < g_DirectionalLightCount; j++)
                {
                    fixed3 L = normalize(g_DirectionalLightDirs[j]);
                    fixed NoL = dot(i.normal, L);
                    fixed3 V = normalize(_WorldSpaceCameraPos - i.pos_W);
                    fixed3 H = normalize(V + L);
                    fixed HoN = dot(H, i.normal);
                    diffuse += fixed4(saturate(NoL).xxx, 1.0) * g_DirectionalLightColors[j];
                    specular += pow(saturate(HoN), _SpecularPower) * g_DirectionalLightColors[j];
                }
                // fixed4 specular = 0.0;
                return col * (_Kd * diffuse + _Ks * specular);
            }
            ENDCG
        }
    }
}
