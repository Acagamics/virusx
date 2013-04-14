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

        public Paused(Menu menu)
            : base(menu)
        { }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
        }

        public override void Update(GameTime gameTime)
        {
            // back to game
            if (InputManager.Instance.WasPauseButtonPressed())
            {
                // reset wait for reconnect settings - not connected pads will now be ignored
                InputManager.Instance.ResetWaitingForReconnect();
                menu.ChangePage(Menu.Page.INGAME, gameTime);
            }

            // shutdown per (any) pad
         //   if (InputManager.Instance.PressedButton(Buttons.Back) || InputManager.Instance.PressedButton(Keys.Escape))
          //      menu.ChangePage(Menu.Page.MAINMENU, gameTime);

            // selected?
            if (InputManager.Instance.WasContinueButtonPressed())
            {
                InputManager.Instance.ResetWaitingForReconnect();
                if(currentButton == Buttons.CONTINUE)
                    menu.ChangePage(Menu.Page.INGAME, gameTime);
                else if (currentButton == Buttons.QUIT_TO_MAINMENU)
                    menu.ChangePage(Menu.Page.MAINMENU, gameTime);
            }

            // changed option
            if (InputManager.Instance.AnyRightButtonPressed() || InputManager.Instance.AnyLeftButtonPressed())
                currentButton = currentButton == Buttons.CONTINUE ? Buttons.QUIT_TO_MAINMENU : Buttons.CONTINUE;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // background
            spriteBatch.Draw(menu.TexPixel, new Rectangle(0, 0, menu.ScreenWidth, menu.ScreenHeight), Color.Black * 0.5f);

            // paused string
            Vector2 stringSizePaused = menu.FontHeading.MeasureString("PAUSED");
            Vector2 positionPaused = (new Vector2(menu.ScreenWidth, menu.ScreenHeight-200) - stringSizePaused) / 2;
            spriteBatch.DrawString(menu.FontHeading, "PAUSED", positionPaused, Color.White);

            // continue & mainmenu
            const int BUTTON_WIDTH = 250;
            float y = positionPaused.Y + 50;
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Continue", new Vector2((menu.ScreenWidth - BUTTON_WIDTH) / 2 - 10, y), currentButton == Buttons.CONTINUE, menu.TexPixel);
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Quit to Menu", new Vector2(menu.ScreenWidth / 2 + 10, y), currentButton == Buttons.QUIT_TO_MAINMENU, menu.TexPixel);
            y += 80;


            // disconnected message
            int numDisconnectMessages = 0;
            for (int i = 0; i < 4; ++i)
            {
                if (InputManager.Instance.IsWaitingForReconnect(i))
                {
                    string message = " - gamepad " + (i + 1) + " disconnected! - ";
                    Vector2 position = (new Vector2(menu.ScreenWidth/2, y) - menu.Font.MeasureString(message)/2);

                    // find out player color
                    int playerIndex = i;
                    for(; playerIndex<Player.MaxNumPlayers; ++playerIndex)
                    {
                        if (Settings.Instance.PlayerControls[playerIndex] == InputManager.ControlType.GAMEPAD0 + i)
                            break;
                    }
                    if(Settings.Instance.PlayerColorIndices[playerIndex] >=0)
                        spriteBatch.DrawString(menu.Font, message, position, Player.Colors[Settings.Instance.PlayerColorIndices[playerIndex]]);
 

                    ++numDisconnectMessages;
                    y += 35;
                }
            }

            
        }
    }
}
