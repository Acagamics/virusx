using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

#if !WINDOWS_UWP
            if (System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "de")
            {
                VirusXStrings.Instance.Language = VirusXStrings.Languages.German;
            }
            else
            {
                VirusXStrings.Instance.Language = VirusXStrings.Languages.English;
            }
#endif

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
        public void ReadSettings()
        {
#if !NETFX_CORE // TODO: port
            bool dirty = false;
            Reset();
            System.Xml.XmlTextReader xmlConfigReader = null;
            try
            {
                xmlConfigReader = new System.Xml.XmlTextReader("settings.xml");
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
            catch
            {
                // error in xml document - write a new one with standard values
                try
                {
                    Reset();
                    dirty = true;
                }
                catch
                {
                }
            }
            finally
            {
                if(xmlConfigReader != null)
                    xmlConfigReader.Close();
            }

            if (dirty)
                Save();
#endif
        }

        public void Save()
        {

#if !NETFX_CORE // TODO: port
            System.Xml.XmlTextWriter settingsXML = new System.Xml.XmlTextWriter("settings.xml", System.Text.Encoding.UTF8);
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
            settingsXML.Close();
#endif
        }
    }
}