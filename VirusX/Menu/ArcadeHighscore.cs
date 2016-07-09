using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace VirusX.Menu
{
    class ArcadeHighscore : MenuPage
    {
        public struct HighScoreEntry
        {
            public string PlayerName;
            public float Time;
        };

        const int MAX_NUM_DISPLAYED_SCORED = 10;
        private List<HighScoreEntry> highScoreEntries = new List<HighScoreEntry>();

#if WINDOWS_UWP
        const string highscoreFilename = "arcadescores";
#else
        const string highscoreFilename = "Content/arcadescores";
#endif

        bool enterNewHighScore = false;

        private const int MAX_HIGHSCORE_NAME_LENGTH = 26;
        private string newHighScoreName;

        public float NewHighScoreTime
        {
            get { return newHighScoreTime; }
            set { newHighScoreTime = value; enterNewHighScore = true; }
        }
        private float newHighScoreTime;
        private bool firstOnNewHighscore = false;

        private bool cameFromGame = false;


        // buttons
        enum Button
        {
            AGAIN,
            MAINMENU,

            NUM_BUTTONS
        };
        Button selectedButton = Button.MAINMENU;

        public ArcadeHighscore(Menu menu)
            : base(menu)
        {
            // load highscore or generate generic
            ReadHighScore();

            string text = VirusXStrings.Instance.MainMenuHighscore;
            int width = (int)menu.FontHeading.MeasureString(text).X;

            int height = -300;
            Interface.Add(new InterfaceButton(text, new Vector2(-width / 2, height), true, Alignment.CENTER_CENTER));

            height += 90;

            for (int i = 0; i < MAX_NUM_DISPLAYED_SCORED; ++i)
            {
                int index = i;
                Interface.Add(new InterfaceButton(() => highScoreEntries.Count > index ? highScoreEntries[index].PlayerName : " - ",
                                new Vector2(-300, height + i * 40), false, Alignment.CENTER_CENTER));
                Interface.Add(new InterfaceButton(() => Utils.GenerateTimeString(highScoreEntries.Count > index ? highScoreEntries[index].Time : 0.0f),
                                new Vector2(250, height + i * 40), false, Alignment.CENTER_CENTER));
            }

            // play again button
            text = VirusXStrings.Instance.PlayAgain;
            width = (int)menu.Font.MeasureString(text).X;
            Interface.Add(new InterfaceButton(text,
                new Vector2(-(int)(menu.Font.MeasureString(text).X / 2) - InterfaceImageButton.PADDING, menu.GetFontHeight() * 2 + InterfaceImageButton.PADDING * 7),
                () => { return selectedButton == Button.AGAIN; },
                () => cameFromGame,
                Alignment.BOTTOM_CENTER));

            // main menu button
            text = VirusXStrings.Instance.BackToMainMenu;
            width = (int)menu.Font.MeasureString(text).X;
            Interface.Add(new InterfaceButton(text,
                new Vector2(-(int)(menu.Font.MeasureString(text).X / 2) - InterfaceImageButton.PADDING, menu.GetFontHeight() + InterfaceImageButton.PADDING * 4),
                () => { return selectedButton == Button.MAINMENU; },
                () => true,
                Alignment.BOTTOM_CENTER));

            // enter highscore stuff
            Interface.Add(new InterfaceFiller(Vector2.Zero, Color.Black * 0.8f, () => enterNewHighScore));

            Vector2 stringSize = menu.FontHeading.MeasureString(VirusXStrings.Instance.ScoreNewHighScore);
            Interface.Add(new InterfaceButton(() => (string)VirusXStrings.Instance.ScoreNewHighScore, new Vector2(-stringSize.X / 2, -stringSize.Y * 4.5f), () => false, () => enterNewHighScore, true, Alignment.CENTER_CENTER));

            Interface.Add(new InterfaceButton(() => Utils.GenerateTimeString(NewHighScoreTime), new Vector2(-30, -stringSize.Y * 3.0f), () => false, () => enterNewHighScore, 60, Alignment.CENTER_CENTER));

            const int enterNameWidth = 400;
            stringSize = menu.Font.MeasureString(VirusXStrings.Instance.ScoreEnterYourName);
            Interface.Add(new InterfaceButton(() => newHighScoreName, new Vector2(-enterNameWidth / 2, -stringSize.Y / 2), () => false, () => enterNewHighScore, enterNameWidth, Alignment.CENTER_CENTER));

            stringSize = menu.Font.MeasureString(VirusXStrings.Instance.ScoreSubmitScore);
            Interface.Add(new InterfaceButton((string)VirusXStrings.Instance.ScoreSubmitScore, new Vector2(-stringSize.X / 2, stringSize.Y * 3), () => true, () => enterNewHighScore, Alignment.CENTER_CENTER));


            newHighScoreName = VirusXStrings.Instance.ScoreEnterYourName;
        }


        public async void ReadHighScore()
        {
            int readscores = 0;
            try
            {
#if WINDOWS_UWP
                Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                using (Stream readStream = await localFolder.OpenStreamForReadAsync(highscoreFilename))
#else
                using (Stream readStream = new FileStream(highscoreFilename, FileMode.Open, FileAccess.Read))
#endif
                {
                    using (BinaryReader reader = new BinaryReader(readStream))
                    {
                        readscores = reader.ReadInt32();
                        for(int i=0; i<readscores; ++i)
                        {
                            highScoreEntries.Add(new HighScoreEntry
                            {
                                PlayerName = reader.ReadString(),
                                Time = reader.ReadSingle()
                            });
                        }
                    }
                }
            }
            catch
            {
                // Failed to read the highscore. Ignore error.
            }

            highScoreEntries.OrderByDescending(x => x.Time);
            SaveHighScore();
        }

        public async void SaveHighScore()
        {
            // We used to use encryption with the key saved in this very file which was pretty silly but gave us unreadability for normal users.
            // However, there were some porting problems with that approach. Instead the data is now written binary which should have more or less same effect.
            try
            {
#if WINDOWS_UWP
                Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                using (Stream writeStream = await localFolder.OpenStreamForWriteAsync(highscoreFilename, Windows.Storage.CreationCollisionOption.ReplaceExisting))
#else
                using (Stream writeStream = new FileStream(highscoreFilename, FileMode.Create, FileAccess.Write))
#endif
                {
                    using (BinaryWriter writer = new BinaryWriter(writeStream))
                    {
                        writer.Write(highScoreEntries.Count);
                        for (int i = 0; i < highScoreEntries.Count; ++i)
                        {
                            writer.Write(highScoreEntries[i].PlayerName);
                            writer.Write(highScoreEntries[i].Time);
                        }
                    }
                }
            }
            catch
            {
                // Failed to write the highscore. Ignore error.
            }
        }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            cameFromGame = oldPage == Menu.Page.INGAME;
            firstOnNewHighscore = true;
            base.Update(gameTime);  // reduces flicker
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            // enter name
            if (enterNewHighScore)
            {
                bool upperCase = Keyboard.GetState().GetPressedKeys().Contains(Keys.LeftShift) ||
                                 Keyboard.GetState().GetPressedKeys().Contains(Keys.RightShift) ||
                                 Keyboard.GetState().GetPressedKeys().Contains(Keys.CapsLock);
                foreach (Keys key in Keyboard.GetState().GetPressedKeys())
                {
                    if (InputManager.Instance.IsButtonPressed(key))
                    {
                        char character = InputManager.ConvertKeyboardInput(key, upperCase);
                        bool validKey = character != '\0' || key == Keys.Back || key == Keys.Space;

                        if (firstOnNewHighscore && validKey)
                        {
                            newHighScoreName = "";
                            firstOnNewHighscore = false;
                        }

                        if (key == Keys.Back && newHighScoreName.Length > 0)
                            newHighScoreName = newHighScoreName.Remove(newHighScoreName.Length - 1, 1);
                        else if (key == Keys.Space)
                            newHighScoreName = newHighScoreName.Insert(newHighScoreName.Length, " ");
                        else if (newHighScoreName.Length < MAX_HIGHSCORE_NAME_LENGTH)
                        {
                            if (character != '\0')
                                newHighScoreName += character;
                        }
                    }
                }
            }

            // button control
            if (cameFromGame)
                selectedButton = (Button)(Menu.Loop((int)selectedButton, (int)Button.NUM_BUTTONS, Settings.Instance.StartingControls));
            else
                selectedButton = Button.MAINMENU;

            // cancel - highscore will be lost
            if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.PAUSE)
                || InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.EXIT)
                || (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.HOLD) && !enterNewHighScore)
                || InputManager.Instance.AnyPressedButton(Buttons.Y))
            {
                enterNewHighScore = false;
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);
            }

            // action button
            else if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.ACTION))
            {
                // currently entering highscore
                if (enterNewHighScore)
                {
                    if (newHighScoreName != "")
                    {
                        highScoreEntries.Add(new HighScoreEntry() { Time = NewHighScoreTime, PlayerName = newHighScoreName });
                        highScoreEntries = highScoreEntries.OrderByDescending(x => x.Time).ToList();
                        enterNewHighScore = false;
                        SaveHighScore();
                    }
                }
                // otherwise
                else
                {
                    if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.ACTION))
                    {
                        switch (selectedButton)
                        {
                            case Button.AGAIN:
                                menu.ChangePage(Menu.Page.NEWGAME, gameTime);
                                break;
                            case Button.MAINMENU:
                                menu.ChangePage(Menu.Page.MAINMENU, gameTime);
                                break;
                        }
                    }
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);
        }
    }
}