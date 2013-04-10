using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ParticleStormControl
{
    class Background
    {
        private BackgroundParticles backgroundParticles;

        private VertexBuffer quadVertexBuffer;
        private Texture2D backgroundTexture;
        private Effect backgroundShader;

        private List<Vector2> cellPositions = new List<Vector2>();
        private Vector2 relativeCoordMax;

        public Background(GraphicsDevice device, ContentManager content)
        {
            backgroundParticles = new BackgroundParticles(device, content);
            backgroundShader = content.Load<Effect>("shader/backgroundCells");

            quadVertexBuffer = new VertexBuffer(device, ScreenTriangleRenderer.ScreenAlignedTriangleVertex.VertexDeclaration, 4, BufferUsage.WriteOnly);
            quadVertexBuffer.SetData(new Vector2[4] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) });
        }



        private float cubicPulse(float peak, float width, float x)
        {
            // http://www.iquilezles.org/www/articles/functions/functions.htm
            x = Math.Abs(x - peak);
            if (x > width) return 0.0f;
            x /= width;
            return 1.0f - x * x * (3.0f - 2.0f * x);
        }

        public void Generate(List<Vector2> cellPositions, Vector2 relativeMax)
        {
            this.cellPositions = cellPositions;
            this.relativeCoordMax = relativeMax;

            if (cellPositions.Count == 0)
                return;


            backgroundShader.Parameters["NumCells"].SetValue(cellPositions.Count);
            backgroundShader.Parameters["Cells_Pos2D"].SetValue(cellPositions.ToArray());
/*
            // TODO: Use Shader & Optimize!
            Color[] colorValues = new Color[backgroundTexture.Width * backgroundTexture.Height];
            float[,] greyvalues = new float[backgroundTexture.Width, backgroundTexture.Height];
            int[,] cellIndex = new int[backgroundTexture.Width, backgroundTexture.Height];


            const float FALLOFF = 95.0f;

            Parallel.For(0, backgroundTexture.Height, y => // simple parallalization - just a "brute force" speed up an far from optimal threading!
            {
                for (int x = 0; x < backgroundTexture.Width; ++x)
                {
                    Vector2 v = new Vector2((float)x / (backgroundTexture.Width - 1), (float)y / (backgroundTexture.Height - 1)) * relativeCoordMax;

                    float minDist = (float)cellPositions.Min(cellPos => { return Vector2.Distance(v, cellPos); });
                  
                    double worley = cellPositions.Sum(cellPos => { return Math.Pow(2, -FALLOFF * Vector2.Distance(v, cellPos)); });
                    double worleySecond = worley - Math.Pow(2, -FALLOFF * minDist);

                    float value = (float)Math.Log(worley / worleySecond);

                    greyvalues[x, y] = 0.1f + (float)Math.Sqrt(value) * 0.2f + cubicPulse(2, 0.5f, (float)value) * 0.1f + cubicPulse(3.0f, 3.0f, (float)value) * 0.4f;

                    //greyvalues[x, y] = FACTOR * (float)Math.Log(cellPositions.Sum(cellPos => { return Math.Pow(2, -FALLOFF * Vector2.Distance(v, cellPos)); }));
                    //greyvalues[x, y] = (float)cellPositions.Min(cellPos => { return Vector2.DistanceSquared(v, cellPos); }) * 20;
                }
            });

        
            for (int y = 0; y < backgroundTexture.Height; ++y)
            {
                for (int x = 0; x < backgroundTexture.Width; ++x)
                {
                    colorValues[x + y * backgroundTexture.Width] = Color.White * greyvalues[x, y];
                }
            }

            backgroundTexture.SetData<Color>(colorValues);*/
        }

        /// <summary>
        /// Resizes the background.
        /// Please call once before using!
        /// </summary>
        public void Resize(GraphicsDevice device, Rectangle areaInPixel, Vector2 relativeCoordMax)
        {
            Resize(device, areaInPixel, this.cellPositions, this.relativeCoordMax);
        }

        /// <summary>
        /// resizing + new cellpositions
        /// </summary>
        public void Resize(GraphicsDevice device, Rectangle areaInPixel, List<Vector2> cellPositions, Vector2 relativeCoordMax)
        {
            this.relativeCoordMax = relativeCoordMax;

            // resize background particles
            backgroundParticles.Resize(device.Viewport.Width, device.Viewport.Height, new Point(areaInPixel.Width, areaInPixel.Height), areaInPixel.Location, relativeCoordMax);

            // resize background and regenerate
            if (backgroundTexture != null)
                backgroundTexture.Dispose();
            backgroundTexture = new Texture2D(device, areaInPixel.Width, areaInPixel.Height, false, SurfaceFormat.Color);
            Generate(cellPositions, relativeCoordMax);


            // background shader
            Vector2 posScale = new Vector2(areaInPixel.Width, -areaInPixel.Height) /
                                   new Vector2(device.Viewport.Width, device.Viewport.Height) * 2;
            Vector2 posOffset = new Vector2(areaInPixel.X, -areaInPixel.Y) /
                                   new Vector2(device.Viewport.Width, device.Viewport.Height) * 2 - new Vector2(1, -1);
            backgroundShader.Parameters["PosScale"].SetValue(posScale);
            backgroundShader.Parameters["PosOffset"].SetValue(posOffset);
            backgroundShader.Parameters["RelativeMax"].SetValue(relativeCoordMax);
        }

        public void Draw(GraphicsDevice device, float totalTimeSeconds)
        {
            // background particles
            device.BlendState = BlendState.NonPremultiplied;
            backgroundParticles.Draw(device, totalTimeSeconds);

            // cells
            backgroundShader.CurrentTechnique = backgroundShader.Techniques["TCompute"];
            backgroundShader.Parameters["BackgroundTexture"].SetValue(backgroundTexture);
            device.SetVertexBuffer(quadVertexBuffer);
            backgroundShader.CurrentTechnique.Passes[0].Apply();
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            device.Textures[0] = null;
        }
    }
}
