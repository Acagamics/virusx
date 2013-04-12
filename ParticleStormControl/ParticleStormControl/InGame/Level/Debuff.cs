using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ParticleStormControl
{
    public class Debuff : CapturableObject
    {
        /// <summary>
        /// max explosion size
        /// </summary>
        public const float explosionMaxSize = 0.3f;

        private const float explosionDamage = 0.3f;
        private const float explosionDuration = 1.0f;
        private float currentExplosionSize;
        private float currentExplosionAlpha;
        private float explosionRotation = 0.0f;

        private readonly Stopwatch explosionTimer = new Stopwatch();
        

        private SoundEffect explosionSound;
        private Texture2D itemTexture;
        private Texture2D explosionTexture;

        private readonly Vector2 textureCenter;

        public Debuff(Vector2 Position, ContentManager content)
            : base(Position, -1, 0.01f, 20.0f, 3)
        {
            explosionSound = content.Load<SoundEffect>("sound/explosion");
            explosionTexture = content.Load<Texture2D>("explosion");
            itemTexture = content.Load<Texture2D>("items/debuff");

            Size = 0.05f;
            textureCenter = new Vector2(itemTexture.Width/2, itemTexture.Height/2);
        }

        protected override void OnPossessingChanged()
        {
            if(Settings.Instance.Sound)
                explosionSound.Play();
            explosionTimer.Start();
            explosionRotation = (float)(Random.NextDouble()*MathHelper.TwoPi);
        }

        public override void Update(float frameTimeSeconds, float totalTimeSeconds)
        {
            base.Update(frameTimeSeconds, totalTimeSeconds);

            float effectSeconds = (float)explosionTimer.Elapsed.TotalSeconds;
            float scaling = MathHelper.Clamp((float)Math.Log(effectSeconds * 16 + 1.0f) / 3, 0.0f, 1.0f);
            currentExplosionSize = explosionMaxSize * scaling;
            currentExplosionAlpha = 1.0f - effectSeconds / explosionDuration;

            if (explosionTimer.Elapsed.TotalSeconds >= 1.0f)
                Alive = false;
        }

        public override void ApplyDamage(DamageMap damageMap, float timeInterval)
        {
            // does NOT use the damage function of CapturableObject since its agnostic of the capturing player

            if (!explosionTimer.IsRunning)
            {
                float[] damage = new float[Player.MaxNumPlayers+1];
                for (int i = 0; i < damage.Length; ++i)
                {
                    damage[i] = 0.0f;
                }
                for (int y = damageMap_MinY; y <= damageMap_MaxY; ++y)
                {
                    for (int x = damageMap_MinX; x <= damageMap_MaxX; ++x)
                    {
                        for (int i = 0; i < Player.MaxNumPlayers; ++i)
                        {
                            damage[i] += damageMap.GetPlayerDamageAt(x, y, i);
                            damage[Player.MaxNumPlayers] += damageMap.GetPlayerDamageAt(x, y, i);
                        }
                    }
                }
                PossessingPercentage += damage[Player.MaxNumPlayers]*damageFactor*timeInterval;

                if (PossessingPercentage >= 1.0f)
                {
                    float maxDmg = damage[0];
                    CapturingPlayer = 0;
                    for (int i = 1; i < Player.MaxNumPlayers; ++i)
                    {
                        if (maxDmg < damage[i])
                        {
                            maxDmg = damage[i];
                            CapturingPlayer = i;
                        }
                    }
                    OnPossessingChanged();
                }
            }
        }

        public override void DrawToDamageMap(SpriteBatch spriteBatch)
        {
            if (explosionTimer.IsRunning)
            {
                Color damage = Color.White * explosionDamage * currentExplosionAlpha;
                spriteBatch.Draw(explosionTexture, DamageMap.ComputePixelRect(Position, currentExplosionSize), null, damage,
                                        explosionRotation, new Vector2(explosionTexture.Width / 2, explosionTexture.Height / 2), SpriteEffects.None, 1.0f);
            }
        }

        public override void Draw_AlphaBlended(SpriteBatch spriteBatch, Level level, float totalTimeSeconds)
        {
            // item
            if (!explosionTimer.IsRunning)
            {
                spriteBatch.Draw(itemTexture, level.ComputePixelRect(Position, Size), null, Color.Lerp(Color.White, Color.Black, PossessingPercentage), 
                                    totalTimeSeconds, textureCenter, SpriteEffects.None, 0.9f);
            }
            // explosion
            if (explosionTimer.IsRunning)
            {
                Color explosionColor = Color.Black;
                explosionColor.A = (byte)(255 * currentExplosionAlpha);
                spriteBatch.Draw(explosionTexture, level.ComputePixelRect(Position, currentExplosionSize), null,
                                 explosionColor, explosionRotation, new Vector2(explosionTexture.Width / 2, explosionTexture.Height / 2),
                                 SpriteEffects.None, 0.1f);
            }
        }
    }
}
