﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirusX;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

            Interface.Add(new InterfaceButton("A game made by", new Vector2(100, Settings.Instance.ResolutionY), true));

            List<string> names = new List<string>() {
                "Andreas Reich", "Programming, Gameplay, Graphics",
                "Enrico Gebert", "Programming, Gameplay, Balancing",
                "Sebastian Lay", "Programming, Interface, Music/Sound",
                "Maria Manneck", "2D Arts, Interface",
            };
            AddNames(names, Settings.Instance.ResolutionY + 200);

            names = new List<string>() {
                "Special Thanks",
                "Tim Benedict Jagla, Fritz and all our testers"
            };
            AddNames(names, Settings.Instance.ResolutionY + 1100);

            Interface.Add(new InterfaceButton("Sounds/Music", new Vector2(100, Settings.Instance.ResolutionY + 1500), true));

            names = new List<string>() {
                "Beach - PaulFitzZaland", "soundcloud.com/paulfitzzaland",
                "Light Switch of doom - CosmicD", "freesound.org/people/CosmicD",
                "snare - switchy - room", "freesound.org/people/room",
                "Woosh.01 - Andromadax24", "freesound.org/people/Andromadax24"
            };
            AddNames(names, Settings.Instance.ResolutionY + 1700);

            Interface.Add(new InterfaceImage("acagamicslogo", new Vector2(-500 / 2, Settings.Instance.ResolutionY + 2600), Alignment.TOP_CENTER));

            Interface.Add(new InterfaceImage("Gruppe1", new Vector2(-768 / 2, Settings.Instance.ResolutionY + 9001), Alignment.TOP_CENTER));
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
