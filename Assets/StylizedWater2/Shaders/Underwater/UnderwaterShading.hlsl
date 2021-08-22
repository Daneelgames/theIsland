//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

#ifndef UNDERWATER_SHADING_INCLUDED
#define UNDERWATER_SHADING_INCLUDED

float _ClipOffset;
#define CLIP_DISTANCE _ClipOffset  //Near-clip plane

//#if !defined(SHADERGRAPH_PREVIEW)
#if UNDERWATER_ENABLED
TEXTURE2D(_UnderwaterMask);
SAMPLER(sampler_UnderwaterMask);
#endif
//#endif

#include "UnderwaterFog.hlsl"

void ApplyUnderwaterShading(inout float3 color, inout float density, float3 worldPos, float3 normal, float3 viewDir, float3 shallowColor, float3 deepColor, float bottomFace)
{
#if UNDERWATER_ENABLED
	float fogDistFactor = 0;
	float fogHeightFactor = 0;
	float waterDensity = ComputeDensity(worldPos, _WaterLevel, 2, fogDistFactor, fogHeightFactor);

	float mask = (bottomFace * fogDistFactor);
	
	float3 waterColor = 0;
	GetUnderwaterFogColor(shallowColor.rgb, deepColor.rgb, fogDistFactor, fogHeightFactor, waterColor);

	density = fogDistFactor;
	color = lerp(color, waterColor, mask);

	//density = 1;
	//color = mask.rrr;
#endif
}

void ApplyUnderwaterShading_float(in float3 inColor, float3 worldPos, out float3 outColor, inout float density)
{
	outColor = inColor;
	density = 0;
	
	#if UNDERWATER_ENABLED
	ApplyUnderwaterShading(outColor, density, worldPos, float3(0,1,0), float3(0,0,0), _WaterShallowColor.rgb, _WaterDeepColor.rgb, 1.0);
	#endif

	outColor = lerp(inColor, outColor, density);
}

float3 SampleUnderwaterReflections(float3 reflectionVector, float smoothness, float3 wPos, float3 normal, float3 viewDir, float2 pixelOffset)
{
	#if !defined(SHADERGRAPH_PREVIEW)
	//Mirror since the normal is that of the top surface
	reflectionVector.y = -reflectionVector.y;

	#if VERSION_GREATER_EQUAL(12,0)
	float3 probe = (GlossyEnvironmentReflection(reflectionVector, wPos, 0.05, 1.0)).rgb;
	#else
	float3 probe = (GlossyEnvironmentReflection(reflectionVector, 0.5, 1.0)).rgb;
	#endif

	//Realistically, the underwater fog should be darker and dynamic exposure would make the refracted scene brighter
	return probe;
	#else
	return 0;
	#endif
}

float SampleUnderwaterMask(float4 screenPos)
{
	#if UNDERWATER_ENABLED
	return SAMPLE_TEXTURE2D_X(_UnderwaterMask, sampler_UnderwaterMask, (screenPos.xy / screenPos.w)).r;
	#else
	return 0;
	#endif
}

//Shader Graph
void SampleUnderwaterMask_float(float4 screenPos, out float mask)
{
	mask = SampleUnderwaterMask(screenPos);
}

#define CAM_FWD unity_CameraToWorld._13_23_33
#define NEAR_PLANE _ProjectionParams.y

//Clip the water using a fake near-clipping plane.
void ClipSurface(float4 screenPos, float3 wPos, float3 positionCS, float vFace)
{
#if UNDERWATER_ENABLED && !defined(SHADERGRAPH_PREVIEW) && !defined(UNITY_GRAPHFUNCTIONS_LW_INCLUDED)

	//NOTE: Still needs improvement, the backface still shows up
	float clipDepth = saturate((LinearEyeDepth(positionCS.z, _ZBufferParams)) / CLIP_DISTANCE + _ProjectionParams.y);
	float underwaterMask = SampleUnderwaterMask(screenPos);

	float mask = floor(clipDepth);
	//Clip space depth is not enough since vertex density is likely lower than the underwater mask
	mask *= lerp(underwaterMask, 1-underwaterMask, vFace);

	//mask = saturate(mask);

	//float dist = length((wPos - _WorldSpaceCameraPos) * CAM_FWD.xyz);
	
	clip(mask - 0.5);
#endif
}
#endif