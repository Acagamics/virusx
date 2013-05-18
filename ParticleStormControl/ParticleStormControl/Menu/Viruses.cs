using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ParticleStormControl.Menu
{
    class Viruses : MenuPage
    {
        private int virusIndex = 0;
        int width = 800;
        int startTop = 100;
        int padding = 40;

        private Effect virusRenderEffect;

        public Viruses(Menu menu)
            : base(menu)
        {
            // Update text and position
            int top = startTop;
            int left = -400;

            // descriptions
            // can't be put in a loop because of the anonymous method
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                Interface.Add(new InterfaceButton(() => { return GetLabels(virusIndex)[index]; }, new Vector2(left, top), Alignment.TOP_CENTER));
                top += (int)menu.Font.MeasureString(GetLabels(virusIndex)[index]).Y + padding;
                if (i == 0)
                    top += padding;
            }

            // arrows
            Interface.Add(new InterfaceImageButton(
                "icons",
                new Rectangle(left - padding - 16, -8, 16, 16),
                new Rectangle(0, 0, 16, 16),
                new Rectangle(32, 0, 16, 16),
                () => !InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.LEFT, true),
                Alignment.CENTER_CENTER));
            
            Interface.Add(new InterfaceImageButton(
                "icons",
                new Rectangle(-left + padding, -8, 16, 16),
                new Rectangle(16, 0, 16, 16),
                new Rectangle(48, 0, 16, 16),
                () => !InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.RIGHT, true),
                Alignment.CENTER_CENTER));

            // back button
            string label = "► Back to Menu";
            Interface.Add(new InterfaceButton(label, new Vector2(-(int)(menu.Font.MeasureString(label).X / 2), 100), () => { return true; }, Alignment.BOTTOM_CENTER));
        }

        public override void LoadContent(ContentManager content)
        {
            virusRenderEffect = content.Load<Effect>("shader/particleRendering");

            base.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            menu.BackToMainMenu(gameTime);

            // loopin
            if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.LEFT))
                virusIndex = virusIndex == (int)VirusSwarm.VirusType.NUM_VIRUSES - 1 ? 0 : virusIndex + 1;
            else if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.RIGHT))
                virusIndex = virusIndex == 0 ? (int)VirusSwarm.VirusType.NUM_VIRUSES - 1 : virusIndex - 1;

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);

            int left = (Settings.Instance.ResolutionX - width) / 2;

            // virus
            virusRenderEffect.Parameters["ScreenSize"].SetValue(new Vector2(menu.ScreenWidth, menu.ScreenHeight));

            const int VIRUS_SIZE = 140;
            const int VIRUS_PADDING = 10;
            Rectangle virusImageRect = new Rectangle(left + width - VIRUS_SIZE, startTop, VIRUS_SIZE, VIRUS_SIZE);
            virusImageRect.Inflate(VIRUS_PADDING, VIRUS_PADDING);
            spriteBatch.Draw(menu.TexPixel, virusImageRect, Color.Black);
            virusImageRect.Inflate(-VIRUS_PADDING, -VIRUS_PADDING);
            spriteBatch.End(); // yeah this sucks terrible! TODO better solution
            switch ((VirusSwarm.VirusType)virusIndex)
            {
                case VirusSwarm.VirusType.EPSTEINBARR:
                    virusRenderEffect.CurrentTechnique = virusRenderEffect.Techniques["EpsteinBar_Spritebatch"];
                    break;
                case VirusSwarm.VirusType.H5N1:
                    virusRenderEffect.CurrentTechnique = virusRenderEffect.Techniques["H5N1_Spritebatch"];
                    break;
                case VirusSwarm.VirusType.HIV:
                    virusRenderEffect.CurrentTechnique = virusRenderEffect.Techniques["HIV_Spritebatch"];
                    break;
                case VirusSwarm.VirusType.HEPATITISB:
                    virusRenderEffect.CurrentTechnique = virusRenderEffect.Techniques["HepatitisB_Spritebatch"];
                    break;
            }
            virusRenderEffect.Parameters["Color"].SetValue(Color.Gray.ToVector4()*1.8f);
            spriteBatch.Begin(0, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, virusRenderEffect);
            spriteBatch.Draw(menu.TexPixel, virusImageRect, Color.White);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
        }

        /// <summary>
        /// Reads the description for every virus
        /// </summary>
        /// <param name="virusindex"></param>
        /// <returns></returns>
        private List<string> GetLabels(int virusindex)
        {
            return new List<string>() {
                VirusSwarm.VirusNames[virusIndex] + " (" + VirusSwarm.VirusShortName[virusIndex] + ")",
                VirusSwarm.VirusClassification[virusIndex],
                "Caused desease:\n" + VirusSwarm.VirusCausedDisease[virusIndex],
                "Description:\n" + VirusSwarm.VirusAdditionalInfo[virusIndex],
            };
        }
    }
}
