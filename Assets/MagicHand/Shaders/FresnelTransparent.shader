Shader "Custom/FresnelTransparent"
{
    Properties
    {
        _Color ("Tint", Color) = (0, 0.8, 1, 0.4)
        _FresnelPower ("Fresnel Power", Range(1,8)) = 3
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            float4 _Color;
            float _FresnelPower;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                o.normal = worldNormal;
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float fresnel = pow(1.0 - saturate(dot(i.normal, i.viewDir)), _FresnelPower);
                return float4(_Color.rgb, _Color.a * fresnel);
            }
            ENDCG
        }
    }
}
