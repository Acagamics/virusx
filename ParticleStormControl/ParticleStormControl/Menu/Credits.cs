using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParticleStormControl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ParticleStormControl.Menu
{
    class Credits : MenuPage
    {
        private Texture2D logo;
        private Texture2D acagamicsLogo;

        public Credits(Menu menu)
            : base(menu)
        { }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            logo = content.Load<Texture2D>("logo");
            acagamicsLogo = content.Load<Texture2D>("acagamicslogo");
        }

        public override void Update(float frameTimeInterval)
        {
            // back to game
            if (InputManager.Instance.PauseButton() || InputManager.Instance.ContinueButton() || InputManager.Instance.PressedButton(Keys.Escape))
                menu.ActivePage = Menu.Page.MAINMENU;
        }

        public override void Draw(SpriteBatch spriteBatch, float frameTimeInterval)
        {
            spriteBatch.Draw(logo, new Vector2(menu.ScreenWidth- logo.Width + 200, menu.ScreenHeight - logo.Height + 300), Color.White);
            spriteBatch.Draw(acagamicsLogo, new Vector2(menu.ScreenWidth - acagamicsLogo.Width - 50, 50), Color.White);

            SimpleButton.Draw(spriteBatch, menu.FontHeading, "A totally awesome game by", new Vector2(100, 100), false, menu.PixelTexture);
            SimpleButton.Draw(spriteBatch, menu.Font, "Andreas, Fritz, Sebastian and Tim", new Vector2(100, 160), false, menu.PixelTexture);
            SimpleButton.Draw(spriteBatch, menu.Font, "(Prototype on the Global Game Jam 2012)", new Vector2(100, 220), false, menu.PixelTexture);


            SimpleButton.Draw(spriteBatch, menu.FontHeading, "Rewritten and deluxified by", new Vector2(100, 310), false, menu.PixelTexture);
            SimpleButton.Draw(spriteBatch, menu.Font, "Andreas Reich", new Vector2(100, 370), false, menu.PixelTexture);

            SimpleButton.Draw(spriteBatch, menu.FontHeading, "XBox360-Support & Testing by", new Vector2(100, 460), false, menu.PixelTexture);
            SimpleButton.Draw(spriteBatch, menu.Font, "Enrico", new Vector2(100, 520), false, menu.PixelTexture);

            SimpleButton.Draw(spriteBatch, menu.Font, "I like that!", new Vector2(100, 610), true, menu.PixelTexture);
        }
    }
}
