using Microsoft.Xna.Framework;
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
        public virtual void OnActivated(Menu.Page oldPage) { }
        public abstract void LoadContent(ContentManager content);
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch, float frameTimeInterval);
    }
}
