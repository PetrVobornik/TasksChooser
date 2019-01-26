using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amporis.TasksChooser
{
    public abstract class TaskRnd
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public abstract string GetValue(TaskRandom rnd);
    }

    public class TaskRndS : TaskRnd
    {
        public string[] Values { get; set; }
        public override string GetValue(TaskRandom rnd) => Values[rnd.NextInt(Values.Length)];
    }

    public class TaskRndI : TaskRnd
    {
        public int Minimum { get; set; }
        public int Maximum { get; set; }
        public override string GetValue(TaskRandom rnd) => rnd.NextRange(Minimum, Maximum).ToString();
    }

    public class TaskRndC : TaskRnd
    {
        public string Values { get; set; }
        public char Minimum { get; set; }
        public char Maximum { get; set; }
        public override string GetValue(TaskRandom rnd)
        {
            if (String.IsNullOrEmpty(Values))
                return ((char)(byte)rnd.NextRange((byte)Minimum, (byte)Maximum)).ToString();
            return Values[rnd.NextInt(Values.Length)].ToString();
        }
    }

    public class TaskRndE : TaskRnd
    {
        public TaskText[] Texts { get; set; }

        public override string GetValue(TaskRandom rnd) => TaskRender.RenderText(Texts[rnd.NextInt(Texts.Length)], rnd);
    }
}
