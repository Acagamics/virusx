using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VirusX
{
    abstract class MapObject
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

        public virtual void Update(GameTime gameTime)
        {}

        public abstract void Draw_AlphaBlended(SpriteBatch spriteBatch, Level level, GameTime gameTime);

        public virtual void Draw_Additive(SpriteBatch spriteBatch, Level level, GameTime gameTime) { }
    }
}
