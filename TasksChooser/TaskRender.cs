using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Amporis.TasksChooser
{
    public class TaskRender
    {
        TaskRandom random;
        Tasks tasks;
        TaskSetting setting;
        StringBuilder output;
        Dictionary<int, int> countOfTaskPreviousleyChoosed;
        List<List<TaskItem>> previousChoosedCombinations; // choosed items in each round
        string[] selectedIds; // For Log

        private TaskRender(Tasks tasks, TaskSetting setting)
        {
            this.tasks = tasks;
            this.setting = TaskLoader.MergeSettings(tasks.Setting, setting);
            random = new TaskRandom(setting.Seed);
            output = new StringBuilder();
            countOfTaskPreviousleyChoosed = new Dictionary<int, int>();
            previousChoosedCombinations = new List<List<TaskItem>>();
        }

        internal static string RenderText(TaskText text, TaskRandom random, string[] settingLevel)
        {
            if (text == null) return "";
            string str = text.Text;

            // Random elements
            if (text.Randoms != null)
                foreach (var rnd in text.Randoms)
                    str = str.Replace($"%{rnd.Id}%", LevelCheck(rnd.Level, settingLevel) ? rnd.GetValue(random) : "");

            // Check level for subelements
            if (str.Contains('<') && str.Contains(" level=\""))
            {
                var eText = XElement.Parse($"<text>{str}</text>");
                var subElements = eText.Descendants().ToList();
                foreach (XElement el in subElements)
                    if (el.Attribute("level") != null)
                        if (!LevelCheck(TaskLoader.ReadLevel(el), settingLevel))
                            el.Remove();
                str = eText.InnerText();
            }

            // Hidden HTML tags (starts with '\', e.g. '\&lt;')
            str = str.EncodeEscapedHtmlTags();

            return str;
        }

        private void RenderText(TaskText text)
        {
            output.AppendLine(RenderText(text, random, setting.Level));
        }

        private static bool LevelCheck(string[] itemLevel, string[] wantedLevel)
        {
            if (wantedLevel.Any(c => c.StartsWith("-") && itemLevel.Contains(c.TrimStart('-'))))
                return false; // Wanted level list contains NOT supported code (e.g. "-A"), so if the item has it, it's NO OK
            if (wantedLevel.Any(c => itemLevel.Contains(c)))
                return true;  // One of the item's codes corresponds to one of the required codes
            if (wantedLevel.Contains("*") || itemLevel.Contains("*"))
                return true;  // Wanted level filter or an item allows everything (the bans were dealt with earlier)
            return false;
        }

        private bool RoundCheck(int[] forRounds, int round, bool negate)
        {
            if (forRounds == null || forRounds.Length == 0)
                return true;
            if (negate)
                return !forRounds.Contains(round); // not contains round(s)
            return forRounds.Contains(round);      // contains round(s)
        }

        private void ChooseRandomItems(List<TaskItem> allItems, int count, string[] fromCategory, List<TaskItem> targetList, bool negateFromCategory = false)
        {
            IEnumerable<TaskItem> sourceList;
            // Restrict only to the required categories
            if (fromCategory == null || fromCategory.Contains("*"))
                sourceList = new List<TaskItem>(allItems);
            else
                if (negateFromCategory)
                    sourceList = allItems.Where(x => x.Categories?.Any(c => fromCategory.Contains(c)) != true);
                else 
                    sourceList = allItems.Where(x => x.Categories?.Any(c => fromCategory.Contains(c)) == true);

            // Prepare items for sorting
            sourceList = sourceList.Where(x => !targetList.Contains(x)); // Skip items that are already in the selection
            var lastChoosedItems = previousChoosedCombinations?.LastOrDefault();
            sourceList.ForEach(x => x.OrderData = new TaskItemOrderData() {
                    CountOfPreviousUsing = countOfTaskPreviousleyChoosed.Get(x.Index) ?? 0,
                    WasInPreviousRound = lastChoosedItems?.Contains(x) == true,
                    CountOfPreviousRoundWhenWasInPairWithOtherItem = 0,
                    RandomValue = random.NextDouble(),
                });

            // Anonymous method for sorting of sourceList
            void sortSourceList() => sourceList = sourceList
                .OrderBy(x => x.OrderData.CountOfPreviousUsing) // Sort by the number of previous uses
                .ThenBy(x => x.OrderData.WasInPreviousRound)    // Penalise items chosen in the last round
                .ThenBy(x => x.OrderData.CountOfPreviousRoundWhenWasInPairWithOtherItem) // Only required number (first N)
                .ThenBy(x => x.OrderData.RandomValue);           // At last level, sort randomly   
            sortSourceList(); // 1st sort

            // TODO: stejné kombinace úloh (count > 1), jako v minulsoti při se snažit "rozhodit"
            if (setting.SeparatePreviousPairs && count > 1 && previousChoosedCombinations.Count > 0)
            {
                var selectedItems = new List<TaskItem>();
                selectedItems.Add(sourceList.First());
                sourceList = sourceList.Skip(1); // Remove first item
                while (selectedItems.Count < count && sourceList.Count() > 0)
                {
                    sourceList.ForEach(x => x.OrderData.CountOfPreviousRoundWhenWasInPairWithOtherItem =
                        previousChoosedCombinations
                            .Where(y => y.Contains(x))
                            .Count(y => y.Intersect(selectedItems).Any()));
                            //.Sum(y => prvItems.Count(z => y.Contains(y))));
                    sortSourceList();
                    selectedItems.Add(sourceList.First());
                    sourceList = sourceList.Skip(1); // Remove first item
                }
                sourceList = selectedItems;
            }

            sourceList = sourceList.Take(count).ToList();

            targetList.AddRange(sourceList);

            foreach (var item in sourceList)
                if (countOfTaskPreviousleyChoosed.ContainsKey(item.Index))
                    countOfTaskPreviousleyChoosed[item.Index]++;
                else
                    countOfTaskPreviousleyChoosed[item.Index] = 1;
        }


        private List<TaskItem> MixItems(List<TaskItem> allItems)
        {
            var items = new List<TaskItem>();
            var ic = setting.ItemsCount;
            if (ic.CountsFromCategories == null)
                ChooseRandomItems(allItems, ic.TotalItemsCount ?? 1, null, items); // No filter by categories 
            else
                for (int i = 0; i < ic.CountsFromCategories.Count; i++)  // For each wanted category
                {
                    var item = ic.CountsFromCategories[i];
                    if (item.Categories?.Length == 1 && item.Categories[0] == "*") // "*" - select from all without categories used before
                    {
                        IEnumerable<string> noCats = new string[0];
                        ic.CountsFromCategories.Take(i).ForEach(x => noCats = noCats.Union(x.Categories)); // previously used categories
                        var yesCats = allItems.SelectMany(c => c.Categories).Distinct().Where(x => !noCats.Contains(x)).OrderBy(x => random.NextDouble()).ToArray(); 
                        foreach (var cat in yesCats) // remaining unused categories
                        {
                            ChooseRandomItems(allItems, item.ItemsCount, new[] { cat }, items); // negative selection (not in these categories)
                            if (items.Count > ic.TotalItemsCount) break; // done
                        }
                        if (ic.TotalItemsCount != null && ic.TotalItemsCount < items.Count)
                            items = items.Take((int)ic.TotalItemsCount).ToList(); // truncate now, on last is unnecessary items
                    }
                    else 
                        ChooseRandomItems(allItems, item.ItemsCount, item.Categories, items);
                }
            // Mix/Order
            if (items.Count > 1)
                if (setting.RandomOrder)
                    items = items.OrderBy(x => random.NextDouble()).ToList();
                else
                    items = items.OrderBy(x => x.Index).ToList();
            // Total items count
            if (ic.TotalItemsCount != null && ic.TotalItemsCount < items.Count)
                items = items.Take((int)ic.TotalItemsCount).ToList();
            //lastChoosedItems = items;
            return items;
        }

        private string Render(int round)
        {
            output.Clear();
            // Where condition
            bool testItem(TaskText item) =>
                (item.FromRound == null || round >= item.FromRound) &&  // FromRound
                (item.ToRound == null || round <= item.ToRound) &&      // ToRound
                RoundCheck(item.ForRounds, round, true) &&              // ForRounds
                RoundCheck(item.NotForRounds, round, false) &&          // NotForRounds   
                LevelCheck(item.Level, setting.Level);                  // Level
            // Before
            tasks.Before.Where(x => testItem(x)).ForEach(x => RenderText(x));
            // Items
            var choosedItems = MixItems(tasks.TaskItems.Where(x => testItem(x)).ToList());
            previousChoosedCombinations.Add(choosedItems);
            choosedItems.ForEach(x => RenderText(x));
            selectedIds = choosedItems.Select(t => t.Id).ToArray(); // Log
            // After
            tasks.After.Where(x => testItem(x)).ForEach(x => RenderText(x));
            // Result
            output = output.Replace("{round}", round.ToString());
            output = output.Replace("{date}", DateTime.Today.ToString("dd.MM.yyyy"));
            return output.ToString();
        }


        public static string Render(Tasks tasks, TaskSetting setting)
            => Render(tasks, setting, out string selectedItemIds);

        public static string Render(Tasks tasks, TaskSetting setting, out string selectedItemIds)
        {
            var render = new TaskRender(tasks, setting);
            string result = "";
            for (int i = 1; i <= render.setting.Round; i++)
                result = render.Render(i);
            selectedItemIds = render.selectedIds.ArrayToCommaText();
            return result;
        }

        public static string RenderMulti(Tasks tasks, TaskSetting setting, int roundTo)
            => RenderMulti(tasks, setting, roundTo, out string selectedItemIds);

        public static string RenderMulti(Tasks tasks, TaskSetting setting, int roundTo, out string selectedItemIds)
        {
            var render = new TaskRender(tasks, setting);
            StringBuilder result = new StringBuilder();
            if (roundTo >= render.setting.Round)
                for (int i = 1; i <= roundTo; i++)
                    if (i >= render.setting.Round)
                        result.AppendLine(render.Render(i));
                    else
                        render.Render(i);
            selectedItemIds = render.selectedIds.ArrayToCommaText();
            return result.ToString();
        }
    }
}
