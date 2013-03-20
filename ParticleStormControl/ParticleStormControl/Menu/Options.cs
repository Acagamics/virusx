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
    class Options : MenuPage
    {
        Texture2D logo;

        enum Button
        {
            RESOLUTION,
            FULLSCREEN,
            BACK,

            NUM_BUTTONS
        };
        Button selectedButton = Button.BACK;

        public Options(Menu menu)
            : base(menu)
        {
        }

        public override void Initialize()
        {
        }

        public override void LoadContent(ContentManager content)
        {
            logo = content.Load<Texture2D>("logo");
        }

        public override void Update(float frameTimeInterval)
        {
            // loopin
            int selectionInt = (int)selectedButton;
            if (InputManager.Instance.AnyDownButtonPressed())
                selectionInt = selectionInt == (int)Button.NUM_BUTTONS - 1 ? 0 : selectionInt + 1;
            else if (InputManager.Instance.AnyUpButtonPressed())
                selectionInt = selectionInt == 0 ? (int)Button.NUM_BUTTONS - 1 : selectionInt - 1;
            selectedButton = (Button)(selectionInt);

            // button selected
            if (InputManager.Instance.ContinueButton() && selectedButton == Button.BACK)
            {
                menu.ApplyChangedGraphicsSettings();
                menu.ChangePage(Menu.Page.MAINMENU);
            }
            if (selectedButton == Button.FULLSCREEN && (InputManager.Instance.ContinueButton() || InputManager.Instance.AnyLeftButtonPressed() || InputManager.Instance.AnyRightButtonPressed()))
                Settings.Instance.Fullscreen = !Settings.Instance.Fullscreen;
            else if (selectedButton == Button.RESOLUTION)
            {
                if (InputManager.Instance.ContinueButton() || InputManager.Instance.AnyRightButtonPressed())
                {
                    // find "nearest" display mode and set - uses width for searching
                    GraphicsDevice device = menu.PixelTexture.GraphicsDevice;
                    foreach(DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                    {
                        if (mode.Width >= Settings.MINIMUM_SCREEN_WIDTH && mode.Width > Settings.Instance.ResolutionX)
                        {
                            Settings.Instance.ResolutionX = mode.Width;
                            Settings.Instance.ResolutionY = mode.Height;
                            break;
                        }
                    }
                }
                else if (InputManager.Instance.AnyLeftButtonPressed())
                {
                    // find "nearest" display mode and set - uses width for searching
                    GraphicsDevice device = menu.PixelTexture.GraphicsDevice;
                    DisplayMode biggestSmallerOne = null;
                    foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)  // iterating backwards is not possible
                    {
                        if (mode.Width >= Settings.MINIMUM_SCREEN_WIDTH && mode.Width < Settings.Instance.ResolutionX)
                            biggestSmallerOne = mode;
                    }
                    if (biggestSmallerOne == null)
                        biggestSmallerOne = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.First(x => x.Width >= Settings.MINIMUM_SCREEN_WIDTH);
                    Settings.Instance.ResolutionX = biggestSmallerOne.Width;
                    Settings.Instance.ResolutionY = biggestSmallerOne.Height;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float timeInterval)
        {
            Vector2 screenMid = new Vector2(menu.ScreenWidth / 2, menu.ScreenHeight / 2);

            SimpleButton.Draw(spriteBatch, menu.Font, "< Screen Resolution >", screenMid + new Vector2(-200, -75), selectedButton == Button.RESOLUTION);
            SimpleButton.Draw(spriteBatch, menu.Font, Settings.Instance.ResolutionX + " x " + Settings.Instance.ResolutionY, screenMid + new Vector2(200, -75), Color.Black);
            SimpleButton.Draw(spriteBatch, menu.Font, "Fullscreen", screenMid + new Vector2(-200, -25), selectedButton == Button.FULLSCREEN);
            if(Settings.Instance.Fullscreen)
                SimpleButton.Draw(spriteBatch, menu.Font, "On", screenMid + new Vector2(200, -25), Color.Black);
            else
                SimpleButton.Draw(spriteBatch, menu.Font, "Off", screenMid + new Vector2(200, -25), Color.Black);
            SimpleButton.Draw(spriteBatch, menu.Font, "Apply and Back", screenMid + new Vector2(0, 75), selectedButton == Button.BACK);
        }
    }
}
