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
            Interface.Add(new InterfaceImage("logoNew", new Vector2(50, 50)));

            Interface.Add(new InterfaceButton("New Game", new Vector2(100, 370), () => { return selectedButton == Button.NEWGAME; }, true));
            Interface.Add(new InterfaceButton("Controls", new Vector2(100, 440), () => { return selectedButton == Button.CONTROLS; }));
            Interface.Add(new InterfaceButton("Options", new Vector2(100, 500), () => { return selectedButton == Button.OPTIONS; }));
            Interface.Add(new InterfaceButton("Viruses", new Vector2(100, 560), () => { return selectedButton == Button.VIRUSES; }));
            Interface.Add(new InterfaceButton("Credits", new Vector2(100, 620), () => { return selectedButton == Button.CREDITS; }));
            Interface.Add(new InterfaceButton("Exit Game", new Vector2(100, 680), () => { return selectedButton == Button.END; }));

            Interface.Add(new InterfaceButton("How to Play", new Vector2(620, 100), Alignment.TOP_RIGHT));
            Interface.Add(new InterfaceImage("instructions", new Vector2(620, 100 + menu.GetFontHeight() + 2 * InterfaceElement.PADDING), Alignment.TOP_RIGHT));

            Interface.Add(new InterfaceButton(ParticleStormControl.VERSION, new Vector2(2 * InterfaceElement.PADDING, 2 * InterfaceElement.PADDING) + menu.Font.MeasureString(ParticleStormControl.VERSION), Alignment.BOTTOM_RIGHT));
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {

            // loopin
            selectedButton = (Button)(Menu.LoopEnum((int)selectedButton, (int)Button.NUM_BUTTONS));

            // button selected
            if (InputManager.Instance.AnyPressedButton(Buttons.Start))
            {
                menu.ChangePage(Menu.Page.NEWGAME, gameTime);
            }
            else
            {
                // as manual loop for identifying the used controls
                foreach (InputManager.ControlType control in Enum.GetValues(typeof(InputManager.ControlType)))
                {
                    if(InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.ACTION, control))
                    {
                        switch (selectedButton)
                        {
                            case Button.NEWGAME:
                                ((NewGame)menu.GetPage(Menu.Page.NEWGAME)).StartingControls = control;
                               // ((Mode)menu.GetPage(Menu.Page.MODE)).StartingControls = control;
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
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);
        }
    }
}
