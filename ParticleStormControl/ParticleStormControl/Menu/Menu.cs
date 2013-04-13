using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

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

        public SpriteFont FontHeading { get { return fontHeading; } }
        private SpriteFont fontHeading;

        public SpriteFont FontCountdown { get { return fontCountdown; } }
        private SpriteFont fontCountdown;
        #endregion

        #region Sound
        private SoundEffect soundEffect;
        public SoundEffect SoundEffect { get { return soundEffect; } }
        #endregion

        public int ScreenWidth { get { return game.GraphicsDevice.Viewport.Width; } }
        public int ScreenHeight { get { return game.GraphicsDevice.Viewport.Height; } }

        public Texture2D PixelTexture { get { return pixelTexture; } }
        private Texture2D pixelTexture;

        private Page activePage = Page.MAINMENU;
        public Page ActivePage
        {
            get { return activePage; }
        }
        private MenuPage[] pages = new MenuPage[(int)Page.NUM_PAGES];

        private ParticleStormControl game;

        public ParticleStormControl Game // Haaaack (how else do I get the statistics outside the inGame?)
        {
            get { return game; }
        }

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
            font = content.Load<SpriteFont>("fonts/font");
            fontHeading = content.Load<SpriteFont>("fonts/fontHeading");
            fontCountdown = content.Load<SpriteFont>("fonts/fontCountdown");
            pixelTexture = content.Load<Texture2D>("pix");
            soundEffect = content.Load<SoundEffect>("sound/room__snare-switchy");
            foreach (MenuPage page in pages)
            {
                if (page != null)
                    page.LoadContent(content);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (pages[(int)activePage] != null)
                pages[(int)activePage].Update(gameTime);
        }

        public void Draw(float frameTimeInterval, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
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

        public void ChangePage(Page newPage, GameTime gameTime)
        {
            if(PageChangingEvent != null)
                PageChangingEvent(newPage, activePage);
            pages[(int)newPage].OnActivated(activePage);
            activePage = newPage;
            if(newPage != Page.INGAME)
                SimpleButton.Instance.ChangeHappened(gameTime, SoundEffect);
        }

        public void Shutdown()
        {
            game.Exit();
        }

        #endregion
    }
}
