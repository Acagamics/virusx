using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ParticleStormControl.Menu
{
    class SimpleButton
    {
        private static readonly Color selectedColor = Color.Black;
        private static readonly Color normalColor = Color.White;

        private static readonly int padding = 10;

        static public void Draw(SpriteBatch spriteBatch, SpriteFont font, string label, Vector2 position, bool selected, Texture2D texture)
        {
            DrawBackground(spriteBatch, position, selected ? normalColor : selectedColor, texture, font.MeasureString(label));
            spriteBatch.DrawString(font, label, position, selected ? selectedColor : normalColor);
        }

        static public void Draw(SpriteBatch spriteBatch, SpriteFont font, string label, Vector2 position, Color color, Texture2D texture)
        {
            DrawBackground(spriteBatch, position, color, texture, font.MeasureString(label));
            spriteBatch.DrawString(font, label, position, normalColor);
        }

        static public void DrawTexture(SpriteBatch spriteBatch, Texture2D texture, Rectangle destinationRectangle, Rectangle sourceRectangle, bool selected, Texture2D background)
        {
            DrawBackground(spriteBatch, new Vector2(destinationRectangle.X, destinationRectangle.Y), selected ? selectedColor : normalColor, background, new Vector2(sourceRectangle.Width + 8, sourceRectangle.Height + 8));
            destinationRectangle.Offset(4, 4);
            spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, Color.White);
        }

        static private void DrawBackground(SpriteBatch spriteBatch, Vector2 position, Color color, Texture2D texture, Vector2 size)
        {
            spriteBatch.Draw(texture, new Rectangle((int)position.X - padding, (int)position.Y - padding, (int)size.X + 2 * padding, (int)size.Y + 2 * padding), color);
        }
    }
}
