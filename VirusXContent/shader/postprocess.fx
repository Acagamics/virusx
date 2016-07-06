texture ScreenTexture;
sampler2D sampScreen = sampler_state
{	
	Texture = <ScreenTexture>;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = POINT;
};

float2 InversePixelSize;

/*
// radial displacements
#define MAX_NUM_RADIAL_DISPLACEMENTS 4
float2 RadialDisplacementPositions_TexcoordSpace[MAX_NUM_RADIAL_DISPLACEMENTS];
float2 RadialDisplacementSizeFade[MAX_NUM_RADIAL_DISPLACEMENTS];	// x: size, y: fade
int NumRadialDisplacements;
const float DisplacementRippleWidth = 0.005f;
*/

// vignetting
float VignetteStrength;
//float VignetteScreenRatio;
float2 Vignetting_PosOffset;
float2 Vignetting_PosScale;

// blurr
float GroundBlur;
#define NUM_POISSON_SAMPLES 12
static const float VignettBlurFactor = 9.0f;
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
    return output;
}

float cubicPulse(float c, float w, float x)
{
    x = abs(x - c);
    if( x>w ) return 0.0f;
    x /= w;
    return 1.0f - x*x*(3.0f-2.0f*x);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// scale texcoord for vignett area
	float2 vignettCord = input.Texcoord * Vignetting_PosScale + Vignetting_PosOffset;
	[flatten] if(all(vignettCord != saturate(vignettCord)))
		discard;

	float blur = GroundBlur;

	// radial displacement
/*	for(int i=0; i<NumRadialDisplacements; ++i)
	{
		float2 toDisplacement = RadialDisplacementPositions_TexcoordSpace[i] - vignettCord;
		toDisplacement.y *= VignetteScreenRatio;
		float dispDistSq = dot(toDisplacement,toDisplacement);
		float impulseStrength = cubicPulse(RadialDisplacementSizeFade[i].x, DisplacementRippleWidth, dispDistSq) * RadialDisplacementSizeFade[i].y;
		blur += impulseStrength;
	}*/

	// compute vignette
	float2 v = vignettCord * 2 - 1;
	v = pow(v, 10);
	float vignettFactor = sqrt(max(0, dot(v,v)*0.5)) * VignetteStrength;

	// blur
	blur += vignettFactor * VignettBlurFactor;
	float3 color = float3(0.0f,0.0f,0.0f);
	[branch] if(blur > 0.001)
	{
		float2 textureOffsetFactor = blur * InversePixelSize;
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
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}