Shader "Custom/GhostHandRaymarch_URP"
{
    Properties
    {
        _Color ("Tint", Color) = (0, 0.8, 1, 0.4)
        _FresnelPower ("Fresnel Power", Range(0,8)) = 3
        _StepSize ("Step Size", Range(0.001,0.05)) = 0.01
        _MaxDistance ("Max March Distance", Range(1,10)) = 5
        _CapsuleRadius ("Capsule Radius", Range(0.01,0.1)) = 0.03
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 rayOrigin : TEXCOORD0;
                float3 rayDir : TEXCOORD1;
            };

            float4 _Color;
            float _FresnelPower;
            float _StepSize;
            float _MaxDistance;
            float _CapsuleRadius;
            float _SphereRadius;

            #define MAX_CAPSULES 64
            #define MAX_SPHERES 64

            int _CapsuleCount;
            float4 _CapsuleA[MAX_CAPSULES];
            float4 _CapsuleB[MAX_CAPSULES];

            int _SphereCount;
            float4 _SpherePos[MAX_SPHERES];

            // --- SDF functions ---
            float sdCapsule(float3 p, float3 a, float3 b, float r)
            {
                float3 pa = p - a;
                float3 ba = b - a;
                float h = saturate(dot(pa, ba) / dot(ba, ba));
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
                float d = 9999.0;

                for (int i = 0; i < _CapsuleCount; i++)
                {
                    float cd = sdCapsule(p, _CapsuleA[i].xyz, _CapsuleB[i].xyz, _CapsuleRadius);
                    d = smoothUnion(d, cd, 0.01);
                }

                for (int i = 0; i < _SphereCount; i++)
                {
                    float sd = length(p - _SpherePos[i].xyz) - _SphereRadius;
                    d = min(d, sd);
                }

                return d;
            }
            /*
            v2f vert(appdata IN)
            {
                v2f OUT;

                // camera position in object space
                float3 camWorld = _WorldSpaceCameraPos;
                OUT.rayOrigin = mul(unity_WorldToObject, float4(camWorld, 1.0)).xyz;

                // vertex position in object space
                float3 objVertex = IN.vertex.xyz;

                // ray direction from camera to vertex
                OUT.rayDir = normalize(objVertex - OUT.rayOrigin);

                OUT.pos = TransformObjectToHClip(IN.vertex);
                return OUT;
            }
            */

            v2f vert(appdata IN) {
					v2f OUT;
					UNITY_SETUP_INSTANCE_ID(IN); //Insert
					UNITY_INITIALIZE_OUTPUT(v2f, OUT); //Insert
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT); //Insert
                    float3 camWorld = _WorldSpaceCameraPos;
                    OUT.rayOrigin = mul(unity_WorldToObject, float4(camWorld, 1.0)).xyz;
                     // vertex position in object space
                    float3 objVertex = IN.vertex.xyz;

                    // ray direction from camera to vertex
                    OUT.rayDir = normalize(objVertex - OUT.rayOrigin);

                    OUT.pos = UnityObjectToClipPos(IN.vertex);
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

                for (int j = 0; j < 128; j++)
                {
                    pos = i.rayOrigin + i.rayDir * t;
                    dist = map(pos);
                    if (dist < 0.001) break;
                    t += dist;
                    if (t > tMax) discard;
                }

                float3 n = normalize(float3(
                    map(pos + float3(0.01,0,0)) - map(pos - float3(0.01,0,0)),
                    map(pos + float3(0,0.01,0)) - map(pos - float3(0,0.01,0)),
                    map(pos + float3(0,0,0.01)) - map(pos - float3(0,0,0.01))
                ));

                float3 vDir = normalize(i.rayOrigin - pos);
                float fresnel = pow(1.0 - saturate(dot(n, vDir)), _FresnelPower);

                return float4(_Color.rgb, _Color.a * fresnel);
            }

            ENDCG
        }
    }
}
