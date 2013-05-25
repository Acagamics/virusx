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
        private RenderTarget2D screenTarget;

        private VertexBuffer vignettingQuadVertexBuffer;
        private Effect vignettingShader;

        private bool vignettingOn = true;

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

            CreateRenderTarget(device);
            Resize(fieldSize_pixel, fieldOffset_pixel);
        }

        private void CreateRenderTarget(GraphicsDevice device)
        {
            if (screenTarget == null || screenTarget.Width != Settings.Instance.ResolutionX || screenTarget.Height != Settings.Instance.ResolutionY)
            {
                if (screenTarget != null)
                    screenTarget.Dispose();
                screenTarget = new RenderTarget2D(device, Settings.Instance.ResolutionX, Settings.Instance.ResolutionY, false, SurfaceFormat.Color, DepthFormat.None);
                vignettingShader.Parameters["HalfPixelCorrection"].SetValue(new Vector2(0.5f / screenTarget.Width, 0.5f / screenTarget.Height));
            }
        }


        public void UpdateVignettingSettings(bool vignettingOn, Vector2 fieldSize_pixel, Vector2 fieldOffset_pixel)
        {
            this.vignettingOn = vignettingOn;

            Vector2 posScale = new Vector2(Settings.Instance.ResolutionX, Settings.Instance.ResolutionY) / new Vector2(fieldSize_pixel.X, fieldSize_pixel.Y);
            Vector2 posOffset = -new Vector2(fieldOffset_pixel.X, fieldOffset_pixel.Y) / new Vector2(Settings.Instance.ResolutionX, Settings.Instance.ResolutionY);
            vignettingShader.Parameters["Vignetting_PosScale"].SetValue(posScale);
            vignettingShader.Parameters["Vignetting_PosOffset"].SetValue(posOffset);
        }

        public void Resize(Vector2 fieldSize_pixel, Vector2 fieldOffset_pixel)
        {
            UpdateVignettingSettings(vignettingOn, fieldSize_pixel, fieldOffset_pixel);
            CreateRenderTarget(screenTarget.GraphicsDevice);
        }

        /// <summary>
        /// starts drawing onto the Postprocessing target
        /// </summary>
        public void Begin(GraphicsDevice device)
        {
            if (!vignettingOn)
                return;
            vignettingShader.Parameters["ScreenTexture"].SetValue((Texture2D)null);
            device.SetRenderTarget(screenTarget);
            device.Clear(Color.Black);
        }

        /// <summary>
        /// ends drawing to the postpro target
        /// will resolve the effect to the backbuffer
        /// </summary>
        public void EndAndApply(GraphicsDevice device)
        {
            if (!vignettingOn)
                return;

            device.SetRenderTarget(null);
            vignettingShader.Parameters["ScreenTexture"].SetValue(screenTarget);
            device.BlendState = BlendState.Opaque;
            vignettingShader.CurrentTechnique.Passes[0].Apply();
            ScreenTriangleRenderer.Instance.DrawScreenAlignedTriangle(device);
        }
    }
}
