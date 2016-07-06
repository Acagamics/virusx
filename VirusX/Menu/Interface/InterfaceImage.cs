using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace VirusX.Menu
{
    class InterfaceImage : InterfaceElement
    {
        Alignment alignment;
        Color backgroundColor;
        string textureName;
        Texture2D texture;
        Texture2D backgroundTexture;
        int width;
        int height;
        bool scaleImage;

        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        public InterfaceImage(string textureName, Rectangle destination, Alignment alignment = Alignment.TOP_LEFT, bool scaleImage = false)
            : this(textureName, new Vector2(destination.X, destination.Y), destination.Width, destination.Height, Color.FromNonPremultiplied(0, 0, 0, 0), () => { return true; }, alignment, scaleImage)
        { }

        public InterfaceImage(string textureName, Rectangle destination, Color backgroundColor, Alignment alignment = Alignment.TOP_LEFT, bool scaleImage = false)
            : this(textureName, new Vector2(destination.X, destination.Y), destination.Width, destination.Height, backgroundColor, () => { return true; }, alignment, scaleImage)
        { }

        public InterfaceImage(string textureName, Rectangle destination, Color backgroundColor, Func<bool> visible, Alignment alignment = Alignment.TOP_LEFT, bool scaleImage = false)
            : this(textureName, new Vector2(destination.X, destination.Y), destination.Width, destination.Height, backgroundColor, visible, alignment, scaleImage)
        { }

        public InterfaceImage(string textureName, Vector2 position, Alignment alignment = Alignment.TOP_LEFT, bool scaleImage = false)
            : this(textureName, position, -1, -1, Color.FromNonPremultiplied(0, 0, 0, 0), () => { return true; }, alignment, scaleImage)
        { }

        public InterfaceImage(string textureName, Vector2 position, int width, int height, Color backgroundColor, Func<bool> visible, Alignment alignment = Alignment.TOP_LEFT, bool scaleImage = false)
        {
            this.textureName = textureName;
            this.width = width;
            this.height = height;
            this.alignment = alignment;
            this.backgroundColor = backgroundColor;
            this.position = position;
            this.visible = visible;
            this.scaleImage = scaleImage;
        }

        public override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>(textureName);
            backgroundTexture = content.Load<Texture2D>("pix");
        }

        public override void Update(GameTime gameTime)
        { }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Point _position = CalculateAlignedPosition(Position, alignment);
            int _width = width < 0 ? texture.Width : width;
            int _height = height < 0 ? texture.Height : height;

            // if a background color is given: draw a background
            if(backgroundColor != Color.FromNonPremultiplied(0, 0, 0, 0))
                spriteBatch.Draw(backgroundTexture, new Rectangle(_position.X, _position.Y, _width, _height), backgroundColor);

            // the image itself gets drawn centered
            if(scaleImage)
                spriteBatch.Draw(texture, new Rectangle(_position.X, _position.Y, _width, _height), Color.White);
            else
                spriteBatch.Draw(texture, new Rectangle(_position.X + (_width - texture.Width) / 2, _position.Y + (_height - texture.Height) / 2, texture.Width, texture.Height), Color.White);
        }
    }
}
