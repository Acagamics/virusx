using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
using ParticleStormControl;

namespace ParticleStormControl
{
    public class SpawnPoint : CapturableObject
    {
        public float SpawnSize { get; private set; }
        public float SpawnTimeAccum { get; set; }

        #region explosion
        /// <summary>
        /// max explosion size
        /// </summary>
        public const float explosionMaxSize = 0.3f;

        /// <summary>
        ///  current explosionsize
        /// </summary>
        private float currentExplosionSize;
        private float currentExplosionAlpha;
        private float explosionRotation;
        private const int explosionDamage = 10;
        private const float explosionDuration = 1.0f;
        
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

        // sounds
        private SoundEffect capture;
        private SoundEffect captureExplosion;

        // textures
        private Texture2D glowTexture;
        private Texture2D explosionTexture;
        private Texture2D innerTexture;
        private Texture2D outerTexture;


        private readonly float glowSize_Game;

        public SpawnPoint(Vector2 PositionIn, float spawnSize, float glowSize_Game, int startposession, SoundEffect capture, SoundEffect captureExplosion, 
                          Texture2D glowTexture, Texture2D explosionTexture, Texture2D innerTexture, Texture2D outerTexture)
            : base(PositionIn, startposession, /*1.0f / (float)Math.Log(spawnSize)*/ 4.0f / spawnSize, -1.0f, 5)
        {
            this.capture = capture;
            this.captureExplosion = captureExplosion;
            this.glowTexture = glowTexture;
            this.innerTexture = innerTexture;
            this.outerTexture = outerTexture;
            this.explosionTexture = explosionTexture;
            this.SpawnSize = spawnSize;
            this.glowSize_Game = glowSize_Game;

            if(startposession != -1)
                glowtimer.Start();

            Size = ((spawnSize - 100.0f )/ 900.0f) * (0.025f) + 0.025f;
        }

        protected override void OnPossessingChanged()
        {
            if(PossessingPlayer != -1)
            {
                captureExplosion.Play();
                glowtimer.Start();
                explosionTimer.Start();
                explosionRotation = (float)(Random.NextDouble() * MathHelper.TwoPi);
            }
            SpawnTimeAccum = 0.0f;
        }

        public override void Update(float frameTimeSeconds, float totalTimeSeconds)
        {
            base.Update(frameTimeSeconds, totalTimeSeconds);

            if (explosionTimer.IsRunning)
            {
                float effectseconds = (float) explosionTimer.Elapsed.TotalSeconds;
                if (effectseconds > explosionDuration)
                {
                    explosionTimer.Reset();
                    capture.Play();
                }
                else
                {
                    float scaling = MathHelper.Clamp((float) Math.Log(effectseconds*16 + 1.0f)/3, 0.0f, 1.0f);
                    currentExplosionSize = explosionMaxSize*scaling;
                    currentExplosionAlpha = 1.0f - effectseconds/explosionDuration;
                }
            }
        }

        public override void Draw_AlphaBlended(SpriteBatch spriteBatch, Level level, float totalTimeSeconds)
        {
            // main
            Color color = ComputeColor();
            Rectangle rect = level.ComputePixelRect(Position, Size);
            spriteBatch.Draw(outerTexture, rect, null, color, totalTimeSeconds, new Vector2(outerTexture.Width * 0.5f, outerTexture.Height * 0.5f), SpriteEffects.None, 0.8f);
            spriteBatch.Draw(innerTexture, rect, null, color, totalTimeSeconds, new Vector2(innerTexture.Width * 0.5f, innerTexture.Height * 0.5f), SpriteEffects.None, 0.7f);

            // explosion
            if (explosionTimer.IsRunning && PossessingPlayer != -1)
            {
                Color explosionColor = Settings.Instance.GetPlayerColor(PossessingPlayer) * currentExplosionAlpha;   // using premultiplied values, the whole colore has to be multiplied for alphablending
                spriteBatch.Draw(explosionTexture, level.ComputePixelRect(Position, currentExplosionSize), null, explosionColor, explosionRotation,
                                        new Vector2(explosionTexture.Width / 2, explosionTexture.Height / 2), SpriteEffects.None, 0.1f);
            }
        }

        public override void Draw_ScreenBlended(SpriteBatch spriteBatch, Level level, float totalTimeSeconds)
        {
            // glow for possed ones
            if (PossessingPlayer != -1)
            {
                float currentsize = MathHelper.Clamp((float)glowtimer.Elapsed.TotalSeconds * 2.5f, 0.0f, 1.0f) * glowSize_Game;
                Color glowColor = Settings.Instance.GetPlayerColor(PossessingPlayer) * 0.5f;
                if (currentsize > 0.0f)
                    spriteBatch.Draw(glowTexture, level.ComputePixelRect_Centered(Position, currentsize), null, glowColor, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            }
        }

        public override void DrawToDamageMap(SpriteBatch spriteBatch)
        {
            if (explosionTimer.IsRunning && PossessingPlayer != -1)
            {
                Color damage = Player.TextureDamageValue[PossessingPlayer] * explosionDamage * currentExplosionAlpha;
                spriteBatch.Draw(explosionTexture, DamageMap.ComputePixelRect(Position, currentExplosionSize), null, damage, explosionRotation,
                                     new Vector2(explosionTexture.Width / 2, explosionTexture.Height / 2), SpriteEffects.None, 1.0f);
            }

            // uncaptured? kills people!
            if (PossessingPlayer == -1)
            {
                spriteBatch.Draw(glowTexture, DamageMap.ComputePixelRect_Centred(Position, capturingDamageSize), capturingDamage);
            }
        }
    }
}
