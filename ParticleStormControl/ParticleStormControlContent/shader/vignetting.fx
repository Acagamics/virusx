texture ScreenTexture;
sampler2D sampScreen = sampler_state
{	
	Texture = <ScreenTexture>;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

float2 Vignetting_PosOffset;
float2 Vignetting_PosScale;
float2 HalfPixelCorrection;

struct VertexShaderInput
{
    float2 Position	: POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	output.Position.xy = input.Position;
	output.Position.zw = float2(0,1);
	output.Texcoord.xy = input.Position * 0.5f + 0.5f;
	output.Texcoord.y = 1.0f - output.Texcoord.y;
	output.Texcoord += HalfPixelCorrection;
    return output;
}


float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(sampScreen, input.Texcoord);

	float2 vignettCord = saturate(input.Texcoord * Vignetting_PosScale + Vignetting_PosOffset);
	float2 v = vignettCord * 2 - 1;
	v = pow(v, 10);
	float vignettFactor = 1.0f - sqrt(max(0, dot(v,v)*0.5));
	
	return color * vignettFactor;
}

technique Normal
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}