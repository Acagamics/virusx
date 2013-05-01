﻿using System;
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

        public StatisticsScreen(Menu menu)
            : base(menu)
        {
            // zentrieren
            int pad = (menu.ScreenWidth - (captions.Count + 1) * COLUMN_WIDTH + SIDE_PADDING * 2 - COLUMN_PADDING) / 2;

            // header um eins verschoben

            // draw winning string
            Interface.Add(new InterfaceButton(() => { return Player.ColorNames[PlayerColorIndices[WinPlayerIndex]] + " wins!"; }, new Vector2(-(int)menu.FontHeading.MeasureString("Player ... wins!").X / 2, 60), () => { return false; }, Color.White, Color.FromNonPremultiplied(255, 255, 255, 0), Alignment.TOP_CENTER)); 

            // draw all captions
            for (int i = 0; i < captions.Count; i++)
            {
                Interface.Add(new InterfaceButton(captions[i], new Vector2(pad + COLUMN_WIDTH * (i + 1), 150), () => { return false; }, COLUMN_WIDTH - COLUMN_PADDING));
            }

            // fill table with values
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                Interface.Add(new InterfaceButton(() => { return Player.ColorNames[PlayerColorIndices[index]]; }, new Vector2(pad, 210 + 60 * index), () => { return false; }, () => { return index < statistics.PlayerCount; }, COLUMN_WIDTH - COLUMN_PADDING));
                for (int j = 0; j < captions.Count; j++)
                {
                    int x = i;
                    int y = j;
                    Interface.Add(new InterfaceButton(() => values[x][y], new Vector2(pad + COLUMN_WIDTH * (y + 1), 210 + 60 * x), () => { return false; }, () => { return x < statistics.PlayerCount; }, COLUMN_WIDTH - COLUMN_PADDING));
                }
            }

            // stats descriptor
            diagramDescription = new InterfaceButton(() => DIAGRAM_DESCRIPTIONS[(int)currentDiagramType], Vector2.Zero);
            Interface.Add(diagramDescription);

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

            // continue button
            string text = "continue";
            int width = (int)menu.Font.MeasureString(text).X;
            Interface.Add(new InterfaceButton(text, new Vector2((int)((Settings.Instance.ResolutionX - width) * 0.5f), Settings.Instance.ResolutionY - 80), () => { return timeUntilContinueIsAvailable < 0.0f; }));
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
                    counter++;
                }
            }

            ((InterfaceButton)Interface[0]).BackgroundColor = Player.Colors[PlayerColorIndices[WinPlayerIndex]];

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
                    DrawDiagram(spriteBatch, (step, player) => statistics.getDominationInStep(player, step),
                                                    (float)gameTime.ElapsedGameTime.TotalSeconds, maxWidth, height, yPos);
                    break;
                case DiagramType.HEALTH:
                    DrawDiagram(spriteBatch, (step, player) => //statistics.getHealthInStep(step) == 0 ? 1.0f / statistics.PlayerCount :
                                                        (float)statistics.getHealthInStep(player, step) / statistics.MaxOverallSimultaneousHealth, //statistics.getHealthInStep(step),
                                                                     (float)gameTime.ElapsedGameTime.TotalSeconds, maxWidth, height, yPos);
                    break;
                case DiagramType.MASS:
                    DrawDiagram(spriteBatch, (step, player) => //statistics.getParticlesInStep(step) == 0 ? 1.0f / statistics.PlayerCount :
                                                        (float)statistics.getParticlesInStep(player, step) / statistics.MaxOverallSimultaneousParticles, //statistics.getParticlesInStep(step),
                                                                     (float)gameTime.ElapsedGameTime.TotalSeconds, maxWidth, height, yPos);
                    break;
                case DiagramType.SPAWN_POINTS:
                    DrawDiagram(spriteBatch, (step, player) => (float)statistics.getPossessingSpawnPointsInStep(player, step) / statistics.OverallNumberOfSpawnPoints,
                                                                     (float)gameTime.ElapsedGameTime.TotalSeconds, maxWidth, height, yPos);
                    break;
            }

            base.Draw(spriteBatch, gameTime);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="heightFunction">function returning a height for a given (1st para) step from a given (2nd para) player. Sum of heights in a step should be one!</param>
        /// <param name="frameTimeInterval"></param>
        /// <param name="area"></param>
        private void DrawDiagram(SpriteBatch spriteBatch, Func<int, int, float> heightFunction, float frameTimeInterval, int maxWidth, int heightVal, int yPos)
        {
            Rectangle area;

            const int ARROW_PADDING = 20;

            int diagramAreaMaxWidth = maxWidth - ARROW_SIZE * 2 - ARROW_PADDING * 2;
            int stepWidth = Math.Max(1, diagramAreaMaxWidth / statistics.Steps);
            int startStep;
            if (statistics.Steps > diagramAreaMaxWidth)
                startStep = (statistics.Steps - diagramAreaMaxWidth) / stepWidth + 1;   // if a step is only a pixel... DAMDAM
            else
                startStep = 0;
            area.Width = stepWidth * (statistics.Steps - startStep) + 20;

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

            // draw amount of particles per player and step
            for (int step = startStep; step < statistics.Steps; step++)     
            {
                int offset = 0;
                for (int playerIndex = statistics.PlayerCount-1; playerIndex >= 0; --playerIndex)
                {
                    float percentage = heightFunction(step, playerIndex);
                    int height = (int)(percentage * area.Height);

                    offset += height;

                    Rectangle rect = new Rectangle(area.X + (step - startStep) * stepWidth, area.Bottom - offset, stepWidth, height);
                    spriteBatch.Draw(menu.TexPixel, rect, Player.Colors[PlayerColorIndices[playerIndex]]);
                }
            }

            // hide rounding errors ...
            Rectangle rouningHide = area;
            rouningHide.Y = rouningHide.Top-10;
            rouningHide.Height = 13;
            spriteBatch.Draw(menu.TexPixel, rouningHide, Color.Black);

            // draw items
            for (int step = startStep; step < statistics.Steps; step++)    
            {
                int offset = 0;
                for (int playerIndex = statistics.PlayerCount - 1; playerIndex >= 0; --playerIndex)
                {
                    float percentage = heightFunction(step, playerIndex);
                    int height = (int)(percentage * area.Height);


                    offset += height;

                    // render
                    Statistics.StatItems? itemUsed = statistics.getFirstUsedItemInStep(playerIndex, step);
                    if (itemUsed != null)
                    {
                        int y = (int)MathHelper.Clamp(area.Bottom - offset + (height - ITEM_DISPLAY_SIZE) / 2, area.Y, area.Y + area.Height - ITEM_DISPLAY_SIZE);
                        int x = area.X + (step - startStep) * stepWidth - ITEM_DISPLAY_SIZE / 2;

                        if ((Statistics.StatItems)itemUsed == Statistics.StatItems.MUTATION)
                        {
                            if (Level.SWITCH_COUNTDOWN_LENGTH / statistics.StepTime + step > statistics.Steps - startStep)  // never happen?
                                continue;
                            x += stepWidth * (int)(Level.SWITCH_COUNTDOWN_LENGTH / statistics.StepTime);
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
                int depthStep = statistics.getDeathStepOfPlayer(playerIndex);
                if (depthStep >= 0)
                {
                    float percentage = (float)depthStep / (statistics.Steps - startStep);
                    int xPos = (int)MathHelper.Clamp(area.X + percentage * area.Width - ITEM_DISPLAY_SIZE / 2, area.X, area.X + area.Width - ITEM_DISPLAY_SIZE);
                    spriteBatch.Draw(playerDiedTexture, new Rectangle(xPos, area.Bottom - ITEM_DISPLAY_SIZE + 5, ITEM_DISPLAY_SIZE, ITEM_DISPLAY_SIZE),
                                            Player.Colors[PlayerColorIndices[playerIndex]]);
                }
            }

            area.Inflate(10, 10);

            // draw timings
            const int numTimeDisplays = 4;
            for (int i = 0; i < numTimeDisplays; ++i)
            {
                float progress = (float)i / (numTimeDisplays-1);
                float time = ((statistics.Steps - startStep) * progress + statistics.FirstStep + startStep) * statistics.StepTime;
                string endTimeString = GenerateTimeString(time);
                float textLen = menu.Font.MeasureString(endTimeString).X;
                
                float textOffset;
                //if(i == 0)
                    //textOffset = InterfaceButton.PADDING;
                //else if (i == numTimeDisplays-1)
                    //textOffset = -textLen - InterfaceButton.PADDING;
                //else
                    textOffset = -textLen / 2;
                
                float x = area.X + area.Width * progress + textOffset;
                //InterfaceButton.Instance.Draw(spriteBatch, menu.Font, endTimeString, new Vector2(x, area.Y + area.Height + 10), false, menu.TexPixel);
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
