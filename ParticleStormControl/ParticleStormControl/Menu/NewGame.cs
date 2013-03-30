#define QUICK_TWO_PLAYER_DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using ParticleStormControl;
using Microsoft.Xna.Framework.Input;

namespace ParticleStormControl.Menu
{
    class NewGame : MenuPage
    {
        private const int padd = 40; // padding for boxes
        private const int offset = 5; // offset for arrows

        private const int maxCountdown = 3;
        private const int safeCountdown = 1;

        private bool[] playerConnected = new bool[4];
        private bool[] playerReady = new bool[4];

        private Texture2D[] viruses = new Texture2D[4];
        private Texture2D icons;

        private readonly Color fontColor = Color.Black;
        private TimeSpan countdown = new TimeSpan();

        public NewGame(Menu menu)
            : base(menu)
        {
            Settings.Instance.ResetPlayerSettingsToDefault();
            playerConnected = new bool[4];
            playerReady = new bool[4];
            countdown = new TimeSpan();
        }

        public override void OnActivated(Menu.Page oldPage)
        {
            Settings.Instance.ResetPlayerSettingsToDefault();
            for (int i = 0; i < 4; ++i)
            {
                playerConnected[i] = false;
                playerReady[i] = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        public override void LoadContent(ContentManager content)
        {
            viruses[0] = content.Load<Texture2D>("viruses/H1N1");
            viruses[1] = content.Load<Texture2D>("viruses/HepatitisB");
            viruses[2] = content.Load<Texture2D>("viruses/HIV");
            viruses[3] = content.Load<Texture2D>("viruses/Noro");
            icons = content.Load<Texture2D>("icons");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frameTimeInterval"></param>
        public override void Update(float frameTimeInterval)
        {
            // :P
#if QUICK_TWO_PLAYER_DEBUG
            Settings.Instance.PlayerControls[0] = InputManager.ControlType.KEYBOARD0;
            Settings.Instance.PlayerColorIndices[0] = 0;
            Settings.Instance.PlayerControls[1] = InputManager.ControlType.KEYBOARD1;
            Settings.Instance.PlayerColorIndices[1] = 1;
            Settings.Instance.NumPlayers = 2;
            playerReady[0] = playerReady[1] = playerConnected[0] = playerConnected[1] = Settings.Instance.PlayerConnected[0] = Settings.Instance.PlayerConnected[1] = true;
            menu.ChangePage(Menu.Page.INGAME);
#endif

            if (InputManager.Instance.ExitButton())
                menu.ChangePage(Menu.Page.MAINMENU);

            TimeSpan oldCountdown = countdown;
            countdown = countdown.Subtract(TimeSpan.FromSeconds(frameTimeInterval));
            if (oldCountdown.TotalSeconds > 0 && countdown.TotalSeconds <= 0)
            {
                menu.ChangePage(Menu.Page.INGAME);
                return;
            }

            // test continue buttons
            foreach (InputManager.ControlType type in InputManager.Instance.ContinueButtonsPressed())
            {
                bool found = false;

                for (int i = 0; i < 4; i++)
                {
                    // player already connected
                    if (type == Settings.Instance.PlayerControls[i])
                    {
                        found = true;

                        toggleReady(i);
                    }
                }

                // add new player
                if (!found)
                {
                    int index = getFreePlayerIndex();
                    int colorIndex = getNextFreeColorIndex(0);
                    playerConnected[index] = true;
                    Settings.Instance.PlayerControls[index] = type;
                    Settings.Instance.PlayerColorIndices[index] = colorIndex;
                    Settings.Instance.NumPlayers++;
                    Settings.Instance.PlayerConnected[index] = true;
                    countdown = TimeSpan.FromSeconds(-1);
                }
            }

            // test various buttons
            for (int i = 0; i < 4; i++)
            {
                if (playerConnected[i])
                {
                    if (InputManager.Instance.ButtonPressed(InputManager.ControlActions.LEFT, Settings.Instance.PlayerControls[i], false) && !playerReady[i])
                    {
                        if (--Settings.Instance.PlayerVirusIndices[i] < 0)
                            Settings.Instance.PlayerVirusIndices[i] = Player.Viruses.Length - 1;
                    }

                    if (InputManager.Instance.ButtonPressed(InputManager.ControlActions.RIGHT, Settings.Instance.PlayerControls[i], false) && !playerReady[i])
                    {
                        if (++Settings.Instance.PlayerVirusIndices[i] >= Player.Viruses.Length)
                            Settings.Instance.PlayerVirusIndices[i] = 0;
                    }

                    if (InputManager.Instance.ButtonPressed(InputManager.ControlActions.UP, Settings.Instance.PlayerControls[i], false) && !playerReady[i])
                    {
                        Settings.Instance.PlayerColorIndices[i] = getPreviousFreeColorIndex(Settings.Instance.PlayerColorIndices[i]);
                    }

                    if (InputManager.Instance.ButtonPressed(InputManager.ControlActions.DOWN, Settings.Instance.PlayerControls[i], false) && !playerReady[i])
                    {
                        Settings.Instance.PlayerColorIndices[i] = getNextFreeColorIndex(Settings.Instance.PlayerColorIndices[i]);
                    }

                    if (InputManager.Instance.ButtonPressed(InputManager.ControlActions.HOLD, Settings.Instance.PlayerControls[i], false))
                    {
                        if (playerReady[i])
                        {
                            toggleReady(i);
                        }
                        else
                        {
                            // free slot
                            Settings.Instance.ResetPlayerSettingsToDefault(i);
                            Settings.Instance.NumPlayers--;
                            Settings.Instance.PlayerConnected[i] = false;
                            playerConnected[i] = false;
                            playerReady[i] = false;
                            startCountdown();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="timeInterval"></param>
        public override void Draw(SpriteBatch spriteBatch, float timeInterval)
        {
            int boxWidth = (Settings.Instance.ResolutionX - 3 * padd) / 2;
            int boxHeight = (int)(Settings.Instance.ResolutionX * 0.5625f - 3 * padd) / 2;

            int offX = 0;
            int offY = (Settings.Instance.ResolutionY - 2 * boxHeight - 3 * padd) / 2;

            if (boxHeight * 2 + 3 * padd > Settings.Instance.ResolutionY)
            {
                boxWidth = (int)(Settings.Instance.ResolutionY * 16 / 9 - 3 * padd) / 2;
                boxHeight = (Settings.Instance.ResolutionY - 3 * padd) / 2;


                offX = (Settings.Instance.ResolutionX - 2 * boxWidth - 3 * padd) / 2;
                offY = 0;
            }

            int x = padd * 2 + boxWidth;
            int y = padd * 2 + boxHeight;

            // four boxes
            spriteBatch.Draw(menu.PixelTexture, new Rectangle(padd + offX, padd + offY, boxWidth, boxHeight), Color.FromNonPremultiplied(255, 255, 255, 128));
            spriteBatch.Draw(menu.PixelTexture, new Rectangle(x + offX, padd + offY, boxWidth, boxHeight), Color.FromNonPremultiplied(255, 255, 255, 128));
            spriteBatch.Draw(menu.PixelTexture, new Rectangle(padd + offX, y + offY, boxWidth, boxHeight), Color.FromNonPremultiplied(255, 255, 255, 128));
            spriteBatch.Draw(menu.PixelTexture, new Rectangle(x + offX, y + offY, boxWidth, boxHeight), Color.FromNonPremultiplied(255, 255, 255, 128));

            // for each player
            for (int i = 0; i < 4; i++)
            {
                Vector2 origin = new Vector2();
                switch (i)
                {
                    case 2:
                        origin = new Vector2(padd + offX, padd + offY);
                        break;
                    case 3:
                        origin = new Vector2(x + offX, y + offY);
                        break;
                    case 1:
                        origin = new Vector2(x + offX, padd + offY);
                        break;
                    case 0:
                        origin = new Vector2(padd + offX, y + offY);
                        break;
                }

                if (playerConnected[i])
                {
                    // text
                    spriteBatch.DrawString(menu.Font, Player.VirusNames[Settings.Instance.PlayerVirusIndices[i]].ToString(), origin + new Vector2(20 + boxWidth / 2, 20), fontColor);
                    spriteBatch.DrawString(menu.FontBold, "Color: ", origin + new Vector2(20 + boxWidth / 2, 150), fontColor);
                    spriteBatch.DrawString(menu.FontBold, Player.ColorNames[Settings.Instance.PlayerColorIndices[i]].ToString(), origin + new Vector2(110 + boxWidth / 2, 150), Player.Colors[Settings.Instance.PlayerColorIndices[i]]);
                    spriteBatch.DrawString(menu.FontBold, "Controls: " + Player.ControlNames[(int)Settings.Instance.PlayerControls[i]].ToString(), origin + new Vector2(20 + boxWidth / 2, 80), fontColor);
                    spriteBatch.DrawString(menu.FontBold, playerReady[i] ? "ready!" : "not ready", origin + new Vector2(40 + boxWidth / 2, 200), fontColor);

                    // image
                    Rectangle destination = new Rectangle((int)origin.X + padd, (int)origin.Y + padd, boxWidth / 2 - padd, boxWidth / 2 - padd);
                    spriteBatch.Draw(viruses[Settings.Instance.PlayerVirusIndices[i]], destination, Color.White);

                    // arrows left & right
                    spriteBatch.Draw(icons, new Rectangle((int)origin.X + 16 - offsetIfDown(InputManager.ControlActions.LEFT, i), (int)origin.Y + boxHeight / 2 - 8, 16, 16), new Rectangle(0, 0, 16, 16), Color.White);
                    spriteBatch.Draw(icons, new Rectangle((int)origin.X + boxWidth - 32 + offsetIfDown(InputManager.ControlActions.RIGHT, i), (int)origin.Y + boxHeight / 2 - 8, 16, 16), new Rectangle(16, 0, 16, 16), Color.White);

                    // arrows up & down
                    spriteBatch.Draw(icons, new Rectangle((int)origin.X + 85 + boxWidth / 2, (int)origin.Y + 150 - offsetIfDown(InputManager.ControlActions.UP, i), 16, 16), new Rectangle(0, 16, 16, 16), Color.White);
                    spriteBatch.Draw(icons, new Rectangle((int)origin.X + 85 + boxWidth / 2, (int)origin.Y + 160 + offsetIfDown(InputManager.ControlActions.DOWN, i), 16, 16), new Rectangle(16, 16, 16, 16), Color.White);

                    // ready icon
                    spriteBatch.Draw(icons, new Rectangle((int)origin.X + 20 + boxWidth / 2, (int)origin.Y + 205, 16, 16), new Rectangle(playerReady[i] ? 16 : 0, 32, 16, 16), Color.White);
                }
                else
                {
                    string joinText = "press continue to join game";
                    Vector2 stringSize = menu.Font.MeasureString(joinText);
                    spriteBatch.DrawString(menu.FontSmall, joinText, origin + new Vector2((boxWidth - stringSize.X) / 2, (boxHeight - stringSize.Y) / 2), Color.Black);
                }
            }

            // countdown
            if (countdown.TotalSeconds > 0)
            {
                // last half ? draw black fade
                if (countdown.TotalSeconds < InGame.GAME_BLEND_DURATION)
                {
                    float blend = (float)(InGame.GAME_BLEND_DURATION - countdown.TotalSeconds) / InGame.GAME_BLEND_DURATION;
                    spriteBatch.Draw(menu.PixelTexture, new Rectangle(0, 0, menu.ScreenWidth, menu.ScreenHeight), Color.Black * blend);
                }

                // countdown
                String text = ((int)countdown.TotalSeconds + 1).ToString();
                spriteBatch.DrawString(menu.FontCountdown, text, new Vector2(Settings.Instance.ResolutionX / 2 - 5, Settings.Instance.ResolutionY / 2 - 15), countdown.TotalSeconds > safeCountdown ? Color.White : Color.Red,
                                        0.0f, menu.FontCountdown.MeasureString(text) * 0.5f, 1, SpriteEffects.None, 1.0f);
            }
        }

        /* Helper */

        private int getNextFreeColorIndex(int start)
        {
            while (Settings.Instance.PlayerColorIndices.Contains(start) || start >= Player.Colors.Length)
            {
                if(start++ >= Player.Colors.Length) start = 0;
            }
            return start;
        }

        private int getPreviousFreeColorIndex(int start)
        {
            while (Settings.Instance.PlayerColorIndices.Contains(start) || start < 0)
            {
                if (start-- < 0) start = Player.Colors.Length - 1;
            }
            return start;
        }

        private int getFreePlayerIndex()
        {
            int i = 0;
            while (playerConnected[i] && i < 3)
                i++;
            return i;
        }

        private int offsetIfDown(InputManager.ControlActions action, int index)
        {
            return InputManager.Instance.ButtonPressed(action, Settings.Instance.PlayerControls[index], true) && !playerReady[index] ? offset : 0;
        }

        private void startCountdown()
        {
            bool allReady = playerConnected.Count(x => x) > 1;
            for (int i = 0; i < 3; i++)
            {
                if (playerConnected[i] != playerReady[i])
                    allReady = false;
            }
            if (allReady && Settings.Instance.NumPlayers > 0)
                countdown = TimeSpan.FromSeconds(maxCountdown - 0.001);
        }

        private void toggleReady(int index)
        {
            // toggle ready
            if (countdown.TotalSeconds > safeCountdown || countdown.TotalSeconds <= 0)
                playerReady[index] = !playerReady[index];

            // countdown
            if (playerReady[index] && countdown.TotalSeconds <= 0)
                startCountdown();
            else if (countdown.TotalSeconds > safeCountdown)
                countdown = TimeSpan.FromSeconds(-1);
        }
    }
}
