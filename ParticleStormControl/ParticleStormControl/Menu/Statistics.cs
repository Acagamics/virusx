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
    class StatisticsScreen : MenuPage
    {
        private Statistics statistics;
        private List<string> captions = new List<string> { "Captured", "Lost", "Max #", "Average #", "Average HP", "Items" };
        private List<string>[] values = new List<string>[0];

        private Texture2D[] itemTextures = new Texture2D[(int)Statistics.StatItems.NUM_STAT_ITEMS]; 
        private const int ITEM_DISPLAY_SIZE = 30;

        private Texture2D playerDiedTexture;

        private Texture2D icons;

        public int WinPlayerIndex { get; set; }
        public Player.Type[] PlayerTypes { get; set; }
        public int[] PlayerColorIndices { get; set; }

        const float DURATION_CONTINUE_UNAVAILABLE = 1.0f;
        const int SIDE_PADDING = 30; // padding from left
        const int COLUMN_WIDTH = 145; // width of the columns WITH PADDING!
        const int COLUMN_PADDING = 20;

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

        public StatisticsScreen(Menu menu)
            : base(menu)
        {
            // zentrieren
            int leftStart = -COLUMN_WIDTH * (captions.Count+1) / 2;

            // header um eins verschoben

            // draw winning string
            winnerLabel = new InterfaceButton(() => { return Player.ColorNames[PlayerColorIndices[WinPlayerIndex]] + " wins!"; },
                                                Vector2.Zero,   // positioning later - string size changes!
                                                () => { return false; },
                                                Color.White, Color.FromNonPremultiplied(255, 255, 255, 0), Alignment.TOP_CENTER);
            Interface.Add(winnerLabel); 

            // draw all captions
            for (int i = 0; i < captions.Count; i++)
            {
                Interface.Add(new InterfaceButton(captions[i], new Vector2(leftStart + COLUMN_WIDTH * (i + 1), 150), () => { return false; },
                                        COLUMN_WIDTH - COLUMN_PADDING, Alignment.TOP_CENTER));
            }

            // fill table with values
            for (int i = 0; i < playerStatLabels.Length; i++)
            {
                int index = i;
                playerStatLabels[i] = new InterfaceButton(() => { return Player.ColorNames[PlayerColorIndices[index]]; }, new Vector2(leftStart, 210 + 60 * index),
                                () => { return false; }, () => { return index < statistics.PlayerCount; }, COLUMN_WIDTH - COLUMN_PADDING, 
                                Color.White, Color.Black, false, Alignment.TOP_CENTER);
                Interface.Add(playerStatLabels[i]);
                for (int j = 0; j < captions.Count; j++)
                {
                    int x = i;
                    int y = j;
                    Interface.Add(new InterfaceButton(() => values[x][y], new Vector2(leftStart + COLUMN_WIDTH * (y + 1), 210 + 60 * x),
                                    () => { return false; }, () => { return x < statistics.PlayerCount; }, COLUMN_WIDTH - COLUMN_PADDING, Alignment.TOP_CENTER));
                }
            }

            // stats descriptor
            diagramDescription = new InterfaceButton(() => DIAGRAM_DESCRIPTIONS[(int)currentDiagramType], Vector2.Zero);
            Interface.Add(diagramDescription);

            // continue button
            string text = "► Continue";
            int width = (int)menu.Font.MeasureString(text).X;
            Interface.Add(new InterfaceButton(text, new Vector2(-(int)(menu.Font.MeasureString(text).X / 2) - InterfaceImageButton.PADDING, 
                                                                    menu.GetFontHeight() + InterfaceImageButton.PADDING * 2 + 50),
                                        () => { return timeUntilContinueIsAvailable < 0.0f; }, Alignment.BOTTOM_CENTER));

            // arrows
            leftButton = new InterfaceImageButton(
                "icons",
                Vector2.Zero, // later
                ARROW_SIZE - InterfaceImageButton.PADDING*2, ARROW_SIZE - InterfaceImageButton.PADDING*2,
                new Rectangle(0, 0, 16, 16),
                new Rectangle(32, 0, 16, 16),
                () => { return InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.LEFT, true); }, () => true
            );
            Interface.Add(leftButton);
            rightButton = new InterfaceImageButton(
                "icons",
                Vector2.Zero, // later
                ARROW_SIZE - InterfaceImageButton.PADDING * 2, ARROW_SIZE- InterfaceImageButton.PADDING*2,
                new Rectangle(16, 0, 16, 16),
                new Rectangle(48, 0, 16, 16),
                () => { return InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.RIGHT, true); }, () => true
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
            statistics = menu.Game.InGame.Level.GameStatistics;
            currentDiagramType = DiagramType.DOMINATION;

            values = new List<string>[statistics.PlayerCount];
            int counter = 0;
            for (int i = 0; i < PlayerTypes.Length; i++)
            {
                if (PlayerTypes[i] != Player.Type.NONE)
                {
                    values[counter] = new List<string>();
                    values[counter].Add(statistics.getCapturedSpawnPoints(i).ToString());
                    values[counter].Add(statistics.getLostSpawnPoints(i).ToString());
                    values[counter].Add(statistics.getMaxSimultaneousParticles(i).ToString());
                    values[counter].Add(statistics.getAverageParticles(i).ToString());
                    values[counter].Add(statistics.getAverageHealth(i).ToString());
                    values[counter].Add(statistics.getCollectedItems(i).ToString());
                    playerStatLabels[i].BackgroundColor = Settings.Instance.GetPlayerColor(i);
                    counter++;
                }
            }

            winnerLabel.BackgroundColor = Player.Colors[PlayerColorIndices[WinPlayerIndex]];
            winnerLabel.Position = new Vector2(-(int)menu.FontHeading.MeasureString(winnerLabel.Text()).X / 2, 60);

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
            if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.ACTION) && timeUntilContinueIsAvailable < 0.0f)
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);

            if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.RIGHT))
                currentDiagramType = (DiagramType)(((int)currentDiagramType + 1) % (int)DiagramType.NUM_VALUES);

            if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.LEFT))
                currentDiagramType = (DiagramType)((((int)currentDiagramType - 1) < 0 ? (int)DiagramType.NUM_VALUES : (int)(currentDiagramType)) -1);

            timeUntilContinueIsAvailable -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // draw diagrams
            int yPos = 220 + 60 * values.Length;
            int height = Settings.Instance.ResolutionY - 400 - 60 * values.Length;
            int maxWidth = Settings.Instance.ResolutionX - SIDE_PADDING * 2;

            switch (currentDiagramType)
            {
                case DiagramType.DOMINATION:
                    DrawDiagram(spriteBatch, (progress, player) => DataInterpolation(progress,  x=>statistics.getDominationInStep(player, x), statistics.Steps-1),
                                                    (float)gameTime.ElapsedGameTime.TotalSeconds, maxWidth, height, yPos);
                    break;
                case DiagramType.HEALTH:
                    DrawDiagram(spriteBatch, (progress, player) => DataInterpolation(progress,  x=>statistics.getHealthInStep(player, x), statistics.Steps-1) / statistics.MaxOverallSimultaneousHealth,
                                                                     (float)gameTime.ElapsedGameTime.TotalSeconds, maxWidth, height, yPos);
                    break;
                case DiagramType.MASS:
                    DrawDiagram(spriteBatch, (progress, player) => DataInterpolation(progress, x => statistics.getParticlesInStep(player, x), statistics.Steps - 1) / statistics.MaxOverallSimultaneousParticles,
                                                                     (float)gameTime.ElapsedGameTime.TotalSeconds, maxWidth, height, yPos);
                    break;
                case DiagramType.SPAWN_POINTS:
                    DrawDiagram(spriteBatch, (progress, player) => DataInterpolation(progress, x => statistics.getPossessingSpawnPointsInStep(player, x), statistics.Steps - 1) / statistics.OverallNumberOfSpawnPoints,
                                                                     (float)gameTime.ElapsedGameTime.TotalSeconds, maxWidth, height, yPos);
                    break;
            }

            base.Draw(spriteBatch, gameTime);
        }


        private float DataInterpolation(float progress, Func<int, float> stepToDataFunc, int maxStep)
        {
            float stepFloat = progress * statistics.Steps;
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
                for (int playerIndex = statistics.PlayerCount-1; playerIndex >= 0; --playerIndex)
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
            float stepWidth = (float)area.Width / statistics.Steps;
            for (int step = 0; step < statistics.Steps; step++)    
            {
                int offset = 0;
                float progress = (float)step / (statistics.Steps-1);
                for (int playerIndex = statistics.PlayerCount - 1; playerIndex >= 0; --playerIndex)
                {
                    float percentage = heightFunction(progress, playerIndex);
                    int height = (int)(percentage * area.Height);

                    offset += height;

                    // render
                    Statistics.StatItems? itemUsed = statistics.getFirstUsedItemInStep(playerIndex, step + statistics.FirstStep);
                    if (itemUsed != null)
                    {
                        int y = (int)MathHelper.Clamp(area.Bottom - offset + (height - ITEM_DISPLAY_SIZE) / 2, area.Y, area.Y + area.Height - ITEM_DISPLAY_SIZE);
                        int x = (int)(area.X + step * stepWidth - ITEM_DISPLAY_SIZE / 2 + 0.5f);

                        if ((Statistics.StatItems)itemUsed == Statistics.StatItems.MUTATION)
                        {
                            if (Level.SWITCH_COUNTDOWN_LENGTH / statistics.StepTime + step > statistics.Steps)  // never happen?
                                continue;
                            x += (int)(Level.SWITCH_COUNTDOWN_LENGTH / statistics.StepTime * stepWidth);
                        }

                        x = (int)MathHelper.Clamp(x, area.X, area.X + area.Width - ITEM_DISPLAY_SIZE);


                        spriteBatch.Draw(itemTextures[(int)itemUsed],
                            new Rectangle(x, y, ITEM_DISPLAY_SIZE, ITEM_DISPLAY_SIZE),
                            Color.White);
                    }
                }
            }
            
            // draw deaths
            for (int playerIndex = 0; playerIndex<statistics.PlayerCount; ++playerIndex)
            {
                int depthStep = statistics.getDeathStepOfPlayer(playerIndex) - statistics.FirstStep;
                if (depthStep >= 0)
                {
                    float percentage = (float)depthStep / statistics.Steps;
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
                float time = statistics.LastStep * progress * statistics.StepTime;
                string endTimeString = GenerateTimeString(time);
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

        private string GenerateTimeString(float time)
        {
            int minutes = (int)(time / 60.0f);
            int seconds = (int)(time - minutes * 60 + 0.5f);
            return String.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}
