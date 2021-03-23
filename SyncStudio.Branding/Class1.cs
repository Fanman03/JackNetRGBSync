using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SyncStudio.Branding
{
    public interface IBranding
    {
        UserControl GetDarkSplashLogo();
        UserControl GetLightSplashLogo();

        string GetAppName();
        string GetAppAuthor();
        BitmapImage GetIcon();

    }
}
