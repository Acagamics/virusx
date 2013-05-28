using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Game = global::VirusX.InGame;

namespace VirusX.Menu
{
    class MainMenu : MenuPage
    {
        enum Button
        {
            NEWGAME,
            HELP,
            OPTIONS,
            END,

            NUM_BUTTONS
        };
        enum ButtonNewGameSubmenu
        {
            MODE,
            ITEMS,
            CONTINUE,

            NUM_BUTTONS
        };
        enum ButtonHelpSubmenu
        {
            GAMEPLAY,
            CONTROLS,
            VIRUSES,
            CREDITS,

            NUM_BUTTONS
        };
        public enum SubMenu
        {
            NONE,
            NEWGAME,
            HELP
        };
        Button selectedButton = Button.NEWGAME;
        ButtonNewGameSubmenu selectedButton_NewGameSubmenu = ButtonNewGameSubmenu.MODE;
        ButtonHelpSubmenu selectedButton_HelpSubmenu = ButtonHelpSubmenu.GAMEPLAY;
        
        InterfaceButton useItemsButton;

        SubMenu submenuVisible;
        public SubMenu SubmenuVisible { get { return submenuVisible; } }

        public MainMenu(Menu menu)
            : base(menu)
        {
            Interface.Add(new InterfaceImage("logoNew", new Vector2(50, 50)));

            Interface.Add(new InterfaceButton("New Game", new Vector2(100, 370), () => { return selectedButton == Button.NEWGAME; }, true));
            Interface.Add(new InterfaceButton("Help", new Vector2(100, 440), () => { return selectedButton == Button.HELP; }));
            Interface.Add(new InterfaceButton("Options", new Vector2(100, 500), () => { return selectedButton == Button.OPTIONS; }));
        //    Interface.Add(new InterfaceButton("Credits", new Vector2(100, 560), () => { return selectedButton == Button.CREDITS; }));
            Interface.Add(new InterfaceButton("Exit Game", new Vector2(100, 560), () => { return selectedButton == Button.END; }));

        //    Interface.Add(new InterfaceButton("How to Play", new Vector2(620, 100), Alignment.TOP_RIGHT));
        //    Interface.Add(new InterfaceImage("instructions", new Vector2(620, 100 + menu.GetFontHeight() + 2 * InterfaceElement.PADDING), Alignment.TOP_RIGHT));

            // submenu newgame
            Interface.Add(new InterfaceButton("Game mode", new Vector2(320, 370), () => { return selectedButton_NewGameSubmenu == ButtonNewGameSubmenu.MODE; }, () => submenuVisible == SubMenu.NEWGAME));
            Interface.Add(new InterfaceButton(() => { return "◄ " + Game.GAMEMODE_NAME[(int)Settings.Instance.GameMode] + " ►"; }, new Vector2(480, 370), () => { return false; }, () => submenuVisible == SubMenu.NEWGAME));
            Interface.Add(new InterfaceButton("Items", new Vector2(320, 420), () => { return selectedButton_NewGameSubmenu == ButtonNewGameSubmenu.ITEMS; }, () => submenuVisible == SubMenu.NEWGAME));
            useItemsButton = new InterfaceButton(() => { return Settings.Instance.UseItems ? "◄ ON ►" : "◄ OFF ►"; }, new Vector2(480, 420), () => { return false; }, () => submenuVisible == SubMenu.NEWGAME, Color.White, Settings.Instance.UseItems ? Color.Green : Color.Red);
            Interface.Add(useItemsButton);
            Interface.Add(new InterfaceButton("► Start", new Vector2(320, 470), () => { return selectedButton_NewGameSubmenu == ButtonNewGameSubmenu.CONTINUE; }, () => submenuVisible == SubMenu.NEWGAME));

            // submenu 
            Interface.Add(new InterfaceButton("► Gameplay", new Vector2(320, 370), () => { return selectedButton_HelpSubmenu == ButtonHelpSubmenu.GAMEPLAY; }, () => submenuVisible == SubMenu.HELP));
            Interface.Add(new InterfaceButton("► Controls", new Vector2(320, 420), () => { return selectedButton_HelpSubmenu == ButtonHelpSubmenu.CONTROLS; }, () => submenuVisible == SubMenu.HELP));
            Interface.Add(new InterfaceButton("► Viruses", new Vector2(320, 470), () => { return selectedButton_HelpSubmenu == ButtonHelpSubmenu.VIRUSES; }, () => submenuVisible == SubMenu.HELP));
            Interface.Add(new InterfaceButton("► Credits", new Vector2(320, 520), () => { return selectedButton_HelpSubmenu == ButtonHelpSubmenu.CREDITS; }, () => submenuVisible == SubMenu.HELP));

            Interface.Add(new InterfaceButton(ParticleStormControl.VERSION, new Vector2(2 * InterfaceElement.PADDING, 2 * InterfaceElement.PADDING) + menu.Font.MeasureString(ParticleStormControl.VERSION), Alignment.BOTTOM_RIGHT));
        }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            if (oldPage == Menu.Page.STATS)
                submenuVisible = SubMenu.NONE;
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {

            // loopin
            switch(submenuVisible)
            {
                case SubMenu.NONE:
                    selectedButton = (Button)(Menu.Loop((int)selectedButton, (int)Button.NUM_BUTTONS));
                    
                    // as manual loop for identifying the used controls
                    foreach (InputManager.ControlType control in Enum.GetValues(typeof(InputManager.ControlType)))
                    {
                        // if an back or escape button is pressed then jump to exit
                        if(InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.EXIT, control))
                        {
                            // if alrady on exit, quit the game
                            if (selectedButton == Button.END)
                                menu.Exit();
                            AudioManager.Instance.PlaySoundeffect("click");
                            selectedButton = Button.END;
                        }

                        if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.ACTION, control))
                        {
                            switch (selectedButton)
                            {
                                case Button.NEWGAME:
                                    Settings.Instance.StartingControls = control;
                                    submenuVisible = SubMenu.NEWGAME;
                                    AudioManager.Instance.PlaySoundeffect("click");
                                    break;

                                case Button.HELP:
                                    Settings.Instance.StartingControls = control;
                                    submenuVisible = SubMenu.HELP;
                                    AudioManager.Instance.PlaySoundeffect("click");
                                    break;

                                case Button.OPTIONS:
                                    menu.ChangePage(Menu.Page.OPTIONS, gameTime);
                                    break;

                                case Button.END:
                                    menu.Exit();
                                    break;
                            }
                        }
                    }
                    break;

                case SubMenu.NEWGAME:
                    selectedButton_NewGameSubmenu = (ButtonNewGameSubmenu)(Menu.Loop((int)selectedButton_NewGameSubmenu, (int)ButtonNewGameSubmenu.NUM_BUTTONS, Settings.Instance.StartingControls));
                    switch (selectedButton_NewGameSubmenu)
                    {
                        case ButtonNewGameSubmenu.MODE:
                            Settings.Instance.GameMode = (Game.GameMode)Menu.Loop((int)Settings.Instance.GameMode, (int)Game.GameMode.NUM_MODES, Settings.Instance.StartingControls, true);
                            // if action butten is pressed jump to the continue button
                            if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.ACTION, Settings.Instance.StartingControls))
                                selectedButton_NewGameSubmenu = ButtonNewGameSubmenu.CONTINUE;
                            break;
                        case ButtonNewGameSubmenu.ITEMS:
                            Settings.Instance.UseItems = Menu.Toggle(Settings.Instance.UseItems, Settings.Instance.StartingControls);
                            //if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.ACTION, Settings.Instance.StartingControls))
                            //    selectedButton_NewGameSubmenu = ButtonNewGameSubmenu.CONTINUE;
                            break;
                        case ButtonNewGameSubmenu.CONTINUE:
                            if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.ACTION, Settings.Instance.StartingControls))
                                menu.ChangePage(Menu.Page.NEWGAME, gameTime);
                            break;
                    }
                    break;

                case SubMenu.HELP:
                    selectedButton_HelpSubmenu = (ButtonHelpSubmenu)(Menu.Loop((int)selectedButton_HelpSubmenu, (int)ButtonHelpSubmenu.NUM_BUTTONS, Settings.Instance.StartingControls));
                    if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.ACTION, Settings.Instance.StartingControls))
                    {
                        switch (selectedButton_HelpSubmenu)
                        {
                            case ButtonHelpSubmenu.CONTROLS:
                                menu.ChangePage(Menu.Page.CONTROLS, gameTime);
                                break;
                            case ButtonHelpSubmenu.VIRUSES:
                                menu.ChangePage(Menu.Page.VIRUSES, gameTime);
                                break;
                            case ButtonHelpSubmenu.GAMEPLAY:
                                //menu.ChangePage(Menu.Page, gameTime);
                                break;
                            case ButtonHelpSubmenu.CREDITS:
                                menu.ChangePage(Menu.Page.CREDITS, gameTime);
                                break;
                        }
                    }
                    break;
            }

            // cancel submenu
            if (submenuVisible != SubMenu.NONE)
            {
                if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.EXIT, Settings.Instance.StartingControls)
                    || InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.HOLD, Settings.Instance.StartingControls))
                {
                    submenuVisible = SubMenu.NONE;
                    AudioManager.Instance.PlaySoundeffect("click");
                }
            }

            // change background colors
            useItemsButton.BackgroundColor = Settings.Instance.UseItems ? Color.Green : Color.Red;

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);
        }
    }
}
