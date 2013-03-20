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

        public override void Initialize()
        {
            
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            logo = content.Load<Texture2D>("logo");
            acagamicsLogo = content.Load<Texture2D>("acagamicslogo");
        }

        public override void Update(float frameTimeInterval)
        {
            // back to game
            if (InputManager.Instance.PauseButton() || InputManager.Instance.ContinueButton())
                menu.ActivePage = Menu.Page.MAINMENU;
        }

        private static readonly Color CaptionColor = Color.FromNonPremultiplied(120, 120, 120, 255);
        private static readonly Color PersonColor = Color.FromNonPremultiplied(70, 70, 70, 255);

        public override void Draw(SpriteBatch spriteBatch, float frameTimeInterval)
        {
            spriteBatch.Draw(logo, new Vector2(menu.ScreenWidth- logo.Width + 200, menu.ScreenHeight - logo.Height + 300), Color.White);
            spriteBatch.Draw(acagamicsLogo, new Vector2(menu.ScreenWidth - acagamicsLogo.Width, 0), Color.White);
            spriteBatch.DrawString(menu.Font, "Particle Storm Control ~Deluxe~", new Vector2(20, 20), Color.FromNonPremultiplied(50, 50, 50, 255));


            spriteBatch.DrawString(menu.FontSmall, "A totally awesome game by", new Vector2(20, 80), CaptionColor);
            spriteBatch.DrawString(menu.FontSmall, "Andreas, Fritz, Sebastian and Tim", new Vector2(20, 110), PersonColor);
            spriteBatch.DrawString(menu.FontSmall, "(Prototyp on the Global Game Jam 2012)", new Vector2(20, 140), CaptionColor);
            

            spriteBatch.DrawString(menu.FontSmall, "Rewritten and deluxified by", new Vector2(20, 200), CaptionColor);
            spriteBatch.DrawString(menu.FontSmall, "Andreas Reich", new Vector2(20, 230), PersonColor);

            spriteBatch.DrawString(menu.FontSmall, "XBox360-Support & Testing by", new Vector2(20, 270), CaptionColor);
            spriteBatch.DrawString(menu.FontSmall, "Enrico", new Vector2(20, 300), PersonColor);
        }
    }
}
