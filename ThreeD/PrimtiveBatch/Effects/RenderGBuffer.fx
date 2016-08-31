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
	float3 PosW :TEXCOORD3;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.Position = mul(input.Position, WorldViewProj);
	output.TexCoord = input.TexCoord;                            //pass the texture coordinates further
	output.Normal = input.Normal;               //get normal into world space
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;
	output.Color = input.Color;

	output.PosW = input.Position.xyz / input.Position.w;
	
	return output;
}

struct PixelShaderOutput
{
	float4 Color : COLOR0;
	float4 Normal : COLOR1;
	float4 Depth : COLOR2;
};


float3x3 cotangent_frame(float3 N, float3 p, float2 uv) {

	float3 dp1 = ddx(p);
	float3 dp2 = ddy(p);
	float2 duv1 = ddx(uv);
	float2 duv2 = ddy(uv);

	float3 dp2perp = cross(dp2, N);
	float3 dp1perp = cross(N, dp1);
	float3 T = dp2perp * duv1.x + dp1perp * duv2.x;
	float3 B = dp2perp * duv1.y + dp1perp * duv2.y;

	float invmax = rsqrt(max(dot(T,T), dot(B,B)));

	return float3x3(T * invmax, B * invmax, N);
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;
	output.Color = tex2D(diffuseSampler, input.TexCoord) * input.Color;            //output Color
	output.Color.a = specularIntensity;                                              //output SpecularIntensity
	
	float4 normalMap = (tex2D(normalSampler, input.TexCoord)) * 2.0 - 1.0;
		normalMap.z = -normalMap.z;
		//normalMap = -normalMap;

	float3 position = 1;
	position.x = input.TexCoord.x * 2.0f - 1.0f;
	position.y = -(input.TexCoord.y * 2.0f - 1.0f);
	position.z = input.Depth.x / input.Depth.y;

	float3x3 TBN = cotangent_frame(normalize(input.Normal.xyz), -input.PosW, input.TexCoord);

	output.Normal.rgb = .5f * normalize(mul(normalMap.xyz, TBN)) + 1.0f;
	output.Normal.rgb -= .5f;

	//output.Normal.rgb = input.PosW;
	//output.Normal.rgb = 0.5f * (normalize(-input.Normal) + 1.0f);

	output.Normal.a = specularPower;                                            //output SpecularPower

	output.Depth = input.Depth.x / input.Depth.y;                           //output Depth

	//output.Depth = float4(input.Depth.y, 1, 0, 1);
	return output;
}





technique Technique1
{
	pass Pass1
	{

        //ZEnable = TRUE;
        //ZWriteEnable = TRUE;
        //AlphaBlendEnable = FALSE;
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