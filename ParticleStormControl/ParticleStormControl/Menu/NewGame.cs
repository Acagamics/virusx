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

        private bool[] playerSlotOccupied = new bool[4];
        private bool[] playerReadyBySlot = new bool[4];
        private int[] slotIndexToPlayerIndexMapper = new int[4];

        private Effect virusRenderEffect;
        private Texture2D icons;

        private readonly Color fontColor = Color.Black;
        private TimeSpan countdown = new TimeSpan();

        /// <summary>
        /// controls of the player who opened this menu
        /// </summary>
        public InputManager.ControlType StartingControls {get;set;}

        public NewGame(Menu menu)
            : base(menu)
        {
            playerSlotOccupied = new bool[4];
            playerReadyBySlot = new bool[4];
            countdown = new TimeSpan();
        }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            Settings.Instance.ResetPlayerSettings();
            countdown = TimeSpan.Zero;
            for (int i = 0; i < 4; ++i)
            {
                playerSlotOccupied[i] = false;
                playerReadyBySlot[i] = false;
                slotIndexToPlayerIndexMapper[i] = i;
            }

            if (StartingControls != InputManager.ControlType.NONE)
                AddPlayer(false, StartingControls);
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
            if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.EXIT) ||
                InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.HOLD, StartingControls))
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);

            TimeSpan oldCountdown = countdown;
            countdown = countdown.Subtract(gameTime.ElapsedGameTime);
            if (oldCountdown.TotalSeconds > 0 && countdown.TotalSeconds <= 0)
            {
                menu.ChangePage(Menu.Page.INGAME, gameTime);
                return;
            }

            // test various buttons
            for (int playerIndex = 0; playerIndex < Settings.Instance.NumPlayers; playerIndex++)
            {
                int slot = Settings.Instance.GetPlayer(playerIndex).SlotIndex;
                if (playerSlotOccupied[slot] && Settings.Instance.GetPlayer(playerIndex).Type == Player.Type.HUMAN)
                {
                    // color
                    if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.LEFT, Settings.Instance.GetPlayer(playerIndex).ControlType, false) && !playerReadyBySlot[slot])
                    {
                        if (--Settings.Instance.GetPlayer(playerIndex).VirusIndex < 0)
                            Settings.Instance.GetPlayer(playerIndex).VirusIndex = VirusSwarm.Viruses.Length - 1;
                    }
                    else if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.RIGHT, Settings.Instance.GetPlayer(playerIndex).ControlType, false) && !playerReadyBySlot[slot])
                    {
                        if (++Settings.Instance.GetPlayer(playerIndex).VirusIndex >= VirusSwarm.Viruses.Length)
                            Settings.Instance.GetPlayer(playerIndex).VirusIndex = 0;
                    }

                    // color
                    if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.UP, Settings.Instance.GetPlayer(playerIndex).ControlType, false) && !playerReadyBySlot[slot])
                        Settings.Instance.GetPlayer(playerIndex).ColorIndex = GetPreviousFreeColorIndex(Settings.Instance.GetPlayer(playerIndex).ColorIndex);
                    else if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.DOWN, Settings.Instance.GetPlayer(playerIndex).ControlType, false) && !playerReadyBySlot[slot])
                        Settings.Instance.GetPlayer(playerIndex).ColorIndex = GetNextFreeColorIndex(Settings.Instance.GetPlayer(playerIndex).ColorIndex);

                    // remove
                    if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.HOLD, Settings.Instance.GetPlayer(playerIndex).ControlType, false) ||
                        InputManager.Instance.IsWaitingForReconnect(Settings.Instance.GetPlayer(playerIndex).ControlType))
                    {
                        if (playerReadyBySlot[slot])
                            ToggleReady(slot);
                        else
                        {
                            RemovePlayer(slot);
                            break; // this blocks other inputs, but @30fps min thats not that bad
                        }
                    }

                    // ready
                    if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.ACTION, Settings.Instance.GetPlayer(playerIndex).ControlType, false))
                        ToggleReady(slot);
                }
            }

            // new human player?
            foreach (InputManager.ControlType type in InputManager.Instance.ContinueButtonsPressed())
            {
                if (!Settings.Instance.GetPlayerSettingSelection(x=>x.ControlType).Contains(type))
                    AddPlayer(false, type);
            }

            // test add/remove ai
            if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.ADD_AI))
            {
                int index = AddPlayer(true, InputManager.ControlType.NONE);
                if (index != -1)    
                    ToggleReady(index);
            }
            else if(InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.REMOVE_AI))
            {
                // search for an ai player
                for (int i = Settings.Instance.NumPlayers-1; i >= 0; --i)
                {
                    if (playerSlotOccupied[Settings.Instance.GetPlayer(i).SlotIndex] && Settings.Instance.GetPlayer(i).Type == Player.Type.AI)
                    {
                        RemovePlayer(Settings.Instance.GetPlayer(i).SlotIndex);
                        break;
                    }
                }
            }
        }

        int AddPlayer(bool ai, InputManager.ControlType controlType)
        {
            int slotIndex = GetFreeSlotIndex();
            int playerIndex = Settings.Instance.NumPlayers;
            if (slotIndex != -1 && playerIndex < Player.MAX_NUM_PLAYERS)
            {
                int colorIndex = GetNextFreeColorIndex(0);
                playerSlotOccupied[slotIndex] = true;
                playerReadyBySlot[slotIndex] = false;
                slotIndexToPlayerIndexMapper[slotIndex] = playerIndex;

                Settings.Instance.AddPlayer(new Settings.PlayerSettings()
                {
                    SlotIndex = slotIndex,
                    ControlType = ai ? InputManager.ControlType.NONE : controlType,
                    ColorIndex = colorIndex,
                    VirusIndex = Random.Next((int)VirusSwarm.VirusType.NUM_VIRUSES),
                    Type = ai ? Player.Type.AI : Player.Type.HUMAN,
                });


                countdown = TimeSpan.FromSeconds(-1);
            }

            return slotIndex;
        }

        void RemovePlayer(int slot)
        {
            int playerIndex = slotIndexToPlayerIndexMapper[slot];
            Settings.Instance.RemovePlayer(playerIndex);
            playerSlotOccupied[slot] = false;
            playerReadyBySlot[slot] = false;

            // refresh slotIndexToPlayerIndexMapper
            for (int i = 0; i < 4; ++i)
                slotIndexToPlayerIndexMapper[i] = -1;
            for(int i = 0; i < Settings.Instance.NumPlayers; ++i)
                slotIndexToPlayerIndexMapper[Settings.Instance.GetPlayer(i).SlotIndex] = i;

            CheckStartCountdown();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="timeInterval"></param>
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
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

                if (playerSlotOccupied[i])
                {
                    int playerIndex = slotIndexToPlayerIndexMapper[i];

                    // text
                    SimpleButton.Instance.Draw(spriteBatch, menu.FontHeading, VirusSwarm.VirusNames[Settings.Instance.GetPlayer(playerIndex).VirusIndex].ToString(),
                                                    origin + new Vector2(SIDE_PADDING, 0), playerReadyBySlot[i], menu.TexPixel);

                    // controlls
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, InputManager.CONTROL_NAMES[(int)Settings.Instance.GetPlayer(playerIndex).ControlType].ToString(),
                                                    origin + new Vector2(SIDE_PADDING, textBoxHeight*2 + SimpleButton.PADDING * 2), false, menu.TexPixel);

                    /*
                    string colorString = "Color: ";
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, colorString, origin + new Vector2(SIDE_PADDING, textBoxHeight*2.5f), false, menu.PixelTexture);
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, Player.ColorNames[Settings.Instance.GetPlayer(playerIndex).ColorIndex].ToString(),
                                    origin + new Vector2(SIDE_PADDING + SimpleButton.PADDING * 2 + menu.Font.MeasureString(colorString).X, textBoxHeight * 2.5f), Settings.Instance.GetPlayerColor(i), menu.PixelTexture);
                    */

                    // ready
                    int readyHeight = textBoxHeight * 4;
                    string playerReadyText = playerReadyBySlot[i] ? "ready!" : "not ready";
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, playerReadyText, origin + new Vector2(SIDE_PADDING + ARROW_SIZE, readyHeight), playerReadyBySlot[i], menu.TexPixel);
                    SimpleButton.Instance.DrawTexture_NoScalingNoPadding(spriteBatch, icons, new Rectangle((int)origin.X + SIDE_PADDING - SimpleButton.PADDING,
                                            (int)origin.Y + readyHeight - SimpleButton.PADDING, ARROW_SIZE, ARROW_SIZE), new Rectangle(playerReadyBySlot[i] ? 48 : 0, 32, 16, 16), !playerReadyBySlot[i], menu.TexPixel);

                    // description
                    int descpStrLen = (int)menu.Font.MeasureString("Discipline").X + 7;
                    int symbolLen = (int)menu.Font.MeasureString("++++").X + SimpleButton.PADDING;
                    int backgroundLength = boxWidth - SIDE_PADDING * 2 + SimpleButton.PADDING*2;//(descpStrLen + symbolLen) * 2 + 15;
                    int descpX0 = SIDE_PADDING;
                    int descpX1 = descpX0 + backgroundLength / 2;
                    float descpY = boxHeight - textBoxHeight * 2;

                    string symbols = VirusSwarm.DESCRIPTOR_Speed[Settings.Instance.GetPlayer(playerIndex).VirusIndex];//. AttributValueToString(Player.speed_byVirus[Settings.Instance.GetPlayer(playerIndex).VirusIndex]);
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Speed", origin + new Vector2(descpX0, descpY), false, menu.TexPixel, backgroundLength);
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, symbols, origin + new Vector2(descpX0 + descpStrLen, descpY), false, menu.TexPixel, -1);
                    symbols = VirusSwarm.DESCRIPTOR_Mass[Settings.Instance.GetPlayer(playerIndex).VirusIndex];//Player.AttributValueToString(Player.mass_byVirus[Settings.Instance.GetPlayer(playerIndex).VirusIndex]);
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Mass", origin + new Vector2(descpX0, descpY + textBoxHeight), false, menu.TexPixel, backgroundLength);
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, symbols, origin + new Vector2(descpX0 + descpStrLen, descpY + textBoxHeight), false, menu.TexPixel, -1);
                    symbols = VirusSwarm.DESCRIPTOR_Disciplin[Settings.Instance.GetPlayer(playerIndex).VirusIndex];//Player.AttributValueToString(Player.disciplin_byVirus[Settings.Instance.GetPlayer(playerIndex).VirusIndex]);
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Discipline", origin + new Vector2(descpX1, descpY), false, menu.TexPixel, -1);
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, symbols, origin + new Vector2(descpX1 + descpStrLen, descpY), false, menu.TexPixel, -1);
                    symbols = VirusSwarm.DESCRIPTOR_Health[Settings.Instance.GetPlayer(playerIndex).VirusIndex];//Player.AttributValueToString(Player.health_byVirus[Settings.Instance.GetPlayer(playerIndex).VirusIndex]);
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Health", origin + new Vector2(descpX1, descpY + textBoxHeight), false, menu.TexPixel, -1);
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, symbols, origin + new Vector2(descpX1 + descpStrLen, descpY + textBoxHeight), false, menu.TexPixel, -1);
                   
                    // image
                    const int VIRUS_SIZE = 110;
                    const int VIRUS_PADDING = 10;
                    Rectangle virusImageRect = new Rectangle((int)origin.X + boxWidth - VIRUS_SIZE - SIDE_PADDING, 
                                                    (int)origin.Y + textBoxHeight + 40, VIRUS_SIZE, VIRUS_SIZE);
                    virusImageRect.Inflate(VIRUS_PADDING, VIRUS_PADDING);
                    spriteBatch.Draw(menu.TexPixel, virusImageRect, Color.Black);
                    virusImageRect.Inflate(-VIRUS_PADDING, -VIRUS_PADDING);
                    spriteBatch.End(); // yeah this sucks terrible! TODO better solution
                    switch(VirusSwarm.Viruses[Settings.Instance.GetPlayer(playerIndex).VirusIndex])
                    {
                        case VirusSwarm.VirusType.EPSTEINBARR:
                            virusRenderEffect.CurrentTechnique = virusRenderEffect.Techniques["EpsteinBar_Spritebatch"];
                            break;
                        case VirusSwarm.VirusType.H5N1:
                            virusRenderEffect.CurrentTechnique = virusRenderEffect.Techniques["H5N1_Spritebatch"];
                            break;
                        case VirusSwarm.VirusType.HIV:
                            virusRenderEffect.CurrentTechnique = virusRenderEffect.Techniques["HIV_Spritebatch"];
                            break;
                        case VirusSwarm.VirusType.HEPATITISB:
                            virusRenderEffect.CurrentTechnique = virusRenderEffect.Techniques["HepatitisB_Spritebatch"];
                            break;
                    }
                    virusRenderEffect.Parameters["Color"].SetValue(VirusSwarm.ParticleColors[Settings.Instance.GetPlayer(playerIndex).ColorIndex].ToVector4() * 1.5f);
                    spriteBatch.Begin(0, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, virusRenderEffect);
                    spriteBatch.Draw(menu.TexPixel, virusImageRect, Color.White);
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);

                    const int ARROW_UP_SIZE = 25;
                    const int ARROW_WIDDEN = 8;
                    SimpleButton.Instance.DrawTexture_NoScalingNoPadding(spriteBatch, icons, new Rectangle(virusImageRect.Center.X - (ARROW_UP_SIZE + ARROW_WIDDEN) / 2,
                                                            virusImageRect.Y - ARROW_UP_SIZE - VIRUS_PADDING / 2, ARROW_UP_SIZE + ARROW_WIDDEN, ARROW_UP_SIZE),
                                                new Rectangle(0 + isActive(InputManager.ControlActions.UP, playerIndex, 32), 16, 16, 16), isActive(InputManager.ControlActions.UP, playerIndex), menu.TexPixel);
                    SimpleButton.Instance.DrawTexture_NoScalingNoPadding(spriteBatch, icons, new Rectangle(virusImageRect.Center.X - (ARROW_UP_SIZE + ARROW_WIDDEN) / 2,
                                                            virusImageRect.Bottom + VIRUS_PADDING / 2, ARROW_UP_SIZE + ARROW_WIDDEN, ARROW_UP_SIZE),
                                                new Rectangle(16 + isActive(InputManager.ControlActions.DOWN, playerIndex, 32), 16, 16, 16), isActive(InputManager.ControlActions.DOWN, playerIndex), menu.TexPixel);
                    
                    // arrows left & right
                    int arrowY = (int)readyHeight - ARROW_SIZE;//boxHeight / 2 - ARROW_SIZE;
                    SimpleButton.Instance.DrawTexture_NoScalingNoPadding(spriteBatch, icons, new Rectangle((int)origin.X, (int)origin.Y + arrowY, ARROW_SIZE, ARROW_SIZE),
                                                new Rectangle(0 + isActive(InputManager.ControlActions.LEFT, playerIndex, 32), 0, 16, 16), isActive(InputManager.ControlActions.LEFT, playerIndex), menu.TexPixel);
                    SimpleButton.Instance.DrawTexture_NoScalingNoPadding(spriteBatch, icons, new Rectangle((int)origin.X + boxWidth - ARROW_SIZE, (int)origin.Y + arrowY, ARROW_SIZE, ARROW_SIZE),
                                                new Rectangle(16 + isActive(InputManager.ControlActions.RIGHT, playerIndex, 32), 0, 16, 16), isActive(InputManager.ControlActions.RIGHT, playerIndex), menu.TexPixel);

                    // big fat "comp" for those who need it
                    if (Settings.Instance.GetPlayer(playerIndex).Type == Player.Type.AI)
                    {
                        Vector2 stringSize = menu.FontCountdown.MeasureString("COMP");
                        spriteBatch.DrawString(menu.FontCountdown, "COMP", origin + new Vector2(boxWidth, boxHeight) / 2 - stringSize / 2, Color.DarkGray * 0.8f);
                    }
                }
                else
                {
                    string joinText = "< press continue to join game >";
                    Vector2 stringSize = menu.Font.MeasureString(joinText);
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, joinText, origin + new Vector2((int)((boxWidth - stringSize.X) / 2), (int)((boxHeight - stringSize.Y) / 2)), true, menu.TexPixel);
                }
            }

            // countdown
            if (countdown.TotalSeconds > 0)
            {
                // last half ? draw black fade
                if (countdown.TotalSeconds < InGame.GAME_BLEND_DURATION)
                {
                    float blend = (float)(InGame.GAME_BLEND_DURATION - countdown.TotalSeconds) / InGame.GAME_BLEND_DURATION;
                    spriteBatch.Draw(menu.TexPixel, new Rectangle(0, 0, menu.ScreenWidth, menu.ScreenHeight), Color.Black * blend);
                }

                // countdown
                String text = "game starts in " + ((int)countdown.TotalSeconds + 1).ToString() + "...";
                Vector2 size = menu.FontCountdown.MeasureString(text);
                spriteBatch.Draw(menu.TexPixel, new Rectangle(0, 0, Settings.Instance.ResolutionX, Settings.Instance.ResolutionY), Color.FromNonPremultiplied(0, 0, 0, 128));
                SimpleButton.Instance.DrawTexture_NoScalingNoPadding(spriteBatch, menu.TexPixel, new Rectangle(0, Settings.Instance.ResolutionY / 2 - (int)(0.75f * size.Y), Settings.Instance.ResolutionX, (int)(size.Y * 1.5f)), menu.TexPixel.Bounds, !(countdown.TotalSeconds > safeCountdown), menu.TexPixel);
                SimpleButton.Instance.Draw(spriteBatch, menu.FontCountdown, text, new Vector2(Settings.Instance.ResolutionX / 2, Settings.Instance.ResolutionY / 2) - (size / 2), !(countdown.TotalSeconds > safeCountdown), menu.TexPixel);
            }
        }

        /* Helper */

        private int GetNextFreeColorIndex(int start)
        {
            var usedColors = Settings.Instance.GetPlayerSettingSelection(x=>x.ColorIndex);
            while (start >= Player.Colors.Length || usedColors.Contains(start))
            {
                if(start++ >= Player.Colors.Length)
                    start = 0;
            }
            return start;
        }

        private int GetPreviousFreeColorIndex(int start)
        {
            while (Settings.Instance.GetPlayerSettingSelection(x => x.ColorIndex).Contains(start) || start < 0)
            {
                if (start-- < 0) start = Player.Colors.Length - 1;
            }
            return start;
        }

        // if all slots are full -1 is returned
        private int GetFreeSlotIndex()
        {
            int i = 0;
            while (i < 4 && playerSlotOccupied[i])
                i++;
            return i == 4 ? -1 : i;
        }

        private bool isActive(InputManager.ControlActions action, int index)
        {
            return InputManager.Instance.SpecificActionButtonPressed(action, Settings.Instance.GetPlayer(index).ControlType, true) && !playerReadyBySlot[index];
        }

        private int isActive(InputManager.ControlActions action, int index, int offset)
        {
            return isActive(action, index) ? 0 : offset;
        }

        private void CheckStartCountdown()
        {
            bool allReady = playerSlotOccupied.Count(x => x) > 1;
            for (int i = 0; i < 4; i++)
            {
                if (playerSlotOccupied[i] != playerReadyBySlot[i])
                    allReady = false;
            }
            if (allReady && Settings.Instance.NumPlayers > 0)
                countdown = TimeSpan.FromSeconds(maxCountdown - 0.001);
        }

        private void ToggleReady(int slotIndex)
        {
            // toggle ready
            if (countdown.TotalSeconds > safeCountdown || countdown.TotalSeconds <= 0)
                playerReadyBySlot[slotIndex] = !playerReadyBySlot[slotIndex];

            // countdown
            if (playerReadyBySlot[slotIndex] && countdown.TotalSeconds <= 0)
                CheckStartCountdown();
            else if (countdown.TotalSeconds > safeCountdown)
                countdown = TimeSpan.FromSeconds(-1);
        }

        private void drawBackground(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
        {
            int padding = 20;
            spriteBatch.Draw(menu.TexPixel, new Rectangle((int)position.X - padding, (int)position.Y - padding, (int)size.X + 2 * padding, (int)size.Y + 2 * padding), Color.Black);
        }
    }
}
