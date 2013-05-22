using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using CustomExtensions;

namespace VirusX.Menu
{
    class InterfaceTooltip : InterfaceElement
    {
        public enum ArrowPosition
        {
            TOP,
            RIGHT,
            BOTTOM,
            LEFT
        }

        const int ARROW_SIZE = 80;

        #region local variables

        Alignment alignment;
        ArrowPosition arrowPosition;
        Func<string> heading;
        Func<string> description;
        int width;
        Texture2D texture;
        Texture2D arrows;
        SpriteFont fontSmall;
        SpriteFont fontBig;

        #endregion

        #region constructors

        /// <summary>
        /// A tooltip with
        /// </summary>
        /// <param name="heading"></param>
        /// <param name="description"></param>
        /// <param name="position"></param>
        /// <param name="visible"></param>
        /// <param name="width"></param>
        /// <param name="alignment"></param>
        public InterfaceTooltip(Func<string> heading, Func<string> description, Vector2 position, Func<bool> visible, int width, ArrowPosition arrowPosition, Alignment alignment = Alignment.TOP_LEFT)
        {
            this.heading = heading;
            this.description = description;
            this.position = position;
            this.visible = visible;
            this.width = width;
            this.alignment = alignment;
            this.arrowPosition = arrowPosition;
        }

        #endregion

        #region methods

        public override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("pix");
            arrows = content.Load<Texture2D>("arrows");
            fontSmall = content.Load<SpriteFont>("fonts/font");
            fontBig = content.Load<SpriteFont>("fonts/fontHeading");
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Point _position = CalculateAlignedPosition(Position, alignment);
            int height = (int)(fontSmall.MeasureString(description()).Y) + (int)(fontBig.MeasureString(heading()).Y) + InterfaceElement.PADDING * 3;
            Point origin = GetOrigin(_position, height);
            Rectangle arrowRect = GetArrowRect(origin, height);

            spriteBatch.Draw(texture, new Rectangle(origin.X, origin.Y, width, height), InterfaceElement.COLOR_HIGHLIGHT);
            spriteBatch.Draw(arrows, arrowRect, GetArrowSource(), Color.White);
            spriteBatch.DrawString(fontBig, heading(), new Vector2(origin.X + InterfaceElement.PADDING, origin.Y + InterfaceElement.PADDING), InterfaceElement.COLOR_NORMAL);
            spriteBatch.DrawString(fontSmall, description(),
                new Vector2(origin.X + InterfaceElement.PADDING, origin.Y + InterfaceElement.PADDING * 2 + (int)(fontBig.MeasureString(heading()).Y)), InterfaceElement.COLOR_NORMAL);
        }

        private Point GetOrigin(Point position, int height)
        {
            switch (arrowPosition)
            {
                case ArrowPosition.TOP:
                    return new Point(position.X - width / 2, position.Y + ARROW_SIZE / 2);
                case ArrowPosition.RIGHT:
                    return new Point(position.X - width - ARROW_SIZE / 2, position.Y - height / 2);
                case ArrowPosition.BOTTOM:
                    return new Point(position.X - width / 2, position.Y - height - ARROW_SIZE / 2);
                case ArrowPosition.LEFT:
                    return new Point(position.X + ARROW_SIZE / 2, position.Y - height / 2);
                default:
                    return new Point();
            }
        }

        private Rectangle GetArrowRect(Point origin, int height)
        {
            switch (arrowPosition)
            {
                case ArrowPosition.TOP:
                    return new Rectangle(origin.X + width / 2 - ARROW_SIZE / 2, origin.Y - ARROW_SIZE / 2, ARROW_SIZE, ARROW_SIZE / 2);
                case ArrowPosition.RIGHT:
                    return new Rectangle(origin.X + width, origin.Y + height / 2 - ARROW_SIZE / 2, ARROW_SIZE / 2, ARROW_SIZE);
                case ArrowPosition.BOTTOM:
                    return new Rectangle(origin.X + width / 2 - ARROW_SIZE / 2, origin.Y + height, ARROW_SIZE, ARROW_SIZE / 2);
                case ArrowPosition.LEFT:
                    return new Rectangle(origin.X - ARROW_SIZE / 2, origin.Y + height / 2 - ARROW_SIZE / 2, ARROW_SIZE / 2, ARROW_SIZE);
                default:
                    return new Rectangle();
            }
        }

        private Rectangle GetArrowSource()
        {
            switch (arrowPosition)
            {
                case ArrowPosition.TOP:
                    return new Rectangle(0, 0, ARROW_SIZE, ARROW_SIZE / 2);
                case ArrowPosition.RIGHT:
                    return new Rectangle(ARROW_SIZE / 2, 0, ARROW_SIZE / 2, ARROW_SIZE);
                case ArrowPosition.BOTTOM:
                    return new Rectangle(0, ARROW_SIZE / 2, ARROW_SIZE, ARROW_SIZE / 2);
                case ArrowPosition.LEFT:
                    return new Rectangle(0, 0, ARROW_SIZE / 2, ARROW_SIZE);
                default:
                    return new Rectangle();
            }
        }

        #endregion
    }
}
