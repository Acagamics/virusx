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
        private static readonly Color selectedColor = Color.Red;
        private static readonly Color normalColor = Color.Black;

        static public void Draw(SpriteBatch spriteBatch, SpriteFont font, string label, Vector2 midPos, Color color)
        {
            Vector2 halfStringSize = font.MeasureString(label) / 2;
            spriteBatch.DrawString(font, label, midPos - halfStringSize, color);
        }

        static public void Draw(SpriteBatch spriteBatch, SpriteFont font, string label, Vector2 midPos, bool selected) 
        {
            Draw(spriteBatch, font, label, midPos, selected ? selectedColor : normalColor);
        }
    }
}
