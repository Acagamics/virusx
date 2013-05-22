﻿using System;
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
            CONTROLS,
            OPTIONS,
            VIRUSES,
            CREDITS,
            END,

            NUM_BUTTONS
        };
        enum ButtonSubmenu
        {
            MODE,
            ITEMS,
            CONTINUE,

            NUM_BUTTONS
        };
        Button selectedButton = Button.NEWGAME;
        ButtonSubmenu selectedButtonSubmenu = ButtonSubmenu.MODE;
        
        InterfaceButton useItemsButton;

        bool submenuVisible;
        public bool SubmenuVisible { get { return submenuVisible; } }

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

        //    Interface.Add(new InterfaceButton("How to Play", new Vector2(620, 100), Alignment.TOP_RIGHT));
        //    Interface.Add(new InterfaceImage("instructions", new Vector2(620, 100 + menu.GetFontHeight() + 2 * InterfaceElement.PADDING), Alignment.TOP_RIGHT));

            // submenu
            Interface.Add(new InterfaceButton("Game mode", new Vector2(320, 370), () => { return selectedButtonSubmenu == ButtonSubmenu.MODE; }, () => submenuVisible));
            Interface.Add(new InterfaceButton(() => { return "◄ " + Game.GAMEMODE_NAME[(int)Settings.Instance.GameMode] + " ►"; }, new Vector2(480, 370), () => { return false; }, () => submenuVisible));
            Interface.Add(new InterfaceButton("Items", new Vector2(320, 430), () => { return selectedButtonSubmenu == ButtonSubmenu.ITEMS; }, () => submenuVisible));
            useItemsButton = new InterfaceButton(() => { return Settings.Instance.UseItems ? "◄ ON ►" : "◄ OFF ►"; }, new Vector2(480, 430), () => { return false; }, () => submenuVisible, Color.White, Settings.Instance.UseItems ? Color.Green : Color.Red);
            Interface.Add(useItemsButton);
            Interface.Add(new InterfaceButton("► Start", new Vector2(320, 490), () => { return selectedButtonSubmenu == ButtonSubmenu.CONTINUE; }, () => submenuVisible));

            Interface.Add(new InterfaceButton(ParticleStormControl.VERSION, new Vector2(2 * InterfaceElement.PADDING, 2 * InterfaceElement.PADDING) + menu.Font.MeasureString(ParticleStormControl.VERSION), Alignment.BOTTOM_RIGHT));
        }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            if (oldPage == Menu.Page.STATS)
                submenuVisible = false;
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {

            // loopin
            if(submenuVisible)
                selectedButtonSubmenu = (ButtonSubmenu)(Menu.Loop((int)selectedButtonSubmenu, (int)ButtonSubmenu.NUM_BUTTONS, Settings.Instance.StartingControls));
            else
                selectedButton = (Button)(Menu.Loop((int)selectedButton, (int)Button.NUM_BUTTONS));

            if (submenuVisible)
            {
                switch (selectedButtonSubmenu)
                {
                    case ButtonSubmenu.MODE:
                        Settings.Instance.GameMode = (Game.GameMode)Menu.Loop((int)Settings.Instance.GameMode, (int)Game.GameMode.NUM_MODES, Settings.Instance.StartingControls, true);
                        break;
                    case ButtonSubmenu.ITEMS:
                        Settings.Instance.UseItems = Menu.Toggle(Settings.Instance.UseItems, Settings.Instance.StartingControls);
                        break;
                    case ButtonSubmenu.CONTINUE:
                        if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.ACTION, Settings.Instance.StartingControls))
                            menu.ChangePage(Menu.Page.NEWGAME, gameTime);
                        break;
                }

                if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.EXIT, Settings.Instance.StartingControls)
                    || InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.HOLD, Settings.Instance.StartingControls))
                {
                    submenuVisible = false;
                    AudioManager.Instance.PlaySoundeffect("click");
                }
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
                                Settings.Instance.StartingControls = control;
                                submenuVisible = true;
                                AudioManager.Instance.PlaySoundeffect("click");
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
