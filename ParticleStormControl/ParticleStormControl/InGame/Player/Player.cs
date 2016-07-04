using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace VirusX
{
    abstract class Player
    {
        public enum Type
        {
            HUMAN,
            AI,
            NONE
        };

        public enum Teams
        {
            // capture the cell
            DEFENDER,
            ATTACKER,

            // 2 vs 2
            LEFT,
            RIGHT,
            
            // death match
            NONE,

            NUM_TEAMS
        };

        static public String[] TEAM_NAMES
        {
            get
            {
                return new String[]
                {
                    VirusXStrings.Instance.TeamNameDefender,
                    VirusXStrings.Instance.TeamNameAttacker,
                    VirusXStrings.Instance.TeamNameLeft,
                    VirusXStrings.Instance.TeamNameRight,
                    VirusXStrings.Instance.TeamNameNone
                };
            }
        }

        protected VirusSwarm virusSwarm;

        #region Basic Properties

        public int Index { get { return playerIndex; } }
        public readonly int playerIndex;
        
        public VirusSwarm.VirusType Virus { get { return virusSwarm.Virus; } }

        public Teams Team { get { return team; } set { team = value; } }
        private Teams team;

        #endregion

        #region Virus rendering access

        // see VirusSwarm for descriptions

        public Texture2D HealthTexture { get { return virusSwarm.HealthTexture; } }
        public Texture2D PositionTexture { get { return virusSwarm.PositionTexture; } }
        public int HighestUsedParticleIndex { get { return virusSwarm.HighestUsedParticleIndex; } }

        public Color DamageMapDrawColor { get { return virusSwarm.DamageMapDrawColor; } }
        public Vector4 DamageMapMask { get { return virusSwarm.DamageMapMask; } }

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


        private Crosshair crosshair;

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
        protected readonly static float cursor_offset_x = 0.2f;
        protected readonly static float cursor_offset_y = 0.04f;
        protected readonly static Vector2[] cursorStartPositions =
            {
                new Vector2(0.2f + cursor_offset_x, Level.RELATIVE_MAX.Y-0.2f - cursor_offset_y),
                new Vector2(Level.RELATIVE_MAX.X-0.2f - cursor_offset_x, 0.2f + cursor_offset_y),
                new Vector2(Level.RELATIVE_MAX.X-0.2f - cursor_offset_x, Level.RELATIVE_MAX.Y-0.2f- cursor_offset_y),
                new Vector2(0.2f + cursor_offset_x, 0.2f + cursor_offset_y)
            };

        protected readonly static Vector2[] cursorStartPositionsCTC =
            {
                new Vector2(Level.RELATIVE_MAX.X / 2, 0.45f),  // THE CELL
                new Vector2(Level.RELATIVE_MAX.X - MapGenerator.LEVEL_BORDER - 0.2f - (cursor_offset_x * 0.5f), MapGenerator.LEVEL_BORDER+0.1f - cursor_offset_y),  // upper right
                new Vector2(Level.RELATIVE_MAX.X / 2, Level.RELATIVE_MAX.Y - MapGenerator.LEVEL_BORDER+0.1f - (cursor_offset_y * 2f)), // lower cell
                new Vector2(MapGenerator.LEVEL_BORDER + 0.2f + (cursor_offset_x * 0.5f), MapGenerator.LEVEL_BORDER+0.1f - cursor_offset_y) // upper left
            };
#endif
        /// <summary>
        /// movementspeed of the cursor
        /// </summary>
        public static readonly float CURSOR_SPEED = 1.0f;


        #endregion

        #region life and lifetime

        public bool Alive
        {
            get { return alive; }
            set {
                alive = value;
                if (!value)
                    AudioManager.Instance.PlaySoundeffect("death");
            }
        }
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

        #region Items

        Item.ItemType itemSlot = Item.ItemType.NONE; 
        /// <summary>
        /// The item a player currently possesses
        /// </summary>
        public Item.ItemType ItemSlot
        {
            get { return itemSlot; }
            set
            {
                currentItemPossessionTime = 0f;
                itemSlot = value;
            }
        }
        
        /// <summary>
        /// The time a player can possess an item, if the automatic deletion is active
        /// </summary>
        private float maxItemPossessionTime = 15f;
        /// <summary>
        /// The time how long a player possess an item
        /// </summary>
        private float currentItemPossessionTime = 0f;

        private float itemBlinkTimePercentage = 0.75f;
        private float itemAlphaValue = 1f;
        public float ItemAlphaValue 
        {
            get
            {
                return itemAlphaValue;
            }
        }

        #endregion

        public Player(int playerIndex, VirusSwarm.VirusType virusIndex, int colorIndex, Teams team, InGame.GameMode gameMode,
                        GraphicsDevice device, ContentManager content, Texture2D noiseTexture)
        {
            this.playerIndex = playerIndex;
            this.colorIndex = colorIndex;
            this.team = team;
            this.ItemSlot = global::VirusX.Item.ItemType.NONE;

            // place cursor
            if (gameMode == InGame.GameMode.CAPTURE_THE_CELL || gameMode == InGame.GameMode.ARCADE)
                cursorPosition = cursorStartPositionsCTC[Settings.Instance.GetPlayer(playerIndex).SlotIndex];
            else
                cursorPosition = cursorStartPositions[Settings.Instance.GetPlayer(playerIndex).SlotIndex];

            // create virusswarm
            List<int> friendlyPlayers = new List<int>();
            if (team != Teams.NONE)
            {
                for (int player = 0; player < Settings.Instance.NumPlayers; ++player)
                {
                    if (Settings.Instance.GetPlayer(player).Team == team)
                        friendlyPlayers.Add(player);
                }
            }
            virusSwarm = new VirusSwarm(virusIndex, playerIndex, friendlyPlayers, device, content, noiseTexture);

            // create crosshair
            crosshair = new Crosshair(content);
        }

        /// <summary>
        /// performs a switch between 2 players
        /// </summary>
        public static void SwitchPlayer(Player player1, Player player2)
        {
            // if one of the player isn't alive, do nothing
            // this shouldn't happen at all, but just to be safe..
            if (!player1.Alive || !player2.Alive)
            {
#if DEBUG
                throw new Exception("SwitchPlayer: One of the player isn't alive!");
#else
                return;
#endif
            }

            // swarm posession switch
            VirusSwarm.SwitchSwarm(player1.virusSwarm, player2.virusSwarm);

            // reset time since no spawnpoint to zero, just to be fair
            player1.timeWithoutSpawnPoint = 0.0f;
            player2.timeWithoutSpawnPoint = 0.0f;
        }
        

        public void UpdateGPUPart(GameTime gameTime, GraphicsDevice device, Texture2D damageMapTexture)
        {
            if (!alive)
                return;

            virusSwarm.UpdateGPUPart(device, (float)gameTime.ElapsedGameTime.TotalSeconds, damageMapTexture, particleAttractionPosition);
        }

        /// <summary>
        /// performs all cpu updates
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spawnPoints"></param>
        public void UpdateCPUPart(GameTime gameTime, IEnumerable<SpawnPoint> spawnPoints)
        {
            crosshair.Update(gameTime, this);

            if (!alive)
                return;

            var posessedSpawns = spawnPoints.Where(x => x.PossessingPlayer == playerIndex);
            int numOwnedSpawnPoints = posessedSpawns.Count();
            virusSwarm.UpdateCPUPart((float)gameTime.ElapsedGameTime.TotalSeconds, posessedSpawns);

            // dead due to particle loss
            if (numOwnedSpawnPoints == 0 && NumParticlesAlive == 0)
                Alive = false;

            PossessingSpawnPoints = (uint)posessedSpawns.Count();
            PossessingSpawnPointsOverallSize = posessedSpawns.Sum(x => x.Size);

            // alive due to number of spawnpoints?
            if (numOwnedSpawnPoints == 0 && Settings.Instance.GameMode != InGame.GameMode.ARCADE)
            {
                timeWithoutSpawnPoint += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (timeWithoutSpawnPoint > MAX_TIME_WITHOUT_SPAWNPOINT)
                    Alive = false;
            }
            else
                timeWithoutSpawnPoint = 0.0f;

            // loose item
            if (Settings.Instance.AutomaticItemDeletion)
            {
                if (ItemSlot != Item.ItemType.NONE)
                {
                    currentItemPossessionTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (currentItemPossessionTime > itemBlinkTimePercentage * maxItemPossessionTime)
                    {
                        itemAlphaValue = ((float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 10f)*0.4f + 0.5f);
                    }
                    else itemAlphaValue = 1f;

                    if (currentItemPossessionTime >= maxItemPossessionTime)
                    {
                        ItemSlot = Item.ItemType.NONE;
                        itemAlphaValue = 1f;
                    }
                }
            }
        }

        public void ReadGPUResults()
        {
            virusSwarm.ReadGPUResults();
        }

        abstract public void UserControl(float frameTimeInterval, Level level);


        public void DrawCrosshairDamageMap(SpriteBatch spriteBatch)
        {
            crosshair.DrawToDamageMap(spriteBatch, DamageMapDrawColor);
        }

        public void DrawCrosshairAlphaBlended(SpriteBatch spriteBatch, Level level, GameTime gameTime)
        {
            crosshair.Draw_AlphaBlended(spriteBatch, level, gameTime, Color);
        }
    }
}
