using Microsoft.Win32;
using System.Diagnostics;

namespace RGBSyncStudio.Helper
{
    public static class BrowserHelper
    {

        private static string GetBrowserPath()
        {
            string browserName = "iexplore.exe";
            using (RegistryKey userChoiceKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice"))
            {
                if (userChoiceKey != null)
                {
                    object progIdValue = userChoiceKey.GetValue("Progid");
                    if (progIdValue != null)
                    {
                        if (progIdValue.ToString().ToLower().Contains("chrome"))
                            browserName = "chrome.exe";
                        else if (progIdValue.ToString().ToLower().Contains("firefox"))
                            browserName = "firefox.exe";
                        else if (progIdValue.ToString().ToLower().Contains("safari"))
                            browserName = "safari.exe";
                        else if (progIdValue.ToString().ToLower().Contains("opera"))
                            browserName = "opera.exe";
                        else if (progIdValue.ToString().ToLower().Contains("edge"))
                            browserName = "msedge.exe";
                    }
                }
            }

            return browserName;
        }

        public static void NavigateToUrlInDefaultBrowser(this string input)
        {
            string browserPath = GetBrowserPath();
            if (browserPath == string.Empty)
                browserPath = "iexplore";
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(browserPath);
            process.StartInfo.Arguments = "\"" + input + "\"";
            process.Start();


        }
    }
}
