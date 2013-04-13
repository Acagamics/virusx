using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace ParticleStormControl.Menu
{
    class Viruses : MenuPage
    {
        public Viruses(Menu menu)
            : base(menu)
        { }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            // back to main menu
            if (InputManager.Instance.PauseButton()
                || InputManager.Instance.ContinueButton()
                || InputManager.Instance.PressedButton(Buttons.B)
                || InputManager.Instance.PressedButton(Keys.Escape)
                || InputManager.Instance.ExitButton())
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Microsoft.Xna.Framework.GameTime gameTime)
        {
        }
    }
}
