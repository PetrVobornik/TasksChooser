using Amporis.TasksChooser;
using System;
using System.Collections.Generic;
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
            string fileName = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.bin";

            RandomTester.CreateBinaryData(fileName);

            Console.WriteLine("Done");
            Console.ReadLine();
        }

    }
}
