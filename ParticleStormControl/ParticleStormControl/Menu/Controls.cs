using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ParticleStormControl.Menu
{
    class Controls : MenuPage
    {
        public Controls(Menu menu)
            : base(menu)
        {
        }

        public override void LoadContent(ContentManager content)
        {
        }

        public override void Update(GameTime gameTime)
        {
            // back to main menu
            if (InputManager.Instance.PauseButton() || InputManager.Instance.ContinueButton() || InputManager.Instance.PressedButton(Buttons.B) || InputManager.Instance.PressedButton(Keys.Escape) || InputManager.Instance.ExitButton())
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, float frameTimeInterval)
        {
            string[,] data = {
                                 { null,                 "Keyboard 0",   "Keyboard 1",   "Gamepad" },
                                 { "Up",                 "W",            "Arrow Up",     null },
                                 { "Left",               "A",            "Arrow Left",   null },
                                 { "Down",               "S",            "Arrow Down",   null },
                                 { "Right",              "D",            "Arrow Right",  null },
                                 { "Action / Use Item",  "Space",        "Enter",        null },
                                 { "Back / Hold Cursor", "V",            "Shift",        null },
                             };

            // big table
            int column = 220;   // column width
            int row = 60;       // row height
            int gap = 20;       // gap between columns
            int top = 100;       // distance from top
            int left = (Settings.Instance.ResolutionX - data.GetLength(1) * (column + gap)) / 2 + gap; // distance from left (centered)

            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    if (data[i, j] != null)
                        if(i == 0 || j == 0)
                            SimpleButton.Instance.Draw(spriteBatch, menu.Font, data[i, j], new Vector2(left + j * (column + gap), top + i * row), false, menu.TexPixel, column);
                        else
                            SimpleButton.Instance.Draw(spriteBatch, menu.Font, data[i, j], new Vector2(left + j * (column + gap), top + i * row), Color.White, Color.Black, menu.TexPixel, column);
                }
            }

            // draw icons
            int fontHeight = (int)menu.Font.MeasureString("Test").Y + 2 * SimpleButton.PADDING;
            
            SimpleButton.Instance.DrawTexture_NoScalingNoPadding(
                spriteBatch,
                menu.TexLeftThumbstick,
                new Rectangle(left + 3 * (column + gap) - SimpleButton.PADDING, top + 1 * row - SimpleButton.PADDING, column, row * 3 + fontHeight), //new Rectangle(left + 3 * (column + gap) - SimpleButton.PADDING + (column - menu.TexLeftThumbstick.Width) / 2, top + 1 * row - SimpleButton.PADDING + (row * 4 - gap - menu.TexLeftThumbstick.Height) / 2, menu.TexLeftThumbstick.Width, menu.TexLeftThumbstick.Height),
                menu.TexLeftThumbstick.Bounds,
                true,
                menu.TexPixel,
                column);
            SimpleButton.Instance.DrawTexture_NoScalingNoPadding(
                spriteBatch,
                menu.TexA,
                new Rectangle(left + 3 * (column + gap) - SimpleButton.PADDING, top + 5 * row - SimpleButton.PADDING, column, fontHeight),
                menu.TexA.Bounds,
                true,
                menu.TexPixel);
            SimpleButton.Instance.DrawTexture_NoScalingNoPadding(
                spriteBatch,
                menu.TexB,
                new Rectangle(left + 3 * (column + gap) - SimpleButton.PADDING, top + 6 * row - SimpleButton.PADDING, column, fontHeight),
                menu.TexB.Bounds,
                true,
                menu.TexPixel);

            // back button
            string label =  "Back to Menu";
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, label, new Vector2((int)((Settings.Instance.ResolutionX - menu.Font.MeasureString(label).X) / 2), Settings.Instance.ResolutionY - 100), true, menu.TexPixel);
        }
    }
}
