using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ParticleStormControl.Menu
{
    public class Menu
    {
        public enum Page
        {
            MAINMENU,
            NEWGAME,
            CREDITS,
            OPTIONS,
            PAUSED,

            INGAME,
            WIN,

            NUM_PAGES,
        };

        #region Fonts
        public SpriteFont Font { get { return font; } }
        private SpriteFont font;

        public SpriteFont FontSmall { get { return fontSmall; } }
        private SpriteFont fontSmall;
        #endregion

        public int ScreenWidth { get { return game.GraphicsDevice.Viewport.Width; } }
        public int ScreenHeight { get { return game.GraphicsDevice.Viewport.Height; } }

        public Texture2D PixelTexture { get { return pixelTexture; } }
        private Texture2D pixelTexture;

        private Page activePage = Page.MAINMENU;
        public Page ActivePage
        {
            get { return activePage; }
            set { ChangePage(value); }
        }
        private MenuPage[] pages = new MenuPage[(int)Page.NUM_PAGES];

        private ParticleStormControl game;

        public Menu(ParticleStormControl game)
        {
            this.game = game;

            pages[(int)Page.MAINMENU] =  new MainMenu(this);
            pages[(int)Page.OPTIONS] = new Options(this);
            pages[(int)Page.PAUSED] = new Paused(this);
            pages[(int)Page.NEWGAME] = new NewGame(this);
            pages[(int)Page.WIN] = new Win(this);
            pages[(int)Page.INGAME] = new InGame(this);
            pages[(int)Page.CREDITS] = new Credits(this);
        }

        public void LoadContent(ContentManager content)
        {
            font = content.Load<SpriteFont>("font");
            fontSmall = content.Load<SpriteFont>("fontSmall");
            pixelTexture = content.Load<Texture2D>("pix");
            foreach (MenuPage page in pages)
            {
                if (page != null)
                    page.LoadContent(content);
            }
        }

        public void Update(float frameTimeInterval)
        {
            if (pages[(int)activePage] != null)
                pages[(int)activePage].Update(frameTimeInterval);
        }
        public void Draw(float frameTimeInterval, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            if(pages[(int)activePage] != null)
                pages[(int)activePage].Draw(spriteBatch, frameTimeInterval);
            spriteBatch.End();
        }

        /// <summary>
        /// exit command to end the game
        /// </summary>
        public void Exit()
        {
            game.Exit();
        }

        /// <summary>
        /// applies new graphics settings
        /// </summary>
        public void ApplyChangedGraphicsSettings()
        {
            game.ApplyChangedGraphicsSettings();
        }

        public MenuPage GetPage(Page page)
        {
            return pages[(int)page];
        }


        #region controlling functions

        public delegate void PageChanging(Page newPage, Page oldPage);
        public event PageChanging PageChangingEvent;

        public void ChangePage(Page newPage)
        {
            if(PageChangingEvent != null)
                PageChangingEvent(newPage, activePage);
            activePage = newPage;
        }

        public void Shutdown()
        {
            game.Exit();
        }

        #endregion
    }
}
