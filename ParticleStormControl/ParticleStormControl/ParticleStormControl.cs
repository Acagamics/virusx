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
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

#endif

namespace ParticleStormControl
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ParticleStormControl : Microsoft.Xna.Framework.Game
    {
        public const string VERSION = "version 0.8 - 15.04.2013";

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
        private Background background;

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

            Random.InitRandom((uint)DateTime.Now.Ticks);

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
            Window.ClientSizeChanged += new EventHandler<EventArgs>(WindowClientSizeChanged);
            Window.Title = "Virus X";
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

            GraphicsDevice.Viewport = new Viewport(0, 0, 
                GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);

            inGame.Resize(GraphicsDevice);
            if (background != null)
                RegenerateBackground();
        }

        void WindowClientSizeChanged(object sender, EventArgs e)
        {
           /* graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            graphics.ApplyChanges();
            inGame.Resize(GraphicsDevice);
            RegenerateBackground(); */
            ApplyChangedGraphicsSettings();
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

            background = new Background(GraphicsDevice, Content);
            RegenerateBackground();
        }

        private void RegenerateBackground()
        {
            // generate cell positions for menu background
            Vector2 relativeMax = new Vector2(GraphicsDevice.Viewport.AspectRatio, 1.0f);
            List<Vector2> cellPositions = Level.GenerateCellPositions(7, 4, 0.09f, Vector2.Zero, relativeMax);
            background.Resize(GraphicsDevice, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), cellPositions, relativeMax);
            background.UpdateColors(Enumerable.Repeat(Color.White, cellPositions.Count).ToArray()); 
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

            //InterfaceButton.Instance.Update(gameTime);

            inGame.Update(gameTime);

            if (InputManager.Instance.IsButtonPressed(Keys.F12))
                showStatistics = !showStatistics;
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

            if (inGame.State == global::ParticleStormControl.InGame.GameState.Inactive)
                background.Draw(GraphicsDevice, (float)gameTime.TotalGameTime.TotalSeconds);

            inGame.Draw_Backbuffer(gameTime, spriteBatch);
            menu.Draw(gameTime, spriteBatch);

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
                spriteBatch.DrawString(menu.Font, statistic, new Vector2(5, 5), Color.White);
                spriteBatch.End();
            }

            base.Draw(gameTime);

            // screenshots
#if WINDOWS
            if (InputManager.Instance.IsButtonPressed(Keys.PrintScreen))
            {
                int w = GraphicsDevice.PresentationParameters.BackBufferWidth;
                int h = GraphicsDevice.PresentationParameters.BackBufferHeight;

                // pull the picture from the buffer
                int[] backBuffer = new int[w * h];
                GraphicsDevice.GetBackBufferData(backBuffer);

                // copy into a texture
                using (Texture2D texture = new Texture2D(GraphicsDevice, w, h, false, GraphicsDevice.PresentationParameters.BackBufferFormat))
                {
                    // yeah this sucks.. but is the official solution
                    texture.SetData(backBuffer);

                    // find
                    string filename;
                    int screenshot = 0;
                    do
                    {
                        ++screenshot;
                        filename = "Screenshot" + screenshot + ".png";
                    } while (System.IO.File.Exists(filename));

                    // save to disk
                    using (var stream = System.IO.File.OpenWrite(filename))
                    {
                        texture.SaveAsPng(stream, w, h);
                        stream.Close();
                    }
                }
            }
#endif
        }
    }
}
