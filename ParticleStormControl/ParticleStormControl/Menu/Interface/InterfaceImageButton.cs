using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace VirusX.Menu
{
    class InterfaceImageButton : InterfaceElement
    {
        #region local variables

        Alignment alignment;
        int width;
        int height;
        Rectangle? source;
        Rectangle? sourceSelected;
        Func<bool> selected;
        bool selectedNow;
        string textureName;
        Texture2D texture;
        Texture2D backgroundTexture;

        #endregion

        #region constructors

        public InterfaceImageButton(string textureName, Vector2 position, Func<bool> selected, Alignment alignment = Alignment.TOP_LEFT)
            : this(textureName, position, -1, -1, null, null, selected, () => { return true; }, alignment)
        {
        }

        public InterfaceImageButton(string textureName, Rectangle destination, Rectangle? source, Rectangle? sourceSelected, Func<bool> selected, Alignment alignment = Alignment.TOP_LEFT)
            : this(textureName, new Vector2(destination.X, destination.Y), destination.Width, destination.Height, source, sourceSelected, selected, () => { return true; }, alignment)
        {
        }

        public InterfaceImageButton(string textureName, Rectangle destination, Rectangle? source, Rectangle? sourceSelected, Func<bool> selected, Func<bool> visible, Alignment alignment = Alignment.TOP_LEFT)
            : this(textureName, new Vector2(destination.X, destination.Y), destination.Width, destination.Height, source, sourceSelected, selected, visible, alignment)
        {
        }

        public InterfaceImageButton(string textureName, Vector2 position, int width, int height, Rectangle? source, Rectangle? sourceSelected, Func<bool> selected, Func<bool> visible, Alignment alignment = Alignment.TOP_LEFT)
        {
            this.textureName = textureName;
            this.width = width;
            this.height = height;
            this.source = source;
            this.sourceSelected = sourceSelected;
            this.selected = selected;
            this.alignment = alignment;
            this.position = position;
            this.visible = visible;
        }

        #endregion

        #region methods

        public override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>(textureName);
            backgroundTexture = content.Load<Texture2D>("pix");
        }

        public override void Update(GameTime gameTime)
        {
            // evaluate if selected
            selectedNow = selected();
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Point _position = CalculateAlignedPosition(Position, alignment);
            Rectangle? _source = selectedNow ? sourceSelected : source;
            int _sourceWidth, _sourceHeight;
            if(_source != null)
            {
                _sourceWidth = _source.Value.Width;
                _sourceHeight = _source.Value.Height;
            }
            else
            {
                _sourceWidth = texture.Width;
                _sourceHeight = texture.Height;
            }

            int _width = width < 0 ? _sourceWidth : width;
            _width += 2 * PADDING;
            int _height = height < 0 ? _sourceHeight : height;
            _height += 2 * PADDING;
            Color _backgroundColor = selectedNow ? COLOR_NORMAL : COLOR_HIGHLIGHT;

            // the image itself gets drawn centered
            spriteBatch.Draw(backgroundTexture, new Rectangle(_position.X, _position.Y, _width, _height), _backgroundColor);
            spriteBatch.Draw(
                texture,
                new Rectangle(_position.X + (_width - _sourceWidth) / 2, _position.Y + (_height - _sourceHeight) / 2, _sourceWidth, _sourceHeight),
                selectedNow ? sourceSelected : source,
                Color.White
            );
        }

        #endregion
    }
}
