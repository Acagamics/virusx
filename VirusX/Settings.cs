using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace VirusX
{
    /// <summary>
    /// Singleton managing all game settings.
    /// Please take care updating related stuff yourself!
    /// </summary>
    class Settings
    {
        private static readonly Settings instance = new Settings();
        static public Settings Instance { get { return instance; } }
        private Settings() {
            playerSettings = new List<PlayerSettings>(MAX_NUM_PLAYERS);
        }

        #region Graphics

        public const int MINIMUM_SCREEN_WIDTH = 1024;
        public const int MINIMUM_SCREEN_HEIGHT = 768;

        private bool fullscreen = false;
        public bool Fullscreen { get { return fullscreen; } set { fullscreen = value; } }
        int resolutionX = MINIMUM_SCREEN_WIDTH, resolutionY = MINIMUM_SCREEN_HEIGHT;
        public int ResolutionX { get { return resolutionX; } set { Debug.Assert(value >= MINIMUM_SCREEN_WIDTH); resolutionX = value; } }
        public int ResolutionY { get { return resolutionY; } set { Debug.Assert(value >= MINIMUM_SCREEN_HEIGHT); resolutionY = value; } }

        #endregion

        #region Game

        public const int MAX_NUM_PLAYERS = 4;

        public class PlayerSettings
        {
            public int ColorIndex;
            public VirusSwarm.VirusType Virus;
            public int SlotIndex;
            public Player.Teams Team;
            public Player.Type Type;
            public InputManager.ControlType ControlType;
        };
        private List<PlayerSettings> playerSettings;


        public int NumPlayers { get { return playerSettings.Count; } }

        public void AddPlayer(PlayerSettings settings)
        {
            Debug.Assert(NumPlayers < 4);
            playerSettings.Add(settings);
        }

        public void RemovePlayer(int playerIndex)
        {
            playerSettings.RemoveAt(playerIndex);
        }

        public PlayerSettings GetPlayer(int playerIndex)
        {
            if (playerIndex >= 0 && playerIndex < NumPlayers)
                return playerSettings[playerIndex];
            else
                return null;
        }

        public IEnumerable<T> GetPlayerSettingSelection<T>(Func<PlayerSettings, T> selector)
        {
            return playerSettings.Select(selector);
        }


        /// <summary>
        /// active gamemode
        /// </summary>
        public InGame.GameMode GameMode { get; set; }

        private bool useItems = true;
        public bool UseItems { get { return useItems; } set { useItems = value; } }

        /// <summary>
        /// If true the item will be removed after a given amount of time
        /// </summary>
        public bool AutomaticItemDeletion { get { return automaticItemDeletion; } set { automaticItemDeletion = value; } }
        private bool automaticItemDeletion = false;


        #endregion

        #region Misc

        private bool sound = true;
        public bool Sound { get { return sound; } set { sound = value; } }

        private bool music = true;
        public bool Music { get { return music; } set { music = value; } }

        private bool forceFeedback = true;
        public bool ForceFeedback
        {
            get { return forceFeedback; }
            set { InputManager.Instance.ActivateRumble = value; forceFeedback = value; }
        }

        private InputManager.ControlType startingControls = InputManager.ControlType.NONE;
        public InputManager.ControlType StartingControls { get { return startingControls; } set { startingControls = value; } }

        /// <summary>
        /// Is the game running for the first time?
        /// </summary>
        public bool FirstStart { get { return firstStart; } set { firstStart = value; } }
        private bool firstStart = true;

        #endregion

        public void Reset()
        {
            ForceFeedback = true;
            Sound = true;
            Music = true;
            Fullscreen = true;

            VirusXStrings.Instance.ResetLanguageToDefault();

            ChooseStandardResolution();
        }

        /// <summary>
        /// choose best available resolution with "color" as default
        /// </summary>
        private void ChooseStandardResolution()
        {
            ResolutionX = MINIMUM_SCREEN_WIDTH;
            ResolutionY = MINIMUM_SCREEN_HEIGHT;
            foreach (var mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                if (mode.Format == SurfaceFormat.Color &&
                    resolutionX <= mode.Width && resolutionY <= mode.Height)
                {
                    resolutionX = mode.Width;
                    resolutionY = mode.Height;
                }
            }
        }

        /// <summary>
        /// returns color of player
        /// </summary>
        /// <param name="playerIndex">player's index - not slotindex!</param>
        /// <returns>white if invalid index, otherwise player color (not particle color!)</returns>
        public Color GetPlayerColor(int playerIndex)
        {
            if (playerIndex >= playerSettings.Count || playerIndex < 0)
                return Color.White;

            return Player.Colors[playerSettings[playerIndex].ColorIndex];
        }

        public void ResetPlayerSettings()
        {
            playerSettings.Clear();
        }

        /// <summary>
        /// loads a configuration from a xml-file - if there isn't one, use default settings
        /// </summary>
        public async void ReadSettings()
        {
            bool dirty = false;
            Reset(); // Reset to fill out potentially missing settings.

            try
            {
#if WINDOWS_UWP
                // It would be more appropriate to use the local settings folder, but then it is harder to store the xml cross platform.
                //Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                using (System.IO.Stream readStream = await localFolder.OpenStreamForReadAsync("settings.xml"))
#else
                using (System.IO.Stream readStream = new System.IO.FileStream("settings.xml", FileMode.Open, FileAccess.Read))
#endif
                {
                    System.Xml.XmlReader xmlConfigReader = System.Xml.XmlReader.Create(readStream);

                    while (xmlConfigReader.Read())
                    {
                        if (xmlConfigReader.NodeType == System.Xml.XmlNodeType.Element)
                        {
                            switch (xmlConfigReader.Name)
                            {
                                case "display":
                                    fullscreen = Convert.ToBoolean(xmlConfigReader.GetAttribute("fullscreen"));
                                    resolutionX = Convert.ToInt32(xmlConfigReader.GetAttribute("resolutionX"));
                                    resolutionY = Convert.ToInt32(xmlConfigReader.GetAttribute("resolutionY"));

                                    // validate resolution
                                    if (!GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.Any(x => x.Format == SurfaceFormat.Color &&
                                                                                                    x.Height == resolutionY && x.Width == resolutionX))
                                    {
                                        ChooseStandardResolution();
                                        dirty = true;
                                    }
                                    break;

                                case "sound":
                                    Sound = Convert.ToBoolean(xmlConfigReader.GetAttribute("sound_on"));
                                    Music = Convert.ToBoolean(xmlConfigReader.GetAttribute("music_on"));
                                    break;

                                case "input":
                                    ForceFeedback = Convert.ToBoolean(xmlConfigReader.GetAttribute("forcefeedback"));
                                    break;
                                case "misc":
                                    VirusXStrings.Instance.Language = (VirusXStrings.Languages)Enum.Parse(typeof(VirusXStrings.Languages), xmlConfigReader.GetAttribute("language"), true);
                                    break;
                            }
                        }
                    }
                }
            }

            // Error in reading or opening of the xml document - write a new one with standard values.
            catch
            {
                Reset();
                dirty = true;
            }
            finally
            {
                if (dirty)
                    Save();
            }
        }

        public async void Save()
        {
            try
            {
#if WINDOWS_UWP
                // It would be more appropriate to use the local settings folder, but then it is harder to store the xml cross platform.
                //Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                using (System.IO.Stream writeStream = await localFolder.OpenStreamForWriteAsync("settings.xml", Windows.Storage.CreationCollisionOption.ReplaceExisting))
#else
                using (System.IO.Stream writeStream = new System.IO.FileStream("settings.xml", FileMode.Create, FileAccess.Write))
#endif
                {
                    System.Xml.XmlWriter settingsXML = System.Xml.XmlWriter.Create(writeStream);

                    settingsXML.WriteStartDocument();
                    settingsXML.WriteStartElement("settings");

                    settingsXML.WriteStartElement("display");
                    settingsXML.WriteStartAttribute("fullscreen");
                    settingsXML.WriteValue(fullscreen);
                    settingsXML.WriteStartAttribute("resolutionX");
                    settingsXML.WriteValue(resolutionX);
                    settingsXML.WriteStartAttribute("resolutionY");
                    settingsXML.WriteValue(resolutionY);
                    settingsXML.WriteEndElement();

                    settingsXML.WriteStartElement("sound");
                    settingsXML.WriteStartAttribute("sound_on");
                    settingsXML.WriteValue(Sound);
                    settingsXML.WriteStartAttribute("music_on");
                    settingsXML.WriteValue(Music);
                    settingsXML.WriteEndElement();

                    settingsXML.WriteStartElement("input");
                    settingsXML.WriteStartAttribute("forcefeedback");
                    settingsXML.WriteValue(ForceFeedback);
                    settingsXML.WriteEndElement();

                    settingsXML.WriteStartElement("misc");
                    settingsXML.WriteStartAttribute("firststart");
                    settingsXML.WriteValue(FirstStart);

                    settingsXML.WriteStartAttribute("language");
                    settingsXML.WriteValue(VirusXStrings.Instance.Language.ToString());
                    settingsXML.WriteEndElement();

                    settingsXML.WriteEndElement();
                    settingsXML.WriteEndDocument();

                    settingsXML.Flush();
                }
            }
            catch
            {
                // Writing failed! That is unfortunate, but what should we do? Certainly not crash.
            }
        }
    }
}