SamplerState InfoSampler : register(s0);
SamplerState VirusSampler : register(s1);

texture2D VirusTexture : register(t0);
texture2D PositionTexture : register(t1);
texture2D InfoTexture : register(t2);

float4 Color;
float TextureSize;
float MinHealth;
float2 HealthToSizeScale;
float2 RelativeMax;

struct VertexShaderInput
{
  float2 Position			: SV_Position;
  uint  InstanceIndex : SV_InstanceID;
};

struct VertexShaderOutput
{
  float4 Position : SV_Position;
  float2 Texcoord : TEXCOORD0;
};

VertexShaderOutput VS(VertexShaderInput input)
{
  VertexShaderOutput output;

  float instanceIndexF = input.InstanceIndex;

  float2 vertexTexcoord;
  vertexTexcoord.x = fmod(instanceIndexF, (TextureSize-1));
  vertexTexcoord.y = floor(instanceIndexF / (TextureSize-1));
  vertexTexcoord.xy += 0.5f;
  vertexTexcoord.xy /= TextureSize;
  float health = InfoTexture.SampleLevel(InfoSampler, vertexTexcoord, 0.0f).x;

  // health near zero brings this element into clipping space
  if (health > 0)	// dynamic branch? could spare texture lookup - let compiler decide
  {
    health += MinHealth;
    float2 instancePosition = PositionTexture.SampleLevel(InfoSampler, vertexTexcoord, 0.0f).xy;

    // "random" rotation
    float cosRot = cos(instanceIndexF);
    float sinRot = sin(instanceIndexF);
    float2 quadTransl = float2(dot(input.Position, float2(cosRot, -sinRot)),
                               dot(input.Position, float2(sinRot, cosRot)));

    // positioning
    output.Position.xy = (quadTransl * health * HealthToSizeScale + instancePosition / RelativeMax) * float2(2, -2) + float2(-1, 1);
    output.Position.zw = float2(0.5f, 1.0f);
  }
  else
  {
    output.Position = float4(100.0f, 100.0f, 0.0f, 1.0f);
  }

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

  output.Position.xy = input.Position.xy / ScreenSize * float2(2, -2) + float2(-1, 1);
  output.Position.zw = float2(0.5f, 1.0f);

  output.Texcoord = input.Texcoord;
  return output;
}

float4 PS_Virus(VertexShaderOutput input) : COLOR0
{
  float virus = VirusTexture.Sample(VirusSampler, input.Texcoord).r;
  clip(virus - 1.0f / 255.0f);
  return Color * virus;
}

float4 PS_NoFalloff(VertexShaderOutput input) : COLOR0
{
  float2 v = input.Texcoord*2.0f - 1.0f;
  float alpha = dot(v,v) < 1.0f;

  clip(alpha - 1.0f / 255.0f);

  return Color * alpha;
}

technique Virus
{
  pass Pass1
  {
    VertexShader = compile vs_4_0 VS();
    PixelShader = compile ps_4_0 PS_Virus();
  }
}

technique Virus_Spritebatch
{
  pass Pass1
  {
    VertexShader = compile vs_4_0 VS_SpritebatchParticle();
    PixelShader = compile ps_4_0 PS_Virus();
  }
}


technique DamageMap
{
  pass Pass1
  {
    VertexShader = compile vs_4_0 VS();
    PixelShader = compile ps_4_0 PS_NoFalloff();
  }
}
