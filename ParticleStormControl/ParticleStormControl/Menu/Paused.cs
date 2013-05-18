using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParticleStormControl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace ParticleStormControl.Menu
{
    class Paused : MenuPage
    {
        enum Buttons
        {
            CONTINUE,
            QUIT_TO_MAINMENU,
        }
        Buttons currentButton = Buttons.CONTINUE;


        List<int> reconnectWaitingPlayerIndices = new List<int>();

        /// <summary>
        /// player that controlls this menu
        /// </summary>
        public int ControllingPlayer { get; set; }

        public Paused(Menu menu)
            : base(menu)
        {
            // background
            Interface.Add(new InterfaceFiller(Vector2.Zero, menu.ScreenWidth, menu.ScreenHeight, Color.Black * 0.5f, () => { return true; }));

            // paused string
            string label = "game paused";
            Vector2 stringSizePaused = menu.FontHeading.MeasureString(label);
            Vector2 positionPaused = (new Vector2(menu.ScreenWidth, menu.ScreenHeight - 300) - stringSizePaused) / 2;
            Interface.Add(new InterfaceButton(label, positionPaused, true));

            // continue & mainmenu
            const int BUTTON_WIDTH = 150;
            float y = positionPaused.Y + 180;
            Interface.Add(new InterfaceButton("► Continue", new Vector2((menu.ScreenWidth) / 2 - 20 - BUTTON_WIDTH, y), () => { return currentButton == Buttons.CONTINUE; }, BUTTON_WIDTH));
            Interface.Add(new InterfaceButton("► Quit to Menu", new Vector2(menu.ScreenWidth / 2 + 20, y), () => { return currentButton == Buttons.QUIT_TO_MAINMENU; }, BUTTON_WIDTH));
            y += 100;

            // disconnected message
            foreach (int playerIndex in reconnectWaitingPlayerIndices)
            {
                string message = "Player " + (playerIndex + 1) + " is disconnected!";
                Vector2 position = (new Vector2(menu.ScreenWidth / 2, y) - menu.Font.MeasureString(message) / 2);
                Interface.Add(new InterfaceButton(message, position));
                y += 50;
            }
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);
        }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            base.OnActivated(oldPage, gameTime);

            // who controlls if nobody pressed?
            if (InputManager.Instance.IsWaitingForReconnect())
            {
                ControllingPlayer = 0;
                while (ControllingPlayer < Settings.Instance.NumPlayers &&
                    InputManager.Instance.IsWaitingForReconnect(Settings.Instance.GetPlayer(ControllingPlayer).ControlType))
                {
                    ++ControllingPlayer;
                }
                if (ControllingPlayer >= Settings.Instance.NumPlayers)
                    ControllingPlayer = 0;
            }
        }

        public override void Update(GameTime gameTime)
        {
            // if keyboard, anybody is allowed!
            int controllerBefore = ControllingPlayer;
            List<int> controls = new List<int>();
            if (InputManager.IsKeyboardControlType(Settings.Instance.GetPlayer(ControllingPlayer).ControlType))
            {
                for (int i = 0; i < Settings.Instance.NumPlayers; ++i)
                {
                    if (InputManager.IsKeyboardControlType(Settings.Instance.GetPlayer(i).ControlType))
                        controls.Add(i);
                }
            }
            else
                controls.Add(ControllingPlayer);
            foreach(int controllerIndex in controls)
            {
                // back to game
                if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.PAUSE, controllerIndex))
                {
                    InputManager.Instance.ResetWaitingForReconnect();
                    menu.ChangePage(Menu.Page.INGAME, gameTime);
                }

                // back to menu
                if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.EXIT, controllerIndex))
                {
                    InputManager.Instance.ResetWaitingForReconnect();
                    menu.ChangePage(Menu.Page.MAINMENU, gameTime);
                }

                // shutdown per (any) pad
                //   if (InputManager.Instance.PressedButton(Buttons.Back) || InputManager.Instance.PressedButton(Keys.Escape))
                //      menu.ChangePage(Menu.Page.MAINMENU, gameTime);

                // selected?
                if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.ACTION, controllerIndex))
                {
                    InputManager.Instance.ResetWaitingForReconnect();
                    if (currentButton == Buttons.CONTINUE)
                        menu.ChangePage(Menu.Page.INGAME, gameTime);
                    else if (currentButton == Buttons.QUIT_TO_MAINMENU)
                        menu.ChangePage(Menu.Page.MAINMENU, gameTime);
                }

                // changed option
                if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.RIGHT, controllerIndex) ||
                    InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.LEFT, controllerIndex))
                    currentButton = currentButton == Buttons.CONTINUE ? Buttons.QUIT_TO_MAINMENU : Buttons.CONTINUE;
            }


            // unconnected players?
            reconnectWaitingPlayerIndices.Clear();
            for (int i = 0; i < Settings.Instance.NumPlayers; ++i)
            {
                if (InputManager.Instance.IsWaitingForReconnect(Settings.Instance.GetPlayer(i).ControlType))
                    reconnectWaitingPlayerIndices.Add(i);
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);
        }
    }
}
