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
        public string[] Level { get; set; }
        public bool IsGlobal { get; set; } = false;
        public bool IsLocalVariableSource { get; set; } = false;
        public bool IsInMemoryOnly { get; set; } = false;
        public string[] Except { get; set; }
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

    public interface IWantKnowRender 
    {
        TaskRender Render { get; set; }
    }

    public class TaskRndE : TaskRnd, IWantKnowRender
    {
        public TaskText[] Texts { get; set; }
        public TaskRender Render { get; set; }

        public override string GetValue(TaskRandom rnd) => Render.RenderText(Texts[rnd.NextInt(Texts.Length)], rnd.GetSubRandom(), new[] { "" });
    }

    public class TaskSwitch
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string[] Level { get; set; }
        public bool IsGlobal { get; set; } = false;
        public bool IsLocalVariableSource { get; set; } = false;
        public bool IsInMemoryOnly { get; set; } = false;
        public string Value { get; set; }
        public string[] Cases { get; set; }
        public string[] Values { get; set; }
        public string Default { get; set; }
    }

}
