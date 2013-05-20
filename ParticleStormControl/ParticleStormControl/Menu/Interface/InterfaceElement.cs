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
    /// <summary>
    /// Base class for all interface elements (like buttons, labels, etc.)
    /// </summary>
    abstract class InterfaceElement
    {
        // common constants for all interface elements
        public static Color COLOR_NORMAL = Color.Black;
        public static Color COLOR_HIGHLIGHT = Color.White;
        public static int PADDING = 10;

        protected Vector2 position;
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        protected Func<bool> visible;
        public Func<bool> Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        public abstract void LoadContent(ContentManager content);

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);

        protected Point CalculateAlignedPosition(Vector2 position, Alignment alignment)
        {
            int top = 0, left = 0;
            switch (alignment)
            {
                case Alignment.TOP_LEFT:
                case Alignment.TOP_RIGHT:
                case Alignment.TOP_CENTER:
                    top = (int)position.Y;
                    break;
                case Alignment.CENTER_LEFT:
                case Alignment.CENTER_RIGHT:
                case Alignment.CENTER_CENTER:
                    top = Settings.Instance.ResolutionY / 2 + (int)Position.Y;
                    break;
                case Alignment.BOTTOM_LEFT:
                case Alignment.BOTTOM_RIGHT:
                case Alignment.BOTTOM_CENTER:
                    top = Settings.Instance.ResolutionY - (int)Position.Y;
                    break;
            }
            switch (alignment)
            {
                case Alignment.TOP_LEFT:
                case Alignment.CENTER_LEFT:
                case Alignment.BOTTOM_LEFT:
                    left = (int)position.X;
                    break;
                case Alignment.TOP_CENTER:
                case Alignment.CENTER_CENTER:
                case Alignment.BOTTOM_CENTER:
                    left = Settings.Instance.ResolutionX / 2 + (int)Position.X;
                    break;
                case Alignment.TOP_RIGHT:
                case Alignment.CENTER_RIGHT:
                case Alignment.BOTTOM_RIGHT:
                    left = Settings.Instance.ResolutionX - (int)Position.X;
                    break;
            }
            return new Point(left, top);
        }
    }

    enum Alignment
    {
        TOP_LEFT,
        TOP_RIGHT,
        TOP_CENTER,
        CENTER_LEFT,
        CENTER_RIGHT,
        CENTER_CENTER,
        BOTTOM_LEFT,
        BOTTOM_RIGHT,
        BOTTOM_CENTER
    }
}
