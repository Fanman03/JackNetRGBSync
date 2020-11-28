using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using RGBSyncPlus.Helper;

namespace RGBSyncPlus.UI
{
    /// <summary>
    /// Interaction logic for CrashWindow.xaml
    /// </summary>
    public partial class CrashWindow : Window
    {
        public CrashWindow()
        {
            InitializeComponent();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public async Task<string> Send_Report(object sender, RoutedEventArgs e)
        {
            string text = ApplicationManager.Logger.Log;

            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>
            {
                { "data", text }
            };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("https://api.rgbsync.com/crashlogs/newReport/", content);

            var responseString = await response.Content.ReadAsStringAsync();
            this.Close();
            return responseString;
        }

        public string SendReport()
        {
            string text = ApplicationManager.Logger.Log;

            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>
            {
                { "data", text }
            };

            var content = new FormUrlEncodedContent(values);

            var response = client.PostAsync("https://api.rgbsync.com/crashlogs/newReport/", content).Result;

            var responseString = response.Content.ReadAsStringAsync().Result;
            
            return responseString;
        }

        public string ErrorReportUrl;
        private void ClickQRCode(object sender, RoutedEventArgs e)
        {
            ErrorReportUrl.NavigateToUrlInDefaultBrowser();
        }

        private void ViewReport(object sender, RoutedEventArgs e)
        {
            ErrorReportUrl.NavigateToUrlInDefaultBrowser();
        }
    }
}
