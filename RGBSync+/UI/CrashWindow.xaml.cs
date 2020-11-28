using System.Collections.Generic;
using System.Net.Http;
using System.Windows;

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

        private async void Send_Report(object sender, RoutedEventArgs e)
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
        }
    }
}
