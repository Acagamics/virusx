using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace VirusX.Menu
{
    class InGame : MenuPage
    {
        private float blendIn;
        internal const float GAME_BLEND_DURATION = 0.5f;

        private bool ignoreFirstUpdateStep;

        public InGame(Menu menu) : base(menu)
        {

            if (Settings.Instance.GameMode == VirusX.InGame.GameMode.TUTORIAL)
            {
                Interface.Add(new InterfaceTooltip(() => { return "Tooltip"; }, () => { return "und zwar ein ganz toller\nmit mehreren\nzeilen und zeichen"; }, new Vector2(300, 0), () => true, 300, InterfaceTooltip.ArrowPosition.LEFT, Alignment.CENTER_LEFT));
            }
        }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            if (oldPage != Menu.Page.PAUSED && oldPage != Menu.Page.CONTROLS)
            {
                blendIn = GAME_BLEND_DURATION;
                ignoreFirstUpdateStep = true;
            }
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            if (ignoreFirstUpdateStep)
            {
                ignoreFirstUpdateStep = false;
                return;
            }

            if (Settings.Instance.GameMode == VirusX.InGame.GameMode.TUTORIAL)
            {
                UpdateTutorial();
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

            if (InputManager.Instance.IsButtonPressed(Keys.F1))
                menu.ChangePage(Menu.Page.CONTROLS, gameTime);

            blendIn -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);

            if(blendIn > 0.0f)
               spriteBatch.Draw(menu.TexPixel, new Rectangle(0, 0, menu.ScreenWidth, menu.ScreenHeight), Color.Black * (blendIn / GAME_BLEND_DURATION));
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateTutorial()
        {

        }
    }
}
