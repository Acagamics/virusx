using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;

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

        List<HighScoreEntry> highScoreEntries = new List<HighScoreEntry>();

        const int MAX_NUM_DISPLAYED_SCORED = 10;
        const string highscoreFileLocation = "Content/arcadescores";

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

        // encryption stuff
        const int keySize = 256;
        const string encryptionKey = "1234567890123456"; //"don't even think of it!F9LG@pxd2_7BCc4gff+]@-FG5ugZir#479-/{U>W§D)Fp-_-§_";
        static readonly byte[] key = ASCIIEncoding.UTF8.GetBytes(encryptionKey);
        static readonly PasswordDeriveBytes password = new PasswordDeriveBytes(encryptionKey, null);
        static readonly byte[] keyBytes = password.GetBytes(keySize / 8);
        static readonly byte[] initVectorBytes = Encoding.UTF8.GetBytes(encryptionKey);


        public ArcadeHighscore(Menu menu)
            : base(menu)
        {
            // load highscore or generate generic
            ReadHighScore();

            string text = VirusXStrings.MainMenuHighscore;
            int width = (int)menu.FontHeading.MeasureString(text).X;

            int height = -300;
            Interface.Add(new InterfaceButton(text, new Vector2(-width / 2, height), true, Alignment.CENTER_CENTER));

            height += 90;

            for (int i = 0; i < MAX_NUM_DISPLAYED_SCORED; ++i)
            {
                int index = i;
                Interface.Add(new InterfaceButton(() => highScoreEntries[index].PlayerName, new Vector2(-300, height + i * 40), false, Alignment.CENTER_CENTER));
                Interface.Add(new InterfaceButton(() => Utils.GenerateTimeString(highScoreEntries[index].Time), new Vector2(250, height + i * 40), false, Alignment.CENTER_CENTER));
            }

            // back button
            text = VirusXStrings.BackToMainMenu;
            width = (int)menu.Font.MeasureString(text).X;
            Interface.Add(new InterfaceButton(text,
                new Vector2(-(int)(menu.Font.MeasureString(text).X / 2) - InterfaceImageButton.PADDING, menu.GetFontHeight() + InterfaceImageButton.PADDING * 4), () => true,
                Alignment.BOTTOM_CENTER));

            // enter highscore stuff
            Interface.Add(new InterfaceFiller(Vector2.Zero, Color.Black * 0.8f, () => enterNewHighScore));

            Vector2 stringSize = menu.FontHeading.MeasureString(VirusXStrings.ScoreNewHighScore);
            Interface.Add(new InterfaceButton(() => VirusXStrings.ScoreNewHighScore, new Vector2(-stringSize.X / 2, -stringSize.Y * 4.5f), () => false, () => enterNewHighScore, true, Alignment.CENTER_CENTER));

            Interface.Add(new InterfaceButton(() => Utils.GenerateTimeString(NewHighScoreTime), new Vector2(-30, -stringSize.Y * 3.0f), () => false, () => enterNewHighScore, 60, Alignment.CENTER_CENTER));

            const int enterNameWidth = 400;
            stringSize = menu.Font.MeasureString(VirusXStrings.ScoreEnterYourName);
            Interface.Add(new InterfaceButton(() => newHighScoreName, new Vector2(-enterNameWidth / 2, -stringSize.Y / 2), () => false, () => enterNewHighScore, enterNameWidth, Alignment.CENTER_CENTER));

            stringSize = menu.Font.MeasureString(VirusXStrings.ScoreSubmitScore);
            Interface.Add(new InterfaceButton(VirusXStrings.ScoreSubmitScore, new Vector2(-stringSize.X / 2, stringSize.Y * 3), () => true, () => enterNewHighScore, Alignment.CENTER_CENTER));


            newHighScoreName = VirusXStrings.ScoreEnterYourName;
        }


        public void ReadHighScore()
        {
            try
            {
                byte[] encryptedBytes = File.ReadAllBytes(highscoreFileLocation);

                var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC };
                ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
                byte[] plainTextBytes;
                using (var memoryStream = new MemoryStream(encryptedBytes))
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    plainTextBytes = new byte[encryptedBytes.Length];
                    cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                }
                string xml = Encoding.UTF8.GetString(plainTextBytes);


                // read xml
                XmlReader xmlReader = XmlReader.Create(new StringReader(xml));
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == System.Xml.XmlNodeType.Element)
                    {
                        HighScoreEntry entry;
                        entry.PlayerName = xmlReader.GetAttribute("name");
                        entry.Time = float.Parse(xmlReader.GetAttribute("time"));
                        highScoreEntries.Add(entry);
                    }
                }
            }
            catch
            {
            }

            bool dirty = false;
            while (highScoreEntries.Count < 10)
            {
                highScoreEntries.Add(new HighScoreEntry() { PlayerName = " ", Time = 0.0f });
                dirty = true;
            }
            highScoreEntries.OrderByDescending(x => x.Time);
            if(dirty)
                SafeHighScore();
        }

        public void SafeHighScore()
        {
            // write to string
            using (var stringWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
                {
                    xmlWriter.WriteStartDocument();
                    for(int i=0; i<highScoreEntries.Count; ++i)
                    {
                        xmlWriter.WriteStartElement("elem" + i.ToString());
                        xmlWriter.WriteStartAttribute("name");
                        xmlWriter.WriteValue(highScoreEntries[i].PlayerName);
                        xmlWriter.WriteStartAttribute("time");
                        xmlWriter.WriteValue(highScoreEntries[i].Time);
                    }
                    xmlWriter.WriteEndDocument();
                }

                // encrypt
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(stringWriter.ToString());
                var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC };
                ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
                using(FileStream filestream = new FileStream(highscoreFileLocation, FileMode.Create, FileAccess.Write))
                using (CryptoStream cryptoStream = new CryptoStream(filestream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                }
            }
        }

        public override void OnActivated(Menu.Page oldPage, GameTime gameTime)
        {
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

            if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.PAUSE)
                || InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.EXIT)
                || (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.HOLD) && !enterNewHighScore)
                || InputManager.Instance.AnyPressedButton(Buttons.Y))
            {
                enterNewHighScore = false;
                menu.ChangePage(Menu.Page.MAINMENU, gameTime);
            }
            else if (InputManager.Instance.WasAnyActionPressed(InputManager.ControlActions.ACTION))
            {
                if (enterNewHighScore)
                {
                    if (newHighScoreName != "")
                    {
                        highScoreEntries.Add(new HighScoreEntry() { Time = NewHighScoreTime, PlayerName = newHighScoreName });
                        highScoreEntries = highScoreEntries.OrderByDescending(x => x.Time).ToList();
                        enterNewHighScore = false;
                        SafeHighScore();
                    }
                }
                else
                    menu.ChangePage(Menu.Page.MAINMENU, gameTime);
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);
        }
    }
}
