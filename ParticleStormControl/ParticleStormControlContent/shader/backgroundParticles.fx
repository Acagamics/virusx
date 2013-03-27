float2 PosScale;
float2 PosOffset;
float2 RelativeMax;
float ParticleMoving;

struct VertexShaderInput
{
    float2 Position	: POSITION0;
	float2 InstancePos : TEXCOORD0;
	float2 InstanceDir : TEXCOORD1;
	float Size : TEXCOORD2;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	float2 particlePos = frac(input.InstancePos + ParticleMoving * input.InstanceDir / input.Size);
	output.Position.xy = input.Position * input.Size / RelativeMax + particlePos;
	output.Position.xy = output.Position.xy * PosScale + PosOffset;

	output.Position.zw = float2(0,1);
	output.Texcoord = input.Position + float2(0.5f, 0.5f);
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 v = (input.Texcoord * 2 - 1);
	float distSq = dot(v,v);
	float distSqInv = 1 - distSq;
	return (distSqInv - distSqInv*distSqInv) * 0.5f;
}

technique WithFalloff
{
    pass Pass1
    {
		CullMode = None;
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}