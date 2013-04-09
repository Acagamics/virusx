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

        public void Generate(List<Vector2> cellPositions, Vector2 relativeCoordMax)
        {
            this.cellPositions = cellPositions;
            this.relativeCoordMax = relativeCoordMax;
            backgroundShader.Parameters["RelativeMax"].SetValue(relativeCoordMax);

            // TODO: Use Shader & Optimize!
            Color[] colorValues = new Color[backgroundTexture.Width * backgroundTexture.Height];
            float[,] greyvalues = new float[backgroundTexture.Width, backgroundTexture.Height];
            int[,] cellIndex = new int[backgroundTexture.Width, backgroundTexture.Height];


            const float FALLOFF = 95.0f;
            const float FACTOR = -(1.0f / 16.0f);

            Parallel.For(0, backgroundTexture.Height, y => // simple parallalization - just a "brute force" speed up an far from optimal threading!
            {
                for (int x = 0; x < backgroundTexture.Width; ++x)
                {
                    Vector2 v = new Vector2((float)x / (backgroundTexture.Width - 1), (float)y / (backgroundTexture.Height - 1)) * relativeCoordMax;
                    greyvalues[x, y] = FACTOR * (float)Math.Log(cellPositions.Sum(cellPos => { return Math.Pow(2, -FALLOFF * Vector2.Distance(v, cellPos)); }));
                }
            });

            /*
                        int maxIndexX = backgroundTexture.Width - 1;
                        int maxIndexY = backgroundTexture.Height - 1;

                        Parallel.For(0, backgroundTexture.Height, y => // simple parallalization - just a "brute force" speed up an far from optimal threading!
                        {
                            for (int x = 0; x < backgroundTexture.Width; ++x)
                            {
                                Vector2 v = new Vector2((float)x / maxIndexX, (float)y / maxIndexY) * RELATIVE_MAX;
                                float minDist = 9999;
                            //    float secondMinDist = 9999;
                                for (int cell = 0; cell < cellPositions.Count; ++cell)
                                {
                                    float dist = Vector2.DistanceSquared(v, cellPositions[cell]);
                                   // if (dist < secondMinDist)
                                  //  {
                                        if (dist < minDist)
                                        {
                                       //     secondMinDist = minDist;
                                            minDist = dist;
                                            cellIndex[x,y] = cell;
                                        }
                                       // else
                                        //    secondMinDist = dist;
                                    //}
                                }

                          //      greyvalues[x, y] = (secondMinDist - minDist) > 0.01 ? 1 : 0;
                            }
                        });*/

            //   const int KERNEL = 64;

            /*
                        // better: summed area table
                        Parallel.For(0, backgroundTexture.Height, y => // simple parallalization - just a "brute force" speed up an far from optimal threading!
                        {
                            for (int x = 0; x < backgroundTexture.Width; ++x)
                            {
                                int minX = Math.Max(0, x - KERNEL);
                                int maxX = Math.Min(maxIndexX, x + KERNEL);
                                int minY = Math.Max(0, y - KERNEL);
                                int maxY = Math.Min(maxIndexY, y + KERNEL);
                                int numMyCells = -1;
                                int myCell = cellIndex[x, y];
                                for (int neighbourX = minX; neighbourX < maxX; ++neighbourX)
                                {
                                    for (int neighbourY = minY; neighbourY < maxY; ++neighbourY)
                                    {
                                        numMyCells += myCell == cellIndex[neighbourX, neighbourY] ? 1 : 0;
                                    }
                                }

                                greyvalues[x, y] = (float)numMyCells / (KERNEL * KERNEL*4);
                            }
                        });
                        */
            for (int y = 0; y < backgroundTexture.Height; ++y)
            {
                for (int x = 0; x < backgroundTexture.Width; ++x)
                {
                    colorValues[x + y * backgroundTexture.Width] = Color.White * greyvalues[x, y];
                }
            }

            backgroundTexture.SetData<Color>(colorValues);
        }

        /// <summary>
        /// Resizes the background.
        /// Please call once before using!
        /// </summary>
        public void Resize(GraphicsDevice device, Rectangle areaInPixel)
        {
            Resize(device, areaInPixel, this.cellPositions, this.relativeCoordMax);
        }

        /// <summary>
        /// resizing + new cellpositions
        /// </summary>
        public void Resize(GraphicsDevice device, Rectangle areaInPixel, List<Vector2> cellPositions, Vector2 relativeCoordMax)
        {
            // resize background particles
            backgroundParticles.Resize(device.Viewport.Width, device.Viewport.Height, new Point(areaInPixel.Width, areaInPixel.Height), areaInPixel.Location);

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
        }

        public void Draw(GraphicsDevice device, float totalTimeSeconds)
        {
            // background particles
            device.BlendState = BlendState.NonPremultiplied;
            backgroundParticles.Draw(device, totalTimeSeconds);

            // cells
            backgroundShader.Parameters["BackgroundTexture"].SetValue(backgroundTexture);
            device.SetVertexBuffer(quadVertexBuffer);
            backgroundShader.CurrentTechnique.Passes[0].Apply();
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            device.Textures[0] = null;
        }
    }
}
