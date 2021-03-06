﻿using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace VirusX
{
    /// <summary>
    /// explosion object on the map
    /// currently nearly the same as DamageArea
    /// </summary>
    class Debuff : CapturableObject
    {
        /// <summary>
        ///  test for arcade mode
        /// </summary>
        public bool IsDeactivated { get; set; }
        /// <summary>
        /// max explosion size
        /// </summary>
        public const float explosionMaxSize = 0.3f;

        private const float explosionDamage = 0.3f;
        private const float duration = 1.0f;
        private float currentExplosionSize;
        private float currentExplosionAlpha;
        private float explosionRotation = 0.0f;

        private readonly Stopwatch explosionTimer = new Stopwatch();
        
        private Texture2D itemTexture;
        private Texture2D explosionTexture;

        private readonly Vector2 textureCenter;

        public bool IsExploding { get { return explosionTimer.IsRunning && Alive; } }

        public Debuff(Vector2 Position, ContentManager content)
            : base(Position, -1, 0.01f, 20.0f, 3)
        {
            explosionTexture = content.Load<Texture2D>("explosion");
            itemTexture = content.Load<Texture2D>("items/debuff");

            Size = 0.05f;
            textureCenter = new Vector2(itemTexture.Width/2, itemTexture.Height/2);
            IsDeactivated = false;
        }

        protected override void OnPossessingChanged()
        {
            AudioManager.Instance.PlaySoundeffect("explosion");
            explosionTimer.Start();
            explosionRotation = (float)(Random.NextDouble()*MathHelper.TwoPi);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float effectSeconds = (float)explosionTimer.Elapsed.TotalSeconds;
            float scaling = MathHelper.Clamp((float)Math.Log(effectSeconds * 16 + 1.0f) / 3, 0.0f, 1.0f);
            currentExplosionSize = explosionMaxSize * scaling;
            currentExplosionAlpha = 1.0f - effectSeconds / duration;

            if (RemainingLifeTime <= duration && !explosionTimer.IsRunning && !IsDeactivated)
                OnPossessingChanged();

            if (explosionTimer.Elapsed.TotalSeconds >= duration)
                Alive = false;
        }

        public override void ApplyDamage(DamageMap damageMap, float timeInterval)
        {
            if (explosionTimer.IsRunning || IsDeactivated)
                return;

            // does NOT use the damage function of CapturableObject since its agnostic of the capturing player

            float[] damage = GetDamageInArea(damageMap, timeInterval);
            float totalDamage = 0.0f;
            for (int i = 0; i<damage.Length; ++i) totalDamage += damage[i];
            PossessingPercentage += totalDamage;

            // done?
            if (PossessingPercentage >= 1.0f)
            {
                // who did this?
                float maxDmg = damage[0];
                CapturingPlayer = 0;
                for (int i = 1; i < damage.Length; ++i)
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

        public override void DrawToDamageMap(SpriteBatch spriteBatch)
        {
            if (explosionTimer.IsRunning)
            {
                Color damage = Color.White * explosionDamage * currentExplosionAlpha;
                spriteBatch.Draw(explosionTexture, DamageMap.ComputePixelRect(Position, currentExplosionSize), null, damage,
                                        explosionRotation, new Vector2(explosionTexture.Width / 2, explosionTexture.Height / 2), SpriteEffects.None, 1.0f);
            }
        }

        override public Color ComputeColor()
        {
            Color resColor = Color.Lerp(Color.White, Color.Black, PossessingPercentage);

            if (IsDeactivated)
                resColor = Color.Black;//Settings.Instance.GetPlayerColor(CapturingPlayer);
            Vector4 w = resColor.ToVector4();
            w.W = opacity / (IsDeactivated ? 4 : 1);
            return new Color(w);
        }

        public override void Draw_AlphaBlended(SpriteBatch spriteBatch, Level level, GameTime gameTime)
        {
            // item
            if (!explosionTimer.IsRunning)
            {
                spriteBatch.Draw(itemTexture, level.ComputePixelPosition(Position), null, ComputeColor(),
                                    (float)gameTime.TotalGameTime.TotalSeconds, textureCenter, level.ComputeTextureScale(Size, itemTexture.Width), SpriteEffects.None, 0.9f);
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
