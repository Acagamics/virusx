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
            ITEMS,
            CONTINUE,
            BACK,

            NUM_BUTTONS
        };
        Button selectedButton = Button.MODE;

        NewGame.GameMode gameMode = NewGame.GameMode.CLASSIC;

        InterfaceButton useItemsButton;

        public Mode(Menu menu)
            : base(menu)
        {
            Interface.Add(new InterfaceButton("New Game", new Vector2(100, 100), true));

            Interface.Add(new InterfaceButton("Game mode", new Vector2(100, 220), () => { return selectedButton == Button.MODE; }));
            Interface.Add(new InterfaceButton(() => { return "◄ " + NewGame.GAMEMODE_NAME[(int)gameMode] + " ►"; }, new Vector2(450, 220)));
            Interface.Add(new InterfaceButton("Items", new Vector2(100, 280), () => { return selectedButton == Button.ITEMS; }));
            useItemsButton = new InterfaceButton(() => { return Settings.Instance.UseItems ? "◄ ON ►" : "◄ OFF ►"; }, new Vector2(450, 280), () => { return false; }, Color.White, Settings.Instance.UseItems ? Color.Green : Color.Red);
            Interface.Add(useItemsButton);

            Interface.Add(new InterfaceButton("► Continue", new Vector2(100, 400), () => { return selectedButton == Button.CONTINUE; }));
            Interface.Add(new InterfaceButton("► Back to Menu", new Vector2(100, 460), () => { return selectedButton == Button.BACK; }));
        }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            selectedButton = Button.MODE;
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            selectedButton = (Button)(Menu.Loop((int)selectedButton, (int)Button.NUM_BUTTONS, StartingControls));
            
            // back to main menu (ignoring the selected button)
            if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.EXIT, StartingControls) ||
                InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.HOLD, StartingControls))
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);

            switch (selectedButton)
            {
                // loop game mode or take a shortcut
                case Button.MODE:
                    gameMode = (NewGame.GameMode)Menu.Loop((int)gameMode, (int)NewGame.GameMode.NUM_MODES, StartingControls, true);
                    GotoNewGame(gameTime);
                    break;
                // toggle items
                case Button.ITEMS:
                    Settings.Instance.UseItems = Menu.Toggle(Settings.Instance.UseItems, StartingControls);
                    break;
                // go to new game screen
                case Button.CONTINUE:
                    GotoNewGame(gameTime);
                    break;
                // back to main menu
                case Button.BACK:
                    if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.ACTION, StartingControls))
                        menu.ChangePage(Menu.Page.MAINMENU, gameTime);
                    break;
            }

            // change background colors
            useItemsButton.BackgroundColor = Settings.Instance.UseItems ? Color.Green : Color.Red;

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);
        }

        private void GotoNewGame(GameTime gameTime)
        {
            if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.ACTION, StartingControls))
            {
                ((NewGame)menu.GetPage(Menu.Page.NEWGAME)).Mode = gameMode;
                menu.ChangePage(Menu.Page.NEWGAME, gameTime);
            }
        }
    }
}
