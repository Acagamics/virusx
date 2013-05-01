//#define DAMAGEMAP_DEBUGGING

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Linq;
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

        /// <summary>
        /// list of all players
        /// </summary>
        public Player[] Players { get { return players;  } }
        private Player[] players;

        /// <summary>
        /// noise texture needed for players, generated only once!
        /// </summary>
        private Texture2D noiseWhite2D;

        private Level level;
        private InGameInterface inGameInterface;
        private PercentageBar percentageBar;
        private ParticleRenderer particleRenderer;
        private DamageMap damageMap;

        private ContentManager content;


        /// <summary>
        /// current game state
        /// </summary>
        public GameState State { get; private set; }

        
        /// <summary>
        /// without this timer, the player would instantly die because of 0 particles
        /// </summary>
        private const float instantDeathProtectingDuration = 1.0f;
        private float instantDeathProtectingTime = 0.0f;

        /// <summary>
        /// background music
        /// </summary>
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
                    particleRenderer = null;
                    damageMap.Clear(graphicsDevice);
                }

                State = InGame.GameState.Inactive;
            }
        }

        public void StartNewGame(GraphicsDevice graphicsDevice, ContentManager content)
        {
            instantDeathProtectingTime = 0.0f;

            players = new Player[Settings.Instance.NumPlayers];
            int count = 0;
            for (int i = 0; i < 4; ++i)
            {
                if (Settings.Instance.GetPlayer(i).Type != Player.Type.NONE)
                {
                    if (Settings.Instance.GetPlayer(i).Type == Player.Type.AI)
                    {
                        players[count] = new AIPlayer(i, Settings.Instance.GetPlayer(i).VirusIndex, Settings.Instance.GetPlayer(i).ColorIndex,
                            Settings.Instance.GetPlayer(i).Team, graphicsDevice, content, noiseWhite2D);
                    }
                    else
                    {
                        players[count] = new HumanPlayer(i, Settings.Instance.GetPlayer(i).VirusIndex, Settings.Instance.GetPlayer(i).ColorIndex,
                            Settings.Instance.GetPlayer(i).Team, graphicsDevice, content, noiseWhite2D, Settings.Instance.GetPlayer(i).ControlType);
                    }
                    count++;
                    if (count == Settings.Instance.NumPlayers)
                        break;
                }
            }

            // restart stuff
            level.NewGame(graphicsDevice, players);
            particleRenderer = new ParticleRenderer(graphicsDevice, content, players.Length);

            // init statistics
            level.GameStatistics = new Statistics(Settings.Instance.NumPlayers, 2400, (uint)level.SpawnPoints.Count);

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
            this.content = content;

            damageMap = new DamageMap();
            noiseWhite2D = NoiseTexture.GenerateNoise2D16f(graphicsDevice, 64, 64);

            damageMap.LoadContent(graphicsDevice);

            level = new Level(graphicsDevice, content);
            inGameInterface = new InGameInterface(content);
            percentageBar = new PercentageBar(content);

            song = content.Load<Song>("sound/09 Beach");
            MediaPlayer.Volume = 0.5f;
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

            if (Settings.Instance.Music && MediaPlayer.State == MediaState.Stopped)
                MediaPlayer.Play(song);
            else if (!Settings.Instance.Music && MediaPlayer.State == MediaState.Playing)
                MediaPlayer.Stop();

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
                bool[] playerRecentlyDied = new bool[players.Length];
                Array.Clear(playerRecentlyDied, 0, playerRecentlyDied.Length);
                Task[] playerTaskList = new Task[players.Length];
                var func = new Action<object>((index) =>
                {
                    try
                    {
                        bool alive = players[(int)index].Alive;
                        players[(int)index].UpdateCPUPart(gameTime, level.SpawnPoints, playerCantDie);
                        if (alive && !players[(int)index].Alive)
                            playerRecentlyDied[(int)index] = true; //
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

                // die events
                for (int i = 0; i < playerRecentlyDied.Length; ++i)
                {
                    if (playerRecentlyDied[i])
                    {
                        GameStatistics.playerDied(i);
                        level.AddMapObject(DamageArea.CreatePlayerDeathDamage(content, players[i].CursorPosition, i));
                    }
                }

                // damaging - switch every frame to be xbox friendly (preserve content stuff)
                // see also correlating gpu-functions
                if (levelDamageFrame)
                {
                    damageMap.UpdateCPUData();
                    level.ApplyDamage(damageMap, passedFrameTime, players);
                }

                // level update
                level.Update((float)gameTime.ElapsedGameTime.TotalSeconds, passedFrameTime, players);

                // wining
                CheckWinning(gameTime);
            }
        }

        private void CheckWinning(GameTime gameTime)
        {
            // only one player alive
            int winPlayerIndex = -1;
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

                ((Menu.Win)menu.GetPage(Menu.Menu.Page.WIN)).WinPlayerIndex = winPlayerIndex;
                ((Menu.Win)menu.GetPage(Menu.Menu.Page.WIN)).PlayerTypes = Settings.Instance.GetPlayerSettingSelection(x=>x.Type).ToArray();
                ((Menu.Win)menu.GetPage(Menu.Menu.Page.WIN)).PlayerColorIndices = Settings.Instance.GetPlayerSettingSelection(x => x.ColorIndex).ToArray();
                
                menu.ChangePage(Menu.Menu.Page.WIN, gameTime);
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
                    players[i].UpdateGPUPart(gameTime, graphicsDevice, damageMap.DamageTexture);
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
            if (State == GameState.Inactive)
                return;

            float totalGameTime = (float)gameTime.TotalGameTime.TotalSeconds;
            float timeSinceLastFrame = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // draw level
            level.Draw(totalGameTime, spriteBatch.GraphicsDevice, players);

            inGameInterface.DrawInterface(players, spriteBatch, level.FieldPixelSize, level.FieldPixelOffset, gameTime);

            // debug draw damagemap
#if DAMAGEMAP_DEBUGGING
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque);
            spriteBatch.Draw(damageMap.DamageTexture, new Vector2(Settings.Instance.ResolutionX - DamageMap.attackingMapSizeX, 0), Color.White);
            spriteBatch.End();  
#endif

            // draw the percentage bar
            percentageBar.Draw(players, spriteBatch, level.FieldPixelSize, level.FieldPixelOffset);

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
