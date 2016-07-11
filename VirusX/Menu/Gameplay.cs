using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace VirusX.Menu
{
    class Gameplay : MenuPage
    {
        Menu.Page origin;

        private const int ARROW_SIZE = 50;
        private InterfaceImageButton leftButton;
        private InterfaceImageButton rightButton;

        private InterfaceImage shownImage;
        private int currentDisplayedImage = 0;
        private int imageOffset = 4;
        private const int NUM_IMAGES = 6;
        private Texture2D[] imageTextures = new Texture2D[NUM_IMAGES]; 
        private string DisplayedImagePath { get { return "gameplay/" + VirusXStrings.Instance.Get("LanguageName") + "/Folie" + (currentDisplayedImage + imageOffset); } }


        public Gameplay(Menu menu)
            : base(menu)
        {
            // background
            Interface.Add(new InterfaceFiller(Vector2.Zero, Color.Black * 0.5f, () => { return origin == Menu.Page.INGAME || origin == Menu.Page.NEWGAME; }));

            int width = 960;
            int height = 720;
            int left = -width / 2;
            int top = -height / 2;       // distance from top

            shownImage = new InterfaceImage(
                DisplayedImagePath,
                new Rectangle(left, top, width, height),
                Color.FromNonPremultiplied(0, 0, 0, 0),
                Alignment.CENTER_CENTER);
            Interface.Add(shownImage);

            // back button
            string label = VirusXStrings.Instance.Get("MenuBack");
            Interface.Add(new InterfaceButton(label, new Vector2(-(int)(menu.Font.MeasureString(label).X / 2), 50), () => { return true; }, Alignment.BOTTOM_CENTER));

            // arrows
            leftButton = new InterfaceImageButton(
                "icons",
                new Vector2((-width / 2) - ARROW_SIZE - InterfaceImageButton.PADDING * 2, 0), // later
                ARROW_SIZE - InterfaceImageButton.PADDING * 2, ARROW_SIZE - InterfaceImageButton.PADDING * 2,
                new Rectangle(0, 0, 16, 16),
                new Rectangle(32, 0, 16, 16),
                () => { return InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.LEFT, Settings.Instance.StartingControls, true); },
                () => { return true; },
                Color.FromNonPremultiplied(0, 0, 0, 0),
                Alignment.CENTER_CENTER
            );
            Interface.Add(leftButton);
            rightButton = new InterfaceImageButton(
                "icons",
                new Vector2((width / 2), 0), // later
                ARROW_SIZE - InterfaceImageButton.PADDING * 2, ARROW_SIZE - InterfaceImageButton.PADDING * 2,
                new Rectangle(16, 0, 16, 16),
                new Rectangle(48, 0, 16, 16),
                () => { return InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.RIGHT, Settings.Instance.StartingControls, true); },
                () => { return true; },
                Color.FromNonPremultiplied(0, 0, 0, 0),
                Alignment.CENTER_CENTER
            );
            Interface.Add(rightButton);
        }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            origin = oldPage;
            base.Update(gameTime);  // reduces flicker
        }

        public override void LoadContent(ContentManager content)
        {
            for (int i = 0; i < NUM_IMAGES; ++i)
            {
                currentDisplayedImage = i;
                imageTextures[i] = content.Load<Texture2D>(DisplayedImagePath);
            }
            currentDisplayedImage = 0;

            base.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.PAUSE)
                || InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.EXIT)
                || InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.ACTION)
                || InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.HOLD)
                || InputManager.Instance.IsButtonPressed(Keys.F1)
                || InputManager.Instance.AnyPressedButton(Buttons.Y))
                menu.ChangePage(origin, gameTime);

            if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.RIGHT, Settings.Instance.StartingControls))
            {
                currentDisplayedImage = (++currentDisplayedImage) % NUM_IMAGES;
                shownImage.Texture = imageTextures[currentDisplayedImage];
            }

            if (InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.LEFT, Settings.Instance.StartingControls))
            {
                if (--currentDisplayedImage < 0)
                    currentDisplayedImage = NUM_IMAGES - 1;
                shownImage.Texture = imageTextures[currentDisplayedImage];
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);
        }
    }
}
