using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VirusX.Menu
{
    class Credits : MenuPage
    {
        TimeSpan entry;

        int offset;

        public Credits(Menu menu)
            : base(menu)
        {
            Initialize();
        }

        private void Initialize()
        {
            Interface.Add(new InterfaceImage("logoNew", new Vector2(350, -250 / 2), Alignment.CENTER_RIGHT));

            Interface.Add(new InterfaceButton(VirusXStrings.Instance.CreditsGameBy, new Vector2(100, Settings.Instance.ResolutionY), true));

            List<string> names = new List<string>() {
                "Andreas Reich", VirusXStrings.Instance.CreditsAndreas,
                "Enrico Gebert", VirusXStrings.Instance.CreditsEnrico,
                "Sebastian Lay", VirusXStrings.Instance.CreditsSebastian,
                "Maria Manneck", VirusXStrings.Instance.CreditsMaria
            };
            AddNames(names, Settings.Instance.ResolutionY + 200);

            names = new List<string>() {
                VirusXStrings.Instance.CreditsThanks,
                VirusXStrings.Instance.CreditsThanksNames
            };
            AddNames(names, Settings.Instance.ResolutionY + 1100);

            Interface.Add(new InterfaceButton(VirusXStrings.Instance.CreditsMusic, new Vector2(100, Settings.Instance.ResolutionY + 1500), true));

            names = new List<string>() {
                "Beach - PaulFitzZaland", "soundcloud.com/paulfitzzaland",
                "Light Switch of doom - CosmicD", "freesound.org/people/CosmicD",
                "snare - switchy - room", "freesound.org/people/room",
                "Woosh.01 - Andromadax24", "freesound.org/people/Andromadax24"
            };
            AddNames(names, Settings.Instance.ResolutionY + 1700);

            Interface.Add(new InterfaceImage("credits/acagamicslogo", new Vector2(-500 / 2, Settings.Instance.ResolutionY + 2600), Alignment.TOP_CENTER));

            Interface.Add(new InterfaceImage("credits/Gruppe1", new Vector2(-768 / 2, Settings.Instance.ResolutionY + 9001), Alignment.TOP_CENTER));
        }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            entry = gameTime.TotalGameTime;
            Interface.Clear();
            Initialize();
            base.LoadContent(menu.Game.Content);
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

            offset /= 2;
            if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.UP))
                offset = 10;
            if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.DOWN))
                offset = -10;

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // update position for scrolling effect
            for (int i = 0; i < Interface.Count; i++)
            {
                Interface[i].Position -= new Vector2(0, (float)gameTime.ElapsedGameTime.Milliseconds / 10 + offset);
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
