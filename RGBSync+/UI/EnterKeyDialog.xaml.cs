using System;
using System.Collections.Generic;
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
    /// Interaction logic for EnterKeyDialog.xaml
    /// </summary>
    public partial class EnterKeyDialog : Window
    {
        public EnterKeyDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                WebClient client = new WebClient();
                string response = client.DownloadString("https://rgbsync.com/api/validateKey.php?token=zQlszc7d1l9t8cv734nmte8ui4o3s8d15pcz&key=" + keyBox.Text);
                if(response == "valid")
                {
                    ApplicationManager.Instance.AppSettings.ApiKey = keyBox.Text;
                    App.SaveSettings();
                    this.Close();
                } else
                {
                    GenericErrorDialog error = new GenericErrorDialog("Invalid Cloud Access Key.", "Error!", "Invalid Cloud Access Key.");
                    error.Show();
                }
            }
            catch
            {
                GenericErrorDialog error = new GenericErrorDialog("Unable to complete API request.", "Error!", "Unable to complete API request.");
                error.Show();
            }
        }
    }
}
