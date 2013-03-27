float2 PosOffset;
float2 PosScale;

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
	output.Position.xy = input.Position * PosScale + PosOffset;
	output.Position.zw = float2(0,1);
	output.Texcoord = input.Position;
    return output;
}


float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 v = input.Texcoord * 2 - 1;
	v = pow(v, 10);
	float f = sqrt(max(0, dot(v,v)*0.5));
	clip(f - 0.001f);
	return 1.0 - f;
}

technique Normal
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}