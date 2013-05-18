using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace ParticleStormControl
{
    public class AudioManager
    {
        #region singleton
        private static readonly AudioManager instance = new AudioManager();
        public static AudioManager Instance { get { return instance; } }
        private AudioManager() { }
        #endregion


    }
}