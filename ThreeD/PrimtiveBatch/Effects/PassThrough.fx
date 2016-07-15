uniform extern float4x4 WorldViewProj : WORLDVIEWPROJECTION;

struct VS_OUTPUT
{
	float4 position : POSITION;
	float4 color : COLOR0;
};

VS_OUTPUT Transform(
	float4 Pos  : POSITION,
	float4 Color : COLOR0)
{
	VS_OUTPUT Out = (VS_OUTPUT)0;

	Out.position = mul(Pos, WorldViewProj);
	Out.color = Color;

	return Out;
}

float4 ApplyAPixelShader(VS_OUTPUT vsout) : SV_TARGET0
{
	//return vsout.color;
	return float4(1 , 0, 0, 1);
}

technique TransformTechnique
{
	pass Pass1
	{
#if SM4
		VertexShader = compile vs_4_0_level_9_1 Transform();
		PixelShader = compile ps_4_0_level_9_1 ApplyAPixelShader();

#elif SM3

		VertexShader = compile vs_3_0 Transform();
		PixelShader = compile ps_3_0 ApplyAPixelShader();

#else
		VertexShader = compile vs_2_0 Transform();
		PixelShader = compile ps_2_0 ApplyAPixelShader();

#endif

	}
}