using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Content;

namespace ParticleStormControl
{
    public class Player
    {
        public const int MaxNumPlayers = 4;

        #region Particles

        /// <summary>
        /// size of particle-data rendertargets and textures
        /// </summary>
        public const int maxParticlesSqrt = 128;
        public const int maxParticles = maxParticlesSqrt * maxParticlesSqrt;

        // info texture:
        // X: Health
        // Y: Speed

        public Texture2D PositionTexture { get { return positionTargets[currentTextureIndex]; } }
        public Texture2D InfoTexture { get { return infoTargets[currentTextureIndex]; } }
        private Texture2D MovementTexture { get { return movementTexture[currentTextureIndex]; } }
        private Texture2D noiseTexture;

        private int currentTargetIndex = 0;
        private int currentTextureIndex = 1;
        private RenderTarget2D[] positionTargets = new RenderTarget2D[2];
        private RenderTarget2D[] infoTargets = new RenderTarget2D[2];
        private RenderTarget2D[] movementTexture = new RenderTarget2D[2];
        private RenderTargetBinding[][] renderTargetBindings;

        private HalfVector2[] particleInfos = new HalfVector2[maxParticlesSqrt * maxParticlesSqrt];

        private const int maxSpawnsPerFrame = 15;

        public int CurrentSpawnNumber
        { get { return numSpawns; } }
        private int numSpawns = 0;
        private Vector4[] spawnsAt_Positions = new Vector4[maxSpawnsPerFrame];
        private Vector4[] spawnInfos = new Vector4[maxSpawnsPerFrame];

        Effect particleProcessing;

        public int NumParticlesAlive
        { get; private set; }

        public int HighestUsedParticleIndex
        { get; private set; }

        #endregion

        #region Colors

        public readonly static Color[] Colors = { Color.Red, Color.Blue, Color.Yellow, Color.Green, Color.Black, Color.Pink, Color.Orange  };
        public readonly static Color[] ParticleColors = { new Color(240, 80, 70), new Color(75, 95, 220), new Color(250, 216, 50), new Color(80, 200, 80), Color.DarkSlateGray, Color.HotPink, new Color(250, 120, 20) };
        public readonly static string[] Names = { "Red", "Blue", "Yellow", "Green", "Black", "Pink", "Orange" };

#if XBOX
        public readonly static Color[] TextureDamageValue = {  new Color(0, 0, 0, 1),
                                                               new Color(0, 0, 1, 0),
                                                               new Color(0, 1, 0, 0),
                                                               new Color(1, 0, 0, 0)   };

        private readonly static Vector4[] DamageMapMask = { new Vector4(1, 1, 1, 0),
                                                            new Vector4(1, 1, 0, 1),
                                                            new Vector4(1, 0, 1, 1),
                                                            new Vector4(0, 1, 1, 1) };
#else
        public readonly static Color[] TextureDamageValue = {  new Color(1, 0, 0, 0),
                                                               new Color(0, 1, 0, 0),
                                                               new Color(0, 0, 1, 0),
                                                               new Color(0, 0, 0, 1)   };
        private readonly static Vector4[] DamageMapMask = { new Vector4(0, 1, 1, 1),
                                                            new Vector4(1, 0, 1, 1),
                                                            new Vector4(1, 1, 0, 1),
                                                            new Vector4(1, 1, 1, 0)  };
#endif

        #endregion

        #region Control

        public enum ControlType
        {
            KEYBOARD0,KEYBOARD1,
            GAMEPAD0,
            GAMEPAD1,
            GAMEPAD2,
            GAMEPAD3
        };
        static public readonly String[] ControlNames = new String[]
        {
            "Arrow Keys + STRG", "WASD + SHIFT",
            "Gamepad 1",
            "Gamepad 2",
            "Gamepad 3",
            "Gamepad 4",
        };


        public ControlType Controls { get; set; }

        #endregion

        #region hold move
        // variables to control the hold move, it is just test
        /// <summary>
        /// Position for the hold move
        /// </summary>
        private Vector2 holdTargedPosition = Vector2.Zero;
        /// <summary>
        /// is the hold move actice?
        /// </summary>
        private bool holdTargetPositionSet = false;
        #endregion

        #region cursor
        /// <summary>
        /// position of the particle cursor
        /// </summary>
        public Vector2 CursorPosition
        {
            get { return cursorPosition; }
        }
        private Vector2 cursorPosition;
        private readonly static Vector2[] cursorStartPositions =
            {
                new Vector2(0.2f, 0.8f),
                new Vector2(0.8f, 0.2f),
                new Vector2(0.8f, 0.8f),
                new Vector2(0.2f, 0.2f)
            };
        #endregion

        // who is who (blue etc.)
        public readonly int playerIndex;
        public int Index { get { return playerIndex; } }

        // spawn stuff!
        private const float spawnConstant = 12.0f;  // higher means LESS!
        private const float spawnSettingFactor = 5.0f;  // remeber that high mass means mass_health=-1.0f

        #region pad properties

        // please all values from -1 to 1
        private float disciplin_speed = 0.0f;  // negative more disciplin, ...
        public float Disciplin_speed
        {
            get { return disciplin_speed; }
            set { disciplin_speed = MathHelper.Clamp(value, -1, 1); }
        }

        private float mass_health = 0.0f;      // negative more mass, positive more health 
        public float Mass_health
        {
            get { return mass_health; }
            set { mass_health = MathHelper.Clamp(value, -1, 1); }
        }


        // speed stuff
        private const float speedConstant = 0.10f; 
        private const float speedSettingFactor = 0.04f;

        // life stuff
        public const float healthConstant = 15.0f;
        public const float healthSettingFactor = 15.0f;

        // discilplin constant - higher means that the player will move more straight in player's direction
        private const float disciplinConstant = 0.4f;
        #endregion

        private const float attackingPerSecond = 30.0f;

        #region stuff about randomization

        readonly Random random = new Random();
        private const float NoiseToMovementFactor = 15 * disciplinConstant;

        #endregion

        public bool Alive
        { get { return alive; } }
        private bool alive = true;
        public float TimeDead
        { get; private set; }

        private int colorIndex;
        public Color Color
        { get { return Colors[colorIndex]; } }
        public Color ParticleColor
        { get { return ParticleColors[colorIndex]; } }



        public float RemainingTimeAlive
        {
            get
            {
                if (timeWithoutSpawnPoint <= 0.0f)
                    return float.PositiveInfinity;
                else
                    return maxTimeWithoutSpawnPoint - timeWithoutSpawnPoint;
            }
        }

        private float timeWithoutSpawnPoint = 0.0f;
        private const float maxTimeWithoutSpawnPoint = 15.0f;

        /// <summary>
        /// performs a switch between 2 players
        /// </summary>
        public static void SwitchPlayer(Player player1, Player player2)
        {
            RenderTarget2D[] targets = player1.infoTargets;
            player1.infoTargets = player2.infoTargets;
            player2.infoTargets = targets;

            targets = player1.positionTargets;
            player1.positionTargets = player2.positionTargets;
            player2.positionTargets = targets;

            targets = player1.movementTexture;
            player1.movementTexture = player2.movementTexture;
            player2.movementTexture = targets;


            RenderTargetBinding[][] bindings = player1.renderTargetBindings;
            player1.renderTargetBindings = player2.renderTargetBindings;
            player2.renderTargetBindings = bindings;

            int i = player1.currentTextureIndex;
            player1.currentTextureIndex = player2.currentTextureIndex;
            player2.currentTextureIndex = i;

            i = player1.currentTargetIndex;
            player1.currentTargetIndex = player2.currentTargetIndex;
            player2.currentTargetIndex = i;

            HalfVector2[] vh4 = player1.particleInfos;
            player1.particleInfos = player2.particleInfos;
            player2.particleInfos = vh4;

            i = player1.HighestUsedParticleIndex;
            player1.HighestUsedParticleIndex = player2.HighestUsedParticleIndex;
            player2.HighestUsedParticleIndex = i;
        }

        public Player(int playerIndex, GraphicsDevice device, ContentManager content, Texture2D noiseTexture, int colorIndex)
        {
            this.playerIndex = playerIndex;
            this.noiseTexture = noiseTexture;
            this.colorIndex = colorIndex;

            cursorPosition = cursorStartPositions[playerIndex];

            for (int i = 0; i < maxParticles; ++i)
                particleInfos[i] = new HalfVector2(-1.0f, -1.0f);

            // create rendertargets (they are pingponging ;) )
            positionTargets[0] = new RenderTarget2D(device, maxParticlesSqrt, maxParticlesSqrt, false, SurfaceFormat.HalfVector2, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            positionTargets[1] = new RenderTarget2D(device, maxParticlesSqrt, maxParticlesSqrt, false, SurfaceFormat.HalfVector2, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            infoTargets[0] = new RenderTarget2D(device, maxParticlesSqrt, maxParticlesSqrt, false, SurfaceFormat.HalfVector2, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            infoTargets[1] = new RenderTarget2D(device, maxParticlesSqrt, maxParticlesSqrt, false, SurfaceFormat.HalfVector2, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            movementTexture[0] = new RenderTarget2D(device, maxParticlesSqrt, maxParticlesSqrt, false, SurfaceFormat.HalfVector2, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            movementTexture[1] = new RenderTarget2D(device, maxParticlesSqrt, maxParticlesSqrt, false, SurfaceFormat.HalfVector2, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            renderTargetBindings = new RenderTargetBinding[][] { new RenderTargetBinding[] { positionTargets[0], movementTexture[0], infoTargets[0] }, 
                                                                new RenderTargetBinding[] { positionTargets[1], movementTexture[1], infoTargets[1] } };
            particleProcessing = content.Load<Effect>("shader/particleProcessing");


            InfoTexture.SetData<HalfVector2>(particleInfos);
        }

        public void UpdateGPUPart(GraphicsDevice device, float timeInterval, Texture2D damageMapTexture)
        {
            device.Textures[0] = null;
            device.Textures[1] = null;
            device.Textures[2] = null;
            device.Textures[3] = null;

            // render
            particleProcessing.Parameters["halfPixelCorrection"].SetValue(new Vector2(-0.5f / maxParticlesSqrt, 0.5f / maxParticlesSqrt));

            particleProcessing.Parameters["Positions"].SetValue(PositionTexture);
            particleProcessing.Parameters["Movements"].SetValue(MovementTexture);
            particleProcessing.Parameters["Infos"].SetValue(InfoTexture);

            // hold move
            if (holdTargetPositionSet)
                particleProcessing.Parameters["CursorPosition"].SetValue(holdTargedPosition);
            else
                particleProcessing.Parameters["CursorPosition"].SetValue(cursorPosition);
            particleProcessing.Parameters["MovementChangeFactor"].SetValue(disciplinConstant*timeInterval);
            particleProcessing.Parameters["TimeInterval"].SetValue(timeInterval);
            particleProcessing.Parameters["DamageMap"].SetValue(damageMapTexture);
            particleProcessing.Parameters["DamageFactor"].SetValue(DamageMapMask[playerIndex] * (attackingPerSecond * timeInterval * 255));

            particleProcessing.Parameters["NumSpawns"].SetValue(numSpawns);
            particleProcessing.Parameters["SpawnsAt_Positions"].SetValue(spawnsAt_Positions);
            particleProcessing.Parameters["SpawnInfos"].SetValue(spawnInfos);

            particleProcessing.Parameters["NoiseToMovementFactor"].SetValue(timeInterval * NoiseToMovementFactor);
            particleProcessing.Parameters["NoiseTexture"].SetValue(noiseTexture);


            device.BlendState = BlendState.Opaque;
            device.SetRenderTargets(renderTargetBindings[currentTargetIndex]);
            particleProcessing.CurrentTechnique.Passes[0].Apply();
            ScreenTriangleRenderer.instance.DrawScreenAlignedTriangle(device);


            int i = currentTargetIndex;
            currentTargetIndex = currentTextureIndex;
            currentTextureIndex = i;
        }

        public void ReadGPUResults()
        {
            InfoTexture.GetData<HalfVector2>(particleInfos);
        }

        public void UpdateCPUPart(float timeInterval, IList<MapObject> mapObjects, bool cantDie)
        {
            float health = ((mass_health * 0.5f) + 1.5f) * healthConstant;
            float speed = speedConstant + speedSettingFactor * disciplin_speed;

            for (int i = 0; i < maxSpawnsPerFrame; ++i)
            {
                spawnsAt_Positions[i] = new Vector4(-1.0f);
                spawnInfos[i] = new Vector4(-1.0f);
            }

            // compute spawnings
            numSpawns = 0;
            int numSpawnPoints = 0;
            foreach (MapObject interest in mapObjects)
            {
                SpawnPoint spawn = interest as SpawnPoint;
                if (spawn != null && spawn.PossessingPlayer == playerIndex)
                {
                    spawn.SpawnTimeAccum += timeInterval;
                    float f = spawn.SpawnSize / (spawnConstant + spawnSettingFactor * mass_health);
                    int numSpawned = (int)(spawn.SpawnTimeAccum * f);

                    if (numSpawned > 0)
                    {
                        spawn.SpawnTimeAccum -= numSpawned / f; // don't miss anything!
                        for (int i = 0; i < numSpawned; ++i)
                        {
                            if (numSpawns == maxSpawnsPerFrame)
                                break;

                            // random movement
                            Vector2 movement = new Vector2((float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f);
                            movement.Normalize();

                            // add
                            spawnsAt_Positions[numSpawns].Z = spawn.Position.X;
                            spawnsAt_Positions[numSpawns].W = spawn.Position.Y;
                            spawnInfos[numSpawns] = new Vector4(movement.X, movement.Y, health, speed);
                            ++numSpawns;
                        }
                    }
                    ++numSpawnPoints;
                }
            }

            // find places for spawning and check if there are any particles
            int biggestAliveIndex = 0;
            NumParticlesAlive = 0;
            int currentSpawn = 0;
            int imax = (int)MathHelper.Clamp(HighestUsedParticleIndex + numSpawns + 1, 0, maxParticles);
            for (int i = 0; i < imax; ++i)
            {
                if (IsAlive(i))
                {
                    ++NumParticlesAlive;
                    biggestAliveIndex = i;
                }
                else if (currentSpawn < numSpawns)
                {
                    spawnsAt_Positions[currentSpawn].X = (float)(i % maxParticlesSqrt) / maxParticlesSqrt;
                    spawnsAt_Positions[currentSpawn].Y = (float)(i / maxParticlesSqrt) / maxParticlesSqrt;
                    ++currentSpawn;

                    ++NumParticlesAlive;
                    biggestAliveIndex = i;
                }
            }
            if (currentSpawn == 0 && alive)
                alive = NumParticlesAlive > 0 || cantDie; // still alive *sing*
            HighestUsedParticleIndex = biggestAliveIndex;

            // alive due to number of spawnpoints?
            UpdateAliveStatusBySpawns(timeInterval, numSpawnPoints);
        }


        private bool IsAlive(int particleIndex)
        {
            return (particleInfos[particleIndex].PackedValue & (((UInt32)1) << 15)) == 0;

            // save version - use this temporary in case of bad xbox behaviour
            // return particleInfos[particleIndex].ToVector4().Z < 0;
        }

        /// <summary>
        /// controll through a gamepad or 
        /// </summary>
        public void UserControl(float frameTimeInterval)
        {
            Vector2 cursorMove;
            Vector2 padMove;
            MovementsFromControls(out cursorMove, out padMove);

            float len = padMove.Length();
            if (len > 1.0f) padMove /= len;
            len = cursorMove.Length();
            if (len > 1.0f) cursorMove /= len;

            cursorMove *= frameTimeInterval * 0.5f;
            padMove *= frameTimeInterval * 2.0f;

            mass_health += padMove.Y;
            disciplin_speed -= padMove.X;
            cursorPosition += cursorMove;

            mass_health = MathHelper.Clamp(mass_health, -1.0f, 1.0f);
            disciplin_speed = MathHelper.Clamp(disciplin_speed, -1.0f, 1.0f);
            cursorPosition.X = MathHelper.Clamp(cursorPosition.X, 0.0f, 1.0f);
            cursorPosition.Y = MathHelper.Clamp(cursorPosition.Y, 0.0f, 1.0f);
        }

        private void MovementsFromControls(out Vector2 cursorMove, out Vector2 padMove)
        {
            padMove = Vector2.Zero;
            cursorMove = Vector2.Zero;

            switch(Controls)
            {

#if !XBOX

                case ControlType.KEYBOARD0:
                    {
                        Vector2 keyboardMove = new Vector2(InputManager.Instance.IsButtonDown(Keys.Right) ? 1 : 0 - (InputManager.Instance.IsButtonDown(Keys.Left) ? 1 : 0),
                                                            InputManager.Instance.IsButtonDown(Keys.Down) ? -1 : 0 + (InputManager.Instance.IsButtonDown(Keys.Up) ? 1 : 0));
                        float l = keyboardMove.Length();
                        if (l > 1) keyboardMove /= l;
                        if (InputManager.Instance.IsButtonDown(Keys.RightControl))
                            padMove = keyboardMove;
                        else
                            cursorMove = keyboardMove;
                        // Some simple Test for a new movment technique (hold move)
                        if (InputManager.Instance.IsButtonDown(Keys.Space))
                        {
                            if (!holdTargetPositionSet)
                            {
                                holdTargedPosition = cursorPosition;
                                holdTargetPositionSet = true;
                            }
                        }
                        else
                        {
                            holdTargetPositionSet = false;
                        }
                        break;
                    }
                
                case ControlType.KEYBOARD1:
                    {
                        Vector2 keyboardMove = new Vector2(InputManager.Instance.IsButtonDown(Keys.D) ? 1 : 0 - (InputManager.Instance.IsButtonDown(Keys.A) ? 1 : 0),
                                                            InputManager.Instance.IsButtonDown(Keys.S) ? -1 : 0 + (InputManager.Instance.IsButtonDown(Keys.W) ? 1 : 0));
                        float l = keyboardMove.Length();
                        if (l > 1) keyboardMove /= l;
                        if (InputManager.Instance.IsButtonDown(Keys.LeftControl))
                            padMove = keyboardMove;
                        else
                            cursorMove = keyboardMove;
                        break;
                    }

               
#endif
                case ControlType.GAMEPAD0:
                    padMove = InputManager.Instance.GetRightStickMovement(0);
                    cursorMove = InputManager.Instance.GetLeftStickMovement(0);
                    break;

                case ControlType.GAMEPAD1:
                    padMove = InputManager.Instance.GetRightStickMovement(1);
                    cursorMove = InputManager.Instance.GetLeftStickMovement(1);
                    break;

                case ControlType.GAMEPAD2:
                    padMove = InputManager.Instance.GetRightStickMovement(2);
                    cursorMove = InputManager.Instance.GetLeftStickMovement(2);
                    break;

                case ControlType.GAMEPAD3:
                    padMove = InputManager.Instance.GetRightStickMovement(3);
                    cursorMove = InputManager.Instance.GetLeftStickMovement(3);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            padMove.Y = -padMove.Y;
            cursorMove.Y = -cursorMove.Y;
        }

        /// <summary>
        /// still alive?
        /// </summary>
        private void UpdateAliveStatusBySpawns(float timeInterval, int numSpawns)
        {
            if (alive)
            {
                if (numSpawns == 0)
                {
                    timeWithoutSpawnPoint += timeInterval;

                    if (timeWithoutSpawnPoint > maxTimeWithoutSpawnPoint)
                        alive = false;
                }
                else
                    timeWithoutSpawnPoint = 0.0f;
            }
            else
                TimeDead += timeInterval;
        }
    }
}
