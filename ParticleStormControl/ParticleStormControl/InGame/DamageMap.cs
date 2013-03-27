using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ParticleStormControl
{
    public class DamageMap
    {
        // attacking
        // map is currently rgb 8bit because  a) 32bit min and b) single-float format does not support alpha blending

        public static readonly int attackingMapSizeY = 256;
        public static readonly int attackingMapSizeX = (int)(Level.RELATIVECOR_ASPECT_RATIO * attackingMapSizeY + 0.5f);

        public Texture2D DamageTexture { get { return damageTexture; } }
        private RenderTarget2D damageTexture;

        private byte[] damageDataCache = new byte[attackingMapSizeX * attackingMapSizeY * 4];   // 4 byte per pixel

        private SpriteBatch spriteBatch;
        private Color clearColor = new Color(0,0,0,0);

        private BlendState damageAdditive = new BlendState()
                                                {
                                                    AlphaBlendFunction = BlendFunction.Add,
                                                    AlphaDestinationBlend = Blend.One,
                                                    AlphaSourceBlend = Blend.One,
                                                    BlendFactor = Color.Black,
                                                    ColorBlendFunction = BlendFunction.Add,
                                                    ColorDestinationBlend = Blend.One,
                                                    ColorSourceBlend = Blend.One
                                                };

        public void LoadContent(GraphicsDevice graphicsDevice)
        {
            damageTexture = new RenderTarget2D(graphicsDevice, attackingMapSizeX, attackingMapSizeY, false, 
                                        SurfaceFormat.Color, DepthFormat.None, 0 , RenderTargetUsage.PreserveContents);

            spriteBatch = new SpriteBatch(graphicsDevice);
        }

        /// <summary>
        /// computes pixelrect on damage map from relative cordinates
        /// </summary>
        /// <param name="relativePosition">position in relative game cor</param>
        /// <param name="uniformSize">uniform size in relative game cord</param>
        public static Rectangle ComputePixelRect(Vector2 relativePosition, float uniformSize)
        {
            return ComputePixelRect(relativePosition, new Vector2(uniformSize / Level.RELATIVECOR_ASPECT_RATIO, uniformSize));
        }

        /// <summary>
        /// computes pixelrect on damage map from relative cordinates
        /// </summary>
        /// <param name="relativePosition">position in relative game cor</param>
        /// <param name="size">size in relative game cord</param>
        public static Rectangle ComputePixelRect(Vector2 relativePosition, Vector2 size)
        {
            int rectSizeX = (int)(size.X * attackingMapSizeX + 0.5f);
            int rectSizeY = (int)(size.Y * attackingMapSizeY + 0.5f);
            int rectx = (int)(relativePosition.X / Level.RELATIVE_MAX.X * attackingMapSizeX);
            int recty = (int)(relativePosition.Y / Level.RELATIVE_MAX.Y * attackingMapSizeY);

            return new Rectangle(rectx, recty, rectSizeX, rectSizeY);
        }

        /// <summary>
        /// computes centered pixelrect on damage map from relative cordinates
        /// </summary>
        /// <param name="relativePosition">position in relative game cor</param>
        /// <param name="uniformSize">uniform size in relative game cord</param>
        public static Rectangle ComputePixelRect_Centred(Vector2 relativePosition, float uniformSize)
        {
            return ComputePixelRect_Centred(relativePosition, new Vector2(uniformSize / Level.RELATIVECOR_ASPECT_RATIO, uniformSize));
        }

        /// <summary>
        /// computes centered pixelrect on damage map from relative cordinates
        /// </summary>
        /// <param name="relativePosition">position in relative game cor</param>
        /// <param name="size">size in relative game cord</param>
        public static Rectangle ComputePixelRect_Centred(Vector2 position, Vector2 size)
        {
            int rectSizeX = (int)(size.X * attackingMapSizeX + 0.5f);
            int halfSizeX = rectSizeX / 2;
            int rectSizeY = (int)(size.Y * attackingMapSizeY + 0.5f);
            int halfSizeY = rectSizeY / 2;

            int rectx = (int)(position.X / Level.RELATIVE_MAX.X * attackingMapSizeX);
            int recty = (int)(position.Y / Level.RELATIVE_MAX.Y * attackingMapSizeY);

            return new Rectangle(rectx - halfSizeX, recty - halfSizeY, rectSizeX, rectSizeY);
        }
      
        public byte GetPlayerDamageAt(int x, int y, int damagingPlayer)
        {
#if DEBUG
            if (x < 0 || y < 0 || x > attackingMapSizeX - 1 || y > attackingMapSizeY - 1)
                throw new Exception("invalid index!");
#endif

            return damageDataCache[(x + y * DamageMap.attackingMapSizeX) * 4 + damagingPlayer];
        }

        public void UpdateCPUData()
        {
            damageTexture.GetData(damageDataCache);
        }

        public void UpdateGPU_Particles(GraphicsDevice device, ParticleRenderer particleRenderer, Player[] players)
        {
            device.SetRenderTarget(damageTexture);
            device.Clear(ClearOptions.Target, clearColor, 0, 0);
            device.BlendState = damageAdditive;

            particleRenderer.Draw(device, players, true);

            device.BlendState = BlendState.Opaque;
            device.SetRenderTarget(null);
        }

        public void UpdateGPU_Map(GraphicsDevice device, Level level)
        {
            device.SetRenderTarget(damageTexture);
            device.Clear(ClearOptions.Target, clearColor, 0, 0);
            device.BlendState = damageAdditive;

            spriteBatch.Begin(SpriteSortMode.Deferred, damageAdditive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone);

            level.DrawToDamageMap(spriteBatch);

            spriteBatch.End();
        }
    }
}
