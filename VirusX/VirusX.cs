using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CustomExtensions
{
    /// <summary>
    /// some helpful methods for Points
    /// sorry, no operator overloading :(
    /// </summary>
    public static class PointExtensions
    {
        public static Point Add(this Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static Point Add(this Point p, int value)
        {
            return new Point(p.X + value, p.Y + value);
        }

        public static Vector2 ToVector2(this Point p)
        {
            return new Vector2(p.X, p.Y);
        }
    }
}

namespace VirusX
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    class VirusX : Microsoft.Xna.Framework.Game
    {
        private bool showStatistics = false;
     
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        #region frame rate measurement

        private float frameRateCounterElapsed;
        private float frameRate;
        private float frames;

        #endregion

        private InGame inGame;
        private Menu.Menu menu;
        private Tutorial tutorial;
        private Background background;

        private bool firstUpdate = true;
        private bool isResizing = false;

        public VirusX()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.SynchronizeWithVerticalRetrace = true; // V-Sync always on.

            Random.InitRandom((uint)DateTime.Now.Ticks);

            // read start settings
            Settings.Instance.ReadSettings();

            // Apply settings.
            ApplyChangedGraphicsSettings();

            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
            IsFixedTimeStep = false;

            Content.RootDirectory = "Content";
            Window.AllowUserResizing = false;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(WindowClientSizeChanged);
            Window.Title = "Virus X";
            Window.AllowUserResizing = true; // Is true on UWP anyway, so activate it always.

            AudioManager.Instance.Initialize(Content);
        }

        public void ApplyChangedGraphicsSettings()
        {
            // Depending on the platform graphics.PreferredBackBufferWidth/Height might already been set, but the rest of this call is still necessary.
            // However, ApplyChanges might lead to implicit recursive calls. Therefore the following low-tec solution:
            if (isResizing) return;
            isResizing = true;

            graphics.PreferredBackBufferWidth = Settings.Instance.ResolutionX;
            graphics.PreferredBackBufferHeight = Settings.Instance.ResolutionY;
            if (graphics.IsFullScreen != Settings.Instance.Fullscreen)
                graphics.ToggleFullScreen();
            graphics.ApplyChanges();

            // If we are in the init phase the GraphicsDevice might not yet be up.
            if (GraphicsDevice != null)
            {
                GraphicsDevice.Viewport = new Viewport(0, 0,
                    GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            }
            // Same applies to the game.
            if (inGame != null)
            {
                inGame.Resize(GraphicsDevice);
            }

            isResizing = false;
        }

        void WindowClientSizeChanged(object sender, EventArgs e)
        {
            if (!isResizing)
            {
                Settings.Instance.ResolutionX = Window.ClientBounds.Width;
                Settings.Instance.ResolutionY = Window.ClientBounds.Height;
                ApplyChangedGraphicsSettings();
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            menu = new Menu.Menu(this);
            inGame = new InGame(menu);
            tutorial = new Tutorial(this);

            menu.PageChangingEvent += inGame.OnMenuPageChanged;
            menu.PageChangingEvent += tutorial.OnMenuPageChanged;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            inGame.LoadContent(GraphicsDevice, Content);
            menu.LoadContent(Content);
			tutorial.LoadContent(Content);

            background = new Background(GraphicsDevice, Content);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Settings.Instance.Save();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            AudioManager.Instance.PlaySongsRandom();
            InputManager.Instance.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (firstUpdate)
            {
                menu.ChangePage(global::VirusX.Menu.Menu.Page.MAINMENU, gameTime);
                firstUpdate = false;
            }

            menu.Update(gameTime);
            inGame.Update(gameTime);
            tutorial.Update(gameTime);

            if (InputManager.Instance.IsButtonPressed(Keys.F12))
                showStatistics = !showStatistics;
            // toggle automatic item deletion
#if DEBUG
            if (InputManager.Instance.IsButtonPressed(Keys.F11))
                Settings.Instance.AutomaticItemDeletion = !Settings.Instance.AutomaticItemDeletion;
#endif
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (spriteBatch.IsDisposed)
                return;

            // offsite stuff
            inGame.Draw_OffsiteBuffers(gameTime, GraphicsDevice);

            // draw backbuffer
            GraphicsDevice.Clear(Color.Black);

            inGame.Draw_Backbuffer(spriteBatch, gameTime);
            menu.Draw(spriteBatch, gameTime);
            tutorial.Draw(spriteBatch, gameTime);

            // show statistics
            if (showStatistics)
            {
                frameRateCounterElapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (frameRateCounterElapsed > 1.0f)
                {
                    frameRateCounterElapsed -= 1.0f;
                    frameRate = frames;
                    frames = 0;
                }
                else
                    frames += 1;

                string statistic = "";
                statistic += gameTime.ElapsedGameTime.TotalMilliseconds.ToString("000.0") + "ms | " + frameRate.ToString("0.0") + "fps";

                int totalParticleCount = 0;
                if (inGame.State == InGame.GameState.Playing)
                {
                    for (int i = 0; i < inGame.Players.Length; ++i)
                    {
                        statistic += "\nPlayer" + i + ": NumParticles: " + inGame.Players[i].NumParticlesAlive + " | HighestUsedIndex: " +
                                            inGame.Players[i].HighestUsedParticleIndex + " | NumSpawns: " + inGame.Players[i].CurrentSpawnNumber;
                        totalParticleCount += inGame.Players[i].NumParticlesAlive;
                    }
                    statistic += "\nTotalParticles: " + totalParticleCount;
                }

#if DEBUG
                statistic += "\nAutomaticItemDeletion: " + Settings.Instance.AutomaticItemDeletion;
#endif

                spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
                spriteBatch.DrawString(menu.Font, statistic, new Vector2(5, 5), Color.White);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}