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

float4 PS_EpsteinBar(VertexShaderOutput input) : COLOR0
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


//	return float4(1,0,1,1);
}

float4 PS_H5N1(VertexShaderOutput input) : COLOR0
{
	const float stickCount = 6.0;

	float2 v = input.Texcoord *2 - 1.0;

	float midDist = dot(v,v);
	float midDistInv = 1.0-midDist;
	float circle = smoothstep(0.5, 1.0, midDistInv);
//	circle = sqrt(circle);
	float angle = atan2(v.x, v.y) + input.InstanceIndex;
	float sticks = smoothstep(0.0, 0.5, sin(angle * stickCount)) * 
				   smoothstep(1.0, 0.4, midDistInv) * midDistInv;
	float virus = (max(circle, sticks) - smoothstep(0.7, 1.0, midDistInv)*0.6) * 1.3f;
	clip(virus - 0.001f);

    return Color * virus;
}

float4 PS_HepatitisB(VertexShaderOutput input) : COLOR0
{
	const float stickCount = 6.0;

	float2 v = input.Texcoord *2 - 1.0;

	float angle = atan2(v.x, v.y) + input.InstanceIndex;
	float sticks = sin(angle * 10.0)*0.5 + 0.5;

	float distSq = dot(v,v);
	float distSqInv = saturate(1.0 - distSq);
	float virus = saturate(distSqInv + sticks*(distSqInv-0.6)) - distSqInv * distSqInv * 0.7;
	
	clip(virus - 0.001f);

    return Color * virus * 1.5f;
}

float4 PS_HIV(VertexShaderOutput input) : COLOR0
{
	const float stickCount = 6.0;

	float2 v = input.Texcoord *2 - 1.0;

	float angle = atan2(v.x, v.y) + input.InstanceIndex;
	float sticks = sin(angle * 10.0)*0.5 + 0.5;

	float distSq = dot(v,v);
	float distSqInv = saturate(1.0 - distSq);
	
	float distSqInvSq = distSqInv*distSqInv;
	float circle = distSqInv - distSqInvSq*3.0;
	float sticksSq = sticks*sticks;
	float plates = saturate(circle * 15.0 * sticksSq);
	float virus = saturate(saturate(plates + sticksSq * distSqInv) + distSqInvSq*1.5) - distSqInvSq*0.6;

	clip(virus - 0.001f);

    return Color * virus;
}

float4 PS_Marburg(VertexShaderOutput input) : COLOR0
{
	const float stickCount = 6.0;

	// random rotation
	float cosRot = cos(input.InstanceIndex);
	float sinRot = sin(input.InstanceIndex);
	float2 vBasic = input.Texcoord *2 - 1.0;
	float2 v = float2(dot(vBasic, float2(cosRot, -sinRot)),
					  dot(vBasic, float2(sinRot,  cosRot)));
	
	// strain
	v += sin(v.y*5.0 + input.InstanceIndex)*0.03;
	v.y *= 1.1;
	
	float distSq = dot(v,v);
	float distSqInv = saturate(1.0 - distSq);
	
	float stick = pow(saturate(1.0-v.x*v.x*8.0), 20.0);
	stick *= distSqInv;

	float virus = stick;
	virus = saturate(smoothstep(virus, 0.0, 0.001)*0.2 - virus)*6.0 + virus * 2.0;

	clip(virus - 0.001f);

    return Color * virus;
}


float4 PS_NoFalloff(VertexShaderOutput input) : COLOR0
{
	float2 v = input.Texcoord*2.0f - 1.0f;
	float alpha = dot(v,v) < 1.0f;
	
	clip(alpha - 1.0f/255.0f);

    return Color * alpha;
}

technique H5N1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS_H5N1();
    }
}

technique EpsteinBar
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS_EpsteinBar();
    }
}

technique HIV
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS_HIV();
    }
}

technique HepatitisB
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS_HepatitisB();
    }
}
technique Marburg
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS_Marburg();
    }
}

technique EpsteinBar_Spritebatch
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VS_SpritebatchParticle();
        PixelShader = compile ps_3_0 PS_EpsteinBar();
    }
}

technique H5N1_Spritebatch
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VS_SpritebatchParticle();
        PixelShader = compile ps_3_0 PS_H5N1();
    }
}

technique HIV_Spritebatch
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VS_SpritebatchParticle();
        PixelShader = compile ps_3_0 PS_HIV();
    }
}

technique HepatitisB_Spritebatch
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VS_SpritebatchParticle();
        PixelShader = compile ps_3_0 PS_HepatitisB();
    }
}

technique Marburg_Spritebatch
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VS_SpritebatchParticle();
        PixelShader = compile ps_3_0 PS_Marburg();
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
