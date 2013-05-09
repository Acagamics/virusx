//#define EMPTY_LEVELDEBUG
//#define NO_ITEMS

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ParticleStormControl
{
    public class Level
    {
        // statistics
        public Statistics GameStatistics { get; set; }
        private bool dontSaveTheFirstStepBecauseThatLeadsToSomeUglyStatisticsBug = true;

        private List<MapObject> mapObjects = new List<MapObject>();
        private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
        public List<SpawnPoint> SpawnPoints { get { return spawnPoints; } }

        private Background background;

        private Texture2D pixelTexture;
        private Texture2D mutateBig;

        private SpriteBatch spriteBatch;
        private ContentManager contentManager;

        private RasterizerState scissorTestRasterizerState = new RasterizerState
                                                                {
                                                                    CullMode = CullMode.None,
                                                                    ScissorTestEnable = true,
                                                                };



        private VertexBuffer vignettingQuadVertexBuffer;
        private Effect vignettingShader;
        public static BlendState VignettingBlend = new BlendState
                                                {
                                                    ColorSourceBlend = Blend.Zero,
                                                    ColorDestinationBlend = Blend.SourceAlpha,
                                                    ColorBlendFunction = BlendFunction.Add,
                                                    AlphaSourceBlend = Blend.Zero,
                                                    AlphaDestinationBlend = Blend.One
                                                };

        public static BlendState ShadowBlend = new BlendState
                                                   {
                                                       ColorSourceBlend = Blend.One,
                                                       ColorDestinationBlend = Blend.InverseSourceAlpha,
                                                       ColorBlendFunction = BlendFunction.Add,
                                                       AlphaSourceBlend = Blend.Zero,
                                                       AlphaDestinationBlend = Blend.One
                                                   };

        #region field dimension & cordinates

        /// <summary>
        /// all relative coordinates are from 0 to RELATIVE_MAX
        /// </summary>
        static public readonly Vector2 RELATIVE_MAX = new Vector2(2, 1);

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

        private SoundEffect switchSound;
        private bool switchCountdownActive = false;
        private float switchCountdownTimer;
        public const float SWITCH_COUNTDOWN_LENGTH = 6.0f;
        private SpriteFont fontCountdownLarge;

        #endregion

        #region item possibilities

        /// <summary>
        /// item possebilities [0] = no item; [5] = no item; [1] = antibody; [2] = dangerZone; [3] = mutate; [4] = wipeout
        /// every value is [i-1] + possebility
        /// </summary>
        private static readonly float[] itemPossibilities = new float[] { 0.0f, 0.17f, 0.45f, 0.60f, 0.73f, 1.0f };
        /// <summary>
        /// determines how many seconds should pass until the next item placement attemp will be started
        /// </summary>
        private static readonly float itemSpawnTime = 2.8f;
        /// <summary>
        /// a probarbility which determines how often the weakest player will get items near is territory center and 
        /// the strongest player gets antibodies. This is used to give weaker players a chance to come back into the game
        /// </summary>
        private static readonly float itemWinLoseRubberBand = 0.33f;
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

            // switch
            switchSound = content.Load<SoundEffect>("sound/switch");
            fontCountdownLarge = content.Load<SpriteFont>("fonts/fontCountdown");

            // background & vignetting
            background = new Background(device, content);
            vignettingQuadVertexBuffer = new VertexBuffer(device, ScreenTriangleRenderer.ScreenAlignedTriangleVertex.VertexDeclaration, 4, BufferUsage.WriteOnly);
            vignettingQuadVertexBuffer.SetData(new Vector2[4] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) });
            vignettingShader = content.Load<Effect>("shader/vignetting");

            // effects
            mutateBig = content.Load<Texture2D>("Mutate_big");
    
            // setup size
            Resize(device);
        }

        public void NewGame(GraphicsDevice device, Player[] players)
        {
            mapObjects.Clear();
            spawnPoints.Clear();

            switchCountdownActive = false;
            // create level
            CreateLevel(device, contentManager, players.Length);

            // crosshairs for players
            for (int i = 0; i < players.Length; ++i)
                mapObjects.Add(new Crosshair(i, contentManager));
        }

        static public List<Vector2> GenerateCellPositions(int numX, int numY, float positionJitter, Vector2 border, Vector2 valueRange)
        {
            List<Vector2> spawnPositions = new List<Vector2>();
            for (int x = 0; x < numX; ++x)
            {
                for (int y = 0; y < numY; ++y)
                {
                    // "natural skip"
                    if ((x == numX - 1 && y % 2 == 0) ||
                        ((x == 0 || x == numX - 2 + y % 2) && (y == 0 || y == numY - 1)))
                        continue;

                    // position
                    Vector2 pos = new Vector2((float)x / (numX - 1), (float)y / (numY - 1));
                    if (y % 2 == 0)
                        pos.X += 0.5f / (numX - 1);
                    pos *= valueRange - border * 2.0f;
                    pos += border;

                    // position jitter
                    float posJitter = (float)(Random.NextDouble() * 2.0 - 1.0) * positionJitter;
                    pos += Random.NextDirection() * posJitter;

                    spawnPositions.Add(pos);
                }
            }
            return spawnPositions;
        }

        private void CreateLevel(GraphicsDevice device, ContentManager content, int numPlayers)
        {
            // player starts
            List<Vector2> cellPositions = new List<Vector2>();
            const float LEVEL_BORDER = 0.2f;
            const float START_POINT_GLOW_SIZE = 0.35f;
            cellPositions.Add(new Vector2(LEVEL_BORDER, RELATIVE_MAX.Y - LEVEL_BORDER));
            cellPositions.Add(new Vector2(RELATIVE_MAX.X - LEVEL_BORDER, LEVEL_BORDER));
            cellPositions.Add(new Vector2(RELATIVE_MAX.X - LEVEL_BORDER, RELATIVE_MAX.Y - LEVEL_BORDER));
            cellPositions.Add(new Vector2(LEVEL_BORDER, LEVEL_BORDER));

            for (int playerIndex = 0; playerIndex < Settings.Instance.NumPlayers; ++playerIndex)
            {
                if (Settings.Instance.GetPlayer(playerIndex).Type != Player.Type.NONE)
                {
                    int slot = Settings.Instance.GetPlayer(playerIndex).SlotIndex;
                    spawnPoints.Add(new SpawnPoint(cellPositions[slot], 1000.0f, START_POINT_GLOW_SIZE, playerIndex, content));
                }
            }

            // generate in a grid of equilateral triangles
            const int SPAWNS_GRID_X = 6;
            const int SPAWNS_GRID_Y = 3;
            List<Vector2> spawnPositions = GenerateCellPositions(SPAWNS_GRID_X, SPAWNS_GRID_Y, 0.12f, new Vector2(LEVEL_BORDER), RELATIVE_MAX);


            // random skipping - nonlinear randomness!
            const int MAX_SKIPS = 5;
            int numSkips = (int)(Math.Pow(Random.NextDouble(), 4) * MAX_SKIPS + 0.5f);
            Vector2[] removedCells = new Vector2[numSkips];
            for (int i = 0; i < numSkips; ++i)
            {
                int index = Random.Next(spawnPositions.Count);
                removedCells[i] = spawnPositions[index];
                spawnPositions.RemoveAt(index);
            }
#if EMPTY_LEVELDEBUG
            spawnPositions.Clear();
#endif
            // spawn generation
            foreach(Vector2 pos in spawnPositions)
            {
                // brute force..
                double nearestDist = spawnPositions.Min(x => { return x == pos ? 1 : (x - pos).LengthSquared(); });

                float capturesize = (float)(100.0 + nearestDist * nearestDist * 25000);
                capturesize = Math.Min(capturesize, 1100);

                spawnPoints.Add(new SpawnPoint(pos, capturesize, (float)Math.Sqrt(nearestDist), - 1, content));
            }
            mapObjects.AddRange(spawnPoints);

            // background
            List<Vector2> renderCells = new List<Vector2>(spawnPoints.Select(x => x.Position));// to keep things simple, place spawnpoints at the beginning
            renderCells.AddRange(removedCells);
            for (int playerIndex = 0; playerIndex < Settings.Instance.NumPlayers; ++playerIndex)
            {
                if (Settings.Instance.GetPlayer(playerIndex).Type == Player.Type.NONE)
                    renderCells.Add(cellPositions[Settings.Instance.GetPlayer(playerIndex).SlotIndex]);
            }
            background.Generate(device, renderCells, Level.RELATIVE_MAX);
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
            int rectSizeX = (int)(size.X * fieldSize_pixel.X + 0.5f);
            int rectSizeY = (int)(size.Y * fieldSize_pixel.Y + 0.5f);
            int rectx = (int)(position.X / RELATIVE_MAX.X * FieldPixelSize.X + FieldPixelOffset.X);
            int recty = (int)(position.Y / RELATIVE_MAX.Y * FieldPixelSize.Y + FieldPixelOffset.Y);

            return new Rectangle(rectx, recty, rectSizeX, rectSizeY);
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
            foreach (MapObject interest in mapObjects)
            {
                // statistics
                if (interest is SpawnPoint)
                {
                    //if((interest as SpawnPoint).PossessingPercentage == 1f)
                        prevPosPlayer = (interest as SpawnPoint).PossessingPlayer;
                }
                // original line
                interest.ApplyDamage(damageMap, timeInterval);
                // statistics
                if (interest is SpawnPoint)
                {
                    if (prevPosPlayer != (interest as SpawnPoint).PossessingPlayer)
                    {
                        if (prevPosPlayer != -1)
                        {
                            GameStatistics.addLostSpawnPoints(prevPosPlayer);
                            InputManager.Instance.StartRumble(prevPosPlayer, 0.42f, 0.6f);
                        }
                        if ((interest as SpawnPoint).PossessingPercentage == 1f && (interest as SpawnPoint).PossessingPlayer != -1)
                        {
                            GameStatistics.addCaptueredSpawnPoints((interest as SpawnPoint).PossessingPlayer);
                            InputManager.Instance.StartRumble((interest as SpawnPoint).PossessingPlayer,0.42f,0.5f);
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

                Crosshair crosshair = mapObject as Crosshair;
                if (crosshair != null)
                {
                    crosshair.Position = players[crosshair.PlayerIndex].CursorPosition;
                    crosshair.ParticleAttractionPosition = players[crosshair.PlayerIndex].ParticleAttractionPosition;
                    //crosshair.Alive = players[crosshair.PlayerIndex].Alive;
                    crosshair.PlayerAlive = players[crosshair.PlayerIndex].Alive;
                }

                // statistics
                else if (mapObject is SpawnPoint)
                {
                    SpawnPoint sp = mapObject as SpawnPoint;
                    if (sp.PossessingPlayer != -1)
                    {
                        possesingSpawnPoints[sp.PossessingPlayer]++;
                        possesingSpawnPointsOverallSize[sp.PossessingPlayer] += sp.Size;
                    }
                }
            }

            // remove dead objects
            for (int i = 0; i < mapObjects.Count; ++i)
            {
                if (!mapObjects[i].Alive)
                {
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

                    mapObjects.RemoveAt(i);
                    --i;
                }
            }

            // random events
            if (pickuptimer.Elapsed.TotalSeconds > itemSpawnTime)
            {
                // random position within a certain range
                Vector2 position;
                
#if NO_ITEMS
#else
                double next_item_pos = Random.NextDouble();
                int item = 0;
                for (int i = 1; i < itemPossibilities.Length; i++)
                {
                    if (itemPossibilities[i - 1] < next_item_pos && next_item_pos < itemPossibilities[i])
                    {
                        item = i;
                    }
                }

                if (Random.NextDouble() < itemWinLoseRubberBand && GameStatistics.Steps > itemWinLoseRubberBandMinStepsPlayed)
                {
                    int weakestPlayer = 0;
                    float minDomination = GameStatistics.getDominationInStep(0, GameStatistics.Steps - 1);
                    for (int i = 1; i < GameStatistics.PlayerCount; ++i)
                    {
                        if(item != 1)
                            if (minDomination > GameStatistics.getDominationInStep(i, GameStatistics.Steps - 1))
                            {
                                weakestPlayer = i;
                                minDomination = GameStatistics.getDominationInStep(i, GameStatistics.Steps - 1);
                            }
                            else if(minDomination < GameStatistics.getDominationInStep(i, GameStatistics.Steps - 1))
                            {
                                weakestPlayer = i;
                                minDomination = GameStatistics.getDominationInStep(i, GameStatistics.Steps - 1);
                            }
                    }

                    var spwp = spawnPoints.Where(x => x.PossessingPlayer == weakestPlayer);
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
                    position = new Vector2((float)(Random.NextDouble()) * (RELATIVE_MAX.X - 0.2f) + 0.1f, (float)(Random.NextDouble()) * (RELATIVE_MAX.Y - 0.2f) + 0.1f);

                AddItem(item, position);
#endif
                // restart timer
                pickuptimer.Reset();
                pickuptimer.Start();
            }

            // statistics
            if (GameStatistics.UpdateTimer((float)gameTime.TotalGameTime.TotalSeconds))
            {
                if (!dontSaveTheFirstStepBecauseThatLeadsToSomeUglyStatisticsBug)
                {
                    for (int i = 0; i < players.Length; ++i)
                    {
                        GameStatistics.setParticlesAndHealthAndPossesingSpawnPoints(i, (uint)players[i].NumParticlesAlive, (uint)players[i].TotalVirusHealth, (uint)possesingSpawnPoints[i]);
                    }
                    GameStatistics.UpdateDomination(players);
                }
                else dontSaveTheFirstStepBecauseThatLeadsToSomeUglyStatisticsBug = false;
            }

            // background colors
            var colors = spawnPoints.Select(x => {
                Color color = x.ComputeColor();
                float saturation = Vector3.Dot(color.ToVector3(), new Vector3(0.3f, 0.59f, 0.11f));
                return Color.Lerp(color, new Color(saturation, saturation, saturation), 0.8f) * 1.5f;
            })
            .Concat(Enumerable.Repeat(Color.DimGray, background.NumBackgroundCells - spawnPoints.Count));
            background.UpdateColors(colors.ToArray());
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
                case 1: mapObjects.Add(new Debuff(position, contentManager)); break;
                case 2: mapObjects.Add(new Item(position, Item.ItemType.DANGER_ZONE, contentManager)); break;
                case 3: mapObjects.Add(new Item(position, Item.ItemType.MUTATION, contentManager)); break;
                case 4: mapObjects.Add(new Item(position, Item.ItemType.WIPEOUT, contentManager)); break;
                default: break;
            }
        }

        public void UpdateSwitching(float frameTimeSeconds, Player[] players)
        {
            switchCountdownTimer -= frameTimeSeconds;
            if (switchCountdownActive && switchCountdownTimer < 0.0f)
            {
                if(Settings.Instance.Sound)
                    switchSound.Play();

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

        public void Draw(GameTime gameTime, GraphicsDevice device, Player[] players)
        {
            // activate scissor test - is this a performance issue?
            device.ScissorRectangle = fieldPixelRectangle;
            device.RasterizerState = scissorTestRasterizerState;

            // background
            background.Draw(device, (float)gameTime.TotalGameTime.TotalSeconds);

            // screenblend stuff
           /* spriteBatch.Begin(SpriteSortMode.BackToFront, ScreenBlend);
            foreach (MapObject mapObject in mapObjects)
            {
                if (mapObject.Alive)
                    mapObject.Draw_ScreenBlended(spriteBatch, this, totalTimeSeconds);
            }
            spriteBatch.End();*/

            // the particles!
            DrawParticles(device);

            // vignetting
            device.BlendState = VignettingBlend;
            device.SetVertexBuffer(vignettingQuadVertexBuffer);
            vignettingShader.CurrentTechnique.Passes[0].Apply();
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            device.BlendState = BlendState.Opaque;

            // alphablended spritebatch stuff
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, scissorTestRasterizerState);

            // all alpha blended objects
            foreach (MapObject mapObject in mapObjects)
            {
                if (mapObject.Alive)
                    mapObject.Draw_AlphaBlended(spriteBatch, this, gameTime);
            }

            // countdown
            DrawCountdown(device, (float)gameTime.TotalGameTime.TotalSeconds);


            spriteBatch.End();

            // rest rasterizer state if not allready happend
            device.RasterizerState = RasterizerState.CullNone;
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
                                                     mutateBigRect.Y - fontCountdownLarge.MeasureString(text).Y / 2 + 10),
                                    Color.LightGray, 0.0f, Vector2.Zero, 1, SpriteEffects.None, 0.0f);
            }
        }

        public void Resize(GraphicsDevice device)
        {
            // letterboxing
            float sizeY = Settings.Instance.ResolutionX / RELATIVECOR_ASPECT_RATIO;
            if (sizeY + PercentageBar.HEIGHT > Settings.Instance.ResolutionY)
                sizeY = Settings.Instance.ResolutionY - PercentageBar.HEIGHT;
            fieldSize_pixel = new Point((int)(sizeY * RELATIVECOR_ASPECT_RATIO), (int)sizeY);

            fieldOffset_pixel = new Point(Settings.Instance.ResolutionX - fieldSize_pixel.X, Settings.Instance.ResolutionY - fieldSize_pixel.Y + PercentageBar.HEIGHT);
            fieldOffset_pixel.X /= 2;
            fieldOffset_pixel.Y /= 2;

            fieldPixelRectangle = new Rectangle(fieldOffset_pixel.X, fieldOffset_pixel.Y, fieldSize_pixel.X, fieldSize_pixel.Y);

            // setup background
            Vector2 posScale = new Vector2(fieldSize_pixel.X, -fieldSize_pixel.Y) /
                               new Vector2(Settings.Instance.ResolutionX, Settings.Instance.ResolutionY) * 2;
            Vector2 posOffset = new Vector2(fieldOffset_pixel.X, -fieldOffset_pixel.Y) /
                                   new Vector2(Settings.Instance.ResolutionX, Settings.Instance.ResolutionY) * 2 - new Vector2(1, -1);
            vignettingShader.Parameters["PosScale"].SetValue(posScale);
            vignettingShader.Parameters["PosOffset"].SetValue(posOffset);

            CreateParticleTarget(device);

            // bg particles
            background.Resize(device, fieldPixelRectangle, Level.RELATIVE_MAX);
        }

        private void CreateParticleTarget(GraphicsDevice device)
        {
            if (particleTexture != null)
                particleTexture.Dispose();

            particleTexture = new RenderTarget2D(device, (int)(FieldPixelSize.X),
                                                         (int)(FieldPixelSize.Y),
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
                    switchCountdownTimer = SWITCH_COUNTDOWN_LENGTH;
                    switchCountdownActive = true;
                    break;

                case Item.ItemType.WIPEOUT:
                    mapObjects.Add(DamageArea.CreateWipeout(contentManager));
                    break;
            }
            // statistic
            GameStatistics.addUsedItems(player.Index);
            GameStatistics.ItemUsed(player.Index, player.ItemSlot);
        }
    }
}
