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
using System.Windows;
using Newtonsoft.Json;
using SharedCode;

namespace Launcher
{
    public class UpdateCheck
    {
        public bool Complete = false;
        public UpgradingWindow UpgradingWindow;
        public async Task Execute(LauncherPrefs.ReleaseType releaseType, UpgradingWindow upgrading, string destFolder)
        {
            UpgradingWindow = upgrading;
            UpgradingWindow.Show();
            UpgradingWindow.UpdateLayout();
            UpgradingWindow.UpdateDefaultStyle();
            
            UpgradingWindow.vm.Message = "Checking For Update...";
            await Task.Delay(1000);
            string url = "";
            string regexPattern = "";

            releaseType = LauncherPrefs.ReleaseType.Beta;

            //  try
            {
                switch (releaseType)
                {
                    case LauncherPrefs.ReleaseType.CI:
                        url = "http://cdn.ezmuze.co.uk/rgbsync/";
                        regexPattern = @"\'/rgbsync(.*?)\'";
                        break;

                    case LauncherPrefs.ReleaseType.Beta:
                        url = "https://cdn.rgbsync.com/prerelease/";
                        regexPattern = @"\'/prerelease(.*?)\'";
                        break;

                    case LauncherPrefs.ReleaseType.Release:
                        url = "https://cdn.rgbsync.com/release/";
                        regexPattern = @"\'/release(.*?)\'";
                        break;

                }

                string html = "";
                using (HttpClient client = new HttpClient())
                {
                    html = client.GetStringAsync(url).Result;
                }

                MatchCollection urls = Regex.Matches(html, regexPattern);

               // MessageBox.Show(urls[0].ToString());
                Dictionary<string, int> usableUrls = urls.Cast<Match>().ToDictionary(match => (url + (match.Value.Substring(2, (match.Value).Length - 3).Split('/').Last())), match => int.Parse(match.Value.Split('_').Last().Split('.').First()));

                Debug.WriteLine(usableUrls);

                int maxReleaseNumber = usableUrls.Values.Max();

                if (Core.LauncherPrefs.ReleaseInstalled != maxReleaseNumber || Core.LauncherPrefs.ReleaseTypeInstalled != releaseType )
                {
                    
                    try
                    {
                        if (File.Exists(destFolder+"\\RGBSync+.exe"))
                        {
                            if (!Directory.Exists(destFolder+"\\.old"))
                            {
                                DirectoryInfo dir = Directory.CreateDirectory(destFolder+"\\old");
                                
                                dir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                            }

                            File.Move("RGBSync+.exe", destFolder+"\\.old\\oldrss_" +Guid.NewGuid()+".exe");
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
                        foreach (var f in Directory.GetFiles(destFolder+"\\.old\\"))
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

                    if (Directory.Exists(destFolder+"\\.old"))
                    {
                        Thread.Sleep(1000);

                        if (Directory.GetFiles(destFolder+"\\.old\\").Length == 0)
                        {
                            Directory.Delete(destFolder+"\\.old", true);
                        }
                    }

                    vm.Message = "Installing " + releaseType + " release " + maxReleaseNumber;

                    string zipPath = destFolder+"\\"+releaseType + "_" + maxReleaseNumber + ".zip";

                    WebClient wc = new WebClient();
                    wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                    wc.DownloadFileCompleted += new AsyncCompletedEventHandler((object sender, AsyncCompletedEventArgs e) =>
                        {
                            if (Directory.Exists(destFolder+"\\temp"))
                            {
                                Directory.Delete(destFolder+"\\temp", true);
                            }

                            Directory.CreateDirectory(destFolder+"\\temp");

                            vm.Message = "Extracting...";
                            ZipFile.ExtractToDirectory(zipPath, destFolder+"\\temp");

                            DirectoryCopy(destFolder+"\\temp", destFolder, true);

                            File.Delete(zipPath);
                            try
                            {
                                Directory.Delete(destFolder+"\\temp", true);
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
                            File.WriteAllText(destFolder+"\\LauncherPrefs.json", json);


                            if (Directory.Exists(destFolder+"\\.old"))
                            {
                                Thread.Sleep(1000);

                                if (Directory.GetFiles(destFolder+"\\.old\\").Length == 0)
                                {
                                    Directory.Delete(destFolder+"\\.old", true);
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

            vm.Message = "Downloaded " + (e.BytesReceived / 1000000).ToString() + " MB of " + (e.TotalBytesToReceive / 1000000).ToString() + " MB.";

            vm.Percentage = 100-(int)percentage;
        }

        private UpgradingViewModel vm => UpgradingWindow.vm;
    }
}
