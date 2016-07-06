#define MAX_NUM_CELLS 32
float2 Cells_Pos2D[MAX_NUM_CELLS];	// todo compress
float3 Cells_Color[MAX_NUM_CELLS];

int NumCells;

float2 PosOffset;
float2 PosScale;
float2 RelativeMax;

texture BackgroundTexture;
sampler2D sampBackground = sampler_state
{
  Texture = <BackgroundTexture>;
  MagFilter = POINT;
  MinFilter = POINT;
  Mipfilter = POINT;
};

texture CellColorTexture;
sampler2D sampCellColor = sampler_state
{
  Texture = <CellColorTexture>;
  MagFilter = POINT;
  MinFilter = POINT;
  Mipfilter = POINT;
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
  output.Position.xy = input.Position * PosScale + PosOffset;
  output.Position.zw = float2(0, 1);
  output.Texcoord = input.Position * RelativeMax;
  return output;
}

float cubicPulse(float peak, float width, float x)
{
  // http://www.iquilezles.org/www/articles/functions/functions.htm
  x = abs(x - peak);
  if (x > width) return 0.0f;
  x /= width;
  return 1.0f - x * x * (3.0f - 2.0f * x);
}

float4 ComputeBackground_PS(VertexShaderOutput input) : COLOR0
{
  float2 v = input.Texcoord;

  const float FALLOFF = 95.0f;

  float maxComp = -99999;
  float worley = 0.0f;
  float cellColorTexcoord = 0;
  [loop]for (int i = 0; i < NumCells; ++i)
  {
    float dist = distance(v, Cells_Pos2D[i]);
    float comp = exp2(-FALLOFF * dist);
    worley += comp;
    [flatten]if (maxComp < comp)
    {
      cellColorTexcoord = i;
      maxComp = comp;
    }
  }
  cellColorTexcoord /= MAX_NUM_CELLS - 1 + 0.5f / MAX_NUM_CELLS;

  float worleySecond = worley - maxComp;
  float value = min(log(worley / worleySecond), 10.0f);	// loga - logb

  float cellFactor = (value - 0.8f) * 0.3f -
            cubicPulse(2.5f, 1.0f, value) * 0.5f +
            cubicPulse(3.0f, 0.5f, value) * 0.3f +
            cubicPulse(4.0f, 3.0f, value) * 0.4f;

  float4 outColor;
  outColor.r = cellFactor;
  outColor.g = 0.95f - saturate(value*0.03f);
  outColor.b = cellColorTexcoord;
  outColor.a = 1.0f;
  return outColor;
}

float4 Output_PS(VertexShaderOutput input) : COLOR0
{
  float3 backgroundInfo = tex2D(sampBackground, input.Texcoord).rgb;
  float3 cellColor = tex2D(sampCellColor, float2(backgroundInfo.b, 0.5f)).rgb;

  return float4(backgroundInfo.r * cellColor, backgroundInfo.g);
}

technique TOutput
{
  pass Pass1
  {
    VertexShader = compile vs_4_0 VertexShaderFunction();
    PixelShader = compile ps_4_0 Output_PS();
  }
}


technique TCompute
{
  pass Pass1
  {
    VertexShader = compile vs_4_0 VertexShaderFunction();
    PixelShader = compile ps_4_0 ComputeBackground_PS();
  }
}