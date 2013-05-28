using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace VirusX.Menu
{
    class Viruses : MenuPage
    {
        private int virusIndex = 0;
        int width = 800;
        int startTop = 100;
        int padding = 40;
        int left = -400;

        private ContentManager content;

        const int VIRUS_SIZE = 140;

        private InterfaceImage virusImage;
        private List<InterfaceButton> descriptions = new List<InterfaceButton>();

        public Viruses(Menu menu)
            : base(menu)
        {
            // Update text and position
            int top = startTop;

            // descriptions
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                InterfaceButton button = new InterfaceButton(() => { return GetLabels(virusIndex)[index]; }, new Vector2(left, top), index == 0, Alignment.TOP_CENTER);
                descriptions.Add(button);
                Interface.Add(descriptions[index]);
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

            // virus image
            virusImage = new InterfaceImage(ParticleRenderer.GetVirusTextureName((VirusSwarm.VirusType)virusIndex),
                            new Rectangle(left + width - VIRUS_SIZE, startTop, VIRUS_SIZE, VIRUS_SIZE),   // historic rect..
                                        Alignment.TOP_CENTER, true);
            Interface.Add(virusImage);

            // back button
            string label = "► Back to Menu";
            Interface.Add(new InterfaceButton(label, new Vector2(-(int)(menu.Font.MeasureString(label).X / 2), 100), () => { return true; }, Alignment.BOTTOM_CENTER));
        }

        public override void LoadContent(ContentManager content)
        {
            this.content = content;
            base.LoadContent(content);
        }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            base.OnActivated(oldPage, gameTime);
            base.Update(gameTime);  // reduces flicker
        }

        public override void Update(GameTime gameTime)
        {
            menu.BackToMainMenu(gameTime);

            // loopin
            if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.LEFT))
            {
                virusIndex = virusIndex == (int)VirusSwarm.VirusType.NUM_VIRUSES - 1 ? 0 : virusIndex + 1;
                virusImage.Texture = content.Load<Texture2D>(ParticleRenderer.GetVirusTextureName((VirusSwarm.VirusType)virusIndex));
            }
            else if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.RIGHT))
            {
                virusIndex = virusIndex == 0 ? (int)VirusSwarm.VirusType.NUM_VIRUSES - 1 : virusIndex - 1;
                virusImage.Texture = content.Load<Texture2D>(ParticleRenderer.GetVirusTextureName((VirusSwarm.VirusType)virusIndex));
            }

            // Update text and position
            int top = startTop;

            // descriptions
            for (int i = 0; i < 4; i++)
            {
                descriptions[i].Position = new Vector2(left, top);
                top += (int)menu.Font.MeasureString(GetLabels(virusIndex)[i]).Y + padding;
                if (i == 0)
                    top += padding;
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);

            // <missbrauch>
         /*   spriteBatch.End();
            RenderTarget2D virus = new RenderTarget2D(spriteBatch.GraphicsDevice, 128, 128, false, SurfaceFormat.Color, DepthFormat.None);
            spriteBatch.GraphicsDevice.Viewport = new Viewport(0, 0, 128, 128);
            virusRenderEffect.Parameters["ScreenSize"].SetValue(new Vector2(128, 128));
            foreach (VirusSwarm.VirusType type in (VirusSwarm.VirusType[])Enum.GetValues(typeof(VirusSwarm.VirusType)))
            {
                spriteBatch.GraphicsDevice.SetRenderTarget(virus);

                spriteBatch.GraphicsDevice.Clear(Color.Black);
                ParticleRenderer.ChooseVirusDrawTechnique(type, virusRenderEffect, true);
                virusRenderEffect.Parameters["Color"].SetValue(Color.White.ToVector4());
                spriteBatch.Begin(0, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, virusRenderEffect);
                spriteBatch.Draw(menu.TexPixel, new Rectangle(0,0,128,128), Color.White);
                spriteBatch.End();

                spriteBatch.GraphicsDevice.SetRenderTarget(null);

                using (var stream = System.IO.File.OpenWrite(Enum.GetName(typeof(VirusSwarm.VirusType),type) + ".png"))
                {
                    virus.SaveAsPng(stream, 128, 128);
                    stream.Close();
                }
            } */
            // </missbrauch>
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
