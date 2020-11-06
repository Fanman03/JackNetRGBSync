using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RGBSyncPlus.Languages
{
    public class LanguageModel
    {
        public string Code { get; set; }
        public string EnglishName { get; set; }
        public string NativeName { get; set; }
        public List<LanguageItem> Items { get; set; }

        public class LanguageItem
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        public LanguageModel(string path)
        {
            string contents = File.ReadAllText(path);
            List<string> lines = contents.Split('\r').Select(x => x.Trim()).ToList();

            this.Code = lines[0];
            this.EnglishName = lines[1];
            this.NativeName = lines[2];

            this.Items = new List<LanguageItem>();
            foreach (string s in lines.Skip(3))
            {
                var parts = s.Split('\t');
                if (parts.Length > 1)
                {
                    this.Items.Add(new LanguageItem
                    {
                        Key = parts[0],
                        Value = parts[1]
                    });
                }
            }
        }
    }

    public static class LanguageManager
    {
        public static List<LanguageModel> Languages { get; set; } = new List<LanguageModel>();
        static LanguageManager()
        {
            try
            {
                var files = Directory.EnumerateFiles("Languages");

                foreach (string file in files)
                {
                    try
                    {
                        Languages.Add(new LanguageModel(file));
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        public static string GetValue(string key)
        {
            string dbg = "";
            try
            {
                string lang = "EN-US";

                if (ApplicationManager.Instance != null)
                {
                    if (ApplicationManager.Instance.NGSettings?.Lang != null)
                    {
                        lang = ApplicationManager.Instance.NGSettings.Lang;
                    }
                }

                var l = Languages.FirstOrDefault(x => x.Code.ToUpper() == lang.ToUpper());
                if (l == null)
                {
                    l = Languages.FirstOrDefault(x => x.Code.ToUpper().StartsWith(lang.ToUpper().Split('-').First()));
                }

                if (l == null)
                {
                    dbg = dbg + " lang null, loading...";
                    //Have to hardcode path for design time :(
                    string fn = "C:\\Projects\\JackNet\\bin\\Languages\\" + lang+".txt";

                    //return fn;

                    dbg = dbg + "\r\nloading " + fn;
                    l = new LanguageModel(fn);
                    Languages.Add(l);
                    dbg = dbg + "\r\nLoaded";
                }

                Debug.WriteLine("Looking for " + key + " in " + l?.Code);
                var r = l.Items.FirstOrDefault(x => x.Key.ToLower() == key.ToLower())?.Value;
                if (string.IsNullOrWhiteSpace(r))
                {
                    r = "[" + lang + ":" + key + "]";
                    Debug.WriteLine("couldnt find " + r);
                }

                return r;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
