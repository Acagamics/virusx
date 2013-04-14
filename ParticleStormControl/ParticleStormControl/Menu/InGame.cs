using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParticleStormControl.Menu
{
    class InGame : MenuPage
    {
        private float blendIn;
        internal const float GAME_BLEND_DURATION = 0.5f;

        public InGame(Menu menu) : base(menu)
        {}

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            blendIn = GAME_BLEND_DURATION;
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
        }

        public override void Update(GameTime gameTime)
        {
            // controller disconnect -> pause
            if (InputManager.Instance.PressedButton(Microsoft.Xna.Framework.Input.Keys.Escape) || InputManager.Instance.WasPauseButtonPressed() || InputManager.Instance.IsWaitingForReconnect())
                menu.ChangePage(Menu.Page.PAUSED, gameTime);

            blendIn -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, GameTime gameTime)
        {
            if(blendIn > 0.0f)
                spriteBatch.Draw(menu.TexPixel, new Rectangle(0, 0, menu.ScreenWidth, menu.ScreenHeight), Color.Black * (blendIn / GAME_BLEND_DURATION));
        }
    }
}
