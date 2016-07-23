texture colorMap;
texture lightMap;

float4 ambient;

sampler colorSampler = sampler_state
{
	Texture = (colorMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = LINEAR;
};
sampler lightSampler = sampler_state
{
	Texture = (lightMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = LINEAR;
};
struct VertexShaderInput
{
	float3 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};
struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};
float2 halfPixel;
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.Position = float4(input.Position, 1);
	output.TexCoord = input.TexCoord - halfPixel;
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 diffuse = tex2D(colorSampler, input.TexCoord);
	float4 light = tex2D(lightSampler, input.TexCoord);


	float3 result = (diffuse.rgb * (ambient.rgb + light.rgb) ) + light.a;
	return float4(result, 1);
	
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
