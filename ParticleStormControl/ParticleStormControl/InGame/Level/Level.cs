using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ParticleStormControl
{
    public class Level
    {
        // statistics
        public Statistics GameStatistics { get; set; }

        private List<MapObject> mapObjects = new List<MapObject>();
        private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
        public List<SpawnPoint> SpawnPoints { get { return spawnPoints; } }

        private Texture2D pixelTexture;

        private SpriteBatch spriteBatch;
        private ContentManager contentManager;

        private VertexBuffer backgroundVertexBuffer;
        private Effect backgroundShader;

        #region debuff ressources
        private SoundEffect debuffExplosionSound;
        private Texture2D debuffItemTexture;
        private Texture2D debuffExplosionTexture;
        #endregion

        #region dangerzone ressources
        private SoundEffect dangerZoneSound;
        private Texture2D dangerZoneInnerTexture;
        private Texture2D dangerZoneOuterTexture;
        #endregion

        public static BlendState ShadowBlend = new BlendState
                                                   {
                                                       ColorSourceBlend = Blend.One,
                                                       ColorDestinationBlend = Blend.InverseSourceAlpha,
                                                       ColorBlendFunction = BlendFunction.Add
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

        #endregion

        #region switch & wipeout

        private SoundEffect switchSound;
        private bool switchCountdownActive = false;
        private float switchCountdownTimer;
        private const float switchCountdownLength = 6.0f;
        private SpriteFont fontCountdownLarge;

        private Texture2D wipeoutExplosionTexture;
        private Texture2D wipeoutDamageTexture;
        private const float WIPEOUT_SPEED = 1.0f;
        private const float WIPEOUT_SIZEFACTOR = 1.5f;
        private float wipeoutProgress = 0.0f;
        private bool wipeoutActive;

        #endregion


        private Stopwatch pickuptimer;

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

        private BackgroundParticles backgroundParticles;

        public Level(GraphicsDevice device, ContentManager content)
        {
            this.contentManager = content;

            pickuptimer = new Stopwatch();
            pickuptimer.Start();

            pixelTexture = content.Load<Texture2D>("pix");
            
            spriteBatch = new SpriteBatch(device);

            // debuff
            debuffExplosionSound = content.Load<SoundEffect>("sound/explosion");
            debuffExplosionTexture = content.Load<Texture2D>("explosion");
            debuffItemTexture = content.Load<Texture2D>("items/debuff");

            // dangerzone
            dangerZoneSound = content.Load<SoundEffect>("sound/danger_zone");
            dangerZoneInnerTexture = content.Load<Texture2D>("danger_zone_inner");
            dangerZoneOuterTexture = content.Load<Texture2D>("danger_zone_outer");

            // switch
            switchSound = content.Load<SoundEffect>("sound/switch");
            fontCountdownLarge = content.Load<SpriteFont>("fontCountdown");

            // background
            backgroundVertexBuffer = new VertexBuffer(device, ScreenTriangleRenderer.ScreenAlignedTriangleVertex.VertexDeclaration, 4, BufferUsage.WriteOnly);
            backgroundVertexBuffer.SetData(new Vector2[4] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) });
            backgroundShader = content.Load<Effect>("shader/background");

            // bg particles
            backgroundParticles = new BackgroundParticles(device, content, 1000);

            // setup size
            Resize(device);
            
            // wipeout
            wipeoutExplosionTexture = content.Load<Texture2D>("capture_glow");
            wipeoutDamageTexture = content.Load<Texture2D>("capture_glow");
        }

        public void NewGame(Player[] players)
        {
            mapObjects.Clear();
            spawnPoints.Clear();

            // create level
            CreateLevel(contentManager, players.Length);

            // crosshairs for players
            Texture2D crossHairTexture = contentManager.Load<Texture2D>("basic_crosshair");
            for (int i = 0; i < players.Length; ++i)
                mapObjects.Add(new Crosshair(i, crossHairTexture));
        }

        private void CreateLevel(ContentManager content, int numPlayers)
        {
            // needed ressources
            SoundEffect captureExplosion = content.Load<SoundEffect>("sound/captureExplosion");
            SoundEffect capture = content.Load<SoundEffect>("sound/capture");
            Texture2D captureGlow = content.Load<Texture2D>("capture_glow");
            Texture2D glowTexture = content.Load<Texture2D>("glow");
            Texture2D hqInner = content.Load<Texture2D>("unit_hq_inner");
            Texture2D hqOuter = content.Load<Texture2D>("unit_hq_outer");

            // how many?
            const int MIN_NUMPOINTS = 4;
            const int MAX_NUMPOINTS = 12;
            int pointcount = Random.Next(MAX_NUMPOINTS - MIN_NUMPOINTS) + MIN_NUMPOINTS;

            // player starts
            const float LEVEL_BORDER = 0.2f;

            spawnPoints.Add(new SpawnPoint(new Vector2(LEVEL_BORDER, RELATIVE_MAX.Y - LEVEL_BORDER), 1000.0f, 0.3f, 0, capture, captureExplosion,
                                                    glowTexture, captureGlow, hqInner, hqOuter));
            spawnPoints.Add(new SpawnPoint(new Vector2(RELATIVE_MAX.X - LEVEL_BORDER, LEVEL_BORDER), 1000.0f, 0.3f, 1, capture, captureExplosion,
                                                    glowTexture, captureGlow, hqInner, hqOuter));
            if(numPlayers >= 3)
            {
                spawnPoints.Add(new SpawnPoint(new Vector2(RELATIVE_MAX.X - LEVEL_BORDER, RELATIVE_MAX.Y - LEVEL_BORDER), 1000.0f, 0.3f, 2, capture, captureExplosion,
                                    glowTexture, captureGlow, hqInner, hqOuter));
            }
            if (numPlayers == 4)
            {
                spawnPoints.Add(new SpawnPoint(new Vector2(LEVEL_BORDER, LEVEL_BORDER), 1000.0f, 0.3f, 3, capture, captureExplosion,
                                    glowTexture, captureGlow, hqInner, hqOuter));
            }

            // generate in a grid of equilateral triangles
            const int SPAWNS_GRID_X = 6;
            const int SPAWNS_GRID_Y = 3;
            const double POSITION_JITTER = 0.13;
            List<Vector2> spawnPositions = new List<Vector2>();
            for (int x = 0; x < SPAWNS_GRID_X; ++x)
            {
                for (int y = 0; y < SPAWNS_GRID_Y; ++y)
                {
                    // "natural skip"
                    if ((x == SPAWNS_GRID_X-1 && y % 2 == 0) ||
                        ((x == 0 || x == SPAWNS_GRID_X - 2 + y % 2) && (y == 0 || y == SPAWNS_GRID_Y - 1)))
                        continue;

                    // position
                    Vector2 pos = new Vector2((float)x / (SPAWNS_GRID_X - 1), (float)y / (SPAWNS_GRID_Y - 1));
                    if (y % 2 == 0)
                        pos.X += 0.5f / (SPAWNS_GRID_X - 1);
                    pos *= RELATIVE_MAX - new Vector2(LEVEL_BORDER) * 2.0f;
                    pos += new Vector2(LEVEL_BORDER, LEVEL_BORDER);

                    // position jitter
                    double posJitter = (Random.NextDouble() * 2.0 - 1.0) * POSITION_JITTER;
                    pos += Random.NextDirection() * (float)posJitter;

                    spawnPositions.Add(pos);
                }
            }

            // random skipping - nonlinear randomness!
            const int MAX_SKIPS = 5;
            int numSkips = (int)(Math.Pow(Random.NextDouble(), 4) * MAX_SKIPS + 0.5f);
            for(int i=0; i<numSkips; ++i)
                spawnPositions.RemoveAt(Random.Next(spawnPositions.Count));

            // spawn generation
            foreach(Vector2 pos in spawnPositions)
            {
                // brute force..
                double nearestDist = spawnPositions.Min(x => { return x == pos ? 1 : (x - pos).LengthSquared(); });

                float capturesize = (float)(100.0 + nearestDist * nearestDist * 25000);
                capturesize = Math.Min(capturesize, 10000);


                spawnPoints.Add(new SpawnPoint(pos, capturesize, (float)Math.Sqrt(nearestDist), - 1, capture, captureExplosion, glowTexture, captureGlow, hqInner, hqOuter));
            }


            mapObjects.AddRange(spawnPoints);

            // numcells
            backgroundShader.Parameters["NumCells"].SetValue(spawnPoints.Count);
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
            foreach (MapObject interest in mapObjects)
                interest.ApplyDamage(damageMap, timeInterval);
        }

        public void Update(float frameTimeSeconds, float totalTimeSeconds, Player[] players)
        {
            // update
            foreach (MapObject mapObject in mapObjects)
            {
                mapObject.Update(frameTimeSeconds, totalTimeSeconds);

                Crosshair crosshair = mapObject as Crosshair;
                if (crosshair != null)
                {
                    crosshair.Position = players[crosshair.PlayerIndex].CursorPosition;
                    crosshair.ParticleAttractionPosition = players[crosshair.PlayerIndex].ParticleAttractionPosition;
                    crosshair.Alive = players[crosshair.PlayerIndex].Alive;
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
                        // reject if player allready owns a item
                        if (players[item.PossessingPlayer].ItemSlot != Item.ItemType.NONE)
                        {
                            item.Alive = true;
                            continue;
                        }
                        else
                        {
                            players[item.PossessingPlayer].ItemSlot = item.Type;
                            // statistics
                            GameStatistics.addCollectedItems(item.PossessingPlayer);
                        }
                    }

                    mapObjects.RemoveAt(i);
                    --i;
                }
            }

            // random events
            if (pickuptimer.Elapsed.TotalSeconds > 2)
            {
                // random position within a certain range
                Vector2 position = new Vector2((float)(Random.NextDouble()) * 0.8f + 0.1f, (float)(Random.NextDouble()) * 0.8f + 0.1f);

                if (Random.NextDouble() < 0.5)
                    mapObjects.Add(new Item(position, Item.ItemType.DANGER_ZONE, contentManager));
                else if (Random.NextDouble() < 0.3)
                    mapObjects.Add(new Debuff(position, debuffExplosionSound, debuffItemTexture, debuffExplosionTexture));
                else if (Random.NextDouble() < 0.18)
                    mapObjects.Add(new Item(position, Item.ItemType.MUTATION, contentManager));
                else if (Random.NextDouble() < 0.1)
                    mapObjects.Add(new Item(position, Item.ItemType.WIPEOUT, contentManager));

                // restart timer
                pickuptimer.Reset();
                pickuptimer.Start();
            }

            // wipeout
            if (wipeoutActive)
            {
                wipeoutProgress += frameTimeSeconds;
                if (wipeoutProgress > 1.0f)
                    wipeoutActive = false;
            }
        }

        public void UpdateSwitching(float frameTimeSeconds, Player[] players)
        {
            switchCountdownTimer -= frameTimeSeconds;
            if (switchCountdownActive && switchCountdownTimer < 0.0f)
            {
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

        public void Draw(float totalTimeSeconds, GraphicsDevice device, Player[] players)
        {
            // background particles
            device.BlendState = BlendState.AlphaBlend;
            backgroundParticles.Draw(device, totalTimeSeconds);

            // background
            device.SetVertexBuffer(backgroundVertexBuffer);
            backgroundShader.Parameters["Cells_Pos2D"].SetValue(spawnPoints.Select(x => x.Position).ToArray());
            backgroundShader.Parameters["Cells_Color"].SetValue(spawnPoints.Select(x => x.ComputeColor().ToVector3()).ToArray());
            backgroundShader.CurrentTechnique.Passes[0].Apply();
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

            // screenblend stuff
            spriteBatch.Begin(SpriteSortMode.BackToFront, ScreenBlend);
            foreach (MapObject mapObject in mapObjects)
            {
                if (mapObject.Alive)
                    mapObject.Draw_ScreenBlended(spriteBatch, this, totalTimeSeconds);
            }
            spriteBatch.End();

            // the particles!
            DrawParticles(device);

            // alphablended spritebatch stuff
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);

            // countdown
            DrawCountdown(device);
            foreach (MapObject mapObject in mapObjects)
            {
                if (mapObject.Alive)
                    mapObject.Draw_AlphaBlended(spriteBatch, this, totalTimeSeconds);
            }

            // wipeout
            if (wipeoutActive)
                spriteBatch.Draw(wipeoutExplosionTexture, ComputePixelRect_Centered(Level.RELATIVE_MAX / 2, Level.RELATIVE_MAX.X * wipeoutProgress * WIPEOUT_SIZEFACTOR),
                                    new Color(1.0f, 1.0f, 1.0f, 1.0f - wipeoutProgress));
    
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

        public void DrawCountdown(GraphicsDevice device)
        {
            if (switchCountdownActive)
            {
                string text = ((int) (switchCountdownTimer + 1)).ToString();
                spriteBatch.DrawString(fontCountdownLarge, text,
                                       new Vector2(
                                           (device.Viewport.Width - fontCountdownLarge.MeasureString(text).X)*
                                           0.5f,
                                           (device.Viewport.Height - fontCountdownLarge.MeasureString(text).Y)*
                                           0.5f + 40), Color.FromNonPremultiplied(140, 140, 140, 160));
            }
        }

        public void Resize(GraphicsDevice device)
        {
            // letterboxing
            float sizeY = device.Viewport.Width / RELATIVECOR_ASPECT_RATIO + 0.5f;
            if (sizeY > device.Viewport.Height)
                fieldSize_pixel = new Point((int)(device.Viewport.Height * RELATIVECOR_ASPECT_RATIO + 0.5f), device.Viewport.Height);
            else
                fieldSize_pixel = new Point((int)(sizeY * RELATIVECOR_ASPECT_RATIO), (int)sizeY);

            fieldOffset_pixel = new Point(device.Viewport.Width - fieldSize_pixel.X, device.Viewport.Height - fieldSize_pixel.Y + PercentageBar.HEIGHT);
            fieldOffset_pixel.X /= 2;
            fieldOffset_pixel.Y /= 2;

            // setup background
            backgroundShader.Parameters["PosScale"].SetValue(new Vector2(fieldSize_pixel.X, -fieldSize_pixel.Y) /
                                                             new Vector2(device.Viewport.Width, device.Viewport.Height) * 2);
            backgroundShader.Parameters["PosOffset"].SetValue(new Vector2(fieldOffset_pixel.X, -fieldOffset_pixel.Y) /
                                                             new Vector2(device.Viewport.Width, device.Viewport.Height) * 2 - new Vector2(1, -1));
            backgroundShader.Parameters["RelativeMax"].SetValue(Level.RELATIVE_MAX);

            CreateParticleTarget(device);

            // bg particles
            backgroundParticles.Resize(device.Viewport.Width, device.Viewport.Height, fieldSize_pixel, fieldOffset_pixel);
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

            if(wipeoutActive)
                damageSpriteBatch.Draw(wipeoutDamageTexture, 
                    DamageMap.ComputePixelRect_Centred(Level.RELATIVE_MAX / 2, Level.RELATIVE_MAX.X * wipeoutProgress * WIPEOUT_SIZEFACTOR), Color.White);
        }

        public void PlayerUseItem(Player player)
        {
            switch (player.ItemSlot)
            {
                case Item.ItemType.DANGER_ZONE:
                    mapObjects.Add(new DangerZone(player.CursorPosition, dangerZoneSound, dangerZoneInnerTexture, dangerZoneOuterTexture, player.Index));
                    // statistic
                    GameStatistics.addUsedItems(player.Index);
                    break;

                case Item.ItemType.MUTATION:
                    switchCountdownTimer = switchCountdownLength;
                    switchCountdownActive = true;
                    // statistic
                    GameStatistics.addUsedItems(player.Index);
                    break;

                case Item.ItemType.WIPEOUT:
                    if (!wipeoutActive)
                    {
                        wipeoutActive = true;
                        wipeoutProgress = 0.0f;
                    }
                    // statistic
                    GameStatistics.addUsedItems(player.Index);
                    break;
            }
        }
    }
}
