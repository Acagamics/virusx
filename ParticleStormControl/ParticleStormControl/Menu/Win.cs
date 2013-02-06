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
    class Win : MenuPage
    {
        Texture2D logo;

        public int ShownWinnerColorIndex { get; set; }

        public Win(Menu menu)
            : base(menu)
        { }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            logo = content.Load<Texture2D>("logo");
        }

        public override void Update(float frameTimeInterval)
        {
            if (InputManager.Instance.ContinueButton())
                menu.ChangePage(Menu.Page.MAINMENU);
        }

        public override void Draw(SpriteBatch spriteBatch, float frameTimeInterval)
        {
            spriteBatch.Draw(logo, new Vector2(0, menu.ScreenHeight - logo.Height + 200), Color.White);
            string text = Player.Names[ShownWinnerColorIndex] + " wins!";
            Vector2 stringSize = menu.Font.MeasureString(text);
            spriteBatch.DrawString(menu.Font, text, (new Vector2(menu.ScreenWidth, menu.ScreenHeight) - menu.Font.MeasureString(text)) *0.5f, Player.Colors[ShownWinnerColorIndex]);
        }
    }
}
