
uniform extern float4x4 WorldViewProj : WORLDVIEWPROJECTION;


struct VertexInput {
	float4 Position: POSITION0;
	float3 Normal: NORMAL0;
};
struct VertexOutput {
	float4 Position: POSITION0;
	float2 Depth: TEXCOORD0;
	float2 Pos: TEXCOORD1;
};

VertexOutput VertexShaderFunction(VertexInput input){
	VertexOutput output;

	output.Position = mul(input.Position, WorldViewProj);
	//output.Position.w = 1;
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;

	output.Pos.x = output.Position.x;
	output.Pos.y = output.Position.y;
	return output;
}

struct PixelOutput {
	float4 Color: COLOR0;
};

float2 calcMoments(float depth)
{
    float2 moments;
    moments.x = depth;
    float dx = ddx(depth);
    float dy = ddy(depth);
    moments.y = depth * depth + 0.25 + (dx * dx + dy * dy);
    return moments;
}

PixelOutput PixelShaderFunction(VertexOutput input){
	PixelOutput output;

	float depth = ( input.Depth.x / input.Depth.y );

    float2 moments = calcMoments(depth);
    //float2 changes = ddy(input.Depth);
    output.Color = float4(moments.x, moments.y, 0, 0);
	output.Color.r = moments.x;
	output.Color.g = moments.y;
    //output.Color = float4(moments.x, moments.y, 0, 0);
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