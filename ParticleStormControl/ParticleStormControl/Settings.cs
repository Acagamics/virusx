using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParticleStormControl;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace ParticleStormControl
{
    /// <summary>
    /// Singleton managing all game settings.
    /// Please take care updating related stuff yourself!
    /// </summary>
    class Settings
    {
        private static readonly Settings instance = new Settings();
        static public Settings Instance { get { return instance; } }
        private Settings() { }

        #region Graphics

        public const int MINIMUM_SCREEN_WIDTH = 1024;
        public const int MINIMUM_SCREEN_HEIGHT = 768;

        private bool fullscreen = false;
        public bool Fullscreen { get { return fullscreen; } set { fullscreen = value; } }
        int resolutionX = -1, resolutionY = -1;
        public int ResolutionX { get { return resolutionX; } set { Debug.Assert(value >= MINIMUM_SCREEN_WIDTH); resolutionX = value; } }
        public int ResolutionY { get { return resolutionY; } set { Debug.Assert(value >= MINIMUM_SCREEN_HEIGHT); resolutionY = value; } }

        #endregion

        #region Game

        public class PlayerSettings
        {
            public int ColorIndex;
            public int VirusIndex;
            public int SlotIndex;
            public Player.Type Type;
            public InputManager.ControlType ControlType;
        };
        private List<PlayerSettings> playerSettings = new List<PlayerSettings>(Player.MAX_NUM_PLAYERS);

        public int NumPlayers { get { return playerSettings.Count; } }

        public void AddPlayer(PlayerSettings settings)
        {
            Debug.Assert(NumPlayers < Player.MAX_NUM_PLAYERS);
            playerSettings.Add(settings);
        }

        public void RemovePlayer(int playerIndex)
        {
            playerSettings.RemoveAt(playerIndex);
        }

        public PlayerSettings GetPlayer(int playerIndex)
        {
            return playerSettings[playerIndex];
        }

        public IEnumerable<T> GetPlayerSettingSelection<T>(Func<PlayerSettings, T> selector)
        {
            return playerSettings.Select(selector);
        }

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

        #endregion

        public void Reset()
        {
            ForceFeedback = true;
            Sound = true;
            Music = true;
            Fullscreen = true;
            ResolutionX = MINIMUM_SCREEN_WIDTH;
            ResolutionY = MINIMUM_SCREEN_HEIGHT;

            // choose best available resolution with "color" as default
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

        public Color GetPlayerColor(int playerIndex)
        {
#if _DEBUG
            if(playerIndex >= playerColorIndices.Length || playerIndex < 0)
                throw new Exception("Invalid player index!");
#endif
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
            Reset();
#if !XBOX
            try
            {
                System.Xml.XmlTextReader xmlConfigReader = new System.Xml.XmlTextReader("settings.xml");
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
                                break;

                            case "sound":
                                Sound = Convert.ToBoolean(xmlConfigReader.GetAttribute("sound_on"));
                                Music = Convert.ToBoolean(xmlConfigReader.GetAttribute("music_on"));
                                break;

                            case "input":
                                ForceFeedback = Convert.ToBoolean(xmlConfigReader.GetAttribute("forcefeedback"));
                                break;
                        }
                    }
                }
                xmlConfigReader.Close();
            }
            catch
            {
                // error in xml document - write a new one with standard values
                try
                {
                    Reset();
                    Save();
                }
                catch
                {
                }
            }
#endif
        }

        public void Save()
        {
#if !XBOX
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

            settingsXML.WriteEndElement();
            settingsXML.WriteEndDocument();
            settingsXML.Close();
#endif
        }
    }
}
