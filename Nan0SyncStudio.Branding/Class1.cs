using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SyncStudio.Branding;

namespace Nan0SyncStudio.Branding
{
    public class Nan0SyncBranding : IBranding
    {
        public UserControl GetDarkSplashLogo()
        {
            return new LogoDark(false);
        }

        public UserControl GetLightSplashLogo()
        {
            return new LogoDark(true);
        }

        public string GetAppName()
        {
            return "Nan0Sync Studio";
        }

        public BitmapImage GetIcon()
        {
            var derp = Assembly.GetAssembly(typeof(Nan0SyncBranding)).GetManifestResourceNames();


            using (Stream myStream = Assembly.GetAssembly(typeof(Nan0SyncBranding)).GetManifestResourceStream("Nan0SyncStudio.Branding.nano_icon_wMs_icon.ico"))
            {
                if (myStream != null)
                {
                    var image= System.Drawing.Image.FromStream(myStream);
                    return Convert((Bitmap)image);
                }
            }

            return null;
        }
        public string GetAppAuthor()
        {
            return "Sync Studio Team";
        }
        private BitmapImage Convert(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
    }
}
