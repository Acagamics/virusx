using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ParticleStormControl
{
    /// <summary>
    /// class to collect game statistics
    /// </summary>
    public class Statistics
    {
        #region used items
        
        public enum StatItems
        {
            DANGER_ZONE,
            MUTATION,
            WIPEOUT,
            ANTI_BODY,

            NUM_STAT_ITEMS
        }

        public struct ItemUsed 
        {
            public StatItems item;
            public int step;
            public static ItemUsed newItemUsed(StatItems _item, int _step) { ItemUsed res; res.item = _item; res.step = _step; return res; }
        }

        List<ItemUsed>[] itemsUsedByPlayer;

        #endregion

        #region control variables

        //private float stepTime;
        private const float stepTime = 0.25f;

        /// <summary>
        /// the time in seconds between tracking time depending statistic values
        /// </summary>
        public float StepTime { get { return stepTime; } }
        private int playerCount;
        /// <summary>
        /// the number of players
        /// </summary>
        public int PlayerCount { get { return playerCount; } }
        private int steps;
        /// <summary>
        /// the number of time steps for which statistic data is available
        /// </summary>
        public int Steps { get { return steps; } }
        private float remainingTime;

        private int lastStep;
        /// <summary>
        /// the highest time step if you multiply this with the StepTime you get the overall time for the game
        /// </summary>
        public int LastStep { get { return lastStep; } }
        /// <summary>
        /// the first time point for which statistical data is available. Multiplay this with StepTime and you get the start time of the statistics
        /// </summary>
        public int FirstStep { get { return LastStep - Steps; } }

        private int maxStoredSteps;
        /// <summary>
        /// The maximum number of steps which will be stored. If the game takes longer, the data from the beginning of the game will be deletet
        /// </summary>
        public int MaxStoredSteps { get { return maxStoredSteps; } }
        #endregion

        #region absolut statistics

        /// <summary>
        /// The overall Number of SpawnPoints in the game
        /// </summary>
        public uint OverallNumberOfSpawnPoints { get; private set; } 
        private ulong[] generatedParticles;
        private uint[] capturedSpawnPoints; // active
        private uint[] lostSpawnPoints; // active
        private uint[] wonMatches; // active
        //private uint[] lostMatches;
        private uint[] collectedItems; // active ; split for every item
        private uint[] usedItems; // active ; split for every item
        private uint[] killedEnemies;

        private int[] deathStep;

        public int MaxOverallSimultaneousParticles { get { return maxOverallSimultaneousParticles; } }
        private int maxOverallSimultaneousParticles = 1;

        public float MaxOverallSimultaneousHealth { get { return maxOverallSimultaneousHealth; } }
        private float maxOverallSimultaneousHealth = 1;

        #endregion

        #region time depend statistics

        private List<uint>[] particlesInStep; // active
        private List<uint>[] healthInStep; // active
        private uint[] averageParticles; // active
        private uint[] averageHealth; // active
        private uint[] maxSimultaneousParticles; // active
        private List<float>[] dominationInStep; // active
        private List<uint>[] possessingSpawnPointsInStep; // active

        #endregion

        public Statistics(int _playerCount, int _maxStoredSteps, uint _overallNumberOfSpawnPoints)
        {
            steps = lastStep = 0;
            //stepTime = _stepTime;
            playerCount = _playerCount;
            remainingTime = 0f;
            maxStoredSteps = _maxStoredSteps;
            OverallNumberOfSpawnPoints = _overallNumberOfSpawnPoints;
            Init();
        }

        private void Init()
        {
            generatedParticles = new ulong[playerCount];
            capturedSpawnPoints = new uint[playerCount];
            lostSpawnPoints = new uint[playerCount];
            wonMatches = new uint[playerCount];
            collectedItems = new uint[playerCount];
            usedItems = new uint[playerCount];
            killedEnemies = new uint[playerCount];
            deathStep = new int[playerCount];

            
            dominationInStep = new List<float>[playerCount];

            averageParticles = new uint[playerCount];
            averageHealth = new uint[playerCount];
            maxSimultaneousParticles = new uint[playerCount];

            particlesInStep = new List<uint>[playerCount];
            healthInStep = new List<uint>[playerCount];
            possessingSpawnPointsInStep = new List<uint>[playerCount];

            itemsUsedByPlayer = new List<ItemUsed>[playerCount];

            for (int i = 0; i < playerCount; ++i)
            {
                particlesInStep[i] = new List<uint>();
                healthInStep[i] = new List<uint>();
                dominationInStep[i] = new List<float>();
                possessingSpawnPointsInStep[i] = new List<uint>();
                itemsUsedByPlayer[i] = new List<ItemUsed>();
                deathStep[i] = -1;
            }
        }

        public bool UpdateTimer(float frameTimeSeconds)
        {
            if (remainingTime <= 0f)
            {
                remainingTime = stepTime;
                lastStep++;
                return true;
            }
            else
            {
                remainingTime -= frameTimeSeconds;
                return false;
            }
        }

        #region getter for statistics

        public int getDeathStepOfPlayer(int playerIndex) { return playerIndex < playerCount ? playerIndex >= 0 ? deathStep[playerIndex] : -2 : -2; }
        /// <summary>
        /// Not tracked yet. The total amount of generated particles
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public ulong getGeneratedParticles(int playerIndex) { return playerIndex < playerCount ? playerIndex >= 0 ? generatedParticles[playerIndex] : 0 : 0; }
        /// <summary>
        /// The total amount of captuered SpawnPoints/spawnPoints.
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public uint getCapturedSpawnPoints(int playerIndex) { return playerIndex < playerCount ? playerIndex >= 0 ? capturedSpawnPoints[playerIndex] : 0 : 0; }
        /// <summary>
        /// The total amount of lost SpawnPoints/spawnPoints
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public uint getLostSpawnPoints(int playerIndex) { return playerIndex < playerCount ? playerIndex >= 0 ? lostSpawnPoints[playerIndex] : 0 : 0; }
        /// <summary>
        /// The amount of won matches
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public uint getWonMatches(int playerIndex) { return playerIndex < playerCount ? playerIndex >= 0 ? wonMatches[playerIndex] : 0 : 0; }
        /// <summary>
        /// the total amount of collected items
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public uint getCollectedItems(int playerIndex) { return playerIndex < playerCount ? playerIndex >= 0 ? collectedItems[playerIndex] : 0 : 0; }
        /// <summary>
        /// the total amount of used items
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public uint getUsedItems(int playerIndex) { return playerIndex < playerCount ? playerIndex >= 0 ? usedItems[playerIndex] : 0 : 0; }
        /// <summary>
        /// Not tracked yet. The amount of killed enemys
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public uint getKilledEnemies(int playerIndex) { return playerIndex < playerCount ? playerIndex >= 0 ? killedEnemies[playerIndex] : 0 : 0; }

        /// <summary>
        /// The domination value per TimeStep. It is a value between 0 and 1. Zero means no domination (death) and 1 means complete domination (winning player).
        /// The sum of the domination value of all players is always 1
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public float getDominationInStep(int playerIndex, int step) { return step < steps ? (playerIndex < playerCount ? playerIndex >= 0 ? dominationInStep[playerIndex][step] : 0 : 0) : 0; }
        
        /// <summary>
        /// The maximum number of particles a player possessed in a game
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public uint getMaxSimultaneousParticles(int playerIndex) { return playerIndex < playerCount ? playerIndex >= 0 ? maxSimultaneousParticles[playerIndex] : 0 : 0; }
        /// <summary>
        /// The average number of particles in a game
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public uint getAverageParticles(int playerIndex) { return playerIndex < playerCount ? playerIndex >= 0 ? averageParticles[playerIndex] : 0 : 0; }
        /// <summary>
        /// The average healt in a game
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public uint getAverageHealth(int playerIndex) { return playerIndex < playerCount ? playerIndex >= 0 ? averageHealth[playerIndex] : 0 : 0; }
        /// <summary>
        /// the number of particles possessed by all players in a specific time step
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public uint getParticlesInStep(int step)
        {
            uint result = 0;
            for (int i = 0; i < playerCount; i++)
            {
                result += getParticlesInStep(i, step);
            }
            return result;
        }
        /// <summary>
        /// the number of particles a player possessed in a specific time step
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public uint getParticlesInStep(int playerIndex, int step) { return step < steps ? (playerIndex < playerCount ? playerIndex >= 0 ? particlesInStep[playerIndex][step] : 0 : 0) : 0; }
        /// <summary>
        /// the health a player had in a specific time step
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public uint getHealthInStep(int playerIndex, int step) { return step < steps ? (playerIndex < playerCount ? playerIndex >= 0 ? healthInStep[playerIndex][step] : 0 : 0) : 0; }
        /// <summary>
        /// the number of particles possessed by all players in a specific time step
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public uint getHealthInStep(int step)
        {
            uint result = 0;
            for (int i = 0; i < playerCount; i++)
            {
                result += getHealthInStep(i, step);
            }
            return result;
        }
        /// <summary>
        /// the number of SpawnPoints/spawnPoints a player possessed in a specific time step
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public uint getPossessingSpawnPointsInStep(int playerIndex, int step) { return step < steps ? (playerIndex < playerCount ? playerIndex >= 0 ? possessingSpawnPointsInStep[playerIndex][step] : 0 : 0) : 0; }
        
        /// <summary>
        /// a List of all items a player used/activated in a specific time step. The List is empty if the player has not used/activated any item.
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="step"></param>
        /// <returns></returns>
    /*    public List<StatItems> getUsedItemsInStep(int playerIndex, int step)
        {
            List<StatItems> result = new List<StatItems>();

            if (step < steps && playerIndex < playerCount)
            {
                foreach (ItemUsed item in itemsUsedByPlayer[playerIndex])
                {
                    if (item.step == step) result.Add(item.item);
                    else if (step < item.step) break;
                }
            }

            return result;
        } */

        /// <summary>
        /// returns the first item used in a given step by a given player
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="step"></param>
        /// <returns>null if there wasn't any item</returns>
        public StatItems? getFirstUsedItemInStep(int playerIndex, int step)
        {
            if (step < steps && playerIndex < playerCount)
            {
                foreach (ItemUsed item in itemsUsedByPlayer[playerIndex])
                {
                    if (item.step == step)
                        return item.item;
                    else if (step < item.step) break;
                }
            }

            return null;
        }

        #endregion

        #region collecting statistics

        public void playerDied(int playerIndex)
        {
            if (playerIndex >= 0 && playerIndex < PlayerCount)
            {
                deathStep[playerIndex] = lastStep;
            }
        }

        public void addGeneratedParticles(int playerIndex, int particles = 1)
        {
            if (playerIndex < 0 || playerIndex >= playerCount) return;
            if(particles>0) generatedParticles[playerIndex] += (ulong)particles;
            else generatedParticles[playerIndex] -= (ulong)particles;
        }

        public void addCaptueredSpawnPoints(int playerIndex, int SpawnPoints = 1)
        {
            if (playerIndex < 0 || playerIndex >= playerCount) return;
            if (SpawnPoints > 0) capturedSpawnPoints[playerIndex] += (uint)SpawnPoints;
            else capturedSpawnPoints[playerIndex] -= (uint)SpawnPoints;
        }

        public void addLostSpawnPoints(int playerIndex, int SpawnPoints = 1)
        {
            if (playerIndex < 0 || playerIndex >= playerCount) return;
            if (SpawnPoints > 0) lostSpawnPoints[playerIndex] += (uint)SpawnPoints;
            else lostSpawnPoints[playerIndex] -= (uint)SpawnPoints;
        }

        public void addWonMatches(int playerIndex, int matches = 1)
        {
            if (playerIndex < 0 || playerIndex >= playerCount) return;
            if (matches > 0) wonMatches[playerIndex] += (uint)matches;
            else wonMatches[playerIndex] -= (uint)matches;
        }

        public void addCollectedItems(int playerIndex, int items = 1)
        {
            if (playerIndex < 0 || playerIndex >= playerCount) return;
            if (items > 0) collectedItems[playerIndex] += (uint)items;
            else collectedItems[playerIndex] -= (uint)items;
        }

        public void addUsedItems(int playerIndex, int items = 1) 
        {
            if (playerIndex < 0 || playerIndex >= playerCount) return;
            if (items > 0) usedItems[playerIndex] += (uint)items;
            else usedItems[playerIndex] -= (uint)items;
        }

        public void addKilledEnemies(int playerIndex, int enemies = 1) 
        {
            if (playerIndex < 0 || playerIndex >= playerCount) return;
            if (enemies > 0) killedEnemies[playerIndex] += (uint)enemies;
            else killedEnemies[playerIndex] -= (uint)enemies;
        }

        public void setParticlesAndHealthAndPossesingSpawnPoints(int playerIndex, uint particles, uint health, uint SpawnPoints)
        {
            if (playerIndex < 0 || playerIndex >= playerCount) return;
            particlesInStep[playerIndex].Add(particles);
            healthInStep[playerIndex].Add(health);
            possessingSpawnPointsInStep[playerIndex].Add((uint)SpawnPoints);

            if (particles >= maxSimultaneousParticles[playerIndex])
                maxSimultaneousParticles[playerIndex] = particles;

            computeAverage(playerIndex);

            if (steps < particlesInStep[playerIndex].Count) steps = particlesInStep[playerIndex].Count;
        }

        private void reduceAllListsByOne()
        {
            for (int i = 0; i < playerCount; ++i)
            {
                particlesInStep[i].RemoveAt(0);
                healthInStep[i].RemoveAt(0);
                dominationInStep[i].RemoveAt(0);
                possessingSpawnPointsInStep[i].RemoveAt(0);
            }

        }

        private void computeAverage(int playerIndex)
        {
            /*ulong overallParticles = 0;
            ulong overallHealth = 0;

            for (int i = 0; i < steps; ++i)
            {
                overallParticles += (ulong)particlesInStep[playerIndex][i];
                overallHealth += (ulong)healthInStep[playerIndex][i];
            }*/

            averageParticles[playerIndex] = (uint)particlesInStep[playerIndex].ToArray().Average(x => (uint)x);// (uint)(overallParticles / (ulong)particlesInStep[playerIndex].Count);
            averageHealth[playerIndex] = (uint)healthInStep[playerIndex].ToArray().Average(x => (uint)x);//(uint)(overallHealth / (ulong)healthInStep[playerIndex].Count);
        }

        public void UpdateDomination(Player[] players)
        {
            maxOverallSimultaneousParticles = Math.Max(players.Sum(x => x.NumParticlesAlive), maxOverallSimultaneousParticles);
            maxOverallSimultaneousHealth = Math.Max(players.Sum(x => x.TotalVirusHealth), maxOverallSimultaneousHealth);


            float [] percentage = ComputeDomination(players);
            for (int i = 0; i < playerCount; ++i)
            {
                dominationInStep[i].Add(percentage[i]);
            }

            if (steps == maxStoredSteps)
            {
                steps--;
                reduceAllListsByOne();
            }
        }

        public static float[] ComputeDomination(Player[] players)
        {
            float[] result = new float[players.Length];// + 1]; result[players.Length] = 0.0f;
            float overallHealth = players.Sum(x => x.TotalVirusHealth);// 0.0f;
            ulong overallParticles = (ulong)players.Sum(x => x.NumParticlesAlive); //0;
            float overallSpawnPointsizes = players.Sum(x => x.PossessingSpawnPointsOverallSize);// 0.0f;
            uint overallSpawnPoints = (uint)players.Sum(x => x.PossessingSpawnPoints);//0;

            for (int i = 0; i < players.Length; ++i)
            {
                float dev = 5f;
                if (overallHealth <= 0.0f)
                {
                    overallHealth = 1.0f;
                    dev -= 1f;
                }
                if (overallParticles <= 0)
                {
                    overallParticles = 1;
                    dev -= 1f;
                }
                if (overallSpawnPointsizes <= 0.0f)
                {
                    overallSpawnPointsizes = 1;
                    dev -= 2f;
                }
                if (overallSpawnPoints <= 0)
                {
                    overallSpawnPointsizes = 1;
                    dev -= 1f;
                }
                if (dev > 0.0f)
                {
                    result[i] = (((players[i].PossessingSpawnPointsOverallSize / overallSpawnPointsizes) * 2f)
                        + (float)players[i].NumParticlesAlive / overallParticles
                        + players[i].TotalVirusHealth / overallHealth
                        + (float)players[i].PossessingSpawnPoints / overallSpawnPoints) / dev;
                }
                else result[i] = 1f / players.Length;
                //result[players.Length] += result[i];
            }
            
            return result;
        }

        public void itemUsed(int playerIndex, Item.ItemType _item)
        {
            if (playerIndex < 0 || playerIndex >= playerCount) return;

            StatItems item;
            switch (_item)
            {
                case Item.ItemType.DANGER_ZONE: item = StatItems.DANGER_ZONE; break;
                case Item.ItemType.MUTATION: item = StatItems.MUTATION; break;
                case Item.ItemType.WIPEOUT: item = StatItems.WIPEOUT; break;
                default: return;
            }
            itemUsed(playerIndex, item);
        }

        public void itemUsed(int playerIndex, StatItems _item = StatItems.ANTI_BODY)
        {
            if (playerIndex < 0 || playerIndex >= playerCount) return;

            itemsUsedByPlayer[playerIndex].Add(ItemUsed.newItemUsed(_item,steps-1));
        }

        #endregion
    }
}
