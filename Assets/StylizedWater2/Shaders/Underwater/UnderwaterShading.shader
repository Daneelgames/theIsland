//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

Shader "Hidden/StylizedWater2/Underwater"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
		LOD 100

		Pass
		{
			Name "Underwater effects"
			ZTest Always
			ZWrite Off
			Cull Off

			HLSLPROGRAM

			//#define DEBUG_WORLD_POS
			//#define DEBUG_WORLD_NORMALS
		
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			
			#define VERTEX_PASS
			#pragma vertex Vertex
			#undef VERTEX_PASS
			#pragma fragment Fragment
			#pragma multi_compile_fragment _ _LINEAR_TO_SRGB_CONVERSION
			#pragma multi_compile_local_fragment _ _REQUIRE_DEPTH_NORMALS
			#pragma multi_compile_local_fragment _ _SOURCE_DEPTH_NORMALS
			#pragma multi_compile_local_fragment _ _UNLIT
			#pragma multi_compile_local_fragment _ _TRANSLUCENCY
			#pragma multi_compile_local_fragment _ _DEPTH_PREPASS_ENABLED

			#define _ADVANCED_SHADING 1
			#define _CAUSTICS 1
			#define UNDERWATER_ENABLED 1

			#include "../Libraries/Pipeline.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "../Libraries/Common.hlsl"
			#include "../Libraries/Input.hlsl"
			#include "../Libraries/Caustics.hlsl"
			#include "../Underwater/UnderwaterFog.hlsl"
			#include "../Underwater/UnderwaterShading.hlsl"
			#include "../Libraries/Waves.hlsl"
			#include "../Libraries/Lighting.hlsl"
			#include "UnderwaterEffects.hlsl"

			#if _SOURCE_DEPTH_NORMALS && SHADER_LIBRARY_VERSION_MAJOR >= 10
            #define DEPTH_NORMALS_PREPASS_AVAILABLE
            #else
            #undef DEPTH_NORMALS_PREPASS_AVAILABLE
            #endif
			
			#if _REQUIRE_DEPTH_NORMALS
			#ifndef DEPTH_NORMALS_PREPASS_AVAILABLE
			TEXTURE2D(_CameraDepthNormalsTexture); SAMPLER(sampler_CameraDepthNormalsTexture);
			#else
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
			#endif
			float3 SampleDepthNormals(float2 uv)
			{
				#ifdef DEPTH_NORMALS_PREPASS_AVAILABLE
				return half3(SampleSceneNormals(uv));
				#else
				return SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, uv).rgb;
				#endif
			}
			#endif

			float4x4 unity_WorldToLight;
			
			struct FullScreenAttributes
			{
				float3 positionOS   : POSITION;
				float2 uv           : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct FullScreenVaryings
			{
				half4 positionCS    : SV_POSITION;
				half3 positionWS    : TEXCOORD2;
				half4 uv            : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			TEXTURE2D_X(_MainTex); SAMPLER(sampler_MainTex); float4 _MainTex_TexelSize;

			FullScreenVaryings Vertex(FullScreenAttributes input)
			{
				FullScreenVaryings output;

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				
				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				output.uv.xy = UnityStereoTransformScreenSpaceTex(input.uv);
				output.uv.zw = ComputeScreenPos(output.positionCS.xyzw).xy;
				
				output.positionWS = TransformObjectToWorld(input.positionOS);

				return output;
			}
			
			//Note: ZW components is normalized screen position
			float ResolveShadowMask(float3 worldPos)
			{
			    //Fetch shadow coordinates for cascade.
			    float4 coords = TransformWorldToShadowCoord(worldPos);

			    // Screenspace shadowmap is only used for directional lights which use orthogonal projection.
			    ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
			    half4 shadowParams = GetMainLightShadowParams();
			    return SampleShadowmap(TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture), coords, shadowSamplingData, shadowParams, false);
			}
			
			half4 Fragment(FullScreenVaryings input) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				//return float4(input.wavePos.xy, 0, 1);
				float2 uv = input.uv.xy;

				float sceneDepth = SampleSceneDepth(uv);
				float3 worldPos = GetWorldPosition(float4(uv.xy, input.uv.zw), sceneDepth);
				float skyboxMask = Linear01Depth(sceneDepth, _ZBufferParams) > 0.99 ? 1 : 0;
	
				//return float4(skyboxMask.rrr, 1);
				float waterSurfaceMask = 0;
				
				//return float4(waterSurfaceMask.xxx, 1);		
				//Clip by skybox
				worldPos *= 1-skyboxMask;

#if _DEPTH_PREPASS_ENABLED
				float2 waterInfo = SampleWaterSurfaceDepth(uv);
				float waterDepth = waterInfo.r;
				//return float4(waterInfo.r.xxx, 1);
				uint waterSign = waterInfo.g;
				float waterSurfaceMaskUnder = ceil(waterDepth) * 1-waterSign;

				if (sceneDepth < waterDepth) waterSurfaceMask = saturate(1+skyboxMask);
				waterSurfaceMask *= waterSign;
				
				float4 waterPos = float4(GetWorldPosition(float4(uv.xy, input.uv.zw), waterDepth), waterSurfaceMask);
				//Mask by under/above water surface
				waterPos *= saturate(waterSurfaceMask + waterSurfaceMaskUnder);
				//return float4(frac(waterPos.xyz), 1);

				//Alpha test geometry
				if(sceneDepth > waterDepth) waterSurfaceMaskUnder = 0;
				float3 worldPosCombined = lerp(worldPos, waterPos.xyz, waterSurfaceMaskUnder);
#else
				float waterDepth = 0;
				float waterSurfaceMaskUnder = 0;
				uint waterSign = 1;
				float3 worldPosCombined = worldPos;
#endif

				//return float4(frac(worldPosCombined.xyz), 1);	
				
				float underwaterMask = SAMPLE_TEXTURE2D(_UnderwaterMask, sampler_UnderwaterMask, uv).r;
				
#ifdef DEBUG_WORLD_POS
return float4(frac(worldPosCombined).xyz, 1);
#endif

				float shadowMask = 1;
				#if _ADVANCED_SHADING && !SHADER_API_GLES3
				shadowMask = ResolveShadowMask(worldPos);
				#endif
			
				float sceneMask = saturate(underwaterMask) * 1-skyboxMask;

				half4 screenColor = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv);

				float fogDistFactor = 0;
				float fogHeightFactor = 0;
				float waterDensity = ComputeDensity(worldPos, _WaterLevel, 1.0, fogDistFactor, fogHeightFactor);

				//waterDensity *= 1-(exp(-(_WaterLevel+2 - worldPos.y)) / 1);

				waterDensity *= underwaterMask;
				waterDensity -= waterSurfaceMaskUnder;
				waterDensity = saturate(waterDensity);
				//return float4(waterDensity.xxx, 1);	

#if _CAUSTICS				
				float2 projection = worldPos.xz;
				#if _REQUIRE_DEPTH_NORMALS
				//Project from directional light. No great, projection rotates around the light's position just like a cookie
				float3 lightProj = mul((float4x4)unity_WorldToLight, float4(worldPos, 1.0)).xyz;
				projection = lightProj.xy;
				#endif

				float3 caustics = SampleCaustics(projection, _TimeParameters.x * _CausticsSpeed, _CausticsTiling) * _CausticsBrightness;
				caustics *= saturate( sceneMask * (1-waterDensity) * underwaterMask * 1-waterSurfaceMaskUnder) * length(_MainLightColorUnderwater.rgb) * shadowMask;

				//Use depth normals in URP 10 for angle masking
#if _REQUIRE_DEPTH_NORMALS
				float3 viewNormal = SampleDepthNormals(uv) * 2.0 - 1.0;
				float3 worldNormal = normalize(mul((float3x3)unity_MatrixInvV , viewNormal).xyz);

				float NdotL = saturate(dot(worldNormal, _MainLightDir.xyz));
				
				#ifdef DEBUG_WORLD_NORMALS
				return float4(saturate(worldNormal), 1.0);
				//return float4(NdotL.xxx, 1);
				#endif
				
				caustics *= NdotL;
#endif
				
#if _ADVANCED_SHADING
				//Fade the effect out as the sun approaches the horizon (80 to 90 degrees)
				half sunAngle = saturate(dot(float3(0, 1, 0), _MainLightDir));
				half angleMask = saturate(sunAngle * 10); /* 1.0/0.10 = 10 */
				caustics *= angleMask;
#endif

				screenColor.rgb += caustics;
#endif
				
				float3 waterColor = 0;
				GetUnderwaterFogColor(_WaterShallowColor.rgb, _WaterDeepColor.rgb, fogDistFactor, fogHeightFactor, waterColor);

				float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPosCombined);
				
				#if !_UNLIT
				ApplyLighting(waterColor, shadowMask, float3(0,1,0), viewDir);
				#endif

				#if _TRANSLUCENCY
				TranslucencyData translucencyData = PopulateTranslucencyData(_WaterShallowColor.rgb, _MainLightDir,  _MainLightColorUnderwater.rgb, viewDir, float3(0,1,0), float3(0,1,0), 0, _TranslucencyParams);
				translucencyData.strength *= _UnderwaterFogBrightness * _UnderwaterSubsurfaceStrength;
				ApplyTranslucency(translucencyData, waterColor);
				#endif

				screenColor.rgb = lerp(screenColor.rgb, waterColor.rgb, waterDensity);

				return screenColor;
			}

			ENDHLSL
		}
	}
}
