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

texture Health;
sampler2D sampHealth = sampler_state
{	
	Texture = <Health>;
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



float2 particleAttractionPosition;
float MovementChangeFactor;
float MovementFactor;
float TimeInterval;
float4 DamageFactor;
float NoiseToMovementFactor;

// maximum of the relative cordinates
float2 RelativeCorMax;

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
	float4 health : COLOR2;
};
PixelShaderOutput Process(VertexShaderOutput input)
{
	PixelShaderOutput output;
	output.position = tex2D(sampPositions, input.Texcoord);
	output.movement = tex2D(sampMovements, input.Texcoord);
	output.health = tex2D(sampHealth, input.Texcoord);

	// movement
	float2 aimed = normalize(particleAttractionPosition - output.position.xy);
	output.movement.xy = normalize(lerp(output.movement.xy, aimed, MovementChangeFactor));
	output.movement.xy += tex2D(sampNoise, input.Texcoord + TimeInterval/2).xy * NoiseToMovementFactor; // add noise
    output.position.xy += output.movement.xy * MovementFactor;
	
	// texcoord position!
	output.position.xy /= RelativeCorMax;

	// borders
	output.position.xy -= float2(0.005f, 0.005f); // earlier bounce
	float2 posSgn = sign(output.position.xy);
    float2 posInv = output.position.xy - 0.99f;
    float2 combSigns = posInv < 0 ? posSgn : -posSgn;
    output.position.xy = posInv * combSigns + 0.99f * posSgn;
    output.movement.xy *= combSigns;
	output.position.xy += float2(0.005f, 0.005f);	// undo earlier bounce displacement

	// damage
	float4 damageMapMasked = tex2D(sampDamageMap, output.position.xy) * DamageFactor;
	float damage = dot(damageMapMasked, 1.0f); 
	output.health.x -= damage;


	// game position!
	output.position.xy *= RelativeCorMax;

    return output;
}

struct VertexShaderInput_Spawn
{
    float2 Position : POSITION;
    float2 ParticlePosition : TEXCOORD0;
	float2 Movement : TEXCOORD1;
    float2 DamageSpeed : TEXCOORD2;
};

struct VertexShaderOutput_Spawn
{
    float4 Position : POSITION0;
    float2 ParticlePosition : TEXCOORD0;
	float2 Movement : TEXCOORD1;
    float2 DamageSpeed : TEXCOORD2;
};

VertexShaderOutput_Spawn SpawnVS(VertexShaderInput_Spawn input)
{
    VertexShaderOutput_Spawn output;
	output.Position.xy = input.Position - halfPixelCorrection;	// yep subtract
	output.Position.zw = float2(0.0f, 1.0f);
	output.ParticlePosition = input.ParticlePosition;
	output.Movement = input.Movement;
	output.DamageSpeed = input.DamageSpeed;
    return output;
}

PixelShaderOutput SpawnPS(VertexShaderOutput_Spawn input)
{
	PixelShaderOutput output = (PixelShaderOutput)0;
	output.position.xy = input.ParticlePosition;
	output.movement.xy = input.Movement;
	output.health.xy = input.DamageSpeed;
    return output;
}

technique ProcessTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 ScreenAlignedTriangle();
        PixelShader = compile ps_3_0 Process();
    }
}

technique SpawnTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 SpawnVS();
        PixelShader = compile ps_3_0 SpawnPS();
    }
}

