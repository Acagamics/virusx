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
        private int virusIndex = 0;
        private Texture2D icons;

        private Effect virusRenderEffect;

        public Viruses(Menu menu)
            : base(menu)
        { }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            icons = content.Load<Texture2D>("icons");
            virusRenderEffect = content.Load<Effect>("shader/particleRendering");
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            // back to main menu
            if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.PAUSE) ||
                InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.EXIT) ||
                InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.ACTION) ||
                InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.HOLD))
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);

            // loopin
            if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.LEFT))
                virusIndex = virusIndex == (int)Player.VirusType.NUM_VIRUSES - 1 ? 0 : virusIndex + 1;
            else if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.RIGHT))
                virusIndex = virusIndex == 0 ? (int)Player.VirusType.NUM_VIRUSES - 1 : virusIndex - 1;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Microsoft.Xna.Framework.GameTime gameTime)
        {
            int width = 800;
            int left = (Settings.Instance.ResolutionX - width) / 2;
            int startTop = 100;
            int top = startTop;
            int padding = 40;

            // info texts
            List<String> labels = new List<string>() {
                Player.VirusNames[virusIndex] + " (" + Player.VirusShortName[virusIndex] + ")",
                Player.VirusClassification[virusIndex],
                "Caused desease:\n" + Player.VirusCausedDisease[virusIndex],
                "Description:\n" + Player.VirusAdditionalInfo[virusIndex],
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
                new Rectangle(InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.LEFT, true) ? 0 : 32, 0, 16, 16), InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.LEFT, true), menu.TexPixel);
            SimpleButton.Instance.DrawTexture(spriteBatch, icons, new Rectangle(left + width + padding, Settings.Instance.ResolutionY / 2, size, size),
                new Rectangle(InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.RIGHT, true) ? 16 : 48, 0, 16, 16), InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.RIGHT, true), menu.TexPixel);


            // back button
            string label = "Back to Menu";
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, label, new Vector2((int)((Settings.Instance.ResolutionX - menu.Font.MeasureString(label).X) / 2), Settings.Instance.ResolutionY - 60), true, menu.TexPixel);


            // virus
            virusRenderEffect.Parameters["ScreenSize"].SetValue(new Vector2(menu.ScreenWidth, menu.ScreenHeight));

            const int VIRUS_SIZE = 140;
            const int VIRUS_PADDING = 10;
            Rectangle virusImageRect = new Rectangle(left + width - VIRUS_SIZE, startTop, VIRUS_SIZE, VIRUS_SIZE);
            virusImageRect.Inflate(VIRUS_PADDING, VIRUS_PADDING);
            spriteBatch.Draw(menu.TexPixel, virusImageRect, Color.Black);
            virusImageRect.Inflate(-VIRUS_PADDING, -VIRUS_PADDING);
            spriteBatch.End(); // yeah this sucks terrible! TODO better solution
            switch ((Player.VirusType)virusIndex)
            {
                case Player.VirusType.EPSTEINBARR:
                    virusRenderEffect.CurrentTechnique = virusRenderEffect.Techniques["EpsteinBar_Spritebatch"];
                    break;
                case Player.VirusType.H5N1:
                    virusRenderEffect.CurrentTechnique = virusRenderEffect.Techniques["H5N1_Spritebatch"];
                    break;
                case Player.VirusType.HIV:
                    virusRenderEffect.CurrentTechnique = virusRenderEffect.Techniques["HIV_Spritebatch"];
                    break;
                case Player.VirusType.HEPATITISB:
                    virusRenderEffect.CurrentTechnique = virusRenderEffect.Techniques["HepatitisB_Spritebatch"];
                    break;
            }
            virusRenderEffect.Parameters["Color"].SetValue(Color.Gray.ToVector4()*1.8f);
            spriteBatch.Begin(0, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, virusRenderEffect);
            spriteBatch.Draw(menu.TexPixel, virusImageRect, Color.White);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
        }
    }
}
