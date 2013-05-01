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
        private TimeSpan entry;

        public Credits(Menu menu)
            : base(menu)
        {
            Interface.Add(new InterfaceImage("logoNew", new Vector2(350, -250 / 2), Alignment.CENTER_RIGHT));

            Interface.Add(new InterfaceButton("A game made by", new Vector2(100, Settings.Instance.ResolutionY), true)); 

            List<string> names = new List<string>() {
                "Andreas Reich", "Programming, Gamplay, Graphics",
                "Enrico Gebert", "Programming, Gamplay, Balancing",
                "Sebastian Lay", "Programming, Interface, Musik/Sound",
                "Maria Manneck", "2D Arts, Interface"
            };
            AddNames(names, Settings.Instance.ResolutionY + 200);

            Interface.Add(new InterfaceButton("Sounds/Music", new Vector2(100, Settings.Instance.ResolutionY + 1200), true)); 

            names = new List<string>() {
                "Beach - PaulFitzZaland", "soundcloud.com/paulfitzzaland",
                "Light Switch of doom - CosmicD", "freesound.org/people/CosmicD",
                "snare - switchy - room", "freesound.org/people/room",
                "Woosh.01 - Andromadax24", "freesound.org/people/Andromadax24"
            };
            AddNames(names, Settings.Instance.ResolutionY + 1400);

            Interface.Add(new InterfaceImage("acagamicslogo", new Vector2(-500 / 2, Settings.Instance.ResolutionY + 2400), Alignment.TOP_CENTER));

            Interface.Add(new InterfaceImage("Gruppe1", new Vector2(-768 / 2, Settings.Instance.ResolutionY + 9001), Alignment.TOP_CENTER));
        }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            entry = gameTime.TotalGameTime;
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            base.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            menu.BackToMainMenu(gameTime);

            if (gameTime.TotalGameTime.Subtract(entry) > TimeSpan.FromSeconds(120))
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // update position for scrolling effect
            for (int i = 0; i < Interface.Count; i++)
            {
                Interface[i].Position -= new Vector2(0, (float)gameTime.ElapsedGameTime.Milliseconds / 10);
            }

            base.Draw(spriteBatch, gameTime);
        }

        private void AddNames(List<string> names, int offset)
        {
            for (int i = 0; i < names.Count; i++)
            {
                int alternate = i % 2 == 1 ? 60 : 0;
                Interface.Add(new InterfaceButton(names[i], new Vector2(100, (int)(i / 2) * 200 + offset + alternate), i % 2 == 0));
            }
        }
    }
}
