using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amporis.TasksChooser;
using Amporis.TasksChooser;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http.Extensions;
using System.Threading.Tasks;

namespace Amporis.TasksChooser.Web
{
    public class TasksProces
    {
        public static string GetDataFileName(string fileName)
        {
            //string fileName = request.Path.ToString().TrimStart('/');
            if (String.IsNullOrEmpty(fileName)) return String.Empty;
            string dataDir = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            string path = Path.Combine(dataDir, Path.ChangeExtension(fileName, ".xml"));
            if (!File.Exists(path)) return String.Empty;
            return path;
        }

        private static T ReadPropFromForm<T>(HttpRequest request, LaunchData ld, string propKeyPrefix, TaskSettingProp prop, T defaultValue)
        {
            string key = propKeyPrefix + TaskSetting.ShorPropNames[prop];
            if (request.Form.TryGetValue(key, out StringValues sVal))
                if (!String.IsNullOrEmpty(sVal))
                {
                    var val = TaskExtensions.ConvertValue(defaultValue.GetType(), sVal, null);
                    if (val != null && val is T)
                    {
                        ld.Settings.SetProps |= prop;
                        return (T)val;
                    }
                }
            return defaultValue;
        }


        private static LaunchData GetLaunchDataFromMoodle(HttpRequest request)
        {
            // TODO: chceck OAuth hash
            var ld = new LaunchData();
            if (request.Form?.TryGetValue("custom_file", out StringValues file) == true) ld.File = file;
            if (request.Form?.TryGetValue("oauth_consumer_key", out StringValues pass) == true) ld.Password = pass;
            if (request.Form?.TryGetValue("custom_html", out StringValues html) == true) ld.AddHtmlCode = html == "1";
            if (request.Form?.TryGetValue("custom_nocopy", out StringValues noCopy) == true) ld.AddCopyProtection = noCopy == "1";
            if (request.Form?.TryGetValue("custom_multi", out StringValues multi) == true) ld.RenderMultiTo = multi.ToString().ToInt();
            T setProp<T>(TaskSettingProp prop, T defaultValue) => ReadPropFromForm(request, ld, "custom_", prop, defaultValue);
            ld.Settings.ItemsCount = TaskLoader.LoadItemsCount(setProp(TaskSettingProp.ItemsCount, ""));
            ld.Settings.Level = setProp(TaskSettingProp.Level, "").Split(',');
            ld.Settings.RandomOrder = setProp(TaskSettingProp.RandomOrder, ld.Settings.RandomOrder);
            ld.Settings.Round = setProp(TaskSettingProp.Round, ld.Settings.Round);
            ld.Settings.SeparatePreviousPairs = setProp(TaskSettingProp.SeparatePreviousPairs, ld.Settings.SeparatePreviousPairs);
            ld.Settings.Seed = setProp(TaskSettingProp.Seed, ld.Settings.Seed);
            if (request.Form?.TryGetValue("lis_person_contact_email_primary", out StringValues email) == true)
                ld.Settings.Seed += email;
            return ld;
        }

        private static LaunchData GetLaunchDataFromPost(HttpRequest request)
        {
            var ld = new LaunchData();
            if (request.Form?.TryGetValue("file", out StringValues file) == true) ld.File = file;
            if (request.Form?.TryGetValue("password", out StringValues pass) == true) ld.Password = pass;
            if (request.Form?.TryGetValue("html", out StringValues html) == true) ld.AddHtmlCode = html == "1";
            if (request.Form?.TryGetValue("nocopy", out StringValues noCopy) == true) ld.AddCopyProtection = noCopy == "1";
            if (request.Form?.TryGetValue("multi", out StringValues multi) == true) ld.RenderMultiTo = multi.ToString().ToInt();
            T setProp<T>(TaskSettingProp prop, T defaultValue) => ReadPropFromForm(request, ld, "", prop, defaultValue);
            ld.Settings.ItemsCount = TaskLoader.LoadItemsCount(setProp(TaskSettingProp.ItemsCount, ""));
            ld.Settings.Level = setProp(TaskSettingProp.Level, "").Split(',');
            ld.Settings.RandomOrder = setProp(TaskSettingProp.RandomOrder, ld.Settings.RandomOrder);
            ld.Settings.Round = setProp(TaskSettingProp.Round, ld.Settings.Round);
            ld.Settings.SeparatePreviousPairs = setProp(TaskSettingProp.SeparatePreviousPairs, ld.Settings.SeparatePreviousPairs);
            ld.Settings.Seed = setProp(TaskSettingProp.Seed, ld.Settings.Seed);
            return ld;
        }

        private static LaunchData GetLaunchDataFromGet(HttpRequest request)
        {
            var ld = new LaunchData();
            ld.File = request.Query["file"];
            ld.Password = request.Query["password"];
            ld.AddHtmlCode = request.Query["html"] == "1";
            ld.AddCopyProtection = request.Query["nocopy"] == "1";
            ld.RenderMultiTo = request.Query["multi"].ToString().ToInt();
            T setProp<T>(TaskSettingProp prop, T defaultValue) {
                string key = TaskSetting.ShorPropNames[prop];
                string sVal = request.Query[key];
                if (String.IsNullOrEmpty(sVal)) return defaultValue;
                var val = TaskExtensions.ConvertValue(defaultValue.GetType(), sVal, null);
                if (val == null || !(val is T)) return defaultValue;
                ld.Settings.SetProps |= prop;
                return (T)val;
            }
            ld.Settings.ItemsCount = TaskLoader.LoadItemsCount(setProp(TaskSettingProp.ItemsCount, ""));
            ld.Settings.Level = setProp(TaskSettingProp.Level, "").Split(',');
            ld.Settings.RandomOrder = setProp(TaskSettingProp.RandomOrder, ld.Settings.RandomOrder);
            ld.Settings.Round = setProp(TaskSettingProp.Round, ld.Settings.Round);
            ld.Settings.SeparatePreviousPairs = setProp(TaskSettingProp.SeparatePreviousPairs, ld.Settings.SeparatePreviousPairs);
            ld.Settings.Seed = setProp(TaskSettingProp.Seed, ld.Settings.Seed);
            return ld;
        }

        public LaunchData GetLaunchDataFromGetData(HttpRequest request)
        {
            try
            {
                string data64 = request.Query["data"];
                string hash = request.Query["hash"];
                if (String.IsNullOrEmpty(data64) || String.IsNullOrEmpty(hash))
                    return null;

                string data = Encoding.UTF8.GetString(Convert.FromBase64String(data64));
                string hashCals = GetHash(data);

                if (hashCals != hash)
                    return null;

                var prms = data.Split('&').Select(x => x.Split('=')).ToDictionary(k => k[0], v => v[1]);

                string timeOut = prms.GetValueOrDefault("timeout", "");
                if (!String.IsNullOrEmpty(timeOut))
                {
                    var limit = DateTime.Parse(timeOut);
                    if (DateTime.Now > limit)
                        return null;
                }

                var ld = new LaunchData();

                T setProp<T>(TaskSettingProp prop, T defaultValue)
                {
                    string key = TaskSetting.ShorPropNames[prop];
                    string sVal = "";
                    if (prms.ContainsKey(key))
                        sVal = prms[key];
                    if (String.IsNullOrEmpty(sVal)) return defaultValue;
                    var val = TaskExtensions.ConvertValue(defaultValue.GetType(), sVal, null);
                    if (val == null || !(val is T)) return defaultValue;
                    ld.Settings.SetProps |= prop;
                    return (T)val;
                }

                ld.Settings.ItemsCount = TaskLoader.LoadItemsCount(setProp(TaskSettingProp.ItemsCount, ""));
                ld.Settings.Level = setProp(TaskSettingProp.Level, "").Split(',');
                ld.Settings.RandomOrder = setProp(TaskSettingProp.RandomOrder, ld.Settings.RandomOrder);
                ld.Settings.Round = setProp(TaskSettingProp.Round, ld.Settings.Round);
                ld.Settings.SeparatePreviousPairs = setProp(TaskSettingProp.SeparatePreviousPairs, ld.Settings.SeparatePreviousPairs);
                ld.Settings.Seed = setProp(TaskSettingProp.Seed, ld.Settings.Seed);

                ld.Password = prms.GetValueOrDefault("password", ld.File);
                ld.File = prms.GetValueOrDefault("file", ld.File);
                ld.AddHtmlCode = prms.GetValueOrDefault("html", ld.File) == "1";
                ld.AddCopyProtection = prms.GetValueOrDefault("nocopy", ld.File) == "1";

                return ld;
            }
            catch
            {
                return null;
            }
        }

        //public static string LoadData(LaunchData ld)
        //{
        //    if (ld == null) return "";
        //    if (String.IsNullOrEmpty(ld.File)) return "";
        //    var task = TaskLoader.LoadTasksFromFile(ld.File);
        //    task.Setting = TaskLoader.MergeSettings(task.Setting, ld.Settings);
        //    return TaskRender.Render(task);
        //}

        public IMemoryCache Cache { get; set; }
        public Tasks TasksData { get; private set; }
        //public LaunchData LdMoodle { get; private set; }
        public LaunchData LdPost { get; private set; }
        public LaunchData LdGetData { get; private set; }
        public LaunchData LdGet { get; private set; }
        public string FileName { get; private set; }
        public string Result { get; private set; }

        public bool AddHtmlCode { get => LdGetData?.AddHtmlCode == true || LdPost?.AddHtmlCode == true || LdGet?.AddHtmlCode == true || TasksData?.Setting?.CustomSettings?.GetRef("addHtmlCode") == "1"; } 
        public bool AddCopyProtection { get => LdGetData?.AddCopyProtection == true || LdPost?.AddCopyProtection == true || LdGet?.AddCopyProtection == true; }

        public string CustomStyle { get => TasksData?.Setting?.CustomSettings?.GetRef("style") ?? ""; }

        public bool LoadData(HttpRequest request)
        {
            TasksData = null; LdPost = null; LdGet = null; // LdMoodle = null; 
            //try { LdMoodle = GetLaunchDataFromMoodle(request); } catch { }
            try { LdPost   = GetLaunchDataFromPost(request); } catch { }
            try { LdGetData= GetLaunchDataFromGetData(request); } catch { }
            try { LdGet    = GetLaunchDataFromGet(request); } catch { }
            // TODO: Base64 v get 

            FileName = GetDataFileName(TaskExtensions.GetFirstNotNull(
                request.Path.ToString().Trim('/'),
                //LdMoodle?.File, 
                LdGetData?.File, LdPost?.File, LdGet?.File));

            if (String.IsNullOrEmpty(FileName)) return false;

            object cachedTasks = null;

            //if (Cache?.TryGetValue(FileName, out cachedTasks) == true)
            //    TasksData = cachedTasks as Tasks;
            if (TasksData == null)
            {
                TasksData = TaskLoader.LoadTasksFromFile(FileName);
                Cache.Set(FileName, TasksData, TimeSpan.FromMinutes(1));
            }
            if (TasksData == null) return false;

            //if (!TaskLoader.ChcekPassword(TasksData.Password, LdMoodle?.Password)) LdMoodle = null;
            if (!TaskLoader.ChcekPassword(TasksData.Password, LdGetData?.Password)) LdGetData = null;
            if (!TaskLoader.ChcekPassword(TasksData.Password, LdPost?.Password)) LdPost = null;
            if (!TaskLoader.ChcekPassword(TasksData.Password, LdGet?.Password)) LdGet = null;

            if (LdGetData == null && LdPost == null && LdGet == null && !TaskLoader.ChcekPassword(TasksData.Password, null))
                return false; // No password, but it is needed

            var setting = TaskLoader.MergeSettings(LdGetData?.Settings, LdPost?.Settings, LdGet?.Settings);
            int? ldMultiTo = LdGetData?.RenderMultiTo ?? LdPost?.RenderMultiTo ?? LdGet?.RenderMultiTo;

            string selectedIds;
            if (ldMultiTo != null && ldMultiTo > 1 && TasksData.Setting.CustomSettings.GetRef("allowMulti") == "1")
                Result = TaskRender.RenderMulti(TasksData, setting, (int)ldMultiTo, out selectedIds);
            else 
                Result = TaskRender.Render(TasksData, setting, out selectedIds);

            Log(request, setting, FileName, selectedIds);

            return true;
        }


        private void Log(HttpRequest request, TaskSetting setting, string fileName, string selectedIds)
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), "log.txt");

            string urlFull = request.GetEncodedUrl();
            var log = new StringBuilder();
            log.AppendFormat("{0:yyyy-MM-dd HH:mm:ss} ", DateTime.Now);
            log.AppendFormat("selectedItems='{0}' ", selectedIds);
            log.Append(urlFull.Substring(0, urlFull.IndexOf("/", urlFull.IndexOf("://") + 3))); // http://xy.com/
            log.AppendFormat("/{0}?", Path.GetFileNameWithoutExtension(fileName));
            log.AppendFormat("{0}={1}&", TaskSetting.ShorPropNames[TaskSettingProp.Seed], setting.Seed);
            log.AppendFormat("{0}={1}&", TaskSetting.ShorPropNames[TaskSettingProp.Round], setting.Round);
            log.AppendFormat("{0}={1}&", TaskSetting.ShorPropNames[TaskSettingProp.ItemsCount], setting.ItemsCount.OriginalString);
            log.AppendFormat("{0}={1}&", TaskSetting.ShorPropNames[TaskSettingProp.RandomOrder], setting.RandomOrder.ToBin());
            log.AppendFormat("{0}={1}&", TaskSetting.ShorPropNames[TaskSettingProp.SeparatePreviousPairs], setting.SeparatePreviousPairs.ToBin());
            string pass = TaskExtensions.GetFirstNotNull(LdGetData?.Password, LdPost?.Password, LdGet?.Password);
            if (!String.IsNullOrEmpty(pass))
                log.AppendFormat("{0}={1}&", "password", pass);
            log.AppendFormat("{0}={1}&", "html", AddHtmlCode.ToBin());
            log.AppendFormat("{0}={1}", "nocopy", AddCopyProtection.ToBin());

            Task.Run(async delegate 
            {
                int i = 10;
                while (i > 0)
                    try
                    {
                        using (var sw = new StreamWriter(logFilePath, true))
                            sw.WriteLine(log.ToString());
                        i = 0;
                    }
                    catch (Exception)
                    {
                        i--;
                        await Task.Delay(150);
                    }
            });
        }

        public static string LtiToGet(HttpRequest request)
        {
            try
            {
                // TODO: chceck OAuth hash
                var sb = new StringBuilder();

                string checkProp(string key, bool addCustomPrefix = true, bool append = true)
                {
                    if (request.Form?.TryGetValue((addCustomPrefix ? "custom_" : "") + key, out StringValues val) == true)
                    {
                        if (append)
                            sb.Append($"&{key}={val}");
                        return val;
                    }
                    return "";
                }
                // Limit
                string limit = checkProp("limit", true, false);
                if (!String.IsNullOrEmpty(limit))
                {
                    var dLimit = DateTime.Parse(limit);
                    if (DateTime.Now > dLimit)
                        return "";
                }
                // Standard props
                checkProp("file");
                checkProp("html");
                checkProp("nocopy");
                checkProp(TaskSetting.ShorPropNames[TaskSettingProp.ItemsCount]);
                checkProp(TaskSetting.ShorPropNames[TaskSettingProp.Level]);
                checkProp(TaskSetting.ShorPropNames[TaskSettingProp.RandomOrder]);
                checkProp(TaskSetting.ShorPropNames[TaskSettingProp.Round]);
                checkProp(TaskSetting.ShorPropNames[TaskSettingProp.SeparatePreviousPairs]);
                // Seed
                string seed = checkProp(TaskSetting.ShorPropNames[TaskSettingProp.Seed], append: false);
                string email = checkProp("lis_person_contact_email_primary", false, false);
                seed += email.ToLower();
                sb.Append($"&{TaskSetting.ShorPropNames[TaskSettingProp.Seed]}={seed}");
                // Password
                string pass = checkProp("oauth_consumer_key", false, false);
                sb.Append($"&password={pass}");
                // TimeOut
                string timeout = checkProp("timeout", true, false);
                if (!String.IsNullOrEmpty(timeout))
                    timeout = DateTime.Now.AddSeconds(TimeToSeconds(timeout)).ToString("yyyy-MM-dd HH:mm:ss");
                sb.Append($"&timeout={timeout}");
                // Salt
                string salt = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=').Replace("+", "").Replace("/", "");
                sb.Append($"&salt={salt}");

                //string prms = String.Format("{0}|{1}|{2}|{3}|{4}|{5}", file, password, userId.ToLower(), round, timeout, salt);
                string prms = sb.ToString().TrimStart('&');
                string hash = GetHash(prms);
                string prms64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(prms));

                string url = String.Format("~/?data={0}&hash={1}", prms64, hash);

                return url;
            }
            catch
            {
                return "~/";
            }
        }

        /*
        public static bool CheckParams(HttpRequest request, out string error, out string url)
        {
            LaunchData ldModle = null;
            try { ldModle = GetLaunchDataFromMoodle(request); } catch { }
            error = "";
            url = "";
            string appKey = request.Form["oauth_consumer_key"];
            //if (appKey != WebConfigurationManager.AppSettings["appKey"])
            {
                //error = "Neplatný klíč aplikace";
                //return false;
            }
            var sb = new StringBuilder();
            if (ldModle.Settings.SetProps.HasFlag(TaskSettingProp.ItemsCount)) sb.AppendFormat("");

            string userId = request.Form["lis_person_contact_email_primary"];
            string file = request.Form["custom_file"];
            string password = request.Form["custom_password"];
            string round = request.Form["custom_round"];
            string timeout = request.Form["custom_timeout"];
            string limit = request.Form["custom_limit"];
            string salt = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=').Replace("+", "").Replace("/", "");

            if (!String.IsNullOrEmpty(timeout))
                timeout = DateTime.Now.AddSeconds(TimeToSeconds(timeout)).ToString("yyyy-MM-dd HH:mm:ss");

            if (!String.IsNullOrEmpty(limit))
            {
                var dLimit = DateTime.Parse(limit);
                if (DateTime.Now > dLimit)
                {
                    error = "Limit platnosti odkazu vypršel";
                    return false;
                }
            }


            string prms = String.Format("{0}|{1}|{2}|{3}|{4}|{5}", file, password, userId.ToLower(), round, timeout, salt);
            string hash = GetHash(prms);
            string prms64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(prms));

            url = String.Format("~/Task.aspx?data={0}&hash={1}", prms64, hash);

            return true;
        }*/

        public static int TimeToSeconds(string time)
        {
            char mj = time.ToLower().Last();
            string j = time.Substring(0, time.Length - 1);
            switch (mj)
            {
                case 's': return Convert.ToInt32(j);
                case 'm': return Convert.ToInt32(j) * 60;
                case 'h': return Convert.ToInt32(j) * 60 * 60;
                default: return Convert.ToInt32(time);
            }
        }

        public static string GetHash(string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;
            var byty = Encoding.UTF8.GetBytes(text);
            var sha = new SHA1Managed();
            var hash = sha.ComputeHash(byty);
            return Convert.ToBase64String(hash).TrimEnd('=').Replace("+", "").Replace("/", "");
        }


    }
}
