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
using System.Windows.Shapes;

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
