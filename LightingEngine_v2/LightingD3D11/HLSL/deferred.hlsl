#include "GBuffer.hlsl"

float4 Deferred(FullScreenTriangleVSOut input, uint sampleIndex)
{
	// How many total lights?
	uint totalLights, dummy;
	gLight.GetDimensions(totalLights, dummy);

	float3 lit = float3(0.0f, 0.0f, 0.0f);
	MaterialOutput surface = ComputeSurfaceDataFromGBufferSample(uint2(input.PositionViewport.xy), sampleIndex);

	// Avoid shading skybox/background pixels
	if (surface.positionView.z < CameraNearFar.y) 
	{
		for (uint lightIndex = 0; lightIndex < totalLights; ++lightIndex) 
		{
			PointLight light = gLight[lightIndex];
			AccumulateBRDF(surface, light, lit);
		}
	}

	//return gGBufferTextures[3].Load(input.PositionViewport.xy, sampleIndex).xyzw;;
	return float4(lit, 1.0f);
}

float4 DeferredPS(FullScreenTriangleVSOut input) : SV_Target
{
	// Shade only sample 0
	return Deferred(input, 0);
}

float4 DeferredPerSamplePS(FullScreenTriangleVSOut input, uint sampleIndex : SV_SampleIndex) : SV_Target
{
	return Deferred(input, sampleIndex);
	//return float4(1, 0, 0, 1);
}