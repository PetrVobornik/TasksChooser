using Amporis.TasksChooser;
using System;
using System.IO;

namespace TasksChooser.RandomTest
{
    public class RandomTester
    {
        public static void CreateBinaryData(string fileName, string mainSeed = "TasksChooser.RandomTest", int contOfColumns = 100, int contOfRows = 100000)
        {
            var rndForSeeds = new TaskRandom(mainSeed);
            var rnds = new TaskRandom[contOfColumns];

            using (var stream = File.Create(fileName))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // 1st row = seeds
                for (int col = 0; col < contOfColumns; col++)
                {
                    double d = rndForSeeds.NextDouble();
                    string seed = Convert.ToBase64String(BitConverter.GetBytes(d));
                    rnds[col] = new TaskRandom(seed);
                    writer.Write(d);
                }

                // numbers
                int tenPercent = contOfRows / 10;
                for (int row = 0; row < contOfRows; row++)
                {
                    for (int col = 0; col < contOfColumns; col++)
                        writer.Write(rnds[col].NextDouble());
                    //if (row % tenPercent == 0)
                    //    Console.WriteLine("{0:N0}%", 100 * row / (double)contOfRows);
                }
            }
            //Console.WriteLine("{0:N0}%", 100);
        }

    }
}
