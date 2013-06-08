using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirusX;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace VirusXStatistics
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Out.WriteLine("VirusX Statistics\n\n");
            Console.Out.WriteLine("Path to statistics: \n");
            String path = @"D:\projekte\_ImagineCup\PSC\particlestorm\ParticleStormControl\ParticleStormControl\bin\x86\Release\";//Console.In.ReadLine();
            path = @"I:\VirusX_Replays\";
            DirectoryInfo directory = new DirectoryInfo(path);

            List<Statistics> statistics = new List<Statistics>();
            int[] gameTypes = new int[(int)InGame.GameMode.NUM_MODES];
            foreach (FileInfo f in directory.GetFiles().Where(x => x.Extension == ".bin"))
            {
                Console.WriteLine(f.FullName);
                Stream streamRead = File.OpenRead(f.FullName);
                BinaryFormatter binaryRead = new BinaryFormatter();
                statistics.Add((Statistics)binaryRead.Deserialize(streamRead));
                streamRead.Close();
                if(f.FullName.Contains(InGame.GameMode.CAPTURE_THE_CELL.ToString()))
                    gameTypes[(int)InGame.GameMode.CAPTURE_THE_CELL]++;
                else if (f.FullName.Contains(InGame.GameMode.CLASSIC.ToString()))
                    gameTypes[(int)InGame.GameMode.CLASSIC]++;
                else if (f.FullName.Contains(InGame.GameMode.FUN.ToString()))
                    gameTypes[(int)InGame.GameMode.FUN]++;
                else if (f.FullName.Contains(InGame.GameMode.DOMINATION.ToString()))
                    gameTypes[(int)InGame.GameMode.DOMINATION]++;
                else if (f.FullName.Contains(InGame.GameMode.LEFT_VS_RIGHT.ToString()))
                    gameTypes[(int)InGame.GameMode.LEFT_VS_RIGHT]++;
            }

            
            int[] winByVirus = new int[(int)VirusSwarm.VirusType.NUM_VIRUSES];
            int[] usedViruses = new int[(int)VirusSwarm.VirusType.NUM_VIRUSES];
            int steps = 0;
            foreach (Statistics stat in statistics)
            {
                for (int index = 0; index < stat.PlayerCount; ++index)
                {
                    usedViruses[(int)stat.getVirusType(index)]++;
                    if(stat.getDeathStepOfPlayer(index) == -1)
                        winByVirus[(int)stat.getVirusType(index)]++;
                }
                
                steps += stat.LastStep;
            }

            steps /= statistics.Count;
            float time = steps * statistics[0].StepTime;
            for (int i = 0; i < usedViruses.Length; ++i)
            {
                Console.Out.WriteLine((VirusSwarm.VirusType)i + ": " + usedViruses[i].ToString() + " / " + winByVirus[i].ToString() + " / " + ((float)winByVirus[i] / usedViruses[i] * 100f).ToString() + "%");
            }
            Console.Out.WriteLine("\nAverage Time: " + time.ToString());
            for (int i = 0; i < gameTypes.Length; ++i)
            {
                Console.Out.WriteLine((InGame.GameMode)i + ": " + gameTypes[i].ToString());
            }
            Console.In.Read();
        }
    }
}
