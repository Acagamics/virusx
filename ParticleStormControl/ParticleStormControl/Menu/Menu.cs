using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace VirusX.Menu
{
    class Menu
    {
        public enum Page
        {
            MAINMENU,
            NEWGAME,
            CREDITS,
            OPTIONS,
            PAUSED,
            CONTROLS,
            VIRUSES,

            INGAME,
            STATS,

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

        #region Textures
        public Texture2D TexPixel { get { return texPixel; } }
        private Texture2D texPixel;

        public int ScreenWidth { get { return game.GraphicsDevice.Viewport.Width; } }
        public int ScreenHeight { get { return game.GraphicsDevice.Viewport.Height; } }
        #endregion

        private Page activePage = Page.MAINMENU;
        public Page ActivePage
        {
            get { return activePage; }
        }
        private MenuPage[] pages = new MenuPage[(int)Page.NUM_PAGES];

        private ParticleStormControl game;
        public ParticleStormControl Game // Haaaack (how else do I get the statistics outside the inGame?)
        { get { return game; } }

        public Menu(ParticleStormControl game)
        {
            this.game = game;

            // preload fonts to measure size at initialization - not very nice!
            font = game.Content.Load<SpriteFont>("fonts/font");
            fontHeading = game.Content.Load<SpriteFont>("fonts/fontHeading");
            fontCountdown = game.Content.Load<SpriteFont>("fonts/fontCountdown");

            pages[(int)Page.MAINMENU] =  new MainMenu(this);
            pages[(int)Page.OPTIONS] = new Options(this);
            pages[(int)Page.PAUSED] = new Paused(this);
            pages[(int)Page.NEWGAME] = new NewGame(this);
            pages[(int)Page.STATS] = new StatisticsScreen(this);
            pages[(int)Page.INGAME] = new InGame(this);
            pages[(int)Page.CREDITS] = new Credits(this);
            pages[(int)Page.CONTROLS] = new Controls(this);
            pages[(int)Page.VIRUSES] = new Viruses(this);
        }

        public void LoadContent(ContentManager content)
        {
            texPixel = content.Load<Texture2D>("pix");

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

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            if(pages[(int)activePage] != null)
                pages[(int)activePage].Draw(spriteBatch, gameTime);
            spriteBatch.End();
        }

        /// <summary>
        /// Returns maximum height of the used fonts
        /// </summary>
        /// <param name="bigFont">The font used for headings</param>
        /// <returns></returns>
        public int GetFontHeight(bool bigFont = false)
        {
            return bigFont ? (int)fontHeading.MeasureString("Mg").Y : (int)font.MeasureString("Mg").Y;
        }

        /// <summary>
        /// Checks a variety of buttons for exit on one button pages
        /// </summary>
        /// <param name="gameTime"></param>
        public void BackToMainMenu(GameTime gameTime)
        {
            if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.PAUSE) ||
                InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.EXIT) ||
                InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.ACTION) ||
                InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.HOLD))
                ChangePage(Menu.Page.MAINMENU, gameTime);
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
            pages[(int)newPage].OnActivated(activePage, gameTime);
            activePage = newPage;

            AudioManager.Instance.PlaySoundeffect("click");
            
            //if(newPage != Page.INGAME)
            //    InterfaceButton.Instance.ChangeHappened(gameTime, SoundEffect);
        }

        public void Shutdown()
        {
            game.Exit();
        }

        #endregion

        #region helper functions

        /// <summary>
        /// Helper function for loopin through menus
        /// </summary>
        /// <param name="selected"></param>
        /// <param name="maximum"></param>
        /// <param name="control">If InputManager.ControlType.NONE, any control is working</param>
        /// <param name="horizontal">If true: left/right, if false: up/down</param>
        /// <returns></returns>
        public static int Loop(int selected, int maximum, InputManager.ControlType control = InputManager.ControlType.NONE, bool horizontal = false)
        {
            int oldSelected = selected;
            // loopin
            if (control == InputManager.ControlType.NONE)
            {
                if (horizontal)
                {
                    if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.RIGHT))
                        selected = selected == maximum - 1 ? 0 : selected + 1;
                    else if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.LEFT))
                        selected = selected == 0 ? maximum - 1 : selected - 1;
                }
                else
                {
                    if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.DOWN))
                        selected = selected == maximum - 1 ? 0 : selected + 1;
                    else if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.UP))
                        selected = selected == 0 ? maximum - 1 : selected - 1;
                }
            }
            else
            {
                if (horizontal)
                {
                    if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.RIGHT, control))
                        selected = selected == maximum - 1 ? 0 : selected + 1;
                    else if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.LEFT, control))
                        selected = selected == 0 ? maximum - 1 : selected - 1;
                }
                else
                {
                    if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.DOWN, control))
                        selected = selected == maximum - 1 ? 0 : selected + 1;
                    else if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.UP, control))
                        selected = selected == 0 ? maximum - 1 : selected - 1;
                }
            }
            
            // if horizontal play sound
            if (horizontal && oldSelected != selected)
                AudioManager.Instance.PlaySoundeffect("click");

            return selected;
        }

        /// <summary>
        /// helper function for toggling boolean settings in menus
        /// </summary>
        /// <param name="selected"></param>
        /// <param name="control">If InputManager.ControlType.NONE, any control is working</param>
        /// <returns></returns>
        public static bool Toggle(bool selected, InputManager.ControlType control = InputManager.ControlType.NONE)
        {
            bool oldSelected = selected;

            if (control == InputManager.ControlType.NONE)
            {
                if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.LEFT)
                    || InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.RIGHT)
                    || InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.ACTION))
                    selected = !selected;
            }
            else
            {
                if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.LEFT, control)
                    || InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.RIGHT, control)
                    || InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.ACTION, control))
                    selected = !selected;
            }

            // if horizontal play sound
            if (oldSelected != selected)
                AudioManager.Instance.PlaySoundeffect("click");

            return selected;
        }

        #endregion
    }
}
