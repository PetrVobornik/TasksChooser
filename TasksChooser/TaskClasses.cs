using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Amporis.TasksChooser
{
    public abstract class TaskItemBase
    {
        public string[] Level { get; set; }
        public int[] ForRounds { get; set; } 
        public int[] NotForRounds { get; set; }
        public int? FromRound { get; set; }
        public int? ToRound { get; set; }
    }

    public class TaskText : TaskItemBase
    {
        public string Text { get; set; }
        public List<TaskRnd> Randoms { get; set; }
    }

    public class TaskItem : TaskText
    {
        internal string Id { get; set; } 
        internal int Index { get; set; }
        public string[] Categories { get; set; }
        internal TaskItemOrderData OrderData { get; set; }
        public override string ToString() => $"{Id}: {OrderData}";
    }

    internal class TaskItemOrderData
    {
        public int CountOfPreviousUsing { get; set; }
        public bool WasInPreviousRound { get; set; }
        public int CountOfPreviousRoundWhenWasInPairWithOtherItem { get; set; }
        public double RandomValue { get; set; }
        public override string ToString() => $"{CountOfPreviousUsing}-{(WasInPreviousRound?1:0)}-{CountOfPreviousRoundWhenWasInPairWithOtherItem}-{RandomValue:N3}";
    }

    [Flags]
    public enum TaskSettingProp { None=0, Seed=1, Round=2, Level=4, ItemsCount=8, RandomOrder=16, SeparatePreviousPairs=32 }

    public class TaskSetting
    {
        public string Seed { get; set; } = ""; //="1";
        public int Round { get; set; } = 1;
        public string[] Level { get; set; } = new [] { "" };
        public TaskItemsCount ItemsCount { get; set; } = new TaskItemsCount();
        public bool RandomOrder { get; set; } = true;
        public bool SeparatePreviousPairs { get; set; } = true;
        public Dictionary<string, string> CustomSettings { get; set; } = new Dictionary<string, string>();

        public TaskSettingProp ReadOnlyProps { get; set; } = 0;
        public TaskSettingProp SetProps { get; set; } = 0;

        public TaskSetting MakeCopy()
        {
            return new TaskSetting()
            {
                Seed = Seed,
                Round = Round,
                Level = Level.ToArray(),
                ItemsCount = ItemsCount,
                RandomOrder = RandomOrder,
                SeparatePreviousPairs = SeparatePreviousPairs,
                ReadOnlyProps = ReadOnlyProps,
                SetProps = SetProps,
                CustomSettings = new Dictionary<string, string>(CustomSettings),
            };
        }

        public static readonly Dictionary<TaskSettingProp, string> ShorPropNames = new Dictionary<TaskSettingProp, string>()
        {
            { TaskSettingProp.Seed, "seed" },
            { TaskSettingProp.Round, "round" },
            { TaskSettingProp.Level, "level" },
            { TaskSettingProp.ItemsCount, "count" },
            { TaskSettingProp.RandomOrder, "random" },
            { TaskSettingProp.SeparatePreviousPairs, "separate" },
        };
    }

    public struct TaskItemsCount
    {
        public int? TotalItemsCount { get; set; }
        public List<TaskItemsFromCategoryCount> CountsFromCategories { get; set; }

        public TaskItemsCount(int? count = 1) {
            TotalItemsCount = count;
            CountsFromCategories = null;
            OriginalString = count.ToString();
        }

        public string OriginalString { get; set; }

        public override string ToString() => OriginalString;
    }

    public struct TaskItemsFromCategoryCount
    {
        public int ItemsCount { get; set; }
        public string[] Categories { get; set; }
    }

    public class Tasks
    {
        public string[] Password { get; set; }
        //public List<TaskText> Headers { get; set; }
        public List<TaskText> Before { get; set; }
        public List<TaskItem> TaskItems { get; set; }
        public List<TaskText> After { get; set; }
        public TaskSetting Setting { get; set; }   
    }

}
