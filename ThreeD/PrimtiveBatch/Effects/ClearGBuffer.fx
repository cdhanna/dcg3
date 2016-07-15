struct VertexShaderInput
{
	float3 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.Position = float4(input.Position, 0);
	return output;
}
struct PixelShaderOutput
{
	float4 Color : SV_Target0;
	float4 Normal : SV_Target1;
	float4 Depth : SV_Target2;
};
PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;
	//black color
	output.Color = 0.0f;
	output.Color.a = 0.0f;
	//output.Color = float4(1, .5, .2, 1);
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
		/*VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
*/
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