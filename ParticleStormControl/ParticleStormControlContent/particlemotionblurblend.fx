float4x4 ProjectionMatrix;

texture2D oldtex : register(c0);
sampler2D OldSampler
{
	Texture = oldtex;
};

texture2D newtex : register(c1);
sampler2D NewSampler
{
	Texture = newtex;
};

float oldFactor;

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, ProjectionMatrix);
	output.Texcoord = input.Texcoord;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 old = tex2D(OldSampler, input.Texcoord);
	float4 newColor = tex2D(NewSampler, input.Texcoord);
    return newColor + old;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
