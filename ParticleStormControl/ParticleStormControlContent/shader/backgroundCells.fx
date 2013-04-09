#define MAX_NUM_CELLS 16
float2 Cells_Pos2D[MAX_NUM_CELLS];

float2 PosOffset;
float2 PosScale;
float2 RelativeMax;

texture BackgroundTexture;
sampler2D sampBackground = sampler_state
{	
	Texture = <BackgroundTexture>;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};


texture NoiseTexture;
sampler2D sampNoise = sampler_state
{	
	Texture = <NoiseTexture>;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};


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

float4 Dist_PS(VertexShaderOutput input) : COLOR0
{
	float2 v = input.Texcoord * RelativeMax;

	const float FALLOFF = 95.0f;
	const float FACTOR = -(1.0f/16.0f);
	float cellFactor = 0.0f;

	[unroll] for(int i=0; i<MAX_NUM_CELLS; ++i)
	{
		float dist = distance(v, Cells_Pos2D[i]);
		cellFactor += exp2(-FALLOFF * dist);
    }
	cellFactor = saturate(FACTOR * log(cellFactor));

	float4 outColor;
	outColor = cellFactor;
	outColor.a = cellFactor * 1.2f;
	return outColor;
}

float4 Output_PS(VertexShaderOutput input) : COLOR0
{
	return tex2D(sampBackground, input.Texcoord);
}

technique TOutput
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 Output_PS();
    }
}


technique TDist
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 Dist_PS();
    }
}


// another nice solution
/*

*/

/*
{
	float2 v = input.Texcoord * RelativeMax;

	float minDist = 99999;
	float secondMinDist = 99999;

	[unroll] for(int i=0; i<MAX_NUM_CELLS; ++i)
	{
		float2 toPos = v - Cells_Pos2D[i];
		float dist = dot(toPos, toPos);
		[flatten] if(dist < secondMinDist)
		{
			[flatten] if(dist < minDist)
			{
				secondMinDist = minDist;
				minDist = dist;
			}
			else
				secondMinDist = dist;
		}
    }

	float4 outColor;
	outColor = (secondMinDist - minDist) * 5;
	return outColor;
}*/