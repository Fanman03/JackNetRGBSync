using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            var psi = new ProcessStartInfo
            {
                FileName = "https://www.patreon.com/fanman03",
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void DonateCrypto(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "https://rgbsync.com/?crypto",
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        
        private void DonatePayPal (object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "https://paypal.me/ezmuze",
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}
