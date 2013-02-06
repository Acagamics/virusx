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

        public const int attackingMapSize = 128;
        public Texture2D DamageTexture { get { return damageTexture; } }
        private RenderTarget2D damageTexture;

        private byte[] damageDataCache = new byte[attackingMapSize * attackingMapSize * 4];   // 4 byte per pixel

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
            damageTexture = new RenderTarget2D(graphicsDevice, attackingMapSize, attackingMapSize, false, 
                                        SurfaceFormat.Color, DepthFormat.None, 0 , RenderTargetUsage.PlatformContents);

            spriteBatch = new SpriteBatch(graphicsDevice);
        }

        public static Rectangle ComputePixelRect(Vector2 position, float size)
        {
            size *= attackingMapSize;
            int iSize = (int)size;
            return new Rectangle((int)(position.X * attackingMapSize),
                                  (int)(position.Y * attackingMapSize), iSize, iSize);
        }
        public static Rectangle ComputePixelRect_Centred(Vector2 position, float size)
        {
            size *= attackingMapSize;
            int iSize = (int) size;
            float halfSize = size/2;
            return new Rectangle((int)(position.X * attackingMapSize - halfSize),
                                  (int)(position.Y * attackingMapSize - halfSize), iSize, iSize);
        }
      
        public byte GetPlayerDamageAt(int x, int y, int damagingPlayer)
        {
#if DEBUG
            if (x < 0 || y < 0 || x > attackingMapSize - 1 || y > attackingMapSize - 1)
                throw new Exception("invalid index!");
#endif

            return damageDataCache[(x + y * DamageMap.attackingMapSize) * 4 + damagingPlayer];
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

            particleRenderer.Draw(device, new Vector2(-1.0f, 1.0f), Vector2.One*2, players, true);

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
