﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Globalization;

namespace VirusX.Menu
{
    class Options : MenuPage
    {
        Texture2D logo;
        bool fullscreen = Settings.Instance.Fullscreen;
        bool sound = Settings.Instance.Sound;
        bool music = Settings.Instance.Music;
        bool forceFeedback = Settings.Instance.ForceFeedback;

        struct Resolution
        {
            public int width, height;
        };
        List<Resolution> availableResolutions = new List<Resolution>();
        int activeResolution;

        enum Button
        {
            LANGUAGE,
            RESOLUTION,
            FULLSCREEN,
            SOUND,
            MUSIC,
            FORCEFEEDBACK,
            BACK,
            EXIT,

            NUM_BUTTONS
        };
        Button selectedButton = Button.BACK;

        InterfaceButton fullscreenButton;
        InterfaceButton soundButton;
        InterfaceButton musicButton;
        InterfaceButton forceFeedbackButton;

        public Options(Menu menu)
            : base(menu)
        {
            // search.. ehrm.. nearest resolution
            availableResolutions.AddRange(from dispMode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes
                                          where dispMode.Format == SurfaceFormat.Color && dispMode.Width >= Settings.MINIMUM_SCREEN_WIDTH &&
                                                                                          dispMode.Height >= Settings.MINIMUM_SCREEN_HEIGHT &&
                                                                                          dispMode.Width > dispMode.Height
                                          orderby dispMode.Width, dispMode.Height
                                          select new Resolution() { width = dispMode.Width, height = dispMode.Height });
            for (int i = 0; i < availableResolutions.Count; ++i)
            {
                if (availableResolutions[i].width >= Settings.Instance.ResolutionX &&
                    availableResolutions[i].height >= Settings.Instance.ResolutionY)
                {
                    activeResolution = i;
                    break;
                }
            }

            Interface.Add(new InterfaceButton(VirusXStrings.Instance.MainMenuOptions, new Vector2(100, 100), true));

            Interface.Add(new InterfaceButton((string)VirusXStrings.Instance.OptionsLanguage, new Vector2(100, 220), () => { return selectedButton == Button.LANGUAGE; }));
            Interface.Add(new InterfaceButton("◄ " + VirusXStrings.Instance.CurrentLanguageName + " ►", new Vector2(450, 220)));

            Interface.Add(new InterfaceButton((string)VirusXStrings.Instance.OptionsResolution, new Vector2(100, 280), () => { return selectedButton == Button.RESOLUTION; }));
            Interface.Add(new InterfaceButton(() => { return "◄ " + availableResolutions[activeResolution].width + " x " + availableResolutions[activeResolution].height + " ►"; }, new Vector2(450, 280)));

            Interface.Add(new InterfaceButton((string)VirusXStrings.Instance.OptionsFullscreen, new Vector2(100, 340), () => { return selectedButton == Button.FULLSCREEN; }));
            fullscreenButton = new InterfaceButton(() => { return fullscreen ? VirusXStrings.Instance.ON : VirusXStrings.Instance.OFF; }, new Vector2(450, 340), () => { return false; }, Color.White, fullscreen ? Color.Green : Color.Red);
            Interface.Add(fullscreenButton);

            Interface.Add(new InterfaceButton((string)VirusXStrings.Instance.OptionsSound, new Vector2(100, 400), () => { return selectedButton == Button.SOUND; }));
            soundButton = new InterfaceButton(() => { return sound ? VirusXStrings.Instance.ON : VirusXStrings.Instance.OFF; }, new Vector2(450, 400), () => { return false; }, Color.White, sound ? Color.Green : Color.Red);
            Interface.Add(soundButton);

            Interface.Add(new InterfaceButton((string)VirusXStrings.Instance.OptionsMusic, new Vector2(100, 460), () => { return selectedButton == Button.MUSIC; }));
            musicButton = new InterfaceButton(() => { return music ? VirusXStrings.Instance.ON : VirusXStrings.Instance.OFF; }, new Vector2(450, 460), () => { return false; }, Color.White, music ? Color.Green : Color.Red);
            Interface.Add(musicButton);

            Interface.Add(new InterfaceButton((string)VirusXStrings.Instance.OptionsRumble, new Vector2(100, 520), () => { return selectedButton == Button.FORCEFEEDBACK; }));
            forceFeedbackButton = new InterfaceButton(() => { return forceFeedback ? VirusXStrings.Instance.ON : VirusXStrings.Instance.OFF; }, new Vector2(450, 520), () => { return false; }, Color.White, forceFeedback ? Color.Green : Color.Red);
            Interface.Add(forceFeedbackButton);

            Interface.Add(new InterfaceButton((string)VirusXStrings.Instance.OptionsSafeAndExit, new Vector2(100, 620), () => { return selectedButton == Button.BACK; }));
            Interface.Add(new InterfaceButton((string)VirusXStrings.Instance.OptionsCancel, new Vector2(100, 680), () => { return selectedButton == Button.EXIT; }));
        }

        // if changed to main menu without saving
        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            fullscreen = Settings.Instance.Fullscreen;
            sound = Settings.Instance.Sound;
            music = Settings.Instance.Music;

            selectedButton = Button.BACK;
            base.Update(gameTime);  // reduces flicker
        }

        public override void LoadContent(ContentManager content)
        {
            logo = content.Load<Texture2D>("logo");

            base.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            // loopin
            selectedButton = (Button)(Menu.Loop((int)selectedButton, (int)Button.NUM_BUTTONS));

            // button selected
            if ((InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.ACTION) && selectedButton == Button.EXIT) ||
                 InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.EXIT) ||
                 InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.HOLD))
            {
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);
            }
            else if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.ACTION) && selectedButton == Button.BACK)
            {
                Settings.Instance.Fullscreen = fullscreen;
                Settings.Instance.Sound = sound;
                Settings.Instance.Music = music;
                Settings.Instance.ForceFeedback = forceFeedback;
                Settings.Instance.ResolutionX = availableResolutions[activeResolution].width;
                Settings.Instance.ResolutionY = availableResolutions[activeResolution].height;
                Settings.Instance.Save();
                menu.ApplyChangedGraphicsSettings();
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);
            }

            switch (selectedButton)
	        {
                case Button.LANGUAGE:
                    if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.LEFT) ||
                        InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.RIGHT) ||
                        InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.ACTION))
                    {
                        var cultureEn = new CultureInfo("en");
                        var cultureDe = new CultureInfo("de");
                        if (cultureEn.Equals(VirusXStrings.Instance.Culture))
                            VirusXStrings.Instance.Culture = cultureDe;
                        else
                            VirusXStrings.Instance.Culture = cultureEn;

                        menu.ReloadMenus();
                        menu.LoadContent(menu.Game.Content);
                    }
                    break;
		        case Button.RESOLUTION:
                    activeResolution = Menu.Loop(activeResolution, availableResolutions.Count, InputManager.ControlType.NONE, true);
                    break;
                case Button.FULLSCREEN:
                    fullscreen = Menu.Toggle(fullscreen);
                    break;
                case Button.SOUND:
                    sound = Menu.Toggle(sound);
                    break;
                case Button.MUSIC:
                    music = Menu.Toggle(music);
                    break;
                case Button.FORCEFEEDBACK:
                    forceFeedback = Menu.Toggle(forceFeedback);
                 break;
	        }

            // update background colors
            fullscreenButton.BackgroundColor = fullscreen ? Color.Green : Color.Red;
            soundButton.BackgroundColor = sound ? Color.Green : Color.Red;
            musicButton.BackgroundColor = music ? Color.Green : Color.Red;
            forceFeedbackButton.BackgroundColor = forceFeedback ? Color.Green : Color.Red;

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);
        }
    }
}
