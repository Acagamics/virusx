using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace VirusX.Menu
{
    class Paused : MenuPage
    {
        enum Buttons
        {
            CONTINUE,
            QUIT_TO_MAINMENU,
        }
        Buttons currentButton = Buttons.CONTINUE;

        InterfaceButton[] waitingDisplays = new InterfaceButton[4];

        /// <summary>
        /// player that controlls this menu
        /// </summary>
        public int ControllingPlayer { get; set; }

        public Paused(Menu menu)
            : base(menu)
        {
            // background
            Interface.Add(new InterfaceFiller(Vector2.Zero, -1, -1, Color.Black * 0.5f, () => { return true; }));

            // paused string
            string label = "game paused";
            Vector2 stringSizePaused = menu.FontHeading.MeasureString(label);
            Vector2 positionPaused = (new Vector2(0, 0 - 300) - stringSizePaused) / 2;
            Interface.Add(new InterfaceButton(label, positionPaused, true, Alignment.CENTER_CENTER));

            // continue & mainmenu
            const int BUTTON_WIDTH = 150;
            float y = positionPaused.Y + 180;
            Interface.Add(new InterfaceButton("► Continue", new Vector2(-20 - BUTTON_WIDTH, y), () => { return currentButton == Buttons.CONTINUE; }, BUTTON_WIDTH, Alignment.CENTER_CENTER));
            Interface.Add(new InterfaceButton("► Quit to Menu", new Vector2(20, y), () => { return currentButton == Buttons.QUIT_TO_MAINMENU; }, BUTTON_WIDTH, Alignment.CENTER_CENTER));
            y += 100;

            // disconnected message
            for(int i=0; i<4; ++i)
            {
                string message = "Player " + (i + 1) + " is disconnected!";
                Vector2 position = (new Vector2(0, y) - menu.Font.MeasureString(message) / 2);
                waitingDisplays[i] = new InterfaceButton(message, position, ()=>false, Color.White, Color.Black, Alignment.CENTER_CENTER);
                Interface.Add(waitingDisplays[i]);
                y += 50;
            }
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);
        }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            base.OnActivated(oldPage, gameTime);

            // who controlls if nobody pressed?
            if (InputManager.Instance.IsWaitingForReconnect())
            {
                ControllingPlayer = 0;
                while (ControllingPlayer < Settings.Instance.NumPlayers &&
                    InputManager.Instance.IsWaitingForReconnect(Settings.Instance.GetPlayer(ControllingPlayer).ControlType))
                {
                    ++ControllingPlayer;
                }
                if (ControllingPlayer >= Settings.Instance.NumPlayers)
                    ControllingPlayer = 0;
            }

            // colors
            for (int i = 0; i < Settings.Instance.NumPlayers; ++i)
                waitingDisplays[i].BackgroundColor = Settings.Instance.GetPlayerColor(i);
            for (int i = Settings.Instance.NumPlayers; i < waitingDisplays.Length; ++i)
            {
                waitingDisplays[i].BackgroundColor = Color.Black;
                waitingDisplays[i].Visible = () => false;
            }

            base.Update(gameTime);  // reduces flicker
        }

        public override void Update(GameTime gameTime)
        {
            // if keyboard, anybody is allowed!
            int controllerBefore = ControllingPlayer;
            List<int> controls = new List<int>();
            if (InputManager.IsKeyboardControlType(Settings.Instance.GetPlayer(ControllingPlayer).ControlType))
            {
                for (int i = 0; i < Settings.Instance.NumPlayers; ++i)
                {
                    if (InputManager.IsKeyboardControlType(Settings.Instance.GetPlayer(i).ControlType))
                        controls.Add(i);
                }
            }
            else
                controls.Add(ControllingPlayer);
            foreach(int controllerIndex in controls)
            {
                // back to game
                if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.PAUSE, controllerIndex))
                {
                    InputManager.Instance.ResetWaitingForReconnect();
                    menu.ChangePage(Menu.Page.INGAME, gameTime);
                }

                // back to menu
                if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.EXIT, controllerIndex))
                {
                    InputManager.Instance.ResetWaitingForReconnect();
                    menu.ChangePage(Menu.Page.MAINMENU, gameTime);
                }

                // shutdown per (any) pad
                //   if (InputManager.Instance.PressedButton(Buttons.Back) || InputManager.Instance.PressedButton(Keys.Escape))
                //      menu.ChangePage(Menu.Page.MAINMENU, gameTime);

                // selected?
                if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.ACTION, controllerIndex))
                {
                    InputManager.Instance.ResetWaitingForReconnect();
                    if (currentButton == Buttons.CONTINUE)
                        menu.ChangePage(Menu.Page.INGAME, gameTime);
                    else if (currentButton == Buttons.QUIT_TO_MAINMENU)
                        menu.ChangePage(Menu.Page.MAINMENU, gameTime);
                }

                // changed option
                if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.RIGHT, controllerIndex) ||
                    InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.LEFT, controllerIndex))
                    currentButton = currentButton == Buttons.CONTINUE ? Buttons.QUIT_TO_MAINMENU : Buttons.CONTINUE;
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // unconnected players?
            // check here to prevent false drawing
            for (int i = 0; i < Settings.Instance.NumPlayers; ++i)
            {
                int player = i;
                waitingDisplays[i].Visible = () => InputManager.Instance.IsWaitingForReconnect(Settings.Instance.GetPlayer(player).ControlType);
            }

            base.Draw(spriteBatch, gameTime);
        }
    }
}
