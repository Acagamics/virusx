//#define DAMAGEMAP_DEBUGGING

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Threading.Tasks;
using System.Diagnostics;

#if XBOX
using System.Threading;
#endif

namespace ParticleStormControl
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class InGame
    {
        private GraphicsDevice graphicsDevice;

        /// <summary>
        /// Statistics
        /// </summary>
        public Statistics GameStatistics { get { if (level == null) return null; return level.GameStatistics; } }

        private Menu.Menu menu;

        private ParticleRenderer particleRenderer;
        private DamageMap damageMap;

        public Player[] Players { get { return players;  } }
        private Player[] players;
        /// <summary>
        /// noise texture needed for players, generated only once!
        /// </summary>
        private Texture2D noiseWhite2D;

        Level level;
        InGameInterface inGameInterface;
        PercentageBar percentageBar;

        public GameState State { get; private set; }
        private int winPlayerIndex = -1;

        /// <summary>
        /// without this timer, the player would instantly die because of 0 particles
        /// </summary>
        private const float instantDeathProtectingDuration = 1.0f;
        private float instantDeathProtectingTime = 0.0f;

        Song song;

        // damaging is only every second frame - switch every frame to be xbox friendly
        bool levelDamageFrame = false;

        public Level Level
        {
            get { return level; }
        }

        public enum GameState
        {
            Inactive,
            Playing,
            Paused,
        }

        public InGame(Menu.Menu menu)
        {
            this.menu = menu;
            State = GameState.Inactive;
        }

        /// <summary>
        /// callback for menu page changed
        /// </summary>
        public void OnMenuPageChanged(Menu.Menu.Page newPage, Menu.Menu.Page oldPage)
        {
            if (newPage == Menu.Menu.Page.PAUSED)
                State = InGame.GameState.Paused;
            else if (newPage == Menu.Menu.Page.INGAME)
                State = InGame.GameState.Playing;
            else
            {
                // generic level as background
                if (State != InGame.GameState.Inactive)
                {
                    level.NewEmptyLevel(graphicsDevice);
                    particleRenderer = null;
                }

                State = InGame.GameState.Inactive;
            }
        }

        public void StartNewGame(GraphicsDevice graphicsDevice, ContentManager content)
        {
            instantDeathProtectingTime = 0.0f;

            players = new Player[Settings.Instance.NumPlayers];
            int count = 0;
            for (int i = 0; i < 3; ++i)
            {
                if (Settings.Instance.PlayerConnected[i])
                {
                    players[count] = new Player(i, graphicsDevice, content, noiseWhite2D, Settings.Instance.PlayerColorIndices[i]);
                    players[count].Controls = Settings.Instance.PlayerControls[i];
                    count++;
                }
            }

            // restart stuff
            level.NewGame(players);
            particleRenderer = new ParticleRenderer(graphicsDevice, content, players.Length);

            // init statistics
            level.GameStatistics = new Statistics(1f, Settings.Instance.NumPlayers);

            State = GameState.Playing;
            System.GC.Collect();
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        public void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            this.graphicsDevice = graphicsDevice;

            damageMap = new DamageMap();
            noiseWhite2D = NoiseTexture.GenerateNoise2D16f(graphicsDevice, 64, 64);

            damageMap.LoadContent(graphicsDevice);

            level = new Level(graphicsDevice, content);
            level.NewEmptyLevel(graphicsDevice); // fake level for background
            inGameInterface = new InGameInterface(content);
            percentageBar = new PercentageBar(content);

            // backgroundmusic
            song = content.Load<Song>("paniq_Godshatter");
            MediaPlayer.Volume = 0.5f;
            //MediaPlayer.Play(song);
            MediaPlayer.IsRepeating = true;
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            float passedFrameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // playing
            if (State == GameState.Playing)
            {
                // do controlling
                foreach (Player player in players)
                    player.UserControl(passedFrameTime, level);

                // switching - wrong placed this leads player errors
                level.UpdateSwitching(passedFrameTime, players);

                // instant death protecting timer
                instantDeathProtectingTime += passedFrameTime;
                bool playerCantDie = instantDeathProtectingTime < instantDeathProtectingDuration;

                // spawning & move - parallel!
#if XBOX
                // threads for xbox
                ManualResetEvent[] waitHandles = new ManualResetEvent[players.Length];
                for (int i = 0; i < players.Length; ++i)
                {
                    waitHandles[i] = new ManualResetEvent(false);
                    ThreadPool.QueueUserWorkItem(delegate(object obj)
                                                    {
                                                        int index = (int)obj;
                                                        players[(int)index].UpdateCPUPart(passedFrameTime, level.mapObjects, playerCantDie);
                                                        waitHandles[index].Set();
                                                    }, i);
                }
                WaitHandle.WaitAll(waitHandles);
#else
                Task[] playerTaskList = new Task[players.Length];
                var func = new Action<object>((index) =>
                {
                    try
                    {
                        players[(int)index].UpdateCPUPart(passedFrameTime, level.SpawnPoints, playerCantDie);
                    }
                    catch(Exception exp)
                    {
                        Console.WriteLine(exp);
                        Debugger.Break();
                    }
                });
                for (int i = 0; i < players.Length; ++i)
                    playerTaskList[i] = Task.Factory.StartNew(func, i);
                Task.WaitAll(playerTaskList);
#endif
                // wining
                CheckWinning(passedFrameTime);

                // damaging - switch every frame to be xbox friendly (preserve content stuff)
                // see also correlating gpu-functions
                if (levelDamageFrame)
                {
                    damageMap.UpdateCPUData();
                    level.ApplyDamage(damageMap, passedFrameTime, players);
                }

                // level update
                level.Update((float)gameTime.ElapsedGameTime.TotalSeconds, passedFrameTime, players);
            }
        }

        private void CheckWinning(float passedFrameTime)
        {
            // only one player alive
            winPlayerIndex = -1;
            for (int i = 0; i < players.Length; ++i)
            {
                if (players[i].Alive)
                {
                    if (winPlayerIndex != -1)
                    {
                        winPlayerIndex = -1;
                        break;
                    }
                    else
                        winPlayerIndex = i;
                }
            }
            if (winPlayerIndex != -1)
            {
                // statistics
                level.GameStatistics.addWonMatches(winPlayerIndex);

                State = GameState.Inactive;
                ((Menu.Win)menu.GetPage(Menu.Menu.Page.WIN)).ShownWinnerColorIndex = Settings.Instance.PlayerColorIndices[winPlayerIndex];
                menu.ChangePage(Menu.Menu.Page.WIN);
            }
        }


        public void Draw_OffsiteBuffers(GameTime gameTime, GraphicsDevice graphicsDevice)
        {
            if (State == GameState.Playing)
            {
                // update damagemap (GPU)
                if (levelDamageFrame)
                    damageMap.UpdateGPU_Map(graphicsDevice, level);
                else
                    damageMap.UpdateGPU_Particles(graphicsDevice, particleRenderer, players);
                levelDamageFrame = !levelDamageFrame;
             //   graphicsDevice.SetRenderTarget(null);

                // update player gpu
                for (int i = 0; i < players.Length; ++i)
                    players[i].UpdateGPUPart(graphicsDevice, (float)gameTime.ElapsedGameTime.TotalSeconds, damageMap.DamageTexture);
                graphicsDevice.SetRenderTarget(null);

                level.BeginDrawInternParticleTarget(graphicsDevice);
                particleRenderer.Draw(graphicsDevice, players, false);
                level.EndDrawInternParticleTarget(graphicsDevice);
            }
        }

        /// <summary>
        /// This is called when the game should draw itself to the backbuffer
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// <param name="spriteBatch">a unstarted spritebatch</param>
        public void Draw_Backbuffer(GameTime gameTime, SpriteBatch spriteBatch)
        {
            float timeSinceLastFrame = (float)gameTime.TotalGameTime.TotalSeconds;

            // draw level even if inactive as background
            level.Draw(timeSinceLastFrame, spriteBatch.GraphicsDevice, players);
            if (State == GameState.Inactive)
                return;

            inGameInterface.DrawInterface(players, spriteBatch, level.FieldPixelSize, level.FieldPixelOffset, timeSinceLastFrame);

            // debug draw damagemap
#if DAMAGEMAP_DEBUGGING
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque);
            spriteBatch.Draw(damageMap.DamageTexture, new Vector2(Settings.Instance.ResolutionX - DamageMap.attackingMapSizeX, 0), Color.White);
            spriteBatch.End();  
#endif

            // draw the percentage bar
            percentageBar.Draw(players, spriteBatch, level.FieldPixelSize, level.FieldPixelOffset, timeSinceLastFrame);

            // reading player gpu results
            for (int i = 0; i < players.Length; ++i)
                players[i].ReadGPUResults();
        }

        public void Resize(GraphicsDevice graphicsDevice)
        {
            if(level != null)
                level.Resize(graphicsDevice);
        }
    }
}
