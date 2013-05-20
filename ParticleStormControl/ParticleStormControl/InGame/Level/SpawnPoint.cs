using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
using VirusX;
using Microsoft.Xna.Framework.Content;

namespace VirusX
{
    class SpawnPoint : CapturableObject
    {
        public float SpawnSize { get; private set; }
        public float SpawnTimeAccum { get; set; }

        #region explosion
        /// <summary>
        /// max explosion size
        /// </summary>
        public float ExplosionMaxSize { get; private set;}

        /// <summary>
        ///  current explosionsize
        /// </summary>
        private float currentExplosionSize;
        private float currentExplosionAlpha;
        private float explosionRotation;
        private const int explosionDamage = 8;//10;
        private const float duration = 1.0f;
        
        private Stopwatch explosionTimer = new Stopwatch();

        #endregion

        /// <summary>
        /// damage wich uncaptured points give
        /// </summary>
        public readonly Color capturingDamage = new Color(10, 10, 10, 10);
        private const float capturingDamageSize = 0.03f;


        /// <summary>
        /// timer for glow appearing
        /// </summary>
        public Stopwatch glowtimer = new Stopwatch();

        // textures
        private Texture2D glowTexture;
        private Texture2D explosionTexture;
        private Texture2D nucleusTexture_inner;
        private Texture2D nucleusTexture_outer;

        private readonly float randomAngle;

        public SpawnPoint(Vector2 PositionIn, float spawnSize, int startposession, ContentManager content)
            : base(PositionIn, startposession, 1.0f / spawnSize, -1.0f, 6)
        {
            this.glowTexture = content.Load<Texture2D>("glow");
            this.nucleusTexture_inner = content.Load<Texture2D>("nucleus_inner");
            this.nucleusTexture_outer = content.Load<Texture2D>("nucleus_outer");
            this.explosionTexture = content.Load<Texture2D>("capture_glow");
            this.SpawnSize = spawnSize;

            ExplosionMaxSize = 0.75f;

            randomAngle = (float)Random.NextDouble(Math.PI * 2);

            if(startposession != -1)
                glowtimer.Start();

            Size = ((spawnSize - 100.0f)/ 900.0f) * (0.05f) + 0.03f;

            // Fun mode modifications
            if (Settings.Instance.GameMode == InGame.GameMode.FUN)
            {
                this.damageFactor *= 3f;
                this.SpawnSize *= 2f;
                ExplosionMaxSize += .55f;
            }

        }

        protected override void OnPossessingChanged()
        {
            if(PossessingPlayer != -1)
            {
                AudioManager.Instance.PlaySoundeffect("capture");
                glowtimer.Start();
                explosionTimer.Start();
                explosionRotation = (float)(Random.NextDouble() * MathHelper.TwoPi);
            }
            SpawnTimeAccum = 0.0f;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (explosionTimer.IsRunning)
            {
                float effectseconds = (float) explosionTimer.Elapsed.TotalSeconds;
                if (effectseconds > duration)
                {
                    explosionTimer.Reset();
                }
                else
                {
                    float scaling = MathHelper.Clamp((float) Math.Log(effectseconds*16 + 1.0f)/3, 0.0f, 1.0f);
                    currentExplosionSize = ExplosionMaxSize*scaling;
                    currentExplosionAlpha = 1.0f - effectseconds/duration;
                }
            }
        }

        public override void Draw_AlphaBlended(SpriteBatch spriteBatch, Level level, GameTime gameTime)
        {
            // main
            Color color = ComputeColor();
            const float PULSING = 0.01f;
         //   spriteBatch.Draw(outerTexture, rect, null, color, totalTimeSeconds, new Vector2(outerTexture.Width * 0.5f, outerTexture.Height * 0.5f), SpriteEffects.None, 0.8f);
            float innerSize = Size + (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 1.7 + randomAngle) * PULSING - PULSING;
            spriteBatch.Draw(nucleusTexture_inner, level.ComputePixelPosition(Position),
                             null, color, (float)gameTime.TotalGameTime.TotalSeconds + randomAngle,
                             new Vector2(nucleusTexture_inner.Width * 0.5f, nucleusTexture_inner.Height * 0.5f),
                             level.ComputeTextureScale(innerSize, nucleusTexture_inner.Width), SpriteEffects.None, 0.7f);
            spriteBatch.Draw(nucleusTexture_outer, level.ComputePixelPosition(Position), null, color, 0,
                new Vector2(nucleusTexture_outer.Width * 0.5f, nucleusTexture_outer.Height * 0.5f),
                            level.ComputeTextureScale(Size, nucleusTexture_inner.Width), SpriteEffects.None, 0.6f);

            // explosion
            if (explosionTimer.IsRunning && PossessingPlayer != -1)
            {
                Color explosionColor = Settings.Instance.GetPlayerColor(PossessingPlayer) * currentExplosionAlpha;   // using premultiplied values, the whole colore has to be multiplied for alphablending
                spriteBatch.Draw(explosionTexture, level.ComputePixelPosition(Position), null, explosionColor, explosionRotation,
                                        new Vector2(explosionTexture.Width / 2, explosionTexture.Height / 2), 
                                        level.ComputeTextureScale(currentExplosionSize, explosionTexture.Width), SpriteEffects.None, 0.1f);
            }
        }

        public override void DrawToDamageMap(SpriteBatch spriteBatch)
        {
            if (explosionTimer.IsRunning && PossessingPlayer != -1)
            {
                
                Color damage = VirusSwarm.GetDamageMapDrawColor(PossessingPlayer) * explosionDamage * currentExplosionAlpha;
                spriteBatch.Draw(explosionTexture, DamageMap.ComputePixelRect(Position, currentExplosionSize), null, damage, explosionRotation,
                                     new Vector2(explosionTexture.Width / 2, explosionTexture.Height / 2), SpriteEffects.None, 1.0f);
            }

            // uncaptured? kills people!
            if (PossessingPlayer == -1)
            {
                Color modifiedCapturingDamage = capturingDamage;
                if (Settings.Instance.GameMode == InGame.GameMode.FUN)
                {
                    modifiedCapturingDamage.A /= 2;
                    modifiedCapturingDamage.B /= 2;
                    modifiedCapturingDamage.R /= 2;
                    modifiedCapturingDamage.G /= 2;
                }

                spriteBatch.Draw(glowTexture, DamageMap.ComputePixelRect_Centred(Position, capturingDamageSize), capturingDamage);
            }
        }
    }
}
