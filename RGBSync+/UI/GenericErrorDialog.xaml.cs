using System.Windows;

namespace RGBSyncPlus.UI
{
    /// <summary>
    /// Interaction logic for GenericErrorDialog.xaml
    /// </summary>
    /// 

    public partial class GenericErrorDialog : Window
    {
        public GenericErrorDialog(string message, string title, string errorText)
        {
            InitializeComponent();
            this.Title = title;
            Message.Text = message;
            detailedError = errorText;
        }

        public static string detailedError;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(detailedError);
            this.Close();
        }
    }
}
