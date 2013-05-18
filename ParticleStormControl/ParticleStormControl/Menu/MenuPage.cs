using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ParticleStormControl.Menu
{
    abstract class MenuPage
    {
        /// <summary>
        /// Collection of all interface elements for the page
        /// </summary>
        internal List<InterfaceElement> Interface = new List<InterfaceElement>();

        protected Menu menu;
        protected MenuPage(Menu menu)
        {
            this.menu = menu;
        }

        /// <summary>
        /// Gets called everytime the page gets opened
        /// </summary>
        /// <param name="oldPage"></param>
        /// <param name="gameTime"></param>
        public virtual void OnActivated(Menu.Page oldPage, GameTime gameTime) { }

        /// <summary>
        /// Loads content
        /// </summary>
        /// <param name="content"></param>
        public virtual void LoadContent(ContentManager content)
        {
            foreach (InterfaceElement element in Interface)
            {
                element.LoadContent(content);
            }
        }
        
        /// <summary>
        /// Updates the interface and all specific content of the page
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            foreach (InterfaceElement element in Interface)
            {
                element.Update(gameTime);
            }
        }
        
        /// <summary>
        /// Draws the interface and all specific content of the page
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="gameTime"></param>
        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            foreach (InterfaceElement element in Interface)
            {
                if(element.Visible())
                    element.Draw(spriteBatch, gameTime);
            }
        }
    }
}
