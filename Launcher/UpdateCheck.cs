using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedCode;

namespace Launcher
{
    public class UpdateCheck
    {
        public bool Complete = false;
        public UpgradingWindow UpgradingWindow;
        public async Task Execute(LauncherPrefs.ReleaseType releaseType, UpgradingWindow upgrading)
        {
            UpgradingWindow = upgrading;
            UpgradingWindow.Show();
            UpgradingWindow.UpdateLayout();
            UpgradingWindow.UpdateDefaultStyle();
            
            UpgradingWindow.vm.Message = "Checking For Updatification";
            await Task.Delay(1000);
            string url = "";
          //  try
            {
                switch (releaseType)
                {
                    case LauncherPrefs.ReleaseType.CI:
                        url = "http://cdn.ezmuze.co.uk/rgbsync/";
                        break;

                    case LauncherPrefs.ReleaseType.Beta:
                        url = "";
                        break;

                    case LauncherPrefs.ReleaseType.Release:
                        url = "";
                        break;

                }

                string html = "";
                using (HttpClient client = new HttpClient())
                {
                    html = client.GetStringAsync(url).Result;
                }

                MatchCollection urls = Regex.Matches(html, @"\'/rgbsync(.*?)\'");
                Dictionary<string, int> usableUrls = urls.Cast<Match>().ToDictionary(match => (url + (match.Value.Substring(2, (match.Value).Length - 3).Split('/').Last())), match => int.Parse(match.Value.Split('_').Last().Split('.').First()));

                Debug.WriteLine(usableUrls);

                int maxReleaseNumber = usableUrls.Values.Max();

                if (Core.LauncherPrefs.ReleaseInstalled != maxReleaseNumber || Core.LauncherPrefs.ReleaseTypeInstalled != releaseType )
                {
                    
                    try
                    {
                        if (File.Exists("RGBSync+.exe"))
                        {
                            if (!Directory.Exists(".old"))
                            {
                                DirectoryInfo dir = Directory.CreateDirectory("old");
                                
                                dir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                            }

                            File.Move("RGBSync+.exe", ".old\\oldrss_"+Guid.NewGuid()+".exe");
                            File.Delete("oldrss.exe");
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }

                    if (Directory.Exists(".old"))
                    {
                        Thread.Sleep(1000);
                        foreach (var f in Directory.GetFiles(".old\\"))
                        {
                            try
                            {
                                File.Delete(f);
                            }
                            catch
                            {
                            }
                        }
                    }

                    if (Directory.Exists(".old"))
                    {
                        Thread.Sleep(1000);

                        if (Directory.GetFiles(".old\\").Length == 0)
                        {
                            Directory.Delete(".old", true);
                        }
                    }

                    vm.Message = "Installing " + releaseType + " release " + maxReleaseNumber;

                    string zipPath = releaseType + "_" + maxReleaseNumber + ".zip";

                    WebClient wc = new WebClient();
                    wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                    wc.DownloadFileCompleted += new AsyncCompletedEventHandler((object sender, AsyncCompletedEventArgs e) =>
                        {
                            if (Directory.Exists("temp"))
                            {
                                Directory.Delete("temp", true);
                            }

                            Directory.CreateDirectory("temp");

                            vm.Message = "Extracting...";
                            ZipFile.ExtractToDirectory(zipPath, "temp");

                            DirectoryCopy("temp", "", true);

                            File.Delete(zipPath);
                            try
                            {
                                Directory.Delete("temp", true);
                            }
                            catch
                            {
                            }

                            UpgradingWindow.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                UpgradingWindow.Hide();
                                UpgradingWindow.Close();
                            }));

                            Complete = true;

                            Core.LauncherPrefs.ReleaseInstalled = maxReleaseNumber;
                            Core.LauncherPrefs.ReleaseTypeInstalled = releaseType;

                            string json = JsonConvert.SerializeObject(Core.LauncherPrefs);
                            File.WriteAllText("LauncherPrefs.json", json);


                            if (Directory.Exists(".old"))
                            {
                                Thread.Sleep(1000);

                                if (Directory.GetFiles(".old\\").Length == 0)
                                {
                                    Directory.Delete(".old", true);
                                }
                            }

                            
                        });

                    wc.DownloadFileAsync(new Uri(usableUrls.First(x => x.Value == maxReleaseNumber).Key), zipPath);
                }
                else
                {
                    Complete = true;
                }
            }
            //catch (Exception e)
            //{
            //    Complete = true;
            //}

        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!string.IsNullOrWhiteSpace(destDirName) && !Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                Debug.WriteLine("Copying to "+temppath);
                try
                {
                    file.CopyTo(temppath, true);
                }
                catch
                {
                }
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            
            vm.Percentage = 100-(int)percentage;
        }

        private UpgradingViewModel vm => UpgradingWindow.vm;
    }
}
