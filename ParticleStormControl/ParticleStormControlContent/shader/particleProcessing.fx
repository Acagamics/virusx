// the old direct3d9 accurate pixel mapping dillema >.< http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
// set x to -halfpixelsize.x and y to halfpixelsize.y
float2 halfPixelCorrection;

texture Positions;
sampler2D sampPositions = sampler_state
{	
	Texture = <Positions>;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

texture Movements;
sampler2D sampMovements = sampler_state
{	
	Texture = <Movements>;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

texture Infos;
sampler2D sampInfos = sampler_state
{	
	Texture = <Infos>;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

texture DamageMap;
sampler2D sampDamageMap = sampler_state
{	
	Texture = <DamageMap>;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

texture NoiseTexture;
sampler2D sampNoise = sampler_state
{	
	Texture = <NoiseTexture>;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};



float2 CursorPosition;
float MovementChangeFactor;
float TimeInterval;
float4 DamageFactor;
float NoiseToMovementFactor;

// Spawning
#define MAX_NUM_SPAWNS 15
int NumSpawns;
float4 SpawnsAt_Positions[MAX_NUM_SPAWNS];	// xy texcoord for the spawn, zw  position for the new Particle
float4 SpawnInfos[MAX_NUM_SPAWNS];			// the usual 4 info components
float TexcoordDelta = 0.8f / 128.0f;

struct VertexShaderInput
{
    float2 Position : POSITION;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
};

VertexShaderOutput ScreenAlignedTriangle(VertexShaderInput input)
{
    VertexShaderOutput output;
	output.Position.xy = input.Position + halfPixelCorrection;
	output.Position.zw = float2(0.0f, 1.0f);
	output.Texcoord.xy = input.Position * 0.5f + 0.5f;
	output.Texcoord.y = 1.0f - output.Texcoord.y;
    return output;
}

struct PixelShaderOutput
{
	float4 position : COLOR0;
	float4 movement : COLOR1;
	float4 infos : COLOR2;
};
PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;
	output.position = tex2D(sampPositions, input.Texcoord);
	output.movement = tex2D(sampMovements, input.Texcoord);
	output.infos = tex2D(sampInfos, input.Texcoord);

	// movement
	float2 aimed = normalize(CursorPosition - output.position.xy);
	output.movement.xy = normalize(lerp(output.movement.xy, aimed, MovementChangeFactor / output.infos.y));
	output.movement.xy += tex2D(sampNoise, input.Texcoord + TimeInterval/2).xy * NoiseToMovementFactor * output.infos.y; // add noise
    output.position.xy += output.movement.xy * (output.infos.y * TimeInterval);
	
	// borders
	float2 posSgn = sign(output.position.xy);
    float2 posInv = output.position.xy - 0.999f;
    float2 posInvSgn = -sign(posInv);
    float2 combSigns = posInvSgn * posSgn;
    output.position.xy = posInv * combSigns + 0.999f * posSgn;
    output.movement.xy *= combSigns;
	
	// damage
	float4 damageMapMasked = tex2D(sampDamageMap, output.position.xy) * DamageFactor;
	float damage = dot(damageMapMasked, 1.0f); 
	output.infos.x -= damage;

	// spawn
	for(int i=0; i<NumSpawns; ++i)
	{
		float2 d = abs(SpawnsAt_Positions[i].xy - input.Texcoord.xy);
		[flatten] if(d.x + d.y < TexcoordDelta)
		{
			output.position.xy = SpawnsAt_Positions[i].zw;
			output.movement.xy = SpawnInfos[i].xy;
			output.infos.xy = SpawnInfos[i].zw;
		}
	}

    return output;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 ScreenAlignedTriangle();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
