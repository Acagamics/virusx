using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ParticleStormControl.Menu
{
    class Viruses : MenuPage
    {
        int index = 0;
        private Texture2D icons;

        public Viruses(Menu menu)
            : base(menu)
        { }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            icons = content.Load<Texture2D>("icons");
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            // back to main menu
            if (InputManager.Instance.WasPauseButtonPressed()
                || InputManager.Instance.WasContinueButtonPressed()
                || InputManager.Instance.PressedButton(Buttons.B)
                || InputManager.Instance.PressedButton(Keys.Escape)
                || InputManager.Instance.WasExitButtonPressed())
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);

            // loopin
            if (InputManager.Instance.AnyLeftButtonPressed())
                index = index == (int)Player.VirusType.NUM_VIRUSES - 1 ? 0 : index + 1;
            else if (InputManager.Instance.AnyRightButtonPressed())
                index = index == 0 ? (int)Player.VirusType.NUM_VIRUSES - 1 : index - 1;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Microsoft.Xna.Framework.GameTime gameTime)
        {
            int width = 800;
            int left = (Settings.Instance.ResolutionX - width) / 2;
            int top = 100;
            int padding = 40;

            // info texts
            List<String> labels = new List<string>() {
                Player.VirusNames[index] + " (" + Player.VirusShortName[index] + ")",
                Player.VirusClassification[index],
                "Caused desease:\n" + Player.VirusCausedDisease[index],
                "Description:\n" + Player.VirusAdditionalInfo[index],
            };

            for (int i = 0; i < labels.Count; i++)
			{
                SimpleButton.Instance.Draw(spriteBatch, i == 0 ? menu.FontHeading : menu.Font, labels[i], new Vector2(left, top), i == 0, menu.TexPixel);
                top += (int)menu.Font.MeasureString(labels[i]).Y + padding;
                if (i == 0)
                    top += padding;
			}

            // arrows
            int size = 16;
            SimpleButton.Instance.DrawTexture(spriteBatch, icons, new Rectangle(left - size - padding, Settings.Instance.ResolutionY / 2, size, size),
                new Rectangle(InputManager.Instance.AnyLeftButtonDown() ? 0 : 32, 0, 16, 16), InputManager.Instance.AnyLeftButtonDown(), menu.TexPixel);
            SimpleButton.Instance.DrawTexture(spriteBatch, icons, new Rectangle(left + width + padding, Settings.Instance.ResolutionY / 2, size, size),
                new Rectangle(InputManager.Instance.AnyRightButtonDown() ? 16 : 48, 0, 16, 16), InputManager.Instance.AnyRightButtonDown(), menu.TexPixel);


            // back button
            string label = "Back to Menu";
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, label, new Vector2((int)((Settings.Instance.ResolutionX - menu.Font.MeasureString(label).X) / 2), Settings.Instance.ResolutionY - 60), true, menu.TexPixel);
        }
    }
}
