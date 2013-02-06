using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using ParticleStormControl;

namespace ParticleStormControl.Menu
{
    class NewGame : MenuPage
    {
        enum Button
        {
            PLAYERNUMBER,
            COLOR0,
            COLOR1,
            COLOR2,
            COLOR3,
            CONTROLS0,
            CONTROLS1,
            CONTROLS2,
            CONTROLS3,
            DEFAULT,
            START,
            BACK
        };

        private readonly Color selectedColor = Color.Red;
        private readonly Color normalColor = Color.Black;

        private readonly Button[][] selectionTree = new Button[][]
            {
                new Button[]{ Button.PLAYERNUMBER },
                new Button[]{ Button.COLOR0, Button.CONTROLS0 },
                new Button[]{ Button.COLOR1, Button.CONTROLS1 },
                new Button[]{ Button.COLOR2, Button.CONTROLS2 },
                new Button[]{ Button.COLOR3, Button.CONTROLS3 },
                new Button[]{ Button.DEFAULT },
                new Button[]{ Button.START, Button.BACK },
            };
        private int selectionX = 0;
        private int selectionY = 6;

        public NewGame(Menu menu)
            : base(menu)
        {

        }

        public override void LoadContent(ContentManager content)
        {
        }

        public override void Update(float frameTimeInterval)
        {
            // selection
            if (InputManager.Instance.AnyDownButtonPressed())
                ++selectionY;
            else if (InputManager.Instance.AnyUpButtonPressed())
                --selectionY;
            else if (InputManager.Instance.AnyLeftButtonPressed())
                ++selectionX;
            else if (InputManager.Instance.AnyRightButtonPressed())
                --selectionX;
            selectionY = selectionY % selectionTree.Length;
            if (selectionY < 0)
                selectionY = selectionTree.Length - 1;
            selectionX = selectionX % selectionTree[selectionY].Length;
            if (selectionX < 0)
                selectionX = selectionTree[selectionY].Length -1;

            Button selectedButton = selectionTree[selectionY][selectionX];

            // button selected
            if (selectedButton == Button.PLAYERNUMBER)
            {
                if (InputManager.Instance.ContinueButton() || InputManager.Instance.AnyRightButtonPressed())
                {
                    ++Settings.Instance.NumPlayers;
                    Settings.Instance.NumPlayers %= 5;
                    if (Settings.Instance.NumPlayers < 2)
                        Settings.Instance.NumPlayers = 2;
                }
                else if (InputManager.Instance.AnyLeftButtonPressed())
                {
                    --Settings.Instance.NumPlayers;
                    if (Settings.Instance.NumPlayers < 2)
                        Settings.Instance.NumPlayers = 4;
                }
            }
            if (InputManager.Instance.ContinueButton())
            {
                int index;
                switch (selectedButton)
                {
                    case Button.START:
                        menu.ChangePage(Menu.Page.INGAME);
                        break;

                    case Button.BACK:
                        menu.ChangePage(Menu.Page.MAINMENU);
                        break;

                    case Button.DEFAULT:
                        Settings.Instance.ResetPlayerSettingsToDefault();
                        break;

                    case Button.COLOR0:
                    case Button.COLOR1:
                    case Button.COLOR2:
                    case Button.COLOR3:
                        index = (int)selectedButton - (int)Button.COLOR0;
                        int newColorIndex = Settings.Instance.PlayerColorIndices[index] + 1;
                        newColorIndex = newColorIndex % Player.Colors.Length;
                        // swap with the one who already owns this!
                     /*   for (int i = 0; i < Settings.Instance.PlayerColorIndices.Length; ++i)
                        {
                            if (Settings.Instance.PlayerColorIndices[i] == newColorIndex)
                            {
                                Settings.Instance.PlayerColorIndices[i] = Settings.Instance.PlayerColorIndices[index];
                                break;
                            }
                        } */
                        Settings.Instance.PlayerColorIndices[index] = newColorIndex;
                        break;

                    case Button.CONTROLS0:
                    case Button.CONTROLS1:
                    case Button.CONTROLS2:
                    case Button.CONTROLS3:
                        index = (int)selectedButton - (int)Button.CONTROLS0;
                        Player.ControlType newControllerType = Settings.Instance.PlayerControls[index] + 1;
                        newControllerType = (Player.ControlType)((int)newControllerType % Player.ControlNames.Length);
                        // swap with the one who already owns this!
                       /* for (int i = 0; i < Settings.Instance.PlayerControls.Length; ++i)
                        {
                            if (Settings.Instance.PlayerControls[i] == newControllerType)
                            {
                                Settings.Instance.PlayerControls[i] = Settings.Instance.PlayerControls[index];
                                break;
                            }
                        } */
                        Settings.Instance.PlayerControls[index] = newControllerType;
                        break;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float timeInterval)
        {
            Button selectedButton = selectionTree[selectionY][selectionX];
            Vector2 screenMid = new Vector2(menu.ScreenWidth / 2, menu.ScreenHeight / 2);

            SimpleButton.Draw(spriteBatch, menu.Font, "< player count >", screenMid + new Vector2(-100, -170), selectedButton == Button.PLAYERNUMBER);
            SimpleButton.Draw(spriteBatch, menu.Font, Settings.Instance.NumPlayers.ToString(), screenMid + new Vector2(200, -170), Color.Black);

            SimpleButton.Draw(spriteBatch, menu.Font, "color", screenMid + new Vector2(-350, -100), selectedButton == Button.COLOR0);
            SimpleButton.Draw(spriteBatch, menu.Font, Player.Names[Settings.Instance.PlayerColorIndices[0]], screenMid + new Vector2(-200, -100), Settings.Instance.GetPlayerColor(0));
            SimpleButton.Draw(spriteBatch, menu.Font, "color", screenMid + new Vector2(-350, -50), selectedButton == Button.COLOR1);
            SimpleButton.Draw(spriteBatch, menu.Font, Player.Names[Settings.Instance.PlayerColorIndices[1]], screenMid + new Vector2(-200, -50), Settings.Instance.GetPlayerColor(1));
            SimpleButton.Draw(spriteBatch, menu.Font, "color", screenMid + new Vector2(-350, 0), selectedButton == Button.COLOR2);
            SimpleButton.Draw(spriteBatch, menu.Font, Player.Names[Settings.Instance.PlayerColorIndices[2]], screenMid + new Vector2(-200, 0), Settings.Instance.GetPlayerColor(2));
            SimpleButton.Draw(spriteBatch, menu.Font, "color", screenMid + new Vector2(-350, 50), selectedButton == Button.COLOR3);
            SimpleButton.Draw(spriteBatch, menu.Font, Player.Names[Settings.Instance.PlayerColorIndices[3]], screenMid + new Vector2(-200, 50), Settings.Instance.GetPlayerColor(3));

            SimpleButton.Draw(spriteBatch, menu.Font, "controls", screenMid + new Vector2(0, -100), selectedButton == Button.CONTROLS0);
            SimpleButton.Draw(spriteBatch, menu.Font, Player.ControlNames[(int)Settings.Instance.PlayerControls[0]], screenMid + new Vector2(300, -100), Settings.Instance.GetPlayerColor(0));
            SimpleButton.Draw(spriteBatch, menu.Font, "controls", screenMid + new Vector2(0, -50), selectedButton == Button.CONTROLS1);
            SimpleButton.Draw(spriteBatch, menu.Font, Player.ControlNames[(int)Settings.Instance.PlayerControls[1]], screenMid + new Vector2(300, -50), Settings.Instance.GetPlayerColor(1));
            SimpleButton.Draw(spriteBatch, menu.Font, "controls", screenMid + new Vector2(0, 0), selectedButton == Button.CONTROLS2);
            SimpleButton.Draw(spriteBatch, menu.Font, Player.ControlNames[(int)Settings.Instance.PlayerControls[2]], screenMid + new Vector2(300, -0), Settings.Instance.GetPlayerColor(2));
            SimpleButton.Draw(spriteBatch, menu.Font, "controls", screenMid + new Vector2(0, 50), selectedButton == Button.CONTROLS3);
            SimpleButton.Draw(spriteBatch, menu.Font, Player.ControlNames[(int)Settings.Instance.PlayerControls[3]], screenMid + new Vector2(300, 50), Settings.Instance.GetPlayerColor(3));

            SimpleButton.Draw(spriteBatch, menu.Font, "Default", screenMid + new Vector2(0, 120), selectedButton == Button.DEFAULT);

            SimpleButton.Draw(spriteBatch, menu.Font, "Start", screenMid + new Vector2(-80, 200), selectedButton == Button.START);
            SimpleButton.Draw(spriteBatch, menu.Font, "Back", screenMid + new Vector2(80, 200), selectedButton == Button.BACK);
        }
    }
}
