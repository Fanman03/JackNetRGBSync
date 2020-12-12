﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;
using SharedCode;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for UpgradingWindow.xaml
    /// </summary>
    public partial class UpgradingWindow
    {
        public UpgradingWindow()
        {
            InitializeComponent();
            vm = this.DataContext as UpgradingViewModel;
            Core.UpgradingWindow = this;
        }

        public UpgradingViewModel vm;

        private async void AcrylicWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(100);
            await StartCheck();
        }

        public static string BaseFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+"\\RGBSyncStudio";

        private async Task StartCheck()
        {

            if (!Directory.Exists(BaseFolder))
            {
                Directory.CreateDirectory(BaseFolder);

              //  Installer.AddShortcut();
            }

            Process[] processlist = Process.GetProcesses();

            if (processlist.Any(x => x.ProcessName == "RGBSync+"))
            {
                try
                {
                    var proc = processlist.First(x => x.ProcessName == "RGBSync+");
                    proc.Kill();
                    proc.Dispose();
                    proc = null;
                }
                catch
                {
                }
            }

            if (File.Exists(BaseFolder + "\\launcherPrefs.json"))
            {
                Core.LauncherPrefs = JsonConvert.DeserializeObject<LauncherPrefs>(File.ReadAllText(BaseFolder + "\\launcherPrefs.json"));
            }

            UpdateCheck check = new UpdateCheck();
            await check.Execute(Core.LauncherPrefs.ReleaseBranch, this, BaseFolder);
            while (!check.Complete)
            {
                await Task.Delay(100);
            }

            Process p = new Process();
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.FileName = BaseFolder + "\\RGBSync+.exe";
            p.StartInfo.WorkingDirectory = BaseFolder;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;

            if (Core.LauncherPrefs.RunAsAdmin)
            {
                if (System.Environment.OSVersion.Version.Major >= 6)
                {
                    p.StartInfo.Verb = "runas";
                }
            }

            p.Start();



            await Task.Delay(2000);

            System.Windows.Application.Current.Shutdown();
        }

        private void UpgradingWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.LeftShift) && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                try
                {
                    LoadingText.Text = "Resetting app...";
                    Directory.Delete(BaseFolder, true);
                    Thread.Sleep(200);
                    Process.Start(Assembly.GetExecutingAssembly().Location);
                    Environment.Exit(-1);
                }
                catch
                {

                }
            }
        }
    }
}
