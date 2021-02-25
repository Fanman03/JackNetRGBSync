using System.Windows;
using System.Windows.Controls;
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
            if (SyncStudio.Core.ServiceManager.SLSManager.GetTheme() == ThemeWatcher.WindowsTheme.Dark)
            {
                splashLogo = ServiceManager.Instance.Branding.GetDarkSplashLogo();
            }
            else
            {
                splashLogo = ServiceManager.Instance.Branding.GetLightSplashLogo();
            }

            SplashHere.Child = splashLogo;
        }
    }
}
