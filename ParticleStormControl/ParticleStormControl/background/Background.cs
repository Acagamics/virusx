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

        private Texture2D cellColorTexture;

        private List<Vector2> cellPositions = new List<Vector2>();
        private Vector2 relativeCoordMax;

        public int NumBackgroundCells { get { return cellPositions.Count;  } }

        public Background(GraphicsDevice device, ContentManager content)
        {
            backgroundParticles = new BackgroundParticles(device, content);
            backgroundShader = content.Load<Effect>("shader/backgroundCells");

            quadVertexBuffer = new VertexBuffer(device, ScreenTriangleRenderer.ScreenAlignedTriangleVertex.VertexDeclaration, 4, BufferUsage.WriteOnly);
            quadVertexBuffer.SetData(new Vector2[4] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) });
        }

        public void Generate(GraphicsDevice device, List<Vector2> cellPositions, Vector2 relativeMax)
        {
            this.cellPositions = cellPositions;
            this.relativeCoordMax = relativeMax;

            if (cellPositions.Count == 0)
                return;

            backgroundShader.Parameters["NumCells"].SetValue(cellPositions.Count);
            backgroundShader.Parameters["Cells_Pos2D"].SetValue(cellPositions.ToArray());
            backgroundShader.Parameters["RelativeMax"].SetValue(relativeCoordMax);

            if (cellColorTexture != null)
                cellColorTexture.Dispose();
            cellColorTexture = new Texture2D(device, cellPositions.Count, 1, false, SurfaceFormat.Color);
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
            Generate(device, cellPositions, relativeCoordMax);


            // background shader
            Vector2 posScale = new Vector2(areaInPixel.Width, -areaInPixel.Height) /
                                   new Vector2(device.Viewport.Width, device.Viewport.Height) * 2;
            Vector2 posOffset = new Vector2(areaInPixel.X, -areaInPixel.Y) /
                                   new Vector2(device.Viewport.Width, device.Viewport.Height) * 2 - new Vector2(1, -1);
            backgroundShader.Parameters["PosScale"].SetValue(posScale);
            backgroundShader.Parameters["PosOffset"].SetValue(posOffset);
        }

        public void UpdateColors(Color[] colors)
        {
            System.Diagnostics.Debug.Assert(colors.Length == cellPositions.Count);
            cellColorTexture.SetData(colors);
        }

        public void Draw(GraphicsDevice device, float totalTimeSeconds)
        {
            // background particles
            device.BlendState = BlendState.NonPremultiplied;
            backgroundParticles.Draw(device, totalTimeSeconds);

            // cells
            backgroundShader.CurrentTechnique = backgroundShader.Techniques["TCompute"];
            backgroundShader.Parameters["BackgroundTexture"].SetValue(backgroundTexture);
            backgroundShader.Parameters["CellColorTexture"].SetValue(cellColorTexture);
            device.SetVertexBuffer(quadVertexBuffer);
            backgroundShader.CurrentTechnique.Passes[0].Apply();
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            device.Textures[0] = null;
        }
    }
}
