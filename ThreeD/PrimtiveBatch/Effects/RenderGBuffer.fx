float4x4 World;
float4x4 View;
float4x4 Projection;

float specularIntensity = 0.8f;
float specularPower = 0.5f;

texture Texture;

sampler diffuseSampler : register(s0)
{
	Texture = (Texture);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct VertexShaderInput
{
	float4 Position : SV_POSITION0;
	float3 Normal : SV_NORMAL0;
	float2 TexCoord : SV_TEXCOORD0;
};
struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float2 Depth : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
		float4 viewPosition = mul(worldPosition, View);
		output.Position = mul(viewPosition, Projection);
	output.TexCoord = input.TexCoord;                            //pass the texture coordinates further
	output.Normal = mul(input.Normal, World);                   //get normal into world space
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;
	return output;
}

struct PixelShaderOutput
{
	half4 Color : SV_Target0;
	half4 Normal : SV_Target1;
	half4 Depth : SV_Target2;
};

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;
	
	//output.Color = tex2D(diffuseSampler, input.TexCoord);            //output Color
	output.Color = 1;
	//output.Color.a = specularIntensity;                                              //output SpecularIntensity
	output.Color.a = 1;
	output.Color = half4(1, 0, 0, 1);
	
	output.Normal.rgb = 0.5f * (normalize(input.Normal) + 1.0f);    //transform normal domain
	output.Normal.a = specularPower;                                            //output SpecularPower
	output.Depth = input.Depth.x / input.Depth.y;                           //output Depth
	return output;
}





technique Technique1
{
	pass Pass1
	{
#if SM4

		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();

#elif SM3

		PixelShader = compile ps_3_0 PixelShaderFunction();

#else

		PixelShader = compile ps_2_0 PixelShaderFunction();

#endif


	}
}