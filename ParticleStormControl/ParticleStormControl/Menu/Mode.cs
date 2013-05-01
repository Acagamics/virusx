using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ParticleStormControl.Menu
{
    class Mode : MenuPage
    {
        /// <summary>
        /// controls of the player who opened this menu
        /// </summary>
        public InputManager.ControlType StartingControls { get; set; }

        enum Button
        {
            MODE,
            CONTINUE,
            BACK,

            NUM_BUTTONS
        };
        Button selectedButton = Button.MODE;

        NewGame.GameMode gameMode = NewGame.GameMode.CLASSIC;

        public Mode(Menu menu)
            : base(menu)
        {
            Interface.Add(new InterfaceButton(() => { return "< " + NewGame.GAMEMODE_NAME[(int)gameMode] + " >"; }, new Vector2(100, 100), () => { return selectedButton == Button.MODE; }, true));
            Interface.Add(new InterfaceButton("Continue", new Vector2(100, 300), () => { return selectedButton == Button.CONTINUE; }, true));
            Interface.Add(new InterfaceButton("Back to Menu", new Vector2(100, 360), () => { return selectedButton == Button.BACK; }));

        }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            selectedButton = (Button)(Menu.LoopEnum((int)selectedButton, (int)Button.NUM_BUTTONS, StartingControls));
            
            // back to main menu
            if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.EXIT, StartingControls) ||
                InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.HOLD, StartingControls))
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);

            if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.ACTION, StartingControls))
            {
                switch (selectedButton)
                {
                    case Button.CONTINUE:
                    case Button.MODE:
                        ((NewGame)menu.GetPage(Menu.Page.NEWGAME)).Mode = gameMode;
                        menu.ChangePage(Menu.Page.NEWGAME, gameTime);
                        break;
                    case Button.BACK:
                        menu.ChangePage(Menu.Page.MAINMENU, gameTime);
                        break;
                }
            }

            if(selectedButton == Button.MODE)
            {
                gameMode = (NewGame.GameMode)Menu.LoopEnum((int)gameMode, (int)NewGame.GameMode.NUM_MODES, StartingControls, true);
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);
        }
    }
}
