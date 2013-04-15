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
        private Texture2D team;

        private TimeSpan entry;

        public Credits(Menu menu)
            : base(menu)
        { }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            entry = gameTime.TotalGameTime;
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            logo = content.Load<Texture2D>("logoNew");
            acagamicsLogo = content.Load<Texture2D>("acagamicslogo");
            team = content.Load<Texture2D>("Gruppe1");
        }

        public override void Update(GameTime gameTime)
        {
            // back to main menu
            if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.PAUSE) || 
                InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.EXIT) ||
                InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.ACTION) ||
                InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.HOLD) ||
                gameTime.TotalGameTime.Subtract(entry) > TimeSpan.FromSeconds(110))
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            
            int offset = Settings.Instance.ResolutionY - (int)(gameTime.TotalGameTime.Subtract(entry).TotalMilliseconds / 10);

            spriteBatch.Draw(logo, new Vector2(Settings.Instance.ResolutionX - logo.Width - 100, (int)Math.Min((Settings.Instance.ResolutionY - logo.Height) / 2, offset + 1800)), Color.White);

            SimpleButton.Instance.Draw(spriteBatch, menu.FontHeading, "A game made by", new Vector2(100, offset), false, menu.TexPixel);

            List<string> names = new List<string>() {
                "Andreas Reich", "Programming, Gamplay, Graphics",
                "Enrico Gebert", "Programming, Gamplay, Balancing",
                "Sebastian Lay", "Programming, Interface, Musik/Sound",
                "Maria Manneck", "2D Arts, Interface"
            };
            DrawNames(spriteBatch, names, offset + 200);

            SimpleButton.Instance.Draw(spriteBatch, menu.FontHeading, "Sounds/Music", new Vector2(100, offset + 1200), false, menu.TexPixel);

            names = new List<string>() {
                "Beach - PaulFitzZaland", "soundcloud.com/paulfitzzaland",
                "Light Switch of doom - CosmicD", "freesound.org/people/CosmicD",
                "snare - switchy - room", "freesound.org/people/room",
                "Woosh.01 - Andromadax24", "freesound.org/people/Andromadax24"
            };
            DrawNames(spriteBatch, names, offset + 1400);

            spriteBatch.Draw(acagamicsLogo, new Vector2((menu.ScreenWidth - acagamicsLogo.Width) / 2, offset + 2400), Color.White);

            spriteBatch.Draw(team, new Rectangle((Settings.Instance.ResolutionX - team.Width) / 2, (Settings.Instance.ResolutionY - team.Height) / 2 + offset + 9001, team.Width, team.Height), Color.White);
        }

        private void DrawNames(SpriteBatch spriteBatch, List<string> names, int offset)
        {
            for (int i = 0; i < names.Count; i++)
            {
                int alternate = i % 2 == 1 ? 60 : 0;
                if(i % 2 == 0)
                    SimpleButton.Instance.Draw(spriteBatch, menu.FontHeading, names[i], new Vector2(100, (int)(i / 2) * 200 + offset + alternate), false, menu.TexPixel);
                else
                    SimpleButton.Instance.Draw(spriteBatch, menu.Font, names[i], new Vector2(100, (int)(i / 2) * 200 + offset + alternate), false, menu.TexPixel);
            }
        }
    }
}
