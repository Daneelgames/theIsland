//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

//Enabled when inspecting the mesh on a renderer for a specific camera
//#define DEBUG

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

//Strange occurence where the forward vector seems reversed
#if defined(DEBUG) || (SHADER_LIBRARY_VERSION_MAJOR >= 10)
#define FLIP_FORWARD
#else
#undef FLIP_FORWARD
#endif

float _WaterLevel;
float _ClipOffset;
float _WaterLineWidth;
		   
float4 _AnimationParams;
float _WaveHeight;
float _WaveNormalStr; //Unused
float _WaveDistance;
float4 _WaveDirection;
float _WaveSteepness;
float _WaveSpeed;
uint _WaveCount;

//Additional wave height to avoid any cracks
#define PADDING 0.0

#ifdef DEBUG
	float _NearPlane;
	float _FarPlane;
	float _CamFov;
	float3 _CamForward;
	float3 _CampUp;
	float3 _CamRight;
	float3 _CamPos;
	float _CamAspect;

	#define NEAR_PLANE _NearPlane
	#define ASPECT _CamAspect
	#define CAM_FOV _CamFov
	#define CAM_POS _CamPos
	#define CAM_RIGHT _CamRight
	#define CAM_UP _CampUp
	#define CAM_FWD _CamForward
#else
	//Current camera
	#define NEAR_PLANE _ProjectionParams.y
	#define ASPECT _ScreenParams.x / _ScreenParams.y
	#define CAM_FOV unity_CameraInvProjection._m11
	#define CAM_POS _WorldSpaceCameraPos
	#define CAM_RIGHT unity_CameraToWorld._11_21_31
	#define CAM_UP unity_CameraToWorld._12_22_32
	#define CAM_FWD unity_CameraToWorld._13_23_33
#endif   

#define DEGREES_2_RAD PI / 180.0
#define FOV_SCALAR 2.0 //Magical scalar, influenced by aspect ratio?

#include "Assets/StylizedWater2/Shaders/Libraries/Waves.hlsl"
 
struct Attributes
{
	float4 positionOS 	: POSITION;
	float2 uv 			: TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
	float4 positionCS 	: SV_POSITION;
	float3 positionWS	: TEXCOORD0;
	float2 uv 			: TEXCOORD1;
	#ifdef WATERLINE
	float4 screenPos 	: TEXCOORD2;
	#endif
	UNITY_VERTEX_OUTPUT_STEREO
};

float GetWaveAmplitude(float3 position, float3 wavePosition)
{
	//Distance from near plane top to wave position
	float waveAmp = length(wavePosition - position);

	//If above water, amplitude should be negative
	if (position.y > wavePosition.y) waveAmp = -waveAmp;

	return waveAmp + PADDING;
}

float GetWaveAmplitudeXZ(float2 position, float2 wavePosition)
{
	//Distance from near plane top to wave position
	float waveAmp = length(wavePosition - position);

	return waveAmp + PADDING;
}

Varyings VertexWaterLine(Attributes input)
{
	Varyings output = (Varyings)0;

	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

	float3 positionWS = 0;
	output.uv.xy = input.uv.xy;

	float clipPlane = (NEAR_PLANE * 2.0) + _ClipOffset;
	#ifdef WATERLINE
	//Draw just in front of the mask mask
	clipPlane -= 0.001;

	//Scale unit rectangle by width
	input.positionOS.y *= _WaterLineWidth;
	#endif

	//Near-clip plane position
#ifdef FLIP_FORWARD
	float3 nearPlaneCenter = CAM_POS + (CAM_FWD * clipPlane);
#else
	float3 nearPlaneCenter = CAM_POS - (CAM_FWD * clipPlane);
#endif
	
	float fovScalar = FOV_SCALAR * CAM_FOV;

	//Position vertices on the near-clip plane and scale to fit
	positionWS = (nearPlaneCenter + (CAM_RIGHT * (input.positionOS.x * ASPECT * fovScalar) * clipPlane)) + (CAM_UP * input.positionOS.y * clipPlane * fovScalar);
	float3 bottom = (nearPlaneCenter + (CAM_RIGHT * (input.positionOS.x * ASPECT * fovScalar) * clipPlane)) - (CAM_UP * clipPlane * fovScalar);
	float3 top = (nearPlaneCenter + (CAM_RIGHT * (input.positionOS.x * ASPECT * fovScalar) * clipPlane)) + (CAM_UP * clipPlane * fovScalar);
	float planeLength = distance(bottom, top);

	//Distance from near-clip bottom to water level (straight up)
	float depth = _WaterLevel - bottom.y;

	//Camera's X-angle
	float upFactor = dot(CAM_UP, float3(0.0, 1.0, 0.0));
	float angle = (acos(upFactor) * 180.0) / PI;

	//Distance from center to water level along tangent (from known opposite length and angle)
	float hypotenuse = (depth / cos(DEGREES_2_RAD * angle));

	//Intersection point with water level when traveling along the plane's tangent
	float3 samplePos = bottom + (CAM_UP * hypotenuse);
	
	WaveInfo waves = GetWaveInfo(samplePos.xz, ((_TimeParameters.x * _AnimationParams.z) * _AnimationParams.xy) * _WaveSpeed, 1000, 1001);
	waves.position.xz = samplePos.xz;
	//Wave height is relative to 0, convert to absolute world-space height and scale
	waves.position.y = _WaterLevel + (waves.position.y * _WaveHeight);

	//Test
	//waves.position = samplePos;
	
	//Distance from near plane bottom to wave position
	float waveAmp = GetWaveAmplitude(bottom, waves.position);

	//If below the lowest possible wave height, fix to top of plane
	if (top.y + _WaveHeight < _WaterLevel) waveAmp = planeLength;
	
	//Apply offset (wave height relative to clip plane top) to vertex position

#if defined(FULLSCREEN_QUAD)
	//Only affect top row of vertices
	if(input.positionOS.y >= 0.5)
	{	   				
		positionWS = bottom + (CAM_UP * (waveAmp));

		//float2 horizontalOffset = GetWaveAmplitudeXZ(positionWS, waves.position);
		//positionWS.xz += horizontalOffset;
	}
#endif

#ifdef WATERLINE
	waveAmp -= planeLength * 0.5;
	
	positionWS += (CAM_UP * (waveAmp));
#endif

	output.positionCS = TransformWorldToHClip(positionWS);
	output.positionWS = positionWS;

	#ifdef WATERLINE
	output.screenPos = ComputeScreenPos(output.positionCS);
	#endif

	//Debug as regular full-screen mesh
	//output.positionCS = TransformObjectToHClip(input.positionOS.xyz);

   return output;
}