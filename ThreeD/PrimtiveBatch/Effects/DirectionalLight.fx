//direction of the light
float3 lightDirection;
//color of the light 
float3 Color;
//position of the camera, for specular light
float3 cameraPosition;
//this is used to compute the world-position
float4x4 InvertViewProjection;
// diffuse color, and specularIntensity in the alpha channel
texture colorMap;
// normals, and specularPower in the alpha channel
texture normalMap;
//depth
texture depthMap;

texture shadowMap;
float4x4 lightMatrix;
bool shadowsEnabled;

sampler colorSampler =sampler_state
{
	Texture = (colorMap);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};
sampler depthSampler = sampler_state
{
	Texture = (depthMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
};
sampler normalSampler = sampler_state
{
	Texture = (normalMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
};

sampler shadowMapSampler = sampler_state
{
	Texture = (shadowMap);
	AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = ANISOTROPIC;
    MinFilter = ANISOTROPIC;
    Mipfilter = ANISOTROPIC;
};


struct VertexShaderInput
{
	float3 Position : POSITION0;
    //float4 Color : COLOR0;
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
	//align texture coordinates
	output.TexCoord = input.TexCoord - halfPixel;
	//output.Color = float4(1, 0, 0, 1);


	return output;
}

float sampleShadowMap(float2 coords, float compare, float bias){
	//return step(compare, tex2D(shadowMapSampler, coords.xy).r  );

    float distance = tex2D(shadowMapSampler, coords.xy ).r;
    if (distance < compare - (bias / 1024.0f))
    {
        return 0;
    }
    return 1;


}
float getShadowAmount(float4 shadowMapCoords, float bias){

	float3 coords = (shadowMapCoords.xyz / shadowMapCoords.w) * 0.5f + 0.5f;
		//return coords;
    return sampleShadowMap(coords.xy , shadowMapCoords.z, bias);
}


float vsmUpperBound(float2 moments, float t)
{
    float p = (t <= moments.x);
    float variance = moments.y - (moments.x * moments.x);
    variance = max(variance, .00001f);
    float d = t - moments.x;
    float p_max = variance / (variance + d * d);
    //return moments.x;
    return max(p, p_max);
}
float vsmShadow(float2 lightTexCoord, float distanceToLight)
{
    float2 moments = tex2D(shadowMapSampler, lightTexCoord).rg;
    return vsmUpperBound(moments, distanceToLight);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	//return tex2D(normalSampler, input.TexCoord);

	//get normal data from the normalMap
	float4 normalData = tex2D(normalSampler, input.TexCoord);
	//tranform normal back into [-1,1] range
	float3 normal = 2.0f * normalData.xyz - 1.0f;
	//get specular power, and get it into [0,255] range]
	float specularPower = normalData.a * 255;
	//get specular intensity from the colorMap
	float specularIntensity = tex2D(colorSampler, input.TexCoord).a;

	//read depth
	float depthVal = tex2D(depthSampler, input.TexCoord).r;
	//compute screen-space position
	float4 position;
	position.x = input.TexCoord.x * 2.0f - 1.0f;
	position.y = -(input.TexCoord.y * 2.0f - 1.0f);
	position.z = depthVal;
	position.w = 1.0f;
	//transform to world space
	position = mul(position, InvertViewProjection);
	position /= position.w;


	//surface-to-light vector
	float3 lightVector = -normalize(lightDirection);
	//compute diffuse light
	float NdL = max(0, dot(normal, lightVector));
	float3 diffuseLight = NdL * Color.rgb;
	//reflexion vector
	float3 reflectionVector = normalize(reflect(lightVector, normal));
	//camera-to-surface vector
	float3 directionToCamera = normalize(cameraPosition - float3(position.xyz));
	//compute specular light
	float specularLight = specularIntensity * pow(saturate(dot(reflectionVector, directionToCamera)), specularPower);
	//output the two lights
	
	float4 smc;
	//smc.x = input.TexCoord.x * 2.0f - 1.0f;
	//smc.y = -(input.TexCoord.y * 2.0f - 1.0f);
	//smc.z = depthVal;
	//smc.w = 1.0f;
	////transform to world space
	//smc = mul(smc, lightMatrix);
	//smc /= smc.w;
	smc = mul(position, lightMatrix);
	smc /= smc.w;

    smc.y *= -1;


    float3 coords = (smc.xyz / smc.w) * 0.5f + 0.5f;
    float distance = tex2D(shadowMapSampler, coords.xy).r ;
    //return float4(smc.z, distance, (smc.z < distance) ? 1 : 0, 1);
    //return step(distance, coords.z);

    //return float4(NdL, NdL, NdL, 1);
	//return position;
	//return float4(getShadowAmount(smc), 1);
	//return float4(input.TexCoord.xy, 1, 1); // SHOWS SCREEN SPACE

	//return float4(depthVal, depthVal, depthVal, 1);
	//return float4(coords.zzz, 1);
    float bias = 5 * (1 - NdL);

	float shadowContrib = vsmShadow(coords.xy, smc.z);
	//shadowsEnabled = true;
	//shadowContrib = 0;
    return float4(diffuseLight.rgb, specularLight) * (shadowsEnabled ? shadowContrib : 1);
    

	//return float4(diffuseLight.rgb, specularLight) * (shadowsEnabled ? getShadowAmount(smc, bias) : 1);
    //return float4(diffuseLight.rgb, specularLight) ;

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
