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
        private const int ITEM_DISPLAY_SIZE = 40;

        public int WinPlayerIndex { get; set; }
        public bool[] ConnectedPlayers { get; set; }
        public int[] PlayerColorIndices { get; set; }

        public Win(Menu menu)
            : base(menu)
        { }

        public override void OnActivated(Menu.Page oldPage)
        {
            statistics = menu.Game.InGame.Level.GameStatistics;

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
        }

        public override void Update(GameTime gameTime)
        {
            if (InputManager.Instance.ContinueButton())
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, float frameTimeInterval)
        {
            int pad = 250; // padding from left
            int col = 150; // width of the columns
            
            // draw winning string
            string text = Player.ColorNames[PlayerColorIndices[WinPlayerIndex]] + " wins!";
            int width = (int)menu.FontHeading.MeasureString(text).X;
            SimpleButton.Instance.Draw(spriteBatch, menu.FontHeading, text, new Vector2((int)((Settings.Instance.ResolutionX - width) * 0.5f), 60), Player.Colors[PlayerColorIndices[WinPlayerIndex]], menu.PixelTexture);

            // draw all captions
            for (int i = 0; i < captions.Count; i++)
            {
                SimpleButton.Instance.Draw(spriteBatch, menu.Font, captions[i], new Vector2(pad + col * i, 150), false, menu.PixelTexture, col - 20);
            }

            // fill table with values
            for (int i = 0; i < values.Length; i++)
            {
                SimpleButton.Instance.Draw(spriteBatch, menu.Font, Player.ColorNames[PlayerColorIndices[i]], new Vector2(100, 210 + 60 * i), Player.Colors[PlayerColorIndices[i]], menu.PixelTexture, col - 20);
                 for (int j = 0; j < values[0].Count; j++)
                {
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, values[i][j], new Vector2(pad + col * j, 210 + 60 * i), false, menu.PixelTexture, col - 20);
                }
            }

            DrawDiagram(spriteBatch, frameTimeInterval, new Rectangle(90, 250 + 60 * values.Length, Settings.Instance.ResolutionX - 180, Settings.Instance.ResolutionY - 360 - 60 * values.Length));

            // continue button
            text = "continue";
            width = (int)menu.Font.MeasureString(text).X;
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, text, new Vector2((int)((Settings.Instance.ResolutionX - width) * 0.5f), Settings.Instance.ResolutionY - 80), true, menu.PixelTexture);
        }

        private void DrawDiagram(SpriteBatch spriteBatch, float frameTimeInterval, Rectangle area)
        {
            int stepWidth = area.Width / statistics.Steps;
            area.Width = stepWidth * statistics.Steps + 20;

            // draw border
            spriteBatch.Draw(menu.PixelTexture, area, Color.Black);
            area.Inflate(-10, -10);

            // draw amount of particles per player and step
            for (int step = 0; step < statistics.Steps; step++)     // if this stuff slows down the whole game.... suprise suprise, let's optimize! ;P
            {
                uint total = (uint)statistics.getHealthInStep(step);
                int offset = 0;
                for (int j = 0; j < statistics.PlayerCount; j++)
                {
                    int height;
                    if (total != 0)
                        height = (int)((statistics.getHealthInStep(j, step) * (uint)area.Height) / total);
                    else
                        height = area.Height / statistics.PlayerCount;

                    Rectangle rect = new Rectangle(area.X + step * stepWidth, area.Y + offset, stepWidth, height);
                    spriteBatch.Draw(menu.PixelTexture, rect, Player.Colors[PlayerColorIndices[j]]);

                    offset += height;
                }
            }

            // draw items
            for (int step = 0; step < statistics.Steps; step++)     // if this stuff slows down the whole game.... suprise suprise, let's optimize! ;P
            {
                uint total = (uint)statistics.getHealthInStep(step);
                int offset = 0;
                for (int j = 0; j < statistics.PlayerCount; j++)
                {
                    int height;
                    if (total != 0)
                        height = (int)((statistics.getHealthInStep(j, step) * (uint)area.Height) / total);
                    else
                        height = area.Height / statistics.PlayerCount;

                    // render
                    Statistics.StatItems? itemUsed = statistics.getFirstUsedItemInStep(j, step);
                    if (itemUsed != null)
                    {
                        spriteBatch.Draw(itemTextures[(int)itemUsed],
                            new Rectangle(area.X + step * stepWidth - ITEM_DISPLAY_SIZE / 2,
                                          area.Y + offset + (height - ITEM_DISPLAY_SIZE) / 2, ITEM_DISPLAY_SIZE, ITEM_DISPLAY_SIZE),
                            Color.White);
                    }

                    offset += height;
                }
            }

            // hide rounding errors
         /*    area.Inflate(10, 10);
           area.Y = area.Bottom - 12;
            area.Height = 12;
            spriteBatch.Draw(menu.PixelTexture, area, Color.Black); */
        }
    }
}
