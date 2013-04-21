using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ParticleStormControl
{
    public abstract class Player
    {
        public const int MAX_NUM_PLAYERS = 4;

        public enum Type
        {
            HUMAN,
            AI,
            NONE
        };

        protected VirusSwarm virusSwarm;

        #region Basic Properties

        public int Index { get { return playerIndex; } }
        public readonly int playerIndex;
        
        public VirusSwarm.VirusType Virus { get { return virusSwarm.Virus; } }

        public Item.ItemType ItemSlot { get; set; }

        #endregion

        #region Virus rendering access

        public Texture2D HealthTexture { get { return virusSwarm.HealthTexture; } }
        public Texture2D PositionTexture { get { return virusSwarm.PositionTexture; } }
        public int HighestUsedParticleIndex { get { return virusSwarm.HighestUsedParticleIndex; } }

        #endregion

        #region Color

        public readonly static Color[] Colors = { Color.Red, /*Color.Blue*/new Color(40, 50, 250), Color.DarkTurquoise, new Color(20, 148, 20),/* Color.Black,*/ Color.DeepPink, new Color(250, 120, 20) };
        public readonly static string[] ColorNames = { "Red", "Blue", "Turquoise", "Green", /*"Black",*/ "Pink", "Orange" };

        private int colorIndex;

        public Color Color
        { get { return Colors[colorIndex]; } }
        public Color ParticleColor
        { get { return VirusSwarm.ParticleColors[colorIndex]; } }

        #endregion

        #region Cursor(s)

        /// <summary>
        /// Position to wich the particles are attracted
        /// </summary>
        public Vector2 ParticleAttractionPosition
        {
            get { return particleAttractionPosition; }
        }
        protected Vector2 particleAttractionPosition = Vector2.Zero;

        /// <summary>
        /// position of the particle cursor (under direct player control)
        /// </summary>
        public Vector2 CursorPosition
        {
            get { return cursorPosition; }
        }
        protected Vector2 cursorPosition;

#if ALL_CURSORS_IN_CENTER_POSITION
        private readonly static Vector2[] cursorStartPositions =
            {
                new Vector2(Level.RELATIVE_MAX.X-0.2f, 0.2f),
                new Vector2(0.2f, Level.RELATIVE_MAX.Y-0.2f),
                 new Vector2(0.2f, 0.2f),
                 new Vector2(Level.RELATIVE_MAX.X-0.2f, Level.RELATIVE_MAX.Y-0.2f)
                //new Vector2(Level.RELATIVE_MAX.X/2, Level.RELATIVE_MAX.Y/2),
                //new Vector2(Level.RELATIVE_MAX.X/2, Level.RELATIVE_MAX.Y/2),
                //new Vector2(Level.RELATIVE_MAX.X/2, Level.RELATIVE_MAX.Y/2),
                //new Vector2(Level.RELATIVE_MAX.X/2, Level.RELATIVE_MAX.Y/2)
            };
#else
        private readonly static float cursor_offset_x = 0.2f;
        private readonly static float cursor_offset_y = 0.04f;
        private readonly static Vector2[] cursorStartPositions =
            {
                new Vector2(0.2f + cursor_offset_x, Level.RELATIVE_MAX.Y-0.2f - cursor_offset_y),
                new Vector2(Level.RELATIVE_MAX.X-0.2f - cursor_offset_x, 0.2f + cursor_offset_y),
                new Vector2(Level.RELATIVE_MAX.X-0.2f - cursor_offset_x, Level.RELATIVE_MAX.Y-0.2f- cursor_offset_y),
                new Vector2(0.2f + cursor_offset_x, 0.2f + cursor_offset_y)
            };
#endif
        /// <summary>
        /// movementspeed of the cursor
        /// </summary>
        protected const float CURSOR_SPEED = 1.0f;


        #endregion

        #region life and lifetime

        public bool Alive
        { get { return alive; } }
        private bool alive = true;
        
        public float TotalVirusHealth
        { get { return alive ? virusSwarm.TotalHealth : 0; } }

        public int NumParticlesAlive
        { get { return alive ? virusSwarm.NumParticlesAlive : 0; } }

        public int CurrentSpawnNumber
        { get { return virusSwarm.CurrentSpawnNumber; } }

        public float RemainingTimeAlive
        {
            get
            {
                if (timeWithoutSpawnPoint <= 0.0f)
                    return float.PositiveInfinity;
                else
                    return MAX_TIME_WITHOUT_SPAWNPOINT - timeWithoutSpawnPoint;
            }
        }

        private float timeWithoutSpawnPoint = 0.0f;
        public const float MAX_TIME_WITHOUT_SPAWNPOINT = 15.0f;

        #endregion

        #region SpawnPoints

        /// <summary>
        /// for the percentage bar
        /// </summary>
        public uint PossessingSpawnPoints { get; private set; }
        /// <summary>
        /// for the percentage bar
        /// </summary>
        public float PossessingSpawnPointsOverallSize { get; private set; }

        #endregion

        public Player(int playerIndex, int virusIndex, int colorIndex, GraphicsDevice device, ContentManager content, Texture2D noiseTexture)
        {
            this.playerIndex = playerIndex;
            this.colorIndex = colorIndex;
            this.ItemSlot = global::ParticleStormControl.Item.ItemType.NONE;

            cursorPosition = cursorStartPositions[Settings.Instance.GetPlayer(playerIndex).SlotIndex];

            virusSwarm = new VirusSwarm(virusIndex, colorIndex, device, content, noiseTexture);
        }

        /// <summary>
        /// performs a switch between 2 players
        /// </summary>
        public static void SwitchPlayer(Player player1, Player player2)
        {
            VirusSwarm.SwitchSwarm(player1.virusSwarm, player2.virusSwarm);

        /*    Item.ItemType item = player1.ItemSlot;
            player1.ItemSlot = player2.ItemSlot;
            player2.ItemSlot = item; */
        }
        

        public void UpdateGPUPart(GameTime gameTime, GraphicsDevice device, Texture2D damageMapTexture)
        {
            if (!alive)
                return;

            virusSwarm.UpdateGPUPart(device, (float)gameTime.ElapsedGameTime.TotalSeconds, damageMapTexture,
                                     particleAttractionPosition, playerIndex);
        }

        public void UpdateCPUPart(GameTime gameTime, IEnumerable<SpawnPoint> spawnPoints, bool cantDie)
        {
            if (!alive)
                return;

            var posessedSpawns = spawnPoints.Where(x => x.PossessingPlayer == playerIndex);
            virusSwarm.UpdateCPUPart((float)gameTime.ElapsedGameTime.TotalSeconds, posessedSpawns);

            if (virusSwarm.CurrentSpawnNumber == 0 && alive)
                alive = NumParticlesAlive > 0 || cantDie; // still alive *sing*

            PossessingSpawnPoints = (uint)posessedSpawns.Count();
            PossessingSpawnPointsOverallSize = posessedSpawns.Sum(x => x.Size);

            // alive due to number of spawnpoints?
            if (alive)
            {
                if (posessedSpawns.Count() == 0)
                {
                    timeWithoutSpawnPoint += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (timeWithoutSpawnPoint > MAX_TIME_WITHOUT_SPAWNPOINT)
                        alive = false;
                }
                else
                    timeWithoutSpawnPoint = 0.0f;
            }
        }

        public void ReadGPUResults()
        {
            virusSwarm.ReadGPUResults();
        }

        abstract public void UserControl(float frameTimeInterval, Level level);
 
    }
}
