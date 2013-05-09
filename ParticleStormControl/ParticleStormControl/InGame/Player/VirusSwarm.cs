using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ParticleStormControl
{
    /// <summary>
    /// Defines a virus swarm of a given type and color
    /// Every player posesses exactly one Swarm
    /// </summary>
    public class VirusSwarm
    {
        #region Virus Definitions

        public enum VirusType
        {
            H5N1,
            HEPATITISB,
            HIV,
            EPSTEINBARR,

            NUM_VIRUSES
        }

        public readonly static VirusType[] Viruses = { VirusType.H5N1, VirusType.HEPATITISB, VirusType.HIV, VirusType.EPSTEINBARR };
        public readonly static string[] VirusNames = { "Influenza A", "Hepatitis B", "Human immunodeficiency", "Epstein-Barr" };
        public readonly static string[] VirusAdditionalInfo = { "It is a highly contagious virus which is a potential pandemic threat.",
                                                               "Can also lead to cirrhosis and hepatocellular carcinoma.",
                                                               "Infection with HIV occurs by the transfer of blood, semen,\nvaginal fluid, pre-ejaculate, or breast milk.",
                                                               "It is one of the most common viruses in humans." };
        public readonly static string[] VirusClassification = { "Group V; Genus A; Family of Orthomyxoviridae",
                                                                  "Group VII; Genus Orthohepadnavirus; Family of Hepadnaviridae",
                                                                  "Group VI; Genus Lentivirus; Family of Retroviridae",
                                                                  "Group I; Genus of Lymphocryptovirus; Family of Herpesviridae" };
        public readonly static string[] VirusShortName = { "H5N1",
                                                             "HBV",
                                                             "HIV",
                                                             "EBV" };
        public readonly static string[] VirusCausedDisease = { "avian influenza (bird flu)",
                                                                 "hepatitis B",
                                                                 "acquired immunodeficiency syndrome (AIDS)",
                                                                 "Implicated in several diseases that include infectious mononucleosis,\nmultiple sclerosis and Hodgkin lymphoma." };


        // IMPORTANT: The number '+' for each virus should add to the same sum. This is to imply that all virusses are equally strong. ;)
        // Currently the sum is 10
        public static readonly string[] DESCRIPTOR_Mass = new string[] { "++", "++++", "+", "++++" };
        public static readonly string[] DESCRIPTOR_Speed = new string[] { "+++", "+", "++", "++++" };
        public static readonly string[] DESCRIPTOR_Health = new string[] { "+++", "+++", "++++", "+" };
        public static readonly string[] DESCRIPTOR_Discipline = new string[] { "++", "++", "+++", "+" };

        // attributs
        private static readonly float[] MASS_byVirus = new float[] { 0.5f, 0.63f, 0.075f, 1.0f };
        private static readonly float[] SPEED_byVirus = new float[] { 0.44f, -0.40f, 0.3f, 1.0f };
        private static readonly float[] HEALTH_byVirus = new float[] { 0.5f, 0.35f, 1.0f, -0.38f };
        private static readonly float[] DISCIPLIN_byVirus = new float[] { 0.44f, 0.25f, 0.65f, 0.55f };

        // speed stuff
        private const float SPEED_CONSTANT = 0.18f; // 0.13f;
        private const float SPEED_SETTING_FACTOR = 0.08f;

        public float Speed
        { get { return SPEED_CONSTANT + SPEED_SETTING_FACTOR * SPEED_byVirus[virusIndex]; } }

        // life stuff
        public const float HEALTH_CONSTANT = 15.0f;
        public const float healthSettingFactor = 15.0f;

        /// <summary>
        /// returns a mass constant that implies how many particles are spawned per base
        /// </summary>
        public float Mass
        { get { return -2 * MASS_byVirus[virusIndex]; } }

        // spawn stuff!
        private const float SPAWN_CONSTANT = 18.0f;  // higher means LESS!
        private const float SPAWN_SETTING_FACTOR = 5.0f;  // remeber that high mass means mass_health=-1.0f

        /// <summary>
        /// returns the health of new particles
        /// </summary>
        public float HealthStart
        { get { return ((HEALTH_byVirus[virusIndex] * 0.5f) + 1.5f) * HEALTH_CONSTANT; } }

        /// <summary>
        /// discilplin constant - higher means that the particles will move more straight in player's direction
        /// </summary>
        private const float DISCIPLIN_CONSTANT = 0.19f;// 0.15f;

        public float Disciplin
        { get { return 1 - DISCIPLIN_byVirus[virusIndex]; } }

        // attacking constant
        private const float ATTACKING_PER_SECOND = 30.0f * 255;

        #endregion

        #region Particles

        /// <summary>
        /// size of particle-data rendertargets and textures
        /// </summary>
        public const int MAX_PARTICLES_SQRT = 128;
        public const int MAX_PARTICLES = MAX_PARTICLES_SQRT * MAX_PARTICLES_SQRT;

        // info texture:
        // X: Health

        public Texture2D PositionTexture { get { return positionTargets[currentTextureIndex]; } }
        public Texture2D HealthTexture { get { return infoTargets[currentTextureIndex]; } }
        private Texture2D MovementTexture { get { return movementTexture[currentTextureIndex]; } }
        private Texture2D noiseTexture;

        private int currentTargetIndex = 0;
        private int currentTextureIndex = 1;
        private RenderTarget2D[] positionTargets = new RenderTarget2D[2];
        private RenderTarget2D[] infoTargets = new RenderTarget2D[2];
        private RenderTarget2D[] movementTexture = new RenderTarget2D[2];
        private RenderTargetBinding[][] renderTargetBindings;

        private float[] particleHealth = new float[MAX_PARTICLES_SQRT * MAX_PARTICLES_SQRT];

        private Effect particleProcessing;

        public int NumParticlesAlive
        { get; private set; }

        public int HighestUsedParticleIndex
        { get; private set; }

        #region spawning

        /// <summary>
        /// maximum number of particles spawned in a single frame
        /// </summary>
        private const int MAX_SPAWNS_PER_FRAME = 32;

        /// <summary>
        /// num spawns last frame
        /// </summary>
        private int currentSpawnNumber = 0;

        public int CurrentSpawnNumber
        { get { return currentSpawnNumber; } }

        /// <summary>
        /// vertex for a particle spawn
        /// </summary>
        public struct SpawnVertex : IVertexType
        {
            public Vector2 texturePosition;
            public Vector2 particlePosition;
            public Vector2 movement;
            // public Vector2 damageSpeed;
            public float health;

            private static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration(
                        new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                        new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                        new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
                        new VertexElement(24, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2));

            static public VertexDeclaration VertexDeclaration
            { get { return vertexDeclaration; } }
            VertexDeclaration IVertexType.VertexDeclaration
            { get { return vertexDeclaration; } }
        }

        /// <summary>
        /// vertexbuffer that holds all current spawn vertices - per spawn are 2 needed since they are rendered als tiny lines (pixels are not allowed in xna)
        /// </summary>
        private DynamicVertexBuffer spawnVertexBuffer;

        /// <summary>
        /// Buffer for accumulating new vertices
        /// after finished updating all new particles will be writtten to the spawnVertexBuffer
        /// </summary>
        private SpawnVertex[] spawnVerticesRAMBuffer = new SpawnVertex[MAX_SPAWNS_PER_FRAME * 2];

        #endregion

        #endregion

        #region Particle Color Definition

        public readonly static Color[] ParticleColors = { new Color(240, 80, 70), new Color(60, 70, 240), new Color(42, 216, 221), new Color(80, 200, 80), /*Color.DarkSlateGray,*/ Color.DeepPink, new Color(250, 120 + 60, 20 + 30) };

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

        #region Basic Swarm Properties

        private readonly int virusIndex;
        public int VirusIndex
        {
            get { return virusIndex; }
        }
        public VirusType Virus
        { get { return Viruses[virusIndex]; } }


        public float TotalHealth { get { return totalHealth; } }
        private float totalHealth = 0.0f;

        #endregion

        public VirusSwarm(int virusIndex, int colorIndex, GraphicsDevice device, ContentManager content, Texture2D noiseTexture)
        {
            this.noiseTexture = noiseTexture;
            this.virusIndex = virusIndex;

            for (int i = 0; i < MAX_PARTICLES; ++i)
                particleHealth[i] = -1.0f;

            // create rendertargets (they are pingponging ;) )
            positionTargets[0] = new RenderTarget2D(device, MAX_PARTICLES_SQRT, MAX_PARTICLES_SQRT, false, SurfaceFormat.HalfVector2, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            positionTargets[1] = new RenderTarget2D(device, MAX_PARTICLES_SQRT, MAX_PARTICLES_SQRT, false, SurfaceFormat.HalfVector2, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            infoTargets[0] = new RenderTarget2D(device, MAX_PARTICLES_SQRT, MAX_PARTICLES_SQRT, false, SurfaceFormat.Single, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            infoTargets[1] = new RenderTarget2D(device, MAX_PARTICLES_SQRT, MAX_PARTICLES_SQRT, false, SurfaceFormat.Single, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            movementTexture[0] = new RenderTarget2D(device, MAX_PARTICLES_SQRT, MAX_PARTICLES_SQRT, false, SurfaceFormat.HalfVector2, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            movementTexture[1] = new RenderTarget2D(device, MAX_PARTICLES_SQRT, MAX_PARTICLES_SQRT, false, SurfaceFormat.HalfVector2, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            renderTargetBindings = new RenderTargetBinding[][] { new RenderTargetBinding[] { positionTargets[0], movementTexture[0], infoTargets[0] }, 
                                                                new RenderTargetBinding[] { positionTargets[1], movementTexture[1], infoTargets[1] } };
            particleProcessing = content.Load<Effect>("shader/particleProcessing");
            particleProcessing.Parameters["halfPixelCorrection"].SetValue(new Vector2(-0.5f / MAX_PARTICLES_SQRT, 0.5f / MAX_PARTICLES_SQRT));
            particleProcessing.Parameters["RelativeCorMax"].SetValue(Level.RELATIVE_MAX);

            // reset data
            HealthTexture.SetData<float>(particleHealth);

            // spawn vb
            spawnVertexBuffer = new DynamicVertexBuffer(device, SpawnVertex.VertexDeclaration, 
                                                            MAX_SPAWNS_PER_FRAME * 2, BufferUsage.WriteOnly);
        }

        ~VirusSwarm()
        {
            positionTargets[0].Dispose();
            positionTargets[1].Dispose();
            infoTargets[0].Dispose();
            infoTargets[1].Dispose();
            movementTexture[0].Dispose();
            movementTexture[1].Dispose();
            //particleProcessing.Dispose();
        }

        public static void SwitchSwarm(VirusSwarm player1, VirusSwarm player2)
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

            float[] vh4 = player1.particleHealth;
            player1.particleHealth = player2.particleHealth;
            player2.particleHealth = vh4;

            i = player1.HighestUsedParticleIndex;
            player1.HighestUsedParticleIndex = player2.HighestUsedParticleIndex;
            player2.HighestUsedParticleIndex = i;
        }

        public void UpdateGPUPart(GraphicsDevice device, float timeInterval, Texture2D damageMapTexture,
                                    Vector2 particleAttractionPosition, int playerIndex)
        {
            device.SetVertexBuffer(null);
            
            // update spawn vb if necessary
            if (currentSpawnNumber > 0)
                spawnVertexBuffer.SetData<SpawnVertex>(spawnVerticesRAMBuffer, 0, currentSpawnNumber * 2);

            device.Textures[0] = null;
            device.Textures[1] = null;
            device.Textures[2] = null;
            device.Textures[3] = null;

            device.SetRenderTargets(renderTargetBindings[currentTargetIndex]);

            #region PROCESS

            particleProcessing.Parameters["Positions"].SetValue(PositionTexture);
            particleProcessing.Parameters["Movements"].SetValue(MovementTexture);
            particleProcessing.Parameters["Health"].SetValue(HealthTexture);

            particleProcessing.Parameters["particleAttractionPosition"].SetValue(particleAttractionPosition);
            particleProcessing.Parameters["MovementChangeFactor"].SetValue(DISCIPLIN_CONSTANT * timeInterval / (Disciplin * 0.1f));
            particleProcessing.Parameters["TimeInterval"].SetValue(timeInterval);
            particleProcessing.Parameters["DamageMap"].SetValue(damageMapTexture);
            particleProcessing.Parameters["DamageFactor"].SetValue(DamageMapMask[playerIndex] * (ATTACKING_PER_SECOND * timeInterval));

            particleProcessing.Parameters["MovementFactor"].SetValue(Speed * timeInterval);

            particleProcessing.Parameters["NoiseToMovementFactor"].SetValue(timeInterval * Disciplin);
            particleProcessing.Parameters["NoiseTexture"].SetValue(noiseTexture);

            particleProcessing.Parameters["MaxHealth"].SetValue(HealthStart);


            device.BlendState = BlendState.Opaque;
            particleProcessing.CurrentTechnique = particleProcessing.Techniques[0];
            particleProcessing.CurrentTechnique.Passes[0].Apply();
            ScreenTriangleRenderer.instance.DrawScreenAlignedTriangle(device);
            #endregion

            #region spawn

            if (currentSpawnNumber > 0)
            {
                particleProcessing.CurrentTechnique = particleProcessing.Techniques[1];
                particleProcessing.CurrentTechnique.Passes[0].Apply();
                device.SetVertexBuffer(spawnVertexBuffer);
                device.DrawPrimitives(PrimitiveType.LineList, 0, currentSpawnNumber);
            }

            #endregion

            int target = currentTargetIndex;
            currentTargetIndex = currentTextureIndex;
            currentTextureIndex = target;
        }

        /* private bool IsAlive(int particleIndex)
        {
            // halffloat2
            //return (particleHealth[particleIndex].PackedValue & (((UInt32)1) << 15)) == 0;
            // save version - use this temporary in case of bad xbox behaviour
            // return particleHealth[particleIndex].ToVector4().Z < 0;

            return particleHealth[particleIndex] > 0;
        }*/

        public void ReadGPUResults()
        {
            HealthTexture.GetData<float>(particleHealth);   // TODO optimize: don't need to take ALL health values!
        }

        /// <summary>
        /// updates all cpu related particle stuff
        /// </summary>
        /// <param name="timeInterval">time since last frame in seconds</param>
        /// <param name="spawnPoints">list of posessed spawnpoints</param>
        /// <returns>number of spawns in this frame</returns>
        public int UpdateCPUPart(float timeInterval, IEnumerable<SpawnPoint> spawnPoints)
        {
            // compute spawnings
            currentSpawnNumber = 0;
            foreach (SpawnPoint spawn in spawnPoints)
            {
                spawn.SpawnTimeAccum += timeInterval;
                float f = spawn.SpawnSize / (SPAWN_CONSTANT + SPAWN_SETTING_FACTOR * Mass);//Mass_health);
                int spawnPointSpawns = (int)(spawn.SpawnTimeAccum * f);

                if (spawnPointSpawns > 0)
                {
                    spawn.SpawnTimeAccum -= spawnPointSpawns / f; // don't miss anything!
                    for (int i = 0; i < spawnPointSpawns; ++i)
                    {
                        if (currentSpawnNumber == MAX_SPAWNS_PER_FRAME || NumParticlesAlive + currentSpawnNumber == MAX_PARTICLES - 2)
                            break;

                        // add only the first vertex, second is copied later!
                        int vertexIndex = currentSpawnNumber * 2;
                        spawnVerticesRAMBuffer[vertexIndex].particlePosition = spawn.Position;
                        spawnVerticesRAMBuffer[vertexIndex].movement = Random.NextDirection();
                        spawnVerticesRAMBuffer[vertexIndex].health = HealthStart;
                        ++currentSpawnNumber;
                    }
                }
            }

            // find places for spawning and check if there are any particles
            // seperate loop for faster iterating!
            totalHealth = 0.0f;
            int biggestAliveIndex = 0;
            NumParticlesAlive = 0;
            int numAlreadySpawned = 0;
            const float lineOffset = 0.5f / MAX_PARTICLES_SQRT;
            int imax = (int)MathHelper.Clamp(HighestUsedParticleIndex + currentSpawnNumber + 1, 0, MAX_PARTICLES);
            for (int i = 0; i < imax; ++i)
            {
                float currentParticleHealth = particleHealth[i];
                if (currentParticleHealth > 0.0f)
                {
                    totalHealth += currentParticleHealth;
                    ++NumParticlesAlive;
                    biggestAliveIndex = i;
                }
                else if (numAlreadySpawned < currentSpawnNumber)
                {
                    float x = (float)(i % MAX_PARTICLES_SQRT) / MAX_PARTICLES_SQRT;
                    float y = (float)(i / MAX_PARTICLES_SQRT) / MAX_PARTICLES_SQRT;
                    spawnVerticesRAMBuffer[numAlreadySpawned * 2].texturePosition = new Vector2(x * 2.0f - 1.0f - lineOffset, 
                                                                                                (1.0f - y) * 2.0f - 1.0f);
                    spawnVerticesRAMBuffer[numAlreadySpawned * 2 + 1] = spawnVerticesRAMBuffer[numAlreadySpawned * 2]; // copytime
                    spawnVerticesRAMBuffer[numAlreadySpawned * 2 + 1].texturePosition.X += lineOffset;

                    totalHealth += HealthStart;
                    ++numAlreadySpawned;
                    ++NumParticlesAlive;
                    biggestAliveIndex = i;
                }
            }
            HighestUsedParticleIndex = biggestAliveIndex;
            //System.Diagnostics.Debug.Assert(numAlreadySpawned == currentSpawnNumber);
            currentSpawnNumber = numAlreadySpawned;
            return currentSpawnNumber;
        }
    }
}
