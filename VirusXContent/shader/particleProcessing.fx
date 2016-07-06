SamplerState PointSampler : register(s0);

texture2D Positions : register(t0);
texture2D Movements : register(t1);
texture2D Health : register(t2);
texture2D DamageMap : register(t3);
texture2D NoiseTexture : register(t4);

float2 particleAttractionPosition;
float MovementChangeFactor;
float MovementFactor;
float TimeInterval;
float4 DamageFactor;
float NoiseToMovementFactor;

float MaxHealth;	// this prevents an advantage @ mutate

float GlobalDamage;

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
  output.Position.xy = input.Position;
  output.Position.zw = float2(0.0f, 1.0f);
  output.Texcoord.xy = input.Position * 0.5f + 0.5f;
  output.Texcoord.y = 1.0f - output.Texcoord.y;
  return output;
}

struct PixelShaderOutput
{
  float2 position : COLOR0;
  float2 movement : COLOR1;
  float health : COLOR2;
};
PixelShaderOutput Process(VertexShaderOutput input)
{
  PixelShaderOutput output;
  output.position = Positions.SampleLevel(PointSampler, input.Texcoord, 0.0f).xy;
  output.movement = Movements.SampleLevel(PointSampler, input.Texcoord, 0.0f).xy;
  output.health = min(MaxHealth, Health.SampleLevel(PointSampler, input.Texcoord, 0.0f).x);
  output.health.x -= GlobalDamage;

  // movement
  float2 aimed = normalize(particleAttractionPosition - output.position.xy);
  output.movement.xy = normalize(lerp(output.movement.xy, aimed, MovementChangeFactor));
  output.movement.xy += NoiseTexture.SampleLevel(PointSampler, input.Texcoord + TimeInterval / 2, 0.0f).xy * NoiseToMovementFactor; // add noise
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
  float4 damageMapMasked = DamageMap.SampleLevel(PointSampler, output.position.xy, 0.0f) * DamageFactor;
  float damage = dot(damageMapMasked, 1.0f);
  output.health.x -= damage;

  // game position!
  output.position.xy *= RelativeCorMax;

  return output;
}

struct VertexShaderInput_Spawn
{
  float2 Position : SV_Position;
  float2 ParticlePosition : TEXCOORD0;
  float2 Movement : TEXCOORD1;
  float Health : TEXCOORD2;
};

struct VertexShaderOutput_Spawn
{
  float4 Position : SV_Position;
  float2 ParticlePosition : TEXCOORD0;
  float2 Movement : TEXCOORD1;
  float Health : TEXCOORD2;
};

VertexShaderOutput_Spawn SpawnVS(VertexShaderInput_Spawn input)
{
  VertexShaderOutput_Spawn output;
  output.Position.xy = input.Position;
  output.Position.zw = float2(0.5f, 1.0f);
  output.ParticlePosition = input.ParticlePosition;
  output.Movement = input.Movement;
  output.Health = input.Health;
  return output;
}

PixelShaderOutput SpawnPS(VertexShaderOutput_Spawn input)
{
  PixelShaderOutput output = (PixelShaderOutput)0;
  output.position.xy = input.ParticlePosition;
  output.movement.xy = input.Movement;
  output.health.x = input.Health;
  return output;
}

technique ProcessTechnique
{
  pass Pass1
  {
    VertexShader = compile vs_4_0 ScreenAlignedTriangle();
    PixelShader = compile ps_4_0 Process();
  }
}

technique SpawnTechnique
{
  pass Pass1
  {
    VertexShader = compile vs_4_0 SpawnVS();
    PixelShader = compile ps_4_0 SpawnPS();
  }
}

