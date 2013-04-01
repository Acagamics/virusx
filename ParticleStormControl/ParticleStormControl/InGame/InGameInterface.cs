using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ParticleStormControl
{
    class InGameInterface
    {
        private Texture2D itemBox;
        private Texture2D itemBuff;
        private Texture2D itemMutate;
        private Texture2D itemWipeout;

        private const float TRANSPARENCY = 0.6f;

        public InGameInterface(ContentManager content)
        {
            itemBox = content.Load<Texture2D>("itemBox");
            itemBuff = content.Load<Texture2D>("items/buff");
            itemMutate = content.Load<Texture2D>("items/mutate");
            itemWipeout = content.Load<Texture2D>("items/wipeout");
        }

        /// <summary>
        /// draws all interface elements
        /// </summary>
        /// <param name="players">player array</param>
        /// <param name="spriteBatch">spritebatch that is NOT already started</param>
        public void DrawInterface(Player[] players, SpriteBatch spriteBatch, Point levelPixelSize, Point levelPixelOffset, float totalTimeSeconds)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);

            Rectangle[] itemDisplayRectangles = { new Rectangle(levelPixelOffset.X, levelPixelOffset.Y + levelPixelSize.Y - itemBox.Height, itemBox.Width, itemBox.Height),
                                                  new Rectangle(levelPixelOffset.X + levelPixelSize.X - itemBox.Width, levelPixelOffset.Y, itemBox.Width, itemBox.Height),
                                                  new Rectangle(levelPixelOffset.X + levelPixelSize.X - itemBox.Width, levelPixelOffset.Y + levelPixelSize.Y - itemBox.Height, itemBox.Width, itemBox.Height),
                                                  new Rectangle(levelPixelOffset.X, levelPixelOffset.Y, itemBox.Width, itemBox.Height) };
            SpriteEffects[] flips = { SpriteEffects.None, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, SpriteEffects.FlipHorizontally, SpriteEffects.FlipVertically };

            for (int i = 0; i < players.Length; ++i)
            {
                Color color = players[i].Color;
                color.A = (byte)(255 * TRANSPARENCY);

                Vector2 halfBoxSize = new Vector2(itemBuff.Width, itemBuff.Height);
                spriteBatch.Draw(itemBox, itemDisplayRectangles[i], null, color, 0.0f, Vector2.Zero, flips[i], 0);

                DrawItem(spriteBatch, players[i].ItemSlot, itemDisplayRectangles[i], color, Item.ROTATION_SPEED * totalTimeSeconds);
            }
            spriteBatch.End();
        }

        private void DrawItem(SpriteBatch spriteBatch, Item.ItemType type, Rectangle destination, Color color, float rotation)
        {
            const int SIZE_OFFSET = 20;

            Rectangle itemRect = destination;
            itemRect.Offset(SIZE_OFFSET / 2, SIZE_OFFSET / 2);
            itemRect.Width -= SIZE_OFFSET;
            itemRect.Height -= SIZE_OFFSET;
            itemRect.Offset(itemRect.Width / 2, itemRect.Height / 2);
            switch (type)
            {
                case Item.ItemType.DANGER_ZONE:
                    spriteBatch.Draw(itemBuff, itemRect, null, new Color(1.0f, 1.0f, 1.0f, TRANSPARENCY), rotation,
                                                   new Vector2(itemBuff.Width, itemBuff.Height) / 2,SpriteEffects.None, 1); // color);
                    break;
                case Item.ItemType.MUTATION:
                    spriteBatch.Draw(itemMutate, itemRect, null, new Color(1.0f, 1.0f, 1.0f, TRANSPARENCY), rotation,
                                                   new Vector2(itemMutate.Width, itemMutate.Height) / 2, SpriteEffects.None, 1); // color);
                    break;
                case Item.ItemType.WIPEOUT:
                    spriteBatch.Draw(itemWipeout, itemRect, null, new Color(1.0f, 1.0f, 1.0f, TRANSPARENCY), rotation,
                                                   new Vector2(itemWipeout.Width, itemWipeout.Height) / 2, SpriteEffects.None, 1); // color);
                    break;
            }
        }
    }
}
