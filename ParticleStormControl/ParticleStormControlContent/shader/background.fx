#define MAX_NUM_CELLS 16
float2 Cells_Pos2D[MAX_NUM_CELLS];
float3 Cells_Color[MAX_NUM_CELLS];

int NumCells;
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
	float2 v = input.Texcoord;//(input.Texcoord - 0.5f)*2;
	
	float2 toVec = v-Cells_Pos2D[0].xy;
	float dist0 = dot(toVec, toVec);
	toVec = v-Cells_Pos2D[1].xy;
	float dist1 = dot(toVec, toVec);

	float minDist		= min(dist0, dist1);
	float secondMinDist = max(dist0, dist1);
	int cell = minDist == dist0 ? 0 : 1;

	for(int i=2; i<NumCells; ++i)
	{
		float oldMinDist = minDist;
		toVec = v-Cells_Pos2D[i];
		float newDist = dot(toVec, toVec);
		minDist = min(minDist, newDist);
		[flatten] if(minDist < newDist)
		{
		//	float oldSecondMinDist = secondMinDist;
			secondMinDist = min(secondMinDist, newDist);
			//neighborCellPos = secondMinDist == oldSecondMinDist ? neighborCellPos : pos;
		}
		else
		{
			secondMinDist = oldMinDist;
			cell = i;
		}
	}

	// TODO: check asm for efficient lookup(s)

	/*float2 toNeighbor = normalize(neighborCellPos - Cells_Pos2D[cell]);
	float2 toPoint = normalize(v- Cells_Pos2D[cell]);
	float angle = dot(toNeighbor, toPoint);*/

	float4 outColor;
	secondMinDist = sqrt(secondMinDist);
	minDist = sqrt(minDist);

	outColor.rgb = smoothstep(0, 1.0, saturate(secondMinDist-minDist)*10) *0.5* Cells_Color[cell];
	outColor.a = 1;
	return outColor;
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