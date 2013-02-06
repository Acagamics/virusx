float4x4 ProjectionMatrix;

// the texture itself
texture2D ParticleTexture;
// sampler state for our texture
sampler2D ParticleTextureSampler
{
	Texture = ParticleTexture;
};

// Gaussian Blur
	// Kernel Size 12, sigma 
float4 GaussianBlurOffsets[6];
static const float GaussianBlurWeights[6] = 
{ 0.01142106f,
 0.02518326f,
 0.04809282f,
 0.07954466f,
 0.11394740f,
 0.14137100f };

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
    float4 Color = tex2D(ParticleTextureSampler, input.Texcoord);
	[unroll] for(int i = 0; i < 6; i++)
	{
		float4 TexOffseted = input.Texcoord.xyxy + GaussianBlurOffsets[i];
        Color += (tex2D(ParticleTextureSampler, TexOffseted.xy) + tex2D(ParticleTextureSampler, TexOffseted.zw)) * GaussianBlurWeights[i];
	}

    return Color;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
