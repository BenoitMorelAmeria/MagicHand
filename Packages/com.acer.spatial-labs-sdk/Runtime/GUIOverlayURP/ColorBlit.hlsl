TEXTURE2D_X(_CameraOpaqueTexture);
SAMPLER(sampler_CameraOpaqueTexture);

TEXTURE2D(_texture);
SAMPLER(sampler_texture);

struct Attributes
{
    float4 positionHCS : POSITION;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings vert(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    // Note: The pass is setup with a mesh already in clip
    // space, that's why, it's enough to just output vertex
    // positions
    output.positionCS = float4(input.positionHCS.xyz, 1.0);

    #if UNITY_UV_STARTS_AT_TOP
    output.positionCS.y *= -1;
    #endif

    output.uv = input.uv;
    return output;
}

half4 frag(Varyings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    float4 color = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, input.uv);
    float4 guiColor = SAMPLE_TEXTURE2D(_texture, sampler_texture, input.uv);
    color = lerp(color, guiColor, guiColor.a);
    return color;
}
