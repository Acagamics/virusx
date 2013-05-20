using System;

using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("UnitTests")]

namespace VirusX
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (ParticleStormControl game = new ParticleStormControl())
            {
                game.Run();
            }
        }
    }
#endif
}

