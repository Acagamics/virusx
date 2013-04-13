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
        //private Texture2D logo;
        private Texture2D acagamicsLogo;
        private Texture2D team;

        public Credits(Menu menu)
            : base(menu)
        { }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            //logo = content.Load<Texture2D>("logo");
            acagamicsLogo = content.Load<Texture2D>("acagamicslogo");
            team = content.Load<Texture2D>("Gruppe1");
        }

        public override void Update(GameTime gameTime)
        {
            // back to main menu
            if (InputManager.Instance.PauseButton() || InputManager.Instance.ContinueButton() || InputManager.Instance.PressedButton(Buttons.B) || InputManager.Instance.PressedButton(Keys.Escape) || InputManager.Instance.ExitButton())
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, float frameTimeInterval)
        {
            //spriteBatch.Draw(logo, new Vector2(menu.ScreenWidth- logo.Width + 200, menu.ScreenHeight - logo.Height + 300), Color.White);
            //spriteBatch.Draw(acagamicsLogo, new Vector2(menu.ScreenWidth - acagamicsLogo.Width - 50, 50), Color.White);
            //spriteBatch.Draw(team, new Rectangle(menu.ScreenWidth - (team.Width/2) - 105, menu.ScreenHeight - (team.Height / 2) + 100, team.Width / 2, team.Height / 2), Color.White);

            SimpleButton.Instance.Draw(spriteBatch, menu.FontHeading, "Andreas Reich", new Vector2(100, 100), false, menu.TexPixel);
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Programming, Gamplay, Graphics", new Vector2(100, 150), false, menu.TexPixel);
            
            SimpleButton.Instance.Draw(spriteBatch, menu.FontHeading, "Enrico Gebert:", new Vector2(100, 210), false, menu.TexPixel);
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Programming, Gamplay, Balancing", new Vector2(100, 260), false, menu.TexPixel);

            SimpleButton.Instance.Draw(spriteBatch, menu.FontHeading, "Maria Manneck:", new Vector2(100, 320), false, menu.TexPixel);
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "2D Arts, Interface", new Vector2(100, 370), false, menu.TexPixel);
            
            SimpleButton.Instance.Draw(spriteBatch, menu.FontHeading, "Sebastian Lay:", new Vector2(100, 430), false, menu.TexPixel);
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Programming, Interface, Musik/Sound", new Vector2(100, 480), false, menu.TexPixel);
        }
    }
}
