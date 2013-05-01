﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace ParticleStormControl.Menu
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

        public InterfaceImage(string textureName, Rectangle destination, Alignment alignment = Alignment.TOP_LEFT)
            : this(textureName, new Vector2(destination.X, destination.Y), destination.Width, destination.Height, Color.FromNonPremultiplied(0, 0, 0, 0), () => { return true; }, alignment)
        { }

        public InterfaceImage(string textureName, Rectangle destination, Color backgroundColor, Alignment alignment = Alignment.TOP_LEFT)
            : this(textureName, new Vector2(destination.X, destination.Y), destination.Width, destination.Height, backgroundColor, () => { return true; }, alignment)
        { }

        public InterfaceImage(string textureName, Rectangle destination, Color backgroundColor, Func<bool> visible, Alignment alignment = Alignment.TOP_LEFT)
            : this(textureName, new Vector2(destination.X, destination.Y), destination.Width, destination.Height, backgroundColor, visible, alignment)
        { }

        public InterfaceImage(string textureName, Vector2 position, Alignment alignment = Alignment.TOP_LEFT)
            : this(textureName, position, -1, -1, Color.FromNonPremultiplied(0, 0, 0, 0), () => { return true; }, alignment)
        { }

        public InterfaceImage(string textureName, Vector2 position, int width, int height, Color backgroundColor, Func<bool> visible, Alignment alignment = Alignment.TOP_LEFT)
        {
            this.textureName = textureName;
            this.width = width;
            this.height = height;
            this.alignment = alignment;
            this.backgroundColor = backgroundColor;
            this.position = position;
            this.visible = visible;
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
            spriteBatch.Draw(texture, new Rectangle(_position.X + (_width - texture.Width) / 2, _position.Y + (_height - texture.Height) / 2, texture.Width, texture.Height), Color.White);
        }
    }
}