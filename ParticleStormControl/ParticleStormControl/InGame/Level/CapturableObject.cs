using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ParticleStormControl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ParticleStormControl
{
    public abstract class CapturableObject : MapObject
    {
        private readonly Stopwatch lifeTimer = new Stopwatch();
        protected readonly float lifeTime;  // -1 means infinite

        public bool Timeouted { get { return lifeTime > 0 && lifeTimer.Elapsed.TotalSeconds > lifeTime; } }

        public int PossessingPlayer { get; protected set; }
        public float PossessingPercentage { get; protected set; }
        public int CapturingPlayer { get; protected set; }

        protected readonly int damageMap_MinX;
        protected readonly int damageMap_MinY;
        protected readonly int damageMap_MaxX;
        protected readonly int damageMap_MaxY;

        /// <summary>
        /// determines how efficient one can reestablish the owningpercentage of his own object
        /// </summary>
        private const float defenseFactor = 0.25f;

        protected readonly float damageFactor;

        public CapturableObject(Vector2 Position, int possessingPlayer, float damageFactor, float lifeTime/* = -1.0f*/, int damageMapPixelHalfRange /*= 4*/, float size = 0.05f) :
            base(Position, size)
        {
            this.lifeTime = lifeTime;
            PossessingPlayer = possessingPlayer;
            if (possessingPlayer > -1)
                PossessingPercentage = 1.0f;
            else
                PossessingPercentage = 0.0f;
            CapturingPlayer = -1;
            this.damageFactor = damageFactor;

            int xOnDamageMap = (int)(Position.X / Level.RELATIVE_MAX.X * DamageMap.attackingMapSizeX);
            int yOnDamageMap = (int)(Position.Y / Level.RELATIVE_MAX.Y * DamageMap.attackingMapSizeY);
            damageMap_MinX = Math.Max(0, xOnDamageMap - damageMapPixelHalfRange);
            damageMap_MinY = Math.Max(0, yOnDamageMap - damageMapPixelHalfRange);
            damageMap_MaxX = Math.Min(DamageMap.attackingMapSizeX - 1, xOnDamageMap + damageMapPixelHalfRange);
            damageMap_MaxY = Math.Min(DamageMap.attackingMapSizeY - 1, yOnDamageMap + damageMapPixelHalfRange);

            lifeTimer.Start();
        }

        protected abstract void OnPossessingChanged();

        public override void Update(float frameTimeSeconds, float totalTimeSeconds)
        {
            if (lifeTime > 0 && lifeTimer.Elapsed.TotalSeconds > lifeTime)
                Alive = false;
        }

        public override void SwitchPlayer(int[] playerSwitchedTo)
        {
            base.SwitchPlayer(playerSwitchedTo);

            if (PossessingPlayer != -1)
                PossessingPlayer = playerSwitchedTo[PossessingPlayer];

            if (CapturingPlayer != -1)
                CapturingPlayer = playerSwitchedTo[CapturingPlayer];
        }

        public override void ApplyDamage(DamageMap damageMap, float timeInterval)
        {
            float[] damage = new float[4];
            for (int i = 0; i < Player.MaxNumPlayers; ++i)
                damage[i] = 0.0f;

            for (int y = damageMap_MinY; y <= damageMap_MaxY; ++y)
            {
                for (int x = damageMap_MinX; x <= damageMap_MaxX; ++x)
                {
                    for (int i = 0; i < Player.MaxNumPlayers; ++i)
                        damage[i] += damageMap.GetPlayerDamageAt(x, y, i);
                }
            }

            for (int i = 0; i < Player.MaxNumPlayers; ++i)
                damage[i] *= damageFactor*timeInterval;

            // nobody owns this
            if (PossessingPlayer == -1)
            {
                for (int i = 0; i < Player.MaxNumPlayers; ++i)
                {
                    if (CapturingPlayer == i)
                        PossessingPercentage += damage[i];
                    else
                        PossessingPercentage -= damage[i];
                }

                // got it
                if (PossessingPercentage > 1.0f)
                {
                    PossessingPlayer = CapturingPlayer;
                    PossessingPercentage = 1.0f;
                    OnPossessingChanged();
                }

                // new capturer
                else if (PossessingPercentage < 0.0f)
                    NewCapturer(damage);
            }

            // owned
            else
            {
                for (int i = 0; i < Player.MaxNumPlayers; ++i)
                {
                    if (PossessingPlayer == i)
                        PossessingPercentage += damage[i] * defenseFactor;
                    else
                        PossessingPercentage -= damage[i];
                }
                // now nobody owns this
                if (PossessingPercentage < 0.0f)
                {
                    NewCapturer(damage);
                    PossessingPlayer = -1;
                    OnPossessingChanged();
                }
                else if (PossessingPercentage > 1.0f)
                    PossessingPercentage = 1.0f;
            }
        }

        private void NewCapturer(float[] damage)
        {
            PossessingPercentage = 0.0f;
            CapturingPlayer = -1;
            float maxDamage = 0.0f;
            for (int i = 0; i < Player.MaxNumPlayers; ++i)
            {
                if (damage[i] > maxDamage)
                {
                    maxDamage = damage[i];
                    CapturingPlayer = i;
                }
            }
        }

        public Color ComputeColor()
        {
            if (PossessingPlayer != -1)
                return Color.Lerp(Color.LightGray, Settings.Instance.GetPlayerColor(PossessingPlayer), PossessingPercentage);
            else if (CapturingPlayer != -1)
                return Color.Lerp(Color.LightGray, Settings.Instance.GetPlayerColor(CapturingPlayer), PossessingPercentage);
            else
                return Color.LightGray;
        }
    }
}
