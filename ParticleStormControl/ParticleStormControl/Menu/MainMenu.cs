using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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

        public override void LoadContent(ContentManager content)
        {
            logo = content.Load<Texture2D>("logo");
        }

        public override void Update(GameTime gameTime)
        {
            // loopin
            int selectionInt = (int)selectedButton;
            if (InputManager.Instance.AnyDownButtonPressed())
                selectionInt = selectionInt == (int)Button.NUM_BUTTONS-1 ? 0 : selectionInt + 1;
            else if (InputManager.Instance.AnyUpButtonPressed())
                selectionInt = selectionInt == 0 ? (int)Button.NUM_BUTTONS - 1 : selectionInt - 1;
            if (selectionInt != (int)selectedButton)
                SimpleButton.Instance.ChangeHappened(gameTime);
            selectedButton = (Button)(selectionInt);

            // button selected
            if (InputManager.Instance.PressedButton(Buttons.Start))
            {
                menu.ChangePage(Menu.Page.NEWGAME, gameTime);
            }
            else if (InputManager.Instance.ExitButton())
            {
                menu.Exit();
            }
            else if (InputManager.Instance.ContinueButton())
            {
                switch (selectedButton)
                {
                    case Button.NEWGAME:
                        menu.ChangePage(Menu.Page.NEWGAME, gameTime);
                        break;

                    case Button.OPTIONS:
                        menu.ChangePage(Menu.Page.OPTIONS, gameTime);
                        break;

                    case Button.CREDITS:
                        menu.ChangePage(Menu.Page.CREDITS, gameTime);
                        break;

                    case Button.END:
                        menu.Exit();
                        break;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float timeInterval)
        {
            //SimpleButton.Instance.Draw(spriteBatch, menu.FontHeading, "< insert logo here >", new Vector2(100, 100), false, menu.PixelTexture);
            spriteBatch.Draw(logo, new Rectangle(menu.ScreenWidth - (int)(logo.Width*1.2f),(int)(logo.Height*0.2f), logo.Width,logo.Height), Color.White);
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "New Game", new Vector2(100, 220), selectedButton == Button.NEWGAME, menu.PixelTexture);
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Options", new Vector2(100, 280), selectedButton == Button.OPTIONS, menu.PixelTexture);
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Credits", new Vector2(100, 340), selectedButton == Button.CREDITS, menu.PixelTexture);
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Exit Game", new Vector2(100, 400), selectedButton == Button.END, menu.PixelTexture);

            spriteBatch.DrawString(menu.Font, ParticleStormControl.VERSION, new Vector2(menu.ScreenWidth - 200, menu.ScreenHeight - 50), Color.Black);
        }
    }
}
