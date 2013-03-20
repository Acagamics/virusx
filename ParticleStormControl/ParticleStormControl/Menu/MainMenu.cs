using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace ParticleStormControl.Menu
{
    class MainMenu : MenuPage
    {
        Texture2D logo;

        enum Button
        {
            NEWGAME,
            OPTIONS,
            CREDITS,
            END,

            NUM_BUTTONS
        };
        Button selectedButton = Button.NEWGAME;

        public MainMenu(Menu menu)
            : base(menu)
        {
        }

        public override void Initialize()
        {
        }

        public override void LoadContent(ContentManager content)
        {
            logo = content.Load<Texture2D>("logo");
        }

        public override void Update(float frameTimeInterval)
        {
            // loopin
            int selectionInt = (int)selectedButton;
            if (InputManager.Instance.AnyDownButtonPressed())
                selectionInt = selectionInt == (int)Button.NUM_BUTTONS-1 ? 0 : selectionInt + 1;
            else if (InputManager.Instance.AnyUpButtonPressed())
                selectionInt = selectionInt == 0 ? (int)Button.NUM_BUTTONS - 1 : selectionInt - 1;
            selectedButton = (Button)(selectionInt);

            // button selected
            if (InputManager.Instance.ContinueButton())
            {
                switch (selectedButton)
                {
                    case Button.NEWGAME:
                        menu.ChangePage(Menu.Page.NEWGAME);
                        break;

                    case Button.OPTIONS:
                        menu.ChangePage(Menu.Page.OPTIONS);
                        break;

                    case Button.CREDITS:
                        menu.ChangePage(Menu.Page.CREDITS);
                        break;

                    case Button.END:
                        menu.Exit();
                        break;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float timeInterval)
        {
            Vector2 screenMid = new Vector2(menu.ScreenWidth / 2, menu.ScreenHeight / 2);

            SimpleButton.Draw(spriteBatch, menu.Font, "New Game", screenMid + new Vector2(0, -75), selectedButton == Button.NEWGAME);
            SimpleButton.Draw(spriteBatch, menu.Font, "Options", screenMid + new Vector2(0, -25), selectedButton == Button.OPTIONS);
            SimpleButton.Draw(spriteBatch, menu.Font, "Credits", screenMid + new Vector2(0, 25), selectedButton == Button.CREDITS);
            SimpleButton.Draw(spriteBatch, menu.Font, "Exit", screenMid + new Vector2(0, 75), selectedButton == Button.END);

            spriteBatch.DrawString(menu.FontSmall, ParticleStormControl.VERSION, new Vector2(menu.ScreenWidth - 200, menu.ScreenHeight - 50), Color.Black);
        }
    }
}
