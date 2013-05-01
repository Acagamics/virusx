using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace ParticleStormControl.Menu
{
    class InterfaceButton : InterfaceElement
    {
        #region local variables

        Alignment alignment;
        Func<bool> selected;
        Func<string> text;
        bool selectedPrevious;
        bool selectedNow;
        bool useBigFont;
        int width;
        Color textColor;
        Color backgroundColor;
        Texture2D texture;
        SpriteFont fontSmall;
        SpriteFont fontBig;
        SoundEffect soundEffect;

        #endregion

        #region properties

        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }

        #endregion

        #region constructors

        public InterfaceButton(string text, Vector2 position, Alignment alignment = Alignment.TOP_LEFT)
            : this(text, position, () => { return false; }, () => { return true; }, - 1, COLOR_HIGHLIGHT, COLOR_NORMAL, false, alignment)
        { }

        public InterfaceButton(string text, Vector2 position, bool useBigFont, Alignment alignment = Alignment.TOP_LEFT)
            : this(text, position, () => { return false; }, () => { return true; }, - 1, COLOR_HIGHLIGHT, COLOR_NORMAL, useBigFont, alignment)
        { }

        public InterfaceButton(string text, Vector2 position, Func<bool> selected, Alignment alignment = Alignment.TOP_LEFT)
            : this(text, position, selected, () => { return true; }, - 1, COLOR_HIGHLIGHT, COLOR_NORMAL, false, alignment)
        { }

        public InterfaceButton(string text, Vector2 position, Func<bool> selected, Func<bool> visible, Alignment alignment = Alignment.TOP_LEFT)
            : this(text, position, selected, visible, -1, COLOR_HIGHLIGHT, COLOR_NORMAL, false, alignment)
        { }

        public InterfaceButton(string text, Vector2 position, Func<bool> selected, bool useBigFont, Alignment alignment = Alignment.TOP_LEFT)
            : this(text, position, selected, () => { return true; }, -1, COLOR_HIGHLIGHT, COLOR_NORMAL, useBigFont, alignment)
        { }

        public InterfaceButton(string text, Vector2 position, Func<bool> selected, int width, Alignment alignment = Alignment.TOP_LEFT)
            : this(text, position, selected, () => { return true; }, width, COLOR_HIGHLIGHT, COLOR_NORMAL, false, alignment)
        { }

        public InterfaceButton(string text, Vector2 position, Func<bool> selected, Color textColor, Color backgroundColor, Alignment alignment = Alignment.TOP_LEFT)
            : this(text, position, selected, () => { return true; }, - 1, textColor, backgroundColor, false, alignment)
        { }

        public InterfaceButton(string text, Vector2 position, Func<bool> selected, int width, Color textColor, Color backgroundColor, Alignment alignment = Alignment.TOP_LEFT)
            : this(text, position, selected, () => { return true; }, width, textColor, backgroundColor, false, alignment)
        { }

        public InterfaceButton(string text, Vector2 position, Func<bool> selected, Func<bool> visible, int width, Color textColor, Color backgroundColor, bool useBigFont, Alignment alignment = Alignment.TOP_LEFT)
            : this(() => { return text; }, position, selected, visible, width, textColor, backgroundColor, useBigFont, alignment)
        { }

        public InterfaceButton(Func<string> text, Vector2 position, Alignment alignment = Alignment.TOP_LEFT)
            : this(text, position, () => { return false; }, () => { return true; }, -1, COLOR_HIGHLIGHT, COLOR_NORMAL, false, alignment)
        { }

        public InterfaceButton(Func<string> text, Vector2 position, bool useBigFont, Alignment alignment = Alignment.TOP_LEFT)
            : this(text, position, () => { return false; }, () => { return true; }, -1, COLOR_HIGHLIGHT, COLOR_NORMAL, useBigFont, alignment)
        { }

        public InterfaceButton(Func<string> text, Vector2 position, Func<bool> selected, Alignment alignment = Alignment.TOP_LEFT)
            : this(text, position, selected, () => { return true; }, -1, COLOR_HIGHLIGHT, COLOR_NORMAL, false, alignment)
        { }

        public InterfaceButton(Func<string> text, Vector2 position, Func<bool> selected, Func<bool> visible, Alignment alignment = Alignment.TOP_LEFT)
            : this(text, position, selected, visible, -1, COLOR_HIGHLIGHT, COLOR_NORMAL, false, alignment)
        { }

        public InterfaceButton(Func<string> text, Vector2 position, Func<bool> selected, Func<bool> visible, bool useBigFont, Alignment alignment = Alignment.TOP_LEFT)
            : this(text, position, selected, visible, -1, COLOR_HIGHLIGHT, COLOR_NORMAL, useBigFont, alignment)
        { }

        public InterfaceButton(Func<string> text, Vector2 position, Func<bool> selected, bool useBigFont, Alignment alignment = Alignment.TOP_LEFT)
            : this(text, position, selected, () => { return true; }, -1, COLOR_HIGHLIGHT, COLOR_NORMAL, useBigFont, alignment)
        { }

        public InterfaceButton(Func<string> text, Vector2 position, Func<bool> selected, int width, Alignment alignment = Alignment.TOP_LEFT)
            : this(text, position, selected, () => { return true; }, width, COLOR_HIGHLIGHT, COLOR_NORMAL, false, alignment)
        { }

        public InterfaceButton(string text, Vector2 position, Func<bool> selected, Func<bool> visible, int width, Alignment alignment = Alignment.TOP_LEFT)
            : this(() => { return text; }, position, selected, visible, width, COLOR_HIGHLIGHT, COLOR_NORMAL, false, alignment)
        { }

        public InterfaceButton(Func<string> text, Vector2 position, Func<bool> selected, Func<bool> visible, int width, Alignment alignment = Alignment.TOP_LEFT)
            : this(text, position, selected, visible, width, COLOR_HIGHLIGHT, COLOR_NORMAL, false, alignment)
        { }

        public InterfaceButton(Func<string> text, Vector2 position, Func<bool> selected, Color textColor, Color backgroundColor, Alignment alignment = Alignment.TOP_LEFT)
            : this(text, position, selected, () => { return true; }, -1, textColor, backgroundColor, false, alignment)
        { }

        public InterfaceButton(Func<string> text, Vector2 position, Func<bool> selected, int width, Color textColor, Color backgroundColor, Alignment alignment = Alignment.TOP_LEFT)
            : this(text, position, selected, () => { return true; }, width, textColor, backgroundColor, false, alignment)
        { }

        /// <summary>
        /// Button with fixed width, colors and font
        /// </summary>
        /// <param name="text">The text (self updating)</param>
        /// <param name="position">Upper left corner (inclusive padding)</param>
        /// <param name="selected">A method that returns if the button is selected</param>
        /// <param name="visible">A method that returns if the button is visible</param>
        /// <param name="width">The width of the button (inclusive padding)</param>
        /// <param name="textColor">Color of the font</param>
        /// <param name="backgroundColor">Color of the background</param>
        /// <param name="useBigFont">Use a bigger font</param>
        /// <param name="alignment">Aligns the complete button to the viewport</param>
        public InterfaceButton(Func<string> text, Vector2 position, Func<bool> selected, Func<bool> visible, int width, Color textColor, Color backgroundColor, bool useBigFont, Alignment alignment = Alignment.TOP_LEFT)
        {
            this.text = text;
            this.width = width;
            this.useBigFont = useBigFont;
            this.selected = selected;
            this.textColor = textColor;
            this.backgroundColor = backgroundColor;
            this.alignment = alignment;
            this.position = position;
            this.visible = visible;
        }

        #endregion

        #region methods

        public override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("pix");
            fontSmall = content.Load<SpriteFont>("fonts/font");
            fontBig = content.Load<SpriteFont>("fonts/fontHeading");
            soundEffect = content.Load<SoundEffect>("sound/room__snare-switchy");
        }

        public override void Update(GameTime gameTime)
        {
            // evaluate if selected
            selectedPrevious = selectedNow;
            selectedNow = selected();
            
            // play sound if button got selected
            if(selectedNow && !selectedPrevious && Settings.Instance.Sound)
                soundEffect.Play();
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            string _text = text();
            SpriteFont _font = useBigFont ? fontBig : fontSmall;
            Point _position = CalculateAlignedPosition(Position, alignment);
            int _width = width < 0 ? (int)_font.MeasureString(_text).X : width;
            int _height = (int)_font.MeasureString(_text).Y;
            Color _textColor = selectedNow ? backgroundColor : textColor;
            Color _backgroundColor = selectedNow ? textColor : backgroundColor;

            spriteBatch.Draw(texture, new Rectangle(_position.X, _position.Y, _width + 2 * PADDING, _height + 2 * PADDING), _backgroundColor);
            spriteBatch.DrawString(_font, _text, new Vector2(_position.X + PADDING, _position.Y + PADDING), _textColor);
        }

        #endregion
    }
}
