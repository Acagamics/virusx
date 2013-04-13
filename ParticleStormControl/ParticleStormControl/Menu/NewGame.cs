//#define QUICK_TWO_PLAYER_DEBUG
//#define QUICK_FOUR_PLAYER_DEBUG

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
        private const int padd = 30; // padding for boxes
        private const int offset = 5; // offset for arrows

        private const int maxCountdown = 3;
        private const int safeCountdown = 1;

        private bool[] playerConnected = new bool[4];
        private bool[] playerReady = new bool[4];

        private Effect virusRenderEffect;
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
            countdown = TimeSpan.Zero;
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
            virusRenderEffect = content.Load<Effect>("shader/particleRendering");
            icons = content.Load<Texture2D>("icons");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // :P
#if QUICK_TWO_PLAYER_DEBUG
            Settings.Instance.PlayerControls[0] = InputManager.ControlType.KEYBOARD0;
            Settings.Instance.PlayerColorIndices[0] = 0;
            Settings.Instance.PlayerVirusIndices[0] = 2;
            Settings.Instance.PlayerControls[1] = InputManager.ControlType.KEYBOARD1;
            Settings.Instance.PlayerColorIndices[1] = 1;
            Settings.Instance.PlayerVirusIndices[1] = 3;
            Settings.Instance.NumPlayers = 2;
            playerReady[0] = playerReady[1] = playerConnected[0] = playerConnected[1] = Settings.Instance.PlayerConnected[0] = Settings.Instance.PlayerConnected[1] = true;
            menu.ChangePage(Menu.Page.INGAME,gameTime);
#elif QUICK_FOUR_PLAYER_DEBUG
            Settings.Instance.PlayerControls[0] = InputManager.ControlType.KEYBOARD0;
            Settings.Instance.PlayerColorIndices[0] = 1;
            Settings.Instance.PlayerVirusIndices[0] = 0;
            Settings.Instance.PlayerControls[1] = InputManager.ControlType.KEYBOARD1;
            Settings.Instance.PlayerColorIndices[1] = 3;
            Settings.Instance.PlayerVirusIndices[1] = 1;
            Settings.Instance.PlayerControls[2] = InputManager.ControlType.GAMEPAD0;
            Settings.Instance.PlayerColorIndices[2] = 4;
            Settings.Instance.PlayerVirusIndices[2] = 2;
            Settings.Instance.PlayerControls[3] = InputManager.ControlType.GAMEPAD1;
            Settings.Instance.PlayerColorIndices[3] = 5;
            Settings.Instance.PlayerVirusIndices[3] = 3;
            Settings.Instance.NumPlayers = 4;
            playerReady[0] = playerReady[1] = playerReady[2] = playerReady[3] = playerConnected[0] = playerConnected[1] = playerConnected[2] = playerConnected[3] = Settings.Instance.PlayerConnected[0] = Settings.Instance.PlayerConnected[1] = Settings.Instance.PlayerConnected[2] = Settings.Instance.PlayerConnected[3] = true;
            menu.ChangePage(Menu.Page.INGAME,gameTime);
#endif

            if (InputManager.Instance.ExitButton())
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);

            TimeSpan oldCountdown = countdown;
            countdown = countdown.Subtract(gameTime.ElapsedGameTime);
            if (oldCountdown.TotalSeconds > 0 && countdown.TotalSeconds <= 0)
            {
                menu.ChangePage(Menu.Page.INGAME, gameTime);
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
                    Settings.Instance.PlayerVirusIndices[index] = Random.Next(4);
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
                    if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.LEFT, Settings.Instance.PlayerControls[i], false) && !playerReady[i])
                    {
                        if (--Settings.Instance.PlayerVirusIndices[i] < 0)
                            Settings.Instance.PlayerVirusIndices[i] = Player.Viruses.Length - 1;
                    }

                    if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.RIGHT, Settings.Instance.PlayerControls[i], false) && !playerReady[i])
                    {
                        if (++Settings.Instance.PlayerVirusIndices[i] >= Player.Viruses.Length)
                            Settings.Instance.PlayerVirusIndices[i] = 0;
                    }

                    if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.UP, Settings.Instance.PlayerControls[i], false) && !playerReady[i])
                    {
                        Settings.Instance.PlayerColorIndices[i] = getPreviousFreeColorIndex(Settings.Instance.PlayerColorIndices[i]);
                    }

                    if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.DOWN, Settings.Instance.PlayerControls[i], false) && !playerReady[i])
                    {
                        Settings.Instance.PlayerColorIndices[i] = getNextFreeColorIndex(Settings.Instance.PlayerColorIndices[i]);
                    }

                    if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.HOLD, Settings.Instance.PlayerControls[i], false))
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
           /* int boxHeight = (int)(Settings.Instance.ResolutionX * 0.5625f - padd) / 2;
            int boxWidth = (int)(boxHeight * 1.8f);
            
            int offX = 0;
            int offY = (Settings.Instance.ResolutionY - 2 * boxHeight - 2 * padd) / 2;

            if (boxHeight * 2 + 3 * padd > Settings.Instance.ResolutionY)
            {
                boxWidth = (int)(Settings.Instance.ResolutionY * 16 / 9 - 3 * padd) / 2;
                boxHeight = (Settings.Instance.ResolutionY - 3 * padd) / 2;

                offX = (Settings.Instance.ResolutionX - 2 * boxWidth - 3 * padd) / 2;
                offY = 0;
            }*/

            // new fixed resolution solution
            const int boxWidth = 1024 / 2 - padd * 2;
            const int boxHeight = 768 / 2 - padd * 2;

            // four boxes
            /*spriteBatch.Draw(menu.PixelTexture, new Rectangle(padd + offX, padd + offY, boxWidth, boxHeight), Color.FromNonPremultiplied(255, 255, 255, 128));
            spriteBatch.Draw(menu.PixelTexture, new Rectangle(x + offX, padd + offY, boxWidth, boxHeight), Color.FromNonPremultiplied(255, 255, 255, 128));
            spriteBatch.Draw(menu.PixelTexture, new Rectangle(padd + offX, y + offY, boxWidth, boxHeight), Color.FromNonPremultiplied(255, 255, 255, 128));
            spriteBatch.Draw(menu.PixelTexture, new Rectangle(x + offX, y + offY, boxWidth, boxHeight), Color.FromNonPremultiplied(255, 255, 255, 128));*/

            int textBoxHeight = (int)menu.Font.MeasureString("ABC").Y + SimpleButton.PADDING * 2;
            int ARROW_SIZE = textBoxHeight;
            int SIDE_PADDING = ARROW_SIZE + 30;

            virusRenderEffect.Parameters["ScreenSize"].SetValue(new Vector2(menu.ScreenWidth, menu.ScreenHeight));

            // for each player
            for (int i = 0; i < 4; i++)
            {
                Vector2 origin = new Vector2();
                switch (i)
                {
                    case 3:
                        origin = new Vector2(Settings.Instance.ResolutionX / 4 - boxWidth / 2, Settings.Instance.ResolutionY / 4 - boxHeight / 2);
                        break;
                    case 2:
                        origin = new Vector2(Settings.Instance.ResolutionX / 4 * 3 - boxWidth / 2, Settings.Instance.ResolutionY / 4 * 3 - boxHeight / 2);
                        break;
                    case 1:
                        origin = new Vector2(Settings.Instance.ResolutionX / 4 * 3 - boxWidth / 2, Settings.Instance.ResolutionY / 4 - boxHeight / 2);
                        break;
                    case 0:
                        origin = new Vector2(Settings.Instance.ResolutionX / 4 - boxWidth / 2, Settings.Instance.ResolutionY / 4 * 3 - boxHeight / 2);
                        break;
                }

                if (playerConnected[i])
                {
                    // text
                    SimpleButton.Instance.Draw(spriteBatch, menu.FontHeading, Player.VirusNames[Settings.Instance.PlayerVirusIndices[i]].ToString(),
                                                    origin + new Vector2(SIDE_PADDING, 0),false, menu.PixelTexture);

                    // controlls
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, Player.ControlNames[(int)Settings.Instance.PlayerControls[i]].ToString(),
                                                    origin + new Vector2(SIDE_PADDING, textBoxHeight*2 + SimpleButton.PADDING * 2), false, menu.PixelTexture);

                    /*
                    string colorString = "Color: ";
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, colorString, origin + new Vector2(SIDE_PADDING, textBoxHeight*2.5f), false, menu.PixelTexture);
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, Player.ColorNames[Settings.Instance.PlayerColorIndices[i]].ToString(),
                                    origin + new Vector2(SIDE_PADDING + SimpleButton.PADDING * 2 + menu.Font.MeasureString(colorString).X, textBoxHeight * 2.5f), Settings.Instance.GetPlayerColor(i), menu.PixelTexture);
                    */

                    // ready
                    int readyHeight = textBoxHeight * 4;
                    string playerReadyText = playerReady[i] ? "ready!" : "not ready";
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, playerReadyText, origin + new Vector2(SIDE_PADDING + ARROW_SIZE, readyHeight), playerReady[i], menu.PixelTexture);
                    SimpleButton.Instance.DrawTexture_NoScalingNoPadding(spriteBatch, icons, new Rectangle((int)origin.X + SIDE_PADDING - SimpleButton.PADDING,
                                            (int)origin.Y + readyHeight - SimpleButton.PADDING, ARROW_SIZE, ARROW_SIZE), new Rectangle(playerReady[i] ? 48 : 0, 32, 16, 16), playerReady[i], menu.PixelTexture);

                    // description
                    int descpStrLen = (int)menu.Font.MeasureString("Discipline").X + 7;
                    int symbolLen = (int)menu.Font.MeasureString("++++").X + SimpleButton.PADDING;
                    int backgroundLength = boxWidth - SIDE_PADDING * 2 + SimpleButton.PADDING*2;//(descpStrLen + symbolLen) * 2 + 15;
                    int descpX0 = SIDE_PADDING;
                    int descpX1 = descpX0 + backgroundLength / 2;
                    float descpY = boxHeight - textBoxHeight * 2;

                    string symbols = Player.DiscriptorSpeed[Settings.Instance.PlayerVirusIndices[i]];//. AttributValueToString(Player.speed_byVirus[Settings.Instance.PlayerVirusIndices[i]]);
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Speed", origin + new Vector2(descpX0, descpY), false, menu.PixelTexture, backgroundLength);
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, symbols, origin + new Vector2(descpX0 + descpStrLen, descpY), false, menu.PixelTexture, -1);
                    symbols = Player.DiscriptorMass[Settings.Instance.PlayerVirusIndices[i]];//Player.AttributValueToString(Player.mass_byVirus[Settings.Instance.PlayerVirusIndices[i]]);
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Mass", origin + new Vector2(descpX0, descpY + textBoxHeight), false, menu.PixelTexture, backgroundLength);
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, symbols, origin + new Vector2(descpX0 + descpStrLen, descpY + textBoxHeight), false, menu.PixelTexture, -1);
                    symbols = Player.DiscriptorDisciplin[Settings.Instance.PlayerVirusIndices[i]];//Player.AttributValueToString(Player.disciplin_byVirus[Settings.Instance.PlayerVirusIndices[i]]);
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Discipline", origin + new Vector2(descpX1, descpY), false, menu.PixelTexture, -1);
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, symbols, origin + new Vector2(descpX1 + descpStrLen, descpY), false, menu.PixelTexture, -1);
                    symbols = Player.DiscriptorHealth[Settings.Instance.PlayerVirusIndices[i]];//Player.AttributValueToString(Player.health_byVirus[Settings.Instance.PlayerVirusIndices[i]]);
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Health", origin + new Vector2(descpX1, descpY + textBoxHeight), false, menu.PixelTexture, -1);
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, symbols, origin + new Vector2(descpX1 + descpStrLen, descpY + textBoxHeight), false, menu.PixelTexture, -1);
                   
                    // image
                    const int VIRUS_SIZE = 110;
                    const int VIRUS_PADDING = 10;
                    Rectangle virusImageRect = new Rectangle((int)origin.X + boxWidth - VIRUS_SIZE - SIDE_PADDING, 
                                                    (int)origin.Y + textBoxHeight + 40, VIRUS_SIZE, VIRUS_SIZE);
                    virusImageRect.Inflate(VIRUS_PADDING, VIRUS_PADDING);
                    spriteBatch.Draw(menu.PixelTexture, virusImageRect, Color.Black);
                    virusImageRect.Inflate(-VIRUS_PADDING, -VIRUS_PADDING);
                    spriteBatch.End(); // yeah this sucks terrible! TODO better solution
                    switch(Player.Viruses[Settings.Instance.PlayerVirusIndices[i]])
                    {
                        case Player.VirusType.EPSTEINBARR:
                            virusRenderEffect.CurrentTechnique = virusRenderEffect.Techniques["EpsteinBar_Spritebatch"];
                            break;
                        case Player.VirusType.H5N1:
                            virusRenderEffect.CurrentTechnique = virusRenderEffect.Techniques["H5N1_Spritebatch"];
                            break;
                        case Player.VirusType.HIV:
                            virusRenderEffect.CurrentTechnique = virusRenderEffect.Techniques["HIV_Spritebatch"];
                            break;
                        case Player.VirusType.HEPATITISB:
                            virusRenderEffect.CurrentTechnique = virusRenderEffect.Techniques["HepatitisB_Spritebatch"];
                            break;
                    }
                    virusRenderEffect.Parameters["Color"].SetValue(Player.ParticleColors[Settings.Instance.PlayerColorIndices[i]].ToVector4() * 1.5f);
                    spriteBatch.Begin(0, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, virusRenderEffect);
                    spriteBatch.Draw(menu.PixelTexture, virusImageRect, Color.White);
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);

                    const int ARROW_UP_SIZE = 25;
                    const int ARROW_WIDDEN = 8;
                    SimpleButton.Instance.DrawTexture_NoScalingNoPadding(spriteBatch, icons, new Rectangle(virusImageRect.Center.X - (ARROW_UP_SIZE + ARROW_WIDDEN) / 2,
                                                            virusImageRect.Y - ARROW_UP_SIZE - VIRUS_PADDING / 2, ARROW_UP_SIZE + ARROW_WIDDEN, ARROW_UP_SIZE),
                                                new Rectangle(0 + isActive(InputManager.ControlActions.UP, i, 32), 16, 16, 16), isActive(InputManager.ControlActions.UP, i), menu.PixelTexture);
                    SimpleButton.Instance.DrawTexture_NoScalingNoPadding(spriteBatch, icons, new Rectangle(virusImageRect.Center.X - (ARROW_UP_SIZE + ARROW_WIDDEN) / 2,
                                                            virusImageRect.Bottom + VIRUS_PADDING / 2, ARROW_UP_SIZE + ARROW_WIDDEN, ARROW_UP_SIZE),
                                                new Rectangle(16 + isActive(InputManager.ControlActions.DOWN, i, 32), 16, 16, 16), isActive(InputManager.ControlActions.DOWN, i), menu.PixelTexture);
                    
                    // arrows left & right
                    int arrowY = (int)readyHeight - ARROW_SIZE;//boxHeight / 2 - ARROW_SIZE;
                    SimpleButton.Instance.DrawTexture_NoScalingNoPadding(spriteBatch, icons, new Rectangle((int)origin.X, (int)origin.Y + arrowY, ARROW_SIZE, ARROW_SIZE), new Rectangle(0 + isActive(InputManager.ControlActions.LEFT, i, 32), 0, 16, 16), isActive(InputManager.ControlActions.LEFT, i), menu.PixelTexture);
                    SimpleButton.Instance.DrawTexture_NoScalingNoPadding(spriteBatch, icons, new Rectangle((int)origin.X + boxWidth - ARROW_SIZE, (int)origin.Y + arrowY, ARROW_SIZE, ARROW_SIZE), new Rectangle(16 + isActive(InputManager.ControlActions.RIGHT, i, 32), 0, 16, 16), isActive(InputManager.ControlActions.RIGHT, i), menu.PixelTexture);
                }
                else
                {
                    string joinText = "< press continue to join game >";
                    Vector2 stringSize = menu.Font.MeasureString(joinText);
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, joinText, origin + new Vector2((int)((boxWidth - stringSize.X) / 2), (int)((boxHeight - stringSize.Y) / 2)), true, menu.PixelTexture);
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
                String text = "game starts in " + ((int)countdown.TotalSeconds + 1).ToString() + "...";
                Vector2 size = menu.FontCountdown.MeasureString(text) / 2;
                SimpleButton.Instance.Draw(spriteBatch, menu.FontCountdown, text, new Vector2(Settings.Instance.ResolutionX / 2 - 5, Settings.Instance.ResolutionY / 2 - 15) - size, !(countdown.TotalSeconds > safeCountdown), menu.PixelTexture);
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

        private bool isActive(InputManager.ControlActions action, int index)
        {
            return InputManager.Instance.SpecificActionButtonPressed(action, Settings.Instance.PlayerControls[index], true) && !playerReady[index];
        }

        private int isActive(InputManager.ControlActions action, int index, int offset)
        {
            return isActive(action, index) ? offset : 0;
        }

        private void startCountdown()
        {
            bool allReady = playerConnected.Count(x => x) > 1;
            for (int i = 0; i < 4; i++)
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

        private void drawBackground(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
        {
            int padding = 20;
            spriteBatch.Draw(menu.PixelTexture, new Rectangle((int)position.X - padding, (int)position.Y - padding, (int)size.X + 2 * padding, (int)size.Y + 2 * padding), Color.Black);
        }
    }
}
