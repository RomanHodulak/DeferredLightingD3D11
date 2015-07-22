struct VS_Input
{
	float3 Position : Position;
	float3 Normal : Normal0;
	float2 TexCoord : TexCoord0;
	float3 Tangent : Tangent0;
	float3 Binormal : Binormal0;
};
struct VS_Output
{
	float4 Position : SV_Position;
	float3 ViewPosition : Position0;
	float3 Normal : Normal0;
	float2 TexCoord : TexCoord0;
	float3 Tangent : Tangent0;
	float3 Binormal : Binormal0;
};
cbuffer PerObject : register(b0)
{
	float4x4 World;
	float4x4 ViewProj;
	float4x4 View;
	float4x4 WorldViewInverseTranspose;
}

VS_Output VS(VS_Input input)
{
	VS_Output output;
	input.Position.z *= -1;
	input.Normal.z *= -1;
	input.Binormal.z *= -1;
	input.Tangent.z *= -1;
	float4 worldPosition = mul(float4(input.Position, 1), World);
	output.Position = mul(worldPosition, ViewProj);
	output.ViewPosition = mul(worldPosition, View);
	output.TexCoord = input.TexCoord;
	output.Normal = (mul(input.Normal, (float3x3)WorldViewInverseTranspose));
	output.Binormal = (mul(input.Binormal, (float3x3)WorldViewInverseTranspose));
	output.Tangent = (mul(input.Tangent, (float3x3)WorldViewInverseTranspose));
	return output;
}