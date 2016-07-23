
struct VertexShaderInput
{
	float3 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.Position = float4(input.Position, 1);
	output.TexCoord = input.TexCoord;
	return output;
}
struct PixelShaderOutput
{
	float4 Color : COLOR0;
	float4 Normal : SV_TARGET1;
	float4 Depth : SV_TARGET2;
};
PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;
	//black color
	output.Color = 0.0f;
	output.Color.a = 0.0f;
	//when transforming 0.5f into [-1,1], we will get 0.0f
	output.Normal.rgb = 0.5f;
	//no specular power
	output.Normal.a = 0.0f;
	//max depth
	output.Depth = float4(1, 1, 1, 1);
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