texture PositionTexture;
sampler2D sampPositions = sampler_state
{	
	Texture = <PositionTexture>;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};
texture InfoTexture;
sampler2D sampInfos = sampler_state
{	
	Texture = <InfoTexture>;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

texture VirusTexture;
sampler2D sampVirus = sampler_state
{	
	Texture = <VirusTexture>;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};



float4 Color;
float TextureSize;
float2 HealthToSizeScale;
float MinHealth;

float2 RelativeMax;

struct VertexShaderInput
{
    float2 Position			: POSITION0;
	float  InstanceIndex	: TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
	float InstanceIndex : TEXCOORD1;
};

VertexShaderOutput VS(VertexShaderInput input)
{
    VertexShaderOutput output;

	float4 vertexTexcoord;
	vertexTexcoord.x = fmod(input.InstanceIndex, TextureSize);
	vertexTexcoord.y = floor(input.InstanceIndex / TextureSize);
	vertexTexcoord.xy += 0.5f;
	vertexTexcoord.xy /= TextureSize;
	vertexTexcoord.zw = 0;
	float health = tex2Dlod(sampInfos, vertexTexcoord).x;
	
	// health near zero brings this element into clipping space
	if(health > 0)	// dynamic branch? could spare texture lookup - let compiler decide
	{
		health += MinHealth;
		float2 instancePosition = tex2Dlod(sampPositions, vertexTexcoord).xy;	
		output.Position.xy = (input.Position * health * HealthToSizeScale + instancePosition / RelativeMax) * float2(2, -2) + float2(-1, 1);
		output.Position.zw = float2(0.5f, 1.0f);
	}
	else
		output.Position.x = 100;
	
	output.InstanceIndex = input.InstanceIndex;
	output.Texcoord = input.Position + float2(0.5f, 0.5f);
    return output;
}

// http://blogs.msdn.com/b/shawnhar/archive/2010/04/05/spritebatch-and-custom-shaders-in-xna-game-studio-4-0.aspx
float2 ScreenSize;
struct SpriteBatchVSInput
{
	float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
	float4 Color    : COLOR0;
};

VertexShaderOutput VS_SpritebatchParticle(SpriteBatchVSInput input)
{
    VertexShaderOutput output;

	output.Position.xy = input.Position / ScreenSize * float2(2, -2) + float2(-1, 1);
	output.Position.zw = float2(0.5f, 1.0f);

	output.InstanceIndex = 0;
	output.Texcoord = input.Texcoord;
    return output;
}

float4 PS_Virus(VertexShaderOutput input) : COLOR0
{
	float virus = tex2D(sampVirus, input.Texcoord).r;
	clip(virus - 1.0f/255.0f);
    return Color * virus;
}

float4 PS_NoFalloff(VertexShaderOutput input) : COLOR0
{
	float2 v = input.Texcoord*2.0f - 1.0f;
	float alpha = dot(v,v) < 1.0f;
	
	clip(alpha - 1.0f/255.0f);

    return Color * alpha;
}

technique Virus
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS_Virus();
    }
}

technique Virus_Spritebatch
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VS_SpritebatchParticle();
        PixelShader = compile ps_3_0 PS_Virus();
    }
}


technique DamageMap
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS_NoFalloff();
    }
}
