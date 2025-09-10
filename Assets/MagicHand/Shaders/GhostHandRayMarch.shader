Shader "Custom/GhostHandRaymarch_URP"
{
    Properties
    {
        _Color ("Tint", Color) = (0, 0.8, 1, 0.4)
        _FresnelPower ("Fresnel Power", Range(1,8)) = 3
        _StepSize ("Step Size", Range(0.001,0.05)) = 0.01
        _MaxDistance ("Max March Distance", Range(1,10)) = 5
        _SphereRadius ("Sphere Radius", Range(0.01,0.1)) = 0.03
        _AmbientColor ("Ambient Color", Color) = (0.2,0.2,0.2,1)
        _AmbientIntensity ("Ambient Intensity", Range(0,1)) = 0.2
        _DiffuseIntensity ("Diffuse Intensity", Range(0,2)) = 1.0
        _SpecularIntensity ("Specular Intensity", Range(0,2)) = 0.5
        _SpecularPower ("Specular Power", Range(1,64)) = 16 
        _EmissiveColor ("Emissive color", Color) = (1, 1, 1, 1)
        _EmissiveIntensity ("Emissive intensity", float) = 0.0
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl" 

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

            float4 _AmbientColor;
            float _AmbientIntensity;
            float _DiffuseIntensity;
            float _SpecularIntensity;
            float _SpecularPower;

            float4 _EmissiveColor;
            float _EmissiveIntensity;

            #define MAX_CAPSULES 64
            #define MAX_SPHERES 64

            int _CapsuleCount;
            float4 _CapsuleA[MAX_CAPSULES];
            float4 _CapsuleB[MAX_CAPSULES];
            float _CapsuleRadii[MAX_CAPSULES]; // per-capsule radii

            int _SphereCount;
            float4 _SpherePos[MAX_SPHERES];

            #define MAX_TRIANGLES 16

            int _TriangleCount;
            float4 _TriP0[MAX_TRIANGLES];
            float4 _TriP1[MAX_TRIANGLES];
            float4 _TriP2[MAX_TRIANGLES];
            float _TriRadius[MAX_TRIANGLES]; // thickness

            // --- SDF functions ---
            float sdCapsule(float3 p, float3 a, float3 b, float r)
            {
                float3 pa = p - a;
                float3 ba = b - a;
                float denom = max(dot(ba, ba), 1e-6);
                float h = saturate(dot(pa, ba) / denom);
                return length(pa - ba * h) - r;
            }

            // Signed distance to a triangle (filled area) with thickness r
            float sdTrianglePrism(float3 p, float3 a, float3 b, float3 c, float thickness)
            {
                // triangle normal
                float3 n = normalize(cross(b - a, c - a));

                // distance along normal
                float distNormal = dot(p - a, n);

                // project point onto triangle plane
                float3 proj = p - distNormal * n;

                // barycentric coordinates
                float3 v0 = b - a;
                float3 v1 = c - a;
                float3 v2 = proj - a;
                float d00 = dot(v0, v0);
                float d01 = dot(v0, v1);
                float d11 = dot(v1, v1);
                float d20 = dot(v2, v0);
                float d21 = dot(v2, v1);
                float denom = d00 * d11 - d01 * d01;
                float u = (d11 * d20 - d01 * d21) / denom;
                float v = (d00 * d21 - d01 * d20) / denom;

                // signed distance in plane
                float distPlane;
                if (u >= 0 && v >= 0 && u + v <= 1.0)
                    distPlane = 0.0; // inside triangle  flat
                else
                {
                    // outside  distance to nearest edge
                    float3 e0 = b - a;
                    float3 e1 = c - b;
                    float3 e2 = a - c;

                    float3 pa = proj - a;
                    float3 pb = proj - b;
                    float3 pc = proj - c;

                    float d0 = length(pa - e0 * clamp(dot(pa, e0)/dot(e0,e0),0,1));
                    float d1 = length(pb - e1 * clamp(dot(pb, e1)/dot(e1,e1),0,1));
                    float d2 = length(pc - e2 * clamp(dot(pc, e2)/dot(e2,e2),0,1));
                    distPlane = min(d0, min(d1,d2));
                }

                // combine plane and normal distances using max/min trick  flat prism
                return max(distPlane, abs(distNormal)) - thickness;
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
                
                    // triangles
                [loop] 
                for (int i = 0; i < _TriangleCount; i++)
                    d = smoothUnion(d, sdTrianglePrism(p, _TriP0[i].xyz, _TriP1[i].xyz, _TriP2[i].xyz, _TriRadius[i]), 0.01);


                return d;
            }

            v2f vert(appdata IN)
            {
                v2f OUT;
                OUT.pos = TransformObjectToHClip(IN.vertex.xyz);
                float3 camWorld = _WorldSpaceCameraPos;
                float3 camOS = TransformWorldToObject(camWorld);
                float3 objVertex = IN.vertex.xyz;
                OUT.rayOrigin = camOS;
                OUT.rayDir = normalize(objVertex - camOS);
                return OUT;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Convert clip-space back to world
                float4 clip = float4(i.pos.xy / i.pos.w, 0, 1); 
                float4 worldNear = mul(UNITY_MATRIX_I_VP, clip);
                worldNear /= worldNear.w;

                // Ray origin = eye position (object space, already passed)
                // Ray direction = worldNear - eyePos
                float3 worldRayDir = normalize(worldNear.xyz - mul(UNITY_MATRIX_I_V, float4(0,0,0,1)).xyz);
                float3 objRayDir = normalize(TransformWorldToObjectDir(worldRayDir));

                // Use these
                float3 rayOrigin = i.rayOrigin;
                float3 rayDir = i.rayDir; 


                float tMin, tMax;
                if (!RayBoxIntersect(rayOrigin, rayDir, tMin, tMax))
                    discard;

                float t = max(tMin, 0.0);
                float3 pos = rayOrigin + rayDir * t;
                float dist = 0.0;

                const int MAX_STEPS = 128;
                for (int j = 0; j < MAX_STEPS; j++)
                {
                    pos = rayOrigin + rayDir * t;
                    dist = map(pos);
                    if (dist < 0.001) break;

                    float step = max(dist, _StepSize);
                    t += step;

                    if (t > tMax)
                        discard;
                }

                if (dist >= 0.001) discard;

                // --- compute normal ---
                const float eps = 0.01;
                float3 n = normalize(float3(
                    map(pos + float3(eps,0,0)) - map(pos - float3(eps,0,0)),
                    map(pos + float3(0,eps,0)) - map(pos - float3(0,eps,0)),
                    map(pos + float3(0,0,eps)) - map(pos - float3(0,0,eps))
                ));

                float3 vDir = normalize(rayOrigin - pos);

                // --- lighting setup ---
                float3 lightDir = normalize(float3(0.5, 0.7, 0.3)); // simple directional light
                float3 lightColor = float3(1.0, 1.0, 1.0);

                // Fresnel
                float fresnel = pow(1.0 - saturate(dot(n, vDir)), _FresnelPower);

                // Diffuse
                float diff = saturate(dot(n, lightDir));

                // Specular (Blinn-Phong)
                float3 h = normalize(lightDir + vDir);
                float spec = pow(saturate(dot(n, h)), _SpecularPower);

                // Combine
                float3 ambient = _AmbientColor.rgb * _AmbientIntensity;
                float3 diffuse = lightColor * diff * _DiffuseIntensity;
                float3 specular = lightColor * spec * _SpecularIntensity;
                float3 color = (_Color.rgb * (ambient + diffuse)) + specular;

                //float glow = exp(-10 * dist);
                //color += _EmissiveColor * glow * _EmissiveIntensity;
               
                float3 emissive = _EmissiveColor.rgb * _EmissiveIntensity * fresnel;
                color += emissive;
                
                // Apply fresnel as alpha mask (same as before)
                return float4(color, _Color.a * fresnel);
            }

            ENDHLSL
        }
    }
}
