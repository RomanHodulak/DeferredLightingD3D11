struct VS_Output
{
	float4 Position : SV_Position;
	float3 ViewPosition : Position0;
	float3 Normal : Normal0;
	float2 TexCoord : TexCoord0;
	float3 Tangent : Tangent0;
	float3 Binormal : Binormal0;
};

struct MaterialOutput
{
	float3 positionView;         // View space position
	float3 positionViewDX;       // Screen space derivatives
	float3 positionViewDY;       // of view space position
	float3 normal;               // View space normal
	float4 albedo;
	float specularAmount;        // Treated as a multiplier on albedo
	float specularPower;
};

SamplerState InterpolatedSampler : register(s0);
Texture2D cobblestone1 : register(t0);
Texture2D cobblestone1_NORM : register(t1);

MaterialOutput MaterialPS(VS_Output input)
{
	MaterialOutput output;
	float4 Texture2DSample0 = cobblestone1_NORM.Sample(InterpolatedSampler, input.TexCoord);
	float4 Texture2DSample2 = cobblestone1.Sample(InterpolatedSampler, input.TexCoord);
	float4 Color = Texture2DSample2 * 1;
	float4 NormalMap = 1 * Texture2DSample0 - float4(0.5, 0.5, 0.5, 0.5);
	float3 Normal = (NormalMap.x * input.Tangent) + (NormalMap.y * input.Binormal) + (input.Normal);

	output.positionView = input.ViewPosition;
	output.positionViewDX = ddx_coarse(output.positionView);
	output.positionViewDY = ddy_coarse(output.positionView);

	// Optionally use face normal instead of shading normal
	float3 faceNormal = cross(ddx_coarse(input.ViewPosition), ddy_coarse(input.ViewPosition));
		output.normal = normalize(Normal);

	output.albedo = Color;

	// Map NULL diffuse textures to white
	uint2 textureDim;
	cobblestone1.GetDimensions(textureDim.x, textureDim.y);
	output.albedo = (textureDim.x == 0U ? float4(1.0f, 1.0f, 1.0f, 1.0f) : output.albedo);

	output.specularAmount = 0.9f;
	output.specularPower = 35.0f;
	return output;
}