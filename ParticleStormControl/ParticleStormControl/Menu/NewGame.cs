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
using Microsoft.Xna.Framework.Audio;

namespace ParticleStormControl.Menu
{
    class NewGame : MenuPage
    {
        const int VIRUS_SIZE = 110;
        const int VIRUS_PADDING = 10;
        const int ARROW_VERTICAL_SIZE = 8;
        const int ARROW_WIDDEN = 8;
        const int BOX_WIDTH = 1024 / 2 - 60;
        const int BOX_HEIGHT = 768 / 2 - 60;

        private const int maxCountdown = 3;
        private const int safeCountdown = 1;

        private bool[] playerSlotOccupied = new bool[4];
        private bool[] playerReadyBySlot = new bool[4];
        private int[] slotIndexToPlayerIndexMapper = new int[4];

        private Effect virusRenderEffect;
        private SoundEffect addPlayerSoundEffect;

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

            // four boxes
            int TEXTBOX_HEIGHT = menu.GetFontHeight() + 2 * InterfaceButton.PADDING;
            int SIDE_PADDING = TEXTBOX_HEIGHT + 30;
            int ARROW_SIZE = menu.GetFontHeight();

            string joinText = "< press continue to join game >";
            Vector2 stringSize = menu.Font.MeasureString(joinText);

            for (int i = 0; i < 4; i++)
            {
                int index = i;
                Vector2 origin = GetOrigin(index);

                // join text
                Interface.Add(new InterfaceButton(joinText, GetOrigin(index) + new Vector2((int)((BOX_WIDTH - stringSize.X) / 2), (int)((BOX_HEIGHT - stringSize.Y) / 2)), () => { return true; }, () => { return !playerSlotOccupied[index]; }));

                // virus name
                Interface.Add(new InterfaceButton(
                    () => { return VirusSwarm.VirusNames[Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).VirusIndex].ToString(); },
                    origin + new Vector2(SIDE_PADDING, 0),
                    () => { return playerReadyBySlot[index]; },
                    () => { return playerSlotOccupied[index]; },
                    true
                ));

                // controls
                Interface.Add(new InterfaceButton(
                    () => { return slotIndexToPlayerIndexMapper[index] < Settings.Instance.NumPlayers ? 
                                        InputManager.CONTROL_NAMES[(int)Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).ControlType].ToString() : ""; },
                    origin + new Vector2(SIDE_PADDING, TEXTBOX_HEIGHT * 2 + InterfaceButton.PADDING * 2),
                    () => { return false; },
                    () => { return playerSlotOccupied[index]; }
                ));

                // ready
                int readyHeight = TEXTBOX_HEIGHT * 4;
                Interface.Add(new InterfaceButton(
                    () => { return playerReadyBySlot[index] ? "ready!" : "not ready"; },
                    origin + new Vector2(SIDE_PADDING + TEXTBOX_HEIGHT, readyHeight),
                    () => { return playerReadyBySlot[index]; },
                    () => { return playerSlotOccupied[index]; }
                ));
                Interface.Add(new InterfaceImageButton(
                    "icons",
                    new Rectangle((int)origin.X + SIDE_PADDING, (int)origin.Y + readyHeight, ARROW_SIZE, ARROW_SIZE),
                    new Rectangle(0, 32, 16, 16),
                    new Rectangle(48, 32, 16, 16),
                    () => { return playerReadyBySlot[index]; },
                    () => { return playerSlotOccupied[index]; }
                ));

                // arrows up & down
                Rectangle virusImageRect = new Rectangle((int)origin.X + BOX_WIDTH - VIRUS_SIZE - SIDE_PADDING, (int)origin.Y + TEXTBOX_HEIGHT + 40, VIRUS_SIZE, VIRUS_SIZE);
                Interface.Add(new InterfaceImageButton(
                    "icons",
                    new Rectangle(virusImageRect.Center.X - (ARROW_VERTICAL_SIZE + ARROW_WIDDEN) / 2 - InterfaceElement.PADDING, virusImageRect.Top - InterfaceElement.PADDING - 2 * ARROW_VERTICAL_SIZE, ARROW_VERTICAL_SIZE + ARROW_WIDDEN, ARROW_VERTICAL_SIZE),
                    new Rectangle(0, 16, 16, 16),
                    new Rectangle(32, 16, 16, 16),
                    () => { return isActive(InputManager.ControlActions.UP, index); },
                    () => { return playerSlotOccupied[index]; }
                ));
                Interface.Add(new InterfaceImageButton(
                    "icons",
                    new Rectangle(virusImageRect.Center.X - (ARROW_VERTICAL_SIZE + ARROW_WIDDEN) / 2 - InterfaceElement.PADDING, virusImageRect.Bottom - InterfaceElement.PADDING + ARROW_VERTICAL_SIZE, ARROW_VERTICAL_SIZE + ARROW_WIDDEN, ARROW_VERTICAL_SIZE),
                    new Rectangle(16, 16, 16, 16),
                    new Rectangle(48, 16, 16, 16),
                    () => { return isActive(InputManager.ControlActions.DOWN, index); },
                    () => { return playerSlotOccupied[index]; }
                ));

                // arrows left & right
                int arrowY = (int)readyHeight - ARROW_SIZE;
                Interface.Add(new InterfaceImageButton(
                    "icons",
                    new Rectangle((int)origin.X, (int)origin.Y + arrowY, ARROW_SIZE, ARROW_SIZE),
                    new Rectangle(0, 0, 16, 16),
                    new Rectangle(32, 0, 16, 16),
                    () => { return isActive(InputManager.ControlActions.LEFT, index); },
                    () => { return playerSlotOccupied[index]; }
                ));
                Interface.Add(new InterfaceImageButton(
                    "icons",
                    new Rectangle((int)origin.X + BOX_WIDTH - ARROW_SIZE, (int)origin.Y + arrowY, ARROW_SIZE, ARROW_SIZE),
                    new Rectangle(16, 0, 16, 16),
                    new Rectangle(48, 0, 16, 16),
                    () => { return isActive(InputManager.ControlActions.RIGHT, index); },
                    () => { return playerSlotOccupied[index]; }
                ));

                // description
                int descpStrLen = (int)menu.Font.MeasureString("Discipline").X + 7 + InterfaceElement.PADDING;
                int symbolLen = (int)menu.Font.MeasureString("++++").X + InterfaceElement.PADDING;
                int backgroundLength = (BOX_WIDTH - SIDE_PADDING * 2 + InterfaceElement.PADDING * 2) / 2; //(descpStrLen + symbolLen) * 2 + 15;
                int descpX0 = SIDE_PADDING;
                int descpX1 = descpX0 + backgroundLength;
                float descpY = BOX_HEIGHT - TEXTBOX_HEIGHT * 2;

                Interface.Add(new InterfaceButton("Speed", origin + new Vector2(descpX0, descpY), () => { return false; }, () => { return playerSlotOccupied[index]; }));
                Interface.Add(new InterfaceButton(() => { return playerSlotOccupied[index] ? VirusSwarm.DESCRIPTOR_Speed[Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).VirusIndex] : ""; }, origin + new Vector2(descpX0 + descpStrLen, descpY), () => { return false; }, () => { return playerSlotOccupied[index]; }));

                Interface.Add(new InterfaceButton("Mass", origin + new Vector2(descpX0, descpY + TEXTBOX_HEIGHT), () => { return false; }, () => { return playerSlotOccupied[index]; }));
                Interface.Add(new InterfaceButton(() => { return playerSlotOccupied[index] ? VirusSwarm.DESCRIPTOR_Mass[Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).VirusIndex] : ""; }, origin + new Vector2(descpX0 + descpStrLen, descpY + TEXTBOX_HEIGHT), () => { return false; }, () => { return playerSlotOccupied[index]; }));

                Interface.Add(new InterfaceButton("Discipline", origin + new Vector2(descpX1, descpY), () => { return false; }, () => { return playerSlotOccupied[index]; }));
                Interface.Add(new InterfaceButton(() => { return playerSlotOccupied[index] ? VirusSwarm.DESCRIPTOR_Discipline[Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).VirusIndex] : ""; }, origin + new Vector2(descpX1 + descpStrLen, descpY), () => { return false; }, () => { return playerSlotOccupied[index]; }));

                Interface.Add(new InterfaceButton("Health", origin + new Vector2(descpX1, descpY + TEXTBOX_HEIGHT), () => { return false; }, () => { return playerSlotOccupied[index]; }));
                Interface.Add(new InterfaceButton(() => { return playerSlotOccupied[index] ? VirusSwarm.DESCRIPTOR_Health[Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).VirusIndex] : ""; }, origin + new Vector2(descpX1 + descpStrLen, descpY + TEXTBOX_HEIGHT), () => { return false; }, () => { return playerSlotOccupied[index]; }));
                
                // big fat "comp" for those who need it
                //stringSize = menu.FontCountdown.MeasureString("COMP");
                //Interface.Add(new InterfaceButton("COMP", origin + new Vector2(BOX_WIDTH, BOX_HEIGHT) / 2, () => { return false; }, () => { return playerSlotOccupied[index] && Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).Type == Player.Type.AI; }));
            }

            // countdown
            String text = "game starts in " + ((int)countdown.TotalSeconds + 1).ToString() + "...";
            Vector2 size = menu.FontHeading.MeasureString(text);
            Interface.Add(new InterfaceFiller(Vector2.Zero, Settings.Instance.ResolutionX, Settings.Instance.ResolutionY, Color.FromNonPremultiplied(0, 0, 0, 128), () => { return countdown.TotalSeconds > 0; }));
            Interface.Add(new InterfaceFiller(new Vector2(0, Settings.Instance.ResolutionY / 2 - (int)(size.Y)), Settings.Instance.ResolutionX, (int)(size.Y * 2.75f), Color.White, () => { return countdown.TotalSeconds > 0; }));
            Interface.Add(new InterfaceFiller(new Vector2(0, Settings.Instance.ResolutionY / 2 - (int)(size.Y)), Settings.Instance.ResolutionX, (int)(size.Y * 2.75f), Color.Black, () => { return countdown.TotalSeconds > safeCountdown; }));
            Interface.Add(new InterfaceButton(() => { return "game starts in " + ((int)countdown.TotalSeconds + 1).ToString() + "..."; }, new Vector2(Settings.Instance.ResolutionX / 2, Settings.Instance.ResolutionY / 2) - (size / 2), () => { return !(countdown.TotalSeconds > safeCountdown); }, () => { return countdown.TotalSeconds > 0; }, true));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldPage"></param>
        /// <param name="gameTime"></param>
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
            addPlayerSoundEffect = content.Load<SoundEffect>("sound/room__snare-switchy");

            base.LoadContent(content);
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

            base.Update(gameTime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ai"></param>
        /// <param name="controlType"></param>
        /// <returns></returns>
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

                addPlayerSoundEffect.Play();
                countdown = TimeSpan.FromSeconds(-1);
            }

            return slotIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slot"></param>
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
            // four boxes
            int TEXTBOX_HEIGHT = menu.GetFontHeight() + 2 * InterfaceButton.PADDING;
            int ARROW_SIZE = TEXTBOX_HEIGHT;
            int SIDE_PADDING = ARROW_SIZE + 30;

            virusRenderEffect.Parameters["ScreenSize"].SetValue(new Vector2(menu.ScreenWidth, menu.ScreenHeight));

            // virus image for each player
            for (int i = 0; i < 4; i++)
            {
                if (playerSlotOccupied[i])
                {
                    Vector2 origin = GetOrigin(i);
                    int playerIndex = slotIndexToPlayerIndexMapper[i];
                    
                    Rectangle virusImageRect = new Rectangle((int)origin.X + BOX_WIDTH - VIRUS_SIZE - SIDE_PADDING, (int)origin.Y + TEXTBOX_HEIGHT + 40, VIRUS_SIZE, VIRUS_SIZE);
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
                }
            }

            base.Draw(spriteBatch, gameTime);

            // countdown
            if (countdown.TotalSeconds > 0 && countdown.TotalSeconds < InGame.GAME_BLEND_DURATION)
            {
                // last half ? draw black fade
                float blend = (float)(InGame.GAME_BLEND_DURATION - countdown.TotalSeconds) / InGame.GAME_BLEND_DURATION;
                spriteBatch.Draw(menu.TexPixel, new Rectangle(0, 0, menu.ScreenWidth, menu.ScreenHeight), Color.Black * blend);
            }
        }

        #region helper methods

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
            return playerSlotOccupied[index] ? InputManager.Instance.SpecificActionButtonPressed(action, Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).ControlType, true) && !playerReadyBySlot[slotIndexToPlayerIndexMapper[index]] : false;
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
            else
                countdown = TimeSpan.FromSeconds(-1);
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

        private Vector2 GetOrigin(int i)
        {
            switch (i)
            {
                case 3:
                    return new Vector2(Settings.Instance.ResolutionX / 4 - BOX_WIDTH / 2, Settings.Instance.ResolutionY / 4 - BOX_HEIGHT / 2);
                case 2:
                    return new Vector2(Settings.Instance.ResolutionX / 4 * 3 - BOX_WIDTH / 2, Settings.Instance.ResolutionY / 4 * 3 - BOX_HEIGHT / 2);
                case 1:
                    return new Vector2(Settings.Instance.ResolutionX / 4 * 3 - BOX_WIDTH / 2, Settings.Instance.ResolutionY / 4 - BOX_HEIGHT / 2);
                case 0:
                    return new Vector2(Settings.Instance.ResolutionX / 4 - BOX_WIDTH / 2, Settings.Instance.ResolutionY / 4 * 3 - BOX_HEIGHT / 2);
                default:
                    return Vector2.Zero;
            }
        }

        #endregion
    }
}
