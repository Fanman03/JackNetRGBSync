using System.Windows;

namespace RGBSyncPlus.UI
{
    /// <summary>
    /// Interaction logic for DeleteLayerDialog.xaml
    /// </summary>
    public partial class DeleteLayerDialog : Window
    {
        public DeleteLayerDialog(string groupName)
        {
            InitializeComponent();
            //            GroupName.Text = " " + groupName + " ";
        }

        private void YesBtnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void NoBtnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
