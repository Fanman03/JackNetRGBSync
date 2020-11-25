using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace RGBSyncPlus.UI.Tabs
{
    /// <summary>
    /// Interaction logic for NewsView.xaml
    /// </summary>
    public partial class NewsView : UserControl
    {
        NewsViewModel vm => this.DataContext as NewsViewModel;
        public void OpenUrl(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            string browserPath = GetBrowserPath();
            if (browserPath == string.Empty)
                browserPath = "iexplore";
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(browserPath);
            process.StartInfo.Arguments = "\"" + e.Parameter.ToString() + "\"";
            process.Start();

            
        }
        public NewsView()
        {
            InitializeComponent();
        }

        private void CloseModal(object sender, RoutedEventArgs e)
        {
            vm.SelectedNewsItem = null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            vm.SelectedNewsItem = ((Button)sender).DataContext as NewsViewModel.NewsItemViewModel;
        }

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
    }
}
