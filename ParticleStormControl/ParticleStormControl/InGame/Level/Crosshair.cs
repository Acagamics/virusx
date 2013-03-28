using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using ParticleStormControl;

namespace ParticleStormControl
{
    class Crosshair:MapObject
    {
        public int PlayerIndex { get; private set; }

        /// <summary>
        /// position of the cursor the particles are attracted to
        /// MapObject.Position stands for the moving cursor position
        /// </summary>
        public Vector2 ParticleAttractionPosition { get; set; }

        private Texture2D crossHairTexture;

        public Crosshair (int playerindex, ContentManager contentManager) :
            base(Vector2.One, 0.05f)   // updated every frame, so why care..
        {
            PlayerIndex = playerindex;
            this.crossHairTexture = contentManager.Load<Texture2D>("basic_crosshair");;
        }

        public override void SwitchPlayer(int[] playerSwitchedTo)
        {
            base.SwitchPlayer(playerSwitchedTo);
            PlayerIndex = playerSwitchedTo[PlayerIndex];
        }

        public override void Draw_AlphaBlended(SpriteBatch spriteBatch, Level level, float totalTimeSeconds)
        {
            Color color = Settings.Instance.GetPlayerColor(PlayerIndex);
            spriteBatch.Draw(crossHairTexture, level.ComputePixelRect(ParticleAttractionPosition, Size), null, color,
                                -totalTimeSeconds, new Vector2(crossHairTexture.Width * 0.5f, crossHairTexture.Height * 0.5f),
                                SpriteEffects.None, 0.0f);

            if (ParticleAttractionPosition != Position)
            {
                color.A = 150;
                spriteBatch.Draw(crossHairTexture, level.ComputePixelRect(Position, Size), null, color,
                    -totalTimeSeconds, new Vector2(crossHairTexture.Width * 0.5f, crossHairTexture.Height * 0.5f),
                    SpriteEffects.None, 0.0f);
            }
        }
    }
}
