#define MAX_NUM_CELLS 16
float2 Cells_Pos2D[MAX_NUM_CELLS];
float3 Cells_Color[MAX_NUM_CELLS];

float2 PosOffset;
float2 PosScale;
float2 RelativeMax;

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
	float2 v = input.Texcoord * RelativeMax;

	const float FALLOFF = 40.0f;
	const float FACTOR = -(1.0f/10.0f);
	float cellFactor = 0.0f;

	[unroll] for(int i=0; i<MAX_NUM_CELLS; ++i)
	{
		float dist = distance(v, Cells_Pos2D[i]);
		cellFactor += exp(-FALLOFF * dist);
    }
	cellFactor = min(1.0, FACTOR * log(cellFactor));

	float4 outColor;
	outColor = cellFactor * 0.8f;
	outColor.a = cellFactor * 1.2f;
	return outColor;
}

technique WithFalloff
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}