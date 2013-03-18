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
        private Color movingCursorColor;

        public Crosshair (int playerindex, Texture2D crossHairTexture) :
            base(Vector2.One, 0.05f)   // updated every frame, so why care..
        {
            PlayerIndex = playerindex;
            this.crossHairTexture = crossHairTexture;

            movingCursorColor = Settings.Instance.GetPlayerColor(PlayerIndex);
            movingCursorColor.A = 150;
        }

        public override void SwitchPlayer(int[] playerSwitchedTo)
        {
            base.SwitchPlayer(playerSwitchedTo);
            PlayerIndex = playerSwitchedTo[PlayerIndex];
        }

        public override void Draw_AlphaBlended(SpriteBatch spriteBatch, Level level, float totalTimeSeconds)
        {
            spriteBatch.Draw(crossHairTexture, level.ComputePixelRect(ParticleAttractionPosition, Size), null, Settings.Instance.GetPlayerColor(PlayerIndex),
                                -totalTimeSeconds, new Vector2(crossHairTexture.Width * 0.5f, crossHairTexture.Height * 0.5f),
                                SpriteEffects.None, 0.0f);

            if (ParticleAttractionPosition != Position)
            {
                spriteBatch.Draw(crossHairTexture, level.ComputePixelRect(Position, Size), null, movingCursorColor,
                    -totalTimeSeconds, new Vector2(crossHairTexture.Width * 0.5f, crossHairTexture.Height * 0.5f),
                    SpriteEffects.None, 0.0f);
            }
        }
    }
}
