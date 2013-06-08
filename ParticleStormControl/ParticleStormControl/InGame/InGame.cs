//#define DAMAGEMAP_DEBUGGING
//#define STATS_TEST
#if !DEBUG
#undef STATS_TEST
#undef DAMAGEMAP_DEBUGGING
#endif

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CustomExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


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
            DOMINATION,

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
            "Domination",
            "Arcade"
        };

        static public readonly String[] GAMEMODE_DESCRIPTION = new String[]
        {
            "standard deathmatch (2-4 players)",
            "one player with a big cell against\na team of three players (4 players)",
            "two teams with one or two\nplayers each (2-4 players)",
            "like classic mode but with\nsome extra twists (2-4 players)",
            "control a number of bases\nfor a certain time (2-4 players)",
            "survive as long as you can (1 player)"
        };

        #endregion

        /// <summary>
        /// Statistics
        /// </summary>
        public Statistics GameStatistics { get { return level.GameStatistics; } }

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

        // damaging is only every second frame - switch every frame to be xbox friendly
        bool levelDamageFrame = false;

        public Level Level
        {
            get { return level; }
        }

        public enum GameState
        {
            Demo,
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
            if (newPage == VirusX.Menu.Menu.Page.INGAME && State == InGame.GameState.Inactive)
                StartNewGame();
            else if (newPage == Menu.Menu.Page.PAUSED || (newPage == Menu.Menu.Page.CONTROLS && oldPage == Menu.Menu.Page.INGAME))
            {
                postPro.ActivatePauseBlur();
                State = InGame.GameState.Paused;
            }
            else if (newPage == Menu.Menu.Page.INGAME)
                State = InGame.GameState.Playing;
            else if (newPage != Menu.Menu.Page.NEWGAME && newPage != Menu.Menu.Page.STATS &&    // no demo in stats and newgame
                     (oldPage != Menu.Menu.Page.NEWGAME || newPage != Menu.Menu.Page.CONTROLS)) // switching from newgame to controls should not start demo!
                StartDemo(false);
            else
                SetupBackground();

            if(State != GameState.Paused)
                postPro.DeactivatePauseBlur();
        }

        #region "start new game" methods

        private void PlayerSetupFromSettingsSingleton()
        {
            // player setup
            players = new Player[Settings.Instance.NumPlayers];
            for (int i = 0; i < Settings.Instance.NumPlayers; ++i)
            {
                if (Settings.Instance.GetPlayer(i).Type != Player.Type.NONE)
                {
                    if (Settings.Instance.GetPlayer(i).Type == Player.Type.AI)
                    {
                        players[i] = new AIPlayer(i, Settings.Instance.GetPlayer(i).Virus, Settings.Instance.GetPlayer(i).ColorIndex,
                            Settings.Instance.GetPlayer(i).Team, Settings.Instance.GameMode, graphicsDevice, content, noiseWhite2D);
                    }
                    else
                    {
                        players[i] = new HumanPlayer(i, Settings.Instance.GetPlayer(i).Virus, Settings.Instance.GetPlayer(i).ColorIndex,
                            Settings.Instance.GetPlayer(i).Team, Settings.Instance.GameMode, graphicsDevice, content, noiseWhite2D, Settings.Instance.GetPlayer(i).ControlType);
                    }
                }
            }
        }

        /// <summary>
        /// starts a new game
        /// </summary>
        private void StartNewGame()
        {
            State = GameState.Playing;
            PlayerSetupFromSettingsSingleton();

            // new map
            MapGenerator.MapType mapType;
            switch(Settings.Instance.GameMode)
            {
                case GameMode.CAPTURE_THE_CELL:
                    mapType = MapGenerator.MapType.CAPTURE_THE_CELL;
                    break;
                case GameMode.FUN:
                    mapType = MapGenerator.MapType.FUN;
                    break;
                case GameMode.ARCADE:
                    mapType = MapGenerator.MapType.ARCADE;
                    break;
                default:
                    mapType = MapGenerator.MapType.NORMAL;
                    break;

            }
            level.NewGame(mapType, Settings.Instance.GameMode, Settings.Instance.UseItems, graphicsDevice, players);
            postPro.UpdateVignettingSettings(true, level.FieldPixelSize.ToVector2(), level.FieldPixelOffset.ToVector2());

            // for Game Mode INSERT_MODE_NAME
            //if (Settings.Instance.GameMode == GameMode.INSERT_MODE_NAME)
            //{
                winTimer = new Stopwatch[players.Length];
                for (int index = 0; index < players.Length; ++index)
                    winTimer[index] = new Stopwatch();
            //}


            // run gc now!
            //System.GC.Collect();
        }

        private void StartDemo(bool forceRestart)
        {
            if (State == GameState.Demo && !forceRestart)
                return;

            // workaround for certain map object properties
            GameMode oldGameMode = Settings.Instance.GameMode;
            Settings.Instance.GameMode = GameMode.CLASSIC;
            // -----

            Settings.Instance.ResetPlayerSettings(); 
            for (int i = 0; i < 2; ++i)
            {
                Settings.Instance.AddPlayer(new Settings.PlayerSettings()
                {
                    ColorIndex = i,
                    ControlType = InputManager.ControlType.NONE,
                    SlotIndex = i,
                    Team = Player.Teams.NONE,
                    Type = Player.Type.AI,
                    Virus = (VirusSwarm.VirusType)Random.Next((int)VirusSwarm.VirusType.NUM_VIRUSES)
                });
            }
              // player setup
            players = new Player[Settings.Instance.NumPlayers];
            for (int i = 0; i < players.Length; ++i)
            {
                players[i] = new AIPlayer(i, Settings.Instance.GetPlayer(i).Virus, Settings.Instance.GetPlayer(i).ColorIndex,
                            Settings.Instance.GetPlayer(i).Team, GameMode.CLASSIC, graphicsDevice, content, noiseWhite2D);
            }

            damageMap.Clear(graphicsDevice);
            level.NewGame(MapGenerator.MapType.BACKGROUND, GameMode.CLASSIC, false, graphicsDevice, players);
            postPro.UpdateVignettingSettings(false, level.FieldPixelSize.ToVector2(), level.FieldPixelOffset.ToVector2());
            
            State = InGame.GameState.Demo;

            // workaround for certain map object properties
            Settings.Instance.GameMode = oldGameMode;
            // -----
        }

        private void SetupBackground()
        {
            if (State == InGame.GameState.Inactive)
                return;

            players = new Player[0];
            level.NewGame(MapGenerator.MapType.BACKGROUND, GameMode.CLASSIC, false, graphicsDevice, players);
            postPro.UpdateVignettingSettings(false, level.FieldPixelSize.ToVector2(), level.FieldPixelOffset.ToVector2());
            State = InGame.GameState.Inactive;
        }

        #endregion


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        public void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            this.graphicsDevice = graphicsDevice;
            this.content = content;

            particleRenderer = new ParticleRenderer(graphicsDevice, content);
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

            // playing
            if (State == GameState.Playing || State == InGame.GameState.Demo)
            {
                // do controlling
                foreach (Player player in players)
                    player.UserControl(passedFrameTime, level);

                // switching - wrong placed this leads player errors
                level.UpdateSwitching(passedFrameTime, players);

                // spawning & move - parallel!
                bool[] playerRecentlyDied = new bool[players.Length];
                Array.Clear(playerRecentlyDied, 0, playerRecentlyDied.Length);
                Task[] playerTaskList = new Task[players.Length];
                var func = new Action<object>((index) =>
                {
                    try
                    {
                        bool alive = players[(int)index].Alive;
                        players[(int)index].UpdateCPUPart(gameTime, level.SpawnPoints);
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

                // damaging - switch every frame to be fast and xbox friendly (preserve content stuff)
                // see also correlating gpu-functions
                if (levelDamageFrame)
                {
                    damageMap.UpdateCPUData();
                    level.ApplyDamage(damageMap, passedFrameTime, players);
                }

                // level update
                level.Update(gameTime, players);

                // winning
                if (State == GameState.Playing)
                    CheckWinning(gameTime);
                else // restart demo
                {
                    if(players.Count(x=>x.Alive) == 1)
                        StartDemo(true);
                }
            }

            postPro.Update(gameTime, level);
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

                case GameMode.DOMINATION:
                    for (int index = 0; index < winTimer.Length; ++index)
                    {
                        if (level.SpawnPoints.Where(x => x.PossessingPlayer == index).Count() > (level.SpawnPoints.Count - players.Length) / players.Length)
                            winTimer[index].Start();
                        else
                            winTimer[index].Stop();
                    }

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
                GameStatistics.addWonMatches(winPlayerIndex);

                // fill stat screen
                Menu.StatisticsScreen statScreen = ((Menu.StatisticsScreen)menu.GetPage(Menu.Menu.Page.STATS));
                statScreen.WinningTeam          = winningTeam;
                statScreen.WinPlayerIndex       = winPlayerIndex;
                statScreen.PlayerTypes          = Settings.Instance.GetPlayerSettingSelection(x => x.Type).ToArray();
                statScreen.PlayerColorIndices   = Settings.Instance.GetPlayerSettingSelection(x => x.ColorIndex).ToArray();
                statScreen.Statistics           = GameStatistics;

                menu.ChangePage(Menu.Menu.Page.STATS, gameTime);
            }

#if STATS_TEST
            // statistics
            GameStatistics.FillWithTestdata(1000);

            ((Menu.StatisticsScreen)menu.GetPage(Menu.Menu.Page.STATS)).WinPlayerIndex = 0;
            ((Menu.StatisticsScreen)menu.GetPage(Menu.Menu.Page.STATS)).PlayerTypes = Settings.Instance.GetPlayerSettingSelection(x => x.Type).ToArray();
            ((Menu.StatisticsScreen)menu.GetPage(Menu.Menu.Page.STATS)).PlayerColorIndices = Settings.Instance.GetPlayerSettingSelection(x => x.ColorIndex).ToArray();
            statScreen.Statistics = GameStatistics;

            menu.ChangePage(Menu.Menu.Page.STATS, gameTime);
#endif
        }

        public void Draw_OffsiteBuffers(GameTime gameTime, GraphicsDevice graphicsDevice)
        {
            if (State == GameState.Playing || State == GameState.Demo)
            {
                // update damagemap (GPU)
                if (levelDamageFrame)
                    damageMap.UpdateGPU_Objects(graphicsDevice, level, players);
                else
                    damageMap.UpdateGPU_Particles(graphicsDevice, particleRenderer, players);
                levelDamageFrame = !levelDamageFrame;
                
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
        public void Draw_Backbuffer(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // postpro start
            postPro.Begin(graphicsDevice);

            // draw level
            level.Draw(gameTime, spriteBatch.GraphicsDevice, players);

            // postpro end
            postPro.EndAndApply(graphicsDevice);

            // crosshairs
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, Level.ScissorTestRasterizerState);
            level.SetupScissorRect(graphicsDevice);
            foreach (Player player in players)
                player.DrawCrosshairAlphaBlended(spriteBatch, level, gameTime);
            spriteBatch.End();

            // interface
            if (State == GameState.Playing || State == GameState.Paused)
            {
                // ingame interface
                if (Settings.Instance.GameMode == GameMode.DOMINATION)
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

            if (State == GameState.Demo)
                StartDemo(true);
        }
    }
}
