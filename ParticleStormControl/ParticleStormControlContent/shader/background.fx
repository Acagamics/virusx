#define MAX_NUM_CELLS 19
float2 Cells_Pos2D[MAX_NUM_CELLS];
float3 Cells_Color[MAX_NUM_CELLS];

int NumCells;
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
	float2 v = input.Texcoord * RelativeMax;//(input.Texcoord - 0.5f)*2;

	// determine first, second and thrid minDist and the color of the minDist

	const float FALLOFF = 60.0f;
	const float FACTOR = -(1.0f/32.0f);
	float cellFactor = 0.0f;
	float cellFactorDx = 0.0f;
	float minDist = 999999;
	float3 color;
	for(int i=0; i<NumCells; ++i)
	{
		float2 toVec = v - Cells_Pos2D[i];
		float distSq = dot(toVec,toVec);
		//float dist = distance(v, Cells_Pos2D[i]) * 2;
		
		//cellFactor += exp(-FALLOFF * dist);
		cellFactor += 1.0/pow(distSq, 8.0);

		[flatten] if(distSq < minDist)
		{
			color = Cells_Color[i];
			minDist = distSq;
		}
    }
	cellFactor = pow( 1.0/cellFactor, 1.0/16.0 );
	//cellFactor = FACTOR * log(cellFactor);



	//cellFactor *= min(1.0f, (secondMinDist - minDist)); // borders

	float4 outColor;
	float border = 1-min(1, pow(cellFactor-0.1,2));
	outColor.rgb = 1-cellFactor *2;//border * lerp(float3(1,1,1), color, saturate(0.95-cellFactor*1.5)); //+ max(0, 1 - pow(border * 4, 2)) * color * 0.4;
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