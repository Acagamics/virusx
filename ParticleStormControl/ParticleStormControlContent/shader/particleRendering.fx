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


float4 Color;
float TextureSize;
float2 HealthToSizeScale;
float MinHealth;

float2 RelativeMax;

struct VertexShaderInput
{
    float2 Position			: POSITION0;
	float2 Texcoord			: TEXCOORD0;
	float2 InstanceIndex	: TEXCOORD1;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
	float InstanceIndex : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	float4 vertexTexcoord;
	vertexTexcoord.x = fmod(input.InstanceIndex, TextureSize);
	vertexTexcoord.y = floor(input.InstanceIndex / TextureSize);
	vertexTexcoord.xy /= TextureSize;
	vertexTexcoord.zw = 0;
	float health = tex2Dlod(sampInfos, vertexTexcoord).x;
	
	// health near zero brings this element into clipping space
	[flatten]if(health > 0)	// dynamic branch? could spare texture lookup
	{
		health += MinHealth;
		float2 instancePosition = tex2Dlod(sampPositions, vertexTexcoord).xy;	
		output.Position.xy = (input.Position * health * HealthToSizeScale + instancePosition / RelativeMax) * float2(2, -2) + float2(-1, 1);
		output.Position.zw = float2(0.5f, 1.0f);
	}
	else
		output.Position = 100;
	
	output.InstanceIndex = input.InstanceIndex;
	output.Texcoord = input.Texcoord;
    return output;
}

float4 PixelShaderFunction_Falloff(VertexShaderOutput input) : COLOR0
{
	const float rippling = 3.0f;

	float2 v = input.Texcoord*2.0f - 1.0f;

	// compute polar cordinates
	float radius = length(v);
	float angle = atan2(v.x, v.y) + input.InstanceIndex;
	
	// disturb radius
	float disturbedRadius = radius * ((sin(angle*rippling) + sin(angle*rippling*2.0f)) * 0.1f + 1.1f);
	float circle = 1.0f - saturate(lerp(radius, disturbedRadius, 0.8f + sin(input.InstanceIndex)*0.5));
	clip(circle - 0.001f);

	circle = smoothstep(0.0f, 0.3f, circle) - smoothstep(0.25f, 0.4f, circle)*0.5f;
	
    return Color * circle;

//	float alpha = 1.0f - dot(v,v);
//   return Color * alpha;
}

float4 PixelShaderFunction_NoFalloff(VertexShaderOutput input) : COLOR0
{
	float2 v = input.Texcoord*2.0f - 1.0f;
	float alpha = dot(v,v) < 1.0f;
	
	clip(alpha - 1.0f/255.0f);

    return Color * alpha;
}

technique WithFalloff
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction_Falloff();
    }
}

technique WithoutFalloff
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction_NoFalloff();
    }
}
