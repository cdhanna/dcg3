//float4x4 World;
//float4x4 View;
//float4x4 Projection;

uniform extern float4x4 WorldViewProj : WORLDVIEWPROJECTION;


float specularIntensity = 0.8f;
float specularPower = 0.5f;

texture Texture;
texture NormalMap;

sampler diffuseSampler = sampler_state
{
	Texture = (Texture);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler normalSampler = sampler_state
{
	Texture = (NormalMap);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
	float4 Color : COLOR0;
};
struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 Color: COLOR0;
	float2 TexCoord : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float2 Depth : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	/*float4 worldPosition = mul(input.Position, World);
		float4 viewPosition = mul(worldPosition, View);
		output.Position = mul(viewPosition, Projection)*/
	output.Position = mul(input.Position, WorldViewProj);
	output.TexCoord = input.TexCoord;                            //pass the texture coordinates further
	output.Normal = input.Normal;               //get normal into world space
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;
	output.Color = input.Color;
	return output;
}

struct PixelShaderOutput
{
	float4 Color : COLOR0;
	float4 Normal : COLOR1;
	float4 Depth : COLOR2;
};

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;
	//output.Color = input.Color;
	output.Color = tex2D(diffuseSampler, input.TexCoord) * input.Color;            //output Color
	output.Color.a = specularIntensity;                                              //output SpecularIntensity
	
	float4 normalMap = tex2D(normalSampler, input.TexCoord);

	output.Normal.rgb = 0.5f * (normalize(-input.Normal) + 1.0f);    //transform normal domain
	
	//output.Normal.rgb = normalize(output.Normal.rgb + normalMap.rgb);

	output.Normal.a = specularPower;                                            //output SpecularPower

	output.Depth = input.Depth.x / input.Depth.y;                           //output Depth
	return output;
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