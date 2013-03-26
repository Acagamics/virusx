using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParticleStormControl
{
    /// <summary>
    /// class to collect game statistics
    /// </summary>
    public class Statistics
    {
        #region control variables

        private float stepTime;
        public float StepTime { get { return stepTime; } }
        private int playerCount;
        public int PlayerCount { get { return playerCount; } }
        private int steps;
        public int Steps { get { return steps; } }

        #endregion

        #region absolut statistics

        private ulong[] generatedParticles;
        private uint[] captueredBases;
        private uint[] lostBases;
        private uint[] wonMatches;
        private uint[] lostMatches;
        private uint[] collectedItems; // split for every item
        private uint[] usedItems; // split for every item
        private uint[] killedEnemies;

        #endregion

        #region time depend statistics

        private List<uint>[] particlesInStep;
        private List<uint>[] healthInStep;
        private uint[] averageParticles;
        private uint[] averageHealth;
        private uint[] maxSimultaneousParticles;
        private float[] domination;

        #endregion

        public Statistics(float _stepTime, int _playerCount)
        {
            steps = 0;
            stepTime = _stepTime;
            playerCount = _playerCount;
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
            
            domination = new float[playerCount];

            averageParticles = new uint[playerCount];
            averageHealth = new uint[playerCount];
            maxSimultaneousParticles = new uint[playerCount];

            particlesInStep = new List<uint>[playerCount];
            healthInStep = new List<uint>[playerCount];

            for (int i = 0; i < playerCount; ++i)
            {
                particlesInStep[i] = new List<uint>();
                healthInStep[i] = new List<uint>();
            }
        }

        #region getter for statistics

        public ulong getGeneratedParticles(int playerIndex) { return playerIndex < playerCount ? generatedParticles[playerIndex] : 0; }
        public uint getCaptueredBases(int playerIndex) { return playerIndex < playerCount ? captueredBases[playerIndex] : 0; }
        public uint getLostBases(int playerIndex) { return playerIndex < playerCount ? lostBases[playerIndex] : 0; }
        public uint getWonMatches(int playerIndex) { return playerIndex < playerCount ? wonMatches[playerIndex] : 0; }
        public uint getCollectedItems(int playerIndex) { return playerIndex < playerCount ? collectedItems[playerIndex] : 0; }
        public uint getUsedItems(int playerIndex) { return playerIndex < playerCount ? usedItems[playerIndex] : 0; }
        public uint getKilledEnemies(int playerIndex) { return playerIndex < playerCount ? killedEnemies[playerIndex] : 0; }
        
        public float getDomination(int playerIndex) { return playerIndex < playerCount ? domination[playerIndex] : 0; }

        public uint getMaxSimultaneousParticles(int playerIndex) { return playerIndex < playerCount ? maxSimultaneousParticles[playerIndex] : 0; }
        public uint getAverageParticles(int playerIndex) { return playerIndex < playerCount ? averageParticles[playerIndex] : 0; }
        public uint getAverageHealth(int playerIndex) { return playerIndex < playerCount ? averageHealth[playerIndex] : 0; }
        public uint getParticlesInStep(int playerIndex, int step) { return step < steps ? particlesInStep[playerIndex][step] : 0; }
        public uint getParticlesInStep(int playerIndex, int step) { return step < steps ? healthInStep[playerIndex][step] : 0; }

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

        #endregion
    }
}
