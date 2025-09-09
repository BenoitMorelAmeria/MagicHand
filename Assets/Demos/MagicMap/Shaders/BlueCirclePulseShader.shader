Shader "Custom/MultiBlueCirclePulse"
{
    Properties
    {
        _Color("Circle Color", Color) = (0,0.5,1,1)
        _Speed("Expansion Speed", Float) = 1
        _Periods("Number of circle periods", Float) = 3
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float _Speed;
            float _Periods;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);

                float timeNow = _Time.y;
                float alpha = 0;

                if (dist > 0.5f) discard; // Outside the circle area)

                alpha = abs(sin((dist * 2.0f * UNITY_PI * _Periods - timeNow * _Speed)));
                alpha *= (0.5f - dist) * 2.0f; // Fade out towards the edge)
                
                fixed4 col = _Color;
                col.a = alpha;
                return col;
            }
            ENDHLSL
        }
    }
}
