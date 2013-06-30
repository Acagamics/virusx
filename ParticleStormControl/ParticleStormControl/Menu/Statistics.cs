#define SAVE_STATISTICS
//#define LOAD_STATISTICS
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


#if SAVE_STATISTICS || LOAD_STATISTICS
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
#endif

namespace VirusX.Menu
{
    class StatisticsScreen : MenuPage
    {
        public Statistics Statistics { get; set; }
        private List<string> captions = new List<string> { "Captured", "Lost", "Max #", "Average #", "Average HP", "Items", "Result" };
        private List<string>[] values = new List<string>[0];

        private Texture2D[] itemTextures = new Texture2D[(int)Statistics.StatItems.NUM_STAT_ITEMS]; 
        private const int ITEM_DISPLAY_SIZE = 30;

        private Texture2D playerDiedTexture;

        private Texture2D icons;

        /// <summary>
        /// winning player index
        /// meaning differns with the game time
        /// </summary>
        public int WinPlayerIndex { get; set; }
        public Player.Teams WinningTeam { get; set; }

        public Player.Type[] PlayerTypes { get; set; }
        public int[] PlayerColorIndices { get; set; }

        const float DURATION_CONTINUE_UNAVAILABLE = 1.5f;
        const int SIDE_PADDING = 10; // padding from left
        const int COLUMN_WIDTH = 115; // width of the columns WITH PADDING!
        const int COLUMN_PADDING = 20;
        const int ROW_HEIGHT = 50;

        /// <summary>
        /// if smaller than 0, continue button is available
        /// </summary>
        float timeUntilContinueIsAvailable;

        enum DiagramType
        {
            DOMINATION,
            HEALTH,
            MASS,
            SPAWN_POINTS,

            NUM_VALUES
        };
        private static readonly string[] DIAGRAM_DESCRIPTIONS = new string[]{ "Domination", "Total HP", "Number of Viruses", "Captured Cells" };
        private InterfaceButton diagramDescription;
        private DiagramType currentDiagramType;

        private const int ARROW_SIZE = 50;
        private InterfaceImageButton leftButton;
        private InterfaceImageButton rightButton;

        private const int NUM_TIME_DISPLAYS = 4;
        private InterfaceButton[] timeDisplays = new InterfaceButton[NUM_TIME_DISPLAYS];

        private InterfaceButton winnerLabel;
        private InterfaceButton[] playerStatLabels = new InterfaceButton[4];

        enum Button
        {
            AGAIN,
            MAINMENU,

            NUM_BUTTONS
        };
        Button selectedButton = Button.MAINMENU;

        public StatisticsScreen(Menu menu)
            : base(menu)
        {
            // zentrieren
            int leftStart = -COLUMN_WIDTH * (captions.Count+1) / 2;

            // header um eins verschoben

            // draw winning string
            Func<string> winnerLabelString = () =>
            {
                switch (WinningTeam)
                {
                    case Player.Teams.NONE:
                        return Player.ColorNames[PlayerColorIndices[WinPlayerIndex]] + " wins!";
                    case Player.Teams.LEFT:
                    case Player.Teams.RIGHT:
                    case Player.Teams.ATTACKER:
                        return Player.TEAM_NAMES[(int)WinningTeam] + " win!";
                    case Player.Teams.DEFENDER:
                        return Player.TEAM_NAMES[(int)WinningTeam] + " wins!";
                    default:
                        throw new NotImplementedException("Unknown team type - can't generate winning string!");
                }
            };
            winnerLabel = new InterfaceButton(winnerLabelString, Vector2.Zero,   // positioning later - string size changes!
                                                () => { return false; },
                                                Color.White, Color.FromNonPremultiplied(255, 255, 255, 0), Alignment.TOP_CENTER);
            Interface.Add(winnerLabel); 

            // draw all captions
            for (int i = 0; i < captions.Count; i++)
            {
                Interface.Add(new InterfaceButton(captions[i], new Vector2(leftStart + COLUMN_WIDTH * (i + 1), 100), () => { return false; },
                                        COLUMN_WIDTH - COLUMN_PADDING, Alignment.TOP_CENTER));
            }

            // fill table with values
            for (int i = 0; i < playerStatLabels.Length; i++)
            {
                int index = i;
                playerStatLabels[i] = new InterfaceButton(() => { return Player.ColorNames[PlayerColorIndices[index]]; }, new Vector2(leftStart, 160 + ROW_HEIGHT * index),
                                () => { return false; }, () => { return index < Statistics.PlayerCount; }, COLUMN_WIDTH - COLUMN_PADDING, 
                                Color.White, Color.Black, false, Alignment.TOP_CENTER);
                Interface.Add(playerStatLabels[i]);
                for (int j = 0; j < captions.Count; j++)
                {
                    int x = i;
                    int y = j;
                    Interface.Add(new InterfaceButton(() => values[x][y], new Vector2(leftStart + COLUMN_WIDTH * (y + 1), 160 + ROW_HEIGHT * x),
                                    () => { return false; }, () => { return x < Statistics.PlayerCount; }, COLUMN_WIDTH - COLUMN_PADDING, Alignment.TOP_CENTER));
                }
            }

            // stats descriptor
            diagramDescription = new InterfaceButton(() => DIAGRAM_DESCRIPTIONS[(int)currentDiagramType], Vector2.Zero);
            Interface.Add(diagramDescription);

            // play again button
            string text = "► Play Again";
            int width = (int)menu.Font.MeasureString(text).X;
            Interface.Add(new InterfaceButton(text,
                new Vector2(-(int)(menu.Font.MeasureString(text).X / 2) - InterfaceImageButton.PADDING, menu.GetFontHeight() * 2 + InterfaceImageButton.PADDING * 7),
                () => { return selectedButton == Button.AGAIN; },
                () => { return timeUntilContinueIsAvailable < 0.0f; },
                Alignment.BOTTOM_CENTER));

            // main menu button
            text = VirusXStrings.BackToMainMenu;
            width = (int)menu.Font.MeasureString(text).X;
            Interface.Add(new InterfaceButton(text,
                new Vector2(-(int)(menu.Font.MeasureString(text).X / 2) - InterfaceImageButton.PADDING, menu.GetFontHeight() + InterfaceImageButton.PADDING * 4),
                () => { return selectedButton == Button.MAINMENU; },
                () => { return timeUntilContinueIsAvailable < 0.0f; },
                Alignment.BOTTOM_CENTER));

            // arrows
            leftButton = new InterfaceImageButton(
                "icons",
                Vector2.Zero, // later
                ARROW_SIZE - InterfaceImageButton.PADDING*2, ARROW_SIZE - InterfaceImageButton.PADDING*2,
                new Rectangle(0, 0, 16, 16),
                new Rectangle(32, 0, 16, 16),
                () => { return InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.LEFT, Settings.Instance.StartingControls, true); },
                () => { return timeUntilContinueIsAvailable < 0.0f; },
                Color.FromNonPremultiplied(0, 0, 0, 0)
            );
            Interface.Add(leftButton);
            rightButton = new InterfaceImageButton(
                "icons",
                Vector2.Zero, // later
                ARROW_SIZE - InterfaceImageButton.PADDING * 2, ARROW_SIZE- InterfaceImageButton.PADDING*2,
                new Rectangle(16, 0, 16, 16),
                new Rectangle(48, 0, 16, 16),
                () => { return InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.RIGHT, Settings.Instance.StartingControls, true); },
                () => { return timeUntilContinueIsAvailable < 0.0f; },
                Color.FromNonPremultiplied(0, 0, 0, 0)
            );
            Interface.Add(rightButton);

            // time displays
            for (int i = 0; i < timeDisplays.Length; ++i)
            {
                timeDisplays[i] = new InterfaceButton("", Vector2.Zero);
                Interface.Add(timeDisplays[i]);
            }

        }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
#if SAVE_STATISTICS
            // save statistics
            String fileName = DateTime.Now.Date.Year.ToString()
                + DateTime.Now.Date.Month.ToString()
                + DateTime.Now.Date.Day.ToString()
                + "_" + DateTime.Now.TimeOfDay.Hours.ToString()
                + DateTime.Now.TimeOfDay.Minutes.ToString()
                + DateTime.Now.TimeOfDay.Seconds.ToString()
                + "_" + Settings.Instance.GameMode.ToString()
                + "_VirusXStat.bin";
            Stream streamWrite = File.Create(fileName);
            BinaryFormatter binaryWrite = new BinaryFormatter();
            binaryWrite.Serialize(streamWrite, Statistics);
            streamWrite.Close();
#endif
#if LOAD_STATISTICS
            Stream streamRead = File.OpenRead("21.05.2013_VirusXStat.bin");
            BinaryFormatter binaryRead = new BinaryFormatter();
            statistics = (Statistics)binaryRead.Deserialize(streamRead);
            streamRead.Close();
#endif
            currentDiagramType = DiagramType.DOMINATION;

            values = new List<string>[Statistics.PlayerCount];
            int counter = 0;
            for (int i = 0; i < PlayerTypes.Length; i++)
            {
                if (PlayerTypes[i] != Player.Type.NONE)
                {
                    values[counter] = new List<string>();
                    values[counter].Add(Statistics.getCapturedSpawnPoints(i).ToString());
                    values[counter].Add(Statistics.getLostSpawnPoints(i).ToString());
                    values[counter].Add(Statistics.getMaxSimultaneousParticles(i).ToString());
                    values[counter].Add(Statistics.getAverageParticles(i).ToString());
                    values[counter].Add(Statistics.getAverageHealth(i).ToString());
                    values[counter].Add(Statistics.getCollectedItems(i).ToString());
                    values[counter].Add(IsPlayerWinner(i) ? "Winner" : "Looser");
                    playerStatLabels[i].BackgroundColor = Settings.Instance.GetPlayerColor(i);
                    counter++;
                }
            }

            if(WinPlayerIndex >= 0)
                winnerLabel.BackgroundColor = Player.Colors[PlayerColorIndices[WinPlayerIndex]];
            else
                winnerLabel.BackgroundColor = Color.Black;
            winnerLabel.Position = new Vector2(-(int)menu.FontHeading.MeasureString(winnerLabel.Text()).X / 2, 30);

            timeUntilContinueIsAvailable = DURATION_CONTINUE_UNAVAILABLE;
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            itemTextures[(int)Statistics.StatItems.DANGER_ZONE] = content.Load<Texture2D>("items/danger");
            itemTextures[(int)Statistics.StatItems.MUTATION] = content.Load<Texture2D>("items/mutate");
            itemTextures[(int)Statistics.StatItems.WIPEOUT] = content.Load<Texture2D>("items/wipeout");
            itemTextures[(int)Statistics.StatItems.ANTI_BODY] = content.Load<Texture2D>("items/debuff");

            playerDiedTexture = content.Load<Texture2D>("death");

            icons = content.Load<Texture2D>("icons");

            base.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            if (timeUntilContinueIsAvailable < 0.0f)
            {
                selectedButton = (Button)(Menu.Loop((int)selectedButton, (int)Button.NUM_BUTTONS, Settings.Instance.StartingControls));

                if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.ACTION))
                {
                    switch (selectedButton)
                    {
                        case Button.AGAIN:
                            menu.ChangePage(Menu.Page.NEWGAME, gameTime);
                            break;
                        case Button.MAINMENU:
                            menu.ChangePage(Menu.Page.MAINMENU, gameTime);
                            break;
                    }
                }

                if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.RIGHT, Settings.Instance.StartingControls))
                    currentDiagramType = (DiagramType)(((int)currentDiagramType + 1) % (int)DiagramType.NUM_VALUES);

                if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.LEFT, Settings.Instance.StartingControls))
                    currentDiagramType = (DiagramType)((((int)currentDiagramType - 1) < 0 ? (int)DiagramType.NUM_VALUES : (int)(currentDiagramType)) - 1);
            }
            timeUntilContinueIsAvailable -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // draw diagrams
            int yPos = 180 + ROW_HEIGHT * values.Length;
            int height = Settings.Instance.ResolutionY - 360 - ROW_HEIGHT * values.Length;
            int maxWidth = Settings.Instance.ResolutionX - SIDE_PADDING * 2;

            switch (currentDiagramType)
            {
                case DiagramType.DOMINATION:
                    DrawDiagram(spriteBatch, (progress, player) => DataInterpolation(progress,  x=>Statistics.getDominationInStep(player, x), Statistics.Steps-1),
                                                    (float)gameTime.ElapsedGameTime.TotalSeconds, maxWidth, height, yPos);
                    break;
                case DiagramType.HEALTH:
                    DrawDiagram(spriteBatch, (progress, player) => DataInterpolation(progress,  x=>Statistics.getHealthInStep(player, x), Statistics.Steps-1) / Statistics.MaxOverallSimultaneousHealth,
                                                                     (float)gameTime.ElapsedGameTime.TotalSeconds, maxWidth, height, yPos);
                    break;
                case DiagramType.MASS:
                    DrawDiagram(spriteBatch, (progress, player) => DataInterpolation(progress, x => Statistics.getParticlesInStep(player, x), Statistics.Steps - 1) / Statistics.MaxOverallSimultaneousParticles,
                                                                     (float)gameTime.ElapsedGameTime.TotalSeconds, maxWidth, height, yPos);
                    break;
                case DiagramType.SPAWN_POINTS:
                    DrawDiagram(spriteBatch, (progress, player) => ((float)Statistics.getPossessingSpawnPointsInStep(player, (int)(progress * Statistics.Steps)) / Statistics.OverallNumberOfSpawnPoints),
                                                                     (float)gameTime.ElapsedGameTime.TotalSeconds, maxWidth, height, yPos);
                    break;
            }

            base.Draw(spriteBatch, gameTime);
        }


        private float DataInterpolation(float progress, Func<int, float> stepToDataFunc, int maxStep)
        {
            float stepFloat = progress * Statistics.Steps;
            int stepFloor = (int)(stepFloat);
            int stepCeil = Math.Min((int)Math.Ceiling(stepFloat), maxStep);
            float stepPercentage = stepFloat - stepFloor;

            return MathHelper.Lerp(stepToDataFunc(stepFloor), stepToDataFunc(stepCeil), stepPercentage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="heightFunction">function returning a height for a given (1st para) progress-percentage from a given (2nd para) player. Sum of heights in a step should be one!</param>
        /// <param name="frameTimeInterval"></param>
        /// <param name="area"></param>
        private void DrawDiagram(SpriteBatch spriteBatch, Func<float, int, float> heightFunction, float frameTimeInterval, int maxWidth, int heightVal, int yPos)
        {
            Rectangle area;

            const int ARROW_PADDING = 20;

            int diagramAreaMaxWidth = maxWidth - ARROW_SIZE * 2 - ARROW_PADDING * 2;
            area.Width = diagramAreaMaxWidth + 20;
            area.Height = heightVal;
            area.X = (menu.ScreenWidth-area.Width) / 2;
            area.Y = yPos + InterfaceElement.PADDING*2 + menu.GetFontHeight();

            diagramDescription.Position = new Vector2(area.X, yPos);

            // draw border
            spriteBatch.Draw(menu.TexPixel, area, Color.Black);
            area.Inflate(-10, -10);

            // if this stuff could slow down the whole game.... suprise suprise, let's optimize! ;P

            // gray background
            spriteBatch.Draw(menu.TexPixel, area, Color.Gray);

            // draw amount of particles per player and pixel
            for (int pixel = 0; pixel < area.Width; pixel++)     
            {
                int offset = 0;
                float progress = (float)pixel / area.Width;
                for (int playerIndex = Statistics.PlayerCount-1; playerIndex >= 0; --playerIndex)
                {
                    float percentage = heightFunction(progress, playerIndex);
                    int height = (int)(percentage * area.Height);

                    offset += height;

                    Rectangle rect = new Rectangle(area.X + pixel, area.Bottom - offset, 1, height);
                    spriteBatch.Draw(menu.TexPixel, rect, Player.Colors[PlayerColorIndices[playerIndex]]);
                }
            }

            // hide rounding errors ...
            Rectangle rouningHide = area;
            rouningHide.Y = rouningHide.Top-10;
            rouningHide.Height = 13;
            spriteBatch.Draw(menu.TexPixel, rouningHide, Color.Black);

            // draw items
            float stepWidth = (float)area.Width / Statistics.Steps;
            for (int step = 0; step < Statistics.Steps; step++)    
            {
                int offset = 0;
                float progress = (float)step / (Statistics.Steps-1);
                for (int playerIndex = Statistics.PlayerCount - 1; playerIndex >= 0; --playerIndex)
                {
                    float percentage = heightFunction(progress, playerIndex);
                    int height = (int)(percentage * area.Height);

                    offset += height;

                    // render
                    Statistics.StatItems? itemUsed = Statistics.getFirstUsedItemInStep(playerIndex, step + Statistics.FirstStep);
                    if (itemUsed != null)
                    {
                        int y = (int)MathHelper.Clamp(area.Bottom - offset + (height - ITEM_DISPLAY_SIZE) / 2, area.Y, area.Y + area.Height - ITEM_DISPLAY_SIZE);
                        int x = (int)(area.X + step * stepWidth - ITEM_DISPLAY_SIZE / 2 + 0.5f);

                        if ((Statistics.StatItems)itemUsed == Statistics.StatItems.MUTATION)
                        {
                            if (Level.SWITCH_COUNTDOWN_LENGTH / Statistics.StepTime + step > Statistics.Steps)  // never happen?
                                continue;
                            x += (int)(Level.SWITCH_COUNTDOWN_LENGTH / Statistics.StepTime * stepWidth);
                        }

                        x = (int)MathHelper.Clamp(x, area.X, area.X + area.Width - ITEM_DISPLAY_SIZE);


                        spriteBatch.Draw(itemTextures[(int)itemUsed],
                            new Rectangle(x, y, ITEM_DISPLAY_SIZE, ITEM_DISPLAY_SIZE),
                            Color.White);
                    }
                }
            }
            
            // draw deaths
            for (int playerIndex = 0; playerIndex<Statistics.PlayerCount; ++playerIndex)
            {
                int depthStep = Statistics.getDeathStepOfPlayer(playerIndex) - Statistics.FirstStep;
                if (depthStep >= 0)
                {
                    float percentage = (float)depthStep / Statistics.Steps;
                    int xPos = (int)MathHelper.Clamp(area.X + percentage * area.Width - ITEM_DISPLAY_SIZE / 2, area.X, area.X + area.Width - ITEM_DISPLAY_SIZE);
                    spriteBatch.Draw(playerDiedTexture, new Rectangle(xPos, area.Bottom - ITEM_DISPLAY_SIZE + 5, ITEM_DISPLAY_SIZE, ITEM_DISPLAY_SIZE),
                                            Player.Colors[PlayerColorIndices[playerIndex]]);
                }
            }

            area.Inflate(10, 10);

            // position time displays
            for (int i = 0; i < timeDisplays.Length; ++i)
            {
                float progress = (float)i / (timeDisplays.Length-1);
                float time = Statistics.LastStep * progress * Statistics.StepTime;
                string endTimeString = Utils.GenerateTimeString(time);
                float textLen = menu.Font.MeasureString(endTimeString).X;
                
                float textOffset;
                if(i == 0)
                    textOffset = 0;
                else if (i == timeDisplays.Length - 1)
                    textOffset = -textLen - InterfaceButton.PADDING*2;
                else
                    textOffset = -textLen / 2;

                timeDisplays[i].Position = new Vector2(area.X + area.Width * progress + textOffset, area.Y + area.Height);
                timeDisplays[i].Text = () => endTimeString;
            }

            int arrowY = area.Y + (area.Height - ARROW_SIZE) / 2;//boxHeight / 2 - ARROW_SIZE;
            leftButton.Position = new Vector2(area.Left - ARROW_SIZE - InterfaceImageButton.PADDING, arrowY);
            rightButton.Position = new Vector2(area.Right + InterfaceImageButton.PADDING, arrowY);
        }

        private bool IsPlayerWinner(int index)
        {
            switch (WinningTeam)
            {
                case Player.Teams.NONE:
                    return index == WinPlayerIndex;
                case Player.Teams.LEFT:
                    return index == 0 || index == 3;
                case Player.Teams.RIGHT:
                    return index == 1 || index == 2;
                case Player.Teams.ATTACKER:
                    return index >= 1;
                case Player.Teams.DEFENDER:
                    return index == 0;
                default:
                    throw new NotImplementedException("Unknown team type - can't evaluate winner!");
            }
        }
    }
}
