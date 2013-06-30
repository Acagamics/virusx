
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("UnitTests")]
[assembly: InternalsVisibleTo("VirusXStatistics")]

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
            using (VirusX game = new VirusX())
            {
                game.Run();
            }
        }
    }
#endif
}

