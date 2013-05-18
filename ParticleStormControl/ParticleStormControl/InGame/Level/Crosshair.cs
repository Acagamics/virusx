using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using ParticleStormControl;
using System;
using System.Diagnostics;

namespace ParticleStormControl
{
    class Crosshair:MapObject
    {
        public int PlayerIndex { get; private set; }
        public bool PlayerAlive { get; set; }

        /// <summary>
        /// position of the cursor the particles are attracted to
        /// MapObject.Position stands for the moving cursor position
        /// </summary>
        public Vector2 ParticleAttractionPosition { get; set; }

        private Texture2D crossHairTexture;

        private float currentRotation;

        private const float CURSOR_SIZE = 0.07f;

        /// <summary>
        /// cursor image for dead players, also used for damage
        /// </summary>
        private Texture2D deadPlayerCursor;
        private float deathTimer = 0.0f;
        private const float DEATH_CURSOR_SIZE_PERCENTAGE = 0.25f;    // percentage of MAX_DEATH_EXPL_SIZE that is the normal death cursor size
        private const float MAX_DEATH_EXPL_SIZE = 0.35f;
        private const float EXPL_SCALE_SPEED = 2.0f;
        private const float DEATH_DAMAGE = 60; // is multiplied by the size, so don't be afraid, this value isn't as high as it looks
       
        public Crosshair (int playerindex, ContentManager contentManager) :
            base(Vector2.One, CURSOR_SIZE)
        {
            PlayerIndex = playerindex;
            this.crossHairTexture = contentManager.Load<Texture2D>("basic_crosshair");
            this.deadPlayerCursor = contentManager.Load<Texture2D>("death");

            currentRotation = (float)Random.NextDouble() * MathHelper.TwoPi;
            PlayerAlive = true;
        }

        public override void Update(GameTime gameTime)
        {
            currentRotation -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!PlayerAlive)
            {
                deathTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                float scaling = deathTimer * EXPL_SCALE_SPEED;
                if(scaling > 1.0f)
                {
                    scaling = 2.0f - scaling;
                    if (scaling < DEATH_CURSOR_SIZE_PERCENTAGE)
                        scaling = DEATH_CURSOR_SIZE_PERCENTAGE;
                }

                Size = MAX_DEATH_EXPL_SIZE * scaling;
            }

            base.Update(gameTime);
        }

        public override void Draw_AlphaBlended(SpriteBatch spriteBatch, Level level, GameTime gameTime)
        {
            Color color = Settings.Instance.GetPlayerColor(PlayerIndex);
            float saturation = Vector3.Dot(color.ToVector3(), new Vector3(0.3f, 0.59f, 0.11f));
            float modificator = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds) * 0.5f;
            color = Color.Lerp(color, new Color(saturation, saturation, saturation), 0.4f /*+ modificator*/) * (1.0f + 0.5f + modificator);

            // normal cursor
            if (PlayerAlive)
            {
                spriteBatch.Draw(crossHairTexture, level.ComputePixelRect(ParticleAttractionPosition, Size), null, color,
                                    currentRotation, new Vector2(crossHairTexture.Width * 0.5f, crossHairTexture.Height * 0.5f),
                                    SpriteEffects.None, 0.0f);

                if (ParticleAttractionPosition != Position)
                {
                    color.A = 150;
                    spriteBatch.Draw(crossHairTexture, level.ComputePixelRect(Position, Size), null, color,
                       currentRotation, new Vector2(crossHairTexture.Width * 0.5f, crossHairTexture.Height * 0.5f),
                        SpriteEffects.None, 0.0f);
                }
            }
            // dead cursor
            else
            {
                color.A = 150;
                spriteBatch.Draw(deadPlayerCursor, level.ComputePixelRect(Position, Size), null, color,
                                    currentRotation, new Vector2(deadPlayerCursor.Width * 0.5f, deadPlayerCursor.Height * 0.5f),
                                    SpriteEffects.None, 0.0f);
            }
        }

        public override void DrawToDamageMap(SpriteBatch spriteBatch)
        {
            Color damage = VirusSwarm.GetDamageMapDrawColor(PlayerIndex) * (DEATH_DAMAGE * Size);
            spriteBatch.Draw(deadPlayerCursor, DamageMap.ComputePixelRect(Position, Size), null, damage, currentRotation, 
                                new Vector2(deadPlayerCursor.Width, deadPlayerCursor.Height) * 0.5f, SpriteEffects.None, 0);
        }
    }
}
