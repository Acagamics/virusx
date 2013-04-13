using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

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

        public const int PADDING = 10;
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
        /// <param name="width">override the original width, 0 for auto, -1 for none</param>
        public void Draw(SpriteBatch spriteBatch, SpriteFont font, string label, Vector2 position, bool selected, Texture2D texture, int width = 0)
        {
            if(width >= 0)
                DrawBackground(spriteBatch, position, selected && !IsAnimated() ? normalColor : selectedColor, texture, font.MeasureString(label), false, width); // do not try to understand this
            
            if (IsAnimated() && selected)
                DrawBackground(spriteBatch, position, normalColor, texture, font.MeasureString(label), true, width);

            spriteBatch.DrawString(font, label, position, selected ? selectedColor : normalColor);
        }

        /// <summary>
        /// Draws a string with a colored background (can't be selected)
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="font"></param>
        /// <param name="label"></param>
        /// <param name="position"></param>
        /// <param name="backgroundColor"></param>
        /// <param name="texture">The pixel texture</param>
        /// <param name="width">override the original width</param>
        public void Draw(SpriteBatch spriteBatch, SpriteFont font, string label, Vector2 position, Color backgroundColor, Texture2D texture, int width = 0)
        {
            DrawBackground(spriteBatch, position, backgroundColor, texture, font.MeasureString(label), false, width);
            spriteBatch.DrawString(font, label, position, normalColor);
        }

        /// <summary>
        /// Draws a string with a colored background (can't be selected) and a non-standard colored string
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="font"></param>
        /// <param name="label"></param>
        /// <param name="position"></param>
        /// <param name="backgroundColor"></param>
        /// <param name="texture">The pixel texture</param>
        /// <param name="width">override the original width</param>
        public void Draw(SpriteBatch spriteBatch, SpriteFont font, string label, Vector2 position, Color backgroundColor, Color textColor, Texture2D texture, int width = 0)
        {
            DrawBackground(spriteBatch, position, backgroundColor, texture, font.MeasureString(label), false, width);
            spriteBatch.DrawString(font, label, position, textColor);
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
            DrawBackground(spriteBatch, new Vector2(destinationRectangle.X, destinationRectangle.Y), selected ? normalColor : selectedColor, background, new Vector2(sourceRectangle.Width, sourceRectangle.Height), false, width);
         //   destinationRectangle.Offset(4, 4);
            spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, Color.White);
        }

        /// <summary>
        /// same as DrawTexture but without background padding and scaling of the texture - the only rect given is the background
        /// the graphic will then be placed in the middle of this rect
        /// </summary>
        public void DrawTexture_NoScalingNoPadding(SpriteBatch spriteBatch, Texture2D texture, Rectangle backgroundRectangle, Rectangle sourceRectangle, bool selected, Texture2D background, int width = 0)
        {
            DrawBackgroundNoPadding(spriteBatch, backgroundRectangle, selected ? normalColor : selectedColor, background, false, width);
            Rectangle destinationRectangle = new Rectangle();
            destinationRectangle.Width = sourceRectangle.Width;
            destinationRectangle.Height = sourceRectangle.Height;
            destinationRectangle.X = backgroundRectangle.Center.X - sourceRectangle.Width / 2;
            destinationRectangle.Y = backgroundRectangle.Center.Y - sourceRectangle.Height / 2;
            spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, Color.White);
        }

        private void DrawBackgroundNoPadding(SpriteBatch spriteBatch, Rectangle rect, Color color, Texture2D texture, bool animated, int width = 0)
        {
            spriteBatch.Draw(texture, rect, color);
        }

        private void DrawBackground(SpriteBatch spriteBatch, Vector2 position, Color color, Texture2D texture, Vector2 size, bool animated, int width = 0)
        {
            int _width = (int)size.X + 2 * PADDING;
            if (width > 0)
                _width = width;

            int _height = (int)size.Y + 2 * PADDING;
            if (animated)
                _height = GetHeight(_height);

            spriteBatch.Draw(texture, new Rectangle((int)position.X - PADDING, (int)position.Y - PADDING, _width, _height), color);
        }

        /// <summary>
        /// Call this function when the selection changed!
        /// </summary>
        /// <param name="gameTime"></param>
        public void ChangeHappened(GameTime gameTime, SoundEffect sound)
        {
            lastChange = gameTime.TotalGameTime;
            if(Settings.Instance.Sound)
                sound.Play();
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
            return (int)((currentTime.Subtract(lastChange).TotalMilliseconds * maxHeight) / animationDuration.TotalMilliseconds);
        }

        private bool IsAnimated()
        {
            return currentTime.Subtract(lastChange) < animationDuration;
        }
    }
}
