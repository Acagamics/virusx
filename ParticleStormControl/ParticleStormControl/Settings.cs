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

        private InputManager.ControlType[] playerControls = new InputManager.ControlType[]{ InputManager.ControlType.NONE, InputManager.ControlType.NONE, InputManager.ControlType.NONE, InputManager.ControlType.NONE };
        public InputManager.ControlType[] PlayerControls { get { return playerControls; } }
        private int numPlayers;
        public int NumPlayers { get { return numPlayers; } set { numPlayers = value; } }
        private int[] playerColorIndices = new int[] { -1, -1, -1, -1 };
        public int[] PlayerColorIndices { get { return playerColorIndices; } }
        private int[] playerVirusIndices = new int[] { 0, 0, 0, 0 };
        public int[] PlayerVirusIndices { get { return playerVirusIndices; } }
        private bool[] playerConnected = new bool[] { false, false, false, false };
        public bool[] PlayerConnected
        {
            get { return playerConnected; }
            set { playerConnected = value; }
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

        public Color GetPlayerColor(int playerIndex)
        {
#if _DEBUG
            if(playerIndex >= playerColorIndices.Length || playerIndex < 0)
                throw new Exception("Invalid player index!");
#endif
            return Player.Colors[playerColorIndices[playerIndex]];
        }

        public void ResetPlayerSettingsToDefault()
        {
            numPlayers = 0;
            ForceFeedback = true;
            Sound = true;
            Music = true;
#if XBOX
            for (int i = 0; i < 4; ++i)
                numPlayers += GamePad.GetState((PlayerIndex)i).IsConnected ? 1 : 0;

            playerControls = new InputManager.ControlType[] { InputManager.ControlType.GAMEPAD0, InputManager.ControlType.GAMEPAD1, InputManager.ControlType.GAMEPAD2, InputManager.ControlType.GAMEPAD3 };
            
#endif

            for (int i = 0; i < 4; i++)
            {
                ResetPlayerSettingsToDefault(i);
            }
        }

        public void ResetPlayerSettingsToDefault(int index)
        {
            playerControls[index] = InputManager.ControlType.NONE;
            playerConnected[index] = false;
            playerColorIndices[index] = -1;
            playerVirusIndices[index] = 0;
        }

        /// <summary>
        /// loads a configuration from a xml-file - if there isn't one, use default settings
        /// </summary>
        public void ReadSettings()
        {
            // choose best available resolution with "color" as default
            foreach (var mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                if (mode.Format == SurfaceFormat.Color)
                {
                    resolutionX = Math.Max(resolutionX, mode.Width);
                    resolutionY = Math.Max(resolutionY, mode.Height);
                }
            }
            if (resolutionX == -1 || resolutionY == -1)
                throw new Exception("Can't find appropriate resolution - this shouldn't be possible, please contact andreas@acagamics.de");


            ResetPlayerSettingsToDefault();
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
                    Save();
                }
                catch
                {
                }
            }

            if (numPlayers < 0)
                numPlayers = 0;
            else if (numPlayers > 4)
                numPlayers = 4;
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
