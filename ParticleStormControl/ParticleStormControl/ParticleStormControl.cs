//#define NVIDIAPERFHUD_POSSIBLE
//#define DAMAGEMAP_DEBUGGING

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


#if XBOX

using System.Threading;

#else

using System.Threading.Tasks;
using ParticleStormControl;
using ParticleStormControl.Menu;

#endif

namespace ParticleStormControl
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ParticleStormControl : Microsoft.Xna.Framework.Game
    {
        public const string VERSION = "pscd XX.04.2013";

        private bool showStatistics = true;
     
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        #region frame rate measurement

        private float frameRateCounterElapsed;
        private float frameRate;
        private float frames;

        #endregion

        private InGame inGame;
        private Menu.Menu menu;

        public InGame InGame // haaaack alert!
        {
            get { return inGame; }
        }

        public ParticleStormControl()
        {
            graphics = new GraphicsDeviceManager(this);
#if NVIDIAPERFHUD_POSSIBLE
            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
#endif

            graphics.SynchronizeWithVerticalRetrace = true;
            /* too high framerates kill the particle behaviour!
            // no vsync if statistics are wanted
#if SHOW_STATISTICS
            graphics.SynchronizeWithVerticalRetrace = false;
#endif
             * */

            Random.InitRandom((uint)DateTime.Now.Millisecond);

            // read start settings
            Settings.Instance.ReadSettings();

            // apply settings
            if (graphics.IsFullScreen != Settings.Instance.Fullscreen)
                graphics.ToggleFullScreen();
            graphics.PreferredBackBufferWidth = Settings.Instance.ResolutionX;
            graphics.PreferredBackBufferHeight = Settings.Instance.ResolutionY;

            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
            IsFixedTimeStep = false;

            Content.RootDirectory = "Content";
            Window.AllowUserResizing = false;
         //   Window.ClientSizeChanged += new EventHandler<EventArgs>(WindowClientSizeChanged);
            Window.Title = "Particle Storm Control ~Deluxe~";
        }

        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
           foreach (GraphicsAdapter adapter in GraphicsAdapter.Adapters)
           {
              if (adapter.Description.Contains("PerfHUD"))
              {
                 e.GraphicsDeviceInformation.Adapter = adapter;
                 GraphicsAdapter.UseReferenceDevice = true;
                 break;
              }
           }
        }

        public void ApplyChangedGraphicsSettings()
        {
            if (graphics.IsFullScreen != Settings.Instance.Fullscreen)
                graphics.ToggleFullScreen();
            graphics.PreferredBackBufferWidth = Settings.Instance.ResolutionX;
            graphics.PreferredBackBufferHeight = Settings.Instance.ResolutionY;
            graphics.ApplyChanges();
            inGame.Resize(GraphicsDevice);
        }

        void WindowClientSizeChanged(object sender, EventArgs e)
        {
            graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            graphics.ApplyChanges();
            inGame.Resize(GraphicsDevice);
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

            menu.PageChangingEvent += OnMenuPageChanged;
            menu.PageChangingEvent += inGame.OnMenuPageChanged;
            
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
        /// callback for menu page changed
        /// </summary>
        public void OnMenuPageChanged(Menu.Menu.Page newPage, Menu.Menu.Page oldPage)
        {
            if (newPage == Menu.Menu.Page.INGAME && inGame.State == InGame.GameState.Inactive)
                inGame.StartNewGame(GraphicsDevice, Content);
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            float passedFrameTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

            InputManager.Instance.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            menu.Update(gameTime);

            SimpleButton.Instance.Update(gameTime);

            inGame.Update(gameTime);

            if (InputManager.Instance.PressedButton(Keys.F1))
                showStatistics = !showStatistics;

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // offsite stuff
            inGame.Draw_OffsiteBuffers(gameTime, GraphicsDevice);


            GraphicsDevice.Clear(Color.Black);

            inGame.Draw_Backbuffer(gameTime, spriteBatch);
            menu.Draw((float)gameTime.ElapsedGameTime.TotalSeconds, spriteBatch);


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

                spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
                SimpleButton.Instance.Draw(spriteBatch, menu.Font, statistic, new Vector2(5, 5), Color.FromNonPremultiplied(0, 0, 0, 128), menu.PixelTexture);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
