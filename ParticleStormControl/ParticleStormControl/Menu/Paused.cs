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
        public Paused(Menu menu)
            : base(menu)
        { }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
        }

        public override void Update(GameTime gameTime)
        {
            // back to game
            if (InputManager.Instance.PauseButton() || InputManager.Instance.ContinueButton())
            {
                // reset wait for reconnect settings - not connected pads will now be ignored
                InputManager.Instance.ResetWaitingForReconnect();
                menu.ChangePage(Menu.Page.INGAME, gameTime);
            }

            // shutdown per (any) pad
            if (InputManager.Instance.PressedButton(Buttons.Back) || InputManager.Instance.PressedButton(Keys.Escape))
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, float frameTimeInterval)
        {
            spriteBatch.Draw(menu.TexPixel, new Rectangle(0, 0, menu.ScreenWidth, menu.ScreenHeight), Color.Black * 0.5f);
            Vector2 stringSizePaused = menu.Font.MeasureString("PAUSED");
            Vector2 positionPaused = (new Vector2(menu.ScreenWidth, menu.ScreenHeight) - stringSizePaused) / 2;

            spriteBatch.DrawString(menu.Font, "PAUSED", positionPaused, Color.White);

            int numDisconnectMessages = 0;
            for (int i = 0; i < 4; ++i)
            {
                if (InputManager.Instance.IsWaitingForReconnect(i))
                {
                    string message = " - gamepad " + (i + 1) + " disconnected! - ";
                    Vector2 position = (new Vector2(menu.ScreenWidth, menu.ScreenWidth + stringSizePaused.Y * (1 + numDisconnectMessages) + 20) -
                                                 menu.Font.MeasureString(message)) / 2;
                    spriteBatch.DrawString(menu.Font, message, position, Color.Black); //Player.Colors[Settings.Instance.PlayerColorIndices[i]]); ATTENTION - PLAYER COLOR OF THE USED GAMEPAD WOULD BE NEEDED
                    ++numDisconnectMessages;
                }
            }
        }
    }
}
