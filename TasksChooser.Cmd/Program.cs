using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amporis.TasksChooser;

namespace Amporis.TasksChooser.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            var tasks = TaskLoader.LoadTasksFromFile("data1.xml");
            var settings = tasks.Setting.MakeCopy();
            settings.Seed = "abc123";
            settings.SetProps |= TaskSettingProp.Round;

            for (int i = 1; i <= 10; i++)
            {
                settings.Round = i;
                Console.WriteLine($"Round {i}");
                var text = TaskRender.Render(tasks, settings);
                Console.WriteLine(text);
            }

            Console.ReadLine();
        }
    }
}
