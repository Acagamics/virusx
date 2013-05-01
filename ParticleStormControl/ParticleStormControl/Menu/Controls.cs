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
            string[,] data = {
                                 { null,                 "Keyboard 1",   "Keyboard 2",   "Gamepad" },
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
            int left = -(data.GetLength(1) * (column + gap)) / 2 + gap;

            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    if (data[i, j] != null)
                        if(i == 0 || j == 0)
                            Interface.Add(new InterfaceButton(data[i, j], new Vector2(left + j * (column + gap), top + i * row), () => { return true; }, column - gap, Alignment.TOP_CENTER));
                        else
                            Interface.Add(new InterfaceButton(data[i, j], new Vector2(left + j * (column + gap), top + i * row), () => { return false; }, column - gap, Alignment.TOP_CENTER));
                }
            }

            // draw icons
            int fontHeight = menu.GetFontHeight() + 2 * InterfaceButton.PADDING;
            Interface.Add(new InterfaceImage(
                "ButtonImages/xboxControllerLeftThumbstick",
                new Rectangle(left + 3 * (column + gap), top + 1 * row, column, row * 3 + fontHeight),
                InterfaceElement.COLOR_NORMAL,
                Alignment.TOP_CENTER));
            Interface.Add(new InterfaceImage(
                "ButtonImages/xboxControllerButtonA",
                new Rectangle(left + 3 * (column + gap), top + 5 * row, column, fontHeight),
                InterfaceElement.COLOR_NORMAL,
                Alignment.TOP_CENTER));
            Interface.Add(new InterfaceImage(
                "ButtonImages/xboxControllerButtonB",
                new Rectangle(left + 3 * (column + gap), top + 6 * row, column, fontHeight),
                InterfaceElement.COLOR_NORMAL,
                Alignment.TOP_CENTER));

            // back button
            string label = "Back to Menu";
            Interface.Add(new InterfaceButton(label, new Vector2(-(int)(menu.Font.MeasureString(label).X / 2), 100), () => { return true; }, Alignment.BOTTOM_CENTER));
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            menu.BackToMainMenu(gameTime);

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);
        }
    }
}
