using System;

namespace TasksChooser.RandomTest.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.bin";

            RandomTester.CreateBinaryData(fileName);

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
