using System.Diagnostics;
using System.Windows;

namespace RGBSyncPlus.UI
{
    /// <summary>
    /// Interaction logic for PremiumMessageBox.xaml
    /// </summary>
    public partial class PremiumMessageBox : Window
    {
        public PremiumMessageBox()
        {
            InitializeComponent();
        }

        public void YesClicked(object sender, RoutedEventArgs e)
        {
            Process.Start("https://patreon.com/fanman03");
            Close();
        }
        public void NoClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
