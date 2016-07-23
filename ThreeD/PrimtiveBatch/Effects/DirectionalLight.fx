﻿//direction of the light
float3 lightDirection;
//color of the light 
float3 Color;
//position of the camera, for specular light
float3 cameraPosition;
//this is used to compute the world-position
float4x4 InvertViewProjection;
// diffuse color, and specularIntensity in the alpha channel
texture colorMap;
// normals, and specularPower in the alpha channel
texture normalMap;
//depth
texture depthMap;
sampler colorSampler =sampler_state
{
	Texture = (colorMap);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};


sampler depthSampler = sampler_state
{
	Texture = (depthMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
};
sampler normalSampler = sampler_state
{
	Texture = (normalMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
};

struct VertexShaderInput
{
	float3 Position : POSITION0;
    //float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
};
struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
    //float4 Color : COLOR0;
};
float2 halfPixel;
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.Position = float4(input.Position, 1);
	//align texture coordinates
	output.TexCoord = input.TexCoord - halfPixel;
	//output.Color = float4(1, 0, 0, 1);
	return output;
}


float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	//return tex2D(normalSampler, input.TexCoord);

	//get normal data from the normalMap
	float4 normalData = tex2D(normalSampler, input.TexCoord);
	//tranform normal back into [-1,1] range
	float3 normal = 2.0f * normalData.xyz - 1.0f;
	//get specular power, and get it into [0,255] range]
	float specularPower = normalData.a * 255;
	//get specular intensity from the colorMap
	float specularIntensity = tex2D(colorSampler, input.TexCoord).a;

	//read depth
	float depthVal = tex2D(depthSampler, input.TexCoord).g;
	//compute screen-space position
	float4 position;
	position.x = input.TexCoord.x * 2.0f - 1.0f;
	position.y = -(input.TexCoord.y * 2.0f - 1.0f);
	position.z = depthVal;
	position.w = 1.0f;
	//transform to world space
	position = mul(position, InvertViewProjection);
	position /= position.w;

	//surface-to-light vector
	float3 lightVector = -normalize(lightDirection);
	//compute diffuse light
	float NdL = max(0, dot(normal, lightVector));
	float3 diffuseLight = NdL * Color.rgb;
	//reflexion vector
	float3 reflectionVector = normalize(reflect(lightVector, normal));
	//camera-to-surface vector
	float3 directionToCamera = normalize(cameraPosition - float3(position.xyz));
	//compute specular light
	float specularLight = specularIntensity * pow(saturate(dot(reflectionVector, directionToCamera)), specularPower);
	//output the two lights
	return float4(diffuseLight.rgb, specularLight);
}

technique Technique1
{
	pass Pass1
	{
#if SM4
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();

#elif SM3

		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();

#else
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();

#endif


	}
}