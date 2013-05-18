using System;
using System.Diagnostics;
using ParticleStormControl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace ParticleStormControl
{
    /// <summary>
    /// a damage dealing area on the map
    /// </summary>
    public class DamageArea : MapObject
    {
        /// <summary>
        /// max explosion size
        /// </summary>
        private readonly float explosionMaxSize;

        private readonly float explosionDamage;
        private readonly float explosionScaleSpeed;
        private readonly float duration;
        private float currentRotation;

        private Stopwatch explosionTimer;
        private float alpha = 1.0f;

        private readonly float fadeOutTime;

        private Texture2D damageZoneTexture;

        private readonly Vector2 textureCenterZone;

        private readonly int possessingPlayer;

        /// <summary>
        /// creates a new danger zone damaging area
        /// </summary>
        public static DamageArea CreateDangerZone(ContentManager content, Vector2 Position, int possessingPlayer)
        {
            AudioManager.Instance.PlaySoundeffect("danger");
            return new DamageArea(content.Load<Texture2D>("danger_zone_inner"), Position, possessingPlayer,
                                            0.22f, 3, 8.0f, 7.0f, 0.2f);
        }

        /// <summary>
        /// creates a new wipeout damaging area
        /// </summary>
        public static DamageArea CreateWipeout(ContentManager content)
        {
            AudioManager.Instance.PlaySoundeffect("wipeout");
            return new DamageArea(content.Load<Texture2D>("Wipeout_big"), Level.RELATIVE_MAX / 2, -1,
                                            3.0f, 1.6f, 1000.0f, 0.6f, 0.6f);
        }

        /// <summary>
        /// creates a new playerdeath damaging area
        /// </summary>
     /*   public static DamageArea CreatePlayerDeathDamage(ContentManager content, Vector2 Position, int possessingPlayer)
        {
            return new DamageArea(content.Load<Texture2D>("death"), Position, possessingPlayer,
                                            0.35f, 1, 100.0f, 1.0f);
        }*/

        public DamageArea(Texture2D damageZoneTexture, Vector2 Position, int possessingPlayer,
                            float explosionMaxSize, float explosionScaleSpeed, float explosionDamage, float duration, float fadeoutTime)
            : base(Position, 0.0f)
        {
            this.damageZoneTexture = damageZoneTexture;
            this.explosionMaxSize = explosionMaxSize;
            this.explosionDamage = explosionDamage;
            this.duration = duration;
            this.possessingPlayer = possessingPlayer;
            this.explosionScaleSpeed = explosionScaleSpeed;
            this.fadeOutTime = fadeoutTime;

            explosionTimer = new Stopwatch();

            Size = 0.05f;
            textureCenterZone = new Vector2(damageZoneTexture.Width / 2, damageZoneTexture.Height / 2);

            explosionTimer.Start();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float effectSeconds = (float)explosionTimer.Elapsed.TotalSeconds;
            float scaling = MathHelper.Clamp(effectSeconds * explosionScaleSpeed, 0.0f, 1.0f);
            Size = explosionMaxSize * scaling;

            alpha = 1.0f;//Size / explosionMaxSize
            float remainingTime = duration - effectSeconds;
            if (effectSeconds - duration < fadeOutTime)
                alpha = Math.Min(alpha, remainingTime / fadeOutTime);

            currentRotation += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (remainingTime < 0)
                Alive = false;
        }

        public override void DrawToDamageMap(SpriteBatch spriteBatch)
        {
            Color playerColor;
            if (possessingPlayer < 0)
                playerColor = Color.White;
            else
                playerColor = VirusSwarm.GetDamageMapDrawColor(possessingPlayer);
            Color damage = playerColor * (explosionDamage * alpha);
            spriteBatch.Draw(damageZoneTexture, DamageMap.ComputePixelRect(Position, Size), null, damage, currentRotation, textureCenterZone, SpriteEffects.None, 0);
        }

        public override void Draw_AlphaBlended(SpriteBatch spriteBatch, Level level, GameTime gameTime)
        {
            // explosion
            Rectangle rect = level.ComputePixelRect(Position, Size);
            spriteBatch.Draw(damageZoneTexture, rect, null, Settings.Instance.GetPlayerColor(possessingPlayer) * alpha, currentRotation, textureCenterZone, SpriteEffects.None, 0);
        }
    }
}

