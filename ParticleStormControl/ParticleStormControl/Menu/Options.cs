using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using VirusX;
using Microsoft.Xna.Framework.Input;

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

            Interface.Add(new InterfaceButton("Options", new Vector2(100, 100), true));

            Interface.Add(new InterfaceButton("Screen Resolution", new Vector2(100, 220), () => { return selectedButton == Button.RESOLUTION; }));
            Interface.Add(new InterfaceButton(() => { return "◄ " + availableResolutions[activeResolution].width + " x " + availableResolutions[activeResolution].height + " ►"; }, new Vector2(450, 220)));

            Interface.Add(new InterfaceButton("Fullscreen", new Vector2(100, 280), () => { return selectedButton == Button.FULLSCREEN; }));
            fullscreenButton = new InterfaceButton(() => { return fullscreen ? "◄ ON ►" : "◄ OFF ►"; }, new Vector2(450, 280), () => { return false; }, Color.White, fullscreen ? Color.Green : Color.Red);
            Interface.Add(fullscreenButton);

            Interface.Add(new InterfaceButton("Sounds", new Vector2(100, 340), () => { return selectedButton == Button.SOUND; }));
            soundButton = new InterfaceButton(() => { return sound ? "◄ ON ►" : "◄ OFF ►"; }, new Vector2(450, 340), () => { return false; }, Color.White, sound ? Color.Green : Color.Red);
            Interface.Add(soundButton);

            Interface.Add(new InterfaceButton("Music", new Vector2(100, 400), () => { return selectedButton == Button.MUSIC; }));
            musicButton = new InterfaceButton(() => { return music ? "◄ ON ►" : "◄ OFF ►"; }, new Vector2(450, 400), () => { return false; }, Color.White, music ? Color.Green : Color.Red);
            Interface.Add(musicButton);

            Interface.Add(new InterfaceButton("Rumble", new Vector2(100, 460), () => { return selectedButton == Button.FORCEFEEDBACK; }));
            forceFeedbackButton = new InterfaceButton(() => { return forceFeedback ? "◄ ON ►" : "◄ OFF ►"; }, new Vector2(450, 460), () => { return false; }, Color.White, forceFeedback ? Color.Green : Color.Red);
            Interface.Add(forceFeedbackButton);

            Interface.Add(new InterfaceButton("► Save and Exit", new Vector2(100, 580), () => { return selectedButton == Button.BACK; }));
            Interface.Add(new InterfaceButton("► Cancel", new Vector2(100, 640), () => { return selectedButton == Button.EXIT; }));
        }

        // if changed to main menu without saving
        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            fullscreen = Settings.Instance.Fullscreen;
            sound = Settings.Instance.Sound;
            music = Settings.Instance.Music;

            selectedButton = Button.BACK;
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
