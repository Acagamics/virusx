using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace VirusX
{
    class MovingSpawnPoint : SpawnPoint
    {
        /// <summary>
        /// normalized direction
        /// </summary>
        private Vector2 direction;

        /// <summary>
        /// current speed of the spawn point
        /// </summary>
        //private float currentSpeed;


        /// <summary>
        /// link between spawnsize and lifetime: startlifetime = spawnsize * thisconst
        /// </summary>
        const float SPAWNSIZE_TO_LIFETIME = 0.02f;

        public MovingSpawnPoint(Vector2 direction, Vector2 startPosition, 
                            float spawnSize, int startPosession, ContentManager content) :
            base(startPosition, spawnSize, startPosession, content, spawnSize * SPAWNSIZE_TO_LIFETIME)
        {
            this.direction = direction;
            this.damageFactor *= 3f;

            this.nucleusTexture_outer = content.Load<Texture2D>("nucleus_outer_moving");
        }

        public override void Update(GameTime gameTime)
        {
            float speed = 5 / SpawnSize;
            this.Position += direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateDamageMapZoneFromPosition();

            base.Update(gameTime);
        }

        public override Color ComputeColor()
        {
            Color color = Settings.Instance.GetPlayerColor(PossessingPlayer);
            color.A = (byte)(255 * opacity);
            return color;
        }
    }
}
