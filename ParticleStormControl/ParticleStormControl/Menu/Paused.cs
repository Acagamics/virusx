using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParticleStormControl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
        { }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
        }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            base.OnActivated(oldPage, gameTime);

            // who controlls if nobody pressed?
            if (InputManager.Instance.IsWaitingForReconnect())
            {
                ControllingPlayer = 0;
                while (ControllingPlayer < Settings.Instance.NumPlayers &&
                    InputManager.Instance.IsWaitingForReconnect(Settings.Instance.PlayerControls[ControllingPlayer]))
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
            bool otherKeyboard = false;
            int controllerBefore = ControllingPlayer;
            if(Settings.Instance.PlayerControls[ControllingPlayer] == InputManager.ControlType.KEYBOARD0)
            {
                int i = Array.IndexOf(Settings.Instance.PlayerControls, InputManager.ControlType.KEYBOARD1);
                if (i >= 0)
                {
                    otherKeyboard = true;
                    ControllingPlayer = i;
                }
            }
            else if(Settings.Instance.PlayerControls[ControllingPlayer] == InputManager.ControlType.KEYBOARD1)
            {
                int i = Array.IndexOf(Settings.Instance.PlayerControls, InputManager.ControlType.KEYBOARD0);
                if (i >= 0)
                {
                    otherKeyboard = true;
                    ControllingPlayer = i;
                }
            }
            do
            {
                // back to game
                if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.PAUSE, ControllingPlayer))
                {
                    InputManager.Instance.ResetWaitingForReconnect();
                    menu.ChangePage(Menu.Page.INGAME, gameTime);
                }
                // back to menu
                if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.EXIT, ControllingPlayer))
                {
                    InputManager.Instance.ResetWaitingForReconnect();
                    menu.ChangePage(Menu.Page.MAINMENU, gameTime);
                }

                // shutdown per (any) pad
                //   if (InputManager.Instance.PressedButton(Buttons.Back) || InputManager.Instance.PressedButton(Keys.Escape))
                //      menu.ChangePage(Menu.Page.MAINMENU, gameTime);

                // selected?
                if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.ACTION, ControllingPlayer))
                {
                    InputManager.Instance.ResetWaitingForReconnect();
                    if (currentButton == Buttons.CONTINUE)
                        menu.ChangePage(Menu.Page.INGAME, gameTime);
                    else if (currentButton == Buttons.QUIT_TO_MAINMENU)
                        menu.ChangePage(Menu.Page.MAINMENU, gameTime);
                }

                // changed option
                if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.RIGHT, ControllingPlayer) ||
                    InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.LEFT, ControllingPlayer))
                    currentButton = currentButton == Buttons.CONTINUE ? Buttons.QUIT_TO_MAINMENU : Buttons.CONTINUE;


                otherKeyboard = ControllingPlayer != controllerBefore;
                ControllingPlayer = controllerBefore;
            } while (otherKeyboard);


            // unconnected players?
            reconnectWaitingPlayerIndices.Clear();
            for (int i = 0; i < Settings.Instance.NumPlayers; ++i)
            {
                if (InputManager.Instance.IsWaitingForReconnect(Settings.Instance.PlayerControls[i]))
                    reconnectWaitingPlayerIndices.Add(i);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // background
            spriteBatch.Draw(menu.TexPixel, new Rectangle(0, 0, menu.ScreenWidth, menu.ScreenHeight), Color.Black * 0.5f);

            // paused string
            string label = "game paused";
            Vector2 stringSizePaused = menu.FontCountdown.MeasureString(label);
            Vector2 positionPaused = (new Vector2(menu.ScreenWidth, menu.ScreenHeight - 300) - stringSizePaused) / 2;
            spriteBatch.DrawString(menu.FontCountdown, label, positionPaused, Color.White);

            // continue & mainmenu
            const int BUTTON_WIDTH = 150;
            float y = positionPaused.Y + 180;
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Continue", new Vector2((menu.ScreenWidth) / 2 - 20 - BUTTON_WIDTH, y), currentButton == Buttons.CONTINUE, menu.TexPixel, BUTTON_WIDTH);
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Quit to Menu", new Vector2(menu.ScreenWidth / 2 + 20, y), currentButton == Buttons.QUIT_TO_MAINMENU, menu.TexPixel, BUTTON_WIDTH);
            y += 100;

            // disconnected message
            foreach(int playerIndex in reconnectWaitingPlayerIndices)
            {
                string message = "Player " + (playerIndex + 1) + " is disconnected!";
                Vector2 position = (new Vector2(menu.ScreenWidth / 2, y) - menu.Font.MeasureString(message) / 2);
                SimpleButton.Instance.Draw(spriteBatch, menu.Font, message, position, Settings.Instance.GetPlayerColor(playerIndex), menu.TexPixel);
                y += 50;
            }
        }
    }
}
