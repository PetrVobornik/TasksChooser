using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Amporis.TasksChooser
{
    public class TaskLoader
    {
        private TaskRnd LoadTaskRnd(XElement eRnd)
        {
            TaskRnd rnd;
            char separator = ',';
            switch (eRnd.Name.ToString().ToUpper().Last())
            {
                case 'I':
                    rnd = new TaskRndI()
                    {
                        Minimum = eRnd.GetAtt("min", 0),
                        Maximum = eRnd.GetAtt("max", 0),
                    };
                    break;
                case 'C':
                    rnd = new TaskRndC()
                    {
                        Minimum = eRnd.GetAtt("min", "A").FirstOrDefault(),
                        Maximum = eRnd.GetAtt("max", "Z").FirstOrDefault(),
                        Values = eRnd.GetAtt("values", ""),
                    };
                    break;
                case 'E': 
                    rnd = new TaskRndE()
                    {
                        Texts = eRnd.Elements("item").Select(x => LoadTaskText(x)).ToArray(),
                    };
                    break;
                //case 'G':
                //    rnd = new TaskRndG(); // Getter for global variables
                //    break;
                //case 'V':
                //    rnd = new TaskRndV(); // Getter for local variables
                //    break;
                case 'S':
                default: 
                    string values = eRnd.GetAtt("values", "");
                    separator = eRnd.GetAtt("separator", ",").FirstOrDefault();
                    rnd = new TaskRndS()
                    {
                        Values = values.Split(separator),
                    };
                    break;
            }
            rnd.Level = ReadLevel(eRnd);
            rnd.IsGlobal = eRnd.GetAtt("g", false);
            string id = eRnd.GetAtt("id", String.Empty);
            rnd.IsLocalVariableSource = !rnd.IsGlobal && !String.IsNullOrEmpty(id); 
            if (!String.IsNullOrEmpty(id))
                rnd.Id = id; // else has GUID as Id
            rnd.IsInMemoryOnly = eRnd.GetAtt("mem", false);
            rnd.Except = eRnd.GetAtt("except", "").Split(separator);
            return rnd;
        }

        internal static string[] ReadLevel(XElement eText)
            => eText.Attribute("level")?.Value?.Split(',') ?? new[] { "" };


        private TaskSwitch LoadTaskSwitch(XElement eSwitch)
        {
            char separator = eSwitch.GetAtt("separator", ",").FirstOrDefault();
            TaskSwitch sw = new TaskSwitch();
            sw.Level = ReadLevel(eSwitch);
            sw.IsInMemoryOnly = eSwitch.GetAtt("mem", false);
            sw.IsGlobal = eSwitch.GetAtt("g", false);
            string id = eSwitch.GetAtt("id", String.Empty);
            sw.IsLocalVariableSource = !sw.IsGlobal && !String.IsNullOrEmpty(id);
            if (!String.IsNullOrEmpty(id))
                sw.Id = id; // else has GUID as Id
            sw.Value = eSwitch.GetAtt("value", "");
            sw.Default = eSwitch.GetAtt("default", "");
            sw.Cases = eSwitch.GetAtt("cases", "").Split(separator);
            sw.Values = eSwitch.GetAtt("values", "").Split(separator);
            return sw;
        }

        private TaskText LoadTaskText(XElement eText, TaskText text)
        {
            text.Level = ReadLevel(eText);
            text.ForRounds = eText.Attribute("forRound")?.Value?.Split(',').Select(s => Convert.ToInt32(s)).ToArray();
            text.NotForRounds = eText.Attribute("notForRound")?.Value?.Split(',').Select(s => Convert.ToInt32(s)).ToArray();
            text.FromRound = eText.GetAtt<int?>("fromRound", null);
            text.ToRound = eText.GetAtt<int?>("toRound", null);
            text.Randoms = new List<TaskRnd>();
            text.Switches = new List<TaskSwitch>();
            text.Variables = new List<string>();

            var subElements = eText.Descendants().ToList();
            foreach (XElement el in subElements)
            {
                // Randoms
                if (el.Name.ToString().ToLower().StartsWith("rnd") && el.Name.ToString().Length == 4)
                {
                    var rnd = LoadTaskRnd(el);
                    text.Randoms.Add(rnd);
                    if (!rnd.IsInMemoryOnly)
                        el.AddBeforeSelf($"%{rnd.Id}%");
                    el.Remove();
                }
                // Switches
                if (el.Name.ToString().ToLower() == "switch")
                {
                    var sw = LoadTaskSwitch(el);
                    text.Switches.Add(sw);
                    if (!sw.IsInMemoryOnly)
                        el.AddBeforeSelf($"%{sw.Id}%");
                    el.Remove();
                }
                // Variables
                if (el.Name.ToString().ToLower() == "var")
                {
                    string varId = el.GetAtt("id", String.Empty);
                    el.AddBeforeSelf($"%{varId}%");
                    if (!text.Variables.Contains(varId))
                        text.Variables.Add(varId);
                    el.Remove();
                }
            }
            // Remove Ids added in Randoms or Switches
            text.Variables.RemoveAll(x => text.Randoms.Any(y => y.Id == x) || text.Switches.Any(y => y.Id == x));

            text.Text = eText.InnerText();
            return text;
        }

        private TaskText LoadTaskText(XElement eText)
        {
            var text = new TaskText();
            LoadTaskText(eText, text);
            return text;
        }

        private TaskItem LoadTaskItem(XElement eItem, int index)
        {
            var item = new TaskItem();
            LoadTaskText(eItem, item);
            item.Categories = eItem.Attribute("cat")?.Value?.Split(',') ?? new[] { "" };
            item.Id = TaskExtensions.GetFirstNotNull(eItem.Attribute("id")?.Value, "id-"+index.ToString());
            item.Index = index;
            return item;
        }

        public static TaskItemsCount LoadItemsCount(string itemsCount)
        {
            var tic = new TaskItemsCount();
            string ic = itemsCount.Trim();
            if (String.IsNullOrEmpty(ic)) return tic;
            tic.OriginalString = ic;

            // "3" - Total count only (one number only)
            if (int.TryParse(ic, out int count))
            {
                tic.TotalItemsCount = count;
                return tic;
            } // else - more complex code...

            // "3:..." - Total cound and next breakdown
            if (ic.Contains(':'))
            {
                var icParts = ic.Split(':');
                if (!int.TryParse(icParts[0], out count))
                    throw new TaskException($"ItemsCount ('{ic}') parsing error: '{icParts[0]}' (count of items) is not valid integer");
                tic.TotalItemsCount = count;
                ic = icParts[1];
            } else
                tic.TotalItemsCount = null; // total items count is not defined
            if (String.IsNullOrEmpty(ic)) return tic;

            // "2/a,1/b,3/c|d,1/*"
            tic.CountsFromCategories = new List<TaskItemsFromCategoryCount>();
            var cats = ic.Split(',').Select(s => s.Trim()).ToArray(); // "2/a", "1/b", "3/c|d"
            foreach (var cat in cats)
            {
                var catParts = cat.Split('/');
                if (catParts.Length != 2)
                    throw new TaskException($"ItemsCount ('{ic}') parsing error: '{cat}' is invalid (right format is '3/a' or '3/a|b')");
                if (!int.TryParse(catParts[0], out count))
                    throw new TaskException($"ItemsCount ('{ic}') parsing error: '{catParts[0]}' (count of items form category {catParts[1]}) is not valid integer");
                tic.CountsFromCategories.Add(new TaskItemsFromCategoryCount() {
                    ItemsCount = count,
                    Categories = catParts[1].Split('|'),
                });
            }

            return tic;
        }

        private TaskSetting LoadTaskSettings(XElement eSettings)
        {
            var setting = new TaskSetting();
            if (eSettings == null) return setting;
            T setProp<T> (TaskSettingProp prop, T defaultValue) {
                string sVal = eSettings.GetAtt(TaskSetting.ShorPropNames[prop], "");
                if (sVal.StartsWith("!"))
                {
                    setting.ReadOnlyProps |= prop;
                    sVal = sVal.TrimStart('!');
                }
                if (!String.IsNullOrEmpty(sVal))
                    setting.SetProps |= prop;
                return TaskExtensions.ConvertValue(sVal, defaultValue);
            }
            setting.Level = setProp(TaskSettingProp.Level, "").Split(',') ?? setting.Level;
            setting.RandomOrder = setProp(TaskSettingProp.RandomOrder, setting.RandomOrder);
            setting.Round = setProp(TaskSettingProp.Round, setting.Round);
            setting.Seed = setProp(TaskSettingProp.Seed, setting.Seed);
            setting.ItemsCount = LoadItemsCount(setProp(TaskSettingProp.ItemsCount, ""));
            setting.SeparatePreviousPairs = setProp(TaskSettingProp.SeparatePreviousPairs, setting.SeparatePreviousPairs);
            eSettings.Attributes().Where(a => a.Name.LocalName.StartsWith("custom_")).ForEach(a => 
                setting.CustomSettings.Add(a.Name.LocalName.Substring(7), a.Value));
            return setting;
        }

        private static TaskSetting MergeTwoSettings(TaskSetting baseSettings, TaskSetting externalSettings)
        {
            if (externalSettings == null) return baseSettings?.MakeCopy();
            if (baseSettings == null) return externalSettings.MakeCopy();
            var setting = new TaskSetting();
            T copyProp<T>(TaskSettingProp prop, T baseVal, T extVal)
            {
                if (baseSettings.ReadOnlyProps.HasFlag(prop))     // Base is read-only, external value is skiped
                {
                    setting.ReadOnlyProps |= prop;                // Copy RO flag
                    return baseVal;
                }
                if (externalSettings.ReadOnlyProps.HasFlag(prop)) // Copy RO flag
                    setting.ReadOnlyProps |= prop;
                if (!externalSettings.SetProps.HasFlag(prop))     // External value was not set (is default) - use base value
                    return baseVal;
                return extVal;
            }
            setting.Level = copyProp(TaskSettingProp.Level, baseSettings.Level, externalSettings.Level);
            setting.RandomOrder = copyProp(TaskSettingProp.RandomOrder, baseSettings.RandomOrder, externalSettings.RandomOrder);
            setting.Round = copyProp(TaskSettingProp.Round, baseSettings.Round, externalSettings.Round);
            setting.Seed = baseSettings?.Seed ?? "";
            setting.Seed += externalSettings.Seed; //copyProp(TaskSettingProp.Seed, baseSettings.Seed, externalSettings.Seed);
            setting.ItemsCount = copyProp(TaskSettingProp.ItemsCount, baseSettings.ItemsCount, externalSettings.ItemsCount);
            setting.SeparatePreviousPairs = copyProp(TaskSettingProp.SeparatePreviousPairs, baseSettings.SeparatePreviousPairs, externalSettings.SeparatePreviousPairs);
            return setting;
        }

        public static TaskSetting MergeSettings(params TaskSetting[] settings)
        {
            if ((settings?.Length ?? 0) <= 1)
                return settings?.FirstOrDefault();
            var setting = settings?.FirstOrDefault();
            for (int i = 1; i < settings.Length; i++)
                setting = MergeTwoSettings(setting, settings[i]);
            return setting;
        }

        public static bool ChcekPassword(string[] filePassword, string requestPassword)
        {
            if (filePassword == null ||
                filePassword.Length == 0 ||
                filePassword.Any(p => String.IsNullOrEmpty(p)))
                return true; // File is not locked
            if (String.IsNullOrEmpty(requestPassword))
                return false;
            return filePassword.Any(p => p == requestPassword);
        }

        private Tasks Load(XElement eTasks)
        {
            var tasks = new Tasks();
            int i = 0;
            tasks.Password = eTasks.Attribute("password")?.Value?.Split(',');
            tasks.Before = eTasks.Elements("before")?.Select(el => LoadTaskText(el)).ToList();
            tasks.TaskItems = eTasks.Element("items")?.Elements("item")?.Select(el => LoadTaskItem(el, ++i)).ToList();
            tasks.After = eTasks.Elements("after")?.Select(el => LoadTaskText(el)).ToList();
            tasks.Setting = LoadTaskSettings(eTasks.Element("settings"));
            return tasks;
        }


        public static Tasks LoadTasksFromFile(string tasksFilePath) => LoadTasks(XDocument.Load(tasksFilePath));

        public static Tasks LoadTasks(string tasksData) => LoadTasks(XDocument.Parse(tasksData));

        public static Tasks LoadTasks(TextReader tasksData) => LoadTasks(XDocument.Load(tasksData));

        public static Tasks LoadTasks(Stream tasksData) => LoadTasks(XDocument.Load(tasksData));

        public static Tasks LoadTasks(XDocument tasksDocument) => LoadTasks(tasksDocument.Root);

        public static Tasks LoadTasks(XElement tasksElement) => (new TaskLoader()).Load(tasksElement);

    }
}
