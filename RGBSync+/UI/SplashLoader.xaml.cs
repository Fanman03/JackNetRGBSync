using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SimpleLed;

namespace SyncStudio.WPF.UI
{
    /// <summary>
    /// Interaction logic for SplashLoader.xaml
    /// </summary>
    public partial class SplashLoader : Window
    {
        public SplashLoader()
        {
            InitializeComponent();
            
            
            UserControl splashLogo;
        //todo    if (SyncStudio.Core.ServiceManager.SLSManager.GetTheme() == ThemeWatcher.WindowsTheme.Dark)

        if (true)
            {
                splashLogo = ServiceManager.Instance.Branding.GetDarkSplashLogo();
                (this.DataContext as SplashLoaderViewModel).SecondaryColor = Colors.Black;
                (this.DataContext as SplashLoaderViewModel).PrimaryColor = Colors.White;
                (this.DataContext as SplashLoaderViewModel).PrimarySolidColorBrush = new SolidColorBrush(Colors.White);
            }
            else
            {
                splashLogo = ServiceManager.Instance.Branding.GetLightSplashLogo();
                (this.DataContext as SplashLoaderViewModel).SecondaryColor = Colors.White;
                (this.DataContext as SplashLoaderViewModel).PrimaryColor = Colors.Black;
                (this.DataContext as SplashLoaderViewModel).PrimarySolidColorBrush = new SolidColorBrush(Colors.Black);

            }

            this.Icon = ServiceManager.Instance.Branding.GetIcon();

            SplashHere.Child = splashLogo;
        }
    }
}
