﻿using System;
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

        #region Viruses
        
        public enum VirusType
	    {
            H5N1,
            HEPATITISB,
            HIV,
            EPSTEINBAR,

            NUM_VIRUSES
	    }

        public readonly static VirusType[] Viruses = { VirusType.H5N1, VirusType.HEPATITISB, VirusType.HIV, VirusType.EPSTEINBAR };
        public readonly static string[] VirusNames = { "Influenza A virus", "Hepatitis B virus", "Human immunodeficiency virus", "Epstein-Bar virus" };
        public readonly static string[] VirusDescriptions = { "causes influenza in birds and some mammals, and is the only species of influenzavirus A.",
                                                               "is a species of the genus Orthohepadnavirus, which is likewise a part of the Hepadnaviridae family of viruses.",
                                                               "is a lentivirus (slowly replicating retrovirus) that causes acquired immunodeficiency syndrome (AIDS).",
                                                               "best known as the cause of infectious mononucleosis (glandular fever). It is also associated with particular forms of cancer, such as Hodgkin's lymphoma, Burkitt's lymphoma, " };
        private int virusIndex;
        public int VirusIndex
        {
            get { return virusIndex; }
            set { virusIndex = value; }
        }
        public VirusType Virus
        { get { return Viruses[virusIndex]; } }


        #region properties

        // please all values from -1 to 1
        private static readonly float[] disciplin_speed_byVirus = new float[]{ 0.7f, 0.0f, -0.6f, 0.5f };  // negative more disciplin, ...
        public float Disciplin_speed
        {  get { return disciplin_speed_byVirus[virusIndex]; } }

        private static readonly float[] mass_health_byVirus = new float[] { -0.5f, -0.2f, 0.6f, -1.0f };  // negative more mass, positive more health 
        public float Mass_health
        { get { return mass_health_byVirus[virusIndex]; } }



        // speed stuff
        private const float speedConstant = 0.13f;
        private const float speedSettingFactor = 0.04f;

        // life stuff
        public const float healthConstant = 15.0f;
        public const float healthSettingFactor = 15.0f;

        // discilplin constant - higher means that the particles will move more straight in player's direction
        private const float disciplinConstant = 0.5f;
        // attacking constant
        private const float attackingPerSecond = 30.0f;

        #endregion

        #endregion

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

        Effect particleProcessing;

        public int NumParticlesAlive
        { get; private set; }

        public int HighestUsedParticleIndex
        { get; private set; }

        #region spawning

        private const int maxSpawnsPerFrame = 32;
        private int numSpawns = 0;

        public int CurrentSpawnNumber
        { get { return numSpawns; } }

        /// <summary>
        /// vertex for a particle spawn
        /// </summary>
        public struct SpawnVertex : IVertexType
        {
            public Vector2 texturePosition;
            public Vector2 particlePosition;
            public Vector2 movement;
            public Vector2 damageSpeed;

            private static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration(
                        new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                        new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                        new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
                        new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 2));

            static public VertexDeclaration VertexDeclaration
            { get { return vertexDeclaration; } }
            VertexDeclaration IVertexType.VertexDeclaration
            { get { return vertexDeclaration; } }
        }

        // spawn stuff!
        private const float spawnConstant = 18.0f;  // higher means LESS!
        private const float spawnSettingFactor = 5.0f;  // remeber that high mass means mass_health=-1.0f

        /// <summary>
        /// vertexbuffer that holds all current spawn vertices - per spawn are 2 needed since they are rendered als tiny lines (pixels are not allowed in xna)
        /// </summary>
        private DynamicVertexBuffer spawnVertexBuffer;

        /// <summary>
        /// Buffer for accumulating new vertices
        /// after finished updating all new particles will be writtten to the spawnVertexBuffer
        /// </summary>
        private SpawnVertex[] spawnVerticesRAMBuffer = new SpawnVertex[maxSpawnsPerFrame*2];

        #endregion

        #endregion

        #region Colors

        public readonly static Color[] Colors = { Color.Red, Color.Blue, Color.Yellow, Color.Green, /*Color.Black,*/ Color.Pink, Color.Orange  };
        public readonly static Color[] ParticleColors = { new Color(240, 80, 70), new Color(75, 95, 220), new Color(250, 216, 50), new Color(80, 200, 80),/* Color.DarkSlateGray, */Color.HotPink, new Color(250, 120, 20) };
        public readonly static string[] ColorNames = { "Red", "Blue", "Yellow", "Green", /*"Black", */"Pink", "Orange" };

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

        private int colorIndex;
        public Color Color
        { get { return Colors[colorIndex]; } }
        public Color ParticleColor
        { get { return ParticleColors[colorIndex]; } }

        #endregion

        #region Control

        static public readonly String[] ControlNames = new String[]
        {
            "WASD + SHIFT",
            "Arrow Keys + CTRL",
            "Gamepad 1",
            "Gamepad 2",
            "Gamepad 3",
            "Gamepad 4",
            "No Control"
        };


        public InputManager.ControlType Controls
        {
            get { return InputManager.Instance.getControlType(playerIndex); }
            set { InputManager.Instance.setControlType(playerIndex, value); }
        }

        #endregion

        #region cursor
        /// <summary>
        /// Position to wich the particles are attracted
        /// </summary>
        public Vector2 ParticleAttractionPosition
        {
            get { return particleAttractionPosition; }
        }
        private Vector2 particleAttractionPosition = Vector2.Zero;

        /// <summary>
        /// position of the particle cursor (under direct player control)
        /// </summary>
        public Vector2 CursorPosition
        {
            get { return cursorPosition; }
        }
        private Vector2 cursorPosition;
        private readonly static Vector2[] cursorStartPositions =
            {
                new Vector2(0.2f, Level.RELATIVE_MAX.Y-0.2f),
                new Vector2(Level.RELATIVE_MAX.X-0.2f, 0.2f),
                new Vector2(Level.RELATIVE_MAX.X-0.2f, Level.RELATIVE_MAX.Y-0.2f),
                new Vector2(0.2f, 0.2f)
            };

        /// <summary>
        /// movementspeed of the cursor
        /// </summary>
        private const float CURSOR_SPEED = 1.0f;

        #endregion

        #region item

        public Item.ItemType ItemSlot { get; set; }

        #endregion

        #region index/identifier

        // who is who (blue etc.)
        public readonly PlayerIndex playerIndex;
        public int Index { get { return (int)playerIndex; } }

        #endregion

        #region random

        readonly Random random = new Random();
        private const float NoiseToMovementFactor = 15 * disciplinConstant;

        #endregion

        #region life and lifetime

        public bool Alive
        { get { return alive; } }
        private bool alive = true;
        public float TimeDead
        { get; private set; }

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

        #endregion

        public Player(int playerIndex, int virusTypeIndex, GraphicsDevice device, ContentManager content, Texture2D noiseTexture, int colorIndex)
        {
            this.playerIndex = (PlayerIndex)playerIndex;
            this.noiseTexture = noiseTexture;
            this.colorIndex = colorIndex;
            this.virusIndex = virusTypeIndex;

            this.ItemSlot = global::ParticleStormControl.Item.ItemType.NONE;

            cursorPosition = cursorStartPositions[(int)playerIndex];

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
            particleProcessing.Parameters["halfPixelCorrection"].SetValue(new Vector2(-0.5f / maxParticlesSqrt, 0.5f / maxParticlesSqrt));
            particleProcessing.Parameters["RelativeCorMax"].SetValue(Level.RELATIVE_MAX);

            // reset data
            InfoTexture.SetData<HalfVector2>(particleInfos);

            // spawn vb
            spawnVertexBuffer = new DynamicVertexBuffer(device, SpawnVertex.VertexDeclaration, 
                                                            maxSpawnsPerFrame * 2, BufferUsage.WriteOnly);
        }

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

            Item.ItemType item = player1.ItemSlot;
            player1.ItemSlot = player2.ItemSlot;
            player2.ItemSlot = item;
        }

        public void UpdateGPUPart(GraphicsDevice device, float timeInterval, Texture2D damageMapTexture)
        {
            // update spawn vb if necessary
            if(numSpawns > 0)
                spawnVertexBuffer.SetData<SpawnVertex>(spawnVerticesRAMBuffer, 0, numSpawns*2);

            device.Textures[0] = null;
            device.Textures[1] = null;
            device.Textures[2] = null;
            device.Textures[3] = null;

            device.SetRenderTargets(renderTargetBindings[currentTargetIndex]);

            #region PROCESS

            particleProcessing.Parameters["Positions"].SetValue(PositionTexture);
            particleProcessing.Parameters["Movements"].SetValue(MovementTexture);
            particleProcessing.Parameters["Infos"].SetValue(InfoTexture);

            particleProcessing.Parameters["particleAttractionPosition"].SetValue(particleAttractionPosition);
            particleProcessing.Parameters["MovementChangeFactor"].SetValue(disciplinConstant * timeInterval);
            particleProcessing.Parameters["TimeInterval"].SetValue(timeInterval);
            particleProcessing.Parameters["DamageMap"].SetValue(damageMapTexture);
            particleProcessing.Parameters["DamageFactor"].SetValue(DamageMapMask[Index] * (attackingPerSecond * timeInterval * 255));

            particleProcessing.Parameters["NoiseToMovementFactor"].SetValue(timeInterval * NoiseToMovementFactor);
            particleProcessing.Parameters["NoiseTexture"].SetValue(noiseTexture);


            device.BlendState = BlendState.Opaque;
            particleProcessing.CurrentTechnique = particleProcessing.Techniques[0];
            particleProcessing.CurrentTechnique.Passes[0].Apply();
            ScreenTriangleRenderer.instance.DrawScreenAlignedTriangle(device);
            #endregion

            #region spawn

            if(numSpawns > 0)
            {
                particleProcessing.CurrentTechnique = particleProcessing.Techniques[1];
                particleProcessing.CurrentTechnique.Passes[0].Apply();
                device.SetVertexBuffer(spawnVertexBuffer);
                device.DrawPrimitives(PrimitiveType.LineList, 0, numSpawns);
            }

            #endregion

            int target = currentTargetIndex;
            currentTargetIndex = currentTextureIndex;
            currentTextureIndex = target;
        }

        public void ReadGPUResults()
        {
            InfoTexture.GetData<HalfVector2>(particleInfos);
        }

        public void UpdateCPUPart(float timeInterval, IList<SpawnPoint> spawnPoints, bool cantDie)
        {
            float health = ((Mass_health * 0.5f) + 1.5f) * healthConstant;
            float speed = speedConstant + speedSettingFactor * Disciplin_speed;

            // compute spawnings
            numSpawns = 0;
            int numSpawnPoints = 0;
            foreach (SpawnPoint spawn in spawnPoints)
            {
                if (spawn.PossessingPlayer == (int)playerIndex)
                {
                    spawn.SpawnTimeAccum += timeInterval;
                    float f = spawn.SpawnSize / (spawnConstant + spawnSettingFactor * Mass_health);
                    int numSpawned = (int)(spawn.SpawnTimeAccum * f);

                    if (numSpawned > 0)
                    {
                        spawn.SpawnTimeAccum -= numSpawned / f; // don't miss anything!
                        for (int i = 0; i < numSpawned; ++i)
                        {
                            if (numSpawns == maxSpawnsPerFrame || NumParticlesAlive + numSpawns == maxParticles-1)
                                break;

                            // random movement
                            Vector2 movement = new Vector2((float)Random.NextDouble() - 0.5f, (float)Random.NextDouble() - 0.5f);
                            movement.Normalize();

                            // add only the first vertex, second is copied later!
                            int vertexIndex = numSpawns * 2;
                            spawnVerticesRAMBuffer[vertexIndex].particlePosition = spawn.Position;
                            spawnVerticesRAMBuffer[vertexIndex].movement = movement;
                            spawnVerticesRAMBuffer[vertexIndex].damageSpeed.X = health;
                            spawnVerticesRAMBuffer[vertexIndex].damageSpeed.Y = speed;
                            ++numSpawns;
                        }
                    }
                    ++numSpawnPoints;
                }
            }

            // find places for spawning and check if there are any particles
            // seperate loop for faster iterating!
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
                    float x = (float)(i % maxParticlesSqrt) / maxParticlesSqrt;
                    float y = (float)(i / maxParticlesSqrt) / maxParticlesSqrt;
                    spawnVerticesRAMBuffer[currentSpawn*2].texturePosition = new Vector2(x * 2.0f - 1.0f, (1.0f - y) * 2.0f - 1.0f);
                    spawnVerticesRAMBuffer[currentSpawn * 2 + 1] = spawnVerticesRAMBuffer[currentSpawn * 2]; // copytime!
                    spawnVerticesRAMBuffer[currentSpawn * 2 + 1].texturePosition.X += 0.5f / maxParticlesSqrt;

                    ++currentSpawn;

                    ++NumParticlesAlive;
                    biggestAliveIndex = i;
                }
             /*   else
                {
                    break;
                } */
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
        public void UserControl(float frameTimeInterval, Level level)
        {
            Vector2 cursorMove = InputManager.Instance.GetMovement(playerIndex);
            cursorMove *= frameTimeInterval * CURSOR_SPEED;

            float len = cursorMove.Length();
            if (len > 1.0f) cursorMove /= len;
            cursorPosition += cursorMove;

            cursorPosition.X = MathHelper.Clamp(cursorPosition.X, 0.0f, Level.RELATIVE_MAX.X);
            cursorPosition.Y = MathHelper.Clamp(cursorPosition.Y, 0.0f, Level.RELATIVE_MAX.Y);

            // hold move
            if(!InputManager.Instance.HoldButtonPressed(playerIndex))
                particleAttractionPosition = cursorPosition;
            
            // action
            if (InputManager.Instance.ActionButtonPressed(playerIndex) && ItemSlot != Item.ItemType.NONE)
            {
                level.PlayerUseItem(this);
                ItemSlot = Item.ItemType.NONE;
            }
            
#if DEBUG
            // save particle textures on pressing space
            if (InputManager.Instance.PressedButton(Keys.Tab))
            {
                using (var file = new System.IO.FileStream("position target " + playerIndex + ".png", System.IO.FileMode.Create))
                    positionTargets[currentTargetIndex].SaveAsPng(file, maxParticlesSqrt, maxParticlesSqrt);
                using (var file = new System.IO.FileStream("info target " + playerIndex + ".png", System.IO.FileMode.Create))
                    infoTargets[currentTargetIndex].SaveAsPng(file, maxParticlesSqrt, maxParticlesSqrt);
                using (var file = new System.IO.FileStream("movement target " + playerIndex + ".png", System.IO.FileMode.Create))
                    movementTexture[currentTargetIndex].SaveAsPng(file, maxParticlesSqrt, maxParticlesSqrt);
            }
#endif
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
