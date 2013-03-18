﻿using System;
using System.Diagnostics;
using ParticleStormControl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace ParticleStormControl
{
    public class DangerZone : MapObject
    {
        /// <summary>
        /// max explosion size
        /// </summary>
        public const float explosionMaxSize = 0.22f;

        private const float explosionDamage = 8;
        private const float explosionDuration = 7.0f;
        private float currentExplosionSize;
        private float currentRotation;

        private Stopwatch explosionTimer;
        private float alpha = 1.0f;

        private SoundEffect explosionSound;
        private Texture2D dangerZoneTextureOuter;
        private Texture2D dangerZoneTextureInner;

        private readonly Vector2 textureCenterZone;

        private readonly int possessingPlayer;

        public DangerZone(Vector2 Position, SoundEffect explosionSound, Texture2D dangerZoneTextureInner, Texture2D dangerZoneTextureOuter, int possessingPlayer)
            : base(Position, 0.05f)
        {
            this.explosionSound = explosionSound;
            this.dangerZoneTextureOuter = dangerZoneTextureOuter;
            this.dangerZoneTextureInner = dangerZoneTextureInner;
            this.possessingPlayer = possessingPlayer;

            explosionTimer = new Stopwatch();

            Size = 0.05f;
            textureCenterZone = new Vector2(dangerZoneTextureOuter.Width / 2, dangerZoneTextureOuter.Height / 2);

            Activate();
        }

        private void Activate()
        {
            explosionSound.Play();
            explosionTimer.Start();
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
            Color damage = Player.TextureDamageValue[possessingPlayer] * explosionDamage * alpha;
            spriteBatch.Draw(dangerZoneTextureInner, DamageMap.ComputePixelRect(Position, currentExplosionSize), null, damage, currentRotation, textureCenterZone, SpriteEffects.None, 0);
        }

        public override void Draw_AlphaBlended(SpriteBatch spriteBatch, Level level, float totalTimeSeconds)
        {
            // explosion
            Rectangle rect = level.ComputePixelRect(Position, currentExplosionSize);
            spriteBatch.Draw(dangerZoneTextureOuter, rect, null, Settings.Instance.GetPlayerColor(possessingPlayer) * alpha, 0.0f, textureCenterZone, SpriteEffects.None, 0);
            spriteBatch.Draw(dangerZoneTextureInner, rect, null, Settings.Instance.GetPlayerColor(possessingPlayer) * alpha, currentRotation, textureCenterZone, SpriteEffects.None, 0);
        }
    }
}

