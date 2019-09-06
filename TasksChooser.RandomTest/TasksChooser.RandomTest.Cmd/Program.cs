using Amporis.TasksChooser;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksChooser.RandomTest.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            //string mainSeed = (args?.Length??0) > 0 ? args[0] : "TasksChooser.RandomTest";
            //int contOfColumns = (args?.Length ?? 0) > 1 ? Convert.ToInt32(args[1]) : 100;
            //int contOfIRows = (args?.Length ?? 0) > 2 ? Convert.ToInt32(args[2]) : 100000;
            string fileName = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}";

            RandomTester.CreateBinaryData(fileName + ".bin");
            //CreateTextData(fileName + ".csv");

            Console.WriteLine("Done");
            Console.ReadLine();
        }


        public static void CreateTextData(string fileName, string seed = "TasksChooser.RandomTest", int count = 1000000)
        {
            var rnd = new TaskRandom(seed);

            using (var stream = File.Create(fileName))
            using (var writer = new StreamWriter(stream))
            {
                // numbers
                int tenPercent = count / 10;
                for (int row = 0; row < count; row++)
                {
                    writer.WriteLine(rnd.NextDouble()); //.ToString(CultureInfo.InvariantCulture));
                    if (row % tenPercent == 0)
                        Console.WriteLine("{0:N0}%", 100 * row / (double)count);
                }
            }
            Console.WriteLine("{0:N0}%", 100);
        }


    }
}
