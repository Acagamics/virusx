#define MAX_NUM_CELLS 16
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
	float2 v = input.Texcoord * RelativeMax;

	// determine first, second and thrid minDist and the color of the minDist

	const float FALLOFF = 32.0f;
	const float FACTOR = -(1.0f/16.0f);
	float cellFactor = 0.0f;
	float cellFactorDx = 0.0f;
	float minDist = 999999;
	float secondMinDist = 999999;
	float3 color;
	[unroll] for(int i=0; i<MAX_NUM_CELLS; ++i)
	{
	//	float2 toVec = v - Cells_Pos2D[i];
	//	float distSq = dot(toVec,toVec);
		float dist = distance(v, Cells_Pos2D[i]) * 2;
		
		cellFactor += exp(-FALLOFF * dist);
		//cellFactor += 1.0/pow(distSq, 10.0);

		[flatten] if(dist < minDist)
		{
			color = Cells_Color[i];
			secondMinDist = minDist;
			minDist = dist;
		}
		else
			secondMinDist = min(dist, secondMinDist);
    }
	//cellFactor = pow(1.0f/cellFactor, 1.0f/16.0f);
	cellFactor = min(1.0, FACTOR * log(cellFactor));
	float cellFactorSq = cellFactor*cellFactor;
	//cellFactor *= cellFactor;


	//cellFactor *= min(1.0f, (secondMinDist - minDist)); // borders


	float hardBorder = smoothstep(0.1f, 0.0f, secondMinDist - minDist);
	float smoothBorder = max(cellFactorSq*cellFactorSq, hardBorder*0.2);

	float inner = saturate((0.8f - max(cellFactor, smoothBorder)) * (1 - hardBorder));
	float4 outColor;
	outColor.rgb = min(1, cellFactor*cellFactor) * 0.8f;
		//(1-cellFactor)*(color+0.4f)*0.8f * (1-hardBorder) + hardBorder*hardBorder*hardBorder * 0.2;//lerp(float3(0.2f,0.2f,0.2f), color + 0.4f, inner) * 0.8f;
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