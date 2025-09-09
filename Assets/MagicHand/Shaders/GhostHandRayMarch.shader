Shader "Custom/GhostHandRaymarch_URP"
{
    Properties
    {
        _Color ("Tint", Color) = (0, 0.8, 1, 0.4)
        _FresnelPower ("Fresnel Power", Range(1,8)) = 3
        _StepSize ("Step Size", Range(0.001,0.05)) = 0.01
        _MaxDistance ("Max March Distance", Range(1,10)) = 5
        _SphereRadius ("Sphere Radius", Range(0.01,0.1)) = 0.03
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 rayOrigin : TEXCOORD0; // object space
                float3 rayDir : TEXCOORD1;    // object space
            };

            float4 _Color;
            float _FresnelPower;
            float _StepSize;
            float _MaxDistance;
            float _SphereRadius;

            #define MAX_CAPSULES 64
            #define MAX_SPHERES 64

            int _CapsuleCount;
            float4 _CapsuleA[MAX_CAPSULES];
            float4 _CapsuleB[MAX_CAPSULES];
            float _CapsuleRadii[MAX_CAPSULES]; // per-capsule radii

            int _SphereCount;
            float4 _SpherePos[MAX_SPHERES];

            // --- SDF functions ---
            float sdCapsule(float3 p, float3 a, float3 b, float r)
            {
                float3 pa = p - a;
                float3 ba = b - a;
                float denom = max(dot(ba, ba), 1e-6);
                float h = saturate(dot(pa, ba) / denom);
                return length(pa - ba * h) - r;
            }

            float smoothUnion(float d1, float d2, float k)
            {
                float h = saturate(0.5 + 0.5 * (d2 - d1) / k);
                return lerp(d2, d1, h) - k * h * (1.0 - h);
            }

            bool RayBoxIntersect(float3 rayOrigin, float3 rayDir, out float tMin, out float tMax)
            {
                float3 invDir = 1.0 / rayDir;
                float3 t0s = (-0.5 - rayOrigin) * invDir;
                float3 t1s = (0.5 - rayOrigin) * invDir;
                float3 tsmaller = min(t0s, t1s);
                float3 tbigger = max(t0s, t1s);
                tMin = max(max(tsmaller.x, tsmaller.y), tsmaller.z);
                tMax = min(min(tbigger.x, tbigger.y), tbigger.z);
                return tMax > max(tMin, 0.0);
            }

            float map(float3 p)
            {
                // start large
                float d = 1e6;

                // capsules with per-capsule radius
                [loop]
                for (int i = 0; i < _CapsuleCount; i++)
                {
                    float r = _CapsuleRadii[i];
                    float cd = sdCapsule(p, _CapsuleA[i].xyz, _CapsuleB[i].xyz, r);
                    d = smoothUnion(d, cd, 0.01);
                }

                // spheres
                [loop]
                for (int i = 0; i < _SphereCount; i++)
                {
                    float sd = length(p - _SpherePos[i].xyz) - _SphereRadius;
                    d = min(d, sd);
                }

                return d;
            }

            v2f vert(appdata IN)
            {
                v2f OUT;

                // camera position in world space
                float3 camWorld = GetCameraPositionWS();

                // convert camera to object space
                float3 camOS = TransformWorldToObject(camWorld);

                // vertex position in object space
                float3 objVertex = IN.vertex.xyz;

                // ray direction from camera to vertex (object space)
                OUT.rayOrigin = camOS;
                OUT.rayDir = normalize(objVertex - camOS);

                // transform object -> HClip (helper from SRP)
                OUT.pos = TransformObjectToHClip(float4(objVertex, 1.0));
                return OUT;
            }

            float4 frag(v2f i) : SV_Target
            {
                float tMin, tMax;
                if (!RayBoxIntersect(i.rayOrigin, i.rayDir, tMin, tMax))
                    discard;

                float t = max(tMin, 0.0);
                float3 pos = i.rayOrigin + i.rayDir * t;
                float dist = 0.0;

                const int MAX_STEPS = 128;
                for (int j = 0; j < MAX_STEPS; j++)
                {
                    pos = i.rayOrigin + i.rayDir * t;
                    dist = map(pos);
                    if (dist < 0.001) break;

                    // avoid zero step by clamping to _StepSize
                    float step = max(dist, _StepSize);
                    t += step;

                    if (t > tMax)
                        discard;
                }

                // If we didn't hit anything, discard
                if (dist >= 0.001) discard;

                // normal via central differences
                const float eps = 0.01;
                float3 n = normalize(float3(
                    map(pos + float3(eps,0,0)) - map(pos - float3(eps,0,0)),
                    map(pos + float3(0,eps,0)) - map(pos - float3(0,eps,0)),
                    map(pos + float3(0,0,eps)) - map(pos - float3(0,0,eps))
                ));

                float3 vDir = normalize(i.rayOrigin - pos);
                float fresnel = pow(1.0 - saturate(dot(n, vDir)), _FresnelPower);

                return float4(_Color.rgb, _Color.a * fresnel);
            }

            ENDHLSL
        }
    }
}
