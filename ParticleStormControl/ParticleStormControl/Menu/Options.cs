using System;
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

        public Options(Menu menu)
            : base(menu)
        {
            // search.. ehrm.. nearest resolution
            availableResolutions.AddRange(from dispMode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes
                                          where dispMode.Format == SurfaceFormat.Color && dispMode.Width >= Settings.MINIMUM_SCREEN_WIDTH &&
                                                                                          dispMode.Height >= Settings.MINIMUM_SCREEN_HEIGHT
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
            Interface.Add(new InterfaceButton(() => { return "< " + availableResolutions[activeResolution].width + " x " + availableResolutions[activeResolution].height + " >"; }, new Vector2(450, 220)));

            Interface.Add(new InterfaceButton("Fullscreen", new Vector2(100, 280), () => { return selectedButton == Button.FULLSCREEN; }));
            Interface.Add(new InterfaceButton(() => { return fullscreen ? "< ON >" : "< OFF >"; }, new Vector2(450, 280), () => { return false; }, Color.White, fullscreen ? Color.Green : Color.Red));

            Interface.Add(new InterfaceButton("Sounds", new Vector2(100, 340), () => { return selectedButton == Button.SOUND; }));
            Interface.Add(new InterfaceButton(() => { return sound ? "< ON >" : "< OFF >"; }, new Vector2(450, 340), () => { return false; }, Color.White, sound ? Color.Green : Color.Red));

            Interface.Add(new InterfaceButton("Music", new Vector2(100, 400), () => { return selectedButton == Button.MUSIC; }));
            Interface.Add(new InterfaceButton(() => { return music ? "< ON >" : "< OFF >"; }, new Vector2(450, 400), () => { return false; }, Color.White, music ? Color.Green : Color.Red));

            Interface.Add(new InterfaceButton("Rumble", new Vector2(100, 460), () => { return selectedButton == Button.FORCEFEEDBACK; }));
            Interface.Add(new InterfaceButton(() => { return forceFeedback ? "< ON >" : "< OFF >"; }, new Vector2(450, 460), () => { return false; }, Color.White, forceFeedback ? Color.Green : Color.Red));

            Interface.Add(new InterfaceButton("Save and Exit", new Vector2(100, 580), () => { return selectedButton == Button.BACK; }));
            Interface.Add(new InterfaceButton("Cancel", new Vector2(100, 640), () => { return selectedButton == Button.EXIT; }));
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
            selectedButton = (Button)(Menu.LoopEnum((int)selectedButton, (int)Button.NUM_BUTTONS));

            // button selected
            bool changedOne = (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.LEFT) || InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.RIGHT) || InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.ACTION));
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
            else if (changedOne && selectedButton == Button.FULLSCREEN)
            {
                fullscreen = !fullscreen;
            }
            else if (changedOne && selectedButton == Button.SOUND)
            {
                sound = !sound;
            }
            else if (changedOne && selectedButton == Button.MUSIC)
            {
                music = !music;
            }
            else if (changedOne && selectedButton == Button.FORCEFEEDBACK)
            {
                forceFeedback = !forceFeedback;
            }
            else if (changedOne && selectedButton == Button.RESOLUTION)
            {
                if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.RIGHT))
                    activeResolution = activeResolution == availableResolutions.Count - 1 ? 0 : (activeResolution + 1);
                else if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.LEFT))
                    activeResolution = (activeResolution == 0 ? availableResolutions.Count : activeResolution) - 1;
            }

            // update background colors
            ((InterfaceButton)Interface[4]).BackgroundColor = fullscreen ? Color.Green : Color.Red;
            ((InterfaceButton)Interface[6]).BackgroundColor = sound ? Color.Green : Color.Red;
            ((InterfaceButton)Interface[8]).BackgroundColor = music ? Color.Green : Color.Red;
            ((InterfaceButton)Interface[10]).BackgroundColor = forceFeedback ? Color.Green : Color.Red;

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);
            //Button.Instance.Draw(spriteBatch, menu.FontHeading, "Options", new Vector2(100, 100), false, menu.TexPixel);

            //Button.Instance.Draw(spriteBatch, menu.Font, "Screen Resolution", new Vector2(100, 220), selectedButton == Button.RESOLUTION, menu.TexPixel);
            //Button.Instance.Draw(spriteBatch, menu.Font, "< " + availableResolutions[activeResolution].width + " x " + availableResolutions[activeResolution].height + " >", new Vector2(450, 220), false, menu.TexPixel);

            //Button.Instance.Draw(spriteBatch, menu.Font, "Fullscreen", new Vector2(100, 280), selectedButton == Button.FULLSCREEN, menu.TexPixel);
            //if(fullscreen)
            //    Button.Instance.Draw(spriteBatch, menu.Font, "< ON >", new Vector2(450, 280), Color.Green, menu.TexPixel);
            //else
            //    Button.Instance.Draw(spriteBatch, menu.Font, "< OFF >", new Vector2(450, 280), Color.Red, menu.TexPixel);

            //Button.Instance.Draw(spriteBatch, menu.Font, "Sounds", new Vector2(100, 340), selectedButton == Button.SOUND, menu.TexPixel);
            //if (sound)
            //    Button.Instance.Draw(spriteBatch, menu.Font, "< ON >", new Vector2(450, 340), Color.Green, menu.TexPixel);
            //else
            //    Button.Instance.Draw(spriteBatch, menu.Font, "< OFF >", new Vector2(450, 340), Color.Red, menu.TexPixel);

            //Button.Instance.Draw(spriteBatch, menu.Font, "Music", new Vector2(100, 400), selectedButton == Button.MUSIC, menu.TexPixel);
            //if (music)
            //    Button.Instance.Draw(spriteBatch, menu.Font, "< ON >", new Vector2(450, 400), Color.Green, menu.TexPixel);
            //else
            //    Button.Instance.Draw(spriteBatch, menu.Font, "< OFF >", new Vector2(450, 400), Color.Red, menu.TexPixel);

            //Button.Instance.Draw(spriteBatch, menu.Font, "Rumble", new Vector2(100, 460), selectedButton == Button.FORCEFEEDBACK, menu.TexPixel);
            //if (forceFeedback)
            //    Button.Instance.Draw(spriteBatch, menu.Font, "< ON >", new Vector2(450, 460), Color.Green, menu.TexPixel);
            //else
            //    Button.Instance.Draw(spriteBatch, menu.Font, "< OFF >", new Vector2(450, 460), Color.Red, menu.TexPixel);

            //Button.Instance.Draw(spriteBatch, menu.Font, "Save and Exit", new Vector2(100, 580), selectedButton == Button.BACK, menu.TexPixel);
            //Button.Instance.Draw(spriteBatch, menu.Font, "Cancel", new Vector2(100, 640), selectedButton == Button.EXIT, menu.TexPixel);
        }
    }
}
