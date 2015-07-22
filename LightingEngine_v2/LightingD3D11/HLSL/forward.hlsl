#include "materialPS.hlsl"
#include "lighting.hlsl"

float4 ForwardPS(VS_Output input) : SV_Target
{
	// How many total lights?
	uint totalLights, dummy;
	gLight.GetDimensions(totalLights, dummy);

	float3 lit = float3(0.0f, 0.0f, 0.0f);

	MaterialOutput surface = MaterialPS(input);
	for (uint lightIndex = 0; lightIndex < totalLights; ++lightIndex)
	{
		PointLight light = gLight[lightIndex];
		AccumulateBRDF(surface, light, lit);
	}
	return float4(lit, 1.0f);
}

float4 ForwardAlphaTestPS(VS_Output input) : SV_Target
{
	// Always use face normal for alpha tested stuff since it's double-sided
	input.Normal = cross(ddx_coarse(input.ViewPosition), ddy_coarse(input.ViewPosition));

	// Alpha test: dead code and CSE will take care of the duplication here
	MaterialOutput surface = MaterialPS(input);
	clip(surface.albedo.a - 0.3f);

	// Otherwise run the normal shader
	return ForwardPS(input);
}

// Does ONLY alpha test, not color. Useful for pre-z pass
void ForwardAlphaTestOnlyPS(VS_Output input)
{
	// Alpha test: dead code and CSE will take care of the duplication here
	MaterialOutput surface = MaterialPS(input);
	clip(surface.albedo.a - 0.3f);
}