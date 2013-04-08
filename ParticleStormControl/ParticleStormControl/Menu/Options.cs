﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using ParticleStormControl;
using Microsoft.Xna.Framework.Input;

namespace ParticleStormControl.Menu
{
    class Options : MenuPage
    {
        Texture2D logo;
        bool fullscreen = Settings.Instance.Fullscreen;
        bool sound = Settings.Instance.Sound;

        struct Resolution
        {
            public int width, height;
        };
        List<Resolution> availableResolutions = new List<Resolution>();
        int activeResolution;

        enum Button
        {
            RESOLUTION,
            FULLSCREEN,
            SOUND,
            BACK,
            EXIT,

            NUM_BUTTONS
        };
        Button selectedButton = Button.BACK;

        public Options(Menu menu)
            : base(menu)
        {
        }

        // if changed to main menu without saving
        public override void OnActivated(Menu.Page oldPage)
        {
            fullscreen = Settings.Instance.Fullscreen;
            sound = Settings.Instance.Sound;

            // search.. ehrm.. nearest resolution
            availableResolutions.AddRange(GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.Select(
                                dispMode => new Resolution() { width = dispMode.Width, height = dispMode.Height }));
            availableResolutions.Sort((a, b) => a.width == b.width ? a.height - b.height : a.width - b.width);

            for(int i=0; i<availableResolutions.Count; ++i)
            {
                if (availableResolutions[i].width >= Settings.Instance.ResolutionX &&
                    availableResolutions[i].height >= Settings.Instance.ResolutionY)
                {
                    activeResolution = i;
                    break;
                }
            }
            selectedButton = Button.BACK;
            
        }

        public override void LoadContent(ContentManager content)
        {
            logo = content.Load<Texture2D>("logo");
        }

        public override void Update(GameTime gameTime)
        {
            // loopin
            int selectionInt = (int)selectedButton;
            if (InputManager.Instance.AnyDownButtonPressed())
                selectionInt = selectionInt == (int)Button.NUM_BUTTONS - 1 ? 0 : selectionInt + 1;
            else if (InputManager.Instance.AnyUpButtonPressed())
                selectionInt = selectionInt == 0 ? (int)Button.NUM_BUTTONS - 1 : selectionInt - 1;
            if (selectionInt != (int)selectedButton)
                SimpleButton.Instance.ChangeHappened(gameTime);
            selectedButton = (Button)(selectionInt);

            // button selected
            if (InputManager.Instance.ContinueButton() && selectedButton == Button.EXIT || InputManager.Instance.PressedButton(Keys.Escape) || InputManager.Instance.ExitButton())
            {
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);
            }
            else if (InputManager.Instance.ContinueButton() && selectedButton == Button.BACK)
            {
                Settings.Instance.Fullscreen = fullscreen;
                Settings.Instance.Sound = sound;
                Settings.Instance.ResolutionX = availableResolutions[activeResolution].width;
                Settings.Instance.ResolutionY = availableResolutions[activeResolution].height;
                menu.ApplyChangedGraphicsSettings();
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);
            }
            else if (selectedButton == Button.FULLSCREEN && (InputManager.Instance.AnyLeftButtonPressed() || InputManager.Instance.AnyRightButtonPressed() || InputManager.Instance.ContinueButton()))
            {
                fullscreen = !fullscreen;
            }
            else if (selectedButton == Button.SOUND && (InputManager.Instance.AnyLeftButtonPressed() || InputManager.Instance.AnyRightButtonPressed() || InputManager.Instance.ContinueButton()))
            {
                sound = !sound;
            }
            else if (selectedButton == Button.RESOLUTION)
            {
                if (InputManager.Instance.AnyRightButtonPressed())
                    activeResolution = activeResolution == availableResolutions.Count - 1 ? 0 : (activeResolution + 1);
                else if (InputManager.Instance.AnyLeftButtonPressed())
                    activeResolution = (activeResolution == 0 ? availableResolutions.Count : activeResolution) - 1;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float timeInterval)
        {
            SimpleButton.Instance.Draw(spriteBatch, menu.FontHeading, "Options", new Vector2(100, 100), false, menu.PixelTexture);

            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Screen Resolution", new Vector2(100, 220), selectedButton == Button.RESOLUTION, menu.PixelTexture);
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "< " + availableResolutions[activeResolution].width + " x " + availableResolutions[activeResolution].height + " >", new Vector2(450, 220), false, menu.PixelTexture);

            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Fullscreen", new Vector2(100, 280), selectedButton == Button.FULLSCREEN, menu.PixelTexture);
            if(fullscreen)
                SimpleButton.Instance.Draw(spriteBatch, menu.Font, "< ON >", new Vector2(450, 280), Color.Green, menu.PixelTexture);
            else
                SimpleButton.Instance.Draw(spriteBatch, menu.Font, "< OFF >", new Vector2(450, 280), Color.Red, menu.PixelTexture);

            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Music and Sounds", new Vector2(100, 340), selectedButton == Button.SOUND, menu.PixelTexture);
            if (sound)
                SimpleButton.Instance.Draw(spriteBatch, menu.Font, "< ON >", new Vector2(450, 340), Color.Green, menu.PixelTexture);
            else
                SimpleButton.Instance.Draw(spriteBatch, menu.Font, "< OFF >", new Vector2(450, 340), Color.Red, menu.PixelTexture);

            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Save and Exit", new Vector2(100, 460), selectedButton == Button.BACK, menu.PixelTexture);
            SimpleButton.Instance.Draw(spriteBatch, menu.Font, "Cancel", new Vector2(100, 520), selectedButton == Button.EXIT, menu.PixelTexture);
        }
    }
}
