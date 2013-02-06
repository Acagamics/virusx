using System;
using System.Diagnostics;
using ParticleStormControl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace ParticleStormControl
{
    public class DangerZone : CapturableObject
    {
        /// <summary>
        /// max explosion size
        /// </summary>
        public const float explosionMaxSize = 0.22f;

        private const float explosionDamage = 15;
        private const float explosionDuration = 7.0f;
        private float currentExplosionSize;
        private float currentRotation;

        private Stopwatch explosionTimer;
        private float alpha = 1.0f;

        private SoundEffect explosionSound;
        private Texture2D itemTexture;
        private Texture2D dangerZoneTextureOuter;
        private Texture2D dangerZoneTextureInner;

        private readonly Vector2 textureCenterItem;
        private readonly Vector2 textureCenterZone;

        public DangerZone(Vector2 Position, SoundEffect explosionSound, Texture2D itemTexture, Texture2D dangerZoneTextureInner, Texture2D dangerZoneTextureOuter)
            : base(Position, -1, 0.1f, 10.0f, 3)
        {
            this.explosionSound = explosionSound;
            this.itemTexture = itemTexture;
            this.dangerZoneTextureOuter = dangerZoneTextureOuter;
            this.dangerZoneTextureInner = dangerZoneTextureInner;

            explosionTimer = new Stopwatch();

            Size = 0.05f;
            textureCenterItem = new Vector2(itemTexture.Width / 2, itemTexture.Height / 2);
            textureCenterZone = new Vector2(dangerZoneTextureOuter.Width / 2, dangerZoneTextureOuter.Height / 2);
        }

        protected override void OnPossessingChanged()
        {
            if (PossessingPlayer != -1)
            {
                explosionSound.Play();
                explosionTimer.Start();
            }
        }

        public override void ApplyDamage(DamageMap damageMap, float timeInterval)
        {
            if(PossessingPlayer == -1)
                base.ApplyDamage(damageMap, timeInterval);
        }

        public override void Update(float frameTimeSeconds, float totalTimeSeconds)
        {
            base.Update(frameTimeSeconds, totalTimeSeconds);

            float effectSeconds = (float)explosionTimer.Elapsed.TotalSeconds;
            float scaling = MathHelper.Clamp((float)Math.Log(effectSeconds * 16 + 1.0f) / 3, 0.0f, 1.0f);
            currentExplosionSize = explosionMaxSize * scaling;

            alpha = 1.0f - (float)Math.Pow(effectSeconds/explosionDuration, 30.0f);

            currentRotation += frameTimeSeconds;

            if (effectSeconds >= explosionDuration)
                Alive = false;
        }

        public override void DrawToDamageMap(SpriteBatch spriteBatch)
        {
            if (explosionTimer.IsRunning)
            {
                Color damage = Player.TextureDamageValue[PossessingPlayer] * explosionDamage * alpha;
                spriteBatch.Draw(dangerZoneTextureInner, DamageMap.ComputePixelRect(Position, currentExplosionSize), null, damage, currentRotation, textureCenterZone, SpriteEffects.None, 0);
            }
        }

        public override void Draw_AlphaBlended(SpriteBatch spriteBatch, Level level, float totalTimeSeconds)
        {
            // explosion
            if (explosionTimer.IsRunning)
            {
                Rectangle rect = level.ComputePixelRect(Position, currentExplosionSize);
                spriteBatch.Draw(dangerZoneTextureOuter, rect, null, Settings.Instance.GetPlayerColor(PossessingPlayer) * alpha, 0.0f, textureCenterZone, SpriteEffects.None, 0);
                spriteBatch.Draw(dangerZoneTextureInner, rect, null, Settings.Instance.GetPlayerColor(PossessingPlayer) * alpha, currentRotation, textureCenterZone, SpriteEffects.None, 0);
            }
                    
            // item
            else
            {
                spriteBatch.Draw(itemTexture, level.ComputePixelRect(Position, Size), null, Color.Lerp(Color.White, Color.Black, PossessingPercentage),
                                    totalTimeSeconds, textureCenterItem, SpriteEffects.None, 1.0f);
            }
        }
    }
}

