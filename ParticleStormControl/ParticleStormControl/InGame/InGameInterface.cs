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
        private Texture2D itemDanger;
        private Texture2D itemMutate;
        private Texture2D itemWipeout;

        private SpriteFont dieCountdownFont;

        private const float TRANSPARENCY = 0.6f;

        public InGameInterface(ContentManager content)
        {
            itemBox = content.Load<Texture2D>("itemBox");
            itemDanger = content.Load<Texture2D>("items/danger");
            itemMutate = content.Load<Texture2D>("items/mutate");
            itemWipeout = content.Load<Texture2D>("items/wipeout");

            dieCountdownFont = content.Load<SpriteFont>("fonts/fontHeading");
        }

        /// <summary>
        /// draws all interface elements
        /// </summary>
        /// <param name="players">player array</param>
        /// <param name="spriteBatch">spritebatch that is NOT already started</param>
        public void DrawInterface(Player[] players, SpriteBatch spriteBatch, Point levelPixelSize, Point levelPixelOffset, GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);

            Point[] corners = { new Point(levelPixelOffset.X, levelPixelOffset.Y + levelPixelSize.Y),
                                new Point(levelPixelOffset.X + levelPixelSize.X, levelPixelOffset.Y),
                                new Point(levelPixelOffset.X + levelPixelSize.X, levelPixelOffset.Y + levelPixelSize.Y),
                                new Point(levelPixelOffset.X, levelPixelOffset.Y)
                              };
            Rectangle[] itemDisplayRectangles = { new Rectangle(corners[0].X, corners[0].Y - itemBox.Height, itemBox.Width, itemBox.Height),
                                                  new Rectangle(corners[1].X - itemBox.Width, corners[1].Y, itemBox.Width, itemBox.Height),
                                                  new Rectangle(corners[2].X - itemBox.Width, corners[2].Y - itemBox.Height, itemBox.Width, itemBox.Height),
                                                  new Rectangle(corners[3].X, corners[3].Y, itemBox.Width, itemBox.Height) };
 
            SpriteEffects[] flips = { SpriteEffects.None, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, SpriteEffects.FlipHorizontally, SpriteEffects.FlipVertically };

            for (int i = 0; i < players.Length; ++i)
            {
                if (players[i].Alive)
                {
                    Color color = players[i].Color;
                    color.A = (byte)(255 * TRANSPARENCY);

                    //Vector2 halfBoxSize = new Vector2(itemBox.Width, itemBox.Height);
                    spriteBatch.Draw(itemBox, itemDisplayRectangles[i], null, color, 0.0f, Vector2.Zero, flips[i], 0);

                    DrawItem(spriteBatch, players[i].ItemSlot, itemDisplayRectangles[i], corners[i], color, Item.ROTATION_SPEED * (float)gameTime.TotalGameTime.TotalSeconds);

                    // countdown if this player is dying soon
                    if(players[i].RemainingTimeAlive <  Player.MAX_TIME_WITHOUT_SPAWNPOINT)
                    {
                        string countdownString = ((int)players[i].RemainingTimeAlive).ToString();
                        Vector2 dragToCorner = new Vector2(itemDisplayRectangles[i].Width / 4 * Math.Sign(corners[i].X - itemDisplayRectangles[i].Center.X),
                                                           itemDisplayRectangles[i].Height / 4 * Math.Sign(corners[i].Y - itemDisplayRectangles[i].Center.Y));
                        Vector2 position = new Vector2(itemDisplayRectangles[i].Center.X, itemDisplayRectangles[i].Center.Y) + dragToCorner;
                        spriteBatch.DrawString(dieCountdownFont, countdownString, position, Color.White, 0.0f, dieCountdownFont.MeasureString(countdownString) / 2, 
                                                   (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds)*0.2f + 1.4f, SpriteEffects.None, 0);
                    }
                }
            }
            spriteBatch.End();
        }

        private void DrawItem(SpriteBatch spriteBatch, Item.ItemType type, Rectangle destination, Point corner, Color color, float rotation)
        {
            const int SIZE_OFFSET = 60;

            Rectangle itemRect = destination;
            itemRect.Inflate(-SIZE_OFFSET / 2, -SIZE_OFFSET / 2);
            // drag to corner
            itemRect.Offset(SIZE_OFFSET / 3 * Math.Sign(corner.X - itemRect.Center.X),
                            SIZE_OFFSET / 3 * Math.Sign(corner.Y - itemRect.Center.Y));

            // centered for rotation
            itemRect.Offset(itemRect.Width / 2, itemRect.Height / 2);
            
            switch (type)
            {
                case Item.ItemType.DANGER_ZONE:
                    spriteBatch.Draw(itemDanger, itemRect, null, new Color(1.0f, 1.0f, 1.0f, TRANSPARENCY), rotation,
                                                   new Vector2(itemDanger.Width, itemDanger.Height) / 2,SpriteEffects.None, 1); // color);
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
