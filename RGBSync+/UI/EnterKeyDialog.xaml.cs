using System.Windows;

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
            //try
            //{
            //    WebClient client = new WebClient();
            //    string response = client.DownloadString("https://rgbsync.com/api/validateKey.php?token=zQlszc7d1l9t8cv734nmte8ui4o3s8d15pcz&key=" + keyBox.Text);
            //    if(response == "valid")
            //    {
            //        //ServiceManager.Instance.ApplicationManager.AppSettings.ApiKey = keyBox.Text;
            //        App.SaveSettings();
            //        this.Close();
            //    } else
            //    {
            //        GenericErrorDialog error = new GenericErrorDialog("Invalid Cloud Access Key.", "Error!", "Invalid Cloud Access Key.");
            //        error.Show();
            //    }
            //}
            //catch
            //{
            //    GenericErrorDialog error = new GenericErrorDialog("Unable to complete API request.", "Error!", "Unable to complete API request.");
            //    error.Show();
            //}
        }
    }
}
