//#define DAMAGEMAP_DEBUGGING
//#define STATS_TEST
#if !DEBUG
#undef STATS_TEST
#undef DAMAGEMAP_DEBUGGING
#endif

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

using CustomExtensions;


namespace VirusX
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    class InGame
    {
        private GraphicsDevice graphicsDevice;

        #region GameMode

        public enum GameMode
        {
            // no teams, up to 4 players
            CLASSIC,

            // start player -> defender, up to 3 others -> attackers
            CAPTURE_THE_CELL,

            // 2 teams, each up to 2 players
            LEFT_VS_RIGHT,

            // no teams, up to 4 players
            FUN,

            // no teams, up to 4 plyers
            INSERT_MODE_NAME,

            // 1 player vs 1 computer
            TUTORIAL,

            // 1 player arcarde mode
            ARCADE,

            NUM_MODES
        };
        static public readonly String[] GAMEMODE_NAME = new String[]
        {
            "Classic",
            "Capture the Cell",
            "Left vs. Right",
            "Fun",
            "Insert mode name",
            "Tutorial",
            "Arcade"
        };

        #endregion

        /// <summary>
        /// Statistics
        /// </summary>
        public Statistics GameStatistics { get { if (level == null) return null; return level.GameStatistics; } }

        /// <summary>
        /// reference to the menu
        /// </summary>
        private Menu.Menu menu;

        /// <summary>
        /// list of all players
        /// </summary>
        public Player[] Players { get { return players;  } }
        private Player[] players = new Player[0];

        // for Game Mode INSERT_MODE_NAME
        private Stopwatch[] winTimer;
        public Stopwatch[] WinTimer { get { return winTimer; } }
        public static float ModeWinTime = 25f;

        /// <summary>
        /// noise texture needed for players, generated only once!
        /// </summary>
        private Texture2D noiseWhite2D;

        private Level level;
        private InGameInterface inGameInterface;
        private PostProcessing postPro;
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

        // damaging is only every second frame - switch every frame to be xbox friendly
        bool levelDamageFrame = false;

        public Level Level
        {
            get { return level; }
        }

        public enum GameState
        {
    //        Demo,
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
            if (newPage == Menu.Menu.Page.PAUSED || (newPage == Menu.Menu.Page.CONTROLS && oldPage == Menu.Menu.Page.INGAME ))
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
                    level.NewGame(MapGenerator.MapType.BACKGROUND, graphicsDevice, new Player[0]);
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
                        players[count] = new AIPlayer(i, Settings.Instance.GetPlayer(i).Virus, Settings.Instance.GetPlayer(i).ColorIndex,
                            Settings.Instance.GetPlayer(i).Team, graphicsDevice, content, noiseWhite2D);
                    }
                    else
                    {
                        players[count] = new HumanPlayer(i, Settings.Instance.GetPlayer(i).Virus, Settings.Instance.GetPlayer(i).ColorIndex,
                            Settings.Instance.GetPlayer(i).Team, graphicsDevice, content, noiseWhite2D, Settings.Instance.GetPlayer(i).ControlType);
                    }
                    count++;
                    if (count == Settings.Instance.NumPlayers)
                        break;
                }
            }

            // restart stuff
            MapGenerator.MapType mapType;
            switch(Settings.Instance.GameMode)
            {
                case GameMode.CAPTURE_THE_CELL:
                    mapType = MapGenerator.MapType.CAPTURE_THE_CELL;
                    break;
                case GameMode.FUN:
                    mapType = MapGenerator.MapType.FUN;
                    break;
                default:
                    mapType = MapGenerator.MapType.NORMAL;
                    break;

            }
            level.NewGame(mapType, graphicsDevice, players);
            particleRenderer = new ParticleRenderer(graphicsDevice, content, players.Length);

            // init statistics
            level.GameStatistics = new Statistics(Settings.Instance.NumPlayers, 2400, (uint)level.SpawnPoints.Count);

            // collect the virus types
            foreach (Player player in players)
                level.GameStatistics.SetVirusType(player.playerIndex, player.Virus);

            State = GameState.Playing;
            System.GC.Collect();

            // for Game Mode INSERT_MODE_NAME
            //if (Settings.Instance.GameMode == GameMode.INSERT_MODE_NAME)
            //{
                winTimer = new Stopwatch[players.Length];
                for (int index = 0; index < players.Length; ++index)
                    winTimer[index] = new Stopwatch();
            //}
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

            postPro = new PostProcessing(graphicsDevice, content, level.FieldPixelSize.ToVector2(), level.FieldPixelOffset.ToVector2());
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            float passedFrameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            AudioManager.Instance.PlaySongsRandom();

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
                            playerRecentlyDied[(int)index] = true;
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
                        GameStatistics.playerDied(i);
                }

                // damaging - switch every frame to be xbox friendly (preserve content stuff)
                // see also correlating gpu-functions
                if (levelDamageFrame)
                {
                    damageMap.UpdateCPUData();
                    level.ApplyDamage(damageMap, passedFrameTime, players);
                }

                // level update
                level.Update(gameTime, players);

                if(Settings.Instance.GameMode == GameMode.INSERT_MODE_NAME && State == GameState.Playing)
                {
                    for(int index = 0;index < winTimer.Length;++index)
                    {
                        if (level.SpawnPoints.Where(x => x.PossessingPlayer == index).Count() > (level.SpawnPoints.Count - players.Length) / 2)
                            winTimer[index].Start();
                        else
                            winTimer[index].Stop();
                    }
                }
                // winning
                CheckWinning(gameTime);
            }
        }

        private void CheckWinning(GameTime gameTime)
        {
            int winPlayerIndex = -1;
            Player.Teams winningTeam = Player.Teams.NONE;
            switch (Settings.Instance.GameMode)
            {
                // only one player alive
                case GameMode.FUN:
                case GameMode.CLASSIC:
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
                    break;

                case GameMode.LEFT_VS_RIGHT:
                    bool leftAlive = players.Any(x => x.Alive && x.Team == Player.Teams.LEFT);
                    bool rightAlive = players.Any(x => x.Alive && x.Team == Player.Teams.RIGHT);
                    if (leftAlive && !rightAlive)
                        winningTeam = Player.Teams.LEFT;
                    else if (!leftAlive && rightAlive)
                        winningTeam = Player.Teams.RIGHT;
                    break;

                case GameMode.CAPTURE_THE_CELL:
                    bool attackerAlive = players.Any(x => x.Alive && x.Team == Player.Teams.ATTACKER);
                    bool defenderAlive = players.Any(x => x.Alive && x.Team == Player.Teams.DEFENDER);
                    if (attackerAlive && !defenderAlive)
                        winningTeam = Player.Teams.ATTACKER;
                    else if (!attackerAlive && defenderAlive)
                        winningTeam = Player.Teams.DEFENDER;
                    break;

                // player died
                case GameMode.ARCADE:
                    winPlayerIndex = players[0].Alive ? -1 : 0;
                    break;

                case GameMode.INSERT_MODE_NAME:
                    for(int index=0;index<winTimer.Length;++index)
                        if (winTimer[index].Elapsed.TotalSeconds > ModeWinTime)
                        {
                            winPlayerIndex = index;
                            break;
                        }
                    if(winPlayerIndex != -1)
                        for (int index = 0; index < winTimer.Length; ++index)
                            if (winTimer[index].Elapsed.TotalSeconds > ModeWinTime)
                            {
                                winTimer[index].Stop();
                            }
                    break;
                default:
                    throw new NotImplementedException("Unknown GameType - can't evaluate win condition");
            }

            if (winPlayerIndex != -1 || winningTeam != Player.Teams.NONE)
            {
                // statistics
                level.GameStatistics.addWonMatches(winPlayerIndex);

                ((Menu.StatisticsScreen)menu.GetPage(Menu.Menu.Page.STATS)).WinningTeam = winningTeam;
                ((Menu.StatisticsScreen)menu.GetPage(Menu.Menu.Page.STATS)).WinPlayerIndex = winPlayerIndex;
                ((Menu.StatisticsScreen)menu.GetPage(Menu.Menu.Page.STATS)).PlayerTypes = Settings.Instance.GetPlayerSettingSelection(x => x.Type).ToArray();
                ((Menu.StatisticsScreen)menu.GetPage(Menu.Menu.Page.STATS)).PlayerColorIndices = Settings.Instance.GetPlayerSettingSelection(x => x.ColorIndex).ToArray();

                menu.ChangePage(Menu.Menu.Page.STATS, gameTime);
            }

#if STATS_TEST
            // statistics
            level.GameStatistics.FillWithTestdata(1000);

            ((Menu.StatisticsScreen)menu.GetPage(Menu.Menu.Page.STATS)).WinPlayerIndex = 0;
            ((Menu.StatisticsScreen)menu.GetPage(Menu.Menu.Page.STATS)).PlayerTypes = Settings.Instance.GetPlayerSettingSelection(x => x.Type).ToArray();
            ((Menu.StatisticsScreen)menu.GetPage(Menu.Menu.Page.STATS)).PlayerColorIndices = Settings.Instance.GetPlayerSettingSelection(x => x.ColorIndex).ToArray();

            menu.ChangePage(Menu.Menu.Page.STATS, gameTime);
#endif
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
            float totalGameTime = (float)gameTime.TotalGameTime.TotalSeconds;
            float timeSinceLastFrame = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // draw level
            level.Draw(gameTime, spriteBatch.GraphicsDevice, players);

            
               
            if (State == GameState.Playing || State == GameState.Paused)
            {
                // apply postprocessing
                postPro.Draw(graphicsDevice);

                // ingame interface
                if (Settings.Instance.GameMode == GameMode.INSERT_MODE_NAME)
                    inGameInterface.DrawInterface(players, spriteBatch, level.FieldPixelSize, level.FieldPixelOffset, gameTime, winTimer);
                else
                    inGameInterface.DrawInterface(players, spriteBatch, level.FieldPixelSize, level.FieldPixelOffset, gameTime);

                // debug draw damagemap
#if DAMAGEMAP_DEBUGGING
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque);
                spriteBatch.Draw(damageMap.DamageTexture, new Vector2(Settings.Instance.ResolutionX - DamageMap.attackingMapSizeX, 0), Color.White);
                spriteBatch.End();  
#endif
            }

            // reading player gpu results
            for (int i = 0; i < players.Length; ++i)
                players[i].ReadGPUResults();
        }

        public void Resize(GraphicsDevice graphicsDevice)
        {
            if (level != null)
                level.Resize(graphicsDevice);
            if (postPro != null)
                postPro.Resize(level.FieldPixelSize.ToVector2(), level.FieldPixelOffset.ToVector2());
        }
    }
}
