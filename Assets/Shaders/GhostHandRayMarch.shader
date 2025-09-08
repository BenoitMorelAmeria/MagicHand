Shader "Custom/GhostHandRaymarch"
{
    Properties
    {
        _Color ("Tint", Color) = (0, 0.8, 1, 0.4)
        _FresnelPower ("Fresnel Power", Range(1,8)) = 3
        _StepSize ("Step Size", Range(0.001,0.05)) = 0.01
        _MaxDistance ("Max March Distance", Range(1,10)) = 3
        _CapsuleRadius ("Capsule Radius", Range(0.01,0.2)) = 0.05
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float3 rayOrigin : TEXCOORD0; float3 rayDir : TEXCOORD1; };

            float4 _Color;
            float _FresnelPower, _StepSize, _MaxDistance, _CapsuleRadius;

            // max numbers (adjust if needed)
            #define MAX_CAPSULES 64
            float4 _CapsuleA[MAX_CAPSULES];
            float4 _CapsuleB[MAX_CAPSULES];
            int _CapsuleCount;

            // ---- SDF Primitives ----
            float sdCapsule(float3 p, float3 a, float3 b, float r) {
                float3 pa = p - a, ba = b - a;
                float h = saturate(dot(pa, ba) / dot(ba, ba));
                return length(pa - ba * h) - r;
            }
            float smoothUnion(float d1, float d2, float k) {
                float h = saturate(0.5 + 0.5*(d2-d1)/k);
                return lerp(d2, d1, h) - k*h*(1.0-h);
            }

            // Ray-AABB intersection for cube [-0.5, +0.5]
            bool RayBoxIntersect(float3 rayOrigin, float3 rayDir, out float tMin, out float tMax)
            {
                float3 invDir = 1.0 / rayDir;
                float3 t0s = (-0.5 - rayOrigin) * invDir;
                float3 t1s = ( 0.5 - rayOrigin) * invDir;

                float3 tsmaller = min(t0s, t1s);
                float3 tbigger  = max(t0s, t1s);

                tMin = max(max(tsmaller.x, tsmaller.y), tsmaller.z);
                tMax = min(min(tbigger.x, tbigger.y), tbigger.z);

                return tMax > max(tMin, 0.0);
            }


            // ---- Hand SDF ----
            float map(float3 p) {
                float d = 9999.0;
                for (int i = 0; i < _CapsuleCount; i++) {
                    float3 a = _CapsuleA[i].xyz;
                    float3 b = _CapsuleB[i].xyz;
                    float cd = sdCapsule(p, a, b, _CapsuleRadius);
                    d = smoothUnion(d, cd, 0.05);
                }
                return d;
            }

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // camera origin in object space
                o.rayOrigin = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0)).xyz;

                // ray dir: from camera to vertex in object space
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 objPos = mul(unity_WorldToObject, float4(worldPos, 1.0)).xyz;
                o.rayDir = normalize(objPos - o.rayOrigin);

                return o;
            }

           fixed4 frag(v2f i) : SV_Target {
                float tMin, tMax;
                if (!RayBoxIntersect(i.rayOrigin, i.rayDir, tMin, tMax))
                    discard; // ray misses cube

                float t = max(tMin, 0.0);
                float3 pos = 0;
                float dist = 0.0;

                for (int j = 0; j < 128; j++) {
                    pos = i.rayOrigin + i.rayDir * t;
                    dist = map(pos);
                    if (dist < 0.001) break;
                    t += dist;
                    if (t > tMax) discard; // ray marched past cube
                }

                // surface normal
                float3 n = normalize(float3(
                    map(pos + float3(0.01,0,0)) - map(pos - float3(0.01,0,0)),
                    map(pos + float3(0,0.01,0)) - map(pos - float3(0,0.01,0)),
                    map(pos + float3(0,0,0.01)) - map(pos - float3(0,0,0.01))
                ));

                // fresnel shading
                float3 vDir = normalize(i.rayOrigin - pos);
                float fresnel = pow(1.0 - saturate(dot(n, vDir)), _FresnelPower);

                return float4(_Color.rgb, _Color.a * fresnel);
            }
            ENDHLSL
        }
    }
}
