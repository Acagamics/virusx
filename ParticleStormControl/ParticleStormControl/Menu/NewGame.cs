using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using ParticleStormControl;
using Microsoft.Xna.Framework.Input;

namespace ParticleStormControl.Menu
{
    class NewGame : MenuPage
    {
        int padd = 40; // padding
        Texture2D[] viruses = new Texture2D[4];
        Texture2D icons;

        public NewGame(Menu menu)
            : base(menu)
        {
            InputManager.Instance.resetAllControlTypes();
        }

        public override void LoadContent(ContentManager content)
        {
            viruses[0] = content.Load<Texture2D>("viruses/H1N1");
            viruses[1] = content.Load<Texture2D>("viruses/HepatitisB");
            viruses[2] = content.Load<Texture2D>("viruses/HIV");
            viruses[3] = content.Load<Texture2D>("viruses/Noro");
            icons = content.Load<Texture2D>("icons");
        }

        public override void Update(float frameTimeInterval)
        {
            // must-haves
            /*
            InputManager.Instance.AnyDownButtonPressed()
            Settings.Instance.NumPlayers;
                    
            menu.ChangePage(Menu.Page.INGAME);
            Settings.Instance.ResetPlayerSettingsToDefault();
            
            Settings.Instance.PlayerColorIndices[index]
            Player.Colors.Length;
            
            InputManager.ControlType newControllerType = Settings.Instance.PlayerControls[index] + 1;
            newControllerType = (InputManager.ControlType)((int)newControllerType % Player.ControlNames.Length);
            Settings.Instance.PlayerControls[index] = newControllerType;

            */

            if (InputManager.Instance.ExitButton())
                menu.ChangePage(Menu.Page.MAINMENU);

            // test continue buttons
            foreach (InputManager.ControlType type in InputManager.Instance.ContinueButtonsPressed())
            {
                bool found = false;

                for (int i = 0; i < Settings.Instance.NumPlayers; i++)
                {
                    if (type == Settings.Instance.PlayerControls[i])
                    {
                        found = true;
                        menu.ChangePage(Menu.Page.INGAME);
                    }
                }

                if (!found)
                {
                    int index = Settings.Instance.NumPlayers;
                    int colorIndex = getNextFreeColorIndex(0);
                    Settings.Instance.PlayerControls[index] = type;
                    Settings.Instance.PlayerColorIndices[index] = colorIndex;
                    Settings.Instance.NumPlayers++;
                }
            }

            // test direction buttons
            for (int i = 0; i < Settings.Instance.NumPlayers; i++)
            {
                if (InputManager.Instance.DirectionButtonPressed(InputManager.ControlActions.LEFT, Settings.Instance.PlayerControls[i]))
                {
                    if (--Settings.Instance.PlayerVirusIndices[i] < 0)
                        Settings.Instance.PlayerVirusIndices[i] = Player.Viruses.Length - 1;
                }

                if (InputManager.Instance.DirectionButtonPressed(InputManager.ControlActions.RIGHT, Settings.Instance.PlayerControls[i]))
                {
                    if (++Settings.Instance.PlayerVirusIndices[i] >= Player.Viruses.Length)
                        Settings.Instance.PlayerVirusIndices[i] = 0;
                }

                if (InputManager.Instance.DirectionButtonPressed(InputManager.ControlActions.UP, Settings.Instance.PlayerControls[i]))
                {
                    Settings.Instance.PlayerColorIndices[i] = getPreviousFreeColorIndex(Settings.Instance.PlayerColorIndices[i]);
                }

                if (InputManager.Instance.DirectionButtonPressed(InputManager.ControlActions.DOWN, Settings.Instance.PlayerControls[i]))
                {
                    Settings.Instance.PlayerColorIndices[i] = getNextFreeColorIndex(Settings.Instance.PlayerColorIndices[i]);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float timeInterval)
        {
            int boxWidth = (Settings.Instance.ResolutionX - 3 * padd) / 2;
            int boxHeight = (Settings.Instance.ResolutionY - 3 * padd) / 2;

            int x = padd * 2 + boxWidth;
            int y = padd * 2 + boxHeight;

            spriteBatch.Draw(menu.PixelTexture, new Rectangle(padd, padd, boxWidth, boxHeight), Color.FromNonPremultiplied(255, 255, 255, 128));
            spriteBatch.Draw(menu.PixelTexture, new Rectangle(x, padd, boxWidth, boxHeight), Color.FromNonPremultiplied(255, 255, 255, 128));
            spriteBatch.Draw(menu.PixelTexture, new Rectangle(padd, y, boxWidth, boxHeight), Color.FromNonPremultiplied(255, 255, 255, 128));
            spriteBatch.Draw(menu.PixelTexture, new Rectangle(x, y, boxWidth, boxHeight), Color.FromNonPremultiplied(255, 255, 255, 128));

            for (int i = 0; i < Settings.Instance.NumPlayers; i++)
            {
                Vector2 origin = new Vector2();
                switch (i)
                {
                    case 0:
                        origin = new Vector2(padd, padd);
                        break;
                    case 1:
                        origin = new Vector2(x, padd);
                        break;
                    case 2:
                        origin = new Vector2(padd, y);
                        break;
                    case 3:
                        origin = new Vector2(x, y);
                        break;
                }
                
                spriteBatch.DrawString(menu.Font, Player.VirusNames[Settings.Instance.PlayerVirusIndices[i]].ToString(), origin + new Vector2(20 + boxWidth / 2, 20), Color.Black);
                spriteBatch.DrawString(menu.FontBold, Player.ColorNames[Settings.Instance.PlayerColorIndices[i]].ToString(), origin + new Vector2(20 + boxWidth / 2, 50), Player.Colors[Settings.Instance.PlayerColorIndices[i]]);

                Rectangle destination = new Rectangle((int)origin.X + padd, (int)origin.Y + padd, boxWidth / 2 - padd, boxWidth / 2 - padd);
                spriteBatch.Draw(viruses[Settings.Instance.PlayerVirusIndices[i]], destination, Color.White);

                spriteBatch.Draw(icons, new Rectangle((int)origin.X + 16, (int)origin.Y + boxHeight / 2 - 8, 16, 16), new Rectangle(0, 0, 16, 16), Color.White);
                spriteBatch.Draw(icons, new Rectangle((int)origin.X + boxWidth - 32, (int)origin.Y + boxHeight / 2 - 8, 16, 16), new Rectangle(16, 0, 16, 16), Color.White);
            }
        }

        /* Helper */

        int getNextFreeColorIndex(int start)
        {
            while (Settings.Instance.PlayerColorIndices.Contains(start) || start >= Player.Colors.Length)
            {
                if(start++ >= Player.Colors.Length) start = 0;
            }
            return start;
        }

        int getPreviousFreeColorIndex(int start)
        {
            while (Settings.Instance.PlayerColorIndices.Contains(start) || start < 0)
            {
                if (start-- < 0) start = Player.Colors.Length - 1;
            }
            return start;
        }
    }
}
