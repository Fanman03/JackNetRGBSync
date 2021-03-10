using Newtonsoft.Json;
using SyncStudio.WPF.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using SyncStudio.ClientService;

namespace SyncStudio.WPF.UI
{
    /// <summary>
    /// Interaction logic for CrashWindow.xaml
    /// </summary>
    public partial class CrashWindow : Window
    {
        private ClientService.Settings settings = new Settings();
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
            string text = JsonConvert.SerializeObject(ServiceManager.Instance.Logger.Log);

            HttpClient client = new HttpClient();
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "data", text }
            };

            FormUrlEncodedContent content = new FormUrlEncodedContent(values);

            HttpResponseMessage response = await client.PostAsync("https://api.rgbsync.com/crashlogs/newReport/", content);

            string responseString = await response.Content.ReadAsStringAsync();
            this.Close();
            return responseString;
        }

        public string SendReport(Exception exception)
        {
            CrashContainer crashContainer = new CrashContainer();

            if (ServiceManager.Instance.SLSAuthService?.SimpleLedAuthenticated == true)
            {
                crashContainer.SimpleLedUserId = settings.SimpleLedUserId;
                crashContainer.SimpleLedUserName = settings.SimpleLedUserName;
            }

            StackTrace st = new StackTrace(exception, true);
            StackFrame frame = st.GetFrame(st.FrameCount - 1);

            crashContainer.ErrorName = exception.GetType().ToString();
            crashContainer.ErrorLocation = frame.GetFileName() + " / " + frame.GetMethod().Name + " / " + frame.GetFileLineNumber();
            crashContainer.Logs = ServiceManager.Instance.Logger.Log;

            string text = JsonConvert.SerializeObject(crashContainer);

            HttpClient client = new HttpClient();
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "data", text }
            };

            FormUrlEncodedContent content = new FormUrlEncodedContent(values);

            HttpResponseMessage response = client.PostAsync("https://api.rgbsync.com/crashlogs/newReport/", content).Result;

            string responseString = response.Content.ReadAsStringAsync().Result;

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

        public class CrashContainer
        {
            public string SimpleLedUserName { get; set; }
            public Guid SimpleLedUserId { get; set; }
            public string ErrorName { get; set; }
            public string ErrorLocation { get; set; }
            public List<SimpleLogger.LogEntry> Logs { get; set; } = new List<SimpleLogger.LogEntry>();
        }
    }
}
