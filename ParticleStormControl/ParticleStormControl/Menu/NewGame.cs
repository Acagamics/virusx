//#define QUICK_TWO_PLAYER_DEBUG
//#define QUICK_FOUR_PLAYER_DEBUG
//#define QUICK_CTC_DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Game = global::VirusX.InGame;

namespace VirusX.Menu
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
        private int[] preSlotMapper = new int[7];

        private readonly Color fontColor = Color.Black;
        private TimeSpan countdown = new TimeSpan();

        private int numSlots = 4;

        //private InterfaceImage[] virusImages = new InterfaceImage[4];
        private Effect virusRenderEffect;

        /// <summary>
        /// reference to the content manager
        /// needed because the "lazy load" of all these items
        /// </summary>
        private ContentManager content;

        public NewGame(Menu menu)
            : base(menu)
        {
            countdown = new TimeSpan();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldPage"></param>
        /// <param name="gameTime"></param>
        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            // if play again "unreadify" all human players
            if (oldPage == Menu.Page.STATS)
            {
                for (int i = 0; i < 4; ++i)
                {
                    if(Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[i]) != null
                        && Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[i]).Type == Player.Type.HUMAN)
                    {
                        playerReadyBySlot[i] = false;
                    }
                }
            }

            // reset all slots if not play again
            if (oldPage != Menu.Page.CONTROLS && oldPage != Menu.Page.STATS)
            {
                Settings.Instance.ResetPlayerSettings();
                countdown = TimeSpan.Zero;
                for (int i = 0; i < 4; ++i)
                {
                    playerSlotOccupied[i] = false;
                    playerReadyBySlot[i] = false;
                    slotIndexToPlayerIndexMapper[i] = i;
                }

                // distribute ControlTypes all over the place
                if (Settings.Instance.GameMode != Game.GameMode.ARCADE)
                {
                    preSlotMapper[0] = 0;
                    preSlotMapper[1] = 1;
                    preSlotMapper[2] = 2;
                    preSlotMapper[3] = 0;
                    preSlotMapper[4] = 1;
                    preSlotMapper[5] = 2;
                    preSlotMapper[6] = 3;
                }
                else
                {
                    for (int i = 0; i < preSlotMapper.Length; ++i)
                        preSlotMapper[i] = 0;
                }

                //if (Settings.Instance.StartingControls != InputManager.ControlType.NONE)
                //    AddPlayer(false, Settings.Instance.StartingControls);

                // create ui @ onActia
                Interface.Clear();

                // num slots
                if (Settings.Instance.GameMode == Game.GameMode.ARCADE)
                    numSlots = 1;
                else
                    numSlots = 4;

                // boxes
                int TEXTBOX_HEIGHT = menu.GetFontHeight() + 2 * InterfaceButton.PADDING;
                int SIDE_PADDING = TEXTBOX_HEIGHT + 30;
                int ARROW_SIZE = menu.GetFontHeight();

                for (int i = 0; i < numSlots; i++)
                {
                    int index = i;
                    Vector2 origin = GetOrigin(index);

                    // team divider
                    int thickness = 10;
                    int offset = 30;
                    Interface.Add(new InterfaceFiller(new Vector2(Settings.Instance.ResolutionX / 2, 0), thickness, Settings.Instance.ResolutionY, InterfaceElement.COLOR_HIGHLIGHT, () => { return Settings.Instance.GameMode == Game.GameMode.LEFT_VS_RIGHT; }));
                    Interface.Add(new InterfaceFiller(new Vector2(0, Settings.Instance.ResolutionY / 2 - offset), Settings.Instance.ResolutionX / 2, thickness, InterfaceElement.COLOR_HIGHLIGHT, () => { return Settings.Instance.GameMode == Game.GameMode.CAPTURE_THE_CELL; }));
                    Interface.Add(new InterfaceFiller(new Vector2(Settings.Instance.ResolutionX / 2, Settings.Instance.ResolutionY / 2 - offset), thickness, Settings.Instance.ResolutionY / 2 + offset, InterfaceElement.COLOR_HIGHLIGHT, () => { return Settings.Instance.GameMode == Game.GameMode.CAPTURE_THE_CELL; }));


                    // join text
                    Vector2 stringSize = menu.Font.MeasureString(GetJoinText(index));
                    Interface.Add(new InterfaceButton(GetJoinText(index), GetOrigin(index) + new Vector2((int)((BOX_WIDTH - stringSize.X) / 2), (int)((BOX_HEIGHT - stringSize.Y) / 2)), () => { return true; }, () => { return !playerSlotOccupied[index]; }));

                    // virus name
                    Interface.Add(new InterfaceButton(
                        () => { return VirusSwarm.VirusNames[(int)Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).Virus].ToString(); },
                        origin + new Vector2(SIDE_PADDING, 0),
                        () => { return playerReadyBySlot[index]; },
                        () => { return playerSlotOccupied[index]; },
                        true
                    ));

                    // teams
                    Interface.Add(new InterfaceButton(
                        () =>
                        { return Player.TEAM_NAMES[(int)Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).Team]; },
                        origin + new Vector2(SIDE_PADDING, TEXTBOX_HEIGHT * 2 - InterfaceElement.PADDING),
                        () => { return false; },
                        () => { return playerSlotOccupied[index] && Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).Team != Player.Teams.NONE; }
                    ));

                    // controls
                    Interface.Add(new InterfaceButton(
                        () => { return InputManager.CONTROL_NAMES[(int)Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).ControlType]; },
                        origin + new Vector2(SIDE_PADDING, TEXTBOX_HEIGHT * 3),
                        () => { return false; },
                        () => { return playerSlotOccupied[index]; }
                    ));

                    // ready
                    int top = TEXTBOX_HEIGHT * 4 + InterfaceElement.PADDING;
                    Interface.Add(new InterfaceButton(
                        () => { return playerReadyBySlot[index] ? VirusXStrings.NewGameReady : VirusXStrings.NewGameNotReady; },
                        origin + new Vector2(SIDE_PADDING + TEXTBOX_HEIGHT, top),
                        () => { return playerReadyBySlot[index]; },
                        () => { return playerSlotOccupied[index]; }
                    ));
                    Interface.Add(new InterfaceImageButton(
                        "icons",
                        new Rectangle((int)origin.X + SIDE_PADDING, (int)origin.Y + top, ARROW_SIZE, ARROW_SIZE),
                        new Rectangle(0, 32, 16, 16),
                        new Rectangle(48, 32, 16, 16),
                        () => { return playerReadyBySlot[index]; },
                        () => { return playerSlotOccupied[index]; }
                    ));

                    // arrows up & down
                    if (Settings.Instance.GameMode != Game.GameMode.LEFT_VS_RIGHT)
                    {
                        Rectangle virusImageRect = new Rectangle((int)origin.X + BOX_WIDTH - VIRUS_SIZE - SIDE_PADDING, (int)origin.Y + TEXTBOX_HEIGHT + 40, VIRUS_SIZE, VIRUS_SIZE);
                        Interface.Add(new InterfaceImageButton(
                            "icons",
                            new Rectangle(virusImageRect.Center.X - (ARROW_VERTICAL_SIZE + ARROW_WIDDEN) / 2 - InterfaceElement.PADDING, virusImageRect.Top - InterfaceElement.PADDING - 2 * ARROW_VERTICAL_SIZE, ARROW_VERTICAL_SIZE + ARROW_WIDDEN, ARROW_VERTICAL_SIZE),
                            new Rectangle(0, 16, 16, 16),
                            new Rectangle(32, 16, 16, 16),
                            () => { return isActive(InputManager.ControlActions.UP, index); },
                            () => { return playerSlotOccupied[index] && Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).Type == Player.Type.HUMAN; }
                        ));
                        Interface.Add(new InterfaceImageButton(
                            "icons",
                            new Rectangle(virusImageRect.Center.X - (ARROW_VERTICAL_SIZE + ARROW_WIDDEN) / 2 - InterfaceElement.PADDING, virusImageRect.Bottom - InterfaceElement.PADDING + ARROW_VERTICAL_SIZE, ARROW_VERTICAL_SIZE + ARROW_WIDDEN, ARROW_VERTICAL_SIZE),
                            new Rectangle(16, 16, 16, 16),
                            new Rectangle(48, 16, 16, 16),
                            () => { return isActive(InputManager.ControlActions.DOWN, index); },
                            () => { return playerSlotOccupied[index] && Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).Type == Player.Type.HUMAN; }
                        ));
                    }

                    // arrows left & right
                    int arrowY = TEXTBOX_HEIGHT * 4 - ARROW_SIZE;
                    Interface.Add(new InterfaceImageButton(
                        "icons",
                        new Rectangle((int)origin.X, (int)origin.Y + arrowY, ARROW_SIZE, ARROW_SIZE),
                        new Rectangle(0, 0, 16, 16),
                        new Rectangle(32, 0, 16, 16),
                        () => { return isActive(InputManager.ControlActions.LEFT, index); },
                        () => { return playerSlotOccupied[index] && Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).Type == Player.Type.HUMAN; }
                    ));
                    Interface.Add(new InterfaceImageButton(
                        "icons",
                        new Rectangle((int)origin.X + BOX_WIDTH - ARROW_SIZE, (int)origin.Y + arrowY, ARROW_SIZE, ARROW_SIZE),
                        new Rectangle(16, 0, 16, 16),
                        new Rectangle(48, 0, 16, 16),
                        () => { return isActive(InputManager.ControlActions.RIGHT, index); },
                        () => { return playerSlotOccupied[index] && Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).Type == Player.Type.HUMAN; }
                    ));

                    // virus
                    /* virusImages[i] = new InterfaceImage(Settings.Instance.NumPlayers > i ? ParticleRenderer.GetVirusTextureName(Settings.Instance.GetPlayer(i).Virus) : "pix",
                                             new Rectangle((int)origin.X + BOX_WIDTH - VIRUS_SIZE - SIDE_PADDING, (int)origin.Y + TEXTBOX_HEIGHT + 40, VIRUS_SIZE, VIRUS_SIZE),   // historic rect..
                                                             Color.Black, () => { return playerSlotOccupied[index]; }, Alignment.TOP_LEFT, true);
                     Interface.Add(virusImages[i]); */

                    // description
                    int backgroundLength = (BOX_WIDTH - SIDE_PADDING * 2 + InterfaceElement.PADDING * 2) / 2; //(descpStrLen + symbolLen) * 2 + 15;
                    int symbolLen = backgroundLength - TEXTBOX_HEIGHT - InterfaceButton.PADDING * 3;//(int)menu.Font.MeasureString("++++").X + InterfaceElement.PADDING;
                    int descpX0 = SIDE_PADDING;
                    int descpX1 = descpX0 + backgroundLength;
                    int descpY = BOX_HEIGHT - TEXTBOX_HEIGHT * 2;


                    Interface.Add(new InterfaceImage("symbols/speed", new Rectangle((int)origin.X + descpX0, (int)origin.Y + descpY, TEXTBOX_HEIGHT, TEXTBOX_HEIGHT),
                                                        Color.Black, () => { return playerSlotOccupied[index]; }, Alignment.TOP_LEFT, false));
                    Interface.Add(new InterfaceButton(() => { return VirusSwarm.DESCRIPTOR_Speed[(int)Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).Virus]; },
                                origin + new Vector2(descpX0 + TEXTBOX_HEIGHT, descpY), () => { return false; }, () => { return playerSlotOccupied[index]; }, symbolLen));

                    Interface.Add(new InterfaceImage("symbols/mass", new Rectangle((int)origin.X + descpX0, (int)origin.Y + descpY + TEXTBOX_HEIGHT, TEXTBOX_HEIGHT, TEXTBOX_HEIGHT),
                                                        Color.Black, () => { return playerSlotOccupied[index]; }, Alignment.TOP_LEFT, false));
                    Interface.Add(new InterfaceButton(() => { return VirusSwarm.DESCRIPTOR_Mass[(int)Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).Virus]; },
                                  origin + new Vector2(descpX0 + TEXTBOX_HEIGHT, descpY + TEXTBOX_HEIGHT), () => { return false; }, () => { return playerSlotOccupied[index]; }, symbolLen));

                    Interface.Add(new InterfaceImage("symbols/discipline", new Rectangle((int)origin.X + descpX1, (int)origin.Y + descpY, TEXTBOX_HEIGHT, TEXTBOX_HEIGHT),
                                                        Color.Black, () => { return playerSlotOccupied[index]; }, Alignment.TOP_LEFT, false));
                    Interface.Add(new InterfaceButton(() => { return VirusSwarm.DESCRIPTOR_Discipline[(int)Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).Virus]; },
                                    origin + new Vector2(descpX1 + TEXTBOX_HEIGHT, descpY), () => { return false; }, () => { return playerSlotOccupied[index]; }, symbolLen));

                    Interface.Add(new InterfaceImage("symbols/health", new Rectangle((int)origin.X + descpX1, (int)origin.Y + descpY + TEXTBOX_HEIGHT, TEXTBOX_HEIGHT, TEXTBOX_HEIGHT),
                                                        Color.Black, () => { return playerSlotOccupied[index]; }, Alignment.TOP_LEFT, false));
                    Interface.Add(new InterfaceButton(() => { return VirusSwarm.DESCRIPTOR_Health[(int)Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).Virus]; },
                                    origin + new Vector2(descpX1 + TEXTBOX_HEIGHT, descpY + TEXTBOX_HEIGHT), () => { return false; }, () => { return playerSlotOccupied[index]; }, symbolLen));
                }
                for (int i = numSlots; i < playerSlotOccupied.Length; i++)
                    playerReadyBySlot[i] = playerSlotOccupied[i] = true;

                int textBoxHeight = menu.GetFontHeight() + 2 * InterfaceElement.PADDING;
                if (numSlots > 1)
                {
                    // help text pad
                    Interface.Add(new InterfaceImage("ButtonImages/xboxControllerLeftShoulder", new Rectangle(-395, textBoxHeight, 100, textBoxHeight), Color.Black, () => !InputManager.IsKeyboardControlType(Settings.Instance.StartingControls), Alignment.BOTTOM_CENTER));
                    Interface.Add(new InterfaceButton(VirusXStrings.NewGameRemoveComputer, new Vector2(-295, textBoxHeight), () => false, () => !InputManager.IsKeyboardControlType(Settings.Instance.StartingControls), 180, Alignment.BOTTOM_CENTER));
                    Interface.Add(new InterfaceImage("ButtonImages/xboxControllerRightShoulder", new Rectangle(-115, textBoxHeight, 100, textBoxHeight), Color.Black, () => !InputManager.IsKeyboardControlType(Settings.Instance.StartingControls), Alignment.BOTTOM_CENTER));
                    Interface.Add(new InterfaceButton(VirusXStrings.NewGameAddComputer, new Vector2(-15, textBoxHeight), () => false, () => !InputManager.IsKeyboardControlType(Settings.Instance.StartingControls), 180, Alignment.BOTTOM_CENTER));
                    Interface.Add(new InterfaceImage("ButtonImages/xboxControllerButtonY", new Rectangle(165, textBoxHeight, 50, textBoxHeight), Color.Black, () => !InputManager.IsKeyboardControlType(Settings.Instance.StartingControls), Alignment.BOTTOM_CENTER));
                    Interface.Add(new InterfaceButton(VirusXStrings.NewGameShowControls, new Vector2(215, textBoxHeight), () => false, () => !InputManager.IsKeyboardControlType(Settings.Instance.StartingControls), 180, Alignment.BOTTOM_CENTER));

                    // help text keyboard
                    Interface.Add(new InterfaceButton("  -", new Vector2(-345, textBoxHeight), () => true, () => InputManager.IsKeyboardControlType(Settings.Instance.StartingControls), 50, Alignment.BOTTOM_CENTER));
                    Interface.Add(new InterfaceButton(VirusXStrings.NewGameRemoveComputer, new Vector2(-295, textBoxHeight), () => false, () => InputManager.IsKeyboardControlType(Settings.Instance.StartingControls), 180, Alignment.BOTTOM_CENTER));
                    Interface.Add(new InterfaceButton("  +", new Vector2(-115, textBoxHeight), () => true, () => InputManager.IsKeyboardControlType(Settings.Instance.StartingControls), 50, Alignment.BOTTOM_CENTER));
                    Interface.Add(new InterfaceButton(VirusXStrings.NewGameAddComputer, new Vector2(-65, textBoxHeight), () => false, () => InputManager.IsKeyboardControlType(Settings.Instance.StartingControls), 180, Alignment.BOTTOM_CENTER));
                    Interface.Add(new InterfaceButton(" F1", new Vector2(115, textBoxHeight), () => true, () => InputManager.IsKeyboardControlType(Settings.Instance.StartingControls), 50, Alignment.BOTTOM_CENTER));
                    Interface.Add(new InterfaceButton(VirusXStrings.NewGameShowControls, new Vector2(165, textBoxHeight), () => false, () => InputManager.IsKeyboardControlType(Settings.Instance.StartingControls), 180, Alignment.BOTTOM_CENTER));
                }
                else
                {
                    // help text pad
                    Interface.Add(new InterfaceImage("ButtonImages/xboxControllerButtonY", new Rectangle(-115, textBoxHeight, 100, textBoxHeight), Color.Black, () => !InputManager.IsKeyboardControlType(Settings.Instance.StartingControls), Alignment.BOTTOM_CENTER));
                    Interface.Add(new InterfaceButton(VirusXStrings.NewGameShowControls, new Vector2(-15, textBoxHeight), () => false, () => !InputManager.IsKeyboardControlType(Settings.Instance.StartingControls), 180, Alignment.BOTTOM_CENTER));
                
                    // help text keyboard
                    Interface.Add(new InterfaceButton("F1", new Vector2(-115, textBoxHeight), () => true, () => InputManager.IsKeyboardControlType(Settings.Instance.StartingControls), 50, Alignment.BOTTOM_CENTER));
                    Interface.Add(new InterfaceButton(VirusXStrings.NewGameShowControls, new Vector2(-65, textBoxHeight), () => false, () => InputManager.IsKeyboardControlType(Settings.Instance.StartingControls), 180, Alignment.BOTTOM_CENTER));
                }

                // preSlotImages
                for (int i = 0; i < 4; i++)
                {
                    foreach (InputManager.ControlType value in Enum.GetValues(typeof(InputManager.ControlType)))
                    {
                        if (value != InputManager.ControlType.NONE)
                        {
                            int x = i;
                            Interface.Add(new InterfaceImageButton("controltypes", new Rectangle((int)GetPreSlotPosition(value, i).X, (int)GetPreSlotPosition(value, i).Y, 96, 96), new Rectangle((int)value * 96, 0, 96, 96), new Rectangle(7 * 96, 0, 96, 96), () => { return preSlotMapper[(int)value] != x; }, () => { return !playerSlotOccupied[x]; }, Color.FromNonPremultiplied(255, 255, 255, 1)));
                        }
                    }
                }

                // countdown
                String text = VirusXStrings.NewGameStartsIn + ((int)countdown.TotalSeconds + 1).ToString() + "...";
                Vector2 size = menu.FontHeading.MeasureString(text);
                Interface.Add(new InterfaceFiller(Vector2.Zero, Settings.Instance.ResolutionX, Settings.Instance.ResolutionY, Color.FromNonPremultiplied(0, 0, 0, 128), () => { return countdown.TotalSeconds > 0; }));
                Interface.Add(new InterfaceFiller(new Vector2(0, Settings.Instance.ResolutionY / 2 - (int)(size.Y)), Settings.Instance.ResolutionX, (int)(size.Y * 2.75f), Color.White, () => { return countdown.TotalSeconds > 0; }));
                Interface.Add(new InterfaceFiller(new Vector2(0, Settings.Instance.ResolutionY / 2 - (int)(size.Y)), Settings.Instance.ResolutionX, (int)(size.Y * 2.75f), Color.Black, () => { return countdown.TotalSeconds > safeCountdown; }));
                InterfaceButton countdownButton = new InterfaceButton(() => { return VirusXStrings.NewGameStartsIn + ((int)countdown.TotalSeconds + 1).ToString() + "..."; }, new Vector2(Settings.Instance.ResolutionX / 2, Settings.Instance.ResolutionY / 2) - (size / 2), () => { return !(countdown.TotalSeconds > safeCountdown); }, () => { return countdown.TotalSeconds > 0; }, true);
                countdownButton.Silent = true;
                Interface.Add(countdownButton);

                base.LoadContent(content);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        public override void LoadContent(ContentManager content)
        {
            this.content = content;
            virusRenderEffect = content.Load<Effect>("shader/particleRendering");
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
            Settings.Instance.ResetPlayerSettings();
            Settings.Instance.UseItems = false;
            Settings.Instance.AddPlayer(new Settings.PlayerSettings()
            {
                ColorIndex = 0,
                ControlType = InputManager.ControlType.NONE,
                SlotIndex = 0,
                Team = Player.Teams.NONE,
                Type = Player.Type.AI,
                Virus = VirusSwarm.VirusType.EBOLA
            });
            Settings.Instance.AddPlayer(new Settings.PlayerSettings()
            {
                ColorIndex = 1,
                ControlType = InputManager.ControlType.NONE,
                SlotIndex = 1,
                Team = Player.Teams.NONE,
                Type = Player.Type.AI,
                Virus = VirusSwarm.VirusType.EPSTEINBARR
            });
            Settings.Instance.AddPlayer(new Settings.PlayerSettings()
            {
                ColorIndex = 2,
                ControlType = InputManager.ControlType.NONE,
                SlotIndex = 2,
                Team = Player.Teams.NONE,
                Type = Player.Type.AI,
                Virus = VirusSwarm.VirusType.H5N1
            });
            Settings.Instance.AddPlayer(new Settings.PlayerSettings()
            {
                ColorIndex = 3,
                ControlType = InputManager.ControlType.NONE,
                SlotIndex = 3,
                Team = Player.Teams.NONE,
                Type = Player.Type.AI,
                Virus = VirusSwarm.VirusType.HEPATITISB
            });
            //playerReady[0] = playerReady[1] = playerReady[2] = playerReady[3] = playerConnected[0] = playerConnected[1] = playerConnected[2] = playerConnected[3] = Settings.Instance.PlayerConnected[0] = Settings.Instance.PlayerConnected[1] = Settings.Instance.PlayerConnected[2] = Settings.Instance.PlayerConnected[3] = true;
            menu.ChangePage(Menu.Page.INGAME,gameTime);
#elif QUICK_CTC_DEBUG
            Settings.Instance.ResetPlayerSettings();
            Settings.Instance.AddPlayer(new Settings.PlayerSettings()
            {
                Team = Player.Teams.DEFENDER,
                SlotIndex = 0,
                ControlType = InputManager.ControlType.KEYBOARD0,
                VirusIndex = 0,
                ColorIndex = 0,
                Type = Player.Type.HUMAN
            });
            Settings.Instance.AddPlayer(new Settings.PlayerSettings()
            {
                Team = Player.Teams.ATTACKER,
                SlotIndex = 1,
                ControlType = InputManager.ControlType.KEYBOARD1,
                VirusIndex = 1,
                ColorIndex = 1,
                Type = Player.Type.HUMAN
            });
            Settings.Instance.AddPlayer(new Settings.PlayerSettings()
            {
                Team = Player.Teams.ATTACKER,
                SlotIndex = 2,
                ControlType = InputManager.ControlType.KEYBOARD2,
                VirusIndex = 2,
                ColorIndex = 2,
                Type = Player.Type.HUMAN
            });
            Settings.Instance.AddPlayer(new Settings.PlayerSettings()
            {
                Team = Player.Teams.ATTACKER,
                SlotIndex = 3,
                ControlType = InputManager.ControlType.GAMEPAD0,
                VirusIndex = 3,
                ColorIndex = 3,
                Type = Player.Type.HUMAN
            });
            Settings.Instance.GameMode = Game.GameMode.CAPTURE_THE_CELL;
            menu.ChangePage(Menu.Page.INGAME, gameTime);
#endif
            if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.EXIT, Settings.Instance.StartingControls) ||
                InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.EXIT, InputManager.ControlType.KEYBOARD0)) // escape brings always back   
            {
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);
                return;
            }

            TimeSpan oldCountdown = countdown;
            countdown = countdown.Subtract(gameTime.ElapsedGameTime);
            if (oldCountdown.TotalSeconds > 0 && countdown.TotalSeconds <= 0)
            {
                menu.ChangePage(Menu.Page.INGAME, gameTime);
                return;
            }

            // overlay controls screen
            if (InputManager.Instance.IsButtonPressed(Keys.F1) || InputManager.Instance.AnyPressedButton(Buttons.Y))
            {
                menu.ChangePage(Menu.Page.CONTROLS, gameTime);
                return;
            }

            // test various buttons for controls without players
            foreach (InputManager.ControlType controlType in Enum.GetValues(typeof(InputManager.ControlType)))
            {
                if (controlType != InputManager.ControlType.NONE)
                {
                    bool inputUsed = false;
                    for (int i = 0; i < Settings.Instance.NumPlayers; i++)
                    {
                        if (Settings.Instance.GetPlayer(i).ControlType == controlType)
                            inputUsed = true;
                    }
                    if (!inputUsed)
                    {
                        InputManager.ControlActions[] switchActions = new InputManager.ControlActions[] { InputManager.ControlActions.LEFT , InputManager.ControlActions.RIGHT,
                                                                                                          InputManager.ControlActions.UP, InputManager.ControlActions.DOWN };
                        foreach (var action in switchActions)
                        {
                            if (InputManager.Instance.SpecificActionButtonPressed(action, controlType, false))
                            {
                                preSlotMapper[(int)controlType] = SwitchPreSlots(preSlotMapper[(int)controlType], action);
                                break;
                            }
                        }
                    }
                }
            }


            // test various buttons if player has a slot
            for (int playerIndex = 0; playerIndex < Settings.Instance.NumPlayers; playerIndex++)
            {
                int slot = Settings.Instance.GetPlayer(playerIndex).SlotIndex;
                if (playerSlotOccupied[slot] && Settings.Instance.GetPlayer(playerIndex).Type == Player.Type.HUMAN)
                {
                    // virus
                    if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.LEFT, Settings.Instance.GetPlayer(playerIndex).ControlType, false) && !playerReadyBySlot[slot])
                    {
                        if (--Settings.Instance.GetPlayer(playerIndex).Virus < 0)
                            Settings.Instance.GetPlayer(playerIndex).Virus = (VirusSwarm.VirusType)((int)VirusSwarm.VirusType.NUM_VIRUSES - 1);

                        //virusImages[slot].Texture = content.Load<Texture2D>(ParticleRenderer.GetVirusTextureName(Settings.Instance.GetPlayer(playerIndex).Virus));
                    }
                    else if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.RIGHT, Settings.Instance.GetPlayer(playerIndex).ControlType, false) && !playerReadyBySlot[slot])
                    {
                        Settings.Instance.GetPlayer(playerIndex).Virus = (VirusSwarm.VirusType)((int)Settings.Instance.GetPlayer(playerIndex).Virus + 1);
                        if ((int)Settings.Instance.GetPlayer(playerIndex).Virus >= (int)VirusSwarm.VirusType.NUM_VIRUSES)
                            Settings.Instance.GetPlayer(playerIndex).Virus = 0;

                        //virusImages[slot].Texture = content.Load<Texture2D>(ParticleRenderer.GetVirusTextureName(Settings.Instance.GetPlayer(playerIndex).Virus));
                    }

                    // color
                    if (Settings.Instance.GameMode != Game.GameMode.LEFT_VS_RIGHT)
                    {
                        if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.UP, Settings.Instance.GetPlayer(playerIndex).ControlType, false) && !playerReadyBySlot[slot])
                            Settings.Instance.GetPlayer(playerIndex).ColorIndex = GetPreviousFreeColorIndex(Settings.Instance.GetPlayer(playerIndex).ColorIndex);
                        else if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.DOWN, Settings.Instance.GetPlayer(playerIndex).ControlType, false) && !playerReadyBySlot[slot])
                            Settings.Instance.GetPlayer(playerIndex).ColorIndex = GetNextFreeColorIndex(Settings.Instance.GetPlayer(playerIndex).ColorIndex);
                    }

                    // remove
                    if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.HOLD, Settings.Instance.GetPlayer(playerIndex).ControlType, false) ||
                        InputManager.Instance.IsWaitingForReconnect(Settings.Instance.GetPlayer(playerIndex).ControlType))
                    {
                        if (playerReadyBySlot[slot])
                            ToggleReady(slot);
                        else
                        {
                            //if (Settings.Instance.GetPlayer(playerIndex).ControlType == Settings.Instance.StartingControls)
                            //{
                            //    menu.ChangePage(Menu.Page.MAINMENU, gameTime);
                            //    return;
                            //}
                            //else
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
            InputManager.ControlType controlTypePressingContinue; 
            if(InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.ACTION, out controlTypePressingContinue))
            {
                if (!Settings.Instance.GetPlayerSettingSelection(x => x.ControlType).Contains(controlTypePressingContinue))
                    AddPlayer(false, controlTypePressingContinue);
            }

            // test add/remove ai
            if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.ADD_AI, Settings.Instance.StartingControls))
            {
                int index = AddPlayer(true, InputManager.ControlType.NONE);
                if (index != -1)    
                    ToggleReady(index);
            }
            else if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.REMOVE_AI, Settings.Instance.StartingControls))
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
            if (ai && Settings.Instance.GameMode == Game.GameMode.ARCADE)
                return -1;

            int slotIndex = -1;
            if(controlType == InputManager.ControlType.NONE)
                slotIndex = GetFreeSlotIndex(-1);
            else
                slotIndex = preSlotMapper[(int)controlType];
            int playerIndex = Settings.Instance.NumPlayers;
            if (slotIndex != -1)
            {
                int colorIndex;
                if (Settings.Instance.GameMode != Game.GameMode.LEFT_VS_RIGHT)
                    colorIndex = GetNextFreeColorIndex(0);
                else
                {
                    switch (slotIndex)  // see Player.Colors
                    {
                        case 0:
                            colorIndex = 0; // red
                            break;
                        case 1:
                            colorIndex = 2; // turquoise
                            break;
                        case 2:
                            colorIndex = 1; // blue
                            break;
                        default:
                            colorIndex = 4; // pink
                            break;
                    }
                }


                playerSlotOccupied[slotIndex] = true;
                playerReadyBySlot[slotIndex] = false;
                slotIndexToPlayerIndexMapper[slotIndex] = playerIndex;
                Player.Teams team = Player.Teams.NONE;

                switch (Settings.Instance.GameMode)
                {
                    case Game.GameMode.CAPTURE_THE_CELL:
                        if (slotIndex == 0)
                            team = Player.Teams.DEFENDER;
                        else
                            team = Player.Teams.ATTACKER;
                        break;
                    case Game.GameMode.LEFT_VS_RIGHT:
                        if (slotIndex == 0 || slotIndex == 3)
                            team = Player.Teams.LEFT;
                        else
                            team = Player.Teams.RIGHT;
                        break;
                }

                Settings.Instance.AddPlayer(new Settings.PlayerSettings()
                {
                    SlotIndex = slotIndex,
                    ControlType = ai ? InputManager.ControlType.NONE : controlType,
                    ColorIndex = colorIndex,
                    Team = team,
                    Virus = (VirusSwarm.VirusType)Random.Next((int)VirusSwarm.VirusType.NUM_VIRUSES),
                    Type = ai ? Player.Type.AI : Player.Type.HUMAN,
                });
                //if(virusImages[slotIndex] != null)
                //    virusImages[slotIndex].Texture = content.Load<Texture2D>(ParticleRenderer.GetVirusTextureName(Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[slotIndex]).Virus));

                CheckPreSlots();

                AudioManager.Instance.PlaySoundeffect("click");
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

            CheckPreSlots();

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
            for (int i = 0; i < numSlots; i++)
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
                    virusRenderEffect.CurrentTechnique = virusRenderEffect.Techniques["Virus_Spritebatch"];
                  //  virusRenderEffect.Parameters["VirusTexture"].SetValue());
                    var texture = content.Load<Texture2D>(ParticleRenderer.GetVirusTextureName(Settings.Instance.GetPlayer(playerIndex).Virus));
                    virusRenderEffect.Parameters["Color"].SetValue(VirusSwarm.ParticleColors[Settings.Instance.GetPlayer(playerIndex).ColorIndex].ToVector4() * 1.5f);
                    spriteBatch.Begin(0, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, virusRenderEffect);
                    spriteBatch.Draw(texture, virusImageRect, Color.White);
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
        private int GetFreeSlotIndex(int oldSlot)
        {
            switch (oldSlot)
	        {
		        case 0:
                    if(!playerSlotOccupied[2]) return 2;
                    if(!playerSlotOccupied[3]) return 3;
                    if(!playerSlotOccupied[1]) return 1;
                    break;
		        case 1:
                    if(!playerSlotOccupied[3]) return 3;
                    if(!playerSlotOccupied[2]) return 2;
                    if(!playerSlotOccupied[0]) return 0;
                    break;
		        case 2:
                    if(!playerSlotOccupied[0]) return 0;
                    if(!playerSlotOccupied[1]) return 1;
                    if(!playerSlotOccupied[3]) return 3;
                    break;
		        case 3:
                    if(!playerSlotOccupied[1]) return 1;
                    if(!playerSlotOccupied[0]) return 0;
                    if(!playerSlotOccupied[2]) return 2;
                    break;
                default:
                    if(!playerSlotOccupied[0]) return 0;
                    if(!playerSlotOccupied[1]) return 1;
                    if(!playerSlotOccupied[2]) return 2;
                    if(!playerSlotOccupied[3]) return 3;
                    break;
	        }
            return -1;
        }

        private bool isActive(InputManager.ControlActions action, int index)
        {
            return playerSlotOccupied[index] ? InputManager.Instance.SpecificActionButtonPressed(action, Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[index]).ControlType, true) && !playerReadyBySlot[slotIndexToPlayerIndexMapper[index]] : false;
        }

        private void CheckStartCountdown()
        {
            int playersNeeded = 2;
            if (Settings.Instance.GameMode == Game.GameMode.CAPTURE_THE_CELL)
                playersNeeded = 4;
            else if(Settings.Instance.GameMode == Game.GameMode.ARCADE)
                playersNeeded = 1;

            // make sure everyone is ready
            bool allReady = playerSlotOccupied.Count(x => x) >= playersNeeded;
            for (int i = 0; i < 4; i++)
            {
                if (playerSlotOccupied[i] != playerReadyBySlot[i])
                    allReady = false;
            }

            // make sure we have at least one human player
            bool hasHuman = false;
            for (int i = 0; i < Settings.Instance.NumPlayers; i++)
            {
                if (Settings.Instance.GetPlayer(i).ControlType != InputManager.ControlType.NONE)
                    hasHuman = true;
            }

            // one of every team
            bool teamCondition = true;
            if (Settings.Instance.GameMode == Game.GameMode.LEFT_VS_RIGHT)
            {
                bool teamRight = false;
                bool teamLeft = false;
                for (int i = 0; i < Settings.Instance.NumPlayers; ++i)
                {
                    teamRight |= Settings.Instance.GetPlayer(i).Team == Player.Teams.RIGHT;
                    teamLeft |= Settings.Instance.GetPlayer(i).Team == Player.Teams.LEFT;
                }
                teamCondition = teamRight && teamLeft;
            }

            // if everything is fine -> start!
            if (hasHuman && allReady && Settings.Instance.NumPlayers > 0 && teamCondition)
                countdown = TimeSpan.FromSeconds(maxCountdown - 0.001);
            else
                countdown = TimeSpan.FromSeconds(-1);
        }

        private void ToggleReady(int slotIndex)
        {
            // toggle ready
            if (countdown.TotalSeconds > safeCountdown || countdown.TotalSeconds <= 0)
            {
               // if (playerReadyBySlot[slotIndex]) 
               //     AudioManager.Instance.PlaySoundeffect("click");
                playerReadyBySlot[slotIndex] = !playerReadyBySlot[slotIndex];
            }
            // countdown
            if (playerReadyBySlot[slotIndex] && countdown.TotalSeconds <= 0)
                CheckStartCountdown();
            else if (countdown.TotalSeconds > safeCountdown)
                countdown = TimeSpan.FromSeconds(-1);
        }

        private Vector2 GetOrigin(int i)
        {
            if (numSlots == 1)  // arcade
            {
               if(i == 0)
                    return new Vector2((Settings.Instance.ResolutionX - BOX_WIDTH) / 2, (Settings.Instance.ResolutionY - BOX_HEIGHT) / 2);
               else
                    return Vector2.Zero;
            }
            else
            {
                switch (i)
                {
                    case 3:
                        return new Vector2(Settings.Instance.ResolutionX / 4 - BOX_WIDTH / 2, Settings.Instance.ResolutionY / 4 - BOX_HEIGHT / 2 - InterfaceButton.PADDING * 3);
                    case 2:
                        return new Vector2(Settings.Instance.ResolutionX / 4 * 3 - BOX_WIDTH / 2, Settings.Instance.ResolutionY / 4 * 3 - BOX_HEIGHT / 2 - InterfaceButton.PADDING * 3);
                    case 1:
                        return new Vector2(Settings.Instance.ResolutionX / 4 * 3 - BOX_WIDTH / 2, Settings.Instance.ResolutionY / 4 - BOX_HEIGHT / 2 - InterfaceButton.PADDING * 3);
                    case 0:
                        return new Vector2(Settings.Instance.ResolutionX / 4 - BOX_WIDTH / 2, Settings.Instance.ResolutionY / 4 * 3 - BOX_HEIGHT / 2 - InterfaceButton.PADDING * 3);
                    default:
                        return Vector2.Zero;
                }
            }
        }

        private string GetJoinText(int index)
        {
            //if (Settings.Instance.GameMode == Game.GameMode.TUTORIAL)
            //    return "< add a computer player >";
            //else
            if (Settings.Instance.GameMode == Game.GameMode.CAPTURE_THE_CELL)
                return VirusXStrings.NewGameJoinHelpText + "\n\n" + VirusXStrings.NewGameCTCHelpText;
            else
                return VirusXStrings.NewGameJoinHelpText;
        }

        // drawing position of the PreSlotImages
        private Vector2 GetPreSlotPosition(InputManager.ControlType control, int slotIndex)
        {
            switch (control)
            {
                case InputManager.ControlType.KEYBOARD0:
                    return GetOrigin(slotIndex) + new Vector2(60, 20);
                case InputManager.ControlType.KEYBOARD1:
                    return GetOrigin(slotIndex) + new Vector2(180, 20);
                case InputManager.ControlType.KEYBOARD2:
                    return GetOrigin(slotIndex) + new Vector2(300, 20);
                case InputManager.ControlType.GAMEPAD0:
                    return GetOrigin(slotIndex) + new Vector2(0, 210);
                case InputManager.ControlType.GAMEPAD1:
                    return GetOrigin(slotIndex) + new Vector2(120, 210);
                case InputManager.ControlType.GAMEPAD2:
                    return GetOrigin(slotIndex) + new Vector2(240, 210);
                case InputManager.ControlType.GAMEPAD3:
                    return GetOrigin(slotIndex) + new Vector2(360, 210);
                default:
                    return GetOrigin(slotIndex);
            }
        }

        // rearranges PreSlotImages if necessary 
        private void CheckPreSlots()
        {
            for (int i = 0; i < preSlotMapper.Length; i++)
            {
                // if currently no slot attached -> search new one
                if (preSlotMapper[i] < 0)
                    preSlotMapper[i] = GetFreeSlotIndex(-1);
                // if slot is occupied and control is not the player in this slot -> search new one
                if(preSlotMapper[i] >= 0 && playerSlotOccupied[preSlotMapper[i]] && (int)Settings.Instance.GetPlayer(slotIndexToPlayerIndexMapper[preSlotMapper[i]]).ControlType != i)
                    preSlotMapper[i] = GetFreeSlotIndex(preSlotMapper[i]);
            }
        }

        // abuses the controlactions for convenience
        private int SwitchPreSlots(int oldSlot, InputManager.ControlActions actions)
        {
            switch (oldSlot)
	        {
                case 0:
                    switch (actions)
	                {
		                case InputManager.ControlActions.UP:
                            if (!playerSlotOccupied[3]) return 3;
                            if (!playerSlotOccupied[1]) return 1;
                            break;
                        case InputManager.ControlActions.RIGHT:
                            if (!playerSlotOccupied[2]) return 2;
                            if (!playerSlotOccupied[1]) return 1;
                            break;
	                }
                    break;
                case 1:
                    switch (actions)
                    {
                        case InputManager.ControlActions.DOWN:
                            if (!playerSlotOccupied[2]) return 2;
                            if (!playerSlotOccupied[0]) return 0;
                            break;
                        case InputManager.ControlActions.LEFT:
                            if (!playerSlotOccupied[3]) return 3;
                            if (!playerSlotOccupied[0]) return 0;
                            break;
                    }
                    break;
                case 2:
                    switch (actions)
                    {
                        case InputManager.ControlActions.UP:
                            if (!playerSlotOccupied[1]) return 1;
                            if (!playerSlotOccupied[3]) return 3;
                            break;
                        case InputManager.ControlActions.LEFT:
                            if (!playerSlotOccupied[0]) return 0;
                            if (!playerSlotOccupied[3]) return 3;
                            break;
                    }
                    break;
                case 3:
                    switch (actions)
                    {
                        case InputManager.ControlActions.DOWN:
                            if (!playerSlotOccupied[0]) return 0;
                            if (!playerSlotOccupied[2]) return 2;
                            break;
                        case InputManager.ControlActions.RIGHT:
                            if (!playerSlotOccupied[1]) return 1;
                            if (!playerSlotOccupied[2]) return 2;
                            break;
                    }
                    break;
	        }
            return oldSlot;
        }

        #endregion
    }
}
