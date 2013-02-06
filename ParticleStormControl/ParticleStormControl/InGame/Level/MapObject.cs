using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ParticleStormControl
{
    public abstract class MapObject
    {
        protected MapObject(Vector2 PositionIn, float size /*= 0.05f*/)
        {
            Size = size;
            Position = PositionIn;
            Alive = true;
        }

        public float Size { get; set; }
        public Vector2 Position { get; set; }
        public bool Alive { get; set; }

        public virtual void SwitchPlayer(int[] playerSwitchedTo)
        {}

        public virtual void ApplyDamage(DamageMap damageMap, float timeInterval)
        {}

        public virtual void DrawToDamageMap(SpriteBatch spriteBatch)
        {}

        public virtual void Update(float frameTimeSeconds, float totalTimeSeconds)
        {}

        public abstract void Draw_AlphaBlended(SpriteBatch spriteBatch, Level level, float totalTimeSeconds);
        public virtual void Draw_ScreenBlended(SpriteBatch spriteBatch, Level level, float totalTimeSeconds)
        {}
    }
}
