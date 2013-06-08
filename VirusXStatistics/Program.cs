//#define HARD_CODED_PATH

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
            String path = Console.In.ReadLine();
#if HARD_CODED_PATH
            path = @"..\..\VirusX_Replays\";//D:\projekte\_ImagineCup\PSC\particlestorm\ParticleStormControl\ParticleStormControl\bin\x86\Release\";//
            //path = @"I:\VirusX_Replays\";
#endif
            DirectoryInfo directory = new DirectoryInfo(path);

            List<Statistics> statisticsAllGames = new List<Statistics>();

            List<Statistics> statisticsCTC = new List<Statistics>();
            List<Statistics> statisticsClassic = new List<Statistics>();
            List<Statistics> statisticsFun = new List<Statistics>();
            List<Statistics> statisticsDomination = new List<Statistics>();
            List<Statistics> statisticsLvsR = new List<Statistics>();
            int[] gameTypes = new int[(int)InGame.GameMode.NUM_MODES];
            foreach (FileInfo f in directory.GetFiles().Where(x => x.Extension == ".bin"))
            {
                Console.WriteLine(f.FullName);
                Stream streamRead = File.OpenRead(f.FullName);
                BinaryFormatter binaryRead = new BinaryFormatter();
                Statistics stat = (Statistics)binaryRead.Deserialize(streamRead);
                statisticsAllGames.Add(stat);
                streamRead.Close();
                if (f.FullName.Contains(InGame.GameMode.CAPTURE_THE_CELL.ToString()))
                {
                    gameTypes[(int)InGame.GameMode.CAPTURE_THE_CELL]++;
                    statisticsCTC.Add(stat);
                }
                else if (f.FullName.Contains(InGame.GameMode.CLASSIC.ToString()))
                {
                    gameTypes[(int)InGame.GameMode.CLASSIC]++;
                    statisticsClassic.Add(stat);
                }
                else if (f.FullName.Contains(InGame.GameMode.FUN.ToString()))
                {
                    gameTypes[(int)InGame.GameMode.FUN]++;
                    statisticsFun.Add(stat);
                }
                else if (f.FullName.Contains(InGame.GameMode.INSERT_MODE_NAME.ToString()))
                {
                    gameTypes[(int)InGame.GameMode.INSERT_MODE_NAME]++;
                    statisticsDomination.Add(stat);
                }
                else if (f.FullName.Contains(InGame.GameMode.LEFT_VS_RIGHT.ToString()))
                {
                    gameTypes[(int)InGame.GameMode.LEFT_VS_RIGHT]++;
                    statisticsLvsR.Add(stat);
                }
            }

            
            int[] winByVirus = new int[(int)VirusSwarm.VirusType.NUM_VIRUSES];
            int[] usedViruses = new int[(int)VirusSwarm.VirusType.NUM_VIRUSES];
            int steps = 0;
            foreach (Statistics stat in statisticsAllGames)
            {
                for (int index = 0; index < stat.PlayerCount; ++index)
                {
                    usedViruses[(int)stat.getVirusType(index)]++;
                    if(stat.getDeathStepOfPlayer(index) == -1)
                        winByVirus[(int)stat.getVirusType(index)]++;
                }
                
                steps += stat.LastStep;
            }

            steps /= statisticsAllGames.Count;
            float time = steps * statisticsAllGames[0].StepTime;
            for (int i = 0; i < usedViruses.Length; ++i)
            {
                Console.Out.WriteLine((VirusSwarm.VirusType)i + ": " + usedViruses[i].ToString() + " / " + winByVirus[i].ToString() + " / " + ((float)winByVirus[i] / usedViruses[i] * 100f).ToString() + "%");
            }
            Console.Out.WriteLine("\nAverage Time: " + time.ToString());
            for (int i = 0; i < gameTypes.Length; ++i)
            {
                Console.Out.WriteLine((InGame.GameMode)i + ": " + gameTypes[i].ToString());
            }

            Console.Out.WriteLine("\nStatistics for CTC:");
            AnalyzeStatistics(statisticsCTC);
            Console.Out.WriteLine("\nStatistics for Classic:");
            AnalyzeStatistics(statisticsClassic);
            Console.Out.WriteLine("\nStatistics for Fun:");
            AnalyzeStatistics(statisticsFun);
            Console.Out.WriteLine("\nStatistics for Domination:");
            AnalyzeStatistics(statisticsDomination);
            Console.Out.WriteLine("\nStatistics for LvsR:");
            AnalyzeStatistics(statisticsLvsR);

            Console.In.Read();
        }

        static private void AnalyzeStatistics(List<Statistics> statistics)
        {
            var stat2Player = statistics.Where(x => x.PlayerCount == 2);
            var stat3Player = statistics.Where(x => x.PlayerCount == 3);
            var stat4Player = statistics.Where(x => x.PlayerCount == 4);

            Console.Out.WriteLine("Beginn analyzing...\n-------------------\n");
            Console.Out.WriteLine("Number of Games: " + statistics.Count.ToString());
            Console.Out.WriteLine(".with 2 Players: " + stat2Player.Count().ToString());
            Console.Out.WriteLine(".with 3 Players: " + stat3Player.Count().ToString());
            Console.Out.WriteLine(".with 4 Players: " + stat4Player.Count().ToString());
            Console.Out.WriteLine();
            if (stat2Player.Count() > 0)
            {
                Console.Out.WriteLine("\nStatistics for 2 player games\n");
                AnalyzeStatistics(stat2Player);
            }
            if (stat3Player.Count() > 0)
            {
                Console.Out.WriteLine("\nStatistics for 3 player games\n");
                AnalyzeStatistics(stat3Player);
            }
            if (stat4Player.Count() > 0)
            {
                Console.Out.WriteLine("\nStatistics for 4 player games\n");
                AnalyzeStatistics(stat4Player);
            }
        }

        static private void AnalyzeStatistics(IEnumerable<Statistics> statistics)
        {
            int[] winByVirus = new int[(int)VirusSwarm.VirusType.NUM_VIRUSES];
            int[] usedViruses = new int[(int)VirusSwarm.VirusType.NUM_VIRUSES];
            int[] winByPlayers = new int[statistics.First().PlayerCount];
            int steps = 0;
            foreach (Statistics stat in statistics)
            {
                for (int index = 0; index < stat.PlayerCount; ++index)
                {
                    usedViruses[(int)stat.getVirusType(index)]++;
                    if (stat.getDeathStepOfPlayer(index) == -1)
                    {
                        winByVirus[(int)stat.getVirusType(index)]++;
                        winByPlayers[index]++;
                    }
                }

                steps += stat.LastStep;
            }

            steps /= statistics.Count();
            float time = steps * statistics.First().StepTime;
            string output = "";
            //string temp = "";
            /*for (int i = 0; i < usedViruses.Length; ++i)
            {
                output = ((VirusSwarm.VirusType)i).ToString();
                if (output.Length < VirusSwarm.VirusType.NUM_VIRUSES.ToString().Length)
                    output += GetSpaceString(VirusSwarm.VirusType.NUM_VIRUSES.ToString().Length - output.Length);
                Console.Out.WriteLine(output + ": " + usedViruses[i].ToString() + " / " + winByVirus[i].ToString() + " / " + ((float)winByVirus[i] / usedViruses[i] * 100f).ToString() + "%");
            }*/

            int sumViruses = usedViruses.Sum();
            int sumWin = winByVirus.Sum();

            Console.Out.WriteLine("{0,-14}|{1,14} |{2,14} |{3,14} |{4,14} |{5,14}",
                "Virus Type","times choosen","overall %","times won","win %","overall win %");
            Console.Out.WriteLine("---------------------------------------------------------------------------------------------");
            for (int i = 0; i < usedViruses.Length; ++i)
            {
                output = String.Format("{0,-14}|{1,14} |{2,13}% |{3,14} |{4,13}% |{5,13}%", 
                    ((VirusSwarm.VirusType)i).ToString(),
                    usedViruses[i],
                    ((float)usedViruses[i] / sumViruses * 100f),
                    winByVirus[i],
                    ((float)winByVirus[i] / usedViruses[i] * 100f),
                    ((float)winByVirus[i] / sumWin * 100f));
                /*output = ((VirusSwarm.VirusType)i).ToString();
                if (output.Length < 13)
                    output += GetSpaceString(13 - output.Length);
                output += "| ";

                temp = usedViruses[i].ToString();
                temp = temp + GetSpaceString(14 - temp.Length);
                output += (temp + "| ");

                compRes = ((float)usedViruses[i] / sumViruses * 100f);*/
                Console.Out.WriteLine(output);// + "| " + compRes.ToString() + "%");
            }
            Console.Out.WriteLine("\nWin ratio by player index: ");
            for (int i = 0; i < winByPlayers.Length; ++i)
            {
                Console.Out.WriteLine("Player " + i.ToString() + ": " + winByPlayers[i] + "/" + statistics.Count() + " --> " + ((float)winByPlayers[i] / statistics.Count() * 100f).ToString() + "%");
            }
            Console.Out.WriteLine("\nAverage time per game: " + time.ToString());
        }

        private static string GetSpaceString(int p)
        {
            string res = "";
            for (int i = 0; i < p; ++i)
                res += " ";
            return res;
        }

        
    }
}
