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
        internal const float GAME_BLEND_DURATION = 0.3f;

        public InGame(Menu menu) : base(menu)
        {}

        public override void OnActivated(Menu.Page oldPage)
        {
            blendIn = GAME_BLEND_DURATION;
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
        }

        public override void Update(float frameTimeInterval)
        {
            // controller disconnect -> pause
            if (InputManager.Instance.PressedButton(Microsoft.Xna.Framework.Input.Keys.Escape) || InputManager.Instance.PauseButton() || InputManager.Instance.IsWaitingForReconnect())
                menu.ActivePage = Menu.Page.PAUSED;

            blendIn -= frameTimeInterval;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, float frameTimeInterval)
        {
            if(blendIn > 0.0f)
                spriteBatch.Draw(menu.PixelTexture, new Rectangle(0, 0, menu.ScreenWidth, menu.ScreenHeight), Color.Black * (blendIn / GAME_BLEND_DURATION));
        }
    }
}
