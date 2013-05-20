using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace VirusX.Menu
{
    class InterfaceFiller : InterfaceElement
    {
        Color color;
        Texture2D texture;
        int width;
        int height;

        public InterfaceFiller(Vector2 position, int width, int height, Color color, Func<bool> visible)
        {
            this.width = width;
            this.height = height;
            this.position = position;
            this.color = color;
            this.visible = visible;
        }

        public override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("pix");
        }

        public override void Update(GameTime gameTime)
        { }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(texture, new Rectangle((int)position.X, (int)position.Y, width, height), color);
        }
    }
}
