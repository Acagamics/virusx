using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
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

            height += 50;

            for (int i = 0; i < MAX_NUM_DISPLAYED_SCORED; ++i)
            {
                Interface.Add(new InterfaceButton(highScoreEntries[i].PlayerName, new Vector2(-300, height + i * 40), true, Alignment.CENTER_CENTER));
                Interface.Add(new InterfaceButton(Utils.GenerateTimeString(highScoreEntries[i].Time), new Vector2(250, height + i * 40), true, Alignment.CENTER_CENTER));
            }

            // back button
            text = VirusXStrings.BackToMainMenu;
            width = (int)menu.Font.MeasureString(text).X;
            Interface.Add(new InterfaceButton(text,
                new Vector2(-(int)(menu.Font.MeasureString(text).X / 2) - InterfaceImageButton.PADDING, menu.GetFontHeight() + InterfaceImageButton.PADDING * 4), () => true,
                Alignment.BOTTOM_CENTER));
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
                    }
                }
            }
            catch
            {
                highScoreEntries = new HighScoreEntry[10];
                for (int i = 0; i < 10; ++i)
                    highScoreEntries[i] = new HighScoreEntry() { PlayerName = "asdasdfasdf", Time = 9001.0f };
                SafeHighScore();
            }
        }

        public void SafeHighScore()
        {
            // write to string
            using (var stringWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
                {
                    xmlWriter.WriteStartDocument();
                    for(int i=0; i<highScoreEntries.Length; ++i)
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
                MemoryStream filestream = new MemoryStream();//using(FileStream filestream = new FileStream(highscoreFileLocation, FileMode.Create, FileAccess.Write))
                using (CryptoStream cryptoStream = new CryptoStream(filestream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                }
            }
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
