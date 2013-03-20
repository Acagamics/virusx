using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParticleStormControl.Menu
{
    class InGame : MenuPage
    {
        public InGame(Menu menu) : base(menu)
        { }

        public override void Initialize()
        {
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
        }

        public override void Update(float frameTimeInterval)
        {
            // controller disconnect -> pause
            if (InputManager.Instance.PauseButton() || InputManager.Instance.IsWaitingForReconnect())
                menu.ActivePage = Menu.Page.PAUSED;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, float frameTimeInterval)
        {
        }
    }
}
