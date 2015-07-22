#include "materialPS.hlsl"
#include "fullscreenVS.hlsl"
#include "lighting.hlsl"

cbuffer PerFrameConstants : register(b0)
{
	float4x4 CameraProj;
	float4 CameraNearFar;
};

struct GBuffer
{
	float4 normal_specular : SV_Target0;
	float4 albedo : SV_Target1;
	float2 positionZGrad : SV_Target2;
};

// Above values PLUS depth buffer (last element)
Texture2DMS<float4, MSAA_SAMPLES> gGBufferTextures[4] : register(t0);

float2 EncodeSphereMap(float3 n)
{
	float oneMinusZ = 1.0f - n.z;
	float p = sqrt(n.x * n.x + n.y * n.y + oneMinusZ * oneMinusZ);
	return n.xy / p * 0.5f + 0.5f;
}

float3 DecodeSphereMap(float2 e)
{
	float2 tmp = e - e * e;
		float f = tmp.x + tmp.y;
	float m = sqrt(4.0f * f - 1.0f);

	float3 n;
	n.xy = m * (e * 4.0f - 2.0f);
	n.z = 3.0f - 8.0f * f;
	return n;
}

float3 ComputePositionViewFromZ(float2 positionScreen, float viewSpaceZ)
{
	float2 screenSpaceRay = float2(positionScreen.x / CameraProj._11, positionScreen.y / CameraProj._22);

	float3 positionView;
	positionView.z = viewSpaceZ;
	// Solve the two projection equations
	positionView.xy = screenSpaceRay.xy * positionView.z;

	return positionView;
}

MaterialOutput ComputeSurfaceDataFromGBufferSample(uint2 positionViewport, uint sampleIndex)
{
	// Load the raw data from the GBuffer
	GBuffer rawData;
	rawData.normal_specular = gGBufferTextures[0].Load(positionViewport.xy, sampleIndex).xyzw;
	rawData.albedo = gGBufferTextures[1].Load(positionViewport.xy, sampleIndex).xyzw;
	rawData.positionZGrad = gGBufferTextures[2].Load(positionViewport.xy, sampleIndex).xy;
	float zBuffer = gGBufferTextures[3].Load(positionViewport.xy, sampleIndex).x;

	float2 gbufferDim;
	uint dummy;
	gGBufferTextures[0].GetDimensions(gbufferDim.x, gbufferDim.y, dummy);

	// Compute screen/clip-space position and neighbour positions
	// NOTE: Mind DX11 viewport transform and pixel center!
	// NOTE: This offset can actually be precomputed on the CPU but it's actually slower to read it from
	// a constant buffer than to just recompute it.
	float2 screenPixelOffset = float2(2.0f, -2.0f) / gbufferDim;
	float2 positionScreen = (float2(positionViewport.xy) + 0.5f) * screenPixelOffset.xy + float2(-1.0f, 1.0f);
	float2 positionScreenX = positionScreen + float2(screenPixelOffset.x, 0.0f);
	float2 positionScreenY = positionScreen + float2(0.0f, screenPixelOffset.y);

	// Decode into reasonable outputs
	MaterialOutput data;

	// Unproject depth buffer Z value into view space
	float viewSpaceZ = CameraProj._43 / (zBuffer - CameraProj._33);

	data.positionView = ComputePositionViewFromZ(positionScreen, viewSpaceZ);
	data.positionViewDX = ComputePositionViewFromZ(positionScreenX, viewSpaceZ + rawData.positionZGrad.x) - data.positionView;
	data.positionViewDY = ComputePositionViewFromZ(positionScreenY, viewSpaceZ + rawData.positionZGrad.y) - data.positionView;

	data.normal = DecodeSphereMap(rawData.normal_specular.xy);
	data.albedo = rawData.albedo;

	data.specularAmount = rawData.normal_specular.z;
	data.specularPower = rawData.normal_specular.w;

	return data;
}

void ComputeSurfaceDataFromGBufferAllSamples(uint2 positionViewport, out MaterialOutput surface[MSAA_SAMPLES])
{
	// Load data for each sample
	// Most of this time only a small amount of this data is actually used so unrolling
	// this loop ensures that the compiler has an easy time with the dead code elimination.
	[unroll] 
	for (uint i = 0; i < MSAA_SAMPLES; ++i) 
	{
		surface[i] = ComputeSurfaceDataFromGBufferSample(positionViewport, i);
	}
}

// Check if a given pixel can be shaded at pixel frequency (i.e. they all come from
// the same surface) or if they require per-sample shading
bool RequiresPerSampleShading(MaterialOutput surface[MSAA_SAMPLES])
{
	bool perSample = false;

	const float maxZDelta = abs(surface[0].positionViewDX.z) + abs(surface[0].positionViewDY.z);
	const float minNormalDot = 0.99f;        // Allow ~8 degree normal deviations

	[unroll] for (uint i = 1; i < MSAA_SAMPLES; ++i) {
		// Using the position derivatives of the triangle, check if all of the sample depths
		// could possibly have come from the same triangle/surface
		perSample = perSample ||
			abs(surface[i].positionView.z - surface[0].positionView.z) > maxZDelta;

		// Also flag places where the normal is different
		perSample = perSample ||
			dot(surface[i].normal, surface[0].normal) < minNormalDot;
	}

	return perSample;
}

// Initialize stencil mask with per-sample/per-pixel flags
void RequiresPerSampleShadingPS(FullScreenTriangleVSOut input)
{
	MaterialOutput surfaceSamples[MSAA_SAMPLES];
	ComputeSurfaceDataFromGBufferAllSamples(uint2(input.PositionViewport.xy), surfaceSamples);
	bool perSample = RequiresPerSampleShading(surfaceSamples);

	// Kill fragment (i.e. don't write stencil) if we don't require per sample shading
	[flatten] if (!perSample) {
		discard;
	}
}

//--------------------------------------------------------------------------------------
// G-buffer rendering
//--------------------------------------------------------------------------------------
void GBufferPS(VS_Output input, out GBuffer outputGBuffer)
{
	MaterialOutput surface = MaterialPS(input);
	outputGBuffer.normal_specular = float4(EncodeSphereMap(surface.normal), surface.specularAmount, surface.specularPower);
	outputGBuffer.albedo = surface.albedo;
	outputGBuffer.positionZGrad = float2(ddx_coarse(surface.positionView.z), ddy_coarse(surface.positionView.z));
}

void GBufferAlphaTestPS(VS_Output input, out GBuffer outputGBuffer)
{
	GBufferPS(input, outputGBuffer);

	// Alpha test
	clip(outputGBuffer.albedo.a - 0.3f);

	// Always use face normal for alpha tested stuff since it's double-sided
	outputGBuffer.normal_specular.xy = EncodeSphereMap(normalize(ComputeFaceNormal(input.ViewPosition)));
}