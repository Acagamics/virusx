﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ParticleStormControl.Menu
{
    class SimpleButton
    {
        #region singleton

        private static readonly SimpleButton instance = new SimpleButton();
        public static SimpleButton Instance { get { return instance; } }
        private SimpleButton() {}
        
        #endregion

        private readonly Color selectedColor = Color.Black;
        private readonly Color normalColor = Color.White;

        private readonly int padding = 10;
        private TimeSpan animationDuration = TimeSpan.FromMilliseconds(200);
        private TimeSpan lastChange = new TimeSpan();
        private TimeSpan currentTime = new TimeSpan();

        /// <summary>
        /// Draws a string with a background (
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="font"></param>
        /// <param name="label"></param>
        /// <param name="position"></param>
        /// <param name="selected"></param>
        /// <param name="texture">The pixel texture</param>
        /// <param name="width">override the original width</param>
        public void Draw(SpriteBatch spriteBatch, SpriteFont font, string label, Vector2 position, bool selected, Texture2D texture, int width = 0)
        {
            DrawBackground(spriteBatch, position, selected && !IsAnimated() ? normalColor : selectedColor, texture, font.MeasureString(label), false, width); // do not try to understand this
            spriteBatch.DrawString(font, label, position, selected ? selectedColor : normalColor);

            if (IsAnimated() && selected)
                DrawBackground(spriteBatch, position, normalColor, texture, font.MeasureString(label), true);
        }

        /// <summary>
        /// Draws a string with a colored background (can't be selected)
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="font"></param>
        /// <param name="label"></param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        /// <param name="texture">The pixel texture</param>
        /// <param name="width">override the original width</param>
        public void Draw(SpriteBatch spriteBatch, SpriteFont font, string label, Vector2 position, Color color, Texture2D texture, int width = 0)
        {
            DrawBackground(spriteBatch, position, color, texture, font.MeasureString(label), false, width);
            spriteBatch.DrawString(font, label, position, normalColor);
        }

        /// <summary>
        /// Draws e.g. an icon with a background
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="texture"></param>
        /// <param name="destinationRectangle"></param>
        /// <param name="sourceRectangle"></param>
        /// <param name="selected"></param>
        /// <param name="background"></param>
        /// <param name="width"></param>
        public void DrawTexture(SpriteBatch spriteBatch, Texture2D texture, Rectangle destinationRectangle, Rectangle sourceRectangle, bool selected, Texture2D background, int width = 0)
        {
            DrawBackground(spriteBatch, new Vector2(destinationRectangle.X, destinationRectangle.Y), selected ? selectedColor : normalColor, background, new Vector2(sourceRectangle.Width + 8, sourceRectangle.Height + 8), false, width);
            destinationRectangle.Offset(4, 4);
            spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, Color.White);
        }

        private void DrawBackground(SpriteBatch spriteBatch, Vector2 position, Color color, Texture2D texture, Vector2 size, bool animated, int width = 0)
        {
            int _width = (int)size.X + 2 * padding;
            if (width > 0)
                _width = width;
            
            int _height = (int)size.Y + 2 * padding;
            if (animated)
                _height = GetHeight(_height);
            

            spriteBatch.Draw(texture, new Rectangle((int)position.X - padding, (int)position.Y - padding, _width, _height), color);
        }

        /// <summary>
        /// Call this function when the selection changed!
        /// </summary>
        /// <param name="gameTime"></param>
        public void ChangeHappened(GameTime gameTime)
        {
            lastChange = gameTime.TotalGameTime;
        }

        public void Update(GameTime gameTime)
        {
            currentTime = gameTime.TotalGameTime;
        }

        /// <summary>
        /// Calculates the correct height for the animation
        /// </summary>
        /// <param name="maxHeight"></param>
        /// <returns></returns>
        private int GetHeight(int maxHeight)
        {
            return (int)((currentTime.Subtract(lastChange).TotalMilliseconds * maxHeight) / animationDuration.Milliseconds);
        }

        private bool IsAnimated()
        {
            return currentTime.Subtract(lastChange) < animationDuration;
        }
    }
}
