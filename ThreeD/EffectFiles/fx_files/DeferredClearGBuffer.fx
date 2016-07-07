struct VertexShaderInput {
	float3 Position : POSITION0;
};
struct VertexShaderOutput {
	float4 Position : POSITION0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input) {
	VertexShaderOutput output;
	output.Position = float4(input.Position, 1);
	return output;
}


struct PixelShaderOutput {
	float4 Color : COLOR0;
	float4 Normal : COLOR1;
	float4 Depth : COLOR2;
};

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input){
	PixelShaderOutput output;

	//black color
	output.Color = 0.0f;
	output.Color.a = 0.0f;

	output.Normal.rgb = 0.5f;
	output.Normal.a = 0.0f;

	output.Depth = 1.0f;
	return output;

}


technique Technique1 {
	pass Pass1 {

#if SM4

		PixelShader = compile vs_4_0_level_9_1 PixelShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();

#elif SM3

		PixelShader = compile vs_3_0 PixelShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();

#else

		PixelShader = compile vs_2_0 PixelShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();

#endif

	}
}
