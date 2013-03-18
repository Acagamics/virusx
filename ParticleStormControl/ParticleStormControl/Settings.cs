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

        private bool fullscreen = false;
        public bool Fullscreen { get { return fullscreen; } set { fullscreen = value; } }
        int resolutionX = -1, resolutionY = -1;
        public int ResolutionX { get { return resolutionX; } set { Debug.Assert(value >= MINIMUM_SCREEN_WIDTH); resolutionX = value; } }
        public int ResolutionY { get { return resolutionY; } set { resolutionY = value; } }
        #endregion

        #region Game
        private Player.ControlType[] playerControls;
        public Player.ControlType[] PlayerControls { get { return playerControls; } }
        private int numPlayers;
        public int NumPlayers { get { return numPlayers; } set { numPlayers = value; } }
        private int[] playerColorIndices = new int[]{ 0, 1, 2, 3 };
        public int[] PlayerColorIndices { get { return playerColorIndices; } }
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
#if XBOX
            numPlayers = 0;
            for (int i = 0; i < 4; ++i)
                numPlayers += GamePad.GetState((PlayerIndex)i).IsConnected ? 1 : 0;

            playerControls = new Player.ControlType[] { Player.ControlType.GAMEPAD0, Player.ControlType.GAMEPAD1, Player.ControlType.GAMEPAD2, Player.ControlType.GAMEPAD3 };
#else
            numPlayers = 2;
            playerControls = new Player.ControlType[] { Player.ControlType.KEYBOARD0, Player.ControlType.KEYBOARD1, Player.ControlType.GAMEPAD0, Player.ControlType.GAMEPAD1 };
#endif
            for(int i=0; i<playerColorIndices.Length; ++i)
                playerColorIndices[i] = i;
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

                            case "player":
                                numPlayers = Convert.ToInt32(xmlConfigReader.GetAttribute("numPlayers"));
                                for (int i = 0; i < 4; ++i)
                                {
                                    Enum.TryParse(xmlConfigReader.GetAttribute("controls" + i.ToString()), out playerControls[i]);
                                    playerColorIndices[i] = Convert.ToInt32(xmlConfigReader.GetAttribute("color" + i.ToString()));
                                }

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

            if (numPlayers < 2)
                numPlayers = 2;
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

            settingsXML.WriteStartElement("player");
            settingsXML.WriteStartAttribute("numPlayers");
            settingsXML.WriteValue(numPlayers);

            for (int i = 0; i < 4; ++i)
            {
                settingsXML.WriteStartAttribute("controls" + i);
                settingsXML.WriteValue(playerControls[i].ToString());
                settingsXML.WriteStartAttribute("color" + i);
                settingsXML.WriteValue(playerColorIndices[i].ToString());
            }

            settingsXML.WriteEndElement();

            settingsXML.WriteEndElement();
            settingsXML.WriteEndDocument();
            settingsXML.Close();
#endif
        }
    }
}
