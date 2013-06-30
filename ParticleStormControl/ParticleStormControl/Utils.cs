using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirusX
{
    static class Utils
    {
        static public string GenerateTimeString(float time)
        {
            int minutes = (int)(time / 60.0f);
            int seconds = (int)(time - minutes * 60 + 0.5f);
            return String.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}
