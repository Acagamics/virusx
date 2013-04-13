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
    class Win : MenuPage
    {
        private Statistics statistics;
        private List<String> captions = new List<String> { "Captured", "Lost", "Max #", "Average #", "Average HP", "Items" };
        private List<String>[] values;

        private Texture2D[] itemTextures = new Texture2D[(int)Statistics.StatItems.NUM_STAT_ITEMS]; 
        private const int ITEM_DISPLAY_SIZE = 30;

        private Texture2D playerDiedTexture;

        private Texture2D icons;

        public int WinPlayerIndex { get; set; }
        public bool[] ConnectedPlayers { get; set; }
        public int[] PlayerColorIndices { get; set; }

        enum DiagramType
        {
            DOMINATION,
            HEALTH,
            MASS,
            SPAWN_POINTS,

            NUM_VALUES
        };
        private static readonly string[] DIAGRAM_DESCRIPTIONS = new string[]{ "Domination", "Total HP", "Number of Viruses", "Number of Cells" };
        private DiagramType currentDiagramType;

        public Win(Menu menu)
            : base(menu)
        { }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            statistics = menu.Game.InGame.Level.GameStatistics;
            currentDiagramType = DiagramType.DOMINATION;

            values = new List<string>[statistics.PlayerCount];
            int counter = 0;
            for (int i = 0; i < 4; i++)
            {
                if (ConnectedPlayers[i])
                {
                    values[counter] = new List<string>();
                    values[counter].Add(statistics.getCapturedBases(i).ToString());
                    values[counter].Add(statistics.getLostBases(i).ToString());
                    values[counter].Add(statistics.getMaxSimultaneousParticles(i).ToString());
                    values[counter].Add(statistics.getAverageParticles(i).ToString());
                    values[counter].Add(statistics.getAverageHealth(i).ToString());
                    values[counter].Add(statistics.getCollectedItems(i).ToString());
                    counter++;
                }
            }
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            itemTextures[(int)Statistics.StatItems.DANGER_ZONE] = content.Load<Texture2D>("items/danger");
            itemTextures[(int)Statistics.StatItems.MUTATION] = content.Load<Texture2D>("items/mutate");
            itemTextures[(int)Statistics.StatItems.WIPEOUT] = content.Load<Texture2D>("items/wipeout");
            itemTextures[(int)Statistics.StatItems.ANTI_BODY] = content.Load<Texture2D>("items/debuff");

            playerDiedTexture = content.Load<Texture2D>("death");

            icons = content.Load<Texture2D>("icons");
        }

        public override void Update(GameTime gameTime)
        {
            if (InputManager.Instance.ContinueButton())
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);

            if (InputManager.Instance.AnyRightButtonPressed())
                currentDiagramType = (DiagramType)(((int)currentDiagramType + 1) % (int)DiagramType.NUM_VALUES);

            if (InputManager.Instance.AnyLeftButtonPressed())
                currentDiagramType = (DiagramType)((((int)currentDiagramType - 1) < 0 ? (int)DiagramType.NUM_VALUES : (int)(currentDiagramType)) -1);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            const int SIDE_PADDING = 30; // padding from left
            const int COLUMN_WIDTH = 145; // width of the columns WITH PADDING!
            const int COLUMN_PADDING = 20;
            

            // zentrieren
            int pad = (menu.ScreenWidth - (captions.Count + 1) * COLUMN_WIDTH + SIDE_PADDING * 2 - COLUMN_PADDING) / 2;

            // header um eins verschoben

            // draw winning string
            string text = Player.ColorNames[PlayerColorIndices[WinPlayerIndex]] + " wins!";
            int width = (int)menu.FontHeading.MeasureString(text).X;
            SimpleButton.Instance.Draw(spriteBatch, menu.FontHeading, text, new Vector2((int)((Settings.Instance.ResolutionX - width) * 0.5f), 60), Player.Colors[PlayerColorIndices[WinPlayerIndex]], menu.TexPixel);

            // draw all captions
            for (int i = 0; i < captions.Count; i++)
            {
                SimpleButton.Instance.Draw(spriteBatch, menu.Font, captions[i], new Vector2(pad + COLUMN_WIDTH * (i + 1), 150), false, menu.TexPixel, COLUMN_WIDTH - COLUMN_PADDING);
            }

            // fill table with values
            for (int i = 0; i < values.Length; i++)
            {
                SimpleButton.Instance.Draw(spriteBatch, menu.Font, Player.ColorNames[PlayerColorIndices[i]], new Vector2(pad, 210 + 60 * i), Player.Colors[PlayerColorIndices[i]], menu.TexPixel, COLUMN_WIDTH - COLUMN_PADDING);
                for (int j = 0; j < values[0].Count; j++)
                {
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, values[i][j], new Vector2(pad + COLUMN_WIDTH * (j + 1), 210 + 60 * i), false, menu.TexPixel, COLUMN_WIDTH - COLUMN_PADDING);
                }
            }

            // draw diagrams
            int yPos = 220 + 60 * values.Length;
            int height = Settings.Instance.ResolutionY - 400 - 60 * values.Length;
            int maxWidth = Settings.Instance.ResolutionX - SIDE_PADDING * 2;
            switch (currentDiagramType)
            {
                case DiagramType.DOMINATION:
                    DrawDiagram(DIAGRAM_DESCRIPTIONS[(int)currentDiagramType], spriteBatch, (step, player) => statistics.getDominationInStep(player, step),
                                                    (float)gameTime.ElapsedGameTime.TotalSeconds, maxWidth, height, yPos);
                    break;
                case DiagramType.HEALTH:
                    DrawDiagram(DIAGRAM_DESCRIPTIONS[(int)currentDiagramType], spriteBatch, (step, player) => statistics.getHealthInStep(step) == 0 ? 1.0f / statistics.PlayerCount :
                                                        (float)statistics.getHealthInStep(player, step) / statistics.getHealthInStep(step),
                                                                     (float)gameTime.ElapsedGameTime.TotalSeconds, maxWidth, height, yPos);
                    break;
                case DiagramType.MASS:
                    DrawDiagram(DIAGRAM_DESCRIPTIONS[(int)currentDiagramType], spriteBatch, (step, player) => statistics.getParticlesInStep(step) == 0 ? 1.0f / statistics.PlayerCount :
                                                        (float)statistics.getParticlesInStep(player, step) / statistics.getParticlesInStep(step),
                                                                     (float)gameTime.ElapsedGameTime.TotalSeconds, maxWidth, height, yPos);
                    break;
                case DiagramType.SPAWN_POINTS:
                    DrawDiagram(DIAGRAM_DESCRIPTIONS[(int)currentDiagramType], spriteBatch, (step, player) => (float)statistics.getPossessingBasesInStep(player, step) / statistics.OverallNumberOfBases,
                                                                     (float)gameTime.ElapsedGameTime.TotalSeconds, maxWidth, height, yPos);
                    break;
            }

            // continue button
            text = "continue";
            width = (int)menu.Font.MeasureString(text).X;
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, text, new Vector2((int)((Settings.Instance.ResolutionX - width) * 0.5f), 
                                                                                    Settings.Instance.ResolutionY - 80), true, menu.TexPixel);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="heightFunction">function returning a height for a given (1st para) step from a given (2nd para) player. Sum of heights in a step should be one!</param>
        /// <param name="frameTimeInterval"></param>
        /// <param name="area"></param>
        private void DrawDiagram(string description, SpriteBatch spriteBatch, Func<int, int, float> heightFunction, float frameTimeInterval, int maxWidth, int heightVal, int yPos)
        {
            Rectangle area;

            const int ARROW_SIZE = 50;
            const int ARROW_PADDING = 20;

            int diagramAreaMaxWidth = maxWidth - ARROW_SIZE * 2 - ARROW_PADDING * 2;
            int stepWidth = Math.Max(1, diagramAreaMaxWidth / statistics.Steps);
            int startStep;
            if (stepWidth == 1)
                startStep = (int)((float)(statistics.Steps - diagramAreaMaxWidth) / stepWidth + 0.5f);   // if a step is only a pixel... DAMDAM
            else
                startStep = 0;
            area.Width = stepWidth * (statistics.Steps - startStep) + 20;

            area.Height = heightVal;
            area.X = (menu.ScreenWidth-area.Width) / 2;
            area.Y = yPos + (int)menu.FontHeading.MeasureString(DIAGRAM_DESCRIPTIONS[(int)currentDiagramType]).Y + SimpleButton.PADDING;


            // draw heading
            SimpleButton.Instance.Draw(spriteBatch, menu.FontHeading, description,
                                       new Vector2(area.X + SimpleButton.PADDING, yPos),
                                       false, menu.TexPixel);

            // draw border
            spriteBatch.Draw(menu.TexPixel, area, Color.Black);
            area.Inflate(-10, -10);

            // if this stuff could slow down the whole game.... suprise suprise, let's optimize! ;P

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

                // this ehm.. could be a single quad
                if (offset < area.Height)
                {
                    int height = area.Height - offset;
                    Rectangle rect = new Rectangle(area.X + (step - startStep) * stepWidth, area.Top, stepWidth, height);
                    spriteBatch.Draw(menu.TexPixel, rect, Color.Gray);
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
                            x += (int)(stepWidth * ((float)Level.SWITCH_COUNTDOWN_LENGTH / statistics.StepTime - 0.5f));
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
                if(i == 0)
                    textOffset = SimpleButton.PADDING;
                else if (i == numTimeDisplays-1)
                    textOffset = -textLen - SimpleButton.PADDING;
                else
                    textOffset = -textLen / 2;
                
                float x = area.X + area.Width * progress + textOffset;
                SimpleButton.Instance.Draw(spriteBatch, menu.Font, endTimeString, new Vector2(x, area.Y + area.Height + 10), false, menu.TexPixel);
            }

            // arrows
            int arrowY = area.Y + (area.Height - ARROW_SIZE) / 2;//boxHeight / 2 - ARROW_SIZE;
            SimpleButton.Instance.DrawTexture_NoScalingNoPadding(spriteBatch, icons, new Rectangle(area.Left - ARROW_SIZE - ARROW_PADDING, arrowY, ARROW_SIZE, ARROW_SIZE),
                                                                new Rectangle(0 + (!InputManager.Instance.AnyLeftButtonDown() ? 32 : 0), 0, 16, 16), InputManager.Instance.AnyLeftButtonDown(), menu.TexPixel);
            SimpleButton.Instance.DrawTexture_NoScalingNoPadding(spriteBatch, icons, new Rectangle(area.Right + ARROW_PADDING, arrowY, ARROW_SIZE, ARROW_SIZE),
                                                                new Rectangle(16 + (!InputManager.Instance.AnyRightButtonDown() ? 32 : 0), 0, 16, 16), InputManager.Instance.AnyRightButtonDown(), menu.TexPixel);

        }

        private string GenerateTimeString(float time)
        {
            int minutes = (int)(time / 60.0f);
            int seconds = (int)(time - minutes * 60 + 0.5f);
            return String.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}
