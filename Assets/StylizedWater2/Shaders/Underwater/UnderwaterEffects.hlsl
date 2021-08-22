//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

TEXTURE2D_FLOAT(_WaterDepth);
SAMPLER(sampler_WaterDepth);

//_MainLightColor is already used as a per-renderer property
float4 _MainLightColorUnderwater;
float4 _AmbientParams; //Skybox multiplier
//X: Intensity (skybox)
//Y: (bool) Skybox shading
float4 _AmbientColor;
float3 _MainLightDir;

#define AMBIENT_SKY_INTENSITY _AmbientParams.x
#define AMBIENT_SKYBOX _AmbientParams.y == 1
#define AMBIENT_SKYBOX_MIP 4 //Really don't need super detailed directional color accuracy, only sampling one texel

void ApplyLighting(inout float3 color, float shadowMask, float3 normal, float3 viewDir)
{
	float3 ambientColor = _AmbientColor.rgb; //Value most likely lingers around from the opaque pass, so perfect
	
	if(AMBIENT_SKYBOX)
	{
		float3 reflectVec = reflect(-viewDir, normal);
		float4 encodedIrradiance = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVec, AMBIENT_SKYBOX_MIP).rgba * AMBIENT_SKY_INTENSITY;

		#if !defined(UNITY_USE_NATIVE_HDR)
		ambientColor.rgb = DecodeHDREnvironment(encodedIrradiance , unity_SpecCube0_HDR) ;
		#else
		ambientColor.rgb = encodedIrradiance.rgb;
		#endif
	}

	//Mirror lambert shading
	float diffuseTerm = saturate(dot(_MainLightDir.xyz, normal));

	float3 directColor = _MainLightColorUnderwater.rgb * diffuseTerm;
	float3 bakedGI = ambientColor;

	float3 diffuseColor = (bakedGI + directColor);

	color = color * diffuseColor;
}
float3 GetWorldPosition(float4 uv, float deviceDepth )
{
	float4x4 inverseProjection = unity_CameraInvProjection;
	#if UNITY_REVERSED_Z
	deviceDepth  = 1.0 - deviceDepth;
	//https://issuetracker.unity3d.com/issues/shadergraph-inverse-view-projection-transformation-matrix-is-not-the-inverse-of-view-projection-transformation-matrix
	//Check if needed for VR!
	inverseProjection._12_22_32_42 = -inverseProjection._12_22_32_42;         
	#endif

	deviceDepth = deviceDepth * 2.0 - 1.0;

	#if VERSION_GREATER_EQUAL(10,0)
	float3 vpos = ComputeViewSpacePosition(uv.xy, deviceDepth , inverseProjection);
	#else
	float3 vpos = ComputeWorldSpacePosition(uv.xy, deviceDepth , inverseProjection);
	#endif

	float3 wpos = mul(unity_CameraToWorld, float4(vpos, 1.0)).xyz;

	return  wpos;
}

half4 BoxFilter4(TEXTURE2D_X_PARAM(textureName, samplerTex), float2 uv, float2 texelSize, float amount)
{
	//return float4(0, 1, 0, 0);
	float4 d = texelSize.xyxy * float4(-amount, -amount, amount, amount);

	half4 s;
	s = (SAMPLE_TEXTURE2D_X(textureName, samplerTex, (uv + d.xy)));
	s += (SAMPLE_TEXTURE2D_X(textureName, samplerTex, (uv + d.zy)));
	s += (SAMPLE_TEXTURE2D_X(textureName, samplerTex, (uv + d.xw)));
	s += (SAMPLE_TEXTURE2D_X(textureName, samplerTex, (uv + d.zw)));

	return s * 0.25h;
}

half LuminanceThreshold(half color, half threshold)
{
	half br = color;

	half contrib = max(0, br - threshold);

	contrib /= max(br, 0.001);

	return color * contrib;
}

float2 SampleWaterSurfaceDepth(float2 screenPos)
{
	return SAMPLE_TEXTURE2D_X(_WaterDepth, sampler_WaterDepth, screenPos).rg;
}

float CombineDepth(float sceneDepth, float waterDepth)
{
	return max(sceneDepth, waterDepth);
}

#define DISTORTION_STRENGTH 0.02
#define DISTORTION_FREQ 0.05

TEXTURE2D(_DistortionNoise); SAMPLER(sampler_DistortionNoise);
TEXTURE2D(_DistortionSphere); SAMPLER(sampler_DistortionSphere);

#define HQ_WORLDSPACE_DISTORTION

#if SHADER_API_MOBILE
#undef HQ_WORLDSPACE_DISTORTION
#endif

float MapWorldSpaceDistortionOffsets(float3 wPos)
{
	wPos *= 2.0;
	float distortionOffset = _TimeParameters.x * DISTORTION_FREQ;
	
	float x1 =  SAMPLE_TEXTURE2D(_DistortionNoise, sampler_DistortionNoise, float2(wPos.y + distortionOffset, wPos.z + distortionOffset)).r * 2.0 - 1.0;
	#ifdef HQ_WORLDSPACE_DISTORTION
	float x2 =  SAMPLE_TEXTURE2D(_DistortionNoise, sampler_DistortionNoise, float2(wPos.y - distortionOffset * 0.5, wPos.z + distortionOffset)).r * 2.0 - 1.0;
	#endif

	//Note: okay to skip Y-axis projection
	
	float z1 =  SAMPLE_TEXTURE2D(_DistortionNoise, sampler_DistortionNoise, float2(wPos.x + distortionOffset, wPos.y + distortionOffset)).r * 2.0 - 1.0;
	#ifdef HQ_WORLDSPACE_DISTORTION
	float z2 =  SAMPLE_TEXTURE2D(_DistortionNoise, sampler_DistortionNoise, float2(wPos.x + distortionOffset * 0.5, wPos.y + distortionOffset)).r * 2.0 - 1.0;
	#endif

	#ifdef HQ_WORLDSPACE_DISTORTION
	return max(max(x1, x2), max(z1, z2));
	#else
	return max(x1, z1);
	#endif
}

void DistortUV(float2 uv, inout float2 distortedUV)
{
	float offset = 0;
	
#if SCREENSPACE_DISTORTION
	float2 distortionFreq = uv * 1.0;
	float distortionOffset = _TimeParameters.x * DISTORTION_FREQ;
				
	float n1 = SAMPLE_TEXTURE2D(_DistortionNoise, sampler_DistortionNoise, float2(distortionFreq.x + distortionOffset, distortionFreq.y + distortionOffset)).r * 2.0 - 1.0;
	float n2 = SAMPLE_TEXTURE2D(_DistortionNoise, sampler_DistortionNoise, float2(distortionFreq.x - (distortionOffset * 0.5), distortionFreq.y)).r * 2.0 - 1.0;

	offset = max(n1, n2) * DISTORTION_STRENGTH;
#endif

#if CAMERASPACE_DISTORTION
	offset = SAMPLE_TEXTURE2D(_DistortionSphere, sampler_DistortionSphere, uv).r * DISTORTION_STRENGTH;
#endif

	#ifdef UNITY_REVERSED_Z
	//Offset always has to push up, otherwise creates a seam where the water meets the shore
	distortedUV = uv.xy - offset;
	#else
	distortedUV = uv.xy + offset;
	#endif
}