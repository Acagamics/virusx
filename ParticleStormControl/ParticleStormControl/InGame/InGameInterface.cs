using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Globalization;

namespace VirusX
{
    class InGameInterface
    {
        private Texture2D itemBox;
        private Texture2D itemDanger;
        private Texture2D itemMutate;
        private Texture2D itemWipeout;

        private SpriteFont dieCountdownFont;

        private const float TRANSPARENCY = 0.6f;

        private PercentageBar percentageBar;
     
        public InGameInterface(ContentManager content)
        {
            itemBox = content.Load<Texture2D>("itemBox");
            itemDanger = content.Load<Texture2D>("items/danger");
            itemMutate = content.Load<Texture2D>("items/mutate");
            itemWipeout = content.Load<Texture2D>("items/wipeout");

            dieCountdownFont = content.Load<SpriteFont>("fonts/fontHeading");

            percentageBar = new PercentageBar(content);
        }

        /// <summary>
        /// draws all interface elements
        /// </summary>
        /// <param name="players">player array</param>
        /// <param name="spriteBatch">spritebatch that is NOT already started</param>
        public void DrawInterface(Player[] players, SpriteBatch spriteBatch, Point levelPixelSize, Point levelPixelOffset, GameTime gameTime)
        {
            Point[] corners;
            Rectangle[] itemDisplayRectangles;
            SpriteEffects[] flips;
            ComputeRectangles(out corners, out itemDisplayRectangles, out flips, levelPixelSize, levelPixelOffset);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            for (int playerIndex = 0; playerIndex < players.Length; ++playerIndex)
            {
                if (players[playerIndex].Alive)
                {
                    Color color = players[playerIndex].Color;
                    color.A = (byte)(255 * TRANSPARENCY);
                    int slot = Settings.Instance.GetPlayer(playerIndex).SlotIndex;
                    //Vector2 halfBoxSize = new Vector2(itemBox.Width, itemBox.Height);
                    spriteBatch.Draw(itemBox, itemDisplayRectangles[slot], null, color, 0.0f, Vector2.Zero, flips[slot], 0);

                    color.A = (byte)(255 /* TRANSPARENCY*/ * players[playerIndex].ItemAlphaValue);
                    
                    DrawItem(spriteBatch, players[playerIndex].ItemSlot, itemDisplayRectangles[slot], corners[slot], color, Item.ROTATION_SPEED * (float)gameTime.TotalGameTime.TotalSeconds, players[playerIndex].ItemAlphaValue);

                    // countdown if this player is dying soon
                    if(players[playerIndex].RemainingTimeAlive <  Player.MAX_TIME_WITHOUT_SPAWNPOINT)
                    {
                        string countdownString = ((int)players[playerIndex].RemainingTimeAlive).ToString();

                        Vector2 dragToCorner = new Vector2(itemDisplayRectangles[slot].Width / 5 * Math.Sign(corners[slot].X - itemDisplayRectangles[slot].Center.X),
                                                           itemDisplayRectangles[slot].Height / 5 * Math.Sign(corners[slot].Y - itemDisplayRectangles[slot].Center.Y));
                        Vector2 position = new Vector2(itemDisplayRectangles[slot].Center.X, itemDisplayRectangles[slot].Center.Y) + dragToCorner;
                        spriteBatch.DrawString(dieCountdownFont, countdownString, position, new Color(1f,1f,1f,0.5f), 0.0f, dieCountdownFont.MeasureString(countdownString) / 2, 
                                                   (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds)*0.2f + 1.4f, SpriteEffects.None, 0);
                    }
                }
            }
            spriteBatch.End();

            // draw the percentage bar
            percentageBar.Draw(players, spriteBatch, levelPixelSize, levelPixelOffset);
        }

        private void ComputeRectangles(out Point[] corners, out Rectangle[] itemDisplayRectangles, out SpriteEffects[] flips, Point levelPixelSize, Point levelPixelOffset)
        {
            corners = new Point[]{ new Point(levelPixelOffset.X, levelPixelOffset.Y + levelPixelSize.Y),
                                new Point(levelPixelOffset.X + levelPixelSize.X, levelPixelOffset.Y),
                                new Point(levelPixelOffset.X + levelPixelSize.X, levelPixelOffset.Y + levelPixelSize.Y),
                                new Point(levelPixelOffset.X, levelPixelOffset.Y)
                              };
            itemDisplayRectangles = new Rectangle[]{ new Rectangle(corners[0].X, corners[0].Y - itemBox.Height, itemBox.Width, itemBox.Height),
                                                  new Rectangle(corners[1].X - itemBox.Width, corners[1].Y, itemBox.Width, itemBox.Height),
                                                  new Rectangle(corners[2].X - itemBox.Width, corners[2].Y - itemBox.Height, itemBox.Width, itemBox.Height),
                                                  new Rectangle(corners[3].X, corners[3].Y, itemBox.Width, itemBox.Height) };

            flips = new SpriteEffects[]{ SpriteEffects.None, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, SpriteEffects.FlipHorizontally, SpriteEffects.FlipVertically };
        }

        /// <summary>
        /// draws all interface elements
        /// </summary>
        /// <param name="players">player array</param>
        /// <param name="spriteBatch">spritebatch that is NOT already started</param>
        public void DrawInterface(Player[] players, SpriteBatch spriteBatch, Point levelPixelSize, Point levelPixelOffset, GameTime gameTime, System.Diagnostics.Stopwatch[] winTimer )
        {
            Point[] corners;
            Rectangle[] itemDisplayRectangles;
            SpriteEffects[] flips;
            ComputeRectangles(out corners, out itemDisplayRectangles, out flips, levelPixelSize, levelPixelOffset);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            for (int playerIndex = 0; playerIndex < players.Length; ++playerIndex)
            {
                if (players[playerIndex].Alive)
                {
                    Color color = players[playerIndex].Color;
                    color.A = (byte)(255 * TRANSPARENCY);
                    int slot = Settings.Instance.GetPlayer(playerIndex).SlotIndex;
                    //Vector2 halfBoxSize = new Vector2(itemBox.Width, itemBox.Height);
                    spriteBatch.Draw(itemBox, itemDisplayRectangles[slot], null, color, 0.0f, Vector2.Zero, flips[slot], 0);

                    color.A = (byte)(100 /* TRANSPARENCY*/ * players[playerIndex].ItemAlphaValue);

                    DrawItem(spriteBatch, players[playerIndex].ItemSlot, itemDisplayRectangles[slot], corners[slot], color, Item.ROTATION_SPEED * (float)gameTime.TotalGameTime.TotalSeconds, players[playerIndex].ItemAlphaValue);
                    
                    string countdownString = (InGame.ModeWinTime - winTimer[playerIndex].Elapsed.TotalSeconds).ToString("N0");

                    Vector2 dragToCorner = new Vector2(itemDisplayRectangles[slot].Width / 5 * Math.Sign(corners[slot].X - itemDisplayRectangles[slot].Center.X),
                                                       itemDisplayRectangles[slot].Height / 5 * Math.Sign(corners[slot].Y - itemDisplayRectangles[slot].Center.Y));
                    Vector2 position = new Vector2(itemDisplayRectangles[slot].Center.X, itemDisplayRectangles[slot].Center.Y) + dragToCorner;
                    spriteBatch.DrawString(dieCountdownFont, countdownString, position, new Color(0f, 0f, 0f, 1f), 0.0f, dieCountdownFont.MeasureString(countdownString) / 2,
                                              (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds) * 0.2f + 1.4f, SpriteEffects.None, 0);
                }
            }
            spriteBatch.End();
        }

        private void DrawItem(SpriteBatch spriteBatch, Item.ItemType type, Rectangle destination, Point corner, Color color, float rotation, float itemAlpha)
        {
            const int SIZE_OFFSET = 60;

            Rectangle itemRect = destination;
            itemRect.Inflate(-SIZE_OFFSET / 2, -SIZE_OFFSET / 2);
            // drag to corner
            itemRect.Offset(SIZE_OFFSET / 3 * Math.Sign(corner.X - itemRect.Center.X),
                            SIZE_OFFSET / 3 * Math.Sign(corner.Y - itemRect.Center.Y));

            // centered for rotation
            itemRect.Offset(itemRect.Width / 2, itemRect.Height / 2);
            float transparency = itemAlpha;
            switch (type)
            {
                case Item.ItemType.DANGER_ZONE:
                    spriteBatch.Draw(itemDanger, itemRect, null, new Color(1.0f, 1.0f, 1.0f, transparency), rotation,
                                                   new Vector2(itemDanger.Width, itemDanger.Height) / 2,SpriteEffects.None, 1); // color);
                    break;
                case Item.ItemType.MUTATION:
                    spriteBatch.Draw(itemMutate, itemRect, null, new Color(1.0f, 1.0f, 1.0f, transparency), rotation,
                                                   new Vector2(itemMutate.Width, itemMutate.Height) / 2, SpriteEffects.None, 1); // color);
                    break;
                case Item.ItemType.WIPEOUT:
                    spriteBatch.Draw(itemWipeout, itemRect, null, new Color(1.0f, 1.0f, 1.0f, transparency), rotation,
                                                   new Vector2(itemWipeout.Width, itemWipeout.Height) / 2, SpriteEffects.None, 1); // color);
                    break;
            }
        }
    }
}
