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

float4 Circle(VertexShaderOutput input) : COLOR0
{
	float2 v = (input.Texcoord * 2 - 1);
	float distSq = dot(v,v);
	float distSqInv = 1 - distSq;
	float intensity = (distSqInv - distSqInv*distSqInv)*2.5f;
	clip(intensity - 1.0f/255.0f);
	return intensity;
}

float4 FilledCircle(VertexShaderOutput input) : COLOR0
{
	float2 v = (input.Texcoord * 2 - 1);
	float intensity = (1.0f-dot(v,v)) * 0.5f;
	clip(intensity - 1.0f/255.0f);
	return intensity;
}


technique T0
{
    pass PCircle
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 Circle();
    }

    pass PFilledCircle
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 FilledCircle();
    }
}