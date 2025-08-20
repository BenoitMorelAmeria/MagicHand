Shader "ColorBlit"
{
	Properties
	{
		_texture ("Texture", 2D) = "" {}
	}

	SubShader
	{
		PackageRequirements { "com.unity.render-pipelines.universal": "12.1.0" }
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
		LOD 100
		ZWrite Off Cull Off
		Pass
		{
			Name "ColorBlitPass"

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "ColorBlit.hlsl"
			ENDHLSL
		}
	}
}
