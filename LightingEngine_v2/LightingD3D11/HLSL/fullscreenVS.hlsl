struct FullScreenTriangleVSOut
{
	float4 PositionViewport : SV_Position;
};

FullScreenTriangleVSOut FullScreenTriangleVS(uint vertexID : SV_VertexID)
{
	FullScreenTriangleVSOut output;

	// Parametrically work out vertex location for full screen triangle
	float2 grid = float2((vertexID << 1) & 2, vertexID & 2);
	output.PositionViewport = float4(grid * float2(2.0f, -2.0f) + float2(-1.0f, 1.0f), 1.0f, 1.0f);

	return output;
}