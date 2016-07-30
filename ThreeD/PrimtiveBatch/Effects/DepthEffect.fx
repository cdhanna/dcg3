
uniform extern float4x4 WorldViewProj : WORLDVIEWPROJECTION;


struct VertexInput {
	float4 Position: POSITION0;
	float3 Normal: NORMAL0;
};
struct VertexOutput {
	float4 Position: POSITION0;
	float2 Depth: TEXCOORD0;
};

VertexOutput VertexShaderFunction(VertexInput input){
	VertexOutput output;

	output.Position = mul(input.Position, WorldViewProj);
	output.Position.w = 1;
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;


	return output;
}

struct PixelOutput {
	float4 Color: COLOR0;
};

PixelOutput PixelShaderFunction(VertexOutput input){
	PixelOutput output;

	output.Color = input.Depth.x / input.Depth.y;
    
    //output.Color = 1 - output.Color;
	//output.Color.r = 1.0f;
	return output;
}

technique Technique1
{
	pass Pass1
	{
#if SM4
		VertexShader = compile vs_4_0_level_9_3 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_3 PixelShaderFunction();

#elif SM3

		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();

#else
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();

#endif
	}
}