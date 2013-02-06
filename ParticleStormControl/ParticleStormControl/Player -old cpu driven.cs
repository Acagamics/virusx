//#define PLAYER_ZERO_IS_CHEATING

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ZoneControl
{
    public class Player
    {
        public const int MaxNumPlayers = 4;

        #region agents

        public struct Agent
        {
            public Vector2 position;
            public Vector2 movement;
            public float speed;
            public float health; // negative infinity is dead, everything else too but not "deleted" yet

            public int next; // -1 is invalid, next dead if dead
        };

        private int currentMaxAgentIndex = 0;

        // use THIS ONE for iterating on alive agents - attention, there are dead ones
        public int CurrentMaxAgentIndex
        {
            get { return currentMaxAgentIndex; }
        }

        public const int maxAgents = 5000;


        public int NumAlive { get; private set; }
        public float OverallHealth { get; private set; }

        
        public Agent[] AgentList
        {
            get { return agentList; }
            set { agentList = value; }
        }
        private Agent[] agentList;

        public uint SpawnedPerFrame { get; private set; }

        #endregion

        #region Colors

        public readonly static Color[] Colors = { Color.Red, Color.Blue, Color.Yellow, Color.Green };
        public readonly static Color[] ParticleColor = { new Color(240, 80, 70), new Color(75, 95, 220), new Color(250, 216, 50), new Color(80, 200, 80) };
        public readonly static string[] Names = { "Red", "Blue", "Yellow", "Green" };

#if XBOX
        public readonly static Color[] TextureDamageValue = {
                                                       new Color(0, 0, 0, 1),
                                                       new Color(0, 0, 1, 0),
                                                       new Color(0, 1, 0, 0),
                                                       new Color(1, 0, 0, 0)
                                                            };
#else
        public readonly static Color[] TextureDamageValue = {
                                                       new Color(1, 0, 0, 0),
                                                       new Color(0, 1, 0, 0),
                                                       new Color(0, 0, 1, 0),
                                                       new Color(0, 0, 0, 1)
                                                            };
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

        public ControlType Controls { get; set; }

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

        // spawn stuff!
        private const float spawnConstant = 12.0f;  // higher means LESS!
        private const float spawnSettingFactor = 5.0f;  // remeber that high mass means mass_health=-1.0f
        private int nextSpawnIndex = 0;

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
        public const float minHealth = 5.0f;    // particles below this health-treshold will be declared dead!

        // disciplin stuff -- implicated by speed
        private const float disciplinConstant = 0.007f;
        #endregion

        // inflicted damage
        float sumDamage;
        public float SumDamage
        {
            get { return sumDamage; }
        }

        private const float attackingPerSecond = 30.0f;

        #region stuff about randomization

        readonly Random random = new Random();
        private float randomTimeAccum = 0.0f;
        private const float RandomForcesPercentagePerSecond = 0.2f;
        private const float RandomForceStrength = 1.0f*disciplinConstant;

        #endregion

        public bool Alive
        { get { return alive; } }
        private bool alive = true;
        public float TimeDead
        { get; private set; }

        private float timeWithoutSpawnPoint = 0.0f;
        private const float maxTimeWithoutSpawnPoint = 15.0f;

        /// <summary>
        /// performs a switch between 2 players
        /// </summary>
        public static void SwitchPlayer(Player player1, Player player2)
        {
            // agents
            Agent[] agentArray1 = player1.AgentList;
            player1.AgentList = player2.AgentList;
            player2.AgentList = agentArray1;

            int i = player1.currentMaxAgentIndex;
            player1.currentMaxAgentIndex = player2.currentMaxAgentIndex;
            player2.currentMaxAgentIndex = i;

            i = player1.nextSpawnIndex;
            player1.nextSpawnIndex = player2.nextSpawnIndex;
            player2.nextSpawnIndex = i;

            int ui = player1.NumAlive;
            player1.NumAlive = player2.NumAlive;
            player2.NumAlive = ui;

            float f = player1.OverallHealth;
            player1.OverallHealth = player2.OverallHealth;
            player2.OverallHealth = f;

            f = player1.sumDamage;
            player1.sumDamage = player2.sumDamage;
            player2.sumDamage = f;
        }

        public Player(int playerIndex)
        {
            this.playerIndex = playerIndex;
            cursorPosition = cursorStartPositions[playerIndex];

            agentList = new Agent[maxAgents];
            for (int i = 0; i < maxAgents - 1; ++i)
            {
                agentList[i].next = i + 1;
                agentList[i].health = float.NegativeInfinity;  // officially dead marked
            }
            agentList[maxAgents - 1].next = -1;

            OverallHealth = 0.0f;
        }

        /// <summary>
        /// performs particle-spawn operations
        /// </summary>
        public void spawn(float timeInterval, IList<MapObject> mapObjects)
        {
            float health = ((mass_health * 0.5f) + 1.5f) * healthConstant;
            float speed = speedConstant + speedSettingFactor * disciplin_speed;

            // own spawnpoint
            SpawnedPerFrame = 0;
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
                            spawnAgent(health, speed, spawn.Position);
                    }
                    ++numSpawnPoints;
                }
            }
            UpdateAliveStatus(timeInterval, numSpawnPoints);
        }

        /// <summary>
        /// spawns a single particle
        /// </summary>
        private void spawnAgent(float health, float speed, Vector2 position)
        {
            if (nextSpawnIndex < 0)
                return;

            agentList[nextSpawnIndex].health = health;
            OverallHealth += health;

            agentList[nextSpawnIndex].movement = new Vector2((float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f);
            agentList[nextSpawnIndex].movement.Normalize();
            agentList[nextSpawnIndex].speed = speed;
            agentList[nextSpawnIndex].position = position;

            // currently dead index is in next!
            nextSpawnIndex = agentList[nextSpawnIndex].next;

            currentMaxAgentIndex = Math.Max(nextSpawnIndex, currentMaxAgentIndex);
            ++NumAlive;
            ++SpawnedPerFrame;
        }

        /// <summary>
        /// performs all moving operations for particles
        /// </summary>
        public void move(float timeInterval, DamageMap damageMap)
        {
            float damageFactor = attackingPerSecond * timeInterval;

            sumDamage = 0;
            int damageIndex0 = (playerIndex + 1) % 4;
            int damageIndex1 = (playerIndex + 2) % 4;
            int damageIndex2 = (playerIndex + 3) % 4; 

            // move them!!
            for (int i = 0; i <= currentMaxAgentIndex; ++i)
            {
                if (float.IsNegativeInfinity(agentList[i].health))
                    continue;

                #region movement
                Vector2 aimed = cursorPosition - agentList[i].position;
                float l = aimed.Length() + 0.0001f;
                aimed *= disciplinConstant / l;

                agentList[i].movement += aimed;
                agentList[i].movement.Normalize();
                agentList[i].movement *= agentList[i].speed;

                agentList[i].position += agentList[i].movement * timeInterval;
                #endregion

                #region borders

                // never come out of the interval [0;1[
                float posSgn = Math.Sign(agentList[i].position.X);
                float posInv = agentList[i].position.X - 0.999f;
                float posInvSgn = -Math.Sign(posInv);
                float combSigns = posInvSgn * posSgn;
                agentList[i].position.X = posInv * combSigns + 0.999f * posSgn;
                agentList[i].movement.X *= combSigns;

                posSgn = Math.Sign(agentList[i].position.Y);
                posInv = agentList[i].position.Y - 0.999f;
                posInvSgn = -Math.Sign(posInv);
                combSigns = posInvSgn * posSgn;
                agentList[i].position.Y = posInv * combSigns + 0.999f * posSgn;
                agentList[i].movement.Y *= combSigns;

                /*
                if (agentList[i].position.X < 0.001f)
                {
                    agentList[i].position.X = 0.001f;
                    agentList[i].movement.X = -agentList[i].movement.X;
                }
                else if (agentList[i].position.X > 0.999f)
                {
                    agentList[i].position.X = 0.999f;
                    agentList[i].movement.X = -agentList[i].movement.X;
                }
                if (agentList[i].position.Y < 0.001f)
                {
                    agentList[i].position.Y = 0.001f;
                    agentList[i].movement.Y = -agentList[i].movement.Y;
                }
                else if (agentList[i].position.Y > 0.999f)
                {
                    agentList[i].position.Y = 0.999f;
                    agentList[i].movement.Y = -agentList[i].movement.Y;
                }
*/

                #endregion

                #region damage
                // position to map
                int x = (int)(agentList[i].position.X * DamageMap.attackingMapSize);
                int y = (int)(agentList[i].position.Y * DamageMap.attackingMapSize);
                float damage = (damageMap.GetPlayerDamageAt(x, y, damageIndex0) + 
                                damageMap.GetPlayerDamageAt(x, y, damageIndex1) +
                                damageMap.GetPlayerDamageAt(x, y, damageIndex2)) * damageFactor;

#if PLAYER_ZERO_IS_CHEATING
                damage = playerIndex == 0 ? 0 : damage * 2;
#endif

                    // damage
                agentList[i].health -= damage;
                OverallHealth -= damage;
                sumDamage += damage;

                    // dead?
                if (agentList[i].health < minHealth)
                {
                    OverallHealth -= agentList[i].health; // be fair, add this
                    agentList[i].health = float.NegativeInfinity;

                    // chained list for spawning
                    agentList[i].next = nextSpawnIndex;
                    nextSpawnIndex = i;
                    if (currentMaxAgentIndex == i)
                        --currentMaxAgentIndex;
                    --NumAlive;
                }
                #endregion
            }

            // add random dissortion
            randomTimeAccum += timeInterval;
            int numRandomActions = (int)(RandomForcesPercentagePerSecond * randomTimeAccum * NumAlive);
            if(numRandomActions > 0)
            {
                randomTimeAccum = 0.0f;
                for (int r = 0; r < numRandomActions; ++r)
                {
                    int randomIndex = random.Next(currentMaxAgentIndex + 1);

                    // invalid index? simply skip
                    if (float.IsNegativeInfinity(agentList[randomIndex].health))
                        continue;

                    Vector2 aimed = new Vector2((float)random.NextDouble(), (float)random.NextDouble()); 
                    float l = aimed.Length() + 0.000001f;
                    aimed *= agentList[randomIndex].speed * RandomForceStrength / l;

                    agentList[randomIndex].movement += aimed;
                    l = agentList[randomIndex].movement.Length();
                    agentList[randomIndex].movement *= agentList[randomIndex].speed / l;
                }
            }
        }

        /// <summary>
        /// controll through a gamepad or 
        /// </summary>
        public void UserControl(float frameTimeInterval, GamePadState[] currentGamePadStates, KeyboardState currentKeyBoardState)
        {
            Vector2 cursorMove;
            Vector2 padMove;
            MovementsFromControls(out cursorMove, out padMove, currentGamePadStates, currentKeyBoardState);

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

        private void MovementsFromControls(out Vector2 cursorMove, out Vector2 padMove, GamePadState[] currentGamePadStates, KeyboardState currentKeyBoardState)
        {
            padMove = Vector2.Zero;
            cursorMove = Vector2.Zero;

            switch(Controls)
            {

#if !XBOX
                case ControlType.KEYBOARD0:
                    {
                        Vector2 keyboardMove = new Vector2(currentKeyBoardState.IsKeyDown(Keys.D) ? 1 : 0 - (currentKeyBoardState.IsKeyDown(Keys.A) ? 1 : 0),
                                                            currentKeyBoardState.IsKeyDown(Keys.S) ? -1 : 0 + (currentKeyBoardState.IsKeyDown(Keys.W) ? 1 : 0));
                        float l = keyboardMove.Length();
                        if (l > 1) keyboardMove /= l;
                        if (currentKeyBoardState.IsKeyDown(Keys.LeftControl))
                            padMove = keyboardMove;
                        else
                            cursorMove = keyboardMove;
                        break;
                    }

                case ControlType.KEYBOARD1:
                    {
                        Vector2 keyboardMove = new Vector2(currentKeyBoardState.IsKeyDown(Keys.Right) ? 1 : 0 - (currentKeyBoardState.IsKeyDown(Keys.Left) ? 1 : 0),
                                                            currentKeyBoardState.IsKeyDown(Keys.Down) ? -1 : 0 + (currentKeyBoardState.IsKeyDown(Keys.Up) ? 1 : 0));
                        float l = keyboardMove.Length();
                        if (l > 1) keyboardMove /= l;
                        if (currentKeyBoardState.IsKeyDown(Keys.RightControl))
                            padMove = keyboardMove;
                        else
                            cursorMove = keyboardMove;
                        break;
                    }
#endif
                case ControlType.GAMEPAD0:
                    padMove = currentGamePadStates[0].ThumbSticks.Right;
                    cursorMove = currentGamePadStates[0].ThumbSticks.Left;
                    break;

                case ControlType.GAMEPAD1:
                    padMove = currentGamePadStates[1].ThumbSticks.Right;
                    cursorMove = currentGamePadStates[1].ThumbSticks.Left;
                    break;

                case ControlType.GAMEPAD2:
                    padMove = currentGamePadStates[2].ThumbSticks.Right;
                    cursorMove = currentGamePadStates[2].ThumbSticks.Left;
                    break;

                case ControlType.GAMEPAD3:
                    padMove = currentGamePadStates[3].ThumbSticks.Right;
                    cursorMove = currentGamePadStates[3].ThumbSticks.Left;
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
        private void UpdateAliveStatus(float timeInterval, int numSpawns)
        {
            if (Alive)
            {
                if (numSpawns == 0)
                {
                    timeWithoutSpawnPoint += timeInterval;

                    if (NumAlive == 0 || timeWithoutSpawnPoint > maxTimeWithoutSpawnPoint)
                    {
                        alive = false;
                        NumAlive = 0;
                        currentMaxAgentIndex = 0;
                    }
                }
                else
                    timeWithoutSpawnPoint = 0.0f;
            }
            else
                TimeDead += timeInterval;
        }
    }
}
