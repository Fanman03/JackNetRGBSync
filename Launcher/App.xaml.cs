using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using SharedCode;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            if (File.Exists("launcherPrefs.json"))
            {
                Core.LauncherPrefs =
                    JsonConvert.DeserializeObject<LauncherPrefs>(File.ReadAllText("launcherPrefs.json"));
            }

            UpdateCheck check = new UpdateCheck();
            check.Execute(Core.LauncherPrefs.ReleaseBranch);
            while (!check.Complete)
            {
                Task.Delay(100).Wait();
            }

            Process p = new Process();
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.FileName = "RGBSync+.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            if (System.Environment.OSVersion.Version.Major >= 6)
            {
                p.StartInfo.Verb = "runas";
            }

            p.Start();
        }

    }
}
