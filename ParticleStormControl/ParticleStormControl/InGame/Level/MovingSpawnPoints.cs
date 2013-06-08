using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

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
        /// time in seconds until the spawn will fade
        /// </summary>
        private float remainingLifeTime;

        /// <summary>
        /// link between spawnsize and lifetime: startlifetime = spawnsize * thisconst
        /// </summary>
        const float SPAWNSIZE_TO_LIFETIME = 0.1f;

        public MovingSpawnPoint(Vector2 direction, Vector2 startPosition, 
                            float spawnSize, int startPosession, ContentManager content) : 
            base(startPosition, spawnSize, startPosession, content)
        {
            this.direction = direction;
            remainingLifeTime = spawnSize * SPAWNSIZE_TO_LIFETIME;
        }

        public override void Update(GameTime gameTime)
        {
            float speed = 0.2f;
            this.Position += direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);

            remainingLifeTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (remainingLifeTime < 0)
                Alive = false;
        }
    }
}
