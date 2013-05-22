using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirusX
{
    /// <summary>
    /// postprocessing effect class
    /// </summary>
    class PostProcessing
    {
        private VertexBuffer vignettingQuadVertexBuffer;
        private Effect vignettingShader;
        private readonly static BlendState VignettingBlend = new BlendState
        {
            ColorSourceBlend = Blend.Zero,
            ColorDestinationBlend = Blend.SourceAlpha,
            ColorBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.One
        };

        public PostProcessing(GraphicsDevice device, ContentManager content, Vector2 fieldSize_pixel, Vector2 fieldOffset_pixel)
        {
            vignettingQuadVertexBuffer = new VertexBuffer(device, ScreenTriangleRenderer.ScreenAlignedTriangleVertex.VertexDeclaration, 4, BufferUsage.WriteOnly);
            vignettingQuadVertexBuffer.SetData(new Vector2[4] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) });
            vignettingShader = content.Load<Effect>("shader/vignetting");

            Resize(fieldSize_pixel, fieldOffset_pixel);
        }

        public void Resize(Vector2 fieldSize_pixel, Vector2 fieldOffset_pixel)
        {
            Vector2 posScale = new Vector2(fieldSize_pixel.X, -fieldSize_pixel.Y) /
                               new Vector2(Settings.Instance.ResolutionX, Settings.Instance.ResolutionY) * 2;
            Vector2 posOffset = new Vector2(fieldOffset_pixel.X, -fieldOffset_pixel.Y) /
                                   new Vector2(Settings.Instance.ResolutionX, Settings.Instance.ResolutionY) * 2 - new Vector2(1, -1);

            vignettingShader.Parameters["PosScale"].SetValue(posScale);
            vignettingShader.Parameters["PosOffset"].SetValue(posOffset);
        }

        public void Draw(GraphicsDevice device)
        {
            // vignetting
            device.BlendState = VignettingBlend;
            device.SetVertexBuffer(vignettingQuadVertexBuffer);
            vignettingShader.CurrentTechnique.Passes[0].Apply();
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            device.BlendState = BlendState.Opaque;
        }
    }
}
