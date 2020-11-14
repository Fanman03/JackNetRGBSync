using System.Windows;

namespace RGBSyncPlus.UI
{
    /// <summary>
    /// Interaction logic for GenericInfoDialog.xaml
    /// </summary>
    public partial class GenericInfoDialog : Window
    {
        public GenericInfoDialog(string message, string title)
        {
            InitializeComponent();
            this.Title = title;
            Message.Text = message;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
