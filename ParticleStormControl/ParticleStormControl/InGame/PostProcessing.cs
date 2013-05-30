using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

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

        private float groundBlurRaiseTo = 0.0f;
        private float groundBlurFactor = 0.0f;
        private const float BLUR_TRANSITION_SPEED = 20.0f;
        private const float PAUSE_BLUR_FACTOR = 5.0f;

       // private const float SPAWN_POINT_DISPLACEMENT_SIZE = 0.04f;
      //  private const float SPAWN_POINT_DISPLACEMENT_STRENGTH = 4.0f;
        
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
            vignettingShader = content.Load<Effect>("shader/postprocess");

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
                vignettingShader.Parameters["InversePixelSize"].SetValue(new Vector2(1.0f / screenTarget.Width, 1.0f / screenTarget.Height));
            }
        }

        public void UpdateVignettingSettings(bool vignettingOn, Vector2 fieldSize_pixel, Vector2 fieldOffset_pixel)
        {
            this.vignettingOn = vignettingOn;

            if (vignettingOn)
            {
                Vector2 posScale = new Vector2(Settings.Instance.ResolutionX, Settings.Instance.ResolutionY) / new Vector2(fieldSize_pixel.X, fieldSize_pixel.Y);
                Vector2 posOffset = -new Vector2(fieldOffset_pixel.X, fieldOffset_pixel.Y) / new Vector2(Settings.Instance.ResolutionX, Settings.Instance.ResolutionY) * posScale;
                vignettingShader.Parameters["Vignetting_PosScale"].SetValue(posScale);
                vignettingShader.Parameters["Vignetting_PosOffset"].SetValue(posOffset);
          //      vignettingShader.Parameters["VignetteScreenRatio"].SetValue((float)fieldSize_pixel.X / fieldSize_pixel.Y);
                vignettingShader.Parameters["VignetteStrength"].SetValue(1.0f);
            }
            else
            {
                vignettingShader.Parameters["Vignetting_PosScale"].SetValue(Vector2.One);
                vignettingShader.Parameters["Vignetting_PosOffset"].SetValue(Vector2.Zero);
           //     vignettingShader.Parameters["VignetteScreenRatio"].SetValue((float)Settings.Instance.ResolutionX / Settings.Instance.ResolutionY);
                vignettingShader.Parameters["VignetteStrength"].SetValue(0.0f);
            }
        }

        public void Resize(Vector2 fieldSize_pixel, Vector2 fieldOffset_pixel)
        {
            UpdateVignettingSettings(vignettingOn, fieldSize_pixel, fieldOffset_pixel);
            CreateRenderTarget(screenTarget.GraphicsDevice);
        }

        /// <summary>
        /// activates a general blur, meant for pause screen
        /// </summary>
        public void ActivatePauseBlur()
        {
            groundBlurRaiseTo = PAUSE_BLUR_FACTOR;
        }

        /// <summary>
        /// deactivates the general blur for pause screen
        /// </summary>
        public void DeactivatePauseBlur()
        {
            groundBlurRaiseTo = 0.0f;
        }

        public void Update(GameTime gameTime, Level level)
        {
            if (groundBlurFactor != groundBlurRaiseTo)
            {
                if (groundBlurFactor < groundBlurRaiseTo)
                {
                    groundBlurFactor += (float)gameTime.ElapsedGameTime.TotalSeconds * BLUR_TRANSITION_SPEED;
                    if (groundBlurFactor > groundBlurRaiseTo)
                        groundBlurFactor = groundBlurRaiseTo;
                }
                else
                {
                    groundBlurFactor -= (float)gameTime.ElapsedGameTime.TotalSeconds * BLUR_TRANSITION_SPEED;
                    if (groundBlurFactor < groundBlurRaiseTo)
                        groundBlurFactor = groundBlurRaiseTo;
                }
                vignettingShader.Parameters["GroundBlur"].SetValue(groundBlurFactor);
            }

            // effect for all spawn points
    /*        int numDisplacements = 0;
            Vector2[] radialDisplacementPositions_TexcoordSpace = new Vector2[4];
            Vector2[] radialDisplacementSizeFade = new Vector2[4];
            foreach (SpawnPoint spawn in level.SpawnPoints)
            {
                if (spawn.ExplosionProgress != 0.0f)
                {
                    radialDisplacementPositions_TexcoordSpace[numDisplacements] = spawn.Position / Level.RELATIVE_MAX;
                    radialDisplacementSizeFade[numDisplacements].X = spawn.ExplosionMaxSize * spawn.ExplosionProgress * SPAWN_POINT_DISPLACEMENT_SIZE;
                    radialDisplacementSizeFade[numDisplacements].X *= radialDisplacementSizeFade[numDisplacements].X;
                    radialDisplacementSizeFade[numDisplacements].Y = (float)System.Math.Sin(spawn.ExplosionProgress * System.Math.PI) * SPAWN_POINT_DISPLACEMENT_STRENGTH;
                    ++numDisplacements;
                    if(numDisplacements == radialDisplacementPositions_TexcoordSpace.Length)
                        break;
                }
            }
            vignettingShader.Parameters["NumRadialDisplacements"].SetValue(numDisplacements);
            if(numDisplacements > 0)
            {
                vignettingShader.Parameters["RadialDisplacementPositions_TexcoordSpace"].SetValue(radialDisplacementPositions_TexcoordSpace);
                vignettingShader.Parameters["RadialDisplacementSizeFade"].SetValue(radialDisplacementSizeFade);
            } */
        }

        /// <summary>
        /// starts drawing onto the Postprocessing target
        /// </summary>
        public void Begin(GraphicsDevice device)
        {
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
            device.SetRenderTarget(null);
            vignettingShader.Parameters["ScreenTexture"].SetValue(screenTarget);
            device.BlendState = BlendState.Opaque;
            vignettingShader.CurrentTechnique.Passes[0].Apply();
            ScreenTriangleRenderer.Instance.DrawScreenAlignedTriangle(device);
        }
    }
}
