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
            CONTROLS,
            OPTIONS,
            VIRUSES,
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
            logo = content.Load<Texture2D>("logoNew");
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
                SimpleButton.Instance.ChangeHappened(gameTime, menu.SoundEffect);
            selectedButton = (Button)(selectionInt);

            // button selected
            if (InputManager.Instance.PressedButton(Buttons.Start))
            {
                menu.ChangePage(Menu.Page.NEWGAME, gameTime);
            }
            else if (InputManager.Instance.ContinueButton())
            {
                switch (selectedButton)
                {
                    case Button.NEWGAME:
                        menu.ChangePage(Menu.Page.NEWGAME, gameTime);
                        break;

                    case Button.CONTROLS:
                        menu.ChangePage(Menu.Page.CONTROLS, gameTime);
                        break;

                    case Button.OPTIONS:
                        menu.ChangePage(Menu.Page.OPTIONS, gameTime);
                        break;

                    case Button.VIRUSES:
                        menu.ChangePage(Menu.Page.VIRUSES, gameTime);
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

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(logo, new Rectangle(50, 50, logo.Width,logo.Height), Color.White);

            SimpleButton.Instance.Draw(spriteBatch, menu.FontHeading, "New Game", new Vector2(100, 370), selectedButton == Button.NEWGAME, menu.TexPixel);
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Controls", new Vector2(100, 440), selectedButton == Button.CONTROLS, menu.TexPixel);
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Options", new Vector2(100, 500), selectedButton == Button.OPTIONS, menu.TexPixel);
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Viruses", new Vector2(100, 560), selectedButton == Button.VIRUSES, menu.TexPixel);
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Credits", new Vector2(100, 620), selectedButton == Button.CREDITS, menu.TexPixel);
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Exit Game", new Vector2(100, 680), selectedButton == Button.END, menu.TexPixel);

            SimpleButton.Instance.Draw(spriteBatch, menu.Font, ParticleStormControl.VERSION, new Vector2(menu.ScreenWidth - SimpleButton.PADDING, menu.ScreenHeight - SimpleButton.PADDING) - menu.Font.MeasureString(ParticleStormControl.VERSION), false, menu.TexPixel);
        }
    }
}
