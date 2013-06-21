using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace VirusX.Menu
{
    //[System.Serializable]
    class ArcadeHighscore : MenuPage
    {
      //  [System.Serializable]
        public struct HighScoreEntry
        {
            public string PlayerName;
            public float Time;
        };

        HighScoreEntry[] highScoreEntries;

        const int MAX_NUM_DISPLAYED_SCORED = 10;
        const string highscoreFileLocation = "Content/arcadescores";

      //  const string encryptionKey = "don't even think of it!F9LG@pxd2_7BCc4gff+]@-FG5ugZir#479-/{U>W§D)Fp-_-§_";

        public ArcadeHighscore(Menu menu)
            : base(menu)
        {
            // load highscore or generate generic
            ReadHighScore();

            string text = VirusXStrings.MainMenuHighscore;
            int width = (int)menu.FontHeading.MeasureString(text).X;

            int height = -300;
            Interface.Add(new InterfaceButton(text, new Vector2(-width / 2, height), true, Alignment.CENTER_CENTER));

            height += 50;

            for (int i = 0; i < MAX_NUM_DISPLAYED_SCORED; ++i)
            {
                Interface.Add(new InterfaceButton("asdök", new Vector2(-300, height + i*40), true, Alignment.CENTER_CENTER));
                Interface.Add(new InterfaceButton("00:00", new Vector2(250, height + i * 40), true, Alignment.CENTER_CENTER));
            }

            // back button
            text = VirusXStrings.BackToMainMenu;
            width = (int)menu.Font.MeasureString(text).X;
            Interface.Add(new InterfaceButton(text,
                new Vector2(-(int)(menu.Font.MeasureString(text).X / 2) - InterfaceImageButton.PADDING, menu.GetFontHeight() + InterfaceImageButton.PADDING * 4),
                Alignment.BOTTOM_CENTER));
        }

        public void ReadHighScore()
        {
        /*    try
            {
                // todo: decrypt
                XmlSerializer serializer = new XmlSerializer(typeof(HighScoreEntry[]));
                TextReader textWriter = new StreamReader(highscoreFileLocation);
                highScoreEntries = (HighScoreEntry[])serializer.Deserialize(textWriter);
                textWriter.Close();
            }
            catch*/
            {
                highScoreEntries = new HighScoreEntry[10];
                for (int i = 0; i < 10; ++i)
                    highScoreEntries[i] = new HighScoreEntry() { PlayerName = "asdasdfasdf", Time = 9001.0f };
                SafeHighScore();
            }
        }

        public void SafeHighScore()
        {
            // todo encrypt
         /*   XmlSerializer serializer = new XmlSerializer(typeof(HighScoreEntry[]));
            TextWriter textWriter = new StreamWriter(highscoreFileLocation);
            serializer.Serialize(textWriter, highScoreEntries);
            textWriter.Close(); */
        }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
            base.Update(gameTime);  // reduces flicker
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.PAUSE)
                || InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.EXIT)
                || InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.ACTION)
                || InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.HOLD)
                || InputManager.Instance.AnyPressedButton(Buttons.Y))
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);
        }
    }
}
