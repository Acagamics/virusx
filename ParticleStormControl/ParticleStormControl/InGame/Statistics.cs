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
        #region control variables

        private float stepTime;
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

        #endregion

        #region absolut statistics

        private ulong[] generatedParticles;
        private uint[] captueredBases; // active
        private uint[] lostBases; // active
        private uint[] wonMatches; // active
        //private uint[] lostMatches;
        private uint[] collectedItems; // active ; split for every item
        private uint[] usedItems; // active ; split for every item
        private uint[] killedEnemies;

        #endregion

        #region time depend statistics

        private List<uint>[] particlesInStep; // active
        private List<uint>[] healthInStep; // active but wrong input
        private uint[] averageParticles; // active
        private uint[] averageHealth; // active
        private uint[] maxSimultaneousParticles; // active
        private List<float>[] dominationInStep;
        private List<uint>[] possessingBasesInStep;

        #endregion

        public Statistics(float _stepTime, int _playerCount)
        {
            steps = 0;
            stepTime = _stepTime;
            playerCount = _playerCount;
            remainingTime = 0f;
            init();
        }

        private void init()
        {
            generatedParticles = new ulong[playerCount];
            captueredBases = new uint[playerCount];
            lostBases = new uint[playerCount];
            wonMatches = new uint[playerCount];
            collectedItems = new uint[playerCount];
            usedItems = new uint[playerCount];
            killedEnemies = new uint[playerCount];
            
            dominationInStep = new List<float>[playerCount];

            averageParticles = new uint[playerCount];
            averageHealth = new uint[playerCount];
            maxSimultaneousParticles = new uint[playerCount];

            particlesInStep = new List<uint>[playerCount];
            healthInStep = new List<uint>[playerCount];
            possessingBasesInStep = new List<uint>[playerCount];

            for (int i = 0; i < playerCount; ++i)
            {
                particlesInStep[i] = new List<uint>();
                healthInStep[i] = new List<uint>();
                dominationInStep[i] = new List<float>();
                possessingBasesInStep[i] = new List<uint>();
            }
        }

        public bool UpdateTimer(float frameTimeSeconds)
        {
            if (remainingTime <= 0f)
            {
                remainingTime = stepTime;
                return true;
            }
            else
            {
                remainingTime -= frameTimeSeconds;
                return false;
            }
        }

        #region getter for statistics

        /// <summary>
        /// Not tracked yet. The total amount of generated particles
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public ulong getGeneratedParticles(int playerIndex) { return playerIndex < playerCount ? generatedParticles[playerIndex] : 0; }
        /// <summary>
        /// The total amount of captuered bases/spawnPoints.
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public uint getCaptueredBases(int playerIndex) { return playerIndex < playerCount ? captueredBases[playerIndex] : 0; }
        /// <summary>
        /// The total amount of lost bases/spawnPoints
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public uint getLostBases(int playerIndex) { return playerIndex < playerCount ? lostBases[playerIndex] : 0; }
        /// <summary>
        /// The amount of won matches
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public uint getWonMatches(int playerIndex) { return playerIndex < playerCount ? wonMatches[playerIndex] : 0; }
        /// <summary>
        /// the total amount of collected items
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public uint getCollectedItems(int playerIndex) { return playerIndex < playerCount ? collectedItems[playerIndex] : 0; }
        /// <summary>
        /// the total amount of used items
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public uint getUsedItems(int playerIndex) { return playerIndex < playerCount ? usedItems[playerIndex] : 0; }
        /// <summary>
        /// Not tracked yet. The amount of killed enemys
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public uint getKilledEnemies(int playerIndex) { return playerIndex < playerCount ? killedEnemies[playerIndex] : 0; }

        /// <summary>
        /// The domination value per TimeStep. It is a value between 0 and 2. Zero means no domination (death) and 2 means complete domination (winning player)
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public float getDominationInStep(int playerIndex, int step) { return step < steps ? (playerIndex < playerCount ? dominationInStep[playerIndex][step] : 0) : 0; }
        /// <summary>
        /// the added domination for all players in a specific time step
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public float getDominationInStep(int step) 
        {
            float result = 0;
            for (int i = 0; i < playerCount; i++)
            {
                result += getDominationInStep(i, step);
            }
            return result;
        }
        /// <summary>
        /// The maximum number of particles a player possessed in a game
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public uint getMaxSimultaneousParticles(int playerIndex) { return playerIndex < playerCount ? maxSimultaneousParticles[playerIndex] : 0; }
        /// <summary>
        /// The average number of particles in a game
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public uint getAverageParticles(int playerIndex) { return playerIndex < playerCount ? averageParticles[playerIndex] : 0; }
        /// <summary>
        /// The average healt in a game
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public uint getAverageHealth(int playerIndex) { return playerIndex < playerCount ? averageHealth[playerIndex] : 0; }
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
        public uint getParticlesInStep(int playerIndex, int step) { return step < steps ? (playerIndex < playerCount ? particlesInStep[playerIndex][step] : 0) : 0; }
        /// <summary>
        /// the health a player had in a specific time step
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public uint getHealthInStep(int playerIndex, int step) { return step < steps ? (playerIndex < playerCount ? healthInStep[playerIndex][step] : 0) : 0; }
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
        /// the number of bases/spawnPoints a player possessed in a specific time step
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public uint getPossessingBasesInStep(int playerIndex, int step) { return step < steps ? (playerIndex < playerCount ? possessingBasesInStep[playerIndex][step] : 0) : 0; }

        #endregion

        #region collecting statistics

        public void addGeneratedParticles(int playerIndex, int particles = 1)
        {
            if (playerIndex < 0 || playerIndex >= playerCount) return;
            if(particles>0) generatedParticles[playerIndex] += (ulong)particles;
            else generatedParticles[playerIndex] -= (ulong)particles;
        }

        public void addCaptueredBases(int playerIndex, int bases = 1)
        {
            if (playerIndex < 0 || playerIndex >= playerCount) return;
            if (bases > 0) captueredBases[playerIndex] += (uint)bases;
            else captueredBases[playerIndex] -= (uint)bases;
        }

        public void addLostBases(int playerIndex, int bases = 1)
        {
            if (playerIndex < 0 || playerIndex >= playerCount) return;
            if (bases > 0) lostBases[playerIndex] += (uint)bases;
            else lostBases[playerIndex] -= (uint)bases;
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

        public void setParticlesAndHealth(int playerIndex, uint particles, uint health)
        {
            if (playerIndex < 0 || playerIndex >= playerCount) return;
            particlesInStep[playerIndex].Add(particles);
            healthInStep[playerIndex].Add(health);

            if (particles >= maxSimultaneousParticles[playerIndex])
                maxSimultaneousParticles[playerIndex] = particles;

            computeAverage(playerIndex);

            if (steps < particlesInStep[playerIndex].Count) steps = particlesInStep[playerIndex].Count;
        }

        public void setPossessingBases(int playerIndex, uint bases)
        {
            if (playerIndex < 0 || playerIndex >= playerCount) return;
            possessingBasesInStep[playerIndex].Add((uint)bases);
        }

        private void computeAverage(int playerIndex)
        {
            ulong overallParticles = 0;
            ulong overallHealth = 0;

            foreach (uint i in particlesInStep[playerIndex])
            {
                overallParticles += (ulong)i;
            }

            foreach (uint i in healthInStep[playerIndex])
            {
                overallHealth += (ulong)i;
            }

            averageParticles[playerIndex] = (uint)(overallParticles / (ulong)particlesInStep[playerIndex].Count);
            averageHealth[playerIndex] = (uint)(overallHealth / (ulong)healthInStep[playerIndex].Count);
        }

        public void UpdateDomination()
        {
            int currentStep = steps - 1;
            ulong overallHealth = 0;
            uint overallBases = 0;
            for (int i = 0; i < playerCount; ++i)
            {
                overallHealth += healthInStep[i][currentStep];
                overallBases += possessingBasesInStep[i][currentStep];
            }

            float healthPercentage = 0f;
            float basePercentage = 0f;
            for (int i = 0; i < playerCount; ++i)
            {
                healthPercentage = (float)healthInStep[i][currentStep] / overallHealth;
                basePercentage = (float)possessingBasesInStep[i][currentStep] / overallBases;
                dominationInStep[i].Add(healthPercentage + basePercentage);
            }
        }

        #endregion
    }
}
