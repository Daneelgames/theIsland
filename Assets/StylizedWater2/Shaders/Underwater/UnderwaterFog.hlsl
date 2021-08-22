//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

#ifndef UNDERWATER_FOG_INCLUDED
#define UNDERWATER_FOG_INCLUDED

float _WaterLevel;

float4 _WaterShallowColor;
float4 _WaterDeepColor;

float _HorizontalDensity;
float _VerticalDensity;
float _VerticalDepth;
float _StartDistance;
float _UnderwaterFogBrightness;
float _UnderwaterSubsurfaceStrength;

#define BRIGHTNESS _UnderwaterFogBrightness

float ComputeVerticalDistance(float3 wPos, float multiplier)
{
	//Radial distance
	float dist = length(_WorldSpaceCameraPos.xz - wPos.xz);

	//Start distance
	dist -= _ProjectionParams.y + _StartDistance;
	dist *= _HorizontalDensity * multiplier;
	
	return saturate(1-(exp(-dist)));
}

#define HEIGHT _WaterHeight
#define HEIGHT_DENSITY _VerticalDensity

float ComputeHeight(float3 wpos, float waterLevel)
{
	waterLevel -= _VerticalDepth;
	
	float3 wsDir = _WorldSpaceCameraPos.xyz - wpos;
	float FH = waterLevel; //Height
	float3 P = wpos;
	float FdotC = _WorldSpaceCameraPos.y - waterLevel; //Camera/fog plane height difference
	float k = (FdotC <= 0.0f ? 1.0f : 0.0f); //Is camera below height fog
	float FdotP = P.y - FH;
	float FdotV = wsDir.y;
	float c1 = k * (FdotP + FdotC);
	float c2 = (1 - 2 * k) * FdotP;
	float g = min(c2, 0.0);
	g = -HEIGHT_DENSITY * (c1 - g * g / abs(FdotV + 1.0e-5f));
	return 1-exp(-g);
}

float ComputeDensity(float3 wPos, float heightLevel, float distanceMultiplier, out float distance, out float height)
{
	distance = ComputeVerticalDistance(wPos, distanceMultiplier) ;
	height = ComputeHeight(wPos, heightLevel);

	//Density
	return saturate(distance + height);
}

void GetWaterDensity_float(float4 wPos, out float density)
{
	density = saturate(ComputeVerticalDistance(wPos.xyz, 1.0) + ComputeHeight(wPos.xyz, _WaterLevel));
}

void GetUnderwaterFogColor(float3 shallow, float3 deep, float fogDistFactor, float fogHeightFactor, inout float3 color)
{
	float density = saturate(fogDistFactor + fogHeightFactor);

	float3 waterColor = lerp(shallow.rgb, deep.rgb, density) * BRIGHTNESS;
	color = lerp(color, waterColor.rgb, density);

	//color = density.rrr;
}
#endif