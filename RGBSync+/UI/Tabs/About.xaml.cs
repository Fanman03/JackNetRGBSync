using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace RGBSyncPlus.UI.Tabs
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : UserControl
    {
        public About()
        {
            InitializeComponent();
        }

        private void DonatePatreon(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "https://www.patreon.com/fanman03",
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void DonateCrypto(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "https://rgbsync.com/?crypto",
                UseShellExecute = true
            };
            Process.Start(psi);
        }


        private void DonatePayPal(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "https://paypal.me/ezmuze",
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}
