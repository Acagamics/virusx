using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace VirusX
{
    abstract class CapturableObject : MapObject
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

        protected float damageFactor;

        /// <summary>
        /// the opacity of the object it is used to let objects blink
        /// </summary>
        protected float opacity = 1f;

        protected bool captureable = true;
        public bool Captureable { get { return captureable; } }

        public CapturableObject(Vector2 Position, int possessingPlayer, float damageFactor, float lifeTime/* = -1.0f*/, int damageMapPixelHalfRange /*= 4*/, float size = 0.06f) :
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

        public override void Update(GameTime gameTime)
        {
            if (lifeTime > 0)
            {
                if(lifeTimer.Elapsed.TotalSeconds > lifeTime * 0.75f)
                    opacity = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 10.0f) * 0.4f + 0.5f;
                else opacity = 1f;            
            }
            else opacity = 1f;
            
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

        /// <summary>
        /// computes summed and scaled (damageFactor) damage in the area of this object
        /// </summary>
        protected float[] GetDamageInArea(DamageMap damageMap, float timeInterval)
        {
            float[] damage = new float[Settings.Instance.NumPlayers];
          //  int[] damageMapChannels = new int[damage.Length];
          //  for (int i = 0; i < damage.Length; ++i)
          //      damageMapChannels[i] = VirusSwarm.GetDamageColorIndexFromTeam(i);

            for (int y = damageMap_MinY; y <= damageMap_MaxY; ++y)
            {
                for (int x = damageMap_MinX; x <= damageMap_MaxX; ++x)
                {
                    for (int i = 0; i < damage.Length; ++i)
                        damage[i] += damageMap.GetPlayerDamageAt(x, y, i);
                }
            }
            for (int i = 0; i < damage.Length; ++i)
                damage[i] *= damageFactor*timeInterval;

            return damage;
        }

        public override void ApplyDamage(DamageMap damageMap, float timeInterval)
        {
            float[] damage = GetDamageInArea(damageMap, timeInterval);

            // nobody owns this
            if (PossessingPlayer == -1)
            {
                for (int i = 0; i < damage.Length; ++i)
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
            else if(captureable)
            {
                for (int i = 0; i < damage.Length; ++i)
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
            for (int i = 0; i < damage.Length; ++i)
            {
                if (damage[i] > maxDamage)
                {
                    maxDamage = damage[i];
                    CapturingPlayer = i;
                }
            }
        }

        virtual public Color ComputeColor()
        {
            Color resColor;
            if (PossessingPlayer != -1)
                resColor = Color.Lerp(Color.White, Settings.Instance.GetPlayerColor(PossessingPlayer), PossessingPercentage);
            else if (CapturingPlayer != -1)
                resColor = Color.Lerp(Color.White, Settings.Instance.GetPlayerColor(CapturingPlayer), PossessingPercentage);
            else
                resColor = Color.White;

            Vector4 w = resColor.ToVector4();
            w.W = opacity;
            return new Color(w);
        }
    }
}
