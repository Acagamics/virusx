using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace VirusX
{
    class Crosshair
    {
        private bool playerAlive;

        /// <summary>
        /// position of the cursor the particles are attracted to
        /// </summary>
        private Vector2 particleAttractionPosition;

        /// <summary>
        /// position of the particle cursor (under direct player control)
        /// </summary>
        private Vector2 cursorPosition;

        /// <summary>
        /// texture for the normal cursor
        /// </summary>
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

        private float Size;

        public Crosshair (ContentManager contentManager)
        {
            this.crossHairTexture = contentManager.Load<Texture2D>("basic_crosshair");
            this.deadPlayerCursor = contentManager.Load<Texture2D>("death");

            currentRotation = (float)Random.NextDouble() * MathHelper.TwoPi;
            playerAlive = true;

            Size = CURSOR_SIZE;
        }

        public void Update(GameTime gameTime, Player player)
        {
            cursorPosition = player.CursorPosition;
            particleAttractionPosition = player.ParticleAttractionPosition;
            playerAlive = player.Alive;

            currentRotation -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!playerAlive)
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
        }

        public void Draw_AlphaBlended(SpriteBatch spriteBatch, Level level, GameTime gameTime, Color color)
        {
            float saturation = Vector3.Dot(color.ToVector3(), new Vector3(0.3f, 0.59f, 0.11f));
            float modificator = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds) * 0.5f;
            color = Color.Lerp(color, new Color(saturation, saturation, saturation), 0.4f /*+ modificator*/) * (1.0f + 0.5f + modificator);

            // normal cursor
            if (playerAlive)
            {
                spriteBatch.Draw(crossHairTexture, level.ComputePixelPosition(particleAttractionPosition), null, color,
                                    currentRotation, new Vector2(crossHairTexture.Width * 0.5f, crossHairTexture.Height * 0.5f), level.ComputeTextureScale(Size, crossHairTexture.Width),
                                    SpriteEffects.None, 0.0f);

                if (particleAttractionPosition != cursorPosition)
                {
                    color.A = 150;
                    spriteBatch.Draw(crossHairTexture, level.ComputePixelPosition(cursorPosition), null, color,
                       currentRotation, new Vector2(crossHairTexture.Width * 0.5f, crossHairTexture.Height * 0.5f), level.ComputeTextureScale(Size, crossHairTexture.Width),
                        SpriteEffects.None, 0.0f);
                }
            }
            // dead cursor
            else
            {
                color.A = 150;
                spriteBatch.Draw(deadPlayerCursor, level.ComputePixelPosition(cursorPosition), null, color,
                                    currentRotation, new Vector2(deadPlayerCursor.Width * 0.5f, deadPlayerCursor.Height * 0.5f), level.ComputeTextureScale(Size, deadPlayerCursor.Width),
                                    SpriteEffects.None, 0.0f);
            }
        }

        public void DrawToDamageMap(SpriteBatch spriteBatch, Color damageMapDrawColor)
        {
            Color damage = damageMapDrawColor * (DEATH_DAMAGE * Size);
            spriteBatch.Draw(deadPlayerCursor, DamageMap.ComputePixelRect(cursorPosition, Size), null, damage, currentRotation, 
                                new Vector2(deadPlayerCursor.Width, deadPlayerCursor.Height) * 0.5f, SpriteEffects.None, 0);
        }
    }
}
