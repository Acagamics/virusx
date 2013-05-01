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

        private bool ignoreFirstUpdateStep;

        public InGame(Menu menu) : base(menu)
        {
            
        }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            if (oldPage != Menu.Page.PAUSED)
            {
                blendIn = GAME_BLEND_DURATION;
                ignoreFirstUpdateStep = true;
            }
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
        }

        public override void Update(GameTime gameTime)
        {
            if (ignoreFirstUpdateStep)
            {
                ignoreFirstUpdateStep = false;
                return;
            }

            // controller disconnect -> pause
            for (int i = 0; i < Settings.Instance.NumPlayers; ++i)
            {
                if (InputManager.Instance.IsWaitingForReconnect())
                    menu.ChangePage(Menu.Page.PAUSED, gameTime);

                if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.PAUSE, i))
                {
                    ((Paused)menu.GetPage(Menu.Page.PAUSED)).ControllingPlayer = i;
                    menu.ChangePage(Menu.Page.PAUSED, gameTime);
                }
            }

            blendIn -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, GameTime gameTime)
        {
            if(blendIn > 0.0f)
               spriteBatch.Draw(menu.TexPixel, new Rectangle(0, 0, menu.ScreenWidth, menu.ScreenHeight), Color.Black * (blendIn / GAME_BLEND_DURATION));
        }
    }
}
