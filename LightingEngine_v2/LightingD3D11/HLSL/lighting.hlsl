struct PointLight
{
	float3 positionView;
	float attenuationBegin;
	float3 color;
	float attenuationEnd;
};

StructuredBuffer<PointLight> gLight : register(t5);

float3 ComputeFaceNormal(float3 position)
{
	return cross(ddx_coarse(position), ddy_coarse(position));
}

float linstep(float min, float max, float v)
{
	return saturate((v - min) / (max - min));
}

// As below, we separate this for diffuse/specular parts for convenience in deferred lighting
void AccumulatePhongBRDF(float3 normal, float3 lightDir, float3 viewDir, float3 lightContrib, float specularPower, inout float3 litDiffuse, inout float3 litSpecular)
{
	// Simple Phong
	float NdotL = dot(normal, lightDir);
	[flatten]
	if (NdotL > 0.0f)
	{
		float3 r = reflect(lightDir, normal);
			float RdotV = max(0.0f, dot(r, viewDir));
		float specular = pow(RdotV, specularPower);

		litDiffuse += lightContrib * NdotL;
		litSpecular += lightContrib * specular;
	}
}

// Uses an in-out for accumulation to avoid returning and accumulating 0
void AccumulateBRDF(MaterialOutput surface, PointLight light, inout float3 lit)
{
	float3 directionToLight = light.positionView - surface.positionView;
	float distanceToLight = length(directionToLight);

	[branch] 
	if (distanceToLight < light.attenuationEnd) 
	{
		float attenuation = linstep(light.attenuationEnd, light.attenuationBegin, distanceToLight);
		directionToLight *= rcp(distanceToLight);       // A full normalize/RSQRT might be as fast here anyways...

		float3 litDiffuse = float3(0.0f, 0.0f, 0.0f);
		float3 litSpecular = float3(0.0f, 0.0f, 0.0f);
		AccumulatePhongBRDF(surface.normal, directionToLight, normalize(surface.positionView),
		attenuation * light.color, surface.specularPower, litDiffuse, litSpecular);

		lit += surface.albedo.rgb * (litDiffuse + surface.specularAmount * litSpecular);
	}
}