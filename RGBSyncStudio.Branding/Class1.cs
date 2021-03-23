using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SyncStudio.Branding;

namespace RGBSyncStudio.Branding
{
    public class RGBSyncStudioBranding : IBranding
    {
        public UserControl GetDarkSplashLogo()
        {
           return new SplashLogo(false);
        }

        public UserControl GetLightSplashLogo()
        {
            return new SplashLogo(true);
        }

        public string GetAppName()
        {
            return "RGB Sync Studio";
        }

        public string GetAppAuthor()
        {
            return "Sync Studio Team";
        }

        public BitmapImage GetIcon()
        {
            var derp = Assembly.GetAssembly(typeof(RGBSyncStudioBranding)).GetManifestResourceNames();


            using (Stream myStream = Assembly.GetAssembly(typeof(RGBSyncStudioBranding)).GetManifestResourceStream("RGBSyncStudioBranding.ProgramIcon.ico"))
            {
                if (myStream != null)
                {
                    var image = System.Drawing.Image.FromStream(myStream);
                    return Convert((Bitmap)image);
                }
            }

            return null;
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
