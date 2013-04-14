using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ParticleStormControl
{
    class PercentageBar
    {
        static public readonly int HEIGHT = 40;

        Texture2D pixel;
        Texture2D bar;

        public PercentageBar(ContentManager Content)
        {
            pixel = Content.Load<Texture2D>("pix");
            bar = Content.Load<Texture2D>("percentagebar");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="players"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="levelPixelSize"></param>
        /// <param name="levelPixelOffset"></param>
        public void Draw(Player[] players, SpriteBatch spriteBatch, Point levelPixelSize, Point levelPixelOffset)
        {
            spriteBatch.Begin();

            int sum = 0;
            for (int i = 0; i < players.Length; i++)
            {
                sum += GetPercent(players, i);
            }

            // only colors
            int offset = 0;
            for (int i = 0; i < players.Length; i++)
            {
                int width = (int)(GetPercent(players, i) / (float)sum * levelPixelSize.X);
                spriteBatch.Draw(pixel, new Rectangle(levelPixelOffset.X + offset, levelPixelOffset.Y - HEIGHT, width, HEIGHT), players[i].Color);
                offset += width;
            }

            // obfuscate
            for (int i = 0; i <= levelPixelSize.X / bar.Width; i++)
            {
                spriteBatch.Draw(bar, new Rectangle(levelPixelOffset.X + bar.Width * i, levelPixelOffset.Y - HEIGHT, bar.Width, HEIGHT), Color.White);
            }

            spriteBatch.End();
        }

        private int GetPercent(Player[] players, int index)
        {
            return (int)players[index].TotalHealth+players[index].NumParticlesAlive+(100000*(int)players[index].PossingBases);
        }
    }
}
