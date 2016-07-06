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
            Interface.Add(new InterfaceFiller(Vector2.Zero, Color.Black * 0.5f, () => { return origin == Menu.Page.INGAME || origin == Menu.Page.NEWGAME; }));

            string[,] data = {
                                 { null,                 VirusXStrings.Instance.ControlKeyboard1,   VirusXStrings.Instance.ControlKeyboard2,  VirusXStrings.Instance.ControlKeyboard3,   VirusXStrings.Instance.ControlGamepad },
                                 { VirusXStrings.Instance.ControlMoveUp,                 "W",           VirusXStrings.Instance.ControlArrowUp,    VirusXStrings.Instance.ControlNumpad + " 8",     null },
                                 { VirusXStrings.Instance.ControlMoveLeft,               "A",           VirusXStrings.Instance.ControlArrowLeft,  VirusXStrings.Instance.ControlNumpad + " 4",     null },
                                 { VirusXStrings.Instance.ControlMoveDown,               "S",           VirusXStrings.Instance.ControlArrowDown,   VirusXStrings.Instance.ControlNumpad + " 5/2",    null },
                                 { VirusXStrings.Instance.ControlMoveRight,              "D",           VirusXStrings.Instance.ControlArrowRight,  VirusXStrings.Instance.ControlNumpad + " 6",    null },
                                 { VirusXStrings.Instance.ControlActionUse,  "Tab",        "Enter",        VirusXStrings.Instance.ControlNumpad + " 0",    null },
                                 { VirusXStrings.Instance.ControlBackHold, "L-Shift",       "R-Shift",        VirusXStrings.Instance.ControlNumpad + " 7/9",    null },
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
            string label = VirusXStrings.Instance.MenuBack;
            Interface.Add(new InterfaceButton(label, new Vector2(-(int)(menu.Font.MeasureString(label).X / 2), 100), () => { return true; }, Alignment.BOTTOM_CENTER));
        }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            origin = oldPage;
            base.Update(gameTime);  // reduces flicker
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
