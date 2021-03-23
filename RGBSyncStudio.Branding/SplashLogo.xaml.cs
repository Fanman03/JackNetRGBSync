using System;
using System.Collections.Generic;
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

namespace RGBSyncStudio.Branding
{
    /// <summary>
    /// Interaction logic for SplashLogo.xaml
    /// </summary>
    public partial class SplashLogo : UserControl
    {
        public SplashLogo(bool lightTheme)
        {
            InitializeComponent();
            if (lightTheme)
            {
                this.Resources["SystemAltHighColorBrush"] = new SolidColorBrush(Colors.White);
                this.Resources["SystemBaseHighColorBrush"] = new SolidColorBrush(Colors.Black);
            }
        }
    }
}
