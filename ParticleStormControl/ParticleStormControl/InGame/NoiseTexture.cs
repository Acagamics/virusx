using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace ParticleStormControl
{
    class NoiseTexture
    {
        static public Texture2D GenerateNoise2D16f(GraphicsDevice device, int width, int height)
        {
            Texture2D noise = new Texture2D(device, width, height, false, SurfaceFormat.HalfVector2);

            HalfSingle[] data = new HalfSingle[width * height * 2];
            for (int i = 0; i < data.Length; ++i)
                data[i] = new HalfSingle((float)Random.NextDouble() * 2.0f - 1.0f);

            noise.SetData<HalfSingle>(data);

            return noise;
        }
    }
}
