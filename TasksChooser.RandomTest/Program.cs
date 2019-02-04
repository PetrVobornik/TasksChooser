using Amporis.TasksChooser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksChooser.RandomTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string mainSeed = (args?.Length??0) > 0 ? args[0] : "TasksChooser.RandomTest";
            int contOfColumns = (args?.Length ?? 0) > 1 ? Convert.ToInt32(args[1]) : 100;
            int contOfIRows = (args?.Length ?? 0) > 2 ? Convert.ToInt32(args[2]) : 100000;
            string fileName = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.txt";

            var rndForSeeds = new TaskRandom(mainSeed);
            var rnds = new TaskRandom[contOfColumns];

            using (StreamWriter writer = File.CreateText(fileName))
            {
                // 1st row = seeds
                for (int col = 0; col < contOfColumns; col++)
                {
                    string seed = rndForSeeds.NextDouble().ToString("N17").Substring(2);
                    rnds[col] = new TaskRandom(seed);
                    writer.Write(seed+";");
                }
                writer.WriteLine();

                // numbers
                int tenPercent = contOfIRows / 10;
                for (int row = 0; row < contOfIRows; row++)
                {
                    for (int col = 0; col < contOfColumns; col++)
                        writer.Write(rnds[col].NextDouble().ToString("N15") + ";");
                    writer.WriteLine();
                    if (row % tenPercent == 0)
                        Console.WriteLine("{0:N0}%", 100 * row / (double)contOfIRows);
                }
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
