﻿//#define NO_ITEMS

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace VirusX
{
    class Level
    {
        // statistics
        public Statistics GameStatistics { get; set; }
        private bool skipFirstStatisticStep = true;

        // map objects
        private List<MapObject> mapObjects = new List<MapObject>();
        public List<MapObject> MapObjects { get { return mapObjects; } }
        public IEnumerable<MapObject> Items { get { return mapObjects.Where(x => x is Item); } }
        public IEnumerable<SpawnPoint> SpawnPoints { get { return mapObjects.OfType<SpawnPoint>(); } }

        /// <summary>
        /// active map type
        /// </summary>
        private MapGenerator.MapType currentMapType = MapGenerator.MapType.BACKGROUND;

        // background
        private Background background;

        // textures
        private Texture2D pixelTexture;
        private Texture2D mutateBig;

        private SpriteBatch spriteBatch;
        private ContentManager contentManager;

        public readonly RasterizerState ScissorTestRasterizerState = new RasterizerState
                                                                {
                                                                    CullMode = CullMode.None,
                                                                    ScissorTestEnable = true,
                                                                };

        public static BlendState ShadowBlend = new BlendState
                                                   {
                                                       ColorSourceBlend = Blend.One,
                                                       ColorDestinationBlend = Blend.InverseSourceAlpha,
                                                       ColorBlendFunction = BlendFunction.Add,
                                                       AlphaSourceBlend = Blend.Zero,
                                                       AlphaDestinationBlend = Blend.One
                                                   };

        /// <summary>
        /// on/off for itemplacement
        /// </summary>
        private bool spawnItems;

        #region field dimension & cordinates

        /// <summary>
        /// all relative coordinates are from 0 to RELATIVE_MAX
        /// </summary>
        static public readonly Vector2 RELATIVE_MAX = new Vector2(2.0f, 1.0f);

        static public readonly float RELATIVECOR_ASPECT_RATIO = (float)RELATIVE_MAX.X / RELATIVE_MAX.Y;

        /// <summary>
        /// size of the playing area in pixel
        /// </summary>
        public Point FieldPixelSize
        {
            get { return fieldSize_pixel; }
        }
        private Point fieldSize_pixel;


        /// <summary>
        /// offset in x and y of the playing area in pixel
        /// </summary>
        public Point FieldPixelOffset
        {
            get { return fieldOffset_pixel; }
        }
        private Point fieldOffset_pixel;

        /// <summary>
        /// the field area in pixels as rectangle
        /// </summary>
        private Rectangle fieldPixelRectangle;

        #endregion

        #region switch

        private bool switchCountdownActive = false;
        private float switchCountdownTimer;
        public const float SWITCH_COUNTDOWN_LENGTH = 6.0f;
        public const float SWITCH_COUNTDOWN_LENGTH_FUN = 1.0f;
        private SpriteFont fontCountdownLarge;

        #endregion

        #region arcade mode

        // Speed for other game modes
        public const float ANTIBODY_SPEED_FUN = 0.048f;
        public const float ANTIBODY_SPEED_CLASSIC = 0.024f;

        // the time over which the distribution of cells and antibodies will changes.
        // if this time is over, the distribution reaches its min for cells and max for antibodies.
        public const float ARCADE_CHANGE_TIME = 60 * 5f;
        public const float MAX_ANTIBODY_SPEED = 0.03f;
        public const float MIN_ANTIBODY_SPEED = 0.0f;
        public const float ANTIBODY_SPEED_INCREASE = 0.001f;
        // Time until the anti bodies speed will be increased in seconds
        public const float MAX_TIME_UNTIL_ANTIBODY_SPEED_INCREASE = 5f;
        private float currentAntiBodySpeed = MIN_ANTIBODY_SPEED;
        private float timeUntilAntiBodySpeedIncreas = MAX_TIME_UNTIL_ANTIBODY_SPEED_INCREASE;


        #endregion

        #region item possibilities

        /// <summary>
        /// if this is set to true, all player items will be removed in the next level update step
        /// the same goes for all items in the level.
        /// </summary>
        private bool clearAllItems = false;

        /// <summary>
        /// item possebilities [0] = no item; [5] = no item; [1] = antibody; [2] = dangerZone; [3] = mutate; [4] = wipeout
        /// every value is [i-1] + possebility
        /// </summary>
        private static readonly float[] itemPossibilities = new float[] { 0.0f, 0.17f, 0.45f, 0.60f, 0.73f, 1.0f };
        /// <summary>
        /// item possebilities for CTC mode [0] = no item; [5] = no item; [1] = antibody; [2] = dangerZone; [3] = mutate; [4] = wipeout
        /// every value is [i-1] + possebility
        /// </summary>
        private static readonly float[] itemPossibilitiesCTC = new float[] { 0.0f, 0.25f, 0.56f, 0.56f, 0.73f, 1.0f };
        /// <summary>
        /// determines how many seconds should pass until the next item placement attemp will be started
        /// </summary>
        private static readonly float itemSpawnTime = 2.8f;
        /// <summary>
        /// a probarbility which determines how often the weakest player will get items near is territory center and 
        /// the strongest player gets antibodies. This is used to give weaker players a chance to come back into the game
        /// </summary>
        private static readonly float itemWinLoseRubberBand = 0.27f;
        /// <summary>
        /// controls how much steps (from Statistics) should pass until the rubber band takes effect
        /// </summary>
        private static readonly int itemWinLoseRubberBandMinStepsPlayed = 50;

        #endregion
        
        private Stopwatch pickuptimer;

        /// <summary>
        /// target to that all viruses are added
        /// </summary>
        private RenderTarget2D particleTexture;

        /// <summary>
        /// gamemode changes various properties
        /// </summary>
        private InGame.GameMode gameMode;

        private BlendState ScreenBlend = new BlendState()
                                             {
                                                 ColorBlendFunction = BlendFunction.Add,
                                                 ColorSourceBlend = Blend.InverseDestinationColor,
                                                 ColorDestinationBlend = Blend.One,
                                                 AlphaBlendFunction = BlendFunction.Add,
                                                 AlphaSourceBlend = Blend.InverseDestinationAlpha,
                                                 AlphaDestinationBlend = Blend.One
                                             };

        public Level(GraphicsDevice device, ContentManager content)
        {
            this.contentManager = content;

            pickuptimer = new Stopwatch();
            pickuptimer.Start();

            pixelTexture = content.Load<Texture2D>("pix");

            spriteBatch = new SpriteBatch(device);

            fontCountdownLarge = content.Load<SpriteFont>("fonts/fontCountdown");

            // background & vignetting
            background = new Background(device, content);

            // effects
            mutateBig = content.Load<Texture2D>("Mutate_big");
    
            // setup size
            Resize(device);
        }

        public void NewGame(MapGenerator.MapType mapType, InGame.GameMode gameMode, bool spawnItems, GraphicsDevice device, Player[] players)
        {
            this.spawnItems = spawnItems;
            this.gameMode = gameMode;
            switchCountdownActive = false;
            currentMapType = mapType;

            // recompute dimensions (offset can be variable!) without influencing the coming-soon-updated background
            Resize(device, false);

            // create level
            mapObjects.Clear();
            mapObjects.AddRange(MapGenerator.GenerateLevel(mapType, device, contentManager, players.Length, background,
                                    fieldSize_pixel, fieldOffset_pixel));

            // clear player rendering
            BeginDrawInternParticleTarget(device);
            EndDrawInternParticleTarget(device);

            // init statistics
            GameStatistics = new Statistics(Settings.Instance.NumPlayers, 2400, (uint)SpawnPoints.Count());
            // collect the virus types for statistic
            foreach (Player player in players)
                GameStatistics.SetVirusType(player.playerIndex, player.Virus);

            // arcarde mode
            switch (gameMode)
            {
                case InGame.GameMode.FUN:
                    currentAntiBodySpeed = ANTIBODY_SPEED_FUN;
                    break;
                case InGame.GameMode.ARCADE:
                    currentAntiBodySpeed = MIN_ANTIBODY_SPEED;
                    break;
                default:
                    currentAntiBodySpeed = ANTIBODY_SPEED_CLASSIC;
                    break;
            }
            timeUntilAntiBodySpeedIncreas = MAX_TIME_UNTIL_ANTIBODY_SPEED_INCREASE;
        }

        /// <summary>
        /// computes pixelrect on damage map from relative cordinates
        /// </summary>
        /// <param name="relativePosition">position in relative game cor</param>
        /// <param name="uniformSize">uniform size in relative game cord</param>
        public Rectangle ComputePixelRect(Vector2 position, float uniformSize)
        {
            return ComputePixelRect(position, new Vector2(uniformSize / RELATIVECOR_ASPECT_RATIO, uniformSize));
        }

        public Rectangle ComputePixelRect(Vector2 position, Vector2 size)
        {
            Vector2 outSize = ComputePixelScale(size) + new Vector2(0.5f);
            Vector2 outpos = ComputePixelPosition(position) + new Vector2(0.5f);
            return new Rectangle((int)outpos.X, (int)outpos.Y, (int)outSize.X, (int)outSize.Y);
        }

        public Vector2 ComputePixelPosition(Vector2 position)
        {
            return new Vector2(position.X / RELATIVE_MAX.X * FieldPixelSize.X + FieldPixelOffset.X,
                               position.Y / RELATIVE_MAX.Y * FieldPixelSize.Y + FieldPixelOffset.Y);
        }

        public float ComputePixelScale(float relativeScale)
        {
            return relativeScale * fieldSize_pixel.Y;
        }
        public float ComputeTextureScale(float relativeSize, int textureSize)
        {
            return ComputePixelScale(relativeSize) / textureSize;
        }
        public Vector2 ComputePixelScale(Vector2 relativeScale)
        {
            return new Vector2(relativeScale.X * fieldSize_pixel.X,
                               relativeScale.Y * fieldSize_pixel.Y);
        }

        /// <summary>
        /// computes centered pixelrect on damage map from relative cordinates
        /// </summary>
        /// <param name="relativePosition">position in relative game cor</param>
        /// <param name="uniformSize">uniform size in relative game cord</param>
        public Rectangle ComputePixelRect_Centered(Vector2 position, float uniformSize)
        {
            return ComputePixelRect_Centered(position, new Vector2(uniformSize / RELATIVECOR_ASPECT_RATIO, uniformSize));
        }

        public Rectangle ComputePixelRect_Centered(Vector2 position, Vector2 size)
        {
            int rectSizeX = (int)(size.X * fieldSize_pixel.X + 0.5f);
            int halfSizeX = rectSizeX / 2;
            int rectSizeY = (int)(size.Y * fieldSize_pixel.Y + 0.5f);
            int halfSizeY = rectSizeY / 2;

            int rectx = (int)(position.X / RELATIVE_MAX.X * FieldPixelSize.X + FieldPixelOffset.X);
            int recty = (int)(position.Y / RELATIVE_MAX.Y * FieldPixelSize.Y + FieldPixelOffset.Y);
            
            return new Rectangle(rectx - halfSizeX, recty - halfSizeY, rectSizeX, rectSizeY);
        }

        public void ApplyDamage(DamageMap damageMap, float timeInterval, Player[] playerList)
        {
            int prevPosPlayer = -1;
            foreach (MapObject mapObject in mapObjects)
            {
                // statistics
                if (mapObject is SpawnPoint)
                    prevPosPlayer = (mapObject as SpawnPoint).PossessingPlayer;

                // damage
                mapObject.ApplyDamage(damageMap, timeInterval);

                // statistics
                if (mapObject is SpawnPoint)
                {
                    if (prevPosPlayer != (mapObject as SpawnPoint).PossessingPlayer)
                    {
                        if (prevPosPlayer != -1)
                        {
                            GameStatistics.addLostSpawnPoints(prevPosPlayer);
                            InputManager.Instance.StartRumble(prevPosPlayer, 0.42f, 0.6f);
                        }
                        if ((mapObject as SpawnPoint).PossessingPercentage == 1f && (mapObject as SpawnPoint).PossessingPlayer != -1)
                        {
                            GameStatistics.addCaptueredSpawnPoints((mapObject as SpawnPoint).PossessingPlayer);
                            InputManager.Instance.StartRumble((mapObject as SpawnPoint).PossessingPlayer,0.42f,0.5f);
                        }
                    }
                }
            }
        }

        public void Update(GameTime gameTime, Player[] players)
        {
            // statistics
            uint[] possesingSpawnPoints = new uint[players.Length];
            float[] possesingSpawnPointsOverallSize = new float[players.Length];
            for (int i = 0; i < possesingSpawnPoints.Length; ++i) { possesingSpawnPoints[i] = 0; possesingSpawnPointsOverallSize[i] = 0.0f; }

            // update
            foreach (MapObject mapObject in mapObjects)
            {
                mapObject.Update(gameTime);

                if (mapObject is SpawnPoint)
                {
                    SpawnPoint sp = mapObject as SpawnPoint;
                    // statistics
                    if (sp.PossessingPlayer != -1)
                    {
                        possesingSpawnPoints[sp.PossessingPlayer]++;
                        possesingSpawnPointsOverallSize[sp.PossessingPlayer] += sp.Size;
                    }

                    // remove AntiBodies which are in the explosion range
                    if (gameMode == InGame.GameMode.ARCADE)
                    {
                        if (sp.PossessingPlayer != -1 && sp.ExplosionProgress < 1.0f)
                        {
                            var removeAntiBody = mapObjects.Where(x => x is Debuff);
                            if (removeAntiBody.Count() > 0)
                                removeAntiBody = removeAntiBody.Where(x => System.Math.Abs(Vector2.Distance(x.Position, sp.Position)) < sp.currentExplosionSize / 2);
                            foreach (Debuff antiBody in removeAntiBody)
                            {
                                antiBody.Alive = false;
                            }
                        }
                    }
                }
                // move antibodies to the nearest player if not in arcarde mode
                else if (mapObject is Debuff && players.Length > 0)// && gameMode != InGame.GameMode.ARCADE)
                {
                    var nearestPlayer = players.OrderBy(x => Vector2.DistanceSquared(x.ParticleAttractionPosition, mapObject.Position)).First();
                    Vector2 move = nearestPlayer.ParticleAttractionPosition - mapObject.Position;
                    float distanceToPlayerAttractionPos = move.Length();
                    if (distanceToPlayerAttractionPos > 0.02f)
                    {
                        move /= distanceToPlayerAttractionPos;
                        move *= (float)gameTime.ElapsedGameTime.TotalSeconds * (Player.CURSOR_SPEED * currentAntiBodySpeed);
                        mapObject.Position += move;
                    }
                }
            }

            // remove dead objects
            for (int i = 0; i < mapObjects.Count; ++i)
            {
                if (mapObjects[i].Alive)
                    continue;

                // if its a item, give it to a player if its 100% his
                Item item = mapObjects[i] as Item;
                if (item != null && !item.Timeouted && item.PossessingPlayer != -1 && item.PossessingPercentage == 1.0f)
                {
                    players[item.PossessingPlayer].ItemSlot = item.Type;
                    // statistics
                    GameStatistics.addCollectedItems(item.PossessingPlayer);
                    InputManager.Instance.StartRumble(item.PossessingPlayer, 0.25f, 0.37f);
                }

                // statistics (a debuff has been activated)
                Debuff debuff = mapObjects[i] as Debuff;
                if (debuff != null)
                if(debuff.CapturingPlayer != -1 && debuff.PossessingPercentage >= 1.0f)
                {
                    GameStatistics.ItemUsed(debuff.CapturingPlayer);
                    InputManager.Instance.StartRumble(debuff.CapturingPlayer, 0.25f, 0.22f);
                }

                // remove itself
                mapObjects.RemoveAt(i);
                --i;
            }

            // items
            if (spawnItems)
            {
                if (pickuptimer.Elapsed.TotalSeconds > itemSpawnTime)
                {
                    PlaceItems(players);

                    // restart timer
                    pickuptimer.Reset();
                    pickuptimer.Start();
                }
                
                if (clearAllItems)
                {
                    // remove all Items from the level after a wipeout
                    foreach (var item in mapObjects.OfType<Item>())
                        item.Alive = false;

                    // remove all player items after a wipeout
                    foreach (Player p in players)
                        p.ItemSlot = Item.ItemType.NONE;

                    clearAllItems = false;
                }
            }
            
            // arcade mode stuff
            if (currentMapType == MapGenerator.MapType.ARCADE)
            {
                PlaceArcadeModeElements(players[0]);
                DoArcadeComputations(gameTime);
            }

            // statistics
            if (GameStatistics.UpdateTimer((float)gameTime.ElapsedGameTime.TotalSeconds))
            {
                if (!skipFirstStatisticStep)
                {
                    for (int i = 0; i < players.Length; ++i)
                    {
                        GameStatistics.setParticlesAndHealthAndPossesingSpawnPoints(i, (uint)players[i].NumParticlesAlive, (uint)players[i].TotalVirusHealth, (uint)possesingSpawnPoints[i]);
                    }
                    GameStatistics.UpdateDomination(players);
                }
                else
                    skipFirstStatisticStep = false;
            }
        }

        private void DoArcadeComputations(GameTime gameTime)
        {
            timeUntilAntiBodySpeedIncreas -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timeUntilAntiBodySpeedIncreas < 0f)
            {
                timeUntilAntiBodySpeedIncreas = MAX_TIME_UNTIL_ANTIBODY_SPEED_INCREASE;
                currentAntiBodySpeed += ANTIBODY_SPEED_INCREASE;
                if (currentAntiBodySpeed > MAX_ANTIBODY_SPEED)
                    currentAntiBodySpeed = MAX_ANTIBODY_SPEED;
            }
        }

        /// <summary>
        /// places cells and antibodies for arcade mode
        /// </summary>
        public void PlaceArcadeModeElements(Player player)
        {
            const float ARCADE_MODE_SPAWN_INTERVAL = 0.2f;

            if(pickuptimer.Elapsed.TotalSeconds > ARCADE_MODE_SPAWN_INTERVAL)
            {
                pickuptimer.Restart();

                float time = GameStatistics.LastStep * GameStatistics.StepTime;

                float PROBABILITY_SPAWN = MathHelper.Clamp(ARCADE_CHANGE_TIME - (time / ARCADE_CHANGE_TIME), 0.2f, 0.7f);//0.2f;
                float PROBABILITY_ANTIBODY = MathHelper.Clamp(time / ARCADE_CHANGE_TIME, 0.2f, 0.7f); //0.7f;
                float h = 0.5f;
                float sum = PROBABILITY_SPAWN + PROBABILITY_ANTIBODY + h;
                PROBABILITY_SPAWN /= sum;
                PROBABILITY_ANTIBODY /= sum;

                float rnd = (float)Random.NextDouble();
                if (rnd + (h/sum) < PROBABILITY_SPAWN)
                {
                    Vector2 position = new Vector2((float)(Random.NextDouble()) * (RELATIVE_MAX.X - MapGenerator.LEVEL_BORDER) + MapGenerator.LEVEL_BORDER / 2,
                        (float)(Random.NextDouble()) * (RELATIVE_MAX.Y - MapGenerator.LEVEL_BORDER) + MapGenerator.LEVEL_BORDER / 2);
                    // Random.NextDirection() * (RELATIVE_MAX - new Vector2(MapGenerator.LEVEL_BORDER) * 2)*0.5f + new Vector2(MapGenerator.LEVEL_BORDER);
                    float spawnSize = (float)Random.NextDouble(100, 700);
                    mapObjects.Add(new MovingSpawnPoint(Random.NextDirection(), position, spawnSize, -1, contentManager));
                }
                else if (rnd + (h/sum) < PROBABILITY_ANTIBODY + PROBABILITY_SPAWN)
                {
                    Vector2 position = new Vector2((float)(Random.NextDouble()) * (RELATIVE_MAX.X - MapGenerator.LEVEL_BORDER) + MapGenerator.LEVEL_BORDER / 2,
                        (float)(Random.NextDouble()) * (RELATIVE_MAX.Y - MapGenerator.LEVEL_BORDER) + MapGenerator.LEVEL_BORDER / 2);
                    mapObjects.Add(new Debuff(position, contentManager));
                }
            }
        }

        private void PlaceItems(Player[] players)
        {
            double next_item_pos = Random.NextDouble();
            int item = 0;
            if (gameMode == InGame.GameMode.CAPTURE_THE_CELL)
            {
                for (int i = 1; i < itemPossibilitiesCTC.Length; i++)
                {
                    if (itemPossibilitiesCTC[i - 1] < next_item_pos && next_item_pos < itemPossibilitiesCTC[i])
                    {
                        item = i;
                    }
                }
            }
            else
            {
                for (int i = 1; i < itemPossibilities.Length; i++)
                {
                    if (itemPossibilities[i - 1] < next_item_pos && next_item_pos < itemPossibilities[i])
                    {
                        item = i;
                    }
                }
            }

            Vector2 position;
            if (Random.NextDouble() < itemWinLoseRubberBand && GameStatistics.Steps > itemWinLoseRubberBandMinStepsPlayed)
            {
                int weakestPlayer = 0;
                float minDomination = GameStatistics.getDominationInStep(0, GameStatistics.Steps - 1);
                for (int i = 1; i < GameStatistics.PlayerCount; ++i)
                {
                    if (item != 1)
                        if (minDomination > GameStatistics.getDominationInStep(i, GameStatistics.Steps - 1))
                        {
                            weakestPlayer = i;
                            minDomination = GameStatistics.getDominationInStep(i, GameStatistics.Steps - 1);
                        }
                        else if (minDomination < GameStatistics.getDominationInStep(i, GameStatistics.Steps - 1))
                        {
                            weakestPlayer = i;
                            minDomination = GameStatistics.getDominationInStep(i, GameStatistics.Steps - 1);
                        }
                }

                var spwp = SpawnPoints.Where(x => x.PossessingPlayer == weakestPlayer);
                Vector2 weakestPlayerCenter = players[weakestPlayer].CursorPosition;
                if (spwp.Count() > 1)
                {
                    weakestPlayerCenter.X = spwp.Average(x => x.Position.X);
                    weakestPlayerCenter.Y = spwp.Average(x => x.Position.Y);
                }
                position = weakestPlayerCenter + new Vector2((float)(Random.NextDouble()) * 0.4f - 0.2f, (float)(Random.NextDouble()) * 0.2f - 0.1f);
                position.X = MathHelper.Clamp(position.X, 0.1f, Level.RELATIVE_MAX.X);
                position.Y = MathHelper.Clamp(position.Y, 0.1f, Level.RELATIVE_MAX.Y);
            }
            else
                position = new Vector2((float)(Random.NextDouble()) * (RELATIVE_MAX.X - MapGenerator.LEVEL_BORDER) + MapGenerator.LEVEL_BORDER / 2,
                        (float)(Random.NextDouble()) * (RELATIVE_MAX.Y - MapGenerator.LEVEL_BORDER) + MapGenerator.LEVEL_BORDER / 2);
            
            AddItem(item, position);
        }

        public void AddMapObject(MapObject mapObject)
        {
            mapObjects.Add(mapObject);
            if (mapObject.GetType() == typeof(SpawnPoint))
                mapObjects.Add(mapObject);
        }

        private void AddItem(int i, Vector2 position)
        {
            switch (i)
            {
                case 1:
                    mapObjects.Add(new Debuff(position, contentManager));
                    break;
                case 2:
                    mapObjects.Add(new Item(position, Item.ItemType.DANGER_ZONE, contentManager));
                    break;
                case 3:
                    if(gameMode != InGame.GameMode.CAPTURE_THE_CELL)
                        mapObjects.Add(new Item(position, Item.ItemType.MUTATION, contentManager));
                    break;
                case 4:
                    mapObjects.Add(new Item(position, Item.ItemType.WIPEOUT, contentManager));
                    break;
            }
        }

        public void UpdateSwitching(float frameTimeSeconds, Player[] players)
        {
            switchCountdownTimer -= frameTimeSeconds;
            if (switchCountdownActive && switchCountdownTimer < 0.0f)
            {
                AudioManager.Instance.PlaySoundeffect("switch");

                int[] playerIndices = { 0, 1, 2, 3 };

                // count alive players, create reduced playerlist
                int alivePlayerCount = players.Length;
                foreach (Player player in players)
                    alivePlayerCount -= player.Alive ? 0 : 1;
                Player[] reducedPlayerList = new Player[alivePlayerCount];
                int index = 0;
                foreach (Player player in players)
                {
                    if (player.Alive)
                    {
                        reducedPlayerList[index] = player;
                        index++;
                    }
                }


                if (reducedPlayerList.Length == 2)
                {
                    Player.SwitchPlayer(reducedPlayerList[0], reducedPlayerList[1]);
                    SwapInts(playerIndices, reducedPlayerList[0].Index, reducedPlayerList[1].Index);
                }
                else if (reducedPlayerList.Length == 3)
                {
                    bool rotateLeft = Random.Next(2) == 0;
                    if (rotateLeft)
                    {
                        Player.SwitchPlayer(reducedPlayerList[0], reducedPlayerList[1]);
                        SwapInts(playerIndices, reducedPlayerList[0].Index, reducedPlayerList[1].Index);
                        Player.SwitchPlayer(reducedPlayerList[1], reducedPlayerList[2]);
                        SwapInts(playerIndices, reducedPlayerList[1].Index, reducedPlayerList[2].Index);
                    }
                    else
                    {
                        Player.SwitchPlayer(reducedPlayerList[0], reducedPlayerList[1]);
                        SwapInts(playerIndices, reducedPlayerList[0].Index, reducedPlayerList[1].Index);
                        Player.SwitchPlayer(reducedPlayerList[0], reducedPlayerList[2]);
                        SwapInts(playerIndices, reducedPlayerList[0].Index, reducedPlayerList[2].Index);
                    }
                }
                else if (reducedPlayerList.Length == 4)
                {
                    int first = Random.Next(0, 4);
                    int second = (first + Random.Next(1, 4)) % 4;
                    Player.SwitchPlayer(players[first], players[second]);
                    SwapInts(playerIndices, reducedPlayerList[first].Index, reducedPlayerList[second].Index);

                    int third = -1;
                    int fourth = -1;
                    for (int i = 0; i < 4; ++i)
                    {
                        if (first != i && second != i)
                        {
                            if (third == -1)
                                third = i;
                            else
                                fourth = i;
                        }
                    }
                    Player.SwitchPlayer(players[third], players[fourth]);
                    SwapInts(playerIndices, reducedPlayerList[third].Index, reducedPlayerList[fourth].Index);

                    playerIndices[first] = second;
                    playerIndices[second] = first;
                    playerIndices[third] = fourth;
                    playerIndices[fourth] = third;
                }

                foreach (MapObject mapObject in mapObjects)
                    mapObject.SwitchPlayer(playerIndices);

                switchCountdownActive = false;
            }    
        }

        static void SwapInts(int[] array, int position1, int position2)
        {
            int temp = array[position1];
            array[position1] = array[position2];
            array[position2] = temp;
        }

        public void BeginDrawInternParticleTarget(GraphicsDevice device)
        {
            // particles to offscreen target
            device.SetRenderTarget(particleTexture);
            device.Clear(ClearOptions.Target, new Color(0, 0, 0, 0), 0, 0);
            spriteBatch.GraphicsDevice.BlendState = ScreenBlend;
        }

        public void EndDrawInternParticleTarget(GraphicsDevice device)
        {
            device.SetRenderTarget(null);
        }

        public void SetupScissorRect(GraphicsDevice device)
        {
            device.ScissorRectangle = fieldPixelRectangle;
            device.RasterizerState = ScissorTestRasterizerState;
        }

        public void Draw(GameTime gameTime, GraphicsDevice device, Player[] players)
        {
            // background colors
            var colors = SpawnPoints.Where(x => x.GetType() == typeof(SpawnPoint)).Select(x =>
            {
                Color color = x.ComputeColor();
                float saturation = Vector3.Dot(color.ToVector3(), new Vector3(0.3f, 0.59f, 0.11f));
                return Color.Lerp(color, new Color(saturation, saturation, saturation), 0.8f) * 1.5f;
            });
            colors = colors.Concat(Enumerable.Repeat(Color.DimGray, background.NumBackgroundCells - colors.Count()));
            background.UpdateColors(colors.ToArray());

            // activate scissor test - is this a performance issue?
            if (currentMapType != MapGenerator.MapType.BACKGROUND)
                SetupScissorRect(device);

            // background
            background.Draw(device, (float)gameTime.TotalGameTime.TotalSeconds);

            // the particles!
            DrawParticles(device);

            // additive spritebatch stuff
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, ScissorTestRasterizerState);
            foreach (MapObject mapObject in mapObjects.Where(x => x.Alive))
                mapObject.Draw_Additive(spriteBatch, this, gameTime);
            spriteBatch.End();

            // alphablended spritebatch stuff
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, ScissorTestRasterizerState);
            foreach (MapObject mapObject in mapObjects.Where(x=>x.Alive))
                mapObject.Draw_AlphaBlended(spriteBatch, this, gameTime);
            spriteBatch.End();

            // countdown
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, ScissorTestRasterizerState);
            DrawCountdown(device, (float)gameTime.TotalGameTime.TotalSeconds);
            spriteBatch.End();
        }

        private void DrawParticles(GraphicsDevice device)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, ShadowBlend, SamplerState.LinearClamp,
                              DepthStencilState.None, RasterizerState.CullNone);
            spriteBatch.Draw(particleTexture, new Rectangle(FieldPixelOffset.X, FieldPixelOffset.Y, particleTexture.Width, particleTexture.Height),
                                    Color.White);
            spriteBatch.End(); 
        }

        public void DrawCountdown(GraphicsDevice device, float totalPassedTime)
        {
            if (switchCountdownActive)
            {
                Rectangle mutateBigRect = ComputePixelRect(RELATIVE_MAX * 0.5f, RELATIVE_MAX.Y * 0.7f);
                spriteBatch.Draw(mutateBig, mutateBigRect, null, Color.LightSlateGray * 0.6f,
                                    -totalPassedTime, new Vector2(mutateBig.Width, mutateBig.Height) / 2, SpriteEffects.None, 1.0f);
                string text = ((int) (switchCountdownTimer + 1)).ToString();

                spriteBatch.DrawString(fontCountdownLarge, text,
                                       new Vector2(mutateBigRect.X - fontCountdownLarge.MeasureString(text).X / 2,
                                                     mutateBigRect.Y - fontCountdownLarge.MeasureString(text).Y / 2),
                                    Color.LightGray, 0.0f, Vector2.Zero, 1, SpriteEffects.None, 0.0f);
            }
        }

        public void Resize(GraphicsDevice device, bool resizeBackground = true)
        {
            // letterboxing
            float sizeY = Settings.Instance.ResolutionX / RELATIVECOR_ASPECT_RATIO;
            if (sizeY + PercentageBar.HEIGHT > Settings.Instance.ResolutionY)
                sizeY = Settings.Instance.ResolutionY - PercentageBar.HEIGHT;
            fieldSize_pixel = new Point((int)(sizeY * RELATIVECOR_ASPECT_RATIO), (int)sizeY);

            fieldOffset_pixel = new Point(Settings.Instance.ResolutionX - fieldSize_pixel.X, Settings.Instance.ResolutionY - fieldSize_pixel.Y + 
                                                                                (currentMapType != MapGenerator.MapType.BACKGROUND ? PercentageBar.HEIGHT : 0));
            fieldOffset_pixel.X /= 2;
            fieldOffset_pixel.Y /= 2;

            fieldPixelRectangle = new Rectangle(fieldOffset_pixel.X, fieldOffset_pixel.Y, fieldSize_pixel.X, fieldSize_pixel.Y);

            if (resizeBackground)
            {
                // setup background
                CreateParticleTarget(device);

                // bg particles
                if (currentMapType == MapGenerator.MapType.BACKGROUND)
                    NewGame(MapGenerator.MapType.BACKGROUND, gameMode, spawnItems, device, new Player[0]); //background.Resize(device, new Rectangle(0, 0, Settings.Instance.ResolutionX, Settings.Instance.ResolutionY));
                else
                    background.Resize(device, fieldPixelRectangle, Level.RELATIVE_MAX);
            }
        }

        private void CreateParticleTarget(GraphicsDevice device)
        {
            if (particleTexture != null)
            {
                if (particleTexture.Width == FieldPixelSize.X && particleTexture.Height == FieldPixelSize.Y)
                    return;
                particleTexture.Dispose();
            }

            particleTexture = new RenderTarget2D(device, (int)(FieldPixelSize.X), (int)(FieldPixelSize.Y),
                                                    false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PlatformContents);
        }

        public void DrawToDamageMap(SpriteBatch damageSpriteBatch)
        {
            foreach (MapObject obj in mapObjects)
                obj.DrawToDamageMap(damageSpriteBatch);
        }

        public void PlayerUseItem(Player player)
        {
            switch (player.ItemSlot)
            {
                case Item.ItemType.DANGER_ZONE:
                    mapObjects.Add(DamageArea.CreateDangerZone(contentManager, player.CursorPosition, player.Index));
                    break;

                case Item.ItemType.MUTATION:
                    if (gameMode == InGame.GameMode.FUN)
                        switchCountdownTimer = SWITCH_COUNTDOWN_LENGTH_FUN;
                    else
                        switchCountdownTimer = SWITCH_COUNTDOWN_LENGTH;
                    switchCountdownActive = true;
                    break;

                case Item.ItemType.WIPEOUT:
                    mapObjects.Add(DamageArea.CreateWipeout(contentManager));
                    clearAllItems = true;
                    break;
            }
            // statistic
            GameStatistics.addUsedItems(player.Index);
            GameStatistics.ItemUsed(player.Index, player.ItemSlot);
        }
    }
}
