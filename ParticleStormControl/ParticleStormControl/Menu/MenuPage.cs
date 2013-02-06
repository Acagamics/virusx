using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ParticleStormControl.Menu
{
    public abstract class MenuPage
    {
        protected Menu menu;
        protected MenuPage(Menu menu)
        {
            this.menu = menu;
        }

        public abstract void LoadContent(ContentManager content);
        public abstract void Update(float frameTimeInterval);
        public abstract void Draw(SpriteBatch spriteBatch, float frameTimeInterval);
    }
}
