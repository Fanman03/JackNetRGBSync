using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RGBSyncPlus.UI
{
    /// <summary>
    /// Interaction logic for GetUpdateWindow.xaml
    /// </summary>
    /// 

    public partial class GetUpdateWindow : Window
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public GetUpdateWindow()
        {
            InitializeComponent();
        }

        public void YesClicked(object sender, RoutedEventArgs e)
        {
            Logger.Info("Beginning update process...");
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string link = ApplicationManager.Instance.AppSettings.updateURL;
            WebClient wc = new WebClient();
            string filename = Directory.GetCurrentDirectory() + "\\~TEMP_setup.exe";
            Logger.Info("Downloading new installer...");
            try
            {
                wc.DownloadFile(link, filename);
                Logger.Info("Done!");
                Logger.Info("Staring installer...");
                Process.Start(filename);
                Close();
                Logger.Info("Closing app so update can complete.");
                ApplicationManager.Instance.Exit();
            } catch(Exception ex)
            {
                Logger.Error("Error downloading installer. " + ex);
            }
        }
        public void NoClicked(object sender, RoutedEventArgs e)
        {
            Logger.Info("Update refused by user.");
            this.Close();
        }

    }
}
