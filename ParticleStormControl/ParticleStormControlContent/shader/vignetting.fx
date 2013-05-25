texture ScreenTexture;
sampler2D sampScreen = sampler_state
{	
	Texture = <ScreenTexture>;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = POINT;
};

float2 Vignetting_PosOffset;
float2 Vignetting_PosScale;
float2 HalfPixelCorrection;
float2 InversePixelSize;

#define NUM_POISSON_SAMPLES 12
static const float BlurFactor = 9.0f;
static const float2 PoissonDisk[NUM_POISSON_SAMPLES] =
{
        float2(-0.326212f, -0.40581f),
        float2(-0.840144f, -0.07358f),
        float2(-0.695914f, 0.457137f),
        float2(-0.203345f, 0.620716f),
        float2(0.96234f, -0.194983f),
        float2(0.473434f, -0.480026f),
        float2(0.519456f, 0.767022f),
        float2(0.185461f, -0.893124f),
        float2(0.507431f, 0.064425f),
        float2(0.89642f, 0.412458f),
        float2(-0.32194f, -0.932615f),
        float2(-0.791559f, -0.59771f)
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
	output.Position.xy = input.Position;
	output.Position.zw = float2(0,1);
	output.Texcoord.xy = input.Position * 0.5f + 0.5f;
	output.Texcoord.y = 1.0f - output.Texcoord.y;
	output.Texcoord += HalfPixelCorrection;
    return output;
}


float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 vignettCord = input.Texcoord * Vignetting_PosScale + Vignetting_PosOffset;
	[flatten] if(all(vignettCord != saturate(vignettCord)))
		discard;

	float2 v = vignettCord * 2 - 1;
	v = pow(v, 10);
	float vignettFactor = sqrt(max(0, dot(v,v)*0.5));
	
	float3 color = float3(0.0f,0.0f,0.0f);
	[branch] if(vignettFactor > 0.01)
	{
		float textureOffsetFactor = (vignettFactor * BlurFactor) * InversePixelSize;
		[unroll]for(int i=0; i<NUM_POISSON_SAMPLES; ++i)
		{
			color += tex2Dlod(sampScreen, float4(input.Texcoord + PoissonDisk[i] * textureOffsetFactor, 0.0f, 0.0f)).rgb;
		}
		color /= NUM_POISSON_SAMPLES;
	}
	else
	{
		color = tex2Dlod(sampScreen, float4(input.Texcoord, 0.0f, 0.0f)).rgb;
	}

	return float4(color * (1.0f - vignettFactor), 1.0f); // float4(vignettFactor,vignettFactor,vignettFactor,vignettFactor);
}

technique Normal
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}