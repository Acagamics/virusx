using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace VirusX.Menu
{
    class Controls : MenuPage
    {
        Menu.Page origin;

        public Controls(Menu menu)
            : base(menu)
        {
            // background
            Interface.Add(new InterfaceFiller(Vector2.Zero, menu.ScreenWidth, menu.ScreenHeight, Color.Black * 0.5f, () => { return origin == Menu.Page.INGAME; }));

            string[,] data = {
                                 { null,                 "Keyboard 1",   "Keyboard 2",  "Keyboard 3",   "Gamepad" },
                                 { "Up",                 "W",            "Arrow Up",    "Numpad 8",     null },
                                 { "Left",               "A",            "Arrow Left",  "Numpad 4",     null },
                                 { "Down",               "S",            "Arrow Down",   "Numpad 5/2",    null },
                                 { "Right",              "D",            "Arrow Right",  "Numpad 6",    null },
                                 { "Action / Use Item",  "Space",        "Enter",        "Numpad 0",    null },
                                 { "Back / Hold Cursor", "V",            "Shift",        "Numpad 7/9",    null },
                             };

            // big table
            int column = 190;   // column width
            int row = 60;       // row height
            int gap = 15;       // gap between columns
            int top = 100;       // distance from top
            int left = -(data.GetLength(1) * (column + gap) - gap + InterfaceButton.PADDING) / 2;

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
            int width = column + InterfaceImage.PADDING/2;
            Interface.Add(new InterfaceImage(
                "ButtonImages/xboxControllerLeftThumbstick",
                new Rectangle(left + 4 * (column + gap), top + 1 * row, width, row * 3 + fontHeight),
                InterfaceElement.COLOR_NORMAL,
                Alignment.TOP_CENTER));
            Interface.Add(new InterfaceImage(
                "ButtonImages/xboxControllerButtonA",
                new Rectangle(left + 4 * (column + gap), top + 5 * row, width, fontHeight),
                InterfaceElement.COLOR_NORMAL,
                Alignment.TOP_CENTER));
            Interface.Add(new InterfaceImage(
                "ButtonImages/xboxControllerButtonB",
                new Rectangle(left + 4 * (column + gap), top + 6 * row, width, fontHeight),
                InterfaceElement.COLOR_NORMAL,
                Alignment.TOP_CENTER));

            // back button
            string label = "► Back";
            Interface.Add(new InterfaceButton(label, new Vector2(-(int)(menu.Font.MeasureString(label).X / 2), 100), () => { return true; }, Alignment.BOTTOM_CENTER));
        }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            origin = oldPage;
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.PAUSE)
                || InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.EXIT)
                || InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.ACTION)
                || InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.HOLD)
                || InputManager.Instance.IsButtonPressed(Keys.F1)
                || InputManager.Instance.AnyPressedButton(Buttons.Y))
                menu.ChangePage(origin, gameTime);

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);
        }
    }
}
