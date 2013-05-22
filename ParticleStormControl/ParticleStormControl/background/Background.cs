using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace VirusX
{
    class Background
    {
        private BackgroundParticles backgroundParticles;

        // quad 
        private VertexBuffer quadVertexBuffer;

        // precomputed background texture
        private RenderTarget2D backgroundTexture;
        private Effect backgroundShader;

        // texture with cell colors
        private Texture2D cellColorTexture;

        // last settings
        private List<Vector2> cellPositions = new List<Vector2>();
        private Vector2 relativeCoordMax;
        private Vector2 posScale;
        private Vector2 posOffset;
        private Rectangle areaInPixel;



        public int NumBackgroundCells { get { return cellPositions.Count;  } }

        public Background(GraphicsDevice device, ContentManager content)
        {
            backgroundParticles = new BackgroundParticles(device, content);
            backgroundShader = content.Load<Effect>("shader/backgroundCells");

            quadVertexBuffer = new VertexBuffer(device, ScreenTriangleRenderer.ScreenAlignedTriangleVertex.VertexDeclaration, 4, BufferUsage.WriteOnly);
            quadVertexBuffer.SetData(new Vector2[4] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) });

            cellColorTexture = new Texture2D(device, 32, 1, false, SurfaceFormat.Color);

        //    ParticleStormControl.DeviceLostEvent += () => { Resize(device, areaInPixel, relativeCoordMax); };
        }

        /// <summary>
        /// generate + resize
        /// </summary>
        /// <param name="mapLetterBoxing">true if the background uses the level's letter box</param>
        public void Generate(GraphicsDevice device, Rectangle areaInPixel, List<Vector2> cellPositions, Vector2 relativeCoordMax)
        {
            this.relativeCoordMax = relativeCoordMax;
            this.areaInPixel = areaInPixel;

            // resize background particles
            backgroundParticles.Resize(Settings.Instance.ResolutionX, Settings.Instance.ResolutionY, new Point(areaInPixel.Width, areaInPixel.Height), areaInPixel.Location, relativeCoordMax);

            // resize background texture if necessary
            backgroundShader.Parameters["BackgroundTexture"].SetValue((Texture2D)null);
            if (backgroundTexture == null || backgroundTexture.IsContentLost || 
                backgroundTexture.Width != areaInPixel.Width || backgroundTexture.Height != areaInPixel.Height)
            {
                if (backgroundTexture != null)
                    backgroundTexture.Dispose();
                backgroundTexture = new RenderTarget2D(device, areaInPixel.Width, areaInPixel.Height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            }

            // resize background and regenerate
            Generate(device, cellPositions, relativeCoordMax);

            // background shader
            posScale = new Vector2(areaInPixel.Width, -areaInPixel.Height) /
                                   new Vector2(Settings.Instance.ResolutionX, Settings.Instance.ResolutionY) * 2.0f;
            posOffset = new Vector2(areaInPixel.X, -areaInPixel.Y) /
                                   new Vector2(Settings.Instance.ResolutionX, Settings.Instance.ResolutionY) * 2.0f - new Vector2(1, -1);
        }

        public void Generate(GraphicsDevice device, List<Vector2> cellPositions, Vector2 relativeMax)
        {
            this.cellPositions = cellPositions;
            this.relativeCoordMax = relativeMax;

            if (cellPositions.Count == 0)
                return;

            // new cellcolors
            backgroundShader.Parameters["CellColorTexture"].SetValue((Texture2D)null);

            // new settings
            backgroundShader.Parameters["NumCells"].SetValue(cellPositions.Count);
            backgroundShader.Parameters["Cells_Pos2D"].SetValue(cellPositions.ToArray());
            backgroundShader.Parameters["RelativeMax"].SetValue(relativeCoordMax);
            backgroundShader.Parameters["PosScale"].SetValue(new Vector2(2.0f, -2.0f));
            backgroundShader.Parameters["PosOffset"].SetValue(new Vector2(-1.0f, 1.0f));
            backgroundShader.CurrentTechnique = backgroundShader.Techniques["TCompute"];

            // precompute background
            device.SetRenderTarget(backgroundTexture);
            device.SetVertexBuffer(quadVertexBuffer);
            backgroundShader.CurrentTechnique.Passes[0].Apply();
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

            // setup for normal rendering
            device.SetRenderTarget(null);
            backgroundShader.CurrentTechnique = backgroundShader.Techniques["TOutput"];

    //         using (var file = new System.IO.FileStream("background.png", System.IO.FileMode.Create))
    //            backgroundTexture.SaveAsPng(file, backgroundTexture.Width, backgroundTexture.Height);
        }

        /// <summary>
        /// Resizes the background.
        /// Please call once before using!
        /// </summary>
        public void Resize(GraphicsDevice device, Rectangle areaInPixel)
        {
            Generate(device, areaInPixel, this.cellPositions, relativeCoordMax);
        }
        public void Resize(GraphicsDevice device, Rectangle areaInPixel, Vector2 relativeCoordMax)
        {
            Generate(device, areaInPixel, this.cellPositions, relativeCoordMax);
        }

        public void UpdateColors(Color[] colors)
        {
            cellColorTexture.GraphicsDevice.Textures[0] = null;
            cellColorTexture.GraphicsDevice.Textures[1] = null;
            System.Diagnostics.Debug.Assert(colors.Length == cellPositions.Count);
            if(colors.Length < cellColorTexture.Width)
                cellColorTexture.SetData(colors.Concat(Enumerable.Repeat(Color.Black, cellColorTexture.Width - colors.Length)).ToArray());
        }

        public void Draw(GraphicsDevice device, float totalTimeSeconds)
        {
            if (backgroundTexture.IsContentLost)
                Resize(device, areaInPixel, relativeCoordMax);

            // background particles
            device.BlendState = BlendState.NonPremultiplied;
            backgroundParticles.Draw(device, totalTimeSeconds);

            // cells
            backgroundShader.Parameters["PosScale"].SetValue(posScale);
            backgroundShader.Parameters["PosOffset"].SetValue(posOffset);
            backgroundShader.Parameters["RelativeMax"].SetValue(Vector2.One);
            backgroundShader.Parameters["BackgroundTexture"].SetValue(backgroundTexture);
            backgroundShader.Parameters["CellColorTexture"].SetValue(cellColorTexture);
            backgroundShader.Parameters["HalfPixelCorrection"].SetValue(new Vector2(0.5f / backgroundTexture.Width, 0.5f / backgroundTexture.Height));
            
            device.SetVertexBuffer(quadVertexBuffer);
            backgroundShader.CurrentTechnique.Passes[0].Apply();
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

            // just to be safe - this target could get lost!
            backgroundShader.Parameters["BackgroundTexture"].SetValue((Texture2D)null);
        }
    }
}
